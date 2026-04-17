using ArcGIS.Core.Data;
using ArcGIS.Core.SystemCore;
using CommandLine;
using ICSharpCode.SharpZipLib.Zip;


//using S100Framework.DomainModel
using System.Text.RegularExpressions;
using Esri = ArcGIS.Core.Hosting.Host;
using IO = System.IO;




namespace S100Framework.Applications
{
    internal class VortexLoader
    {
        //NIS: --v --target "C:\Users\Jens Søe\OneDrive\ArcGIS\Projects\Vortex\S100ed4.gdb" --source "C:\Users\Jens Søe\OneDrive\ArcGIS\Projects\Vortex\s57.gdb"

        //  --query "PLTS_COMP_SCALE = 22000"

        //private static Serilog.Core.Logger? _logger;

        private static readonly Regex _substitute = new(@"^S(?<number>\d+)$", RegexOptions.Singleline | RegexOptions.CultureInvariant | RegexOptions.IgnorePatternWhitespace | RegexOptions.IgnoreCase);

        public class Options
        {
            [Option('d', "dataset", Required = false, HelpText = "")]
            public string? Dataset { get; set; }

            [Option('t', "target", Required = true, HelpText = "Target Geodatabase.")]
            public string? Target { get; set; }

            [Option('s', "source", Required = false, HelpText = "Source Geodatabase.")]
            public string? Source { get; set; }

            //[Option('a', "append", Required = false, HelpText = "Append dataset.")]
            //public bool Append { get; set; }

            //[Option('q', "query", Required = false, HelpText = "Definition query.")]
            //public string? Query { get; set; }

            [Option('v', "verbose", Required = false, HelpText = "Set output to verbose messages.")]
            public bool Verbose { get; set; }

            [Option("s128", Required = false, HelpText = "Create ElectronicProducts.", Default = false)]
            public bool S128 { get; set; }

            [Option('n', "notespath", Required = false, HelpText = "Path to notes files references in TXTDSC.")]
            public string? NotesPath { get; set; }

            [Option('s', "skinofearthonly", Required = false, HelpText = "Exports only DEPARE, DRGARE, UNSARE and LNDARE.")]
            public string? SkinOfEarthOnly { get; set; }

            [Option('f', "scaminfiles", Required = false, HelpText = "Path to folder with scamin files. Supports only Grønland and Denmark scamin files.")]
            public string? ScaminFilesPath { get; set; }

            [Option("vdat", Required = false, Default = "3=44")]
            public string? VerticalDatumConverter { get; set; } //  --vdat 3=44            


            [Option('l', "minimumDisplayScale", Default = int.MaxValue, Required = false)]
            public int? minimumDisplayScale { get; set; }

            [Option('u', "maximumDisplayScale", Default = 0, Required = false)]
            public int? maximumDisplayScale { get; set; }
        }

        static void Main(string[] args) {
            var arguments = Parser.Default.ParseArguments<Options>(args)
                               .WithParsed<Options>(o => {
                               });

            AppDomain.CurrentDomain.UnhandledException += (sender, e) => {
                Logger.Current.Fatal((Exception)e.ExceptionObject, "UnhandledException");
            };

            if (arguments.Errors.Any())
                return;

            Esri.Initialize();

            Func<Geodatabase> createGeodatabase = () => { throw new NotImplementedException(); };

            Action<bool> initialize = (append) => { };

            bool append = false;


            {
                FastZip fastZip = new();
                fastZip.ExtractZip(IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "scamin.gdb.zip"), IO.Path.GetFullPath(IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "scamin.gdb")), null);
                fastZip.ExtractZip(IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "coverage.gdb.zip"), IO.Path.GetFullPath(IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "coverage.gdb")), null);
           }

            arguments.WithParsed<Options>(o => {
                var target = o.Target!;

                if (!string.IsNullOrEmpty(o.VerticalDatumConverter)) {
                    var dictionary = o.VerticalDatumConverter.Split(',').Select(e => e.Split('=')).ToDictionary(e => int.Parse(e[0]), e => int.Parse(e[1]));
                }

                if (IO.File.Exists(target) && ".sde".Equals(IO.Path.GetExtension(target), StringComparison.OrdinalIgnoreCase)) {
                    createGeodatabase = () => {
                        var geodatabase = new Geodatabase(new DatabaseConnectionFile(new Uri(IO.Path.GetFullPath(target))));

                        return geodatabase;
                    };
                }
                else if (IO.Directory.Exists(target) && ".gdb".Equals(IO.Path.GetExtension(target), StringComparison.OrdinalIgnoreCase)) {
                    initialize = (append) => {
                        if (!append) {
                            FastZip fastZip = new();

                            //IO.Directory.Delete(target, true);

                            fastZip.ExtractZip(IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "s100ed12.gdb.zip"), IO.Path.GetFullPath(target), null);
                        }
                    };


                    createGeodatabase = () => {
                        var geodatabase = new Geodatabase(new FileGeodatabaseConnectionPath(new Uri(IO.Path.GetFullPath(target))));

                        return geodatabase;
                    };
                }
                else if (Uri.IsWellFormedUriString(target, UriKind.Absolute)) {
                    var arcGisSignOn = ArcGISSignOn.Instance;

                    var signedId = arcGisSignOn.IsSignedOn(new Uri("https://nuvion.gst.dk/portal"));
                    if (!signedId) throw new InvalidOperationException("!signedId");

                    createGeodatabase = () => {
                        var serviceProps = new ServiceConnectionProperties(new Uri(target, UriKind.Absolute)) {
                            Version = "sde.DEFAULT"
                        };

                        var geodatabase = new Geodatabase(serviceProps);
                        return geodatabase;
                    };

                }
                else
                    throw new System.ArgumentOutOfRangeException(nameof(target));

                //append = o.Append;
            });

            initialize(append);

            bool result = ImporterNIS.Load(createGeodatabase, arguments);
        }
#if GML
        private static bool ImporterGML(Geodatabase geodatabase, ParserResult<Options> arguments) {
            S100Framework.GML.Dataset? dataset = null;

            bool append = false;

            arguments.WithParsed<Options>(o => {
                if (o.Append) {
                    append = o.Append;
                }

                if (!IO.File.Exists(o.Dataset))
                    throw new FileNotFoundException(o.Dataset);
                dataset = S100Framework.GML.Dataset.Load(o.Dataset);
            });

            if (dataset is null)
                throw new InvalidProgramException();

            using var tableInformationType = geodatabase.OpenDataset<Table>("informationtype");

            using var fcPoint = geodatabase.OpenDataset<FeatureClass>(geodatabase.GetName("point"));
            using var fcPointSet = geodatabase.OpenDataset<FeatureClass>(geodatabase.GetName("pointset"));
            using var fcCurve = geodatabase.OpenDataset<FeatureClass>(geodatabase.GetName("curve"));
            using var fcSurface = geodatabase.OpenDataset<FeatureClass>(geodatabase.GetName("surface"));

            using var bufferInformationType = tableInformationType.CreateRowBuffer();
            using var bufferPoint = fcPoint.CreateRowBuffer();
            using var bufferPointSet = fcPointSet.CreateRowBuffer();
            using var bufferCurve = fcCurve.CreateRowBuffer();
            using var bufferSurface = fcSurface.CreateRowBuffer();

            if (!append) {
                var filter = new QueryFilter {
                    WhereClause = $"ps = '{dataset.ProductSpecification}'",
                };
                tableInformationType.DeleteRows(filter);
                fcPoint.DeleteRows(filter);
                fcPointSet.DeleteRows(filter);
                fcCurve.DeleteRows(filter);
                fcSurface.DeleteRows(filter);
            }

            var members = dataset.Members();

            var referencedGeometry = new Dictionary<string, string[][]>();

            foreach (var m in members) {
                if (m is S100Framework.GML.Dataset.InformationType informationType) {
                    var value = informationType.Value;

                    Console.WriteLine($"InformationType: {value.GetType().Name}");

                    var json = JsonSerializer.Serialize(value, value!.GetType());

                    var rowbuffer = bufferInformationType;
                    rowbuffer["ps"] = dataset.ProductSpecification;
                    rowbuffer["code"] = value.GetType().Name;
                    row//buffer["__json__"] = json;

                    tableInformationType.CreateRow(bufferInformationType);
                }
                if (m is S100Framework.GML.Dataset.FeatureType featureType) {
                    var value = featureType.Value;

                    var geometryType = featureType.GeometryType;

                    if (geometryType == null)
                        continue;

                    var rowbuffer = geometryType switch {
                        "pointproperty" => bufferPoint,
                        "curveproperty" => bufferCurve,
                        "surfaceproperty" => bufferSurface,
                        _ => throw new NotImplementedException(),
                    };

                    var json = JsonSerializer.Serialize(value, value!.GetType());

                    rowbuffer["ps"] = dataset.ProductSpecification;
                    rowbuffer["code"] = value.GetType().Name;
                    row//buffer["__json__"] = json;

                    // Geometry
                    var coordinates = featureType.Coordinates();

                    if (coordinates == null || coordinates[0].Length == 0) {
                        if (string.IsNullOrEmpty(featureType.GeometryIdentifier))
                            continue;

                        var found = referencedGeometry.TryGetValue(featureType.GeometryIdentifier, out var coords);

                        if (!found)
                            continue;

                        coordinates = coords;
                    }
                    else if (!string.IsNullOrEmpty(featureType.GeometryIdentifier)) {
                        _ = referencedGeometry.TryAdd(featureType.GeometryIdentifier!, coordinates);
                    }

                    var geometry = GeometryExtensions.BuildGeometry(geometryType, coordinates!);

                    if (geometry is MapPoint point) {
                        if (point.HasZ == false)
                            bufferPoint["shape"] = MapPointBuilderEx.CreateMapPoint(point.X, point.Y, 0.00, geometry.SpatialReference);
                        else
                            bufferPoint["shape"] = point;

                        using var row = fcPoint.CreateRow(bufferPoint);
                    }
                    else if (geometry is Multipoint multipoint) {
                        bufferPointSet["shape"] = multipoint;
                        using var row = fcPointSet.CreateRow(bufferPointSet);
                    }
                    else if (geometry is Polyline curve) {
                        bufferCurve["shape"] = curve;
                        using var row = fcCurve.CreateRow(bufferCurve);
                    }
                    else if (geometry is Polygon polygon) {
                        bufferSurface["shape"] = polygon;
                        using var row = fcSurface.CreateRow(bufferSurface);
                    }
                }
            }

            return true;
        }
#endif
    }
}