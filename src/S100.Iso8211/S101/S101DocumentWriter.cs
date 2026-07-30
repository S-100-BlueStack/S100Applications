using S100.Iso8211.Serialization;

namespace S100.Iso8211.S101;

/// <summary>
/// Serialises an <see cref="S101Dataset"/> as YAML or JSON. Two shapes are available: a document
/// that keeps the header metadata alongside the features, and a plain RFC 7946 FeatureCollection.
/// </summary>
public static class S101DocumentWriter
{
    /// <param name="dataset">The dataset to serialise.</param>
    /// <param name="output">Destination stream. It is flushed but not closed.</param>
    /// <param name="format">Defaults to YAML.</param>
    /// <param name="indented">Applies to JSON only; YAML block style is always indented.</param>
    /// <param name="geoJsonOnly">Drop the header and the association detail, leaving a plain FeatureCollection.</param>
    public static void Write(
        S101Dataset dataset,
        Stream output,
        OutputFormat format = OutputFormat.Yaml,
        bool indented = true,
        bool geoJsonOnly = false)
    {
        using IStructuredWriter w = StructuredWriterFactory.Create(output, format, indented);

        w.WriteStartObject();
        w.WriteString("type", "FeatureCollection");
        if (dataset.Source is not null) w.WriteString("name", dataset.Source);

        if (!geoJsonOnly)
        {
            w.WriteStartObject("header");
            WriteKeyValues(w, "datasetIdentification", dataset.DatasetIdentification);
            WriteKeyValues(w, "datasetStructure", dataset.DatasetStructure);
            WriteCoordinateReferenceSystem(w, dataset.CoordinateReferenceSystem);

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

    /// <param name="dataset">The dataset to serialise.</param>
    /// <param name="path">Destination file, created or overwritten.</param>
    /// <param name="format">When null, inferred from the file extension.</param>
    /// <param name="indented">Applies to JSON only; YAML block style is always indented.</param>
    /// <param name="geoJsonOnly">Drop the header and the association detail, leaving a plain FeatureCollection.</param>
    public static void WriteToFile(
        S101Dataset dataset, string path, OutputFormat? format = null, bool indented = true, bool geoJsonOnly = false)
    {
        using var fs = File.Create(path);
        Write(dataset, fs, format ?? OutputFormats.FromPath(path), indented, geoJsonOnly);
    }

    /// <summary>
    /// Writes the CRS record as its identifier subfields plus a "components" sequence, one entry per
    /// CRSH. Flattening the components into a single mapping would emit duplicate keys and silently
    /// drop every component but the last.
    /// </summary>
    private static void WriteCoordinateReferenceSystem(IStructuredWriter w, S101CoordinateReferenceSystem? crs)
    {
        w.WriteStartObject("coordinateReferenceSystem");

        if (crs is not null)
        {
            if (crs.Identifier is not null) WriteSubfieldsInline(w, crs.Identifier);

            w.WriteStartArray("components");
            foreach (var component in crs.Components)
            {
                w.WriteStartObject();
                WriteSubfieldsInline(w, component.Header);

                foreach (var field in component.Fields)
                {
                    w.WritePropertyName(field.Tag);
                    Iso8211DocumentWriter.WriteFieldValue(w, field);
                }

                w.WriteEndObject();
            }
            w.WriteEndArray();

            if (crs.UnattachedFields.Count > 0)
            {
                w.WriteStartObject("unattachedFields");
                foreach (var field in crs.UnattachedFields)
                {
                    w.WritePropertyName(field.Tag);
                    Iso8211DocumentWriter.WriteFieldValue(w, field);
                }
                w.WriteEndObject();
            }
        }

        w.WriteEndObject();
    }

    /// <summary>Writes each subfield of a non-repeating field as a property of the current mapping.</summary>
    private static void WriteSubfieldsInline(IStructuredWriter w, DataField field)
    {
        foreach (var instance in field.Instances)
            foreach (var value in instance.Values)
            {
                w.WritePropertyName(value.Label);
                WriteValue(w, value.Value);
            }
    }

    private static void WriteFeature(IStructuredWriter w, S101Feature feature, bool geoJsonOnly)
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
    private static void WriteAttributes(IStructuredWriter w, IReadOnlyList<S101Attribute> attributes)
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

    private static void WriteAttributeValue(IStructuredWriter w, S101Attribute attribute)
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

    /// <summary>Shared with the S-57 layer, which produces the same geometry container.</summary>
    internal static void WriteGeometry(IStructuredWriter w, S101Geometry? geometry)
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

    private static void WriteCoordinateNode(IStructuredWriter w, object? node)
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

    private static void WriteRefs(IStructuredWriter w, string name, IReadOnlyList<S101RecordRef> refs)
    {
        w.WriteStartArray(name);
        foreach (var r in refs) w.WriteStringValue(r.ToString());
        w.WriteEndArray();
    }

    private static void WriteKeyValues(
        IStructuredWriter w, string name, IReadOnlyList<KeyValuePair<string, object?>> values)
    {
        w.WriteStartObject(name);
        foreach (var (key, value) in values)
        {
            w.WritePropertyName(key);
            WriteValue(w, value);
        }
        w.WriteEndObject();
    }

    private static void WriteValue(IStructuredWriter w, object? value)
    {
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
}
