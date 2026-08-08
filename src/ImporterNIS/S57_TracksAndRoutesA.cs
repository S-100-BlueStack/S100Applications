using ArcGIS.Core.Data;
using S100FC;
using S100FC.S101.FeatureTypes;
using S100Framework.Applications.S57.esri;
using S100Framework.Applications.Singletons;

namespace S100Framework.Applications
{
    internal static partial class ImporterNIS
    {
        private static void S57_TracksAndRoutesA(Geodatabase source, Geodatabase target, QueryFilter filter) {
            var tableName = "TracksAndRoutesA";

            using var tracksAndRoutesA = source.OpenDataset<FeatureClass>(source.GetName(tableName));
            Subtypes.Instance.RegisterSubtypes(tracksAndRoutesA);

            using var featureClass = target.OpenDataset<FeatureClass>(target.GetName("surface"));

            using var buffer = featureClass.CreateRowBuffer();

            using var cursor = tracksAndRoutesA.Search(filter, true);
            int recordCount = 0;

            while (cursor.MoveNext()) {
                recordCount += 1;

                var feature = (Feature)cursor.Current;

                if (feature.GetShape() is null) continue;
                if (feature.GetShape().IsEmpty) continue;

                var current = new TracksAndRoutesA(feature);

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
                    case 1: { // DWRTPT_DeepWaterRoutePart
                            var instance = (DeepWaterRoutePart)ImporterNIS.Build("DWRTPT", feature, buffer);

                            using var featureN = featureClass.CreateRow(buffer);
                            var name = featureN.UID();

                            if (FeatureRelations.Instance.HasSlaves(current.GLOBALID)) {
                                relatedEquipment!.CreateRelatedAreaEquipment(current, instance, featureN, instance.scaleMinimum);
                            }
                            ConversionAnalytics.Instance.AddConverted(tableName, current.GLOBALID, name);
                            Logger.Current.DataObject(objectid, tableName, longname, System.Text.Json.JsonSerializer.Serialize(instance, ImporterNIS.jsonSerializerOptions));
                        }
                        break;
                    case 5: { // FAIRWY_Fairway
                            var instance = (Fairway)ImporterNIS.Build("FAIRWY", feature, buffer);

                            using var featureN = featureClass.CreateRow(buffer);
                            var name = featureN.UID();

                            if (FeatureRelations.Instance.HasSlaves(current.GLOBALID)) {
                                relatedEquipment!.CreateRelatedAreaEquipment(current, instance, featureN, instance.scaleMinimum);
                            }
                            ConversionAnalytics.Instance.AddConverted(tableName, current.GLOBALID, name);
                            Logger.Current.DataObject(objectid, tableName, longname, System.Text.Json.JsonSerializer.Serialize(instance, ImporterNIS.jsonSerializerOptions));
                        }
                        break;
                    case 10: { // FERYRT_FerryRoute
                            var instance = (FerryRoute)ImporterNIS.Build("FERYRT", feature, buffer);

                            using var featureN = featureClass.CreateRow(buffer);
                            var name = featureN.UID();

                            if (FeatureRelations.Instance.HasSlaves(current.GLOBALID)) {
                                relatedEquipment!.CreateRelatedAreaEquipment(current, instance, featureN, instance.scaleMinimum);
                            }
                            ConversionAnalytics.Instance.AddConverted(tableName, current.GLOBALID, name);
                            Logger.Current.DataObject(objectid, tableName, longname, System.Text.Json.JsonSerializer.Serialize(instance, ImporterNIS.jsonSerializerOptions));
                        }
                        break;

                    case 15: { // ISTZNE_InshoreTrafficZone
                            var instance = (InshoreTrafficZone)ImporterNIS.Build("ISTZNE", feature, buffer);

                            using var featureN = featureClass.CreateRow(buffer);
                            var name = featureN.UID();

                            if (FeatureRelations.Instance.HasSlaves(current.GLOBALID)) {
                                relatedEquipment!.CreateRelatedAreaEquipment(current, instance, featureN, instance.scaleMinimum);
                            }
                            ConversionAnalytics.Instance.AddConverted(tableName, current.GLOBALID, name);
                            Logger.Current.DataObject(objectid, tableName, longname, System.Text.Json.JsonSerializer.Serialize(instance, ImporterNIS.jsonSerializerOptions));
                        }
                        break;
                    case 20: { // PRCARE_PrecautionaryArea
                            var instance = (PrecautionaryArea)ImporterNIS.Build("PRCARE", feature, buffer);

                            using var featureN = featureClass.CreateRow(buffer);
                            var name = featureN.UID();

                            if (FeatureRelations.Instance.HasSlaves(current.GLOBALID)) {
                                relatedEquipment!.CreateRelatedAreaEquipment(current, instance, featureN, instance.scaleMinimum);
                            }
                            ConversionAnalytics.Instance.AddConverted(tableName, current.GLOBALID, name);
                            Logger.Current.DataObject(objectid, tableName, longname, System.Text.Json.JsonSerializer.Serialize(instance, ImporterNIS.jsonSerializerOptions));
                        }
                        break;
                    case 25: { // RADRNG_RadarRange
                            var instance = (RadarRange)ImporterNIS.Build("RADRNG", feature, buffer);

                            using var featureN = featureClass.CreateRow(buffer);
                            var name = featureN.UID();

                            if (FeatureRelations.Instance.HasSlaves(current.GLOBALID)) {
                                relatedEquipment!.CreateRelatedAreaEquipment(current, instance, featureN, instance.scaleMinimum);
                            }

                            ConversionAnalytics.Instance.AddConverted(tableName, current.GLOBALID, name);
                            Logger.Current.DataObject(objectid, tableName, longname, System.Text.Json.JsonSerializer.Serialize(instance, ImporterNIS.jsonSerializerOptions));
                        }
                        break;
                    case 30: { // RCTLPT_RecommendedTrafficLanePart
                            var instance = (RecommendedTrafficLanePart)ImporterNIS.Build("RCTLPT", feature, buffer);

                            using var featureN = featureClass.CreateRow(buffer);
                            var name = featureN.UID();

                            if (FeatureRelations.Instance.HasSlaves(current.GLOBALID)) {
                                relatedEquipment!.CreateRelatedAreaEquipment(current, instance, featureN, instance.scaleMinimum);
                            }
                            ConversionAnalytics.Instance.AddConverted(tableName, current.GLOBALID, name);
                            Logger.Current.DataObject(objectid, tableName, longname, System.Text.Json.JsonSerializer.Serialize(instance, ImporterNIS.jsonSerializerOptions));
                        }
                        break;
                    case 40: { // RECTRC_RecommendedTrack

                            ConversionAnalytics.Instance.AddConverted(tableName, current.GLOBALID, "");
                            Logger.Current.DataObject(objectid, tableName, longname, "IGNORED - see S-65");
                            Logger.Current.DataError(current.OBJECTID ?? -1, tableName, longname, $"RECTRC with primitive type area are not converted to S-101. See S-65.");
                            continue;

                            //var instance = new RecommendedTrack {
                            //    basedOnFixedMarks = default,
                            //    orientationValue = default,
                            //    trafficFlow = default,
                            //};

                            //if (current.CATTRK.HasValue) {
                            //    if (current.CATTRK.Value == 1) {
                            //        instance.basedOnFixedMarks = true;
                            //    }
                            //    else if (current.CATTRK.Value == 2) {
                            //        instance.basedOnFixedMarks = false;
                            //    }
                            //    else {
                            //        Logger.Current.DataError(current.OBJECTID ?? -1, tableName, longname, $"Cannot convert value {current.CATTRK.Value} to basedOnFixedMarks boolean. Only values 1 and 2 are supported.");
                            //    }
                            //}

                            //if (current.DRVAL1.HasValue) {
                            //    instance.depthRangeMinimumValue = current.DRVAL1.Value;
                            //}

                            //instance.featureName = GetFeatureName(current.OBJNAM, current.NOBJNM);

                            //DateHelper.TryGetFixedDateRange(current.DATSTA, current.DATEND, out var dateRange);
                            //if (dateRange != default) {
                            //    instance.fixedDateRange = dateRange;
                            //}

                            //// TODO: interoperabilityIdentifier

                            //// TODO: maximumPermittedDraught

                            //if (current.ORIENT.HasValue && current.ORIENT.Value != -32767m) {
                            //    instance.orientationValue = current.ORIENT.Value;
                            //}

                            //DateHelper.TryGetPeriodicDateRange(current.PERSTA, current.PEREND, out var periodicDateRange);
                            //if (periodicDateRange != default) {
                            //    instance.periodicDateRange = periodicDateRange;
                            //}

                            //if (current.QUASOU != default) {
                            //    instance.qualityOfVerticalMeasurement = EnumHelper.GetEnumValues(current.QUASOU);
                            //}

                            //if (current.STATUS != default) {
                            //    instance.status = GetStatus(current.STATUS);
                            //}

                            //if (current.TECSOU != null) {
                            //    instance.techniqueOfVerticalMeasurement = EnumHelper.GetEnumValues(current.TECSOU);
                            //}

                            //if (current.TRAFIC.HasValue) {
                            //    instance.trafficFlow = EnumHelper.GetEnumValue(current.TRAFIC.Value);
                            //}

                            //if (current.SOUACC.HasValue) {
                            //    instance.verticalUncertainty = new() {
                            //        uncertaintyFixed = current.SOUACC.Value
                            //    };
                            //}

                            //if (current.PLTS_COMP_SCALE.HasValue && current.SHAPE != null) {
                            //    string subtype = "";
                            //    if (current.TableName != default && current.FCSUBTYPE.HasValue && !Subtypes.Instance.TryGetSubtype(current.TableName, current.FCSUBTYPE.Value, out subtype))
                            //        throw new NotSupportedException($"Unknown subtype for {current.TableName}, {current.FCSUBTYPE.Value}");
                            //    instance.scaleMinimum = Scamin.Instance.GetMinimumScale(current, subtype, current.PLTS_COMP_SCALE.Value, isRelatedToStructure: false);
                            //}

                            //instance.SetInformationBindings(AddInformation(instance.information, current.OBJECTID!.Value, current.TableName!, current.NTXTDS, current.TXTDSC, current.INFORM, current.NINFOM));

                            //buffer["ps"] = ps101;
                            //buffer["code"] = instance.GetType().Name;
                            //
                            //
                            //buffer["informationbindings"] = System.Text.Json.JsonSerializer.Serialize(instance.GetInformationBindings(), jsonSerializerOptions);

                            //SetShape(buffer, current.SHAPE); buffer["sourceIdentifier"] = instance.sourceIdentifier;
                            //SetUsageBand(buffer, current.PLTS_COMP_SCALE!.Value);

                            //using var featureN = featureClass.CreateRow(buffer);
                            //var name = featureN.Crc32();

                            //if (FeatureRelations.Instance.HasSlaves(current.GLOBALID)) {
                            //    relatedEquipment!.CreateRelatedAreaEquipment(current, instance, featureN, instance.scaleMinimum);
                            //}

                            //ConversionAnalytics.Instance.AddConverted(tableName, current.GLOBALID, name);

                            //Logger.Current.DataObject(objectid, tableName, longname, System.Text.Json.JsonSerializer.Serialize(instance, ImporterNIS.jsonSerializerOptions));
                        }
                        break;
                    case 45: { // SUBTLN_SubmarineTransitLane
                            var instance = (SubmarineTransitLane)ImporterNIS.Build("SUBTLN", feature, buffer);

                            using var featureN = featureClass.CreateRow(buffer);
                            var name = featureN.UID();

                            if (FeatureRelations.Instance.HasSlaves(current.GLOBALID)) {
                                relatedEquipment!.CreateRelatedAreaEquipment(current, instance, featureN, instance.scaleMinimum);
                            }

                            ConversionAnalytics.Instance.AddConverted(tableName, current.GLOBALID, name);
                            Logger.Current.DataObject(objectid, tableName, longname, System.Text.Json.JsonSerializer.Serialize(instance, ImporterNIS.jsonSerializerOptions));
                        }
                        break;                
                    case 50: { // TSEZNE_TrafficSeparationZone
                            var instance = (SeparationZoneOrLine)ImporterNIS.Build("TSEZNE", feature, buffer);

                            using var featureN = featureClass.CreateRow(buffer);
                            var name = featureN.UID();

                            if (FeatureRelations.Instance.HasSlaves(current.GLOBALID)) {
                                relatedEquipment!.CreateRelatedAreaEquipment(current, instance, featureN, instance.scaleMinimum);
                            }

                            ConversionAnalytics.Instance.AddConverted(tableName, current.GLOBALID, name);
                            Logger.Current.DataObject(objectid, tableName, longname, System.Text.Json.JsonSerializer.Serialize(instance, ImporterNIS.jsonSerializerOptions));
                        }
                        break;
                    case 55: { // TSSCRS_TrafficSeparationSchemeCrossing
                            throw new NotImplementedException($"No TSSCRS_TrafficSeparationSchemeCrossing in DK or GL. {tableName}");
                        }

                    case 60: { // TSSLPT_TrafficSeparationSchemeLanePart
                            var instance = (TrafficSeparationSchemeLanePart)ImporterNIS.Build("TSSLPT", feature, buffer);

                            using var featureN = featureClass.CreateRow(buffer);
                            var name = featureN.UID();

                            if (FeatureRelations.Instance.HasSlaves(current.GLOBALID)) {
                                relatedEquipment!.CreateRelatedAreaEquipment(current, instance, featureN, instance.scaleMinimum);
                            }
                            ConversionAnalytics.Instance.AddConverted(tableName, current.GLOBALID, name);
                            Logger.Current.DataObject(objectid, tableName, longname, System.Text.Json.JsonSerializer.Serialize(instance, ImporterNIS.jsonSerializerOptions));
                        }
                        break;
                    case 65: { // TSSRON_TrafficSeparationSchemeRoundabout
                            var instance = (TrafficSeparationSchemeRoundabout)ImporterNIS.Build("TSSRON", feature, buffer);

                            using var featureN = featureClass.CreateRow(buffer);
                            var name = featureN.UID();

                            if (FeatureRelations.Instance.HasSlaves(current.GLOBALID)) {
                                relatedEquipment!.CreateRelatedAreaEquipment(current, instance, featureN, instance.scaleMinimum);
                            }
                            ConversionAnalytics.Instance.AddConverted(tableName, current.GLOBALID, name);
                            Logger.Current.DataObject(objectid, tableName, longname, System.Text.Json.JsonSerializer.Serialize(instance, ImporterNIS.jsonSerializerOptions));
                        }
                        break;
                    case 70: { // TWRTPT_TwoWayRoutePart
                            var instance = (TwoWayRoutePart)ImporterNIS.Build("TWRTPT", feature, buffer);

                            using var featureN = featureClass.CreateRow(buffer);
                            var name = featureN.UID();

                            if (FeatureRelations.Instance.HasSlaves(current.GLOBALID)) {
                                relatedEquipment!.CreateRelatedAreaEquipment(current, instance, featureN, instance.scaleMinimum);
                            }
                            ConversionAnalytics.Instance.AddConverted(tableName, current.GLOBALID, name);
                            Logger.Current.DataObject(objectid, tableName, longname, System.Text.Json.JsonSerializer.Serialize(instance, ImporterNIS.jsonSerializerOptions));
                        }
                        break;
                    default:
                        // code block
                        throw new Exception("Unhandled subtype");

                }



            }
            Logger.Current.DataTotalCount(tableName, recordCount, ConversionAnalytics.Instance.GetConvertedCount(tableName));
        }


    }
}
