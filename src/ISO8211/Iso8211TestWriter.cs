using System.Text;

namespace S100.Iso8211.SelfTest;

/// <summary>
/// A minimal ISO/IEC 8211 encoder. It exists so the reader can be tested against a file this
/// project produced itself; it is not a general purpose S-101 encoder, but it is enough to build
/// fixtures and to show what the physical layout looks like.
/// </summary>
public sealed class Iso8211TestWriter
{
    private const int SizeTag = 4;
    private const int SizeLength = 5;
    private const int SizePosition = 5;
    private const string NormalFieldControls9 = "00;&   ";   // appended after structure + type digits

    private readonly List<FieldDef> _definitions = new();
    private readonly List<(string Parent, string Child)> _tagPairs = new();
    private readonly List<List<(string Tag, byte[] Data)>> _records = new();

    private sealed record FieldDef(string Tag, string Name, string ArrayDescriptor, string FormatControls, int Structure, int DataType);

    public Iso8211TestWriter Define(string tag, string name, string arrayDescriptor, string formatControls,
                                    int structure = 1, int dataType = 6)
    {
        _definitions.Add(new FieldDef(tag, name, arrayDescriptor, formatControls, structure, dataType));
        return this;
    }

    public Iso8211TestWriter Tree(string parent, string child)
    {
        _tagPairs.Add((parent, child));
        return this;
    }

    public Iso8211TestWriter Record(params (string Tag, byte[] Data)[] fields)
    {
        _records.Add(fields.ToList());
        return this;
    }

    public void Save(string path)
    {
        using var fs = File.Create(path);
        Save(fs);
    }

    public void Save(Stream output)
    {
        // Data descriptive record.
        var ddrFields = new List<(string, byte[])> { (Iso8211Constants.FieldControlFieldTag, BuildFieldControlField()) };
        foreach (var def in _definitions) ddrFields.Add((def.Tag, BuildFieldDefinition(def)));

        byte[] ddr = BuildRecord('L', ddrFields, isDdr: true);
        output.Write(ddr);

        foreach (var record in _records)
            output.Write(BuildRecord('D', record, isDdr: false));
    }

    private byte[] BuildFieldControlField()
    {
        var buffer = new List<byte>();
        buffer.AddRange(A("0000;&   "));                         // field controls of the FCF itself
        buffer.AddRange(A(""));                                   // external file title
        buffer.Add(Iso8211Constants.UnitTerminator);
        foreach (var (parent, child) in _tagPairs)
        {
            buffer.AddRange(A(parent.PadRight(SizeTag)));
            buffer.AddRange(A(child.PadRight(SizeTag)));
        }
        buffer.Add(Iso8211Constants.FieldTerminator);
        return buffer.ToArray();
    }

    private static byte[] BuildFieldDefinition(FieldDef def)
    {
        var buffer = new List<byte>();
        buffer.AddRange(A($"{def.Structure}{def.DataType}{NormalFieldControls9}"));
        buffer.AddRange(Encoding.UTF8.GetBytes(def.Name));
        buffer.Add(Iso8211Constants.UnitTerminator);
        buffer.AddRange(Encoding.UTF8.GetBytes(def.ArrayDescriptor));
        buffer.Add(Iso8211Constants.UnitTerminator);
        buffer.AddRange(A(def.FormatControls));
        buffer.Add(Iso8211Constants.FieldTerminator);
        return buffer.ToArray();
    }

    private static byte[] BuildRecord(char leaderIdentifier, List<(string Tag, byte[] Data)> fields, bool isDdr)
    {
        int entryLength = SizeTag + SizeLength + SizePosition;
        int directoryLength = fields.Count * entryLength + 1;      // + field terminator
        int baseAddress = Iso8211Constants.LeaderLength + directoryLength;

        var directory = new List<byte>(directoryLength);
        int position = 0;
        foreach (var (tag, data) in fields)
        {
            directory.AddRange(A(tag.PadRight(SizeTag)));
            directory.AddRange(A(data.Length.ToString().PadLeft(SizeLength, '0')));
            directory.AddRange(A(position.ToString().PadLeft(SizePosition, '0')));
            position += data.Length;
        }
        directory.Add(Iso8211Constants.FieldTerminator);

        int recordLength = baseAddress + position;

        var leader = new StringBuilder(24);
        leader.Append(recordLength.ToString().PadLeft(5, '0'));
        leader.Append(isDdr ? ' ' : ' ');                          // interchange level / reserved
        leader.Append(leaderIdentifier);
        leader.Append(isDdr ? 'E' : ' ');                          // in-line code extension indicator
        leader.Append(isDdr ? '1' : ' ');                          // version number
        leader.Append(' ');                                        // application indicator
        leader.Append(isDdr ? "09" : "  ");                        // field control length
        leader.Append(baseAddress.ToString().PadLeft(5, '0'));
        leader.Append(isDdr ? "   " : "   ");                      // extended character set indicator
        leader.Append(SizeLength);
        leader.Append(SizePosition);
        leader.Append(' ');                                        // reserved
        leader.Append(SizeTag);

        var buffer = new List<byte>(recordLength);
        buffer.AddRange(A(leader.ToString()));
        buffer.AddRange(directory);
        foreach (var (_, data) in fields) buffer.AddRange(data);
        return buffer.ToArray();
    }

    private static byte[] A(string s)
    {
        var bytes = new byte[s.Length];
        for (int i = 0; i < s.Length; i++) bytes[i] = (byte)s[i];
        return bytes;
    }
}

/// <summary>Fluent builder for the byte content of a single field.</summary>
public sealed class FieldBuilder
{
    private readonly List<byte> _bytes = new();

    public static FieldBuilder New() => new();

    /// <summary>b11 - unsigned 1-byte integer.</summary>
    public FieldBuilder B11(long v) { _bytes.Add((byte)v); return this; }

    /// <summary>b12 - unsigned 2-byte integer, least significant byte first.</summary>
    public FieldBuilder B12(long v) { _bytes.AddRange(BitConverter.GetBytes((ushort)v)); return this; }

    /// <summary>b14 - unsigned 4-byte integer.</summary>
    public FieldBuilder B14(long v) { _bytes.AddRange(BitConverter.GetBytes((uint)v)); return this; }

    /// <summary>b24 - signed 4-byte integer.</summary>
    public FieldBuilder B24(long v) { _bytes.AddRange(BitConverter.GetBytes((int)v)); return this; }

    /// <summary>b48 - 8-byte IEEE double.</summary>
    public FieldBuilder B48(double v) { _bytes.AddRange(BitConverter.GetBytes(v)); return this; }

    /// <summary>Variable-length text, unit terminated.</summary>
    public FieldBuilder Text(string v)
    {
        _bytes.AddRange(Encoding.UTF8.GetBytes(v));
        _bytes.Add(Iso8211Constants.UnitTerminator);
        return this;
    }

    /// <summary>Fixed-width text, padded or truncated to <paramref name="width"/>.</summary>
    public FieldBuilder Text(string v, int width)
    {
        string s = v.Length > width ? v[..width] : v.PadRight(width);
        _bytes.AddRange(Encoding.UTF8.GetBytes(s));
        return this;
    }

    /// <summary>Closes the field with a field terminator and returns the bytes.</summary>
    public byte[] End()
    {
        var copy = new List<byte>(_bytes) { Iso8211Constants.FieldTerminator };
        return copy.ToArray();
    }
}
