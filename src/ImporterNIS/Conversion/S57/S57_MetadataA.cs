using ArcGIS.Core.Data;
using S100FC;
using S100FC.S101.ComplexAttributes;
using S100FC.S101.FeatureTypes;
using S100FC.S101.InformationTypes;
using S100FC.S101.SimpleAttributes;
using S100FC.S128.SimpleAttributes;
using S100Framework.Applications.S57.esri;
using S100Framework.Applications.Singletons;

namespace S100Framework.Applications
{
    internal static partial class ImporterNIS
    {

        private static void S57_MetadataA(Geodatabase source, Geodatabase target, QueryFilter filter) {
            var tableName = "MetadataA";

            using var metadataa = source.OpenDataset<FeatureClass>(source.GetName(tableName));
            Subtypes.Instance.RegisterSubtypes(metadataa);

            using var depthsa = source.OpenDataset<FeatureClass>(source.GetName("depthsa"));

            using var featureClass = target.OpenDataset<FeatureClass>(target.GetName("surface"));

            using var buffer = featureClass.CreateRowBuffer();

            using var cursor = metadataa.Search(filter, true);
            int recordCount = 0;

            while (cursor.MoveNext()) {
                recordCount += 1;

                var feature = (Feature)cursor.Current;

                if (feature.GetShape() is null) continue;
                if (feature.GetShape().IsEmpty) continue;

                var current = new MetaDataA(feature);

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

                //var displayScale = GetDisplayScale(serie); // DisplayScale.GetNearestBelowKey(plts_comp_scale) ?? default;

                if (current.PUBREF != default) {
                    //if (System.Diagnostics.Debugger.IsAttached)
                    //System.Diagnostics.Debugger.Break();
                }

                switch (fcSubtype) {
                    case 1: { // M_ACCY_AccuracyOfData
                            var instance = (QualityOfNonBathymetricData)ImporterNIS.Build("M_ACCY", feature, buffer);

                            using var featureN = featureClass.CreateRow(buffer);
                            var name = featureN.UID();

                            if (FeatureRelations.Instance.HasSlaves(current.GLOBALID)) {
                                relatedEquipment!.CreateRelatedAreaEquipment(current, instance, featureN, null);
                            }
                            ConversionAnalytics.Instance.AddConverted(tableName, current.GLOBALID, name);
                            Logger.Current.DataObject(objectid, tableName, longname, System.Text.Json.JsonSerializer.Serialize(instance, ImporterNIS.jsonSerializerOptions));
                        }
                        break;
                    case 20: { // M_CSCL_CompilationScaleOfData
                            continue;   //  S57_ProductCoverage
                            //throw new NotImplementedException($"No M_CSCL_CompilationScaleOfData in DK or GL. {tableName}");
                        }
                    case 25: { // M_HOPA_HorizontalDatumShiftParameters
                            throw new NotImplementedException($"No M_HOPA_HorizontalDatumShiftParameters in DK or GL. {tableName}");
                        }
                    case 30: { // M_NPUB_NauticalPublicationInformation
                            var instance = (InformationArea)ImporterNIS.Build("M_NPUB", feature, buffer);

                            using var featureN = featureClass.CreateRow(buffer);
                            var name = featureN.UID();

                            if (FeatureRelations.Instance.HasSlaves(current.GLOBALID)) {
                                relatedEquipment!.CreateRelatedAreaEquipment(current, instance, featureN, null);
                            }
                            ConversionAnalytics.Instance.AddConverted(tableName, current.GLOBALID, name);
                            Logger.Current.DataObject(objectid, tableName, longname, System.Text.Json.JsonSerializer.Serialize(instance, ImporterNIS.jsonSerializerOptions));
                        }
                        break;
                    case 35: { // M_NSYS_NavigationalSystemOfMarks // Navigational System of Marks - region A and B globally
                            var instance = ImporterNIS.Build("M_NSYS", feature, buffer);

                            using var featureN = featureClass.CreateRow(buffer);
                            var name = featureN.UID();

                            if (FeatureRelations.Instance.HasSlaves(current.GLOBALID)) {
                                if (instance is LocalDirectionOfBuoyage localDirectionOfBuoyage)
                                    relatedEquipment!.CreateRelatedAreaEquipment(current, localDirectionOfBuoyage, featureN, null);
                                if (instance is NavigationalSystemOfMarks navigationalSystemOfMarks)
                                    relatedEquipment!.CreateRelatedAreaEquipment(current, navigationalSystemOfMarks, featureN, null);
                            }
                            ConversionAnalytics.Instance.AddConverted(tableName, current.GLOBALID, name);
                            Logger.Current.DataObject(objectid, tableName, longname, System.Text.Json.JsonSerializer.Serialize(instance, ImporterNIS.jsonSerializerOptions));
                        }
                        break;
                    case 40: { // M_QUAL_QualityOfData // SKIN OF EARTH
                            var instance = ImporterNIS.M_QUAL(feature, buffer, filter, target);

                            using var featureN = featureClass.CreateRow(buffer);
                            var name = featureN.UID();

                            if (FeatureRelations.Instance.HasSlaves(current.GLOBALID)) {
                                relatedEquipment!.CreateRelatedAreaEquipment(current, instance, featureN, null);
                            }
                            ConversionAnalytics.Instance.AddConverted(tableName, current.GLOBALID, name);
                            Logger.Current.DataObject(objectid, tableName, longname, System.Text.Json.JsonSerializer.Serialize(instance, ImporterNIS.jsonSerializerOptions));
                        }
                        break;
                    case 45: { // M_SDAT_SoundingDatum
                               // Handled by S101_SoundingDatum
                        }
                        break;
                    case 50: { // M_SREL_SurveyReliability
                            var instance = (QualityOfSurvey)ImporterNIS.Build("M_SREL", feature, buffer);

                            using var featureN = featureClass.CreateRow(buffer);
                            var name = featureN.UID();

                            if (FeatureRelations.Instance.HasSlaves(current.GLOBALID)) {
                                relatedEquipment!.CreateRelatedAreaEquipment(current, instance, featureN, null);
                            }
                            ConversionAnalytics.Instance.AddConverted(tableName, current.GLOBALID, name);
                            Logger.Current.DataObject(objectid, tableName, longname, System.Text.Json.JsonSerializer.Serialize(instance, ImporterNIS.jsonSerializerOptions));
                        }
                        break;
                    case 55: { // M_VDAT_VerticalDatumOfData
                            var instance = (VerticalDatumOfData)ImporterNIS.Build("M_VDAT", feature, buffer);

                            using var featureN = featureClass.CreateRow(buffer);
                            var name = featureN.UID();

                            if (FeatureRelations.Instance.HasSlaves(current.GLOBALID)) {
                                relatedEquipment!.CreateRelatedAreaEquipment(current, instance, featureN, null);
                            }
                            ConversionAnalytics.Instance.AddConverted(tableName, current.GLOBALID, name);
                            Logger.Current.DataObject(objectid, tableName, longname, System.Text.Json.JsonSerializer.Serialize(instance, ImporterNIS.jsonSerializerOptions));

                            if (instance.verticalDatum is not null) {
                                VerticalDatums.Instance.Add(current.SHAPE!.Clone(), instance.verticalDatum);
                            }
                            else {
                                Logger.Current.DataError(current.OBJECTID.Value, current.TableName!, current.LNAM!, $"M_VDAT_VerticalDatumOfData has no VERDAT");
                            }
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
