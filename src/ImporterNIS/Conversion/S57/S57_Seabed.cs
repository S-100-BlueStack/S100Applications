using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using S100FC.S101.FeatureTypes;
using S100Framework.Applications.S57.esri;
using S100Framework.Applications.Singletons;

namespace S100Framework.Applications
{
    internal static partial class ImporterNIS
    {
        private static void S57_Seabed(string tableName, Func<string, FeatureClass> source, QueryFilter filter, Func<FeatureClass> target, Action<RowBuffer, Geometry> setShape) {
            using var dataset = source(tableName);
            Subtypes.Instance.RegisterSubtypes(dataset);

            using var featureClass = target();    // target.OpenDataset<FeatureClass>(target.GetName("point"));

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

                    case "seabedl::10": { // SBDARE_SeabedArea
                            var instance = (SeabedArea)ImporterNIS.Build("SBDARE", current, buffer);

                            using var featureN = featureClass.CreateRow(buffer);
                            var name = featureN.UID();

                            if (FeatureRelations.Instance.HasSlaves(current.GLOBALID())) {
                                relatedEquipment!.CreateRelatedLineEquipment(new SeabedP(current), instance, featureN);
                            }
                            ConversionAnalytics.Instance.AddConverted(tableName, current.GLOBALID(), name);
                            Logger.Current.DataObject(objectid, tableName, longname, System.Text.Json.JsonSerializer.Serialize(instance, ImporterNIS.jsonSerializerOptions));
                        }
                        break;

                    case "seabedp::15": { // SBDARE_SeabedArea                            
                            var instance = (SeabedArea)ImporterNIS.Build("SBDARE", current, buffer);

                            using var featureN = featureClass.CreateRow(buffer);
                            var name = featureN.UID();

                            if (FeatureRelations.Instance.HasSlaves(current.GLOBALID())) {
                                relatedEquipment!.CreateRelatedPointEquipment(new SeabedP(current), instance, featureN, instance.scaleMinimum);
                            }
                            ConversionAnalytics.Instance.AddConverted(tableName, current.GLOBALID(), name);
                            Logger.Current.DataObject(objectid, tableName, longname, System.Text.Json.JsonSerializer.Serialize(instance, ImporterNIS.jsonSerializerOptions));
                        }
                        break;
                    case "seabeda::15": { // SBDARE_SeabedArea                            
                            var instance = (SeabedArea)ImporterNIS.Build("SBDARE", current, buffer);

                            using var featureN = featureClass.CreateRow(buffer);
                            var name = featureN.UID();

                            if (FeatureRelations.Instance.HasSlaves(current.GLOBALID())) {
                                relatedEquipment!.CreateRelatedAreaEquipment(current, instance, featureN, instance.scaleMinimum);
                            }
                            ConversionAnalytics.Instance.AddConverted(tableName, current.GLOBALID(), name);
                            Logger.Current.DataObject(objectid, tableName, longname, System.Text.Json.JsonSerializer.Serialize(instance, ImporterNIS.jsonSerializerOptions));
                        }
                        break;

                    case "seabedl::15": { // SNDWAV_SandWaves
                            var instance = (Sandwave)ImporterNIS.Build("SNDWAV", current, buffer);

                            using var featureN = featureClass.CreateRow(buffer);
                            var name = featureN.UID();

                            if (FeatureRelations.Instance.HasSlaves(current.GLOBALID())) {
                                relatedEquipment!.CreateRelatedLineEquipment(new SeabedP(current), instance, featureN);
                            }
                            ConversionAnalytics.Instance.AddConverted(tableName, current.GLOBALID(), name);
                            Logger.Current.DataObject(objectid, tableName, longname, System.Text.Json.JsonSerializer.Serialize(instance, ImporterNIS.jsonSerializerOptions));
                        }
                        break;

                    case "seabedp::25": { // SNDWAV_SandWaves
                            var instance = (Sandwave)ImporterNIS.Build("SNDWAV", current, buffer);

                            using var featureN = featureClass.CreateRow(buffer);
                            var name = featureN.UID();

                            if (FeatureRelations.Instance.HasSlaves(current.GLOBALID())) {
                                relatedEquipment!.CreateRelatedPointEquipment(new SeabedP(current), instance, featureN, instance.scaleMinimum);
                            }
                            ConversionAnalytics.Instance.AddConverted(tableName, current.GLOBALID(), name);
                            Logger.Current.DataObject(objectid, tableName, longname, System.Text.Json.JsonSerializer.Serialize(instance, ImporterNIS.jsonSerializerOptions));
                        }
                        break;
                    case "seabeda::30": { // SNDWAV_SandWaves
                            var instance = (Sandwave)ImporterNIS.Build("SNDWAV", current, buffer);

                            using var featureN = featureClass.CreateRow(buffer);
                            var name = featureN.UID();

                            if (FeatureRelations.Instance.HasSlaves(current.GLOBALID())) {
                                relatedEquipment!.CreateRelatedAreaEquipment(current, instance, featureN, instance.scaleMinimum);
                            }
                            ConversionAnalytics.Instance.AddConverted(tableName, current.GLOBALID(), name);
                            Logger.Current.DataObject(objectid, tableName, longname, System.Text.Json.JsonSerializer.Serialize(instance, ImporterNIS.jsonSerializerOptions));
                        }
                        break;

                    case "seabedp::30": { // SPRING_Spring
                            var instance = (Spring)ImporterNIS.Build("SPRING", current, buffer);

                            using var featureN = featureClass.CreateRow(buffer);
                            var name = featureN.UID();

                            if (FeatureRelations.Instance.HasSlaves(current.GLOBALID())) {
                                relatedEquipment!.CreateRelatedPointEquipment(new SeabedP(current), instance, featureN, instance.scaleMinimum);
                            }
                            ConversionAnalytics.Instance.AddConverted(tableName, current.GLOBALID(), name);
                            Logger.Current.DataObject(objectid, tableName, longname, System.Text.Json.JsonSerializer.Serialize(instance, ImporterNIS.jsonSerializerOptions));
                        }
                        break;

                    case "seabedp::35": { // WEDKLP_WeedKelp
                            var instance = ImporterNIS.Build("WEDKLP", current, buffer);

                            using var featureN = featureClass.CreateRow(buffer);
                            var name = featureN.UID();

                            if (FeatureRelations.Instance.HasSlaves(current.GLOBALID())) {
                                if (instance is WeedKelp weedKelp)
                                    relatedEquipment!.CreateRelatedPointEquipment(new SeabedP(current), instance, featureN, weedKelp.scaleMinimum);
                                if (instance is Seagrass seagrass)
                                    relatedEquipment!.CreateRelatedPointEquipment(new SeabedP(current), instance, featureN, seagrass.scaleMinimum);
                            }
                            ConversionAnalytics.Instance.AddConverted(tableName, current.GLOBALID(), name);
                            Logger.Current.DataObject(objectid, tableName, longname, System.Text.Json.JsonSerializer.Serialize(instance, ImporterNIS.jsonSerializerOptions));
                        }
                        break;
                    case "seabeda::40": { // WEDKLP_WeedKelp
                            var instance = ImporterNIS.Build("WEDKLP", current, buffer);

                            using var featureN = featureClass.CreateRow(buffer);
                            var name = featureN.UID();

                            if (FeatureRelations.Instance.HasSlaves(current.GLOBALID())) {
                                if (instance is WeedKelp weedKelp)
                                    relatedEquipment!.CreateRelatedAreaEquipment(current, instance, featureN, weedKelp.scaleMinimum);
                                if (instance is Seagrass seagrass)
                                    relatedEquipment!.CreateRelatedAreaEquipment(current, instance, featureN, seagrass.scaleMinimum);
                            }
                            ConversionAnalytics.Instance.AddConverted(tableName, current.GLOBALID(), name);
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
