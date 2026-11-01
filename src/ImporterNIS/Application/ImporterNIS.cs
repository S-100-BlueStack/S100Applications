#define CREATE_SUBTYPES

using ArcGIS.Core.Data;
using ArcGIS.Core.Data.DDL;
using ArcGIS.Core.Geometry;

using ImporterNIS.Singletons;
using S100FC;
using S100FC.Catalogues;
using S100FC.S101;
using S100FC.S101.ComplexAttributes;
using S100FC.S101.FeatureAssociation;
using S100FC.S101.FeatureTypes;
using S100FC.S101.InformationAssociation;
using S100FC.S101.InformationTypes;
using S100FC.S101.SimpleAttributes;
using S100Framework.Applications.S57.esri;
using S100Framework.Applications.Singletons;
using System.Data;
using System.Globalization;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using VortexLoader;
using VortexLoader.Singletons;

using IO = System.IO;

[assembly: InternalsVisibleTo("TestNisImporter")]
namespace S100Framework.Applications
{
    internal static partial class ImporterNIS
    {

        internal static readonly JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions {
            WriteIndented = false,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            PropertyNameCaseInsensitive = true,
        }.AppendTypeInfoResolver();

        //  https://github.com/iho-ohi/S-57-to-S-101-conversion-sub-WG
        internal static string _notesPath = "";
        internal static string _scaminFilesPath = "";

        internal static string ps101 = S100FC.S101.Summary.ProductId;
        internal static string ps128 = S100FC.S128.Summary.ProductId;
        internal static string s101version = S100FC.S101.Summary.Version.ToString();
        internal static Geodatabase? _geodatabase;

        internal static RelatedEquipment? relatedEquipment;

        internal static ConverterRegistry _converterRegistry = new ConverterRegistry();

        public static QueryFilter QueryFilter { get; internal set; } = new();

        //public static IDictionary<int, int> VerticalDatumConverter = new Dictionary<int, int>();


        public static bool Load(Func<Geodatabase> createTargetGeodatabase, ImportOptions options) {
            Logger.Current.Information("Starting");

            Func<Geodatabase> createGeodatabase = () => { throw new NotImplementedException(); };
            Func<Geodatabase> createGeodatabaseElectronicProduct = () => { throw new NotImplementedException(); };

            // default value - overwritten by args
            //var s128 = true;

            // default value - overwritten by args
            var skinOfEarthOnly = false;
            string status = null!;
            bool validationPassed = true;

            int maximumDisplayScale = 0;
            int minimumDisplayScale = int.MaxValue;

            string? filter = "";

            bool clip = false;

            maximumDisplayScale = options.MaximumDisplayScale ?? 0;
            minimumDisplayScale = options.MinimumDisplayScale ?? int.MaxValue;
            createGeodatabase = GeodatabaseFactory.Create(options.Source!);
            createGeodatabaseElectronicProduct = string.IsNullOrWhiteSpace(options.S128)
                ? createTargetGeodatabase
                : GeodatabaseFactory.Create(options.S128);
            _notesPath = options.NotesPath ?? "";
            _scaminFilesPath = options.ScaminFilesPath ?? "";
            skinOfEarthOnly = bool.TryParse(options.SkinOfEarthOnly, out var skinOnly) && skinOnly;
            filter = options.Filter;
            clip = options.Clipping;

            Func<Action<Geodatabase>, Geodatabase, bool> Store = (a, database) => {
                database.ApplyEdits(() => {
                    a.Invoke(database);
                });
                return true;
            };

            //using (var destination = createTargetGeodatabase()) {
            //    if (destination.IsTraditionallyVersioned()) {
            //        Store = (a, _) => {
            //            _.ApplyEdits(() => {
            //                a.Invoke(_);
            //            }, true);
            //            return true;
            //        };
            //    }
            //}

            RegisterConverters();

            //goto __skip_truncate;

            //  Truncate geodatabase
            using (Geodatabase destination = createTargetGeodatabase()) {
                var featureClasses = destination.GetDefinitions<FeatureClassDefinition>();

                foreach (var featureClass in featureClasses) {
                    using var _ = destination.OpenDataset<FeatureClass>(featureClass.GetName());

                    Store((destination) => {
                        Logger.Current.Debug($"Deleting data from destination: {_.GetName()}");

                        using (var seach = _.Search(new QueryFilter {
                            WhereClause = "1=1",
                            SubFields = "OBJECTID",
                        }, false)) {
                            while (seach.MoveNext()) {
                                seach.Current.Delete();
                            }
                        }

                    }, destination);
                }

                var tables = destination.GetDefinitions<TableDefinition>();

                foreach (var table in tables) {
                    if (table.GetName().Split(',')[^1].EndsWith("configuration")) continue;

                    using var _ = destination.OpenDataset<Table>(table.GetName());

                    Store((destination) => {
                        Logger.Current.Debug($"Deleting data from destination: {_.GetName()}");

                        using (var seach = _.Search(new QueryFilter {
                            WhereClause = "1=1",
                            SubFields = "OBJECTID",
                        }, false)) {
                            while (seach.MoveNext()) {
                                seach.Current.Delete();
                            }
                        }
                    }, destination);
                }
            }

            ConversionAnalytics.Initialize();

        __skip_truncate:
            long[] scalesCompilation = [];

            S101ProductCoverage[] s101ProductCoverages = [];
            //using (var destination = createTargetGeodatabase()) 
            {
                QueryFilter.WhereClause = $"(PLTS_COMP_SCALE >= {maximumDisplayScale} AND PLTS_COMP_SCALE < {minimumDisplayScale}) AND (Shape IS NOT NULL)";

                using (Geodatabase source = createGeodatabase()) {
                    Logger.Current.Debug($"Converting Product Coverages");

                    using var electronicProducts = createGeodatabaseElectronicProduct();

                    foreach (var featureClass in electronicProducts.GetDefinitions<FeatureClassDefinition>()) {
                        using var _ = electronicProducts.OpenDataset<FeatureClass>(featureClass.GetName());

                        Store((electronicProducts) => {
                            using (var seach = _.Search(new QueryFilter {
                                WhereClause = $"upper(ps) = 'S-128'",
                                SubFields = "OBJECTID",
                            }, false)) {
                                while (seach.MoveNext()) {
                                    seach.Current.Delete();
                                }
                            }
                        }, electronicProducts);
                    }
                    foreach (var table in electronicProducts.GetDefinitions<TableDefinition>()) {
                        if (table.GetName().Split(',')[^1].EndsWith("configuration")) continue;

                        using var _ = electronicProducts.OpenDataset<Table>(table.GetName());

                        Store((electronicProducts) => {
                            using (var seach = _.Search(new QueryFilter {
                                WhereClause = $"upper(ps) = 'S-128'",
                                SubFields = "OBJECTID",
                            }, false)) {
                                while (seach.MoveNext()) {
                                    seach.Current.Delete();
                                }
                            }
                        }, electronicProducts);
                    }

                    Store((d) => S57_ProductCoverage_Full(source, electronicProducts, QueryFilter, minimumDisplayScale, ref s101ProductCoverages, filter, (products) => {
                        using var metadataAFeatureClass = source.OpenDataset<FeatureClass>(source.GetName("MetaDataA"));

                        void CreateProducts(Geodatabase target101) {
                            using (var featureClass = target101.OpenDataset<FeatureClass>(target101.GetName("surface"))) {
                                using var buffer = featureClass.CreateRowBuffer();
                                buffer["ps"] = ps101;

                                foreach (var c in products) {
                                    buffer["code"] = c.DataCoverage.GetType().Name;
                                    buffer["attributebindings"] = c.DataCoverage.Flatten();
                                    buffer["informationbindings"] = "[]";
                                    buffer["featurebindings"] = "[]";
                                    //buffer["specificusage"] = c.specificUsage;
                                    buffer["compilationscale"] = optimumDisplayScaleConverter(c.PLTS_COMP_SCALE);
                                    buffer["sourceIdentifier"] = c.DataCoverage.sourceIdentifier;

                                    foreach (var p in c.Coverage.Split()) {
                                        SetShape(buffer, p);
                                        using var featureN = featureClass.CreateRow(buffer);
                                        var name = featureN.UID();
                                    }
                                }

                                foreach (var c in products) {
                                    (VerticalDatumOfData VerticalDatum, Polygon Shape)[] verticalDatums = [];

                                    foreach (var p in c.Coverage.Split()) {
                                        var m_vdat = Geometries.Features<MetaDataA>(metadataAFeatureClass, new SpatialQueryFilter {
                                            WhereClause = $"(PLTS_COMP_SCALE = {c.PLTS_COMP_SCALE}) AND fcsubtype = 55",
                                            SpatialRelationship = SpatialRelationship.Contains,
                                            FilterGeometry = p,
                                        });

                                        var _geometry = (Polygon)p.Clone();
                                        if (m_vdat.Any()) {
                                            Polygon[] polygons = [];
                                            foreach (var __m_vdat in m_vdat) {
                                                verticalDatums = [.. verticalDatums, (new VerticalDatumOfData {
                                                    verticalDatum = GetVerticalDatum(__m_vdat.VERDAT!.Value, __m_vdat.Shape!)!.value,
                                                }, (Polygon)__m_vdat.Shape!.Clone())];
                                                polygons = [.. polygons, (Polygon)__m_vdat.Shape!.Clone()];
                                            }

                                            var union = GeometryEngine.Instance.Union(polygons);

                                            var difference = GeometryEngine.Instance.Difference(_geometry, union);

                                            if (difference is Polygon polygon) {
                                                foreach (var part in polygon.Split()) {
                                                    verticalDatums = [.. verticalDatums, (new VerticalDatumOfData {
                                                        verticalDatum = c.VDAT!.value,
                                                    }, part)];
                                                }
                                            }
                                            else
                                                System.Diagnostics.Debugger.Break();
                                        }
                                        else {
                                            foreach (var part in _geometry.Split()) {
                                                verticalDatums = [.. verticalDatums, (new VerticalDatumOfData {
                                                    verticalDatum = c.VDAT!.value,
                                                }, part)];
                                            }
                                        }
                                    }

                                    foreach (var s in verticalDatums) {
                                        buffer["code"] = s.VerticalDatum.GetType().Name;
                                        buffer["attributebindings"] = s.VerticalDatum.Flatten();
                                        buffer["informationbindings"] = "[]";
                                        buffer["featurebindings"] = "[]";
                                        buffer["compilationscale"] = optimumDisplayScaleConverter(c.PLTS_COMP_SCALE);
                                        buffer["sourceIdentifier"] = s.VerticalDatum.sourceIdentifier;

                                        SetShape(buffer, s.Shape);
                                        using var featureN = featureClass.CreateRow(buffer);
                                        var name = featureN.UID();

                                        VerticalDatums.Instance.Add(s.Shape, s.VerticalDatum.verticalDatum);
                                    }
                                }

                                foreach (var c in products) {
                                    (SoundingDatum SoundingDatum, Polygon Shape)[] soundingDatums = [];

                                    foreach (var p in c.Coverage.Split()) {
                                        var m_sdat = Geometries.Features<MetaDataA>(metadataAFeatureClass, new SpatialQueryFilter {
                                            WhereClause = $"(PLTS_COMP_SCALE = {c.PLTS_COMP_SCALE}) AND fcsubtype = 45",
                                            SpatialRelationship = SpatialRelationship.Contains,
                                            FilterGeometry = p,
                                        });

                                        var _geometry = (Polygon)p.Clone();

                                        if (m_sdat.Any()) {
                                            Polygon[] polygons = [];
                                            foreach (var __m_sdat in m_sdat) {
                                                soundingDatums = [.. soundingDatums, (new SoundingDatum {
                                                    verticalDatum = GetSoundingDatum(__m_sdat.VERDAT!.Value, __m_sdat.Shape!)!.value,
                                                }, (Polygon)__m_sdat.Shape!.Clone())];
                                                polygons = [.. polygons, (Polygon)__m_sdat.Shape!.Clone()];
                                            }

                                            var union = GeometryEngine.Instance.Union(polygons);

                                            var difference = GeometryEngine.Instance.Difference(_geometry, union);

                                            if (difference is Polygon polygon) {
                                                foreach (var part in polygon.Split()) {
                                                    soundingDatums = [.. soundingDatums, (new SoundingDatum {
                                                    verticalDatum = c.SDAT!.value,
                                                }, part)];
                                                }
                                            }
                                            else
                                                System.Diagnostics.Debugger.Break();
                                        }
                                        else {
                                            foreach (var part in _geometry.Split()) {
                                                soundingDatums = [.. soundingDatums, (new SoundingDatum {
                                                    verticalDatum = c.SDAT!.value,
                                                }, part)];
                                            }
                                        }
                                    }

                                    foreach (var s in soundingDatums) {
                                        buffer["code"] = s.SoundingDatum.GetType().Name;
                                        buffer["attributebindings"] = s.SoundingDatum.Flatten();
                                        buffer["informationbindings"] = "[]";
                                        buffer["featurebindings"] = "[]";
                                        buffer["compilationscale"] = optimumDisplayScaleConverter(c.PLTS_COMP_SCALE);
                                        buffer["sourceIdentifier"] = s.SoundingDatum.sourceIdentifier;

                                        SetShape(buffer, s.Shape);
                                        using var featureN = featureClass.CreateRow(buffer);
                                        var name = featureN.UID();

                                        SoundingDatums.Instance.Add(s.Shape, s.SoundingDatum.verticalDatum);
                                    }
                                }
                            }
                        }

                        if (createTargetGeodatabase.Equals(createGeodatabaseElectronicProduct))
                            CreateProducts(d);
                        else {
                            using var s101 = createTargetGeodatabase();

                            Store((database) => {
                                CreateProducts(database);
                            }, s101);
                        }
                    }), electronicProducts);
                }
            }

            scalesCompilation = s101ProductCoverages.Select(e => (long)e.PLTS_COMP_SCALE).Distinct().OrderByDescending(e => e).ToArray();

            foreach (var scale in scalesCompilation) {
                if (!clip) {
                    if (Array.IndexOf(scalesCompilation, scale) == 0) {
                        QueryFilter.WhereClause = $"(PLTS_COMP_SCALE >= {scale} AND PLTS_COMP_SCALE < {minimumDisplayScale}) AND (Shape IS NOT NULL)";
                        Logger.Current.Verbose(QueryFilter.WhereClause);
                    }
                    else {
                        QueryFilter.WhereClause = $"(PLTS_COMP_SCALE >= {scale} AND PLTS_COMP_SCALE < {scalesCompilation[Array.IndexOf(scalesCompilation, scale) - 1]}) AND (Shape IS NOT NULL)";
                        Logger.Current.Verbose(QueryFilter.WhereClause);
                    }
                }
                else {
                    if (Array.IndexOf(scalesCompilation, scale) == 0) {
                        QueryFilter.WhereClause = $"(PLTS_COMP_SCALE >= {scale} AND PLTS_COMP_SCALE < {minimumDisplayScale}) AND (Shape IS NOT NULL)";
                        Logger.Current.Verbose(QueryFilter.WhereClause);
                    }
                    else {
                        QueryFilter.WhereClause = $"(PLTS_COMP_SCALE >= {scale} AND PLTS_COMP_SCALE < {scalesCompilation[Array.IndexOf(scalesCompilation, scale) - 1]}) AND (Shape IS NOT NULL)";
                        Logger.Current.Verbose(QueryFilter.WhereClause);

                        Polygon[] clipping = [];
                        foreach (var e in s101ProductCoverages.Where(e => e.PLTS_COMP_SCALE >= scale && e.PLTS_COMP_SCALE < scalesCompilation[Array.IndexOf(scalesCompilation, scale) - 1])) {
                            var shape = e.Coverage;
                            var ring = shape.GetExteriorRing(0, true);
                            clipping = [.. clipping, ring];
                        }

                        using (var destination = createTargetGeodatabase()) {
                            foreach (var queryPolygon in clipping) {
                                using (var featureClass = destination.OpenDataset<FeatureClass>(destination.GetName("point"))) {
                                    var targetSR = featureClass.GetDefinition().GetSpatialReference();
                                    var queryPolygonProjected = (Polygon)GeometryEngine.Instance.Project(queryPolygon, targetSR);

                                    var spatialFilter = new SpatialQueryFilter {
                                        WhereClause = "UPPER(ps) = 'S-101' AND (Shape IS NOT NULL)",
                                        FilterGeometry = queryPolygonProjected,
                                        SpatialRelationship = SpatialRelationship.Contains
                                    };
                                    Store((destination) => {
                                        featureClass.DeleteRows(spatialFilter);
                                    }, destination);
                                }

                                using (var featureClass = destination.OpenDataset<FeatureClass>(destination.GetName("pointset"))) {
                                    var targetSR = featureClass.GetDefinition().GetSpatialReference();
                                    var queryPolygonProjected = (Polygon)GeometryEngine.Instance.Project(queryPolygon, targetSR);

                                    var spatialFilter = new SpatialQueryFilter {
                                        WhereClause = "UPPER(ps) = 'S-101' AND (Shape IS NOT NULL)",
                                        FilterGeometry = queryPolygonProjected,
                                        SpatialRelationship = SpatialRelationship.Contains
                                    };

                                    Store((destination) => {
                                        featureClass.DeleteRows(spatialFilter);
                                    }, destination);
                                }

                                {   //  curve
                                    long[] hits = [];

                                    using (var featureClass = destination.OpenDataset<FeatureClass>(destination.GetName("curve"))) {
                                        var targetSR = featureClass.GetDefinition().GetSpatialReference();
                                        var queryPolygonProjected = (Polygon)GeometryEngine.Instance.Project(queryPolygon, targetSR);

                                        var spatialFilter = new SpatialQueryFilter {
                                            WhereClause = "UPPER(ps) = 'S-101' AND (Shape IS NOT NULL)",
                                            FilterGeometry = queryPolygonProjected,
                                            SpatialRelationship = SpatialRelationship.IndexIntersects
                                        };

                                        using (var cursor = featureClass.Search(spatialFilter, true)) {
                                            while (cursor.MoveNext()) {
                                                hits = [.. hits, cursor.Current.GetObjectID()];
                                            }
                                        }
                                    }

                                    long[] updated = [];
                                    long[] created = [];
                                    long[] deleted = [];

                                    Store((destination) => {
                                        using (var featureClass = destination.OpenDataset<FeatureClass>(destination.GetName("curve"))) {
                                            var targetSR = featureClass.GetDefinition().GetSpatialReference();
                                            var queryPolygonProjected = (Polygon)GeometryEngine.Instance.Project(queryPolygon, targetSR);

                                            using var insert = featureClass.CreateInsertCursor();

                                            using var buffer = featureClass.CreateRowBuffer();

                                            foreach (var objectid in hits) {
                                                using var cursor = featureClass.Search(new QueryFilter {
                                                    WhereClause = $"OBJECTID = {objectid}",
                                                }, false);

                                                cursor.MoveNext();

                                                using var feature = (Feature)cursor.Current;
                                                var shape = (Polyline)feature.GetShape();

                                                if (GeometryEngine.Instance.Disjoint(shape, queryPolygonProjected))
                                                    continue;

                                                if (GeometryEngine.Instance.Within(shape, queryPolygonProjected)) {
                                                    deleted = [.. deleted, objectid];
                                                }
                                                else if (GeometryEngine.Instance.Intersects(queryPolygonProjected, shape)) {
                                                    deleted = [.. deleted, objectid];
                                                    var difference = GeometryEngine.Instance.Difference(shape, queryPolygonProjected);

                                                    if (difference is Polyline polyline) {
                                                        //if (polyline.PartCount > 1) System.Diagnostics.Debugger.Break();

                                                        //using var buffer = featureClass.CreateRowBuffer(feature);
                                                        feature.Clone(buffer, ["shape"]);
                                                        buffer["shape"] = polyline;
                                                        var _ = insert.Insert(buffer);
                                                        created = [.. created, _];
                                                    }
                                                    else
                                                        System.Diagnostics.Debugger.Break();
                                                }
                                            }

                                            insert.Flush();

                                            if (deleted.Any()) {
                                                featureClass.DeleteRows(new QueryFilter {
                                                    WhereClause = $"OBJECTID IN ({string.Join(',', deleted)})",
                                                });
                                            }

                                            featureClass.DeleteRows(new SpatialQueryFilter {
                                                WhereClause = "UPPER(ps) = 'S-101' AND (Shape IS NOT NULL)",
                                                FilterGeometry = queryPolygonProjected,
                                                SpatialRelationship = SpatialRelationship.Contains
                                            });
                                        }
                                    }, destination);
                                }

                                {   //  surface
                                    long[] hits = [];
                                    using (var featureClass = destination.OpenDataset<FeatureClass>(destination.GetName("surface"))) {
                                        var targetSR = featureClass.GetDefinition().GetSpatialReference();
                                        var queryPolygonProjected = (Polygon)GeometryEngine.Instance.Project(queryPolygon, targetSR);

                                        var spatialFilter = new SpatialQueryFilter {
                                            WhereClause = "UPPER(ps) = 'S-101' AND (Shape IS NOT NULL)",
                                            FilterGeometry = queryPolygonProjected,
                                            SpatialRelationship = SpatialRelationship.IndexIntersects
                                        };

                                        using (var cursor = featureClass.Search(spatialFilter, true)) {
                                            while (cursor.MoveNext()) {
                                                hits = [.. hits, cursor.Current.GetObjectID()];
                                            }
                                        }
                                    }

                                    long[] updated = [];
                                    long[] created = [];
                                    long[] deleted = [];

                                    Store((destination) => {
                                        using (var featureClass = destination.OpenDataset<FeatureClass>(destination.GetName("surface"))) {
                                            var targetSR = featureClass.GetDefinition().GetSpatialReference();
                                            var queryPolygonProjected = (Polygon)GeometryEngine.Instance.Project(queryPolygon, targetSR);

                                            using var insert = featureClass.CreateInsertCursor();

                                            using var buffer = featureClass.CreateRowBuffer();

                                            foreach (var objectid in hits) {
                                                using var cursor = featureClass.Search(new QueryFilter {
                                                    WhereClause = $"OBJECTID = {objectid}",
                                                }, false);

                                                cursor.MoveNext();

                                                using var feature = (Feature)cursor.Current;
                                                var shape = (Polygon)feature.GetShape();

                                                if (GeometryEngine.Instance.Disjoint(shape, queryPolygonProjected))
                                                    continue;

                                                if (GeometryEngine.Instance.Within(shape, queryPolygonProjected)) {
                                                    deleted = [.. deleted, objectid];
                                                }
                                                else if (GeometryEngine.Instance.Intersects(queryPolygonProjected, shape)) {
                                                    deleted = [.. deleted, objectid];
                                                    var difference = GeometryEngine.Instance.Difference(shape, queryPolygonProjected);

                                                    if (difference is Polygon polygon) {
                                                        if (polygon.ExteriorRingCount > 1) {
                                                            Polygon[] polygons = [];
                                                            ReadOnlySegmentCollection[] segments = [polygon.Parts[0]];
                                                            for (int i = 1; i < polygon.PartCount; i++) {
                                                                var p = PolygonBuilderEx.CreatePolygon(polygon.Parts[i]);
                                                                if (p.Area < 0)
                                                                    segments = [.. segments, polygon.Parts[i]];
                                                                else {
                                                                    var _ = PolygonBuilderEx.CreatePolygon(segments);
                                                                    polygons = [.. polygons, _];
                                                                    segments = [polygon.Parts[i]];
                                                                }
                                                            }
                                                            if (segments.Any()) {
                                                                var _ = PolygonBuilderEx.CreatePolygon(segments);
                                                                polygons = [.. polygons, _];
                                                            }

                                                            //using var buffer = featureClass.CreateRowBuffer(feature);

                                                            for (int i = 0; i < polygons.Length; i++) {
                                                                feature.Clone(buffer, ["shape"]);
                                                                buffer["shape"] = polygons[i];
                                                                var _ = insert.Insert(buffer);
                                                                created = [.. created, _];
                                                            }
                                                        }
                                                        else {
                                                            //using var buffer = featureClass.CreateRowBuffer(feature);
                                                            feature.Clone(buffer, ["shape"]);
                                                            buffer["shape"] = polygon;
                                                            var _ = insert.Insert(buffer);
                                                            created = [.. created, _];
                                                        }
                                                    }
                                                }
                                            }

                                            insert.Flush();

                                            if (deleted.Any()) {
                                                featureClass.DeleteRows(new QueryFilter {
                                                    WhereClause = $"OBJECTID IN ({string.Join(',', deleted)})",
                                                });
                                            }

                                            featureClass.DeleteRows(new SpatialQueryFilter {
                                                WhereClause = "UPPER(ps) = 'S-101' AND (Shape IS NOT NULL)",
                                                FilterGeometry = queryPolygonProjected,
                                                SpatialRelationship = SpatialRelationship.Contains
                                            });
                                        }
                                    }, destination);
                                }
                            }
                        }
                    }
                }

                using (var destination = createTargetGeodatabase()) {
                    using (Geodatabase source = createGeodatabase()) {
                        Logger.Current.Debug($"Loading subtypes codes to subtype name");
                        Subtypes.Initialize(source);

                        Logger.Current.Debug($"Loading featurerelations");
                        FeatureRelations.Initialize(source);

                        Logger.Current.Information($"Initializing SpatialRelationResolver");
                        SpatialRelationResolver.Initialize(source);

                        Logger.Current.Information($"Initializing SpatialAssociations");
                        SpatialAssociations.Initialize(source, QueryFilter);

                        Logger.Current.Information($"Initializing NauticalInformations");
                        NauticalInformations.Initialize(destination);

                        relatedEquipment = new RelatedEquipment(source, destination);

                        if (skinOfEarthOnly) {
                            Logger.Current.Information($"Skin of the earth");
                            Logger.Current.Verbose($"WhereClause: {QueryFilter.WhereClause}");

                            var whereClause = QueryFilter.WhereClause.Clone();
                            QueryFilter.WhereClause = $"({whereClause}) and fcsubtype in (1,5,15)";
                            Store((destination) => S57_Depths(
                                "DepthsA",
                                (tableName) => source.OpenDataset<FeatureClass>(source.GetName(tableName)),
                                QueryFilter,
                                () => destination.OpenDataset<FeatureClass>(destination.GetName("surface")),
                                (buffer, shape) => SetShape(buffer, shape), []), destination);

                            QueryFilter.WhereClause = $"({whereClause}) and fcsubtype in (5)";
                            Store((destination) => S57_NaturalFeatures(
                                "NaturalFeaturesA",
                                (tableName) => source.OpenDataset<FeatureClass>(source.GetName(tableName)),
                                QueryFilter,
                                () => destination.OpenDataset<FeatureClass>(destination.GetName("surface")),
                                (buffer, shape) => SetShape(buffer, shape)), destination);

                            QueryFilter.WhereClause = $"({whereClause}) and fcsubtype in (40,60,80)";
                            Store((destination) => S57_PortsAndServicesA(source, destination, QueryFilter), destination);

                            QueryFilter.WhereClause = $"({whereClause}) and fcsubtype in (40)";
                            Store((destination) => S57_Metadata(
                                "MetadataA",
                                (tableName) => source.OpenDataset<FeatureClass>(source.GetName(tableName)),
                                QueryFilter,
                                () => destination.OpenDataset<FeatureClass>(destination.GetName("surface")),
                                destination,
                                (buffer, shape) => SetShape(buffer, shape)), destination);
                        }
                        else {
                            Logger.Current.Verbose($"WhereClause: {QueryFilter.WhereClause}");

                            Logger.Current.Debug($"Converting Metadata");
                            Store((destination) => S57_Metadata(
                                "MetadataA",
                                (tableName) => source.OpenDataset<FeatureClass>(source.GetName(tableName)),
                                QueryFilter,
                                () => destination.OpenDataset<FeatureClass>(destination.GetName("surface")),
                                destination,
                                (buffer, shape) => SetShape(buffer, shape)), destination);
                            Store((destination) => S57_Metadata(
                                "MetadataP",
                                (tableName) => source.OpenDataset<FeatureClass>(source.GetName(tableName)),
                                QueryFilter,
                                () => destination.OpenDataset<FeatureClass>(destination.GetName("point")),
                                destination,
                                (buffer, shape) => SetShape(buffer, shape)), destination);


                            Logger.Current.Debug($"Converting Areas And Limits");
                            Store((destination) => S57_RegulatedAreasAndLimits(
                                "RegulatedAreasAndLimitsA",
                                (tableName) => source.OpenDataset<FeatureClass>(source.GetName(tableName)),
                                QueryFilter,
                                () => destination.OpenDataset<FeatureClass>(destination.GetName("surface")),
                                (buffer, shape) => SetShape(buffer, shape)), destination);
                            Store((destination) => S57_RegulatedAreasAndLimits(
                                "RegulatedAreasAndLimitsL",
                                (tableName) => source.OpenDataset<FeatureClass>(source.GetName(tableName)),
                                QueryFilter,
                                () => destination.OpenDataset<FeatureClass>(destination.GetName("curve")),
                                (buffer, shape) => SetShape(buffer, shape)), destination);
                            Store((destination) => S57_RegulatedAreasAndLimits(
                                "RegulatedAreasAndLimitsP",
                                (tableName) => source.OpenDataset<FeatureClass>(source.GetName(tableName)),
                                QueryFilter,
                                () => destination.OpenDataset<FeatureClass>(destination.GetName("point")),
                                (buffer, shape) => SetShape(buffer, shape)), destination);


                            Logger.Current.Debug($"Converting Dangers");
                            Store((destination) => S57_Dangers(
                                "DangersA",
                                (tableName) => source.OpenDataset<FeatureClass>(source.GetName(tableName)),
                                QueryFilter,
                                () => destination.OpenDataset<FeatureClass>(destination.GetName("surface")),
                                (buffer, shape) => SetShape(buffer, shape)), destination);
                            Store((destination) => S57_Dangers(
                                "DangersL",
                                (tableName) => source.OpenDataset<FeatureClass>(source.GetName(tableName)),
                                QueryFilter,
                                () => destination.OpenDataset<FeatureClass>(destination.GetName("curve")),
                                (buffer, shape) => SetShape(buffer, shape)), destination);
                            Store((destination) => S57_Dangers(
                                "DangersP",
                                (tableName) => source.OpenDataset<FeatureClass>(source.GetName(tableName)),
                                QueryFilter,
                                () => destination.OpenDataset<FeatureClass>(destination.GetName("point")),
                                (buffer, shape) => SetShape(buffer, shape)), destination);



                            Logger.Current.Debug($"Converting Natural Features");
                            Store((destination) => S57_NaturalFeatures(
                                "NaturalFeaturesA",
                                (tableName) => source.OpenDataset<FeatureClass>(source.GetName(tableName)),
                                QueryFilter,
                                () => destination.OpenDataset<FeatureClass>(destination.GetName("surface")),
                                (buffer, shape) => SetShape(buffer, shape)), destination);
                            Store((destination) => S57_NaturalFeatures(
                                "NaturalFeaturesL",
                                (tableName) => source.OpenDataset<FeatureClass>(source.GetName(tableName)),
                                QueryFilter,
                                () => destination.OpenDataset<FeatureClass>(destination.GetName("curve")),
                                (buffer, shape) => SetShape(buffer, shape)), destination);
                            Store((destination) => S57_NaturalFeatures(
                                "NaturalFeaturesP",
                                (tableName) => source.OpenDataset<FeatureClass>(source.GetName(tableName)),
                                QueryFilter,
                                () => destination.OpenDataset<FeatureClass>(destination.GetName("point")),
                                (buffer, shape) => SetShape(buffer, shape)), destination);


                            Logger.Current.Debug($"Converting Cultural Features");
                            Store((destination) => S57_CulturalFeaturesA(source, destination, QueryFilter), destination);
                            Store((destination) => S57_CulturalFeaturesL(source, destination, QueryFilter), destination);
                            Store((destination) => S57_CulturalFeaturesP(source, destination, QueryFilter), destination);

                            informationBinding<SpatialAssociation>[] spatialQuality = [];
                            Store((destination) => {
                                spatialQuality = CreateAssociationSpatialQuality(destination);
                            }, destination);

                            Logger.Current.Debug($"Converting Contours");
                            Store((destination) => S57_Depths(
                                "DepthsL",
                                (tableName) => source.OpenDataset<FeatureClass>(source.GetName(tableName)),
                                QueryFilter,
                                () => destination.OpenDataset<FeatureClass>(destination.GetName("curve")),
                                (buffer, shape) => SetShape(buffer, shape), spatialQuality), destination);


                            //Logger.Current.Information($"Converting S101_RecommendedTracksAndRoutes");
                            //Store(() => S101_RecommendedTracksAndRoutes(source, destination, QueryFilter));



                            Logger.Current.Debug($"Converting PortsAndServices");
                            Store((destination) => S57_PortsAndServicesA(source, destination, QueryFilter), destination);
                            Store((destination) => S57_PortsAndServicesL(source, destination, QueryFilter), destination);
                            Store((destination) => S57_PortsAndServicesP(source, destination, QueryFilter), destination);

                            Logger.Current.Debug($"Converting Soundings");
                            Store((destination) => S57_SoundingsP(source, destination, QueryFilter), destination);

                            Logger.Current.Debug($"Converting Tides And Variations");
                            Store((destination) => S57_TidesAndVariations(
                                "TidesAndVariationsA",
                                (tableName) => source.OpenDataset<FeatureClass>(source.GetName(tableName)),
                                QueryFilter,
                                () => destination.OpenDataset<FeatureClass>(destination.GetName("surface")),
                                (buffer, shape) => SetShape(buffer, shape)), destination);
                            Store((destination) => S57_TidesAndVariations(
                                "TidesAndVariationsL",
                                (tableName) => source.OpenDataset<FeatureClass>(source.GetName(tableName)),
                                QueryFilter,
                                () => destination.OpenDataset<FeatureClass>(destination.GetName("curve")),
                                (buffer, shape) => SetShape(buffer, shape)), destination);
                            Store((destination) => S57_TidesAndVariations(
                                "TidesAndVariationsP",
                                (tableName) => source.OpenDataset<FeatureClass>(source.GetName(tableName)),
                                QueryFilter,
                                () => destination.OpenDataset<FeatureClass>(destination.GetName("point")),
                                (buffer, shape) => SetShape(buffer, shape)), destination);

                            Logger.Current.Debug($"Converting Seabeds");
                            Store((destination) => S57_Seabed(
                                "SeabedA",
                                (tableName) => source.OpenDataset<FeatureClass>(source.GetName(tableName)),
                                QueryFilter,
                                () => destination.OpenDataset<FeatureClass>(destination.GetName("surface")),
                                (buffer, shape) => SetShape(buffer, shape)), destination);
                            Store((destination) => S57_Seabed(
                                "SeabedL",
                                (tableName) => source.OpenDataset<FeatureClass>(source.GetName(tableName)),
                                QueryFilter,
                                () => destination.OpenDataset<FeatureClass>(destination.GetName("curve")),
                                (buffer, shape) => SetShape(buffer, shape)), destination);
                            Store((destination) => S57_Seabed(
                                "SeabedP",
                                (tableName) => source.OpenDataset<FeatureClass>(source.GetName(tableName)),
                                QueryFilter,
                                () => destination.OpenDataset<FeatureClass>(destination.GetName("point")),
                                (buffer, shape) => SetShape(buffer, shape)), destination);

                            Logger.Current.Debug($"Converting CoastLines");
                            Store((destination) => S57_Coastline(
                                "CoastlineA",
                                (tableName) => source.OpenDataset<FeatureClass>(source.GetName(tableName)),
                                QueryFilter,
                                () => destination.OpenDataset<FeatureClass>(destination.GetName("surface")),
                                (buffer, shape) => SetShape(buffer, shape), []), destination);
                            Store((destination) => S57_Coastline(
                                "CoastlineL",
                                (tableName) => source.OpenDataset<FeatureClass>(source.GetName(tableName)),
                                QueryFilter,
                                () => destination.OpenDataset<FeatureClass>(destination.GetName("curve")),
                                (buffer, shape) => SetShape(buffer, shape), spatialQuality), destination);
                            Store((destination) => S57_Coastline(
                                "CoastlineP",
                                (tableName) => source.OpenDataset<FeatureClass>(source.GetName(tableName)),
                                QueryFilter,
                                () => destination.OpenDataset<FeatureClass>(destination.GetName("point")),
                                (buffer, shape) => SetShape(buffer, shape), []), destination);

                            Logger.Current.Debug($"Converting Depth Areas");
                            Store((destination) => S57_Depths(
                                "DepthsA",
                                (tableName) => source.OpenDataset<FeatureClass>(source.GetName(tableName)),
                                QueryFilter,
                                () => destination.OpenDataset<FeatureClass>(destination.GetName("surface")),
                                (buffer, shape) => SetShape(buffer, shape), []), destination);


                            Logger.Current.Debug($"Converting Ice features");
                            Store((destination) => S57_IcefeaturesA(source, destination, QueryFilter), destination);

                            Logger.Current.Debug($"Converting Military Features");
                            Store((destination) => S57_MilitaryFeature(
                                "MilitaryFeaturesA",
                                (tableName) => source.OpenDataset<FeatureClass>(source.GetName(tableName)),
                                QueryFilter,
                                () => destination.OpenDataset<FeatureClass>(destination.GetName("surface")),
                                (buffer, shape) => SetShape(buffer, shape)), destination);
                            Store((destination) => S57_MilitaryFeature(
                                "MilitaryFeaturesP",
                                (tableName) => source.OpenDataset<FeatureClass>(source.GetName(tableName)),
                                QueryFilter,
                                () => destination.OpenDataset<FeatureClass>(destination.GetName("point")),
                                (buffer, shape) => SetShape(buffer, shape)), destination);



                            Logger.Current.Debug($"Converting Offshore Installations");
                            Store((destination) => S57_OffshoreInstallations(
                                "OffshoreInstallationsA",
                                (tableName) => source.OpenDataset<FeatureClass>(source.GetName(tableName)),
                                QueryFilter,
                                () => destination.OpenDataset<FeatureClass>(destination.GetName("surface")),
                                (buffer, shape) => SetShape(buffer, shape)), destination);
                            Store((destination) => S57_OffshoreInstallations(
                                "OffshoreInstallationsL",
                                (tableName) => source.OpenDataset<FeatureClass>(source.GetName(tableName)),
                                QueryFilter,
                                () => destination.OpenDataset<FeatureClass>(destination.GetName("curve")),
                                (buffer, shape) => SetShape(buffer, shape)), destination);
                            Store((destination) => S57_OffshoreInstallations(
                                "OffshoreInstallationsP",
                                (tableName) => source.OpenDataset<FeatureClass>(source.GetName(tableName)),
                                QueryFilter,
                                () => destination.OpenDataset<FeatureClass>(destination.GetName("point")),
                                (buffer, shape) => SetShape(buffer, shape)), destination);

                            Logger.Current.Debug($"Converting Tracks And Routes");
                            Store((destination) => S57_TracksAndRoutes(
                                "TracksAndRoutesA",
                                (tableName) => source.OpenDataset<FeatureClass>(source.GetName(tableName)),
                                QueryFilter,
                                () => destination.OpenDataset<FeatureClass>(destination.GetName("surface")),
                                (buffer, shape) => SetShape(buffer, shape)), destination);
                            Store((destination) => S57_TracksAndRoutes(
                                "TracksAndRoutesL",
                                (tableName) => source.OpenDataset<FeatureClass>(source.GetName(tableName)),
                                QueryFilter,
                                () => destination.OpenDataset<FeatureClass>(destination.GetName("curve")),
                                (buffer, shape) => SetShape(buffer, shape)), destination);
                            Store((destination) => S57_TracksAndRoutes(
                                "TracksAndRoutesP",
                                (tableName) => source.OpenDataset<FeatureClass>(source.GetName(tableName)),
                                QueryFilter,
                                () => destination.OpenDataset<FeatureClass>(destination.GetName("point")),
                                (buffer, shape) => SetShape(buffer, shape)), destination);

                            Logger.Current.Debug($"Converting Aids to Navigation");
                            Store((destination) => S57_AidsToNavigationP(source, destination, QueryFilter), destination);

                            Logger.Current.Debug($"Converting User Defined Features");
                            Store((destination) => S57_UserDefinedFeaturesP(source, destination, QueryFilter), destination);



                            Logger.Current.Debug($"Converting Note files");
                            Store((destination) => NauticalInformations.Instance.Flush(destination), destination);


                            //Store(() => FeatureRelations.Instance.CreateRelations(destination));
                        }
                        //#endif
                        //Logger.Current.Information($"Igniting afterburner");
                        //Afterburner.Initialize(destination);
                        //Afterburner.Instance.CutClosedRoadLines();

                        //using (var surface = destination.OpenDataset<FeatureClass>("surface")) {
                        //    var reader = new WKTReader();

                        //    using var search = surface.Search(null, true);
                        //    while (search.MoveNext()) {
                        //        var shape = (Polygon)((Feature)search.Current).GetShape();

                        //        if (shape.ExteriorRingCount > 1) System.Diagnostics.Debugger.Break();

                        //        string wkt = GeometryEngine.Instance.ExportToWKT(WktExportFlags.WktExportPolygon, shape);

                        //        var geometry = reader.Read(wkt);
                        //        if (!geometry.IsValid) System.Diagnostics.Debugger.Break();

                        //        //if(geometry is NetTopologySuite.Geometries.MultiPolygon multiPolygon) {
                        //        //    System.Diagnostics.Debugger.Break();
                        //        //}
                        //    }
                        //}
                    }
                }
            }

            //  TODO: M_NPUB
            using (var destination = createTargetGeodatabase()) {
                Logger.Current.Information($"Building M_NPUB");

                using var surface = destination.OpenDataset<FeatureClass>(destination.GetName("surface"));

            }

            //  Aggregations
            using (var destination = createTargetGeodatabase()) {
                Logger.Current.Information($"Building aggregations...");

                using (Geodatabase source = createGeodatabase()) {
                    Dictionary<Guid, (string OBJNAM, string NOBJNM)> collections = [];

                    using var naturalFeaturesA = source.OpenDataset<FeatureClass>(source.GetName("NaturalFeaturesA"));

                    // naturalFeaturesA: FCSubType = 25 SEAARE_SeaAreaNamedWaterArea

                    using var plts_collections = source.OpenDataset<Table>(source.GetName("PLTS_COLLECTIONS"));
                    using var plts_frel = source.OpenDataset<Table>(source.GetName("PLTS_FREL"));

                    using (var search = plts_collections.Search(new QueryFilter {
                        WhereClause = "FCSubType = 1",  //  C_AGGR_Aggregation
                    }, true)) {
                        while (search.MoveNext()) {
                            var current = search.Current;

                            var noid = Guid.Parse(Convert.ToString(current["NOID"])!);
                            var objnam = Convert.ToString(current["OBJNAM"]) ?? string.Empty;
                            var nobjnm = Convert.ToString(current["NOBJNM"]) ?? string.Empty;

                            collections.Add(noid, (objnam, nobjnm));
                        }
                    }

                    var regexTss = new Regex(@"(^TSS )|(^Traffic Separation Scheme )", RegexOptions.IgnoreCase | RegexOptions.Singleline);

                    string[] errors = [];

                    foreach (var c_aggr in collections) {
                        Store((d) => {
                            using var point = d.OpenDataset<FeatureClass>(d.GetName("point"));
                            using var curve = d.OpenDataset<FeatureClass>(d.GetName("curve"));
                            using var surface = d.OpenDataset<FeatureClass>(d.GetName("surface"));
                            using var featuretype = d.OpenDataset<Table>(d.GetName("featuretype"));

                            var key = $"{c_aggr.Key:B}".ToUpperInvariant();

                            //if ("{4D776A32-6F87-4D34-A462-AB6D3AC332D2}".Equals(key)) System.Diagnostics.Debugger.Break();

                            //if("{777AA18E-F33A-4D84-97D4-032ADC48C958}".Equals(key)) System.Diagnostics.Debugger.Break();

                            var aggregationTrafficSeparationScheme = TrafficSeparationScheme.featureBindingsDefinitions.Single(e => "TrafficSeparationSchemeAggregation".Equals(e.association) && "association".Equals(e.roleType));
                            var aggregationDeepWaterRoute = DeepWaterRoute.featureBindingsDefinitions.Single(e => "DeepWaterRouteAggregation".Equals(e.association) && "association".Equals(e.roleType));
                            var aggregationRangeSystemAggregation = RangeSystem.featureBindingsDefinitions.Single(e => "RangeSystemAggregation".Equals(e.association) && "association".Equals(e.roleType));
                            var aggregationFairwayAggregation = FairwaySystem.featureBindingsDefinitions.Single(e => "FairwayAggregation".Equals(e.association) && "association".Equals(e.roleType));
                            var aggregationRoofedStructureAggregation = StructureOverNavigableWater.featureBindingsDefinitions.Single(e => "RoofedStructureAggregation".Equals(e.association) && "association".Equals(e.roleType));
                            var aggregationMooringTrotAggregation = MooringTrot.featureBindingsDefinitions.Single(e => "MooringTrotAggregation".Equals(e.association) && "association".Equals(e.roleType));
                            var aggregationTwoWayRouteAggregation = TwoWayRoute.featureBindingsDefinitions.Single(e => "TwoWayRouteAggregation".Equals(e.association) && "association".Equals(e.roleType));
                            var aggregationCautionAreaAssociation = CautionArea.featureBindingsDefinitions.Single(e => "CautionAreaAssociation".Equals(e.association) && "association".Equals(e.roleType));
                            var aggregationASLAggregation = ArchipelagicSeaLane.featureBindingsDefinitions.Single(e => "ASLAggregation".Equals(e.association) && "association".Equals(e.roleType));
                            //var aggregationAidsToNavigationAssociation = .featureBindingsDefinitions.Single(e => "AidsToNavigationAssociation".Equals(e.association) && "association".Equals(e.roleType));
                            var aggregationPilotageDistrictAssociation = PilotageDistrict.featureBindingsDefinitions.Single(e => "PilotageDistrictAssociation".Equals(e.association) && "association".Equals(e.roleType));
                            var aggregationIslandAggregation = IslandGroup.featureBindingsDefinitions.Single(e => "IslandAggregation".Equals(e.association) && "association".Equals(e.roleType));

                            string[] uids = [];

                            using (var search = plts_frel.Search(new QueryFilter {
                                WhereClause = $"upper(SRC_SUB) = 'C_AGGR_AGGREGATION' AND SRC_UID = '{key}'"
                            }, true)) {
                                while (search.MoveNext()) {
                                    var current = search.Current;

                                    var dest_fc = Convert.ToString(current["DEST_FC"])!;
                                    var dest_uid = Guid.Parse(Convert.ToString(current["DEST_UID"])!);

                                    var oids = ConversionAnalytics.Instance.GetOIDs(dest_fc, dest_uid);

                                    uids = [.. uids, .. oids.Where(e => !e.Equals("ATTRIBUTE. NO NAME AVAILABLE", StringComparison.InvariantCultureIgnoreCase))];
                                }
                            }

                            if (!uids.Any()) System.Diagnostics.Debugger.Break();

                            var isTrafficSeparationScheme = false;
                            var isDeepWaterRoute = false;
                            var isRangeSystemAggregation = false;
                            var isFairwayAggregation = false;
                            var isRoofedStructureAggregation = false;
                            var isMooringTrotAggregation = false;
                            var isTwoWayRouteAggregation = false;
                            var isCautionAreaAssociation = false;
                            var isASLAggregation = false;
                            var isPilotageDistrictAssociation = false;
                            var isIslandAggregation = false;

                            string[] aggregationCodes = [];

                            using (var search = curve.Search(new QueryFilter {
                                WhereClause = $"GlobalID IN ({string.Join(',', uids.Select(e => $"'{e.ToUpper()}'"))})",
                                SubFields = "OBJECTID,GlobalID, code"
                            }, true)) {
                                while (search.MoveNext()) {
                                    var current = (Feature)search.Current;

                                    var name = current.UID();
                                    var code = Convert.ToString(current["code"])!;

                                    aggregationCodes = [.. aggregationCodes, code];

                                    isTrafficSeparationScheme |= aggregationTrafficSeparationScheme.featureTypes.Contains(code);
                                    isDeepWaterRoute |= aggregationDeepWaterRoute.featureTypes.Contains(code);
                                    isRangeSystemAggregation |= aggregationRangeSystemAggregation.featureTypes.Contains(code);
                                    isFairwayAggregation |= aggregationFairwayAggregation.featureTypes.Contains(code);
                                    isRoofedStructureAggregation |= aggregationRoofedStructureAggregation.featureTypes.Contains(code);
                                    isMooringTrotAggregation |= aggregationMooringTrotAggregation.featureTypes.Contains(code);
                                    isTwoWayRouteAggregation |= aggregationTwoWayRouteAggregation.featureTypes.Contains(code);
                                    isCautionAreaAssociation |= aggregationCautionAreaAssociation.featureTypes.Contains(code);
                                    isASLAggregation |= aggregationASLAggregation.featureTypes.Contains(code);
                                    isPilotageDistrictAssociation |= aggregationPilotageDistrictAssociation.featureTypes.Contains(code);
                                    isIslandAggregation |= aggregationIslandAggregation.featureTypes.Contains(code);
                                }
                            }

                            using (var search = surface.Search(new QueryFilter {
                                WhereClause = $"GlobalID IN ({string.Join(',', uids.Select(e => $"'{e.ToUpper()}'"))})",
                                SubFields = "OBJECTID,GlobalID, code"
                            }, true)) {
                                while (search.MoveNext()) {
                                    var current = (Feature)search.Current;

                                    var name = current.UID();
                                    var code = Convert.ToString(current["code"])!;

                                    aggregationCodes = [.. aggregationCodes, code];

                                    isTrafficSeparationScheme |= aggregationTrafficSeparationScheme.featureTypes.Contains(code);
                                    isDeepWaterRoute |= aggregationDeepWaterRoute.featureTypes.Contains(code);
                                    isRangeSystemAggregation |= aggregationRangeSystemAggregation.featureTypes.Contains(code);
                                    isFairwayAggregation |= aggregationFairwayAggregation.featureTypes.Contains(code);
                                    isRoofedStructureAggregation |= aggregationRoofedStructureAggregation.featureTypes.Contains(code);
                                    isMooringTrotAggregation |= aggregationMooringTrotAggregation.featureTypes.Contains(code);
                                    isTwoWayRouteAggregation |= aggregationTwoWayRouteAggregation.featureTypes.Contains(code);
                                    isCautionAreaAssociation |= aggregationCautionAreaAssociation.featureTypes.Contains(code);
                                    isASLAggregation |= aggregationASLAggregation.featureTypes.Contains(code);
                                    isPilotageDistrictAssociation |= aggregationPilotageDistrictAssociation.featureTypes.Contains(code);
                                    isIslandAggregation |= aggregationIslandAggregation.featureTypes.Contains(code);
                                }
                            }

                            using (var search = point.Search(new QueryFilter {
                                WhereClause = $"GlobalID IN ({string.Join(',', uids.Select(e => $"'{e.ToUpper()}'"))})",
                                SubFields = "OBJECTID,GlobalID, code"
                            }, true)) {
                                while (search.MoveNext()) {
                                    var current = (Feature)search.Current;

                                    var name = current.UID();
                                    var code = Convert.ToString(current["code"])!;

                                    aggregationCodes = [.. aggregationCodes, code];

                                    isTrafficSeparationScheme |= aggregationTrafficSeparationScheme.featureTypes.Contains(code);
                                    isDeepWaterRoute |= aggregationDeepWaterRoute.featureTypes.Contains(code);
                                    isRangeSystemAggregation |= aggregationRangeSystemAggregation.featureTypes.Contains(code);
                                    isFairwayAggregation |= aggregationFairwayAggregation.featureTypes.Contains(code);
                                    isRoofedStructureAggregation |= aggregationRoofedStructureAggregation.featureTypes.Contains(code);
                                    isMooringTrotAggregation |= aggregationMooringTrotAggregation.featureTypes.Contains(code);
                                    isTwoWayRouteAggregation |= aggregationTwoWayRouteAggregation.featureTypes.Contains(code);
                                    isCautionAreaAssociation |= aggregationCautionAreaAssociation.featureTypes.Contains(code);
                                    isASLAggregation |= aggregationASLAggregation.featureTypes.Contains(code);
                                    isPilotageDistrictAssociation |= aggregationPilotageDistrictAssociation.featureTypes.Contains(code);
                                    isIslandAggregation |= aggregationIslandAggregation.featureTypes.Contains(code);
                                }
                            }

                            aggregationCodes = [.. aggregationCodes.Distinct()];

                            S100FC.FeatureType? instance = null;

                            Func<Feature, featureBinding[], featureBinding[]> callbackFeatureBindings = (current, featureBindings) => {
                                return featureBindings;
                            };

                            Func<string, Row, featureBinding[], featureBinding[]> callbackUpdate = (featureId, current, featureBindings) => {
                                return featureBindings;
                            };

                            if (isTwoWayRouteAggregation)
                                ;

                            if (isDeepWaterRoute && !isTrafficSeparationScheme) {
                                //Logger.Current.Information($"DeepWaterRouteAggregation: {key} ({string.Join(',', aggregationCodes)})");

                                var deepWaterRoute = new DeepWaterRoute();
                                instance = deepWaterRoute;

                                var aidsToNavigationAssociations = DeepWaterRoute.featureBindingsDefinitions.Single(e => "AidsToNavigationAssociation".Equals(e.association) && "association".Equals(e.roleType));

                                Primitives primitive = Primitives.noGeometry;

                                callbackFeatureBindings = (current, featureBindings) => {
                                    var name = current.UID();
                                    var code = Convert.ToString(current["code"]);

                                    if ("SeaAreaNamedWaterArea".Equals(code)) {
                                        var seaAreaNamedWaterArea = AttributeFlattenExtensions.Unflatten<SeaAreaNamedWaterArea>(Convert.ToString(current["attributebindings"])!, typeof(SeaAreaNamedWaterArea));

                                        if (seaAreaNamedWaterArea.featureName.Any()) {
                                            deepWaterRoute.featureName = [];
                                            foreach (var e in seaAreaNamedWaterArea.featureName) {
                                                deepWaterRoute.featureName = [.. deepWaterRoute.featureName, new featureName {
                                                    name = regexTss.Replace(e.name!, string.Empty),
                                                    language=e.language,
                                                    nameUsage = e.nameUsage,
                                                }];
                                            }

                                            primitive = current.GetShape().GeometryType switch {
                                                GeometryType.Point => Primitives.point,
                                                GeometryType.Multipoint => Primitives.pointSet,
                                                GeometryType.Polyline => Primitives.curve,
                                                GeometryType.Polygon => Primitives.surface,
                                                _ => throw new InvalidOperationException(),
                                            };
                                        }
                                    }

                                    if (aggregationDeepWaterRoute.featureTypes.Contains(code)) {
                                        featureBindings = [.. featureBindings, new featureBinding {
                                             roleType = "association",
                                             role = "theComponent",
                                             association = new association{
                                                 S100FC_code = nameof(DeepWaterRouteAggregation),
                                             },
                                             featureType = code,
                                             featureId = name,
                                             Primitive = Primitives.surface,
                                        }];
                                    }

                                    if (aidsToNavigationAssociations.featureTypes.Contains(code)) {
                                        featureBindings = [.. featureBindings, new featureBinding {
                                             roleType = "association",
                                             role = "theComponent",
                                             association = new association{
                                                 S100FC_code = nameof(AidsToNavigationAssociation),
                                             },
                                             featureType = code,
                                             featureId = name,
                                             Primitive = Primitives.point,
                                        }];
                                    }

                                    return featureBindings;
                                };

                                callbackUpdate = (featureId, current, featureBindings) => {
                                    var code = Convert.ToString(current["code"]);

                                    if (aidsToNavigationAssociations.featureTypes.Contains(code)) {
                                        featureBindings = [.. featureBindings, new featureBinding {
                                            roleType = "aggregation",
                                            role = "theCollection",
                                            association = new association {
                                                S100FC_code = nameof(AidsToNavigationAssociation),
                                            },
                                            featureType = nameof(TrafficSeparationScheme),
                                            featureId = featureId,
                                            Primitive = primitive,
                                        }];
                                    }
                                    else if (aggregationDeepWaterRoute.featureTypes.Contains(code)) {
                                        featureBindings = [.. featureBindings, new featureBinding {
                                            roleType = "aggregation",
                                            role = "theCollection",
                                            association = new association {
                                                S100FC_code = nameof(DeepWaterRouteAggregation),
                                            },
                                            featureType = nameof(DeepWaterRoute),
                                            featureId = featureId,
                                            Primitive = primitive,
                                        }];
                                    }

                                    return featureBindings;
                                };

                            }
                            else if (isTwoWayRouteAggregation) {
                                Logger.Current.Information($"TwoWayRouteAggregation: {key} ({string.Join(',', aggregationCodes)})");
                                return;
                            }
                            else if (isFairwayAggregation) {
                                Logger.Current.Information($"FairwayAggregation: {key} ({string.Join(',', aggregationCodes)})");
                                return;
                            }
                            else if (isRangeSystemAggregation) {
                                //Logger.Current.Information($"RangeSystemAggregation: {key} ({string.Join(',', aggregationCodes)})");

                                var rangeSystem = new RangeSystem();
                                instance = rangeSystem;

                                Primitives primitive = Primitives.noGeometry;

                                callbackFeatureBindings = (current, featureBindings) => {
                                    var name = current.UID();
                                    var code = Convert.ToString(current["code"]);

                                    if ("SeaAreaNamedWaterArea".Equals(code)) {
                                        var seaAreaNamedWaterArea = AttributeFlattenExtensions.Unflatten<SeaAreaNamedWaterArea>(Convert.ToString(current["attributebindings"])!, typeof(SeaAreaNamedWaterArea));

                                        if (seaAreaNamedWaterArea.featureName.Any()) {
                                            rangeSystem.featureName = [];
                                            foreach (var e in seaAreaNamedWaterArea.featureName) {
                                                rangeSystem.featureName = [.. rangeSystem.featureName, new featureName {
                                                    name = regexTss.Replace(e.name!, string.Empty),
                                                    language=e.language,
                                                    nameUsage = e.nameUsage,
                                                }];
                                            }

                                            primitive = current.GetShape().GeometryType switch {
                                                GeometryType.Point => Primitives.point,
                                                GeometryType.Multipoint => Primitives.pointSet,
                                                GeometryType.Polyline => Primitives.curve,
                                                GeometryType.Polygon => Primitives.surface,
                                                _ => throw new InvalidOperationException(),
                                            };
                                        }
                                    }

                                    if (aggregationRangeSystemAggregation.featureTypes.Contains(code)) {
                                        featureBindings = [.. featureBindings, new featureBinding {
                                             roleType = "association",
                                             role = "theComponent",
                                             association = new association{
                                                 S100FC_code = nameof(RangeSystemAggregation),
                                             },
                                             featureType = code,
                                             featureId = name,
                                             Primitive = Primitives.surface,
                                        }];
                                    }

                                    return featureBindings;
                                };

                                callbackUpdate = (featureId, current, featureBindings) => {
                                    var code = Convert.ToString(current["code"]);

                                    if (aggregationDeepWaterRoute.featureTypes.Contains(code)) {
                                        featureBindings = [.. featureBindings, new featureBinding {
                                            roleType = "aggregation",
                                            role = "theCollection",
                                            association = new association {
                                                S100FC_code = nameof(RangeSystemAggregation),
                                            },
                                            featureType = nameof(RangeSystem),
                                            featureId = featureId,
                                            Primitive = primitive,
                                        }];
                                    }

                                    return featureBindings;
                                };

                            }
                            else if (isTrafficSeparationScheme) {
                                //Logger.Current.Information($"TrafficSeparationSchemeAggregation: {key} ({string.Join(',', aggregationCodes)})");

                                var trafficSeparationScheme = new TrafficSeparationScheme();
                                instance = trafficSeparationScheme;

                                var aidsToNavigationAssociations = TrafficSeparationScheme.featureBindingsDefinitions.Single(e => "AidsToNavigationAssociation".Equals(e.association) && "association".Equals(e.roleType));

                                Primitives primitive = Primitives.noGeometry;

                                callbackFeatureBindings = (current, featureBindings) => {
                                    var name = current.UID();
                                    var code = Convert.ToString(current["code"]);

                                    if ("SeaAreaNamedWaterArea".Equals(code)) {
                                        var seaAreaNamedWaterArea = AttributeFlattenExtensions.Unflatten<SeaAreaNamedWaterArea>(Convert.ToString(current["attributebindings"])!, typeof(SeaAreaNamedWaterArea));

                                        if (seaAreaNamedWaterArea.featureName.Any()) {
                                            trafficSeparationScheme.featureName = [];
                                            foreach (var e in seaAreaNamedWaterArea.featureName) {
                                                trafficSeparationScheme.featureName = [.. trafficSeparationScheme.featureName, new featureName {
                                                    name = regexTss.Replace(e.name!, string.Empty),
                                                    language=e.language,
                                                    nameUsage = e.nameUsage,
                                                }];
                                            }

                                            primitive = current.GetShape().GeometryType switch {
                                                GeometryType.Point => Primitives.point,
                                                GeometryType.Multipoint => Primitives.pointSet,
                                                GeometryType.Polyline => Primitives.curve,
                                                GeometryType.Polygon => Primitives.surface,
                                                _ => throw new InvalidOperationException(),
                                            };
                                        }
                                    }

                                    if (aggregationTrafficSeparationScheme.featureTypes.Contains(code)) {
                                        featureBindings = [.. featureBindings, new featureBinding {
                                             roleType = "association",
                                             role = "theComponent",
                                             association = new association{
                                                 S100FC_code = nameof(TrafficSeparationSchemeAggregation),
                                             },
                                             featureType = code,
                                             featureId = name,
                                             Primitive = Primitives.surface,
                                        }];
                                    }

                                    if (aidsToNavigationAssociations.featureTypes.Contains(code)) {
                                        featureBindings = [.. featureBindings, new featureBinding {
                                             roleType = "association",
                                             role = "theComponent",
                                             association = new association{
                                                 S100FC_code = nameof(AidsToNavigationAssociation),
                                             },
                                             featureType = code,
                                             featureId = name,
                                             Primitive = Primitives.point,
                                        }];
                                    }

                                    return featureBindings;
                                };

                                callbackUpdate = (featureId, current, featureBindings) => {
                                    var code = Convert.ToString(current["code"]);

                                    if (aidsToNavigationAssociations.featureTypes.Contains(code)) {
                                        featureBindings = [.. featureBindings, new featureBinding {
                                            roleType = "aggregation",
                                            role = "theCollection",
                                            association = new association {
                                                S100FC_code = nameof(AidsToNavigationAssociation),
                                            },
                                            featureType = nameof(TrafficSeparationScheme),
                                            featureId = featureId,
                                            Primitive = primitive,
                                        }];
                                    }
                                    else if (aggregationTrafficSeparationScheme.featureTypes.Contains(code)) {
                                        featureBindings = [.. featureBindings, new featureBinding {
                                            roleType = "aggregation",
                                            role = "theCollection",
                                            association = new association {
                                                S100FC_code = nameof(TrafficSeparationSchemeAggregation),
                                            },
                                            featureType = nameof(TrafficSeparationScheme),
                                            featureId = featureId,
                                            Primitive = primitive,
                                        }];
                                    }

                                    return featureBindings;
                                };
                            }
                            else {
                                Logger.Current.Information($"Unknown Aggregation: {key} ({string.Join(',', aggregationCodes)})");
                                return;
                            }

                            featureBinding[] featureBindings = [];

                            var any = false;

                            Polygon? shapeSeaAreaNamedWaterArea = null;

                            Polygon[] shapeSurfaces = [];
                            Polyline[] shapeCurves = [];

                            int compilationscale = 10000000;

                            using (var search = surface.Search(new QueryFilter {
                                WhereClause = $"GlobalID IN ({string.Join(',', uids.Select(e => $"'{e.ToUpper()}'"))})",
                            }, true)) {
                                while (search.MoveNext()) {
                                    any = true;
                                    var current = (Feature)search.Current;

                                    var name = current.UID();
                                    var code = Convert.ToString(current["code"]);

                                    if ("SeaAreaNamedWaterArea".Equals(code)) {
                                        //if (shapeSeaAreaNamedWaterArea is not null)
                                        //    System.Diagnostics.Debugger.Break();
                                        shapeSeaAreaNamedWaterArea = (Polygon)current.GetShape().Clone();
                                        compilationscale = Convert.ToInt32(current["compilationscale"]);
                                    }
                                    else {
                                        shapeSurfaces = [.. shapeSurfaces, (Polygon)current.GetShape().Clone()];
                                    }

                                    featureBindings = callbackFeatureBindings(current, featureBindings);

                                }
                            }

                            using (var search = curve.Search(new QueryFilter {
                                WhereClause = $"GlobalID IN ({string.Join(',', uids.Select(e => $"'{e.ToUpper()}'"))})",
                            }, true)) {
                                while (search.MoveNext()) {
                                    any = true;
                                    var current = (Feature)search.Current;

                                    shapeCurves = [.. shapeCurves, (Polyline)current.GetShape().Clone()];

                                    featureBindings = callbackFeatureBindings(current, featureBindings);
                                }
                            }

                            using (var search = point.Search(new QueryFilter {
                                WhereClause = $"GlobalID IN ({string.Join(',', uids.Select(e => $"'{e.ToUpper()}'"))})",
                            }, true)) {
                                while (search.MoveNext()) {
                                    any = true;
                                    var current = (Feature)search.Current;

                                    featureBindings = callbackFeatureBindings(current, featureBindings);
                                }
                            }

                            if (!any) return;

                            string? featureId = null;

                            if (shapeSeaAreaNamedWaterArea is not null) {
                                using var buffer = surface.CreateRowBuffer();
                                buffer["ps"] = ps101;

                                buffer["code"] = instance.GetType().Name;
                                buffer["attributebindings"] = instance.Flatten();
                                buffer["featureBindings"] = System.Text.Json.JsonSerializer.Serialize(featureBindings);
                                buffer["informationbindings"] = "[]";
                                buffer["compilationscale"] = optimumDisplayScaleConverter(compilationscale) ?? 10000000;
                                buffer["sourceIdentifier"] = instance.sourceIdentifier;
                                SetShape(buffer, shapeSeaAreaNamedWaterArea);

                                var featureN = surface.CreateRow(buffer);
                                featureId = featureN.UID();
                            }
                            else if (shapeSurfaces.Any()) {
                                var shape = (Polygon)GeometryEngine.Instance.Union(shapeSurfaces);

                                if (!instance.permittedPrimitives.Contains(Primitives.surface))
                                    System.Diagnostics.Debugger.Break();

                                using var buffer = surface.CreateRowBuffer();
                                buffer["ps"] = ps101;

                                buffer["code"] = instance.GetType().Name;
                                buffer["attributebindings"] = instance.Flatten();
                                buffer["featureBindings"] = System.Text.Json.JsonSerializer.Serialize(featureBindings);
                                buffer["informationbindings"] = "[]";
                                buffer["compilationscale"] = optimumDisplayScaleConverter(compilationscale) ?? 10000000;
                                buffer["sourceIdentifier"] = instance.sourceIdentifier;
                                SetShape(buffer, shape);

                                var featureN = surface.CreateRow(buffer);
                                featureId = featureN.UID();
                            }
                            else if (shapeCurves.Any()) {
                                var shape = (Polyline)GeometryEngine.Instance.Union(shapeCurves);

                                if (!instance.permittedPrimitives.Contains(Primitives.curve))
                                    System.Diagnostics.Debugger.Break();

                                using var buffer = curve.CreateRowBuffer();
                                buffer["ps"] = ps101;

                                buffer["code"] = instance.GetType().Name;
                                buffer["attributebindings"] = instance.Flatten();
                                buffer["featureBindings"] = System.Text.Json.JsonSerializer.Serialize(featureBindings);
                                buffer["informationbindings"] = "[]";
                                buffer["compilationscale"] = optimumDisplayScaleConverter(compilationscale) ?? 10000000;
                                buffer["sourceIdentifier"] = instance.sourceIdentifier;
                                SetShape(buffer, shape);

                                var featureN = curve.CreateRow(buffer);
                                featureId = featureN.UID();
                            }
                            else {
                                if (!instance.permittedPrimitives.Contains(Primitives.noGeometry))
                                    System.Diagnostics.Debugger.Break();

                                using var buffer = featuretype.CreateRowBuffer();
                                buffer["ps"] = ps101;

                                buffer["code"] = instance.GetType().Name;
                                buffer["attributebindings"] = instance.Flatten();
                                buffer["featureBindings"] = System.Text.Json.JsonSerializer.Serialize(featureBindings);
                                buffer["informationbindings"] = "[]";
                                buffer["sourceIdentifier"] = instance.sourceIdentifier;

                                var featureN = featuretype.CreateRow(buffer);
                                featureId = featureN.UID();
                            }
                            ;

                            //  Update featurebindings
                            using (var search = point.CreateUpdateCursor(new QueryFilter {
                                WhereClause = $"GlobalID IN ({string.Join(',', featureBindings.Select(e => $"'{e.featureId}'"))})"
                            }, true)) {
                                while (search.MoveNext()) {
                                    var code = Convert.ToString(search.Current["code"]);

                                    var json = search.Current.IsNull("featureBindings") ? "[{}]" : Convert.ToString(search.Current["featureBindings"])!;
                                    var _ = System.Text.Json.JsonSerializer.Deserialize<featureBinding[]>(json);
                                    if (_ is null)
                                        _ = [];

                                    _ = callbackUpdate(featureId, search.Current, _);

                                    search.Current["featureBindings"] = System.Text.Json.JsonSerializer.Serialize(_);
                                    search.Update(search.Current);
                                }
                            }

                            using (var search = curve.CreateUpdateCursor(new QueryFilter {
                                WhereClause = $"GlobalID IN ({string.Join(',', featureBindings.Select(e => $"'{e.featureId}'"))})"
                            }, true)) {
                                while (search.MoveNext()) {
                                    var code = Convert.ToString(search.Current["code"]);

                                    var json = search.Current.IsNull("featureBindings") ? "[{}]" : Convert.ToString(search.Current["featureBindings"])!;
                                    var _ = System.Text.Json.JsonSerializer.Deserialize<featureBinding[]>(json);
                                    if (_ is null)
                                        _ = [];

                                    _ = callbackUpdate(featureId, search.Current, _);

                                    search.Current["featureBindings"] = System.Text.Json.JsonSerializer.Serialize(_);
                                    search.Update(search.Current);
                                }
                            }

                            using (var search = surface.CreateUpdateCursor(new QueryFilter {
                                WhereClause = $"GlobalID IN ({string.Join(',', featureBindings.Select(e => $"'{e.featureId}'"))})"
                            }, true)) {
                                while (search.MoveNext()) {
                                    var code = Convert.ToString(search.Current["code"]);

                                    var json = search.Current.IsNull("featureBindings") ? "[{}]" : Convert.ToString(search.Current["featureBindings"])!;
                                    var _ = System.Text.Json.JsonSerializer.Deserialize<featureBinding[]>(json);
                                    if (_ is null)
                                        _ = [];

                                    _ = callbackUpdate(featureId, search.Current, _);

                                    search.Current["featureBindings"] = System.Text.Json.JsonSerializer.Serialize(_);
                                    search.Update(search.Current);
                                }
                            }

                        }, destination);
                    }

                    ;
                }
            }


            //  Bridges
            using (var destination = createTargetGeodatabase()) {
                Logger.Current.Information($"Building bridges...");

                using var surface = destination.OpenDataset<FeatureClass>(destination.GetName("surface"));

                var whereClause = $"ps = '{ps101}' AND code IN ('SpanFixed','SpanOpening','Pontoon','PylonBridgeSupport')";

                int[] compilationscales = [];
                {
                    using var cursor = surface.Search(new QueryFilter {
                        PrefixClause = "Distinct",
                        WhereClause = whereClause,
                        SubFields = "compilationscale",
                        PostfixClause = "ORDER BY compilationscale DESC",
                    }, true);

                    while (cursor.MoveNext()) {
                        var _ = Convert.ToInt32(cursor.Current["compilationscale"]);
                        compilationscales = [.. compilationscales, _];
                    }
                }

                foreach (var compilationscale in compilationscales.OrderDescending()) {
                    var hashGeometry = new Dictionary<string, Polygon>();

                    //var hashGeometryBridgesupport = new Dictionary<string, Polygon>();

                    var hashFeatureType = new Dictionary<string, S100FC.FeatureType>();

                    using var cursor = surface.Search(new QueryFilter {
                        WhereClause = whereClause + $" AND compilationscale = {compilationscale}",
                    }, true);

                    while (cursor.MoveNext()) {
                        var f = (Feature)cursor.Current;

                        S100FC.FeatureType featureType = Convert.ToString(f["code"])?.ToLowerInvariant() switch {
                            "spanfixed" => AttributeFlattenExtensions.Unflatten<SpanFixed>(Convert.ToString(f["attributeBindings"]), typeof(SpanFixed)),
                            "spanopening" => AttributeFlattenExtensions.Unflatten<SpanOpening>(Convert.ToString(f["attributeBindings"]), typeof(SpanOpening)),
                            "pontoon" => AttributeFlattenExtensions.Unflatten<Pontoon>(Convert.ToString(f["attributeBindings"]), typeof(Pontoon)),
                            "pylonbridgesupport" => AttributeFlattenExtensions.Unflatten<PylonBridgeSupport>(Convert.ToString(f["attributeBindings"]), typeof(PylonBridgeSupport)),
                            _ => throw new NotImplementedException(),
                        };
                        hashFeatureType.Add(f.UID(), featureType);
                        hashGeometry.Add(f.UID(), (Polygon)f.GetShape().Clone());
                    }

                    var geometries = hashGeometry.Select(e => (e.Key, e.Value)).ToArray();


                    var hashSetUnion = GroupTouchingPolygons([.. geometries], (left, right) => {
                        // Touches = share boundary but interiors don't overlap
                        // Intersects = catches overlapping polygons too

                        //// If either feature is a PylonBridgeSupport on check for intersection!
                        //if (hashFeatureType[left.key] is PylonBridgeSupport || hashFeatureType[right.key] is PylonBridgeSupport) {
                        //    return GeometryEngine.Instance.Intersects(left.polygon, right.polygon);
                        //}
                        return GeometryEngine.Instance.Touches(left.polygon, right.polygon)
                                                        || GeometryEngine.Instance.Intersects(left.polygon, right.polygon);
                    });


                    Store((d) => {
                        using var surface = d.OpenDataset<FeatureClass>(d.GetName("surface"));
                        using var bufferBridge = surface.CreateRowBuffer();

                        bufferBridge["ps"] = ps101;

                        foreach (var _ in hashSetUnion) {
                            var dissolvedGeometry = _.Select(e => e.polygon);
                            var polygon = (Polygon)GeometryEngine.Instance.Union([.. dissolvedGeometry]);

                            var instance = new Bridge();

                            var ids = _.Select(e => e.key).ToList();

                            if (hashFeatureType.Where(e => ids.Contains(e.Key)).All(e => e.Value is Pontoon)) continue;

                            var parts = ids.Where(e => BridgeParts.Instance.Parts(e) is not null).Select(e => (e, BridgeParts.Instance.Parts(e)));

                            var catbrg = parts.Where(e => !string.IsNullOrEmpty(e.Item2!.CATBRG));
                            var condtn = parts.Where(e => e.Item2!.CONDTN.HasValue);
                            var natcon = parts.Where(e => !string.IsNullOrEmpty(e.Item2!.NATCON));

                            if (hashFeatureType.Where(e => ids.Contains(e.Key)).Any(e => e.Value is SpanOpening)) {
                                instance.openingBridge = true;

                                if (catbrg.Any()) {
                                    var values = catbrg.SelectMany(e => e.Item2!.CATBRG!.Split(',', StringSplitOptions.RemoveEmptyEntries));

                                    string[] categoryOfOpeningBridge = ["3", "4", "5", "7"];
                                    if (values.Any(e => categoryOfOpeningBridge.Contains(e))) {
                                        var c = values.Where(e => categoryOfOpeningBridge.Contains(e)).Distinct();
                                        if (c.Count() != 1) {
                                            Logger.Current.Error($"Bridge having multiple categories #{c.Count()}");
                                            instance.categoryOfOpeningBridge = null;
                                        }
                                        else {
                                            instance.categoryOfOpeningBridge = c.First() switch {
                                                "3" => 3,  //  swing bridge
                                                "4" => 4,   //  lifting bridge
                                                "5" => 5,  //  bascule bridge
                                                "7" => 7,    //  drawbridge
                                                "-32767" => null,
                                                _ => throw new NotImplementedException(),
                                            };
                                        }
                                    }
                                }
                                else
                                    instance.categoryOfOpeningBridge = null;    //  UNKNOWN
                            }
                            else {
                                instance.openingBridge = false;
                            }

                            if (catbrg.Any()) {
                                var values = catbrg.SelectMany(e => e.Item2!.CATBRG!.Split(',', StringSplitOptions.RemoveEmptyEntries));

                                string[] construction = ["10", "6", "12", "8"];
                                if (values.Any(e => construction.Contains(e))) {
                                    var c = values.Where(e => construction.Contains(e)).Distinct();
                                    if (c.Count() != 1) System.Diagnostics.Debugger.Break();
                                    instance.bridgeConstruction = c.First() switch {
                                        "10" => 2,  //  viaduct
                                        "6" => 3,   //  pontoon bridge
                                        "12" => 4,  //  suspension bridge
                                        "8" => 5,   //  transporter bridge
                                        "-32767" => null,
                                        _ => throw new NotImplementedException(),
                                    };
                                }
                            }

                            if (natcon.Any()) {
                                var values = natcon.SelectMany(e => e.Item2!.NATCON!.Split(',', StringSplitOptions.RemoveEmptyEntries));

                                string[] natureOfConstruction = ["1", "2", "6", "7"];
                                if (values.Any(e => natureOfConstruction.Contains(e))) {
                                    var distinct = values.Where(e => natureOfConstruction.Contains(e)).Distinct().ToArray();
                                    if (distinct.Any()) {
                                        int?[] _natureOfConstruction = [];
                                        for (int i = 0; i < distinct.Count(); i++) {
                                            _natureOfConstruction = [.._natureOfConstruction, distinct[i] switch {
                                                "1" => 1,
                                                "2" => 2,
                                                "6" => 6,
                                                "7" => 7,
                                                "-32767" => null,
                                                _ => throw new NotImplementedException(),
                                            }];
                                        }
                                        instance.natureOfConstruction = _natureOfConstruction;
                                    }
                                }
                            }

                            if (condtn.Any()) {
                                var values = condtn.Select(e => e.Item2!.CONDTN!.Value);

                                List<int> condition = [1, 2, 5];
                                if (values.Any(e => condition.Contains(e))) {
                                    var c = values.Where(e => condition.Contains(e)).Distinct();
                                    if (c.Count() != 1) System.Diagnostics.Debugger.Break();
                                    instance.condition = c.First() switch {
                                        1 => 1, //  under construction
                                        2 => 2, //  ruined
                                        5 => 5, //  planned construction                                     
                                        -32767 => null,
                                        _ => throw new NotImplementedException(),
                                    };
                                }
                            }

                            var pylons = parts.Where(e => e.Item2!.FcSubtype == 45);
                            if (pylons.Any()) {
                                var height = pylons.Where(e => e.Item2!.HEIGHT.HasValue);
                                if (height.Any()) {
                                    var values = height.Select(e => e.Item2!.HEIGHT!.Value);

                                    instance.height = values.Max() switch {
                                        -32767 => null,
                                        _ => values.Max(),
                                    };
                                    System.Diagnostics.Debugger.Break();
                                }
                            }

                            var colour = parts.Where(e => !string.IsNullOrEmpty(e.Item2!.COLOUR));
                            if (colour.Any()) {
                                System.Diagnostics.Debugger.Break();
                            }

                            var date = parts.Where(e => !string.IsNullOrEmpty(e.Item2!.DATEND) || !string.IsNullOrEmpty(e.Item2.DATSTA));
                            if (date.Any()) {
                                var datsta = date.Where(e => !string.IsNullOrEmpty(e.Item2!.DATSTA));
                                var datend = date.Where(e => !string.IsNullOrEmpty(e.Item2!.DATEND));

                                var fixedDateRange = new fixedDateRange();
                                if (datsta.Any()) {
                                    var values = datsta.Select(e => e.Item2!.DATSTA!).Distinct();
                                    if (values.Count() > 1) System.Diagnostics.Debugger.Break();
                                    fixedDateRange.dateStart = values.First() switch {
                                        "-32767" => null,
                                        _ => values.First(),
                                    };
                                }
                                if (datend.Any()) {
                                    var values = datend.Select(e => e.Item2!.DATEND!).Distinct();
                                    if (values.Count() > 1) System.Diagnostics.Debugger.Break();
                                    fixedDateRange.dateEnd = values.First() switch {
                                        "-32767" => null,
                                        _ => values.First(),
                                    };
                                }
                                instance.fixedDateRange = fixedDateRange;
                            }

                            featureName[] featureNames = [];
                            foreach (var p in parts) {
                                var names = GetFeatureName(p.Item2!.OBJNAM, p.Item2!.NOBJNM);
                                if (names is not null) {
                                    foreach (var e in names) {
                                        if (featureNames.Any(f => f.name!.Equals(e.name, StringComparison.InvariantCultureIgnoreCase) && f.language!.Equals(e.language, StringComparison.InvariantCultureIgnoreCase)))
                                            continue;
                                        featureNames = [.. featureNames, e];
                                    }
                                }
                            }

                            if (featureNames.Any())
                                instance.featureName = featureNames;

                            var scale = parts.Select(e => e.Item2!.PLTS_COMP_SCALE!.Value).Distinct();
                            if (scale.Count() > 1) System.Diagnostics.Debugger.Break();

                            if (true == instance.openingBridge) {
                                if (!instance.attributeBindings.Any(e => e.S100FC_code.Equals("categoryOfOpeningBridge")))
                                    instance.categoryOfOpeningBridge = null;
                            }

                            bufferBridge["code"] = instance.GetType().Name;
                            bufferBridge["attributebindings"] = instance.Flatten();
                            bufferBridge["featureBindings"] = "[]";
                            bufferBridge["informationbindings"] = "[]";
                            //bufferBridge["specificusage"] = usage;
                            bufferBridge["compilationscale"] = optimumDisplayScaleConverter(scale.First()) ?? 10000000;
                            bufferBridge["sourceIdentifier"] = instance.sourceIdentifier;

                            SetShape(bufferBridge, polygon);

                            var featureN = surface.CreateRow(bufferBridge);
                            var name = featureN.UID();

                            string[] aggregation = [nameof(SpanFixed), nameof(SpanOpening), nameof(Pontoon), nameof(PylonBridgeSupport)];

                            foreach (var uid in ids) {
                                var guid = Guid.Parse($"{uid.Split("::")[^1]}");
                                using var cursor = surface.CreateUpdateCursor(new QueryFilter {
                                    WhereClause = $"GlobalID = '{guid:B}'"
                                }, false);
                                cursor.MoveNext();

                                using var current = cursor.Current;

                                if (!aggregation.Contains(Convert.ToString(current["code"]))) continue;

                                var json = current.IsNull("featureBindings") ? "[{}]" : Convert.ToString(current["featureBindings"])!;
                                var featureBindings = System.Text.Json.JsonSerializer.Deserialize<featureBinding[]>(json);
                                if (featureBindings is null)
                                    featureBindings = [];

                                featureBindings = [.. featureBindings, new featureBinding {
                                     roleType = "aggregation",
                                     role = "theCollection",
                                     association = new association{
                                         S100FC_code = nameof(BridgeAggregation),
                                     },
                                     featureType = nameof(Bridge),
                                     featureId = name,
                                     Primitive = Primitives.surface,
                                }];

                                current["featureBindings"] = System.Text.Json.JsonSerializer.Serialize(featureBindings);
                                current.Store();
                            }
                        }
                    }, destination);
                }
            }

            //  Navigational System of Marks
            using (var destination = createTargetGeodatabase()) {
                Logger.Current.Information($"Fixing Navigational System of Marks");

                Store((d) => {
                    using var surface = destination.OpenDataset<FeatureClass>(destination.GetName("surface"));

                    var whereClause = $"ps = '{ps101}' AND code IN ('NavigationalSystemOfMarks')";

                    int[] compilationscales = [];
                    {
                        using var cursor = surface.Search(new QueryFilter {
                            PrefixClause = "Distinct",
                            WhereClause = whereClause,
                            SubFields = "compilationscale",
                            PostfixClause = "ORDER BY compilationscale DESC",
                        }, true);

                        while (cursor.MoveNext()) {
                            var _ = Convert.ToInt32(cursor.Current["compilationscale"]);
                            compilationscales = [.. compilationscales, _];
                        }
                    }

                    foreach (var compilationscale in compilationscales.OrderDescending()) {
                        (string UID, int? marksNavigationalSystemOf, Polygon shape)[] navigationalSystemOfMarks = [];

                        using (var cursor = surface.Search(new QueryFilter {
                            WhereClause = whereClause + $" AND compilationscale = {compilationscale}",
                        }, true)) {
                            while (cursor.MoveNext()) {
                                var current = (Feature)cursor.Current;

                                var uid = current.UID();
                                var json = Convert.ToString(current["attributeBindings"]);
                                if (string.IsNullOrEmpty(json)) json = "{}";

                                var navigationalSystemOfMark = AttributeFlattenExtensions.Unflatten<NavigationalSystemOfMarks>(json, typeof(NavigationalSystemOfMarks));

                                navigationalSystemOfMarks = [.. navigationalSystemOfMarks, (uid, navigationalSystemOfMark.marksNavigationalSystemOf, (Polygon)current.GetShape().Clone())];
                            }
                        }

                        using (var cursor = surface.CreateUpdateCursor(new QueryFilter {
                            WhereClause = $"ps = '{ps101}' AND code IN ('LocalDirectionOfBuoyage') AND compilationscale = {compilationscale}",
                        }, false)) {
                            while (cursor.MoveNext()) {
                                var current = (Feature)cursor.Current;

                                var uid = current.UID();
                                var json = Convert.ToString(current["attributeBindings"]);
                                if (string.IsNullOrEmpty(json)) json = "{}";

                                var localDirectionOfBuoyage = AttributeFlattenExtensions.Unflatten<LocalDirectionOfBuoyage>(json, typeof(LocalDirectionOfBuoyage));

                                foreach (var e in navigationalSystemOfMarks) {


                                    if (GeometryEngine.Instance.Disjoint(current.GetShape(), e.shape)) continue;

                                    if (GeometryEngine.Instance.Within(current.GetShape(), e.shape)) {
                                        if (e.marksNavigationalSystemOf.HasValue) {
                                            if (e.marksNavigationalSystemOf != localDirectionOfBuoyage.marksNavigationalSystemOf) {
                                                localDirectionOfBuoyage.marksNavigationalSystemOf = e.marksNavigationalSystemOf;

                                                current["attributeBindings"] = localDirectionOfBuoyage.Flatten();
                                                current.Store();
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }, destination);
            }


            using (Geodatabase source = createGeodatabase()) {
                using (var destination = createTargetGeodatabase()) {
                    Store((d) => {
                        Logger.Current.Debug($"Converting Sounding Datums");
                        ImporterNIS.QueryFilter.WhereClause = $"PLTS_COMP_SCALE >= {maximumDisplayScale} AND PLTS_COMP_SCALE < {minimumDisplayScale}";
                        S101_SoundingDatum(source, destination, QueryFilter, s101ProductCoverages);
                    }, destination);
                }
            }

            //using (Geodatabase source = createGeodatabase()) {
            //    using (var destination = createTargetGeodatabase()) {
            //        Store((d) => {
            //            ImporterNIS.QueryFilter.WhereClause = $"PLTS_COMP_SCALE >= {maximumDisplayScale} AND PLTS_COMP_SCALE < {minimumDisplayScale}";
            //            Bridges.Initialize(source, d, ImporterNIS.QueryFilter);
            //            Bridges.Instance.CreateRelations();
            //        }, destination);
            //    }
            //}

#if CREATE_SUBTYPES
            Logger.Current.Verbose("Creating subtypes...");
            using (var destination = createTargetGeodatabase()) {
                SchemaBuilder schemaBuilder = new SchemaBuilder(destination);

                string[] featureClasses = ["point", "pointset", "curve", "surface"];
                foreach (var featureClassName in featureClasses) {
                    var primitive = featureClassName switch {
                        "point" => S100FC.Primitives.point,
                        "pointset" => S100FC.Primitives.pointSet,
                        "curve" => S100FC.Primitives.curve,
                        "surface" => S100FC.Primitives.surface,
                        _ => throw new NotImplementedException()
                    };

                    var features = Summary.PrimitiveFeatures(primitive);

                    FeatureClassDefinition fcDefinition = destination.GetDefinition<FeatureClassDefinition>(featureClassName);

                    FeatureClassDescription fcDescription = new FeatureClassDescription(fcDefinition);

                    var definitionReferences = new Dictionary<int, string> { { 0, "UNKNOWN" } };
                    fcDescription.SubtypeFieldDescription = new SubtypeFieldDescription("sourceIdentifier", definitionReferences);
                    schemaBuilder.Modify(fcDescription);
                    schemaBuilder.Build();

                    foreach (var e in Summary.definitionReferenceFeatureTypes.Where(e => features.Contains(e.code)).OrderBy(e => e.code)) {
                        definitionReferences.Add(e.sourceIdentifier, e.code);
                    }
                    fcDescription.SubtypeFieldDescription = new SubtypeFieldDescription("sourceIdentifier", definitionReferences);
                    schemaBuilder.Modify(fcDescription);
                    schemaBuilder.Build();
                }
                {
                    var features = Summary.PrimitiveFeatures(Primitives.noGeometry);

                    var tableDefinition = destination.GetDefinition<TableDefinition>("featuretype");

                    var tableDescription = new TableDescription(tableDefinition);

                    var definitionReferences = new Dictionary<int, string> { { 0, "UNKNOWN" } };
                    tableDescription.SubtypeFieldDescription = new SubtypeFieldDescription("sourceIdentifier", definitionReferences);
                    schemaBuilder.Modify(tableDescription);
                    schemaBuilder.Build();

                    foreach (var e in Summary.definitionReferenceFeatureTypes.Where(e => features.Contains(e.code)).OrderBy(e => e.code)) {
                        definitionReferences.Add(e.sourceIdentifier, e.code);
                    }
                    tableDescription.SubtypeFieldDescription = new SubtypeFieldDescription("sourceIdentifier", definitionReferences);
                    schemaBuilder.Modify(tableDescription);
                    schemaBuilder.Build();
                }
                {
                    var tableDefinition = destination.GetDefinition<TableDefinition>("informationtype");

                    var tableDescription = new TableDescription(tableDefinition);

                    var definitionReferences = new Dictionary<int, string> { { 0, "UNKNOWN" } };
                    schemaBuilder.Modify(tableDescription);
                    schemaBuilder.Build();

                    foreach (var e in Summary.definitionReferenceInformationTypes.OrderBy(e => e.sourceIdentifier)) {
                        definitionReferences.Add(e.sourceIdentifier, e.code);
                    }
                    tableDescription.SubtypeFieldDescription = new SubtypeFieldDescription("sourceIdentifier", definitionReferences);
                    schemaBuilder.Modify(tableDescription);
                    schemaBuilder.Build();
                }
            }
#endif

            Logger.Current.Warning("Drawing-index and edition checks are not implemented; these checks have been skipped.");
            Logger.Current.Verbose("Validating...");
            using (var destination = createTargetGeodatabase()) {
                Logger.Current.Information($"Loading sanity checker");
                SanityChecker.Initialize(destination);

                Logger.Current.Information($"Validating ESRI Uknown values");
                status = SanityChecker.Instance.Check_GetEsriUnknown32767ErrorCount() == 0 ? "PASSED" : "FAILED";
                validationPassed &= status == "PASSED";
                Logger.Current.Information($"No ESRI unknown values (-32767) in S-101: {status}");

                Logger.Current.Information($"Validating default clearance");
                status = SanityChecker.Instance.Check_GetDefaultClearanceViolationCount() == 0 ? "PASSED" : "FAILED";
                validationPassed &= status == "PASSED";
                Logger.Current.Information($"No defaultClearanceViolation in S-101: {status}");

                Logger.Current.Information("Done");

                Logger.Current.Information($"!: CHECK LOGS AT: {Logger.LogDir}");
            }
            return validationPassed;
        }

    }
}
