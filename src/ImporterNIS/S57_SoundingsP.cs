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

                        var current = (Feature)searchCursor.Current;

                        var globalid = current.GLOBALID();

                        if (FeatureRelations.Instance.IsSlave(globalid)) {
                            continue;
                        }

                        if (ConversionAnalytics.Instance.IsConverted(globalid)) {
                            throw new Exception("Ups. Not supported");
                        }

                        switch (current.FCSubtype()) {
                            case 1: {
                                    var instance = ImporterNIS.Build("SOUNDG", current, buffer);

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
