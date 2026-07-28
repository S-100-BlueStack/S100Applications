using System.Text.Json;

namespace S100.Iso8211.Json;

/// <summary>
/// Streams a complete, lossless JSON rendering of an ISO/IEC 8211 file: the DDR (leader, field
/// control field, every data descriptive field) followed by every data record.
/// </summary>
/// <remarks>
/// Field value shape:
/// <list type="bullet">
/// <item>a field whose only group repeats becomes a JSON array of objects (ATTR, SPAS, C2IT, ...);</item>
/// <item>a field with a single non-repeating group becomes a JSON object (FRID, FOID, DSID, ...);</item>
/// <item>a concatenated field becomes an object holding the fixed subfields plus a
/// <c>"values"</c> array for the repeating tail (C3IL and friends).</item>
/// </list>
/// </remarks>
public static class Iso8211JsonWriter
{
    public static void Write(Iso8211Reader reader, Stream output, bool indented = true, string? sourceName = null)
    {
        using var writer = new Utf8JsonWriter(output, new JsonWriterOptions
        {
            Indented = indented,
            SkipValidation = false
        });

        writer.WriteStartObject();
        if (sourceName is not null) writer.WriteString("source", sourceName);

        WriteDdr(writer, reader.Ddr);

        writer.WriteStartArray("records");
        foreach (var record in reader.ReadRecords()) WriteRecord(writer, record);
        writer.WriteEndArray();

        writer.WriteEndObject();
        writer.Flush();
    }

    public static void WriteToFile(Iso8211Reader reader, string path, bool indented = true)
    {
        using var fs = File.Create(path);
        Write(reader, fs, indented, Path.GetFileName(path));
    }

    private static void WriteDdr(Utf8JsonWriter w, DataDescriptiveRecord ddr)
    {
        w.WriteStartObject("ddr");

        w.WriteStartObject("leader");
        w.WriteNumber("recordLength", ddr.Leader.RecordLength);
        w.WriteString("interchangeLevel", ddr.Leader.InterchangeLevel.ToString());
        w.WriteString("leaderIdentifier", ddr.Leader.LeaderIdentifier.ToString());
        w.WriteString("inlineCodeExtensionIndicator", ddr.Leader.InlineCodeExtensionIndicator.ToString());
        w.WriteString("versionNumber", ddr.Leader.VersionNumber.ToString());
        w.WriteString("applicationIndicator", ddr.Leader.ApplicationIndicator.ToString());
        w.WriteNumber("fieldControlLength", ddr.Leader.FieldControlLength);
        w.WriteNumber("baseAddressOfFieldArea", ddr.Leader.BaseAddressOfFieldArea);
        w.WriteString("extendedCharacterSetIndicator", ddr.Leader.ExtendedCharacterSetIndicator);
        w.WriteNumber("sizeOfFieldLength", ddr.Leader.SizeOfFieldLength);
        w.WriteNumber("sizeOfFieldPosition", ddr.Leader.SizeOfFieldPosition);
        w.WriteNumber("sizeOfFieldTag", ddr.Leader.SizeOfFieldTag);
        w.WriteEndObject();

        if (ddr.FieldControlField is { } fcf)
        {
            w.WriteStartObject("fieldControlField");
            w.WriteString("tag", fcf.Tag);
            w.WriteString("externalFileTitle", fcf.Name);
            w.WriteString("fieldControls", fcf.Controls.Raw);
            w.WriteStartArray("tagPairs");
            foreach (var (parent, child) in fcf.TagPairs)
            {
                w.WriteStartObject();
                w.WriteString("parent", parent);
                w.WriteString("child", child);
                w.WriteEndObject();
            }
            w.WriteEndArray();
            w.WriteEndObject();
        }

        w.WriteStartArray("fields");
        foreach (var def in ddr.FieldDefinitions.Values) WriteFieldDefinition(w, def);
        w.WriteEndArray();

        w.WriteEndObject();
    }

    private static void WriteFieldDefinition(Utf8JsonWriter w, FieldDefinition def)
    {
        w.WriteStartObject();
        w.WriteString("tag", def.Tag);
        w.WriteString("name", def.Name);
        w.WriteString("structure", def.Controls.Structure.ToString());
        w.WriteString("dataType", def.Controls.DataType.ToString());
        w.WriteString("truncatedEscapeSequence", def.Controls.TruncatedEscapeSequence);
        w.WriteString("encoding", def.Encoding.WebName);
        w.WriteString("arrayDescriptor", def.ArrayDescriptor);
        w.WriteString("formatControls", def.FormatControls);

        w.WriteStartArray("groups");
        foreach (var g in def.Groups)
        {
            w.WriteStartObject();
            w.WriteBoolean("repeats", g.Repeats);
            w.WriteStartArray("subfields");
            foreach (var sf in g.Subfields)
            {
                w.WriteStartObject();
                w.WriteString("label", sf.Label);
                w.WriteString("format", sf.Format.Raw);
                w.WriteString("kind", sf.Format.Kind.ToString());
                if (sf.Format.Width > 0) w.WriteNumber("width", sf.Format.Width);
                w.WriteEndObject();
            }
            w.WriteEndArray();
            w.WriteEndObject();
        }
        w.WriteEndArray();

        w.WriteEndObject();
    }

    private static void WriteRecord(Utf8JsonWriter w, DataRecord record)
    {
        w.WriteStartObject();
        w.WriteNumber("ordinal", record.Ordinal);
        w.WriteNumber("recordLength", record.Leader.RecordLength);

        w.WriteStartArray("fields");
        foreach (var field in record.Fields)
        {
            w.WriteStartObject();
            w.WriteString("tag", field.Tag);
            w.WriteString("name", field.Definition.Name);
            w.WritePropertyName("value");
            WriteFieldValue(w, field);
            w.WriteEndObject();
        }
        w.WriteEndArray();

        w.WriteEndObject();
    }

    /// <summary>Writes a field value using the shape documented on the class.</summary>
    public static void WriteFieldValue(Utf8JsonWriter w, DataField field)
    {
        bool onlyRepeating = field.Groups.Count == 1 && field.Groups[0].Repeats;

        if (onlyRepeating)
        {
            w.WriteStartArray();
            foreach (var inst in field.Groups[0].Instances) WriteInstance(w, inst);
            w.WriteEndArray();
            return;
        }

        w.WriteStartObject();
        foreach (var group in field.Groups)
        {
            if (!group.Repeats)
            {
                foreach (var inst in group.Instances)
                    foreach (var v in inst.Values) WriteNamedValue(w, v);
            }
            else
            {
                w.WriteStartArray("values");
                foreach (var inst in group.Instances) WriteInstance(w, inst);
                w.WriteEndArray();
            }
        }
        w.WriteEndObject();
    }

    private static void WriteInstance(Utf8JsonWriter w, SubfieldGroupInstance instance)
    {
        w.WriteStartObject();
        foreach (var v in instance.Values) WriteNamedValue(w, v);
        w.WriteEndObject();
    }

    private static void WriteNamedValue(Utf8JsonWriter w, SubfieldValue value)
    {
        w.WritePropertyName(value.Label);
        switch (value.Value)
        {
            case null: w.WriteNullValue(); break;
            case string s: w.WriteStringValue(s); break;
            case long l: w.WriteNumberValue(l); break;
            case ulong u: w.WriteNumberValue(u); break;
            case int i: w.WriteNumberValue(i); break;
            case double d when double.IsFinite(d): w.WriteNumberValue(d); break;
            case double: w.WriteNullValue(); break;
            case byte[] b: w.WriteStringValue(Convert.ToHexString(b)); break;
            default: w.WriteStringValue(value.AsString()); break;
        }
    }
}
