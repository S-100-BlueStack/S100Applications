using System.Text;

namespace S100.Iso8211;

/// <summary>Control characters and fixed sizes defined by ISO/IEC 8211:1994.</summary>
public static class Iso8211Constants
{
    /// <summary>Field terminator (FT / RS), 0x1E.</summary>
    public const byte FieldTerminator = 0x1E;

    /// <summary>Unit terminator (UT / US), 0x1F.</summary>
    public const byte UnitTerminator = 0x1F;

    /// <summary>Every record leader is exactly 24 bytes.</summary>
    public const int LeaderLength = 24;

    /// <summary>Tag of the field control field in the DDR (all zeroes, tag-length wide).</summary>
    public const string FieldControlFieldTag = "0000";

    /// <summary>Tag of the record identifier field that opens every S-100 data record.</summary>
    public const string RecordIdentifierFieldTag = "0001";
}

public sealed class Iso8211Exception : Exception
{
    public Iso8211Exception(string message) : base(message) { }
    public Iso8211Exception(string message, Exception inner) : base(message, inner) { }
}

/// <summary>Data structure code, byte 0 of the field controls.</summary>
public enum DataStructureCode
{
    Elementary = 0,
    Linear = 1,
    Cartesian = 2,
    Concatenated = 3,
    Unknown = -1
}

/// <summary>Data type code, byte 1 of the field controls.</summary>
public enum DataTypeCode
{
    CharacterString = 0,
    ImplicitPoint = 1,
    ExplicitPoint = 2,
    ExplicitPointScaled = 3,
    CharacterModeBitString = 4,
    BitString = 5,
    MixedDataTypes = 6,
    Unknown = -1
}

/// <summary>
/// The 24-byte leader that opens every logical record. The DDR leader (identifier 'L') carries
/// file-wide descriptive parameters; DR leaders ('D', or 'R' when the previous directory is reused)
/// only carry the base address and the entry map.
/// </summary>
public sealed class Iso8211Leader
{
    public int RecordLength { get; init; }
    public char InterchangeLevel { get; init; }
    public char LeaderIdentifier { get; init; }
    public char InlineCodeExtensionIndicator { get; init; }
    public char VersionNumber { get; init; }
    public char ApplicationIndicator { get; init; }
    public int FieldControlLength { get; init; }
    public int BaseAddressOfFieldArea { get; init; }
    public string ExtendedCharacterSetIndicator { get; init; } = "   ";
    public int SizeOfFieldLength { get; init; }
    public int SizeOfFieldPosition { get; init; }
    public int SizeOfFieldTag { get; init; }

    public bool IsDataDescriptiveRecord => LeaderIdentifier is 'L';
    public bool ReusesPreviousDirectory => LeaderIdentifier is 'R';
    public int DirectoryEntryLength => SizeOfFieldTag + SizeOfFieldLength + SizeOfFieldPosition;

    public static Iso8211Leader Parse(ReadOnlySpan<byte> b)
    {
        if (b.Length < Iso8211Constants.LeaderLength)
            throw new Iso8211Exception($"Leader truncated: {b.Length} bytes, expected 24.");

        var leader = new Iso8211Leader
        {
            RecordLength = Ascii.ReadInt(b.Slice(0, 5), 0),
            InterchangeLevel = (char)b[5],
            LeaderIdentifier = (char)b[6],
            InlineCodeExtensionIndicator = (char)b[7],
            VersionNumber = (char)b[8],
            ApplicationIndicator = (char)b[9],
            FieldControlLength = Ascii.ReadInt(b.Slice(10, 2), 0),
            BaseAddressOfFieldArea = Ascii.ReadInt(b.Slice(12, 5), 0),
            ExtendedCharacterSetIndicator = Ascii.ReadString(b.Slice(17, 3)),
            SizeOfFieldLength = Ascii.ReadInt(b.Slice(20, 1), 0),
            SizeOfFieldPosition = Ascii.ReadInt(b.Slice(21, 1), 0),
            SizeOfFieldTag = Ascii.ReadInt(b.Slice(23, 1), 0)
        };

        if (leader.LeaderIdentifier is not ('L' or 'D' or 'R'))
            throw new Iso8211Exception(
                $"Unexpected leader identifier '{leader.LeaderIdentifier}' - the stream is probably not ISO/IEC 8211.");

        if (leader.DirectoryEntryLength <= 0)
            throw new Iso8211Exception("Leader entry map is empty (tag/length/position sizes are all zero).");

        return leader;
    }
}

/// <summary>One directory entry: which tag lives where inside the field area.</summary>
public readonly record struct Iso8211DirectoryEntry(string Tag, int Length, int Position);

/// <summary>
/// The field controls that prefix every data descriptive field: structure and type codes, the
/// auxiliary controls, the printable graphics, and the truncated escape sequence that selects the
/// character set for text subfields.
/// </summary>
public sealed class FieldControls
{
    public string Raw { get; init; } = string.Empty;
    public DataStructureCode Structure { get; init; } = DataStructureCode.Unknown;
    public DataTypeCode DataType { get; init; } = DataTypeCode.Unknown;
    public string AuxiliaryControls { get; init; } = string.Empty;
    public string PrintableGraphics { get; init; } = string.Empty;
    public string TruncatedEscapeSequence { get; init; } = string.Empty;

    /// <summary>
    /// Character set implied by the truncated escape sequence. A blank sequence nominally means
    /// ISO 646, but UTF-8 is a strict superset of ASCII so it is a safe (and S-100 friendly) choice.
    /// </summary>
    public Encoding ResolveEncoding(Encoding fallback) => TruncatedEscapeSequence switch
    {
        "   " or "" => fallback,
        "-A " => Encoding.Latin1,
        "%/G" or "%/H" or "%/I" or "%/J" or "%/K" or "%/L" => Encoding.UTF8,
        "%/@" or "%/A" or "%/B" or "%/C" or "%/D" or "%/E" or "%/F" => Encoding.BigEndianUnicode,
        _ => fallback
    };

    public static FieldControls Parse(string raw)
    {
        static int Digit(string s, int i) => i < s.Length && s[i] is >= '0' and <= '9' ? s[i] - '0' : -1;

        return new FieldControls
        {
            Raw = raw,
            Structure = (DataStructureCode)Digit(raw, 0),
            DataType = (DataTypeCode)Digit(raw, 1),
            AuxiliaryControls = Slice(raw, 2, 2),
            PrintableGraphics = Slice(raw, 4, 2),
            TruncatedEscapeSequence = Slice(raw, 6, 3)
        };

        static string Slice(string s, int start, int len) =>
            start >= s.Length ? string.Empty : s.Substring(start, Math.Min(len, s.Length - start));
    }
}

internal static class Ascii
{
    public static string ReadString(ReadOnlySpan<byte> b)
    {
        Span<char> chars = stackalloc char[b.Length];
        for (int i = 0; i < b.Length; i++) chars[i] = (char)b[i];
        return new string(chars);
    }

    /// <summary>Reads a blank- or zero-padded ASCII integer, tolerating spaces and empty fields.</summary>
    public static int ReadInt(ReadOnlySpan<byte> b, int fallback)
    {
        long value = 0;
        bool any = false;
        foreach (byte x in b)
        {
            if (x is (byte)' ' or 0) continue;
            if (x is < (byte)'0' or > (byte)'9') return fallback;
            value = value * 10 + (x - '0');
            any = true;
        }
        return any ? (int)value : fallback;
    }
}
