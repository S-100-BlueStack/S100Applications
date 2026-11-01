using ArcGIS.Core.Data;
using S100FC;
using S100FC.S101.FeatureTypes;
using S100Framework.Applications.S57.esri;
using S100Framework.Applications.Singletons;
using Windows.Storage.Streams;

namespace S100Framework.Applications
{
    internal static partial class ImporterNIS
    {
        private static void S57_CoastlineA(Geodatabase source, Geodatabase target, QueryFilter filter) {
            var tableName = "CoastlineA";

            using var coastlinea = source.OpenDataset<FeatureClass>(source.GetName(tableName));

            //using var featureClassTopo = target.OpenDataset<FeatureClass>(target.GetName("topo_surface"));
            using var featureClassTopo = target.OpenDataset<FeatureClass>(target.GetName("surface"));
            using var bufferTopo = featureClassTopo.CreateRowBuffer();

            using var cursor = coastlinea.Search(filter, true);
            int recordCount = 0;

            while (cursor.MoveNext()) {
                recordCount += 1;

                var feature = (Feature)cursor.Current;

                if (feature.GetShape() is null) continue;
                if (feature.GetShape().IsEmpty) continue;

                var current = new CoastlineA(feature);

                var objectid = current.OBJECTID ?? default;
                var globalid = current.GLOBALID;

                if (FeatureRelations.Instance.IsSlave(globalid)) {
                    continue;
                }

                if (ConversionAnalytics.Instance.IsConverted(globalid)) {
                    throw new Exception("Ups. Not supported");
                }

                var fcSubtype = current.FCSUBTYPE ?? default;
                var watlev = current.WATLEV ?? default;
                var plts_comp_scale = current.PLTS_COMP_SCALE ?? default;
                var longname = current.LNAM ?? Strings.UNKNOWN;
                var status = current.STATUS ?? default;

                switch (fcSubtype) {
                    case 1: { // SLCONS_ShorelineConstruction
                            var instance = (ShorelineConstruction)ImporterNIS.Build("SLCONS", feature, bufferTopo);

                            using var featureN = featureClassTopo.CreateRow(bufferTopo);
                            var name = featureN.UID();

                            if (FeatureRelations.Instance.HasSlaves(globalid)) {
                                relatedEquipment!.CreateRelatedAreaEquipment(current, instance, featureN, null);
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
