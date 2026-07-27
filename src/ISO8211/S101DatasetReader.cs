using System.Globalization;

namespace S100.Iso8211.S101;

public sealed class S101ReaderOptions
{
    /// <summary>Optional feature-catalogue lookup: feature type code to human readable name.</summary>
    public IReadOnlyDictionary<string, string>? FeatureTypeNames { get; set; }

    /// <summary>Optional attribute-catalogue lookup: attribute code to human readable name.</summary>
    public IReadOnlyDictionary<string, string>? AttributeNames { get; set; }

    /// <summary>Override the DSSI-derived coordinate scaling.</summary>
    public S101CoordinateTransform? Transform { get; set; }

    /// <summary>Keep the underlying <see cref="DataRecord"/> on each feature. Off saves a lot of memory.</summary>
    public bool KeepSourceRecords { get; set; } = true;

    /// <summary>Emit Z ordinates when the vector records carry them.</summary>
    public bool IncludeZ { get; set; } = true;
}

/// <summary>
/// Turns the generic ISO 8211 record stream into the S-101 object model: dataset metadata,
/// information records, vector records and features with assembled geometry.
/// </summary>
/// <remarks>
/// Records are read once and held in memory, because features reference spatial records by id and
/// a cell's geometry must be available when the feature records are resolved.
/// </remarks>
public static class S101DatasetReader
{
    public static S101Dataset Read(string path, S101ReaderOptions? options = null, Iso8211ReaderOptions? iso = null)
    {
        using var reader = Iso8211Reader.Open(path, iso);
        return Read(reader, options, Path.GetFileName(path));
    }

    public static S101Dataset Read(Iso8211Reader reader, S101ReaderOptions? options = null, string? source = null)
    {
        options ??= new S101ReaderOptions();
        var warnings = new List<string>();

        var datasetId = new List<KeyValuePair<string, object?>>();
        var datasetStructure = new List<KeyValuePair<string, object?>>();
        var crs = new List<KeyValuePair<string, object?>>();

        var spatialSources = new List<(S101RecordKind Kind, DataRecord Record)>();
        var featureRecords = new List<DataRecord>();
        var informationRecords = new List<DataRecord>();

        foreach (var record in reader.ReadRecords())
        {
            switch (Classify(record))
            {
                case S101RecordKind.DataSetGeneralInformation:
                    Collect(record[S101Codes.DatasetIdentification], datasetId);
                    Collect(record[S101Codes.DatasetStructure], datasetStructure);
                    foreach (var f in record.Fields)
                        if (f.Tag is not (S101Codes.DatasetIdentification or S101Codes.DatasetStructure
                            or Iso8211Constants.RecordIdentifierFieldTag))
                            Collect(f, datasetStructure);
                    break;

                case S101RecordKind.CoordinateReferenceSystem:
                    foreach (var f in record.Fields)
                        if (f.Tag != Iso8211Constants.RecordIdentifierFieldTag) Collect(f, crs);
                    break;

                case S101RecordKind.Information:
                    informationRecords.Add(record);
                    break;

                case S101RecordKind.Feature:
                    featureRecords.Add(record);
                    break;

                case S101RecordKind.Point:
                    spatialSources.Add((S101RecordKind.Point, record)); break;
                case S101RecordKind.MultiPoint:
                    spatialSources.Add((S101RecordKind.MultiPoint, record)); break;
                case S101RecordKind.Curve:
                    spatialSources.Add((S101RecordKind.Curve, record)); break;
                case S101RecordKind.CompositeCurve:
                    spatialSources.Add((S101RecordKind.CompositeCurve, record)); break;
                case S101RecordKind.Surface:
                    spatialSources.Add((S101RecordKind.Surface, record)); break;

                default:
                    warnings.Add($"Record #{record.Ordinal} could not be classified " +
                                 $"(fields: {string.Join(", ", record.Fields.Select(f => f.Tag))}).");
                    break;
            }
        }

        var transform = options.Transform ?? BuildTransform(datasetStructure);

        var spatial = new Dictionary<S101RecordRef, S101SpatialRecord>();
        foreach (var (kind, record) in spatialSources)
        {
            var sr = BuildSpatialRecord(kind, record, transform, options);
            spatial[sr.Reference] = sr;
        }

        var builder = new GeometryAssembler(spatial, options);

        var features = new List<S101Feature>(featureRecords.Count);
        foreach (var record in featureRecords)
            features.Add(BuildFeature(record, builder, options));

        var infos = new List<S101InformationRecord>(informationRecords.Count);
        foreach (var record in informationRecords)
        {
            var idField = record[S101Codes.InformationIdentifier];
            infos.Add(new S101InformationRecord
            {
                Reference = RefFrom(idField, "RCNM", "RCID"),
                InformationTypeCode = FirstOf(idField, "NITC", "OBJC", "ITYP", "NFTC")?.AsString(),
                Attributes = BuildAttributes(record, options)
            });
        }

        return new S101Dataset
        {
            Source = source,
            DatasetIdentification = datasetId,
            DatasetStructure = datasetStructure,
            CoordinateReferenceSystem = crs,
            Transform = transform,
            Features = features,
            InformationRecords = infos,
            SpatialRecords = spatial,
            Warnings = warnings
        };
    }

    // ---------------------------------------------------------------- classification

    public static S101RecordKind Classify(DataRecord record)
    {
        if (record.Has(S101Codes.FeatureIdentifier)) return S101RecordKind.Feature;
        if (record.Has(S101Codes.DatasetIdentification)) return S101RecordKind.DataSetGeneralInformation;
        if (record.Has(S101Codes.PointIdentifier)) return S101RecordKind.Point;
        if (record.Has(S101Codes.MultiPointIdentifier)) return S101RecordKind.MultiPoint;
        if (record.Has(S101Codes.CompositeCurveIdentifier)) return S101RecordKind.CompositeCurve;
        if (record.Has(S101Codes.SurfaceIdentifier)) return S101RecordKind.Surface;
        if (record.Has(S101Codes.InformationIdentifier)) return S101RecordKind.Information;
        if (record.Has(S101Codes.CrsIdentifier)) return S101RecordKind.CoordinateReferenceSystem;

        // CRID is the curve identifier in S-101 1.0.0 but was the CRS identifier in early drafts,
        // so fall back to the record-name code to tell them apart.
        var crid = record[S101Codes.CurveIdentifier];
        if (crid is not null)
        {
            long? rcnm = crid.GetInt64("RCNM");
            if (rcnm == S101Codes.RcnmCoordinateReferenceSystem) return S101RecordKind.CoordinateReferenceSystem;
            return S101RecordKind.Curve;
        }

        return S101RecordKind.Unknown;
    }

    // ---------------------------------------------------------------- spatial records

    private static S101SpatialRecord BuildSpatialRecord(
        S101RecordKind kind, DataRecord record, S101CoordinateTransform transform, S101ReaderOptions options)
    {
        string idTag = kind switch
        {
            S101RecordKind.Point => S101Codes.PointIdentifier,
            S101RecordKind.MultiPoint => S101Codes.MultiPointIdentifier,
            S101RecordKind.Curve => S101Codes.CurveIdentifier,
            S101RecordKind.CompositeCurve => S101Codes.CompositeCurveIdentifier,
            _ => S101Codes.SurfaceIdentifier
        };

        var idField = record[idTag];
        var links = new List<S101SpatialLink>();

        foreach (var field in record.Fields)
        {
            string tag = field.Tag;
            if (tag is not (S101Codes.PointAssociation or S101Codes.CurveComponent or S101Codes.RingAssociation))
                continue;

            foreach (var inst in field.Instances)
            {
                long? rrnm = inst["RRNM"]?.AsInt64();
                long? rrid = inst["RRID"]?.AsInt64();
                if (rrnm is null || rrid is null) continue;

                links.Add(new S101SpatialLink
                {
                    Target = new S101RecordRef(rrnm.Value, rrid.Value),
                    Orientation = inst["ORNT"]?.AsInt64(),
                    TopologyIndicator = inst["TOPI"]?.AsInt64(),
                    Usage = inst["USAG"]?.AsInt64()
                });
            }
        }

        return new S101SpatialRecord
        {
            Reference = RefFrom(idField, "RCNM", "RCID"),
            Kind = kind,
            Source = options.KeepSourceRecords ? record : record,
            Version = idField?.GetInt64("RVER"),
            UpdateInstruction = idField?.GetInt64("RUIN"),
            Coordinates = ExtractCoordinates(record, transform, options.IncludeZ),
            Links = links
        };
    }

    /// <summary>
    /// Pulls every coordinate tuple out of a record. Coordinate fields are recognised by their
    /// subfield labels (XCOO / YCOO / ZCOO) rather than by tag, so C2IT, C3IL, C2FT, the older
    /// C2DI/C3DF spellings and any vendor variant all work.
    /// </summary>
    public static IReadOnlyList<double[]> ExtractCoordinates(
        DataRecord record, S101CoordinateTransform transform, bool includeZ)
    {
        var result = new List<double[]>();

        foreach (var field in record.Fields)
        {
            foreach (var inst in field.Instances)
            {
                var x = inst["XCOO"];
                var y = inst["YCOO"];
                if (x is null || y is null) continue;

                double? xv = x.AsDouble();
                double? yv = y.AsDouble();
                if (xv is null || yv is null) continue;

                double lon = NeedsScaling(x) ? transform.X(xv.Value) : xv.Value;
                double lat = NeedsScaling(y) ? transform.Y(yv.Value) : yv.Value;

                var z = inst["ZCOO"];
                if (includeZ && z?.AsDouble() is { } zv)
                    result.Add(new[] { lon, lat, NeedsScaling(z) ? transform.Z(zv) : zv });
                else
                    result.Add(new[] { lon, lat });
            }
        }

        return result;
    }

    private static bool NeedsScaling(SubfieldValue v) =>
        v.Definition.Format.Kind == SubfieldKind.IntegerText ||
        (v.Definition.Format.Kind == SubfieldKind.Binary &&
         v.Definition.Format.BinaryKind is BinaryKind.UnsignedInteger or BinaryKind.SignedInteger);

    // ---------------------------------------------------------------- features

    private static S101Feature BuildFeature(DataRecord record, GeometryAssembler assembler, S101ReaderOptions options)
    {
        var frid = record[S101Codes.FeatureIdentifier];
        var foid = record[S101Codes.FeatureObjectIdentifier];

        var spatialLinks = new List<S101SpatialLink>();
        foreach (var field in record.FieldsWithTag(S101Codes.SpatialAssociation))
        {
            foreach (var inst in field.Instances)
            {
                long? rrnm = inst["RRNM"]?.AsInt64();
                long? rrid = inst["RRID"]?.AsInt64();
                if (rrnm is null || rrid is null) continue;
                spatialLinks.Add(new S101SpatialLink
                {
                    Target = new S101RecordRef(rrnm.Value, rrid.Value),
                    Orientation = inst["ORNT"]?.AsInt64(),
                    Usage = inst["USAG"]?.AsInt64()
                });
            }
        }

        var infoLinks = CollectRefs(record, S101Codes.InformationAssociation);
        var featureLinks = CollectRefs(record, S101Codes.FeatureAssociation);
        if (featureLinks.Count == 0) featureLinks = CollectRefs(record, S101Codes.FeatureAssociationLegacy);

        long? agen = foid?.GetInt64("AGEN");
        long? fidn = foid?.GetInt64("FIDN");
        long? fids = foid?.GetInt64("FIDS");

        string? typeCode = FirstOf(frid, "NFTC", "OBJC", "FTYP", "FCID", "OBJL")?.AsString();

        var feature = new S101Feature
        {
            Reference = RefFrom(frid, "RCNM", "RCID"),
            Source = record,
            FeatureTypeCode = typeCode,
            FeatureTypeName = typeCode is not null && options.FeatureTypeNames?.TryGetValue(typeCode, out var n) == true ? n : null,
            Version = frid?.GetInt64("RVER"),
            UpdateInstruction = frid?.GetInt64("RUIN"),
            Agency = agen,
            FeatureIdentificationNumber = fidn,
            FeatureIdentificationSubdivision = fids,
            FeatureObjectId = foid is null ? null : $"{agen}:{fidn}:{fids}",
            Attributes = BuildAttributes(record, options),
            SpatialAssociations = spatialLinks,
            InformationAssociations = infoLinks,
            FeatureAssociations = featureLinks
        };

        var warnings = new List<string>();
        feature.Geometry = assembler.Build(spatialLinks, warnings);
        feature.Warnings = warnings;
        return feature;
    }

    private static List<S101RecordRef> CollectRefs(DataRecord record, string tag)
    {
        var list = new List<S101RecordRef>();
        foreach (var field in record.FieldsWithTag(tag))
            foreach (var inst in field.Instances)
            {
                long? rrnm = inst["RRNM"]?.AsInt64();
                long? rrid = inst["RRID"]?.AsInt64();
                if (rrnm is not null && rrid is not null) list.Add(new S101RecordRef(rrnm.Value, rrid.Value));
            }
        return list;
    }

    /// <summary>
    /// Builds the attribute tree from the ATTR field. PAIX is the 1-based row number of the parent
    /// attribute inside the same field; zero means the attribute sits at the top level.
    /// </summary>
    public static IReadOnlyList<S101Attribute> BuildAttributes(DataRecord record, S101ReaderOptions options)
    {
        var rows = new List<S101Attribute>();

        foreach (var field in record.FieldsWithTag(S101Codes.Attribute))
        {
            foreach (var inst in field.Instances)
            {
                string? code = (inst["ATLB"] ?? inst["ATTL"])?.AsString();
                if (code is null) continue;

                var attr = new S101Attribute
                {
                    Row = rows.Count + 1,
                    Code = code,
                    Index = inst["ATIX"]?.AsInt64(),
                    ParentIndex = inst["PAIX"]?.AsInt64(),
                    Instruction = inst["ATIN"]?.AsInt64(),
                    Value = (inst["ATVL"] ?? inst["ATVA"])?.AsString()
                };

                if (options.AttributeNames?.TryGetValue(code, out var name) == true) attr.Name = name;
                rows.Add(attr);
            }
        }

        var roots = new List<S101Attribute>();
        foreach (var attr in rows)
        {
            long parent = attr.ParentIndex ?? 0;
            if (parent > 0 && parent <= rows.Count && parent != attr.Row) rows[(int)parent - 1].Children.Add(attr);
            else roots.Add(attr);
        }

        return roots;
    }

    // ---------------------------------------------------------------- helpers

    private static S101RecordRef RefFrom(DataField? field, string nameLabel, string idLabel) =>
        new(field?.GetInt64(nameLabel) ?? 0, field?.GetInt64(idLabel) ?? 0);

    private static SubfieldValue? FirstOf(DataField? field, params string[] labels)
    {
        if (field is null) return null;
        foreach (string label in labels)
        {
            var v = field.Find(label);
            if (v is not null) return v;
        }
        return null;
    }

    private static void Collect(DataField? field, List<KeyValuePair<string, object?>> into)
    {
        if (field is null) return;
        foreach (var inst in field.Instances)
            foreach (var v in inst.Values)
                into.Add(new KeyValuePair<string, object?>(v.Label, v.Value));
    }

    private static S101CoordinateTransform BuildTransform(IReadOnlyList<KeyValuePair<string, object?>> dssi)
    {
        double Get(string key, double fallback)
        {
            foreach (var kv in dssi)
            {
                if (!string.Equals(kv.Key, key, StringComparison.OrdinalIgnoreCase)) continue;
                return kv.Value switch
                {
                    long l => l,
                    double d => d,
                    ulong u => u,
                    string s when double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out var p) => p,
                    _ => fallback
                };
            }
            return fallback;
        }

        return new S101CoordinateTransform
        {
            MultiplicationFactorX = Get("CMFX", 1),
            MultiplicationFactorY = Get("CMFY", 1),
            MultiplicationFactorZ = Get("CMFZ", 1),
            OriginX = Get("DCOX", 0),
            OriginY = Get("DCOY", 0),
            OriginZ = Get("DCOZ", 0)
        };
    }
}
