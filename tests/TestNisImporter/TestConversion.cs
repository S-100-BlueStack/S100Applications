using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using NetTopologySuite.Algorithm;
using S100Framework.Applications;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using IO = System.IO;


namespace TestNisImporter
{
    public class TestConversion
    {
        private readonly ITestOutputHelper _output;

        public TestConversion(ITestOutputHelper output) {
            this._output = output;
            ArcGIS.Core.Hosting.Host.Initialize();
        }

        [Fact]
        public void Test_INFORM() {
            var fullpath = IO.Path.GetFullPath(Environment.GetEnvironmentVariable("NIS")!);
            using var instance = new Geodatabase(new DatabaseConnectionFile(new Uri(IO.Path.GetFullPath(fullpath))));
        }

        [Fact]
        public void Test_DateConversion() {
            string[] dates = [
                ];

            var fullpath = IO.Path.GetFullPath(Environment.GetEnvironmentVariable("NIS")!);
            using var instance = new Geodatabase(new DatabaseConnectionFile(new Uri(IO.Path.GetFullPath(fullpath))));

            foreach (var def in instance.GetDefinitions<FeatureClassDefinition>()) {
                string[] fields = ["DATSTA", "DATEND", "PERSTA", "PEREND"];

                var names = def.GetFields().Select(e => e.Name).Where(e => fields.Contains(e.ToUpperInvariant()));

                if (!names.Any()) continue;

                using (var fc = instance.OpenDataset<FeatureClass>(def.GetName())) {
                    foreach (var name in names) {
                        using (var search = fc.Search(new QueryFilter {
                            WhereClause = "1=1",
                            SubFields = $"{name}",
                            PrefixClause = "Distinct"
                        })) {
                            while (search.MoveNext()) {
                                var value = Convert.ToString(search.Current[name]);

                                if (!string.IsNullOrEmpty(value) && string.IsNullOrWhiteSpace(value))
                                    this._output.WriteLine($"{def.GetName()}::{name} is empty");

                                if (string.IsNullOrEmpty(value)) continue;

                                dates = [.. dates, value];
                            }
                        }
                    }
                }
            }
            dates = [.. dates.Distinct()];

            foreach (var date in dates) {
                var _ = date.Length<8 ? $"--{date}" : date;
                var yyyy = string.Join("", _!.Skip(0).Take(4));
                var mm = string.Join("", _!.Skip(4).Take(2));
                var dd = string.Join("", _!.Skip(6).Take(2));


                var success = S100Framework.ImporterNIS.Core.S57DateParser.TryParsePeriodicDate(date, out var text);

                if (!success)
                    this._output.WriteLine($"{date}: {text}");

            }
        }
    }
}
