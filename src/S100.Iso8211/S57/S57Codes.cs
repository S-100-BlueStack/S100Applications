using System.Buffers.Binary;
using System.Globalization;

namespace S100.Iso8211.S57;

/// <summary>
/// Field tags and codes from IHO S-57 Edition 3.1. S-57 predates S-100 and uses a different record
/// model: vector records rather than curves and surfaces, and packed <c>NAME</c> / <c>LNAM</c>
/// pointers rather than separate record-name and record-id subfields.
/// </summary>
public static class S57Codes
{
    // Dataset descriptive records.
    public const string DatasetIdentification = "DSID";
    public const string DatasetStructure = "DSSI";
    public const string DatasetParameter = "DSPM";
    public const string Projection = "DSPR";
    public const string RegistrationControl = "DSRC";
    public const string DatasetHistory = "DSHT";
    public const string DatasetAccuracy = "DSAC";

    // Catalogue and data dictionary records (exchange-set files, not cells).
    public const string CatalogueDirectory = "CATD";
    public const string CatalogueCrossReference = "CATX";

    // Vector records.
    public const string VectorRecordIdentifier = "VRID";
    public const string VectorAttribute = "ATTV";
    public const string VectorPointerControl = "VRPC";
    public const string VectorPointer = "VRPT";
    public const string CoordinateControl = "SGCC";
    public const string Coordinate2D = "SG2D";
    public const string Coordinate3D = "SG3D";

    // Feature records.
    public const string FeatureRecordIdentifier = "FRID";
    public const string FeatureObjectIdentifier = "FOID";
    public const string FeatureAttribute = "ATTF";
    public const string NationalAttribute = "NATF";
    public const string ObjectPointerControl = "FFPC";
    public const string ObjectPointer = "FFPT";
    public const string SpatialPointerControl = "FSPC";
    public const string SpatialPointer = "FSPT";

    // Record name (RCNM) codes.
    public const int RcnmDatasetIdentification = 10;
    public const int RcnmDatasetParameter = 20;
    public const int RcnmDatasetHistory = 30;
    public const int RcnmDatasetAccuracy = 40;
    public const int RcnmCatalogueDirectory = 50;
    public const int RcnmCatalogueCrossReference = 60;
    public const int RcnmDataDictionaryDefinition = 70;
    public const int RcnmDataDictionaryDomain = 80;
    public const int RcnmDataDictionarySchema = 90;
    public const int RcnmFeature = 100;
    public const int RcnmIsolatedNode = 110;
    public const int RcnmConnectedNode = 120;
    public const int RcnmEdge = 130;
    public const int RcnmFace = 140;

    /// <summary>PRIM - the geometric primitive a feature is built from.</summary>
    public const int PrimPoint = 1;
    public const int PrimLine = 2;
    public const int PrimArea = 3;
    public const int PrimNone = 255;

    /// <summary>ORNT - orientation of a pointer.</summary>
    public const int OrientForward = 1;
    public const int OrientReverse = 2;
    public const int OrientNull = 255;

    /// <summary>USAG - usage indicator on a spatial pointer.</summary>
    public const int UsageExterior = 1;
    public const int UsageInterior = 2;
    public const int UsageExteriorTruncated = 3;
    public const int UsageNull = 255;

    /// <summary>TOPI - topology indicator on a vector pointer.</summary>
    public const int TopologyBeginNode = 1;
    public const int TopologyEndNode = 2;
    public const int TopologyLeftFace = 3;
    public const int TopologyRightFace = 4;
    public const int TopologyContainingFace = 5;
    public const int TopologyNull = 255;

    /// <summary>MASK - whether a boundary edge should be drawn.</summary>
    public const int MaskMask = 1;
    public const int MaskShow = 2;
    public const int MaskNull = 255;

    public static string DescribeRecordName(long? rcnm) => rcnm switch
    {
        RcnmDatasetIdentification => "DataSetIdentification",
        RcnmDatasetParameter => "DataSetParameter",
        RcnmDatasetHistory => "DataSetHistory",
        RcnmDatasetAccuracy => "DataSetAccuracy",
        RcnmCatalogueDirectory => "CatalogueDirectory",
        RcnmCatalogueCrossReference => "CatalogueCrossReference",
        RcnmFeature => "Feature",
        RcnmIsolatedNode => "IsolatedNode",
        RcnmConnectedNode => "ConnectedNode",
        RcnmEdge => "Edge",
        RcnmFace => "Face",
        null => "Unknown",
        _ => $"Unknown({rcnm})"
    };

    public static string DescribePrimitive(long? prim) => prim switch
    {
        PrimPoint => "Point",
        PrimLine => "Line",
        PrimArea => "Area",
        PrimNone => "None",
        null => "Unknown",
        _ => $"Unknown({prim})"
    };

    /// <summary>
    /// Decodes an S-57 <c>NAME</c> pointer: a 5-byte <c>B(40)</c> holding the record name in the
    /// first byte and the record id in the following four, least significant byte first.
    /// </summary>
    public static bool TryDecodeName(object? value, out S57RecordRef reference)
    {
        reference = default;
        if (!TryGetBytes(value, out byte[] bytes) || bytes.Length < 5) return false;

        reference = new S57RecordRef(bytes[0], BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(1, 4)));
        return true;
    }

    /// <summary>
    /// Decodes an S-57 <c>LNAM</c> long name: an 8-byte <c>B(64)</c> holding the producing agency
    /// (2 bytes), the feature identification number (4 bytes) and the subdivision (2 bytes), each
    /// least significant byte first.
    /// </summary>
    public static bool TryDecodeLongName(object? value, out S57LongName longName)
    {
        longName = default;
        if (!TryGetBytes(value, out byte[] bytes) || bytes.Length < 8) return false;

        longName = new S57LongName(
            BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(0, 2)),
            BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(2, 4)),
            BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(6, 2)));
        return true;
    }

    private static bool TryGetBytes(object? value, out byte[] bytes)
    {
        switch (value)
        {
            case byte[] raw:
                bytes = raw;
                return true;

            case string hex when hex.Length % 2 == 0 && hex.Length >= 10:
                try { bytes = Convert.FromHexString(hex); return true; }
                catch (FormatException) { bytes = Array.Empty<byte>(); return false; }

            default:
                bytes = Array.Empty<byte>();
                return false;
        }
    }
}

/// <summary>A reference to a record within the cell: the record name code plus the record id.</summary>
public readonly record struct S57RecordRef(long RecordName, long RecordId)
{
    public override string ToString() => $"{S57Codes.DescribeRecordName(RecordName)}:{RecordId}";
}

/// <summary>The world-unique identity of a feature object, as carried by FOID and LNAM.</summary>
public readonly record struct S57LongName(long Agency, long FeatureIdentificationNumber, long Subdivision)
{
    public override string ToString() =>
        string.Create(CultureInfo.InvariantCulture,
            $"{Agency}:{FeatureIdentificationNumber}:{Subdivision}");
}
