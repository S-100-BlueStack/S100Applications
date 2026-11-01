using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using S100FC.S101.FeatureTypes;
using S100Framework.Applications.S57.esri;
using S100Framework.Applications.Singletons;

namespace S100Framework.Applications
{
    internal static partial class ImporterNIS
    {

        private static void S57_MilitaryFeature(string tableName, Func<string, FeatureClass> source, QueryFilter filter, Func<FeatureClass> target, Action<RowBuffer, Geometry> setShape) {
            using var dataset = source(tableName);
            Subtypes.Instance.RegisterSubtypes(dataset);

            using var featureClass = target();

            using var buffer = featureClass.CreateRowBuffer();

            using var cursor = dataset.Search(filter, true);
            int recordCount = 0;

            while (cursor.MoveNext()) {
                recordCount += 1;
                var feature = (Feature)cursor.Current;
                var current = feature;

                var objectid = current.GetObjectID();
                var globalid = current.GLOBALID();

                if (FeatureRelations.Instance.IsSlave(globalid)) {
                    continue;
                }

                if (ConversionAnalytics.Instance.IsConverted(globalid)) {
                    throw new Exception("Ups. Not supported");
                }

                var longname = current.LNAM() ?? string.Empty;

                switch ($"{tableName}::{feature.FCSubtype()}".ToLowerInvariant()) {
                    case "militaryfeaturesp::30": { // MIPARE_MilitaryPracticeArea
                            var instance = (MilitaryPracticeArea)ImporterNIS.Build("MIPARE", feature, buffer);

                            using var featureN = featureClass.CreateRow(buffer);
                            var name = featureN.UID();

                            if (FeatureRelations.Instance.HasSlaves(globalid)) {
                                relatedEquipment!.CreateRelatedPointEquipment(new MilitaryFeaturesP(current), instance, featureN, instance.scaleMinimum);
                            }
                            ConversionAnalytics.Instance.AddConverted(tableName, globalid, name);
                            Logger.Current.DataObject(objectid, tableName, longname, System.Text.Json.JsonSerializer.Serialize(instance, ImporterNIS.jsonSerializerOptions));
                        }
                        break;

                    case "militaryfeaturesa::60": { // MIPARE_MilitaryPracticeArea
                            var instance = (MilitaryPracticeArea)ImporterNIS.Build("MIPARE", feature, buffer);

                            using var featureN = featureClass.CreateRow(buffer);
                            var name = featureN.UID();

                            if (FeatureRelations.Instance.HasSlaves(globalid)) {
                                relatedEquipment!.CreateRelatedAreaEquipment(current, instance, featureN, instance.scaleMinimum);
                            }
                            ConversionAnalytics.Instance.AddConverted(tableName, globalid, name);
                            Logger.Current.DataObject(objectid, tableName, longname, System.Text.Json.JsonSerializer.Serialize(instance, ImporterNIS.jsonSerializerOptions));
                        }
                        break;
                    default:
                        // code block
                        System.Diagnostics.Debugger.Break();
                        break;

                }
            }
            Logger.Current.DataTotalCount(tableName, recordCount, ConversionAnalytics.Instance.GetConvertedCount(tableName));
        }


    }
}
