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
    }
}
