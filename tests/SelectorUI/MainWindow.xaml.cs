using PropertyGridApplication;
using S100FC;
using S100FC.S101;
using S100FC.S101.ComplexAttributes;
using S100FC.S101.FeatureAssociation;
using S100FC.S101.FeatureTypes;
using S100FC.S101.InformationAssociation;
using S100Framework.WPF.ViewModel;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Windows;
using System.Xml.Linq;

namespace SelectorUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions {
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.Never,
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        }.AppendTypeInfoResolver();

        public MainWindow() {
            this.InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            var featureTypeTestFeature = new TestFeature {
                featuresDetectedNested = new featuresDetectedNested {
                    featureName = [new featureName {
                        name ="Nested",
                        language="eng",
                    }],
                },

                categoryOfTemporalVariation = 1,
                dataAssessment = 1,

                featuresDetected = new featuresDetected {
                    significantFeaturesDetected = true,
                    leastDepthOfDetectedFeaturesMeasured = false,
                },
                zoneOfConfidence = [new zoneOfConfidence {
                    categoryOfZoneOfConfidenceInData = 1,
                }]
            };

            var featureTypeWreck = new Wreck {

            };

            //var selectedObject = new S100AttributeEditorViewModel(featureType, "123456");

            //var featureType = new IslandGroup {

            //};

            //var featureType = new S100FC.S128.FeatureTypes.ElectronicProduct {

            //};


            var qualityOfBathymetricData = new QualityOfBathymetricData {
                depthRangeMaximumValue = 17.5m,
                depthRangeMinimumValue = 5.2m,
                fullSeafloorCoverageAchieved = true,
                featuresDetected = new featuresDetected {
                    leastDepthOfDetectedFeaturesMeasured = true,
                    significantFeaturesDetected = false,
                    sizeOfFeaturesDetected = 3.5m,
                },
                zoneOfConfidence = [new zoneOfConfidence {
                    categoryOfZoneOfConfidenceInData = 1,
                }, new zoneOfConfidence {
                    categoryOfZoneOfConfidenceInData = 2,
                }],
            };


            var codeSpanFixed = "SpanFixed";
            var jsonSpanFixed = "{  \"verticalClearanceFixed.verticalClearanceValue\": 18,  \"horizontalClearanceFixed.horizontalClearanceValue\": null,  \"horizontalClearanceFixed.horizontalDistanceUncertainty\": null}";
            var jsonSpanFixedInformationBindings = "[{\"association\":{\"S100FC_code\":\"AdditionalInformation\",\"attributes\":[]},\"roleType\":\"association\",\"role\":\"theInformation\",\"informationType\":\"NauticalInformation\",\"informationId\":\"I1\"}]";
            var jsonSpanFixedFeatureBindings = "[{\"association\":{\"S100FC_code\":\"BridgeAggregation\",\"attributes\":[]},\"roleType\":\"aggregation\",\"role\":\"theCollection\",\"featureType\":\"Bridge\",\"featureId\":\"F1550755\"}]";


            var codeLateralBuoy = "LateralBuoy";
            var jsonLateralBuoy = "{\"buoyShape\":2,\"categoryOfLateralMark\":1,\"colour[0]\":3,\"colour[1]\":4,\"featureName[0].language\":\"eng\",\"featureName[0].name\":\"No 6\",\"featureName[0].nameUsage\":1,\"periodicDateRange[0].dateEnd\":\"----1115\",\"periodicDateRange[0].dateStart\":\"----0401\",\"status[0]\":5,\"scaleMinimum\":179999}";

            var jsonObstructionAttributeBindings = "{\"waterLevelEffect\": null,\"surroundingDepth\": 4,\"valueOfSounding\": null,\"scaleMinimum\": 89999,\"defaultClearanceDepth\": -15}";
            var jsonObstructionInformationBindings = "[{\"code\":\"AdditionalInformation\",\"association\":{\"attributes\":[]},\"roleType\":\"association\",\"role\":\"theInformation\",\"informationType\":\"NauticalInformation\",\"informationId\":\"I13\"}]";

            var code = codeLateralBuoy;
            var json = jsonLateralBuoy;


            //var json = qualityOfBathymetricData.Flatten();

            var ps = XDocument.Load(System.IO.Path.Combine(Environment.GetEnvironmentVariable("GITHUB-IHO")!, @"S-101-Documentation-and-FC\S-101FC\FeatureCatalogue.xml"));

            //var typePoints = ps.GetFeatureTypes(Primitives.point);

            //var selectedObjectFC = new S100AttributeEditorViewModelFC(ps, "QualityOfBathymetricData").LoadAttributeBindings(json);
            var selectedObjectFC = new S100AttributeEditorViewModel(ps).Initialize(code)
                .LoadAttributeBindings(jsonLateralBuoy);
                //.LoadInformationBindings(jsonSpanFixedInformationBindings)
                //.LoadFeatureBindings(jsonSpanFixedFeatureBindings);

            selectedObjectFC.PropertyChanged += this.PropertyGrid_PropertyChanged;

            //selectedObjectFC += new informationBinding<QualityOfBathymetricDataComposition> {
            //    roleType = "association",
            //    role = "theQualityInformation",
            //    informationType = "SpatialQuality",
            //    informationId = RandomString(5),
            //};

            //selectedObjectFC += new featureBinding<UpdatedInformation> {
            //    roleType = "association",
            //    role = "theUpdate",
            //    featureType = "UpdateInformation",
            //    featureId = RandomString(5),
            //};

            //System.Diagnostics.Debugger.Break();


            //var selectedObject = new S100AttributeEditorViewModel(featureTypeTestFeature, "123456") {
            //    RequestInformation = async (s, e) => {
            //        var random = new Random(DateTime.Now.Millisecond);
            //        string[] result = [];
            //        for (int i = 0; i < random.Next(1, 8); i++) {
            //            var text = RandomString(5).ToUpperInvariant();
            //            result = [.. result, text];
            //        }
            //        return result;
            //    },
            //    RequestFeatures = async (s, e) => {
            //        var random = new Random(DateTime.Now.Millisecond);
            //        string[] result = [];
            //        for (int i = 0; i < random.Next(1, 8); i++) {
            //            var text = RandomString(5).ToUpperInvariant();
            //            result = [.. result, text];
            //        }
            //        return result;
            //    },
            //};

            //selectedObject += new informationBinding<QualityOfBathymetricDataComposition> {
            //    roleType = "association",
            //    role = "theQualityInformation",
            //    informationType = "SpatialQuality",
            //    informationId = RandomString(5),
            //};

            //selectedObject += new featureBinding<UpdatedInformation> {
            //    roleType = "association",
            //    role = "theUpdate",
            //    featureType = "UpdateInformation",
            //    featureId = RandomString(5),
            //};


            //var bindings = (featureBinding[])selectedObject;

            //var json2 = System.Text.Json.JsonSerializer.Serialize(bindings, jsonSerializerOptions);


            //selectedObject.PropertyChanged += this.PropertyGrid_PropertyChanged;

            this.PropertyGrid.SelectedObject = selectedObjectFC;

            this.PropertyGrid.PropertyChanged += this.PropertyGrid_PropertyChanged;
        }

        private void PropertyGrid_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e) {
            ;
        }

        private static readonly char[] _chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789".ToCharArray();

        public static string RandomString(int length) { var result = new char[length]; var rng = Random.Shared; for (int i = 0; i < length; i++) result[i] = _chars[rng.Next(_chars.Length)]; return new string(result); }
    }


    public class JsonUnflattener
    {
        public static JsonNode Unflatten(string jsonString) {
            // 1. Parse the flat string into a dictionary
            var flatDict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(jsonString);
            var root = new JsonObject();

            foreach (var kvp in flatDict) {
                // Split by dot, but ignore dots inside brackets if necessary 
                // (Simple split works for your example)
                string[] parts = kvp.Key.Split('.');
                ProcessPath(root, parts, kvp.Value);
            }

            return root;
        }

        private static void ProcessPath(JsonObject currentParent, string[] parts, JsonElement value) {
            JsonNode currentNode = currentParent;

            for (int i = 0; i < parts.Length; i++) {
                string part = parts[i];
                bool isLast = (i == parts.Length - 1);

                // Check if the part indicates an array, e.g., "zoneOfConfidence[0]"
                var arrayMatch = Regex.Match(part, @"^(.+)\[(\d+)\]$");

                if (arrayMatch.Success) {
                    string arrayName = arrayMatch.Groups[1].Value;
                    int index = int.Parse(arrayMatch.Groups[2].Value);

                    // Ensure the array exists
                    if (!currentParent.ContainsKey(arrayName) || currentParent[arrayName] == null) {
                        currentParent[arrayName] = new JsonArray();
                    }

                    JsonArray array = currentParent[arrayName].AsArray();

                    // Expand array with nulls if index is higher than current count
                    while (array.Count <= index) { array.Add(null); }

                    if (isLast) {
                        array[index] = JsonValue.Create(value);
                    }
                    else {
                        // If not last, we need an object at this index to continue
                        if (array[index] == null) { array[index] = new JsonObject(); }
                        currentParent = array[index].AsObject();
                    }
                }
                else {
                    // It's a regular property
                    if (isLast) {
                        currentParent[part] = JsonValue.Create(value);
                    }
                    else {
                        if (!currentParent.ContainsKey(part) || currentParent[part] == null) {
                            currentParent[part] = new JsonObject();
                        }
                        currentParent = currentParent[part].AsObject();
                    }
                }
            }
        }
    }


}