using System.Text;

namespace S100.Iso8211;

public enum SubfieldKind
{
    /// <summary>A(n) - graphic characters.</summary>
    Text,
    /// <summary>I(n) - integer represented as characters.</summary>
    IntegerText,
    /// <summary>R(n) - real represented as characters.</summary>
    RealText,
    /// <summary>B(n) - bit string of n bits.</summary>
    BitString,
    /// <summary>b(t)(w) - binary form.</summary>
    Binary,
    /// <summary>X(n) - filler, n bytes discarded.</summary>
    Filler
}

public enum BinaryKind
{
    UnsignedInteger = 1,
    SignedInteger = 2,
    FixedPointReal = 3,
    FloatingPointReal = 4,
    FloatingPointComplex = 5
}

/// <summary>One format control, e.g. <c>A(2)</c>, <c>I(5)</c>, <c>b14</c>, <c>b48</c>, <c>B(40)</c>.</summary>
public sealed class SubfieldFormat
{
    public string Raw { get; init; } = string.Empty;
    public SubfieldKind Kind { get; init; }

    /// <summary>Width in bytes. Zero means variable length, delimited by UT or FT.</summary>
    public int Width { get; init; }

    /// <summary>Length in bits, for <see cref="SubfieldKind.BitString"/>.</summary>
    public int BitLength { get; init; }

    public BinaryKind BinaryKind { get; init; }

    public bool IsVariableLength => Width == 0 && Kind is SubfieldKind.Text or SubfieldKind.IntegerText or SubfieldKind.RealText;

    /// <summary>The default used when a field declares subfield labels but no format controls.</summary>
    public static SubfieldFormat VariableText { get; } =
        new() { Raw = "A", Kind = SubfieldKind.Text, Width = 0 };

    public override string ToString() => Raw;
}

/// <summary>
/// Parses an ISO/IEC 8211 format-controls string such as <c>(b11,b14,2b12,b11)</c> or
/// <c>(A(2),I(5),3(b11,A))</c> into a flat, ordered list of <see cref="SubfieldFormat"/>.
/// Repeat counts and nested groups are expanded.
/// </summary>
public static class FormatControlsParser
{
    private const int MaxExpandedFormats = 4096;

    public static IReadOnlyList<SubfieldFormat> Parse(string? formatControls)
    {
        if (string.IsNullOrWhiteSpace(formatControls)) return Array.Empty<SubfieldFormat>();

        string s = formatControls.Trim().TrimEnd((char)Iso8211Constants.FieldTerminator);
        int pos = 0;
        var result = new List<SubfieldFormat>();

        SkipWhitespace(s, ref pos);
        if (pos < s.Length && s[pos] == '(')
        {
            ParseGroup(s, ref pos, result);
        }
        else
        {
            // Tolerate a bare list without the surrounding parentheses.
            ParseList(s, ref pos, result, terminator: '\0');
        }

        return result;
    }

    private static void ParseGroup(string s, ref int pos, List<SubfieldFormat> output)
    {
        Expect(s, ref pos, '(');
        ParseList(s, ref pos, output, terminator: ')');
        Expect(s, ref pos, ')');
    }

    private static void ParseList(string s, ref int pos, List<SubfieldFormat> output, char terminator)
    {
        while (true)
        {
            SkipWhitespace(s, ref pos);
            if (pos >= s.Length) return;
            if (s[pos] == terminator) return;

            int repeat = ReadRepeatCount(s, ref pos);
            SkipWhitespace(s, ref pos);
            if (pos >= s.Length) return;

            if (s[pos] == '(')
            {
                var nested = new List<SubfieldFormat>();
                ParseGroup(s, ref pos, nested);
                for (int i = 0; i < repeat; i++) AddRange(output, nested);
            }
            else
            {
                SubfieldFormat item = ReadFormatCode(s, ref pos);
                for (int i = 0; i < repeat; i++) Add(output, item);
            }

            SkipWhitespace(s, ref pos);
            if (pos < s.Length && s[pos] == ',') { pos++; continue; }
            return;
        }
    }

    private static SubfieldFormat ReadFormatCode(string s, ref int pos)
    {
        char code = s[pos++];

        // Binary: b<type><width>, e.g. b11 (1-byte unsigned), b24 (4-byte signed), b48 (8-byte real).
        if (code is 'b' && pos + 1 < s.Length + 1 && pos < s.Length && char.IsDigit(s[pos]))
        {
            int type = s[pos++] - '0';
            int width = 0;
            while (pos < s.Length && char.IsDigit(s[pos])) width = width * 10 + (s[pos++] - '0');
            return new SubfieldFormat
            {
                Raw = $"b{type}{width}",
                Kind = SubfieldKind.Binary,
                Width = width,
                BinaryKind = (BinaryKind)type
            };
        }

        int? parenWidth = ReadParenthesisedWidth(s, ref pos);

        return char.ToUpperInvariant(code) switch
        {
            'A' or 'C' or 'S' => new SubfieldFormat
            {
                Raw = parenWidth is null ? code.ToString() : $"{code}({parenWidth})",
                Kind = SubfieldKind.Text,
                Width = parenWidth ?? 0
            },
            'I' => new SubfieldFormat
            {
                Raw = parenWidth is null ? "I" : $"I({parenWidth})",
                Kind = SubfieldKind.IntegerText,
                Width = parenWidth ?? 0
            },
            'R' => new SubfieldFormat
            {
                Raw = parenWidth is null ? "R" : $"R({parenWidth})",
                Kind = SubfieldKind.RealText,
                Width = parenWidth ?? 0
            },
            'B' => new SubfieldFormat
            {
                Raw = $"B({parenWidth ?? 0})",
                Kind = SubfieldKind.BitString,
                BitLength = parenWidth ?? 0,
                Width = ((parenWidth ?? 0) + 7) / 8
            },
            'X' => new SubfieldFormat
            {
                Raw = $"X({parenWidth ?? 1})",
                Kind = SubfieldKind.Filler,
                Width = parenWidth ?? 1
            },
            _ => throw new Iso8211Exception($"Unsupported format control '{code}' in \"{s}\".")
        };
    }

    private static int? ReadParenthesisedWidth(string s, ref int pos)
    {
        SkipWhitespace(s, ref pos);
        if (pos >= s.Length || s[pos] != '(') return null;
        pos++;
        int width = 0;
        bool any = false;
        while (pos < s.Length && char.IsDigit(s[pos])) { width = width * 10 + (s[pos++] - '0'); any = true; }
        Expect(s, ref pos, ')');
        return any ? width : null;
    }

    private static int ReadRepeatCount(string s, ref int pos)
    {
        int start = pos;
        int value = 0;
        while (pos < s.Length && char.IsDigit(s[pos])) value = value * 10 + (s[pos++] - '0');
        return pos == start ? 1 : Math.Max(value, 1);
    }

    private static void Expect(string s, ref int pos, char c)
    {
        SkipWhitespace(s, ref pos);
        if (pos >= s.Length || s[pos] != c)
            throw new Iso8211Exception($"Malformed format controls \"{s}\": expected '{c}' at offset {pos}.");
        pos++;
    }

    private static void SkipWhitespace(string s, ref int pos)
    {
        while (pos < s.Length && char.IsWhiteSpace(s[pos])) pos++;
    }

    private static void Add(List<SubfieldFormat> list, SubfieldFormat item)
    {
        if (list.Count >= MaxExpandedFormats)
            throw new Iso8211Exception("Format controls expand to an implausible number of subfields.");
        list.Add(item);
    }

    private static void AddRange(List<SubfieldFormat> list, List<SubfieldFormat> items)
    {
        foreach (var i in items) Add(list, i);
    }
}
