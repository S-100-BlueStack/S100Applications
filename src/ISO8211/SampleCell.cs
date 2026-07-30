namespace S100.Iso8211.SelfTest;

/// <summary>
/// Builds a small but structurally realistic S-101 cell: dataset header, CRS record, four point
/// records, a multi point record with a concatenated 3-D coordinate field, three curves joined by a
/// composite curve into a surface, and four features exercising Point, LineString, Polygon and
/// MultiPoint geometry plus a complex attribute.
/// </summary>
public static class SampleCell
{
    public const int ExpectedRecordCount = 16;

    private const int Cmf = 10_000_000;   // coordinate multiplication factor for X and Y
    private const int CmfZ = 1_000;       // coordinate multiplication factor for Z

    private static int Deg(double v) => (int)Math.Round(v * Cmf);
    private static int Depth(double v) => (int)Math.Round(v * CmfZ);

    public static void Write(string path)
    {
        var w = new Iso8211TestWriter();

        // ---- data descriptive fields -------------------------------------------------
        w.Define("0001", "ISO/IEC 8211 Record Identifier", "", "(b12)", structure: 0, dataType: 5)
         .Define("DSID", "Data Set Identification",
                 "RCNM!RCID!ENSP!ENED!PRSP!PRED!PROF!DSNM!DSTL!DSRD!DSLG!DSAB!DSED!DSTC",
                 "(b11,b14,7A,A(8),3A,b11)", 1, 6)
         .Define("DSSI", "Data Set Structure Information",
                 "DCOX!DCOY!DCOZ!CMFX!CMFY!CMFZ!NOIR!NOPM!NOMN!NOCN!NOXN!NOSN!NOFR",
                 "(3b48,10b14)", 1, 6)
         .Define("CSID", "Coordinate Reference System Record Identifier", "RCNM!RCID!NCRC", "(b11,b14,b11)", 1, 5)
         .Define("CRSH", "Coordinate Reference System Header",
                 "CRIX!CRST!CSTY!CRNM!CRSI!CRSS!SCRI", "(3b11,2A,b11,A)", 1, 6)
         .Define("CSAX", "Coordinate System Axes", "*AXTY!AXUM", "(2b11)", 1, 5)
         .Define("GDAT", "Geodetic Datum",
                 "DTNM!ELNM!ESMA!ESPT!ESPM!CMNM!CMGL", "(2A,b48,b11,b48,A,b48)", 1, 6)
         .Define("VDAT", "Vertical Datum", "DTIX!DTNM!DTID!DTSR!SCRI", "(b11,2A,b11,A)", 1, 6)
         .Define("PRID", "Point Record Identifier", "RCNM!RCID!RVER!RUIN", "(b11,b14,b12,b11)", 1, 5)
         .Define("MRID", "Multi Point Record Identifier", "RCNM!RCID!RVER!RUIN", "(b11,b14,b12,b11)", 1, 5)
         .Define("CRID", "Curve Record Identifier", "RCNM!RCID!RVER!RUIN", "(b11,b14,b12,b11)", 1, 5)
         .Define("CCID", "Composite Curve Record Identifier", "RCNM!RCID!RVER!RUIN", "(b11,b14,b12,b11)", 1, 5)
         .Define("SRID", "Surface Record Identifier", "RCNM!RCID!RVER!RUIN", "(b11,b14,b12,b11)", 1, 5)
         .Define("C2IT", "2-D Integer Coordinate Tuple", "YCOO!XCOO", "(2b24)", 1, 5)
         .Define("C2IL", "2-D Integer Coordinate List", "*YCOO!XCOO", "(2b24)", 1, 5)
         .Define("C3IL", "3-D Integer Coordinate List", "VDID\\*YCOO!XCOO!ZCOO", "(b11,3b24)", 2, 5)
         .Define("PTAS", "Point Association", "*RRNM!RRID!TOPI", "(b11,b14,b11)", 1, 5)
         .Define("CUCO", "Curve Component", "*RRNM!RRID!ORNT", "(b11,b14,b11)", 1, 5)
         .Define("RIAS", "Ring Association", "*RRNM!RRID!ORNT!USAG!RAUI", "(b11,b14,3b11)", 1, 5)
         .Define("FRID", "Feature Type Record Identifier", "RCNM!RCID!NFTC!RVER!RUIN", "(b11,b14,2b12,b11)", 1, 5)
         .Define("FOID", "Feature Object Identifier", "AGEN!FIDN!FIDS", "(b12,b14,b12)", 1, 5)
         .Define("ATTR", "Attribute", "*ATLB!ATIX!PAIX!ATIN!ATVL", "(3b12,b11,A)", 1, 6)
         .Define("SPAS", "Spatial Association", "*RRNM!RRID!ORNT!SMIN!SMAX!SAUI", "(b11,b14,b11,2b14,b11)", 1, 5);

        foreach (var (parent, child) in new[]
                 {
                     ("0001", "DSID"), ("DSID", "DSSI"),
                     ("0001", "CSID"), ("CSID", "CRSH"),
                     ("CRSH", "CSAX"), ("CRSH", "GDAT"), ("CRSH", "VDAT"),
                     ("0001", "PRID"), ("PRID", "C2IT"),
                     ("0001", "MRID"), ("MRID", "C3IL"),
                     ("0001", "CRID"), ("CRID", "PTAS"), ("CRID", "C2IL"),
                     ("0001", "CCID"), ("CCID", "CUCO"),
                     ("0001", "SRID"), ("SRID", "RIAS"),
                     ("0001", "FRID"), ("FRID", "FOID"), ("FRID", "ATTR"), ("FRID", "SPAS")
                 })
            w.Tree(parent, child);

        int rid = 0;
        (string, byte[]) RecordId() => ("0001", FieldBuilder.New().B12(++rid).End());

        // ---- 1. dataset general information -------------------------------------------
        w.Record(
            RecordId(),
            ("DSID", FieldBuilder.New()
                .B11(10).B14(1)
                .Text("INT.IHO.S-101.1.0")     // ENSP
                .Text("1.0.0")                 // ENED
                .Text("INT.IHO.S-101.1.0")     // PRSP
                .Text("1.0.0")                 // PRED
                .Text("1")                     // PROF
                .Text("101DK00DEMO.000")       // DSNM
                .Text("Demo cell - Køge Bugt") // DSTL
                .Text("20260727", 8)           // DSRD, fixed width A(8)
                .Text("en")                    // DSLG
                .Text("Synthetic test data")   // DSAB
                .Text("1")                     // DSED
                .B11(1)                        // DSTC
                .End()),
            ("DSSI", FieldBuilder.New()
                .B48(0).B48(0).B48(0)                       // DCOX, DCOY, DCOZ
                .B14(Cmf).B14(Cmf).B14(CmfZ)                // CMFX, CMFY, CMFZ
                .B14(0).B14(4).B14(1).B14(3).B14(1).B14(1).B14(4)
                .End()));

        // ---- 2. coordinate reference system ---------------------------------------------
        // Compound: a horizontal component and a vertical one, which is the normal case for a cell.
        // CSAX / GDAT / VDAT belong to the CRSH they follow.
        w.Record(
            RecordId(),
            ("CSID", FieldBuilder.New().B11(15).B14(1).B11(2).End()),

            ("CRSH", FieldBuilder.New()
                .B11(1).B11(1).B11(1)
                .Text("WGS 84").Text("4326").B11(2).Text("EPSG")
                .End()),
            ("CSAX", FieldBuilder.New().B11(1).B11(1).B11(2).B11(1).End()),
            ("GDAT", FieldBuilder.New()
                .Text("World Geodetic System 1984").Text("WGS 84")
                .B48(6378137.0).B11(1).B48(298.257223563)
                .Text("Greenwich").B48(0.0)
                .End()),

            ("CRSH", FieldBuilder.New()
                .B11(2).B11(5).B11(3)
                .Text("Depth - lowest astronomical tide").Text("").B11(255).Text("")
                .End()),
            ("CSAX", FieldBuilder.New().B11(12).B11(4).End()),
            ("VDAT", FieldBuilder.New()
                .B11(1)                             // DTIX  b11
                .Text("lowest astronomical tide")   // DTNM  A
                .Text("23")                         // DTID  A
                .B11(2)                             // DTSR  b11
                .Text("")                           // SCRI  A
                .End()));

        // ---- 3. point records -----------------------------------------------------------
        void Point(int id, double lon, double lat) => w.Record(
            RecordId(),
            ("PRID", FieldBuilder.New().B11(110).B14(id).B12(1).B11(1).End()),
            ("C2IT", FieldBuilder.New().B24(Deg(lat)).B24(Deg(lon)).End()));

        Point(1, 12.60, 55.70);   // the buoy
        Point(2, 12.60, 55.70);   // node A
        Point(3, 12.70, 55.70);   // node B
        Point(4, 12.65, 55.75);   // node C

        // ---- 4. multi point (soundings) with a concatenated 3-D coordinate field ---------
        w.Record(
            RecordId(),
            ("MRID", FieldBuilder.New().B11(115).B14(1).B12(1).B11(1).End()),
            ("C3IL", FieldBuilder.New()
                .B11(23)                                                      // VDID, vertical datum
                .B24(Deg(55.71)).B24(Deg(12.61)).B24(Depth(12.5))
                .B24(Deg(55.72)).B24(Deg(12.62)).B24(Depth(9.3))
                .B24(Deg(55.73)).B24(Deg(12.63)).B24(Depth(21.8))
                .End()));

        // ---- 5. curves ------------------------------------------------------------------
        w.Record(
            RecordId(),
            ("CRID", FieldBuilder.New().B11(120).B14(1).B12(1).B11(1).End()),
            ("PTAS", FieldBuilder.New()
                .B11(110).B14(2).B11(1)     // begin node A
                .B11(110).B14(3).B11(2)     // end node B
                .End()),
            ("C2IL", FieldBuilder.New().B24(Deg(55.68)).B24(Deg(12.65)).End()));

        w.Record(
            RecordId(),
            ("CRID", FieldBuilder.New().B11(120).B14(2).B12(1).B11(1).End()),
            ("PTAS", FieldBuilder.New().B11(110).B14(3).B11(1).B11(110).B14(4).B11(2).End()));

        w.Record(
            RecordId(),
            ("CRID", FieldBuilder.New().B11(120).B14(3).B12(1).B11(1).End()),
            ("PTAS", FieldBuilder.New().B11(110).B14(4).B11(1).B11(110).B14(2).B11(2).End()));

        // ---- 6. composite curve and surface ---------------------------------------------
        w.Record(
            RecordId(),
            ("CCID", FieldBuilder.New().B11(125).B14(1).B12(1).B11(1).End()),
            ("CUCO", FieldBuilder.New()
                .B11(120).B14(1).B11(1)
                .B11(120).B14(2).B11(1)
                .B11(120).B14(3).B11(1)
                .End()));

        w.Record(
            RecordId(),
            ("SRID", FieldBuilder.New().B11(130).B14(1).B12(1).B11(1).End()),
            ("RIAS", FieldBuilder.New().B11(125).B14(1).B11(1).B11(1).B11(1).End()));

        // ---- 7. features ------------------------------------------------------------------
        w.Record(
            RecordId(),
            ("FRID", FieldBuilder.New().B11(100).B14(1).B12(17).B12(1).B11(1).End()),
            ("FOID", FieldBuilder.New().B12(540).B14(1811).B12(1).End()),
            ("ATTR", FieldBuilder.New()
                .B12(100).B12(1).B12(0).B11(1).Text("")            // complex parent, row 1
                .B12(136).B12(1).B12(1).B11(1).Text("340")         // child of row 1
                .B12(136).B12(2).B12(1).B11(1).Text("330")         // child of row 1
                .B12(116).B12(1).B12(0).B11(1).Text("Køge Bugt N") // simple, top level
                .End()),
            ("SPAS", FieldBuilder.New().B11(110).B14(1).B11(1).B14(0).B14(0).B11(1).End()));

        w.Record(
            RecordId(),
            ("FRID", FieldBuilder.New().B11(100).B14(2).B12(71).B12(1).B11(1).End()),
            ("FOID", FieldBuilder.New().B12(540).B14(1812).B12(1).End()),
            ("SPAS", FieldBuilder.New().B11(120).B14(1).B11(1).B14(0).B14(0).B11(1).End()));

        w.Record(
            RecordId(),
            ("FRID", FieldBuilder.New().B11(100).B14(3).B12(42).B12(1).B11(1).End()),
            ("FOID", FieldBuilder.New().B12(540).B14(1813).B12(1).End()),
            ("SPAS", FieldBuilder.New().B11(130).B14(1).B11(1).B14(0).B14(0).B11(1).End()));

        w.Record(
            RecordId(),
            ("FRID", FieldBuilder.New().B11(100).B14(4).B12(129).B12(1).B11(1).End()),
            ("FOID", FieldBuilder.New().B12(540).B14(1814).B12(1).End()),
            ("SPAS", FieldBuilder.New().B11(115).B14(1).B11(1).B14(0).B14(0).B11(1).End()));

        w.Save(path);
    }
}
