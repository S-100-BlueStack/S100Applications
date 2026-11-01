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

        private static QualityOfSurvey M_SREL(Feature current, RowBuffer buffer) {
            var instance = new QualityOfSurvey();

            if (current.DRVAL1_HasValue()) {
                instance.depthRangeMinimumValue = current.DRVAL1() != -32767m ? current.DRVAL1() : null;
            }
            if (current.DRVAL2_HasValue()) {
                instance.depthRangeMaximumValue = current.DRVAL2() != -32767m ? current.DRVAL2() : null;
            }

            // TODO: featuresdetected

            // TODO: full seafloor covearge achieved

            // TODO: line spacing maximum

            // TODO: line spacing minimum

            if (current.SDISMX_HasValue()) {
                if (current.SDISMX() == -32767m) {
                    instance.measurementDistanceMaximum = null;
                }
                else {
                    if (current.SDISMX() % 1 == 0) {
                        instance.measurementDistanceMaximum = Convert.ToInt32(current.SDISMX());
                    }
                    else {
                        Logger.Current.DataError(current.GetObjectID(), current.LNAM() ?? "Empty LNAM", current.TableName() ?? "Unknown tablename", $"SDISMX on M_SREL: value is {current.SDISMX} and cannot be converted to an integer");
                    }
                }
            }

            if (current.SDISMN_HasValue()) {
                if (current.SDISMN() == -32767m) {
                    instance.measurementDistanceMaximum = null;
                }
                else {
                    if (current.SDISMN() % 1 == 0) {
                        instance.measurementDistanceMaximum = Convert.ToInt32(current.SDISMN());
                    }
                    else {
                        Logger.Current.DataError(current.GetObjectID(), current.LNAM() ?? "Empty LNAM", current.TableName() ?? "Unknown tablename", $"SDISMN on M_SREL: value is {current.SDISMN} and cannot be converted to an integer");
                    }
                }
            }

            if (current.QUAPOS_HasValue()) {
                instance.qualityOfHorizontalMeasurement = current.QUAPOS() switch {
                    4 => 4, //qualityOfHorizontalMeasurement.Approximate,
                    _ => default,
                };
            }

            if (current.QUASOU_HasValue()) {
                var qualityOfVerticalMeasurement = EnumHelper.GetEnumValues(current.QUASOU(), instance.attributeBindingDefinition("qualityOfVerticalMeasurement")!.permitedValues!);
                if (qualityOfVerticalMeasurement is not null && qualityOfVerticalMeasurement.Any())
                    instance.qualityOfVerticalMeasurement = qualityOfVerticalMeasurement;
            }

            if (current.SCVAL1_HasValue()) {
                instance.scaleValueMaximum = current.SCVAL1() == -32767 ? null : current.SCVAL1();
            }

            if (current.SCVAL2_HasValue()) {
                instance.scaleValueMinimum = current.SCVAL2() == -32767 ? null : current.SCVAL2();
            }

            if (current.SURATH_HasValue()) {
                instance.surveyAuthority = "-32767".Equals(current.SURATH()) ? null : current.SURATH();
            }

            if (DateHelper.TryGetSurveyDateRange(current.SURSTA(), current.SUREND(), out var surveyDateRange)) {
                instance.surveyDateRange = surveyDateRange!;
            }

            if (current.SURTYP_HasValue()) {
                var surveyType = EnumHelper.GetEnumValues(current.SURTYP());
                if (surveyType is not null)
                    instance.surveyType = surveyType;
            }

            if (current.TECSOU_HasValue()) {
                var techniqueOfVerticalMeasurement = EnumHelper.GetEnumValues(current.TECSOU(), instance.attributeBindingDefinition("techniqueOfVerticalMeasurement")!.permitedValues!);
                if (techniqueOfVerticalMeasurement is not null && techniqueOfVerticalMeasurement.Any())
                    instance.techniqueOfVerticalMeasurement = techniqueOfVerticalMeasurement;
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

