using S100.Iso8211.S101;

namespace S100.Iso8211.S57;

/// <summary>
/// Builds geometry from S-57 chain-node topology. A point feature resolves to a node, a line feature
/// chains edges, and an area feature chains edges into closed rings.
/// </summary>
internal sealed class S57GeometryAssembler
{
    private readonly IReadOnlyDictionary<S57RecordRef, S57VectorRecord> _vectors;

    public S57GeometryAssembler(IReadOnlyDictionary<S57RecordRef, S57VectorRecord> vectors) => _vectors = vectors;

    public S101Geometry? Build(long? primitive, IReadOnlyList<S57SpatialPointer> pointers, List<string> warnings)
    {
        if (primitive == S57Codes.PrimNone || pointers.Count == 0) return null;

        return primitive switch
        {
            S57Codes.PrimPoint => BuildPoint(pointers, warnings),
            S57Codes.PrimLine => BuildLine(pointers, warnings),
            S57Codes.PrimArea => BuildArea(pointers, warnings),
            _ => Warn(warnings, $"Unsupported PRIM value {primitive}.")
        };
    }

    // ------------------------------------------------------------------ point

    /// <summary>
    /// A point feature references one node. An isolated node carrying SG3D holds many soundings, so
    /// it becomes a MultiPoint - that is how SOUNDG is encoded.
    /// </summary>
    private S101Geometry? BuildPoint(IReadOnlyList<S57SpatialPointer> pointers, List<string> warnings)
    {
        var positions = new List<double[]>();
        bool soundings = false;

        foreach (var pointer in pointers)
        {
            if (!_vectors.TryGetValue(pointer.Target, out var node))
            {
                warnings.Add($"Spatial pointer {pointer.Target} could not be resolved.");
                continue;
            }

            soundings |= node.HasSoundings;
            positions.AddRange(node.Coordinates);
        }

        if (positions.Count == 0) return Warn(warnings, "Point feature resolved to no coordinates.");

        return positions.Count == 1 && !soundings
            ? S101Geometry.Point(positions[0])
            : S101Geometry.Of("MultiPoint", ToNodes(positions));
    }

    // ------------------------------------------------------------------ line

    private S101Geometry? BuildLine(IReadOnlyList<S57SpatialPointer> pointers, List<string> warnings)
    {
        var line = new List<double[]>();

        foreach (var pointer in pointers)
        {
            var part = EdgeCoordinates(pointer, warnings);
            if (part is null) continue;
            Append(line, part);
        }

        return line.Count >= 2
            ? S101Geometry.Of("LineString", ToNodes(line))
            : Warn(warnings, "Line feature resolved to fewer than two positions.");
    }

    // ------------------------------------------------------------------ area

    /// <summary>
    /// Walks the FSPT list in order, chaining edges until the ring closes. Exterior rings carry USAG
    /// 1 or 3 - a single exterior ring can mix the two when part of it runs along the data limit - and
    /// interior rings carry USAG 2.
    /// </summary>
    private S101Geometry? BuildArea(IReadOnlyList<S57SpatialPointer> pointers, List<string> warnings)
    {
        var exterior = new List<List<double[]>>();
        var interior = new List<List<double[]>>();

        var current = new List<double[]>();
        bool currentIsInterior = false;

        void Flush()
        {
            if (current.Count == 0) return;

            if (current.Count < 4)
            {
                warnings.Add($"Discarded a ring with only {current.Count} position(s).");
            }
            else
            {
                if (!SamePosition(current[0], current[^1]))
                {
                    warnings.Add("A ring did not close; closing it explicitly.");
                    current.Add(current[0]);
                }
                (currentIsInterior ? interior : exterior).Add(current);
            }

            current = new List<double[]>();
        }

        foreach (var pointer in pointers)
        {
            if (current.Count == 0) currentIsInterior = pointer.IsInterior;

            var part = EdgeCoordinates(pointer, warnings);
            if (part is null) continue;

            Append(current, part);

            // Ring boundaries are not flagged; a ring simply closes back on its first position.
            if (current.Count >= 4 && SamePosition(current[0], current[^1])) Flush();
        }

        Flush();

        if (exterior.Count == 0)
        {
            if (interior.Count == 0) return Warn(warnings, "Area feature produced no rings.");

            warnings.Add("No exterior ring was flagged; promoting the first ring.");
            exterior.Add(interior[0]);
            interior.RemoveAt(0);
        }

        // One exterior ring means a Polygon. Several means a MultiPolygon, and interior rings are
        // assigned to whichever exterior ring encloses them.
        if (exterior.Count == 1)
        {
            var rings = new List<object> { ToNodes(exterior[0]) };
            foreach (var hole in interior) rings.Add(ToNodes(hole));
            return S101Geometry.Of("Polygon", rings);
        }

        var polygons = new List<object>(exterior.Count);
        var shells = new List<List<object>>(exterior.Count);

        foreach (var shell in exterior)
        {
            var rings = new List<object> { ToNodes(shell) };
            shells.Add(rings);
            polygons.Add(rings);
        }

        foreach (var hole in interior)
        {
            int owner = FindEnclosingRing(exterior, hole);
            if (owner < 0)
            {
                warnings.Add("An interior ring fell outside every exterior ring; attached to the first.");
                owner = 0;
            }
            shells[owner].Add(ToNodes(hole));
        }

        return S101Geometry.Of("MultiPolygon", polygons);
    }

    private static int FindEnclosingRing(List<List<double[]>> exterior, List<double[]> hole)
    {
        for (int i = 0; i < exterior.Count; i++)
            if (Contains(exterior[i], hole[0])) return i;
        return -1;
    }

    /// <summary>Standard ray-casting test, used only to decide which shell a hole belongs to.</summary>
    private static bool Contains(List<double[]> ring, double[] point)
    {
        bool inside = false;
        for (int i = 0, j = ring.Count - 1; i < ring.Count; j = i++)
        {
            double xi = ring[i][0], yi = ring[i][1];
            double xj = ring[j][0], yj = ring[j][1];

            if (yi > point[1] != yj > point[1] &&
                point[0] < (xj - xi) * (point[1] - yi) / (yj - yi) + xi)
                inside = !inside;
        }
        return inside;
    }

    // ------------------------------------------------------------------ edges

    /// <summary>
    /// An edge's full geometry: begin node, its own interior vertices, end node - reversed when the
    /// pointer's orientation says so.
    /// </summary>
    private List<double[]>? EdgeCoordinates(S57SpatialPointer pointer, List<string> warnings)
    {
        if (!_vectors.TryGetValue(pointer.Target, out var vector))
        {
            warnings.Add($"Spatial pointer {pointer.Target} could not be resolved.");
            return null;
        }

        var part = new List<double[]>();

        if (vector.Kind == S57RecordKind.Edge)
        {
            if (NodePosition(vector.BeginNode, warnings) is { } begin) part.Add(begin);
            part.AddRange(vector.Coordinates);
            if (NodePosition(vector.EndNode, warnings) is { } end) part.Add(end);
        }
        else
        {
            // A node used directly, which happens for a degenerate boundary.
            part.AddRange(vector.Coordinates);
        }

        if (pointer.Orientation == S57Codes.OrientReverse) part.Reverse();
        return part;
    }

    private double[]? NodePosition(S57RecordRef? reference, List<string> warnings)
    {
        if (reference is null) return null;

        if (!_vectors.TryGetValue(reference.Value, out var node))
        {
            warnings.Add($"Node {reference} referenced by an edge is missing.");
            return null;
        }

        if (node.Coordinates.Count != 0) return node.Coordinates[0];

        warnings.Add($"Node {reference} carries no coordinate.");
        return null;
    }

    // ------------------------------------------------------------------ helpers

    private static void Append(List<double[]> target, List<double[]> part)
    {
        if (part.Count == 0) return;
        int start = target.Count > 0 && SamePosition(target[^1], part[0]) ? 1 : 0;
        for (int i = start; i < part.Count; i++) target.Add(part[i]);
    }

    private static bool SamePosition(double[] a, double[] b)
    {
        int n = Math.Min(a.Length, b.Length);
        for (int i = 0; i < n; i++)
            if (Math.Abs(a[i] - b[i]) > 1e-11) return false;
        return true;
    }

    private static List<object> ToNodes(IReadOnlyList<double[]> positions)
    {
        var list = new List<object>(positions.Count);
        foreach (var p in positions) list.Add(p);
        return list;
    }

    private static S101Geometry? Warn(List<string> warnings, string message)
    {
        warnings.Add(message);
        return null;
    }
}
