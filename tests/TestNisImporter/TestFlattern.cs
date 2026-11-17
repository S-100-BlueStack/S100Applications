using JsonFlatten;
using Newtonsoft.Json.Linq;
using S100FC;
using S100FC.S101.FeatureAssociation;
using S100FC.S101.FeatureTypes;
using S100FC.S101.InformationAssociation;
using S100FC.S128.FeatureAssociation;
using S100FC.S128.FeatureTypes;
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
            //ArcGIS.Core.Hosting.Host.Initialize();
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

            var instance = AttributeFlattenExtensions.UnflattenInformationBindings(flatten);

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

            var instance = AttributeFlattenExtensions.UnflattenFeatureBindings(flatten);

            System.Diagnostics.Debugger.Break();
        }

        [Fact]
        public void Test_Json() {
            var jsonFeatureBindings = "[{\"association\":{\"code\":\"ProductMapping\",\"attributes\":[{\"code\":\"categoryOfProductMapping\",\"value\":2}]},\"roleType\":\"association\",\"role\":\"theReference\",\"featureType\":\"ElectronicProduct\",\"featureId\":\"{12eb50d8-7027-47e8-a46f-118729157ad4}\",\"Primitive\":\"surface\"}]";

            var structuredObject = System.Text.Json.JsonSerializer.Deserialize<featureBinding[]>(jsonFeatureBindings);

        }

        [Fact]
        public void Test_FlattenElectronicProduct() {
            var electronicProduct = new ElectronicProduct();
            electronicProduct.classification = "My Classification";
            
            var flatten1 = electronicProduct.Flatten();
            var instance1 = AttributeFlattenExtensions.UnflattenFeatureType<ElectronicProduct>(flatten1);



            var productMapping = new ProductMapping();
            productMapping.categoryOfProductMapping = 1;

            var features = new featureBinding[] {
                new featureBinding {
                    association = productMapping,
                    roleType = "association",
                    role = "theReference",
                    featureType="ElectronicProduct",
                    featureId="{C7C08676-21A0-486C-9054-733727D12C75}",
                    Primitive = Primitives.surface,
                }
            };

            var flatten2 = features.Flatten();
            var instance2 = AttributeFlattenExtensions.UnflattenFeatureBindings(flatten2);

            System.Diagnostics.Debugger.Break();
        }

        [Fact]
        public void Test_MandatoryEnumerations() {
            var cardinalBeacon = new CardinalBeacon();

            var flatten = cardinalBeacon.Flatten();
            
            var instance1 = AttributeFlattenExtensions.UnflattenFeatureType<CardinalBeacon>(flatten);

            var instance2 = AttributeFlattenExtensions.UnflattenFeatureType(flatten, typeof(CardinalBeacon));

            var structuredObject = JsonUnflattener.Unflatten(flatten)!;

            var properties = JsonUnflattener.GetAllProperties(structuredObject).ToArray();

            System.Diagnostics.Debugger.Break();
        }
    }
}
