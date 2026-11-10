using ArcGIS.Core.Data;
using S100FC;
using S100FC.S101.FeatureTypes;
using S100Framework.Applications.Singletons;

namespace S100Framework.Applications
{
    using ArcGIS.Core.Geometry;
    using S100Framework.Applications.S57.esri;

    internal static partial class ImporterNIS
    {

        private static FeatureType SOUNDG(Feature current) {
            //var shape = (MapPoint)current.SHAPE()!;

            //SetShape(buffer, MultipointBuilderEx.CreateMultipoint(mappoint));

            var depth = current.DEPTH()!.Value;
            var quasou = current.QUASOU() ?? default;
            var quapos = current.P_QUAPOS() ?? default;
            var tecsou = current.TECSOU() ?? default;

            //var mappoint = MapPointBuilderEx.CreateMapPoint(shape.X, shape.Y, Convert.ToDouble(depth), shape.SpatialReference);

            if (quasou == default || string.IsNullOrEmpty(quasou) || !string.Equals(quasou, "5", StringComparison.OrdinalIgnoreCase)) {
                var instance = new Sounding();

                if (current.QUASOU_HasValue()) {
                    var qualityOfVerticalMeasurement = EnumHelper.GetEnumValues(current.QUASOU(), instance.attributeBindingDefinition("qualityOfVerticalMeasurement")!.permitedValues!);
                    if (qualityOfVerticalMeasurement is not null && qualityOfVerticalMeasurement.Any())
                        instance.qualityOfVerticalMeasurement = qualityOfVerticalMeasurement;
                }

                if (current.TECSOU_HasValue()) {
                    var techniqueOfVerticalMeasurement = EnumHelper.GetEnumValues(current.TECSOU(), instance.attributeBindingDefinition("techniqueOfVerticalMeasurement")!.permitedValues!);
                    if (techniqueOfVerticalMeasurement is not null && techniqueOfVerticalMeasurement.Any())
                        instance.techniqueOfVerticalMeasurement = techniqueOfVerticalMeasurement;
                }

                var featureName = GetFeatureName(current.OBJNAM(), current.NOBJNM());
                if (featureName is not null)
                    instance.featureName = featureName;

                if (current.SORDAT_HasValue()) {
                    if (DateHelper.TryConvertSordat(current.SORDAT()!, out var reportedDate)) {
                        instance.reportedDate = reportedDate;
                    }
                    else {
                        Logger.Current.DataError(current.GetObjectID(), current.TableName(), current.LNAM() ?? "Unknown LNAM", $"Cannot convert date {current.SORDAT()}");
                    }
                }

                if (current.STATUS_HasValue()) {
                    var status = EnumHelper.GetEnumValues(current.STATUS(), instance.attributeBindingDefinition("status")!.permitedValues!);
                    if (status is not null && status.Any())
                        instance.status = status[0];
                }

                if (current.TECSOU_HasValue()) {
                    var techniqueOfVerticalMeasurement = EnumHelper.GetEnumValues(current.TECSOU(), instance.attributeBindingDefinition("techniqueOfVerticalMeasurement")!.permitedValues!);
                    if (techniqueOfVerticalMeasurement is not null && techniqueOfVerticalMeasurement.Any())
                        instance.techniqueOfVerticalMeasurement = techniqueOfVerticalMeasurement;
                }

                if (current.PLTS_COMP_SCALE_HasValue()) {
                    string subtype = "";

                    if (!Subtypes.Instance.TryGetSubtype(current.TableName(), current.FCSubtype(), out subtype))
                        throw new NotSupportedException($"Unknown subtype for {current.TableName}, {current.FCSubtype()}");

                    var scaleMinimum = Scamin.Instance.GetMinimumScale(current, subtype, current.PLTS_COMP_SCALE()!.Value, isRelatedToStructure: false);
                    if (scaleMinimum.HasValue)
                        instance.scaleMinimum = scaleMinimum;
                }

                var result = ImporterNIS.AddInformation(current.GetObjectID(), current.TableName(), current.NTXTDS(), current.TXTDSC(), current.INFORM(), current.NINFOM());
                instance.information = [.. result.information];
                instance.SetInformationBindings(result.InformationBindings.ToArray());

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

                return instance;
            }
            else {
                /*  SOUNDG with attribute QUASOU = 5 (no bottom found at value shown) will be converted to an
                    instance of the S-101 Feature _s101type Depth – No Bottom Found. Where this is the case, the attributes
                    EXPSOU, NOBJNM, OBJNAM, SOUACC and STATUS will not be converted. It is considered that
                    these attributes are not relevant for Depth – No Bottom Found in S-101. */
                var instance = new DepthNoBottomFound();

                if (current.TECSOU_HasValue()) {
                    instance.techniqueOfVerticalMeasurement = [.. current.TECSOU()!.Split(',').Select(e => EnumHelper.GetEnumValue(e))];
                }

                if (current.PLTS_COMP_SCALE_HasValue()) {
                    string subtype = "";

                    if (!Subtypes.Instance.TryGetSubtype(current.TableName(), current.FCSubtype(), out subtype))
                        throw new NotSupportedException($"Unknown subtype for {current.TableName}, {current.FCSubtype()}");

                    var scaleMinimum = Scamin.Instance.GetMinimumScale(current, subtype, current.PLTS_COMP_SCALE()!.Value, isRelatedToStructure: false);
                    if (scaleMinimum.HasValue)
                        instance.scaleMinimum = scaleMinimum;
                }

                var result = ImporterNIS.AddInformation(current.GetObjectID(), current.TableName(), current.NTXTDS(), current.TXTDSC(), current.INFORM(), current.NINFOM());
                instance.information = [.. result.information];
                instance.SetInformationBindings(result.InformationBindings.ToArray());

                return instance;
            }
        }
    }
}

