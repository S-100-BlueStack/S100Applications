using S100.Iso8211.S101;
using S100.Iso8211.Serialization;

namespace S100.Iso8211.S57;

/// <summary>
/// Serialises an <see cref="S57Dataset"/> as YAML or JSON, in the same FeatureCollection shape the
/// S-101 writer produces, so downstream consumers do not have to care which product they were given.
/// </summary>
public static class S57DocumentWriter
{
    /// <param name="dataset">The dataset to serialise.</param>
    /// <param name="output">Destination stream. It is flushed but not closed.</param>
    /// <param name="format">Defaults to YAML.</param>
    /// <param name="indented">Applies to JSON only; YAML block style is always indented.</param>
    /// <param name="geoJsonOnly">Drop the header and the pointer detail, leaving a plain FeatureCollection.</param>
    public static void Write(
        S57Dataset dataset,
        Stream output,
        OutputFormat format = OutputFormat.Yaml,
        bool indented = true,
        bool geoJsonOnly = false)
    {
        using IStructuredWriter w = StructuredWriterFactory.Create(output, format, indented);

        w.WriteStartObject();
        w.WriteString("type", "FeatureCollection");
        w.WriteString("product", "S-57");
        if (dataset.Source is not null) w.WriteString("name", dataset.Source);

        if (!geoJsonOnly)
        {
            w.WriteStartObject("header");
            WriteKeyValues(w, "datasetIdentification", dataset.DatasetIdentification);
            WriteKeyValues(w, "datasetStructure", dataset.DatasetStructure);
            WriteKeyValues(w, "parameters", dataset.Parameters);
            if (dataset.ProjectionParameters.Count > 0)
                WriteKeyValues(w, "projection", dataset.ProjectionParameters);
            if (dataset.RegistrationControl.Count > 0)
                WriteKeyValues(w, "registrationControl", dataset.RegistrationControl);
            if (dataset.History.Count > 0) WriteKeyValues(w, "history", dataset.History);
            if (dataset.Accuracy.Count > 0) WriteKeyValues(w, "accuracy", dataset.Accuracy);

            w.WriteStartObject("coordinateTransform");
            w.WriteNumber("coordinateMultiplicationFactor", dataset.Transform.CoordinateMultiplicationFactor);
            w.WriteNumber("soundingMultiplicationFactor", dataset.Transform.SoundingMultiplicationFactor);
            if (dataset.CompilationScale is not null) w.WriteNumber("compilationScale", dataset.CompilationScale.Value);
            if (dataset.CoordinateUnits is not null) w.WriteNumber("coordinateUnits", dataset.CoordinateUnits.Value);
            w.WriteBoolean("geographic", dataset.IsGeographic);
            w.WriteEndObject();

            w.WriteNumber("vectorRecordCount", dataset.VectorRecords.Count);
            w.WriteNumber("featureCount", dataset.Features.Count);
            w.WriteEndObject();

            if (dataset.Warnings.Count > 0)
            {
                w.WriteStartArray("warnings");
                foreach (string warning in dataset.Warnings) w.WriteStringValue(warning);
                w.WriteEndArray();
            }
        }

        w.WriteStartArray("features");
        foreach (var feature in dataset.Features) WriteFeature(w, feature, geoJsonOnly);
        w.WriteEndArray();

        w.WriteEndObject();
        w.Flush();
    }

    /// <param name="dataset">The dataset to serialise.</param>
    /// <param name="path">Destination file, created or overwritten.</param>
    /// <param name="format">When null, inferred from the file extension.</param>
    /// <param name="indented">Applies to JSON only; YAML block style is always indented.</param>
    /// <param name="geoJsonOnly">Drop the header and the pointer detail, leaving a plain FeatureCollection.</param>
    public static void WriteToFile(
        S57Dataset dataset, string path, OutputFormat? format = null, bool indented = true, bool geoJsonOnly = false)
    {
        using var fs = File.Create(path);
        Write(dataset, fs, format ?? OutputFormats.FromPath(path), indented, geoJsonOnly);
    }

    private static void WriteFeature(IStructuredWriter w, S57Feature feature, bool geoJsonOnly)
    {
        w.WriteStartObject();
        w.WriteString("type", "Feature");
        w.WriteString("id", feature.LongName?.ToString() ?? feature.Reference.ToString());

        w.WriteStartObject("properties");
        w.WriteNumber("recordId", feature.Reference.RecordId);
        if (feature.ObjectLabel is not null) w.WriteNumber("objectLabel", feature.ObjectLabel.Value);
        if (feature.ObjectClass is not null) w.WriteString("objectClass", feature.ObjectClass);
        w.WriteString("primitive", S57Codes.DescribePrimitive(feature.Primitive));
        if (feature.Group is not null) w.WriteNumber("group", feature.Group.Value);

        if (feature.LongName is { } lnam)
        {
            w.WriteNumber("agency", lnam.Agency);
            w.WriteNumber("featureIdentificationNumber", lnam.FeatureIdentificationNumber);
            w.WriteNumber("featureIdentificationSubdivision", lnam.Subdivision);
        }

        if (feature.Version is not null) w.WriteNumber("version", feature.Version.Value);
        if (feature.UpdateInstruction is not null) w.WriteNumber("updateInstruction", feature.UpdateInstruction.Value);

        WriteAttributes(w, "attributes", feature.Attributes);
        if (feature.NationalAttributes.Count > 0)
            WriteAttributes(w, "nationalAttributes", feature.NationalAttributes);

        if (!geoJsonOnly)
        {
            if (feature.ObjectPointers.Count > 0)
            {
                w.WriteStartArray("objectPointers");
                foreach (var pointer in feature.ObjectPointers)
                {
                    w.WriteStartObject();
                    w.WriteString("target", pointer.Target.ToString());
                    if (pointer.Relationship is not null) w.WriteNumber("relationship", pointer.Relationship.Value);
                    if (!string.IsNullOrEmpty(pointer.Comment)) w.WriteString("comment", pointer.Comment);
                    w.WriteEndObject();
                }
                w.WriteEndArray();
            }

            if (feature.SpatialPointers.Count > 0)
            {
                w.WriteStartArray("spatialPointers");
                foreach (var pointer in feature.SpatialPointers)
                {
                    w.WriteStartObject();
                    w.WriteString("target", pointer.Target.ToString());
                    if (pointer.Orientation is not null) w.WriteNumber("orientation", pointer.Orientation.Value);
                    if (pointer.Usage is not null) w.WriteNumber("usage", pointer.Usage.Value);
                    if (pointer.Mask is not null) w.WriteNumber("mask", pointer.Mask.Value);
                    w.WriteEndObject();
                }
                w.WriteEndArray();
            }

            if (feature.Warnings.Count > 0)
            {
                w.WriteStartArray("warnings");
                foreach (string warning in feature.Warnings) w.WriteStringValue(warning);
                w.WriteEndArray();
            }
        }

        w.WriteEndObject();   // properties

        w.WritePropertyName("geometry");
        S101DocumentWriter.WriteGeometry(w, feature.Geometry);

        w.WriteEndObject();   // feature
    }

    /// <summary>
    /// S-57 attributes are flat, so this is a plain mapping. A code repeated within one record
    /// becomes an array rather than overwriting.
    /// </summary>
    private static void WriteAttributes(IStructuredWriter w, string name, IReadOnlyList<S57Attribute> attributes)
    {
        w.WriteStartObject(name);

        foreach (var group in attributes.GroupBy(a => a.Name ?? a.Code, StringComparer.Ordinal))
        {
            var items = group.ToList();
            w.WritePropertyName(group.Key);

            if (items.Count == 1)
            {
                WriteAttributeValue(w, items[0]);
            }
            else
            {
                w.WriteStartArray();
                foreach (var item in items) WriteAttributeValue(w, item);
                w.WriteEndArray();
            }
        }

        w.WriteEndObject();
    }

    private static void WriteAttributeValue(IStructuredWriter w, S57Attribute attribute)
    {
        if (attribute.Value is null) w.WriteNullValue();
        else w.WriteStringValue(attribute.Value);
    }

    private static void WriteKeyValues(
        IStructuredWriter w, string name, IReadOnlyList<KeyValuePair<string, object?>> values)
    {
        w.WriteStartObject(name);
        foreach (var (key, value) in values)
        {
            w.WritePropertyName(key);
            switch (value)
            {
                case null: w.WriteNullValue(); break;
                case string s: w.WriteStringValue(s); break;
                case long l: w.WriteNumberValue(l); break;
                case ulong u: w.WriteNumberValue(u); break;
                case double d when double.IsFinite(d): w.WriteNumberValue(d); break;
                case double: w.WriteNullValue(); break;
                case byte[] b: w.WriteStringValue(Convert.ToHexString(b)); break;
                default: w.WriteStringValue(value.ToString()); break;
            }
        }
        w.WriteEndObject();
    }
}
