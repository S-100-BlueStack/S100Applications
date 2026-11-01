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

        private static CableSubmarine CBLSUB(Feature current, RowBuffer buffer) {
            var instance = new CableSubmarine();

            if (current.BURDEP_HasValue()) {
                instance.buriedDepth = current.BURDEP() != -32767m ? current.BURDEP() : null;
            }

            if (current.CATCBL_HasValue()) {
                if (current.CATCBL() == 4) {
                    /*
                        S-65 Annex B
                        Amended guidance for conversion of CATCBL value 4 (telephone) to convert
                        to new value category of cable = 10 (telecommunications cable).
                        11.5.1, 11.5.3, A-2
                     */

                    instance.categoryOfCable = 10; //categoryOfCable.TelecommunicationsCable;
                }
                else if (EnumHelper.GetEnumValue(current.CATCBL(), out int? categoryOfCable, instance.attributeBindingDefinition("categoryOfCable")!.permitedValues!)) {
                    instance.categoryOfCable = categoryOfCable;
                }
            }

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

