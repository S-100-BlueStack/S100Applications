namespace S100.Iso8211.S101;

/// <summary>
/// Field tags and record-name codes from the S-100 Part 10a / S-101 ISO 8211 profile.
/// Everything here is data, not logic: adjust it if your product specification edition differs.
/// </summary>
public static class S101Codes
{
    // Identifier field tags. The reader classifies a record by which of these it carries,
    // so it never has to guess from a numeric code.
    public const string DatasetIdentification = "DSID";
    public const string DatasetStructure = "DSSI";
    public const string CrsIdentifier = "CSID";
    public const string CrsIdentifierLegacy = "CRID";   // pre-1.0.0 drafts used CRID for the CRS record
    public const string CrsHeader = "CRSH";
    public const string CoordinateSystemAxes = "CSAX";
    public const string Projection = "PROJ";
    public const string GeodeticDatum = "GDAT";
    public const string VerticalDatum = "VDAT";
    public const string InformationIdentifier = "IRID";
    public const string PointIdentifier = "PRID";
    public const string MultiPointIdentifier = "MRID";
    public const string CurveIdentifier = "CRID";
    public const string CompositeCurveIdentifier = "CCID";
    public const string SurfaceIdentifier = "SRID";
    public const string FeatureIdentifier = "FRID";
    public const string FeatureObjectIdentifier = "FOID";

    // Association / content field tags.
    public const string Attribute = "ATTR";
    public const string InformationAssociation = "INAS";
    public const string SpatialAssociation = "SPAS";
    public const string FeatureAssociation = "FASC";
    public const string FeatureAssociationLegacy = "FEAS";
    public const string ThemeAssociation = "THAS";
    public const string MaskedSpatial = "MASK";
    public const string PointAssociation = "PTAS";
    public const string CurveComponent = "CUCO";
    public const string RingAssociation = "RIAS";

    // Record name (RCNM / RRNM) codes.
    public const int RcnmDataset = 10;
    public const int RcnmCoordinateReferenceSystem = 15;
    public const int RcnmFeature = 100;
    public const int RcnmPoint = 110;
    public const int RcnmMultiPoint = 115;
    public const int RcnmCurve = 120;
    public const int RcnmCompositeCurve = 125;
    public const int RcnmSurface = 130;
    public const int RcnmInformation = 150;

    public static string DescribeRecordName(long? rcnm) => rcnm switch
    {
        RcnmDataset => "DataSet",
        RcnmCoordinateReferenceSystem => "CoordinateReferenceSystem",
        RcnmFeature => "Feature",
        RcnmPoint => "Point",
        RcnmMultiPoint => "MultiPoint",
        RcnmCurve => "Curve",
        RcnmCompositeCurve => "CompositeCurve",
        RcnmSurface => "Surface",
        RcnmInformation => "Information",
        null => "Unknown",
        _ => $"Unknown({rcnm})"
    };
}

public enum S101RecordKind
{
    Unknown,
    DataSetGeneralInformation,
    CoordinateReferenceSystem,
    Information,
    Point,
    MultiPoint,
    Curve,
    CompositeCurve,
    Surface,
    Feature
}

/// <summary>Key of a record inside a dataset: the record-name code plus the record id.</summary>
public readonly record struct S101RecordRef(long RecordName, long RecordId)
{
    public override string ToString() => $"{S101Codes.DescribeRecordName(RecordName)}:{RecordId}";
}

/// <summary>
/// A GeoJSON-shaped geometry. <see cref="Coordinates"/> is a nested structure whose leaves are
/// <c>double[]</c> positions and whose branches are <c>List&lt;object&gt;</c>, matching the nesting
/// depth required by <see cref="Type"/>.
/// </summary>
public sealed class S101Geometry
{
    public required string Type { get; init; }
    public object? Coordinates { get; init; }
    public IReadOnlyList<S101Geometry>? Geometries { get; init; }

    public static S101Geometry Point(double[] position) => new() { Type = "Point", Coordinates = position };

    public static S101Geometry Of(string type, object? coordinates) => new() { Type = type, Coordinates = coordinates };

    public static S101Geometry Collection(IReadOnlyList<S101Geometry> parts) =>
        new() { Type = "GeometryCollection", Geometries = parts };
}

/// <summary>One attribute occurrence from an ATTR field, with any complex children attached.</summary>
public sealed class S101Attribute
{
    /// <summary>Row number of this attribute inside the ATTR field, 1-based. Referenced by child PAIX values.</summary>
    public int Row { get; init; }

    /// <summary>Attribute label: a numeric feature-catalogue code, or a name for complex attributes.</summary>
    public required string Code { get; init; }

    /// <summary>Resolved from the injected code-to-name map, when one was supplied.</summary>
    public string? Name { get; set; }

    public long? Index { get; init; }
    public long? ParentIndex { get; init; }
    public long? Instruction { get; init; }
    public string? Value { get; init; }

    public List<S101Attribute> Children { get; } = new();
    public bool IsComplex => Children.Count > 0;
}

/// <summary>A spatial (vector) record: point, multi point, curve, composite curve or surface.</summary>
public sealed class S101SpatialRecord
{
    public required S101RecordRef Reference { get; init; }
    public required S101RecordKind Kind { get; init; }
    public required DataRecord Source { get; init; }

    public long? Version { get; init; }
    public long? UpdateInstruction { get; init; }

    /// <summary>Coordinates carried directly by this record, already scaled to real-world units.</summary>
    public IReadOnlyList<double[]> Coordinates { get; init; } = Array.Empty<double[]>();

    /// <summary>PTAS begin/end point references for a curve; CUCO components; RIAS rings.</summary>
    public IReadOnlyList<S101SpatialLink> Links { get; init; } = Array.Empty<S101SpatialLink>();
}

/// <summary>A typed reference from one spatial record to another.</summary>
public sealed class S101SpatialLink
{
    public required S101RecordRef Target { get; init; }
    /// <summary>Orientation: 1 = forward, 2 = reverse.</summary>
    public long? Orientation { get; init; }
    /// <summary>Topology indicator on PTAS: 1 = begin node, 2 = end node.</summary>
    public long? TopologyIndicator { get; init; }
    /// <summary>Usage indicator on RIAS: 1 = exterior ring, 2 = interior ring.</summary>
    public long? Usage { get; init; }
}

/// <summary>A feature record with its attributes, associations and assembled geometry.</summary>
public sealed class S101Feature
{
    public required S101RecordRef Reference { get; init; }
    public required DataRecord Source { get; init; }

    /// <summary>Feature type code from FRID (NFTC / OBJC, depending on edition).</summary>
    public string? FeatureTypeCode { get; init; }
    public string? FeatureTypeName { get; set; }

    public long? Version { get; init; }
    public long? UpdateInstruction { get; init; }

    /// <summary>The FOID triple, formatted as agency:number:subdivision when present.</summary>
    public string? FeatureObjectId { get; init; }
    public long? Agency { get; init; }
    public long? FeatureIdentificationNumber { get; init; }
    public long? FeatureIdentificationSubdivision { get; init; }

    public IReadOnlyList<S101Attribute> Attributes { get; init; } = Array.Empty<S101Attribute>();
    public IReadOnlyList<S101SpatialLink> SpatialAssociations { get; init; } = Array.Empty<S101SpatialLink>();
    public IReadOnlyList<S101RecordRef> InformationAssociations { get; init; } = Array.Empty<S101RecordRef>();
    public IReadOnlyList<S101RecordRef> FeatureAssociations { get; init; } = Array.Empty<S101RecordRef>();

    public S101Geometry? Geometry { get; set; }

    /// <summary>Non-fatal problems hit while assembling this feature, e.g. a dangling spatial reference.</summary>
    public IReadOnlyList<string> Warnings { get; set; } = Array.Empty<string>();
}

/// <summary>An information record (IRID) with its attributes.</summary>
public sealed class S101InformationRecord
{
    public required S101RecordRef Reference { get; init; }
    public string? InformationTypeCode { get; init; }
    public IReadOnlyList<S101Attribute> Attributes { get; init; } = Array.Empty<S101Attribute>();
}

/// <summary>
/// Coordinate scaling taken from DSSI: the integer coordinates in the vector records are divided by
/// the multiplication factors and shifted by the data coordinate origin.
/// </summary>
public sealed class S101CoordinateTransform
{
    public double MultiplicationFactorX { get; init; } = 1;
    public double MultiplicationFactorY { get; init; } = 1;
    public double MultiplicationFactorZ { get; init; } = 1;
    public double OriginX { get; init; }
    public double OriginY { get; init; }
    public double OriginZ { get; init; }

    public static S101CoordinateTransform Identity { get; } = new();

    public double X(double raw) => OriginX + raw / Safe(MultiplicationFactorX);
    public double Y(double raw) => OriginY + raw / Safe(MultiplicationFactorY);
    public double Z(double raw) => OriginZ + raw / Safe(MultiplicationFactorZ);

    private static double Safe(double f) => f == 0 || double.IsNaN(f) ? 1 : f;
}

/// <summary>
/// One component of the coordinate reference system: a single CRSH field plus the CSAX, PROJ, GDAT
/// and VDAT fields that follow it in the record and therefore belong to it.
/// </summary>
/// <remarks>
/// A cell is usually compound - a horizontal CRS component and a vertical one - so this is a list,
/// not a single value. Flattening the components together loses the horizontal CRS.
/// </remarks>
public sealed class S101CrsComponent
{
    /// <summary>The CRSH field this component was built from.</summary>
    public required DataField Header { get; init; }

    /// <summary>CSAX, PROJ, GDAT, VDAT and anything else attached to this component, in file order.</summary>
    public required IReadOnlyList<DataField> Fields { get; init; }

    /// <summary>CRIX - which component this is, matching NCRC on the record identifier.</summary>
    public long? Index => Header.GetInt64("CRIX");

    /// <summary>CRST - CRS type, for example 1 for 2-D geographic and 5 for vertical.</summary>
    public long? CrsType => Header.GetInt64("CRST");

    /// <summary>CSTY - coordinate system type, for example 1 for ellipsoidal and 3 for vertical.</summary>
    public long? CoordinateSystemType => Header.GetInt64("CSTY");

    /// <summary>CRNM - the CRS name, for example "WGS84" or "Depth - lowest astronomical tide".</summary>
    public string? Name => Header.GetString("CRNM");

    /// <summary>CRSI - the CRS identifier within the source register, for example the EPSG code.</summary>
    public string? Identifier => Header.GetString("CRSI");

    /// <summary>CRSS - the register the identifier comes from, for example 2 for EPSG.</summary>
    public long? Source => Header.GetInt64("CRSS");

    /// <summary>SCRI - free text describing the source, normally omitted when CRSS names a register.</summary>
    public string? SourceInformation => Header.GetString("SCRI");

    public DataField? Field(string tag)
    {
        foreach (var f in Fields)
            if (string.Equals(f.Tag, tag, StringComparison.OrdinalIgnoreCase)) return f;
        return null;
    }

    public DataField? Axes => Field(S101Codes.CoordinateSystemAxes);
    public DataField? ProjectionField => Field(S101Codes.Projection);
    public DataField? GeodeticDatum => Field(S101Codes.GeodeticDatum);
    public DataField? VerticalDatum => Field(S101Codes.VerticalDatum);

    public override string ToString() => $"CRS {Index}: {Name}";
}

/// <summary>The coordinate reference system record (RCNM 15): one CSID, then one or more components.</summary>
public sealed class S101CoordinateReferenceSystem
{
    /// <summary>The CSID field.</summary>
    public DataField? Identifier { get; init; }

    public required IReadOnlyList<S101CrsComponent> Components { get; init; }

    /// <summary>Fields that appeared before any CRSH, kept so nothing in the record is dropped.</summary>
    public IReadOnlyList<DataField> UnattachedFields { get; init; } = Array.Empty<DataField>();

    public long? RecordName => Identifier?.GetInt64("RCNM");
    public long? RecordId => Identifier?.GetInt64("RCID");

    /// <summary>NCRC - how many components the record declares. Compare against Components.Count.</summary>
    public long? ComponentCount => Identifier?.GetInt64("NCRC");

    /// <summary>True when NCRC disagrees with the number of CRSH fields actually present.</summary>
    public bool ComponentCountMismatch => ComponentCount is not null && ComponentCount != Components.Count;

    public override string ToString() => $"CRS record with {Components.Count} component(s)";
}

/// <summary>Everything a caller needs from an S-101 cell: header metadata, features and geometry.</summary>
public sealed class S101Dataset
{
    public string? Source { get; init; }

    /// <summary>DSID subfields, in file order.</summary>
    public IReadOnlyList<KeyValuePair<string, object?>> DatasetIdentification { get; init; }
        = Array.Empty<KeyValuePair<string, object?>>();

    /// <summary>DSSI subfields, in file order.</summary>
    public IReadOnlyList<KeyValuePair<string, object?>> DatasetStructure { get; init; }
        = Array.Empty<KeyValuePair<string, object?>>();

    /// <summary>The coordinate reference system record, or null when the cell carries none.</summary>
    public S101CoordinateReferenceSystem? CoordinateReferenceSystem { get; init; }

    public S101CoordinateTransform Transform { get; init; } = S101CoordinateTransform.Identity;

    public IReadOnlyList<S101Feature> Features { get; init; } = Array.Empty<S101Feature>();
    public IReadOnlyList<S101InformationRecord> InformationRecords { get; init; } = Array.Empty<S101InformationRecord>();
    public IReadOnlyDictionary<S101RecordRef, S101SpatialRecord> SpatialRecords { get; init; }
        = new Dictionary<S101RecordRef, S101SpatialRecord>();

    public IReadOnlyList<string> Warnings { get; init; } = Array.Empty<string>();
}
