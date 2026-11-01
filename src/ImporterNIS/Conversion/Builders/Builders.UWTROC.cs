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

        private static UnderwaterAwashRock UWTROC(Feature current, RowBuffer buffer, QueryFilter filter) {
            var instance = new UnderwaterAwashRock();

            if (current.EXPSOU_HasValue()) {
                if (EnumHelper.GetEnumValue(current.EXPSOU(), out int? expositionOfSounding, instance.attributeBindingDefinition("expositionOfSounding")!.permitedValues!))
                    instance.expositionOfSounding = expositionOfSounding;
            }

            var featureName = GetFeatureName(current.OBJNAM(), current.NOBJNM());
            if (featureName is not null)
                instance.featureName = featureName;

            if (current.NATSUR_HasValue()) {
                if (EnumHelper.GetEnumValue(current.NATSUR(), out int? natureOfSurface, instance.attributeBindingDefinition("natureOfSurface")!.permitedValues!))
                    instance.natureOfSurface = natureOfSurface;
            }

            if (current.QUASOU_HasValue()) {
                var qualityOfVerticalMeasurement = EnumHelper.GetEnumValues(current.QUASOU(), instance.attributeBindingDefinition("qualityOfVerticalMeasurement")!.permitedValues!);
                if (qualityOfVerticalMeasurement is not null && qualityOfVerticalMeasurement.Any())
                    instance.qualityOfVerticalMeasurement = qualityOfVerticalMeasurement;
            }

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

            if (current.VALSOU_HasValue()) {
                instance.valueOfSounding = current.VALSOU() != -32767m ? current.VALSOU() : null;
            }

            if (current.WATLEV_HasValue()) {
                if (EnumHelper.GetEnumValue(current.WATLEV(), out int? waterLevelEffect, instance.attributeBindingDefinition("waterLevelEffect")!.permitedValues!))
                    instance.waterLevelEffect = waterLevelEffect;
            }


            bool coveredByUnsurveyedArea = false;
            bool coveredByDredgedArea = false;
            decimal? leastDepth = null;

            var surroundingDepths = ImporterNIS.GetSurrounding_DepthArea(current.Shape()!, (Geodatabase)current.GetTable().GetDatastore(), filter.WhereClause).ToList();
            if (surroundingDepths.Any()) {
                var surroundingDepth = surroundingDepths.OrderByDescending(e => e.DRVAL1!.Value).Last();

                leastDepth = surroundingDepth.DRVAL1.HasValue ? surroundingDepth.DRVAL1.Value : null;
                if (surroundingDepth.FcSubtype == 15) {  // UNSARE
                    coveredByUnsurveyedArea = true;
                }
                if (surroundingDepth.FcSubtype == 5) {  // DRGARE
                    coveredByDredgedArea = true;
                    instance.surroundingDepth = leastDepth != -32767m ? leastDepth : null;
                }
                if (surroundingDepth.FcSubtype == 1) {  // DEPARE
                    instance.surroundingDepth = leastDepth != -32767m ? leastDepth : null;
                }

                instance.surroundingDepth = leastDepth != -32767m ? leastDepth : null;
            }

            //  Table 30.2 - Values for default clearance depth – Underwater/Awash Rock features
            var expsou = current.EXPSOU_HasValue() ? current.EXPSOU() : null;
            var watlev = current.WATLEV_HasValue() ? current.WATLEV() : null;
            var valsou = current.VALSOU_HasValue() ? current.VALSOU() : null;

            if (leastDepth.HasValue) {
                if (
                    (valsou.HasValue && valsou.Value != -32767m)
                    ) {
                    //  / (null)
                }
                else if (
                        (expsou.HasValue && (expsou.Value == 1 || expsou.Value == 3)) &&
                        (valsou.HasValue && valsou.Value == -32767m) &&
                        (watlev.HasValue && watlev.Value == 3)
                    ) {
                    instance.defaultClearanceDepth = leastDepth!.Value;
                }
                else if (
                        (expsou.HasValue && (expsou.Value == 2 || expsou.Value == -32767)) &&
                        (valsou.HasValue && valsou.Value == -32767m) &&
                        (watlev.HasValue && watlev.Value == 3)
                    ) {
                    instance.defaultClearanceDepth = 0.1m;
                }
                else if (
                        (!expsou.HasValue) &&
                        (valsou.HasValue && valsou.Value == -32767m) &&
                        (watlev.HasValue && watlev.Value == 3)
                    ) {
                    instance.expositionOfSounding = null;
                    instance.defaultClearanceDepth = 0.1m;
                }
                else if (
                        (expsou.HasValue && (expsou.Value == 2 || expsou.Value == -32767)) &&
                        (valsou.HasValue && valsou.Value == -32767m) &&
                        (watlev.HasValue && watlev.Value == 5)
                    ) {
                    instance.defaultClearanceDepth = 0m;
                }
                else if (
                        (!expsou.HasValue) &&
                        (valsou.HasValue && valsou.Value == -32767m) &&
                        (watlev.HasValue && watlev.Value == 5)
                    ) {
                    instance.expositionOfSounding = null;
                    instance.defaultClearanceDepth = 0m;
                }
                else if (
                        (expsou.HasValue && (expsou.Value == 2 || expsou.Value == -32767)) &&
                        (valsou.HasValue && valsou.Value == -32767m) &&
                        (watlev.HasValue && (watlev.Value == 4 || watlev.Value == -32767))
                    ) {
                    instance.defaultClearanceDepth = -15m;
                }
                else if (
                        (!expsou.HasValue) &&
                        (valsou.HasValue && valsou.Value == -32767m) &&
                        (watlev.HasValue && (watlev.Value == 4 || watlev.Value == -32767))
                    ) {
                    instance.expositionOfSounding = null;
                    instance.defaultClearanceDepth = -15m;
                }
                else {
                    //  Hit me with a baseball bat
                    Logger.Current.DataError(current.GetObjectID(), current.TableName(), current.LNAM() ?? "Unknown LNAM", $"defaultClearanceDepth UWTROC! expsou:{expsou}, valsou:{valsou}, watlev:{watlev}, leastDepth:{leastDepth}");
                }
            }
            else {
                if (
                        (valsou.HasValue && valsou.Value != -32767m) &&
                        (watlev.HasValue && watlev.Value == 3)
                    ) {
                    instance.defaultClearanceDepth = 0.1m;
                }
                else if (
                        (valsou.HasValue && valsou.Value != -32767m) &&
                        (watlev.HasValue && watlev.Value == 5)
                    ) {
                    instance.defaultClearanceDepth = 0m;
                }
                else if (
                        (valsou.HasValue && valsou.Value != -32767m) &&
                        (watlev.HasValue && (watlev.Value == 4 || watlev.Value == -32767))
                    ) {
                    instance.defaultClearanceDepth = -15m;
                }
                else {
                    //  Hit me with a baseball bat
                    Logger.Current.DataError(current.GetObjectID(), current.TableName(), current.LNAM() ?? "Unknown LNAM", $"defaultClearanceDepth UWTROC! expsou:{expsou}, valsou:{valsou}, watlev:{watlev}, leastDepth:{leastDepth}");
                }
            }

            if (!instance.valueOfSounding.HasValue && !instance.defaultClearanceDepth.HasValue) {
                //Logger.Current.DataError(current.GetObjectID(), current.TableName(), current.LNAM() ?? "Unknown LNAM", $"defaultClearanceDepth UWTROC! expsou:{expsou}, valsou:{valsou}, watlev:{watlev}, leastDepth:{leastDepth}");
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

