using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using NetTopologySuite.Geometries;
using S100FC.S101.ComplexAttributes;
using S100FC.S101.FeatureTypes;
using S100FC.S101.SimpleAttributes;
using S100Framework.Applications.S57.esri;

namespace S100Framework.Applications
{
    public static class AidsToNavigationExtension
    {
        static readonly PrecisionModel precisionModel = new PrecisionModel(10000000);    //ENC

        static readonly GeometryFactory factory = new GeometryFactory(precisionModel, srid: 4326); // Or PrecisionModels.Floating        

        internal static Type GetS101CatlitTypeFrom(this AidsToNavigationP aton) {
            if (aton.FCSUBTYPE != 65) {
                throw new NotImplementedException($"Only light types are supported.");
            }

            List<int> catlits = [];

            if (aton.CATLIT != default) {
                catlits = aton.CATLIT.Split(',')
                                   .Select(int.Parse)
                                   .ToList();
            }

            if ((aton.SECTR1 == default || aton.SECTR2 == default) && !(catlits.Contains(1) || catlits.Contains(6) || catlits.Contains(7) || catlits.Contains(16))) {
                return typeof(LightAllAround);
            }
            else if (aton.SECTR1 != default && aton.SECTR2 != default || catlits.Contains(1) || catlits.Contains(16)) {
                return typeof(LightSectored);
            }
            else if (catlits.Contains(6)) {
                return typeof(LightAirObstruction);
            }
            else if (catlits.Contains(7)) {
                return typeof(LightFogDetector);
            }
            else {
                throw new NotSupportedException($"LIGHT catlit: {aton.CATLIT} : {aton.LNAM}");
            }
        }

        internal static (topmark topmark, Guid relatedId)? GetTopMark<TType>(this Geodatabase geodatabase, AidsToNavigationP structure) where TType : S100FC.FeatureType {
            AidsToNavigationP[] topmarks = [];

            Guid[] lookup = [];
            using var PLTS_FREL = geodatabase.OpenDataset<Table>(geodatabase.GetName("PLTS_FREL"));
            using (var search = PLTS_FREL.Search(new QueryFilter {
                WhereClause = $"DEST_FC = 'AidsToNavigationP' And SRC_UID = '{structure.GlobalId:B}' And DEST_SUB = 'TOPMAR_Topmark'",
            }, true)) {
                while (search.MoveNext()) {
                    lookup = [.. lookup, search.Current.GetGlobalID()];
                }
            }

            if (lookup.Any()) {
                using var AidsToNavigationP = geodatabase.OpenDataset<FeatureClass>(geodatabase.GetName("AidsToNavigationP"));
                using (var search = AidsToNavigationP.Search(new QueryFilter {
                    WhereClause = $"GlobalID IN ({string.Join(',', lookup.Select(e => $"'{e:B}'".ToUpperInvariant()))}"
                }, true)) {
                    while (search.MoveNext()) {
                        var topmark = new AidsToNavigationP((Feature)search.Current);
                        topmarks = [.. topmarks, topmark];
                    }
                }
            }

            return GetTopMark<TType>(structure, topmarks);
        }

        internal static (topmark topmark, Guid relatedId)? GetTopMark<TType>(AidsToNavigationP structure, AidsToNavigationP[] topmarks) where TType : S100FC.FeatureType {
            //var topmarks = FeatureRelations.Instance.GetRelated<AidsToNavigationP>(typeof(topmark), structure.GLOBALID);

            if (topmarks == null || topmarks.Count() == 0) {
                return null;
            }

            if (topmarks.Count() > 1) {
                Logger.Current.Error("Multiple related topmarks #{count} ({objectid})", topmarks.Count(), structure.OBJECTID);
                //throw new NotSupportedException("Multiple related topmarks");
            }

            var relatedTopmark = topmarks.First();

            if (relatedTopmark != null) {

                int?[]? topmarkColours = null;

                colourPattern? topmarkColourPattern = null;

                if (relatedTopmark.COLOUR != default) {
                    topmarkColours = ImporterNIS.GetColours(relatedTopmark.COLOUR);
                }

                if (relatedTopmark.COLPAT != default) {
                    if (topmarkColours is not null && topmarkColours.Length > 1)
                        topmarkColourPattern = ImporterNIS.GetColourPattern(relatedTopmark.COLPAT);
                }

                var topmark = new topmark() {
                    //topmarkDaymarkShape = default,
                    // TODO: shapeinformation #15 @https://geodatastyrelsen.atlassian.net/wiki/spaces/SOEKORT/pages/5070028848/S-57+to+S-101+Conversion+Action+Points?force_transition=910d1b59-0dc5-42d7-bd2c-a81edd431caf,

                };

                if (topmarkColours != null) {
                    topmark.colour = topmarkColours;
                }

                if (topmarkColourPattern is not null) {
                    topmark.colourPattern = topmarkColourPattern.value;
                }

                if (relatedTopmark.TOPSHP.HasValue) {
                    topmark.topmarkDaymarkShape = EnumHelper.GetEnumValue(relatedTopmark.TOPSHP.Value);
                }

                //ConversionAnalytics.Instance.AddConverted("AidsToNavigationP", relatedTopmark.GLOBALID, "ATTRIBUTE. NO NAME AVAILABLE");

                return (topmark, relatedTopmark.GLOBALID);
            }
            return null;
        }

        internal static (int?, string?, string?, string?, string?, decimal?, int?, int?, string?, string?, string?, int?, double, double) ToToken(this AidsToNavigationP e) {
            var shape = (MapPoint)e.Shape!;
            var p = factory.CreatePoint(new NetTopologySuite.Geometries.Coordinate(shape.X, shape.Y));
            return (e.EXCLIT, e.OBJNAM, e.NOBJNM, e.DATSTA, e.DATEND, e.HEIGHT, e.MARSYS, e.MLTYLT, e.PERSTA, e.PEREND, e.STATUS, e.SIGGEN, p.X, p.Y);
        }
    }
}
