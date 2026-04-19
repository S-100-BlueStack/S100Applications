using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using NetTopologySuite.Index.HPRtree;
using S100FC;
using S100FC.S101.FeatureTypes;
using S100FC.S101.SimpleAttributes;
using S100FC.S128.SimpleAttributes;
using S100Framework.Applications.S57.esri;
using S100Framework.Applications.Singletons;
using VortexLoader.Singletons;
using static System.Net.Mime.MediaTypeNames;


namespace S100Framework.Applications
{
    internal static partial class ImporterNIS
    {
        private static void S101_SoundingDatum(Geodatabase source, Geodatabase target, QueryFilter filter, S101ProductCoverage[] coverages) {
            using var metadataA = source.OpenDataset<FeatureClass>(source.GetName("MetaDataA"));

            //  M_SDAT
            using var search = metadataA.Search(new QueryFilter {
                WhereClause = $"({filter.WhereClause} AND fcsubtype = 45)",
            }, true);

            (S100FC.S101.FeatureTypes.SoundingDatum SDAT, int PLTS_COMP_SCALE, Polygon shape)[] verticalDatums = [];

            while (search.MoveNext()) {
                var m_sdat = new MetaDataA((Feature)search.Current);

                var instance = new SoundingDatum {
                    verticalDatum = GetVerticalDatum(m_sdat.VERDAT ?? 3)!.value,
                };

                if (!string.IsNullOrEmpty(m_sdat.SORDAT)) {
                    //  TODO SORDAT ???
                }

                verticalDatums = [.. verticalDatums, (instance!, m_sdat.PLTS_COMP_SCALE!.Value, (Polygon)m_sdat.SHAPE!)];
            }

            if (verticalDatums.Any() && System.Diagnostics.Debugger.IsAttached) System.Diagnostics.Debugger.Break();

            //  Clip DataCoverage geometries
            var combined = PolygonBuilderEx.CreatePolygon(verticalDatums.Select(e => e.shape));

            (SoundingDatum SoundingDatum, Polygon Coverage)[] soundingDatums = verticalDatums.Select(e => (e.SDAT, e.shape)).ToArray();

            foreach (var c in coverages) {
                if (GeometryEngine.Instance.Disjoint(c.Coverage, combined)) {
                    soundingDatums = [.. soundingDatums, (new SoundingDatum {
                        verticalDatum = c.VDAT!.value,
                    }, c.Coverage)];
                    continue;
                }

                var difference = GeometryEngine.Instance.Difference(c.Coverage, combined);

                var multipart = c.Coverage;

                if (difference is Polygon polygon) {
                    if (polygon.ExteriorRingCount > 1) {
                        Polygon[] polygons = [];
                        ReadOnlySegmentCollection[] segments = [polygon.Parts[0]];
                        for (int x = 1; x < polygon.PartCount; x++) {
                            var p = PolygonBuilderEx.CreatePolygon(polygon.Parts[x]);
                            if (p.Area < 0)
                                segments = [.. segments, polygon.Parts[x]];
                            else {
                                var _ = PolygonBuilderEx.CreatePolygon(segments);
                                polygons = [.. polygons, _];
                                segments = [polygon.Parts[x]];
                            }
                        }
                        if (segments.Any()) {
                            var _ = PolygonBuilderEx.CreatePolygon(segments);
                            polygons = [.. polygons, _];
                        }

                        multipart = PolygonBuilderEx.CreatePolygon(polygons);
                    }
                    else
                        multipart = PolygonBuilderEx.CreatePolygon(polygon);
                }
                else
                    System.Diagnostics.Debugger.Break();

                if (multipart.IsEmpty) continue;

                foreach (var p in multipart.Split()) {
                    soundingDatums = [.. soundingDatums, (new SoundingDatum {
                        verticalDatum = c.VDAT!.value,
                    }, p)];
                }
            }


            //  Create SoundingDatum's
            using var featureClass = target.OpenDataset<FeatureClass>(target.GetName("surface"));

            using var buffer = featureClass.CreateRowBuffer();
            buffer["ps"] = ps101;

            foreach (var e in soundingDatums) {
                buffer["code"] = e.SoundingDatum.GetType().Name; buffer["sourceIdentifier"] = e.SoundingDatum.sourceIdentifier;
                buffer["attributebindings"] = e.SoundingDatum.Flatten();
                SetShape(buffer, e.Coverage);
                var featureN = featureClass.CreateRow(buffer);
                var name = featureN.UID();
            }
        }

#if null
        private static void S101_SoundingDatum(Geodatabase source, Geodatabase target, QueryFilter filter) {
            var metadataATableName = "MetaDataA";
            var productCoverageTableName = "ProductCoverage";

            using var metadataA = source.OpenDataset<FeatureClass>(source.GetName(metadataATableName));
            using var productCoverage = source.OpenDataset<FeatureClass>(source.GetName(productCoverageTableName));

            using var featureClass = target.OpenDataset<FeatureClass>(target.GetName("surface"));

            using var buffer = featureClass.CreateRowBuffer();

            var whereClause = filter.WhereClause.Clone();

            var m_qual_filter = "(fcsubtype = 40)";
            var cat_cov_filter = "CATCOV = 1";
            var m_sdat_filter = "(fcsubtype = 45)";

            var M_Qual_WhereFilter = new QueryFilter() {
                WhereClause = $"({whereClause}) AND {m_qual_filter}"
            };
            var productCoverageFilter = new QueryFilter() {
                WhereClause = $"({whereClause}) AND {cat_cov_filter}"
            };
            var M_SDAT_WhereFilter = new QueryFilter() {
                WhereClause = $"({whereClause}) AND {m_sdat_filter}"
            };

            int dissolved_M_QUAL_Count = 0;
            int M_SDAT_Count = 0;

            // Take all M_QUAL and cut out M_SDAT

            //var all_M_QUAL = Geometries.AllGeometries(metadataA, M_Qual_WhereFilter);
            //var all_M_SDAT = Geometries.AllGeometries(metadataA, M_SDAT_WhereFilter);

            var all_M_QUAL_geometries = Geometries.AllGeometries(metadataA, M_Qual_WhereFilter, ["verdat", "plts_comp_scale"]);
            var all_M_SDAT_geometries = Geometries.AllGeometries(metadataA, M_SDAT_WhereFilter, ["verdat", "plts_comp_scale"]);

            var uniqueComscalesMQuals = all_M_QUAL_geometries.Select(e => Convert.ToInt32(e.FieldName_FieldValue!["plts_comp_scale"])).Distinct().ToList();
            var uniqueComscalesMSdats = all_M_SDAT_geometries.Select(e => Convert.ToInt32(e.FieldName_FieldValue!["plts_comp_scale"])).Distinct().ToList();

            //if (uniqueComscalesMQuals.Count() > 1) {
            //    throw new NotSupportedException("Multiple scales not supported.");
            //}
            //if (uniqueComscalesMSdats.Count() > 1) {
            //    throw new NotSupportedException("Multiple scales not supported.");
            //}

            foreach (var scale in uniqueComscalesMQuals) {
                M_Qual_WhereFilter = new QueryFilter() {
                    WhereClause = $"(plts_comp_scale = {scale}) AND {m_qual_filter}"
                };
                productCoverageFilter = new QueryFilter() {
                    WhereClause = $"(plts_comp_scale = {scale}) AND {cat_cov_filter}"
                };
                M_SDAT_WhereFilter = new QueryFilter() {
                    WhereClause = $"(plts_comp_scale = {scale}) AND {m_sdat_filter}"
                };

                all_M_QUAL_geometries = Geometries.AllGeometries(metadataA, M_Qual_WhereFilter, ["verdat", "plts_comp_scale"]);
                all_M_SDAT_geometries = Geometries.AllGeometries(metadataA, M_SDAT_WhereFilter, ["verdat", "plts_comp_scale"]);


                var all_M_QUAL_dissolved = Geometries.GetDissolvedClipped(metadataA, M_Qual_WhereFilter, productCoverage, productCoverageFilter);

                var all_dissolved_M_QUALs_without_M_SDATs = Geometries.EraseTouchingParts(all_M_QUAL_dissolved, [.. all_M_SDAT_geometries.Select(e => e.Geometry)]);

                // Store all dissolved m_quals
                foreach (var item in all_dissolved_M_QUALs_without_M_SDATs) {


                    //TODO: ??? loop ??

                    verticalDatum? soundingDatum = default;
                    foreach (var elm in SoundingDatums.Instance.Touch(item)) {
                        soundingDatum = elm.Item2;
                    }

                    if (item.IsEmpty) {
                        continue;
                    }

                    var instance = new SoundingDatum {
                    };

                    //foreach (var datum in Geometries.GetTouchingOrIntersectingGeometries(all_M_QUAL_geometries,item)) {
                    //    instance.verticalDatum = EnumHelper.GetEnumValue(datum.FieldName_FieldValue!["SDAT"].ToString()!); // DomainModel.S101.verticalDatum.BalticSeaChartDatum2000;
                    //}

                    if (soundingDatum == default) {
                        throw new ArgumentException("Cannot set sounding datum.");
                    }
                    instance.verticalDatum = soundingDatum!.value;


                    // Clear vdat if covered by a metadata object with same vdat

                    buffer["ps"] = ps101;
                    buffer["code"] = instance.GetType().Name; buffer["sourceIdentifier"] = instance.sourceIdentifier;

                    buffer["attributebindings"] = instance.Flatten();
                    //buffer["informationbindings"] = "[]";

                    SetShape(buffer, item);
                    ImporterNIS.SetUsageBand(buffer, uniqueComscalesMQuals[0]);
                    dissolved_M_QUAL_Count++;
                    var featureN = featureClass.CreateRow(buffer);
                    var name = featureN.UID();
                }

                // Add all M_SDATs
                foreach (var item in all_M_SDAT_geometries) {
                    if (item.Geometry!.IsEmpty) {
                        continue;
                    }

                    var instance = new SoundingDatum {
                        verticalDatum = EnumHelper.GetEnumValue(item.FieldName_FieldValue!["verdat"])
                    };

                    buffer["ps"] = ps101;
                    buffer["code"] = instance.GetType().Name; buffer["sourceIdentifier"] = instance.sourceIdentifier;

                    buffer["attributebindings"] = instance.Flatten();
                    //buffer["informationbindings"] = "[]";

                    SetShape(buffer, item.Geometry);
                    ImporterNIS.SetUsageBand(buffer, uniqueComscalesMSdats[0]);

                    var featureN = featureClass.CreateRow(buffer);
                    var name = featureN.UID();
                    M_SDAT_Count++;
                }
            }

            Logger.Current.DataTotalCount("M_SDAT", M_SDAT_Count, M_SDAT_Count);
            Logger.Current.DataTotalCount("M_QUAL", dissolved_M_QUAL_Count, dissolved_M_QUAL_Count);
        }
#endif
    }
}