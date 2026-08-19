using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using S100FC;
using S100FC.S101.FeatureTypes;
using S100FC.S128.ComplexAttributes;
using S100Framework.Applications.Singletons;
using System.Text.Json;
using VortexLoader.Singletons;

namespace S100Framework.Applications
{
    using ArcGIS.Core.Data.UtilityNetwork.Trace;
    using NetTopologySuite.Utilities;
    using S100FC.S101.SimpleAttributes;
    using S100FC.S128;
    using S100FC.S128.FeatureTypes;
    using S100FC.S128.SimpleAttributes;
    using S100Framework.Applications.S57.esri;
    using System.Text.RegularExpressions;

    internal static partial class ImporterNIS
    {
        public record S101ProductCoverage(string Name, int PLTS_COMP_SCALE, DataCoverage DataCoverage, S100FC.S101.SimpleAttributes.verticalDatum? VDAT, S100FC.S101.SimpleAttributes.verticalDatum? SDAT, Polygon Coverage, int specificUsage);

        private static int? optimumDisplayScaleConverter(int optimumDisplayScale) => optimumDisplayScale switch {
            >= 10000000 => null,
            >= 3500000 => 3500000,
            >= 1500000 => 1500000,
            >= 700000 => 700000,
            >= 350000 => 350000,
            >= 180000 => 180000,
            >= 90000 => 90000,
            >= 45000 => 45000,
            >= 22000 => 22000,
            >= 12000 => 12000,
            >= 8000 => 8000,
            >= 4000 => 4000,
            >= 3000 => 3000,
            >= 2000 => 2000,
            >= 1000 => 1000,
            _ => 1000,
        };

        private static int? minimumDisplayScaleConverter(int optimumDisplayScale) => optimumDisplayScale switch {
            >= 10000000 => null,
            >= 3500000 => 10000000,
            >= 1500000 => 3500000,
            >= 700000 => 1500000,
            >= 350000 => 700000,
            >= 180000 => 350000,
            >= 90000 => 180000,
            >= 45000 => 90000,
            >= 22000 => 45000,
            >= 12000 => 22000,
            >= 8000 => 12000,
            >= 4000 => 8000,
            >= 3000 => 4000,
            >= 2000 => 3000,
            >= 1000 => 2000,
            _ => 1000,
        };

        private static void S57_ProductCoverage_Full(Geodatabase source, Geodatabase target128, QueryFilter filter, int minimumDisplayScale, ref S101ProductCoverage[] coverages, string datasets, Action<S101ProductCoverage[]> createProductCoverages) {
            JsonSerializerOptions jsonSerializerOptions128 = new JsonSerializerOptions {
                WriteIndented = false,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                PropertyNameCaseInsensitive = true,
            }.AppendTypeInfoResolver();

            coverages = [];

            (Guid globalid, string DSNM, int? optimumDisplayScale, int PLTS_COMP_SCALE, ElectronicProduct s57, ElectronicProduct s101, Polygon fullCoverage, Polygon coverage, S100FC.S101.SimpleAttributes.verticalDatum vdat, S100FC.S101.SimpleAttributes.verticalDatum sdat)[] products = [];

            using var metadataAFeatureClass = source.OpenDataset<FeatureClass>(source.GetName("MetaDataA"));

            //  ProductCoverage
            {
                using var productDefinitionsTable = source.OpenDataset<Table>(source.GetName("ProductDefinitions"));
                using var productCoverageFeatureClass = source.OpenDataset<FeatureClass>(source.GetName("ProductCoverage"));

                var whereclause = $"({filter.WhereClause.Replace("PLTS_COMP_SCALE", "CSCL")})";

                using var productDefinitions = productDefinitionsTable.Search(new QueryFilter {
                    WhereClause = $"({whereclause}) AND (EXPORTTYPE IS NULL OR EXPORTTYPE <> 'Cancel')",
                    PostfixClause = "ORDER BY CSCL DESC",
                }, true);

                var regex = string.IsNullOrEmpty(datasets) ? new Regex(".*") : new Regex(datasets);

                while (productDefinitions.MoveNext()) {
                    var row = productDefinitions.Current;
                    var current = new ProductDefinitions(row);

                    var globalid = current.GLOBALID;

                    var dsnm57 = current.DSNM ?? default;
                    var edtn = current.EDTN ?? default;
                    var updn = current.UPDN ?? default;
                    var isdt = current.ISDT ?? DateTime.Now;
                    var serie = current.SERIES ?? default;

                    if (dsnm57 is null || !regex.IsMatch(dsnm57))
                        continue;
                    if (serie == default) {
                        serie = dsnm57!.Substring(0, 3);
                    }

                    var dsnm101 = $"101{dsnm57!.Substring(0, 2)}00{dsnm57!.Substring(2)}";

                    var specificUsage = SpecificUsage(current.CSCL!.Value);

                    var electronicProduct57 = new S100FC.S128.FeatureTypes.ElectronicProduct {
                        catalogueElementClassification = [1], // catalogueElementClassification.Enc
                        editionNumber = edtn,
                        updateNumber = updn,
                        issueDate = DateOnly.FromDateTime(isdt),
                        notForNavigation = false,
                        typeOfProductFormat = 2,    //typeOfProductFormat.IsoIec8211,
                        datasetName = dsnm57,
                        specificUsage = specificUsage,
                        productSpecification = new productSpecification {
                            editionDate = new DateOnly(2000, 11, 1),
                            name = "S-57",
                            version = "3.1",
                        },
                        maximumDisplayScale = null,
                        optimumDisplayScale = optimumDisplayScaleConverter(current.CSCL!.Value),
                        //minimumDisplayScale = minimumDisplayScaleConverter(current.CSCL!.Value)
                    };

                    var electronicProduct101 = new S100FC.S128.FeatureTypes.ElectronicProduct {
                        catalogueElementClassification = [1], // catalogueElementClassification.Enc
                        editionNumber = 0,
                        updateNumber = 0,
                        //issueDate = DateOnly.FromDateTime(isdt),
                        notForNavigation = true,
                        typeOfProductFormat = 2,    //typeOfProductFormat.IsoIec8211,
                        datasetName = dsnm101,
                        specificUsage = specificUsage,
                        productSpecification = new productSpecification {
                            editionDate = S100FC.S101.Summary.VersionDate,
                            name = S100FC.S101.Summary.ProductId,
                            version = S100FC.S101.Summary.Version.ToString(),
                        },
                        maximumDisplayScale = null,
                        optimumDisplayScale = optimumDisplayScaleConverter(current.CSCL!.Value),
                        //minimumDisplayScale = minimumDisplayScaleConverter(current.CSCL!.Value)
                    };

                    (int catcov, Polygon shape)[] polygons = [];

                    using var cursorCoverage = productCoverageFeatureClass.Search(new QueryFilter {
                        WhereClause = $"Product_GUID = '{globalid:B}'",
                    }, true);

                    while (cursorCoverage.MoveNext()) {
                        var catvoc = Convert.ToInt32(cursorCoverage.Current["CATCOV"]);
                        polygons = [.. polygons, (catvoc, (Polygon)((Feature)cursorCoverage.Current).GetShape().Clone())];
                    }

                    if (!polygons.Any()) System.Diagnostics.Debugger.Break();

                    var coverageFull = (Polygon)(GeometryEngine.Instance.Union([.. polygons.Select(e => e.shape)]));
                    var coverage = (Polygon)GeometryEngine.Instance.Union(polygons.Where(e => e.catcov == 1).Select(e => e.shape));

                    var vdat = GetVerticalDatum(current.VDAT ?? 3, coverage)!;
                    var sdat = GetSoundingDatum(current.SDAT!.Value, coverage)!;

                    electronicProduct57.verticalDatum = sdat!.value;
                    electronicProduct101.verticalDatum = sdat!.value;

                    products = [.. products, (globalid, dsnm57, optimumDisplayScaleConverter(current.CSCL!.Value), current.CSCL!.Value, electronicProduct57, electronicProduct101, coverageFull, coverage, vdat, sdat)];
                }
            }

            var scales = products.Select(e => e.optimumDisplayScale).Distinct().OrderByDescending(e => e).ToArray();

            int[] _cscl = [];

            for (int i = 0; i < scales.Length; i++) {
                var optimumDisplayScale = scales[i];

                var _minimum = i == 0 ? minimumDisplayScaleConverter(minimumDisplayScale) : scales[i - 1];

                foreach (var product in products.Where(e => e.optimumDisplayScale == optimumDisplayScale)) {

                    var hit = products.Where(e => e.optimumDisplayScale > optimumDisplayScale).Where(e => GeometryEngine.Instance.Within(product.coverage, e.fullCoverage));
                    if (hit.Any()) {
                        _minimum = hit.OrderByDescending(e => e.optimumDisplayScale).Last().optimumDisplayScale;
                    }

                    product.s57.minimumDisplayScale = _minimum;
                    product.s101.minimumDisplayScale = _minimum;

                    product.s57.maximumDisplayScale = product.s57.optimumDisplayScale / 2;
                    product.s101.maximumDisplayScale = product.s101.optimumDisplayScale / 2;

                    var m_cscl = Geometries.Features<MetaDataA>(metadataAFeatureClass, new SpatialQueryFilter {
                        WhereClause = $"(PLTS_COMP_SCALE = {product.PLTS_COMP_SCALE}) AND fcsubtype = 20",
                        SpatialRelationship = SpatialRelationship.Contains,
                        FilterGeometry = product.coverage,
                    });

                    var _geometry = product.coverage.Clone();

                    if (m_cscl.Any()) {
                        foreach (var e in m_cscl) {
                            if (Array.IndexOf(_cscl, e.OBJECTID!.Value) >= 0) System.Diagnostics.Debugger.Break();
                            _cscl = [.. _cscl, e.OBJECTID!.Value];

                            coverages = [.. coverages, new S101ProductCoverage(
                                product.s101.datasetName!,
                                product.s101.optimumDisplayScale!.Value,    //e.CSCALE!.Value,
                                new DataCoverage{
                                    minimumDisplayScale = product.s101.minimumDisplayScale,
                                    optimumDisplayScale = optimumDisplayScaleConverter(e.CSCALE!.Value),
                                    maximumDisplayScale = optimumDisplayScaleConverter(e.CSCALE!.Value)/2,
                                },
                                product.vdat,
                                product.sdat,
                                (Polygon)e.Shape!,
                                product.s101.specificUsage!.Value)];

                            if (GeometryEngine.Instance.Disjoint(_geometry, (Polygon)e.Shape!)) System.Diagnostics.Debugger.Break();

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

                    var dataCoverage = new DataCoverage {
                        minimumDisplayScale = product.s101.minimumDisplayScale,
                        optimumDisplayScale = product.s101.optimumDisplayScale,
                        maximumDisplayScale = product.s101.maximumDisplayScale,                        
                    };

                    coverages = [.. coverages, new S101ProductCoverage(
                            product.s101.datasetName!,
                            product.s101.optimumDisplayScale!.Value,
                            dataCoverage,
                            product.vdat,
                            product.sdat,
                            (Polygon)_geometry,
                            product.s101.specificUsage!.Value)];
                }
            }

            createProductCoverages?.Invoke(coverages);

            using (var featureClass = target128.OpenDataset<FeatureClass>(target128.GetName("surface"))) {
                using var buffer = featureClass.CreateRowBuffer();
                buffer["ps"] = ps128;

                foreach(var product in products) {
                    SetShape(buffer, product.fullCoverage);

                    buffer["code"] = product.s57.S100FC_code;
                    buffer["attributebindings"] = product.s57.Flatten();
                    buffer["informationbindings"] = "[]";
                    buffer["featurebindings"] = "[]";
                    //buffer["specificusage"] = product.s57.specificUsage!.Value;
                    buffer["sourceIdentifier"] = product.s57.sourceIdentifier;
                    buffer["nominalscale"] = product.s57.optimumDisplayScale;
                    var s57 = featureClass.CreateRow(buffer);
                    var s57UID = s57.UID();

                    buffer["code"] = product.s101.S100FC_code;
                    buffer["attributebindings"] = product.s101.Flatten();
                    buffer["informationbindings"] = "[]";
                    buffer["featurebindings"] = "[]";
                    //buffer["specificusage"] = product.s101.specificUsage!.Value;
                    buffer["sourceIdentifier"] = product.s101.sourceIdentifier;
                    buffer["nominalscale"] = product.s101.optimumDisplayScale;
                    var s101 = featureClass.CreateRow(buffer);
                    var s101UID = s101.UID();

                    var productMappingS57theReference = new featureBinding<S100FC.S128.FeatureAssociation.ProductMapping> {
                        role = "theReference",
                        roleType = "association",
                        featureId = s101UID,
                        featureType = product.s101.S100FC_code,
                    };
                    ((S100FC.S128.FeatureAssociation.ProductMapping)productMappingS57theReference.association!).categoryOfProductMapping = 1;  //  Higher Priority Alternative

                    featureBinding[] featureBindingsS57 = [productMappingS57theReference];
                    s57["featurebindings"] = System.Text.Json.JsonSerializer.Serialize(featureBindingsS57, jsonSerializerOptions128);
                    s57.Store();

                    var productMappingS101theReference = new featureBinding<S100FC.S128.FeatureAssociation.ProductMapping> {
                        role = "theReference",
                        roleType = "association",
                        featureId = s57UID,
                        featureType = product.s57.S100FC_code,
                    };
                    ((S100FC.S128.FeatureAssociation.ProductMapping)productMappingS101theReference.association!).categoryOfProductMapping = 2;  //  Lower Priority Alternative

                    featureBinding[] featureBindingsS101 = [productMappingS101theReference];
                    s101["featurebindings"] = System.Text.Json.JsonSerializer.Serialize(featureBindingsS101, jsonSerializerOptions128);
                    s101.Store();
                }
            }
        }
    }
}
