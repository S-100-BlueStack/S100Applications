using ArcGIS.Core.Data;
using S100FC.S101.FeatureTypes;

namespace S100Framework.Applications
{
    using S100FC;
    using S100Framework.Applications.S57.esri;
    using S100Framework.Applications.Singletons;

    internal static partial class ImporterNIS
    {

        private static FeatureType LNDMRK(Feature current) {
            if (current.CATLMK_HasValue() && current.CATLMK() == "19") {
                var instance = new WindTurbine();

                if (current.COLOUR_HasValue()) {
                    var colours = ImporterNIS.GetColours(current.COLOUR()!);
                    if (colours is not null)
                        instance.colour = colours;
                }

                if (current.COLPAT_HasValue()) {
                    if (instance.colour is not null && instance.colour.Length > 1)
                        instance.colourPattern = "-32767".Equals(current.COLPAT()) ? null : ImporterNIS.GetColourPattern(current.COLPAT()!)!.value;
                }

                if (current.CONDTN_HasValue()) {
                    instance.condition = ImporterNIS.GetCondition(current.CONDTN()!.Value)?.value;
                }

                if (current.ELEVAT_HasValue()) {
                    instance.elevation = current.ELEVAT() == -32767m ? null : current.ELEVAT();
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

                // TODO: multiplicityOfFeatures

                if (current.NATCON_HasValue()) {
                    var natureOfConstruction = EnumHelper.GetEnumValues(current.NATCON());
                    if (natureOfConstruction is not null && natureOfConstruction.Any())
                        instance.natureOfConstruction = natureOfConstruction;
                }

                if (current.CONRAD_HasValue()) {
                    instance.radarConspicuous = current.CONRAD() == 2 ? false : true;
                }
                if (!string.IsNullOrEmpty(current.SORDAT())) {
                    if (DateHelper.TryConvertSordat(current.SORDAT()!, out var reportedDate)) {
                        instance.reportedDate = reportedDate;
                    }
                    else {
                        Logger.Current.DataError(current.OBJECTID() ?? -1, current.GetType().Name, current.LNAM() ?? "Unknown LNAM", $"Cannot convert date {current.SORDAT()}");
                    }
                }

                if (current.STATUS_HasValue()) {
                    var status = EnumHelper.GetEnumValues(current.STATUS(), instance.attributeBindingDefinition("status")!.permitedValues!);
                    if (status is not null && status.Any())
                        instance.status = status;
                }

                var verticalUncertainty = new S100FC.S101.ComplexAttributes.verticalUncertainty();
                if (current.VERACC_HasValue())
                    verticalUncertainty.uncertaintyFixed = current.VERACC() != -32767m ? current.VERACC() : null;

                //var verticalClearanceFixed = new verticalClearanceFixed {
                //    verticalUncertainty = verticalUncertainty
                //};
                //if (current.VERCLR_HasValue())
                //    verticalClearanceFixed.verticalClearanceValue = current.VERCLR.Value != -32767m ? current.VERCLR.Value : null;

                instance.verticalClearanceFixed = new() {
                    verticalUncertainty = verticalUncertainty,
                };

                if (current.VERLEN_HasValue()) {
                    instance.verticalLength = current.VERLEN() != -32767m ? current.VERLEN() : null;
                    var verticalDatum = ImporterNIS.GetVerticalDatum(current.VERDAT(), current.SHAPE()!);
                    if (verticalDatum != default)
                        instance.verticalDatum = verticalDatum.value;
                }

                if (current.CONVIS_HasValue() /*&& current.CONVIS.Value != -32767*/) {
                    instance.visualProminence = EnumHelper.GetEnumValue(current.CONVIS());
                }

                if (current.WATLEV_HasValue()) {
                    if (current.WATLEV() == -32767)
                        instance.waterLevelEffect = default;
                    else {
                        instance.waterLevelEffect = EnumHelper.GetEnumValue(current.WATLEV());
                    }
                }

                if (current.PLTS_COMP_SCALE_HasValue()) {
                    string subtype = "";
                    if (current.TableName() != default && !Subtypes.Instance.TryGetSubtype(current.TableName(), current.FCSubtype(), out subtype))
                        throw new NotSupportedException($"Unknown subtype for {current.TableName()}, {current.FCSubtype()}");
                    var scaleMinimum = Scamin.Instance.GetMinimumScale(current, subtype, current.PLTS_COMP_SCALE()!.Value, isRelatedToStructure: false);
                    if (scaleMinimum.HasValue)
                        instance.scaleMinimum = scaleMinimum;
                }

                var result = ImporterNIS.AddInformation(current.GetObjectID(), current.TableName(), current.NTXTDS(), current.TXTDSC(), current.INFORM(), current.NINFOM());
                instance.information = [.. result.information];
                instance.SetInformationBindings(result.InformationBindings.ToArray());

                if (current.PICREP_HasValue()) {
                    instance.pictorialRepresentation = FixFilename(current.PICREP()!);
                }
                return instance;
            }
            else {
                var instance = new Landmark();

                if (current.CATLMK_HasValue()) {
                    var categoryOfLandmark = EnumHelper.GetEnumValues(current.CATLMK());
                    if (categoryOfLandmark is not null)
                        instance.categoryOfLandmark = categoryOfLandmark;
                }

                // TODO: CATSPM

                if (current.COLOUR_HasValue()) {
                    var colour = GetColours(current.COLOUR()!);
                    if (colour is not null && colour.Any())
                        instance.colour = colour;
                }

                if (current.COLPAT_HasValue()) {
                    if (instance.colour is not null && instance.colour.Length > 1)
                        instance.colourPattern = GetColourPattern(current.COLPAT()!)!.value;
                }

                if (current.CONDTN_HasValue()) {
                    instance.condition = GetCondition(current.CONDTN()!.Value)?.value;
                }

                if (current.ELEVAT_HasValue()) {
                    instance.elevation = current.ELEVAT() == -32767m ? null : current.ELEVAT();
                }

                var featureName = GetFeatureName(current.OBJNAM(), current.NOBJNM());
                if (featureName is not null)
                    instance.featureName = featureName;

                if (current.FUNCTN_HasValue()) {
                    var function = EnumHelper.GetEnumValues(current.FUNCTN());
                    if (function is not null && function.Any())
                        instance.function = function;
                }

                if (current.HEIGHT_HasValue()) {
                    instance.height = current.HEIGHT() != -32767m ? current.HEIGHT() : null;
                }

                // TODO: multiplicityOfFeatures

                if (current.NATCON_HasValue()) {
                    var natureOfConstruction = EnumHelper.GetEnumValues(current.NATCON());
                    if (natureOfConstruction is not null && natureOfConstruction.Any())
                        instance.natureOfConstruction = natureOfConstruction;
                }

                if (current.CONRAD_HasValue()) {
                    instance.radarConspicuous = current.CONRAD() == 2 ? false : true;
                }

                if (!string.IsNullOrEmpty(current.SORDAT())) {
                    if (DateHelper.TryConvertSordat(current.SORDAT()!, out var reportedDate)) {
                        instance.reportedDate = reportedDate;
                    }
                    else {
                        Logger.Current.DataError(current.OBJECTID() ?? -1, current.GetType().Name, current.LNAM() ?? "Unknown LNAM", $"Cannot convert date {current.SORDAT()}");
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

                if (current.CONVIS_HasValue()) {
                    instance.visualProminence = EnumHelper.GetEnumValue(current.CONVIS());
                }

                if (current.PLTS_COMP_SCALE_HasValue()) {
                    string subtype = "";
                    if (current.TableName() != default && !Subtypes.Instance.TryGetSubtype(current.TableName(), current.FCSubtype(), out subtype))
                        throw new NotSupportedException($"Unknown subtype for {current.TableName()}, {current.FCSubtype()}");
                    var scaleMinimum = Scamin.Instance.GetMinimumScale(current, subtype, current.PLTS_COMP_SCALE()!.Value, isRelatedToStructure: false);
                    if (scaleMinimum.HasValue)
                        instance.scaleMinimum = scaleMinimum;
                }

                var result = ImporterNIS.AddInformation(current.GetObjectID(), current.TableName(), current.NTXTDS(), current.TXTDSC(), current.INFORM(), current.NINFOM());
                instance.information = [.. result.information];
                instance.SetInformationBindings(result.InformationBindings.ToArray());

                if (current.PICREP_HasValue()) {
                    instance.pictorialRepresentation = FixFilename(current.PICREP()!);
                }
                return instance;
            }            
        }
    }
}

