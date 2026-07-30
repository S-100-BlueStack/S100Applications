using System.Buffers.Binary;
using System.Globalization;
using System.Text;

namespace S100.Iso8211;

/// <summary>A decoded subfield: the definition it came from plus its CLR value.</summary>
public sealed class SubfieldValue
{
    public required SubfieldDefinition Definition { get; init; }

    /// <summary>string, long, ulong, double, byte[] or null when the subfield was empty.</summary>
    public required object? Value { get; init; }

    public string Label => Definition.Label;

    public string? AsString() => Value switch
    {
        null => null,
        string s => s,
        byte[] b => Convert.ToHexString(b),
        IFormattable f => f.ToString(null, CultureInfo.InvariantCulture),
        _ => Value.ToString()
    };

    public long? AsInt64() => Value switch
    {
        long l => l,
        ulong u => (long)u,
        double d => (long)d,
        string s when long.TryParse(s.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var v) => v,
        _ => null
    };

    public double? AsDouble() => Value switch
    {
        double d => d,
        long l => l,
        ulong u => u,
        string s when double.TryParse(s.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var v) => v,
        _ => null
    };

    public override string ToString() => $"{Label} = {AsString()}";
}

/// <summary>One instance of a subfield group: label to value, in declaration order.</summary>
public sealed class SubfieldGroupInstance
{
    public required IReadOnlyList<SubfieldValue> Values { get; init; }

    public SubfieldValue? this[string label]
    {
        get
        {
            foreach (var v in Values)
                if (string.Equals(v.Label, label, StringComparison.OrdinalIgnoreCase)) return v;
            return null;
        }
    }
}

/// <summary>The values of one array-descriptor group inside a field.</summary>
public sealed class FieldGroupValues
{
    public required SubfieldGroup Group { get; init; }
    public required IReadOnlyList<SubfieldGroupInstance> Instances { get; init; }
    public bool Repeats => Group.Repeats;
}

/// <summary>A field as it appears inside a data record.</summary>
public sealed class DataField
{
    public required string Tag { get; init; }
    public required FieldDefinition Definition { get; init; }
    public required IReadOnlyList<FieldGroupValues> Groups { get; init; }
    public required ReadOnlyMemory<byte> RawData { get; init; }

    /// <summary>Every group instance flattened, which is what most callers want.</summary>
    public IEnumerable<SubfieldGroupInstance> Instances
    {
        get
        {
            foreach (var g in Groups)
                foreach (var i in g.Instances) yield return i;
        }
    }

    /// <summary>First value found for a label anywhere in the field.</summary>
    public SubfieldValue? Find(string label)
    {
        foreach (var inst in Instances)
        {
            var v = inst[label];
            if (v is not null) return v;
        }
        return null;
    }

    public string? GetString(string label) => Find(label)?.AsString();
    public long? GetInt64(string label) => Find(label)?.AsInt64();
    public double? GetDouble(string label) => Find(label)?.AsDouble();

    public override string ToString() => $"{Tag} ({Definition.Name})";
}

/// <summary>A logical data record (leader identifier 'D' or 'R').</summary>
public sealed class DataRecord
{
    public required int Ordinal { get; init; }
    public required Iso8211Leader Leader { get; init; }
    public required IReadOnlyList<DataField> Fields { get; init; }

    public DataField? this[string tag]
    {
        get
        {
            foreach (var f in Fields)
                if (string.Equals(f.Tag, tag, StringComparison.OrdinalIgnoreCase)) return f;
            return null;
        }
    }

    public IEnumerable<DataField> FieldsWithTag(string tag)
    {
        foreach (var f in Fields)
            if (string.Equals(f.Tag, tag, StringComparison.OrdinalIgnoreCase)) yield return f;
    }

    public bool Has(string tag) => this[tag] is not null;

    public override string ToString() => $"#{Ordinal}: {string.Join(", ", Fields.Select(f => f.Tag))}";
}

/// <summary>Decodes the byte content of a field into subfield values.</summary>
internal static class FieldDataDecoder
{
    public static IReadOnlyList<FieldGroupValues> Decode(
        FieldDefinition definition, ReadOnlySpan<byte> data, bool littleEndianBinary)
    {
        if (data.Length > 0 && data[^1] == Iso8211Constants.FieldTerminator) data = data[..^1];

        var result = new List<FieldGroupValues>(definition.Groups.Count);
        int pos = 0;

        foreach (var group in definition.Groups)
        {
            var instances = new List<SubfieldGroupInstance>();

            if (!group.Repeats)
            {
                instances.Add(ReadInstance(group, definition.Encoding, data, ref pos, littleEndianBinary));
            }
            else
            {
                int guard = 0;
                while (pos < data.Length)
                {
                    if (IsOnlyTerminators(data[pos..])) break;
                    int before = pos;
                    instances.Add(ReadInstance(group, definition.Encoding, data, ref pos, littleEndianBinary));
                    if (pos == before) break;                 // no progress: malformed field, stop safely
                    if (++guard > 1_000_000)
                        throw new Iso8211Exception($"Field {definition.Tag} repeats implausibly often.");
                }
            }

            result.Add(new FieldGroupValues { Group = group, Instances = instances });
        }

        return result;
    }

    private static bool IsOnlyTerminators(ReadOnlySpan<byte> rest)
    {
        foreach (byte b in rest)
            if (b is not (Iso8211Constants.FieldTerminator or Iso8211Constants.UnitTerminator)) return false;
        return true;
    }

    private static SubfieldGroupInstance ReadInstance(
        SubfieldGroup group, Encoding encoding, ReadOnlySpan<byte> data, ref int pos, bool littleEndian)
    {
        var values = new List<SubfieldValue>(group.Subfields.Count);
        foreach (var sf in group.Subfields)
        {
            object? value = ReadValue(sf.Format, encoding, data, ref pos, littleEndian);
            if (sf.Format.Kind == SubfieldKind.Filler) continue;
            values.Add(new SubfieldValue { Definition = sf, Value = value });
        }
        return new SubfieldGroupInstance { Values = values };
    }

    private static object? ReadValue(
        SubfieldFormat format, Encoding encoding, ReadOnlySpan<byte> data, ref int pos, bool littleEndian)
    {
        if (pos >= data.Length) return null;

        switch (format.Kind)
        {
            case SubfieldKind.Filler:
                pos = Math.Min(data.Length, pos + Math.Max(format.Width, 1));
                return null;

            case SubfieldKind.Binary:
            {
                int w = format.Width;
                if (w <= 0 || pos + w > data.Length) { pos = data.Length; return null; }
                ReadOnlySpan<byte> raw = data.Slice(pos, w);
                pos += w;
                return DecodeBinary(raw, format.BinaryKind, littleEndian);
            }

            case SubfieldKind.BitString:
            {
                int w = Math.Max(format.Width, 0);
                if (w == 0 || pos + w > data.Length) { pos = data.Length; return null; }
                byte[] raw = data.Slice(pos, w).ToArray();
                pos += w;
                return raw;
            }

            default:
            {
                ReadOnlySpan<byte> raw;
                if (format.Width > 0)
                {
                    int w = Math.Min(format.Width, data.Length - pos);
                    raw = data.Slice(pos, w);
                    pos += w;
                }
                else
                {
                    int end = pos;
                    while (end < data.Length &&
                           data[end] != Iso8211Constants.UnitTerminator &&
                           data[end] != Iso8211Constants.FieldTerminator) end++;
                    raw = data[pos..end];
                    pos = end < data.Length ? end + 1 : end;   // consume the delimiter
                }

                string text = encoding.GetString(raw).Trim('\0').TrimEnd();
                return format.Kind switch
                {
                    SubfieldKind.IntegerText =>
                        long.TryParse(text.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var l)
                            ? l : (text.Length == 0 ? null : text),
                    SubfieldKind.RealText =>
                        double.TryParse(text.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var d)
                            ? d : (text.Length == 0 ? null : (object)text),
                    _ => text
                };
            }
        }
    }

    private static object DecodeBinary(ReadOnlySpan<byte> raw, BinaryKind kind, bool littleEndian)
    {
        Span<byte> buf = stackalloc byte[8];
        raw.CopyTo(buf);
        if (!littleEndian) buf[..raw.Length].Reverse();   // normalise to little-endian

        switch (kind)
        {
            case BinaryKind.UnsignedInteger:
                return raw.Length switch
                {
                    1 => (long)buf[0],
                    2 => (long)BinaryPrimitives.ReadUInt16LittleEndian(buf),
                    4 => (long)BinaryPrimitives.ReadUInt32LittleEndian(buf),
                    8 => unchecked((long)BinaryPrimitives.ReadUInt64LittleEndian(buf)),
                    _ => ReadUnsignedWidth(buf, raw.Length)
                };

            case BinaryKind.SignedInteger:
                return raw.Length switch
                {
                    1 => (long)(sbyte)buf[0],
                    2 => (long)BinaryPrimitives.ReadInt16LittleEndian(buf),
                    4 => (long)BinaryPrimitives.ReadInt32LittleEndian(buf),
                    8 => BinaryPrimitives.ReadInt64LittleEndian(buf),
                    _ => SignExtend(ReadUnsignedWidth(buf, raw.Length), raw.Length)
                };

            case BinaryKind.FloatingPointReal:
            case BinaryKind.FixedPointReal:
            case BinaryKind.FloatingPointComplex:
                return raw.Length switch
                {
                    4 => (double)BinaryPrimitives.ReadSingleLittleEndian(buf),
                    8 => BinaryPrimitives.ReadDoubleLittleEndian(buf),
                    _ => (double)ReadUnsignedWidth(buf, raw.Length)
                };

            default:
                return raw.ToArray();
        }
    }

    private static long ReadUnsignedWidth(ReadOnlySpan<byte> buf, int width)
    {
        long v = 0;
        for (int i = width - 1; i >= 0; i--) v = (v << 8) | buf[i];
        return v;
    }

    private static long SignExtend(long value, int width)
    {
        int bits = width * 8;
        if (bits >= 64) return value;
        long signBit = 1L << (bits - 1);
        return (value ^ signBit) - signBit;
    }
}
