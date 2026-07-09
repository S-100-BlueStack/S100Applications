using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using CommandLine;
using NetTopologySuite.Geometries;
using S100FC;
using S100FC.S101;
using S100FC.Topology;
using S100FC.YAML;
using Serilog;
using System.Diagnostics;
using System.Text.Json;
using System.Text.RegularExpressions;
using Dataset = S100FC.YAML.Dataset;
using Esri = ArcGIS.Core.Hosting.Host;
using IO = System.IO;

namespace S100Framework.Applications
{
    internal class VortexExporter
    {
        private const string outputTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff}| [{Level:u3}] {Message:lj} {NewLine}{Exception}";

        const string fileReferencePattern = @"^101[A-Z]{2}\d{2}";
        static Regex fileReferenceRegex = new Regex(fileReferencePattern);

        const string DE9IM_Contains = S100FC.Topology.Matrix.DE9IM_Contains;
        const string DE9IM_Crosses = S100FC.Topology.Matrix.DE9IM_Crosses;

        public class Options
        {
            [Option('d', "dnsm", Required = false, HelpText = "")]
            public string? Dataset { get; set; }

            [Option('b', "bulk", Required = false, HelpText = "Multiple datasets. Example: -b dataset1 dataset2 dataset3")]
            public IEnumerable<string> DatasetBulk { get; set; } = [];

            [Option('g', "geodatabase", Required = true, HelpText = "Geodatabase.")]
            public string Geodatabase { get; set; } = string.Empty;

            [Option('e', "exchangeset", Required = false, Default = false, HelpText = "Build exchangeset.")]
            public bool ExchangeSet { get; set; } = false;

            [Option('o', "outputpath", Required = false, HelpText = "OutputPath")]
            public string OutputPath { get; set; } = Directory.GetCurrentDirectory();

            //[Option('n', "notespath", Required = false, HelpText = "Path to notes files references in TXTDSC.")]
            //public string? NotesPath { get; set; }

            [Option('f', "featurecatalogue", Required = false, Default = @"c:\Users\Public\Documents\NuvionPro\Product Files\101_FC_2.0.0.xml", HelpText = "Path to feature catalogue.")]
            public string? FeatureCatalogue { get; set; }

            [Option('v', "verbose", Required = false, HelpText = "Set output to verbose messages.")]
            public bool Verbose { get; set; }

            [Option("s57", Required = false, Default = false)]
            public bool s57 { get; set; }
        }

        [STAThread]
        public static int Main(string[] args) {
            var logpath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"S-100 BlueStack", "ExporterYAML", "ExporterYAML-developer.log");

            // Clears log between each run
            if (File.Exists(logpath))
                File.Delete(logpath);

            Console.Clear();

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Console()
                .WriteTo.File(
                    path: logpath,
                    rollingInterval: RollingInterval.Infinite,
                    retainedFileCountLimit: 1,
                    shared: true,
                    outputTemplate: outputTemplate)
                .CreateLogger();

            //Log.Information("exporter.exe {args}", string.Join(' ', args));


            try {
                var sw = new Stopwatch();
                sw.Start();
                var arguments = Parser.Default.ParseArguments<Options>(args)
                                   .WithParsed<Options>(o => {
                                   });

                AppDomain.CurrentDomain.UnhandledException += (sender, e) => {
                    Log.Fatal((Exception)e.ExceptionObject, "UnhandledException");
                };

                Log.Information("ExporterYAML.exe {args}", string.Join(" ", args));

                if (arguments.Errors.Any())
                    return -1;

                var jsonSerializerOptionsS101 = new JsonSerializerOptions {
                    WriteIndented = false,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                    PropertyNameCaseInsensitive = true,
                }.AppendTypeInfoResolver();

                var jsonSerializerOptionsS128 = new JsonSerializerOptions {
                    WriteIndented = false,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                    PropertyNameCaseInsensitive = true,
                };

                S100FC.S128.Extensions.AppendTypeInfoResolver(jsonSerializerOptionsS128);

                Esri.Initialize();

                string? output = default;
                string? featureCataloguePath = default;
                bool exchangeset = false;
                bool s57 = false;
                string[] datasetNames = [];
                string? wildcard = default;

                //IO.DirectoryInfo? directoryNotes = default;

                Func<Geodatabase> createGeodatabase = () => { throw new NotImplementedException(); };

                _ = arguments.WithParsed<Options>(o => {
                    var geodatabase = o.Geodatabase.ToLowerInvariant();

                    if (IO.File.Exists(geodatabase) && ".sde".Equals(IO.Path.GetExtension(geodatabase), StringComparison.InvariantCultureIgnoreCase)) {
                        createGeodatabase = () => { return new Geodatabase(new DatabaseConnectionFile(new Uri(IO.Path.GetFullPath(geodatabase)))); };
                    }
                    else if (IO.Directory.Exists(geodatabase) && ".gdb".Equals(IO.Path.GetExtension(geodatabase), StringComparison.InvariantCultureIgnoreCase)) {
                        createGeodatabase = () => { return new Geodatabase(new FileGeodatabaseConnectionPath(new Uri(IO.Path.GetFullPath(geodatabase)))); };
                    }
                    else if (IO.File.Exists(geodatabase) && ".geodatabase".Equals(IO.Path.GetExtension(geodatabase), StringComparison.OrdinalIgnoreCase)) {
                        createGeodatabase = () => { return new Geodatabase(new MobileGeodatabaseConnectionPath(new Uri(IO.Path.GetFullPath(geodatabase)))); };
                    }
                    else
                        throw new System.ArgumentOutOfRangeException(nameof(geodatabase));

                    datasetNames = [.. o.DatasetBulk];

                    if (o.Dataset != null) {
                        datasetNames = [.. datasetNames, o.Dataset];
                    }

                    if (o.Dataset != null && o.Dataset.Contains("%")) {
                        datasetNames = [];
                        wildcard = o.Dataset;
                    }


                    exchangeset = o.ExchangeSet;

                    //directoryNotes = new IO.DirectoryInfo(o.NotesPath!);

                    output = o.OutputPath;
                    featureCataloguePath = o.FeatureCatalogue;

                    s57 = o.s57;
                });

                if (datasetNames.Length == 0 && string.IsNullOrEmpty(wildcard))
                    throw new ArgumentNullException("No datasets specified. Use -d or -b to specify dataset(s).");

                Directory.CreateDirectory(output!);
                Log.Information("Output path: {output}", output);

                using Geodatabase source = createGeodatabase();

                var syntax = source.GetSQLSyntax();

                var definitionTables = source.GetDefinitions<TableDefinition>();
                var definitionFeatures = source.GetDefinitions<FeatureClassDefinition>();

                var featureCatalogue = S100FC.Catalogues.FeatureCatalogue.Catalogues.Single(e => e.ProductID.Equals("S-101"));

                var datasets = new List<(Dataset Dataset, SpatialQueryFilter Filters)>();
                {
                    using var surface = source.OpenDataset<FeatureClass>(definitionFeatures.Single(e => syntax.ParseTableName(e.GetName()).Item3.Equals("surface")).GetName());

                    if (!string.IsNullOrEmpty(wildcard)) {
                        using var cursor = surface.Search(new QueryFilter {
                            WhereClause = $"upper(ps) = 'S-128' and attributeBindings LIKE '%\"datasetName\":%\"{wildcard}\"%'",
                        }, true);

                        while (cursor.MoveNext()) {
                            var current = (ArcGIS.Core.Data.Feature)cursor.Current;

                            try {
                                var electricProduct = (S100FC.S128.FeatureTypes.ElectronicProduct)S100FC.AttributeFlattenExtensions.Unflatten<S100FC.FeatureType>(Convert.ToString(current["attributebindings"])!, typeof(S100FC.S128.FeatureTypes.ElectronicProduct));
                                datasetNames = [.. datasetNames, electricProduct.datasetName!];
                            }
                            catch (System.Exception ex) {
                                Log.Error(ex, "Can't deserialize {UID}!", current["UID"]);
                            }
                        }
                    }

                    foreach (var ds in datasetNames) {
                        using var cursor = surface.Search(new QueryFilter {
                            WhereClause = string.IsNullOrEmpty(ds) ? "upper(ps) = 'S-128'" : $"upper(ps) = 'S-128' and attributeBindings LIKE '%\"datasetName\":%\"{ds.ToUpperInvariant()}\"%'",
                        }, true);

                        while (cursor.MoveNext()) {
                            var current = (ArcGIS.Core.Data.Feature)cursor.Current;

                            var electricProduct = (S100FC.S128.FeatureTypes.ElectronicProduct)S100FC.AttributeFlattenExtensions.Unflatten<S100FC.FeatureType>(Convert.ToString(current["attributebindings"])!, typeof(S100FC.S128.FeatureTypes.ElectronicProduct));

                            var shape = (ArcGIS.Core.Geometry.Polygon)current.GetShape().Clone();

                            var whereClause = "upper(ps) = 'S-101'";
                            if (current.FindField("specificusage") != -1 && !current.IsNull("specificusage"))
                                whereClause += $" AND (specificusage = {Convert.ToInt32(current["specificusage"])} OR specificusage = 0)";

                            datasets.Add((new Dataset {
                                CellName = $"{electricProduct!.datasetName!}.000",
                                Comment = "Not for navigation!",
                                Edition = 1,
                                ENCVer = "INT.IHO.S-101.2.0",
                                FCVer = "2.0",
                                verticalDatum = "Baltic Sea Chart Datum 2000,44",
                            }, new SpatialQueryFilter {
                                FilterGeometry = shape,
                                SpatialRelationship = SpatialRelationship.Relation,
                                SpatialRelationshipDescription = "UNKNOWN",
                                WhereClause = whereClause,
                                SubFields = "OBJECTID,UID,GLOBALID,CODE,SHAPE",
                            }));
                        }
                    }
                }

                //Matrix.ParallelOptions = new ParallelOptions { MaxDegreeOfParallelism = 1 };

                //  TEST, TEST, TEST, TEST, TEST, 
                S100FC.Topology.Matrix.ParallelOptions = new ParallelOptions { MaxDegreeOfParallelism = 1 };

                foreach (var e in datasets) {
                    //{
                    //    using var fc = source.OpenDataset<FeatureClass>("main.surface");

                    //    using var cursor = fc.Search(new SpatialQueryFilter {
                    //        WhereClause = $"({e.Filters.WhereClause}) AND CODE = 'DataCoverage'",
                    //        FilterGeometry = e.Filters.FilterGeometry,
                    //        SpatialRelationship = SpatialRelationship.Relation,
                    //        SpatialRelationshipDescription = DE9IM_Contains,
                    //    }, true);

                    //    ;
                    //    while (cursor.MoveNext()) {
                    //        var c = (ArcGIS.Core.Data.Feature)cursor.Current;
                    //    }
                    //}

                    try {
                        var supportFiles = new List<string>();

                        var dataset = e.Dataset;
                        var filter = e.Filters;

                        var datasetName = dataset.CellName.Split('.')[0];

                        Log.Information("{dataset}", datasetName);
                        var spatialAssociations = new Dictionary<string, S100FC.YAML.Association>();
                        var geometries = new List<(ArcGIS.Core.Geometry.Geometry geometry, string name)>();

                        // Build Topology
                        Log.Information("Building topology..");
                        int index = 0;
                        var result = source.BuildTopology(filter, interceptor: (code, arg) => {
                            var persist = code switch {
                                9000 => false,
                                9001 => false,
                                9002 => false,
                                6000 => true,
                                6001 => false,
                                7000 => true,
                                8001 => false,
                                8002 => false,
                                1000 => true,
                                _ => true,
                            };

                            if (!persist) return;

                            //  L:\B061450\ArcGIS\s100ed14_carolina\SQLServer-ncps-sql101041-topology(sde).sde

                            Func<Geodatabase> debugInstanceCreator = () => {
                                if (index == 0) {
                                    foreach (var f in System.IO.Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, $"*topology*.geodatabase*")) {
                                        if (IO.Path.GetFileName(f).Equals("topology.geodatabase")) continue;
                                        System.IO.File.Delete(System.IO.Path.GetFullPath(f));
                                    }
                                }

                                return new Geodatabase(new MobileGeodatabaseConnectionPath(new Uri(IO.Path.GetFullPath("topology.geodatabase"))));
                            };

                            if (IO.File.Exists(@"L:\B061450\ArcGIS\s100ed14_carolina\SQLServer-ncps-sql101041-topology(sde).sde")) {
                                debugInstanceCreator = () => {
                                    return new Geodatabase(new DatabaseConnectionFile(new Uri(IO.Path.GetFullPath(@"L:\B061450\ArcGIS\s100ed14_carolina\SQLServer-ncps-sql101041-topology(sde).sde"))));
                                };
                            }

                            {
                                index += 1;

                                using var debugInstance = debugInstanceCreator();

                                var spatialReference = SpatialReferenceBuilder.CreateSpatialReference(4326);

                                var defnitions = debugInstance.GetDefinitions<FeatureClassDefinition>().ToDictionary(e => e.GetName().ToLowerInvariant().Split('.')[^1], e => e.GetName());

                                using var point = debugInstance.OpenDataset<FeatureClass>(defnitions["point"]);
                                using var polyline = debugInstance.OpenDataset<FeatureClass>(defnitions["linestring"]);
                                using var polygon = debugInstance.OpenDataset<FeatureClass>(defnitions["polygon"]);
                                if (index == 1) {
                                    point.DeleteRows(new QueryFilter {
                                        WhereClause = "1=1",
                                    });
                                    polyline.DeleteRows(new QueryFilter {
                                        WhereClause = "1=1",
                                    });
                                    polygon.DeleteRows(new QueryFilter {
                                        WhereClause = "1=1",
                                    });
                                }

                                var array = arg.ToArray();

                                Action build = code switch {
                                    >= 8000 => () => {
                                        using var buffer = polyline.CreateRowBuffer();
                                        for (int i = 0; i < array.Length; i++) {
                                            buffer["message"] = $"{i}: {array[i].message}";
                                            buffer["shape"] = ConvertToArcGISPolyline(array[i].lineString, spatialReference);
                                            using var f = polyline.CreateRow(buffer);
                                        }
                                    }
                                    ,

                                    >= 7000 => () => {
                                        using var buffer = polyline.CreateRowBuffer();
                                        for (int i = 0; i < array.Length; i++) {
                                            buffer["message"] = $"{array[i].message}";
                                            buffer["shape"] = ConvertToArcGISPolyline(array[i].lineString, spatialReference);
                                            using var f = polyline.CreateRow(buffer);
                                        }
                                    }
                                    ,

                                    >= 6000 => () => {
                                        using (var buffer = polyline.CreateRowBuffer()) {
                                            for (int i = 0; i < array.Length; i++) {
                                                foreach (var segment in Enumerable.Range(0, array[i].lineString.NumPoints - 1).Select(j => new NetTopologySuite.Geometries.LineSegment(array[i].lineString.GetCoordinateN(j), array[i].lineString.GetCoordinateN(j + 1)))) {
                                                    buffer["message"] = $"{i}: {segment.ToString()}, {array[i].message}";
                                                    buffer["shape"] = ConvertToArcGISPolyline(array[i].lineString.Factory.CreateLineString([segment.GetCoordinate(0), segment.GetCoordinate(1)]), spatialReference);
                                                    using var f = polyline.CreateRow(buffer);
                                                }
                                            }
                                        }
                                        using (var buffer = point.CreateRowBuffer()) {
                                            for (int i = 0; i < array.Length; i++) {
                                                var linestring = array[i];
                                                for (int j = 0; j < linestring.lineString.NumPoints; j++) {
                                                    var coord = linestring.lineString.GetPointN(j);
                                                    buffer["message"] = $"{i}: {j} ({coord.ToText()}";
                                                    buffer["shape"] = MapPointBuilderEx.CreateMapPoint(coord.X, coord.Y, spatialReference);
                                                    using var f = point.CreateRow(buffer);
                                                }
                                            }
                                        }
                                        //using (var buffer = polygon.CreateRowBuffer()) {
                                        //    for (int i = 0; i < array.Length; i++) {
                                        //        var p = PolygonBuilderEx.CreatePolygon(array[i].lineString.Coordinates.Select(e => MapPointBuilderEx.CreateMapPoint(e.X, e.Y, spatialReference)));
                                        //        buffer["message"] = $"{i}: {array[i].message}";
                                        //        buffer["shape"] = p;
                                        //        using var f = polygon.CreateRow(buffer);
                                        //    }
                                        //}
                                    }
                                    ,

                                    >= 1000 => () => {
                                        var coords = array.SelectMany(e => e.lineString.Coordinates).Select(e => MapPointBuilderEx.CreateMapPoint(e.X, e.Y, spatialReference));

                                        var p = PolygonBuilderEx.CreatePolygon(coords);

                                        using var buffer = polygon.CreateRowBuffer();
                                        buffer["message"] = $"{index}";
                                        buffer["shape"] = p;
                                        using var f = polygon.CreateRow(buffer);
                                    }
                                    ,
                                    _ => () => {
                                        using var buffer = polyline.CreateRowBuffer();
                                        for (int i = 0; i < array.Length; i++) {
                                            buffer["message"] = $"{i}: {array[i].message}";
                                            buffer["shape"] = ConvertToArcGISPolyline(array[i].lineString, spatialReference);
                                            using var f = polyline.CreateRow(buffer);
                                        }
                                    }
                                    ,
                                }
                            ;

                                build();
                            }

                        })!;

                        var topology = result.matrix;

                        Log.Information("Topology finished! Found {curves} Curves, {composites} CompositeCurves, {surfaces} Surfaces", topology.Curves.Count(), topology.CompositeCurves.Count(), topology.Surfaces.Count());

                        //  Selector
                        {
                            var uid = topology.MappingFOID.Keys.Select(e => $"'{e}'");
                            var select = $"UID IN ({string.Join(',', uid)})";
                            ;
                        }

                        //  Debug
                        {
                            //using var debugInstance = new Geodatabase(new MobileGeodatabaseConnectionPath(new Uri(IO.Path.GetFullPath("topology.geodatabase"))));

                            //var spatialReference = SpatialReferenceBuilder.CreateSpatialReference(4326);

                            //using var polyline = debugInstance.OpenDataset<FeatureClass>("main.curve");
                            //{
                            //    using var buffer = polyline.CreateRowBuffer();
                            //    foreach (CurveFeature curf in topology.Curves) {
                            //        var shape = ConvertToArcGISPolyline(curf.LineString, spatialReference);

                            //        buffer["id"] = $"C{curf.Id}";
                            //        buffer["shape"] = shape;
                            //        using var f = polyline.CreateRow(buffer);
                            //    }
                            //}

                            //using var compositecurve = debugInstance.OpenDataset<Table>("main.compositecurve");
                            //{
                            //    using var buffer = compositecurve.CreateRowBuffer();
                            //    foreach (var curve in topology.CompositeCurves) {
                            //        buffer["id"] = $"C{curve.Id}";
                            //        buffer["CurveIds"] = string.Join(',', curve.Curves.Select(e => e.Reverse ? $"RC{e.Id}" : $"C{e.Id}"));
                            //        using var f = compositecurve.CreateRow(buffer);
                            //    }
                            //}

                            //using var surface = debugInstance.OpenDataset<FeatureClass>("main.surface");
                            //{
                            //    using var buffer = surface.CreateRowBuffer();
                            //    foreach (var s in topology.Surfaces) {
                            //        buffer["id"] = $"S{s.Id}";
                            //        buffer["CompositeCurveId"] = $"S{s.Exterior.Id}";
                            //        using var f = surface.CreateRow(buffer);
                            //    }
                            //}
                        }



                        filter.SubFields = "OBJECTID,UID,GLOBALID,CODE,attributeBindings,informationBindings,featureBindings";

                        // InformationTypes
                        var informationTypes = new List<S100FC.YAML.Information>();
                        var informationsTypesAdded = new List<string>();

                        try {
                            using var informationType = source.OpenDataset<Table>(definitionTables.Single(e => syntax.ParseTableName(e.GetName()).Item3.Equals("informationtype")).GetName());
                            using var informationCursor = informationType.Search();
                            while (informationCursor.MoveNext()) {
                                var current = informationCursor.Current;

                                var name = Convert.ToString(current["UID"]);
                                var code = current["code"].ToString()!;
                                //var json = current["attributebindings"].ToString()!;

                                var type = featureCatalogue.Assembly!.GetType($"{S100FC.Catalogues.FeatureCatalogue.Namespace("S101", "InformationTypes")}.{code}", true)!;

                                var json = Convert.ToString(current["attributebindings"]);
                                var instance = string.IsNullOrEmpty(json) ? null : S100FC.AttributeFlattenExtensions.Unflatten<InformationType>(json, type);

                                //var instance = DBNull.Value.Equals(current["json"]) ? null : System.Text.Json.JsonSerializer.Deserialize(Convert.ToString(current["json"])!, type, jsonSerializerOptionsS101); // jsonSerializerOptionsS101

                                var information = new S100FC.YAML.Information {
                                    Name = code,
                                    ID = name,
                                    Attributes = (S100FC.InformationType)instance!
                                };
                                informationTypes.Add(information);

                                var filenames = S100FC.YAML.Extensions.GetFileNames(json);

                                foreach (var filename in filenames) {
                                    if (!supportFiles.Contains(filename)) {
                                        supportFiles.Add(filename);

                                        var attachment = source.GetAttachment(filename);
                                        if (attachment is not null) {
                                            var base64 = Convert.ToBase64String(attachment.Value.stream.ToArray());
                                            dataset?.Metadata.AddSupportFile(filename, base64);
                                        }
                                        else
                                            System.Diagnostics.Debugger.Break();

                                        //var _ = fileReferenceRegex.Replace(filename, filename.Substring(3, 2));
                                        //var file = directoryNotes?.GetFiles(_, SearchOption.AllDirectories).FirstOrDefault();
                                        //if (file != null) {
                                        //    var base64 = Convert.ToBase64String(IO.File.ReadAllBytes(file.FullName));
                                        //    dataset?.Metadata.AddSupportFile(filename, base64);
                                        //}
                                    }
                                }
                            }
                        }
                        catch (Exception ex) {
                            Log.Error("Exception: {ex}", ex);
                        }

                        // FeatureTypes
                        var featureTypes = new List<S100FC.YAML.Feature>();
                        var featureTypesAdded = new List<string>();

                        try {
                            using var featureType = source.OpenDataset<Table>(definitionTables.Single(e => syntax.ParseTableName(e.GetName()).Item3.Equals("featuretype")).GetName());
                            using var featureCursor = featureType.Search();
                            while (featureCursor.MoveNext()) {
                                var current = featureCursor.Current;

                                var name = Convert.ToString(current["UID"])!;
                                var code = current["code"].ToString()!;
                                //var json = current["json"].ToString()!;

                                var json = Convert.ToString(current["attributebindings"]);

                                var type = featureCatalogue.Assembly!.GetType($"{S100FC.Catalogues.FeatureCatalogue.Namespace("S101", "FeatureTypes")}.{code}", true)!;

                                var instance = string.IsNullOrEmpty(json) ? null : S100FC.AttributeFlattenExtensions.Unflatten<S100FC.FeatureType>(json, type);
                                //var instance = DBNull.Value.Equals(current["json"]) ? null : System.Text.Json.JsonSerializer.Deserialize(Convert.ToString(current["json"])!, type, jsonSerializerOptionsS101) as S100FC.FeatureType;// jsonSerializerOptionsS101

                                var foid = $"110:{name.Substring(1)}:1";       // Geodatastyrelsen: 110 

                                var feature = new S100FC.YAML.Feature {
                                    Prim = Primitive.NoGeometry,
                                    Name = code,
                                    Foid = foid,
                                    Attributes = instance,
                                };

                                featureTypes.Add(feature);

                                var filenames = S100FC.YAML.Extensions.GetFileNames(json);

                                foreach (var filename in filenames) {
                                    if (!supportFiles.Contains(filename)) {
                                        supportFiles.Add(filename);

                                        var attachment = source.GetAttachment(filename);
                                        if (attachment is not null) {
                                            var base64 = Convert.ToBase64String(attachment.Value.stream.ToArray());
                                            dataset?.Metadata.AddSupportFile(filename, base64);
                                        }
                                        else
                                            System.Diagnostics.Debugger.Break();

                                        //var _ = fileReferenceRegex.Replace(filename, filename.Substring(3, 2));
                                        //var file = directoryNotes!.GetFiles(_, SearchOption.AllDirectories).First();
                                        //var base64 = Convert.ToBase64String(IO.File.ReadAllBytes(file.FullName));
                                        //dataset?.Metadata.AddSupportFile(filename, base64);
                                    }
                                }
                            }
                        }
                        catch (Exception ex) {
                            Log.Error("Exception: {ex}", ex);
                        }


                        // Features
                        foreach (var def in source.GetDefinitions<FeatureClassDefinition>()) {
                            var tableName = def.GetName().Split('.').Last();

                            var supported = tableName switch {
                                "curve" => true,
                                "point" => true,
                                "pointset" => true,
                                "surface" => true,
                                "topo_curve" => true,
                                "topo_point" => true,
                                "topo_pointset" => true,
                                "topo_surface" => true,

                                _ => false
                            };

                            if (!supported) {
                                Log.Information("Unsupported table detected: {tableName}", tableName);
                                continue;
                            }

                            using var fc = source.OpenDataset<FeatureClass>(def.GetName());

                            string[] filters = [Matrix.DE9IM_Contains, Matrix.DE9IM_Crosses];

                            var hashSet = new HashSet<long>();

                            foreach (var de9im in filters) {
                                filter.SpatialRelationshipDescription = de9im;

                                using var cursor = fc.Search(filter, true);
                                while (cursor.MoveNext()) {
                                    var current = (ArcGIS.Core.Data.Feature)cursor.Current;

                                    var oid = current.GetObjectID();
                                    if (hashSet.Contains(oid)) continue;
                                    hashSet.Add(oid);

                                    var _uid = Convert.ToString(current["UID"])!;

                                    string[] features = [_uid];

                                    if (result.mapper.Values.Contains(_uid)) {
                                        features = result.mapper.Where(e => e.Value.Equals(_uid)).Select(e => e.Key).ToArray();
                                    }

                                    foreach (var uid in features) {
                                        // Only map geometry, and keep name seperate so foids remain unique
                                        var geometry = uid;

                                        var shapetype = def.GetShapeType();

                                        var prim = shapetype switch {
                                            GeometryType.Point => Primitive.Point,
                                            GeometryType.Multipoint => Primitive.Point,
                                            GeometryType.Polyline => Primitive.Curve,
                                            GeometryType.Polygon => Primitive.Surface,
                                            _ => throw new InvalidOperationException(),
                                        };


                                        if (topology.MappingFOID.TryGetValue(uid!, out var value)) {
                                            geometry = value;
                                        }
                                        else if (prim == Primitive.Surface || prim == Primitive.Curve)
                                            continue;

                                        var code = Convert.ToString(current["code"]);

                                        var foid = uid.Contains(':') ? $"110:{uid.Substring(1)}" : $"110:{uid.Substring(1)}:1";       // Geodatastyrelsen: 110 
                                                                                                                                      //var foid = $"110:{uid.Substring(1)}:1";


                                        try {
                                            var type = featureCatalogue.Assembly!.GetType($"{S100FC.Catalogues.FeatureCatalogue.Namespace("S101", "FeatureTypes")}.{code}", true) ?? default;

                                            if (type == default) {
                                                Log.Error("Could not get type: {type} for feature: {name}", code, uid);
                                                continue;
                                            }

                                            var json = Convert.ToString(current["attributebindings"])!;

                                            var instance = string.IsNullOrEmpty(json) ? null : S100FC.AttributeFlattenExtensions.Unflatten<S100FC.FeatureType>(json, type);

                                            var filenames = S100FC.YAML.Extensions.GetFileNames(json);

                                            foreach (var filename in filenames) {
                                                if (!supportFiles.Contains(filename)) {
                                                    supportFiles.Add(filename);

                                                    var attachment = source.GetAttachment(filename);
                                                    if (attachment is not null) {
                                                        var base64 = Convert.ToBase64String(attachment.Value.stream.ToArray());
                                                        dataset?.Metadata.AddSupportFile(filename, base64);
                                                    }
                                                    //else
                                                    //    System.Diagnostics.Debugger.Break();

                                                    //var _ = fileReferenceRegex.Replace(filename, filename.Substring(3, 2));
                                                    //var file = directoryNotes!.GetFiles(_, SearchOption.AllDirectories).First();
                                                    //var base64 = Convert.ToBase64String(IO.File.ReadAllBytes(file.FullName));
                                                    //dataset?.Metadata.AddSupportFile(filename, base64);
                                                }
                                            }

                                            // Surface Masks
                                            var topologySurface = topology.Surfaces.FirstOrDefault(e => e.Ref!.Equals(uid, StringComparison.InvariantCultureIgnoreCase));

                                            // Build comma seperated string of masks, with :1 or :2 indicating which mask it is. Should be null/omitted if empty.
                                            var masks = new[] {
                                                        topologySurface?.Masks1?.Select(e => $"C{e}:1"),
                                                        topologySurface?.Masks2?.Select(e => $"C{e}:2")
                                                    }.Where(m => m != null).SelectMany(m => m!);

                                            var feature = new S100FC.YAML.Feature {
                                                Name = code,
                                                Foid = foid,
                                                Prim = prim,
                                                Geometry = geometry,
                                                Masks = masks.Any() ? string.Join(",", masks) : null,
                                                Attributes = instance?.attributeBindings.Length > 0 ? instance : null
                                            };


                                            // Information Associations
                                            if (!current.IsNull("informationbindings")) {
                                                var informationBindings = System.Text.Json.JsonSerializer.Deserialize<informationBinding[]>(Convert.ToString(current["informationbindings"])!, jsonSerializerOptionsS101); // jsonSerializerOptionsS101

                                                if (informationBindings != default && informationBindings.Any()) {
                                                    foreach (var binding in informationBindings) {
                                                        var asso = new S100FC.YAML.Association {
                                                            Name = binding.association!.S100FC_code,
                                                            Role = binding.role,
                                                            To = binding.informationId!,
                                                        };

                                                        // Special case for SpatialAssociation. Add to dictionary for later processing.
                                                        if (prim != Primitive.Surface && asso.Name.Equals("SpatialAssociation", StringComparison.CurrentCultureIgnoreCase))
                                                            spatialAssociations.TryAdd(geometry, asso);
                                                        else
                                                            feature?.AddAssociation(asso);

                                                        if (!informationsTypesAdded.Contains(binding.informationId!)) {
                                                            informationsTypesAdded.Add(binding.informationId!);
                                                            dataset!.AddInformation(informationTypes.Single(e => e.ID!.Equals(binding.informationId!)));
                                                        }
                                                    }
                                                }
                                            }

                                            // Feature Associations
                                            if (!current.IsNull("featurebindings")) {
                                                var featureBindingsJson = Convert.ToString(current["featurebindings"])!;
                                                var featureBindings = System.Text.Json.JsonSerializer.Deserialize<featureBinding[]>(featureBindingsJson, jsonSerializerOptionsS101); // jsonSerializerOptionsS101

                                                if (featureBindings != default && featureBindings.Any()) {
                                                    foreach (var binding in featureBindings) {
                                                        var roleType = binding.roleType;

                                                        // Skip association roleType for now
                                                        if (roleType == "association")
                                                            continue;

                                                        var asso = new S100FC.YAML.Association {
                                                            Name = binding.association!.S100FC_code,
                                                            Role = binding.role,
                                                            To = $"110:{binding.featureId!.Substring(1)}:1"
                                                        };

                                                        feature?.AddFeatureAssociation(asso);

                                                        var noGeometry = featureTypes.SingleOrDefault(e => e.Foid.Equals($"110:{binding.featureId.Substring(1)}:1"));
                                                        if (noGeometry != null && !featureTypesAdded.Contains(binding.featureId)) {
                                                            featureTypesAdded.Add(binding.featureId);
                                                            dataset?.AddFeature(noGeometry);
                                                        }
                                                    }
                                                }
                                            }

                                            //if ("F10500070853".Equals(name)) System.Diagnostics.Debugger.Break();

                                            //var lookup = topology.MappingFeature(name);

                                            //if (!lookup.Any())
                                            dataset?.AddFeature(feature!);
                                            //else {
                                            //    int _ = 1;
                                            //    foreach (var c in lookup) {
                                            //        feature!.Foid = $"110:{name.Substring(1)}:{_++}";
                                            //        feature!.Geometry = c;
                                            //        dataset?.AddFeature(feature!);
                                            //    }
                                            //}

                                            Action geometryConverter = tableName.Split('.', StringSplitOptions.RemoveEmptyEntries)[^1] switch {
                                                "pointset" or "topo_pointset" => () => {
                                                    var _ = MultipointBuilderEx.CreateMultipoint((MapPoint)current.GetShape());
                                                    geometries.Add(new(_, uid!));
                                                }
                                                ,
                                                "point" or "topo_point" => () => {
                                                    geometries.Add(new(current.GetShape(), uid!));
                                                }
                                                ,
                                                _ => () => { }
                                                ,
                                            };

                                            geometryConverter();
                                            //geometries.Add(new(current.GetShape(), name!));                                        
                                        }
                                        catch (Exception ex) {
                                            Log.Error("Exception: {ex}", ex);
                                            continue;
                                        }
                                    }
                                }
                            }
                        }

                        Log.Information("FeatureTypes (noGeometry) found: #{count}", featureTypesAdded.Count);
                        Log.Information("InformationTypes found: #{count}", informationsTypesAdded.Count);

                        // Geometries
                        foreach (var (geometry, name) in geometries.OrderBy(e => e.geometry.GeometryType)) {
                            if (geometry.GeometryType == GeometryType.Polygon) continue;    // Skip polygons after topology
                            if (geometry.GeometryType == GeometryType.Polyline) continue;    // Skip curves after topology
                            dataset?.AddGeometry(geometry, name!);
                            Log.Verbose("Adding {geometryType} with ID: {name}", geometry.GeometryType, name);
                        }

                        // Add curves/surfaces after points
                        dataset!.AddTopology(topology);

                        // Add Spatial Association Informationbindings. Must be handled after curves are added to dataset.
                        foreach (var sa in spatialAssociations) {
                            var curve = dataset?.Curves?.FirstOrDefault(e => e.Name == sa.Key);

                            curve?.AddAssociation(sa.Value);
                        }

                        // Serialize to YAML
                        var yaml = S100FC.YAML.Converter.Serialize(dataset!);



                        File.WriteAllText(IO.Path.Combine(output, $"{datasetName}.yaml"), yaml);
                        File.WriteAllText(IO.Path.Combine(@"c:\temp", $"{datasetName}.yaml"), yaml);

                        var s100compiler = @"C:\Program Files\s100compiler\s100compiler.exe";

                        if (!IO.File.Exists(s100compiler)) {
                            var _output = new IO.DirectoryInfo(output);
                            var search = IO.Directory.GetFiles(_output.Parent!.FullName, "s100compiler.exe", SearchOption.AllDirectories);
                            if (search.Any())
                                s100compiler = IO.Path.GetFullPath(search.First());
                        }

                        if (IO.File.Exists(@"C:\Program Files\s100compiler\s100compiler.exe")) {
                            var commandline = $"-f \"{IO.Path.Combine(output, $"{datasetName}.yaml")}\" -c \"{IO.Path.GetFullPath(featureCataloguePath!)}\" -d \"{output}\"";

                            if (IO.Directory.Exists(IO.Path.Combine(output, datasetName)))
                                IO.Directory.Delete(IO.Path.Combine(output, datasetName), true);
                            // IO.Directory.CreateDirectory(IO.Path.Combine(output, datasetName));

                            if (!exchangeset) {
                                //Log.Information("s100compiler.exe -f {dataset}.yaml -d {output}.000 -c {fc}", datasetName, output, IO.Path.GetFileName(featureCataloguePath));

                                Log.Information("s100compiler.exe {arguments}", commandline);

                                var p = new Process();
                                p.StartInfo.CreateNoWindow = true;
                                p.StartInfo.UseShellExecute = false;
                                p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                                p.StartInfo.FileName = @"C:\Program Files\s100compiler\s100compiler.exe";
                                p.StartInfo.Arguments = commandline;
                                p.StartInfo.WorkingDirectory = output;
                                p.StartInfo.RedirectStandardOutput = false;
                                p.StartInfo.RedirectStandardError = true;
                                p.EnableRaisingEvents = true;
                                p.Exited += (s, e) => {
                                };

                                Log.Verbose("{filename} {arguments}", p.StartInfo.FileName, p.StartInfo.Arguments);

                                p.Start();
                                p.WaitForExit();

                                if (p.ExitCode != 0) {
                                    var error = p.StandardError.ReadToEnd();

                                    Log.Error("\"{filename}\" {arguments}", p.StartInfo.FileName, commandline);
                                    Log.Verbose(error);
                                    return p.ExitCode;
                                }
                            }
                            else {
                                Log.Information("s100compiler.exe -f {dataset}.yaml -d {output}.000 -C {dataset} -c {fc}", datasetName, output, IO.Path.GetFileName(featureCataloguePath));
                                commandline += $" -C {datasetName}";

                                var p = new Process();
                                p.StartInfo.CreateNoWindow = true;
                                p.StartInfo.UseShellExecute = false;
                                p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                                p.StartInfo.FileName = @"C:\Program Files\s100compiler\s100compiler.exe";
                                p.StartInfo.Arguments = commandline;
                                p.StartInfo.WorkingDirectory = output;
                                p.StartInfo.RedirectStandardOutput = false;
                                p.StartInfo.RedirectStandardError = true;
                                p.EnableRaisingEvents = true;
                                p.Exited += (s, e) => {
                                };

                                p.Start();
                                p.WaitForExit();

                                if (p.ExitCode != 0) {
                                    var error = p.StandardError.ReadToEnd();

                                    Log.Error("\"{filename}\" {arguments}", p.StartInfo.FileName, commandline);
                                    Log.Verbose(error);
                                    return p.ExitCode;
                                }

                                IO.Directory.Move(IO.Path.Combine(output, "S100_ROOT"), IO.Path.Combine(output, $"{datasetName}_ROOT"));
                            }
                        }

                        if (s57) {
                            if (IO.File.Exists(@"c:\Program Files\s57compiler\s57compiler.exe")) {
                                if (IO.File.Exists(@"c:\Program Files\s100mapper\s100mapper.exe")) {
                                    var filename_s101 = IO.Path.Combine(output, $"{datasetName}.yaml");
                                    var filename_s57 = fileReferenceRegex.Replace(datasetName, datasetName.Substring(3, 2));
                                    filename_s57 = IO.Path.Combine(output, $"{filename_s57}.yaml");

                                    var pipeline = IO.Path.Combine(IO.Path.GetDirectoryName(featureCataloguePath!)!, "pipeline-S101-S57.yaml");
                                    var s100mapper = $"\"{filename_s101}\" \"{filename_s57}\" --fc \"{IO.Path.GetFullPath(featureCataloguePath!)}\" --pipeline \"{pipeline}\"";

                                    Log.Information("s100mapper.exe {s101}.yaml {filename_s57}.yaml --fc {fc} --pipeline pipeline-S101-S57.yaml", datasetName, IO.Path.GetFileNameWithoutExtension(filename_s57), IO.Path.GetFileName(featureCataloguePath));

                                    var p = new Process();
                                    p.StartInfo.CreateNoWindow = true;
                                    p.StartInfo.UseShellExecute = false;
                                    p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                                    p.StartInfo.FileName = @"C:\Program Files\s100mapper\s100mapper.exe";
                                    p.StartInfo.Arguments = s100mapper;
                                    p.StartInfo.WorkingDirectory = IO.Path.GetDirectoryName(pipeline);
                                    p.StartInfo.RedirectStandardOutput = false;
                                    p.StartInfo.RedirectStandardError = true;
                                    p.EnableRaisingEvents = true;
                                    p.Exited += (s, e) => {
                                        ;
                                    };

                                    p.Start();
                                    p.WaitForExit();

                                    if (p.ExitCode != 0) {
                                        var error = p.StandardError.ReadToEnd();

                                        Log.Error("\"{filename}\" {arguments}", IO.Path.GetFileName(p.StartInfo.FileName), s100mapper);
                                        Log.Verbose(error);
                                        return p.ExitCode;
                                    }

                                    var s57Compiler = $"\"{s57}\" s57";
                                    p.StartInfo.FileName = @"C:\Program Files\s57Compiler\s57Compiler.exe";
                                    p.StartInfo.Arguments = s57Compiler;
                                    p.StartInfo.WorkingDirectory = IO.Path.GetDirectoryName(output);

                                    p.Start();
                                    p.WaitForExit();

                                    if (p.ExitCode != 0) {
                                        //var console = p.StandardOutput.ReadToEnd();
                                        var error = p.StandardError.ReadToEnd();

                                        Log.Error("\"{filename}\" {arguments}", IO.Path.GetFileName(p.StartInfo.FileName), s100mapper);
                                        Log.Verbose(error);
                                        return p.ExitCode;
                                    }
                                }
                            }
                        }
                    }
                    catch (System.Exception ex) {
                        Log.Fatal("error: {database}", e.Dataset);
                    }
                    Log.Information("------------------------------------------------------------");
                }
                sw.Stop();
                Log.Information("Elapsed: {elapsed}", sw.Elapsed);

                return 0;
            }
            catch (Exception ex) {
                Log.Error(ex, ex.Message);
                return -1;
            }
        }

        public static ArcGIS.Core.Geometry.Polyline ConvertToArcGISPolyline(NetTopologySuite.Geometries.LineString ntsLineString, SpatialReference? spatialReference = null) {
            // Build a collection of MapPoints from NTS coordinates
            var points = ntsLineString.Coordinates
                .Select(coord => {
                    // NTS uses Z if it's not NaN
                    if (!double.IsNaN(coord.Z))
                        return MapPointBuilderEx.CreateMapPoint(coord.X, coord.Y, coord.Z, spatialReference);
                    else
                        return MapPointBuilderEx.CreateMapPoint(coord.X, coord.Y, spatialReference);
                })
                .ToList();

            // Create the ArcGIS Polyline from the point collection
            return PolylineBuilderEx.CreatePolyline(points, spatialReference);
        }
    }
}

namespace ArcGIS.Core.Data
{
    public static partial class Extension
    {
        public static (S100BlueStack.Settings.SupportFile supportFile, MemoryStream stream)? GetAttachment(this Geodatabase geodatabase, string filename) {
            var sqlsystanx = geodatabase.GetSQLSyntax();

            var definitions = geodatabase.GetDefinitions<TableDefinition>();

            var _ = definitions.Single(e => sqlsystanx.ParseTableName(e.GetName()).Item3.Equals("attachment"));
            using var attachment = geodatabase.OpenDataset<Table>(_.GetName());

            filename = filename.Trim().ToUpper();
            if (string.IsNullOrEmpty(filename)) return default;

            using var cursor = attachment.Search(new QueryFilter {
                WhereClause = $"UPPER(json) LIKE '%\"{filename}\"%'",
            }, true);

            if (!cursor.MoveNext()) return default;

            var json = Convert.ToString(cursor.Current["json"]);
            var blob = cursor.Current["data"] as MemoryStream;

            var instance = AttributeFlattenExtensions.Unflatten<S100BlueStack.Settings.SupportFile>(json, typeof(S100BlueStack.Settings.SupportFile));

            return (instance!, blob!);
        }


    }

}