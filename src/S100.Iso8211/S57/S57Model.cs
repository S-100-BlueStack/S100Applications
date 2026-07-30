using S100.Iso8211.S101;

namespace S100.Iso8211.S57;

public enum S57RecordKind
{
    Unknown,
    DatasetIdentification,
    DatasetParameter,
    DatasetHistory,
    DatasetAccuracy,
    CatalogueDirectory,
    IsolatedNode,
    ConnectedNode,
    Edge,
    Face,
    Feature
}

/// <summary>A flat S-57 attribute. Unlike S-101 there is no nesting: just a code and a value.</summary>
public sealed class S57Attribute
{
    /// <summary>ATTL - the numeric attribute code from the S-57 object catalogue.</summary>
    public required string Code { get; init; }

    /// <summary>Resolved from the injected code-to-name map, when one was supplied.</summary>
    public string? Name { get; set; }

    /// <summary>ATVL - the value. An empty string means "value not known" per S-57.</summary>
    public string? Value { get; init; }

    /// <summary>True when this came from NATF rather than ATTF.</summary>
    public bool IsNational { get; init; }

    public override string ToString() => $"{Name ?? Code} = {Value}";
}

/// <summary>A VRPT entry: an edge or node pointing at another vector record.</summary>
public sealed class S57VectorPointer
{
    public required S57RecordRef Target { get; init; }
    public long? Orientation { get; init; }
    public long? Usage { get; init; }

    /// <summary>TOPI - 1 = begin node, 2 = end node, 3/4 = left/right face, 5 = containing face.</summary>
    public long? Topology { get; init; }

    public long? Mask { get; init; }

    public override string ToString() => $"-> {Target} (TOPI {Topology})";
}

/// <summary>An FSPT entry: a feature pointing at a vector record.</summary>
public sealed class S57SpatialPointer
{
    public required S57RecordRef Target { get; init; }

    /// <summary>ORNT - 1 = forward, 2 = reverse. Reverse means traverse the edge backwards.</summary>
    public long? Orientation { get; init; }

    /// <summary>USAG - 1 = exterior, 2 = interior, 3 = exterior truncated by the data limit.</summary>
    public long? Usage { get; init; }

    /// <summary>MASK - 1 = masked (do not draw), 2 = show. Does not affect geometry.</summary>
    public long? Mask { get; init; }

    public bool IsInterior => Usage == S57Codes.UsageInterior;

    public override string ToString() => $"-> {Target} (ORNT {Orientation}, USAG {Usage})";
}

/// <summary>An FFPT entry: a relationship between two feature objects.</summary>
public sealed class S57ObjectPointer
{
    public required S57LongName Target { get; init; }

    /// <summary>RIND - 1 = master, 2 = slave, 3 = peer.</summary>
    public long? Relationship { get; init; }

    public string? Comment { get; init; }

    public override string ToString() => $"-> {Target} (RIND {Relationship})";
}

/// <summary>A vector record: isolated node, connected node, edge or face.</summary>
public sealed class S57VectorRecord
{
    public required S57RecordRef Reference { get; init; }
    public required S57RecordKind Kind { get; init; }
    public DataRecord? Source { get; init; }

    public long? Version { get; init; }
    public long? UpdateInstruction { get; init; }

    /// <summary>Coordinates from SG2D / SG3D, already scaled to real-world units.</summary>
    public IReadOnlyList<double[]> Coordinates { get; init; } = Array.Empty<double[]>();

    /// <summary>True when the coordinates came from SG3D, so they carry a sounding value.</summary>
    public bool HasSoundings { get; init; }

    public IReadOnlyList<S57VectorPointer> Pointers { get; init; } = Array.Empty<S57VectorPointer>();
    public IReadOnlyList<S57Attribute> Attributes { get; init; } = Array.Empty<S57Attribute>();

    /// <summary>The node this edge begins at, or null when there is no TOPI 1 pointer.</summary>
    public S57RecordRef? BeginNode => FindNode(S57Codes.TopologyBeginNode);

    /// <summary>The node this edge ends at, or null when there is no TOPI 2 pointer.</summary>
    public S57RecordRef? EndNode => FindNode(S57Codes.TopologyEndNode);

    private S57RecordRef? FindNode(long topology)
    {
        foreach (var p in Pointers)
            if (p.Topology == topology) return p.Target;
        return null;
    }

    public override string ToString() => $"{Reference} ({Coordinates.Count} coords)";
}

/// <summary>A feature record with its attributes, relationships and assembled geometry.</summary>
public sealed class S57Feature
{
    public required S57RecordRef Reference { get; init; }
    public DataRecord? Source { get; init; }

    /// <summary>OBJL - the object class code from the S-57 object catalogue.</summary>
    public long? ObjectLabel { get; init; }

    /// <summary>Resolved from the injected object-class map, when one was supplied.</summary>
    public string? ObjectClass { get; set; }

    /// <summary>PRIM - 1 = point, 2 = line, 3 = area, 255 = no geometry.</summary>
    public long? Primitive { get; init; }

    /// <summary>GRUP - the display group, typically 1 for skin-of-the-earth and 2 for everything else.</summary>
    public long? Group { get; init; }

    public long? Version { get; init; }
    public long? UpdateInstruction { get; init; }

    /// <summary>The FOID triple, which is unique world-wide.</summary>
    public S57LongName? LongName { get; init; }

    public IReadOnlyList<S57Attribute> Attributes { get; init; } = Array.Empty<S57Attribute>();
    public IReadOnlyList<S57Attribute> NationalAttributes { get; init; } = Array.Empty<S57Attribute>();
    public IReadOnlyList<S57SpatialPointer> SpatialPointers { get; init; } = Array.Empty<S57SpatialPointer>();
    public IReadOnlyList<S57ObjectPointer> ObjectPointers { get; init; } = Array.Empty<S57ObjectPointer>();

    /// <summary>
    /// GeoJSON-shaped geometry. The container type is shared with the S-101 layer; despite the name
    /// it carries no S-101-specific content.
    /// </summary>
    public S101Geometry? Geometry { get; set; }

    public IReadOnlyList<string> Warnings { get; set; } = Array.Empty<string>();

    public override string ToString() =>
        $"{LongName} {ObjectClass ?? ObjectLabel?.ToString()} ({S57Codes.DescribePrimitive(Primitive)})";
}

/// <summary>
/// Coordinate scaling from DSPM. S-57 divides by a multiplication factor with no origin offset:
/// horizontal ordinates by COMF, sounding values by SOMF.
/// </summary>
public sealed class S57CoordinateTransform
{
    /// <summary>COMF - coordinate multiplication factor.</summary>
    public double CoordinateMultiplicationFactor { get; init; } = 1;

    /// <summary>SOMF - sounding multiplication factor.</summary>
    public double SoundingMultiplicationFactor { get; init; } = 1;

    public static S57CoordinateTransform Identity { get; } = new();

    public double Horizontal(double raw) => raw / Safe(CoordinateMultiplicationFactor);
    public double Sounding(double raw) => raw / Safe(SoundingMultiplicationFactor);

    private static double Safe(double f) => f == 0 || double.IsNaN(f) ? 1 : f;
}

/// <summary>Everything a caller needs from an S-57 cell: header records, features and geometry.</summary>
public sealed class S57Dataset
{
    public string? Source { get; init; }

    /// <summary>DSID subfields, in file order.</summary>
    public IReadOnlyList<KeyValuePair<string, object?>> DatasetIdentification { get; init; }
        = Array.Empty<KeyValuePair<string, object?>>();

    /// <summary>DSSI subfields, in file order.</summary>
    public IReadOnlyList<KeyValuePair<string, object?>> DatasetStructure { get; init; }
        = Array.Empty<KeyValuePair<string, object?>>();

    /// <summary>DSPM subfields, in file order.</summary>
    public IReadOnlyList<KeyValuePair<string, object?>> Parameters { get; init; }
        = Array.Empty<KeyValuePair<string, object?>>();

    /// <summary>DSPR subfields, present only for a projected cell.</summary>
    public IReadOnlyList<KeyValuePair<string, object?>> ProjectionParameters { get; init; }
        = Array.Empty<KeyValuePair<string, object?>>();

    /// <summary>DSRC subfields.</summary>
    public IReadOnlyList<KeyValuePair<string, object?>> RegistrationControl { get; init; }
        = Array.Empty<KeyValuePair<string, object?>>();

    /// <summary>DSHT and DSAC subfields, when the cell carries those records.</summary>
    public IReadOnlyList<KeyValuePair<string, object?>> History { get; init; }
        = Array.Empty<KeyValuePair<string, object?>>();

    public IReadOnlyList<KeyValuePair<string, object?>> Accuracy { get; init; }
        = Array.Empty<KeyValuePair<string, object?>>();

    public S57CoordinateTransform Transform { get; init; } = S57CoordinateTransform.Identity;

    /// <summary>CSCL - compilation scale, as in 1:CSCL.</summary>
    public long? CompilationScale { get; init; }

    /// <summary>COUN - coordinate units. 1 = latitude/longitude, 2 = easting/northing, 3 = chart units.</summary>
    public long? CoordinateUnits { get; init; }

    /// <summary>True when COUN says the coordinates are geographic, so the output is valid GeoJSON.</summary>
    public bool IsGeographic => CoordinateUnits is null or 1;

    public IReadOnlyList<S57Feature> Features { get; init; } = Array.Empty<S57Feature>();

    public IReadOnlyDictionary<S57RecordRef, S57VectorRecord> VectorRecords { get; init; }
        = new Dictionary<S57RecordRef, S57VectorRecord>();

    public IReadOnlyList<string> Warnings { get; init; } = Array.Empty<string>();
}
