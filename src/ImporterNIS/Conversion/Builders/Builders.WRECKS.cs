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

        private static Wreck WRECKS(Feature current, RowBuffer buffer, QueryFilter filter) {
            var instance = new Wreck();

            // action point #42 Attributes converted correctly but the combination of both is prohibited in S-101 (DCEG 13.5). Ignore/ drop CATWRK when VALSOU is populated on conversion.
            if (current.CATWRK_HasValue() && !current.VALSOU_HasValue()) {
                if (EnumHelper.GetEnumValue(current.CATWRK(), out int? categoryOfWreck, instance.attributeBindingDefinition("categoryOfWreck")!.permitedValues!))
                    instance.categoryOfWreck = categoryOfWreck;
            }

            if (current.EXPSOU_HasValue()) {
                if (EnumHelper.GetEnumValue(current.EXPSOU(), out int? expositionOfSounding, instance.attributeBindingDefinition("expositionOfSounding")!.permitedValues!))
                    instance.expositionOfSounding = expositionOfSounding;
            }

            var featureName = GetFeatureName(current.OBJNAM(), current.NOBJNM());
            if (featureName is not null)
                instance.featureName = featureName;

            if (current.HEIGHT_HasValue()) {
                instance.height = current.HEIGHT() != -32767m ? current.HEIGHT() : null;
            }

            if (current.QUASOU_HasValue()) {
                var qualityOfVerticalMeasurement = EnumHelper.GetEnumValues(current.QUASOU(), instance.attributeBindingDefinition("qualityOfVerticalMeasurement")!.permitedValues!);
                if (qualityOfVerticalMeasurement is not null && qualityOfVerticalMeasurement.Any())
                    instance.qualityOfVerticalMeasurement = qualityOfVerticalMeasurement;
            }

            if (current.CONRAD_HasValue()) {
                instance.radarConspicuous = current.CONRAD() == 2 ? false : true;
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
                    instance.status = status;
            }

            if (current.TECSOU_HasValue()) {
                var techniqueOfVerticalMeasurement = EnumHelper.GetEnumValues(current.TECSOU(), instance.attributeBindingDefinition("techniqueOfVerticalMeasurement")!.permitedValues!);
                if (techniqueOfVerticalMeasurement is not null && techniqueOfVerticalMeasurement.Any())
                    instance.techniqueOfVerticalMeasurement = techniqueOfVerticalMeasurement;
            }

            if (current.VALSOU_HasValue()) {
                instance.valueOfSounding = current.VALSOU() != -32767m ? current.VALSOU() : null;
            }

            if (current.CONVIS_HasValue()) {
                if (EnumHelper.GetEnumValue(current.CONVIS(), out int? visualProminence, instance.attributeBindingDefinition("visualProminence")!.permitedValues!))
                    instance.visualProminence = visualProminence;
            }

            if (current.WATLEV_HasValue()) {
                if (EnumHelper.GetEnumValue(current.WATLEV(), out int? waterLevelEffect, instance.attributeBindingDefinition("waterLevelEffect")!.permitedValues!))
                    instance.waterLevelEffect = waterLevelEffect;
            }

            if (current.PLTS_COMP_SCALE_HasValue()) {
                string subtype = "";

                if (!Subtypes.Instance.TryGetSubtype(current.TableName(), current.FCSubtype(), out subtype))
                    throw new NotSupportedException($"Unknown subtype for {current.TableName}, {current.FCSubtype()}");

                var scaleMinimum = Scamin.Instance.GetMinimumScale(current, subtype, current.PLTS_COMP_SCALE()!.Value, isRelatedToStructure: false);
                if (scaleMinimum.HasValue)
                    instance.scaleMinimum = scaleMinimum;
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

            //  Table 30.3 - Values for default clearance depth – Wreck features
            var catwrk = current.CATWRK_HasValue() ? current.CATWRK() : null;
            var expsou = current.EXPSOU_HasValue() ? current.EXPSOU() : null;
            var height = current.HEIGHT_HasValue() ? current.HEIGHT() : null;
            var valsou = current.VALSOU_HasValue() ? current.VALSOU() : null;
            var watlev = current.WATLEV_HasValue() ? current.WATLEV() : null;

            if (leastDepth.HasValue) {
                if (
                    (catwrk.HasValue && (catwrk.Value == 4 || catwrk.Value == 5)) &&
                    (height.HasValue && height.Value != -32676m) &&
                    (watlev.HasValue && (watlev.Value == 1 || watlev.Value == 2 || watlev.Value == -32767))
                    ) {
                    //  / (null)
                }
                else if (
                        (valsou.HasValue && valsou.Value != -32767m) &&
                        (watlev.HasValue && (watlev.Value == 3 || watlev.Value == 4 || watlev.Value == 5 || watlev.Value == -32767))
                    ) {
                    //  / (null)
                }
                else if (
                        (expsou.HasValue && (expsou.Value == 1 || expsou.Value == 3)) &&
                        (valsou.HasValue && valsou.Value == -32767m) &&
                        (watlev.HasValue && watlev.Value == 3)
                    ) {
                    instance.defaultClearanceDepth = leastDepth;
                }
                else if (
                        (expsou.HasValue && (expsou.Value == 1 || expsou.Value == 3)) &&
                        (watlev.HasValue && watlev.Value == 3)
                    ) {
                    instance.defaultClearanceDepth = leastDepth;
                }
                else if (
                        (catwrk.HasValue && catwrk.Value == 1) &&
                        (watlev.HasValue && (watlev.Value == 1 || watlev.Value == 2 || watlev.Value == 4 || watlev.Value == 5 || watlev.Value == -32767))
                    ) {
                    //  20.1 or {Least Depth – 66} (whichever value is larger)
                    var _ = leastDepth!.Value - 66m;
                    instance.defaultClearanceDepth = 20.1m > _ ? 20.1m : _;
                }
                else if (
                        (catwrk.HasValue && catwrk.Value == 1) &&
                        (expsou.HasValue && (expsou.Value == 2 || expsou.Value == -32767m))
                    ) {
                    //  20.1 or {Least Depth – 66} (whichever value is larger)
                    var _ = leastDepth!.Value - 66m;
                    instance.defaultClearanceDepth = 20.1m > _ ? 20.1m : _;
                }
                else if (   //  GST
                        (catwrk.HasValue && catwrk.Value == 1)
                    //(expsou.HasValue && (expsou.Value == 2 || expsou.Value == -32767m))
                    ) {
                    instance.expositionOfSounding = null;

                    //  20.1 or {Least Depth – 66} (whichever value is larger)
                    var _ = leastDepth!.Value - 66m;
                    instance.defaultClearanceDepth = 20.1m > _ ? 20.1m : _;
                }
                else if (
                        (expsou.HasValue && (expsou.Value == 2 || expsou.Value == -32767m)) &&
                        (valsou.HasValue && valsou.Value == -32767m) &&
                        (watlev.HasValue && (watlev.Value == 3 || watlev.Value == 5))
                    ) {
                    instance.defaultClearanceDepth = 0m;
                }
                else if (   //  GST
                        (!expsou.HasValue) &&
                        (valsou.HasValue && valsou.Value == -32767m) &&
                        (watlev.HasValue && (watlev.Value == 3 || watlev.Value == 5))
                    ) {
                    instance.expositionOfSounding = null;
                    instance.defaultClearanceDepth = 0m;
                }
                else if (
                        (expsou.HasValue && (expsou.Value == 2 || expsou.Value == -32767m)) &&
                        (valsou.HasValue && valsou.Value == -32767m) &&
                        (watlev.HasValue && (watlev.Value == 4 || watlev.Value == -32767m))
                    ) {
                    instance.defaultClearanceDepth = 15m;
                }
                else if (   //  GST
                        (!expsou.HasValue) &&
                        (valsou.HasValue && valsou.Value == -32767m) &&
                        (watlev.HasValue && (watlev.Value == 4 || watlev.Value == -32767m))
                    ) {
                    instance.expositionOfSounding = null;
                    instance.defaultClearanceDepth = 15m;
                }
                else if (
                        (catwrk.HasValue && (catwrk.Value == 2 || catwrk.Value == 3 || catwrk.Value == 4 || catwrk.Value == 5 || catwrk.Value == -32767)) &&
                        (watlev.HasValue && (watlev.Value == 1 || watlev.Value == 2 || watlev.Value == 4 || watlev.Value == 5 || watlev.Value == -32767m))
                    ) {
                    instance.defaultClearanceDepth = 15m;
                }
                else if (
                        (catwrk.HasValue && (catwrk.Value == 2 || catwrk.Value == 3 || catwrk.Value == 4 || catwrk.Value == 5 || catwrk.Value == -32767)) &&
                        (expsou.HasValue && (expsou.Value == 2 || expsou.Value == -32767m))
                    ) {
                    instance.defaultClearanceDepth = 15m;
                }
                else if (   //  GST
                        (catwrk.HasValue && (catwrk.Value == 2 || catwrk.Value == 3 || catwrk.Value == 4 || catwrk.Value == 5 || catwrk.Value == -32767))
                    //(expsou.HasValue && (expsou.Value == 2 || expsou.Value == -32767m))
                    ) {
                    instance.expositionOfSounding = null;
                    instance.defaultClearanceDepth = 15m;
                }
                else {
                    //  Hit me with a baseball bat
                    Logger.Current.DataError(current.GetObjectID(), current.TableName(), current.LNAM() ?? "Unknown LNAM", $"defaultClearanceDepth WRECKS! catwrk:{catwrk}, expsou:{expsou}, height:{height}, valsou:{valsou}, watlev:{watlev}, leastDepth:{leastDepth}");
                }
            }
            else {
                if (
                        (catwrk.HasValue && catwrk.Value == 1) &&
                        (watlev.HasValue && (watlev.Value == 3 || watlev.Value == -32767))
                    ) {
                    instance.defaultClearanceDepth = 20.1m;
                }
                else if (
                        (valsou.HasValue && valsou.Value == -32767m) &&
                        (watlev.HasValue && (watlev.Value == 3 || watlev.Value == 5))
                    ) {
                    instance.defaultClearanceDepth = 0m;
                }
                else if (
                        (valsou.HasValue && valsou.Value == -32767m) &&
                        (watlev.HasValue && (watlev.Value == 4 || watlev.Value == -32767))
                    ) {
                    instance.defaultClearanceDepth = -15m;
                }
                else if (
                        (catwrk.HasValue && catwrk.Value == -32767) &&
                        (watlev.HasValue && (watlev.Value == 3 || watlev.Value == 5))
                    ) {
                    instance.defaultClearanceDepth = 0m;
                }
                else if (
                        (catwrk.HasValue && (catwrk.Value == 2 || catwrk.Value == 3 || catwrk.Value == 4 || catwrk.Value == 5)) &&
                        (watlev.HasValue && (watlev.Value == 3 || watlev.Value == 5))
                    ) {
                    instance.defaultClearanceDepth = -15m;
                }
                else if (
                        (catwrk.HasValue && (catwrk.Value == 2 || catwrk.Value == 3 || catwrk.Value == 4 || catwrk.Value == 5 || catwrk.Value == -32767)) &&
                        (watlev.HasValue && (watlev.Value == 4 || watlev.Value == -32767))
                    ) {
                    instance.defaultClearanceDepth = -15m;
                }
                else {
                    //  Hit me with a baseball bat
                    Logger.Current.DataError(current.GetObjectID(), current.TableName(), current.LNAM() ?? "Unknown LNAM", $"defaultClearanceDepth WRECKS! catwrk:{catwrk}, expsou:{expsou}, height:{height}, valsou:{valsou}, watlev:{watlev}, leastDepth:{leastDepth}");
                }
            }

            if (!instance.valueOfSounding.HasValue && !instance.defaultClearanceDepth.HasValue) {
                //Logger.Current.Error("!instance.valueOfSounding.HasValue && !defaultClearanceDepth (WRECKS#{objectid})", current.GetObjectID());
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

