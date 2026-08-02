using ArcGIS.Core.Data;
using S100FC;
using S100FC.S101.FeatureTypes;
using S100Framework.Applications.S57.esri;
using S100Framework.Applications.Singletons;


namespace S100Framework.Applications
{
    internal static partial class ImporterNIS
    {
        private static void S57_RegulatedAreasAndLimitsL(Geodatabase source, Geodatabase target, QueryFilter filter) {
            var tableName = "RegulatedAreasAndLimitsL";

            using var featureClass = target.OpenDataset<FeatureClass>(target.GetName("curve"));

            using var regulatedAreasAndLimitsL = source.OpenDataset<FeatureClass>(source.GetName(tableName));
            Subtypes.Instance.RegisterSubtypes(regulatedAreasAndLimitsL);

            int recordCount = 0;

            using var buffer = featureClass.CreateRowBuffer();

            using var cursor = regulatedAreasAndLimitsL.Search(filter, true);

            while (cursor.MoveNext()) {
                recordCount += 1;

                var feature = (Feature)cursor.Current;

                if (feature.GetShape() is null) continue;
                if (feature.GetShape().IsEmpty) continue;

                var current = new RegulatedAreasAndLimitsL(feature);

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
                var verlen = current.VERLEN ?? default;

                switch (fcSubtype) {
                    case 1: { // ASLXIS_ArchipelagicSeaLaneAxis
                            throw new NotImplementedException($"No ASLXIS_ArchipelagicSeaLaneAxis in DK or GL. {tableName}");
                        }
                    case 25: { // MARCUL_MarineFarmCulture
                            var instance = (MarineFarmCulture)ImporterNIS.Build("MARCUL", feature, buffer);

                            using var featureN = featureClass.CreateRow(buffer);
                            var name = featureN.UID();

                            if (FeatureRelations.Instance.HasSlaves(current.GLOBALID)) {
                                relatedEquipment!.CreateRelatedAreaEquipment(current, instance, featureN, instance.scaleMinimum);
                            }
                            ConversionAnalytics.Instance.AddConverted(tableName, current.GLOBALID, name);
                            Logger.Current.DataObject(objectid, tableName, longname, System.Text.Json.JsonSerializer.Serialize(instance, ImporterNIS.jsonSerializerOptions));
                        }
                        break;
                    case 30: { // STSLNE_StraightTerritorialSeaBaseline
                            var instance = (StraightTerritorialSeaBaseline)ImporterNIS.Build("STSLNE", feature, buffer);

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
                        System.Diagnostics.Debugger.Break();
                        break;
                }




            }
            Logger.Current.DataTotalCount(tableName, recordCount, ConversionAnalytics.Instance.GetConvertedCount(tableName));
        }
    }
}




