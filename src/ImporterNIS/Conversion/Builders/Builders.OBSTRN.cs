using ArcGIS.Core.Data;
using S100FC;
using S100FC.S101.FeatureTypes;
using S100Framework.Applications.Singletons;

namespace S100Framework.Applications
{
    using S100Framework.Applications.S57.esri;

    internal static partial class ImporterNIS
    {

        private static FeatureType OBSTRN(Feature current, QueryFilter filter) {
            if (!string.IsNullOrEmpty(current.INFORM()) && regexDam.IsMatch(current.INFORM()!)) {
                var instance = new Dam();

                if (current.CONDTN_HasValue()) {
                    instance.condition = GetCondition(current.CONDTN()!.Value)?.value;
                }

                var featureName = GetFeatureName(current.OBJNAM(), current.NOBJNM());
                if (featureName is not null)
                    instance.featureName = featureName;

                DateHelper.TryGetFixedDateRange(current.DATSTA(), current.DATEND(), out var dateRange);
                if (dateRange != default) {
                    instance.fixedDateRange = dateRange;
                }
                if (current.HEIGHT_HasValue()) {
                    instance.height = current.HEIGHT() != -32767m ? current.HEIGHT() : null;
                }

                if (current.NATCON_HasValue()) {
                    var natureOfConstruction = EnumHelper.GetEnumValues(current.NATCON);
                    if (natureOfConstruction is not null && natureOfConstruction.Any())
                        instance.natureOfConstruction = natureOfConstruction;
                }

                if (current.CONRAD_HasValue()) {
                    instance.radarConspicuous = current.CONRAD() == 2 ? false : true;
                }

                if (current.STATUS_HasValue()) {
                    var status = EnumHelper.GetEnumValues(current.STATUS(), instance.attributeBindingDefinition("status")!.permitedValues!);
                    if (status is not null && status.Any())
                        instance.status = status;
                }

                if (current.VERLEN_HasValue()) {
                    instance.verticalLength = current.VERLEN() != -32767m ? current.VERLEN() : null;
                }

                if (current.CONVIS_HasValue()) {
                    if (EnumHelper.GetEnumValue(current.CONVIS(), out int? visualProminence, instance.attributeBindingDefinition("visualProminence")!.permitedValues!))
                        instance.visualProminence = visualProminence;
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
            else if (current.CATOBS_HasValue() && current.CATOBS() == 7) {
                var instance = new FoulGround();

                var featureName = GetFeatureName(current.OBJNAM(), current.NOBJNM());
                if (featureName is not null)
                    instance.featureName = featureName;

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

                return instance;
            }
            else {
                var instance = new Obstruction();

                if (current.CATOBS_HasValue()) {
                    if (EnumHelper.GetEnumValue(current.CATOBS(), out int? categoryOfObstruction, instance.attributeBindingDefinition("categoryOfObstruction")!.permitedValues!))
                        instance.categoryOfObstruction = categoryOfObstruction;
                }

                if (current.CONDTN_HasValue()) {
                    instance.condition = GetCondition(current.CONDTN()!.Value)?.value;
                }

                if (current.EXPSOU_HasValue()) {
                    if (EnumHelper.GetEnumValue(current.EXPSOU(), out int? expositionOfSounding, instance.attributeBindingDefinition("expositionOfSounding")!.permitedValues!))
                        instance.expositionOfSounding = expositionOfSounding;
                }

                var featureName = GetFeatureName(current.OBJNAM(), current.NOBJNM());
                if (featureName is not null)
                    instance.featureName = featureName;

                //if (current.HEIGHT_HasValue()) {
                //    instance.height = current.HEIGHT() != -32767m ? current.HEIGHT() : null;
                //}

                var inform = current.INFORM();
                if (!string.IsNullOrEmpty(inform) && regexMaximumDraughtPermitted.IsMatch(inform)) {
                    var _value = regexMaximumDraughtPermitted.Match(inform).Groups["value"]?.Value;

                    if (decimal.TryParse(_value, out decimal value)) {
                        instance.maximumPermittedDraught = value;
                    }
                }

                if (current.NATSUR_HasValue()) {
                    var natureOfSurface = EnumHelper.GetEnumValues(current.NATSUR(), instance.attributeBindingDefinition("natureOfSurface")!.permitedValues!);
                    if (natureOfSurface is not null && natureOfSurface.Any())
                        instance.natureOfSurface = natureOfSurface;
                }

                if (current.PRODCT_HasValue()) {
                    var product = EnumHelper.GetEnumValues(current.PRODCT());
                    if (product is not null && product.Any())
                        instance.product = product;
                }

                // TODO: QualityOfVerticalMeasurement
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

                //if (current.VALSOU_HasValue()) {
                //    instance.valueOfSounding = current.VALSOU() != -32767m ? current.VALSOU() : null;
                //}

                if (current.VERLEN_HasValue()) {
                    instance.verticalLength = current.VERLEN() != -32767m ? current.VERLEN() : null;
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
                decimal? leastDepth = null;

                var surroundingDepths = ImporterNIS.GetSurrounding_DepthArea(current.Shape()!, (Geodatabase)current.GetTable().GetDatastore(), filter.WhereClause).ToList();
                if (surroundingDepths.Any()) {
                    var surroundingDepth = surroundingDepths.OrderByDescending(e => e.DRVAL1!.Value).Last();

                    leastDepth = surroundingDepth.DRVAL1.HasValue ? surroundingDepth.DRVAL1.Value : null;
                    if (surroundingDepth.FcSubtype == 15) {  // UNSARE
                    }
                    if (surroundingDepth.FcSubtype == 5) {  // DRGARE
                        instance.surroundingDepth = leastDepth != -32767m ? leastDepth : null;
                    }
                    if (surroundingDepth.FcSubtype == 1) {  // DEPARE
                        instance.surroundingDepth = leastDepth != -32767m ? leastDepth : null;
                    }

                    instance.surroundingDepth = leastDepth != -32767m ? leastDepth : null;
                }

                //  Table 30.1 - Values for default clearance depth – Obstruction features
                var catobs = current.CATOBS_HasValue() ? current.CATOBS() : null;
                var expsou = current.EXPSOU_HasValue() ? current.EXPSOU() : null;
                var height = current.HEIGHT_HasValue() ? current.HEIGHT() : null;
                var valsou = current.VALSOU_HasValue() ? current.VALSOU() : null;
                var watlev = current.WATLEV_HasValue() ? current.WATLEV() : null;

                if ((valsou.HasValue && valsou.Value == -32767m) && (height.HasValue && height.Value == -32767m))
                    System.Diagnostics.Debugger.Break();

                if (current.VALSOU_HasValue()) {
                    if (!(watlev.HasValue && watlev.Value == 7)) { //  GST
                        instance.valueOfSounding = current.VALSOU() != -32767m ? current.VALSOU() : null;
                    }
                }

                if (current.HEIGHT_HasValue()) {
                    if (!current.VALSOU_HasValue())    //  Exactly one of the attributes height or value of sounding must be populated.
                        instance.height = current.HEIGHT() != -32767m ? current.HEIGHT() : null;
                    else if (watlev.HasValue && watlev.Value == 7) { //  GST
                        instance.height = null;
                    }
                }
                else if (watlev.HasValue && watlev.Value == 7) { //  GST
                    instance.height = null;
                }

                if (leastDepth.HasValue) {
                    //if ((!catobs.HasValue && !expsou.HasValue && !height.HasValue) && (watlev.HasValue && watlev.Value == 7)) { //  GST
                    //    instance.height = null;
                    //}
                    //else if ((catobs.HasValue && catobs.Value == 10) && (!expsou.HasValue && !height.HasValue) && (watlev.HasValue && watlev.Value == 7)) { //  GST
                    //    instance.height = null;
                    //}
                    if (
                            (height.HasValue && height.Value != -32767m) &&
                            (watlev.HasValue && (watlev.Value == 1 || watlev.Value == 2))
                        ) {
                        //  / (null)
                    }
                    else if (
                            (height.HasValue && height.Value == -32767m) &&
                            (watlev.HasValue && (watlev.Value == 1 || watlev.Value == 2 || watlev.Value == 7))
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
                            (catobs.HasValue && catobs.Value == 6) &&
                            (expsou.HasValue && (expsou.Value == 2 || expsou.Value == -32767m)) &&
                            (valsou.HasValue && valsou.Value == -32767m)
                        ) {
                        instance.defaultClearanceDepth = 0.1m;
                    }
                    else if (   //  GST
                            (catobs.HasValue && catobs.Value == 6) &&
                            (!expsou.HasValue) &&
                            (valsou.HasValue && valsou.Value == -32767m)
                        ) {
                        instance.expositionOfSounding = null;
                        instance.defaultClearanceDepth = 0.1m;
                    }
                    else if (
                            (expsou.HasValue && (expsou.Value == 2 || expsou.Value == -32767m)) &&
                            (valsou.HasValue && valsou.Value == -32767m) &&
                            (watlev.HasValue && watlev.Value == 3)
                        ) {
                        instance.defaultClearanceDepth = 0.1m;
                    }
                    else if (   //  GST
                            (!expsou.HasValue) &&
                            (valsou.HasValue && valsou.Value == -32767m) &&
                            (watlev.HasValue && watlev.Value == 3)
                        ) {
                        instance.expositionOfSounding = null;
                        instance.defaultClearanceDepth = 0.1m;
                    }
                    else if (
                            (catobs.HasValue && catobs.Value != 6) &&
                            (expsou.HasValue && (expsou.Value == 2 || expsou.Value == -32767m)) &&
                            (valsou.HasValue && valsou.Value == -32767m) &&
                            (watlev.HasValue && watlev.Value == 5)
                        ) {
                        instance.defaultClearanceDepth = 0m;
                    }
                    else if (   //  GST
                            (catobs.HasValue && catobs.Value != 6) &&
                            (!expsou.HasValue) &&
                            (valsou.HasValue && valsou.Value == -32767m) &&
                            (watlev.HasValue && watlev.Value == 5)
                        ) {
                        instance.expositionOfSounding = null;
                        instance.defaultClearanceDepth = 0m;
                    }
                    else if (
                            (catobs.HasValue && catobs.Value != 6) &&
                            (expsou.HasValue && (expsou.Value == 2 || expsou.Value == -32767m)) &&
                            (valsou.HasValue && valsou.Value == -32767m) &&
                            (watlev.HasValue && (watlev.Value == 4 || watlev.Value == -32767))
                        ) {
                        instance.defaultClearanceDepth = -15m;
                    }
                    else if (   //  GST
                            !(catobs.HasValue && catobs.Value == 6) &&
                            //(!expsou.HasValue) &&
                            (valsou.HasValue && valsou.Value == -32767m) &&
                            (watlev.HasValue && (watlev.Value == 4 || watlev.Value == -32767))
                        ) {
                        instance.expositionOfSounding = null;
                        instance.defaultClearanceDepth = -15m;
                    }
                    else {
                        //  Hit me with a baseball bat
                        Logger.Current.DataError(current.GetObjectID(), current.TableName(), current.LNAM() ?? "Unknown LNAM", $"defaultClearanceDepth OBSTRN! catobs:{catobs}, expsou:{expsou}, height:{height}, valsou:{valsou}, watlev:{watlev}, leastDepth:{leastDepth}");
                    }
                }
                else {
                    //if ((!catobs.HasValue && !expsou.HasValue && !height.HasValue) && (watlev.HasValue && watlev.Value == 7)) { //  GST
                    //    instance.height = null;
                    //}
                    //else if ((catobs.HasValue && catobs.Value == 10) && (!expsou.HasValue && !height.HasValue) && (watlev.HasValue && watlev.Value == 7)) { //  GST
                    //    instance.height = null;
                    //}
                    if (
                            (catobs.HasValue && catobs.Value == 6) &&
                            (valsou.HasValue && valsou.Value == -32767m)
                        ) {
                        instance.defaultClearanceDepth = 0.1m;
                    }
                    else if (
                            (valsou.HasValue && valsou.Value == -32767m) &&
                            (watlev.HasValue && watlev.Value == 3)
                        ) {
                        instance.defaultClearanceDepth = 0.1m;
                    }
                    else if (
                            (catobs.HasValue && catobs.Value != 6) &&
                            (valsou.HasValue && valsou.Value == -32767m) &&
                            (watlev.HasValue && watlev.Value == 5)
                        ) {
                        instance.defaultClearanceDepth = 0m;
                    }
                    else if (
                            (catobs.HasValue && catobs.Value != 6) &&
                            (valsou.HasValue && valsou.Value == -32767m) &&
                            (watlev.HasValue && (watlev.Value == 4 || watlev.Value == -32767))
                        ) {
                        instance.defaultClearanceDepth = -15m;
                    }
                    else {
                        //  Hit me with a baseball bat
                        Logger.Current.DataError(current.GetObjectID(), current.TableName(), current.LNAM() ?? "Unknown LNAM", $"defaultClearanceDepth OBSTRN! catobs:{catobs}, expsou:{expsou}, height:{height}, valsou:{valsou}, watlev:{watlev}, leastDepth:{leastDepth}");
                    }
                }

                if (!instance.valueOfSounding.HasValue && !instance.defaultClearanceDepth.HasValue) {
                    //Logger.Current.Error("!instance.valueOfSounding.HasValue && !defaultClearanceDepth (OBSTRN#{objectid})", current.GetObjectID());
                }


                var result = ImporterNIS.AddInformation(current.GetObjectID(), current.TableName(), current.NTXTDS(), current.TXTDSC(), current.INFORM(), current.NINFOM());
                instance.information = [.. result.information];
                instance.SetInformationBindings(result.InformationBindings.ToArray());

                return instance;
            }
        }
    }
}

