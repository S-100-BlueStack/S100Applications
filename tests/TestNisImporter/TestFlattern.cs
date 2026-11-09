using JsonFlatten;
using Newtonsoft.Json.Linq;
using S100FC;
using S100FC.S101.FeatureAssociation;
using S100FC.S101.FeatureTypes;
using S100FC.S101.InformationAssociation;
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

        [Fact]
        public void Test_FlattenInformationBindings() {
            var informations = new informationBinding[] {
                new informationBinding {
                    association = new QualityOfBathymetricDataComposition(),
                    roleType = "association",
                    role = "theQualityInformation",
                    informationType="SpatialQuality",
                    informationId="{C7C08676-21A0-486C-9054-733727D12C75}",
                },
                new informationBinding {
                    association = new QualityOfBathymetricDataComposition(),
                    roleType = "association",
                    role = "theQualityInformation",
                    informationType="SpatialQuality",
                    informationId="{4ED71F0C-8A01-4566-B9F1-06ADF7186FAC}",
                },
            };

            var flatten = informations.Flatten();

            var instance = AttributeFlattenExtensions.Unflatten<informationBinding[]>(flatten, typeof(informationBinding[]));

            System.Diagnostics.Debugger.Break();
        }

        [Fact]
        public void Test_FlattenFeatureBindings() {
            var features = new featureBinding[] {
                new featureBinding {
                    association = new UpdatedInformation(),
                    roleType = "association",
                    role = "theUpdate",
                    featureType="UpdateInformation",
                    featureId="{C7C08676-21A0-486C-9054-733727D12C75}",
                    Primitive = Primitives.point,
                },
                new featureBinding {
                    association = new UpdatedInformation(),
                    roleType = "association",
                    role = "theUpdate",
                    featureType="UpdateInformation",
                    featureId="{4ED71F0C-8A01-4566-B9F1-06ADF7186FAC}",
                    Primitive = Primitives.surface,
                },               
            };

            var flatten = features.Flatten();

            var instance = AttributeFlattenExtensions.Unflatten<featureBinding[]>(flatten, typeof(featureBinding[]));

            System.Diagnostics.Debugger.Break();
        }

    }
}
