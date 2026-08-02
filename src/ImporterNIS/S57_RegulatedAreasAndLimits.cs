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
        private static void S57_RegulatedAreasAndLimits(string tableName, Func<string, FeatureClass> source, QueryFilter filter, Func<FeatureClass> target, Action<RowBuffer, Geometry> setShape) {
            using var dataset = source(tableName);   // source.OpenDataset<FeatureClass>(source.GetName(tableName));
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
                    // ACHARE_AnchorageArea
                    case "regulatedareasandlimitsp::1": {
                            var instance = ImporterNIS.Build("ACHARE", feature, buffer);

                            using var featureN = featureClass.CreateRow(buffer);
                            var name = featureN.UID();

                            if (FeatureRelations.Instance.HasSlaves(globalid)) {
                                if (instance is MooringArea mooringArea)
                                    relatedEquipment!.CreateRelatedPointEquipment(new RegulatedAreasAndLimitsP(current), instance, featureN, mooringArea.scaleMinimum);
                                if (instance is AnchorageArea anchorageArea)
                                    relatedEquipment!.CreateRelatedPointEquipment(new RegulatedAreasAndLimitsP(current), instance, featureN, anchorageArea.scaleMinimum);
                            }
                            ConversionAnalytics.Instance.AddConverted(tableName, globalid, name);
                            Logger.Current.DataObject(objectid, tableName, longname, System.Text.Json.JsonSerializer.Serialize(instance, ImporterNIS.jsonSerializerOptions));
                        }
                        break;
                    case "regulatedareasandlimitsa::1": { 
                            var instance = ImporterNIS.Build("ACHARE", feature, buffer);

                            using var featureN = featureClass.CreateRow(buffer);
                            var name = featureN.UID();

                            if (FeatureRelations.Instance.HasSlaves(globalid)) {
                                if (instance is MooringArea mooringArea)
                                    relatedEquipment!.CreateRelatedAreaEquipment(current, instance, featureN, mooringArea.scaleMinimum);
                                if (instance is AnchorageArea anchorageArea)
                                    relatedEquipment!.CreateRelatedAreaEquipment(current, instance, featureN, anchorageArea.scaleMinimum);
                            }
                            ConversionAnalytics.Instance.AddConverted(tableName, globalid, name);
                            Logger.Current.DataObject(objectid, tableName, longname, System.Text.Json.JsonSerializer.Serialize(instance, ImporterNIS.jsonSerializerOptions));
                        }
                        break;
                    case "regulatedareasandlimitsl::1": { // ASLXIS_ArchipelagicSeaLaneAxis
                            throw new NotImplementedException($"No ASLXIS_ArchipelagicSeaLaneAxis in DK or GL. {tableName}");
                        }
                    // ACHBRT_AnchorBerth
                    case "regulatedareasandlimitsp::5": { // ACHBRT_AnchorBerth
                            var instance = (AnchorBerth)ImporterNIS.Build("ACHBRT", feature, buffer);

                            using var featureN = featureClass.CreateRow(buffer);
                            var name = featureN.UID();

                            if (FeatureRelations.Instance.HasSlaves(globalid)) {
                                relatedEquipment!.CreateRelatedPointEquipment(new RegulatedAreasAndLimitsP(current), instance, featureN, instance.scaleMinimum);
                            }
                            ConversionAnalytics.Instance.AddConverted(tableName, globalid, name);
                            Logger.Current.DataObject(objectid, tableName, longname, System.Text.Json.JsonSerializer.Serialize(instance, ImporterNIS.jsonSerializerOptions));
                        }
                        break;
                    case "regulatedareasandlimitsa::5": { // ACHBRT_AnchorBerth
                            var instance = (AnchorBerth)ImporterNIS.Build("ACHBRT", feature, buffer);

                            using var featureN = featureClass.CreateRow(buffer);
                            var name = featureN.UID();

                            if (FeatureRelations.Instance.HasSlaves(globalid)) {
                                relatedEquipment!.CreateRelatedAreaEquipment(current, instance, featureN, instance.scaleMinimum);
                            }
                            ConversionAnalytics.Instance.AddConverted(tableName, globalid, name);
                            Logger.Current.DataObject(objectid, tableName, longname, System.Text.Json.JsonSerializer.Serialize(instance, ImporterNIS.jsonSerializerOptions));
                        }
                        break;
                    case "regulatedareasandlimitsp::10": { // CTSARE_CargoTranshipmentArea
                            var instance = (CargoTranshipmentArea)ImporterNIS.Build("CTSARE", feature, buffer);

                            using var featureN = featureClass.CreateRow(buffer);
                            var name = featureN.UID();

                            if (FeatureRelations.Instance.HasSlaves(globalid)) {
                                relatedEquipment!.CreateRelatedPointEquipment(new RegulatedAreasAndLimitsP(current), instance, featureN, instance.scaleMinimum);
                            }
                            ConversionAnalytics.Instance.AddConverted(tableName, globalid, name);
                            Logger.Current.DataObject(objectid, tableName, longname, System.Text.Json.JsonSerializer.Serialize(instance, ImporterNIS.jsonSerializerOptions));
                        }
                        break;
                    case "regulatedareasandlimitsp::15": { // DMPGRD_DumpingGround
                            var instance = (DumpingGround)ImporterNIS.Build("DMPGRD", feature, buffer);

                            using var featureN = featureClass.CreateRow(buffer);
                            var name = featureN.UID();

                            if (FeatureRelations.Instance.HasSlaves(globalid)) {
                                relatedEquipment!.CreateRelatedPointEquipment(new RegulatedAreasAndLimitsP(current), instance, featureN, instance.scaleMinimum);
                            }
                            ConversionAnalytics.Instance.AddConverted(tableName, globalid, name);
                            Logger.Current.DataObject(objectid, tableName, longname, System.Text.Json.JsonSerializer.Serialize(instance, ImporterNIS.jsonSerializerOptions));
                        }
                        break;
                    // ICNARE_IncinerationArea
                    case "regulatedareasandlimitsp::25":
                    case "regulatedareasandlimitsa::75": { 
                            //The S-57 Object class ICNARE will not be converted. 
                        }
                        break;
                    case "regulatedareasandlimitsp::30": { // LOGPON_LogPond
                            throw new NotImplementedException($"No LOGPON_LogPond in DK or GL. {tableName}");
                        }
                    case "regulatedareasandlimitsl::30": { // STSLNE_StraightTerritorialSeaBaseline
                            var instance = (StraightTerritorialSeaBaseline)ImporterNIS.Build("STSLNE", feature, buffer);

                            using var featureN = featureClass.CreateRow(buffer);
                            var name = featureN.UID();

                            if (FeatureRelations.Instance.HasSlaves(globalid)) {
                                relatedEquipment!.CreateRelatedAreaEquipment(current, instance, featureN, instance.scaleMinimum);
                            }
                            ConversionAnalytics.Instance.AddConverted(tableName, globalid, name);
                            Logger.Current.DataObject(objectid, tableName, longname, System.Text.Json.JsonSerializer.Serialize(instance, ImporterNIS.jsonSerializerOptions));
                        }
                        break;
                    // MARCUL_MarineFarmCulture
                    case "regulatedareasandlimitsp::35": { // MARCUL_MarineFarmCulture
                            var instance = (MarineFarmCulture)ImporterNIS.Build("MARCUL", feature, buffer);

                            using var featureN = featureClass.CreateRow(buffer);
                            var name = featureN.UID();

                            if (FeatureRelations.Instance.HasSlaves(globalid)) {
                                relatedEquipment!.CreateRelatedPointEquipment(new RegulatedAreasAndLimitsP(current), instance, featureN, instance.scaleMinimum);
                            }
                            ConversionAnalytics.Instance.AddConverted(tableName, globalid, name);
                            Logger.Current.DataObject(objectid, tableName, longname, System.Text.Json.JsonSerializer.Serialize(instance, ImporterNIS.jsonSerializerOptions));
                        }
                        break;
                    case "regulatedareasandlimitsl::35": { // MARCUL_MarineFarmCulture
                            var instance = (MarineFarmCulture)ImporterNIS.Build("MARCUL", feature, buffer);

                            using var featureN = featureClass.CreateRow(buffer);
                            var name = featureN.UID();

                            if (FeatureRelations.Instance.HasSlaves(globalid)) {
                                relatedEquipment!.CreateRelatedAreaEquipment(current, instance, featureN, instance.scaleMinimum);
                            }
                            ConversionAnalytics.Instance.AddConverted(tableName, globalid, name);
                            Logger.Current.DataObject(objectid, tableName, longname, System.Text.Json.JsonSerializer.Serialize(instance, ImporterNIS.jsonSerializerOptions));
                        }
                        break;
                    case "regulatedareasandlimitsp::40": { // SPLARE_SeaPlaneLandingArea
                            var instance = (SeaplaneLandingArea)ImporterNIS.Build("SPLARE", feature, buffer);

                            using var featureN = featureClass.CreateRow(buffer);
                            var name = featureN.UID();

                            if (FeatureRelations.Instance.HasSlaves(globalid)) {
                                relatedEquipment!.CreateRelatedPointEquipment(new RegulatedAreasAndLimitsP(current), instance, featureN, instance.scaleMinimum);
                            }
                            ConversionAnalytics.Instance.AddConverted(tableName, globalid, name);
                            Logger.Current.DataObject(objectid, tableName, longname, System.Text.Json.JsonSerializer.Serialize(instance, ImporterNIS.jsonSerializerOptions));
                        }
                        break;
                    case "regulatedareasandlimitsa::10": { // ADMARE_AdministrationAreaNamed                            
                            var instance = ImporterNIS.Build("ADMARE", feature, buffer);

                            using var featureN = featureClass.CreateRow(buffer);
                            var name = featureN.UID();

                            if (FeatureRelations.Instance.HasSlaves(globalid)) {
                                if (instance is AdministrationArea administrationArea)
                                    relatedEquipment!.CreateRelatedAreaEquipment(current, instance, featureN, administrationArea.scaleMinimum);
                                if (instance is PilotageDistrict pilotageDistrict)
                                    relatedEquipment!.CreateRelatedAreaEquipment(current, instance, featureN, pilotageDistrict.scaleMinimum);
                                if (instance is MarinePollutionRegulationsArea marinePollutionRegulationsArea)
                                    relatedEquipment!.CreateRelatedAreaEquipment(current, instance, featureN, marinePollutionRegulationsArea.scaleMinimum);
                                if (instance is VesselTrafficServiceArea vesselTrafficServiceArea)
                                    relatedEquipment!.CreateRelatedAreaEquipment(current, instance, featureN, vesselTrafficServiceArea.scaleMinimum);
                            }
                            ConversionAnalytics.Instance.AddConverted(tableName, globalid, name);
                            Logger.Current.DataObject(objectid, tableName, longname, System.Text.Json.JsonSerializer.Serialize(instance, ImporterNIS.jsonSerializerOptions));
                        }
                        break;
                    case "regulatedareasandlimitsa::15": { // ARCSLN_ArchipelagicSeaLane
                            throw new NotImplementedException($"No ARCSLN_ArchipelagicSeaLane in DK or GL. {tableName}");
                        }
                    case "regulatedareasandlimitsa::20": { // CONZNE_ContiguousZone
                            var instance = (ContiguousZone)ImporterNIS.Build("CONZNE", feature, buffer);

                            using var featureN = featureClass.CreateRow(buffer);
                            var name = featureN.UID();

                            if (FeatureRelations.Instance.HasSlaves(globalid)) {
                                relatedEquipment!.CreateRelatedAreaEquipment(current, instance, featureN, instance.scaleMinimum);
                            }
                            ConversionAnalytics.Instance.AddConverted(tableName, globalid, name);
                            Logger.Current.DataObject(objectid, tableName, longname, System.Text.Json.JsonSerializer.Serialize(instance, ImporterNIS.jsonSerializerOptions));
                        }
                        break;
                    case "regulatedareasandlimitsa::25": { // COSARE_ContinentalShelfArea
                            var instance = (ContinentalShelfArea)ImporterNIS.Build("COSARE", feature, buffer);

                            using var featureN = featureClass.CreateRow(buffer);
                            var name = featureN.UID();

                            if (FeatureRelations.Instance.HasSlaves(globalid)) {
                                relatedEquipment!.CreateRelatedAreaEquipment(current, instance, featureN, instance.scaleMinimum);
                            }
                            ConversionAnalytics.Instance.AddConverted(tableName, globalid, name);
                            Logger.Current.DataObject(objectid, tableName, longname, System.Text.Json.JsonSerializer.Serialize(instance, ImporterNIS.jsonSerializerOptions));
                        }
                        break;

                    case "regulatedareasandlimitsa::30": { // CTSARE_CargoTranshipmentArea
                            var instance = (CargoTranshipmentArea)ImporterNIS.Build("CTSARE", feature, buffer);

                            using var featureN = featureClass.CreateRow(buffer);
                            var name = featureN.UID();

                            if (FeatureRelations.Instance.HasSlaves(globalid)) {
                                relatedEquipment!.CreateRelatedAreaEquipment(current, instance, featureN, instance.scaleMinimum);
                            }
                            ConversionAnalytics.Instance.AddConverted(tableName, globalid, name);
                            Logger.Current.DataObject(objectid, tableName, longname, System.Text.Json.JsonSerializer.Serialize(instance, ImporterNIS.jsonSerializerOptions));
                        }
                        break;
                    case "regulatedareasandlimitsa::35": { // CUSZNE_CustomZone
                            throw new NotImplementedException($"No CUSZNE_CustomZone in DK or GL. {tableName}");
                        }
                    case "regulatedareasandlimitsa::40": { // DMPGRD_DumpingGround
                            var instance = (DumpingGround)ImporterNIS.Build("DMPGRD", feature, buffer);

                            using var featureN = featureClass.CreateRow(buffer);
                            var name = featureN.UID();

                            if (FeatureRelations.Instance.HasSlaves(globalid)) {
                                relatedEquipment!.CreateRelatedAreaEquipment(current, instance, featureN, instance.scaleMinimum);
                            }
                            ConversionAnalytics.Instance.AddConverted(tableName, globalid, name);
                            Logger.Current.DataObject(objectid, tableName, longname, System.Text.Json.JsonSerializer.Serialize(instance, ImporterNIS.jsonSerializerOptions));
                        }
                        break;
                    case "regulatedareasandlimitsa::50": { // EXEZNE_ExclusiveEconomicZone
                            var instance = (ExclusiveEconomicZone)ImporterNIS.Build("EXEZNE", feature, buffer);

                            using var featureN = featureClass.CreateRow(buffer);
                            var name = featureN.UID();

                            if (FeatureRelations.Instance.HasSlaves(globalid)) {
                                relatedEquipment!.CreateRelatedAreaEquipment(current, instance, featureN, instance.scaleMinimum);
                            }
                            ConversionAnalytics.Instance.AddConverted(tableName, globalid, name);
                            Logger.Current.DataObject(objectid, tableName, longname, System.Text.Json.JsonSerializer.Serialize(instance, ImporterNIS.jsonSerializerOptions));
                        }
                        break;
                    case "regulatedareasandlimitsa::55": { // FRPARE_FreePortArea
                            throw new NotImplementedException($"No FRPARE_FreePortArea in DK or GL. {tableName}");
                        }
                    case "regulatedareasandlimitsa::60": { // FSHGRD_FishingGround
                            throw new NotImplementedException($"No FSHGRD_FishingGround in DK or GL. {tableName}");
                        }
                    case "regulatedareasandlimitsa::65": { // FSHZNE_FisheryZone
                            var instance = (FisheryZone)ImporterNIS.Build("FSHZNE", feature, buffer);

                            using var featureN = featureClass.CreateRow(buffer);
                            var name = featureN.UID();

                            if (FeatureRelations.Instance.HasSlaves(globalid)) {
                                relatedEquipment!.CreateRelatedAreaEquipment(current, instance, featureN, instance.scaleMinimum);
                            }
                            ConversionAnalytics.Instance.AddConverted(tableName, globalid, name);
                            Logger.Current.DataObject(objectid, tableName, longname, System.Text.Json.JsonSerializer.Serialize(instance, ImporterNIS.jsonSerializerOptions));
                        }
                        break;
                    case "regulatedareasandlimitsa::70": { // HRBARE_HarbourAreaAdministrative
                            var instance = (HarbourAreaAdministrative)ImporterNIS.Build("HRBARE", feature, buffer);

                            using var featureN = featureClass.CreateRow(buffer);
                            var name = featureN.UID();

                            if (FeatureRelations.Instance.HasSlaves(globalid)) {
                                relatedEquipment!.CreateRelatedAreaEquipment(current, instance, featureN, instance.scaleMinimum);
                            }
                            ConversionAnalytics.Instance.AddConverted(tableName, globalid, name);
                            Logger.Current.DataObject(objectid, tableName, longname, System.Text.Json.JsonSerializer.Serialize(instance, ImporterNIS.jsonSerializerOptions));
                        }
                        break;
                    case "regulatedareasandlimitsa::85": { // LOGPON_LogPond
                            throw new NotImplementedException($"No LOGPON_LogPond in DK or GL. {tableName}");
                        }

                    case "regulatedareasandlimitsa::95": { // MARCUL_MarineFarmCulture
                            var instance = (MarineFarmCulture)ImporterNIS.Build("MARCUL", feature, buffer);

                            using var featureN = featureClass.CreateRow(buffer);
                            var name = featureN.UID();

                            if (FeatureRelations.Instance.HasSlaves(globalid)) {
                                relatedEquipment!.CreateRelatedAreaEquipment(current, instance, featureN, instance.scaleMinimum);
                            }
                            ConversionAnalytics.Instance.AddConverted(tableName, globalid, name);
                            Logger.Current.DataObject(objectid, tableName, longname, System.Text.Json.JsonSerializer.Serialize(instance, ImporterNIS.jsonSerializerOptions));
                        }
                        break;
                    case "regulatedareasandlimitsa::105": { // RESARE_RestrictedArea
                            var instance = (RestrictedArea)ImporterNIS.Build("RESARE", feature, buffer);

                            using var featureN = featureClass.CreateRow(buffer);
                            var name = featureN.UID();

                            if (FeatureRelations.Instance.HasSlaves(globalid)) {
                                relatedEquipment!.CreateRelatedAreaEquipment(current, instance, featureN, instance.scaleMinimum);
                            }
                            ConversionAnalytics.Instance.AddConverted(tableName, globalid, name);
                            Logger.Current.DataObject(objectid, tableName, longname, System.Text.Json.JsonSerializer.Serialize(instance, ImporterNIS.jsonSerializerOptions));
                        }
                        break;
                    case "regulatedareasandlimitsa::110": { // SPLARE_SeaplaneLandingArea
                            var instance = (SeaplaneLandingArea)ImporterNIS.Build("SPLARE", feature, buffer);

                            using var featureN = featureClass.CreateRow(buffer);
                            var name = featureN.UID();

                            if (FeatureRelations.Instance.HasSlaves(globalid)) {
                                relatedEquipment!.CreateRelatedAreaEquipment(current, instance, featureN, instance.scaleMinimum);
                            }
                            ConversionAnalytics.Instance.AddConverted(tableName, globalid, name);
                            Logger.Current.DataObject(objectid, tableName, longname, System.Text.Json.JsonSerializer.Serialize(instance, ImporterNIS.jsonSerializerOptions));
                        }
                        break;
                    case "regulatedareasandlimitsa::115": { // TESARE_TerritorialSeaArea
                            var instance = (TerritorialSeaArea)ImporterNIS.Build("TESARE", feature, buffer);

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




