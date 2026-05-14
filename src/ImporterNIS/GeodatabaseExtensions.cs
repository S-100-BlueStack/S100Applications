using ArcGIS.Core.Data;

namespace S100Framework.Applications
{
    internal static class GeodatabaseExtensions
    {


        internal static string? GetName(this Geodatabase geodatabase, string name) {
            var _layerDefinitions = geodatabase.GetDefinitions<FeatureClassDefinition>();
            var _tableDefinitions = geodatabase.GetDefinitions<TableDefinition>();

            var tableName = _layerDefinitions?.FirstOrDefault<FeatureClassDefinition>(e => e.GetAliasName().Equals(name.ToLower(), StringComparison.InvariantCultureIgnoreCase))?.GetName();
            if (tableName == null) {
                tableName = _tableDefinitions?.FirstOrDefault<TableDefinition>(e => e.GetAliasName().Equals(name.ToLower(), StringComparison.InvariantCultureIgnoreCase))?.GetName();
            }
            return tableName;
        }
        internal static bool IsFeatureClass(this Geodatabase geodatabase, string name) {
            var _layerDefinitions = geodatabase.GetDefinitions<FeatureClassDefinition>();
            var _tableDefinitions = geodatabase.GetDefinitions<TableDefinition>();

            var tableName = _layerDefinitions?.FirstOrDefault<FeatureClassDefinition>(e => e.GetAliasName().Equals(name.ToLower(), StringComparison.InvariantCultureIgnoreCase) || e.GetName().ToLower().Equals(name.ToLower(), StringComparison.InvariantCultureIgnoreCase))?.GetName();
            if (tableName == null) {
                tableName = _tableDefinitions?.FirstOrDefault<TableDefinition>(e => e.GetAliasName().Equals(name.ToLower(), StringComparison.InvariantCultureIgnoreCase) || e.GetName().ToLower().Equals(name.ToLower(), StringComparison.InvariantCultureIgnoreCase))?.GetName();
                return false;
            }
            return true;
        }

        internal static SortedDictionary<int, string> GetSubtypes(this FeatureClass featureClass) {
            var subtypes = featureClass.GetDefinition().GetSubtypes();
            var sortedDict = new SortedDictionary<int, string>();
            foreach (var subtype in subtypes) {
                sortedDict.Add(subtype.GetCode(), subtype.GetName());
            }
            return sortedDict;
        }


        internal static bool IsTraditionallyVersioned(this Geodatabase geodatabase) {
            if (geodatabase.IsVersioningSupported()) {
                if (geodatabase.GetType().Name == "BranchVersionedWorkspace") {
                    return false;
                }

                return true;
            }
            return false;
        }

    }
}

namespace ArcGIS.Core.Data
{
    public static class DataExtensions
    {
        public static string Prefix(string tableName) => tableName.ToLower().Split('.')[^1] switch {
            "featuretype" => "F100",
            "curve" => "F101",
            "point" => "F102",
            "pointset" => "F103",
            "surface" => "F104",
            "topo_curve" => "F105",
            "topo_point" => "F106",            
            "topo_pointset" => "F107",                      
            "topo_surface" => "F108",
            
            "informationtype" => "I109",
            "attachment" => "I110",
            _ => throw new NotImplementedException(),
        };

        //public static string Crc32(this Feature feature) => $"{System.IO.Hashing.Crc32.HashToUInt32(feature.GetGlobalID().ToByteArray())}";
        public static string UID(this Feature feature) => $"{Prefix(feature.GetTable().GetName())}{feature.GetObjectID():00000000}";   // Convert.ToString(feature["UID"])!;

        //public static string Crc32(this Row row) => $"{System.IO.Hashing.Crc32.HashToUInt32(row.GetGlobalID().ToByteArray())}";
        public static string UID(this Row row) => $"{Prefix(row.GetTable().GetName())}{row.GetObjectID():00000000}";   // Convert.ToString(row["UID"])!;
    }
}