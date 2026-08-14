using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Internal.Threading.Tasks;
using NetTopologySuite.Algorithm;
using S100Framework.Applications;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using IO = System.IO;

namespace TestNisImporter
{
    public class TestNisImporter
    {
        internal struct Sequence
        {
            public decimal Duration { get; set; }
            public int Status { get; set; }

            public Sequence(decimal duration, int status) {
                this.Duration = duration;
                this.Status = status;
            }
        }

        private readonly ITestOutputHelper _output;

        public TestNisImporter(ITestOutputHelper output) {
            this._output = output;
            ArcGIS.Core.Hosting.Host.Initialize();
        }

        [Fact]
        public void TestStatus() {
            // Assert.True(ImporterNIS.GetStatus(status).Count == 2, "");
        }

        [Fact]
        public void TestGetCommunicationChannel() {

            //Assert.True(ImporterNIS.GetCommunicationChannel("[74]")[0] == "[VHF0074]");
            //Assert.True(ImporterNIS.GetCommunicationChannel("[04]")[0] == "[VHF0004]");
            //Assert.True(ImporterNIS.GetCommunicationChannel("[WX1]")[0] == "[WX0001]");
            //Assert.True(ImporterNIS.GetCommunicationChannel("[WX01];[04]")[0] == "[WX0001]");
            //Assert.True(ImporterNIS.GetCommunicationChannel("[WX01];[04]")[1] == "[VHF0004]");
        }

        [Fact]
        public void TestSordat() {
            {
                Assert.True(DateHelper.TryConvertSordat("20200613", out var result));
                Assert.Equal("20200613", result);
            }
            {
                Assert.True(DateHelper.TryConvertSordat("202006", out var result));
                Assert.Equal("202006--", result);
            }
            {
                Assert.True(DateHelper.TryConvertSordat("2008", out var result));
                Assert.Equal("2008----", result);
            }
            {
                Assert.False(DateHelper.TryConvertSordat("200813", out var result));
            }
            {
                Assert.False(DateHelper.TryConvertSordat("20081232", out var result));
            }
        }


        [Fact]
        public void TestVesselSpeedLimitExtraction() {
            string[] inputs = {
                "Motorised navigation faster than 6 knots is prohibited.",
                "Speed limit 10 knots",
                "Speedlimit is 10 Knots.",
                "Speed limit is 3 knots",
                "Speed limit is 12 knots outside the channel",
                "Speedlimit 5 knots",
                "During the period from 1st July to 30th September the speed limit is 10 Knots."
            };

            foreach (var text in inputs) {
                //Assert.True(ImporterNIS.GetVesselSpeedLimit(text).Count > 0);
            }

        }


        [Fact]
        public void TestRounding() {

            Assert.True(RoundToIHO(5.6d) == 5.6d);
        }
        public static double RoundToIHO(double value) {

            if (value < -31d) {
                return Math.Floor(value);
            }
            else if (value < -21.0d) {
                return value % 1 < 0.5 ? Math.Ceiling(value) - 0.5d : Math.Ceiling(value);
            }
            else if (value < 0) {
                return RoundDownwards(value, 1, -0.5d);
            }
            else if (value < 21.0d) {
                return RoundDownwards(value, 1);
            }
            else if (value < 31) {
                return value % 1 < 0.5 ? Math.Floor(value) : Math.Floor(value) + 0.5;
            }

            return Math.Floor(value);
        }
        public static double RoundDownwards(double value, int digits, double offset = 0d) {
            var power10 = 1E1;
            value *= power10;
            value += offset;
            value = Math.Truncate(value);
            return value /= power10;
        }

        [Fact]
        public void TestRadarWaveLength() {
            ////var rwl1 = ImporterNIS.GetRadarWaveLengths("0.10-S");
            //{
            //    ImporterNIS.TryGetRadarWaveLengths("0.03-X,0.10-S", out var lengths);
            //    Assert.True(lengths.Count == 2, "");
            //    Assert.True(lengths[0].radarBand == "X");
            //    Assert.True(lengths[0].waveLengthValue == 0.03d);
            //    Assert.True(lengths[1].radarBand == "S");
            //    Assert.True(lengths[1].waveLengthValue == 0.10d);
            //}
            //{
            //    ImporterNIS.TryGetRadarWaveLengths("0.10-S", out var lengths);
            //    Assert.True(lengths.Count == 1, "");
            //    Assert.True(lengths[0].radarBand == "S");
            //    Assert.True(lengths[0].waveLengthValue == 0.10d);
            //}
        }

        [Fact]
        public void TestScaleMinimum() {
            //ImporterNIS._scaminFilesPath = @"G:\indigo\Configuration";
            //{
            //    var val1 = Scamin.Instance.GetMinimumScale(MapPointBuilder.CreateMapPoint(57.0488, 9.9217, SpatialReferences.WGS84), "DMPGRD_DumpingGround", 22000);
            //    Assert.True(val1.HasValue);
            //    Assert.True(val1.Value == 89999, "Wrong scamin");

            //    var val2 = Scamin.Instance.GetMinimumScale(MapPointBuilder.CreateMapPoint(57.0488, 9.9217, SpatialReferences.WGS84), "DMPGRD_DumpingGroundXX", 22000);
            //    Assert.False(val2.HasValue);
            //}
            //{
            //    var val1 = Scamin.Instance.GetMinimumScale(MapPointBuilder.CreateMapPoint(57.0488, 9.9217, SpatialReferences.WGS84), "FLODOC_FloatingDock", 22000);
            //    Assert.False(val1.HasValue);
            //    Assert.True(val1.GetValueOrDefault() == 44999, "Wrong scamin");

            //    var val2 = Scamin.Instance.GetMinimumScale(MapPointBuilder.CreateMapPoint(57.0488, 9.9217, SpatialReferences.WGS84), "FLODOC_FloatingDock", 22000);
            //    Assert.False(val2.HasValue);
            //    Assert.True(val2.GetValueOrDefault() == 44999, "Wrong scamin");

            //    var val3 = Scamin.Instance.GetMinimumScale(MapPointBuilder.CreateMapPoint(57.0488, 9.9217, SpatialReferences.WGS84), "FLODOC_FloatingDock", 22000);
            //    Assert.False(val3.HasValue);
            //}
            //{
            //    var val1 = Scamin.Instance.GetMinimumScale(MapPointBuilder.CreateMapPoint(57.0488, 9.9217, SpatialReferences.WGS84), "BRIDGE_Bridge", 22000); // step value is null
            //    Assert.False(val1.HasValue);
            //    Assert.True(val1.GetValueOrDefault() == 44999, "Wrong scamin");
            //    var val2 = Scamin.Instance.GetMinimumScale(MapPointBuilder.CreateMapPoint(57.0488, 9.9217, SpatialReferences.WGS84), "DMPGRD_DumpingGroundXX", 22000);
            //    Assert.False(val2.HasValue);
            //}
        }


        [Fact]
        public void TestSignalSequence() {
            string input = "12.5+(34.7)+56.8+(78.9)+(91.2)+23.4+(0.09)";
            List<Sequence> sequences = [];

            string pattern = @"(\d+\.\d+)|\((\d+\.\d+)\)";

            Regex regex = new Regex(pattern);
            MatchCollection matches = regex.Matches(input);

            foreach (Match match in matches) {
                if (!string.IsNullOrEmpty(match.Groups[1].Value)) {
                    var duration = decimal.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
                    sequences.Add(new Sequence(duration, 1));
                }
                else if (!string.IsNullOrEmpty(match.Groups[2].Value)) {
                    decimal duration = decimal.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture);
                    sequences.Add(new Sequence(duration, 2));
                }
            }

            Assert.True(sequences[0].Duration == 12.5m, "Duration");
            Assert.True(sequences[0].Status == 1, "Status");
            Assert.True(sequences[1].Duration == 34.7m, "Duration");
            Assert.True(sequences[1].Status == 2, "Status");
            Assert.True(sequences[2].Duration == 56.8m, "Duration");
            Assert.True(sequences[2].Status == 1, "Status");
            Assert.True(sequences[3].Duration == 78.9m, "Duration");
            Assert.True(sequences[3].Status == 2, "Status");
            Assert.True(sequences[4].Duration == 91.2m, "Duration");
            Assert.True(sequences[4].Status == 2, "Status");
            Assert.True(sequences[5].Duration == 23.4m, "Duration");
            Assert.True(sequences[5].Status == 1, "Status");
            Assert.True(sequences[6].Duration == 0.09m, "Duration");
            Assert.True(sequences[6].Status == 2, "Status");
        }

        [Fact]
        public void NoteLoaderTest() {
            var notesPath = @"G:\indigo\ENC\NotesAndPictures";

            foreach (var notePath in Directory.GetFiles(notesPath, "*.txt", SearchOption.AllDirectories)) {
                //var note = new Note(notePath);
                //Assert.True(string.IsNullOrEmpty(note.Header));
                //Assert.True(!string.IsNullOrEmpty(note.Content));

            }
        }

        [Fact]
        public void GenerateSubtypes() {
            var sourcePath = @$"{Environment.GetEnvironmentVariable("OneDrive")}\ArcGIS\Projects\Vortex\replica.gdb";
            var source = new Geodatabase(new FileGeodatabaseConnectionPath(new Uri(IO.Path.GetFullPath(sourcePath))));

            StringBuilder csSubtypes = new StringBuilder();

            var featureClass = source.OpenDataset<FeatureClass>("MetadataP");
            string shapeType = "Point"; // Area | Point | Line

            var subtypes = featureClass.GetDefinition().GetSubtypes();

            var sortedDict = new SortedDictionary<int, string>();

            foreach (var subtype in subtypes) {
                sortedDict.Add(subtype.GetCode(), subtype.GetName());
            }

            foreach (var keyValuePair in sortedDict) {
                csSubtypes.AppendLine($"\t\tcase {keyValuePair.Key}: {{ // {keyValuePair.Value}");

                csSubtypes.AppendLine($"\t\tvar instance = new XXX(){{");
                csSubtypes.AppendLine($"\t\t}};");

                csSubtypes.AppendLine($"\t\t\tif (current.PLTS_COMP_SCALE.HasValue && current.SHAPE != null) {{");
                csSubtypes.AppendLine($"\t\t\tstring subtype = \"\";");

                csSubtypes.AppendLine($"\t\t\tif (current.TableName != default && current.FCSUBTYPE.HasValue && !Subtypes.Instance.TryGetSubtype(current.TableName, current.FCSUBTYPE.Value, out subtype))");
                csSubtypes.AppendLine($"\t\t\tthrow new NotSupportedException($\"Unknown subtype for {{current.TableName}}, {{current.FCSUBTYPE.Value}}\");");

                csSubtypes.AppendLine($"\t\t\tinstance.scaleMinimum = Scamin.Instance.GetMinimumScale(current, subtype, current.PLTS_COMP_SCALE.Value, isRelatedToStructure: false);");
                csSubtypes.AppendLine($"\t\t\t}}");

                csSubtypes.AppendLine($"\t\t\tif (plts_comp_scale != default) {{");
                csSubtypes.AppendLine($"\t\t\t\t\t//instance.scaleMinimum = plts_comp_scale;");
                csSubtypes.AppendLine($"\t\t\t}}");
                csSubtypes.AppendLine($"");
                //csSubtypes.AppendLine($"\t\t\tAddCondition(instance.condition, feature);");
                //csSubtypes.AppendLine($"\t\t\tAddStatus(instance.status, feature);");
                csSubtypes.AppendLine($"\t\t\tinstance.featureName = GetFeatureName(current.OBJNAM, current.NOBJNM);");
                csSubtypes.AppendLine($"\t\t\tAddInformation(instance.information, feature);");
                csSubtypes.AppendLine($"\t\t\tbuffer[\"ps\"] = ps101;");
                csSubtypes.AppendLine($"\t\t\tbuffer[\"code\"] = instance.GetType().Name;");
                csSubtypes.AppendLine($"\t\t\tbuffer[\"json\"] = System.Text.Json.JsonSerializer.Serialize(instance);");

                //csSubtypes.AppendLine($"\t\t\tbuffer[\"shape\"] = current.SHAPE;");
                //csSubtypes.AppendLine($"\t\t\tinsert.Insert(buffer);");
                //csSubtypes.AppendLine($"\t\t\tLogger.Current.DataObject(objectid, tableName, longname, System.Text.Json.JsonSerializer.Serialize(instance));");
                csSubtypes.AppendLine($"\t\t\tSetShape(buffer, current.SHAPE);");

                csSubtypes.AppendLine($"\t\t\tvar featureN = featureClass.CreateRow(buffer);");
                csSubtypes.AppendLine($"\t\t\tvar name = Convert.ToString(featureN[\"name\"]) ?? \"Unknown name\";");

                csSubtypes.AppendLine($"\t\t\tif (FeatureRelations.Instance.HasRelated(current.GLOBALID)) {{");
                csSubtypes.AppendLine($"\t\t\t\trelatedEquipment?.CreateRelated{shapeType}Equipment(current, instance, name, target, source);");
                csSubtypes.AppendLine($"\t\t\t}}");

                csSubtypes.AppendLine($"\t\t\tConversionAnalytics.Instance.AddConverted(tableName, current.GLOBALID, name); ");

                csSubtypes.AppendLine($"\t\t\tLogger.Current.DataObject(objectid, tableName, longname, System.Text.Json.JsonSerializer.Serialize(instance)); ");

                //csSubtypes.AppendLine($"\t\t\tconvertedCount++;");


                csSubtypes.AppendLine($"\t\t}}");
                csSubtypes.AppendLine($"\t\tbreak;");
            }




            csSubtypes.AppendLine($"\t\tdefault:");
            csSubtypes.AppendLine($"\t\t\t// code block");
            csSubtypes.AppendLine($"\t\t\tSystem.Diagnostics.Debugger.Break();");
            csSubtypes.AppendLine($"\t\tbreak;");

            Console.WriteLine(csSubtypes.ToString());
        }

        [Fact]
        public void ListDomainValues() {
            var sourcePath = @$"{Environment.GetEnvironmentVariable("OneDrive")}\ArcGIS\Projects\Vortex\replica.gdb";
            var source = new Geodatabase(new FileGeodatabaseConnectionPath(new Uri(IO.Path.GetFullPath(sourcePath))));

            StringBuilder csDomainValues = new StringBuilder();

            var featureClass = source.OpenDataset<FeatureClass>("CoastlineL");
            var fieldName = "CATSLC";

            var field = featureClass.GetDefinition().GetFields().FirstOrDefault<Field>(e => e.Name.ToLower() == fieldName.ToLower());

            var sortedDict = (field.GetDomain(null) as CodedValueDomain).GetCodedValuePairs();
            csDomainValues.AppendLine($"/*");
            csDomainValues.AppendLine($"{field.GetDomain(null).GetName()}");
            foreach (var keyValuePair in sortedDict) {
                csDomainValues.AppendLine($"\t\t\t{keyValuePair.Key}: {keyValuePair.Value}");

            }
            csDomainValues.AppendLine($"*/");

            Console.WriteLine(csDomainValues.ToString());
        }

        [Fact]
        public void CreateS57Domains() {

        }

        [Fact]
        public void GenerateStatusPage() {
            var featureclasses = new List<string> { "PLTS_SpatialAttributeL",
                                            "TidesAndVariationsA",
                                            "TidesAndVariationsL",
                                            "TidesAndVariationsP",
                                            "SeabedL",
                                            "SeabedP",
                                            "SeabedA",
                                            "DangersL",
                                            "DangersP",
                                            "DangersA",
                                            "DepthsL",
                                            "OffshoreInstallationsL",
                                            "OffshoreInstallationsA",
                                            "MetaDataP",
                                            "TracksAndRoutesA",
                                            "TracksAndRoutesL",
                                            "TracksAndRoutesP",
                                            "AidsToNavigationP",
                                            "IceFeaturesA",
                                            "MilitaryFeaturesA",
                                            "MilitaryFeaturesP",
                                            "UserDefinedFeaturesA",
                                            "UserDefinedFeaturesP",
                                            "UserDefinedFeaturesL",
                                            "DepthsA",
                                            "SoundingsP",
                                            "PortsAndServicesP",
                                            "PortsAndServicesL",
                                            "PortsAndServicesA",
                                            "CulturalFeaturesA",
                                            "CulturalFeaturesL",
                                            "CulturalFeaturesP",
                                            "NaturalFeaturesP",
                                            "NaturalFeaturesL",
                                            "NaturalFeaturesA",
                                            "CoastlineL",
                                            "CoastlineP",
                                            "CoastlineA",
                                            "RegulatedAreasAndLimitsL",
                                            "RegulatedAreasAndLimitsP",
                                            "RegulatedAreasAndLimitsA",
                                            "MetaDataA",
                                            "MetaDataL",
                                            "OffshoreInstallationsP",
                                            "ClosingLinesL",
                                            "ProductCoverage",
                                            //"ProductRestrictions"
            };
            var tables = new List<string> { //"ProductExports",
                                            "ProductDefinitions",
                                            "PLTS_Collections",
                                            "PLTS_Frel",
                                            "PLTS_Master_Slaves"
                                          };

            featureclasses.Sort();

            //var sourcePath = @$"{Environment.GetEnvironmentVariable("OneDrive")}\ArcGIS\Projects\Vortex\replica.gdb";
            //var source = new Geodatabase(new FileGeodatabaseConnectionPath(new Uri(IO.Path.GetFullPath(sourcePath))));
            var sourcePath = IO.Path.GetFullPath(IO.Path.Combine(@"G:\indigo\Databases\nis.sde"));
            var source = new Geodatabase(new DatabaseConnectionFile(new Uri(IO.Path.GetFullPath(sourcePath))));

            var prefix = "NIS.";

            string filePath = IO.Path.GetFullPath(IO.Path.Combine(@".\..\..\..\..\..\..\status_tt.txt"));

            StringBuilder content = new StringBuilder();

            List<Dataset> datasets = [];
            foreach (var featureclass in featureclasses) {
                datasets.Add(source.OpenDataset<FeatureClass>($"{prefix}{featureclass}"));
            }

            int counter = 0;
            using (StreamWriter file = new StreamWriter(filePath)) {
                foreach (var dataset in datasets) {
                    if (dataset is FeatureClass) {
                        var featureclass = (FeatureClass)dataset;
                        var subtypes = featureclass.GetDefinition().GetSubtypes();
                        var fields = featureclass.GetDefinition().GetFields();
                        var fieldHasData = new Dictionary<string, bool>();
                        var fieldAlias = new Dictionary<string, string>();

                        foreach (var field in fields) {
                            fieldHasData[field.Name] = false;
                            fieldAlias[field.Name] = field.AliasName;
                        }

                        var sortedDict = new SortedDictionary<int, string>();

                        var searchCursor = (dataset as FeatureClass)?.Search(new QueryFilter() { WhereClause = "1=1" });
                        if (searchCursor == null) {
                            throw new NotSupportedException("dataset is not a featureclass");
                        }

                        var subtypeCount = new Dictionary<int, int>();

                        int totalCount = 0;
                        while (searchCursor.MoveNext()) {
                            totalCount++;
                            var current = searchCursor.Current;

                            if (current.FindField("fcsubtype") == -1)
                                continue;

                            foreach (var fieldName in fieldHasData.Keys) {
                                if (DBNull.Value != current[fieldName] && current[fieldName] != null && !string.IsNullOrEmpty(current[fieldName].ToString())) {
                                    fieldHasData[fieldName] = true;
                                }
                            }

                            var subtypeValue = current["fcsubtype"];
                            if (subtypeValue != DBNull.Value) {
                                int subtype = Convert.ToInt32(subtypeValue);
                                if (subtypeCount.ContainsKey(subtype)) {
                                    subtypeCount[subtype] += 1;
                                }
                                else {
                                    subtypeCount[subtype] = 1;
                                }
                            }
                        }

                        foreach (var subtype in subtypes) {
                            sortedDict.Add(subtype.GetCode(), subtype.GetName());
                        }

                        foreach (var keyValuePair in sortedDict) {
                            counter += 1;

                            subtypeCount.TryGetValue(keyValuePair.Key, out var subtypeCountN);

                            content.AppendLine($"{counter};SUBTYPE;{dataset.GetName()};{keyValuePair.Value};{keyValuePair.Key};{subtypeCountN}");
                        }

                        foreach (var fieldName in fieldHasData.Keys) {
                            counter += 1;
                            var hasDataTag = fieldHasData[fieldName] ? "CONTAINS DATA" : "EMPTY";
                            content.AppendLine($"{counter};FIELD;{dataset.GetName()};{fieldName};{fieldAlias[fieldName]};{hasDataTag}");

                        }
                    }
                }
                file.WriteLine(content.ToString());
            }
        }

        [Fact]
        public void TestRelation() {
            //var relation1 = new Relation(new(typeof(SpecialPurposeGeneralBeacon), "S1"), new(typeof(LightAirObstruction), "S2"));
            //var relation2 = new Relation(new(typeof(SpecialPurposeGeneralBeacon), "S1"), new(typeof(LightAirObstruction), "S2"));

            //var relations = new HashSet<Relation>();

            //Assert.True(relation1.Equals(relation2));

        }



        [Fact]
        public void GenerateNisModel() {
            var featureclasses = new List<string> { "nis.PLTS_SpatialAttributeL",
                                            "nis.TidesAndVariationsA",
                                            "nis.TidesAndVariationsL",
                                            "nis.TidesAndVariationsP",
                                            "nis.SeabedL",
                                            "nis.SeabedP",
                                            "nis.SeabedA",
                                            "nis.DangersL",
                                            "nis.DangersP",
                                            "nis.DangersA",
                                            "nis.DepthsL",
                                            "nis.OffshoreInstallationsL",
                                            "nis.OffshoreInstallationsA",
                                            "nis.MetaDataP",
                                            "nis.TracksAndRoutesA",
                                            "nis.TracksAndRoutesL",
                                            "nis.TracksAndRoutesP",
                                            "nis.AidsToNavigationP",
                                            "nis.IceFeaturesA",
                                            "nis.MilitaryFeaturesA",
                                            "nis.MilitaryFeaturesP",
                                            "nis.UserDefinedFeaturesA",
                                            "nis.UserDefinedFeaturesP",
                                            "nis.UserDefinedFeaturesL",
                                            "nis.DepthsA",
                                            "nis.SoundingsP",
                                            "nis.PortsAndServicesP",
                                            "nis.PortsAndServicesL",
                                            "nis.PortsAndServicesA",
                                            "nis.CulturalFeaturesA",
                                            "nis.CulturalFeaturesL",
                                            "nis.CulturalFeaturesP",
                                            "nis.NaturalFeaturesP",
                                            "nis.NaturalFeaturesL",
                                            "nis.NaturalFeaturesA",
                                            "nis.CoastlineL",
                                            "nis.CoastlineP",
                                            "nis.CoastlineA",
                                            "nis.RegulatedAreasAndLimitsL",
                                            "nis.RegulatedAreasAndLimitsP",
                                            "nis.RegulatedAreasAndLimitsA",
                                            "nis.MetaDataA",
                                            "nis.MetaDataL",
                                            "nis.OffshoreInstallationsP",
                                            "nis.ClosingLinesL",
                                            "nis.ProductCoverage",
                                            //"ProductRestrictions"
            };
            var tables = new List<string> { //"ProductExports",
                                            "nis.ProductDefinitions",
                                            "nis.PLTS_Collections",
                                            "nis.PLTS_Frel",
                                            "nis.PLTS_Master_Slaves"
                                          };

            //var sourcePath = @$"{Environment.GetEnvironmentVariable("OneDrive")}\ArcGIS\Projects\Vortex\replica.gdb";
            //var source = new Geodatabase(new FileGeodatabaseConnectionPath(new Uri(IO.Path.GetFullPath(sourcePath))));

            string filePath = IO.Path.GetFullPath(IO.Path.Combine(@".\..\..\..\..\..\src\ImporterNIS\S-57.esri\S57EsriAuto.cs"));
            var sourcePath = IO.Path.GetFullPath(IO.Path.Combine(@"\\nas.gst.dk\ncps\administrators\NIS\SQLServer-ncps-sql100824-nis_OS.sde"));
            var source = new Geodatabase(new DatabaseConnectionFile(new Uri(IO.Path.GetFullPath(sourcePath))));


            StringBuilder csFile = new StringBuilder();

            List<Dataset> datasets = [];
            foreach (var featureclass in featureclasses) {
                datasets.Add(source.OpenDataset<FeatureClass>(featureclass));
            }
            foreach (var table in tables) {
                datasets.Add(source.OpenDataset<Table>(table));
            }

            using (StreamWriter file = new StreamWriter(filePath)) {
                csFile.AppendLine("/* THIS FILE IS AUTO GENERATED BY UNIT TEST GenerateNisModel */");
                csFile.AppendLine("/* Run test. GenerateNisModel and copy contents from the output file and change the namespace once compiling. */");
                csFile.AppendLine("/* If error in auto generated file just clear it's contents and run again. */");
                csFile.AppendLine("using ArcGIS.Core.Data;");
                csFile.AppendLine("using ArcGIS.Core.Geometry;");
                csFile.AppendLine("using System.ComponentModel;");
                csFile.AppendLine("namespace S100Framework.Applications.S57auto.esri");
                csFile.AppendLine("{");

                foreach (var dataset in datasets) {
                    var datasetName = dataset.GetName().Split('.')[1];
                    StringBuilder fields = new StringBuilder();
                    StringBuilder ctor = new StringBuilder();
                    StringBuilder objectClass = new StringBuilder();

                    objectClass.AppendLine($"\tinternal class {datasetName} : S100Framework.Applications.S57.esri.S57Object {{");


                    IReadOnlyList<ArcGIS.Core.Data.Field> datasetfields = [];

                    if (dataset is FeatureClass) {
                        datasetfields = ((FeatureClass)dataset).GetDefinition().GetFields();
                        ctor.AppendLine($"\t\tpublic {datasetName}(Feature feature) {{");
                    }
                    else if (dataset is Table) {
                        datasetfields = ((Table)dataset).GetDefinition().GetFields();
                        ctor.AppendLine($"\t\tpublic {datasetName}(Row row) {{");
                    }

                    ctor.AppendLine($"\t\t\tbase.TableName = \"{datasetName}\";");

                    var fieldInfo = (Type: "Int32", Conversion: "Convert.ToInt32", DefaultValue: "default", Alias: string.Empty);

                    foreach (var field in datasetfields) {
                        if (field.Name.ToUpper().StartsWith("SHAPE_")) {
                            //Console.WriteLine("");
                            continue;
                        }
                        else if (field.Name.ToUpper().StartsWith("SHAPE.STLENGTH")) {
                            //Console.WriteLine("");
                            continue;
                        }
                        else if (field.Name.ToUpper().StartsWith("SHAPE.STAREA")) {
                            //Console.WriteLine("");
                            continue;
                        }
                        else if (field.Name.ToUpper().StartsWith("GST_")) {
                            //Console.WriteLine("");
                            continue;
                        }


                        fieldInfo = field.FieldType switch {
                            (FieldType)esriFieldType.esriFieldTypeBigInteger => (Type: "internal long?", Conversion: "Convert.ToLong", Default: "default", Alias: field.AliasName),
                            (FieldType)esriFieldType.esriFieldTypeInteger => (Type: "internal int?", Conversion: "Convert.ToInt32", Default: "default", Alias: field.AliasName),
                            (FieldType)esriFieldType.esriFieldTypeString => (Type: "internal string?", Conversion: "Convert.ToString", Default: "default", Alias: field.AliasName),
                            (FieldType)esriFieldType.esriFieldTypeSmallInteger => (Type: "internal int?", Conversion: "Convert.ToInt32", Default: "default", Alias: field.AliasName),
                            (FieldType)esriFieldType.esriFieldTypeDouble => (Type: "internal decimal?", Conversion: "Convert.ToDecimal", Default: "default", Alias: field.AliasName),
                            //(FieldType)esriFieldType.esriFieldTypeDouble => (Type: "internal double?", Conversion: "Convert.ToDouble", Default: "default", Alias: field.AliasName),
                            (FieldType)esriFieldType.esriFieldTypeSingle => (Type: "internal int?", Conversion: "Convert.ToInt32", Default: "default", Alias: field.AliasName),
                            (FieldType)esriFieldType.esriFieldTypeDate => (Type: "internal DateTime?", Conversion: "Convert.ToDateTime", Default: "default", Alias: field.AliasName),
                            (FieldType)esriFieldType.esriFieldTypeGUID => (Type: "internal Guid", Conversion: "Guid.Parse", Default: "Guid.Empty", Alias: field.AliasName),
                            //(FieldType)esriFieldType.esriFieldTypeBlob => (S101Type: "byte[]", Conversion: "", Default: "new byte[fs.Length]", field.AliasName),
                            //(FieldType)esriFieldType.esriFieldTypeRaster => (S101Type: "Raster", Conversion: "", Default: "default", field.AliasName),
                            (FieldType)esriFieldType.esriFieldTypeOID => (Type: "internal int?", Conversion: "Convert.ToInt32", Default: "default", Alias: field.AliasName),
                            (FieldType)esriFieldType.esriFieldTypeGlobalID => (Type: "internal Guid", Conversion: "Guid.Parse", Default: "Guid.Empty", Alias: field.AliasName),
                            (FieldType)esriFieldType.esriFieldTypeGeometry => (Type: "internal Geometry?", Conversion: "(Geometry?)", Default: "default", Alias: field.AliasName),
                            _ => throw new IndexOutOfRangeException(),
                        };

                        var fieldValue = "";

                        if (dataset is FeatureClass) {
                            if (string.IsNullOrEmpty(fieldInfo.Conversion)) {
                                fieldValue = $@"feature[""{field.Name.ToUpper()}""];";
                            }
                            else {
                                fieldValue = $@"{fieldInfo.Conversion}(feature[""{field.Name.ToUpper()}""])";
                            }
                            if (fieldInfo.Type.ToLower().Contains("guid")) {
                                fieldValue = $@"Guid.TryParse(Convert.ToString(feature[""{field.Name.ToUpper()}""]), out {field.Name.ToUpper()})";
                            }
                        }
                        else if (dataset is Table) {
                            if (string.IsNullOrEmpty(fieldInfo.Conversion)) {
                                fieldValue = $@"row[""{field.Name.ToUpper()}""];";
                            }
                            else {
                                fieldValue = $@"{fieldInfo.Conversion}(row[""{field.Name.ToUpper()}""])";
                            }
                            if (fieldInfo.Type.ToLower().Contains("guid")) {
                                fieldValue = $@"Guid.TryParse(Convert.ToString(row[""{field.Name.ToUpper()}""]), out {field.Name.ToUpper()})";
                            }
                        }

                        fields.AppendLine($"");
                        fields.AppendLine($"\t\t/// <summary>");
                        fields.AppendLine($"\t\t/// {fieldInfo.Alias}");
                        fields.AppendLine($"\t\t/// </summary>");
                        fields.AppendLine($"\t\t[Description(\"{fieldInfo.Alias}\")]");
                        fields.AppendLine($"\t\t{fieldInfo.Type} {field.Name.ToUpper()} = {fieldInfo.DefaultValue};");


                        if (dataset is FeatureClass) {
                            if (field.Name.ToUpper() == "VALIDATIONSTATUS") {
                                ctor.AppendLine($"\t\t\tif (feature.FindField(\"VALIDATIONSTATUS\") > -1) {{ // NOAA Exception");
                                ctor.AppendLine($"\t\t\t\t\tif (DBNull.Value != feature[\"{field.Name.ToUpper()}\"] && feature[\"{field.Name.ToUpper()}\"] is not null) {{");
                            }
                            else {
                                ctor.AppendLine($"\t\t\tif (DBNull.Value != feature[\"{field.Name.ToUpper()}\"] && feature[\"{field.Name.ToUpper()}\"] is not null) {{");
                            }
                        }
                        else if (dataset is Table) {
                            ctor.AppendLine($"\t\t\tif (DBNull.Value != row[\"{field.Name.ToUpper()}\"] && row[\"{field.Name.ToUpper()}\"] is not null) {{");
                        }

                        if (fieldInfo.Type.ToLower().Contains("guid")) {
                            ctor.AppendLine($"\t\t\t\t{fieldValue};");
                            if (field.Name.ToUpper() == "GLOBALID") {
                                ctor.AppendLine($"\t\t\t\tbase.GlobalId = this.GLOBALID;");
                            }
                        }

                        else if (fieldInfo.Type.ToLower().Contains("string")) {
                            if (dataset is FeatureClass) {
                                var value1 = $@"var text = {fieldInfo.Conversion}(feature[""{field.Name.ToUpper()}""])";
                                var value2 = $@"this.{field.Name.ToUpper()} = string.IsNullOrEmpty(text) ? default : text";
                                ctor.AppendLine($"\t\t\t\t{value1};");
                                ctor.AppendLine($"\t\t\t\t{value2};");
                            }
                            else if (dataset is Table) {
                                var value1 = $@"var text = {fieldInfo.Conversion}(row[""{field.Name.ToUpper()}""])";
                                var value2 = $@"this.{field.Name.ToUpper()} = string.IsNullOrEmpty(text) ? default : text";
                                ctor.AppendLine($"\t\t\t\t{value1};");
                                ctor.AppendLine($"\t\t\t\t{value2};");
                            }
                        }

                        else {
                            ctor.AppendLine($"\t\t\t\t{field.Name.ToUpper()} = {fieldValue};");
                            if (field.Name.ToUpper() == "VALIDATIONSTATUS") {
                                ctor.AppendLine($"\t\t\t\t}}");
                            }
                            if (field.Name.ToUpper() == "SHAPE") {
                                ctor.AppendLine($"\t\t\t\tbase.Shape = this.SHAPE;");
                            }
                            if (field.Name.ToUpper() == "PLTS_COMP_SCALE") {
                                ctor.AppendLine($"\t\t\t\tbase.PLTS_COMP_SCALE = this.PLTS_COMP_SCALE.Value;");
                            }
                            if (field.Name.ToUpper() == "SCAMIN_STEP") {
                                ctor.AppendLine($"\t\t\t\tbase.SCAMIN_STEP = this.SCAMIN_STEP.Value;");
                            }
                            if (field.Name.ToUpper() == "FCSUBTYPE") {
                                ctor.AppendLine($"\t\t\t\tbase.FcSubtype = this.FCSUBTYPE.Value;");
                            }
                        }
                        ctor.AppendLine($"\t\t\t}}");
                    }

                    ctor.AppendLine("\t\t}");
                    ctor.AppendLine("\t}");

                    objectClass.Append(fields);
                    objectClass.Append(ctor);
                    csFile.Append(objectClass);

                    //csFile.Append(@"}");
                }
                csFile.AppendLine(@"}");
                file.WriteLine(csFile.ToString());
            }
        }

        [Fact]
        public void H2P() {
            var root = new IO.DirectoryInfo("");

            foreach (var survey in root.GetDirectories()) {
                var name = survey.Name;

                foreach (var gpkg in survey.GetFiles("*.gpkg")) {
                    this._output.WriteLine($"layer = m.addDataFromPath(r\"{IO.Path.GetDirectoryName(gpkg.FullName)}\\{gpkg.Name}\\main.dataset_polygon\")");
                    this._output.WriteLine($"layer.name = r\"{gpkg.Name.Split('.')[0]}\"");
                }
            }
        }

        [Fact]
        public void BuildImportS100CellScripts() {
            IO.DirectoryInfo[] roots = [new IO.DirectoryInfo(@"ENC")];

            roots = [new IO.DirectoryInfo(@"C:\Users\B061450\source\repos\Geodatastyrelsen\S-100\noaa\s100\converted")];

            var python = new StringBuilder();

            foreach (var root in roots) {
                foreach (var enc in root.EnumerateFiles("*.000")) {
                    python.Append($"arcpy.maritime.ImportS100Cell(" + Environment.NewLine +
                            $"    in_feature_catalogue=r\"c:\\Users\\Public\\Documents\\ArcGIS Maritime\\Product Files\\3.7\\S-101\\S-101_FC_2.0.0.xml\"," + Environment.NewLine +
                            $"    in_base_cell=r\"{enc.FullName}\"," + Environment.NewLine +
                             "    target_workspace=r\"geodatabase.gdb\"," + Environment.NewLine +
                             "    in_update_cells=None," + Environment.NewLine +
                             "    create_s57_product=\"CREATE_S57_PRODUCT\"" + Environment.NewLine +
                            ")" + Environment.NewLine);
                }
            }

            this._output.WriteLine(python.ToString());
        }

        [Fact]
        public void BuildExportGeodatabaseToS57Scripts() {
            IO.DirectoryInfo[] roots = [new IO.DirectoryInfo(@"ENC")];

            roots = [new IO.DirectoryInfo(@"c:\Users\B061450\source\repos\Geodatastyrelsen\S-100\noaa\s57")];

            var python = new StringBuilder();

            foreach (var root in roots) {
                foreach(var enc in root.EnumerateDirectories()) {
                    python.Append($"arcpy.maritime.ExportGeodatabaseToS57(" + Environment.NewLine +
                                 "    in_source_gdb=r\"C:\\Users\\B061450\\source\\repos\\Geodatastyrelsen\\S-100\\noaa\\s101.geodatabase\"," + Environment.NewLine +
                                $"    product=r\"{enc.Name}\"," + Environment.NewLine +
                                 "    export_type=\"NEW_EDITION\"," + Environment.NewLine +
                                $"    out_location=r\"C:\\Users\\B061450\\source\\repos\\Geodatastyrelsen\\S-100\\noaa\\s100\\s57\\{enc.Name}\"," + Environment.NewLine +
                                 "    in_product_config=None," + Environment.NewLine +
                                 "    clip_data_option=\"DO_NOT_CLIP\"," + Environment.NewLine +
                                 "    sample_export=\"OFFICIAL_EXPORT\"," + Environment.NewLine +
                                 "    in_scamin_file=None," + Environment.NewLine +
                                $"    in_feature_catalogue=r\"c:\\Users\\Public\\Documents\\ArcGIS Maritime\\Product Files\\3.7\\S-101\\S-101_FC_2.0.0.xml\"" + Environment.NewLine +
                                ")" + Environment.NewLine);
                }
            }

            this._output.WriteLine(python.ToString());
        }

        [Fact]
        public void BuildExportS101CellScripts() {
            IO.DirectoryInfo[] roots = [new IO.DirectoryInfo(@"ENC")];

            roots = [new IO.DirectoryInfo(@"c:\Users\B061450\source\repos\Geodatastyrelsen\S-100\noaa\s100\converted")];

            var python = new StringBuilder();

            foreach (var root in roots) {
                foreach (var enc in root.EnumerateFiles("*.000")) {
                    IO.Directory.CreateDirectory($@"C:\Users\B061450\source\repos\Geodatastyrelsen\S-100\noaa\s100\s101\{IO.Path.GetFileNameWithoutExtension(enc.Name)}");

                    python.Append($"arcpy.maritime.ExportS101Cell(" + Environment.NewLine +
                                $"    in_feature_catalogue=r\"c:\\Users\\Public\\Documents\\ArcGIS Maritime\\Product Files\\3.7\\S-101\\S-101_FC_2.0.0.xml\"," + Environment.NewLine +
                                 "    in_s101_workspace=r\"C:\\Users\\B061450\\source\\repos\\Geodatastyrelsen\\S-100\\noaa\\s101.geodatabase\"," + Environment.NewLine +
                                $"    product=r\"{IO.Path.GetFileNameWithoutExtension(enc.Name)}\"," + Environment.NewLine +
                                 "    export_type=\"NEW_EDITION\"," + Environment.NewLine +
                                $"    output_location=r\"C:\\Users\\B061450\\source\\repos\\Geodatastyrelsen\\S-100\\noaa\\s100\\s101\\{IO.Path.GetFileNameWithoutExtension(enc.Name)}\"," + Environment.NewLine +
                                 "    sample_export=\"OFFICIAL_EXPORT\"," + Environment.NewLine +
                                 "    suppress_exchange_set=\"SUPPRESS_EXCHANGE_SET\"" + Environment.NewLine +                                
                                ")" + Environment.NewLine);
                }
            }

            this._output.WriteLine(python.ToString());
        }

        [Fact]
        public void BuildImportS57ToGeodatabaseScripts() {
            IO.DirectoryInfo[] roots = [new IO.DirectoryInfo(@"ENC")];

            roots = [
                new IO.DirectoryInfo(@"\\nas.gst.dk\ncps\production\indigo\ENC\Approved\DK\DK1"),
                new IO.DirectoryInfo(@"\\nas.gst.dk\ncps\production\indigo\ENC\Approved\DK\DK2"),
                new IO.DirectoryInfo(@"\\nas.gst.dk\ncps\production\indigo\ENC\Approved\DK\DK3"),
                new IO.DirectoryInfo(@"\\nas.gst.dk\ncps\production\indigo\ENC\Approved\DK\DK4"),
                new IO.DirectoryInfo(@"\\nas.gst.dk\ncps\production\indigo\ENC\Approved\DK\DK5"),
                new IO.DirectoryInfo(@"\\nas.gst.dk\ncps\production\indigo\ENC\Approved\GL\GL1"),
                new IO.DirectoryInfo(@"\\nas.gst.dk\ncps\production\indigo\ENC\Approved\GL\GL2"),
                new IO.DirectoryInfo(@"\\nas.gst.dk\ncps\production\indigo\ENC\Approved\GL\GL3"),
                new IO.DirectoryInfo(@"\\nas.gst.dk\ncps\production\indigo\ENC\Approved\GL\GL4"),
                new IO.DirectoryInfo(@"\\nas.gst.dk\ncps\production\indigo\ENC\Approved\GL\GL5")];

            roots = [new IO.DirectoryInfo(@"l:\B061450\ArcGIS\s100ed16_balticsea\s57")];

            var python = new StringBuilder();


            string[] filters = ["US3NY1AE", "US4NJ1DE", "US4NJ1DF", "US4NJ1DG", "US4NJ1EE", "US4NJ1EF", "US4NJ1EG", "US4NJ1FE", "US4NJ1FF", "US4NJ1FG", "US4NJ1FH", "US4NY1AP", "US4NY1AQ", "US4NY1AR", "US4NY1AS", "US5NJ1KJ", "US5NJ1KK", "US5NJ1KL", "US5NJ1LK", "US5NJ1LL", "US5NJ1LM", "US5NJ1MK", "US5NJ1ML", "US5NJ1MM", "US5NJ1NK", "US5NJ1NL", "US5NJ1NM", "US5NJ1OK", "US5NJ1OL", "US5NJ1OM", "US5NJ1PL", "US5NJ1PM", "US5NJ1QL", "US5NJ1QM", "US5NJ1QN", "US5NJ1RM", "US5NJ1RN", "US5NJ1SM", "US5NJ1SN", "US5NJ1TM", "US5NJ1TN", "US5NJ1UM", "US5NJ1UN", "US5NJ1UO", "US5NJ1UP", "US5NJ1UQ", "US5NY1AH", "US5NY1AI", "US5NY1AJ", "US5NY1AK", "US5NY1AL", "US5NY1BE", "US5NY1BF", "US5NY1BG", "US5NY1BH", "US5NY1BI", "US5NY1BJ", "US5NY1BK", "US5NY1BL", "US5NY1CE", "US5NY1CF", "US5NY1CI", "US5NY1CJ", "US5NY1CK", "US5NY1CL", "US5NY1DE", "US5NY1DF", "US5NY1DG", "US5NY1DH", "US5NY9AM", "US5NY9AN", "US5NY9BM", "US5NY9BN", "US5NY9CM", "US5NY9CN", "US5NY9DN", "US5NYCAB", "US5NYCAC", "US5NYCAD", "US5NYCAE", "US5NYCAF", "US5NYCAG", "US5NYCAH", "US5NYCAI", "US5NYCAJ", "US5NYCBB", "US5NYCBC", "US5NYCBD", "US5NYCBE", "US5NYCBF", "US5NYCBG", "US5NYCBH", "US5NYCBI", "US5NYCBJ", "US5NYCCD", "US5NYCCE", "US5NYCCF", "US5NYCCG", "US5NYCDD", "US5NYCDE", "US5NYCDF", "US5NYCDG", "US5NYCEE", "US5NYCEF", "US5NYCEG", "US3SC1CB", "US4SC1BN", "US4SC1BO", "US4SC1BP", "US4SC1BQ", "US4SC1CN", "US4SC1CO", "US4SC1CP", "US4SC1CQ", "US4SC1DP", "US4SC1DQ", "US5CHSBH", "US5CHSBI", "US5CHSCE", "US5CHSCF", "US5CHSCG", "US5CHSCH", "US5CHSDC", "US5CHSDD", "US5CHSDE", "US5CHSDF", "US5CHSEB", "US5CHSEC", "US5CHSED", "US5CHSEE", "US5CHSFC", "US5CHSFD", "US5CHSFE", "US5SC1FI", "US5SC1FL", "US5SC1GI", "US5SC1GJ", "US5SC1GK", "US5SC1GL", "US5SC1HI", "US5SC1HJ", "US5SC1HK", "US5SC1IK", "US5SC1IL", "US5SC1IM", "US5SC1JL", "US5SC1JQ", "US5SC1KQ", "US5SC1KR", "US5SC1LN", "US5SC1LO", "US5SC1LP", "US5SC1LQ", "US5SC1LR", "US5SC1LS", "US5SC1MN", "US5SC1MO", "US5SC1MP", "US5SC1MS", "US5SC1MT", "US5SC1MU", "US5SC1NN", "US5SC1NO", "US5SC1NP", "US5SC1NU", "US5SC1NV", "US5SC1NW", "US5SC1OV", "US5SC1OW", "US5SC1OX", "US5SC1PW", "US5SC1PX", "US5SC1QW", "US5SC1QX", "US5SC2AB", "US5SC2BB", "US3CA1DI", "US5CA1NR", "US5CA1NS", "US5CA59M", "US5CA59M", "US5CA59M", "US5CA59M", "US5CA61M", "US5CA61M", "US5CA61M", "US5CA62M", "US5CA62M", "US5CA63M", "US5CA63M", "US5CA63M", "US5LGBBB", "US5LGBBC", "US5LGBBD", "US5LGBBE", "US5LGBBF", "US5LGBCB", "US5LGBCC", "US5LGBCD", "US5LGBCE", "US5LGBCF", "US5LGBDC", "US5LGBDD", "US5LGBDE", "US5LGBDF"];

            //string[] filters_LosAngeles = ["US3CA1DI","US5CA1NR","US5CA1NS","US5CA59M","US5CA59M","US5CA59M","US5CA59M","US5CA59M","US5CA59M","US5CA59M","US5CA59M","US5CA59M","US5CA59M","US5CA59M","US5CA59M","US5CA59M","US5CA59M","US5CA59M","US5CA59M","US5CA61M","US5CA61M","US5CA61M","US5CA61M","US5CA61M","US5CA61M","US5CA61M","US5CA61M","US5CA61M","US5CA62M","US5CA62M","US5CA62M","US5CA62M","US5CA63M","US5CA63M","US5CA63M","US5CA63M","US5CA63M","US5CA63M","US5CA63M","US5CA63M","US5CA63M","US5CA63M","US5CA63M","US5CA63M","US5CA63M","US5CA63M","US5CA63M","US5CA63M","US5CA63M","US5CA63M","US5CA63M","US5CA63M","US5CA63M","US5CA63M","US5CA63M","US5CA63M","US5CA63M","US5CA83M","US5CA83M","US5CA83M","US5CA83M","US5LGBBB","US5LGBBC","US5LGBBD","US5LGBBE","US5LGBBF","US5LGBCB","US5LGBCC","US5LGBCD","US5LGBCE","US5LGBCF","US5LGBDC","US5LGBDD","US5LGBDE","US5LGBDF","US3CA1DI","US5CA1NR","US5CA1NS","US5CA59M","US5CA59M","US5CA59M","US5CA59M","US5CA59M","US5CA59M","US5CA59M","US5CA59M","US5CA59M","US5CA59M","US5CA59M","US5CA59M","US5CA59M","US5CA59M","US5CA59M","US5CA59M","US5CA61M","US5CA61M","US5CA61M","US5CA61M","US5CA61M","US5CA61M","US5CA61M","US5CA61M","US5CA61M","US5CA62M","US5CA62M","US5CA62M","US5CA62M","US5CA63M","US5CA63M","US5CA63M","US5CA63M","US5CA63M","US5CA63M","US5CA63M","US5CA63M","US5CA63M","US5CA63M","US5CA63M","US5CA63M","US5CA63M","US5CA63M","US5CA63M","US5CA63M","US5CA63M","US5CA63M","US5CA63M","US5CA63M","US5CA63M","US5CA63M","US5CA63M","US5CA63M","US5CA63M","US5CA83M","US5CA83M","US5CA83M","US5CA83M","US5LGBBB","US5LGBBC","US5LGBBD","US5LGBBE","US5LGBBF","US5LGBCB","US5LGBCC","US5LGBCD","US5LGBCE","US5LGBCF","US5LGBDC","US5LGBDD","US5LGBDE","US5LGBDF"];

            //filters = filters_LosAngeles.Distinct().ToArray();          

            filters = [""];

            foreach (var root in roots) {
                foreach (var filter in filters.Distinct()) {
                    foreach (var enc in root.EnumerateDirectories()) {
                        if (enc.Name.Contains("cancel", StringComparison.InvariantCultureIgnoreCase)) continue;
                        if (!string.IsNullOrEmpty(filter) && !enc.Name.Contains(filter)) continue;

                        var command = ImportS57ToGeodatabase(enc, "geodatabase.gdb", (e) => true, true);

                        python.AppendLine(command);
                    }
                }
            }

            this._output.WriteLine(python.ToString());
        }

        private static string ImportS57ToGeodatabase(DirectoryInfo folder, string connection, Func<string, bool> filter, bool updates) {
            var tasks = new List<string>();

            var regex = new Regex(@"\d{3}$");

            if (!folder.GetFiles("*.000", SearchOption.TopDirectoryOnly).Any()) {
                folder = folder.GetDirectories().OrderByDescending(e => e.Name).First();
            }

            foreach (var file in folder.GetFiles("*.000").OrderBy(e => IO.Path.GetFileNameWithoutExtension(e.FullName))) {
                var name = IO.Path.GetFileNameWithoutExtension(file.FullName);

                if (!filter.Invoke(name))
                    continue;


                var _updates = updates ? folder.GetFiles("*.*", SearchOption.TopDirectoryOnly).Where(e => !e.Extension.Equals(".000") && !e.Extension.Equals(".031") && regex.IsMatch(e.Name)).ToList() : [];


                tasks.Add($"arcpy.maritime.ImportS57ToGeodatabase(" + Environment.NewLine +
                $"    in_base_cell = r\"{file.FullName}\"," + Environment.NewLine +
                $"    target_workspace=r\"{connection}\"," + Environment.NewLine +
                $"    in_update_cells=r\"{string.Join(';', _updates)}\"," + Environment.NewLine +
                 "    in_product_config=None" + Environment.NewLine +
                ")" + Environment.NewLine);
            }

            var commands = string.Join(Environment.NewLine, tasks);

            return commands;
        }

    }
}
