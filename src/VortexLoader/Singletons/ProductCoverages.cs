using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using S100FC;
using S100FC.S101.FeatureAssociation;
using S100FC.S101.FeatureTypes;
using System.Diagnostics;


namespace S100Framework.Applications.Singletons
{
    internal class ProductCoverages
    {
        private static ProductCoverages? _instance;
        private static readonly object _lock = new object();

        private static Geodatabase? _source;

        

        private readonly List<ProductRecord> _products = new();

        internal static void Initialize(Geodatabase source, QueryFilter whereClause) {
            if (_instance != null)
                throw new InvalidOperationException("Products has already been initialized.");

            lock (_lock) {
                if (_instance == null)
                    _instance = new ProductCoverages(source, whereClause);
            }
        }

        private ProductCoverages(Geodatabase source, QueryFilter whereClause) {
            _source = source ?? throw new ArgumentNullException(nameof(source));

            var datacoverageName = "productcoverage";

            using var datacoverage = _source.OpenDataset<FeatureClass>(_source.GetName(datacoverageName));
            {
                LoadProducts(datacoverage);
            }
        }

        /// <summary>
        /// Loads polygons and attributes from a feature class
        /// </summary>
        private void LoadProducts(FeatureClass featureClass) {
            using RowCursor cursor = featureClass.Search();

            while (cursor.MoveNext()) {
                using Feature feature = (Feature)cursor.Current;

                var geometry = feature.GetShape();

                if (geometry.GeometryType != GeometryType.Polygon)
                    continue;
                
                Guid.TryParse(Convert.ToString(feature["GLOBALID"]), out var globalid);

                var record = new ProductRecord {
                    Geometry = geometry,
                    GlobalId = globalid,
                    LongName = Convert.ToString(feature["LNAM"]) ?? "",
                    PltsCompScale = Convert.ToInt32(feature["PLTS_COMP_SCALE"]),
                    CScale = Convert.ToInt32(feature["CSCALE"]),
                    Dnsm = Convert.ToString(feature["DSNM"]) ??  ""
                };

                _products.Add(record);
            }
        }


        public static ProductCoverages Instance {
            get {
                if (_instance == null) {
                    throw new NullReferenceException("ProductCoverages is not initialized. Call initialize with a source geodatabase and a filter");
                }
                return _instance;
            }
        }

        /// <summary>
        /// Returns products whose polygons touch/intersect/contain the geometry.
        /// </summary>
        public IEnumerable<ProductRecord> Touch(Geometry geometry) {
            if (geometry == null)
                throw new ArgumentNullException(nameof(geometry));

            return _products.Where(p =>
                GeometryEngine.Instance.Touches(p.Geometry, geometry) ||
                GeometryEngine.Instance.Intersects(p.Geometry, geometry) ||
                GeometryEngine.Instance.Contains(p.Geometry, geometry));
        }
    }

    internal class ProductRecord
    {
        public Geometry Geometry { get; set; } = default!;

        public Guid GlobalId { get; set; }

        public string LongName { get; set; } = string.Empty;

        public int PltsCompScale { get; set; }
        public string Dnsm { get; set; }
        public int CScale { get; set; }

    }
}



