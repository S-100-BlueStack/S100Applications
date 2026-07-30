using System.Globalization;

namespace S100.Iso8211.S57;

public sealed class S57ReaderOptions
{
    /// <summary>Optional object catalogue lookup: OBJL code to acronym or name, e.g. 58 to DEPARE.</summary>
    public IReadOnlyDictionary<string, string>? ObjectClassNames { get; set; }

    /// <summary>Optional attribute catalogue lookup: ATTL code to acronym or name, e.g. 87 to VALDCO.</summary>
    public IReadOnlyDictionary<string, string>? AttributeNames { get; set; }

    /// <summary>Override the DSPM-derived coordinate scaling.</summary>
    public S57CoordinateTransform? Transform { get; set; }

    /// <summary>Keep the underlying <see cref="DataRecord"/> on each feature and vector record.</summary>
    public bool KeepSourceRecords { get; set; }

    /// <summary>Emit the sounding value as a third ordinate when the record carries SG3D.</summary>
    public bool IncludeZ { get; set; } = true;
}

/// <summary>
/// Turns the generic ISO 8211 record stream into the S-57 object model: dataset header records,
/// vector records and features with assembled geometry.
/// </summary>
/// <remarks>
/// S-57 cells use chain-node topology. A feature does not hold coordinates; it points at vector
/// records through FSPT, and an edge in turn points at its begin and end nodes through VRPT. So the
/// whole cell is indexed first and geometry assembled afterwards.
/// </remarks>
public static class S57DatasetReader
{
    public static S57Dataset Read(string path, S57ReaderOptions? options = null, Iso8211ReaderOptions? iso = null)
    {
        using var reader = Iso8211Reader.Open(path, iso);
        return Read(reader, options, Path.GetFileName(path));
    }

    public static S57Dataset Read(Iso8211Reader reader, S57ReaderOptions? options = null, string? source = null)
    {
        options ??= new S57ReaderOptions();
        var warnings = new List<string>();

        var datasetId = new List<KeyValuePair<string, object?>>();
        var structure = new List<KeyValuePair<string, object?>>();
        var parameters = new List<KeyValuePair<string, object?>>();
        var projection = new List<KeyValuePair<string, object?>>();
        var registration = new List<KeyValuePair<string, object?>>();
        var history = new List<KeyValuePair<string, object?>>();
        var accuracy = new List<KeyValuePair<string, object?>>();

        var vectorSources = new List<(S57RecordKind Kind, DataRecord Record)>();
        var featureRecords = new List<DataRecord>();

        foreach (var record in reader.ReadRecords())
        {
            switch (Classify(record))
            {
                case S57RecordKind.DatasetIdentification:
                    Collect(record[S57Codes.DatasetIdentification], datasetId);
                    Collect(record[S57Codes.DatasetStructure], structure);
                    break;

                case S57RecordKind.DatasetParameter:
                    Collect(record[S57Codes.DatasetParameter], parameters);
                    Collect(record[S57Codes.Projection], projection);
                    Collect(record[S57Codes.RegistrationControl], registration);
                    break;

                case S57RecordKind.DatasetHistory:
                    Collect(record[S57Codes.DatasetHistory], history);
                    break;

                case S57RecordKind.DatasetAccuracy:
                    Collect(record[S57Codes.DatasetAccuracy], accuracy);
                    break;

                case S57RecordKind.Feature:
                    featureRecords.Add(record);
                    break;

                case S57RecordKind.IsolatedNode:
                    vectorSources.Add((S57RecordKind.IsolatedNode, record)); break;
                case S57RecordKind.ConnectedNode:
                    vectorSources.Add((S57RecordKind.ConnectedNode, record)); break;
                case S57RecordKind.Edge:
                    vectorSources.Add((S57RecordKind.Edge, record)); break;
                case S57RecordKind.Face:
                    vectorSources.Add((S57RecordKind.Face, record)); break;

                case S57RecordKind.CatalogueDirectory:
                    break;   // exchange-set catalogue entries carry no geometry

                default:
                    warnings.Add($"Record #{record.Ordinal} could not be classified " +
                                 $"(fields: {string.Join(", ", record.Fields.Select(f => f.Tag))}).");
                    break;
            }
        }

        var transform = options.Transform ?? BuildTransform(parameters);

        var vectors = new Dictionary<S57RecordRef, S57VectorRecord>();
        foreach (var (kind, record) in vectorSources)
        {
            var vector = BuildVectorRecord(kind, record, transform, options);
            vectors[vector.Reference] = vector;
        }

        var assembler = new S57GeometryAssembler(vectors);

        var features = new List<S57Feature>(featureRecords.Count);
        foreach (var record in featureRecords)
            features.Add(BuildFeature(record, assembler, options));

        return new S57Dataset
        {
            Source = source,
            DatasetIdentification = datasetId,
            DatasetStructure = structure,
            Parameters = parameters,
            ProjectionParameters = projection,
            RegistrationControl = registration,
            History = history,
            Accuracy = accuracy,
            Transform = transform,
            CompilationScale = Lookup(parameters, "CSCL") is double cscl ? (long)cscl : null,
            CoordinateUnits = Lookup(parameters, "COUN") is double coun ? (long)coun : null,
            Features = features,
            VectorRecords = vectors,
            Warnings = warnings
        };
    }

    // ---------------------------------------------------------------- classification

    public static S57RecordKind Classify(DataRecord record)
    {
        if (record.Has(S57Codes.FeatureRecordIdentifier)) return S57RecordKind.Feature;

        if (record[S57Codes.VectorRecordIdentifier] is { } vrid)
        {
            return vrid.GetInt64("RCNM") switch
            {
                S57Codes.RcnmIsolatedNode => S57RecordKind.IsolatedNode,
                S57Codes.RcnmConnectedNode => S57RecordKind.ConnectedNode,
                S57Codes.RcnmEdge => S57RecordKind.Edge,
                S57Codes.RcnmFace => S57RecordKind.Face,
                _ => S57RecordKind.Unknown
            };
        }

        if (record.Has(S57Codes.DatasetIdentification)) return S57RecordKind.DatasetIdentification;
        if (record.Has(S57Codes.DatasetParameter)) return S57RecordKind.DatasetParameter;
        if (record.Has(S57Codes.DatasetHistory)) return S57RecordKind.DatasetHistory;
        if (record.Has(S57Codes.DatasetAccuracy)) return S57RecordKind.DatasetAccuracy;
        if (record.Has(S57Codes.CatalogueDirectory)) return S57RecordKind.CatalogueDirectory;

        return S57RecordKind.Unknown;
    }

    // ---------------------------------------------------------------- vector records

    private static S57VectorRecord BuildVectorRecord(
        S57RecordKind kind, DataRecord record, S57CoordinateTransform transform, S57ReaderOptions options)
    {
        var vrid = record[S57Codes.VectorRecordIdentifier];

        var pointers = new List<S57VectorPointer>();
        foreach (var field in record.FieldsWithTag(S57Codes.VectorPointer))
            foreach (var instance in field.Instances)
            {
                if (!S57Codes.TryDecodeName(instance["NAME"]?.Value, out var target)) continue;
                pointers.Add(new S57VectorPointer
                {
                    Target = target,
                    Orientation = Normalise(instance["ORNT"]?.AsInt64()),
                    Usage = Normalise(instance["USAG"]?.AsInt64()),
                    Topology = Normalise(instance["TOPI"]?.AsInt64()),
                    Mask = Normalise(instance["MASK"]?.AsInt64())
                });
            }

        var coordinates = new List<double[]>();
        bool hasSoundings = false;

        foreach (var field in record.Fields)
        {
            bool is3d = string.Equals(field.Tag, S57Codes.Coordinate3D, StringComparison.OrdinalIgnoreCase);
            bool is2d = string.Equals(field.Tag, S57Codes.Coordinate2D, StringComparison.OrdinalIgnoreCase);
            if (!is2d && !is3d) continue;

            foreach (var instance in field.Instances)
            {
                double? y = instance["YCOO"]?.AsDouble();
                double? x = instance["XCOO"]?.AsDouble();
                if (x is null || y is null) continue;

                double lon = transform.Horizontal(x.Value);
                double lat = transform.Horizontal(y.Value);

                // SG3D's third ordinate is a sounding, scaled by SOMF rather than COMF.
                double? z = is3d ? instance["VE3D"]?.AsDouble() : null;
                if (is3d) hasSoundings = true;

                if (options.IncludeZ && z is not null)
                    coordinates.Add(new[] { lon, lat, transform.Sounding(z.Value) });
                else
                    coordinates.Add(new[] { lon, lat });
            }
        }

        return new S57VectorRecord
        {
            Reference = new S57RecordRef(vrid?.GetInt64("RCNM") ?? 0, vrid?.GetInt64("RCID") ?? 0),
            Kind = kind,
            Source = options.KeepSourceRecords ? record : null,
            Version = vrid?.GetInt64("RVER"),
            UpdateInstruction = vrid?.GetInt64("RUIN"),
            Coordinates = coordinates,
            HasSoundings = hasSoundings,
            Pointers = pointers,
            Attributes = ReadAttributes(record, S57Codes.VectorAttribute, options, national: false)
        };
    }

    // ---------------------------------------------------------------- features

    private static S57Feature BuildFeature(
        DataRecord record, S57GeometryAssembler assembler, S57ReaderOptions options)
    {
        var frid = record[S57Codes.FeatureRecordIdentifier];
        var foid = record[S57Codes.FeatureObjectIdentifier];

        var spatialPointers = new List<S57SpatialPointer>();
        foreach (var field in record.FieldsWithTag(S57Codes.SpatialPointer))
            foreach (var instance in field.Instances)
            {
                if (!S57Codes.TryDecodeName(instance["NAME"]?.Value, out var target)) continue;
                spatialPointers.Add(new S57SpatialPointer
                {
                    Target = target,
                    Orientation = Normalise(instance["ORNT"]?.AsInt64()),
                    Usage = Normalise(instance["USAG"]?.AsInt64()),
                    Mask = Normalise(instance["MASK"]?.AsInt64())
                });
            }

        var objectPointers = new List<S57ObjectPointer>();
        foreach (var field in record.FieldsWithTag(S57Codes.ObjectPointer))
            foreach (var instance in field.Instances)
            {
                if (!S57Codes.TryDecodeLongName(instance["LNAM"]?.Value, out var target)) continue;
                objectPointers.Add(new S57ObjectPointer
                {
                    Target = target,
                    Relationship = Normalise(instance["RIND"]?.AsInt64()),
                    Comment = instance["COMT"]?.AsString()
                });
            }

        long? objl = frid?.GetInt64("OBJL");
        string? objectClass = null;
        if (objl is not null && options.ObjectClassNames?.TryGetValue(
                objl.Value.ToString(CultureInfo.InvariantCulture), out var name) == true)
            objectClass = name;

        S57LongName? longName = null;
        if (foid is not null)
            longName = new S57LongName(
                foid.GetInt64("AGEN") ?? 0, foid.GetInt64("FIDN") ?? 0, foid.GetInt64("FIDS") ?? 0);

        var feature = new S57Feature
        {
            Reference = new S57RecordRef(frid?.GetInt64("RCNM") ?? 0, frid?.GetInt64("RCID") ?? 0),
            Source = options.KeepSourceRecords ? record : null,
            ObjectLabel = objl,
            ObjectClass = objectClass,
            Primitive = frid?.GetInt64("PRIM"),
            Group = frid?.GetInt64("GRUP"),
            Version = frid?.GetInt64("RVER"),
            UpdateInstruction = frid?.GetInt64("RUIN"),
            LongName = longName,
            Attributes = ReadAttributes(record, S57Codes.FeatureAttribute, options, national: false),
            NationalAttributes = ReadAttributes(record, S57Codes.NationalAttribute, options, national: true),
            SpatialPointers = spatialPointers,
            ObjectPointers = objectPointers
        };

        var warnings = new List<string>();
        feature.Geometry = assembler.Build(feature.Primitive, spatialPointers, warnings);
        feature.Warnings = warnings;
        return feature;
    }

    private static IReadOnlyList<S57Attribute> ReadAttributes(
        DataRecord record, string tag, S57ReaderOptions options, bool national)
    {
        var result = new List<S57Attribute>();

        foreach (var field in record.FieldsWithTag(tag))
            foreach (var instance in field.Instances)
            {
                string? code = instance["ATTL"]?.AsString();
                if (code is null) continue;

                var attribute = new S57Attribute
                {
                    Code = code,
                    Value = instance["ATVL"]?.AsString(),
                    IsNational = national
                };

                if (options.AttributeNames?.TryGetValue(code, out var name) == true) attribute.Name = name;
                result.Add(attribute);
            }

        return result;
    }

    // ---------------------------------------------------------------- helpers

    /// <summary>S-57 writes 255 for "not applicable" in single-byte enumerations; treat it as absent.</summary>
    private static long? Normalise(long? value) => value == 255 ? null : value;

    private static void Collect(DataField? field, List<KeyValuePair<string, object?>> into)
    {
        if (field is null) return;
        foreach (var instance in field.Instances)
            foreach (var value in instance.Values)
                into.Add(new KeyValuePair<string, object?>(value.Label, value.Value));
    }

    private static double? Lookup(IReadOnlyList<KeyValuePair<string, object?>> values, string key)
    {
        foreach (var kv in values)
        {
            if (!string.Equals(kv.Key, key, StringComparison.OrdinalIgnoreCase)) continue;
            return kv.Value switch
            {
                long l => l,
                ulong u => u,
                double d => d,
                string s when double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out var p) => p,
                _ => null
            };
        }
        return null;
    }

    private static S57CoordinateTransform BuildTransform(IReadOnlyList<KeyValuePair<string, object?>> dspm) =>
        new()
        {
            CoordinateMultiplicationFactor = Lookup(dspm, "COMF") ?? 1,
            SoundingMultiplicationFactor = Lookup(dspm, "SOMF") ?? 1
        };
}
