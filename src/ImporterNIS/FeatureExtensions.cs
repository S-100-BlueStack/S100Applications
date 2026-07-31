namespace S100Framework.Applications.S57.esri
{
    using ArcGIS.Core.Data;
    using ArcGIS.Core.Geometry;

    /// <summary>
    /// Strongly typed accessors for the S-57 attributes exposed by an ArcGIS <see cref="Feature"/>.
    /// Every accessor returns <c>null</c> when the field is absent, <see cref="DBNull"/> or holds the S-57 unknown value (-32767).
    /// Each accessor is paired with a <c>_HasValue</c> probe reporting whether the field carries a value at all.
    /// </summary>
    internal static class FeatureExtensions
    {
        /// <summary>
        /// ATTF Lexical Level
        /// </summary>
        /// <param name="feature">The feature to read the <c>AALL</c> field from.</param>
        /// <returns>The value of <c>AALL</c>, or <c>null</c> when it is not set.</returns>
        public static int? AALL(this Feature feature) {
            if (DBNull.Value == feature["AALL"]) return null;
            var v = Convert.ToInt32(feature["AALL"]);
            //if (-32767 == v) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>AALL</c> (ATTF Lexical Level) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>AALL</c> is neither <see cref="DBNull"/> nor <c>null</c>; otherwise <c>false</c>.</returns>
        public static bool AALL_HasValue(this Feature feature) => !(DBNull.Value == feature["AALL"]) && feature["AALL"] is not null;

        /// <summary>
        /// Agency
        /// </summary>
        /// <param name="feature">The feature to read the <c>AGEN</c> field from.</param>
        /// <returns>The value of <c>AGEN</c>, or <c>null</c> when it is not set.</returns>
        public static string? AGEN(this Feature feature) {
            //if (DBNull.Value == feature["AGEN"]) return null;
            var v = Convert.ToString(feature["AGEN"]);
            //if ("-32767".Equals(v)) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>AGEN</c> (Agency) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>AGEN</c> is neither <see cref="DBNull"/> nor <c>null</c> nor an empty string; otherwise <c>false</c>.</returns>
        public static bool AGEN_HasValue(this Feature feature) => !(DBNull.Value == feature["AGEN"]) && feature["AGEN"] is not null && !string.IsNullOrEmpty(Convert.ToString(feature["AGEN"]));

        /// <summary>
        /// Beacon shape
        /// </summary>
        /// <param name="feature">The feature to read the <c>BCNSHP</c> field from.</param>
        /// <returns>The value of <c>BCNSHP</c>, or <c>null</c> when it is not set.</returns>
        public static int? BCNSHP(this Feature feature) {
            if (DBNull.Value == feature["BCNSHP"]) return null;
            var v = Convert.ToInt32(feature["BCNSHP"]);
            //if (-32767 == v) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>BCNSHP</c> (Beacon shape) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>BCNSHP</c> is neither <see cref="DBNull"/> nor <c>null</c>; otherwise <c>false</c>.</returns>
        public static bool BCNSHP_HasValue(this Feature feature) => !(DBNull.Value == feature["BCNSHP"]) && feature["BCNSHP"] is not null;

        /// <summary>
        /// Buoy shape
        /// </summary>
        /// <param name="feature">The feature to read the <c>BOYSHP</c> field from.</param>
        /// <returns>The value of <c>BOYSHP</c>, or <c>null</c> when it is not set.</returns>
        public static int? BOYSHP(this Feature feature) {
            if (DBNull.Value == feature["BOYSHP"]) return null;
            var v = Convert.ToInt32(feature["BOYSHP"]);
            //if (-32767 == v) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>BOYSHP</c> (Buoy shape) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>BOYSHP</c> is neither <see cref="DBNull"/> nor <c>null</c>; otherwise <c>false</c>.</returns>
        public static bool BOYSHP_HasValue(this Feature feature) => !(DBNull.Value == feature["BOYSHP"]) && feature["BOYSHP"] is not null;

        /// <summary>
        /// Building shape
        /// </summary>
        /// <param name="feature">The feature to read the <c>BUISHP</c> field from.</param>
        /// <returns>The value of <c>BUISHP</c>, or <c>null</c> when it is not set.</returns>
        public static int? BUISHP(this Feature feature) {
            if (DBNull.Value == feature["BUISHP"]) return null;
            var v = Convert.ToInt32(feature["BUISHP"]);
            //if (-32767 == v) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>BUISHP</c> (Building shape) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>BUISHP</c> is neither <see cref="DBNull"/> nor <c>null</c>; otherwise <c>false</c>.</returns>
        public static bool BUISHP_HasValue(this Feature feature) => !(DBNull.Value == feature["BUISHP"]) && feature["BUISHP"] is not null;

        /// <summary>
        /// Buried depth
        /// </summary>
        /// <param name="feature">The feature to read the <c>BURDEP</c> field from.</param>
        /// <returns>The value of <c>BURDEP</c>, or <c>null</c> when it is not set.</returns>
        public static decimal? BURDEP(this Feature feature) {
            if (DBNull.Value == feature["BURDEP"]) return null;
            var v = Convert.ToDecimal(feature["BURDEP"]);
            //if (-32767m == v) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>BURDEP</c> (Buried depth) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>BURDEP</c> is neither <see cref="DBNull"/> nor <c>null</c>; otherwise <c>false</c>.</returns>
        public static bool BURDEP_HasValue(this Feature feature) => !(DBNull.Value == feature["BURDEP"]) && feature["BURDEP"] is not null;

        /// <summary>
        /// Call sign
        /// </summary>
        /// <param name="feature">The feature to read the <c>CALSGN</c> field from.</param>
        /// <returns>The value of <c>CALSGN</c>, or <c>null</c> when it is not set.</returns>
        public static string? CALSGN(this Feature feature) {
            //if (DBNull.Value == feature["CALSGN"]) return null;
            var v = Convert.ToString(feature["CALSGN"]);
            //if ("-32767".Equals(v)) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>CALSGN</c> (Call sign) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>CALSGN</c> is neither <see cref="DBNull"/> nor <c>null</c> nor an empty string; otherwise <c>false</c>.</returns>
        public static bool CALSGN_HasValue(this Feature feature) => !(DBNull.Value == feature["CALSGN"]) && feature["CALSGN"] is not null && !string.IsNullOrEmpty(Convert.ToString(feature["CALSGN"]));

        /// <summary>
        /// Category of anchorage
        /// </summary>
        /// <param name="feature">The feature to read the <c>CATACH</c> field from.</param>
        /// <returns>The value of <c>CATACH</c>, or <c>null</c> when it is not set.</returns>
        public static string? CATACH(this Feature feature) {
            //if (DBNull.Value == feature["CATACH"]) return null;
            var v = Convert.ToString(feature["CATACH"]);
            //if ("-32767".Equals(v)) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>CATACH</c> (Category of anchorage) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>CATACH</c> is neither <see cref="DBNull"/> nor <c>null</c> nor an empty string; otherwise <c>false</c>.</returns>
        public static bool CATACH_HasValue(this Feature feature) => !(DBNull.Value == feature["CATACH"]) && feature["CATACH"] is not null && !string.IsNullOrEmpty(Convert.ToString(feature["CATACH"]));

        /// <summary>
        /// Category of airport/airfield
        /// </summary>
        /// <param name="feature">The feature to read the <c>CATAIR</c> field from.</param>
        /// <returns>The value of <c>CATAIR</c>, or <c>null</c> when it is not set.</returns>
        public static string? CATAIR(this Feature feature) {
            //if (DBNull.Value == feature["CATAIR"]) return null;
            var v = Convert.ToString(feature["CATAIR"]);
            //if ("-32767".Equals(v)) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>CATAIR</c> (Category of airport/airfield) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>CATAIR</c> is neither <see cref="DBNull"/> nor <c>null</c> nor an empty string; otherwise <c>false</c>.</returns>
        public static bool CATAIR_HasValue(this Feature feature) => !(DBNull.Value == feature["CATAIR"]) && feature["CATAIR"] is not null && !string.IsNullOrEmpty(Convert.ToString(feature["CATAIR"]));

        /// <summary>
        /// Category of bridge
        /// </summary>
        /// <param name="feature">The feature to read the <c>CATBRG</c> field from.</param>
        /// <returns>The value of <c>CATBRG</c>, or <c>null</c> when it is not set.</returns>
        public static string? CATBRG(this Feature feature) {
            //if (DBNull.Value == feature["CATBRG"]) return null;
            var v = Convert.ToString(feature["CATBRG"]);
            //if ("-32767".Equals(v)) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>CATBRG</c> (Category of bridge) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>CATBRG</c> is neither <see cref="DBNull"/> nor <c>null</c> nor an empty string; otherwise <c>false</c>.</returns>
        public static bool CATBRG_HasValue(this Feature feature) => !(DBNull.Value == feature["CATBRG"]) && feature["CATBRG"] is not null && !string.IsNullOrEmpty(Convert.ToString(feature["CATBRG"]));

        /// <summary>
        /// Category of built-up area
        /// </summary>
        /// <param name="feature">The feature to read the <c>CATBUA</c> field from.</param>
        /// <returns>The value of <c>CATBUA</c>, or <c>null</c> when it is not set.</returns>
        public static int? CATBUA(this Feature feature) {
            if (DBNull.Value == feature["CATBUA"]) return null;
            var v = Convert.ToInt32(feature["CATBUA"]);
            //if (-32767 == v) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>CATBUA</c> (Category of built-up area) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>CATBUA</c> is neither <see cref="DBNull"/> nor <c>null</c>; otherwise <c>false</c>.</returns>
        public static bool CATBUA_HasValue(this Feature feature) => !(DBNull.Value == feature["CATBUA"]) && feature["CATBUA"] is not null;

        /// <summary>
        /// Category of cardinal mark
        /// </summary>
        /// <param name="feature">The feature to read the <c>CATCAM</c> field from.</param>
        /// <returns>The value of <c>CATCAM</c>, or <c>null</c> when it is not set.</returns>
        public static int? CATCAM(this Feature feature) {
            if (DBNull.Value == feature["CATCAM"]) return null;
            var v = Convert.ToInt32(feature["CATCAM"]);
            //if (-32767 == v) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>CATCAM</c> (Category of cardinal mark) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>CATCAM</c> is neither <see cref="DBNull"/> nor <c>null</c>; otherwise <c>false</c>.</returns>
        public static bool CATCAM_HasValue(this Feature feature) => !(DBNull.Value == feature["CATCAM"]) && feature["CATCAM"] is not null;

        /// <summary>
        /// Category of canal
        /// </summary>
        /// <param name="feature">The feature to read the <c>CATCAN</c> field from.</param>
        /// <returns>The value of <c>CATCAN</c>, or <c>null</c> when it is not set.</returns>
        public static int? CATCAN(this Feature feature) {
            if (DBNull.Value == feature["CATCAN"]) return null;
            var v = Convert.ToInt32(feature["CATCAN"]);
            //if (-32767 == v) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>CATCAN</c> (Category of canal) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>CATCAN</c> is neither <see cref="DBNull"/> nor <c>null</c>; otherwise <c>false</c>.</returns>
        public static bool CATCAN_HasValue(this Feature feature) => !(DBNull.Value == feature["CATCAN"]) && feature["CATCAN"] is not null;

        /// <summary>
        /// Category of cable
        /// </summary>
        /// <param name="feature">The feature to read the <c>CATCBL</c> field from.</param>
        /// <returns>The value of <c>CATCBL</c>, or <c>null</c> when it is not set.</returns>
        public static int? CATCBL(this Feature feature) {
            if (DBNull.Value == feature["CATCBL"]) return null;
            var v = Convert.ToInt32(feature["CATCBL"]);
            //if (-32767 == v) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>CATCBL</c> (Category of cable) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>CATCBL</c> is neither <see cref="DBNull"/> nor <c>null</c>; otherwise <c>false</c>.</returns>
        public static bool CATCBL_HasValue(this Feature feature) => !(DBNull.Value == feature["CATCBL"]) && feature["CATCBL"] is not null;

        /// <summary>
        /// Category of checkpoint
        /// </summary>
        /// <param name="feature">The feature to read the <c>CATCHP</c> field from.</param>
        /// <returns>The value of <c>CATCHP</c>, or <c>null</c> when it is not set.</returns>
        public static int? CATCHP(this Feature feature) {
            if (DBNull.Value == feature["CATCHP"]) return null;
            var v = Convert.ToInt32(feature["CATCHP"]);
            //if (-32767 == v) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>CATCHP</c> (Category of checkpoint) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>CATCHP</c> is neither <see cref="DBNull"/> nor <c>null</c>; otherwise <c>false</c>.</returns>
        public static bool CATCHP_HasValue(this Feature feature) => !(DBNull.Value == feature["CATCHP"]) && feature["CATCHP"] is not null;

        /// <summary>
        /// Category of coastline
        /// </summary>
        /// <param name="feature">The feature to read the <c>CATCOA</c> field from.</param>
        /// <returns>The value of <c>CATCOA</c>, or <c>null</c> when it is not set.</returns>
        public static int? CATCOA(this Feature feature) {
            if (DBNull.Value == feature["CATCOA"]) return null;
            var v = Convert.ToInt32(feature["CATCOA"]);
            //if (-32767 == v) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>CATCOA</c> (Category of coastline) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>CATCOA</c> is neither <see cref="DBNull"/> nor <c>null</c>; otherwise <c>false</c>.</returns>
        public static bool CATCOA_HasValue(this Feature feature) => !(DBNull.Value == feature["CATCOA"]) && feature["CATCOA"] is not null;

        /// <summary>
        /// Category of conveyor
        /// </summary>
        /// <param name="feature">The feature to read the <c>CATCON</c> field from.</param>
        /// <returns>The value of <c>CATCON</c>, or <c>null</c> when it is not set.</returns>
        public static int? CATCON(this Feature feature) {
            if (DBNull.Value == feature["CATCON"]) return null;
            var v = Convert.ToInt32(feature["CATCON"]);
            //if (-32767 == v) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>CATCON</c> (Category of conveyor) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>CATCON</c> is neither <see cref="DBNull"/> nor <c>null</c>; otherwise <c>false</c>.</returns>
        public static bool CATCON_HasValue(this Feature feature) => !(DBNull.Value == feature["CATCON"]) && feature["CATCON"] is not null;

        /// <summary>
        /// Category of coverage
        /// </summary>
        /// <param name="feature">The feature to read the <c>CATCOV</c> field from.</param>
        /// <returns>The value of <c>CATCOV</c>, or <c>null</c> when it is not set.</returns>
        public static int? CATCOV(this Feature feature) {
            if (DBNull.Value == feature["CATCOV"]) return null;
            var v = Convert.ToInt32(feature["CATCOV"]);
            //if (-32767 == v) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>CATCOV</c> (Category of coverage) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>CATCOV</c> is neither <see cref="DBNull"/> nor <c>null</c>; otherwise <c>false</c>.</returns>
        public static bool CATCOV_HasValue(this Feature feature) => !(DBNull.Value == feature["CATCOV"]) && feature["CATCOV"] is not null;

        /// <summary>
        /// Category of crane
        /// </summary>
        /// <param name="feature">The feature to read the <c>CATCRN</c> field from.</param>
        /// <returns>The value of <c>CATCRN</c>, or <c>null</c> when it is not set.</returns>
        public static int? CATCRN(this Feature feature) {
            if (DBNull.Value == feature["CATCRN"]) return null;
            var v = Convert.ToInt32(feature["CATCRN"]);
            //if (-32767 == v) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>CATCRN</c> (Category of crane) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>CATCRN</c> is neither <see cref="DBNull"/> nor <c>null</c>; otherwise <c>false</c>.</returns>
        public static bool CATCRN_HasValue(this Feature feature) => !(DBNull.Value == feature["CATCRN"]) && feature["CATCRN"] is not null;

        /// <summary>
        /// Category of control point
        /// </summary>
        /// <param name="feature">The feature to read the <c>CATCTR</c> field from.</param>
        /// <returns>The value of <c>CATCTR</c>, or <c>null</c> when it is not set.</returns>
        public static int? CATCTR(this Feature feature) {
            if (DBNull.Value == feature["CATCTR"]) return null;
            var v = Convert.ToInt32(feature["CATCTR"]);
            //if (-32767 == v) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>CATCTR</c> (Category of control point) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>CATCTR</c> is neither <see cref="DBNull"/> nor <c>null</c>; otherwise <c>false</c>.</returns>
        public static bool CATCTR_HasValue(this Feature feature) => !(DBNull.Value == feature["CATCTR"]) && feature["CATCTR"] is not null;

        /// <summary>
        /// Category of dam
        /// </summary>
        /// <param name="feature">The feature to read the <c>CATDAM</c> field from.</param>
        /// <returns>The value of <c>CATDAM</c>, or <c>null</c> when it is not set.</returns>
        public static int? CATDAM(this Feature feature) {
            if (DBNull.Value == feature["CATDAM"]) return null;
            var v = Convert.ToInt32(feature["CATDAM"]);
            //if (-32767 == v) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>CATDAM</c> (Category of dam) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>CATDAM</c> is neither <see cref="DBNull"/> nor <c>null</c>; otherwise <c>false</c>.</returns>
        public static bool CATDAM_HasValue(this Feature feature) => !(DBNull.Value == feature["CATDAM"]) && feature["CATDAM"] is not null;

        /// <summary>
        /// Category of distance mark
        /// </summary>
        /// <param name="feature">The feature to read the <c>CATDIS</c> field from.</param>
        /// <returns>The value of <c>CATDIS</c>, or <c>null</c> when it is not set.</returns>
        public static int? CATDIS(this Feature feature) {
            if (DBNull.Value == feature["CATDIS"]) return null;
            var v = Convert.ToInt32(feature["CATDIS"]);
            //if (-32767 == v) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>CATDIS</c> (Category of distance mark) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>CATDIS</c> is neither <see cref="DBNull"/> nor <c>null</c>; otherwise <c>false</c>.</returns>
        public static bool CATDIS_HasValue(this Feature feature) => !(DBNull.Value == feature["CATDIS"]) && feature["CATDIS"] is not null;

        /// <summary>
        /// Category of dock
        /// </summary>
        /// <param name="feature">The feature to read the <c>CATDOC</c> field from.</param>
        /// <returns>The value of <c>CATDOC</c>, or <c>null</c> when it is not set.</returns>
        public static int? CATDOC(this Feature feature) {
            if (DBNull.Value == feature["CATDOC"]) return null;
            var v = Convert.ToInt32(feature["CATDOC"]);
            //if (-32767 == v) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>CATDOC</c> (Category of dock) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>CATDOC</c> is neither <see cref="DBNull"/> nor <c>null</c>; otherwise <c>false</c>.</returns>
        public static bool CATDOC_HasValue(this Feature feature) => !(DBNull.Value == feature["CATDOC"]) && feature["CATDOC"] is not null;

        /// <summary>
        /// Category of dumping ground
        /// </summary>
        /// <param name="feature">The feature to read the <c>CATDPG</c> field from.</param>
        /// <returns>The value of <c>CATDPG</c>, or <c>null</c> when it is not set.</returns>
        public static string? CATDPG(this Feature feature) {
            //if (DBNull.Value == feature["CATDPG"]) return null;
            var v = Convert.ToString(feature["CATDPG"]);
            //if ("-32767".Equals(v)) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>CATDPG</c> (Category of dumping ground) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>CATDPG</c> is neither <see cref="DBNull"/> nor <c>null</c> nor an empty string; otherwise <c>false</c>.</returns>
        public static bool CATDPG_HasValue(this Feature feature) => !(DBNull.Value == feature["CATDPG"]) && feature["CATDPG"] is not null && !string.IsNullOrEmpty(Convert.ToString(feature["CATDPG"]));

        /// <summary>
        /// Category of fishing facility
        /// </summary>
        /// <param name="feature">The feature to read the <c>CATFIF</c> field from.</param>
        /// <returns>The value of <c>CATFIF</c>, or <c>null</c> when it is not set.</returns>
        public static int? CATFIF(this Feature feature) {
            if (DBNull.Value == feature["CATFIF"]) return null;
            var v = Convert.ToInt32(feature["CATFIF"]);
            //if (-32767 == v) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>CATFIF</c> (Category of fishing facility) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>CATFIF</c> is neither <see cref="DBNull"/> nor <c>null</c>; otherwise <c>false</c>.</returns>
        public static bool CATFIF_HasValue(this Feature feature) => !(DBNull.Value == feature["CATFIF"]) && feature["CATFIF"] is not null;

        /// <summary>
        /// Category of fence/wall
        /// </summary>
        /// <param name="feature">The feature to read the <c>CATFNC</c> field from.</param>
        /// <returns>The value of <c>CATFNC</c>, or <c>null</c> when it is not set.</returns>
        public static int? CATFNC(this Feature feature) {
            if (DBNull.Value == feature["CATFNC"]) return null;
            var v = Convert.ToInt32(feature["CATFNC"]);
            //if (-32767 == v) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>CATFNC</c> (Category of fence/wall) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>CATFNC</c> is neither <see cref="DBNull"/> nor <c>null</c>; otherwise <c>false</c>.</returns>
        public static bool CATFNC_HasValue(this Feature feature) => !(DBNull.Value == feature["CATFNC"]) && feature["CATFNC"] is not null;

        /// <summary>
        /// Category of fog signal
        /// </summary>
        /// <param name="feature">The feature to read the <c>CATFOG</c> field from.</param>
        /// <returns>The value of <c>CATFOG</c>, or <c>null</c> when it is not set.</returns>
        public static int? CATFOG(this Feature feature) {
            if (DBNull.Value == feature["CATFOG"]) return null;
            var v = Convert.ToInt32(feature["CATFOG"]);
            //if (-32767 == v) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>CATFOG</c> (Category of fog signal) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>CATFOG</c> is neither <see cref="DBNull"/> nor <c>null</c>; otherwise <c>false</c>.</returns>
        public static bool CATFOG_HasValue(this Feature feature) => !(DBNull.Value == feature["CATFOG"]) && feature["CATFOG"] is not null;

        /// <summary>
        /// Category of fortified structure
        /// </summary>
        /// <param name="feature">The feature to read the <c>CATFOR</c> field from.</param>
        /// <returns>The value of <c>CATFOR</c>, or <c>null</c> when it is not set.</returns>
        public static int? CATFOR(this Feature feature) {
            if (DBNull.Value == feature["CATFOR"]) return null;
            var v = Convert.ToInt32(feature["CATFOR"]);
            //if (-32767 == v) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>CATFOR</c> (Category of fortified structure) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>CATFOR</c> is neither <see cref="DBNull"/> nor <c>null</c>; otherwise <c>false</c>.</returns>
        public static bool CATFOR_HasValue(this Feature feature) => !(DBNull.Value == feature["CATFOR"]) && feature["CATFOR"] is not null;

        /// <summary>
        /// Category of ferry
        /// </summary>
        /// <param name="feature">The feature to read the <c>CATFRY</c> field from.</param>
        /// <returns>The value of <c>CATFRY</c>, or <c>null</c> when it is not set.</returns>
        public static int? CATFRY(this Feature feature) {
            if (DBNull.Value == feature["CATFRY"]) return null;
            var v = Convert.ToInt32(feature["CATFRY"]);
            //if (-32767 == v) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>CATFRY</c> (Category of ferry) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>CATFRY</c> is neither <see cref="DBNull"/> nor <c>null</c>; otherwise <c>false</c>.</returns>
        public static bool CATFRY_HasValue(this Feature feature) => !(DBNull.Value == feature["CATFRY"]) && feature["CATFRY"] is not null;

        /// <summary>
        /// Category of gate
        /// </summary>
        /// <param name="feature">The feature to read the <c>CATGAT</c> field from.</param>
        /// <returns>The value of <c>CATGAT</c>, or <c>null</c> when it is not set.</returns>
        public static int? CATGAT(this Feature feature) {
            if (DBNull.Value == feature["CATGAT"]) return null;
            var v = Convert.ToInt32(feature["CATGAT"]);
            //if (-32767 == v) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>CATGAT</c> (Category of gate) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>CATGAT</c> is neither <see cref="DBNull"/> nor <c>null</c>; otherwise <c>false</c>.</returns>
        public static bool CATGAT_HasValue(this Feature feature) => !(DBNull.Value == feature["CATGAT"]) && feature["CATGAT"] is not null;

        /// <summary>
        /// Category of harbour facility
        /// </summary>
        /// <param name="feature">The feature to read the <c>CATHAF</c> field from.</param>
        /// <returns>The value of <c>CATHAF</c>, or <c>null</c> when it is not set.</returns>
        public static string? CATHAF(this Feature feature) {
            //if (DBNull.Value == feature["CATHAF"]) return null;
            var v = Convert.ToString(feature["CATHAF"]);
            //if ("-32767".Equals(v)) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>CATHAF</c> (Category of harbour facility) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>CATHAF</c> is neither <see cref="DBNull"/> nor <c>null</c> nor an empty string; otherwise <c>false</c>.</returns>
        public static bool CATHAF_HasValue(this Feature feature) => !(DBNull.Value == feature["CATHAF"]) && feature["CATHAF"] is not null && !string.IsNullOrEmpty(Convert.ToString(feature["CATHAF"]));

        /// <summary>
        /// Category of hulk
        /// </summary>
        /// <param name="feature">The feature to read the <c>CATHLK</c> field from.</param>
        /// <returns>The value of <c>CATHLK</c>, or <c>null</c> when it is not set.</returns>
        public static string? CATHLK(this Feature feature) {
            //if (DBNull.Value == feature["CATHLK"]) return null;
            var v = Convert.ToString(feature["CATHLK"]);
            //if ("-32767".Equals(v)) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>CATHLK</c> (Category of hulk) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>CATHLK</c> is neither <see cref="DBNull"/> nor <c>null</c> nor an empty string; otherwise <c>false</c>.</returns>
        public static bool CATHLK_HasValue(this Feature feature) => !(DBNull.Value == feature["CATHLK"]) && feature["CATHLK"] is not null && !string.IsNullOrEmpty(Convert.ToString(feature["CATHLK"]));

        /// <summary>
        /// Category of ice
        /// </summary>
        /// <param name="feature">The feature to read the <c>CATICE</c> field from.</param>
        /// <returns>The value of <c>CATICE</c>, or <c>null</c> when it is not set.</returns>
        public static int? CATICE(this Feature feature) {
            if (DBNull.Value == feature["CATICE"]) return null;
            var v = Convert.ToInt32(feature["CATICE"]);
            //if (-32767 == v) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>CATICE</c> (Category of ice) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>CATICE</c> is neither <see cref="DBNull"/> nor <c>null</c>; otherwise <c>false</c>.</returns>
        public static bool CATICE_HasValue(this Feature feature) => !(DBNull.Value == feature["CATICE"]) && feature["CATICE"] is not null;

        /// <summary>
        /// Category of installation buoy
        /// </summary>
        /// <param name="feature">The feature to read the <c>CATINB</c> field from.</param>
        /// <returns>The value of <c>CATINB</c>, or <c>null</c> when it is not set.</returns>
        public static int? CATINB(this Feature feature) {
            if (DBNull.Value == feature["CATINB"]) return null;
            var v = Convert.ToInt32(feature["CATINB"]);
            //if (-32767 == v) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>CATINB</c> (Category of installation buoy) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>CATINB</c> is neither <see cref="DBNull"/> nor <c>null</c>; otherwise <c>false</c>.</returns>
        public static bool CATINB_HasValue(this Feature feature) => !(DBNull.Value == feature["CATINB"]) && feature["CATINB"] is not null;

        /// <summary>
        /// Category of lateral mark
        /// </summary>
        /// <param name="feature">The feature to read the <c>CATLAM</c> field from.</param>
        /// <returns>The value of <c>CATLAM</c>, or <c>null</c> when it is not set.</returns>
        public static int? CATLAM(this Feature feature) {
            if (DBNull.Value == feature["CATLAM"]) return null;
            var v = Convert.ToInt32(feature["CATLAM"]);
            //if (-32767 == v) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>CATLAM</c> (Category of lateral mark) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>CATLAM</c> is neither <see cref="DBNull"/> nor <c>null</c>; otherwise <c>false</c>.</returns>
        public static bool CATLAM_HasValue(this Feature feature) => !(DBNull.Value == feature["CATLAM"]) && feature["CATLAM"] is not null;

        /// <summary>
        /// Category of light
        /// </summary>
        /// <param name="feature">The feature to read the <c>CATLIT</c> field from.</param>
        /// <returns>The value of <c>CATLIT</c>, or <c>null</c> when it is not set.</returns>
        public static string? CATLIT(this Feature feature) {
            //if (DBNull.Value == feature["CATLIT"]) return null;
            var v = Convert.ToString(feature["CATLIT"]);
            //if ("-32767".Equals(v)) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>CATLIT</c> (Category of light) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>CATLIT</c> is neither <see cref="DBNull"/> nor <c>null</c> nor an empty string; otherwise <c>false</c>.</returns>
        public static bool CATLIT_HasValue(this Feature feature) => !(DBNull.Value == feature["CATLIT"]) && feature["CATLIT"] is not null && !string.IsNullOrEmpty(Convert.ToString(feature["CATLIT"]));

        /// <summary>
        /// Category of landmark
        /// </summary>
        /// <param name="feature">The feature to read the <c>CATLMK</c> field from.</param>
        /// <returns>The value of <c>CATLMK</c>, or <c>null</c> when it is not set.</returns>
        public static string? CATLMK(this Feature feature) {
            //if (DBNull.Value == feature["CATLMK"]) return null;
            var v = Convert.ToString(feature["CATLMK"]);
            //if ("-32767".Equals(v)) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>CATLMK</c> (Category of landmark) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>CATLMK</c> is neither <see cref="DBNull"/> nor <c>null</c> nor an empty string; otherwise <c>false</c>.</returns>
        public static bool CATLMK_HasValue(this Feature feature) => !(DBNull.Value == feature["CATLMK"]) && feature["CATLMK"] is not null && !string.IsNullOrEmpty(Convert.ToString(feature["CATLMK"]));

        /// <summary>
        /// Category of land region
        /// </summary>
        /// <param name="feature">The feature to read the <c>CATLND</c> field from.</param>
        /// <returns>The value of <c>CATLND</c>, or <c>null</c> when it is not set.</returns>
        public static string? CATLND(this Feature feature) {
            //if (DBNull.Value == feature["CATLND"]) return null;
            var v = Convert.ToString(feature["CATLND"]);
            //if ("-32767".Equals(v)) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>CATLND</c> (Category of land region) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>CATLND</c> is neither <see cref="DBNull"/> nor <c>null</c> nor an empty string; otherwise <c>false</c>.</returns>
        public static bool CATLND_HasValue(this Feature feature) => !(DBNull.Value == feature["CATLND"]) && feature["CATLND"] is not null && !string.IsNullOrEmpty(Convert.ToString(feature["CATLND"]));

        /// <summary>
        /// Category marine farm/culture
        /// </summary>
        /// <param name="feature">The feature to read the <c>CATMFA</c> field from.</param>
        /// <returns>The value of <c>CATMFA</c>, or <c>null</c> when it is not set.</returns>
        public static int? CATMFA(this Feature feature) {
            if (DBNull.Value == feature["CATMFA"]) return null;
            var v = Convert.ToInt32(feature["CATMFA"]);
            //if (-32767 == v) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>CATMFA</c> (Category marine farm/culture) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>CATMFA</c> is neither <see cref="DBNull"/> nor <c>null</c>; otherwise <c>false</c>.</returns>
        public static bool CATMFA_HasValue(this Feature feature) => !(DBNull.Value == feature["CATMFA"]) && feature["CATMFA"] is not null;

        /// <summary>
        /// Category of mooring/warping facility
        /// </summary>
        /// <param name="feature">The feature to read the <c>CATMOR</c> field from.</param>
        /// <returns>The value of <c>CATMOR</c>, or <c>null</c> when it is not set.</returns>
        public static int? CATMOR(this Feature feature) {
            if (DBNull.Value == feature["CATMOR"]) return null;
            var v = Convert.ToInt32(feature["CATMOR"]);
            //if (-32767 == v) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>CATMOR</c> (Category of mooring/warping facility) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>CATMOR</c> is neither <see cref="DBNull"/> nor <c>null</c>; otherwise <c>false</c>.</returns>
        public static bool CATMOR_HasValue(this Feature feature) => !(DBNull.Value == feature["CATMOR"]) && feature["CATMOR"] is not null;

        /// <summary>
        /// Category of military practice area
        /// </summary>
        /// <param name="feature">The feature to read the <c>CATMPA</c> field from.</param>
        /// <returns>The value of <c>CATMPA</c>, or <c>null</c> when it is not set.</returns>
        public static string? CATMPA(this Feature feature) {
            //if (DBNull.Value == feature["CATMPA"]) return null;
            var v = Convert.ToString(feature["CATMPA"]);
            //if ("-32767".Equals(v)) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>CATMPA</c> (Category of military practice area) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>CATMPA</c> is neither <see cref="DBNull"/> nor <c>null</c> nor an empty string; otherwise <c>false</c>.</returns>
        public static bool CATMPA_HasValue(this Feature feature) => !(DBNull.Value == feature["CATMPA"]) && feature["CATMPA"] is not null && !string.IsNullOrEmpty(Convert.ToString(feature["CATMPA"]));

        /// <summary>
        /// Category of navigation line
        /// </summary>
        /// <param name="feature">The feature to read the <c>CATNAV</c> field from.</param>
        /// <returns>The value of <c>CATNAV</c>, or <c>null</c> when it is not set.</returns>
        public static int? CATNAV(this Feature feature) {
            if (DBNull.Value == feature["CATNAV"]) return null;
            var v = Convert.ToInt32(feature["CATNAV"]);
            //if (-32767 == v) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>CATNAV</c> (Category of navigation line) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>CATNAV</c> is neither <see cref="DBNull"/> nor <c>null</c>; otherwise <c>false</c>.</returns>
        public static bool CATNAV_HasValue(this Feature feature) => !(DBNull.Value == feature["CATNAV"]) && feature["CATNAV"] is not null;

        /// <summary>
        /// Category of obstruction
        /// </summary>
        /// <param name="feature">The feature to read the <c>CATOBS</c> field from.</param>
        /// <returns>The value of <c>CATOBS</c>, or <c>null</c> when it is not set.</returns>
        public static int? CATOBS(this Feature feature) {
            if (DBNull.Value == feature["CATOBS"]) return null;
            var v = Convert.ToInt32(feature["CATOBS"]);
            //if (-32767 == v) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>CATOBS</c> (Category of obstruction) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>CATOBS</c> is neither <see cref="DBNull"/> nor <c>null</c>; otherwise <c>false</c>.</returns>
        public static bool CATOBS_HasValue(this Feature feature) => !(DBNull.Value == feature["CATOBS"]) && feature["CATOBS"] is not null;

        /// <summary>
        /// Category of offshore platform
        /// </summary>
        /// <param name="feature">The feature to read the <c>CATOFP</c> field from.</param>
        /// <returns>The value of <c>CATOFP</c>, or <c>null</c> when it is not set.</returns>
        public static string? CATOFP(this Feature feature) {
            //if (DBNull.Value == feature["CATOFP"]) return null;
            var v = Convert.ToString(feature["CATOFP"]);
            //if ("-32767".Equals(v)) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>CATOFP</c> (Category of offshore platform) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>CATOFP</c> is neither <see cref="DBNull"/> nor <c>null</c> nor an empty string; otherwise <c>false</c>.</returns>
        public static bool CATOFP_HasValue(this Feature feature) => !(DBNull.Value == feature["CATOFP"]) && feature["CATOFP"] is not null && !string.IsNullOrEmpty(Convert.ToString(feature["CATOFP"]));

        /// <summary>
        /// Category of oil barrier
        /// </summary>
        /// <param name="feature">The feature to read the <c>CATOLB</c> field from.</param>
        /// <returns>The value of <c>CATOLB</c>, or <c>null</c> when it is not set.</returns>
        public static int? CATOLB(this Feature feature) {
            if (DBNull.Value == feature["CATOLB"]) return null;
            var v = Convert.ToInt32(feature["CATOLB"]);
            //if (-32767 == v) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>CATOLB</c> (Category of oil barrier) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>CATOLB</c> is neither <see cref="DBNull"/> nor <c>null</c>; otherwise <c>false</c>.</returns>
        public static bool CATOLB_HasValue(this Feature feature) => !(DBNull.Value == feature["CATOLB"]) && feature["CATOLB"] is not null;

        /// <summary>
        /// Category of pilot boarding place
        /// </summary>
        /// <param name="feature">The feature to read the <c>CATPIL</c> field from.</param>
        /// <returns>The value of <c>CATPIL</c>, or <c>null</c> when it is not set.</returns>
        public static int? CATPIL(this Feature feature) {
            if (DBNull.Value == feature["CATPIL"]) return null;
            var v = Convert.ToInt32(feature["CATPIL"]);
            //if (-32767 == v) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>CATPIL</c> (Category of pilot boarding place) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>CATPIL</c> is neither <see cref="DBNull"/> nor <c>null</c>; otherwise <c>false</c>.</returns>
        public static bool CATPIL_HasValue(this Feature feature) => !(DBNull.Value == feature["CATPIL"]) && feature["CATPIL"] is not null;

        /// <summary>
        /// Category of pipeline/pipe
        /// </summary>
        /// <param name="feature">The feature to read the <c>CATPIP</c> field from.</param>
        /// <returns>The value of <c>CATPIP</c>, or <c>null</c> when it is not set.</returns>
        public static string? CATPIP(this Feature feature) {
            //if (DBNull.Value == feature["CATPIP"]) return null;
            var v = Convert.ToString(feature["CATPIP"]);
            //if ("-32767".Equals(v)) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>CATPIP</c> (Category of pipeline/pipe) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>CATPIP</c> is neither <see cref="DBNull"/> nor <c>null</c> nor an empty string; otherwise <c>false</c>.</returns>
        public static bool CATPIP_HasValue(this Feature feature) => !(DBNull.Value == feature["CATPIP"]) && feature["CATPIP"] is not null && !string.IsNullOrEmpty(Convert.ToString(feature["CATPIP"]));

        /// <summary>
        /// Category of pile
        /// </summary>
        /// <param name="feature">The feature to read the <c>CATPLE</c> field from.</param>
        /// <returns>The value of <c>CATPLE</c>, or <c>null</c> when it is not set.</returns>
        public static int? CATPLE(this Feature feature) {
            if (DBNull.Value == feature["CATPLE"]) return null;
            var v = Convert.ToInt32(feature["CATPLE"]);
            //if (-32767 == v) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>CATPLE</c> (Category of pile) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>CATPLE</c> is neither <see cref="DBNull"/> nor <c>null</c>; otherwise <c>false</c>.</returns>
        public static bool CATPLE_HasValue(this Feature feature) => !(DBNull.Value == feature["CATPLE"]) && feature["CATPLE"] is not null;

        /// <summary>
        /// Category of production area
        /// </summary>
        /// <param name="feature">The feature to read the <c>CATPRA</c> field from.</param>
        /// <returns>The value of <c>CATPRA</c>, or <c>null</c> when it is not set.</returns>
        public static int? CATPRA(this Feature feature) {
            if (DBNull.Value == feature["CATPRA"]) return null;
            var v = Convert.ToInt32(feature["CATPRA"]);
            //if (-32767 == v) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>CATPRA</c> (Category of production area) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>CATPRA</c> is neither <see cref="DBNull"/> nor <c>null</c>; otherwise <c>false</c>.</returns>
        public static bool CATPRA_HasValue(this Feature feature) => !(DBNull.Value == feature["CATPRA"]) && feature["CATPRA"] is not null;

        /// <summary>
        /// Category of pylon
        /// </summary>
        /// <param name="feature">The feature to read the <c>CATPYL</c> field from.</param>
        /// <returns>The value of <c>CATPYL</c>, or <c>null</c> when it is not set.</returns>
        public static int? CATPYL(this Feature feature) {
            if (DBNull.Value == feature["CATPYL"]) return null;
            var v = Convert.ToInt32(feature["CATPYL"]);
            //if (-32767 == v) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>CATPYL</c> (Category of pylon) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>CATPYL</c> is neither <see cref="DBNull"/> nor <c>null</c>; otherwise <c>false</c>.</returns>
        public static bool CATPYL_HasValue(this Feature feature) => !(DBNull.Value == feature["CATPYL"]) && feature["CATPYL"] is not null;

        /// <summary>
        /// Category of quality of data
        /// </summary>
        /// <param name="feature">The feature to read the <c>CATQUA</c> field from.</param>
        /// <returns>The value of <c>CATQUA</c>, or <c>null</c> when it is not set.</returns>
        public static int? CATQUA(this Feature feature) {
            if (DBNull.Value == feature["CATQUA"]) return null;
            var v = Convert.ToInt32(feature["CATQUA"]);
            //if (-32767 == v) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>CATQUA</c> (Category of quality of data) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>CATQUA</c> is neither <see cref="DBNull"/> nor <c>null</c>; otherwise <c>false</c>.</returns>
        public static bool CATQUA_HasValue(this Feature feature) => !(DBNull.Value == feature["CATQUA"]) && feature["CATQUA"] is not null;

        /// <summary>
        /// Category of radar station
        /// </summary>
        /// <param name="feature">The feature to read the <c>CATRAS</c> field from.</param>
        /// <returns>The value of <c>CATRAS</c>, or <c>null</c> when it is not set.</returns>
        public static int? CATRAS(this Feature feature) {
            if (DBNull.Value == feature["CATRAS"]) return null;
            var v = Convert.ToInt32(feature["CATRAS"]);
            //if (-32767 == v) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>CATRAS</c> (Category of radar station) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>CATRAS</c> is neither <see cref="DBNull"/> nor <c>null</c>; otherwise <c>false</c>.</returns>
        public static bool CATRAS_HasValue(this Feature feature) => !(DBNull.Value == feature["CATRAS"]) && feature["CATRAS"] is not null;

        /// <summary>
        /// Category of restricted area
        /// </summary>
        /// <param name="feature">The feature to read the <c>CATREA</c> field from.</param>
        /// <returns>The value of <c>CATREA</c>, or <c>null</c> when it is not set.</returns>
        public static string? CATREA(this Feature feature) {
            //if (DBNull.Value == feature["CATREA"]) return null;
            var v = Convert.ToString(feature["CATREA"]);
            //if ("-32767".Equals(v)) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>CATREA</c> (Category of restricted area) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>CATREA</c> is neither <see cref="DBNull"/> nor <c>null</c> nor an empty string; otherwise <c>false</c>.</returns>
        public static bool CATREA_HasValue(this Feature feature) => !(DBNull.Value == feature["CATREA"]) && feature["CATREA"] is not null && !string.IsNullOrEmpty(Convert.ToString(feature["CATREA"]));

        /// <summary>
        /// Category of road
        /// </summary>
        /// <param name="feature">The feature to read the <c>CATROD</c> field from.</param>
        /// <returns>The value of <c>CATROD</c>, or <c>null</c> when it is not set.</returns>
        public static int? CATROD(this Feature feature) {
            if (DBNull.Value == feature["CATROD"]) return null;
            var v = Convert.ToInt32(feature["CATROD"]);
            //if (-32767 == v) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>CATROD</c> (Category of road) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>CATROD</c> is neither <see cref="DBNull"/> nor <c>null</c>; otherwise <c>false</c>.</returns>
        public static bool CATROD_HasValue(this Feature feature) => !(DBNull.Value == feature["CATROD"]) && feature["CATROD"] is not null;

        /// <summary>
        /// Category of radio station
        /// </summary>
        /// <param name="feature">The feature to read the <c>CATROS</c> field from.</param>
        /// <returns>The value of <c>CATROS</c>, or <c>null</c> when it is not set.</returns>
        public static string? CATROS(this Feature feature) {
            //if (DBNull.Value == feature["CATROS"]) return null;
            var v = Convert.ToString(feature["CATROS"]);
            //if ("-32767".Equals(v)) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>CATROS</c> (Category of radio station) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>CATROS</c> is neither <see cref="DBNull"/> nor <c>null</c> nor an empty string; otherwise <c>false</c>.</returns>
        public static bool CATROS_HasValue(this Feature feature) => !(DBNull.Value == feature["CATROS"]) && feature["CATROS"] is not null && !string.IsNullOrEmpty(Convert.ToString(feature["CATROS"]));

        /// <summary>
        /// Category of rescue station
        /// </summary>
        /// <param name="feature">The feature to read the <c>CATRSC</c> field from.</param>
        /// <returns>The value of <c>CATRSC</c>, or <c>null</c> when it is not set.</returns>
        public static string? CATRSC(this Feature feature) {
            //if (DBNull.Value == feature["CATRSC"]) return null;
            var v = Convert.ToString(feature["CATRSC"]);
            //if ("-32767".Equals(v)) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>CATRSC</c> (Category of rescue station) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>CATRSC</c> is neither <see cref="DBNull"/> nor <c>null</c> nor an empty string; otherwise <c>false</c>.</returns>
        public static bool CATRSC_HasValue(this Feature feature) => !(DBNull.Value == feature["CATRSC"]) && feature["CATRSC"] is not null && !string.IsNullOrEmpty(Convert.ToString(feature["CATRSC"]));

        /// <summary>
        /// Category of radar transponder beacon
        /// </summary>
        /// <param name="feature">The feature to read the <c>CATRTB</c> field from.</param>
        /// <returns>The value of <c>CATRTB</c>, or <c>null</c> when it is not set.</returns>
        public static int? CATRTB(this Feature feature) {
            if (DBNull.Value == feature["CATRTB"]) return null;
            var v = Convert.ToInt32(feature["CATRTB"]);
            //if (-32767 == v) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>CATRTB</c> (Category of radar transponder beacon) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>CATRTB</c> is neither <see cref="DBNull"/> nor <c>null</c>; otherwise <c>false</c>.</returns>
        public static bool CATRTB_HasValue(this Feature feature) => !(DBNull.Value == feature["CATRTB"]) && feature["CATRTB"] is not null;

        /// <summary>
        /// Category of runway
        /// </summary>
        /// <param name="feature">The feature to read the <c>CATRUN</c> field from.</param>
        /// <returns>The value of <c>CATRUN</c>, or <c>null</c> when it is not set.</returns>
        public static int? CATRUN(this Feature feature) {
            if (DBNull.Value == feature["CATRUN"]) return null;
            var v = Convert.ToInt32(feature["CATRUN"]);
            //if (-32767 == v) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>CATRUN</c> (Category of runway) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>CATRUN</c> is neither <see cref="DBNull"/> nor <c>null</c>; otherwise <c>false</c>.</returns>
        public static bool CATRUN_HasValue(this Feature feature) => !(DBNull.Value == feature["CATRUN"]) && feature["CATRUN"] is not null;

        /// <summary>
        /// Category of small craft facility
        /// </summary>
        /// <param name="feature">The feature to read the <c>CATSCF</c> field from.</param>
        /// <returns>The value of <c>CATSCF</c>, or <c>null</c> when it is not set.</returns>
        public static string? CATSCF(this Feature feature) {
            //if (DBNull.Value == feature["CATSCF"]) return null;
            var v = Convert.ToString(feature["CATSCF"]);
            //if ("-32767".Equals(v)) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>CATSCF</c> (Category of small craft facility) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>CATSCF</c> is neither <see cref="DBNull"/> nor <c>null</c> nor an empty string; otherwise <c>false</c>.</returns>
        public static bool CATSCF_HasValue(this Feature feature) => !(DBNull.Value == feature["CATSCF"]) && feature["CATSCF"] is not null && !string.IsNullOrEmpty(Convert.ToString(feature["CATSCF"]));

        /// <summary>
        /// Category of sea area
        /// </summary>
        /// <param name="feature">The feature to read the <c>CATSEA</c> field from.</param>
        /// <returns>The value of <c>CATSEA</c>, or <c>null</c> when it is not set.</returns>
        public static int? CATSEA(this Feature feature) {
            if (DBNull.Value == feature["CATSEA"]) return null;
            var v = Convert.ToInt32(feature["CATSEA"]);
            //if (-32767 == v) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>CATSEA</c> (Category of sea area) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>CATSEA</c> is neither <see cref="DBNull"/> nor <c>null</c>; otherwise <c>false</c>.</returns>
        public static bool CATSEA_HasValue(this Feature feature) => !(DBNull.Value == feature["CATSEA"]) && feature["CATSEA"] is not null;

        /// <summary>
        /// Category of silo/tank
        /// </summary>
        /// <param name="feature">The feature to read the <c>CATSIL</c> field from.</param>
        /// <returns>The value of <c>CATSIL</c>, or <c>null</c> when it is not set.</returns>
        public static int? CATSIL(this Feature feature) {
            if (DBNull.Value == feature["CATSIL"]) return null;
            var v = Convert.ToInt32(feature["CATSIL"]);
            //if (-32767 == v) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>CATSIL</c> (Category of silo/tank) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>CATSIL</c> is neither <see cref="DBNull"/> nor <c>null</c>; otherwise <c>false</c>.</returns>
        public static bool CATSIL_HasValue(this Feature feature) => !(DBNull.Value == feature["CATSIL"]) && feature["CATSIL"] is not null;

        /// <summary>
        /// Category of signal station, traffic
        /// </summary>
        /// <param name="feature">The feature to read the <c>CATSIT</c> field from.</param>
        /// <returns>The value of <c>CATSIT</c>, or <c>null</c> when it is not set.</returns>
        public static string? CATSIT(this Feature feature) {
            //if (DBNull.Value == feature["CATSIT"]) return null;
            var v = Convert.ToString(feature["CATSIT"]);
            //if ("-32767".Equals(v)) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>CATSIT</c> (Category of signal station, traffic) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>CATSIT</c> is neither <see cref="DBNull"/> nor <c>null</c> nor an empty string; otherwise <c>false</c>.</returns>
        public static bool CATSIT_HasValue(this Feature feature) => !(DBNull.Value == feature["CATSIT"]) && feature["CATSIT"] is not null && !string.IsNullOrEmpty(Convert.ToString(feature["CATSIT"]));

        /// <summary>
        /// Category of signal station, warning
        /// </summary>
        /// <param name="feature">The feature to read the <c>CATSIW</c> field from.</param>
        /// <returns>The value of <c>CATSIW</c>, or <c>null</c> when it is not set.</returns>
        public static string? CATSIW(this Feature feature) {
            //if (DBNull.Value == feature["CATSIW"]) return null;
            var v = Convert.ToString(feature["CATSIW"]);
            //if ("-32767".Equals(v)) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>CATSIW</c> (Category of signal station, warning) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>CATSIW</c> is neither <see cref="DBNull"/> nor <c>null</c> nor an empty string; otherwise <c>false</c>.</returns>
        public static bool CATSIW_HasValue(this Feature feature) => !(DBNull.Value == feature["CATSIW"]) && feature["CATSIW"] is not null && !string.IsNullOrEmpty(Convert.ToString(feature["CATSIW"]));

        /// <summary>
        /// Category of shoreline construction
        /// </summary>
        /// <param name="feature">The feature to read the <c>CATSLC</c> field from.</param>
        /// <returns>The value of <c>CATSLC</c>, or <c>null</c> when it is not set.</returns>
        public static int? CATSLC(this Feature feature) {
            if (DBNull.Value == feature["CATSLC"]) return null;
            var v = Convert.ToInt32(feature["CATSLC"]);
            //if (-32767 == v) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>CATSLC</c> (Category of shoreline construction) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>CATSLC</c> is neither <see cref="DBNull"/> nor <c>null</c>; otherwise <c>false</c>.</returns>
        public static bool CATSLC_HasValue(this Feature feature) => !(DBNull.Value == feature["CATSLC"]) && feature["CATSLC"] is not null;

        /// <summary>
        /// Category of slope
        /// </summary>
        /// <param name="feature">The feature to read the <c>CATSLO</c> field from.</param>
        /// <returns>The value of <c>CATSLO</c>, or <c>null</c> when it is not set.</returns>
        public static int? CATSLO(this Feature feature) {
            if (DBNull.Value == feature["CATSLO"]) return null;
            var v = Convert.ToInt32(feature["CATSLO"]);
            //if (-32767 == v) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>CATSLO</c> (Category of slope) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>CATSLO</c> is neither <see cref="DBNull"/> nor <c>null</c>; otherwise <c>false</c>.</returns>
        public static bool CATSLO_HasValue(this Feature feature) => !(DBNull.Value == feature["CATSLO"]) && feature["CATSLO"] is not null;

        /// <summary>
        /// Category of special purpose mark
        /// </summary>
        /// <param name="feature">The feature to read the <c>CATSPM</c> field from.</param>
        /// <returns>The value of <c>CATSPM</c>, or <c>null</c> when it is not set.</returns>
        public static string? CATSPM(this Feature feature) {
            //if (DBNull.Value == feature["CATSPM"]) return null;
            var v = Convert.ToString(feature["CATSPM"]);
            //if ("-32767".Equals(v)) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>CATSPM</c> (Category of special purpose mark) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>CATSPM</c> is neither <see cref="DBNull"/> nor <c>null</c> nor an empty string; otherwise <c>false</c>.</returns>
        public static bool CATSPM_HasValue(this Feature feature) => !(DBNull.Value == feature["CATSPM"]) && feature["CATSPM"] is not null && !string.IsNullOrEmpty(Convert.ToString(feature["CATSPM"]));

        /// <summary>
        /// Category of recommended track
        /// </summary>
        /// <param name="feature">The feature to read the <c>CATTRK</c> field from.</param>
        /// <returns>The value of <c>CATTRK</c>, or <c>null</c> when it is not set.</returns>
        public static int? CATTRK(this Feature feature) {
            if (DBNull.Value == feature["CATTRK"]) return null;
            var v = Convert.ToInt32(feature["CATTRK"]);
            //if (-32767 == v) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>CATTRK</c> (Category of recommended track) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>CATTRK</c> is neither <see cref="DBNull"/> nor <c>null</c>; otherwise <c>false</c>.</returns>
        public static bool CATTRK_HasValue(this Feature feature) => !(DBNull.Value == feature["CATTRK"]) && feature["CATTRK"] is not null;

        /// <summary>
        /// Category of Traffic Separation Scheme
        /// </summary>
        /// <param name="feature">The feature to read the <c>CATTSS</c> field from.</param>
        /// <returns>The value of <c>CATTSS</c>, or <c>null</c> when it is not set.</returns>
        public static int? CATTSS(this Feature feature) {
            if (DBNull.Value == feature["CATTSS"]) return null;
            var v = Convert.ToInt32(feature["CATTSS"]);
            //if (-32767 == v) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>CATTSS</c> (Category of Traffic Separation Scheme) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>CATTSS</c> is neither <see cref="DBNull"/> nor <c>null</c>; otherwise <c>false</c>.</returns>
        public static bool CATTSS_HasValue(this Feature feature) => !(DBNull.Value == feature["CATTSS"]) && feature["CATTSS"] is not null;

        /// <summary>
        /// Category of vegetation
        /// </summary>
        /// <param name="feature">The feature to read the <c>CATVEG</c> field from.</param>
        /// <returns>The value of <c>CATVEG</c>, or <c>null</c> when it is not set.</returns>
        public static string? CATVEG(this Feature feature) {
            //if (DBNull.Value == feature["CATVEG"]) return null;
            var v = Convert.ToString(feature["CATVEG"]);
            //if ("-32767".Equals(v)) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>CATVEG</c> (Category of vegetation) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>CATVEG</c> is neither <see cref="DBNull"/> nor <c>null</c> nor an empty string; otherwise <c>false</c>.</returns>
        public static bool CATVEG_HasValue(this Feature feature) => !(DBNull.Value == feature["CATVEG"]) && feature["CATVEG"] is not null && !string.IsNullOrEmpty(Convert.ToString(feature["CATVEG"]));

        /// <summary>
        /// Category of water turbulence
        /// </summary>
        /// <param name="feature">The feature to read the <c>CATWAT</c> field from.</param>
        /// <returns>The value of <c>CATWAT</c>, or <c>null</c> when it is not set.</returns>
        public static int? CATWAT(this Feature feature) {
            if (DBNull.Value == feature["CATWAT"]) return null;
            var v = Convert.ToInt32(feature["CATWAT"]);
            //if (-32767 == v) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>CATWAT</c> (Category of water turbulence) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>CATWAT</c> is neither <see cref="DBNull"/> nor <c>null</c>; otherwise <c>false</c>.</returns>
        public static bool CATWAT_HasValue(this Feature feature) => !(DBNull.Value == feature["CATWAT"]) && feature["CATWAT"] is not null;

        /// <summary>
        /// Category of weed/kelp
        /// </summary>
        /// <param name="feature">The feature to read the <c>CATWED</c> field from.</param>
        /// <returns>The value of <c>CATWED</c>, or <c>null</c> when it is not set.</returns>
        public static int? CATWED(this Feature feature) {
            if (DBNull.Value == feature["CATWED"]) return null;
            var v = Convert.ToInt32(feature["CATWED"]);
            //if (-32767 == v) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>CATWED</c> (Category of weed/kelp) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>CATWED</c> is neither <see cref="DBNull"/> nor <c>null</c>; otherwise <c>false</c>.</returns>
        public static bool CATWED_HasValue(this Feature feature) => !(DBNull.Value == feature["CATWED"]) && feature["CATWED"] is not null;

        /// <summary>
        /// Category of wreck
        /// </summary>
        /// <param name="feature">The feature to read the <c>CATWRK</c> field from.</param>
        /// <returns>The value of <c>CATWRK</c>, or <c>null</c> when it is not set.</returns>
        public static int? CATWRK(this Feature feature) {
            if (DBNull.Value == feature["CATWRK"]) return null;
            var v = Convert.ToInt32(feature["CATWRK"]);
            //if (-32767 == v) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>CATWRK</c> (Category of wreck) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>CATWRK</c> is neither <see cref="DBNull"/> nor <c>null</c>; otherwise <c>false</c>.</returns>
        public static bool CATWRK_HasValue(this Feature feature) => !(DBNull.Value == feature["CATWRK"]) && feature["CATWRK"] is not null;

        /// <summary>
        /// Category of zone of confidence in data
        /// </summary>
        /// <param name="feature">The feature to read the <c>CATZOC</c> field from.</param>
        /// <returns>The value of <c>CATZOC</c>, or <c>null</c> when it is not set.</returns>
        public static int? CATZOC(this Feature feature) {
            if (DBNull.Value == feature["CATZOC"]) return null;
            var v = Convert.ToInt32(feature["CATZOC"]);
            //if (-32767 == v) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>CATZOC</c> (Category of zone of confidence in data) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>CATZOC</c> is neither <see cref="DBNull"/> nor <c>null</c>; otherwise <c>false</c>.</returns>
        public static bool CATZOC_HasValue(this Feature feature) => !(DBNull.Value == feature["CATZOC"]) && feature["CATZOC"] is not null;

        /// <summary>
        /// Category of Tidal stream
        /// </summary>
        /// <param name="feature">The feature to read the <c>CAT_TS</c> field from.</param>
        /// <returns>The value of <c>CAT_TS</c>, or <c>null</c> when it is not set.</returns>
        public static int? CAT_TS(this Feature feature) {
            if (DBNull.Value == feature["CAT_TS"]) return null;
            var v = Convert.ToInt32(feature["CAT_TS"]);
            //if (-32767 == v) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>CAT_TS</c> (Category of Tidal stream) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>CAT_TS</c> is neither <see cref="DBNull"/> nor <c>null</c>; otherwise <c>false</c>.</returns>
        public static bool CAT_TS_HasValue(this Feature feature) => !(DBNull.Value == feature["CAT_TS"]) && feature["CAT_TS"] is not null;

        /// <summary>
        /// Object Class Definition
        /// </summary>
        /// <param name="feature">The feature to read the <c>CLSDEF</c> field from.</param>
        /// <returns>The value of <c>CLSDEF</c>, or <c>null</c> when it is not set.</returns>
        public static string? CLSDEF(this Feature feature) {
            //if (DBNull.Value == feature["CLSDEF"]) return null;
            var v = Convert.ToString(feature["CLSDEF"]);
            //if ("-32767".Equals(v)) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>CLSDEF</c> (Object Class Definition) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>CLSDEF</c> is neither <see cref="DBNull"/> nor <c>null</c> nor an empty string; otherwise <c>false</c>.</returns>
        public static bool CLSDEF_HasValue(this Feature feature) => !(DBNull.Value == feature["CLSDEF"]) && feature["CLSDEF"] is not null && !string.IsNullOrEmpty(Convert.ToString(feature["CLSDEF"]));

        /// <summary>
        /// Object Class Name
        /// </summary>
        /// <param name="feature">The feature to read the <c>CLSNAM</c> field from.</param>
        /// <returns>The value of <c>CLSNAM</c>, or <c>null</c> when it is not set.</returns>
        public static string? CLSNAM(this Feature feature) {
            //if (DBNull.Value == feature["CLSNAM"]) return null;
            var v = Convert.ToString(feature["CLSNAM"]);
            //if ("-32767".Equals(v)) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>CLSNAM</c> (Object Class Name) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>CLSNAM</c> is neither <see cref="DBNull"/> nor <c>null</c> nor an empty string; otherwise <c>false</c>.</returns>
        public static bool CLSNAM_HasValue(this Feature feature) => !(DBNull.Value == feature["CLSNAM"]) && feature["CLSNAM"] is not null && !string.IsNullOrEmpty(Convert.ToString(feature["CLSNAM"]));

        /// <summary>
        /// Collection type
        /// </summary>
        /// <param name="feature">The feature to read the <c>COLLECTION_TYPE</c> field from.</param>
        /// <returns>The value of <c>COLLECTION_TYPE</c>, or <c>null</c> when it is not set.</returns>
        public static int? COLLECTION_TYPE(this Feature feature) {
            if (DBNull.Value == feature["COLLECTION_TYPE"]) return null;
            var v = Convert.ToInt32(feature["COLLECTION_TYPE"]);
            //if (-32767 == v) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>COLLECTION_TYPE</c> (Collection type) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>COLLECTION_TYPE</c> is neither <see cref="DBNull"/> nor <c>null</c>; otherwise <c>false</c>.</returns>
        public static bool COLLECTION_TYPE_HasValue(this Feature feature) => !(DBNull.Value == feature["COLLECTION_TYPE"]) && feature["COLLECTION_TYPE"] is not null;

        /// <summary>
        /// Colour
        /// </summary>
        /// <param name="feature">The feature to read the <c>COLOUR</c> field from.</param>
        /// <returns>The value of <c>COLOUR</c>, or <c>null</c> when it is not set.</returns>
        public static string? COLOUR(this Feature feature) {
            //if (DBNull.Value == feature["COLOUR"]) return null;
            var v = Convert.ToString(feature["COLOUR"]);
            //if ("-32767".Equals(v)) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>COLOUR</c> (Colour) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>COLOUR</c> is neither <see cref="DBNull"/> nor <c>null</c> nor an empty string; otherwise <c>false</c>.</returns>
        public static bool COLOUR_HasValue(this Feature feature) => !(DBNull.Value == feature["COLOUR"]) && feature["COLOUR"] is not null && !string.IsNullOrEmpty(Convert.ToString(feature["COLOUR"]));

        /// <summary>
        /// Colour pattern
        /// </summary>
        /// <param name="feature">The feature to read the <c>COLPAT</c> field from.</param>
        /// <returns>The value of <c>COLPAT</c>, or <c>null</c> when it is not set.</returns>
        public static string? COLPAT(this Feature feature) {
            //if (DBNull.Value == feature["COLPAT"]) return null;
            var v = Convert.ToString(feature["COLPAT"]);
            //if ("-32767".Equals(v)) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>COLPAT</c> (Colour pattern) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>COLPAT</c> is neither <see cref="DBNull"/> nor <c>null</c> nor an empty string; otherwise <c>false</c>.</returns>
        public static bool COLPAT_HasValue(this Feature feature) => !(DBNull.Value == feature["COLPAT"]) && feature["COLPAT"] is not null && !string.IsNullOrEmpty(Convert.ToString(feature["COLPAT"]));

        /// <summary>
        /// Communication channel
        /// </summary>
        /// <param name="feature">The feature to read the <c>COMCHA</c> field from.</param>
        /// <returns>The value of <c>COMCHA</c>, or <c>null</c> when it is not set.</returns>
        public static string? COMCHA(this Feature feature) {
            //if (DBNull.Value == feature["COMCHA"]) return null;
            var v = Convert.ToString(feature["COMCHA"]);
            //if ("-32767".Equals(v)) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>COMCHA</c> (Communication channel) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>COMCHA</c> is neither <see cref="DBNull"/> nor <c>null</c> nor an empty string; otherwise <c>false</c>.</returns>
        public static bool COMCHA_HasValue(this Feature feature) => !(DBNull.Value == feature["COMCHA"]) && feature["COMCHA"] is not null && !string.IsNullOrEmpty(Convert.ToString(feature["COMCHA"]));

        /// <summary>
        /// Coordinate Multiplication Factor
        /// </summary>
        /// <param name="feature">The feature to read the <c>COMF</c> field from.</param>
        /// <returns>The value of <c>COMF</c>, or <c>null</c> when it is not set.</returns>
        public static int? COMF(this Feature feature) {
            if (DBNull.Value == feature["COMF"]) return null;
            var v = Convert.ToInt32(feature["COMF"]);
            //if (-32767 == v) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>COMF</c> (Coordinate Multiplication Factor) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>COMF</c> is neither <see cref="DBNull"/> nor <c>null</c>; otherwise <c>false</c>.</returns>
        public static bool COMF_HasValue(this Feature feature) => !(DBNull.Value == feature["COMF"]) && feature["COMF"] is not null;

        /// <summary>
        /// Comment
        /// </summary>
        /// <param name="feature">The feature to read the <c>COMT</c> field from.</param>
        /// <returns>The value of <c>COMT</c>, or <c>null</c> when it is not set.</returns>
        public static string? COMT(this Feature feature) {
            //if (DBNull.Value == feature["COMT"]) return null;
            var v = Convert.ToString(feature["COMT"]);
            //if ("-32767".Equals(v)) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>COMT</c> (Comment) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>COMT</c> is neither <see cref="DBNull"/> nor <c>null</c> nor an empty string; otherwise <c>false</c>.</returns>
        public static bool COMT_HasValue(this Feature feature) => !(DBNull.Value == feature["COMT"]) && feature["COMT"] is not null && !string.IsNullOrEmpty(Convert.ToString(feature["COMT"]));

        /// <summary>
        /// Condition
        /// </summary>
        /// <param name="feature">The feature to read the <c>CONDTN</c> field from.</param>
        /// <returns>The value of <c>CONDTN</c>, or <c>null</c> when it is not set.</returns>
        public static int? CONDTN(this Feature feature) {
            if (DBNull.Value == feature["CONDTN"]) return null;
            var v = Convert.ToInt32(feature["CONDTN"]);
            //if (-32767 == v) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>CONDTN</c> (Condition) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>CONDTN</c> is neither <see cref="DBNull"/> nor <c>null</c>; otherwise <c>false</c>.</returns>
        public static bool CONDTN_HasValue(this Feature feature) => !(DBNull.Value == feature["CONDTN"]) && feature["CONDTN"] is not null;

        /// <summary>
        /// Conspicuous, radar
        /// </summary>
        /// <param name="feature">The feature to read the <c>CONRAD</c> field from.</param>
        /// <returns>The value of <c>CONRAD</c>, or <c>null</c> when it is not set.</returns>
        public static int? CONRAD(this Feature feature) {
            if (DBNull.Value == feature["CONRAD"]) return null;
            var v = Convert.ToInt32(feature["CONRAD"]);
            //if (-32767 == v) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>CONRAD</c> (Conspicuous, radar) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>CONRAD</c> is neither <see cref="DBNull"/> nor <c>null</c>; otherwise <c>false</c>.</returns>
        public static bool CONRAD_HasValue(this Feature feature) => !(DBNull.Value == feature["CONRAD"]) && feature["CONRAD"] is not null;

        /// <summary>
        /// Conspicuous, visually
        /// </summary>
        /// <param name="feature">The feature to read the <c>CONVIS</c> field from.</param>
        /// <returns>The value of <c>CONVIS</c>, or <c>null</c> when it is not set.</returns>
        public static int? CONVIS(this Feature feature) {
            if (DBNull.Value == feature["CONVIS"]) return null;
            var v = Convert.ToInt32(feature["CONVIS"]);
            //if (-32767 == v) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>CONVIS</c> (Conspicuous, visually) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>CONVIS</c> is neither <see cref="DBNull"/> nor <c>null</c>; otherwise <c>false</c>.</returns>
        public static bool CONVIS_HasValue(this Feature feature) => !(DBNull.Value == feature["CONVIS"]) && feature["CONVIS"] is not null;

        /// <summary>
        /// Coordinate Units
        /// </summary>
        /// <param name="feature">The feature to read the <c>COUN</c> field from.</param>
        /// <returns>The value of <c>COUN</c>, or <c>null</c> when it is not set.</returns>
        public static int? COUN(this Feature feature) {
            if (DBNull.Value == feature["COUN"]) return null;
            var v = Convert.ToInt32(feature["COUN"]);
            //if (-32767 == v) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>COUN</c> (Coordinate Units) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>COUN</c> is neither <see cref="DBNull"/> nor <c>null</c>; otherwise <c>false</c>.</returns>
        public static bool COUN_HasValue(this Feature feature) => !(DBNull.Value == feature["COUN"]) && feature["COUN"] is not null;

        /// <summary>
        /// Compilation scale of data
        /// </summary>
        /// <param name="feature">The feature to read the <c>CSCALE</c> field from.</param>
        /// <returns>The value of <c>CSCALE</c>, or <c>null</c> when it is not set.</returns>
        public static int? CSCALE(this Feature feature) {
            if (DBNull.Value == feature["CSCALE"]) return null;
            var v = Convert.ToInt32(feature["CSCALE"]);
            //if (-32767 == v) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>CSCALE</c> (Compilation scale of data) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>CSCALE</c> is neither <see cref="DBNull"/> nor <c>null</c>; otherwise <c>false</c>.</returns>
        public static bool CSCALE_HasValue(this Feature feature) => !(DBNull.Value == feature["CSCALE"]) && feature["CSCALE"] is not null;

        /// <summary>
        /// Compilation Scale
        /// </summary>
        /// <param name="feature">The feature to read the <c>CSCL</c> field from.</param>
        /// <returns>The value of <c>CSCL</c>, or <c>null</c> when it is not set.</returns>
        public static int? CSCL(this Feature feature) {
            if (DBNull.Value == feature["CSCL"]) return null;
            var v = Convert.ToInt32(feature["CSCL"]);
            //if (-32767 == v) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>CSCL</c> (Compilation Scale) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>CSCL</c> is neither <see cref="DBNull"/> nor <c>null</c>; otherwise <c>false</c>.</returns>
        public static bool CSCL_HasValue(this Feature feature) => !(DBNull.Value == feature["CSCL"]) && feature["CSCL"] is not null;

        /// <summary>
        /// Current velocity
        /// </summary>
        /// <param name="feature">The feature to read the <c>CURVEL</c> field from.</param>
        /// <returns>The value of <c>CURVEL</c>, or <c>null</c> when it is not set.</returns>
        public static decimal? CURVEL(this Feature feature) {
            if (DBNull.Value == feature["CURVEL"]) return null;
            var v = Convert.ToDecimal(feature["CURVEL"]);
            //if (-32767m == v) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>CURVEL</c> (Current velocity) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>CURVEL</c> is neither <see cref="DBNull"/> nor <c>null</c>; otherwise <c>false</c>.</returns>
        public static bool CURVEL_HasValue(this Feature feature) => !(DBNull.Value == feature["CURVEL"]) && feature["CURVEL"] is not null;

        /// <summary>
        /// Date end
        /// </summary>
        /// <param name="feature">The feature to read the <c>DATEND</c> field from.</param>
        /// <returns>The value of <c>DATEND</c>, or <c>null</c> when it is not set.</returns>
        public static string? DATEND(this Feature feature) {
            //if (DBNull.Value == feature["DATEND"]) return null;
            var v = Convert.ToString(feature["DATEND"]);
            //if ("-32767".Equals(v)) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>DATEND</c> (Date end) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>DATEND</c> is neither <see cref="DBNull"/> nor <c>null</c> nor an empty string; otherwise <c>false</c>.</returns>
        public static bool DATEND_HasValue(this Feature feature) => !(DBNull.Value == feature["DATEND"]) && feature["DATEND"] is not null && !string.IsNullOrEmpty(Convert.ToString(feature["DATEND"]));

        /// <summary>
        /// Date start
        /// </summary>
        /// <param name="feature">The feature to read the <c>DATSTA</c> field from.</param>
        /// <returns>The value of <c>DATSTA</c>, or <c>null</c> when it is not set.</returns>
        public static string? DATSTA(this Feature feature) {
            //if (DBNull.Value == feature["DATSTA"]) return null;
            var v = Convert.ToString(feature["DATSTA"]);
            //if ("-32767".Equals(v)) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>DATSTA</c> (Date start) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>DATSTA</c> is neither <see cref="DBNull"/> nor <c>null</c> nor an empty string; otherwise <c>false</c>.</returns>
        public static bool DATSTA_HasValue(this Feature feature) => !(DBNull.Value == feature["DATSTA"]) && feature["DATSTA"] is not null && !string.IsNullOrEmpty(Convert.ToString(feature["DATSTA"]));

        /// <summary>
        /// Delete comment
        /// </summary>
        /// <param name="feature">The feature to read the <c>DELETE_COMMENT</c> field from.</param>
        /// <returns>The value of <c>DELETE_COMMENT</c>, or <c>null</c> when it is not set.</returns>
        public static string? DELETE_COMMENT(this Feature feature) {
            //if (DBNull.Value == feature["DELETE_COMMENT"]) return null;
            var v = Convert.ToString(feature["DELETE_COMMENT"]);
            //if ("-32767".Equals(v)) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>DELETE_COMMENT</c> (Delete comment) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>DELETE_COMMENT</c> is neither <see cref="DBNull"/> nor <c>null</c> nor an empty string; otherwise <c>false</c>.</returns>
        public static bool DELETE_COMMENT_HasValue(this Feature feature) => !(DBNull.Value == feature["DELETE_COMMENT"]) && feature["DELETE_COMMENT"] is not null && !string.IsNullOrEmpty(Convert.ToString(feature["DELETE_COMMENT"]));

        /// <summary>
        /// Depth
        /// </summary>
        /// <param name="feature">The feature to read the <c>DEPTH</c> field from.</param>
        /// <returns>The value of <c>DEPTH</c>, or <c>null</c> when it is not set.</returns>
        public static decimal? DEPTH(this Feature feature) {
            if (DBNull.Value == feature["DEPTH"]) return null;
            var v = Convert.ToDecimal(feature["DEPTH"]);
            //if (-32767m == v) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>DEPTH</c> (Depth) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>DEPTH</c> is neither <see cref="DBNull"/> nor <c>null</c>; otherwise <c>false</c>.</returns>
        public static bool DEPTH_HasValue(this Feature feature) => !(DBNull.Value == feature["DEPTH"]) && feature["DEPTH"] is not null;

        /// <summary>
        /// Destination feature class
        /// </summary>
        /// <param name="feature">The feature to read the <c>DEST_FC</c> field from.</param>
        /// <returns>The value of <c>DEST_FC</c>, or <c>null</c> when it is not set.</returns>
        public static string? DEST_FC(this Feature feature) {
            //if (DBNull.Value == feature["DEST_FC"]) return null;
            var v = Convert.ToString(feature["DEST_FC"]);
            //if ("-32767".Equals(v)) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>DEST_FC</c> (Destination feature class) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>DEST_FC</c> is neither <see cref="DBNull"/> nor <c>null</c> nor an empty string; otherwise <c>false</c>.</returns>
        public static bool DEST_FC_HasValue(this Feature feature) => !(DBNull.Value == feature["DEST_FC"]) && feature["DEST_FC"] is not null && !string.IsNullOrEmpty(Convert.ToString(feature["DEST_FC"]));

        /// <summary>
        /// Destination long name
        /// </summary>
        /// <param name="feature">The feature to read the <c>DEST_LNAM</c> field from.</param>
        /// <returns>The value of <c>DEST_LNAM</c>, or <c>null</c> when it is not set.</returns>
        public static string? DEST_LNAM(this Feature feature) {
            //if (DBNull.Value == feature["DEST_LNAM"]) return null;
            var v = Convert.ToString(feature["DEST_LNAM"]);
            //if ("-32767".Equals(v)) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>DEST_LNAM</c> (Destination long name) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>DEST_LNAM</c> is neither <see cref="DBNull"/> nor <c>null</c> nor an empty string; otherwise <c>false</c>.</returns>
        public static bool DEST_LNAM_HasValue(this Feature feature) => !(DBNull.Value == feature["DEST_LNAM"]) && feature["DEST_LNAM"] is not null && !string.IsNullOrEmpty(Convert.ToString(feature["DEST_LNAM"]));

        /// <summary>
        /// Destination subtype
        /// </summary>
        /// <param name="feature">The feature to read the <c>DEST_SUB</c> field from.</param>
        /// <returns>The value of <c>DEST_SUB</c>, or <c>null</c> when it is not set.</returns>
        public static string? DEST_SUB(this Feature feature) {
            //if (DBNull.Value == feature["DEST_SUB"]) return null;
            var v = Convert.ToString(feature["DEST_SUB"]);
            //if ("-32767".Equals(v)) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>DEST_SUB</c> (Destination subtype) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>DEST_SUB</c> is neither <see cref="DBNull"/> nor <c>null</c> nor an empty string; otherwise <c>false</c>.</returns>
        public static bool DEST_SUB_HasValue(this Feature feature) => !(DBNull.Value == feature["DEST_SUB"]) && feature["DEST_SUB"] is not null && !string.IsNullOrEmpty(Convert.ToString(feature["DEST_SUB"]));

        /// <summary>
        /// Destination universal ID
        /// </summary>
        /// <param name="feature">The feature to read the <c>DEST_UID</c> field from.</param>
        /// <returns>The value of <c>DEST_UID</c>, or <c>null</c> when it is not set.</returns>
        public static string? DEST_UID(this Feature feature) {
            //if (DBNull.Value == feature["DEST_UID"]) return null;
            var v = Convert.ToString(feature["DEST_UID"]);
            //if ("-32767".Equals(v)) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>DEST_UID</c> (Destination universal ID) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>DEST_UID</c> is neither <see cref="DBNull"/> nor <c>null</c> nor an empty string; otherwise <c>false</c>.</returns>
        public static bool DEST_UID_HasValue(this Feature feature) => !(DBNull.Value == feature["DEST_UID"]) && feature["DEST_UID"] is not null && !string.IsNullOrEmpty(Convert.ToString(feature["DEST_UID"]));

        /// <summary>
        /// Depth range value 1
        /// </summary>
        /// <param name="feature">The feature to read the <c>DRVAL1</c> field from.</param>
        /// <returns>The value of <c>DRVAL1</c>, or <c>null</c> when it is not set.</returns>
        public static decimal? DRVAL1(this Feature feature) {
            if (DBNull.Value == feature["DRVAL1"]) return null;
            var v = Convert.ToDecimal(feature["DRVAL1"]);
            //if (-32767m == v) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>DRVAL1</c> (Depth range value 1) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>DRVAL1</c> is neither <see cref="DBNull"/> nor <c>null</c>; otherwise <c>false</c>.</returns>
        public static bool DRVAL1_HasValue(this Feature feature) => !(DBNull.Value == feature["DRVAL1"]) && feature["DRVAL1"] is not null;

        /// <summary>
        /// Depth range value 2
        /// </summary>
        /// <param name="feature">The feature to read the <c>DRVAL2</c> field from.</param>
        /// <returns>The value of <c>DRVAL2</c>, or <c>null</c> when it is not set.</returns>
        public static decimal? DRVAL2(this Feature feature) {
            if (DBNull.Value == feature["DRVAL2"]) return null;
            var v = Convert.ToDecimal(feature["DRVAL2"]);
            //if (-32767m == v) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>DRVAL2</c> (Depth range value 2) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>DRVAL2</c> is neither <see cref="DBNull"/> nor <c>null</c>; otherwise <c>false</c>.</returns>
        public static bool DRVAL2_HasValue(this Feature feature) => !(DBNull.Value == feature["DRVAL2"]) && feature["DRVAL2"] is not null;

        /// <summary>
        /// Data set name
        /// </summary>
        /// <param name="feature">The feature to read the <c>DSNM</c> field from.</param>
        /// <returns>The value of <c>DSNM</c>, or <c>null</c> when it is not set.</returns>
        public static string? DSNM(this Feature feature) {
            //if (DBNull.Value == feature["DSNM"]) return null;
            var v = Convert.ToString(feature["DSNM"]);
            //if ("-32767".Equals(v)) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>DSNM</c> (Data set name) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>DSNM</c> is neither <see cref="DBNull"/> nor <c>null</c> nor an empty string; otherwise <c>false</c>.</returns>
        public static bool DSNM_HasValue(this Feature feature) => !(DBNull.Value == feature["DSNM"]) && feature["DSNM"] is not null && !string.IsNullOrEmpty(Convert.ToString(feature["DSNM"]));

        /// <summary>
        /// DSPM Comment
        /// </summary>
        /// <param name="feature">The feature to read the <c>DSPM_COMT</c> field from.</param>
        /// <returns>The value of <c>DSPM_COMT</c>, or <c>null</c> when it is not set.</returns>
        public static string? DSPM_COMT(this Feature feature) {
            //if (DBNull.Value == feature["DSPM_COMT"]) return null;
            var v = Convert.ToString(feature["DSPM_COMT"]);
            //if ("-32767".Equals(v)) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>DSPM_COMT</c> (DSPM Comment) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>DSPM_COMT</c> is neither <see cref="DBNull"/> nor <c>null</c> nor an empty string; otherwise <c>false</c>.</returns>
        public static bool DSPM_COMT_HasValue(this Feature feature) => !(DBNull.Value == feature["DSPM_COMT"]) && feature["DSPM_COMT"] is not null && !string.IsNullOrEmpty(Convert.ToString(feature["DSPM_COMT"]));

        /// <summary>
        /// Depth Units
        /// </summary>
        /// <param name="feature">The feature to read the <c>DUNI</c> field from.</param>
        /// <returns>The value of <c>DUNI</c>, or <c>null</c> when it is not set.</returns>
        public static int? DUNI(this Feature feature) {
            if (DBNull.Value == feature["DUNI"]) return null;
            var v = Convert.ToInt32(feature["DUNI"]);
            //if (-32767 == v) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>DUNI</c> (Depth Units) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>DUNI</c> is neither <see cref="DBNull"/> nor <c>null</c>; otherwise <c>false</c>.</returns>
        public static bool DUNI_HasValue(this Feature feature) => !(DBNull.Value == feature["DUNI"]) && feature["DUNI"] is not null;

        /// <summary>
        /// Edition Number
        /// </summary>
        /// <param name="feature">The feature to read the <c>EDTN</c> field from.</param>
        /// <returns>The value of <c>EDTN</c>, or <c>null</c> when it is not set.</returns>
        public static int? EDTN(this Feature feature) {
            if (DBNull.Value == feature["EDTN"]) return null;
            var v = Convert.ToInt32(feature["EDTN"]);
            //if (-32767 == v) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>EDTN</c> (Edition Number) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>EDTN</c> is neither <see cref="DBNull"/> nor <c>null</c>; otherwise <c>false</c>.</returns>
        public static bool EDTN_HasValue(this Feature feature) => !(DBNull.Value == feature["EDTN"]) && feature["EDTN"] is not null;

        /// <summary>
        /// Elevation
        /// </summary>
        /// <param name="feature">The feature to read the <c>ELEVAT</c> field from.</param>
        /// <returns>The value of <c>ELEVAT</c>, or <c>null</c> when it is not set.</returns>
        public static decimal? ELEVAT(this Feature feature) {
            if (DBNull.Value == feature["ELEVAT"]) return null;
            var v = Convert.ToDecimal(feature["ELEVAT"]);
            //if (-32767m == v) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>ELEVAT</c> (Elevation) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>ELEVAT</c> is neither <see cref="DBNull"/> nor <c>null</c>; otherwise <c>false</c>.</returns>
        public static bool ELEVAT_HasValue(this Feature feature) => !(DBNull.Value == feature["ELEVAT"]) && feature["ELEVAT"] is not null;

        /// <summary>
        /// Entry date
        /// </summary>
        /// <param name="feature">The feature to read the <c>ENTRY_DATE</c> field from.</param>
        /// <returns>The value of <c>ENTRY_DATE</c>, or <c>null</c> when it is not set.</returns>
        public static decimal? ENTRY_DATE(this Feature feature) {
            if (DBNull.Value == feature["ENTRY_DATE"]) return null;
            var v = Convert.ToDecimal(feature["ENTRY_DATE"]);
            //if (-32767m == v) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>ENTRY_DATE</c> (Entry date) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>ENTRY_DATE</c> is neither <see cref="DBNull"/> nor <c>null</c>; otherwise <c>false</c>.</returns>
        public static bool ENTRY_DATE_HasValue(this Feature feature) => !(DBNull.Value == feature["ENTRY_DATE"]) && feature["ENTRY_DATE"] is not null;

        /// <summary>
        /// Estimated range of transmission
        /// </summary>
        /// <param name="feature">The feature to read the <c>ESTRNG</c> field from.</param>
        /// <returns>The value of <c>ESTRNG</c>, or <c>null</c> when it is not set.</returns>
        public static decimal? ESTRNG(this Feature feature) {
            if (DBNull.Value == feature["ESTRNG"]) return null;
            var v = Convert.ToDecimal(feature["ESTRNG"]);
            //if (-32767m == v) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>ESTRNG</c> (Estimated range of transmission) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>ESTRNG</c> is neither <see cref="DBNull"/> nor <c>null</c>; otherwise <c>false</c>.</returns>
        public static bool ESTRNG_HasValue(this Feature feature) => !(DBNull.Value == feature["ESTRNG"]) && feature["ESTRNG"] is not null;

        /// <summary>
        /// Exhibition condition of light
        /// </summary>
        /// <param name="feature">The feature to read the <c>EXCLIT</c> field from.</param>
        /// <returns>The value of <c>EXCLIT</c>, or <c>null</c> when it is not set.</returns>
        public static int? EXCLIT(this Feature feature) {
            if (DBNull.Value == feature["EXCLIT"]) return null;
            var v = Convert.ToInt32(feature["EXCLIT"]);
            //if (-32767 == v) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>EXCLIT</c> (Exhibition condition of light) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>EXCLIT</c> is neither <see cref="DBNull"/> nor <c>null</c>; otherwise <c>false</c>.</returns>
        public static bool EXCLIT_HasValue(this Feature feature) => !(DBNull.Value == feature["EXCLIT"]) && feature["EXCLIT"] is not null;

        /// <summary>
        /// Export Type
        /// </summary>
        /// <param name="feature">The feature to read the <c>EXPORTTYPE</c> field from.</param>
        /// <returns>The value of <c>EXPORTTYPE</c>, or <c>null</c> when it is not set.</returns>
        public static string? EXPORTTYPE(this Feature feature) {
            //if (DBNull.Value == feature["EXPORTTYPE"]) return null;
            var v = Convert.ToString(feature["EXPORTTYPE"]);
            //if ("-32767".Equals(v)) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>EXPORTTYPE</c> (Export Type) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>EXPORTTYPE</c> is neither <see cref="DBNull"/> nor <c>null</c> nor an empty string; otherwise <c>false</c>.</returns>
        public static bool EXPORTTYPE_HasValue(this Feature feature) => !(DBNull.Value == feature["EXPORTTYPE"]) && feature["EXPORTTYPE"] is not null && !string.IsNullOrEmpty(Convert.ToString(feature["EXPORTTYPE"]));

        /// <summary>
        /// Exposition of sounding
        /// </summary>
        /// <param name="feature">The feature to read the <c>EXPSOU</c> field from.</param>
        /// <returns>The value of <c>EXPSOU</c>, or <c>null</c> when it is not set.</returns>
        public static int? EXPSOU(this Feature feature) {
            if (DBNull.Value == feature["EXPSOU"]) return null;
            var v = Convert.ToInt32(feature["EXPSOU"]);
            //if (-32767 == v) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>EXPSOU</c> (Exposition of sounding) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>EXPSOU</c> is neither <see cref="DBNull"/> nor <c>null</c>; otherwise <c>false</c>.</returns>
        public static bool EXPSOU_HasValue(this Feature feature) => !(DBNull.Value == feature["EXPSOU"]) && feature["EXPSOU"] is not null;

        /// <summary>
        /// FCSubtype
        /// </summary>
        /// <param name="feature">The feature to read the <c>FCSUBTYPE</c> field from.</param>
        /// <returns>The value of <c>FCSUBTYPE</c>, or <c>null</c> when it is not set.</returns>
        public static int? FCSUBTYPE(this Feature feature) {
            if (DBNull.Value == feature["FCSUBTYPE"]) return null;
            var v = Convert.ToInt32(feature["FCSUBTYPE"]);
            //if (-32767 == v) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>FCSUBTYPE</c> (FCSubtype) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>FCSUBTYPE</c> is neither <see cref="DBNull"/> nor <c>null</c>; otherwise <c>false</c>.</returns>
        public static bool FCSUBTYPE_HasValue(this Feature feature) => !(DBNull.Value == feature["FCSUBTYPE"]) && feature["FCSUBTYPE"] is not null;

        /// <summary>
        /// FEATURECLASS
        /// </summary>
        /// <param name="feature">The feature to read the <c>FEATURECLASS</c> field from.</param>
        /// <returns>The value of <c>FEATURECLASS</c>, or <c>null</c> when it is not set.</returns>
        public static string? FEATURECLASS(this Feature feature) {
            //if (DBNull.Value == feature["FEATURECLASS"]) return null;
            var v = Convert.ToString(feature["FEATURECLASS"]);
            //if ("-32767".Equals(v)) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>FEATURECLASS</c> (FEATURECLASS) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>FEATURECLASS</c> is neither <see cref="DBNull"/> nor <c>null</c> nor an empty string; otherwise <c>false</c>.</returns>
        public static bool FEATURECLASS_HasValue(this Feature feature) => !(DBNull.Value == feature["FEATURECLASS"]) && feature["FEATURECLASS"] is not null && !string.IsNullOrEmpty(Convert.ToString(feature["FEATURECLASS"]));

        /// <summary>
        /// Function
        /// </summary>
        /// <param name="feature">The feature to read the <c>FUNCTN</c> field from.</param>
        /// <returns>The value of <c>FUNCTN</c>, or <c>null</c> when it is not set.</returns>
        public static string? FUNCTN(this Feature feature) {
            //if (DBNull.Value == feature["FUNCTN"]) return null;
            var v = Convert.ToString(feature["FUNCTN"]);
            //if ("-32767".Equals(v)) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>FUNCTN</c> (Function) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>FUNCTN</c> is neither <see cref="DBNull"/> nor <c>null</c> nor an empty string; otherwise <c>false</c>.</returns>
        public static bool FUNCTN_HasValue(this Feature feature) => !(DBNull.Value == feature["FUNCTN"]) && feature["FUNCTN"] is not null && !string.IsNullOrEmpty(Convert.ToString(feature["FUNCTN"]));

        /// <summary>
        /// GlobalID
        /// </summary>
        /// <param name="feature">The feature to read the <c>GLOBALID</c> field from.</param>
        /// <returns>The value of <c>GLOBALID</c>, or <c>null</c> when it is not set.</returns>
        public static Guid? GLOBALID(this Feature feature) {
            if (DBNull.Value == feature["GLOBALID"]) return null;
            return Guid.TryParse(Convert.ToString(feature["GLOBALID"]), out var v) ? v : null;
        }

        /// <summary>
        /// Indicates whether <c>GLOBALID</c> (GlobalID) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>GLOBALID</c> is neither <see cref="DBNull"/> nor <c>null</c>; otherwise <c>false</c>.</returns>
        public static bool GLOBALID_HasValue(this Feature feature) => !(DBNull.Value == feature["GLOBALID"]) && feature["GLOBALID"] is not null;

        /// <summary>
        /// Horizontal Geodetic Datum
        /// </summary>
        /// <param name="feature">The feature to read the <c>HDAT</c> field from.</param>
        /// <returns>The value of <c>HDAT</c>, or <c>null</c> when it is not set.</returns>
        public static int? HDAT(this Feature feature) {
            if (DBNull.Value == feature["HDAT"]) return null;
            var v = Convert.ToInt32(feature["HDAT"]);
            //if (-32767 == v) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>HDAT</c> (Horizontal Geodetic Datum) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>HDAT</c> is neither <see cref="DBNull"/> nor <c>null</c>; otherwise <c>false</c>.</returns>
        public static bool HDAT_HasValue(this Feature feature) => !(DBNull.Value == feature["HDAT"]) && feature["HDAT"] is not null;

        /// <summary>
        /// Height
        /// </summary>
        /// <param name="feature">The feature to read the <c>HEIGHT</c> field from.</param>
        /// <returns>The value of <c>HEIGHT</c>, or <c>null</c> when it is not set.</returns>
        public static decimal? HEIGHT(this Feature feature) {
            if (DBNull.Value == feature["HEIGHT"]) return null;
            var v = Convert.ToDecimal(feature["HEIGHT"]);
            //if (-32767m == v) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>HEIGHT</c> (Height) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>HEIGHT</c> is neither <see cref="DBNull"/> nor <c>null</c>; otherwise <c>false</c>.</returns>
        public static bool HEIGHT_HasValue(this Feature feature) => !(DBNull.Value == feature["HEIGHT"]) && feature["HEIGHT"] is not null;

        /// <summary>
        /// Horizontal accuracy
        /// </summary>
        /// <param name="feature">The feature to read the <c>HORACC</c> field from.</param>
        /// <returns>The value of <c>HORACC</c>, or <c>null</c> when it is not set.</returns>
        public static decimal? HORACC(this Feature feature) {
            if (DBNull.Value == feature["HORACC"]) return null;
            var v = Convert.ToDecimal(feature["HORACC"]);
            //if (-32767m == v) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>HORACC</c> (Horizontal accuracy) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>HORACC</c> is neither <see cref="DBNull"/> nor <c>null</c>; otherwise <c>false</c>.</returns>
        public static bool HORACC_HasValue(this Feature feature) => !(DBNull.Value == feature["HORACC"]) && feature["HORACC"] is not null;

        /// <summary>
        /// Horizontal clearance
        /// </summary>
        /// <param name="feature">The feature to read the <c>HORCLR</c> field from.</param>
        /// <returns>The value of <c>HORCLR</c>, or <c>null</c> when it is not set.</returns>
        public static decimal? HORCLR(this Feature feature) {
            if (DBNull.Value == feature["HORCLR"]) return null;
            var v = Convert.ToDecimal(feature["HORCLR"]);
            //if (-32767m == v) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>HORCLR</c> (Horizontal clearance) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>HORCLR</c> is neither <see cref="DBNull"/> nor <c>null</c>; otherwise <c>false</c>.</returns>
        public static bool HORCLR_HasValue(this Feature feature) => !(DBNull.Value == feature["HORCLR"]) && feature["HORCLR"] is not null;

        /// <summary>
        /// Horizontal datum
        /// </summary>
        /// <param name="feature">The feature to read the <c>HORDAT</c> field from.</param>
        /// <returns>The value of <c>HORDAT</c>, or <c>null</c> when it is not set.</returns>
        public static int? HORDAT(this Feature feature) {
            if (DBNull.Value == feature["HORDAT"]) return null;
            var v = Convert.ToInt32(feature["HORDAT"]);
            //if (-32767 == v) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>HORDAT</c> (Horizontal datum) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>HORDAT</c> is neither <see cref="DBNull"/> nor <c>null</c>; otherwise <c>false</c>.</returns>
        public static bool HORDAT_HasValue(this Feature feature) => !(DBNull.Value == feature["HORDAT"]) && feature["HORDAT"] is not null;

        /// <summary>
        /// Horizontal length
        /// </summary>
        /// <param name="feature">The feature to read the <c>HORLEN</c> field from.</param>
        /// <returns>The value of <c>HORLEN</c>, or <c>null</c> when it is not set.</returns>
        public static decimal? HORLEN(this Feature feature) {
            if (DBNull.Value == feature["HORLEN"]) return null;
            var v = Convert.ToDecimal(feature["HORLEN"]);
            //if (-32767m == v) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>HORLEN</c> (Horizontal length) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>HORLEN</c> is neither <see cref="DBNull"/> nor <c>null</c>; otherwise <c>false</c>.</returns>
        public static bool HORLEN_HasValue(this Feature feature) => !(DBNull.Value == feature["HORLEN"]) && feature["HORLEN"] is not null;

        /// <summary>
        /// Horizontal width
        /// </summary>
        /// <param name="feature">The feature to read the <c>HORWID</c> field from.</param>
        /// <returns>The value of <c>HORWID</c>, or <c>null</c> when it is not set.</returns>
        public static decimal? HORWID(this Feature feature) {
            if (DBNull.Value == feature["HORWID"]) return null;
            var v = Convert.ToDecimal(feature["HORWID"]);
            //if (-32767m == v) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>HORWID</c> (Horizontal width) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>HORWID</c> is neither <see cref="DBNull"/> nor <c>null</c>; otherwise <c>false</c>.</returns>
        public static bool HORWID_HasValue(this Feature feature) => !(DBNull.Value == feature["HORWID"]) && feature["HORWID"] is not null;

        /// <summary>
        /// Height Units
        /// </summary>
        /// <param name="feature">The feature to read the <c>HUNI</c> field from.</param>
        /// <returns>The value of <c>HUNI</c>, or <c>null</c> when it is not set.</returns>
        public static int? HUNI(this Feature feature) {
            if (DBNull.Value == feature["HUNI"]) return null;
            var v = Convert.ToInt32(feature["HUNI"]);
            //if (-32767 == v) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>HUNI</c> (Height Units) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>HUNI</c> is neither <see cref="DBNull"/> nor <c>null</c>; otherwise <c>false</c>.</returns>
        public static bool HUNI_HasValue(this Feature feature) => !(DBNull.Value == feature["HUNI"]) && feature["HUNI"] is not null;

        /// <summary>
        /// Ice factor
        /// </summary>
        /// <param name="feature">The feature to read the <c>ICEFAC</c> field from.</param>
        /// <returns>The value of <c>ICEFAC</c>, or <c>null</c> when it is not set.</returns>
        public static decimal? ICEFAC(this Feature feature) {
            if (DBNull.Value == feature["ICEFAC"]) return null;
            var v = Convert.ToDecimal(feature["ICEFAC"]);
            //if (-32767m == v) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>ICEFAC</c> (Ice factor) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>ICEFAC</c> is neither <see cref="DBNull"/> nor <c>null</c>; otherwise <c>false</c>.</returns>
        public static bool ICEFAC_HasValue(this Feature feature) => !(DBNull.Value == feature["ICEFAC"]) && feature["ICEFAC"] is not null;

        /// <summary>
        /// Information
        /// </summary>
        /// <param name="feature">The feature to read the <c>INFORM</c> field from.</param>
        /// <returns>The value of <c>INFORM</c>, or <c>null</c> when it is not set.</returns>
        public static string? INFORM(this Feature feature) {
            //if (DBNull.Value == feature["INFORM"]) return null;
            var v = Convert.ToString(feature["INFORM"]);
            //if ("-32767".Equals(v)) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>INFORM</c> (Information) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>INFORM</c> is neither <see cref="DBNull"/> nor <c>null</c> nor an empty string; otherwise <c>false</c>.</returns>
        public static bool INFORM_HasValue(this Feature feature) => !(DBNull.Value == feature["INFORM"]) && feature["INFORM"] is not null && !string.IsNullOrEmpty(Convert.ToString(feature["INFORM"]));

        /// <summary>
        /// INTU
        /// </summary>
        /// <param name="feature">The feature to read the <c>INTU</c> field from.</param>
        /// <returns>The value of <c>INTU</c>, or <c>null</c> when it is not set.</returns>
        public static int? INTU(this Feature feature) {
            if (DBNull.Value == feature["INTU"]) return null;
            var v = Convert.ToInt32(feature["INTU"]);
            //if (-32767 == v) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>INTU</c> (INTU) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>INTU</c> is neither <see cref="DBNull"/> nor <c>null</c>; otherwise <c>false</c>.</returns>
        public static bool INTU_HasValue(this Feature feature) => !(DBNull.Value == feature["INTU"]) && feature["INTU"] is not null;

        /// <summary>
        /// Issue Date
        /// </summary>
        /// <param name="feature">The feature to read the <c>ISDT</c> field from.</param>
        /// <returns>The value of <c>ISDT</c>, or <c>null</c> when it is not set.</returns>
        public static DateTime? ISDT(this Feature feature) {
            if (DBNull.Value == feature["ISDT"]) return null;
            return Convert.ToDateTime(feature["ISDT"]);
        }

        /// <summary>
        /// Indicates whether <c>ISDT</c> (Issue Date) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>ISDT</c> is neither <see cref="DBNull"/> nor <c>null</c>; otherwise <c>false</c>.</returns>
        public static bool ISDT_HasValue(this Feature feature) => !(DBNull.Value == feature["ISDT"]) && feature["ISDT"] is not null;

        /// <summary>
        /// Is Conflate
        /// </summary>
        /// <param name="feature">The feature to read the <c>IS_CONFLATE</c> field from.</param>
        /// <returns>The value of <c>IS_CONFLATE</c>, or <c>null</c> when it is not set.</returns>
        public static int? IS_CONFLATE(this Feature feature) {
            if (DBNull.Value == feature["IS_CONFLATE"]) return null;
            var v = Convert.ToInt32(feature["IS_CONFLATE"]);
            //if (-32767 == v) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>IS_CONFLATE</c> (Is Conflate) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>IS_CONFLATE</c> is neither <see cref="DBNull"/> nor <c>null</c>; otherwise <c>false</c>.</returns>
        public static bool IS_CONFLATE_HasValue(this Feature feature) => !(DBNull.Value == feature["IS_CONFLATE"]) && feature["IS_CONFLATE"] is not null;

        /// <summary>
        /// Jurisdiction
        /// </summary>
        /// <param name="feature">The feature to read the <c>JRSDTN</c> field from.</param>
        /// <returns>The value of <c>JRSDTN</c>, or <c>null</c> when it is not set.</returns>
        public static int? JRSDTN(this Feature feature) {
            if (DBNull.Value == feature["JRSDTN"]) return null;
            var v = Convert.ToInt32(feature["JRSDTN"]);
            //if (-32767 == v) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>JRSDTN</c> (Jurisdiction) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>JRSDTN</c> is neither <see cref="DBNull"/> nor <c>null</c>; otherwise <c>false</c>.</returns>
        public static bool JRSDTN_HasValue(this Feature feature) => !(DBNull.Value == feature["JRSDTN"]) && feature["JRSDTN"] is not null;

        /// <summary>
        /// Last modified
        /// </summary>
        /// <param name="feature">The feature to read the <c>LAST_MOD</c> field from.</param>
        /// <returns>The value of <c>LAST_MOD</c>, or <c>null</c> when it is not set.</returns>
        public static DateTime? LAST_MOD(this Feature feature) {
            if (DBNull.Value == feature["LAST_MOD"]) return null;
            return Convert.ToDateTime(feature["LAST_MOD"]);
        }

        /// <summary>
        /// Indicates whether <c>LAST_MOD</c> (Last modified) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>LAST_MOD</c> is neither <see cref="DBNull"/> nor <c>null</c>; otherwise <c>false</c>.</returns>
        public static bool LAST_MOD_HasValue(this Feature feature) => !(DBNull.Value == feature["LAST_MOD"]) && feature["LAST_MOD"] is not null;

        /// <summary>
        /// Lifting capacity
        /// </summary>
        /// <param name="feature">The feature to read the <c>LIFCAP</c> field from.</param>
        /// <returns>The value of <c>LIFCAP</c>, or <c>null</c> when it is not set.</returns>
        public static decimal? LIFCAP(this Feature feature) {
            if (DBNull.Value == feature["LIFCAP"]) return null;
            var v = Convert.ToDecimal(feature["LIFCAP"]);
            //if (-32767m == v) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>LIFCAP</c> (Lifting capacity) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>LIFCAP</c> is neither <see cref="DBNull"/> nor <c>null</c>; otherwise <c>false</c>.</returns>
        public static bool LIFCAP_HasValue(this Feature feature) => !(DBNull.Value == feature["LIFCAP"]) && feature["LIFCAP"] is not null;

        /// <summary>
        /// Light characteristic
        /// </summary>
        /// <param name="feature">The feature to read the <c>LITCHR</c> field from.</param>
        /// <returns>The value of <c>LITCHR</c>, or <c>null</c> when it is not set.</returns>
        public static int? LITCHR(this Feature feature) {
            if (DBNull.Value == feature["LITCHR"]) return null;
            var v = Convert.ToInt32(feature["LITCHR"]);
            //if (-32767 == v) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>LITCHR</c> (Light characteristic) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>LITCHR</c> is neither <see cref="DBNull"/> nor <c>null</c>; otherwise <c>false</c>.</returns>
        public static bool LITCHR_HasValue(this Feature feature) => !(DBNull.Value == feature["LITCHR"]) && feature["LITCHR"] is not null;

        /// <summary>
        /// Light visibility
        /// </summary>
        /// <param name="feature">The feature to read the <c>LITVIS</c> field from.</param>
        /// <returns>The value of <c>LITVIS</c>, or <c>null</c> when it is not set.</returns>
        public static string? LITVIS(this Feature feature) {
            //if (DBNull.Value == feature["LITVIS"]) return null;
            var v = Convert.ToString(feature["LITVIS"]);
            //if ("-32767".Equals(v)) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>LITVIS</c> (Light visibility) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>LITVIS</c> is neither <see cref="DBNull"/> nor <c>null</c> nor an empty string; otherwise <c>false</c>.</returns>
        public static bool LITVIS_HasValue(this Feature feature) => !(DBNull.Value == feature["LITVIS"]) && feature["LITVIS"] is not null && !string.IsNullOrEmpty(Convert.ToString(feature["LITVIS"]));

        /// <summary>
        /// Long name
        /// </summary>
        /// <param name="feature">The feature to read the <c>LNAM</c> field from.</param>
        /// <returns>The value of <c>LNAM</c>, or <c>null</c> when it is not set.</returns>
        public static string? LNAM(this Feature feature) {
            //if (DBNull.Value == feature["LNAM"]) return null;
            var v = Convert.ToString(feature["LNAM"]);
            //if ("-32767".Equals(v)) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>LNAM</c> (Long name) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>LNAM</c> is neither <see cref="DBNull"/> nor <c>null</c> nor an empty string; otherwise <c>false</c>.</returns>
        public static bool LNAM_HasValue(this Feature feature) => !(DBNull.Value == feature["LNAM"]) && feature["LNAM"] is not null && !string.IsNullOrEmpty(Convert.ToString(feature["LNAM"]));

        /// <summary>
        /// Marks navigational - System of
        /// </summary>
        /// <param name="feature">The feature to read the <c>MARSYS</c> field from.</param>
        /// <returns>The value of <c>MARSYS</c>, or <c>null</c> when it is not set.</returns>
        public static int? MARSYS(this Feature feature) {
            if (DBNull.Value == feature["MARSYS"]) return null;
            var v = Convert.ToInt32(feature["MARSYS"]);
            //if (-32767 == v) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>MARSYS</c> (Marks navigational - System of) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>MARSYS</c> is neither <see cref="DBNull"/> nor <c>null</c>; otherwise <c>false</c>.</returns>
        public static bool MARSYS_HasValue(this Feature feature) => !(DBNull.Value == feature["MARSYS"]) && feature["MARSYS"] is not null;

        /// <summary>
        /// Mulitiplicity of lights
        /// </summary>
        /// <param name="feature">The feature to read the <c>MLTYLT</c> field from.</param>
        /// <returns>The value of <c>MLTYLT</c>, or <c>null</c> when it is not set.</returns>
        public static int? MLTYLT(this Feature feature) {
            if (DBNull.Value == feature["MLTYLT"]) return null;
            var v = Convert.ToInt32(feature["MLTYLT"]);
            //if (-32767 == v) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>MLTYLT</c> (Mulitiplicity of lights) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>MLTYLT</c> is neither <see cref="DBNull"/> nor <c>null</c>; otherwise <c>false</c>.</returns>
        public static bool MLTYLT_HasValue(this Feature feature) => !(DBNull.Value == feature["MLTYLT"]) && feature["MLTYLT"] is not null;

        /// <summary>
        /// NATF Lexical Level
        /// </summary>
        /// <param name="feature">The feature to read the <c>NALL</c> field from.</param>
        /// <returns>The value of <c>NALL</c>, or <c>null</c> when it is not set.</returns>
        public static int? NALL(this Feature feature) {
            if (DBNull.Value == feature["NALL"]) return null;
            var v = Convert.ToInt32(feature["NALL"]);
            //if (-32767 == v) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>NALL</c> (NATF Lexical Level) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>NALL</c> is neither <see cref="DBNull"/> nor <c>null</c>; otherwise <c>false</c>.</returns>
        public static bool NALL_HasValue(this Feature feature) => !(DBNull.Value == feature["NALL"]) && feature["NALL"] is not null;

        /// <summary>
        /// Name
        /// </summary>
        /// <param name="feature">The feature to read the <c>NAME</c> field from.</param>
        /// <returns>The value of <c>NAME</c>, or <c>null</c> when it is not set.</returns>
        public static string? NAME(this Feature feature) {
            //if (DBNull.Value == feature["NAME"]) return null;
            var v = Convert.ToString(feature["NAME"]);
            //if ("-32767".Equals(v)) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>NAME</c> (Name) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>NAME</c> is neither <see cref="DBNull"/> nor <c>null</c> nor an empty string; otherwise <c>false</c>.</returns>
        public static bool NAME_HasValue(this Feature feature) => !(DBNull.Value == feature["NAME"]) && feature["NAME"] is not null && !string.IsNullOrEmpty(Convert.ToString(feature["NAME"]));

        /// <summary>
        /// Nature of construction
        /// </summary>
        /// <param name="feature">The feature to read the <c>NATCON</c> field from.</param>
        /// <returns>The value of <c>NATCON</c>, or <c>null</c> when it is not set.</returns>
        public static string? NATCON(this Feature feature) {
            //if (DBNull.Value == feature["NATCON"]) return null;
            var v = Convert.ToString(feature["NATCON"]);
            //if ("-32767".Equals(v)) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>NATCON</c> (Nature of construction) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>NATCON</c> is neither <see cref="DBNull"/> nor <c>null</c> nor an empty string; otherwise <c>false</c>.</returns>
        public static bool NATCON_HasValue(this Feature feature) => !(DBNull.Value == feature["NATCON"]) && feature["NATCON"] is not null && !string.IsNullOrEmpty(Convert.ToString(feature["NATCON"]));

        /// <summary>
        /// Nationality
        /// </summary>
        /// <param name="feature">The feature to read the <c>NATION</c> field from.</param>
        /// <returns>The value of <c>NATION</c>, or <c>null</c> when it is not set.</returns>
        public static string? NATION(this Feature feature) {
            //if (DBNull.Value == feature["NATION"]) return null;
            var v = Convert.ToString(feature["NATION"]);
            //if ("-32767".Equals(v)) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>NATION</c> (Nationality) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>NATION</c> is neither <see cref="DBNull"/> nor <c>null</c> nor an empty string; otherwise <c>false</c>.</returns>
        public static bool NATION_HasValue(this Feature feature) => !(DBNull.Value == feature["NATION"]) && feature["NATION"] is not null && !string.IsNullOrEmpty(Convert.ToString(feature["NATION"]));

        /// <summary>
        /// Nature of surface - qualifying terms
        /// </summary>
        /// <param name="feature">The feature to read the <c>NATQUA</c> field from.</param>
        /// <returns>The value of <c>NATQUA</c>, or <c>null</c> when it is not set.</returns>
        public static string? NATQUA(this Feature feature) {
            //if (DBNull.Value == feature["NATQUA"]) return null;
            var v = Convert.ToString(feature["NATQUA"]);
            //if ("-32767".Equals(v)) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>NATQUA</c> (Nature of surface - qualifying terms) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>NATQUA</c> is neither <see cref="DBNull"/> nor <c>null</c> nor an empty string; otherwise <c>false</c>.</returns>
        public static bool NATQUA_HasValue(this Feature feature) => !(DBNull.Value == feature["NATQUA"]) && feature["NATQUA"] is not null && !string.IsNullOrEmpty(Convert.ToString(feature["NATQUA"]));

        /// <summary>
        /// Nature of surface
        /// </summary>
        /// <param name="feature">The feature to read the <c>NATSUR</c> field from.</param>
        /// <returns>The value of <c>NATSUR</c>, or <c>null</c> when it is not set.</returns>
        public static string? NATSUR(this Feature feature) {
            //if (DBNull.Value == feature["NATSUR"]) return null;
            var v = Convert.ToString(feature["NATSUR"]);
            //if ("-32767".Equals(v)) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>NATSUR</c> (Nature of surface) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>NATSUR</c> is neither <see cref="DBNull"/> nor <c>null</c> nor an empty string; otherwise <c>false</c>.</returns>
        public static bool NATSUR_HasValue(this Feature feature) => !(DBNull.Value == feature["NATSUR"]) && feature["NATSUR"] is not null && !string.IsNullOrEmpty(Convert.ToString(feature["NATSUR"]));

        /// <summary>
        /// Information in national language
        /// </summary>
        /// <param name="feature">The feature to read the <c>NINFOM</c> field from.</param>
        /// <returns>The value of <c>NINFOM</c>, or <c>null</c> when it is not set.</returns>
        public static string? NINFOM(this Feature feature) {
            //if (DBNull.Value == feature["NINFOM"]) return null;
            var v = Convert.ToString(feature["NINFOM"]);
            //if ("-32767".Equals(v)) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>NINFOM</c> (Information in national language) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>NINFOM</c> is neither <see cref="DBNull"/> nor <c>null</c> nor an empty string; otherwise <c>false</c>.</returns>
        public static bool NINFOM_HasValue(this Feature feature) => !(DBNull.Value == feature["NINFOM"]) && feature["NINFOM"] is not null && !string.IsNullOrEmpty(Convert.ToString(feature["NINFOM"]));

        /// <summary>
        /// Object name in national language
        /// </summary>
        /// <param name="feature">The feature to read the <c>NOBJNM</c> field from.</param>
        /// <returns>The value of <c>NOBJNM</c>, or <c>null</c> when it is not set.</returns>
        public static string? NOBJNM(this Feature feature) {
            //if (DBNull.Value == feature["NOBJNM"]) return null;
            var v = Convert.ToString(feature["NOBJNM"]);
            //if ("-32767".Equals(v)) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>NOBJNM</c> (Object name in national language) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>NOBJNM</c> is neither <see cref="DBNull"/> nor <c>null</c> nor an empty string; otherwise <c>false</c>.</returns>
        public static bool NOBJNM_HasValue(this Feature feature) => !(DBNull.Value == feature["NOBJNM"]) && feature["NOBJNM"] is not null && !string.IsNullOrEmpty(Convert.ToString(feature["NOBJNM"]));

        /// <summary>
        /// Nautical Object ID
        /// </summary>
        /// <param name="feature">The feature to read the <c>NOID</c> field from.</param>
        /// <returns>The value of <c>NOID</c>, or <c>null</c> when it is not set.</returns>
        public static string? NOID(this Feature feature) {
            //if (DBNull.Value == feature["NOID"]) return null;
            var v = Convert.ToString(feature["NOID"]);
            //if ("-32767".Equals(v)) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>NOID</c> (Nautical Object ID) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>NOID</c> is neither <see cref="DBNull"/> nor <c>null</c> nor an empty string; otherwise <c>false</c>.</returns>
        public static bool NOID_HasValue(this Feature feature) => !(DBNull.Value == feature["NOID"]) && feature["NOID"] is not null && !string.IsNullOrEmpty(Convert.ToString(feature["NOID"]));

        /// <summary>
        /// Pilot district in national language
        /// </summary>
        /// <param name="feature">The feature to read the <c>NPLDST</c> field from.</param>
        /// <returns>The value of <c>NPLDST</c>, or <c>null</c> when it is not set.</returns>
        public static string? NPLDST(this Feature feature) {
            //if (DBNull.Value == feature["NPLDST"]) return null;
            var v = Convert.ToString(feature["NPLDST"]);
            //if ("-32767".Equals(v)) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>NPLDST</c> (Pilot district in national language) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>NPLDST</c> is neither <see cref="DBNull"/> nor <c>null</c> nor an empty string; otherwise <c>false</c>.</returns>
        public static bool NPLDST_HasValue(this Feature feature) => !(DBNull.Value == feature["NPLDST"]) && feature["NPLDST"] is not null && !string.IsNullOrEmpty(Convert.ToString(feature["NPLDST"]));

        /// <summary>
        /// Textual description in national language
        /// </summary>
        /// <param name="feature">The feature to read the <c>NTXTDS</c> field from.</param>
        /// <returns>The value of <c>NTXTDS</c>, or <c>null</c> when it is not set.</returns>
        public static string? NTXTDS(this Feature feature) {
            //if (DBNull.Value == feature["NTXTDS"]) return null;
            var v = Convert.ToString(feature["NTXTDS"]);
            //if ("-32767".Equals(v)) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>NTXTDS</c> (Textual description in national language) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>NTXTDS</c> is neither <see cref="DBNull"/> nor <c>null</c> nor an empty string; otherwise <c>false</c>.</returns>
        public static bool NTXTDS_HasValue(this Feature feature) => !(DBNull.Value == feature["NTXTDS"]) && feature["NTXTDS"] is not null && !string.IsNullOrEmpty(Convert.ToString(feature["NTXTDS"]));

        /// <summary>
        /// ObjectClass
        /// </summary>
        /// <param name="feature">The feature to read the <c>OBJECTCLASS</c> field from.</param>
        /// <returns>The value of <c>OBJECTCLASS</c>, or <c>null</c> when it is not set.</returns>
        public static string? OBJECTCLASS(this Feature feature) {
            //if (DBNull.Value == feature["OBJECTCLASS"]) return null;
            var v = Convert.ToString(feature["OBJECTCLASS"]);
            //if ("-32767".Equals(v)) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>OBJECTCLASS</c> (ObjectClass) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>OBJECTCLASS</c> is neither <see cref="DBNull"/> nor <c>null</c> nor an empty string; otherwise <c>false</c>.</returns>
        public static bool OBJECTCLASS_HasValue(this Feature feature) => !(DBNull.Value == feature["OBJECTCLASS"]) && feature["OBJECTCLASS"] is not null && !string.IsNullOrEmpty(Convert.ToString(feature["OBJECTCLASS"]));

        /// <summary>
        /// OBJECTID
        /// </summary>
        /// <param name="feature">The feature to read the <c>OBJECTID</c> field from.</param>
        /// <returns>The value of <c>OBJECTID</c>, or <c>null</c> when it is not set.</returns>
        public static int? OBJECTID(this Feature feature) {
            if (DBNull.Value == feature["OBJECTID"]) return null;
            var v = Convert.ToInt32(feature["OBJECTID"]);
            //if (-32767 == v) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>OBJECTID</c> (OBJECTID) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>OBJECTID</c> is neither <see cref="DBNull"/> nor <c>null</c>; otherwise <c>false</c>.</returns>
        public static bool OBJECTID_HasValue(this Feature feature) => !(DBNull.Value == feature["OBJECTID"]) && feature["OBJECTID"] is not null;

        /// <summary>
        /// Object name
        /// </summary>
        /// <param name="feature">The feature to read the <c>OBJNAM</c> field from.</param>
        /// <returns>The value of <c>OBJNAM</c>, or <c>null</c> when it is not set.</returns>
        public static string? OBJNAM(this Feature feature) {
            //if (DBNull.Value == feature["OBJNAM"]) return null;
            var v = Convert.ToString(feature["OBJNAM"]);
            //if ("-32767".Equals(v)) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>OBJNAM</c> (Object name) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>OBJNAM</c> is neither <see cref="DBNull"/> nor <c>null</c> nor an empty string; otherwise <c>false</c>.</returns>
        public static bool OBJNAM_HasValue(this Feature feature) => !(DBNull.Value == feature["OBJNAM"]) && feature["OBJNAM"] is not null && !string.IsNullOrEmpty(Convert.ToString(feature["OBJNAM"]));

        /// <summary>
        /// Orientation
        /// </summary>
        /// <param name="feature">The feature to read the <c>ORIENT</c> field from.</param>
        /// <returns>The value of <c>ORIENT</c>, or <c>null</c> when it is not set.</returns>
        public static decimal? ORIENT(this Feature feature) {
            if (DBNull.Value == feature["ORIENT"]) return null;
            var v = Convert.ToDecimal(feature["ORIENT"]);
            //if (-32767m == v) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>ORIENT</c> (Orientation) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>ORIENT</c> is neither <see cref="DBNull"/> nor <c>null</c>; otherwise <c>false</c>.</returns>
        public static bool ORIENT_HasValue(this Feature feature) => !(DBNull.Value == feature["ORIENT"]) && feature["ORIENT"] is not null;

        /// <summary>
        /// Periodic date end
        /// </summary>
        /// <param name="feature">The feature to read the <c>PEREND</c> field from.</param>
        /// <returns>The value of <c>PEREND</c>, or <c>null</c> when it is not set.</returns>
        public static string? PEREND(this Feature feature) {
            //if (DBNull.Value == feature["PEREND"]) return null;
            var v = Convert.ToString(feature["PEREND"]);
            //if ("-32767".Equals(v)) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>PEREND</c> (Periodic date end) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>PEREND</c> is neither <see cref="DBNull"/> nor <c>null</c> nor an empty string; otherwise <c>false</c>.</returns>
        public static bool PEREND_HasValue(this Feature feature) => !(DBNull.Value == feature["PEREND"]) && feature["PEREND"] is not null && !string.IsNullOrEmpty(Convert.ToString(feature["PEREND"]));

        /// <summary>
        /// Periodic date start
        /// </summary>
        /// <param name="feature">The feature to read the <c>PERSTA</c> field from.</param>
        /// <returns>The value of <c>PERSTA</c>, or <c>null</c> when it is not set.</returns>
        public static string? PERSTA(this Feature feature) {
            //if (DBNull.Value == feature["PERSTA"]) return null;
            var v = Convert.ToString(feature["PERSTA"]);
            //if ("-32767".Equals(v)) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>PERSTA</c> (Periodic date start) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>PERSTA</c> is neither <see cref="DBNull"/> nor <c>null</c> nor an empty string; otherwise <c>false</c>.</returns>
        public static bool PERSTA_HasValue(this Feature feature) => !(DBNull.Value == feature["PERSTA"]) && feature["PERSTA"] is not null && !string.IsNullOrEmpty(Convert.ToString(feature["PERSTA"]));

        /// <summary>
        /// Pictorial representation
        /// </summary>
        /// <param name="feature">The feature to read the <c>PICREP</c> field from.</param>
        /// <returns>The value of <c>PICREP</c>, or <c>null</c> when it is not set.</returns>
        public static string? PICREP(this Feature feature) {
            //if (DBNull.Value == feature["PICREP"]) return null;
            var v = Convert.ToString(feature["PICREP"]);
            //if ("-32767".Equals(v)) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>PICREP</c> (Pictorial representation) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>PICREP</c> is neither <see cref="DBNull"/> nor <c>null</c> nor an empty string; otherwise <c>false</c>.</returns>
        public static bool PICREP_HasValue(this Feature feature) => !(DBNull.Value == feature["PICREP"]) && feature["PICREP"] is not null && !string.IsNullOrEmpty(Convert.ToString(feature["PICREP"]));

        /// <summary>
        /// Pilot district
        /// </summary>
        /// <param name="feature">The feature to read the <c>PILDST</c> field from.</param>
        /// <returns>The value of <c>PILDST</c>, or <c>null</c> when it is not set.</returns>
        public static string? PILDST(this Feature feature) {
            //if (DBNull.Value == feature["PILDST"]) return null;
            var v = Convert.ToString(feature["PILDST"]);
            //if ("-32767".Equals(v)) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>PILDST</c> (Pilot district) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>PILDST</c> is neither <see cref="DBNull"/> nor <c>null</c> nor an empty string; otherwise <c>false</c>.</returns>
        public static bool PILDST_HasValue(this Feature feature) => !(DBNull.Value == feature["PILDST"]) && feature["PILDST"] is not null && !string.IsNullOrEmpty(Convert.ToString(feature["PILDST"]));

        /// <summary>
        /// Positional Accuracy
        /// </summary>
        /// <param name="feature">The feature to read the <c>POSACC</c> field from.</param>
        /// <returns>The value of <c>POSACC</c>, or <c>null</c> when it is not set.</returns>
        public static decimal? POSACC(this Feature feature) {
            if (DBNull.Value == feature["POSACC"]) return null;
            var v = Convert.ToDecimal(feature["POSACC"]);
            //if (-32767m == v) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>POSACC</c> (Positional Accuracy) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>POSACC</c> is neither <see cref="DBNull"/> nor <c>null</c>; otherwise <c>false</c>.</returns>
        public static bool POSACC_HasValue(this Feature feature) => !(DBNull.Value == feature["POSACC"]) && feature["POSACC"] is not null;

        /// <summary>
        /// Product Spec Edition
        /// </summary>
        /// <param name="feature">The feature to read the <c>PRED</c> field from.</param>
        /// <returns>The value of <c>PRED</c>, or <c>null</c> when it is not set.</returns>
        public static string? PRED(this Feature feature) {
            //if (DBNull.Value == feature["PRED"]) return null;
            var v = Convert.ToString(feature["PRED"]);
            //if ("-32767".Equals(v)) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>PRED</c> (Product Spec Edition) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>PRED</c> is neither <see cref="DBNull"/> nor <c>null</c> nor an empty string; otherwise <c>false</c>.</returns>
        public static bool PRED_HasValue(this Feature feature) => !(DBNull.Value == feature["PRED"]) && feature["PRED"] is not null && !string.IsNullOrEmpty(Convert.ToString(feature["PRED"]));

        /// <summary>
        /// PRIM
        /// </summary>
        /// <param name="feature">The feature to read the <c>PRIM</c> field from.</param>
        /// <returns>The value of <c>PRIM</c>, or <c>null</c> when it is not set.</returns>
        public static int? PRIM(this Feature feature) {
            if (DBNull.Value == feature["PRIM"]) return null;
            var v = Convert.ToInt32(feature["PRIM"]);
            //if (-32767 == v) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>PRIM</c> (PRIM) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>PRIM</c> is neither <see cref="DBNull"/> nor <c>null</c>; otherwise <c>false</c>.</returns>
        public static bool PRIM_HasValue(this Feature feature) => !(DBNull.Value == feature["PRIM"]) && feature["PRIM"] is not null;

        /// <summary>
        /// Priority
        /// </summary>
        /// <param name="feature">The feature to read the <c>PRIORITY</c> field from.</param>
        /// <returns>The value of <c>PRIORITY</c>, or <c>null</c> when it is not set.</returns>
        public static int? PRIORITY(this Feature feature) {
            if (DBNull.Value == feature["PRIORITY"]) return null;
            var v = Convert.ToInt32(feature["PRIORITY"]);
            //if (-32767 == v) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>PRIORITY</c> (Priority) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>PRIORITY</c> is neither <see cref="DBNull"/> nor <c>null</c>; otherwise <c>false</c>.</returns>
        public static bool PRIORITY_HasValue(this Feature feature) => !(DBNull.Value == feature["PRIORITY"]) && feature["PRIORITY"] is not null;

        /// <summary>
        /// Product
        /// </summary>
        /// <param name="feature">The feature to read the <c>PRODCT</c> field from.</param>
        /// <returns>The value of <c>PRODCT</c>, or <c>null</c> when it is not set.</returns>
        public static string? PRODCT(this Feature feature) {
            //if (DBNull.Value == feature["PRODCT"]) return null;
            var v = Convert.ToString(feature["PRODCT"]);
            //if ("-32767".Equals(v)) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>PRODCT</c> (Product) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>PRODCT</c> is neither <see cref="DBNull"/> nor <c>null</c> nor an empty string; otherwise <c>false</c>.</returns>
        public static bool PRODCT_HasValue(this Feature feature) => !(DBNull.Value == feature["PRODCT"]) && feature["PRODCT"] is not null && !string.IsNullOrEmpty(Convert.ToString(feature["PRODCT"]));

        /// <summary>
        /// Product_GUID
        /// </summary>
        /// <param name="feature">The feature to read the <c>PRODUCT_GUID</c> field from.</param>
        /// <returns>The value of <c>PRODUCT_GUID</c>, or <c>null</c> when it is not set.</returns>
        public static Guid? PRODUCT_GUID(this Feature feature) {
            if (DBNull.Value == feature["PRODUCT_GUID"]) return null;
            return Guid.TryParse(Convert.ToString(feature["PRODUCT_GUID"]), out var v) ? v : null;
        }

        /// <summary>
        /// Indicates whether <c>PRODUCT_GUID</c> (Product_GUID) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>PRODUCT_GUID</c> is neither <see cref="DBNull"/> nor <c>null</c>; otherwise <c>false</c>.</returns>
        public static bool PRODUCT_GUID_HasValue(this Feature feature) => !(DBNull.Value == feature["PRODUCT_GUID"]) && feature["PRODUCT_GUID"] is not null;

        /// <summary>
        /// Application Profile
        /// </summary>
        /// <param name="feature">The feature to read the <c>PROF</c> field from.</param>
        /// <returns>The value of <c>PROF</c>, or <c>null</c> when it is not set.</returns>
        public static int? PROF(this Feature feature) {
            if (DBNull.Value == feature["PROF"]) return null;
            var v = Convert.ToInt32(feature["PROF"]);
            //if (-32767 == v) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>PROF</c> (Application Profile) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>PROF</c> is neither <see cref="DBNull"/> nor <c>null</c>; otherwise <c>false</c>.</returns>
        public static bool PROF_HasValue(this Feature feature) => !(DBNull.Value == feature["PROF"]) && feature["PROF"] is not null;

        /// <summary>
        /// Product Specification
        /// </summary>
        /// <param name="feature">The feature to read the <c>PRSP</c> field from.</param>
        /// <returns>The value of <c>PRSP</c>, or <c>null</c> when it is not set.</returns>
        public static int? PRSP(this Feature feature) {
            if (DBNull.Value == feature["PRSP"]) return null;
            var v = Convert.ToInt32(feature["PRSP"]);
            //if (-32767 == v) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>PRSP</c> (Product Specification) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>PRSP</c> is neither <see cref="DBNull"/> nor <c>null</c>; otherwise <c>false</c>.</returns>
        public static bool PRSP_HasValue(this Feature feature) => !(DBNull.Value == feature["PRSP"]) && feature["PRSP"] is not null;

        /// <summary>
        /// Product Spec Description
        /// </summary>
        /// <param name="feature">The feature to read the <c>PSDN</c> field from.</param>
        /// <returns>The value of <c>PSDN</c>, or <c>null</c> when it is not set.</returns>
        public static string? PSDN(this Feature feature) {
            //if (DBNull.Value == feature["PSDN"]) return null;
            var v = Convert.ToString(feature["PSDN"]);
            //if ("-32767".Equals(v)) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>PSDN</c> (Product Spec Description) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>PSDN</c> is neither <see cref="DBNull"/> nor <c>null</c> nor an empty string; otherwise <c>false</c>.</returns>
        public static bool PSDN_HasValue(this Feature feature) => !(DBNull.Value == feature["PSDN"]) && feature["PSDN"] is not null && !string.IsNullOrEmpty(Convert.ToString(feature["PSDN"]));

        /// <summary>
        /// Publication reference
        /// </summary>
        /// <param name="feature">The feature to read the <c>PUBREF</c> field from.</param>
        /// <returns>The value of <c>PUBREF</c>, or <c>null</c> when it is not set.</returns>
        public static string? PUBREF(this Feature feature) {
            //if (DBNull.Value == feature["PUBREF"]) return null;
            var v = Convert.ToString(feature["PUBREF"]);
            //if ("-32767".Equals(v)) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>PUBREF</c> (Publication reference) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>PUBREF</c> is neither <see cref="DBNull"/> nor <c>null</c> nor an empty string; otherwise <c>false</c>.</returns>
        public static bool PUBREF_HasValue(this Feature feature) => !(DBNull.Value == feature["PUBREF"]) && feature["PUBREF"] is not null && !string.IsNullOrEmpty(Convert.ToString(feature["PUBREF"]));

        /// <summary>
        /// Precision Units
        /// </summary>
        /// <param name="feature">The feature to read the <c>PUNI</c> field from.</param>
        /// <returns>The value of <c>PUNI</c>, or <c>null</c> when it is not set.</returns>
        public static int? PUNI(this Feature feature) {
            if (DBNull.Value == feature["PUNI"]) return null;
            var v = Convert.ToInt32(feature["PUNI"]);
            //if (-32767 == v) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>PUNI</c> (Precision Units) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>PUNI</c> is neither <see cref="DBNull"/> nor <c>null</c>; otherwise <c>false</c>.</returns>
        public static bool PUNI_HasValue(this Feature feature) => !(DBNull.Value == feature["PUNI"]) && feature["PUNI"] is not null;

        /// <summary>
        /// Horizontal datum
        /// </summary>
        /// <param name="feature">The feature to read the <c>P_HORDAT</c> field from.</param>
        /// <returns>The value of <c>P_HORDAT</c>, or <c>null</c> when it is not set.</returns>
        public static int? P_HORDAT(this Feature feature) {
            if (DBNull.Value == feature["P_HORDAT"]) return null;
            var v = Convert.ToInt32(feature["P_HORDAT"]);
            //if (-32767 == v) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>P_HORDAT</c> (Horizontal datum) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>P_HORDAT</c> is neither <see cref="DBNull"/> nor <c>null</c>; otherwise <c>false</c>.</returns>
        public static bool P_HORDAT_HasValue(this Feature feature) => !(DBNull.Value == feature["P_HORDAT"]) && feature["P_HORDAT"] is not null;

        /// <summary>
        /// Positional Accuracy
        /// </summary>
        /// <param name="feature">The feature to read the <c>P_POSACC</c> field from.</param>
        /// <returns>The value of <c>P_POSACC</c>, or <c>null</c> when it is not set.</returns>
        public static decimal? P_POSACC(this Feature feature) {
            if (DBNull.Value == feature["P_POSACC"]) return null;
            var v = Convert.ToDecimal(feature["P_POSACC"]);
            //if (-32767m == v) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>P_POSACC</c> (Positional Accuracy) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>P_POSACC</c> is neither <see cref="DBNull"/> nor <c>null</c>; otherwise <c>false</c>.</returns>
        public static bool P_POSACC_HasValue(this Feature feature) => !(DBNull.Value == feature["P_POSACC"]) && feature["P_POSACC"] is not null;

        /// <summary>
        /// Quality of position
        /// </summary>
        /// <param name="feature">The feature to read the <c>P_QUAPOS</c> field from.</param>
        /// <returns>The value of <c>P_QUAPOS</c>, or <c>null</c> when it is not set.</returns>
        public static int? P_QUAPOS(this Feature feature) {
            if (DBNull.Value == feature["P_QUAPOS"]) return null;
            var v = Convert.ToInt32(feature["P_QUAPOS"]);
            //if (-32767 == v) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>P_QUAPOS</c> (Quality of position) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>P_QUAPOS</c> is neither <see cref="DBNull"/> nor <c>null</c>; otherwise <c>false</c>.</returns>
        public static bool P_QUAPOS_HasValue(this Feature feature) => !(DBNull.Value == feature["P_QUAPOS"]) && feature["P_QUAPOS"] is not null;

        /// <summary>
        /// Quality of position
        /// </summary>
        /// <param name="feature">The feature to read the <c>QUAPOS</c> field from.</param>
        /// <returns>The value of <c>QUAPOS</c>, or <c>null</c> when it is not set.</returns>
        public static int? QUAPOS(this Feature feature) {
            if (DBNull.Value == feature["QUAPOS"]) return null;
            var v = Convert.ToInt32(feature["QUAPOS"]);
            //if (-32767 == v) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>QUAPOS</c> (Quality of position) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>QUAPOS</c> is neither <see cref="DBNull"/> nor <c>null</c>; otherwise <c>false</c>.</returns>
        public static bool QUAPOS_HasValue(this Feature feature) => !(DBNull.Value == feature["QUAPOS"]) && feature["QUAPOS"] is not null;

        /// <summary>
        /// Quality of sounding measurement
        /// </summary>
        /// <param name="feature">The feature to read the <c>QUASOU</c> field from.</param>
        /// <returns>The value of <c>QUASOU</c>, or <c>null</c> when it is not set.</returns>
        public static string? QUASOU(this Feature feature) {
            //if (DBNull.Value == feature["QUASOU"]) return null;
            var v = Convert.ToString(feature["QUASOU"]);
            //if ("-32767".Equals(v)) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>QUASOU</c> (Quality of sounding measurement) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>QUASOU</c> is neither <see cref="DBNull"/> nor <c>null</c> nor an empty string; otherwise <c>false</c>.</returns>
        public static bool QUASOU_HasValue(this Feature feature) => !(DBNull.Value == feature["QUASOU"]) && feature["QUASOU"] is not null && !string.IsNullOrEmpty(Convert.ToString(feature["QUASOU"]));

        /// <summary>
        /// Radius
        /// </summary>
        /// <param name="feature">The feature to read the <c>RADIUS</c> field from.</param>
        /// <returns>The value of <c>RADIUS</c>, or <c>null</c> when it is not set.</returns>
        public static decimal? RADIUS(this Feature feature) {
            if (DBNull.Value == feature["RADIUS"]) return null;
            var v = Convert.ToDecimal(feature["RADIUS"]);
            //if (-32767m == v) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>RADIUS</c> (Radius) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>RADIUS</c> is neither <see cref="DBNull"/> nor <c>null</c>; otherwise <c>false</c>.</returns>
        public static bool RADIUS_HasValue(this Feature feature) => !(DBNull.Value == feature["RADIUS"]) && feature["RADIUS"] is not null;

        /// <summary>
        /// Radar wave length
        /// </summary>
        /// <param name="feature">The feature to read the <c>RADWAL</c> field from.</param>
        /// <returns>The value of <c>RADWAL</c>, or <c>null</c> when it is not set.</returns>
        public static string? RADWAL(this Feature feature) {
            //if (DBNull.Value == feature["RADWAL"]) return null;
            var v = Convert.ToString(feature["RADWAL"]);
            //if ("-32767".Equals(v)) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>RADWAL</c> (Radar wave length) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>RADWAL</c> is neither <see cref="DBNull"/> nor <c>null</c> nor an empty string; otherwise <c>false</c>.</returns>
        public static bool RADWAL_HasValue(this Feature feature) => !(DBNull.Value == feature["RADWAL"]) && feature["RADWAL"] is not null && !string.IsNullOrEmpty(Convert.ToString(feature["RADWAL"]));

        /// <summary>
        /// Restriction
        /// </summary>
        /// <param name="feature">The feature to read the <c>RESTRN</c> field from.</param>
        /// <returns>The value of <c>RESTRN</c>, or <c>null</c> when it is not set.</returns>
        public static string? RESTRN(this Feature feature) {
            //if (DBNull.Value == feature["RESTRN"]) return null;
            var v = Convert.ToString(feature["RESTRN"]);
            //if ("-32767".Equals(v)) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>RESTRN</c> (Restriction) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>RESTRN</c> is neither <see cref="DBNull"/> nor <c>null</c> nor an empty string; otherwise <c>false</c>.</returns>
        public static bool RESTRN_HasValue(this Feature feature) => !(DBNull.Value == feature["RESTRN"]) && feature["RESTRN"] is not null && !string.IsNullOrEmpty(Convert.ToString(feature["RESTRN"]));

        /// <summary>
        /// Relationship indicator
        /// </summary>
        /// <param name="feature">The feature to read the <c>RIND</c> field from.</param>
        /// <returns>The value of <c>RIND</c>, or <c>null</c> when it is not set.</returns>
        public static int? RIND(this Feature feature) {
            if (DBNull.Value == feature["RIND"]) return null;
            var v = Convert.ToInt32(feature["RIND"]);
            //if (-32767 == v) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>RIND</c> (Relationship indicator) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>RIND</c> is neither <see cref="DBNull"/> nor <c>null</c>; otherwise <c>false</c>.</returns>
        public static bool RIND_HasValue(this Feature feature) => !(DBNull.Value == feature["RIND"]) && feature["RIND"] is not null;

        /// <summary>
        /// Reference year for magnetic variation
        /// </summary>
        /// <param name="feature">The feature to read the <c>RYRMGV</c> field from.</param>
        /// <returns>The value of <c>RYRMGV</c>, or <c>null</c> when it is not set.</returns>
        public static string? RYRMGV(this Feature feature) {
            //if (DBNull.Value == feature["RYRMGV"]) return null;
            var v = Convert.ToString(feature["RYRMGV"]);
            //if ("-32767".Equals(v)) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>RYRMGV</c> (Reference year for magnetic variation) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>RYRMGV</c> is neither <see cref="DBNull"/> nor <c>null</c> nor an empty string; otherwise <c>false</c>.</returns>
        public static bool RYRMGV_HasValue(this Feature feature) => !(DBNull.Value == feature["RYRMGV"]) && feature["RYRMGV"] is not null && !string.IsNullOrEmpty(Convert.ToString(feature["RYRMGV"]));

        /// <summary>
        /// Scale value one
        /// </summary>
        /// <param name="feature">The feature to read the <c>SCVAL1</c> field from.</param>
        /// <returns>The value of <c>SCVAL1</c>, or <c>null</c> when it is not set.</returns>
        public static int? SCVAL1(this Feature feature) {
            if (DBNull.Value == feature["SCVAL1"]) return null;
            var v = Convert.ToInt32(feature["SCVAL1"]);
            //if (-32767 == v) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>SCVAL1</c> (Scale value one) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>SCVAL1</c> is neither <see cref="DBNull"/> nor <c>null</c>; otherwise <c>false</c>.</returns>
        public static bool SCVAL1_HasValue(this Feature feature) => !(DBNull.Value == feature["SCVAL1"]) && feature["SCVAL1"] is not null;

        /// <summary>
        /// Scale value two
        /// </summary>
        /// <param name="feature">The feature to read the <c>SCVAL2</c> field from.</param>
        /// <returns>The value of <c>SCVAL2</c>, or <c>null</c> when it is not set.</returns>
        public static int? SCVAL2(this Feature feature) {
            if (DBNull.Value == feature["SCVAL2"]) return null;
            var v = Convert.ToInt32(feature["SCVAL2"]);
            //if (-32767 == v) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>SCVAL2</c> (Scale value two) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>SCVAL2</c> is neither <see cref="DBNull"/> nor <c>null</c>; otherwise <c>false</c>.</returns>
        public static bool SCVAL2_HasValue(this Feature feature) => !(DBNull.Value == feature["SCVAL2"]) && feature["SCVAL2"] is not null;

        /// <summary>
        /// Sounding Datum
        /// </summary>
        /// <param name="feature">The feature to read the <c>SDAT</c> field from.</param>
        /// <returns>The value of <c>SDAT</c>, or <c>null</c> when it is not set.</returns>
        public static int? SDAT(this Feature feature) {
            if (DBNull.Value == feature["SDAT"]) return null;
            var v = Convert.ToInt32(feature["SDAT"]);
            //if (-32767 == v) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>SDAT</c> (Sounding Datum) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>SDAT</c> is neither <see cref="DBNull"/> nor <c>null</c>; otherwise <c>false</c>.</returns>
        public static bool SDAT_HasValue(this Feature feature) => !(DBNull.Value == feature["SDAT"]) && feature["SDAT"] is not null;

        /// <summary>
        /// Sounding distance - minimum
        /// </summary>
        /// <param name="feature">The feature to read the <c>SDISMN</c> field from.</param>
        /// <returns>The value of <c>SDISMN</c>, or <c>null</c> when it is not set.</returns>
        public static decimal? SDISMN(this Feature feature) {
            if (DBNull.Value == feature["SDISMN"]) return null;
            var v = Convert.ToDecimal(feature["SDISMN"]);
            //if (-32767m == v) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>SDISMN</c> (Sounding distance - minimum) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>SDISMN</c> is neither <see cref="DBNull"/> nor <c>null</c>; otherwise <c>false</c>.</returns>
        public static bool SDISMN_HasValue(this Feature feature) => !(DBNull.Value == feature["SDISMN"]) && feature["SDISMN"] is not null;

        /// <summary>
        /// Sounding distance - maximum
        /// </summary>
        /// <param name="feature">The feature to read the <c>SDISMX</c> field from.</param>
        /// <returns>The value of <c>SDISMX</c>, or <c>null</c> when it is not set.</returns>
        public static decimal? SDISMX(this Feature feature) {
            if (DBNull.Value == feature["SDISMX"]) return null;
            var v = Convert.ToDecimal(feature["SDISMX"]);
            //if (-32767m == v) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>SDISMX</c> (Sounding distance - maximum) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>SDISMX</c> is neither <see cref="DBNull"/> nor <c>null</c>; otherwise <c>false</c>.</returns>
        public static bool SDISMX_HasValue(this Feature feature) => !(DBNull.Value == feature["SDISMX"]) && feature["SDISMX"] is not null;

        /// <summary>
        /// Sector limit one
        /// </summary>
        /// <param name="feature">The feature to read the <c>SECTR1</c> field from.</param>
        /// <returns>The value of <c>SECTR1</c>, or <c>null</c> when it is not set.</returns>
        public static decimal? SECTR1(this Feature feature) {
            if (DBNull.Value == feature["SECTR1"]) return null;
            var v = Convert.ToDecimal(feature["SECTR1"]);
            //if (-32767m == v) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>SECTR1</c> (Sector limit one) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>SECTR1</c> is neither <see cref="DBNull"/> nor <c>null</c>; otherwise <c>false</c>.</returns>
        public static bool SECTR1_HasValue(this Feature feature) => !(DBNull.Value == feature["SECTR1"]) && feature["SECTR1"] is not null;

        /// <summary>
        /// Sector limit two
        /// </summary>
        /// <param name="feature">The feature to read the <c>SECTR2</c> field from.</param>
        /// <returns>The value of <c>SECTR2</c>, or <c>null</c> when it is not set.</returns>
        public static decimal? SECTR2(this Feature feature) {
            if (DBNull.Value == feature["SECTR2"]) return null;
            var v = Convert.ToDecimal(feature["SECTR2"]);
            //if (-32767m == v) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>SECTR2</c> (Sector limit two) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>SECTR2</c> is neither <see cref="DBNull"/> nor <c>null</c>; otherwise <c>false</c>.</returns>
        public static bool SECTR2_HasValue(this Feature feature) => !(DBNull.Value == feature["SECTR2"]) && feature["SECTR2"] is not null;

        /// <summary>
        /// Series
        /// </summary>
        /// <param name="feature">The feature to read the <c>SERIES</c> field from.</param>
        /// <returns>The value of <c>SERIES</c>, or <c>null</c> when it is not set.</returns>
        public static string? SERIES(this Feature feature) {
            //if (DBNull.Value == feature["SERIES"]) return null;
            var v = Convert.ToString(feature["SERIES"]);
            //if ("-32767".Equals(v)) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>SERIES</c> (Series) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>SERIES</c> is neither <see cref="DBNull"/> nor <c>null</c> nor an empty string; otherwise <c>false</c>.</returns>
        public static bool SERIES_HasValue(this Feature feature) => !(DBNull.Value == feature["SERIES"]) && feature["SERIES"] is not null && !string.IsNullOrEmpty(Convert.ToString(feature["SERIES"]));

        /// <summary>
        /// Shape
        /// </summary>
        /// <param name="feature">The feature to read the <c>SHAPE</c> field from.</param>
        /// <returns>The value of <c>SHAPE</c>, or <c>null</c> when it is not set.</returns>
        public static Geometry? SHAPE(this Feature feature) {
            if (DBNull.Value == feature["SHAPE"]) return null;
            return feature["SHAPE"] as Geometry;
        }

        /// <summary>
        /// Indicates whether <c>SHAPE</c> (Shape) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>SHAPE</c> is neither <see cref="DBNull"/> nor <c>null</c>; otherwise <c>false</c>.</returns>
        public static bool SHAPE_HasValue(this Feature feature) => !(DBNull.Value == feature["SHAPE"]) && feature["SHAPE"] is not null;

        /// <summary>
        /// Shift parameters
        /// </summary>
        /// <param name="feature">The feature to read the <c>SHIPAM</c> field from.</param>
        /// <returns>The value of <c>SHIPAM</c>, or <c>null</c> when it is not set.</returns>
        public static string? SHIPAM(this Feature feature) {
            //if (DBNull.Value == feature["SHIPAM"]) return null;
            var v = Convert.ToString(feature["SHIPAM"]);
            //if ("-32767".Equals(v)) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>SHIPAM</c> (Shift parameters) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>SHIPAM</c> is neither <see cref="DBNull"/> nor <c>null</c> nor an empty string; otherwise <c>false</c>.</returns>
        public static bool SHIPAM_HasValue(this Feature feature) => !(DBNull.Value == feature["SHIPAM"]) && feature["SHIPAM"] is not null && !string.IsNullOrEmpty(Convert.ToString(feature["SHIPAM"]));

        /// <summary>
        /// Signal frequency
        /// </summary>
        /// <param name="feature">The feature to read the <c>SIGFRQ</c> field from.</param>
        /// <returns>The value of <c>SIGFRQ</c>, or <c>null</c> when it is not set.</returns>
        public static int? SIGFRQ(this Feature feature) {
            if (DBNull.Value == feature["SIGFRQ"]) return null;
            var v = Convert.ToInt32(feature["SIGFRQ"]);
            //if (-32767 == v) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>SIGFRQ</c> (Signal frequency) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>SIGFRQ</c> is neither <see cref="DBNull"/> nor <c>null</c>; otherwise <c>false</c>.</returns>
        public static bool SIGFRQ_HasValue(this Feature feature) => !(DBNull.Value == feature["SIGFRQ"]) && feature["SIGFRQ"] is not null;

        /// <summary>
        /// Signal generation
        /// </summary>
        /// <param name="feature">The feature to read the <c>SIGGEN</c> field from.</param>
        /// <returns>The value of <c>SIGGEN</c>, or <c>null</c> when it is not set.</returns>
        public static int? SIGGEN(this Feature feature) {
            if (DBNull.Value == feature["SIGGEN"]) return null;
            var v = Convert.ToInt32(feature["SIGGEN"]);
            //if (-32767 == v) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>SIGGEN</c> (Signal generation) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>SIGGEN</c> is neither <see cref="DBNull"/> nor <c>null</c>; otherwise <c>false</c>.</returns>
        public static bool SIGGEN_HasValue(this Feature feature) => !(DBNull.Value == feature["SIGGEN"]) && feature["SIGGEN"] is not null;

        /// <summary>
        /// Signal group
        /// </summary>
        /// <param name="feature">The feature to read the <c>SIGGRP</c> field from.</param>
        /// <returns>The value of <c>SIGGRP</c>, or <c>null</c> when it is not set.</returns>
        public static string? SIGGRP(this Feature feature) {
            //if (DBNull.Value == feature["SIGGRP"]) return null;
            var v = Convert.ToString(feature["SIGGRP"]);
            //if ("-32767".Equals(v)) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>SIGGRP</c> (Signal group) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>SIGGRP</c> is neither <see cref="DBNull"/> nor <c>null</c> nor an empty string; otherwise <c>false</c>.</returns>
        public static bool SIGGRP_HasValue(this Feature feature) => !(DBNull.Value == feature["SIGGRP"]) && feature["SIGGRP"] is not null && !string.IsNullOrEmpty(Convert.ToString(feature["SIGGRP"]));

        /// <summary>
        /// Signal period
        /// </summary>
        /// <param name="feature">The feature to read the <c>SIGPER</c> field from.</param>
        /// <returns>The value of <c>SIGPER</c>, or <c>null</c> when it is not set.</returns>
        public static decimal? SIGPER(this Feature feature) {
            if (DBNull.Value == feature["SIGPER"]) return null;
            var v = Convert.ToDecimal(feature["SIGPER"]);
            //if (-32767m == v) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>SIGPER</c> (Signal period) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>SIGPER</c> is neither <see cref="DBNull"/> nor <c>null</c>; otherwise <c>false</c>.</returns>
        public static bool SIGPER_HasValue(this Feature feature) => !(DBNull.Value == feature["SIGPER"]) && feature["SIGPER"] is not null;

        /// <summary>
        /// Signal sequence
        /// </summary>
        /// <param name="feature">The feature to read the <c>SIGSEQ</c> field from.</param>
        /// <returns>The value of <c>SIGSEQ</c>, or <c>null</c> when it is not set.</returns>
        public static string? SIGSEQ(this Feature feature) {
            //if (DBNull.Value == feature["SIGSEQ"]) return null;
            var v = Convert.ToString(feature["SIGSEQ"]);
            //if ("-32767".Equals(v)) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>SIGSEQ</c> (Signal sequence) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>SIGSEQ</c> is neither <see cref="DBNull"/> nor <c>null</c> nor an empty string; otherwise <c>false</c>.</returns>
        public static bool SIGSEQ_HasValue(this Feature feature) => !(DBNull.Value == feature["SIGSEQ"]) && feature["SIGSEQ"] is not null && !string.IsNullOrEmpty(Convert.ToString(feature["SIGSEQ"]));

        /// <summary>
        /// Sounding Multiplication Factor
        /// </summary>
        /// <param name="feature">The feature to read the <c>SOMF</c> field from.</param>
        /// <returns>The value of <c>SOMF</c>, or <c>null</c> when it is not set.</returns>
        public static int? SOMF(this Feature feature) {
            if (DBNull.Value == feature["SOMF"]) return null;
            var v = Convert.ToInt32(feature["SOMF"]);
            //if (-32767 == v) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>SOMF</c> (Sounding Multiplication Factor) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>SOMF</c> is neither <see cref="DBNull"/> nor <c>null</c>; otherwise <c>false</c>.</returns>
        public static bool SOMF_HasValue(this Feature feature) => !(DBNull.Value == feature["SOMF"]) && feature["SOMF"] is not null;

        /// <summary>
        /// Source date
        /// </summary>
        /// <param name="feature">The feature to read the <c>SORDAT</c> field from.</param>
        /// <returns>The value of <c>SORDAT</c>, or <c>null</c> when it is not set.</returns>
        public static string? SORDAT(this Feature feature) {
            //if (DBNull.Value == feature["SORDAT"]) return null;
            var v = Convert.ToString(feature["SORDAT"]);
            //if ("-32767".Equals(v)) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>SORDAT</c> (Source date) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>SORDAT</c> is neither <see cref="DBNull"/> nor <c>null</c> nor an empty string; otherwise <c>false</c>.</returns>
        public static bool SORDAT_HasValue(this Feature feature) => !(DBNull.Value == feature["SORDAT"]) && feature["SORDAT"] is not null && !string.IsNullOrEmpty(Convert.ToString(feature["SORDAT"]));

        /// <summary>
        /// Source indication
        /// </summary>
        /// <param name="feature">The feature to read the <c>SORIND</c> field from.</param>
        /// <returns>The value of <c>SORIND</c>, or <c>null</c> when it is not set.</returns>
        public static string? SORIND(this Feature feature) {
            //if (DBNull.Value == feature["SORIND"]) return null;
            var v = Convert.ToString(feature["SORIND"]);
            //if ("-32767".Equals(v)) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>SORIND</c> (Source indication) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>SORIND</c> is neither <see cref="DBNull"/> nor <c>null</c> nor an empty string; otherwise <c>false</c>.</returns>
        public static bool SORIND_HasValue(this Feature feature) => !(DBNull.Value == feature["SORIND"]) && feature["SORIND"] is not null && !string.IsNullOrEmpty(Convert.ToString(feature["SORIND"]));

        /// <summary>
        /// Sounding accuracy
        /// </summary>
        /// <param name="feature">The feature to read the <c>SOUACC</c> field from.</param>
        /// <returns>The value of <c>SOUACC</c>, or <c>null</c> when it is not set.</returns>
        public static decimal? SOUACC(this Feature feature) {
            if (DBNull.Value == feature["SOUACC"]) return null;
            var v = Convert.ToDecimal(feature["SOUACC"]);
            //if (-32767m == v) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>SOUACC</c> (Sounding accuracy) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>SOUACC</c> is neither <see cref="DBNull"/> nor <c>null</c>; otherwise <c>false</c>.</returns>
        public static bool SOUACC_HasValue(this Feature feature) => !(DBNull.Value == feature["SOUACC"]) && feature["SOUACC"] is not null;

        /// <summary>
        /// Source feature class
        /// </summary>
        /// <param name="feature">The feature to read the <c>SRC_FC</c> field from.</param>
        /// <returns>The value of <c>SRC_FC</c>, or <c>null</c> when it is not set.</returns>
        public static string? SRC_FC(this Feature feature) {
            //if (DBNull.Value == feature["SRC_FC"]) return null;
            var v = Convert.ToString(feature["SRC_FC"]);
            //if ("-32767".Equals(v)) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>SRC_FC</c> (Source feature class) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>SRC_FC</c> is neither <see cref="DBNull"/> nor <c>null</c> nor an empty string; otherwise <c>false</c>.</returns>
        public static bool SRC_FC_HasValue(this Feature feature) => !(DBNull.Value == feature["SRC_FC"]) && feature["SRC_FC"] is not null && !string.IsNullOrEmpty(Convert.ToString(feature["SRC_FC"]));

        /// <summary>
        /// Source long name
        /// </summary>
        /// <param name="feature">The feature to read the <c>SRC_LNAM</c> field from.</param>
        /// <returns>The value of <c>SRC_LNAM</c>, or <c>null</c> when it is not set.</returns>
        public static string? SRC_LNAM(this Feature feature) {
            //if (DBNull.Value == feature["SRC_LNAM"]) return null;
            var v = Convert.ToString(feature["SRC_LNAM"]);
            //if ("-32767".Equals(v)) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>SRC_LNAM</c> (Source long name) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>SRC_LNAM</c> is neither <see cref="DBNull"/> nor <c>null</c> nor an empty string; otherwise <c>false</c>.</returns>
        public static bool SRC_LNAM_HasValue(this Feature feature) => !(DBNull.Value == feature["SRC_LNAM"]) && feature["SRC_LNAM"] is not null && !string.IsNullOrEmpty(Convert.ToString(feature["SRC_LNAM"]));

        /// <summary>
        /// Source subtype
        /// </summary>
        /// <param name="feature">The feature to read the <c>SRC_SUB</c> field from.</param>
        /// <returns>The value of <c>SRC_SUB</c>, or <c>null</c> when it is not set.</returns>
        public static string? SRC_SUB(this Feature feature) {
            //if (DBNull.Value == feature["SRC_SUB"]) return null;
            var v = Convert.ToString(feature["SRC_SUB"]);
            //if ("-32767".Equals(v)) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>SRC_SUB</c> (Source subtype) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>SRC_SUB</c> is neither <see cref="DBNull"/> nor <c>null</c> nor an empty string; otherwise <c>false</c>.</returns>
        public static bool SRC_SUB_HasValue(this Feature feature) => !(DBNull.Value == feature["SRC_SUB"]) && feature["SRC_SUB"] is not null && !string.IsNullOrEmpty(Convert.ToString(feature["SRC_SUB"]));

        /// <summary>
        /// Source universal ID
        /// </summary>
        /// <param name="feature">The feature to read the <c>SRC_UID</c> field from.</param>
        /// <returns>The value of <c>SRC_UID</c>, or <c>null</c> when it is not set.</returns>
        public static string? SRC_UID(this Feature feature) {
            //if (DBNull.Value == feature["SRC_UID"]) return null;
            var v = Convert.ToString(feature["SRC_UID"]);
            //if ("-32767".Equals(v)) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>SRC_UID</c> (Source universal ID) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>SRC_UID</c> is neither <see cref="DBNull"/> nor <c>null</c> nor an empty string; otherwise <c>false</c>.</returns>
        public static bool SRC_UID_HasValue(this Feature feature) => !(DBNull.Value == feature["SRC_UID"]) && feature["SRC_UID"] is not null && !string.IsNullOrEmpty(Convert.ToString(feature["SRC_UID"]));

        /// <summary>
        /// Status
        /// </summary>
        /// <param name="feature">The feature to read the <c>STATUS</c> field from.</param>
        /// <returns>The value of <c>STATUS</c>, or <c>null</c> when it is not set.</returns>
        public static string? STATUS(this Feature feature) {
            //if (DBNull.Value == feature["STATUS"]) return null;
            var v = Convert.ToString(feature["STATUS"]);
            //if ("-32767".Equals(v)) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>STATUS</c> (Status) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>STATUS</c> is neither <see cref="DBNull"/> nor <c>null</c> nor an empty string; otherwise <c>false</c>.</returns>
        public static bool STATUS_HasValue(this Feature feature) => !(DBNull.Value == feature["STATUS"]) && feature["STATUS"] is not null && !string.IsNullOrEmpty(Convert.ToString(feature["STATUS"]));

        /// <summary>
        /// S-57 Edition
        /// </summary>
        /// <param name="feature">The feature to read the <c>STED</c> field from.</param>
        /// <returns>The value of <c>STED</c>, or <c>null</c> when it is not set.</returns>
        public static string? STED(this Feature feature) {
            //if (DBNull.Value == feature["STED"]) return null;
            var v = Convert.ToString(feature["STED"]);
            //if ("-32767".Equals(v)) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>STED</c> (S-57 Edition) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>STED</c> is neither <see cref="DBNull"/> nor <c>null</c> nor an empty string; otherwise <c>false</c>.</returns>
        public static bool STED_HasValue(this Feature feature) => !(DBNull.Value == feature["STED"]) && feature["STED"] is not null && !string.IsNullOrEmpty(Convert.ToString(feature["STED"]));

        /// <summary>
        /// Survey authority
        /// </summary>
        /// <param name="feature">The feature to read the <c>SURATH</c> field from.</param>
        /// <returns>The value of <c>SURATH</c>, or <c>null</c> when it is not set.</returns>
        public static string? SURATH(this Feature feature) {
            //if (DBNull.Value == feature["SURATH"]) return null;
            var v = Convert.ToString(feature["SURATH"]);
            //if ("-32767".Equals(v)) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>SURATH</c> (Survey authority) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>SURATH</c> is neither <see cref="DBNull"/> nor <c>null</c> nor an empty string; otherwise <c>false</c>.</returns>
        public static bool SURATH_HasValue(this Feature feature) => !(DBNull.Value == feature["SURATH"]) && feature["SURATH"] is not null && !string.IsNullOrEmpty(Convert.ToString(feature["SURATH"]));

        /// <summary>
        /// Survey date - end
        /// </summary>
        /// <param name="feature">The feature to read the <c>SUREND</c> field from.</param>
        /// <returns>The value of <c>SUREND</c>, or <c>null</c> when it is not set.</returns>
        public static string? SUREND(this Feature feature) {
            //if (DBNull.Value == feature["SUREND"]) return null;
            var v = Convert.ToString(feature["SUREND"]);
            //if ("-32767".Equals(v)) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>SUREND</c> (Survey date - end) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>SUREND</c> is neither <see cref="DBNull"/> nor <c>null</c> nor an empty string; otherwise <c>false</c>.</returns>
        public static bool SUREND_HasValue(this Feature feature) => !(DBNull.Value == feature["SUREND"]) && feature["SUREND"] is not null && !string.IsNullOrEmpty(Convert.ToString(feature["SUREND"]));

        /// <summary>
        /// Survey date - start
        /// </summary>
        /// <param name="feature">The feature to read the <c>SURSTA</c> field from.</param>
        /// <returns>The value of <c>SURSTA</c>, or <c>null</c> when it is not set.</returns>
        public static string? SURSTA(this Feature feature) {
            if (DBNull.Value == feature["SURSTA"]) return null;
            var v = Convert.ToString(feature["SURSTA"]);
            //if ("-32767".Equals(v)) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>SURSTA</c> (Survey date - start) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>SURSTA</c> is neither <see cref="DBNull"/> nor <c>null</c> nor an empty string; otherwise <c>false</c>.</returns>
        public static bool SURSTA_HasValue(this Feature feature) => !(DBNull.Value == feature["SURSTA"]) && feature["SURSTA"] is not null && !string.IsNullOrEmpty(Convert.ToString(feature["SURSTA"]));

        /// <summary>
        /// Survey type
        /// </summary>
        /// <param name="feature">The feature to read the <c>SURTYP</c> field from.</param>
        /// <returns>The value of <c>SURTYP</c>, or <c>null</c> when it is not set.</returns>
        public static string? SURTYP(this Feature feature) {
            if (DBNull.Value == feature["SURTYP"]) return null;
            var v = Convert.ToString(feature["SURTYP"]);
            //if ("-32767".Equals(v)) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>SURTYP</c> (Survey type) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>SURTYP</c> is neither <see cref="DBNull"/> nor <c>null</c> nor an empty string; otherwise <c>false</c>.</returns>
        public static bool SURTYP_HasValue(this Feature feature) => !(DBNull.Value == feature["SURTYP"]) && feature["SURTYP"] is not null && !string.IsNullOrEmpty(Convert.ToString(feature["SURTYP"]));

        /// <summary>
        /// Symbol Instruction
        /// </summary>
        /// <param name="feature">The feature to read the <c>SYMINS</c> field from.</param>
        /// <returns>The value of <c>SYMINS</c>, or <c>null</c> when it is not set.</returns>
        public static string? SYMINS(this Feature feature) {
            if (DBNull.Value == feature["SYMINS"]) return null;
            var v = Convert.ToString(feature["SYMINS"]);
            //if ("-32767".Equals(v)) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>SYMINS</c> (Symbol Instruction) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>SYMINS</c> is neither <see cref="DBNull"/> nor <c>null</c> nor an empty string; otherwise <c>false</c>.</returns>
        public static bool SYMINS_HasValue(this Feature feature) => !(DBNull.Value == feature["SYMINS"]) && feature["SYMINS"] is not null && !string.IsNullOrEmpty(Convert.ToString(feature["SYMINS"]));

        /// <summary>
        /// Technique of sounding measurement
        /// </summary>
        /// <param name="feature">The feature to read the <c>TECSOU</c> field from.</param>
        /// <returns>The value of <c>TECSOU</c>, or <c>null</c> when it is not set.</returns>
        public static string? TECSOU(this Feature feature) {
            if (DBNull.Value == feature["TECSOU"]) return null;
            var v = Convert.ToString(feature["TECSOU"]);
            //if ("-32767".Equals(v)) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>TECSOU</c> (Technique of sounding measurement) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>TECSOU</c> is neither <see cref="DBNull"/> nor <c>null</c> nor an empty string; otherwise <c>false</c>.</returns>
        public static bool TECSOU_HasValue(this Feature feature) => !(DBNull.Value == feature["TECSOU"]) && feature["TECSOU"] is not null && !string.IsNullOrEmpty(Convert.ToString(feature["TECSOU"]));

        /// <summary>
        /// Time end
        /// </summary>
        /// <param name="feature">The feature to read the <c>TIMEND</c> field from.</param>
        /// <returns>The value of <c>TIMEND</c>, or <c>null</c> when it is not set.</returns>
        public static string? TIMEND(this Feature feature) {
            if (DBNull.Value == feature["TIMEND"]) return null;
            var v = Convert.ToString(feature["TIMEND"]);
            //if ("-32767".Equals(v)) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>TIMEND</c> (Time end) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>TIMEND</c> is neither <see cref="DBNull"/> nor <c>null</c> nor an empty string; otherwise <c>false</c>.</returns>
        public static bool TIMEND_HasValue(this Feature feature) => !(DBNull.Value == feature["TIMEND"]) && feature["TIMEND"] is not null && !string.IsNullOrEmpty(Convert.ToString(feature["TIMEND"]));

        /// <summary>
        /// Time start
        /// </summary>
        /// <param name="feature">The feature to read the <c>TIMSTA</c> field from.</param>
        /// <returns>The value of <c>TIMSTA</c>, or <c>null</c> when it is not set.</returns>
        public static string? TIMSTA(this Feature feature) {
            if (DBNull.Value == feature["TIMSTA"]) return null;
            var v = Convert.ToString(feature["TIMSTA"]);
            //if ("-32767".Equals(v)) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>TIMSTA</c> (Time start) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>TIMSTA</c> is neither <see cref="DBNull"/> nor <c>null</c> nor an empty string; otherwise <c>false</c>.</returns>
        public static bool TIMSTA_HasValue(this Feature feature) => !(DBNull.Value == feature["TIMSTA"]) && feature["TIMSTA"] is not null && !string.IsNullOrEmpty(Convert.ToString(feature["TIMSTA"]));

        /// <summary>
        /// Topmark/daymark shape
        /// </summary>
        /// <param name="feature">The feature to read the <c>TOPSHP</c> field from.</param>
        /// <returns>The value of <c>TOPSHP</c>, or <c>null</c> when it is not set.</returns>
        public static int? TOPSHP(this Feature feature) {
            if (DBNull.Value == feature["TOPSHP"]) return null;
            var v = Convert.ToInt32(feature["TOPSHP"]);
            //if (-32767 == v) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>TOPSHP</c> (Topmark/daymark shape) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>TOPSHP</c> is neither <see cref="DBNull"/> nor <c>null</c>; otherwise <c>false</c>.</returns>
        public static bool TOPSHP_HasValue(this Feature feature) => !(DBNull.Value == feature["TOPSHP"]) && feature["TOPSHP"] is not null;

        /// <summary>
        /// Traffic flow
        /// </summary>
        /// <param name="feature">The feature to read the <c>TRAFIC</c> field from.</param>
        /// <returns>The value of <c>TRAFIC</c>, or <c>null</c> when it is not set.</returns>
        public static int? TRAFIC(this Feature feature) {
            if (DBNull.Value == feature["TRAFIC"]) return null;
            var v = Convert.ToInt32(feature["TRAFIC"]);
            //if (-32767 == v) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>TRAFIC</c> (Traffic flow) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>TRAFIC</c> is neither <see cref="DBNull"/> nor <c>null</c>; otherwise <c>false</c>.</returns>
        public static bool TRAFIC_HasValue(this Feature feature) => !(DBNull.Value == feature["TRAFIC"]) && feature["TRAFIC"] is not null;

        /// <summary>
        /// Tidal stream - panel values
        /// </summary>
        /// <param name="feature">The feature to read the <c>TS_TSP</c> field from.</param>
        /// <returns>The value of <c>TS_TSP</c>, or <c>null</c> when it is not set.</returns>
        public static string? TS_TSP(this Feature feature) {
            if (DBNull.Value == feature["TS_TSP"]) return null;
            var v = Convert.ToString(feature["TS_TSP"]);
            //if ("-32767".Equals(v)) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>TS_TSP</c> (Tidal stream - panel values) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>TS_TSP</c> is neither <see cref="DBNull"/> nor <c>null</c> nor an empty string; otherwise <c>false</c>.</returns>
        public static bool TS_TSP_HasValue(this Feature feature) => !(DBNull.Value == feature["TS_TSP"]) && feature["TS_TSP"] is not null && !string.IsNullOrEmpty(Convert.ToString(feature["TS_TSP"]));

        /// <summary>
        /// Tidal stream - time series values
        /// </summary>
        /// <param name="feature">The feature to read the <c>TS_TSV</c> field from.</param>
        /// <returns>The value of <c>TS_TSV</c>, or <c>null</c> when it is not set.</returns>
        public static string? TS_TSV(this Feature feature) {
            if (DBNull.Value == feature["TS_TSV"]) return null;
            var v = Convert.ToString(feature["TS_TSV"]);
            //if ("-32767".Equals(v)) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>TS_TSV</c> (Tidal stream - time series values) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>TS_TSV</c> is neither <see cref="DBNull"/> nor <c>null</c> nor an empty string; otherwise <c>false</c>.</returns>
        public static bool TS_TSV_HasValue(this Feature feature) => !(DBNull.Value == feature["TS_TSV"]) && feature["TS_TSV"] is not null && !string.IsNullOrEmpty(Convert.ToString(feature["TS_TSV"]));

        /// <summary>
        /// Textual description
        /// </summary>
        /// <param name="feature">The feature to read the <c>TXTDSC</c> field from.</param>
        /// <returns>The value of <c>TXTDSC</c>, or <c>null</c> when it is not set.</returns>
        public static string? TXTDSC(this Feature feature) {
            if (DBNull.Value == feature["TXTDSC"]) return null;
            var v = Convert.ToString(feature["TXTDSC"]);
            //if ("-32767".Equals(v)) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>TXTDSC</c> (Textual description) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>TXTDSC</c> is neither <see cref="DBNull"/> nor <c>null</c> nor an empty string; otherwise <c>false</c>.</returns>
        public static bool TXTDSC_HasValue(this Feature feature) => !(DBNull.Value == feature["TXTDSC"]) && feature["TXTDSC"] is not null && !string.IsNullOrEmpty(Convert.ToString(feature["TXTDSC"]));

        /// <summary>
        /// Tide - accuracy of water level
        /// </summary>
        /// <param name="feature">The feature to read the <c>T_ACWL</c> field from.</param>
        /// <returns>The value of <c>T_ACWL</c>, or <c>null</c> when it is not set.</returns>
        public static int? T_ACWL(this Feature feature) {
            if (DBNull.Value == feature["T_ACWL"]) return null;
            var v = Convert.ToInt32(feature["T_ACWL"]);
            //if (-32767 == v) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>T_ACWL</c> (Tide - accuracy of water level) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>T_ACWL</c> is neither <see cref="DBNull"/> nor <c>null</c>; otherwise <c>false</c>.</returns>
        public static bool T_ACWL_HasValue(this Feature feature) => !(DBNull.Value == feature["T_ACWL"]) && feature["T_ACWL"] is not null;

        /// <summary>
        /// Tide - high and low water values
        /// </summary>
        /// <param name="feature">The feature to read the <c>T_HWLW</c> field from.</param>
        /// <returns>The value of <c>T_HWLW</c>, or <c>null</c> when it is not set.</returns>
        public static string? T_HWLW(this Feature feature) {
            if (DBNull.Value == feature["T_HWLW"]) return null;
            var v = Convert.ToString(feature["T_HWLW"]);
            //if ("-32767".Equals(v)) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>T_HWLW</c> (Tide - high and low water values) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>T_HWLW</c> is neither <see cref="DBNull"/> nor <c>null</c> nor an empty string; otherwise <c>false</c>.</returns>
        public static bool T_HWLW_HasValue(this Feature feature) => !(DBNull.Value == feature["T_HWLW"]) && feature["T_HWLW"] is not null && !string.IsNullOrEmpty(Convert.ToString(feature["T_HWLW"]));

        /// <summary>
        /// Tide - method of tidal prediction
        /// </summary>
        /// <param name="feature">The feature to read the <c>T_MTOD</c> field from.</param>
        /// <returns>The value of <c>T_MTOD</c>, or <c>null</c> when it is not set.</returns>
        public static int? T_MTOD(this Feature feature) {
            if (DBNull.Value == feature["T_MTOD"]) return null;
            var v = Convert.ToInt32(feature["T_MTOD"]);
            //if (-32767 == v) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>T_MTOD</c> (Tide - method of tidal prediction) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>T_MTOD</c> is neither <see cref="DBNull"/> nor <c>null</c>; otherwise <c>false</c>.</returns>
        public static bool T_MTOD_HasValue(this Feature feature) => !(DBNull.Value == feature["T_MTOD"]) && feature["T_MTOD"] is not null;

        /// <summary>
        /// Tide - time and height differences
        /// </summary>
        /// <param name="feature">The feature to read the <c>T_THDF</c> field from.</param>
        /// <returns>The value of <c>T_THDF</c>, or <c>null</c> when it is not set.</returns>
        public static string? T_THDF(this Feature feature) {
            if (DBNull.Value == feature["T_THDF"]) return null;
            var v = Convert.ToString(feature["T_THDF"]);
            //if ("-32767".Equals(v)) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>T_THDF</c> (Tide - time and height differences) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>T_THDF</c> is neither <see cref="DBNull"/> nor <c>null</c> nor an empty string; otherwise <c>false</c>.</returns>
        public static bool T_THDF_HasValue(this Feature feature) => !(DBNull.Value == feature["T_THDF"]) && feature["T_THDF"] is not null && !string.IsNullOrEmpty(Convert.ToString(feature["T_THDF"]));

        /// <summary>
        /// Tide - time interval of values
        /// </summary>
        /// <param name="feature">The feature to read the <c>T_TINT</c> field from.</param>
        /// <returns>The value of <c>T_TINT</c>, or <c>null</c> when it is not set.</returns>
        public static int? T_TINT(this Feature feature) {
            if (DBNull.Value == feature["T_TINT"]) return null;
            var v = Convert.ToInt32(feature["T_TINT"]);
            //if (-32767 == v) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>T_TINT</c> (Tide - time interval of values) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>T_TINT</c> is neither <see cref="DBNull"/> nor <c>null</c>; otherwise <c>false</c>.</returns>
        public static bool T_TINT_HasValue(this Feature feature) => !(DBNull.Value == feature["T_TINT"]) && feature["T_TINT"] is not null;

        /// <summary>
        /// Tide - time series values
        /// </summary>
        /// <param name="feature">The feature to read the <c>T_TSVL</c> field from.</param>
        /// <returns>The value of <c>T_TSVL</c>, or <c>null</c> when it is not set.</returns>
        public static string? T_TSVL(this Feature feature) {
            if (DBNull.Value == feature["T_TSVL"]) return null;
            var v = Convert.ToString(feature["T_TSVL"]);
            //if ("-32767".Equals(v)) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>T_TSVL</c> (Tide - time series values) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>T_TSVL</c> is neither <see cref="DBNull"/> nor <c>null</c> nor an empty string; otherwise <c>false</c>.</returns>
        public static bool T_TSVL_HasValue(this Feature feature) => !(DBNull.Value == feature["T_TSVL"]) && feature["T_TSVL"] is not null && !string.IsNullOrEmpty(Convert.ToString(feature["T_TSVL"]));

        /// <summary>
        /// Tide - value of harmonic constituents
        /// </summary>
        /// <param name="feature">The feature to read the <c>T_VAHC</c> field from.</param>
        /// <returns>The value of <c>T_VAHC</c>, or <c>null</c> when it is not set.</returns>
        public static string? T_VAHC(this Feature feature) {
            if (DBNull.Value == feature["T_VAHC"]) return null;
            var v = Convert.ToString(feature["T_VAHC"]);
            //if ("-32767".Equals(v)) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>T_VAHC</c> (Tide - value of harmonic constituents) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>T_VAHC</c> is neither <see cref="DBNull"/> nor <c>null</c> nor an empty string; otherwise <c>false</c>.</returns>
        public static bool T_VAHC_HasValue(this Feature feature) => !(DBNull.Value == feature["T_VAHC"]) && feature["T_VAHC"] is not null && !string.IsNullOrEmpty(Convert.ToString(feature["T_VAHC"]));

        /// <summary>
        /// Update Application Date
        /// </summary>
        /// <param name="feature">The feature to read the <c>UADT</c> field from.</param>
        /// <returns>The value of <c>UADT</c>, or <c>null</c> when it is not set.</returns>
        public static DateTime? UADT(this Feature feature) {
            if (DBNull.Value == feature["UADT"]) return null;
            return Convert.ToDateTime(feature["UADT"]);
        }

        /// <summary>
        /// Indicates whether <c>UADT</c> (Update Application Date) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>UADT</c> is neither <see cref="DBNull"/> nor <c>null</c>; otherwise <c>false</c>.</returns>
        public static bool UADT_HasValue(this Feature feature) => !(DBNull.Value == feature["UADT"]) && feature["UADT"] is not null;

        /// <summary>
        /// Update Number
        /// </summary>
        /// <param name="feature">The feature to read the <c>UPDN</c> field from.</param>
        /// <returns>The value of <c>UPDN</c>, or <c>null</c> when it is not set.</returns>
        public static int? UPDN(this Feature feature) {
            if (DBNull.Value == feature["UPDN"]) return null;
            var v = Convert.ToInt32(feature["UPDN"]);
            //if (-32767 == v) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>UPDN</c> (Update Number) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>UPDN</c> is neither <see cref="DBNull"/> nor <c>null</c>; otherwise <c>false</c>.</returns>
        public static bool UPDN_HasValue(this Feature feature) => !(DBNull.Value == feature["UPDN"]) && feature["UPDN"] is not null;

        /// <summary>
        /// Value of annual change in magnetic variation
        /// </summary>
        /// <param name="feature">The feature to read the <c>VALACM</c> field from.</param>
        /// <returns>The value of <c>VALACM</c>, or <c>null</c> when it is not set.</returns>
        public static decimal? VALACM(this Feature feature) {
            if (DBNull.Value == feature["VALACM"]) return null;
            var v = Convert.ToDecimal(feature["VALACM"]);
            //if (-32767m == v) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>VALACM</c> (Value of annual change in magnetic variation) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>VALACM</c> is neither <see cref="DBNull"/> nor <c>null</c>; otherwise <c>false</c>.</returns>
        public static bool VALACM_HasValue(this Feature feature) => !(DBNull.Value == feature["VALACM"]) && feature["VALACM"] is not null;

        /// <summary>
        /// Value of depth contour
        /// </summary>
        /// <param name="feature">The feature to read the <c>VALDCO</c> field from.</param>
        /// <returns>The value of <c>VALDCO</c>, or <c>null</c> when it is not set.</returns>
        public static decimal? VALDCO(this Feature feature) {
            if (DBNull.Value == feature["VALDCO"]) return null;
            var v = Convert.ToDecimal(feature["VALDCO"]);
            //if (-32767m == v) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>VALDCO</c> (Value of depth contour) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>VALDCO</c> is neither <see cref="DBNull"/> nor <c>null</c>; otherwise <c>false</c>.</returns>
        public static bool VALDCO_HasValue(this Feature feature) => !(DBNull.Value == feature["VALDCO"]) && feature["VALDCO"] is not null;

        /// <summary>
        /// Value of local magnetic anomaly
        /// </summary>
        /// <param name="feature">The feature to read the <c>VALLMA</c> field from.</param>
        /// <returns>The value of <c>VALLMA</c>, or <c>null</c> when it is not set.</returns>
        public static decimal? VALLMA(this Feature feature) {
            if (DBNull.Value == feature["VALLMA"]) return null;
            var v = Convert.ToDecimal(feature["VALLMA"]);
            //if (-32767m == v) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>VALLMA</c> (Value of local magnetic anomaly) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>VALLMA</c> is neither <see cref="DBNull"/> nor <c>null</c>; otherwise <c>false</c>.</returns>
        public static bool VALLMA_HasValue(this Feature feature) => !(DBNull.Value == feature["VALLMA"]) && feature["VALLMA"] is not null;

        /// <summary>
        /// Value of magnetic variation
        /// </summary>
        /// <param name="feature">The feature to read the <c>VALMAG</c> field from.</param>
        /// <returns>The value of <c>VALMAG</c>, or <c>null</c> when it is not set.</returns>
        public static decimal? VALMAG(this Feature feature) {
            if (DBNull.Value == feature["VALMAG"]) return null;
            var v = Convert.ToDecimal(feature["VALMAG"]);
            //if (-32767m == v) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>VALMAG</c> (Value of magnetic variation) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>VALMAG</c> is neither <see cref="DBNull"/> nor <c>null</c>; otherwise <c>false</c>.</returns>
        public static bool VALMAG_HasValue(this Feature feature) => !(DBNull.Value == feature["VALMAG"]) && feature["VALMAG"] is not null;

        /// <summary>
        /// Value of maximum range
        /// </summary>
        /// <param name="feature">The feature to read the <c>VALMXR</c> field from.</param>
        /// <returns>The value of <c>VALMXR</c>, or <c>null</c> when it is not set.</returns>
        public static decimal? VALMXR(this Feature feature) {
            if (DBNull.Value == feature["VALMXR"]) return null;
            var v = Convert.ToDecimal(feature["VALMXR"]);
            //if (-32767m == v) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>VALMXR</c> (Value of maximum range) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>VALMXR</c> is neither <see cref="DBNull"/> nor <c>null</c>; otherwise <c>false</c>.</returns>
        public static bool VALMXR_HasValue(this Feature feature) => !(DBNull.Value == feature["VALMXR"]) && feature["VALMXR"] is not null;

        /// <summary>
        /// Value of nominal range
        /// </summary>
        /// <param name="feature">The feature to read the <c>VALNMR</c> field from.</param>
        /// <returns>The value of <c>VALNMR</c>, or <c>null</c> when it is not set.</returns>
        public static decimal? VALNMR(this Feature feature) {
            if (DBNull.Value == feature["VALNMR"]) return null;
            var v = Convert.ToDecimal(feature["VALNMR"]);
            //if (-32767m == v) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>VALNMR</c> (Value of nominal range) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>VALNMR</c> is neither <see cref="DBNull"/> nor <c>null</c>; otherwise <c>false</c>.</returns>
        public static bool VALNMR_HasValue(this Feature feature) => !(DBNull.Value == feature["VALNMR"]) && feature["VALNMR"] is not null;

        /// <summary>
        /// Value of sounding
        /// </summary>
        /// <param name="feature">The feature to read the <c>VALSOU</c> field from.</param>
        /// <returns>The value of <c>VALSOU</c>, or <c>null</c> when it is not set.</returns>
        public static decimal? VALSOU(this Feature feature) {
            if (DBNull.Value == feature["VALSOU"]) return null;
            var v = Convert.ToDecimal(feature["VALSOU"]);
            //if (-32767m == v) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>VALSOU</c> (Value of sounding) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>VALSOU</c> is neither <see cref="DBNull"/> nor <c>null</c>; otherwise <c>false</c>.</returns>
        public static bool VALSOU_HasValue(this Feature feature) => !(DBNull.Value == feature["VALSOU"]) && feature["VALSOU"] is not null;

        /// <summary>
        /// Vertical Datum
        /// </summary>
        /// <param name="feature">The feature to read the <c>VDAT</c> field from.</param>
        /// <returns>The value of <c>VDAT</c>, or <c>null</c> when it is not set.</returns>
        public static int? VDAT(this Feature feature) {
            if (DBNull.Value == feature["VDAT"]) return null;
            var v = Convert.ToInt32(feature["VDAT"]);
            //if (-32767 == v) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>VDAT</c> (Vertical Datum) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>VDAT</c> is neither <see cref="DBNull"/> nor <c>null</c>; otherwise <c>false</c>.</returns>
        public static bool VDAT_HasValue(this Feature feature) => !(DBNull.Value == feature["VDAT"]) && feature["VDAT"] is not null;

        /// <summary>
        /// Vertical accuracy
        /// </summary>
        /// <param name="feature">The feature to read the <c>VERACC</c> field from.</param>
        /// <returns>The value of <c>VERACC</c>, or <c>null</c> when it is not set.</returns>
        public static decimal? VERACC(this Feature feature) {
            if (DBNull.Value == feature["VERACC"]) return null;
            var v = Convert.ToDecimal(feature["VERACC"]);
            //if (-32767m == v) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>VERACC</c> (Vertical accuracy) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>VERACC</c> is neither <see cref="DBNull"/> nor <c>null</c>; otherwise <c>false</c>.</returns>
        public static bool VERACC_HasValue(this Feature feature) => !(DBNull.Value == feature["VERACC"]) && feature["VERACC"] is not null;

        /// <summary>
        /// Vertical clearance, closed
        /// </summary>
        /// <param name="feature">The feature to read the <c>VERCCL</c> field from.</param>
        /// <returns>The value of <c>VERCCL</c>, or <c>null</c> when it is not set.</returns>
        public static decimal? VERCCL(this Feature feature) {
            if (DBNull.Value == feature["VERCCL"]) return null;
            var v = Convert.ToDecimal(feature["VERCCL"]);
            //if (-32767m == v) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>VERCCL</c> (Vertical clearance, closed) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>VERCCL</c> is neither <see cref="DBNull"/> nor <c>null</c>; otherwise <c>false</c>.</returns>
        public static bool VERCCL_HasValue(this Feature feature) => !(DBNull.Value == feature["VERCCL"]) && feature["VERCCL"] is not null;

        /// <summary>
        /// Vertical clearance
        /// </summary>
        /// <param name="feature">The feature to read the <c>VERCLR</c> field from.</param>
        /// <returns>The value of <c>VERCLR</c>, or <c>null</c> when it is not set.</returns>
        public static decimal? VERCLR(this Feature feature) {
            if (DBNull.Value == feature["VERCLR"]) return null;
            var v = Convert.ToDecimal(feature["VERCLR"]);
            //if (-32767m == v) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>VERCLR</c> (Vertical clearance) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>VERCLR</c> is neither <see cref="DBNull"/> nor <c>null</c>; otherwise <c>false</c>.</returns>
        public static bool VERCLR_HasValue(this Feature feature) => !(DBNull.Value == feature["VERCLR"]) && feature["VERCLR"] is not null;

        /// <summary>
        /// Vertical clearance, open
        /// </summary>
        /// <param name="feature">The feature to read the <c>VERCOP</c> field from.</param>
        /// <returns>The value of <c>VERCOP</c>, or <c>null</c> when it is not set.</returns>
        public static decimal? VERCOP(this Feature feature) {
            if (DBNull.Value == feature["VERCOP"]) return null;
            var v = Convert.ToDecimal(feature["VERCOP"]);
            //if (-32767m == v) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>VERCOP</c> (Vertical clearance, open) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>VERCOP</c> is neither <see cref="DBNull"/> nor <c>null</c>; otherwise <c>false</c>.</returns>
        public static bool VERCOP_HasValue(this Feature feature) => !(DBNull.Value == feature["VERCOP"]) && feature["VERCOP"] is not null;

        /// <summary>
        /// Vertical clearance, safe
        /// </summary>
        /// <param name="feature">The feature to read the <c>VERCSA</c> field from.</param>
        /// <returns>The value of <c>VERCSA</c>, or <c>null</c> when it is not set.</returns>
        public static decimal? VERCSA(this Feature feature) {
            if (DBNull.Value == feature["VERCSA"]) return null;
            var v = Convert.ToDecimal(feature["VERCSA"]);
            //if (-32767m == v) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>VERCSA</c> (Vertical clearance, safe) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>VERCSA</c> is neither <see cref="DBNull"/> nor <c>null</c>; otherwise <c>false</c>.</returns>
        public static bool VERCSA_HasValue(this Feature feature) => !(DBNull.Value == feature["VERCSA"]) && feature["VERCSA"] is not null;

        /// <summary>
        /// Vertical datum
        /// </summary>
        /// <param name="feature">The feature to read the <c>VERDAT</c> field from.</param>
        /// <returns>The value of <c>VERDAT</c>, or <c>null</c> when it is not set.</returns>
        public static int? VERDAT(this Feature feature) {
            if (DBNull.Value == feature["VERDAT"]) return null;
            var v = Convert.ToInt32(feature["VERDAT"]);
            //if (-32767 == v) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>VERDAT</c> (Vertical datum) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>VERDAT</c> is neither <see cref="DBNull"/> nor <c>null</c>; otherwise <c>false</c>.</returns>
        public static bool VERDAT_HasValue(this Feature feature) => !(DBNull.Value == feature["VERDAT"]) && feature["VERDAT"] is not null;

        /// <summary>
        /// Vertical length
        /// </summary>
        /// <param name="feature">The feature to read the <c>VERLEN</c> field from.</param>
        /// <returns>The value of <c>VERLEN</c>, or <c>null</c> when it is not set.</returns>
        public static decimal? VERLEN(this Feature feature) {
            if (DBNull.Value == feature["VERLEN"]) return null;
            var v = Convert.ToDecimal(feature["VERLEN"]);
            //if (-32767m == v) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>VERLEN</c> (Vertical length) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>VERLEN</c> is neither <see cref="DBNull"/> nor <c>null</c>; otherwise <c>false</c>.</returns>
        public static bool VERLEN_HasValue(this Feature feature) => !(DBNull.Value == feature["VERLEN"]) && feature["VERLEN"] is not null;

        /// <summary>
        /// Water level effect
        /// </summary>
        /// <param name="feature">The feature to read the <c>WATLEV</c> field from.</param>
        /// <returns>The value of <c>WATLEV</c>, or <c>null</c> when it is not set.</returns>
        public static int? WATLEV(this Feature feature) {
            if (DBNull.Value == feature["WATLEV"]) return null;
            var v = Convert.ToInt32(feature["WATLEV"]);
            //if (-32767 == v) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>WATLEV</c> (Water level effect) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>WATLEV</c> is neither <see cref="DBNull"/> nor <c>null</c>; otherwise <c>false</c>.</returns>
        public static bool WATLEV_HasValue(this Feature feature) => !(DBNull.Value == feature["WATLEV"]) && feature["WATLEV"] is not null;

        /// <summary>
        /// Where Clause
        /// </summary>
        /// <param name="feature">The feature to read the <c>WHERECLAUSE</c> field from.</param>
        /// <returns>The value of <c>WHERECLAUSE</c>, or <c>null</c> when it is not set.</returns>
        public static string? WHERECLAUSE(this Feature feature) {
            if (DBNull.Value == feature["WHERECLAUSE"]) return null;
            var v = Convert.ToString(feature["WHERECLAUSE"]);
            //if ("-32767".Equals(v)) return null;
            return v;
        }

        /// <summary>
        /// Indicates whether <c>WHERECLAUSE</c> (Where Clause) holds a value.
        /// </summary>
        /// <param name="feature">The feature to probe.</param>
        /// <returns><c>true</c> when <c>WHERECLAUSE</c> is neither <see cref="DBNull"/> nor <c>null</c> nor an empty string; otherwise <c>false</c>.</returns>
        public static bool WHERECLAUSE_HasValue(this Feature feature) => !(DBNull.Value == feature["WHERECLAUSE"]) && feature["WHERECLAUSE"] is not null && !string.IsNullOrEmpty(Convert.ToString(feature["WHERECLAUSE"]));

        public static int? SCAMIN_STEP(this Feature feature) {
            if (DBNull.Value == feature["SCAMIN_STEP"]) return null;
            var v = Convert.ToInt32(feature["SCAMIN_STEP"]);
            return v;
        }

        public static bool SCAMIN_STEP_HasValue(this Feature feature) => !(DBNull.Value == feature["SCAMIN_STEP"]) && feature["SCAMIN_STEP"] is not null;

        public static int? PLTS_COMP_SCALE(this Feature feature) {
            if (DBNull.Value == feature["PLTS_COMP_SCALE"]) return null;
            var v = Convert.ToInt32(feature["PLTS_COMP_SCALE"]);
            return v;
        }

        public static bool PLTS_COMP_SCALE_HasValue(this Feature feature) => !(DBNull.Value == feature["PLTS_COMP_SCALE"]) && feature["PLTS_COMP_SCALE"] is not null;

        public static int? FCSubtype(this Feature feature) {
            if (DBNull.Value == feature["FCSubtype"]) return null;
            var v = Convert.ToInt32(feature["FCSubtype"]);
            return v;
        }

        public static bool FCSubtype_HasValue(this Feature feature) => !(DBNull.Value == feature["FCSubtype"]) && feature["FCSubtype"] is not null;

        public static Geometry? Shape(this Feature feature) => (Geometry?)feature["SHAPE"];

        public static string TableName(this Feature feature) => feature.GetTable().GetName().Split('.')[^1];
    }
}