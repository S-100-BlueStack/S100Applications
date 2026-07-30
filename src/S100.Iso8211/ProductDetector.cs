namespace S100.Iso8211;

public enum Iso8211Product
{
    /// <summary>Neither profile was recognised. The generic layer still reads the file.</summary>
    Unknown,

    /// <summary>IHO S-57 Edition 3.x: vector records, chain-node topology, packed NAME pointers.</summary>
    S57,

    /// <summary>IHO S-100 Part 10a / S-101: point, curve, composite curve and surface records.</summary>
    S101
}

/// <summary>
/// Works out which product profile a file follows by looking at the tags the DDR declares. Both
/// profiles share <c>FRID</c>, <c>FOID</c> and <c>DSSI</c>, so only tags unique to one are counted.
/// </summary>
public static class ProductDetector
{
    private static readonly string[] S57Markers =
    {
        "VRID", "VRPT", "VRPC", "FSPT", "FSPC", "FFPT", "FFPC",
        "DSPM", "DSPR", "DSRC", "ATTF", "NATF", "ATTV", "SG2D", "SG3D"
    };

    private static readonly string[] S101Markers =
    {
        "PRID", "MRID", "CCID", "SRID", "SPAS", "PTAS", "CUCO", "RIAS",
        "CSID", "CRSH", "CSAX", "ATTR", "INAS", "C2IT", "C2IL", "C3IL", "C2FT", "C2FL"
    };

    public static Iso8211Product Detect(DataDescriptiveRecord ddr)
    {
        int s57 = Count(ddr, S57Markers);
        int s101 = Count(ddr, S101Markers);

        if (s57 == 0 && s101 == 0) return Iso8211Product.Unknown;
        return s57 >= s101 ? Iso8211Product.S57 : Iso8211Product.S101;
    }

    public static Iso8211Product Detect(Iso8211Reader reader) => Detect(reader.Ddr);

    /// <summary>Opens the file just far enough to read its DDR, then closes it again.</summary>
    public static Iso8211Product DetectFile(string path)
    {
        using var reader = Iso8211Reader.Open(path);
        return Detect(reader.Ddr);
    }

    private static int Count(DataDescriptiveRecord ddr, string[] markers)
    {
        int found = 0;
        foreach (string tag in markers)
            if (ddr.FieldDefinitions.ContainsKey(tag)) found++;
        return found;
    }
}
