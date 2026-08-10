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

        //private static int? minimumDisplayScaleConverter(int optimumDisplayScale) => optimumDisplayScale switch {
        //    //_ => default,
        //    //>= 10000000 => default,
        //    >= 3500000 => 10000000,
        //    >= 1500000 => 3500000,
        //    >= 700000 => 1500000,
        //    >= 350000 => 700000,
        //    >= 180000 => 350000,
        //    >= 90000 => 180000,
        //    >= 45000 => 90000,
        //    >= 22000 => 45000,
        //    >= 12000 => 22000,
        //    >= 9000 => 12000,
        //    >= 4000 => 8000,
        //    >= 3000 => 4000,
        //    >= 2000 => 3000,
        //    >= 1000 => 2000,
        //    _ => throw new NotImplementedException(),
        //};

        private static void S57_ProductCoverage_Full(Geodatabase source, Geodatabase target128, QueryFilter filter, int minimumDisplayScale, ref S101ProductCoverage[] coverages, string datasets, Action<S101ProductCoverage[]> createProductCoverages) {
            JsonSerializerOptions jsonSerializerOptions128 = new JsonSerializerOptions {
                WriteIndented = false,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                PropertyNameCaseInsensitive = true,
            }.AppendTypeInfoResolver();

            coverages = [];

            (Guid globalid, string DSNM, int optimumDisplayScale, ElectronicProduct s57, ElectronicProduct s101, Polygon fullCoverage, Polygon coverage, S100FC.S101.SimpleAttributes.verticalDatum vdat, S100FC.S101.SimpleAttributes.verticalDatum sdat)[] products = [];

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
                        optimumDisplayScale = current.CSCL!.Value,
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
                        optimumDisplayScale = current.CSCL!.Value,
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

                    products = [.. products, (globalid, dsnm57, current.CSCL!.Value!, electronicProduct57, electronicProduct101, coverageFull, coverage, vdat, sdat)];
                }
            }

            var scales = products.Select(e => e.optimumDisplayScale).Distinct().OrderByDescending(e => e).ToArray();

            int[] _cscl = [];

            for (int i = 0; i < scales.Length; i++) {
                var optimumDisplayScale = scales[i];

                var _minimum = i == 0 ? minimumDisplayScale : scales[i - 1];

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
                        WhereClause = $"(PLTS_COMP_SCALE = {product.optimumDisplayScale}) AND fcsubtype = 20",
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
                                e.CSCALE!.Value,
                                new DataCoverage{
                                    minimumDisplayScale = product.s101.minimumDisplayScale,
                                    optimumDisplayScale = e.CSCALE!.Value,
                                    maximumDisplayScale = e.CSCALE!.Value/2,
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
                        drawingIndex = product.s101.specificUsage,
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
                    buffer["specificusage"] = product.s57.specificUsage!.Value;
                    buffer["sourceIdentifier"] = product.s57.sourceIdentifier;
                    buffer["nominalscale"] = product.s57.optimumDisplayScale;
                    var s57 = featureClass.CreateRow(buffer);
                    var s57UID = s57.UID();

                    buffer["code"] = product.s101.S100FC_code;
                    buffer["attributebindings"] = product.s101.Flatten();
                    buffer["informationbindings"] = "[]";
                    buffer["featurebindings"] = "[]";
                    buffer["specificusage"] = product.s101.specificUsage!.Value;
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

        private static void S57_ProductCoverage_Full_Legacy(Geodatabase source, Geodatabase target128, QueryFilter filter, int minimumDisplayScale2, ref S101ProductCoverage[] converages, string datasets, Action<S101ProductCoverage[]> createProductCoverages) {
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
                PostfixClause = "ORDER BY CSCL DESC",
            }, true);

            (string Name, int PLTS_COMP_SCALE, DataCoverage DataCoverage, S100FC.S101.SimpleAttributes.verticalDatum? VDAT, S100FC.S101.SimpleAttributes.verticalDatum? SDAT, Polygon[] Coverage)[] coverages = [];

            var regex = string.IsNullOrEmpty(datasets) ? new Regex(".*") : new Regex(datasets);

            (Guid globalid, ElectronicProduct s57, ElectronicProduct s101, Polygon shape)[] electronicProducts = [];

            (Guid productid, Polygon coverage, Polygon fullCoverage)[] coverageByProduct = [];

            var dictionaryCoverage = new Dictionary<string, Polygon>();

            //var dictionaryDataCoverage = new Dictionary<string, DataCoverage>();

            while (productDefinitions.MoveNext()) {
                recordCount += 1;
                var row = productDefinitions.Current;
                var current = new ProductDefinitions(row); // (Row)cursor.Current;

                var objectid = current.OBJECTID ?? default;
                var globalid = current.GLOBALID;

                if (ConversionAnalytics.Instance.IsConverted(globalid)) {
                    continue;
                }

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
                    optimumDisplayScale = current.CSCL!.Value,
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
                    optimumDisplayScale = current.CSCL!.Value,
                    //minimumDisplayScale = minimumDisplayScaleConverter(current.CSCL!.Value)
                };

                using var cursorCoverage = productCoverageFeatureClass.Search(new QueryFilter {
                    WhereClause = $"Product_GUID = '{globalid:B}' AND CATCOV = 1",
                }, true);

                (int catcov, Polygon shape)[] polygons = [];

                //int polygonsCompScale = 0;                

                while (cursorCoverage.MoveNext()) {
                    var catvoc = Convert.ToInt32(cursorCoverage.Current["CATCOV"]);
                    polygons = [.. polygons, (catvoc, (Polygon)((Feature)cursorCoverage.Current).GetShape().Clone())];
                }
                if (!polygons.Any()) System.Diagnostics.Debugger.Break();

                var coverage = (Polygon)GeometryEngine.Instance.Union(polygons.Where(e => e.catcov == 1).Select(e => e.shape));

                var _coverage = (Polygon)(GeometryEngine.Instance.Union([.. polygons.Select(e => e.shape)]));

                coverageByProduct = [.. coverageByProduct, (globalid, coverage, _coverage)];

                dictionaryCoverage.Add(electronicProduct101.datasetName, coverage);
                /*
                                var _minimumDisplayScale = 10000000;

                                var hit = coverages.Where(e => e.PLTS_COMP_SCALE > current.CSCL!.Value && GeometryEngine.Instance.Within(coverage, dictionaryCoverage[e.Name]));

                                if (hit.Any()) {
                                    //_minimumDisplayScale = hit.OrderBy(e => e.PLTS_COMP_SCALE).First().PLTS_COMP_SCALE;
                                    _minimumDisplayScale = hit.OrderBy(e => e.DataCoverage.maximumDisplayScale).First().DataCoverage.maximumDisplayScale!.Value;
                                }

                                dictionaryCoverage.Add(electronicProduct101.datasetName, coverage);

                                electronicProduct57.minimumDisplayScale = electronicProduct101.minimumDisplayScale = _minimumDisplayScale;

                                var dataCoverage = new DataCoverage {
                                    maximumDisplayScale = current.CSCL!.Value / 2,
                                    optimumDisplayScale = current.CSCL!.Value,
                                    minimumDisplayScale = _minimumDisplayScale,
                                };

                                dictionaryDataCoverage.Add(electronicProduct101.datasetName, dataCoverage);                
                */
                var vdat = GetVerticalDatum(current.VDAT ?? 3, _coverage);
                var sdat = GetSoundingDatum(current.SDAT!.Value, _coverage);

                electronicProduct57.verticalDatum = sdat!.value;
                electronicProduct101.verticalDatum = sdat!.value;

                var dataCoverage = new DataCoverage {
                    //maximumDisplayScale = current.CSCL!.Value / 2,
                    optimumDisplayScale = current.CSCL!.Value,
                    //minimumDisplayScale = _minimumDisplayScale,
                };

                coverages = [.. coverages, (dsnm101, current.CSCL!.Value, dataCoverage, vdat, sdat, polygons.Where(e => e.catcov == 1).Select(e => e.shape).ToArray())];

                {
                    using var _ = productCoverageFeatureClass.Search(new QueryFilter {
                        WhereClause = $"Product_GUID = '{globalid:B}'",
                    }, true);

                    Polygon[] productCoverages = [];
                    while (_.MoveNext()) {
                        productCoverages = [.. productCoverages, (Polygon)((Feature)_.Current).GetShape().Clone()];
                    }

                    electronicProducts = [.. electronicProducts, (globalid, electronicProduct57, electronicProduct101, (Polygon)(GeometryEngine.Instance.Union(productCoverages)))];
                }
            }

            using (var featureClass = target128.OpenDataset<FeatureClass>(target128.GetName("surface"))) {
                using var buffer = featureClass.CreateRowBuffer();
                buffer["ps"] = ps128;

                foreach (var electronicProduct in electronicProducts.OrderByDescending(e => e.s101.optimumDisplayScale!.Value)) {
                    SetShape(buffer, electronicProduct.shape);

                    var _minimumDisplayScale = 10000000;

                    var hit = coverages.Where(e => e.PLTS_COMP_SCALE > electronicProduct.s101.optimumDisplayScale!.Value && GeometryEngine.Instance.Within(electronicProduct.shape, dictionaryCoverage[e.Name]));

                    if (hit.Any()) {
                        //_minimumDisplayScale = hit.OrderBy(e => e.PLTS_COMP_SCALE).First().PLTS_COMP_SCALE;
                        _minimumDisplayScale = hit.OrderBy(e => e.DataCoverage.maximumDisplayScale).First().DataCoverage.maximumDisplayScale!.Value;
                    }

                    electronicProduct.s57.minimumDisplayScale = _minimumDisplayScale;
                    electronicProduct.s101.minimumDisplayScale = _minimumDisplayScale;

                    int _maximumDisplayScale = 0;
                    foreach (var f in electronicProducts.Where(e => e.s101.optimumDisplayScale < electronicProduct.s101.optimumDisplayScale)) {
                        if (GeometryEngine.Instance.Disjoint(electronicProduct.shape, f.shape)) continue;
                        if (GeometryEngine.Instance.Intersects(f.shape, electronicProduct.shape)) {
                            if (f.s101.optimumDisplayScale!.Value > _maximumDisplayScale)
                                _maximumDisplayScale = f.s101.optimumDisplayScale!.Value;
                        }
                        else System.Diagnostics.Debugger.Break();
                    }

                    electronicProduct.s57.maximumDisplayScale = _maximumDisplayScale;
                    electronicProduct.s101.maximumDisplayScale = _maximumDisplayScale;

                    var c = coverages.Single(e => e.Name.Equals(electronicProduct.s101.datasetName));
                    c.DataCoverage.minimumDisplayScale = _minimumDisplayScale;
                    c.DataCoverage.maximumDisplayScale = _maximumDisplayScale;

                    //dictionaryDataCoverage[electronicProduct.s101.datasetName!].maximumDisplayScale = _maximumDisplayScale;

                    buffer["code"] = electronicProduct.s57.S100FC_code;
                    buffer["attributebindings"] = electronicProduct.s57.Flatten();
                    buffer["informationbindings"] = "[]";
                    buffer["featurebindings"] = "[]";
                    buffer["specificusage"] = electronicProduct.s57.specificUsage!.Value;
                    buffer["sourceIdentifier"] = electronicProduct.s57.sourceIdentifier;
                    buffer["nominalscale"] = electronicProduct.s57.optimumDisplayScale;

                    var s57 = featureClass.CreateRow(buffer);
                    var s57UID = s57.UID();

                    buffer["code"] = electronicProduct.s101.S100FC_code;
                    buffer["attributebindings"] = electronicProduct.s101.Flatten();
                    buffer["informationbindings"] = "[]";
                    buffer["featurebindings"] = "[]";
                    buffer["specificusage"] = electronicProduct.s101.specificUsage!.Value;
                    buffer["sourceIdentifier"] = electronicProduct.s101.sourceIdentifier;
                    buffer["nominalscale"] = electronicProduct.s101.optimumDisplayScale;

                    var s101 = featureClass.CreateRow(buffer);
                    var s101UID = s101.UID();

                    var productMappingS57theReference = new featureBinding<S100FC.S128.FeatureAssociation.ProductMapping> {
                        role = "theReference",
                        roleType = "association",
                        featureId = s101UID,
                        featureType = electronicProduct.s101.S100FC_code,
                    };
                    ((S100FC.S128.FeatureAssociation.ProductMapping)productMappingS57theReference.association!).categoryOfProductMapping = 1;  //  Higher Priority Alternative

                    featureBinding[] featureBindingsS57 = [productMappingS57theReference];
                    s57["featurebindings"] = System.Text.Json.JsonSerializer.Serialize(featureBindingsS57, jsonSerializerOptions128);
                    s57.Store();

                    var productMappingS101theReference = new featureBinding<S100FC.S128.FeatureAssociation.ProductMapping> {
                        role = "theReference",
                        roleType = "association",
                        featureId = s57UID,
                        featureType = electronicProduct.s57.S100FC_code,
                    };
                    ((S100FC.S128.FeatureAssociation.ProductMapping)productMappingS101theReference.association!).categoryOfProductMapping = 2;  //  Lower Priority Alternative

                    featureBinding[] featureBindingsS101 = [productMappingS101theReference];
                    s101["featurebindings"] = System.Text.Json.JsonSerializer.Serialize(featureBindingsS101, jsonSerializerOptions128);
                    s101.Store();
                }
            }

            var scales = coverages.Select(e => e.PLTS_COMP_SCALE).Distinct().OrderByDescending(e => e).ToArray();

            S101ProductCoverage[] products = [];

            int[] _cscl = [];

            for (int i = 0; i < scales.Length; i++) {
                foreach (var coverage in coverages.Where(e => e.PLTS_COMP_SCALE == scales[i])) {
                    Polygon[] polygons = [];

                    var _minimum = i == 0 ? minimumDisplayScale2 : scales[i - 1];

                    foreach (var c in coverage.Coverage) {
                        //  PLTS_COMP_SCALE >= 0 AND PLTS_COMP_SCALE < 19999999

                        var m_cscl = Geometries.Features<MetaDataA>(metadataAFeatureClass, new SpatialQueryFilter {
                            //WhereClause = $"(PLTS_COMP_SCALE >= {scales[i]} AND PLTS_COMP_SCALE < {_minimum}) AND fcsubtype = 20",
                            WhereClause = $"(PLTS_COMP_SCALE = {coverage.PLTS_COMP_SCALE}) AND fcsubtype = 20",
                            SpatialRelationship = SpatialRelationship.Contains,
                            FilterGeometry = c,
                        });

                        var _geometry = (Polygon)c.Clone();

                        if (m_cscl.Any()) {
                            foreach (var e in m_cscl) {
                                if (Array.IndexOf(_cscl, e.OBJECTID!.Value) >= 0) System.Diagnostics.Debugger.Break();
                                _cscl = [.. _cscl, e.OBJECTID!.Value];

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

            createProductCoverages.Invoke(products);

            //using (var featureClass = target101.OpenDataset<FeatureClass>(target101.GetName("surface"))) {
            //    using var buffer = featureClass.CreateRowBuffer();
            //    buffer["ps"] = ps101;

            //    foreach (var c in products) {
            //        buffer["code"] = c.DataCoverage.GetType().Name;
            //        buffer["attributebindings"] = c.DataCoverage.Flatten();
            //        buffer["informationbindings"] = "[]";
            //        buffer["featurebindings"] = "[]";
            //        buffer["specificusage"] = c.specificUsage;
            //        buffer["sourceIdentifier"] = c.DataCoverage.sourceIdentifier;

            //        foreach (var p in c.Coverage.Split()) {
            //            SetShape(buffer, p);
            //            using var featureN = featureClass.CreateRow(buffer);
            //            var name = featureN.UID();
            //        }
            //    }

            //    foreach (var c in products) {
            //        var vdat = new VerticalDatumOfData {
            //            verticalDatum = c.VDAT?.value,
            //        };

            //        buffer["code"] = vdat.GetType().Name;
            //        buffer["attributebindings"] = vdat.Flatten();
            //        buffer["informationbindings"] = "[]";
            //        buffer["featurebindings"] = "[]";
            //        buffer["specificusage"] = c.specificUsage;
            //        buffer["sourceIdentifier"] = vdat.sourceIdentifier;

            //        foreach (var p in c.Coverage.Split()) {
            //            SetShape(buffer, p);
            //            using var featureN = featureClass.CreateRow(buffer);
            //            var name = featureN.UID();

            //            VerticalDatums.Instance.Add(p, vdat.verticalDatum);

            //            SoundingDatums.Instance.Add(p, c.SDAT!);
            //        }
            //    }
            //}
            Logger.Current.DataTotalCount(tableName, recordCount, ConversionAnalytics.Instance.GetConvertedCount(tableName));
        }

    }
}
