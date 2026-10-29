using JsonFlatten;
using Newtonsoft.Json.Linq;
using S100FC.S101.FeatureTypes;
using System;
using System.Collections.Generic;
using System.Text;

namespace TestNisImporter
{
    public class TestFlattern
    {
        private readonly ITestOutputHelper _output;

        public TestFlattern(ITestOutputHelper output) {
            this._output = output;
            ArcGIS.Core.Hosting.Host.Initialize();
        }

        [Fact]
        public void Test_Dmapper() {
            var instance = new Bridge();

            instance.featureName = [
                    new S100FC.S101.ComplexAttributes.featureName{
                        name = "Hello World 1",
                        language = "eng",
                        nameUsage = 1
                    },
                    new S100FC.S101.ComplexAttributes.featureName{
                        name = "Hello World 2",
                        language = "den",
                        nameUsage = 2
                    },
                ];

            instance.categoryOfOpeningBridge = 3;

            instance.radarConspicuous = false;

            instance.natureOfConstruction = [1, 2];

            instance.colour = [7, 9];
            instance.colourPattern = 2;

            JObject json = JObject.FromObject(instance.attributeBindings);

            IDictionary<string, object> flattened = json.Flatten();

            System.Diagnostics.Debugger.Break();

        }
    }
}
