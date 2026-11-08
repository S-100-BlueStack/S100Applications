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



        private static FeatureType PIPSOL(Feature current, QueryFilter filter) {
            var shape = current.GetShape();

            if (shape.GeometryType == GeometryType.Point) {
                var instance = new Obstruction();

                if (current.CONDTN_HasValue()) {
                    instance.condition = GetCondition(current.CONDTN()!.Value)?.value;
                }

                var featureName = GetFeatureName(current.OBJNAM(), current.NOBJNM());
                if (featureName is not null)
                    instance.featureName = featureName;

                if (current.HEIGHT_HasValue()) {
                    instance.height = current.HEIGHT() != -32767m ? current.HEIGHT() : null;
                }

                var inform = current.INFORM();
                if (!string.IsNullOrEmpty(inform) && regexMaximumDraughtPermitted.IsMatch(inform)) {
                    var _value = regexMaximumDraughtPermitted.Match(inform).Groups["value"]?.Value;

                    if (decimal.TryParse(_value, out decimal value)) {
                        instance.maximumPermittedDraught = value;
                    }
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

                if (current.VERLEN_HasValue()) {
                    instance.verticalLength = current.VERLEN() != -32767m ? current.VERLEN() : null;
                }

                if (current.PLTS_COMP_SCALE_HasValue()) {
                    string subtype = "";

                    if (!Subtypes.Instance.TryGetSubtype(current.TableName(), current.FCSubtype(), out subtype))
                        throw new NotSupportedException($"Unknown subtype for {current.TableName}, {current.FCSubtype()}");

                    var scaleMinimum = Scamin.Instance.GetMinimumScale(current, subtype, current.PLTS_COMP_SCALE()!.Value, isRelatedToStructure: false);
                    if (scaleMinimum.HasValue)
                        instance.scaleMinimum = scaleMinimum;
                }

                /*      30.1 default clearance depth
                        The attribute default clearance depth must be populated with a value, which must not be an empty (null) value, 
                        only if the attribute value of sounding for the feature instance is populated with an empty (null) value and 
                        the attribute height, if an allowable attribute for the feature, is not populated.

                        The value for default clearance depth is determined from the attribute depth range minimum value for the surrounding 
                        encoded Depth Area(s) or Dredged Area (see clauses 11.4 and 11.7) in accordance with the Tables below. 
                        For an area feature covered by more than one depth area, the default clearance depth is determined based on the depth 
                        range minimum value of the shoalest of the depth areas covering the feature.
                */
                if (!instance.height.HasValue && !instance.valueOfSounding.HasValue) {
                    int[] fcsubtypes = [1, 5];

                    var surroundingDepths = ImporterNIS.GetSurrounding_DepthArea(current.Shape()!, (Geodatabase)current.GetTable().GetDatastore(), filter.WhereClause).ToList();
                    surroundingDepths = surroundingDepths.Where(e => fcsubtypes.Contains(e.FcSubtype)).ToList();
                    if (surroundingDepths.Any()) {
                        var surroundingDepth = surroundingDepths.OrderByDescending(e => e.DRVAL1!.Value).Last();

                        if (surroundingDepth.FcSubtype == 1) {  // DEPARE
                            instance.defaultClearanceDepth = surroundingDepth.DRVAL1!.Value;
                        }
                        if (surroundingDepth.FcSubtype == 5) {  // DRGARE
                            instance.defaultClearanceDepth = surroundingDepth.DRVAL1!.Value;
                        }
                    }
                    else
                        instance.defaultClearanceDepth = -15m;
                }

                var result = ImporterNIS.AddInformation(current.GetObjectID(), current.TableName(), current.NTXTDS(), current.TXTDSC(), current.INFORM(), current.NINFOM());
                instance.information = [.. result.information];
                instance.SetInformationBindings(result.InformationBindings.ToArray());

                return instance;
            }
            else {
                var instance = new PipelineSubmarineOnLand();

                if (current.BURDEP_HasValue()) {
                    instance.buriedDepth = current.BURDEP() != -32767m ? current.BURDEP() : null;
                }

                if (current.CATPIP_HasValue()) {
                    var categoryOfPipelinePipe = EnumHelper.GetEnumValues(current.CATPIP());
                    if (categoryOfPipelinePipe is not null && categoryOfPipelinePipe.Any())
                        instance.categoryOfPipelinePipe = categoryOfPipelinePipe;
                }

                if (current.CONDTN_HasValue()) {
                    instance.condition = GetCondition(current.CONDTN()!.Value)?.value;
                }

                if (current.DRVAL1_HasValue()) {
                    instance.depthRangeMinimumValue = current.DRVAL1() != -32767m ? current.DRVAL1() : null;
                }

                if (current.DRVAL2_HasValue()) {
                    instance.depthRangeMaximumValue = current.DRVAL2() != -32767m ? current.DRVAL2() : null;
                }

                var featureName = GetFeatureName(current.OBJNAM(), current.NOBJNM());
                if (featureName is not null)
                    instance.featureName = featureName;

                DateHelper.TryGetFixedDateRange(current.DATSTA(), current.DATEND(), out var dateRange);
                if (dateRange != default) {
                    instance.fixedDateRange = dateRange;
                }

                // TODO: multiplicityOfFeatures

                if (current.PRODCT_HasValue()) {
                    var product = EnumHelper.GetEnumValues(current.PRODCT());
                    if (product is not null && product.Any())
                        instance.product = product;
                }
                if (current.SORDAT_HasValue()) {
                    if (DateHelper.TryConvertSordat(current.SORDAT()!, out var reportedDate)) {
                        instance.reportedDate = reportedDate;
                    }
                    else {
                        Logger.Current.DataError(current.GetObjectID(), current.TableName(), current.LNAM() ?? "Unknown LNAM", $"Cannot convert date {current.SORDAT()}");
                    }
                }

                // TODO: restriction

                if (current.STATUS_HasValue()) {
                    var status = EnumHelper.GetEnumValues(current.STATUS(), instance.attributeBindingDefinition("status")!.permitedValues!);
                    if (status is not null && status.Any())
                        instance.status = status;
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

