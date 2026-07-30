using System.Text.Json;

namespace S100.Iso8211.Serialization;

/// <summary>JSON implementation of <see cref="IStructuredWriter"/>, backed by <see cref="Utf8JsonWriter"/>.</summary>
public sealed class JsonStructuredWriter : IStructuredWriter
{
    private readonly Utf8JsonWriter _writer;

    public JsonStructuredWriter(Stream stream, bool indented = true)
    {
        _writer = new Utf8JsonWriter(stream, new JsonWriterOptions
        {
            Indented = indented,
            SkipValidation = false
        });
    }

    public void WriteStartObject() => _writer.WriteStartObject();
    public void WriteStartObject(string propertyName) => _writer.WriteStartObject(propertyName);
    public void WriteEndObject() => _writer.WriteEndObject();

    public void WriteStartArray() => _writer.WriteStartArray();
    public void WriteStartArray(string propertyName) => _writer.WriteStartArray(propertyName);
    public void WriteEndArray() => _writer.WriteEndArray();

    public void WritePropertyName(string propertyName) => _writer.WritePropertyName(propertyName);

    public void WriteString(string propertyName, string? value) => _writer.WriteString(propertyName, value);
    public void WriteNumber(string propertyName, long value) => _writer.WriteNumber(propertyName, value);
    public void WriteNumber(string propertyName, ulong value) => _writer.WriteNumber(propertyName, value);
    public void WriteNumber(string propertyName, double value) => _writer.WriteNumber(propertyName, value);
    public void WriteBoolean(string propertyName, bool value) => _writer.WriteBoolean(propertyName, value);
    public void WriteNull(string propertyName) => _writer.WriteNull(propertyName);

    public void WriteStringValue(string? value) => _writer.WriteStringValue(value);
    public void WriteNumberValue(long value) => _writer.WriteNumberValue(value);
    public void WriteNumberValue(ulong value) => _writer.WriteNumberValue(value);
    public void WriteNumberValue(double value) => _writer.WriteNumberValue(value);
    public void WriteBooleanValue(bool value) => _writer.WriteBooleanValue(value);
    public void WriteNullValue() => _writer.WriteNullValue();

    public void Flush() => _writer.Flush();

    public void Dispose() => _writer.Dispose();
}
