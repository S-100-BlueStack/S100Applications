using ArcGIS.Core.Data;
using S100FC;
using S100FC.S101.FeatureTypes;
using S100Framework.Applications.S57.esri;
using S100Framework.Applications.Singletons;

namespace S100Framework.Applications
{
    internal static partial class ImporterNIS
    {
        private static void S57_DangersA(Geodatabase source, Geodatabase target, QueryFilter filter) {
            var tableName = "DangersA";

            using var dangersa = source.OpenDataset<FeatureClass>(source.GetName(tableName));
            using var depthsA = source.OpenDataset<FeatureClass>(source.GetName("DepthsA"));
            Subtypes.Instance.RegisterSubtypes(dangersa);

            using var featureClass = target.OpenDataset<FeatureClass>(target.GetName("surface"));

            using var buffer = featureClass.CreateRowBuffer();

            using var cursor = dangersa.Search(filter, true);
            int recordCount = 0;

            while (cursor.MoveNext()) {
                recordCount += 1;

                var feature = (Feature)cursor.Current;

                if (feature.GetShape() is null) continue;
                if (feature.GetShape().IsEmpty) continue;

                var current = new DangersA(feature);

                var objectid = current.OBJECTID ?? default;
                var globalid = current.GLOBALID;

                if (FeatureRelations.Instance.IsSlave(globalid)) {
                    continue;
                }

                if (ConversionAnalytics.Instance.IsConverted(globalid)) {
                    throw new Exception("Ups. Not supported");
                }

                var fcSubtype = current.FCSUBTYPE ?? default;

                var valsou = current.VALSOU ?? default;
                var watlev = current.WATLEV ?? default;
                var plts_comp_scale = current.PLTS_COMP_SCALE ?? default;
                var longname = current.LNAM ?? Strings.UNKNOWN;
                var status = current.STATUS ?? default;

                bool isValsouEmpty = !current.VALSOU.HasValue;

                // The attribute default clearance depth must be populated with a value, which must not be an empty(null)
                // value, only if the attribute value of sounding for the feature instance is populated with an empty(null) value
                // and the attribute height, if an allowable attribute for the feature, is not populated.
                // S-101 Annex A_DCEG Edition 1.5.0_Draft for Edition 2.0.0.pdf: p.771
                //Decimal defaultClearanceDepth = -1;

                switch (fcSubtype) {
                    case 1: { // CTNARE_CautionArea
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
                    case 10: { // FSHFAC_FishingFacility
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
                    case 15: { // OBSTRN_Obstruction
                            var instance = ImporterNIS.Build("OBSTRN", feature, buffer);

                            using var featureN = featureClass.CreateRow(buffer);
                            var name = featureN.UID();

                            if (FeatureRelations.Instance.HasSlaves(globalid)) {
                                if (instance is Dam dam)
                                    relatedEquipment!.CreateRelatedAreaEquipment(current, dam, featureN, dam.scaleMinimum);
                                if(instance is FoulGround foulGround)
                                    relatedEquipment!.CreateRelatedAreaEquipment(current, foulGround, featureN, foulGround.scaleMinimum);
                                if(instance is Obstruction obstruction)
                                    relatedEquipment!.CreateRelatedAreaEquipment(current, obstruction, featureN, obstruction.scaleMinimum);
                            }
                            ConversionAnalytics.Instance.AddConverted(tableName, globalid, name);
                            Logger.Current.DataObject(objectid, tableName, longname, System.Text.Json.JsonSerializer.Serialize(instance, ImporterNIS.jsonSerializerOptions));
                        }
                        break;
                    case 20: { // WATTUR_WaterTurbulence
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
                    case 25: { // WRECKS_Wreck
                            var instance = (Wreck)ImporterNIS.Build("WRECKS", feature, buffer);

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
