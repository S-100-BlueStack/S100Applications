using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using S100FC;
using S100FC.S101.FeatureTypes;
using S100FC.S128.ComplexAttributes;
using S100Framework.Applications.S57.esri;
using S100Framework.Applications.Singletons;
using System.Text.Json;
using VortexLoader.Singletons;

namespace S100Framework.Applications
{
    using ArcGIS.Core.Data.UtilityNetwork.Trace;
    using S100FC.S128;
    using S100Framework.Applications.S57auto.esri;
    using System.Text.RegularExpressions;

    internal static partial class ImporterNIS
    {
        public record S101ProductCoverage(string Name, int PLTS_COMP_SCALE, DataCoverage DataCoverage, S100FC.S101.SimpleAttributes.verticalDatum? VDAT, S100FC.S101.SimpleAttributes.verticalDatum? SDAT, Polygon Coverage, int specificUsage);

        private static int? OptimimDisplayScaleConverter(int optimumDisplayScale) => optimumDisplayScale switch {
            >= 10000000 => default,
            >= 3500000 => 10000000,
            >= 1500000 => 3500000,
            >= 700000 => 1500000,
            >= 350000 => 700000,
            >= 180000 => 350000,
            >= 90000 => 180000,
            >= 45000 => 90000,
            >= 22000 => 45000,
            >= 12000 => 22000,
            >= 9000 => 12000,
            >= 4000 => 8000,
            >= 3000 => 4000,
            >= 2000 => 3000,
            >= 1000 => 2000,
            _ => throw new NotImplementedException(),
        };

        private static void S57_ProductCoverage_Full(Geodatabase source, Geodatabase target, QueryFilter filter, bool s128, ref S101ProductCoverage[] converages, string datasets = "") {
            JsonSerializerOptions jsonSerializerOptions128 = new JsonSerializerOptions {
                WriteIndented = false,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                PropertyNameCaseInsensitive = true,
            }.AppendTypeInfoResolver();

            converages = [];

            var tableName = "ProductCoverage";

            using var productDefinitionsTable = source.OpenDataset<Table>(source.GetName("ProductDefinitions"));
            using var productCoverageFeatureClass = source.OpenDataset<FeatureClass>(source.GetName("ProductCoverage"));
            using var metadataAFeatureClass = source.OpenDataset<FeatureClass>(source.GetName("MetaDataA"));

            var scamin = Scamin.Instance;

            ProductCoverages.Initialize(source, QueryFilter);

            //var allM_CSCL = Geometries.Features<MetaDataA>(metadataAFeatureClass, new() { WhereClause = $"({filter.WhereClause}) AND fcsubtype = 20" });

            int recordCount = 0;

            var whereclause = $"({filter.WhereClause.Replace("PLTS_COMP_SCALE", "CSCL")})";

            using var productDefinitions = productDefinitionsTable.Search(new QueryFilter {
                WhereClause = $"({whereclause}) AND (EXPORTTYPE IS NULL OR EXPORTTYPE <> 'Cancel')",
            }, true);

            (string Name, int PLTS_COMP_SCALE, DataCoverage DataCoverage, S100FC.S101.SimpleAttributes.verticalDatum? VDAT, S100FC.S101.SimpleAttributes.verticalDatum? SDAT, Polygon[] Coverage)[] coverages = [];

            var regex = string.IsNullOrEmpty(datasets) ? new Regex(".*") : new Regex(datasets);

            while (productDefinitions.MoveNext()) {
                recordCount += 1;
                var row = productDefinitions.Current;
                var current = new ProductDefinitions(row); // (Row)cursor.Current;

                var objectid = current.OBJECTID ?? default;
                var globalid = current.GLOBALID;

                if (ConversionAnalytics.Instance.IsConverted(globalid)) {
                    continue;
                }

                var dsnm = current.DSNM ?? default;
                var edtn = current.EDTN ?? default;
                var updn = current.UPDN ?? default;
                var isdt = current.ISDT ?? default;
                var serie = current.SERIES ?? default;

                if (dsnm is null || !regex.IsMatch(dsnm))
                    continue;

                if (serie == default) {
                    serie = dsnm!.Substring(0, 3);
                }

                dsnm = $"101{dsnm!.Substring(0, 2)}00{dsnm!.Substring(2)}";

                ////var specificUsage = dsnm[7] switch {
                ////    '5' => 5,   //S100FC.S128.specificUsage.NavigationalPurposeHarbour,
                ////    '4' => 4,   //S100FC.S128.specificUsage.NavigationalPurposeApproach,
                ////    '3' => 3,   //S100FC.S128.specificUsage.NavigationalPurposeCoastal,
                ////    '2' => 2,   //S100FC.S128.specificUsage.NavigationalPurposeGeneral,
                ////    '1' => 1,   //S100FC.S128.specificUsage.NavigationalPurposeOverview,
                ////    _ => throw new InvalidDataException(),
                ////};
                
                var specificUsage = SpecificUsage(current.CSCL!.Value);

                var electronicProduct = new S100FC.S128.FeatureTypes.ElectronicProduct {
                    catalogueElementClassification = [1], // catalogueElementClassification.Enc
                    editionNumber = edtn,
                    updateNumber = updn,
                    issueDate = DateOnly.FromDateTime(isdt),
                    notForNavigation = true,
                    typeOfProductFormat = 2,    //typeOfProductFormat.IsoIec8211,
                    datasetName = dsnm,
                    specificUsage = specificUsage,
                    productSpecification = new productSpecification {
                        editionDate = S100FC.S101.Summary.VersionDate,
                        name = S100FC.S101.Summary.ProductId,
                        version = S100FC.S101.Summary.Version.ToString(),
                    },
                };

                using var cursorCoverage = productCoverageFeatureClass.Search(new QueryFilter {
                    WhereClause = $"Product_GUID = '{globalid:B}' AND CATCOV = 1",
                }, true);

                Polygon[] polygons = [];

                //int polygonsCompScale = 0;                

                while (cursorCoverage.MoveNext()) {
                    polygons = [.. polygons, (Polygon)((Feature)cursorCoverage.Current).GetShape().Clone()];
                }
                if (!polygons.Any()) System.Diagnostics.Debugger.Break();

                var radarScales = scamin.StandardRadarScale((Polygon)(GeometryEngine.Instance.Union(polygons)));

                //var optimumScaleIndex = Array.IndexOf(radarScales, current.CSCL!.Value);

                //if (optimumScaleIndex < 2)
                //    optimumScaleIndex = 0;
                //else
                //    optimumScaleIndex -= 2;
                // var minimumDisplayScale = radarScales[optimumScaleIndex];

                var minimumDisplayScale = OptimimDisplayScaleConverter(current.CSCL!.Value);

                var dataCoverage = new DataCoverage {
                    maximumDisplayScale = Convert.ToInt32(current.CSCL!.Value / 2),
                    optimumDisplayScale = current.CSCL!.Value,
                    minimumDisplayScale = minimumDisplayScale,
                };

                var vdat = GetVerticalDatum(current.VDAT ?? 3);
                var sdat = GetSoundingDatum(current.SDAT!.Value);

                coverages = [.. coverages, (dsnm, current.CSCL!.Value, dataCoverage, vdat, sdat, polygons)];

                if (s128) {
                    using var _ = productCoverageFeatureClass.Search(new QueryFilter {
                        WhereClause = $"Product_GUID = '{globalid:B}'",
                    }, true);

                    Polygon[] productCoverages = [];
                    while (_.MoveNext()) {
                        productCoverages = [.. productCoverages, (Polygon)((Feature)_.Current).GetShape().Clone()];
                    }

                    using (var featureClass = target.OpenDataset<FeatureClass>(target.GetName("surface"))) {

                        using var buffer = featureClass.CreateRowBuffer();
                        buffer["ps"] = ps128;

                        buffer["code"] = electronicProduct.S100FC_code;
                        buffer["attributebindings"] = electronicProduct.Flatten();
                        buffer["informationbindings"] = "[]";
                        buffer["featurebindings"] = "[]";
                        buffer["specificusage"] = electronicProduct.specificUsage.Value;

                        SetShape(buffer, (Polygon)(GeometryEngine.Instance.Union(productCoverages)));
                        using var featureN = featureClass.CreateRow(buffer);
                        var name = featureN.UID();
                    }
                }
            }

            var scales = coverages.Select(e => e.PLTS_COMP_SCALE).Distinct().OrderByDescending(e => e).ToArray();

            S101ProductCoverage[] products = [];


            for (int i = 0; i < scales.Length; i++) {
                foreach (var coverage in coverages.Where(e => e.PLTS_COMP_SCALE == scales[i])) {
                    Polygon[] polygons = [];

                    foreach (var c in coverage.Coverage) {
                        var m_cscl = Geometries.Features<MetaDataA>(metadataAFeatureClass, new SpatialQueryFilter {
                            WhereClause = $"({filter.WhereClause}) AND fcsubtype = 20",
                            SpatialRelationship = SpatialRelationship.Contains,
                            FilterGeometry = c,
                        });

                        var _geometry = (Polygon)c.Clone();

                        if (m_cscl.Any()) {
                            foreach (var e in m_cscl) {
                                products = [.. products, new S101ProductCoverage(coverage.Name, e.CSCALE!.Value, new DataCoverage {
                                    maximumDisplayScale = Convert.ToInt32(e.CSCALE!.Value / 2),
                                    optimumDisplayScale = e.CSCALE!.Value,
                                    minimumDisplayScale = coverage.DataCoverage.minimumDisplayScale,
                                }, coverage.VDAT, coverage.SDAT, (Polygon)e.SHAPE!, SpecificUsage(coverage.PLTS_COMP_SCALE))];

                                if (GeometryEngine.Instance.Disjoint(_geometry, (Polygon)e.Shape!)) continue;

                                var difference = GeometryEngine.Instance.Difference(_geometry, (Polygon)e.Shape!);

                                if (difference is Polygon polygon) {
                                    if (polygon.ExteriorRingCount > 1) {
                                        Polygon[] _polygons = [];
                                        ReadOnlySegmentCollection[] segments = [polygon.Parts[0]];
                                        for (int x = 1; x < polygon.PartCount; x++) {
                                            var p = PolygonBuilderEx.CreatePolygon(polygon.Parts[x]);
                                            if (p.Area < 0)
                                                segments = [.. segments, polygon.Parts[x]];
                                            else {
                                                var _ = PolygonBuilderEx.CreatePolygon(segments);
                                                _polygons = [.. _polygons, _];
                                                segments = [polygon.Parts[x]];
                                            }
                                        }
                                        if (segments.Any()) {
                                            var _ = PolygonBuilderEx.CreatePolygon(segments);
                                            _polygons = [.. _polygons, _];
                                        }

                                        _geometry = PolygonBuilderEx.CreatePolygon(_polygons);
                                    }
                                    else
                                        _geometry = PolygonBuilderEx.CreatePolygon(polygon);
                                }
                                else
                                    System.Diagnostics.Debugger.Break();
                            }
                        }

                        polygons = [.. polygons, _geometry];
                    }

                    var multipart = polygons.Length == 1 ? polygons[0] : PolygonBuilderEx.CreatePolygon(polygons);
                    products = [.. products, new S101ProductCoverage(coverage.Name, coverage.PLTS_COMP_SCALE, coverage.DataCoverage, coverage.VDAT, coverage.SDAT, multipart, SpecificUsage(coverage.PLTS_COMP_SCALE))];
                }
            }

            (string Name, int PLTS_COMP_SCALE, DataCoverage DataCoverage, Polygon[] Coverage)[] cscl = [];


            converages = products;
            ;

            using (var geodatabase = new Geodatabase(new MobileGeodatabaseConnectionPath(new Uri(System.IO.Path.GetFullPath("coverage.geodatabase"))))) {
                using var fc = geodatabase.OpenDataset<FeatureClass>("surface");

                var b = fc.CreateRowBuffer();
                foreach (var e in products) {
                    b["dsnm"] = e.Name;
                    b["PLTS_COMP_SCALE"] = e.PLTS_COMP_SCALE;

                    foreach (var p in e.Coverage.Split()) {
                        b["shape"] = p;
                        var _ = fc.CreateRow(b);
                        _.Store();
                    }
                }
            }

            using (var featureClass = target.OpenDataset<FeatureClass>(target.GetName("surface"))) {
                using var buffer = featureClass.CreateRowBuffer();
                buffer["ps"] = ps101;

                foreach (var c in products) {
                    buffer["code"] = c.DataCoverage.GetType().Name;
                    buffer["attributebindings"] = c.DataCoverage.Flatten();
                    buffer["informationbindings"] = "[]";
                    buffer["featurebindings"] = "[]";
                    buffer["specificusage"] = c.specificUsage;

                    foreach (var p in c.Coverage.Split()) {
                        SetShape(buffer, p);
                        using var featureN = featureClass.CreateRow(buffer);
                        var name = featureN.UID();
                    }
                }

                foreach (var c in products) {
                    var vdat = new VerticalDatumOfData {
                        verticalDatum = c.VDAT?.value,
                    };

                    buffer["code"] = vdat.GetType().Name;
                    buffer["attributebindings"] = vdat.Flatten();
                    buffer["informationbindings"] = "[]";
                    buffer["featurebindings"] = "[]";
                    buffer["specificusage"] = c.specificUsage;

                    foreach (var p in c.Coverage.Split()) {
                        SetShape(buffer, p);
                        using var featureN = featureClass.CreateRow(buffer);
                        var name = featureN.UID();

                        VerticalDatums.Instance.Add(p, vdat.verticalDatum);

                        SoundingDatums.Instance.Add(p, c.SDAT!);
                    }
                }
            }

            //System.Diagnostics.Debugger.Break();
            ;

#if null
            while (cursorCoverage.MoveNext()) {
                    var productCoverage = new ProductCoverage((Feature)cursorCoverage.Current);
                    var catcov = productCoverage.CATCOV ?? default;
                    var plts_comp_scale = productCoverage.PLTS_COMP_SCALE ?? default;

                    if (catcov != 1)
                        continue;

                    //var displayScale = DisplayScale.GetNearestBelowKey(plts_comp_scale) ?? default;
                    //var displayScale = DisplayScale.GetDisplayScale(serie!)!;
                    var dataCoverage_m_scl = new DataCoverage {
                        maximumDisplayScale = Convert.ToInt32(plts_comp_scale / 2),
                        optimumDisplayScale = plts_comp_scale, //displayScale.OptimumDisplayScale,
                        minimumDisplayScale = plts_comp_scale //displayScale.MinimumDisplayScale
                    };

                    var coverageShape = productCoverage.SHAPE!;




                    //(coverageShape as ArcGIS.Core.Geometry.Polygon).Area != (cutOutM_SCL[0] as ArcGIS.Core.Geometry.Polygon).Area
                    var cutOutM_SCL = Geometries.EraseTouchingParts([coverageShape], allM_CSCL.Select(e => e.SHAPE!).ToList());

                    //if ((coverageShape as ArcGIS.Core.Geometry.Polygon).Area != (cutOutM_SCL[0] as ArcGIS.Core.Geometry.Polygon).Area) {
                    //    ;
                    //}

                    if (cutOutM_SCL.Count == 0) {
                        throw new NotSupportedException("meta sea scale replaces coverage completely");
                    }
                    if (cutOutM_SCL.Count > 1) {
                        throw new NotSupportedException("Multiple coverages after M_SCL cut");
                    }

                    polygonsCompScale = productCoverage.PLTS_COMP_SCALE!.Value;
                    polygons.Add((ArcGIS.Core.Geometry.Polygon)productCoverage.SHAPE!);

                    {
                        //buffer["ps"] = ps128;
                        //buffer["code"] = instance.GetType().Name;
                        //buffer["version"] = ImporterNIS.s101version;
                        //buffer["__json__"] = System.Text.Json.JsonSerializer.Serialize(instance, jsonTestSerializerOptions);
                        //SetShape(buffer, productCoverage.SHAPE);
                        //SetUsageBand(buffer, productCoverage!.PLTS_COMP_SCALE!.Value);
                        //using var featureN = featureClass.CreateRow(buffer);
                        //var name = featureN.Crc32();
                        //// TODO: Create relations
                        //ConversionAnalytics.Instance.AddConverted(tableName, current.GLOBALID, name);
                    }

                    // DATACOVERAGE
                    var dataCoverage = new DataCoverage {
                        maximumDisplayScale = Convert.ToInt32(plts_comp_scale / 2),
                        optimumDisplayScale = plts_comp_scale, //displayScale.OptimumDisplayScale,
                        minimumDisplayScale = plts_comp_scale //displayScale.MinimumDisplayScale
                    };
                    {
                        buffer["ps"] = ps101;
                        buffer["code"] = dataCoverage.GetType().Name;

                        buffer["attributebindings"] = dataCoverage.Flatten();
                        buffer["informationbindings"] = "[]";

                        SetShape(buffer, cutOutM_SCL[0]); // productCoverage.SHAPE);
                        SetUsageBand(buffer, productCoverage.PLTS_COMP_SCALE!.Value);

                        using var featureN = featureClass.CreateRow(buffer);
                        var name = featureN.UID();

                        // TODO: Create relations
                        ConversionAnalytics.Instance.AddConverted(tableName, current.GLOBALID, name);
                    }

                    // VERTICAL DATUM OF DATA
                    {
                        var vdat = new VerticalDatumOfData {
                            verticalDatum = default,
                        };

                        vdat.verticalDatum = GetVerticalDatum(current.VDAT ?? 3)?.value;

                        buffer["ps"] = ps101;
                        buffer["code"] = vdat.GetType().Name;

                        buffer["attributebindings"] = vdat.Flatten();
                        buffer["informationbindings"] = "[]";

                        SetShape(buffer, productCoverage.SHAPE);
                        SetUsageBand(buffer, productCoverage.PLTS_COMP_SCALE.Value);

                        using var featureN = featureClass.CreateRow(buffer);
                        var name = featureN.UID();

                        // Registering vertical datum information for all areas
                        VerticalDatums.Instance.Add(productCoverage!.SHAPE!, vdat.verticalDatum);

                        SoundingDatums.Instance.Add(productCoverage!.SHAPE!, GetSoundingDatum(current.SDAT!.Value)!);

                        // TODO: Create relations
                        ConversionAnalytics.Instance.AddConverted(tableName, current.GLOBALID, name);

                        VerticalDatums.Instance.Add(productCoverage.SHAPE!.Clone(), vdat.verticalDatum);

                    }
                }

                if (s128) {
                    //Store S-128 polygons
                    buffer["ps"] = ps128;
                    buffer["code"] = instance.GetType().Name;

                    buffer["attributebindings"] = instance.Flatten();
                    buffer["informationbindings"] = "[]";

                    SetShape(buffer, (ArcGIS.Core.Geometry.Polygon)GeometryEngine.Instance.Union(polygons));
                    SetUsageBand(buffer, polygonsCompScale);
                    using var featureN = featureClass.CreateRow(buffer);
                    var name = featureN.UID();
                    // TODO: Create relations
                    ConversionAnalytics.Instance.AddConverted(tableName, current.GLOBALID, name);
                }

                Logger.Current.DataObject(objectid, tableName, dsnm, System.Text.Json.JsonSerializer.Serialize(instance, jsonSerializerOptions128));
            }
#endif
            Logger.Current.DataTotalCount(tableName, recordCount, ConversionAnalytics.Instance.GetConvertedCount(tableName));
        }

    }
}
