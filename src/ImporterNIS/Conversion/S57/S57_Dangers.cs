using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using S100FC;
using S100FC.S101.FeatureTypes;
using S100Framework.Applications.S57.esri;
using S100Framework.Applications.Singletons;

namespace S100Framework.Applications
{
    internal static partial class ImporterNIS
    {
        private static void S57_Dangers(string tableName, Func<string, FeatureClass> source, QueryFilter filter, Func<FeatureClass> target, Action<RowBuffer, Geometry> setShape) {
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
                    case "dangersp::1": { // CTNARE_CautionArea
                            var instance = (CautionArea)ImporterNIS.Build("CTNARE", feature, buffer);

                            using var featureN = featureClass.CreateRow(buffer);
                            var name = featureN.UID();

                            if (FeatureRelations.Instance.HasSlaves(globalid)) {
                                relatedEquipment!.CreateRelatedPointEquipment(new DangersP(current), instance, featureN, instance.scaleMinimum);
                            }
                            ConversionAnalytics.Instance.AddConverted(tableName, globalid, name);
                            Logger.Current.DataObject(objectid, tableName, longname, System.Text.Json.JsonSerializer.Serialize(instance, ImporterNIS.jsonSerializerOptions));
                        }
                        break;
                    case "dangersl::1": { // FSHFAC_FishingFacility
                            var instance = (FishingFacility)ImporterNIS.Build("FSHFAC", feature, buffer);

                            using var featureN = featureClass.CreateRow(buffer);
                            var name = featureN.UID();

                            if (FeatureRelations.Instance.HasSlaves(globalid)) {
                                relatedEquipment!.CreateRelatedLineEquipment(current, instance, featureN);
                            }
                            ConversionAnalytics.Instance.AddConverted(tableName, globalid, name);
                            Logger.Current.DataObject(objectid, tableName, longname, System.Text.Json.JsonSerializer.Serialize(instance, ImporterNIS.jsonSerializerOptions));
                        }
                        break;
                    case "dangersa::1": { // CTNARE_CautionArea
                            var instance = (CautionArea)ImporterNIS.Build("CTNARE", feature, buffer);

                            using var featureN = featureClass.CreateRow(buffer);
                            var name = featureN.UID();

                            if (FeatureRelations.Instance.HasSlaves(globalid)) {
                                relatedEquipment!.CreateRelatedAreaEquipment(current, instance, featureN, instance.scaleMinimum);
                            }
                            ConversionAnalytics.Instance.AddConverted(tableName, globalid, name);
                            Logger.Current.DataObject(objectid, tableName, longname, System.Text.Json.JsonSerializer.Serialize(instance, ImporterNIS.jsonSerializerOptions));
                        }
                        break;
                    case "dangersl::5": { // OBSTRN_Obstruction
                            //if (objectid == 5) System.Diagnostics.Debugger.Break();
                            var instance = ImporterNIS.OBSTRN(feature, buffer, filter);

                            using var featureN = featureClass.CreateRow(buffer);
                            var name = featureN.UID();

                            if (FeatureRelations.Instance.HasSlaves(globalid)) {
                                if (instance is Dam dam)
                                    relatedEquipment!.CreateRelatedLineEquipment(current, dam, featureN);
                                if (instance is FoulGround foulGround)
                                    relatedEquipment!.CreateRelatedLineEquipment(current, foulGround, featureN);
                                if (instance is Obstruction obstruction)
                                    relatedEquipment!.CreateRelatedLineEquipment(current, obstruction, featureN);
                            }
                            ConversionAnalytics.Instance.AddConverted(tableName, globalid, name);
                            Logger.Current.DataObject(objectid, tableName, longname, System.Text.Json.JsonSerializer.Serialize(instance, ImporterNIS.jsonSerializerOptions));
                        }
                        break;
                    case "dangersp::10": { // FSHFAC Fishing facilities
                            var instance = (FishingFacility)ImporterNIS.Build("FSHFAC", feature, buffer);

                            using var featureN = featureClass.CreateRow(buffer);
                            var name = featureN.UID();

                            if (FeatureRelations.Instance.HasSlaves(globalid)) {
                                relatedEquipment!.CreateRelatedPointEquipment(new DangersP(current), instance, featureN, instance.scaleMinimum);
                            }
                            ConversionAnalytics.Instance.AddConverted(tableName, globalid, name);
                            Logger.Current.DataObject(objectid, tableName, longname, System.Text.Json.JsonSerializer.Serialize(instance, ImporterNIS.jsonSerializerOptions));
                        }
                        break;
                    case "dangersl::10": { // OILBAR_OilBarrier
                            var instance = (OilBarrier)ImporterNIS.Build("OILBAR", feature, buffer);

                            using var featureN = featureClass.CreateRow(buffer);
                            var name = featureN.UID();

                            if (FeatureRelations.Instance.HasSlaves(globalid)) {
                                relatedEquipment!.CreateRelatedLineEquipment(current, instance, featureN);
                            }
                            ConversionAnalytics.Instance.AddConverted(tableName, globalid, name);
                            Logger.Current.DataObject(objectid, tableName, longname, System.Text.Json.JsonSerializer.Serialize(instance, ImporterNIS.jsonSerializerOptions));
                        }
                        break;
                    case "dangersa::10": { // FSHFAC_FishingFacility
                            var instance = (FishingFacility)ImporterNIS.Build("FSHFAC", feature, buffer);

                            using var featureN = featureClass.CreateRow(buffer);
                            var name = featureN.UID();

                            if (FeatureRelations.Instance.HasSlaves(globalid)) {
                                relatedEquipment!.CreateRelatedAreaEquipment(current, instance, featureN, instance.scaleMinimum);
                            }
                            ConversionAnalytics.Instance.AddConverted(tableName, globalid, name);
                            Logger.Current.DataObject(objectid, tableName, longname, System.Text.Json.JsonSerializer.Serialize(instance, ImporterNIS.jsonSerializerOptions));
                        }
                        break;
                    case "dangersl::15": { // WATTUR_WaterTurbulence
                            var instance = (WaterTurbulence)ImporterNIS.Build("WATTUR", feature, buffer);

                            using var featureN = featureClass.CreateRow(buffer);
                            var name = featureN.UID();

                            if (FeatureRelations.Instance.HasSlaves(globalid)) {
                                relatedEquipment!.CreateRelatedLineEquipment(current, instance, featureN);
                            }
                            ConversionAnalytics.Instance.AddConverted(tableName, globalid, name);
                            Logger.Current.DataObject(objectid, tableName, longname, System.Text.Json.JsonSerializer.Serialize(instance, ImporterNIS.jsonSerializerOptions));
                        }
                        break;
                    case "dangersa::15": { // OBSTRN_Obstruction                            
                            var instance = ImporterNIS.OBSTRN(feature, buffer, filter);

                            using var featureN = featureClass.CreateRow(buffer);
                            var name = featureN.UID();

                            if (FeatureRelations.Instance.HasSlaves(globalid)) {
                                if (instance is Dam dam)
                                    relatedEquipment!.CreateRelatedAreaEquipment(current, dam, featureN, dam.scaleMinimum);
                                if (instance is FoulGround foulGround)
                                    relatedEquipment!.CreateRelatedAreaEquipment(current, foulGround, featureN, foulGround.scaleMinimum);
                                if (instance is Obstruction obstruction)
                                    relatedEquipment!.CreateRelatedAreaEquipment(current, obstruction, featureN, obstruction.scaleMinimum);
                            }
                            ConversionAnalytics.Instance.AddConverted(tableName, globalid, name);
                            Logger.Current.DataObject(objectid, tableName, longname, System.Text.Json.JsonSerializer.Serialize(instance, ImporterNIS.jsonSerializerOptions));
                        }
                        break;
                    case "dangersp::20": { // OBSTRN_Obstruction
                            var instance = ImporterNIS.OBSTRN(feature, buffer, filter);

                            using var featureN = featureClass.CreateRow(buffer);
                            var name = featureN.UID();

                            if (FeatureRelations.Instance.HasSlaves(globalid)) {
                                if (instance is Dam dam)
                                    relatedEquipment!.CreateRelatedPointEquipment(new DangersP(current), dam, featureN, dam.scaleMinimum);
                                if (instance is FoulGround foulGround)
                                    relatedEquipment!.CreateRelatedPointEquipment(new DangersP(current), foulGround, featureN, foulGround.scaleMinimum);
                                if (instance is Obstruction obstruction)
                                    relatedEquipment!.CreateRelatedPointEquipment(new DangersP(current), obstruction, featureN, obstruction.scaleMinimum);
                            }
                            ConversionAnalytics.Instance.AddConverted(tableName, globalid, name);
                            Logger.Current.DataObject(objectid, tableName, longname, System.Text.Json.JsonSerializer.Serialize(instance, ImporterNIS.jsonSerializerOptions));
                        }
                        break;
                    case "dangersa::20": { // WATTUR_WaterTurbulence
                            var instance = (WaterTurbulence)ImporterNIS.Build("WATTUR", feature, buffer);

                            using var featureN = featureClass.CreateRow(buffer);
                            var name = featureN.UID();

                            if (FeatureRelations.Instance.HasSlaves(globalid)) {
                                relatedEquipment!.CreateRelatedAreaEquipment(current, instance, featureN, instance.scaleMinimum);
                            }
                            ConversionAnalytics.Instance.AddConverted(tableName, globalid, name);
                            Logger.Current.DataObject(objectid, tableName, longname, System.Text.Json.JsonSerializer.Serialize(instance, ImporterNIS.jsonSerializerOptions));
                        }
                        break;
                    case "dangersa::25": { // WRECKS_Wreck
                            var instance = ImporterNIS.WRECKS(feature, buffer, filter);

                            using var featureN = featureClass.CreateRow(buffer);
                            var name = featureN.UID();

                            if (FeatureRelations.Instance.HasSlaves(globalid)) {
                                relatedEquipment!.CreateRelatedAreaEquipment(current, instance, featureN, instance.scaleMinimum);
                            }
                            ConversionAnalytics.Instance.AddConverted(tableName, globalid, name);
                            Logger.Current.DataObject(objectid, tableName, longname, System.Text.Json.JsonSerializer.Serialize(instance, ImporterNIS.jsonSerializerOptions));
                        }
                        break;
                    case "dangersp::35": { // UWTROC_UnderwaterAwashRock
                            var instance = (UnderwaterAwashRock)ImporterNIS.UWTROC(feature, buffer, filter);

                            using var featureN = featureClass.CreateRow(buffer);
                            var name = featureN.UID();

                            if (FeatureRelations.Instance.HasSlaves(globalid)) {
                                relatedEquipment!.CreateRelatedPointEquipment(new DangersP(current), instance, featureN, instance.scaleMinimum);
                            }
                            ConversionAnalytics.Instance.AddConverted(tableName, globalid, name);
                            Logger.Current.DataObject(objectid, tableName, longname, System.Text.Json.JsonSerializer.Serialize(instance, ImporterNIS.jsonSerializerOptions));

                        }
                        break;
                    case "dangersp::40": { // WATTUR_WaterTurbulence
                            var instance = (WaterTurbulence)ImporterNIS.Build("WATTUR", feature, buffer);

                            using var featureN = featureClass.CreateRow(buffer);
                            var name = featureN.UID();

                            if (FeatureRelations.Instance.HasSlaves(globalid)) {
                                relatedEquipment!.CreateRelatedPointEquipment(new DangersP(current), instance, featureN, instance.scaleMinimum);
                            }
                            ConversionAnalytics.Instance.AddConverted(tableName, globalid, name);
                            Logger.Current.DataObject(objectid, tableName, longname, System.Text.Json.JsonSerializer.Serialize(instance, ImporterNIS.jsonSerializerOptions));
                        }
                        break;
                    case "dangersp::45": { // WRECKS_Wreck
                            var instance = ImporterNIS.WRECKS(feature, buffer, filter);

                            using var featureN = featureClass.CreateRow(buffer);
                            var name = featureN.UID();

                            if (FeatureRelations.Instance.HasSlaves(globalid)) {
                                relatedEquipment!.CreateRelatedPointEquipment(new DangersP(current), instance, featureN, instance.scaleMinimum);
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
