using ArcGIS.Core.Data;
using S100FC;
using S100FC.S101.ComplexAttributes;
using S100FC.S101.FeatureTypes;
using S100Framework.Applications.Singletons;
using System.Text.RegularExpressions;

namespace S100Framework.Applications
{
    using ArcGIS.Core.Data.UtilityNetwork.Trace;
    using ArcGIS.Core.Geometry;
    using S100FC.S101.InformationTypes;
    using S100FC.S101.SimpleAttributes;
    using S100Framework.Applications.S57.esri;

    internal static partial class ImporterNIS
    {

        private static RecommendedTrack RECTRC(Feature current, RowBuffer buffer) {
            var instance = new RecommendedTrack();

            if (current.CATTRK_HasValue()) {
                if (current.CATTRK() == 1) {
                    instance.basedOnFixedMarks = true;
                }
                else if (current.CATTRK() == 2) {
                    instance.basedOnFixedMarks = false;
                }
                else {
                    Logger.Current.DataError(current.GetObjectID(), current.TableName(), current.LNAM() ?? string.Empty, $"Cannot convert value {current.CATTRK()} to basedOnFixedMarks boolean. Only values 1 and 2 are supported.");
                }
            }

            if (current.DRVAL1_HasValue()) {
                instance.depthRangeMinimumValue = current.DRVAL1() != -32767m ? current.DRVAL1() : null;
            }

            var featureName = GetFeatureName(current.OBJNAM(), current.NOBJNM());
            if (featureName is not null)
                instance.featureName = featureName;

            DateHelper.TryGetFixedDateRange(current.DATSTA(), current.DATEND(), out var dateRange);
            if (dateRange != default) {
                instance.fixedDateRange = dateRange;
            }

            var inform = current.INFORM();
            if (!string.IsNullOrEmpty(inform) && regexMaximumDraughtPermitted.IsMatch(inform)) {
                var _value = regexMaximumDraughtPermitted.Match(inform).Groups["value"]?.Value;

                if (decimal.TryParse(_value, out decimal value)) {
                    instance.maximumPermittedDraught = value;
                }
            }

            if (current.ORIENT_HasValue()) {
                instance.orientationValue = current.ORIENT() != -32767m ? current.ORIENT() : null;
            }

            DateHelper.TryGetPeriodicDateRange(current.PERSTA(), current.PEREND(), out var periodicDateRange);
            if (periodicDateRange != default) {
                instance.periodicDateRange = periodicDateRange;
            }

            if (current.QUASOU_HasValue()) {
                var qualityOfVerticalMeasurement = EnumHelper.GetEnumValues(current.QUASOU(), instance.attributeBindingDefinition("qualityOfVerticalMeasurement")!.permitedValues!);
                if (qualityOfVerticalMeasurement is not null && qualityOfVerticalMeasurement.Any())
                    instance.qualityOfVerticalMeasurement = qualityOfVerticalMeasurement;
            }

            if (current.STATUS_HasValue()) {
                var status = EnumHelper.GetEnumValues(current.STATUS(), instance.attributeBindingDefinition("status")!.permitedValues!);
                if (status is not null && status.Any())
                    instance.status = status;
            }

            if (current.TECSOU_HasValue()) {
                var techniqueOfVerticalMeasurement = EnumHelper.GetEnumValues(current.TECSOU(), instance.attributeBindingDefinition("techniqueOfVerticalMeasurement")!.permitedValues!);
                if (techniqueOfVerticalMeasurement is not null && techniqueOfVerticalMeasurement.Any())
                    instance.techniqueOfVerticalMeasurement = techniqueOfVerticalMeasurement;
            }

            if (current.TRAFIC_HasValue()) {
                if (EnumHelper.GetEnumValue(current.TRAFIC(), out int? trafficFlow, instance.attributeBindingDefinition("trafficFlow")!.permitedValues!))
                    instance.trafficFlow = trafficFlow;
            }

            if (current.SOUACC_HasValue()) {
                instance.verticalUncertainty = new() {
                    uncertaintyFixed = current.SOUACC() != -32767m ? current.SOUACC() : null,
                };
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

            buffer["ps"] = ps101;
            buffer["code"] = instance.GetType().Name;
            buffer["attributebindings"] = instance.Flatten();
            buffer["informationbindings"] = System.Text.Json.JsonSerializer.Serialize(instance.GetInformationBindings(), jsonSerializerOptions);

            buffer["sourceIdentifier"] = instance.sourceIdentifier;
            SetShape(buffer, current.SHAPE());
            SetCompilationScale(buffer, current.PLTS_COMP_SCALE()!.Value);

            return instance;
        }
    }
}

