using ArcGIS.Core.Data;
using NetTopologySuite.Operation.Buffer;
using S100FC;
using S100FC.S101.FeatureTypes;
using S100Framework.Applications.S57.esri;
using S100Framework.Applications.Singletons;


namespace S100Framework.Applications
{
    internal static partial class ImporterNIS
    {
        private static void S57_OffshoreInstallationsP(Geodatabase source, Geodatabase target, QueryFilter filter) {
            var tableName = "OffshoreInstallationsP";


            using var featureClass = target.OpenDataset<FeatureClass>(target.GetName("point"));

            using var offshoreinstallations = source.OpenDataset<FeatureClass>(source.GetName(tableName));
            Subtypes.Instance.RegisterSubtypes(offshoreinstallations);

            int recordCount = 0;


            using var buffer = featureClass.CreateRowBuffer();

            using var cursor = offshoreinstallations.Search(filter, true);

            while (cursor.MoveNext()) {
                recordCount += 1;
                var current = (Feature)cursor.Current;

                var globalid = current.GLOBALID();

                if (FeatureRelations.Instance.IsSlave(globalid)) {
                    continue;
                }

                if (ConversionAnalytics.Instance.IsConverted(globalid)) {
                    throw new Exception("Ups. Not supported");
                }

                var longname = current.LNAM() ?? Strings.UNKNOWN;

                switch (current.FCSubtype()) {

                    case 1: { // OFSPLF_OffshorePlatform
                            var instance = (OffshorePlatform)ImporterNIS.Build("OFSPLF", current, buffer);

                            using var featureN = featureClass.CreateRow(buffer);
                            var name = featureN.UID();

                            if (FeatureRelations.Instance.HasSlaves(current.GLOBALID())) {
                                relatedEquipment!.CreateRelatedAreaEquipment(current, instance, featureN, instance.scaleMinimum);
                            }

                            ConversionAnalytics.Instance.AddConverted(tableName, current.GLOBALID(), name);
                            Logger.Current.DataObject(current.GetObjectID(), tableName, longname, System.Text.Json.JsonSerializer.Serialize(instance, ImporterNIS.jsonSerializerOptions));
                        }
                        break;
                    case 5: { // PIPARE_PipelineArea
                            throw new NotImplementedException($"No PIPARE_PipelineArea in DK or GL. {tableName}");
                        }
                    case 10: { // PIPSOL_PipelineSubmarineOnLand
                            var instance = (PipelineSubmarineOnLand)ImporterNIS.Build("PIPSOL", current, buffer);

                            using var featureN = featureClass.CreateRow(buffer);
                            var name = featureN.UID();

                            if (FeatureRelations.Instance.HasSlaves(current.GLOBALID())) {
                                relatedEquipment!.CreateRelatedAreaEquipment(current, instance, featureN, instance.scaleMinimum);
                            }

                            ConversionAnalytics.Instance.AddConverted(tableName, current.GLOBALID(), name);
                            Logger.Current.DataObject(current.GetObjectID(), tableName, longname, System.Text.Json.JsonSerializer.Serialize(instance, ImporterNIS.jsonSerializerOptions));
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

