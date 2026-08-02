using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using S100FC;
using S100FC.S101.FeatureTypes;
using S100FC.S101.InformationAssociation;
using S100FC.S101.InformationTypes;
using S100Framework.Applications.S57.esri;
using S100Framework.Applications.Singletons;
using Windows.Storage.Streams;

namespace S100Framework.Applications
{
    internal static partial class ImporterNIS
    {
        private static void S57_Depths(string tableName, Func<string, FeatureClass> source, QueryFilter filter, Func<FeatureClass> target, Action<RowBuffer, Geometry> setShape, informationBinding<SpatialAssociation>[] spatialQuality) {
            using var dataset = source(tableName);
            Subtypes.Instance.RegisterSubtypes(dataset);

            using var featureClassTopo = target();

            using var bufferTopo = featureClassTopo.CreateRowBuffer();

            using var cursor = dataset.Search(filter, true);
            int recordCount = 0;

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

                var longname = current.LNAM() ?? string.Empty;

                switch ($"{tableName}::{feature.FCSubtype()}".ToLowerInvariant()) {
                    case "depthsa::1": {     // DEPARE // SKIN OF EARTH
                            var instance = (DepthArea)ImporterNIS.Build("DEPARE", feature, bufferTopo);

                            using var featureN = featureClassTopo.CreateRow(bufferTopo);
                            var name = featureN.UID();

                            if (FeatureRelations.Instance.HasSlaves(globalid)) {
                                relatedEquipment!.CreateRelatedAreaEquipment(current, instance, featureN, default);
                            }
                            ConversionAnalytics.Instance.AddConverted(tableName, globalid, name);
                            Logger.Current.DataObject(objectid, tableName, longname, System.Text.Json.JsonSerializer.Serialize(instance, ImporterNIS.jsonSerializerOptions));
                        }
                        break;
                    case "depthsl::5": { // DEPCNT_DepthContour
                            var instance = (DepthContour)ImporterNIS.Build("DEPCNT", feature, bufferTopo);

                            /*
                               QUAPOS = 1 (surveyed) -> will not be converted
                               QUAPOS = 2 (unsurveyed) -> will not be converted
                               QUAPOS = 3 (inadequately surveyed) -> quality of horizontal measurement = 4 (approximate)
                               QUAPOS = 4 (approximate) -> quality of horizontal measurement = 4 (approximate)
                               QUAPOS = 5 (position doubtful) -> quality of horizontal measurement = 5 (position doubtful)
                               QUAPOS = 6 (unreliable) -> quality of horizontal measurement = 4 (approximate)
                               QUAPOS = 7 (reported (not surveyed)) -> quality of horizontal measurement = 4 (approximate)
                               QUAPOS = 8 (reported (not confirmed)) -> quality of horizontal measurement = 4 (approximate)
                               QUAPOS = 9 (estimated) -> quality of horizontal measurement = 4 (approximate)
                               QUAPOS = 10 (precisely known) -> will not be converted
                               QUAPOS = 11 (calculated) -> quality of horizontal measurement = 4 (approximate)
                            */


                            (Polyline geometry, Action? callback)[] geometry = [((Polyline)current.SHAPE()!, default)];

                            var spatialQualityHits = SpatialAssociations.Instance.GetSpatialAttributeL(feature.GetShape());

                            if (spatialQualityHits.Any()) {
                                Geometry g = current.SHAPE()!;

                                geometry = [];

                                foreach (var p in spatialQualityHits) {
                                    //  Remove extra part if spatialQuality is longer than geometry!
                                    var difference = GeometryEngine.Instance.Difference(p, g);

                                    if (difference is Polyline polyline) {
                                        geometry = [.. geometry, ((Polyline)polyline.Clone(), () => {
                                                bufferTopo["informationbindings"] = System.Text.Json.JsonSerializer.Serialize(spatialQuality, ImporterNIS.jsonSerializerOptions);
                                            })];
                                    }
                                    else
                                        throw new NotImplementedException();

                                    var _ = GeometryEngine.Instance.Difference(g, difference);
                                    if (_ is Polyline)
                                        g = (Polyline)_;
                                    else
                                        throw new NotImplementedException();
                                }
                                if (!g.IsEmpty) {
                                    geometry = [.. geometry, ((Polyline)g, default)];
                                }
                            }

                            foreach (var p in geometry) {
                                bufferTopo["informationbindings"] = "[]";

                                SetShape(bufferTopo, p.geometry);

                                p.callback?.Invoke();

                                using var featureN = featureClassTopo.CreateRow(bufferTopo);
                                var name = featureN.UID();

                                if (FeatureRelations.Instance.HasSlaves(globalid)) {
                                    relatedEquipment?.CreateRelatedLineEquipment(current, instance, featureN);
                                }

                                ConversionAnalytics.Instance.AddConverted(tableName, globalid, name);
                            }
                            Logger.Current.DataObject(objectid, tableName, longname, System.Text.Json.JsonSerializer.Serialize(instance, ImporterNIS.jsonSerializerOptions));
                        }
                        break;
                    case "depthsa::5": {     // DRGARE // SKIN OF EARTH
                            var instance = ImporterNIS.Build("DRGARE", feature, bufferTopo);

                            using var featureN = featureClassTopo.CreateRow(bufferTopo);
                            var name = featureN.UID();

                            if (FeatureRelations.Instance.HasSlaves(globalid)) {
                                relatedEquipment!.CreateRelatedAreaEquipment(current, instance, featureN, default);
                            }
                            ConversionAnalytics.Instance.AddConverted(tableName, globalid, name);
                            Logger.Current.DataObject(objectid, tableName, longname, System.Text.Json.JsonSerializer.Serialize(instance, ImporterNIS.jsonSerializerOptions));
                        }
                        break;
                    case "depthsa::10": {    // SWPARE_SweptArea
                            var instance = (SweptArea)ImporterNIS.Build("SWPARE", feature, bufferTopo);

                            using var featureN = featureClassTopo.CreateRow(bufferTopo);
                            var name = featureN.UID();

                            if (FeatureRelations.Instance.HasSlaves(globalid)) {
                                relatedEquipment!.CreateRelatedAreaEquipment(current, instance, featureN, instance.scaleMinimum);
                            }

                            ConversionAnalytics.Instance.AddConverted(tableName, globalid, name);
                            Logger.Current.DataObject(objectid, tableName, longname, System.Text.Json.JsonSerializer.Serialize(instance, ImporterNIS.jsonSerializerOptions));
                        }
                        break;
                    case "depthsa::15": {    // UNSARE  // SKIN OF EARTH
                            var instance = (SweptArea)ImporterNIS.Build("UNSARE", feature, bufferTopo);

                            using var featureN = featureClassTopo.CreateRow(bufferTopo);
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

        private static informationBinding<SpatialAssociation>? _spatialAssociation = default;

        private static informationBinding<SpatialAssociation>[] CreateAssociationSpatialQuality(Geodatabase target) {
            if (_spatialAssociation is not null) return [_spatialAssociation];

            // create spatial quality
            SpatialQuality spatialQuality101 = new SpatialQuality();

            using var informationTypeTable = target.OpenDataset<Table>(target.GetName("informationtype"));
            using var buffer = informationTypeTable.CreateRowBuffer();

            spatialQuality101.qualityOfHorizontalMeasurement = 4; //    Approximate

            buffer["ps"] = ps101;
            buffer["code"] = spatialQuality101.S100FC_code;

            buffer["attributebindings"] = spatialQuality101.Flatten();

            var informationTypeRow = informationTypeTable.CreateRow(buffer);
            var informationName = informationTypeRow.UID();

            // create binding
            var informationBinding = new informationBinding<SpatialAssociation> {
                informationId = informationName,
                informationType = nameof(SpatialQuality),
                role = "theQualityInformation",
                roleType = "association",
            };

            _spatialAssociation = informationBinding;
            return [_spatialAssociation];
        }
    }
}
