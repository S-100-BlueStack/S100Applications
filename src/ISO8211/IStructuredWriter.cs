namespace S100.Iso8211.Serialization;

/// <summary>Serialisation formats supported by the document writers.</summary>
public enum OutputFormat
{
    /// <summary>Block-style YAML. The default.</summary>
    Yaml,

    /// <summary>JSON.</summary>
    Json
}

public static class OutputFormats
{
    /// <summary>
    /// Infers the format from a file extension: <c>.json</c> gives JSON, everything else - including
    /// <c>.yaml</c>, <c>.yml</c> and unknown extensions - gives YAML.
    /// </summary>
    public static OutputFormat FromPath(string? path)
    {
        string extension = Path.GetExtension(path ?? string.Empty);
        return extension.Equals(".json", StringComparison.OrdinalIgnoreCase)
               || extension.Equals(".geojson", StringComparison.OrdinalIgnoreCase)
            ? OutputFormat.Json
            : OutputFormat.Yaml;
    }

    /// <summary>The conventional extension for a format, including the leading dot.</summary>
    public static string Extension(this OutputFormat format) =>
        format == OutputFormat.Json ? ".json" : ".yaml";
}

/// <summary>
/// The minimal write surface both document writers need. It mirrors the shape of
/// <see cref="System.Text.Json.Utf8JsonWriter"/> so the serialisation logic is written once and
/// emitted as either JSON or YAML.
/// </summary>
public interface IStructuredWriter : IDisposable
{
    void WriteStartObject();
    void WriteStartObject(string propertyName);
    void WriteEndObject();

    void WriteStartArray();
    void WriteStartArray(string propertyName);
    void WriteEndArray();

    void WritePropertyName(string propertyName);

    void WriteString(string propertyName, string? value);
    void WriteNumber(string propertyName, long value);
    void WriteNumber(string propertyName, ulong value);
    void WriteNumber(string propertyName, double value);
    void WriteBoolean(string propertyName, bool value);
    void WriteNull(string propertyName);

    void WriteStringValue(string? value);
    void WriteNumberValue(long value);
    void WriteNumberValue(ulong value);
    void WriteNumberValue(double value);
    void WriteBooleanValue(bool value);
    void WriteNullValue();

    void Flush();
}

public static class StructuredWriterFactory
{
    /// <summary>
    /// Creates a writer for <paramref name="format"/> over <paramref name="stream"/>. The stream is
    /// flushed but never closed by the writer.
    /// </summary>
    /// <param name="stream">Destination stream.</param>
    /// <param name="format">Which serialisation to emit.</param>
    /// <param name="indented">Applies to JSON only; YAML block style is always indented.</param>
    public static IStructuredWriter Create(Stream stream, OutputFormat format, bool indented = true) =>
        format == OutputFormat.Json
            ? new JsonStructuredWriter(stream, indented)
            : new YamlStructuredWriter(stream);
}
