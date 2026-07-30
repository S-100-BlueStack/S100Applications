namespace S100.Iso8211.S101;

/// <summary>
/// Walks the S-101 spatial model - point, multi point, curve, composite curve, surface - and
/// produces GeoJSON-shaped geometry for a feature's spatial associations.
/// </summary>
internal sealed class GeometryAssembler
{
    private readonly IReadOnlyDictionary<S101RecordRef, S101SpatialRecord> _spatial;
    private readonly S101ReaderOptions _options;

    public GeometryAssembler(IReadOnlyDictionary<S101RecordRef, S101SpatialRecord> spatial, S101ReaderOptions options)
    {
        _spatial = spatial;
        _options = options;
    }

    public S101Geometry? Build(IReadOnlyList<S101SpatialLink> links, List<string> warnings)
    {
        if (links.Count == 0) return null;

        var parts = new List<S101Geometry>(links.Count);

        foreach (var link in links)
        {
            if (!_spatial.TryGetValue(link.Target, out var record))
            {
                warnings.Add($"Spatial association {link.Target} could not be resolved.");
                continue;
            }

            var geometry = BuildForRecord(record, link.Orientation, new HashSet<S101RecordRef>(), warnings);
            if (geometry is not null) parts.Add(geometry);
        }

        return parts.Count switch
        {
            0 => null,
            1 => parts[0],
            _ => Merge(parts)
        };
    }

    private S101Geometry? BuildForRecord(
        S101SpatialRecord record, long? orientation, HashSet<S101RecordRef> visited, List<string> warnings)
    {
        if (!visited.Add(record.Reference))
        {
            warnings.Add($"Cycle detected at {record.Reference}; the branch was skipped.");
            return null;
        }

        try
        {
            switch (record.Kind)
            {
                case S101RecordKind.Point:
                    return record.Coordinates.Count > 0
                        ? S101Geometry.Point(record.Coordinates[0])
                        : Warn(warnings, $"Point record {record.Reference} carries no coordinate.");

                case S101RecordKind.MultiPoint:
                    return record.Coordinates.Count > 0
                        ? S101Geometry.Of("MultiPoint", ToNodes(record.Coordinates))
                        : Warn(warnings, $"Multi point record {record.Reference} carries no coordinates.");

                case S101RecordKind.Curve:
                case S101RecordKind.CompositeCurve:
                {
                    var line = LineFor(record, visited, warnings);
                    if (line.Count < 2)
                        return Warn(warnings, $"Curve {record.Reference} resolved to fewer than two positions.");
                    if (orientation == 2) line.Reverse();
                    return S101Geometry.Of("LineString", ToNodes(line));
                }

                case S101RecordKind.Surface:
                {
                    var rings = RingsFor(record, visited, warnings);
                    if (rings.Count == 0)
                        return Warn(warnings, $"Surface {record.Reference} produced no rings.");
                    var nodes = new List<object>(rings.Count);
                    foreach (var ring in rings) nodes.Add(ToNodes(ring));
                    return S101Geometry.Of("Polygon", nodes);
                }

                default:
                    return Warn(warnings, $"Record {record.Reference} is not a spatial type.");
            }
        }
        finally
        {
            visited.Remove(record.Reference);
        }
    }

    /// <summary>Curve: begin node, interior control points, end node. Composite curve: its components in order.</summary>
    private List<double[]> LineFor(S101SpatialRecord record, HashSet<S101RecordRef> visited, List<string> warnings)
    {
        if (record.Kind == S101RecordKind.CompositeCurve)
        {
            var combined = new List<double[]>();
            foreach (var component in record.Links)
            {
                if (!_spatial.TryGetValue(component.Target, out var target))
                {
                    warnings.Add($"Curve component {component.Target} of {record.Reference} is missing.");
                    continue;
                }

                var part = target.Kind == S101RecordKind.CompositeCurve
                    ? LineFor(target, visited, warnings)
                    : LineFor(target, visited, warnings);

                if (component.Orientation == 2) part.Reverse();
                Append(combined, part);
            }
            return combined;
        }

        var line = new List<double[]>();
        double[]? begin = EndPoint(record, topology: 1, warnings);
        double[]? end = EndPoint(record, topology: 2, warnings);

        if (begin is not null) line.Add(begin);
        line.AddRange(record.Coordinates);
        if (end is not null) line.Add(end);

        // A curve with no PTAS at all is still usable if it carries its own vertices.
        return line;
    }

    private double[]? EndPoint(S101SpatialRecord curve, long topology, List<string> warnings)
    {
        foreach (var link in curve.Links)
        {
            if (link.TopologyIndicator != topology) continue;
            if (_spatial.TryGetValue(link.Target, out var point) && point.Coordinates.Count > 0)
                return point.Coordinates[0];
            warnings.Add($"Curve {curve.Reference} references node {link.Target}, which is missing or empty.");
        }
        return null;
    }

    /// <summary>Surface rings: exterior first, then interior rings, each closed.</summary>
    private List<List<double[]>> RingsFor(
        S101SpatialRecord surface, HashSet<S101RecordRef> visited, List<string> warnings)
    {
        var exterior = new List<List<double[]>>();
        var interior = new List<List<double[]>>();

        foreach (var link in surface.Links)
        {
            if (!_spatial.TryGetValue(link.Target, out var target))
            {
                warnings.Add($"Ring association {link.Target} of {surface.Reference} is missing.");
                continue;
            }

            var ring = LineFor(target, visited, warnings);
            if (link.Orientation == 2) ring.Reverse();
            if (ring.Count < 3) { warnings.Add($"Ring {link.Target} has fewer than three positions."); continue; }

            Close(ring);
            if (link.Usage == 2) interior.Add(ring); else exterior.Add(ring);
        }

        // If nothing was flagged as exterior, treat the first ring as the outer boundary.
        if (exterior.Count == 0 && interior.Count > 0)
        {
            exterior.Add(interior[0]);
            interior.RemoveAt(0);
        }

        var rings = new List<List<double[]>>(exterior.Count + interior.Count);
        rings.AddRange(exterior);
        rings.AddRange(interior);
        return rings;
    }

    private static void Close(List<double[]> ring)
    {
        if (ring.Count == 0) return;
        if (!SamePosition(ring[0], ring[^1])) ring.Add(ring[0]);
    }

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
            if (Math.Abs(a[i] - b[i]) > 1e-12) return false;
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

    /// <summary>Collapses several parts into a Multi* geometry, or a GeometryCollection if they differ.</summary>
    private static S101Geometry Merge(List<S101Geometry> parts)
    {
        string type = parts[0].Type;
        foreach (var p in parts)
            if (p.Type != type) return S101Geometry.Collection(parts);

        var coordinates = new List<object>(parts.Count);
        foreach (var p in parts) coordinates.Add(p.Coordinates!);

        return type switch
        {
            "Point" => S101Geometry.Of("MultiPoint", coordinates),
            "LineString" => S101Geometry.Of("MultiLineString", coordinates),
            "Polygon" => S101Geometry.Of("MultiPolygon", coordinates),
            "MultiPoint" or "MultiLineString" or "MultiPolygon" => Flatten(type, parts),
            _ => S101Geometry.Collection(parts)
        };
    }

    private static S101Geometry Flatten(string type, List<S101Geometry> parts)
    {
        var all = new List<object>();
        foreach (var p in parts)
            if (p.Coordinates is List<object> items) all.AddRange(items);
        return S101Geometry.Of(type, all);
    }
}
