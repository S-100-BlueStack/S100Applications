namespace S100.Iso8211.Serialization;

/// <summary>
/// Streams a complete, lossless rendering of an ISO/IEC 8211 file - the DDR (leader, field control
/// field, every data descriptive field) followed by every data record - as YAML or JSON.
/// </summary>
/// <remarks>
/// Field value shape:
/// <list type="bullet">
/// <item>a field whose only group repeats becomes a sequence of mappings (ATTR, SPAS, C2IT, ...);</item>
/// <item>a field with a single non-repeating group becomes a mapping (FRID, FOID, DSID, ...);</item>
/// <item>a concatenated field becomes a mapping holding the fixed subfields plus a
/// <c>"values"</c> array for the repeating tail (C3IL and friends).</item>
/// </list>
/// </remarks>
public static class Iso8211DocumentWriter
{
    /// <param name="reader">An open reader positioned after the DDR.</param>
    /// <param name="output">Destination stream. It is flushed but not closed.</param>
    /// <param name="format">Defaults to YAML.</param>
    /// <param name="indented">Applies to JSON only; YAML block style is always indented.</param>
    /// <param name="sourceName">Recorded as the document's "source" property.</param>
    public static void Write(
        Iso8211Reader reader,
        Stream output,
        OutputFormat format = OutputFormat.Yaml,
        bool indented = true,
        string? sourceName = null)
    {
        using IStructuredWriter writer = StructuredWriterFactory.Create(output, format, indented);

        writer.WriteStartObject();
        if (sourceName is not null) writer.WriteString("source", sourceName);

        WriteDdr(writer, reader.Ddr);

        writer.WriteStartArray("records");
        foreach (var record in reader.ReadRecords()) WriteRecord(writer, record);
        writer.WriteEndArray();

        writer.WriteEndObject();
        writer.Flush();
    }

    /// <param name="reader">An open reader positioned after the DDR.</param>
    /// <param name="path">Destination file, created or overwritten.</param>
    /// <param name="format">When null, inferred from the file extension.</param>
    /// <param name="indented">Applies to JSON only; YAML block style is always indented.</param>
    public static void WriteToFile(
        Iso8211Reader reader, string path, OutputFormat? format = null, bool indented = true)
    {
        using var fs = File.Create(path);
        Write(reader, fs, format ?? OutputFormats.FromPath(path), indented, Path.GetFileName(path));
    }

    private static void WriteDdr(IStructuredWriter w, DataDescriptiveRecord ddr)
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

    private static void WriteFieldDefinition(IStructuredWriter w, FieldDefinition def)
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

    private static void WriteRecord(IStructuredWriter w, DataRecord record)
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
    public static void WriteFieldValue(IStructuredWriter w, DataField field)
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

    private static void WriteInstance(IStructuredWriter w, SubfieldGroupInstance instance)
    {
        w.WriteStartObject();
        foreach (var v in instance.Values) WriteNamedValue(w, v);
        w.WriteEndObject();
    }

    private static void WriteNamedValue(IStructuredWriter w, SubfieldValue value)
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
