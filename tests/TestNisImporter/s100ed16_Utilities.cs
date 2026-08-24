using ArcGIS.Core.Data;
using ArcGIS.Core.Data.DDL;
using ArcGIS.Core.SystemCore;
using S100FC;

using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using IO = System.IO;

namespace TestNisImporter
{
    public class s100ed16_Utilities
    {
        private readonly ITestOutputHelper _output;

        private readonly string _iho;

        Func<Geodatabase> createGeodatabase = () => { throw new NotImplementedException(); };

        public s100ed16_Utilities(ITestOutputHelper output) {
            this._output = output;
            this._iho = Environment.GetEnvironmentVariable("GITHUB-IHO")!;
            ArcGIS.Core.Hosting.Host.Initialize();            
        }

        [Fact]
        public void BuildSubtypes() {
            this.Initialize(@"L:\B061450\ArcGIS\S-101_LaLaLand\S-101_LaLaLand.gdb");


            var ps = XDocument.Load(System.IO.Path.Combine(this._iho, @"S-101-Documentation-and-FC\S-101FC\FeatureCatalogue.xml"));

            var navigator = ps.CreateNavigator();
            navigator.MoveToFollowing(XPathNodeType.Element);

            var scopes = navigator.GetNamespacesInScope(XmlNamespaceScope.All);

            var namespaceManager = new XmlNamespaceManager(new NameTable());
            foreach (var s in scopes)
                namespaceManager.AddNamespace(s.Key, s.Value);

            using (var destination = createGeodatabase()) {
                SchemaBuilder schemaBuilder = new SchemaBuilder(destination);

                string[] featureClasses = ["point", "pointset", "curve", "surface"];

                foreach (var name in featureClasses) {
                    var featureTypes = ps
                        .XPathSelectElements($"//S100FC:S100_FC_FeatureType[S100FC:permittedPrimitives='{name}']", namespaceManager);

                    FeatureClassDefinition fcDefinition = destination.GetDefinition<FeatureClassDefinition>(name);

                    FeatureClassDescription fcDescription = new FeatureClassDescription(fcDefinition);

                    var definitionReferences = new Dictionary<int, string> { { 0, "UNKNOWN" } };
                    fcDescription.SubtypeFieldDescription = new SubtypeFieldDescription("sourceIdentifier", definitionReferences);
                    schemaBuilder.Modify(fcDescription);
                    schemaBuilder.Build();

                    foreach (var featureType in featureTypes) {
                        var code = featureType.XPathSelectElement("S100FC:code", namespaceManager)!.Value;
                        var sourceIdentifier = featureType.XPathSelectElement("S100FC:definitionReference/S100FC:sourceIdentifier",namespaceManager)?.Value;
                        if (string.IsNullOrEmpty(sourceIdentifier)) {
                            this._output.WriteLine($"{name}: {code}, missing sourceIdentifier!");
                            continue;
                        }
                        
                        definitionReferences.Add(int.Parse(sourceIdentifier), code);
                    }
                    fcDescription.SubtypeFieldDescription = new SubtypeFieldDescription("sourceIdentifier", definitionReferences);
                    schemaBuilder.Modify(fcDescription);
                    schemaBuilder.Build();
                }

                {
                    var featureTypes = ps
                        .XPathSelectElements($"//S100FC:S100_FC_FeatureType[S100FC:permittedPrimitives='noGeometry']", namespaceManager);

                    var tableDefinition = destination.GetDefinition<TableDefinition>("featuretype");

                    var tableDescription = new TableDescription(tableDefinition);

                    var definitionReferences = new Dictionary<int, string> { { 0, "UNKNOWN" } };
                    tableDescription.SubtypeFieldDescription = new SubtypeFieldDescription("sourceIdentifier", definitionReferences);
                    schemaBuilder.Modify(tableDescription);
                    schemaBuilder.Build();

                    foreach (var featureType in featureTypes) {
                        var code = featureType.XPathSelectElement("S100FC:code", namespaceManager)!.Value;
                        var sourceIdentifier = featureType.XPathSelectElement("S100FC:definitionReference/S100FC:sourceIdentifier", namespaceManager)?.Value;
                        if (string.IsNullOrEmpty(sourceIdentifier)) {
                            this._output.WriteLine($"featuretype: {code}, missing sourceIdentifier!");
                            continue;
                        }
                        
                        definitionReferences.Add(int.Parse(sourceIdentifier), code);
                    }
                    tableDescription.SubtypeFieldDescription = new SubtypeFieldDescription("sourceIdentifier", definitionReferences);
                    schemaBuilder.Modify(tableDescription);
                    schemaBuilder.Build();
                }
                {
                    var informationTypes = ps
                        .XPathSelectElements($"//S100FC:S100_FC_InformationType", namespaceManager);

                    var tableDefinition = destination.GetDefinition<TableDefinition>("informationtype");

                    var tableDescription = new TableDescription(tableDefinition);

                    var definitionReferences = new Dictionary<int, string> { { 0, "UNKNOWN" } };
                    schemaBuilder.Modify(tableDescription);
                    schemaBuilder.Build();

                    foreach (var featureType in informationTypes) {
                        var code = featureType.XPathSelectElement("S100FC:code", namespaceManager)!.Value;
                        var sourceIdentifier = featureType.XPathSelectElement("S100FC:definitionReference/S100FC:sourceIdentifier", namespaceManager)?.Value;
                        if (string.IsNullOrEmpty(sourceIdentifier)) {
                            this._output.WriteLine($"informationtype: {code}, missing sourceIdentifier!");
                            continue;
                        }
                        
                        definitionReferences.Add(int.Parse(sourceIdentifier), code);
                    }
                    tableDescription.SubtypeFieldDescription = new SubtypeFieldDescription("sourceIdentifier", definitionReferences);
                    schemaBuilder.Modify(tableDescription);
                    schemaBuilder.Build();
                }
            }
        }

        private void Initialize(string target) {
            if (IO.File.Exists(target) && ".sde".Equals(IO.Path.GetExtension(target), StringComparison.OrdinalIgnoreCase)) {
                createGeodatabase = () => {
                    var geodatabase = new Geodatabase(new DatabaseConnectionFile(new Uri(IO.Path.GetFullPath(target))));

                    return geodatabase;
                };
            }
            else if (IO.Directory.Exists(target) && ".gdb".Equals(IO.Path.GetExtension(target), StringComparison.OrdinalIgnoreCase)) {
                createGeodatabase = () => {
                    var geodatabase = new Geodatabase(new FileGeodatabaseConnectionPath(new Uri(IO.Path.GetFullPath(target))));

                    return geodatabase;
                };
            }
            else if (".geodatabase".Equals(IO.Path.GetExtension(target), StringComparison.OrdinalIgnoreCase)) {
                createGeodatabase = () => {
                    var geodatabase = new Geodatabase(new MobileGeodatabaseConnectionPath(new Uri(IO.Path.GetFullPath(target))));

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
        }
    }
}
