using ArcGIS.Core.Data;
using S100FC.S101.FeatureTypes;
using S100Framework.Applications.Singletons;

namespace S100Framework.Applications
{
    using S100Framework.Applications.S57.esri;

    internal static partial class ImporterNIS
    {

        private static CableSubmarine CBLSUB(Feature current) {
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

            return instance;
        }
    }
}

