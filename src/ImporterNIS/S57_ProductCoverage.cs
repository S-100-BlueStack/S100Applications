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
    using S100FC.S128;

    internal static partial class ImporterNIS
    {
        private static void S57_ProductCoverage(Geodatabase source, Geodatabase target, QueryFilter filter, bool s128) {
            return;
            JsonSerializerOptions jsonSerializerOptions128 = new JsonSerializerOptions {
                WriteIndented = false,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                PropertyNameCaseInsensitive = true,
            }.AppendTypeInfoResolver();

            var tableName = "ProductCoverage";

            using var productDefinitionsTable = source.OpenDataset<Table>(source.GetName("ProductDefinitions"));
            using var productCoverageFeatureClass = source.OpenDataset<FeatureClass>(source.GetName("ProductCoverage"));
            using var metadataAFeatureClass = source.OpenDataset<FeatureClass>(source.GetName("MetaDataA"));


            ProductCoverages.Initialize(source, QueryFilter);



            var allM_CSCL = Geometries.Features<MetaDataA>(metadataAFeatureClass, new() { WhereClause = $"({filter.WhereClause}) AND fcsubtype = 20" });

            ProductRecord m_csclProduct = null;


            using var featureClass = target.OpenDataset<FeatureClass>(target.GetName("surface"));

            //featureClass.DeleteRows(new QueryFilter {
            //    WhereClause = $"ps = 'S-128' AND (code = 'ElectronicProduct' or code = 'instance')",
            //});

            int recordCount = 0;

            //replica.gdb has exporttype equal null!!
            //var whereclause = $"({filter.WhereClause.Replace("PLTS_COMP_SCALE", "CSCL")}) AND (exporttype is not null AND upper(exporttype) NOT IN ('CANCEL'))";

            var whereclause = $"({filter.WhereClause.Replace("PLTS_COMP_SCALE", "CSCL")})";

            using var buffer = featureClass.CreateRowBuffer();
            using var cursor = productDefinitionsTable.Search(new QueryFilter {
                WhereClause = whereclause,
            }, true);

            // Add all M_SCL as datacoverages
            foreach (var m_sclPolygon in allM_CSCL) {
                var cscale = m_sclPolygon.CSCALE;

                var touches = ProductCoverages.Instance.Touch((m_sclPolygon.Shape as Polygon)!.Extent.Center);
                var uniqueCompScales = touches
                    //.Select(p => p.CScale)
                    .Select(p => p.PltsCompScale)
                    .Distinct().ToList();

                var pltsScalesCount = uniqueCompScales.Count();

                if (pltsScalesCount > 1) {
                    Logger.Current.Error($"Center of M_CSCL touches more than one product. Multiple scales encountered. Check {m_sclPolygon.DSNM}. Using {uniqueCompScales.First()}");
                }


                var dataCoverage_m_scl = new DataCoverage {
                    maximumDisplayScale = Convert.ToInt32(cscale / 2),
                    optimumDisplayScale = cscale,
                    minimumDisplayScale = uniqueCompScales.First()
                };

                {
                    buffer["ps"] = ps101;
                    buffer["code"] = dataCoverage_m_scl.GetType().Name;

                    buffer["attributebindings"] = dataCoverage_m_scl.Flatten();
                    SetShape(buffer, m_sclPolygon.SHAPE);
                    ImporterNIS.SetUsageBand(buffer, Convert.ToInt32(m_sclPolygon.PLTS_COMP_SCALE));

                    var featureN = featureClass.CreateRow(buffer);
                    var name = featureN.UID();

                    // TODO: Create relations
                }
            }

            while (cursor.MoveNext()) {
                recordCount += 1;
                var row = (Row)cursor.Current;
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



                if (serie == default) {
                    serie = dsnm!.Substring(0, 3);
                }

                dsnm = "101DK00" + dsnm!.Substring(2);

                var specificUsage = dsnm[7] switch {
                    '5' => 5,   //S100FC.S128.specificUsage.NavigationalPurposeHarbour,
                    '4' => 4,   //S100FC.S128.specificUsage.NavigationalPurposeApproach,
                    '3' => 3,   //S100FC.S128.specificUsage.NavigationalPurposeCoastal,
                    '2' => 2,   //S100FC.S128.specificUsage.NavigationalPurposeGeneral,
                    '1' => 1,   //S100FC.S128.specificUsage.NavigationalPurposeOverview,
                    _ => throw new InvalidDataException(),
                };

                var instance = new S100FC.S128.FeatureTypes.ElectronicProduct {
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
                    WhereClause = $"Product_GUID = '{globalid:B}'",
                }, true);

                var polygons = new List<ArcGIS.Core.Geometry.Polygon>();

                int polygonsCompScale = 0;

                while (cursorCoverage.MoveNext()) {
                    var productCoverage = new ProductCoverage((Feature)cursorCoverage.Current);
                    var catcov = productCoverage.CATCOV ?? default;
                    var plts_comp_scale = productCoverage.PLTS_COMP_SCALE ?? default;

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

                    switch (catcov) {
                        case 1: {
                                //buffer["ps"] = ps128;
                                //buffer["code"] = instance.GetType().Name; buffer["sourceIdentifier"] = instance.sourceIdentifier;
                                //buffer["version"] = ImporterNIS.s101version;
                                //buffer["__json__"] = System.Text.Json.JsonSerializer.Serialize(instance, jsonTestSerializerOptions);
                                //SetShape(buffer, productCoverage.SHAPE);
                                //ImporterNIS.SetUsageBand(buffer, productCoverage!.PLTS_COMP_SCALE!.Value);
                                //var featureN = featureClass.CreateRow(buffer);
                                //var name = featureN.Crc32();
                                //// TODO: Create relations
                                //ConversionAnalytics.Instance.AddConverted(tableName, current.GLOBALID, name);
                            }

                            // DATACOVERAGE
                            var dataCoverage = new DataCoverage {
                                maximumDisplayScale = Convert.ToInt32(plts_comp_scale / 2),
                                optimumDisplayScale = plts_comp_scale, //displayScale.OptimumDisplayScale,
                                minimumDisplayScale = plts_comp_scale //displayScale.MinimumDisplayScale
                            }; {
                                buffer["ps"] = ps101;
                                buffer["code"] = dataCoverage.GetType().Name;

                                buffer["attributebindings"] = dataCoverage.Flatten();
                                buffer["informationbindings"] = "[]";

                                SetShape(buffer, cutOutM_SCL[0]); // productCoverage.SHAPE);
                                ImporterNIS.SetUsageBand(buffer, productCoverage.PLTS_COMP_SCALE!.Value);

                                var featureN = featureClass.CreateRow(buffer);
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
                                ImporterNIS.SetUsageBand(buffer, productCoverage.PLTS_COMP_SCALE.Value);

                                var featureN = featureClass.CreateRow(buffer);
                                var name = featureN.UID();

                                // Registering vertical datum information for all areas
                                VerticalDatums.Instance.Add(productCoverage!.SHAPE!, vdat.verticalDatum);

                                SoundingDatums.Instance.Add(productCoverage!.SHAPE!, GetSoundingDatum(current.SDAT!.Value)!);

                                // TODO: Create relations
                                ConversionAnalytics.Instance.AddConverted(tableName, current.GLOBALID, name);

                                VerticalDatums.Instance.Add(productCoverage.SHAPE!.Clone(), vdat.verticalDatum);

                            }
                            break;
                    }
                }

                if (s128) {
                    //Store S-128 polygons
                    buffer["ps"] = ps128;
                    buffer["code"] = instance.GetType().Name; buffer["sourceIdentifier"] = instance.sourceIdentifier;

                    buffer["attributebindings"] = instance.Flatten();
                    buffer["informationbindings"] = "[]";

                    SetShape(buffer, (ArcGIS.Core.Geometry.Polygon)GeometryEngine.Instance.Union(polygons));
                    ImporterNIS.SetUsageBand(buffer, polygonsCompScale);
                    var featureN = featureClass.CreateRow(buffer);
                    var name = featureN.UID();
                    // TODO: Create relations
                    ConversionAnalytics.Instance.AddConverted(tableName, current.GLOBALID, name);
                }

                Logger.Current.DataObject(objectid, tableName, dsnm, System.Text.Json.JsonSerializer.Serialize(instance, jsonSerializerOptions128));
            }
            Logger.Current.DataTotalCount(tableName, recordCount, ConversionAnalytics.Instance.GetConvertedCount(tableName));
        }
    }
}


namespace S100Framework.Applications
{
    using ArcGIS.Core.CIM;
    using S100FC.S128;
    using S100FC.S128.SimpleAttributes;
    using S100Framework.Applications.S57auto.esri;
    using System.Collections.Immutable;
    using Windows.System.Diagnostics;

    internal static partial class ImporterNIS
    {

        private static void S57_ProductCoverage_Full(Geodatabase source, Geodatabase target, QueryFilter filter, int minimumDisplayScale, bool s128) {
            JsonSerializerOptions jsonSerializerOptions128 = new JsonSerializerOptions {
                WriteIndented = false,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                PropertyNameCaseInsensitive = true,
            }.AppendTypeInfoResolver();

            var tableName = "ProductCoverage";

            using var productDefinitionsTable = source.OpenDataset<Table>(source.GetName("ProductDefinitions"));
            using var productCoverageFeatureClass = source.OpenDataset<FeatureClass>(source.GetName("ProductCoverage"));
            using var metadataAFeatureClass = source.OpenDataset<FeatureClass>(source.GetName("MetaDataA"));


            ProductCoverages.Initialize(source, QueryFilter);

            var allM_CSCL = Geometries.Features<MetaDataA>(metadataAFeatureClass, new() { WhereClause = $"({filter.WhereClause}) AND fcsubtype = 20" });

            int recordCount = 0;

            var whereclause = $"({filter.WhereClause.Replace("PLTS_COMP_SCALE", "CSCL")})";

            using var productDefinitions = productDefinitionsTable.Search(new QueryFilter {
                WhereClause = $"({whereclause}) AND (EXPORTTYPE IS NULL OR EXPORTTYPE <> 'Cancel')",
            }, true);

            (string Name, int PLTS_COMP_SCALE, DataCoverage DataCoverage, S100FC.S101.SimpleAttributes.verticalDatum? VDAT, S100FC.S101.SimpleAttributes.verticalDatum? SDAT, Polygon[] Coverage)[] coverages = [];

            while (productDefinitions.MoveNext()) {
                recordCount += 1;
                var row = (Row)productDefinitions.Current;
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

                if (serie == default) {
                    serie = dsnm!.Substring(0, 3);
                }

                dsnm = "101DK00" + dsnm!.Substring(2);

                var specificUsage = dsnm[7] switch {
                    '5' => 5,   //S100FC.S128.specificUsage.NavigationalPurposeHarbour,
                    '4' => 4,   //S100FC.S128.specificUsage.NavigationalPurposeApproach,
                    '3' => 3,   //S100FC.S128.specificUsage.NavigationalPurposeCoastal,
                    '2' => 2,   //S100FC.S128.specificUsage.NavigationalPurposeGeneral,
                    '1' => 1,   //S100FC.S128.specificUsage.NavigationalPurposeOverview,
                    _ => throw new InvalidDataException(),
                };

                //var instance = new S100FC.S128.FeatureTypes.ElectronicProduct {
                //    catalogueElementClassification = [1], // catalogueElementClassification.Enc
                //    editionNumber = edtn,
                //    updateNumber = updn,
                //    issueDate = DateOnly.FromDateTime(isdt),
                //    notForNavigation = true,
                //    typeOfProductFormat = 2,    //typeOfProductFormat.IsoIec8211,
                //    datasetName = dsnm,
                //    specificUsage = specificUsage,
                //    productSpecification = new productSpecification {
                //        editionDate = S100FC.S101.Summary.VersionDate,
                //        name = S100FC.S101.Summary.ProductId,
                //        version = S100FC.S101.Summary.Version.ToString(),
                //    },
                //};

                using var cursorCoverage = productCoverageFeatureClass.Search(new QueryFilter {
                    WhereClause = $"Product_GUID = '{globalid:B}' AND CATCOV = 1",
                }, true);

                Polygon[] polygons = [];

                //int polygonsCompScale = 0;                

                while (cursorCoverage.MoveNext()) {
                    polygons = [.. polygons, (Polygon)((Feature)cursorCoverage.Current).GetShape().Clone()];
                }
                if (!polygons.Any()) System.Diagnostics.Debugger.Break();

                var dataCoverage = new DataCoverage {
                    maximumDisplayScale = Convert.ToInt32(current.CSCL!.Value / 2),
                    optimumDisplayScale = current.CSCL!.Value,
                    minimumDisplayScale = minimumDisplayScale,
                };

                var vdat = GetVerticalDatum(current.VDAT ?? 3);
                var sdat = GetSoundingDatum(current.SDAT!.Value);

                coverages = [.. coverages, (dsnm, current.CSCL!.Value, dataCoverage, vdat, sdat, polygons)];
            }

            (string Name, int PLTS_COMP_SCALE, DataCoverage DataCoverage, verticalDatum? VDAT, verticalDatum? SDAT, Polygon[] Coverage)[] coveragesCSCL = [];

            foreach (var m_sclPolygon in allM_CSCL) {
                var dsnm = $"M_CSCL:{m_sclPolygon.OBJECTID}";
                var PLTS_COMP_SCALE = m_sclPolygon.CSCALE!.Value;

                var shape = (Polygon)m_sclPolygon.Shape!;

                var dataCoverage = new DataCoverage {
                    maximumDisplayScale = PLTS_COMP_SCALE / 2,
                    minimumDisplayScale = minimumDisplayScale,
                    optimumDisplayScale = PLTS_COMP_SCALE,
                };

                //  Calculate center point
                var centroid = GeometryEngine.Instance.Centroid(shape);

                var hits = coverages.Where(e => e.PLTS_COMP_SCALE > PLTS_COMP_SCALE && GeometryEngine.Instance.Contains(PolygonBuilderEx.CreatePolygon(e.Coverage), centroid)).ToArray();

                var hit = hits.OrderByDescending(e => e.PLTS_COMP_SCALE).First();

                coverages = [.. coverages, (dsnm, PLTS_COMP_SCALE, dataCoverage, hit.VDAT, hit.SDAT, [shape])];
            }

            var scales = coverages.Select(e => e.PLTS_COMP_SCALE).Distinct().OrderByDescending(e => e).ToArray();

            (string Name, int PLTS_COMP_SCALE, DataCoverage DataCoverage, S100FC.S101.SimpleAttributes.verticalDatum? VDAT, S100FC.S101.SimpleAttributes.verticalDatum? SDAT, Polygon Coverage)[] products = [];


            for (int i = 0; i < scales.Length; i++) {
                ;
                foreach (var coverage in coverages.Where(e => e.PLTS_COMP_SCALE == scales[i])) {
                    //if (!"101DK001NORSO".Equals(coverage.Name)) continue;
                    var multipart = coverage.Coverage.Length == 1 ? coverage.Coverage[0] : PolygonBuilderEx.CreatePolygon(coverage.Coverage);

                    for (int j = i + 1; j < scales.Length; j++) {
                        foreach (var part in coverages.Where(e => e.PLTS_COMP_SCALE == scales[j])) {
                            var combined = PolygonBuilderEx.CreatePolygon(part.Coverage);
                            if (GeometryEngine.Instance.Disjoint(multipart, combined)) continue;

                            var difference = GeometryEngine.Instance.Difference(multipart, combined);

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
                                ;
                        }
                    }

                    if (multipart.IsEmpty) continue;

                    products = [.. products, (coverage.Name, coverage.PLTS_COMP_SCALE, coverage.DataCoverage, coverage.VDAT, coverage.SDAT, multipart)];
                    //break;
                }
            }

            ;

            using (var geodatabase = new Geodatabase(new FileGeodatabaseConnectionPath(new Uri(System.IO.Path.GetFullPath("coverage.gdb"))))) {
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

                    foreach (var p in c.Coverage.Split()) {
                        SetShape(buffer, p);
                        var featureN = featureClass.CreateRow(buffer);
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

                    foreach (var p in c.Coverage.Split()) {
                        SetShape(buffer, p);
                        var featureN = featureClass.CreateRow(buffer);
                        var name = featureN.UID();

                        VerticalDatums.Instance.Add(p, vdat.verticalDatum);

                        SoundingDatums.Instance.Add(p, c.SDAT!);
                    }
                }
            }

            System.Diagnostics.Debugger.Break();

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
                        //buffer["code"] = instance.GetType().Name; buffer["sourceIdentifier"] = instance.sourceIdentifier;
                        //buffer["version"] = ImporterNIS.s101version;
                        //buffer["__json__"] = System.Text.Json.JsonSerializer.Serialize(instance, jsonTestSerializerOptions);
                        //SetShape(buffer, productCoverage.SHAPE);
                        //ImporterNIS.SetUsageBand(buffer, productCoverage!.PLTS_COMP_SCALE!.Value);
                        //var featureN = featureClass.CreateRow(buffer);
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
                        ImporterNIS.SetUsageBand(buffer, productCoverage.PLTS_COMP_SCALE!.Value);

                        var featureN = featureClass.CreateRow(buffer);
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
                        ImporterNIS.SetUsageBand(buffer, productCoverage.PLTS_COMP_SCALE.Value);

                        var featureN = featureClass.CreateRow(buffer);
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
                    buffer["code"] = instance.GetType().Name; buffer["sourceIdentifier"] = instance.sourceIdentifier;

                    buffer["attributebindings"] = instance.Flatten();
                    buffer["informationbindings"] = "[]";

                    SetShape(buffer, (ArcGIS.Core.Geometry.Polygon)GeometryEngine.Instance.Union(polygons));
                    ImporterNIS.SetUsageBand(buffer, polygonsCompScale);
                    var featureN = featureClass.CreateRow(buffer);
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
