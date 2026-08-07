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
        private static void S57_TidesAndVariations(string tableName, Func<string, FeatureClass> source, QueryFilter filter, Func<FeatureClass> target, Action<RowBuffer, Geometry> setShape) {
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
                    case "tidesandvariationsp::1": { // CURENT_CurrentNonGravitational
                            var instance = (CurrentNonGravitational)ImporterNIS.Build("CURENT", feature, buffer);

                            using var featureN = featureClass.CreateRow(buffer);
                            var name = featureN.UID();

                            if (FeatureRelations.Instance.HasSlaves(globalid)) {
                                relatedEquipment!.CreateRelatedPointEquipment(new TidesAndVariationsP(current), instance, featureN, instance.scaleMinimum);
                            }
                            ConversionAnalytics.Instance.AddConverted(tableName, globalid, name);
                            Logger.Current.DataObject(objectid, tableName, longname, System.Text.Json.JsonSerializer.Serialize(instance, ImporterNIS.jsonSerializerOptions));
                        }
                        break;
                    case "tidesandvariationsp::5": { // LOCMAG_LocalMagneticAnomaly
                            var instance = (LocalMagneticAnomaly)ImporterNIS.Build("LOCMAG", feature, buffer);

                            using var featureN = featureClass.CreateRow(buffer);
                            var name = featureN.UID();

                            if (FeatureRelations.Instance.HasSlaves(globalid)) {
                                relatedEquipment!.CreateRelatedPointEquipment(new TidesAndVariationsP(current), instance, featureN, instance.scaleMinimum);
                            }
                            ConversionAnalytics.Instance.AddConverted(tableName, globalid, name);
                            Logger.Current.DataObject(objectid, tableName, longname, System.Text.Json.JsonSerializer.Serialize(instance, ImporterNIS.jsonSerializerOptions));
                        }
                        break;
                    case "tidesandvariationsl::5": { // LOCMAG_LocalMagneticAnomaly
                            throw new NotImplementedException("No MAGVAR_MagneticVariation in DK | GL NIS");
                        }
                    case "tidesandvariationsa::5": { // LOCMAG_LocalMagneticAnomaly
                            var instance = (LocalMagneticAnomaly)ImporterNIS.Build("LOCMAG", feature, buffer);

                            using var featureN = featureClass.CreateRow(buffer);
                            var name = featureN.UID();

                            if (FeatureRelations.Instance.HasSlaves(globalid)) {
                                relatedEquipment!.CreateRelatedAreaEquipment(current, instance, featureN, instance.scaleMinimum);
                            }
                            ConversionAnalytics.Instance.AddConverted(tableName, globalid, name);
                            Logger.Current.DataObject(objectid, tableName, longname, System.Text.Json.JsonSerializer.Serialize(instance, ImporterNIS.jsonSerializerOptions));
                        }
                        break;
                    case "tidesandvariationsp::10": { // MAGVAR_MagneticVariation
                            var instance = (MagneticVariation)ImporterNIS.Build("MAGVAR", feature, buffer);

                            using var featureN = featureClass.CreateRow(buffer);
                            var name = featureN.UID();

                            if (FeatureRelations.Instance.HasSlaves(globalid)) {
                                relatedEquipment!.CreateRelatedPointEquipment(new TidesAndVariationsP(current), instance, featureN, instance.scaleMinimum);
                            }
                            ConversionAnalytics.Instance.AddConverted(tableName, globalid, name);
                            Logger.Current.DataObject(objectid, tableName, longname, System.Text.Json.JsonSerializer.Serialize(instance, ImporterNIS.jsonSerializerOptions));
                        }
                        break;
                    case "tidesandvariationsl::10": { // MAGVAR_MagneticVariation
                            throw new NotImplementedException("No MAGVAR_MagneticVariation in DK | GL NIS");
                        }
                    case "tidesandvariationsa::10": { // MAGVAR_MagneticVariation
                            var instance = (MagneticVariation)ImporterNIS.Build("MAGVAR", feature, buffer);

                            using var featureN = featureClass.CreateRow(buffer);
                            var name = featureN.UID();

                            if (FeatureRelations.Instance.HasSlaves(globalid)) {
                                relatedEquipment!.CreateRelatedAreaEquipment(current, instance, featureN, instance.scaleMinimum);
                            }
                            ConversionAnalytics.Instance.AddConverted(tableName, globalid, name);
                            Logger.Current.DataObject(objectid, tableName, longname, System.Text.Json.JsonSerializer.Serialize(instance, ImporterNIS.jsonSerializerOptions));
                        }
                        break;
                    case "tidesandvariationsp::15": { // T_HMON_TideHarmonicPrediction
                            throw new NotImplementedException("No T_HMON_TideHarmonicPrediction in DK | GL NIS");
                        }
                    case "tidesandvariationsl::15": { // TIDEWY_Tideway
                            var instance = (Tideway)ImporterNIS.Build("TIDEWY", feature, buffer);

                            using var featureN = featureClass.CreateRow(buffer);
                            var name = featureN.UID();

                            if (FeatureRelations.Instance.HasSlaves(globalid)) {
                                relatedEquipment!.CreateRelatedLineEquipment(current, instance, featureN);
                            }
                            ConversionAnalytics.Instance.AddConverted(tableName, globalid, name);
                            Logger.Current.DataObject(objectid, tableName, longname, System.Text.Json.JsonSerializer.Serialize(instance, ImporterNIS.jsonSerializerOptions));
                        }
                        break;
                    case "tidesandvariationsa::15": { // T_HMON_TideHarmonicPrediction
                            throw new NotImplementedException("No T_HMON_TideHarmonicPrediction in DK | GL NIS");
                        }
                    case "tidesandvariationsp::20": { // T_NHMN_TideNonHarmonicPrediction
                            throw new NotImplementedException("No T_NHMN_TideNonHarmonicPrediction in DK | GL NIS");
                        }
                    case "tidesandvariationsa::20": { // T_NHMN_TideNonHarmonicPrediction
                            throw new NotImplementedException("No T_NHMN_TideNonHarmonicPrediction in DK | GL NIS");
                        }
                    case "tidesandvariationsp::25": { // T_TIMS_TideTimeSeries
                            throw new NotImplementedException("No T_TIMS_TideTimeSeries in DK | GL NIS");
                        }
                    case "tidesandvariationsa::25": { // T_TIMS_TideTimeSeries
                            throw new NotImplementedException("No T_TIMS_TideTimeSeries in DK | GL NIS");
                        }
                    case "tidesandvariationsp::30": { // TS_FEB_TidalStreamFloodEbb
                            var instance = (TidalStreamFloodEbb)ImporterNIS.Build("TS_FEB", feature, buffer);

                            using var featureN = featureClass.CreateRow(buffer);
                            var name = featureN.UID();

                            if (FeatureRelations.Instance.HasSlaves(globalid)) {
                                relatedEquipment!.CreateRelatedPointEquipment(new TidesAndVariationsP(current), instance, featureN, instance.scaleMinimum);
                            }
                            ConversionAnalytics.Instance.AddConverted(tableName, globalid, name);
                            Logger.Current.DataObject(objectid, tableName, longname, System.Text.Json.JsonSerializer.Serialize(instance, ImporterNIS.jsonSerializerOptions));
                        }
                        break;
                    case "tidesandvariationsa::30": { // TIDEWY_Tideway
                            var instance = (Tideway)ImporterNIS.Build("TIDEWY", feature, buffer);

                            using var featureN = featureClass.CreateRow(buffer);
                            var name = featureN.UID();

                            if (FeatureRelations.Instance.HasSlaves(globalid)) {
                                relatedEquipment!.CreateRelatedAreaEquipment(current, instance, featureN, instance.scaleMinimum);
                            }
                            ConversionAnalytics.Instance.AddConverted(tableName, globalid, name);
                            Logger.Current.DataObject(objectid, tableName, longname, System.Text.Json.JsonSerializer.Serialize(instance, ImporterNIS.jsonSerializerOptions));
                        }
                        break;
                    case "tidesandvariationsp::35": { // TS_PAD_TidalStreamPanelData
                            var instance = (TidalStreamPanelData)ImporterNIS.Build("TS_PAD", feature, buffer);

                            using var featureN = featureClass.CreateRow(buffer);
                            var name = featureN.UID();

                            if (FeatureRelations.Instance.HasSlaves(globalid)) {
                                relatedEquipment!.CreateRelatedPointEquipment(new TidesAndVariationsP(current), instance, featureN, instance.scaleMinimum);
                            }
                            ConversionAnalytics.Instance.AddConverted(tableName, globalid, name);
                            Logger.Current.DataObject(objectid, tableName, longname, System.Text.Json.JsonSerializer.Serialize(instance, ImporterNIS.jsonSerializerOptions));
                        }
                        break;
                    case "tidesandvariationsa::35": { // TS_FEB_TidalStreamFloodEbb
                            var instance = (TidalStreamFloodEbb)ImporterNIS.Build("TS_FEB", feature, buffer);

                            using var featureN = featureClass.CreateRow(buffer);
                            var name = featureN.UID();

                            if (FeatureRelations.Instance.HasSlaves(globalid)) {
                                relatedEquipment!.CreateRelatedAreaEquipment(current, instance, featureN, instance.scaleMinimum);
                            }
                            ConversionAnalytics.Instance.AddConverted(tableName, globalid, name);
                            Logger.Current.DataObject(objectid, tableName, longname, System.Text.Json.JsonSerializer.Serialize(instance, ImporterNIS.jsonSerializerOptions));
                        }
                        break;
                    case "tidesandvariationsp::40": { // TS_PNH_TidalStreamNonHarmonicPrediction
                            //  Will not convert to S-101.
                        }
                        break;
                    case "tidesandvariationsa::40": { // TS_PAD_TidalStreamPanelData
                            throw new NotImplementedException("No TS_PAD_TidalStreamPanelData in DK | GL NIS");
                        }
                    case "tidesandvariationsp::45": { // TS_PRH_TidalStreamHarmonicPrediction
                            //  Will not convert to S-101.
                        }
                        break;
                    case "tidesandvariationsa::45": { // TS_PNH_TidalStreamNonHarmonicPrediction
                            throw new NotImplementedException("No TS_PNH_TidalStreamNonHarmonicPrediction in DK | GL NIS");
                        }
                    case "tidesandvariationsp::50": { // TS_TIS_TidalStreamTimeSeries
                            //  Will not convert to S-101.
                        }
                        break;
                    case "tidesandvariationsa::50": { // TS_PRH_TidalStreamHarmonicPrediction
                            throw new NotImplementedException("No TS_PRH_TidalStreamHarmonicPrediction in DK | GL NIS");
                        }
                    case "tidesandvariationsa::55": { // TS_TIS_TidalStreamTimeSeries
                            throw new NotImplementedException("No TS_TIS_TidalStreamTimeSeries in DK | GL NIS");
                        }
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
