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

        private static OffshorePlatform OFSPLF(Feature current, RowBuffer buffer) {
            var instance = new OffshorePlatform();

            if (current.CATOFP_HasValue()) {
                if (EnumHelper.GetEnumValue(current.CATOFP(), out int? categoryOfOffshorePlatform, instance.attributeBindingDefinition("categoryOfOffshorePlatform")!.permitedValues!))
                    instance.categoryOfOffshorePlatform = categoryOfOffshorePlatform;
            }

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
                instance.condition = GetCondition(current.CONDTN()!.Value)!.value;
            }

            var featureName = GetFeatureName(current.OBJNAM(), current.NOBJNM());
            if (featureName is not null)
                instance.featureName = featureName;

            DateHelper.TryGetFixedDateRange(current.DATSTA()!, current.DATEND(), out var dateRange);
            if (dateRange != default) {
                instance.fixedDateRange = dateRange;
            }

            if (current.HEIGHT_HasValue()) {
                instance.height = current.HEIGHT() != -32767m ? current.HEIGHT()!.Value : null;
            }

            if (current.PRODCT_HasValue()) {
                var product = EnumHelper.GetEnumValues(current.PRODCT());
                if (product is not null && product.Any())
                    instance.product = product;
            }

            if (current.CONRAD_HasValue()) {
                instance.radarConspicuous = current.CONRAD() == -32767 ? null : current.CONRAD()!.Value == 2 ? false : true;
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

            if (current.VERLEN_HasValue()) {
                instance.verticalLength = current.VERLEN() != -32767m ? current.VERLEN()!.Value : null;
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
            if (current.PICREP_HasValue()) {
                instance.pictorialRepresentation = FixFilename(current.PICREP()!);
            }

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

