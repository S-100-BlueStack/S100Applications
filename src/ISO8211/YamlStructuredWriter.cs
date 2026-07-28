using System.Globalization;
using System.Text;

namespace S100.Iso8211.Serialization;

/// <summary>
/// A streaming block-style YAML 1.2 emitter. It deliberately supports only what the document
/// writers produce - nested mappings, sequences and scalars - which keeps it small enough to have
/// no third-party dependency and to be auditable.
/// </summary>
/// <remarks>
/// Scalars are emitted unquoted only when that is unambiguously safe; anything else is
/// double-quoted. In particular a text value that looks like a number, a boolean or a null
/// (<c>"340"</c>, <c>"true"</c>, <c>"~"</c>) is quoted, so the string/number distinction carried by
/// the ISO 8211 subfield formats survives a YAML round trip.
/// </remarks>
public sealed class YamlStructuredWriter : IStructuredWriter
{
    private const int IndentStep = 2;

    /// <summary>
    /// <see cref="Document"/> is the implicit root. It holds exactly one value, written at
    /// indent zero with no key and no dash.
    /// </summary>
    private enum ContainerKind { Document, Mapping, Sequence }

    private sealed class Frame
    {
        public ContainerKind Kind;
        public int Indent;
        public bool HasItems;

        /// <summary>Header text not yet written, so an empty container can collapse to <c>{}</c> or <c>[]</c>.</summary>
        public string? PendingHeader;

        /// <summary>True when the pending header is a sequence dash the first child line must continue.</summary>
        public bool PendingHeaderIsInline;
    }

    private static readonly HashSet<string> ReservedPlainScalars = new(StringComparer.OrdinalIgnoreCase)
    {
        "y", "n", "yes", "no", "true", "false", "on", "off", "null", "~"
    };

    private readonly TextWriter _writer;
    private readonly Stack<Frame> _frames = new();
    private string? _pendingPropertyName;

    public YamlStructuredWriter(Stream stream)
        : this(new StreamWriter(stream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false), 1 << 16, leaveOpen: true))
    {
    }

    public YamlStructuredWriter(TextWriter writer)
    {
        _writer = writer ?? throw new ArgumentNullException(nameof(writer));
        _writer.NewLine = "\n";
        _frames.Push(new Frame { Kind = ContainerKind.Document, Indent = 0 });
    }

    // ------------------------------------------------------------------ containers

    public void WriteStartObject() => StartContainer(ContainerKind.Mapping);
    public void WriteStartObject(string propertyName) { WritePropertyName(propertyName); StartContainer(ContainerKind.Mapping); }
    public void WriteEndObject() => EndContainer(ContainerKind.Mapping);

    public void WriteStartArray() => StartContainer(ContainerKind.Sequence);
    public void WriteStartArray(string propertyName) { WritePropertyName(propertyName); StartContainer(ContainerKind.Sequence); }
    public void WriteEndArray() => EndContainer(ContainerKind.Sequence);

    public void WritePropertyName(string propertyName) => _pendingPropertyName = propertyName;

    private void StartContainer(ContainerKind kind)
    {
        Frame parent = _frames.Peek();
        string linePrefix = BeginLine(parent);

        string? header;
        bool inline;
        int indent;

        switch (parent.Kind)
        {
            case ContainerKind.Document:
                header = null;                 // the root value carries no key and no dash
                inline = false;
                indent = 0;
                break;

            case ContainerKind.Mapping:
                header = linePrefix + FormatKey(TakePropertyName()) + ":";
                inline = false;
                indent = parent.Indent + IndentStep;
                break;

            default:
                header = linePrefix + "-";
                inline = true;
                indent = parent.Indent + IndentStep;
                break;
        }

        _frames.Push(new Frame
        {
            Kind = kind,
            Indent = indent,
            PendingHeader = header,
            PendingHeaderIsInline = inline
        });
    }

    private void EndContainer(ContainerKind kind)
    {
        if (_frames.Count <= 1)
            throw new InvalidOperationException("No open container to close.");

        Frame frame = _frames.Pop();

        if (frame.Kind != kind)
            throw new InvalidOperationException($"Closing a {kind} but a {frame.Kind} is open.");

        // Nothing was ever written into it, so collapse to an explicit empty flow collection.
        if (frame.HasItems) return;

        string empty = kind == ContainerKind.Mapping ? "{}" : "[]";
        _writer.WriteLine(frame.PendingHeader is null ? empty : $"{frame.PendingHeader} {empty}");
    }

    // ------------------------------------------------------------------ scalars

    public void WriteString(string propertyName, string? value) { WritePropertyName(propertyName); WriteStringValue(value); }
    public void WriteNumber(string propertyName, long value) { WritePropertyName(propertyName); WriteNumberValue(value); }
    public void WriteNumber(string propertyName, ulong value) { WritePropertyName(propertyName); WriteNumberValue(value); }
    public void WriteNumber(string propertyName, double value) { WritePropertyName(propertyName); WriteNumberValue(value); }
    public void WriteBoolean(string propertyName, bool value) { WritePropertyName(propertyName); WriteBooleanValue(value); }
    public void WriteNull(string propertyName) { WritePropertyName(propertyName); WriteNullValue(); }

    public void WriteStringValue(string? value) =>
        WriteScalar(value is null ? "null" : FormatString(value));

    public void WriteNumberValue(long value) => WriteScalar(value.ToString(CultureInfo.InvariantCulture));
    public void WriteNumberValue(ulong value) => WriteScalar(value.ToString(CultureInfo.InvariantCulture));
    public void WriteBooleanValue(bool value) => WriteScalar(value ? "true" : "false");
    public void WriteNullValue() => WriteScalar("null");

    public void WriteNumberValue(double value)
    {
        if (double.IsNaN(value)) { WriteScalar(".nan"); return; }
        if (double.IsPositiveInfinity(value)) { WriteScalar(".inf"); return; }
        if (double.IsNegativeInfinity(value)) { WriteScalar("-.inf"); return; }
        WriteScalar(value.ToString("R", CultureInfo.InvariantCulture));
    }

    private void WriteScalar(string scalar)
    {
        Frame frame = _frames.Peek();
        string linePrefix = BeginLine(frame);

        _writer.WriteLine(frame.Kind switch
        {
            ContainerKind.Mapping => $"{linePrefix}{FormatKey(TakePropertyName())}: {scalar}",
            ContainerKind.Sequence => $"{linePrefix}- {scalar}",
            _ => $"{linePrefix}{scalar}"       // a bare scalar document
        });
    }

    // ------------------------------------------------------------------ line handling

    /// <summary>
    /// Produces the text a new item's line starts with, flushing the parent's deferred header the
    /// first time the container receives content.
    /// </summary>
    private string BeginLine(Frame frame)
    {
        if (!frame.HasItems && frame.PendingHeader is not null)
        {
            string header = frame.PendingHeader;
            frame.PendingHeader = null;
            frame.HasItems = true;

            if (frame.PendingHeaderIsInline) return header + " ";

            _writer.WriteLine(header);
            return new string(' ', frame.Indent);
        }

        frame.HasItems = true;
        return new string(' ', frame.Indent);
    }

    private string TakePropertyName()
    {
        string name = _pendingPropertyName
                      ?? throw new InvalidOperationException("A mapping entry was written without a property name.");
        _pendingPropertyName = null;
        return name;
    }

    // ------------------------------------------------------------------ scalar formatting

    private static string FormatKey(string key) => FormatString(key);

    internal static string FormatString(string value) =>
        IsSafePlainScalar(value) ? value : Quote(value);

    /// <summary>
    /// Conservative test for plain (unquoted) style. Requiring a letter or underscore first rules
    /// out every numeric, timestamp and indicator form in one go.
    /// </summary>
    private static bool IsSafePlainScalar(string value)
    {
        if (value.Length == 0) return false;
        if (char.IsWhiteSpace(value[0]) || char.IsWhiteSpace(value[^1])) return false;
        if (!char.IsLetter(value[0]) && value[0] != '_') return false;
        if (ReservedPlainScalars.Contains(value)) return false;

        foreach (char c in value)
        {
            if (char.IsLetterOrDigit(c)) continue;
            if (c is ' ' or '_' or '.' or '-' or '/' or '(' or ')' or '+' or '@') continue;
            return false;
        }

        return true;
    }

    private static string Quote(string value)
    {
        var builder = new StringBuilder(value.Length + 2);
        builder.Append('"');

        foreach (char c in value)
        {
            switch (c)
            {
                case '\\': builder.Append("\\\\"); break;
                case '"': builder.Append("\\\""); break;
                case '\n': builder.Append("\\n"); break;
                case '\r': builder.Append("\\r"); break;
                case '\t': builder.Append("\\t"); break;
                case '\0': builder.Append("\\0"); break;
                case '\a': builder.Append("\\a"); break;
                case '\b': builder.Append("\\b"); break;
                case '\f': builder.Append("\\f"); break;
                case '\v': builder.Append("\\v"); break;
                case '\u001b': builder.Append("\\e"); break;
                default:
                    if (c < 0x20 || c == 0x7F || c == 0x85 || c == '\u2028' || c == '\u2029')
                        builder.Append("\\u").Append(((int)c).ToString("X4", CultureInfo.InvariantCulture));
                    else
                        builder.Append(c);
                    break;
            }
        }

        builder.Append('"');
        return builder.ToString();
    }

    // ------------------------------------------------------------------ lifetime

    public void Flush() => _writer.Flush();

    public void Dispose()
    {
        // An entirely empty document still has to be valid YAML.
        if (_frames.Count == 1 && !_frames.Peek().HasItems) _writer.WriteLine("{}");
        _writer.Flush();
        _writer.Dispose();
    }
}
