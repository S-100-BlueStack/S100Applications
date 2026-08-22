using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
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

        private static void S57_Metadata(string tableName, Func<string, FeatureClass> source, QueryFilter filter, Func<FeatureClass> target, Geodatabase geodatabase, Action<RowBuffer, Geometry> setShape) {
            using var dataset = source(tableName);
            Subtypes.Instance.RegisterSubtypes(dataset);

            using var featureClass = target();

            using var buffer = featureClass.CreateRowBuffer();

            using var cursor = dataset.Search(filter, true);
            int recordCount = 0;

            (long ObjectID, string? SORDAT, string? PICREP, string? OBJNAM, string? NOBJNM, string? NTXTDS, string? TXTDSC, string? INFORM, string? NINFOM, string? PUBREF, int? PLTS_COMP_SCALE, Geometry Shape, information?[] information, int? scaleMinimum, Action<InformationArea, string> callback)[] informationAreas = [];

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

                var longname = current.LNAM()!;

                switch ($"{tableName}::{feature.FCSubtype()}".ToLowerInvariant()) {
                    case "metadataa::1": { // M_ACCY_AccuracyOfData
                            var instance = (QualityOfNonBathymetricData)ImporterNIS.Build("M_ACCY", feature, buffer);

                            using var featureN = featureClass.CreateRow(buffer);
                            var name = featureN.UID();

                            if (FeatureRelations.Instance.HasSlaves(globalid)) {
                                relatedEquipment!.CreateRelatedAreaEquipment(current, instance, featureN, null);
                            }
                            ConversionAnalytics.Instance.AddConverted(tableName, globalid, name);
                            Logger.Current.DataObject(objectid, tableName, longname, System.Text.Json.JsonSerializer.Serialize(instance, ImporterNIS.jsonSerializerOptions));
                        }
                        break;
                    case "metadataa::20": { // M_CSCL_CompilationScaleOfData
                            continue;   //  S57_ProductCoverage
                        }
                    case "metadataa::25": { // M_HOPA_HorizontalDatumShiftParameters
                            throw new NotImplementedException($"No M_HOPA_HorizontalDatumShiftParameters in DK or GL. {tableName}");
                        }
                    case "metadatap::1": { // M_NPUB_NauticalPublicationInformation
                            var instance = (InformationArea)ImporterNIS.Build("M_NPUB", feature, buffer);

                            using var featureN = featureClass.CreateRow(buffer);
                            var name = featureN.UID();

                            if (FeatureRelations.Instance.HasSlaves(globalid)) {
                                relatedEquipment!.CreateRelatedPointEquipment(new MetaDataP(current), instance, featureN, null);
                            }
                            ConversionAnalytics.Instance.AddConverted(tableName, globalid, name);
                            Logger.Current.DataObject(objectid, tableName, longname, System.Text.Json.JsonSerializer.Serialize(instance, ImporterNIS.jsonSerializerOptions));
                        }
                        break;
                    case "metadataa::30": { // M_NPUB_NauticalPublicationInformation
                            var instance = (InformationArea)ImporterNIS.Build("M_NPUB", feature, buffer);

                            informationAreas = [.. informationAreas, (
                                feature.GetObjectID(),
                                feature.SORDAT_HasValue() ? feature.SORDAT() : null,
                                feature.PICREP_HasValue() ? feature.PICREP() : null,
                                feature.OBJNAM_HasValue() ? feature.OBJNAM() : null,
                                feature.NOBJNM_HasValue() ? feature.NOBJNM() : null,
                                feature.NTXTDS_HasValue() ? feature.NTXTDS() : null,
                                feature.TXTDSC_HasValue() ? feature.TXTDSC() : null,
                                feature.INFORM_HasValue() ? feature.INFORM() : null,
                                feature.NINFOM_HasValue() ? feature.NINFOM() : null,
                                feature.PUBREF_HasValue() ? feature.PUBREF() : null,
                                feature.PLTS_COMP_SCALE_HasValue() ? feature.PLTS_COMP_SCALE() : null,
                                feature.GetShape().Clone(),
                                instance.information,
                                instance.scaleMinimum,
                                (instance,name) =>{
                                    ConversionAnalytics.Instance.AddConverted(tableName, globalid, name);
                                    Logger.Current.DataObject(objectid, tableName, longname, System.Text.Json.JsonSerializer.Serialize(instance, ImporterNIS.jsonSerializerOptions));
                                })];

                            //using var featureN = featureClass.CreateRow(buffer);
                            //var name = featureN.UID();

                            //if (FeatureRelations.Instance.HasSlaves(globalid)) {
                            //    relatedEquipment!.CreateRelatedAreaEquipment(current, instance, featureN, null);
                            //}
                            //ConversionAnalytics.Instance.AddConverted(tableName, globalid, name);
                            //Logger.Current.DataObject(objectid, tableName, longname, System.Text.Json.JsonSerializer.Serialize(instance, ImporterNIS.jsonSerializerOptions));
                        }
                        break;
                    case "metadataa::35": { // M_NSYS_NavigationalSystemOfMarks // Navigational System of Marks - region A and B globally
                            var instance = ImporterNIS.Build("M_NSYS", feature, buffer);

                            using var featureN = featureClass.CreateRow(buffer);
                            var name = featureN.UID();

                            if (FeatureRelations.Instance.HasSlaves(globalid)) {
                                if (instance is LocalDirectionOfBuoyage localDirectionOfBuoyage)
                                    relatedEquipment!.CreateRelatedAreaEquipment(current, localDirectionOfBuoyage, featureN, null);
                                if (instance is NavigationalSystemOfMarks navigationalSystemOfMarks)
                                    relatedEquipment!.CreateRelatedAreaEquipment(current, navigationalSystemOfMarks, featureN, null);
                            }
                            ConversionAnalytics.Instance.AddConverted(tableName, globalid, name);
                            Logger.Current.DataObject(objectid, tableName, longname, System.Text.Json.JsonSerializer.Serialize(instance, ImporterNIS.jsonSerializerOptions));
                        }
                        break;
                    case "metadataa::40": { // M_QUAL_QualityOfData // SKIN OF EARTH
                            var instance = ImporterNIS.M_QUAL(feature, buffer, filter, geodatabase);

                            using var featureN = featureClass.CreateRow(buffer);
                            var name = featureN.UID();

                            if (FeatureRelations.Instance.HasSlaves(globalid)) {
                                relatedEquipment!.CreateRelatedAreaEquipment(current, instance, featureN, null);
                            }
                            ConversionAnalytics.Instance.AddConverted(tableName, globalid, name);
                            Logger.Current.DataObject(objectid, tableName, longname, System.Text.Json.JsonSerializer.Serialize(instance, ImporterNIS.jsonSerializerOptions));
                        }
                        break;
                    case "metadataa::45": { // M_SDAT_SoundingDatum
                            // Handled by S101_SoundingDatum
                        }
                        break;
                    case "metadataa::50": { // M_SREL_SurveyReliability
                            var instance = (QualityOfSurvey)ImporterNIS.Build("M_SREL", feature, buffer);

                            using var featureN = featureClass.CreateRow(buffer);
                            var name = featureN.UID();

                            if (FeatureRelations.Instance.HasSlaves(globalid)) {
                                relatedEquipment!.CreateRelatedAreaEquipment(current, instance, featureN, null);
                            }
                            ConversionAnalytics.Instance.AddConverted(tableName, globalid, name);
                            Logger.Current.DataObject(objectid, tableName, longname, System.Text.Json.JsonSerializer.Serialize(instance, ImporterNIS.jsonSerializerOptions));
                        }
                        break;
                    case "metadataa::55": { // M_VDAT_VerticalDatumOfData
                            // Handled by S101_VerticalDatumOfData

                            //var instance = (VerticalDatumOfData)ImporterNIS.Build("M_VDAT", feature, buffer);

                            //using var featureN = featureClass.CreateRow(buffer);
                            //var name = featureN.UID();

                            //if (FeatureRelations.Instance.HasSlaves(globalid)) {
                            //    relatedEquipment!.CreateRelatedAreaEquipment(current, instance, featureN, null);
                            //}
                            //ConversionAnalytics.Instance.AddConverted(tableName, globalid, name);
                            //Logger.Current.DataObject(objectid, tableName, longname, System.Text.Json.JsonSerializer.Serialize(instance, ImporterNIS.jsonSerializerOptions));

                            //if (instance.verticalDatum is not null) {
                            //    VerticalDatums.Instance.Add(current.SHAPE()!.Clone(), instance.verticalDatum);
                            //}
                            //else {
                            //    Logger.Current.DataError(current.GetObjectID(), current.TableName(), current.LNAM()!, $"M_VDAT_VerticalDatumOfData has no VERDAT");
                            //}
                        }
                        break;
                    default:
                        // code block
                        System.Diagnostics.Debugger.Break();
                        break;
                }
            }

            foreach (var e in informationAreas.GroupBy(e => (e.SORDAT, e.PICREP, e.OBJNAM, e.NOBJNM, e.PLTS_COMP_SCALE, e.Shape.ToJson()))) {
                var _ = e.First();

                //if (e.Any(i => i.ObjectID == 233 || i.ObjectID == 234)) System.Diagnostics.Debugger.Break();

                var instance = ImporterNIS.M_NPUB(_.SORDAT, _.PICREP, _.OBJNAM, _.NOBJNM);

                if (_.PLTS_COMP_SCALE.HasValue) {
                    instance.scaleMinimum = _.scaleMinimum;
                }

                information[] informations = [];
                informationBinding[] informationBindings = [];

                foreach (var i in e) {
                    var result = ImporterNIS.AddInformation(i.ObjectID, tableName, i.NTXTDS, i.TXTDSC, i.INFORM, i.NINFOM, i.PUBREF);
                    informations = [.. informations, .. result.information];
                    informationBindings = [.. informationBindings, .. result.InformationBindings];
                }

                if (informations.Any())
                    instance.information = informations;                
                instance.SetInformationBindings(informationBindings);


                buffer["ps"] = ps101;
                buffer["code"] = instance.GetType().Name;
                buffer["attributebindings"] = instance.Flatten();
                buffer["informationbindings"] = System.Text.Json.JsonSerializer.Serialize(instance.GetInformationBindings(), jsonSerializerOptions);
                buffer["sourceIdentifier"] = instance.sourceIdentifier;
                SetShape(buffer, _.Shape);
                SetUsageBand(buffer, _.PLTS_COMP_SCALE!.Value);

                using var featureN = featureClass.CreateRow(buffer);
                var name = featureN.UID();

                foreach(var i in e) {
                    i.callback(instance, name);
                }

                //if (FeatureRelations.Instance.HasSlaves(globalid)) {
                //    relatedEquipment!.CreateRelatedAreaEquipment(current, instance, featureN, null);
                //}
            }

            Logger.Current.DataTotalCount(tableName, recordCount, ConversionAnalytics.Instance.GetConvertedCount(tableName));
        }


    }
}
