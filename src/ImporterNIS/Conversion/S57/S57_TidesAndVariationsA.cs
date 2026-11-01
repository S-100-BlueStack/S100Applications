using ArcGIS.Core.Data;
using S100FC;
using S100FC.S101.FeatureTypes;
using S100Framework.Applications.S57.esri;
using S100Framework.Applications.Singletons;

namespace S100Framework.Applications
{
    internal static partial class ImporterNIS
    {
        private static void S57_TidesAndVariationsA(Geodatabase source, Geodatabase target, QueryFilter filter) {
            var tableName = "TidesAndVariationsA";

            using var tidesAndVariationsA = source.OpenDataset<FeatureClass>(source.GetName(tableName));
            Subtypes.Instance.RegisterSubtypes(tidesAndVariationsA);

            using var featureClass = target.OpenDataset<FeatureClass>(target.GetName("surface"));


            using var buffer = featureClass.CreateRowBuffer();

            using var cursor = tidesAndVariationsA.Search(filter, true);
            int recordCount = 0;

            while (cursor.MoveNext()) {
                recordCount += 1;

                var feature = (Feature)cursor.Current;

                if (feature.GetShape() is null) continue;
                if (feature.GetShape().IsEmpty) continue;

                var current = new TidesAndVariationsA(feature);

                var objectid = current.OBJECTID ?? default;
                var globalid = current.GLOBALID;

                if (FeatureRelations.Instance.IsSlave(globalid)) {
                    continue;
                }

                if (ConversionAnalytics.Instance.IsConverted(globalid)) {
                    throw new Exception("Ups. Not supported");
                }



                var fcSubtype = current.FCSUBTYPE ?? default;
                var plts_comp_scale = current.PLTS_COMP_SCALE ?? default;
                var longname = current.LNAM ?? Strings.UNKNOWN;
                var status = current.STATUS ?? default;

                switch (fcSubtype) {
                    case 5: { // LOCMAG_LocalMagneticAnomaly
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
                    case 10: { // MAGVAR_MagneticVariation
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
                    case 15: { // T_HMON_TideHarmonicPrediction
                            throw new NotImplementedException("No T_HMON_TideHarmonicPrediction in DK | GL NIS");
                        }
                    case 20: { // T_NHMN_TideNonHarmonicPrediction
                            throw new NotImplementedException("No T_NHMN_TideNonHarmonicPrediction in DK | GL NIS");
                        }
                    case 25: { // T_TIMS_TideTimeSeries
                            throw new NotImplementedException("No T_TIMS_TideTimeSeries in DK | GL NIS");
                        }
                    case 30: { // TIDEWY_Tideway
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
                    case 35: { // TS_FEB_TidalStreamFloodEbb
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
                    case 40: { // TS_PAD_TidalStreamPanelData
                            throw new NotImplementedException("No TS_PAD_TidalStreamPanelData in DK | GL NIS");
                        }
                    case 45: { // TS_PNH_TidalStreamNonHarmonicPrediction
                            throw new NotImplementedException("No TS_PNH_TidalStreamNonHarmonicPrediction in DK | GL NIS");
                        }
                    case 50: { // TS_PRH_TidalStreamHarmonicPrediction
                            throw new NotImplementedException("No TS_PRH_TidalStreamHarmonicPrediction in DK | GL NIS");
                        }
                    case 55: { // TS_TIS_TidalStreamTimeSeries
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
