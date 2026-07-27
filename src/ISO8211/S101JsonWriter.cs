using System.Text.Json;

namespace S100.Iso8211.S101;

/// <summary>
/// Serialises an <see cref="S101Dataset"/>. Two shapes are available: a document that keeps the
/// header metadata alongside the features, and a plain RFC 7946 FeatureCollection.
/// </summary>
public static class S101JsonWriter
{
    public static void Write(S101Dataset dataset, Stream output, bool indented = true, bool geoJsonOnly = false)
    {
        using var w = new Utf8JsonWriter(output, new JsonWriterOptions { Indented = indented });

        w.WriteStartObject();
        w.WriteString("type", "FeatureCollection");
        if (dataset.Source is not null) w.WriteString("name", dataset.Source);

        if (!geoJsonOnly)
        {
            w.WriteStartObject("header");
            WriteKeyValues(w, "datasetIdentification", dataset.DatasetIdentification);
            WriteKeyValues(w, "datasetStructure", dataset.DatasetStructure);
            WriteKeyValues(w, "coordinateReferenceSystem", dataset.CoordinateReferenceSystem);

            w.WriteStartObject("coordinateTransform");
            w.WriteNumber("multiplicationFactorX", dataset.Transform.MultiplicationFactorX);
            w.WriteNumber("multiplicationFactorY", dataset.Transform.MultiplicationFactorY);
            w.WriteNumber("multiplicationFactorZ", dataset.Transform.MultiplicationFactorZ);
            w.WriteNumber("originX", dataset.Transform.OriginX);
            w.WriteNumber("originY", dataset.Transform.OriginY);
            w.WriteNumber("originZ", dataset.Transform.OriginZ);
            w.WriteEndObject();

            w.WriteNumber("spatialRecordCount", dataset.SpatialRecords.Count);
            w.WriteNumber("featureCount", dataset.Features.Count);
            w.WriteEndObject();

            if (dataset.InformationRecords.Count > 0)
            {
                w.WriteStartArray("informationRecords");
                foreach (var info in dataset.InformationRecords)
                {
                    w.WriteStartObject();
                    w.WriteString("id", info.Reference.ToString());
                    w.WriteNumber("recordId", info.Reference.RecordId);
                    if (info.InformationTypeCode is not null) w.WriteString("informationType", info.InformationTypeCode);
                    w.WriteStartObject("attributes");
                    WriteAttributes(w, info.Attributes);
                    w.WriteEndObject();
                    w.WriteEndObject();
                }
                w.WriteEndArray();
            }

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

    public static void WriteToFile(S101Dataset dataset, string path, bool indented = true, bool geoJsonOnly = false)
    {
        using var fs = File.Create(path);
        Write(dataset, fs, indented, geoJsonOnly);
    }

    private static void WriteFeature(Utf8JsonWriter w, S101Feature feature, bool geoJsonOnly)
    {
        w.WriteStartObject();
        w.WriteString("type", "Feature");
        w.WriteString("id", feature.FeatureObjectId ?? feature.Reference.ToString());

        w.WriteStartObject("properties");
        w.WriteNumber("recordId", feature.Reference.RecordId);
        if (feature.FeatureTypeCode is not null) w.WriteString("featureType", feature.FeatureTypeCode);
        if (feature.FeatureTypeName is not null) w.WriteString("featureTypeName", feature.FeatureTypeName);
        if (feature.Agency is not null) w.WriteNumber("agency", feature.Agency.Value);
        if (feature.FeatureIdentificationNumber is not null)
            w.WriteNumber("featureIdentificationNumber", feature.FeatureIdentificationNumber.Value);
        if (feature.FeatureIdentificationSubdivision is not null)
            w.WriteNumber("featureIdentificationSubdivision", feature.FeatureIdentificationSubdivision.Value);
        if (feature.Version is not null) w.WriteNumber("version", feature.Version.Value);
        if (feature.UpdateInstruction is not null) w.WriteNumber("updateInstruction", feature.UpdateInstruction.Value);

        w.WriteStartObject("attributes");
        WriteAttributes(w, feature.Attributes);
        w.WriteEndObject();

        if (!geoJsonOnly)
        {
            if (feature.InformationAssociations.Count > 0)
                WriteRefs(w, "informationAssociations", feature.InformationAssociations);
            if (feature.FeatureAssociations.Count > 0)
                WriteRefs(w, "featureAssociations", feature.FeatureAssociations);

            if (feature.SpatialAssociations.Count > 0)
            {
                w.WriteStartArray("spatialAssociations");
                foreach (var link in feature.SpatialAssociations)
                {
                    w.WriteStartObject();
                    w.WriteString("target", link.Target.ToString());
                    if (link.Orientation is not null) w.WriteNumber("orientation", link.Orientation.Value);
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
        WriteGeometry(w, feature.Geometry);

        w.WriteEndObject();   // feature
    }

    /// <summary>
    /// Simple attributes become <c>"code": "value"</c>. Repeated attributes become an array.
    /// Complex attributes become a nested object (or an array of objects when repeated).
    /// </summary>
    private static void WriteAttributes(Utf8JsonWriter w, IReadOnlyList<S101Attribute> attributes)
    {
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
    }

    private static void WriteAttributeValue(Utf8JsonWriter w, S101Attribute attribute)
    {
        if (!attribute.IsComplex)
        {
            if (attribute.Value is null) w.WriteNullValue();
            else w.WriteStringValue(attribute.Value);
            return;
        }

        w.WriteStartObject();
        WriteAttributes(w, attribute.Children);
        w.WriteEndObject();
    }

    private static void WriteGeometry(Utf8JsonWriter w, S101Geometry? geometry)
    {
        if (geometry is null) { w.WriteNullValue(); return; }

        w.WriteStartObject();
        w.WriteString("type", geometry.Type);

        if (geometry.Geometries is not null)
        {
            w.WriteStartArray("geometries");
            foreach (var g in geometry.Geometries) WriteGeometry(w, g);
            w.WriteEndArray();
        }
        else
        {
            w.WritePropertyName("coordinates");
            WriteCoordinateNode(w, geometry.Coordinates);
        }

        w.WriteEndObject();
    }

    private static void WriteCoordinateNode(Utf8JsonWriter w, object? node)
    {
        switch (node)
        {
            case null:
                w.WriteNullValue();
                break;
            case double[] position:
                w.WriteStartArray();
                foreach (double d in position) w.WriteNumberValue(d);
                w.WriteEndArray();
                break;
            case IEnumerable<object> children:
                w.WriteStartArray();
                foreach (object child in children) WriteCoordinateNode(w, child);
                w.WriteEndArray();
                break;
            default:
                w.WriteNullValue();
                break;
        }
    }

    private static void WriteRefs(Utf8JsonWriter w, string name, IReadOnlyList<S101RecordRef> refs)
    {
        w.WriteStartArray(name);
        foreach (var r in refs) w.WriteStringValue(r.ToString());
        w.WriteEndArray();
    }

    private static void WriteKeyValues(
        Utf8JsonWriter w, string name, IReadOnlyList<KeyValuePair<string, object?>> values)
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
