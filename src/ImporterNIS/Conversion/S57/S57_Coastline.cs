using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using S100FC;
using S100FC.S101.FeatureTypes;
using S100FC.S101.InformationAssociation;
using S100Framework.Applications.S57.esri;
using S100Framework.Applications.Singletons;

namespace S100Framework.Applications
{
    internal static partial class ImporterNIS
    {
        private static void S57_Coastline(string tableName, Func<string, FeatureClass> source, QueryFilter filter, Func<FeatureClass> target, Action<RowBuffer, Geometry> setShape, informationBinding<SpatialAssociation>[] spatialQuality) {
            using var dataset = source(tableName);
            Subtypes.Instance.RegisterSubtypes(dataset);

            using var featureClass = target();

            using var buffer = featureClass.CreateRowBuffer();

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
                    case "coastlinep::1": { // SLCONS_ShorelineConstruction
                            var instance = (ShorelineConstruction)ImporterNIS.Build("SLCONS", feature, buffer);

                            using var featureN = featureClass.CreateRow(buffer);
                            var name = featureN.UID();

                            if (FeatureRelations.Instance.HasSlaves(globalid)) {
                                relatedEquipment!.CreateRelatedPointEquipment(new CoastlineP(current), instance, featureN, instance.scaleMinimum);
                            }

                            ConversionAnalytics.Instance.AddConverted(tableName, globalid, name);
                            Logger.Current.DataObject(objectid, tableName, longname, System.Text.Json.JsonSerializer.Serialize(instance, ImporterNIS.jsonSerializerOptions));
                        }
                        break;

                    case "coastlinel::1": { // COALNE_Coastline
                            var instance = (Coastline)ImporterNIS.Build("COALNE", feature, buffer);

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
                                                buffer["informationbindings"] = System.Text.Json.JsonSerializer.Serialize(spatialQuality, ImporterNIS.jsonSerializerOptions);
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
                                buffer["informationbindings"] = "[]";

                                SetShape(buffer, p.geometry);

                                p.callback?.Invoke();

                                using var featureN = featureClass.CreateRow(buffer);
                                var name = featureN.UID();

                                if (FeatureRelations.Instance.HasSlaves(globalid)) {
                                    relatedEquipment?.CreateRelatedLineEquipment(current, instance, featureN);
                                }

                                ConversionAnalytics.Instance.AddConverted(tableName, globalid, name);
                            }
                            Logger.Current.DataObject(objectid, tableName, longname, System.Text.Json.JsonSerializer.Serialize(instance, ImporterNIS.jsonSerializerOptions));
                        }
                        break;

                    case "coastlinea::1": { // SLCONS_ShorelineConstruction
                            var instance = (ShorelineConstruction)ImporterNIS.Build("SLCONS", feature, buffer);

                            using var featureN = featureClass.CreateRow(buffer);
                            var name = featureN.UID();

                            if (FeatureRelations.Instance.HasSlaves(globalid)) {
                                relatedEquipment!.CreateRelatedAreaEquipment(current, instance, featureN, null);
                            }
                            ConversionAnalytics.Instance.AddConverted(tableName, globalid, name);
                            Logger.Current.DataObject(objectid, tableName, longname, System.Text.Json.JsonSerializer.Serialize(instance, ImporterNIS.jsonSerializerOptions));
                        }
                        break;

                    case "coastlinel::5": { // SLCONS_ShorelineConstruction
                            var instance = (ShorelineConstruction)ImporterNIS.Build("SLCONS", feature, buffer);

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
                                                buffer["informationbindings"] = System.Text.Json.JsonSerializer.Serialize(spatialQuality, ImporterNIS.jsonSerializerOptions);
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
                                buffer["informationbindings"] = "[]";

                                SetShape(buffer, p.geometry);

                                p.callback?.Invoke();

                                using var featureN = featureClass.CreateRow(buffer);
                                var name = featureN.UID();

                                if (FeatureRelations.Instance.HasSlaves(globalid)) {
                                    relatedEquipment?.CreateRelatedLineEquipment(current, instance, featureN);
                                }

                                ConversionAnalytics.Instance.AddConverted(tableName, globalid, name);
                            }
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
