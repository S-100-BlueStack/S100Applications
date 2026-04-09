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

                        S57_Converter.SOUNDG(feature, buffer, (current, buffer) => {       
                            return insertCursor.Insert(buffer);
                        });

                        if (recordCount % 1500 == 0)
                            insertCursor.Flush();
                    }

                    Logger.Current.DataTotalCount(tableName, recordCount, ConversionAnalytics.Instance.GetConvertedCount(tableName));
                }
            }
        }
    }
}
