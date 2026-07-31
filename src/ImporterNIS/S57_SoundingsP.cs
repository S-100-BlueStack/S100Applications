using ArcGIS.Core.Data;
using S100FC.S101.FeatureTypes;
using S100Framework.Applications.S57.esri;
using S100Framework.Applications.Singletons;

namespace S100Framework.Applications
{
    internal static partial class ImporterNIS
    {
        private static void S57_SoundingsP(Geodatabase source, Geodatabase target, QueryFilter filter) {
            var tableName = "SoundingsP";

            using var soundingsP = source.OpenDataset<FeatureClass>(source.GetName("SoundingsP"));

            Subtypes.Instance.RegisterSubtypes(soundingsP);

            using var pointset = target.OpenDataset<FeatureClass>(target.GetName("pointset"));

            using var searchCursor = soundingsP.Search(filter, true);

            using (var buffer = pointset.CreateRowBuffer()) {
                using (var insertCursor = pointset.CreateInsertCursor()) {

                    var recordCount = 0;

                    while (searchCursor.MoveNext()) {
                        recordCount += 1;

                        var feature = (Feature)searchCursor.Current;
                        var current = new SoundingsP(feature);

                        //var objectid = current.OBJECTID ?? default;
                        var globalid = current.GLOBALID;

                        if (FeatureRelations.Instance.IsSlave(globalid)) {
                            continue;
                        }

                        if (ConversionAnalytics.Instance.IsConverted(globalid)) {
                            throw new Exception("Ups. Not supported");
                        }

                        //var longname = current.LNAM ?? Strings.UNKNOWN;
                        var fcSubtype = current.FCSUBTYPE ?? default;
                        //var depth = current.DEPTH ?? default;
                        //var quasou = current.QUASOU ?? default;
                        //var quapos = current.P_QUAPOS ?? default;
                        //var tecsou = current.TECSOU ?? default;
                        //var objnam = current.OBJNAM ?? default;
                        //var nobjnm = current.NOBJNM ?? default;

                        switch (fcSubtype) {
                            case 1: {
                                    var instance = ImporterNIS.Build("SOUNDG", feature, buffer);

                                    if(instance is Sounding sounding) {
                                        var oid = insertCursor.Insert(buffer);
                                    }
                                    else if(instance is DepthNoBottomFound depthNoBottomFound) {
                                        var oid = insertCursor.Insert(buffer);
                                    }
                                    recordCount++;
                                }
                                break;

                            default:
                                // code block
                                System.Diagnostics.Debugger.Break();
                                break;

                        }

                        if (recordCount % 1500 == 0)
                            insertCursor.Flush();
                    }
                    
                    insertCursor.Flush();
                    Logger.Current.DataTotalCount(tableName, recordCount, ConversionAnalytics.Instance.GetConvertedCount(tableName));
                }
            }
        }
    }
}
