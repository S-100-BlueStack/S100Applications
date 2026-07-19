using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using S100FC;
using S100FC.S101.FeatureTypes;
using S100FC.S101.InformationAssociation;
using S100FC.S101.InformationTypes;
using S100Framework.Applications.S57.esri;
using S100Framework.Applications.Singletons;


namespace S100Framework.Applications
{
    internal static partial class ImporterNIS
    {
        /*
         *  curve
         *  point
         *  pointset
         *  surface
         *  
         */

        private static void S57_DepthsL(Geodatabase source, Geodatabase target, QueryFilter filter) {
            var tableName = "DepthsL";

            using var depthsl = source.OpenDataset<FeatureClass>(source.GetName("DepthsL"));
            Subtypes.Instance.RegisterSubtypes(depthsl);

            //using var plts_spatialattributel = source.OpenDataset<FeatureClass>(source.GetName("PLTS_SpatialAttributeL"));
            //using var informationtype = target.OpenDataset<Table>(target.GetName("informationType"));
            //using var featureClass = target.OpenDataset<FeatureClass>(target.GetName("curve"));
            //using var buffer = featureClass.CreateRowBuffer();

            //using var featureClassTopo = target.OpenDataset<FeatureClass>(target.GetName("topo_curve"));
            using var featureClassTopo = target.OpenDataset<FeatureClass>(target.GetName("curve"));
            using var bufferTopo = featureClassTopo.CreateRowBuffer();


            using var cursor = depthsl.Search(filter, true);
            int recordCount = 0;

            var spatialQuality = CreateAssociationSpatialQuality(target);

            while (cursor.MoveNext()) {
                recordCount += 1;

                var feature = (Feature)cursor.Current;

                if (feature.GetShape() is null) continue;
                if (feature.GetShape().IsEmpty) continue;

                var current = new DepthsL(feature);

                var spatialQualityHits = SpatialAssociations.Instance.GetSpatialAttributeL(feature.GetShape());

                var objectid = current.OBJECTID ?? default;
                var globalid = current.GLOBALID;
                if (FeatureRelations.Instance.IsSlave(globalid)) {
                    continue;
                }

                if (ConversionAnalytics.Instance.IsConverted(globalid)) {
                    throw new Exception("Not supported.");
                }

                var fcSubtype = current.FCSUBTYPE ?? default;
                var plts_comp_scale = current.PLTS_COMP_SCALE ?? default;
                var longname = current.LNAM ?? Strings.UNKNOWN;

                switch (fcSubtype) {
                    case 5: { // DEPCNT_DepthContour
                            var instance = new DepthContour {
                            };

                            if (current.VALDCO.HasValue) {
                                instance.valueOfDepthContour = current.VALDCO.Value;
                            }

                            // TODO: interoperabilityIdentifier

                            if (current.PLTS_COMP_SCALE.HasValue && current.SHAPE != null) {
                                string subtype = "";

                                if (current.TableName != default && current.FCSUBTYPE.HasValue && !Subtypes.Instance.TryGetSubtype(current.TableName, current.FCSUBTYPE.Value, out subtype))
                                    throw new NotSupportedException($"Unknown subtype for {current.TableName}, {current.FCSUBTYPE.Value}");

                                var scamin = Scamin.Instance.GetMinimumScale(current, subtype, current.PLTS_COMP_SCALE!.Value, isRelatedToStructure: false);
                                if (scamin.HasValue)
                                    instance.scaleMinimum = scamin.Value;
                            }

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


                            var result = ImporterNIS.AddInformation(current.OBJECTID!.Value, current.TableName!, current.NTXTDS, current.TXTDSC, current.INFORM, current.NINFOM);
                            instance.information = result.information.ToArray();
                            instance.SetInformationBindings(result.InformationBindings.ToArray());

                            bufferTopo["ps"] = ps101;
                            bufferTopo["code"] = instance.GetType().Name;

                            bufferTopo["attributebindings"] = instance.Flatten();
                            bufferTopo["informationbindings"] = System.Text.Json.JsonSerializer.Serialize(instance.GetInformationBindings(), jsonSerializerOptions);
                            bufferTopo["sourceIdentifier"] = instance.sourceIdentifier;

                            SetTopoUsageBand(bufferTopo, current.PLTS_COMP_SCALE!.Value);

                            (Polyline geometry, Action? callback)[] geometry = [((Polyline)current.SHAPE!, default)];

                            //if (objectid == 85) System.Diagnostics.Debugger.Break();

                            if (spatialQualityHits.Any()) {
                                Geometry g = current.SHAPE!;

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

                                if (FeatureRelations.Instance.HasSlaves(current.GLOBALID)) {
                                    relatedEquipment?.CreateRelatedLineEquipment(current, instance, featureN);
                                }

                                ConversionAnalytics.Instance.AddConverted(tableName, current.GLOBALID, name);
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

