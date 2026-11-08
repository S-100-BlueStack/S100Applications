#define CREATE_SUBTYPES

using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using S100FC.S101.ComplexAttributes;
using S100FC.S101.InformationTypes;
using S100FC.S101.SimpleAttributes;
using S100Framework.Applications.S57.esri;
using S100Framework.Applications.Singletons;
using System.Data;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

using IO = System.IO;


namespace S100Framework.Applications
{
    internal static partial class ImporterNIS
    {
        private static void DeleteAll(Table table) {
            QueryFilter queryFilter = new QueryFilter {
                WhereClause = "1=1" // Gets all rows
            };

            table.DeleteRows(queryFilter);
            return;


            using (var rowCursor = table.CreateUpdateCursor(queryFilter, true)) {
                while (rowCursor.MoveNext()) {
                    rowCursor.Current.Delete();
                }
            }
        }

        const string DE9IM_Contains = "T*****FF*";

        internal static IEnumerable<(int FcSubtype, decimal? DRVAL1)> GetSurrounding_DepthArea(Geometry shape, Geodatabase geodatabase, string queryFilter) {
            (string tablename, SpatialRelationship relationship, string description)[] dictionary = [
                    ("depthsa", SpatialRelationship.Relation, "T*****T**"),
                    ("depthsa", SpatialRelationship.Relation, "T********"),
                ];
            var definitions = geodatabase.GetDefinitions<FeatureClassDefinition>().Select(e => e.GetName().ToLowerInvariant());
            var hits = new HashSet<long>();

            foreach (var e in dictionary) {
                using var danger = geodatabase.OpenDataset<FeatureClass>(definitions.Single(d => d.EndsWith(e.tablename)));

                using var search = danger.Search(new SpatialQueryFilter {
                    WhereClause = $"({queryFilter})",
                    SpatialRelationship = e.relationship,
                    SpatialRelationshipDescription = e.description,
                    FilterGeometry = shape,
                }, false);

                while (search.MoveNext()) {
                    var f = (Feature)search.Current;
                    var _ = f.GetObjectID();
                    if (hits.Contains(_)) continue;
                    hits.Add(_);
                    yield return (f.FCSubtype(), f.DRVAL1());
                    f.Dispose();
                }
            }
            yield break;
        }

        internal static bool IsCoveredByUNSARE_UnsurveyedArea(Geometry shape) {
            foreach (DepthsA depthArea in SpatialRelationResolver.Instance.GetSpatialRelatedValueFrom<DepthsA>(shape, SpatialRelationship.Contains)) {
                //foreach (DepthsA depthArea in SpatialRelationResolver.Instance.GetSpatialRelatedValueFrom<DepthsA>(shape, SpatialRelationship.Contains)) {
                if (depthArea.FcSubtype is 15) {  // UNSARE
                    return true;
                }
            }
            return false;
        }

        internal static bool ContainsBarthyFeatures_UnsurveyedArea(Geometry shape, Geodatabase geodatabase, string queryFilter) {
            (string tablename, SpatialRelationship relationship, string filter)[] dictionary = [
                    ("dangersp", SpatialRelationship.Contains, "FCSubtype IN(35,45,20)"),    //UWTROC,WRECKS,OBSTRN
                    ("dangersl", SpatialRelationship.Contains, "FCSubtype IN(5)"),           //OBSTRN
                    ("dangersa", SpatialRelationship.Contains, "FCSubtype IN(15,25)"),       //OBSTRN,WRECKS
                    ("depthsl", SpatialRelationship.Contains, "FCSubtype IN(5)"),           //DEPCNT                    
                    ("regulatedareasandlimitsp", SpatialRelationship.Contains, "FCSubtype IN(35)"),           //MARCUL                    
                    ("regulatedareasandlimitsl", SpatialRelationship.Contains, "FCSubtype IN(25)"),           //MARCUL                    
                    ("regulatedareasandlimitsa", SpatialRelationship.Contains, "FCSubtype IN(95)"),           //MARCUL                                        
                ];
            var definitions = geodatabase.GetDefinitions<FeatureClassDefinition>().Select(e => e.GetName().ToLowerInvariant());
            foreach (var e in dictionary) {
                using var danger = geodatabase.OpenDataset<FeatureClass>(definitions.Single(d => d.EndsWith(e.tablename)));

                using var search = danger.Search(new SpatialQueryFilter {
                    WhereClause = $"({queryFilter}) AND ({e.filter})",
                    SpatialRelationship = e.relationship,
                    FilterGeometry = shape,
                }, true);

                var hit = search.MoveNext();
                if (hit)
                    return true;
            }

            return false;
        }

        internal static decimal? GetDefaultClearanceDepthWreck(Geometry? shape, decimal? valsou, int? expsou, decimal? height, int? watlev, int? catwrk, long objectid, string tablename, string lnam) {

            bool coveredByUnsurveyedArea = false;
            bool coveredByDredgedArea = false;
            decimal? surroundingDepth = null;
            decimal? leastDepth = null;

            if (shape != null) {
                foreach (DepthsA depthArea in SpatialRelationResolver.Instance.GetSpatialRelatedValueFrom<DepthsA>(shape)) {
                    if (leastDepth is null) {
                        leastDepth = depthArea.DRVAL1.HasValue ? depthArea.DRVAL1.Value : null;
                    }
                    else if (leastDepth > depthArea.DRVAL1) {
                        leastDepth = depthArea.DRVAL1.HasValue ? depthArea.DRVAL1.Value : null;
                    }

                    if (depthArea.FcSubtype is 15) {  // UNSARE
                        coveredByUnsurveyedArea = true;
                        break;
                    }
                    if (depthArea.FcSubtype is 5) {  // DRGARE
                        coveredByDredgedArea = true;
                        surroundingDepth = leastDepth != -32767m ? leastDepth : null;
                    }
                    if (depthArea.FcSubtype is 1) {  // DEPARE
                        surroundingDepth = leastDepth != -32767m ? leastDepth : null;
                    }

                    surroundingDepth = leastDepth != -32767m ? leastDepth : null;
                }
            }

            bool allCoveringDepthRangeMinimumValuesAreKnown = surroundingDepth.HasValue;

            bool unknownDepthCoveredByUnsurveyedArea = coveredByUnsurveyedArea && (valsou.HasValue && valsou is -32767m);

            bool depthDredgedAreaWhereDepthMinimumValueIsUnknown = coveredByDredgedArea && !surroundingDepth.HasValue;

            bool valsouIsKnown = valsou is not null && valsou is not -32767m;
            bool valsouIsUnknown = valsou is -32767m;

            bool catwrkIsUnknown = catwrk is -32767;

            bool heightIsKnown = height is not null && height is not -32767m;
            bool heightIsUnknown = height is -32767m;
            bool expositionOfSoundingIs1Or3 = expsou is 1 || expsou is 3; // || expsou == -32767;
            bool expositionOfSoundingIsUnknown = expsou is -32767;


            if (allCoveringDepthRangeMinimumValuesAreKnown) {
                if ((catwrk is 4 || catwrk is 5) &&
                    heightIsKnown &&
                    (watlev is 1 || watlev is 2 || watlev is -32767)) {
                    return null;
                }
                else if (valsouIsKnown &&
                    (watlev is 3 || watlev is 4 || watlev is 5 || watlev is -32767)) {
                    return null;
                }
                else if (expositionOfSoundingIs1Or3 &&
                    valsouIsUnknown &&
                    (watlev.HasValue && (watlev is 3))) {
                    return leastDepth;
                }
                else if (expositionOfSoundingIs1Or3 &&
                    (watlev.HasValue && (watlev is 3))) {
                    return leastDepth;
                }
                else if ((catwrk is 1) &&
                    ((watlev is 1 || watlev is 2 || watlev is 4 || watlev is 5 || watlev is -32767))) {

                    return 20.1m > (leastDepth - 66) ? 20.1m : (leastDepth - 66); // 20.1 or least depth - 66, whichever is largest
                }
                else if (catwrk is 1 &&
                    (expsou is null || expositionOfSoundingIsUnknown || (expsou is 2))) {
                    return 20.1m > (leastDepth - 66) ? 20.1m : (leastDepth - 66); // 20.1 or least depth - 66, whichever is largest
                }
                else if ((expsou is null || expositionOfSoundingIsUnknown || (expsou is 2)) &&
                    valsouIsUnknown &&
                    (watlev is 3 || watlev is 5)) {
                    return 0m;
                }
                else if ((expsou is null || expositionOfSoundingIsUnknown || (expsou is 2)) &&
                    valsouIsUnknown &&
                    (watlev is 4 || watlev is -32767)) {

                    return -15m;
                }
                else if (((catwrk is 2 || catwrk is 3 || catwrk is 4 || catwrk is 5 || catwrk is -32767)) &&
                    (watlev is 1 || watlev is 2 || watlev is 4 || watlev is 5 || watlev is -32767)) {
                    return -15m;
                }
                else if ((catwrk is 2 || catwrk is 3 || catwrk is 4 || catwrk is 5 || catwrk is -32767) &&
                    (expsou is null || expositionOfSoundingIsUnknown || (expsou is 2))) {
                    return -15m;
                }
                else {
                    Logger.Current.DataError(objectid, tablename, lnam, $"1:Cannot set default clearance depth. Check loader.");
                    return null;

                }
            }
            else if (unknownDepthCoveredByUnsurveyedArea || depthDredgedAreaWhereDepthMinimumValueIsUnknown) {

                if (catwrk is 1 &&
                    (watlev is 3 || watlev is -32767)) {
                    return 20.1m;
                }
                else if (valsouIsUnknown &&
                    (watlev is 3 || watlev is 5)) {
                    return 0m;
                }
                else if (valsouIsUnknown &&
                    (watlev is 4 || watlev is -32767)) {
                    return -15m;
                }
                else if (catwrkIsUnknown &&
                    (watlev is 3 || watlev is 5)) {
                    return 0m;
                }
                else if ((catwrk is 2 || catwrk is 3 || catwrk is 4 || catwrk is 5) &&
                    (watlev is 3 || watlev is 5)) {
                    return -15m;
                }
                else if ((catwrk is 2 || catwrk is 3 || catwrk is 4 || catwrk is 5 || catwrk is -32767) &&
                    (watlev is 4 || watlev is -32767)) {
                    return -15m;
                }
                else {
                    Logger.Current.DataError(objectid, tablename, lnam, $"2:Cannot set default clearance depth. Check loader.");
                    return null;
                }
            }
            else {
                Logger.Current.DataError(objectid, tablename, lnam, $"3:Cannot set default clearance depth. Check loader.");
                return null;
            }
        }

        internal static decimal? GetDefaultClearanceDepthObstruction(Geometry? shape, decimal? valsou, int? expsou, decimal? height, int? watlev, int? catobs, long objectid, string tablename, string lnam) {

            bool coveredByUnsurveyedArea = false;
            bool coveredByDredgedArea = false;
            decimal? surroundingDepth = null;
            decimal? leastDepth = null;

            if (shape != null) {
                foreach (DepthsA depthArea in SpatialRelationResolver.Instance.GetSpatialRelatedValueFrom<DepthsA>(shape)) {
                    leastDepth = depthArea.DRVAL1.HasValue ? depthArea.DRVAL1.Value : null;

                    if (depthArea.FcSubtype!.Value == 15) {  // UNSARE
                        coveredByUnsurveyedArea = true;
                        break;
                    }
                    if (depthArea.FcSubtype!.Value == 5) {  // DRGARE
                        coveredByDredgedArea = true;
                        surroundingDepth = leastDepth != -32767m ? leastDepth : null;
                    }
                    if (depthArea.FcSubtype!.Value == 1) {  // DEPARE
                        surroundingDepth = leastDepth != -32767m ? leastDepth : null;
                    }

                    surroundingDepth = leastDepth != -32767m ? leastDepth : null;
                }
            }

            bool allCoveringDepthRangeMinimumValuesAreKnown = surroundingDepth.HasValue;

            bool unknownDepthCoveredByUnsurveyedArea = coveredByUnsurveyedArea && (valsou.HasValue && valsou.Value == -32767m);

            bool depthDredgedAreaWhereDepthMinimumValueIsUnknown = coveredByDredgedArea && !surroundingDepth.HasValue;

            bool valsouIsKnown = valsou.HasValue && valsou.Value != -32767m;
            bool valsouIsUnknown = valsou.HasValue && valsou.Value == -32767m;
            bool heightIsKnown = height.HasValue && height.Value != -32767m;
            bool heightIsUnknown = height.HasValue && height.Value == -32767m;
            bool expositionOfSoundingIs1Or3 = expsou is 1 || expsou is 3;


            if (allCoveringDepthRangeMinimumValuesAreKnown) {
                if (heightIsKnown &&
                    (watlev is 1 || watlev is 2)) {
                    return null;
                }
                else if (heightIsUnknown &&
                    (watlev is 1 || watlev is 2 || watlev is 7)) {
                    return null;
                }
                else if (valsouIsKnown &&
                    (watlev is 3 || watlev is 4 || watlev is 5 || watlev is -32767)) {
                    return null;
                }
                else if (expositionOfSoundingIs1Or3 &&
                    valsouIsUnknown &&
                    (watlev is 3)) {
                    return leastDepth;
                }
                else if ((catobs is 6) &&
                    (expsou is null || (expsou is 2 || expsou is -32767)) &&
                    valsouIsUnknown) {
                    return 0.1m;
                }
                else if ((expsou is null || (expsou is 2 || expsou is -32767)) &&
                    valsouIsUnknown &&
                    watlev is 3) {
                    return 0.1m;
                }
                else if ((catobs is not 6) &&
                    (expsou is null || (expsou is 2 || expsou is -32767)) &&
                    valsouIsUnknown &&
                    (watlev is 5)) {
                    return 0m;
                }
                else if ((catobs is not 6) &&
                    (expsou is null || (expsou is 2 || expsou is -32767)) &&
                    valsouIsUnknown &&
                    (watlev is 4 || watlev is -32767)) {
                    return -15m;
                }
                else {
                    Logger.Current.DataError(objectid, tablename, lnam, $"1:Cannot set default clearance depth. Check loader.");
                    return null;

                }
            }
            else if (unknownDepthCoveredByUnsurveyedArea || depthDredgedAreaWhereDepthMinimumValueIsUnknown) {

                if ((catobs is 6) &&
                    valsouIsUnknown) {
                    return 0.1m;
                }
                else if (valsouIsUnknown &&
                    (watlev is 3)) {
                    return 0.1m;
                }
                else if ((catobs is 6) &&
                    valsouIsUnknown &&
                    (watlev is 5)) {
                    return 0m;
                }
                else if ((catobs is not 6) &&
                    valsouIsUnknown &&
                    (watlev is 4 || watlev is -32767)) {
                    return -15m;
                }
                else {
                    Logger.Current.DataError(objectid, tablename, lnam, $"2:Cannot set default clearance depth. Check loader.");
                    return null;
                }
            }
            else {
                Logger.Current.DataError(objectid, tablename, lnam, $"3:Cannot set default clearance depth. Check loader.");
                return null;
            }
        }

        static readonly string[] ISO3166 = ["AF", "AX", "AL", "DZ", "AS", "AD", "AO", "AI", "AQ", "AG", "AR", "AM", "AW", "AU", "AT", "AZ", "BS", "BH", "BD", "BB", "BY", "BE", "BZ", "BJ", "BM", "BT", "BO", "BQ", "BA", "BW", "BV", "BR", "IO", "BN", "BG", "BF", "BI", "CV", "KH", "CM", "CA", "KY", "CF", "TD", "CL", "CN", "CX", "CC", "CO", "KM", "CD", "CG", "CK", "CR", "CI", "HR", "CU", "CW", "CY", "CZ", "DK", "DJ", "DM", "DO", "EC", "EG", "SV", "GQ", "ER", "EE", "SZ", "ET", "FK", "FO", "FJ", "FI", "FR", "GF", "PF", "TF", "GA", "GM", "GE", "DE", "GH", "GI", "GR", "GL", "GD", "GP", "GU", "GT", "GG", "GN", "GW", "GY", "HT", "HM", "VA", "HN", "HK", "HU", "IS", "IN", "ID", "IR", "IQ", "IE", "IM", "IL", "IT", "JM", "JP", "JE", "JO", "KZ", "KE", "KI", "KP", "KR", "KW", "KG", "LA", "LV", "LB", "LS", "LR", "LY", "LI", "LT", "LU", "MO", "MG", "MW", "MY", "MV", "ML", "MT", "MH", "MQ", "MR", "MU", "YT", "MX", "FM", "MD", "MC", "MN", "ME", "MS", "MA", "MZ", "MM", "NA", "NR", "NP", "NL", "NC", "NZ", "NI", "NE", "NG", "NU", "NF", "MK", "MP", "NO", "OM", "PK", "PW", "PS", "PA", "PG", "PY", "PE", "PH", "PN", "PL", "PT", "PR", "QA", "RE", "RO", "RU", "RW", "BL", "SH", "KN", "LC", "MF", "PM", "VC", "WS", "SM", "ST", "SA", "SN", "RS", "SC", "SL", "SG", "SX", "SK", "SI", "SB", "SO", "ZA", "GS", "SS", "ES", "LK", "SD", "SR", "SJ", "SE", "CH", "SY", "TW", "TJ", "TZ", "TH", "TL", "TG", "TK", "TO", "TT", "TN", "TR", "TM", "TC", "TV", "UG", "UA", "AE", "GB", "UM", "US", "UY", "UZ", "VU", "VE", "VN", "VG", "VI", "WF", "EH", "YE", "ZM", "ZW"];

        internal static string GetNation(string nation) {
            if (nation.ToUpperInvariant().Equals("UK")) nation = "GB";
            if (ISO3166.Contains(nation.ToUpperInvariant()))
                return nation.ToUpperInvariant();
            throw new NotSupportedException($"Nation {nation} cannot be converted");
        }

        internal static void SetShape(RowBuffer buffer, Geometry? shape) {
            if (shape == null) {
                throw new ArgumentException("Null geometry not supported");
            }

            if (shape.GeometryType == ArcGIS.Core.Geometry.GeometryType.Point && shape.HasZ == false) {
                buffer["shape"] = MapPointBuilderEx.CreateMapPoint(((MapPoint)shape).X, ((MapPoint)shape).Y, 0.00, SpatialReferences.WGS84);
            }
            else {
                //buffer["shape"] = GeometryEngine.Instance.SimplifyAsFeature(shape, true);
                buffer["shape"] = shape;
            }
        }

        internal static int SpecificUsage(int scale) {
            return scale switch {
                -1 => throw new InvalidOperationException("compilation scale isn't initialized!"),
                < 22000 => 5,
                < 90000 => 4,
                < 180000 => 3,
                < 700000 => 2,
                _ => 1
            };
        }

        internal static void SetCompilationScale(RowBuffer buffer, int scale) {
            var _ = SpecificUsage(scale);
            //buffer["specificusage"] = _;
            buffer["compilationscale"] = optimumDisplayScaleConverter(scale) ?? 10000000;
        }

        /// <summary>
        /// DCEG p460
        /// </summary>
        /// <param _s101name="current"></param>
        /// <returns></returns>
        internal static rhythmOfLight GetRythmOfLight<TType>(AidsToNavigationP current) where TType : S100FC.FeatureType {
            /*
                When populating rhythm of light, the
                sub-attributes signal group, signal period and signal sequence are only valid for non-fixed lights
                (that is, sub-attribute light characteristic ≠ 1 (fixed)), with signal group and signal period being
                mandatory
            */

            //current.SIGGRP != default ? new List<string> { current.SIGGRP } : new();
            List<string> parenthesisParts = [];

            if (!String.IsNullOrEmpty(current.SIGGRP)) {
                string pattern = @"\([^()]*\)";
                if (!Regex.Match(current.SIGGRP, pattern).Success) {
                    Logger.Current.DataError(current.OBJECTID ?? -1, current.TableName!, current.LNAM!, $"Cannot parse SIGGRP string: {current.SIGGRP} on {current.GlobalId}");
                    ;
                }
                foreach (Match m in Regex.Matches(current.SIGGRP, pattern)) {
                    parenthesisParts.Add(m.Value);
                }
            }
            //var signalPeriodN = current.SIGPER == -32767m ? default : current.SIGPER;

            var sigseq = current.SIGSEQ;

            //lightCharacteristic? lightCharacteristicsValue = default;

            //if (current.LITCHR.HasValue) {
            //    lightCharacteristicsValue = EnumHelper.GetEnumValue(current.LITCHR.Value);
            //}

            var signalSequences = GetSignalSequences(current.SIGSEQ);

            var rhythmOfLight = new rhythmOfLight() {
                lightCharacteristic = EnumHelper.GetEnumValue(current.LITCHR),
                signalGroup = [.. parenthesisParts],
                signalSequence = [.. signalSequences],
            };
            //if (lightCharacteristicsValue != null)
            //    rhythmOfLight.lightCharacteristic = EnumHelper.GetEnumValue(current.LITCHR);
            //if (parenthesisParts.Any())
            //    rhythmOfLight.signalGroup = parenthesisParts.ToArray();
            if (current.SIGPER.HasValue)
                rhythmOfLight.signalPeriod = current.SIGPER == -32767m ? null : current.SIGPER == 0 ? null : current.SIGPER;
            //if (signalSequences.Any())
            //    rhythmOfLight.signalSequence = signalSequences.ToArray();

            //if (0m == rhythmOfLight.signalPeriod) System.Diagnostics.Debugger.Break();
            return rhythmOfLight;
        }

        internal static verticalDatum? GetVerticalDatum(int? value, Geometry geometry) {
            if (!value.HasValue) return default(verticalDatum?);

            var verticalDatumConverter = Scamin.Instance.GetVerticalDatumConverters(geometry);
            if (verticalDatumConverter.ContainsKey(value.Value)) {
                return EnumHelper.GetEnumValue(verticalDatumConverter[value.Value]);
            }
            return EnumHelper.GetEnumValue(value);
        }

        internal static verticalDatum? GetSoundingDatum(int value, Geometry geometry) {
            var verticalDatumConverter = Scamin.Instance.GetVerticalDatumConverters(geometry);
            if (verticalDatumConverter.ContainsKey(value)) {
                return EnumHelper.GetEnumValue(verticalDatumConverter[value]);
            }
            return EnumHelper.GetEnumValue(value);
        }

        //internal static verticalDatum? GetVerticalDatum<TType>(int value) where TType : DomainModel.FeatureNode {
        //    return EnumHelper.GetEnumValue(value);
        //}


        internal static signalSequence[] GetSignalSequences(string? sigseq) {
            var signalSequences = new List<signalSequence>();

            string pattern = @"(\d+\.\d+)|\((\d+\.\d+)\)";

            if (sigseq != default) {

                Regex regex = new Regex(pattern);
                MatchCollection matches = regex.Matches(sigseq);

                foreach (Match match in matches) {
                    if (!string.IsNullOrEmpty(match.Groups[1].Value)) {
                        var duration = decimal.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
                        // Interval of light
                        signalSequences.Add(new signalSequence() {
                            signalDuration = duration,
                            signalStatus = 1,   //signalStatus.LitSound
                        });
                    }
                    else if (!string.IsNullOrEmpty(match.Groups[2].Value)) {
                        var duration = decimal.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture);
                        // Eclipse
                        signalSequences.Add(new signalSequence() {
                            signalDuration = duration,
                            signalStatus = 2,   //signalStatus.EclipsedSilent
                        });
                    }
                }
            }
            return signalSequences.ToArray();
        }

        internal static int?[]? GetColours(string color) {
            return EnumHelper.GetEnumValues(color);

            //List<colour> colours = new List<colour>();
            //if (color != default) {
            //    if (!string.IsNullOrEmpty(color)) {
            //        foreach (var c in color.Split(',', StringSplitOptions.RemoveEmptyEntries)) {
            //            colour? e = c.ToLowerInvariant() switch {
            //                "1" => colour.White,
            //                "2" => colour.Black,
            //                "3" => colour.Red,
            //                "4" => colour.Green,
            //                "5" => colour.Blue,
            //                "6" => colour.Yellow,
            //                "7" => colour.Grey,
            //                "8" => colour.Brown,
            //                "9" => colour.Amber,
            //                "10" => colour.Violet,
            //                "11" => colour.Orange,
            //                "12" => colour.Magenta,
            //                "13" => colour.Pink,
            //                "-32767" =>(colour)(-1),
            //                _ => throw new IndexOutOfRangeException(),
            //            };
            //            if (e.HasValue) {
            //                colours.Add(e.Value);
            //            }
            //        }
            //    }
            //}
            //return colours;
        }

        //private static buoyShape GetBuoyShape(int? buoyShapeValue) {
        //    return buoyShapeValue.Value switch {
        //        1 => buoyShape.Conical,
        //        2 => buoyShape.Can,
        //        3 => buoyShape.Spherical,
        //        4 => buoyShape.Pillar,
        //        5 => buoyShape.Spar,
        //        6 => buoyShape.Barrel,
        //        7 => buoyShape.Superbuoy,
        //        8 => buoyShape.IceBuoy,
        //        -32767 => (buoyShape)(-1),
        //        _ => throw new IndexOutOfRangeException("Invalid buoy shape value."),
        //    };
        //}

        internal static colourPattern? GetColourPattern(string colorPattern) {
            if (colorPattern.Contains(",")) {
                Logger.Current.Error($"Multiple colorPatterns not supported in S-101 ({colorPattern})!");
            }
            int? colourPat = colorPattern switch {
                "1" => 1,   //colourPattern.HorizontalStripes,
                "2" => 2,   //colourPattern.VerticalStripes,
                "3" => 3,   //colourPattern.DiagonalStripes,
                "4" => 4,   //colourPattern.Squared,
                "5" => 5,   //colourPattern.StripesDirectionUnknown,
                "6" => 6,   //colourPattern.BorderStripe,
                "-32767" => null,

                "3,4" => null, //US
                "6,4" => null, //US

                _ => null,  //throw new IndexOutOfRangeException($"Colourpattern value is not legal {colorPattern}")
            };
            return colourPat;
        }

        private static status GetSingleStatus(string status) {
            return GetStatus(status)[0];


        }

        internal static int?[] GetStatus(string statuses) {
            List<int?> statusList = [];

            var featureStatus = statuses.Trim();

            /*
             * code	status
            alias	STATUS
            _s101name	Status
            definition	The condition of an object at a given instant in time.
            valueType	enumeration  listedValues	

            Permanent	            1	IHOREG	Intended to last or function indefinitely.
            Occasional	            2	IHOREG	Acting on special occasions; happening irregularly.
            Recommended	            3	IHOREG	Presented as worthy of confidence, acceptance, use, etc.
            Not in Use	            4	IHOREG	Use has ceased, but the facility still exists intact; disused.
            Periodic/Intermittent	5	IHOREG	Recurring at intervals.
            Reserved	            6	IHOREG	Set apart for some specific use.
            Temporary	            7	IHOREG	Meant to last only for a time.
            Private	                8	IHOREG	Administered by an individual or corporation, rather than a State or a public body.
            Mandatory	            9	IHOREG	Compulsory; enforced.
            Extinguished	        11	IHOREG	No longer lit.
            Illuminated	            12	IHOREG	Lit by flood lights, strip lights, etc.
            Historic	            13	IHOREG	Famous in history; of historical interest.
            Public	                14	IHOREG	Belonging to, available to, used or shared by, the community as a whole and not restricted to private use.
            Synchronized	        15	IHOREG	Occur at a time, coincide in point of time, be contemporary or simultaneous.
            Watched	                16	IHOREG	Looked at or observed over a period of time especially so as to be aware of any movement or change.
            Unwatched	            17	IHOREG	Usually automatic in operation, without any permanently-stationed personnel to superintend it.
            Existence Doubtful	    18	IHOREG	A feature that has been reported but has not been definitely determined to exist.
            Buoyed	                28	IHOREG	Marked by buoys.

            */


            if (!string.IsNullOrEmpty(featureStatus)) {
                /* See S-101 DCEG clause 5.4 for the listing of allowable values. Values populated in S-57 for this attribute
                    other than the allowable values will not be converted across to S-101. Data Producers are advised to
                    check any populated values for STATUS on LNDARE and amend appropriately. */
                foreach (var c in featureStatus.Split(',', StringSplitOptions.RemoveEmptyEntries)) {
                    int? e = c.ToLowerInvariant() switch {
                        "1" => 1,   //status.Permanent,
                        "2" => 2,   //status.Occasional,
                        "3" => 3,   //status.Recommended,
                        "4" => 4,   //status.NotInUse,
                        "5" => 5,   //status.PeriodicIntermittent,
                        "6" => 6,   //status.Reserved,
                        "7" => 7,   //status.Temporary,
                        "8" => 8,   //status.Private,
                        "9" => 9,   //status.Mandatory,
                        "11" => 11,   //status.Extinguished,
                        "12" => 12,   //status.Illuminated,
                        "13" => 13,   //status.Historic,
                        "14" => 14,   //status.Public,
                        "15" => 15,   //status.Synchronized,
                        "16" => 16,   //status.Watched,
                        "17" => 17,   //status.Unwatched,
                        "18" => 18,   //status.ExistenceDoubtful,
                                      //"28" => ??, // TODO: what to do? STATUS 28
                        "-32767" => null,
                        _ => throw new IndexOutOfRangeException(),
                    };
                    if (e.HasValue) {
                        statusList.Add(e.Value);
                    }
                }

            }
            return statusList.ToArray();
        }



        /*
                code	condition
                alias	CONDTN
                _s101name	Condition
                definition	The various conditions of buildings and other constructions.
                valueType	enumeration
                listedValues	
                Under Construction	    1	IHOREG	Being built but not yet capable of function.
                Ruined	                2	IHOREG	A structure in a decayed or deteriorated condition resulting from neglect or disuse, or a damaged structure in need of repair.
                Under Reclamation	    3	IHOREG	An area of the sea, a lake or the navigable part of a river that is being reclaimed as land, usually by the dumping of earth and other material.
                Wingless	            4	IHOREG	A windmill or wind turbine from which the vanes or turbine blades are missing.
                Planned Construction	5	IHOREG	Detailed planning has been completed but construction has not been initiated.

         */

        //public static colour GetColour(string value) {
        //    return conditionValue switch {
        //        1 => condition.UnderConstruction,      // under construction
        //        2 => condition.Ruined,                 // ruined
        //        3 => condition.UnderReclamation,       // under reclamation
        //        5 => condition.PlannedConstruction,    // planned construction
        //        -32767 => (condition)(-1),                        // unknown or no condition
        //        _ => throw new IndexOutOfRangeException("Invalid condition value.")  // Invalid condition value
        //    };
        //}



        public static condition? GetCondition(int conditionValue) {
            return conditionValue switch {
                1 => 1,         // under construction
                2 => 2,         // ruined
                3 => 3,         // under reclamation
                5 => 5,         // planned construction
                -32767 => null, // unknown or no condition
                _ => throw new IndexOutOfRangeException("Invalid condition value.")
            };
        }


        internal static featureName[]? GetFeatureName(string? objname, string? nobjnme) {
            List<featureName> featureName = [];
            if (objname != default) {
                var objnam = objname.Trim();
                if (!string.IsNullOrEmpty(objnam) && !"-32767".Equals(objnam)) {
                    var item = new featureName {
                        language = "eng",
                        name = objnam,
                        nameUsage = 1    //nameUsage.DefaultNameDisplay,
                    };
                    featureName.Add(item);
                }
            }
            if (nobjnme != default) {
                var nobjnm = nobjnme.Trim();
                if (!string.IsNullOrEmpty(nobjnm) && !"-32767".Equals(nobjnm)) {
                    var item = new featureName {
                        language = "dan",
                        name = nobjnm,
                        nameUsage = 2    //nameUsage.AlternateNameDisplay,
                    };
                    featureName.Add(item);
                }
            }

            if (featureName.Any())
                return featureName.ToArray();
            return null;
        }

#if true
        internal static InformationResult BindNauticalInformationFrom(long sourceObjectid, string? sourceTableName, string? ntxtds, string? txtdsc, string? inform, string? ninform, string? pubref) {

            //if (System.Diagnostics.Debugger.IsAttached && "DKNOT101.TXT".Equals(inform, StringComparison.InvariantCultureIgnoreCase)) System.Diagnostics.Debugger.Break();

            InformationResult result = new();

            var createNauticalInformation = (string fileReference, string language) => {
                return new NauticalInformation {
                    information = [
                    new information() {
                        fileReference = FixFilename(fileReference) ?? default,
                        language = language
                    }]
                };
            };

            if (pubref is not null)
                pubref = pubref.Trim();

            if (!string.IsNullOrEmpty(pubref)) {
                createNauticalInformation = (string fileReference, string language) => {
                    return new NauticalInformation {
                        information = [
                            new information() {
                            fileReference = FixFilename(fileReference) ?? default,
                            language = language,
                            headline = pubref.Equals("-32767") ? null : pubref,
                        }]
                    };
                };
            }

            if (!string.IsNullOrEmpty(ntxtds)) {
                // TODO: make information binding -> Nautical Information - binding.
                if (!string.IsNullOrEmpty(ntxtds) && ntxtds.EndsWith(".txt", StringComparison.InvariantCultureIgnoreCase)) {

                    var file = IO.Directory.GetFiles(_notesPath, $"{ntxtds}", SearchOption.AllDirectories).FirstOrDefault();
                    if (string.IsNullOrEmpty(file))
                        Logger.Current.Error($"{sourceTableName}::{sourceObjectid} [ntxtds] {ntxtds}");
                    else if (file.Contains("Retired", StringComparison.CurrentCultureIgnoreCase))
                        Logger.Current.Warning("{tablename}::{objectid} ntxds {file}", sourceTableName, sourceObjectid, file);

                    string fileReference = ntxtds;
                    string language = "eng";

                    var instance = createNauticalInformation(fileReference, language);
                    result.InformationBindings.Add(NauticalInformations.Instance.Add(instance.information[0]!.fileReference!, instance));
                }
                else if (!string.IsNullOrEmpty(ntxtds)) {
                    string language = "eng";

                    var instance = new information {
                        language = language,
                        text = ntxtds?.Trim(),
                    };
                    result.information.Add(instance);
                }
            }

            if (!string.IsNullOrEmpty(txtdsc)) {
                // TODO: make information binding -> Nautical Information - binding.
                if (!string.IsNullOrEmpty(txtdsc) && txtdsc.EndsWith(".txt", StringComparison.InvariantCultureIgnoreCase)) {
                    var file = IO.Directory.GetFiles(_notesPath, $"{txtdsc}", SearchOption.AllDirectories).FirstOrDefault();
                    if (string.IsNullOrEmpty(file))
                        Logger.Current.Error($"{sourceTableName}::{sourceObjectid} [txtdsc] {txtdsc}");
                    else if (file.Contains("Retired", StringComparison.CurrentCultureIgnoreCase))
                        Logger.Current.Warning("{tablename}::{objectid} txtdsc {file}", sourceTableName, sourceObjectid, file);


                    string fileReference = txtdsc;
                    string language = "eng";

                    var instance = createNauticalInformation(fileReference, language);
                    result.InformationBindings.Add(NauticalInformations.Instance.Add(instance.information[0]!.fileReference!, instance));
                }
                else if (!string.IsNullOrEmpty(txtdsc)) {
                    string fileReference = txtdsc;
                    string language = "eng";

                    var instance = new information {
                        language = language,
                        text = txtdsc?.Trim(),
                    };
                    result.information.Add(instance);
                }
            }

            if (!string.IsNullOrEmpty(inform)) {
                //https://geodatastyrelsen.atlassian.net/wiki/spaces/SOEKORT/pages/4404478463/S-65+Annex+B+Appendix+A+-+Impact+analysis
                // Separate discrete information populated in INFORM using a standard separator such as semicolon “;”.

                string[] informs = inform != null ? inform.Split(';') : Array.Empty<string>();

                foreach (var value in informs) {
                    if (!string.IsNullOrEmpty(value) && value.EndsWith(".txt", StringComparison.InvariantCultureIgnoreCase)) {
                        var file = IO.Directory.GetFiles(_notesPath, $"{value}", SearchOption.AllDirectories).FirstOrDefault();
                        if (string.IsNullOrEmpty(file))
                            Logger.Current.Error($"{sourceTableName}::{sourceObjectid} [inform] {inform}");
                        else if (file.Contains("Retired", StringComparison.CurrentCultureIgnoreCase))
                            Logger.Current.Warning("{tablename}::{objectid} inform {file}", sourceTableName, sourceObjectid, file);

                        string fileReference = value;
                        string language = "eng";

                        var instance = createNauticalInformation(fileReference, language);
                        result.InformationBindings.Add(NauticalInformations.Instance.Add(instance.information[0]!.fileReference!, instance));
                    }
                    else if (!string.IsNullOrEmpty(value)) {
                        string fileReference = value;
                        string language = "eng";

                        var instance = new information {
                            language = language,
                            text = value?.Trim(),
                        };
                        result.information.Add(instance);
                    }
                }
            }

            if (!string.IsNullOrEmpty(ninform)) {
                // https://geodatastyrelsen.atlassian.net/wiki/spaces/SOEKORT/pages/4404478463/S-65+Annex+B+Appendix+A+-+Impact+analysis
                // Separate discrete information populated in INFORM using a standard separator such as semicolon “;”.
                if (!string.IsNullOrEmpty(ninform)) {

                    string[] ninfoms = ninform != null ? ninform.Split(';') : Array.Empty<string>();

                    foreach (var value in ninfoms) {

                        if (!string.IsNullOrEmpty(value) && value.EndsWith(".txt", StringComparison.InvariantCultureIgnoreCase)) {
                            var file = IO.Directory.GetFiles(_notesPath, $"{value}", SearchOption.AllDirectories).FirstOrDefault();
                            if (string.IsNullOrEmpty(file))
                                Logger.Current.Error($"{sourceTableName}::{sourceObjectid} [ninform] {ninform}");
                            else if (file.Contains("Retired", StringComparison.CurrentCultureIgnoreCase))
                                Logger.Current.Warning("{tablename}::{objectid} ninfoms {file}", sourceTableName, sourceObjectid, file);


                            string fileReference = value;
                            string language = "dan";

                            var instance = createNauticalInformation(fileReference, language);
                            result.InformationBindings.Add(NauticalInformations.Instance.Add(instance.information[0]!.fileReference!, instance));
                        }
                        else if (!string.IsNullOrEmpty(value)) {
                            string fileReference = value;
                            string language = "dan";

                            var instance = new information {
                                language = language,
                                text = value?.Trim(),
                            };
                            result.information.Add(instance);
                        }
                    }
                }
            }
            return result;
        }

#endif
#if false
        internal static InformationResult BindNauticalInformationFrom(long sourceObjectid, string? sourceTableName, string? ntxtds, string? txtdsc, string? inform, string? ninform, string? pubref) {

            //if (System.Diagnostics.Debugger.IsAttached && "DKNOT101.TXT".Equals(inform, StringComparison.InvariantCultureIgnoreCase)) System.Diagnostics.Debugger.Break();

            if (pubref is not null)
                pubref = pubref.Trim();

            InformationResult result = new();

            //var createNauticalInformation = (string fileReference, string language) => {
            //    return new NauticalInformation {
            //        information = [
            //        new information() {
            //            fileReference = FixFilename(fileReference) ?? default,
            //            language = language
            //        }]
            //    };
            //};


            //if (!string.IsNullOrEmpty(pubref)) {
            //    createNauticalInformation = (string fileReference, string language) => {
            //        return new NauticalInformation {
            //            information = [
            //                new information() {
            //                fileReference = FixFilename(fileReference) ?? default,
            //                language = language,
            //                headline = pubref.Equals("-32767") ? null : pubref,
            //            }]
            //        };
            //    };
            //}

            NauticalInformation nauticalInformation = new NauticalInformation();

            var createInformation = (string fileReference, string language) => {
                return new information() {
                    fileReference = FixFilename(fileReference) ?? default,
                    language = language
                };
            };


            if (!string.IsNullOrEmpty(pubref)) {
                createInformation = (string fileReference, string language) => {
                    return new information() {
                        fileReference = FixFilename(fileReference) ?? default,
                        language = language,
                        headline = pubref.Equals("-32767") ? null : pubref,
                    };
                };
            }

            if (!string.IsNullOrEmpty(ntxtds)) {
                // TODO: make information binding -> Nautical Information - binding.
                if (!string.IsNullOrEmpty(ntxtds) && ntxtds.EndsWith(".txt", StringComparison.InvariantCultureIgnoreCase)) {

                    var file = IO.Directory.GetFiles(_notesPath, $"{ntxtds}", SearchOption.AllDirectories).FirstOrDefault();
                    if (string.IsNullOrEmpty(file))
                        Logger.Current.Error($"{sourceTableName}::{sourceObjectid} [ntxtds] {ntxtds}");
                    else if (file.Contains("Retired", StringComparison.CurrentCultureIgnoreCase))
                        Logger.Current.Warning("{tablename}::{objectid} ntxds {file}", sourceTableName, sourceObjectid, file);

                    string fileReference = ntxtds;
                    string language = "eng";

                    var instance = createInformation(fileReference, language);
                    nauticalInformation.information = [.. nauticalInformation.information, instance];
                    //result.InformationBindings.Add(NauticalInformations.Instance.Add(instance.information[0]!.fileReference!, instance));
                }
                else if (!string.IsNullOrEmpty(ntxtds)) {
                    string language = "eng";

                    var instance = new information {
                        language = language,
                        text = ntxtds?.Trim(),
                    };
                    result.information.Add(instance);
                }
            }

            if (!string.IsNullOrEmpty(txtdsc)) {
                // TODO: make information binding -> Nautical Information - binding.
                if (!string.IsNullOrEmpty(txtdsc) && txtdsc.EndsWith(".txt", StringComparison.InvariantCultureIgnoreCase)) {
                    var file = IO.Directory.GetFiles(_notesPath, $"{txtdsc}", SearchOption.AllDirectories).FirstOrDefault();
                    if (string.IsNullOrEmpty(file))
                        Logger.Current.Error($"{sourceTableName}::{sourceObjectid} [txtdsc] {txtdsc}");
                    else if (file.Contains("Retired", StringComparison.CurrentCultureIgnoreCase))
                        Logger.Current.Warning("{tablename}::{objectid} txtdsc {file}", sourceTableName, sourceObjectid, file);


                    string fileReference = txtdsc;
                    string language = "eng";

                    var instance = createInformation(fileReference, language);
                    nauticalInformation.information = [.. nauticalInformation.information, instance];
                    //result.InformationBindings.Add(NauticalInformations.Instance.Add(instance.information[0]!.fileReference!, instance));
                }
                else if (!string.IsNullOrEmpty(txtdsc)) {
                    string fileReference = txtdsc;
                    string language = "eng";

                    var instance = new information {
                        language = language,
                        text = txtdsc?.Trim(),
                    };
                    result.information.Add(instance);
                }
            }

            if (!string.IsNullOrEmpty(inform)) {
                //https://geodatastyrelsen.atlassian.net/wiki/spaces/SOEKORT/pages/4404478463/S-65+Annex+B+Appendix+A+-+Impact+analysis
                // Separate discrete information populated in INFORM using a standard separator such as semicolon “;”.

                string[] informs = inform != null ? inform.Split(';') : Array.Empty<string>();

                foreach (var value in informs) {
                    if (!string.IsNullOrEmpty(value) && value.EndsWith(".txt", StringComparison.InvariantCultureIgnoreCase)) {
                        var file = IO.Directory.GetFiles(_notesPath, $"{value}", SearchOption.AllDirectories).FirstOrDefault();
                        if (string.IsNullOrEmpty(file))
                            Logger.Current.Error($"{sourceTableName}::{sourceObjectid} [inform] {inform}");
                        else if (file.Contains("Retired", StringComparison.CurrentCultureIgnoreCase))
                            Logger.Current.Warning("{tablename}::{objectid} inform {file}", sourceTableName, sourceObjectid, file);

                        string fileReference = value;
                        string language = "eng";

                        var instance = createInformation(fileReference, language);
                        nauticalInformation.information = [.. nauticalInformation.information, instance];
                        //result.InformationBindings.Add(NauticalInformations.Instance.Add(instance.information[0]!.fileReference!, instance));
                    }
                    else if (!string.IsNullOrEmpty(value)) {
                        string fileReference = value;
                        string language = "eng";

                        var instance = new information {
                            language = language,
                            text = value?.Trim(),
                        };
                        result.information.Add(instance);
                    }
                }
            }

            if (!string.IsNullOrEmpty(ninform)) {
                // https://geodatastyrelsen.atlassian.net/wiki/spaces/SOEKORT/pages/4404478463/S-65+Annex+B+Appendix+A+-+Impact+analysis
                // Separate discrete information populated in INFORM using a standard separator such as semicolon “;”.
                if (!string.IsNullOrEmpty(ninform)) {

                    string[] ninfoms = ninform != null ? ninform.Split(';') : Array.Empty<string>();

                    foreach (var value in ninfoms) {

                        if (!string.IsNullOrEmpty(value) && value.EndsWith(".txt", StringComparison.InvariantCultureIgnoreCase)) {
                            var file = IO.Directory.GetFiles(_notesPath, $"{value}", SearchOption.AllDirectories).FirstOrDefault();
                            if (string.IsNullOrEmpty(file))
                                Logger.Current.Error($"{sourceTableName}::{sourceObjectid} [ninform] {ninform}");
                            else if (file.Contains("Retired", StringComparison.CurrentCultureIgnoreCase))
                                Logger.Current.Warning("{tablename}::{objectid} ninfoms {file}", sourceTableName, sourceObjectid, file);


                            string fileReference = value;
                            string language = "dan";

                            var instance = createInformation(fileReference, language);
                            nauticalInformation.information = [.. nauticalInformation.information, instance];
                            //result.InformationBindings.Add(NauticalInformations.Instance.Add(instance.information[0]!.fileReference!, instance));
                        }
                        else if (!string.IsNullOrEmpty(value)) {
                            string fileReference = value;
                            string language = "dan";

                            var instance = new information {
                                language = language,
                                text = value?.Trim(),
                            };
                            result.information.Add(instance);
                        }
                    }
                }
            }

            if (nauticalInformation.information.Any()) {
                if (nauticalInformation.information.Count() > 1) System.Diagnostics.Debugger.Break();
                var additionalInformation = NauticalInformations.CreateAdditionalInformation(nauticalInformation);
                result.InformationBindings.Add(additionalInformation);
            }

            return result;
        }
#endif
        internal static string?[] GetCommunicationChannel(string input) {
            var result = new List<string>();
            if (string.IsNullOrWhiteSpace(input)) return [];

            var tokens = input.Split(';');
            foreach (var token in tokens) {
                var trimmed = token.Trim();
                if (!trimmed.StartsWith("[") || !trimmed.EndsWith("]")) {
                    result.Add(trimmed); // Unrecognized format, keep as is
                    continue;
                }

                var content = trimmed.Substring(1, trimmed.Length - 2);

                if (Regex.IsMatch(content, @"[A-Za-z]")) {
                    var match = Regex.Match(content, @"^([A-Za-z]+)(\d+)$");
                    if (match.Success) {
                        var prefix = match.Groups[1].Value;
                        var number = int.Parse(match.Groups[2].Value).ToString("D4");
                        result.Add($"[{prefix}{number}]");
                    }
                    else {
                        result.Add(trimmed);
                    }
                }
                else {
                    if (int.TryParse(content, out int number)) {
                        var formatted = $"[VHF{number:D4}]";
                        result.Add(formatted);
                    }
                    else {
                        result.Add(trimmed);
                    }
                }
            }

            return result.ToArray();
        }

        internal static string? FixFilename(string fileReference) {
            if (fileReference == default) {
                return default;
            }

            return $"101{fileReference.Substring(0, 2)}00{fileReference.Substring(2)}";

            string result = Regex.Replace(fileReference, @"^[dk]", match => {
                string matched = match.Value;

                string replacement = "101";

                replacement += char.IsUpper(matched[0]) ? 'D' : 'd';
                replacement += char.IsUpper(matched[1]) ? 'K' : 'k';
                replacement += "00";

                return replacement;
            }, RegexOptions.IgnoreCase);

            return result;
        }

        //internal static NauticalInformation CreateNauticalInformation(string picrep, string datsta, string datend, string persta, string perend, List<information> information) {
        //    NauticalInformation nobj = new NauticalInformation();
        //    if (picrep != default) {
        //        nobj.pictorialRepresentation = ImporterNIS.FixFilename(picrep) ?? default;
        //    }

        //    nobj.information = information;
        //    nobj.Code = ps101;

        //    DateHelper.TryGetFixedDateRange(datsta, datend, out var dateRange);
        //    if (dateRange != default) {
        //        nobj.fixedDateRange = dateRange;
        //    }

        //    DateHelper.TryGetPeriodicDateRange(persta, perend, out var periodicDateRange);
        //    if (periodicDateRange != default) {
        //        nobj.periodicDateRange = periodicDateRange;
        //    }

        //    return nobj;
        //}

        //internal static void AddInformation(List<information> instanceInformation, Row current) {
        //    List<information> information = CreateInformationFrom(current);
        //    instanceInformation.AddRange(information);
        //}
        internal static InformationResult AddInformation(long sourceObjectid, string? sourceTableName, string? ntxtds, string? txtdsc, string? inform, string? ninform, string? pubref = default) {
            // TODO: TBD.
            //List<information> information = CreateInformationFrom(sourceObjectid, sourceTableName, ntxtds, txtdsc, inform, ninform);
            //instanceInformation.AddRange(information);

            //TODO: Fix binding
            return BindNauticalInformationFrom(sourceObjectid, sourceTableName, ntxtds, txtdsc, inform, ninform, pubref);

            //if (!result.information.Any())
            //    return [];
            //instanceInformation.AddRange(result.information);

            //return result.InformationBindings;
        }

        internal static string EsriJsonToGeoJson(string esriJson) {
            var esri = JsonNode.Parse(esriJson)!.AsObject();

            // Polygon
            if (esri.TryGetPropertyValue("rings", out var ringsNode)) {
                var geo = new JsonObject {
                    ["type"] = "Polygon",
                    ["coordinates"] = ringsNode!.Deserialize<JsonArray>()!
                };
                return geo.ToJsonString(new JsonSerializerOptions { WriteIndented = true });
            }

            // Polyline
            if (esri.TryGetPropertyValue("paths", out var pathsNode)) {
                var geo = new JsonObject {
                    ["type"] = "MultiLineString",
                    ["coordinates"] = pathsNode!.Deserialize<JsonArray>()!
                };
                return geo.ToJsonString(new JsonSerializerOptions { WriteIndented = true });
            }

            // Point
            if (esri.TryGetPropertyValue("x", out var xNode) &&
                esri.TryGetPropertyValue("y", out var yNode)) {
                var geo = new JsonObject {
                    ["type"] = "Point",
                    ["coordinates"] = new JsonArray(
                        xNode!.GetValue<double>(),
                        yNode!.GetValue<double>()
                    )
                };
                return geo.ToJsonString(new JsonSerializerOptions { WriteIndented = true });
            }

            throw new NotSupportedException("Unsupported EsriJSON geometry type.");
        }

        public static List<List<(string key, Polygon polygon)>> GroupTouchingPolygons((string key, Polygon polygon)[] polygons, Func<(string key, Polygon polygon), (string key, Polygon polygon), bool> _adjacent) {
            int count = polygons.Length;

            // --- Union-Find setup ---
            int[] parent = new int[count];
            int[] rank = new int[count];
            for (int i = 0; i < count; i++) parent[i] = i;

            int Find(int x) {
                if (parent[x] != x) parent[x] = Find(parent[x]); // path compression
                return parent[x];
            }

            void Union(int a, int b) {
                int ra = Find(a), rb = Find(b);
                if (ra == rb) return;
                if (rank[ra] < rank[rb]) (ra, rb) = (rb, ra);
                parent[rb] = ra;
                if (rank[ra] == rank[rb]) rank[ra]++;
            }

            // --- Check each pair ---
            var engine = GeometryEngine.Instance;

            for (int i = 0; i < count; i++) {
                for (int j = i + 1; j < count; j++) {
                    bool _ = _adjacent(polygons[i], polygons[j]);
                    if (_)
                        Union(i, j);
                }
            }

            // --- Collect groups ---
            var groups = new Dictionary<int, List<(string, Polygon)>>();

            for (int i = 0; i < count; i++) {
                int root = Find(i);
                if (!groups.ContainsKey(root))
                    groups[root] = [];
                groups[root].Add(polygons[i]);
            }

            return [.. groups.Values];
        }
    }
}


namespace ArcGIS.Core.Geometry
{
    public static class Extension
    {
        public static void Clone(this Feature feature, RowBuffer buffer, string[] exclude) {
            var fields = feature.GetFields();
            for (int i = 0; i < fields.Count; i++) {
                if (exclude.Contains(fields[i].Name.ToLowerInvariant())) {
                    buffer[i] = DBNull.Value;
                }
                else if (!fields[i].IsEditable)
                    ;   // buffer[i] = fields[0].GetDefaultValue();
                else
                    buffer[i] = feature[i];
            }
        }

        public static Polygon[] Split(this Polygon? polygon) {
            if (polygon is null) return [];

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

                return polygons;
            }
            return [polygon];
        }
    }
}

