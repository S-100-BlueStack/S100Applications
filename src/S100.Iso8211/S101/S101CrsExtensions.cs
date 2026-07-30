namespace S100.Iso8211.S101;

/// <summary>
/// Adapters for callers that want the coordinate reference system record as a flat key/value list,
/// as <see cref="S101Dataset.CoordinateReferenceSystem"/> used to be before it became
/// <see cref="S101CoordinateReferenceSystem"/>.
/// </summary>
public static class S101CrsExtensions
{
    /// <summary>
    /// Flattens the whole record into an ordered key/value list. Keys of component content are
    /// qualified with the component index - <c>CRSH[1].CRNM</c>, <c>CRSH[2].VDAT.DTNM</c> - so a
    /// compound CRS produces no duplicate keys and nothing is silently overwritten.
    /// </summary>
    /// <param name="crs">The record, or null.</param>
    /// <param name="qualifyKeys">
    /// When false, component keys are emitted bare (<c>CRNM</c> rather than <c>CRSH[1].CRNM</c>).
    /// Only safe for a single-component CRS; a compound one will produce duplicate keys.
    /// </param>
    public static IReadOnlyList<KeyValuePair<string, object?>> ToKeyValuePairs(
        this S101CoordinateReferenceSystem? crs, bool qualifyKeys = true)
    {
        var result = new List<KeyValuePair<string, object?>>();
        if (crs is null) return result;

        if (crs.Identifier is not null) Append(result, crs.Identifier, basePrefix: string.Empty);

        for (int i = 0; i < crs.Components.Count; i++)
        {
            S101CrsComponent component = crs.Components[i];
            string prefix = qualifyKeys ? $"CRSH[{component.Index ?? i + 1}]" : string.Empty;

            Append(result, component.Header, prefix);

            foreach (var field in component.Fields)
                Append(result, field, Join(prefix, field.Tag));
        }

        foreach (var field in crs.UnattachedFields)
            Append(result, field, qualifyKeys ? field.Tag : string.Empty);

        return result;
    }

    /// <summary>Flattens one component, without the record identifier subfields.</summary>
    public static IReadOnlyList<KeyValuePair<string, object?>> ToKeyValuePairs(this S101CrsComponent component)
    {
        var result = new List<KeyValuePair<string, object?>>();
        Append(result, component.Header, basePrefix: string.Empty);
        foreach (var field in component.Fields) Append(result, field, field.Tag);
        return result;
    }

    private static void Append(List<KeyValuePair<string, object?>> into, DataField field, string basePrefix)
    {
        // A repeating field contributes one numbered group per instance, so labels stay unique.
        bool repeating = field.Groups.Count == 1 && field.Groups[0].Repeats;
        int index = 0;

        foreach (var instance in field.Instances)
        {
            index++;
            string prefix = repeating ? $"{basePrefix}[{index}]" : basePrefix;

            foreach (var value in instance.Values)
                into.Add(new KeyValuePair<string, object?>(Join(prefix, value.Label), value.Value));
        }
    }

    private static string Join(string prefix, string name) =>
        prefix.Length == 0 ? name : $"{prefix}.{name}";
}
