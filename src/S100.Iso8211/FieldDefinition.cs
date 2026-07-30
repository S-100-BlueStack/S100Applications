using System.Text;

namespace S100.Iso8211;

/// <summary>One named subfield of a field, bound to the format control that decodes it.</summary>
public sealed class SubfieldDefinition
{
    public required string Label { get; init; }
    public required SubfieldFormat Format { get; init; }

    /// <summary>Index of the subfield within its group.</summary>
    public int IndexInGroup { get; init; }

    public override string ToString() => $"{Label} {Format}";
}

/// <summary>
/// A dimension of the array descriptor. S-100 fields are usually a single group; concatenated
/// fields (descriptor separator <c>\</c>) carry a fixed head group followed by a repeating tail,
/// for example <c>VDID\*YCOO!XCOO!ZCOO</c>.
/// </summary>
public sealed class SubfieldGroup
{
    /// <summary>True when the descriptor marked this group with <c>*</c>: it repeats until the field data is exhausted.</summary>
    public bool Repeats { get; init; }

    public required IReadOnlyList<SubfieldDefinition> Subfields { get; init; }

    /// <summary>Total fixed byte width of one instance, or null if any subfield is variable length.</summary>
    public int? FixedInstanceWidth
    {
        get
        {
            int total = 0;
            foreach (var sf in Subfields)
            {
                if (sf.Format.Width <= 0) return null;
                total += sf.Format.Width;
            }
            return total;
        }
    }
}

/// <summary>A data descriptive field taken from the DDR: what a tag means and how to decode it.</summary>
public sealed class FieldDefinition
{
    public required string Tag { get; init; }
    public required string Name { get; init; }
    public required FieldControls Controls { get; init; }
    public required string ArrayDescriptor { get; init; }
    public required string FormatControls { get; init; }
    public required IReadOnlyList<SubfieldGroup> Groups { get; init; }

    /// <summary>All subfields across all groups, in declaration order.</summary>
    public IReadOnlyList<SubfieldDefinition> Subfields { get; init; } = Array.Empty<SubfieldDefinition>();

    /// <summary>Text encoding for this field, derived from its truncated escape sequence.</summary>
    public Encoding Encoding { get; init; } = Encoding.UTF8;

    /// <summary>True for the DDR's own field control field (tag <c>0000</c>).</summary>
    public bool IsFieldControlField { get; init; }

    /// <summary>Tag pairs from the field control field, describing the record tree. Empty for normal fields.</summary>
    public IReadOnlyList<(string Parent, string Child)> TagPairs { get; init; } = Array.Empty<(string, string)>();

    public bool HasRepeatingGroup
    {
        get
        {
            foreach (var g in Groups) if (g.Repeats) return true;
            return false;
        }
    }

    public override string ToString() => $"{Tag} \"{Name}\" {ArrayDescriptor} {FormatControls}";

    /// <summary>
    /// Builds a definition from the raw bytes of a DDR field.
    /// Layout: field controls, field name, UT, array descriptor, UT, format controls, FT.
    /// </summary>
    public static FieldDefinition Parse(string tag, ReadOnlySpan<byte> data, int fieldControlLength, Encoding fallbackEncoding)
    {
        // Strip a trailing field terminator if the caller kept it.
        if (data.Length > 0 && data[^1] == Iso8211Constants.FieldTerminator) data = data[..^1];

        int fcLen = Math.Clamp(fieldControlLength, 0, data.Length);
        string rawControls = Ascii.ReadString(data[..fcLen]);
        var controls = FieldControls.Parse(rawControls);
        var encoding = controls.ResolveEncoding(fallbackEncoding);

        ReadOnlySpan<byte> rest = data[fcLen..];
        string part0 = NextPart(ref rest, encoding);   // field name, or external file title for 0000
        string part1 = NextPart(ref rest, encoding);   // array descriptor, or tag pairs for 0000
        string part2 = Decode(rest, encoding);         // format controls

        bool isFcf = tag.TrimEnd() == Iso8211Constants.FieldControlFieldTag
                     || tag.All(c => c == '0');

        if (isFcf)
        {
            return new FieldDefinition
            {
                Tag = tag,
                Name = part0,
                Controls = controls,
                ArrayDescriptor = string.Empty,
                FormatControls = string.Empty,
                Groups = Array.Empty<SubfieldGroup>(),
                Encoding = encoding,
                IsFieldControlField = true,
                TagPairs = ParseTagPairs(part1, tag.Length)
            };
        }

        var groups = BuildGroups(tag, part1, part2);
        var flat = new List<SubfieldDefinition>();
        foreach (var g in groups) flat.AddRange(g.Subfields);

        return new FieldDefinition
        {
            Tag = tag,
            Name = part0,
            Controls = controls,
            ArrayDescriptor = part1,
            FormatControls = part2,
            Groups = groups,
            Subfields = flat,
            Encoding = encoding
        };
    }

    private static IReadOnlyList<SubfieldGroup> BuildGroups(string tag, string arrayDescriptor, string formatControls)
    {
        var formats = FormatControlsParser.Parse(formatControls);

        // The descriptor's dimensions are separated by '\'. A leading '*' marks a repeating dimension.
        string[] rawGroups = arrayDescriptor.Split('\\', StringSplitOptions.RemoveEmptyEntries);
        if (rawGroups.Length == 0) rawGroups = new[] { string.Empty };

        var groups = new List<SubfieldGroup>(rawGroups.Length);
        int formatCursor = 0;

        foreach (string raw in rawGroups)
        {
            string body = raw;
            bool repeats = false;
            while (body.StartsWith('*')) { repeats = true; body = body[1..]; }

            // Trailing '!' separators are common in real files; drop the empty labels they create.
            string[] labels = body.Split('!', StringSplitOptions.None);
            var kept = new List<string>(labels.Length);
            for (int i = 0; i < labels.Length; i++)
            {
                string l = labels[i].Trim();
                if (l.Length == 0 && (i == labels.Length - 1 || labels.Length == 1)) continue;
                kept.Add(l.Length == 0 ? $"SUBFIELD{i + 1}" : l);
            }

            // An elementary field carries no labels: name the single subfield after its tag.
            if (kept.Count == 0) kept.Add(tag.Trim());

            var subfields = new List<SubfieldDefinition>(kept.Count);
            for (int i = 0; i < kept.Count; i++)
            {
                SubfieldFormat fmt = formats.Count == 0
                    ? SubfieldFormat.VariableText
                    : formats[Math.Min(formatCursor, formats.Count - 1)];
                if (formats.Count > 0) formatCursor++;

                subfields.Add(new SubfieldDefinition { Label = kept[i], Format = fmt, IndexInGroup = i });
            }

            groups.Add(new SubfieldGroup { Repeats = repeats, Subfields = subfields });
        }

        return groups;
    }

    private static IReadOnlyList<(string, string)> ParseTagPairs(string text, int tagLength)
    {
        if (tagLength <= 0) return Array.Empty<(string, string)>();
        var pairs = new List<(string, string)>();
        for (int i = 0; i + 2 * tagLength <= text.Length; i += 2 * tagLength)
            pairs.Add((text.Substring(i, tagLength).Trim(), text.Substring(i + tagLength, tagLength).Trim()));
        return pairs;
    }

    private static string NextPart(ref ReadOnlySpan<byte> data, Encoding encoding)
    {
        int idx = data.IndexOf(Iso8211Constants.UnitTerminator);
        if (idx < 0)
        {
            string all = Decode(data, encoding);
            data = ReadOnlySpan<byte>.Empty;
            return all;
        }
        string value = Decode(data[..idx], encoding);
        data = data[(idx + 1)..];
        return value;
    }

    private static string Decode(ReadOnlySpan<byte> data, Encoding encoding) =>
        data.IsEmpty ? string.Empty : encoding.GetString(data).TrimEnd('\0');
}
