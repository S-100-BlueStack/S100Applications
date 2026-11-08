using ArcGIS.Core.Data;
using S100FC.S101.FeatureTypes;
using S100Framework.Applications.Singletons;

namespace S100Framework.Applications
{
    using S100Framework.Applications.S57.esri;

    internal static partial class ImporterNIS
    {

        private static TidalStreamFloodEbb TS_FEB(Feature current) {
            var instance = new TidalStreamFloodEbb();

            if (current.CAT_TS_HasValue()) {
                if (EnumHelper.GetEnumValue(current.CAT_TS(), out int? categoryOfTidalStream, instance.attributeBindingDefinition("categoryOfTidalStream")!.permitedValues!))
                    instance.categoryOfTidalStream = categoryOfTidalStream;
            }

            var featureName = GetFeatureName(current.OBJNAM(), current.NOBJNM());
            if (featureName is not null)
                instance.featureName = featureName;

            //The S-57 attributes PEREND and PERSTA for TS_FEB will not be converted.It is considered that
            //these attributes are not relevant for Tidal Stream – Flood / Ebb in S - 101.

            DateHelper.TryGetFixedDateRange(current.DATSTA(), current.DATEND(), out var dateRange);
            if (dateRange != default) {
                instance.fixedDateRange = dateRange;
            }

            if (current.ORIENT_HasValue()) {
                instance.orientation = new() {
                    orientationValue = current.ORIENT() != -32767m ? current.ORIENT() : null,
                    orientationUncertainty = null,
                };
            }

            if (current.CURVEL_HasValue()) {
                instance.speed = new() {
                    speedMaximum = current.CURVEL() != -32767m ? current.CURVEL() : null,
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
    }
}

