using ArcGIS.Core.Data;
using ArcGIS.Core.Data.DDL;
using S100FC;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using IO = System.IO;

namespace TestNisImporter
{
    public class Builder
    {
        private readonly ITestOutputHelper _output;

        public Builder(ITestOutputHelper output) {
            this._output = output;
            ArcGIS.Core.Hosting.Host.Initialize();
        }


        [Fact]
        public void SubTypes_S122() {
            var sourcePath = @"<FullPath>";

            using var geodatabase = new Geodatabase(new FileGeodatabaseConnectionPath(new Uri(IO.Path.GetFullPath(sourcePath))));

            var ihoPath = Environment.GetEnvironmentVariable("GITHUB-IHO")!;


            SchemaBuilder schemaBuilder = new SchemaBuilder(geodatabase);

            string[] featureClasses = ["point", "pointset", "curve", "surface"];
            foreach (var featureClassName in featureClasses) {
                var primitive = featureClassName switch {
                    "point" => S100FC.Primitives.point,
                    "pointset" => S100FC.Primitives.pointSet,
                    "curve" => S100FC.Primitives.curve,
                    "surface" => S100FC.Primitives.surface,
                    _ => throw new NotImplementedException()
                };

                var features = S100FC.S122.Summary.PrimitiveFeatures(primitive);

                FeatureClassDefinition fcDefinition = geodatabase.GetDefinition<FeatureClassDefinition>(featureClassName);

                FeatureClassDescription fcDescription = new FeatureClassDescription(fcDefinition);

                var definitionReferences = new Dictionary<int, string> { { 0, "UNKNOWN" } };
                fcDescription.SubtypeFieldDescription = new SubtypeFieldDescription(fcDefinition.GetSubtypeField(), definitionReferences);
                schemaBuilder.Modify(fcDescription);
                schemaBuilder.Build();

                foreach (var e in S100FC.S122.Summary.definitionReferenceFeatureTypes.Where(e => features.Contains(e.code)).OrderBy(e => e.code)) {
                    definitionReferences.Add(e.sourceIdentifier, e.code);
                }
                fcDescription.SubtypeFieldDescription = new SubtypeFieldDescription(fcDefinition.GetSubtypeField(), definitionReferences);
                schemaBuilder.Modify(fcDescription);
                schemaBuilder.Build();
            }
            {
                var features = S100FC.S122.Summary.PrimitiveFeatures(Primitives.noGeometry);

                var tableDefinition = geodatabase.GetDefinition<TableDefinition>("featuretype");

                var tableDescription = new TableDescription(tableDefinition);

                var definitionReferences = new Dictionary<int, string> { { 0, "UNKNOWN" } };
                tableDescription.SubtypeFieldDescription = new SubtypeFieldDescription(tableDefinition.GetSubtypeField(), definitionReferences);
                schemaBuilder.Modify(tableDescription);
                schemaBuilder.Build();

                foreach (var e in S100FC.S122.Summary.definitionReferenceFeatureTypes.Where(e => features.Contains(e.code)).OrderBy(e => e.code)) {
                    definitionReferences.Add(e.sourceIdentifier, e.code);
                }
                tableDescription.SubtypeFieldDescription = new SubtypeFieldDescription(tableDefinition.GetSubtypeField(), definitionReferences);
                schemaBuilder.Modify(tableDescription);
                schemaBuilder.Build();
            }
            {
                var tableDefinition = geodatabase.GetDefinition<TableDefinition>("informationtype");

                var tableDescription = new TableDescription(tableDefinition);

                var definitionReferences = new Dictionary<int, string> { { 0, "UNKNOWN" } };
                schemaBuilder.Modify(tableDescription);
                schemaBuilder.Build();

                foreach (var e in S100FC.S122.Summary.definitionReferenceInformationTypes.OrderBy(e => e.sourceIdentifier)) {
                    definitionReferences.Add(e.sourceIdentifier, e.code);
                }
                tableDescription.SubtypeFieldDescription = new SubtypeFieldDescription(tableDefinition.GetSubtypeField(), definitionReferences);
                schemaBuilder.Modify(tableDescription);
                schemaBuilder.Build();
            }
        }
    }
}
