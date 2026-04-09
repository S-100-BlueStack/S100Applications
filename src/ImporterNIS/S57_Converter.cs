using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using S100FC;
using S100FC.S101;
using S100FC.S101.ComplexAttributes;
using S100FC.S101.FeatureTypes;
using S100FC.S101.SimpleAttributes;
using S100Framework.Applications.S57.esri;
using S100Framework.Applications.Singletons;
using System.Text.Json;

namespace S100Framework.Applications
{
    public static partial class S57_Converter
    {
        internal static string ps101 = S100FC.S101.Summary.ProductId;

        public static void SOUNDG(Feature feature, RowBuffer buffer, Func<S57Object, RowBuffer, long> insert) {
            var current = new SoundingsP(feature);

            var objectid = current.OBJECTID ?? default;
            var globalid = current.GLOBALID;

            var tableName = current.TableName!;

            var longname = current.LNAM ?? Strings.UNKNOWN;
            var fcSubtype = current.FCSUBTYPE ?? default;
            var depth = current.DEPTH ?? default;
            var quasou = current.QUASOU ?? default;
            var quapos = current.P_QUAPOS ?? default;
            var tecsou = current.TECSOU ?? default;
            var objnam = current.OBJNAM ?? default;
            var nobjnm = current.NOBJNM ?? default;

            FeatureType? instance = default;

            switch (fcSubtype) {
                case 1:
                    var shape = current.SHAPE as MapPoint;
                    if (shape == default) {
                        Logger.Current.DataError(objectid, tableName, longname, Strings.ERR_NULL_SHAPE);
                        return;
                    }

                    var mappoint = MapPointBuilderEx.CreateMapPoint(shape.X, shape.Y, Convert.ToDouble(depth), shape.SpatialReference);

                    SetShape(buffer, mappoint);

                    if (quasou == default || !string.Equals(quasou, "5", StringComparison.OrdinalIgnoreCase)) {
                        var sounding = new Sounding {
                        };
                        if (quasou != default) {
                            sounding.qualityOfVerticalMeasurement = [EnumHelper.GetEnumValue(quasou)];
                        }
                        if (tecsou != default && !string.IsNullOrEmpty(tecsou)) {
                            sounding.techniqueOfVerticalMeasurement = [EnumHelper.GetEnumValue(tecsou)];
                        }

                        var featureName = GetFeatureName(current.OBJNAM, current.NOBJNM);
                        if (featureName is not null && featureName.Any())
                            sounding.featureName = featureName;

                        if (current.QUASOU != default) {
                            if (current.QUASOU != "-32767") {
                                var qualityOfVerticalMeasurement = EnumHelper.GetEnumValues(current.QUASOU);
                                if (qualityOfVerticalMeasurement is not null && qualityOfVerticalMeasurement.Any())
                                    sounding.qualityOfVerticalMeasurement = qualityOfVerticalMeasurement;
                            }
                        }

                        if (!string.IsNullOrEmpty(current.SORDAT)) {
                            if (DateHelper.TryConvertSordat(current.SORDAT, out var reportedDate)) {
                                sounding.reportedDate = reportedDate;
                            }
                            else {
                                Logger.Current.DataError(current.OBJECTID ?? -1, current.GetType().Name, current.LNAM ?? "Unknown LNAM", $"Cannot convert date {current.SORDAT}");
                            }
                        }

                        if (current.STATUS != default) {
                            sounding.status = GetSingleStatus(current.STATUS)?.value;
                        }

                        if (current.TECSOU != null) {
                            var techniqueOfVerticalMeasurement = EnumHelper.GetEnumValues(current.TECSOU);
                            if (techniqueOfVerticalMeasurement is not null && techniqueOfVerticalMeasurement.Any())
                                sounding.techniqueOfVerticalMeasurement = techniqueOfVerticalMeasurement;
                        }

                        if (current.PLTS_COMP_SCALE.HasValue && current.SHAPE != null) {
                            string subtype = "";

                            if (current.TableName != default && current.FCSUBTYPE.HasValue && !Subtypes.Instance.TryGetSubtype(current.TableName, current.FCSUBTYPE.Value, out subtype))
                                throw new NotSupportedException($"Unknown subtype for {current.TableName}, {current.FCSUBTYPE.Value}");

                            sounding.scaleMinimum = Scamin.Instance.GetMinimumScale(current, subtype, current.PLTS_COMP_SCALE!.Value, isRelatedToStructure: false);
                        }

                        buffer["attributebindings"] = sounding.Flatten();
                        buffer["ps"] = ps101;
                        buffer["code"] = sounding.GetType().Name;

                        var result = ImporterNIS.AddInformation(current.OBJECTID!.Value, current.TableName!, current.NTXTDS, current.TXTDSC, current.INFORM, current.NINFOM);
                        sounding.information = result.information.ToArray();
                        sounding.SetInformationBindings(result.InformationBindings.ToArray());

                        instance = sounding;

                        // TODO: Handle Spatialquality
                        //if (quapos != default && quapos == 4) {
                        //    /*  SOUNDG with attribute QUAPOS = 4 (approximate) will also be converted to an instance of the S101 Information _s101type Spatial Quality (see S-101 DCEG clause 24.5), attribute quality of horizontal
                        //        measurement = 4 (approximate), associated to the geometry of the Sounding feature using the
                        //        association Spatial Association. */
                        //    using var information = informationtype.CreateRowBuffer();

                        //    var row = new SpatialQuality {
                        //        qualityOfHorizontalMeasurement = qualityOfHorizontalMeasurement.Approximate,
                        //    };

                        //    information["ps"] = ps101;
                        //    information["code"] = row.GetType().Name;
                        //    information["json"] = System.Text.Json.JsonSerializer.Serialize(row);
                        //    using var _ = informationtype.CreateRow(information);
                        //}
                    }
                    else {
                        /*  SOUNDG with attribute QUASOU = 5 (no bottom found at value shown) will be converted to an
                            instance of the S-101 Feature _s101type Depth – No Bottom Found. Where this is the case, the attributes
                            EXPSOU, NOBJNM, OBJNAM, SOUACC and STATUS will not be converted. It is considered that
                            these attributes are not relevant for Depth – No Bottom Found in S-101. */
                        var depthNoBottomFound = new DepthNoBottomFound();

                        // TODO: interoperabilityIdentifier

                        if (current.TECSOU != null) {
                            var techniqueOfVerticalMeasurement = EnumHelper.GetEnumValues(current.TECSOU);
                            if (techniqueOfVerticalMeasurement is not null && techniqueOfVerticalMeasurement.Any())
                                depthNoBottomFound.techniqueOfVerticalMeasurement = techniqueOfVerticalMeasurement;
                        }

                        if (current.PLTS_COMP_SCALE.HasValue && current.SHAPE != null) {
                            string subtype = "";

                            if (current.TableName != default && current.FCSUBTYPE.HasValue && !Subtypes.Instance.TryGetSubtype(current.TableName, current.FCSUBTYPE.Value, out subtype))
                                throw new NotSupportedException($"Unknown subtype for {current.TableName}, {current.FCSUBTYPE.Value}");

                            var scamin = Scamin.Instance.GetMinimumScale(current, subtype, current.PLTS_COMP_SCALE!.Value, isRelatedToStructure: false);
                            if (scamin.HasValue)
                                depthNoBottomFound.scaleMinimum = scamin.Value;
                        }


                        var result = ImporterNIS.AddInformation(current.OBJECTID!.Value, current.TableName!, current.NTXTDS, current.TXTDSC, current.INFORM, current.NINFOM);
                        depthNoBottomFound.information = result.information.ToArray();
                        depthNoBottomFound.SetInformationBindings(result.InformationBindings.ToArray());

                        instance = depthNoBottomFound;
                    }

                    var oid = insert(current, buffer);

                    ConversionAnalytics.Instance.AddConverted(tableName, current.GLOBALID, oid.ToString()); Logger.Current.DataObject(objectid, tableName, longname, System.Text.Json.JsonSerializer.Serialize(instance, jsonSerializerOptions));
                    Logger.Current.DataObject(objectid, tableName, longname, System.Text.Json.JsonSerializer.Serialize(instance, jsonSerializerOptions));
                    break;

                default:
                    // code block
                    System.Diagnostics.Debugger.Break();
                    break;

            }
        }
    }

    public static partial class S57_Converter
    {
        internal static readonly JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions {
            WriteIndented = false,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            PropertyNameCaseInsensitive = true,
        }.AppendTypeInfoResolver();

        private static status GetSingleStatus(string status) => GetStatus(status)[0];

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
                        "-32767" => default,
                        _ => throw new IndexOutOfRangeException(),
                    };
                    if (e.HasValue) {
                        statusList.Add(e.Value);
                    }
                }

            }
            return statusList.ToArray();
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

        internal static featureName[]? GetFeatureName(string? objname, string? nobjnme) {
            List<featureName> featureName = [];
            if (objname != default) {
                var objnam = objname.Trim();
                if (!string.IsNullOrEmpty(objnam)) {
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
                if (!string.IsNullOrEmpty(nobjnm)) {
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
    }
}
