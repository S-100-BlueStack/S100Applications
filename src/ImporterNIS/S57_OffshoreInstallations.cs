using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using S100FC.S101.FeatureTypes;
using S100Framework.Applications.S57.esri;
using S100Framework.Applications.Singletons;

namespace S100Framework.Applications
{
    internal static partial class ImporterNIS
    {
        private static void S57_OffshoreInstallations(string tableName, Func<string, FeatureClass> source, QueryFilter filter, Func<FeatureClass> target, Action<RowBuffer, Geometry> setShape) {
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
                    case "offshoreinstallationsp::1": { // OFSPLF_OffshorePlatform
                            var instance = (OffshorePlatform)ImporterNIS.Build("OFSPLF", current, buffer);

                            using var featureN = featureClass.CreateRow(buffer);
                            var name = featureN.UID();

                            if (FeatureRelations.Instance.HasSlaves(current.GLOBALID())) {
                                relatedEquipment!.CreateRelatedPointEquipment(new OffshoreInstallationsP(current), instance, featureN, instance.scaleMinimum);
                                relatedEquipment!.CreateRelatedLineEquipment(current, instance, featureN);
                                relatedEquipment!.CreateRelatedAreaEquipment(current, instance, featureN, instance.scaleMinimum);
                            }
                            ConversionAnalytics.Instance.AddConverted(tableName, current.GLOBALID(), name);
                            Logger.Current.DataObject(current.GetObjectID(), tableName, longname, System.Text.Json.JsonSerializer.Serialize(instance, ImporterNIS.jsonSerializerOptions));
                        }
                        break;
                    case "offshoreinstallationsl::1": { // CBLSUB_CableSubmarine
                            var instance = (CableSubmarine)ImporterNIS.Build("CBLSUB", feature, buffer);

                            using var featureN = featureClass.CreateRow(buffer);
                            var name = featureN.UID();

                            if (FeatureRelations.Instance.HasSlaves(globalid)) {
                                relatedEquipment!.CreateRelatedPointEquipment(new OffshoreInstallationsL(current), instance, featureN, instance.scaleMinimum);
                                relatedEquipment!.CreateRelatedLineEquipment(current, instance, featureN);
                                relatedEquipment!.CreateRelatedAreaEquipment(current, instance, featureN, instance.scaleMinimum);
                            }
                            ConversionAnalytics.Instance.AddConverted(tableName, globalid, name);
                            Logger.Current.DataObject(objectid, tableName, longname, System.Text.Json.JsonSerializer.Serialize(instance, ImporterNIS.jsonSerializerOptions));
                        }
                        break;
                    case "offshoreinstallationsa::1": { // CBLARE_CableArea
                            var instance = (CableArea)ImporterNIS.Build("CBLARE", feature, buffer);

                            using var featureN = featureClass.CreateRow(buffer);
                            var name = featureN.UID();

                            if (FeatureRelations.Instance.HasSlaves(globalid)) {
                                relatedEquipment!.CreateRelatedPointEquipment(new OffshoreInstallationsA(current), instance, featureN, instance.scaleMinimum);
                                relatedEquipment!.CreateRelatedLineEquipment(current, instance, featureN);
                                relatedEquipment!.CreateRelatedAreaEquipment(current, instance, featureN, instance.scaleMinimum);
                            }
                            ConversionAnalytics.Instance.AddConverted(tableName, globalid, name);
                            Logger.Current.DataObject(objectid, tableName, longname, System.Text.Json.JsonSerializer.Serialize(instance, ImporterNIS.jsonSerializerOptions));
                        }
                        break;
                    case "offshoreinstallationsp::5": { // PIPARE_PipelineArea
                            var instance = (SubmarinePipelineArea)ImporterNIS.Build("PIPARE", feature, buffer);

                            using var featureN = featureClass.CreateRow(buffer);
                            var name = featureN.UID();

                            if (FeatureRelations.Instance.HasSlaves(globalid)) {
                                relatedEquipment!.CreateRelatedPointEquipment(new OffshoreInstallationsP(current), instance, featureN, instance.scaleMinimum);
                                relatedEquipment!.CreateRelatedLineEquipment(current, instance, featureN);
                                relatedEquipment!.CreateRelatedAreaEquipment(current, instance, featureN, instance.scaleMinimum);
                            }
                            ConversionAnalytics.Instance.AddConverted(tableName, globalid, name);
                            Logger.Current.DataObject(objectid, tableName, longname, System.Text.Json.JsonSerializer.Serialize(instance, ImporterNIS.jsonSerializerOptions));
                        }
                        break;
                    case "offshoreinstallationsl::5": { // PIPSOL_PipelineSubmarineOnLand
                            var instance = (PipelineSubmarineOnLand)ImporterNIS.Build("PIPSOL", feature, buffer);

                            using var featureN = featureClass.CreateRow(buffer);
                            var name = featureN.UID();

                            if (FeatureRelations.Instance.HasSlaves(globalid)) {
                                relatedEquipment!.CreateRelatedPointEquipment(new OffshoreInstallationsL(current), instance, featureN, instance.scaleMinimum);
                                relatedEquipment!.CreateRelatedLineEquipment(current, instance, featureN);
                                relatedEquipment!.CreateRelatedAreaEquipment(current, instance, featureN, instance.scaleMinimum);
                            }
                            ConversionAnalytics.Instance.AddConverted(tableName, globalid, name);
                            Logger.Current.DataObject(objectid, tableName, longname, System.Text.Json.JsonSerializer.Serialize(instance, ImporterNIS.jsonSerializerOptions));
                        }
                        break;
                    case "offshoreinstallationsa::5": { // OFSPLF_OffshorePlatform
                            var instance = (OffshorePlatform)ImporterNIS.Build("OFSPLF", feature, buffer);

                            using var featureN = featureClass.CreateRow(buffer);
                            var name = featureN.UID();

                            if (FeatureRelations.Instance.HasSlaves(globalid)) {
                                relatedEquipment!.CreateRelatedPointEquipment(new OffshoreInstallationsA(current), instance, featureN, instance.scaleMinimum);
                                relatedEquipment!.CreateRelatedLineEquipment(current, instance, featureN);
                                relatedEquipment!.CreateRelatedAreaEquipment(current, instance, featureN, instance.scaleMinimum);
                            }
                            ConversionAnalytics.Instance.AddConverted(tableName, globalid, name);
                            Logger.Current.DataObject(objectid, tableName, longname, System.Text.Json.JsonSerializer.Serialize(instance, ImporterNIS.jsonSerializerOptions));
                        }
                        break;
                    case "offshoreinstallationsp::10": { // PIPSOL_PipelineSubmarineOnLand
                            var instance = (PipelineSubmarineOnLand)ImporterNIS.Build("PIPSOL", current, buffer);

                            using var featureN = featureClass.CreateRow(buffer);
                            var name = featureN.UID();

                            if (FeatureRelations.Instance.HasSlaves(current.GLOBALID())) {
                                relatedEquipment!.CreateRelatedPointEquipment(new OffshoreInstallationsP(current), instance, featureN, instance.scaleMinimum);
                                relatedEquipment!.CreateRelatedLineEquipment(current, instance, featureN);
                                relatedEquipment!.CreateRelatedAreaEquipment(current, instance, featureN, instance.scaleMinimum);
                            }
                            ConversionAnalytics.Instance.AddConverted(tableName, current.GLOBALID(), name);
                            Logger.Current.DataObject(current.GetObjectID(), tableName, longname, System.Text.Json.JsonSerializer.Serialize(instance, ImporterNIS.jsonSerializerOptions));
                        }
                        break;
                    case "offshoreinstallationsa::10": { // OSPARE_OffshoreProductionArea
                            var instance = (OffshoreProductionArea)ImporterNIS.Build("OSPARE", feature, buffer);

                            using var featureN = featureClass.CreateRow(buffer);
                            var name = featureN.UID();

                            if (FeatureRelations.Instance.HasSlaves(globalid)) {
                                relatedEquipment!.CreateRelatedPointEquipment(new OffshoreInstallationsA(current), instance, featureN, instance.scaleMinimum);
                                relatedEquipment!.CreateRelatedLineEquipment(current, instance, featureN);
                                relatedEquipment!.CreateRelatedAreaEquipment(current, instance, featureN, instance.scaleMinimum);
                            }
                            ConversionAnalytics.Instance.AddConverted(tableName, globalid, name);
                            Logger.Current.DataObject(objectid, tableName, longname, System.Text.Json.JsonSerializer.Serialize(instance, ImporterNIS.jsonSerializerOptions));
                        }
                        break;
                    case "offshoreinstallationsa::15": { // PIPARE_SubmarinePipelineArea
                            var instance = (SubmarinePipelineArea)ImporterNIS.Build("PIPARE", feature, buffer);

                            using var featureN = featureClass.CreateRow(buffer);
                            var name = featureN.UID();

                            if (FeatureRelations.Instance.HasSlaves(globalid)) {
                                relatedEquipment!.CreateRelatedPointEquipment(new OffshoreInstallationsA(current), instance, featureN, instance.scaleMinimum);
                                relatedEquipment!.CreateRelatedLineEquipment(current, instance, featureN);
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

