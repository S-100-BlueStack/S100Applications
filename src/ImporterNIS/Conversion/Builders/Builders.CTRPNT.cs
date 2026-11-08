using ArcGIS.Core.Data;
using S100FC.S101.FeatureTypes;
using S100Framework.Applications.Singletons;

namespace S100Framework.Applications
{
    using S100Framework.Applications.S57.esri;

    internal static partial class ImporterNIS
    {

        private static Landmark CTRPNT(Feature current) {
            var instance = new Landmark {
                /*
When converting the S-57 CTRPNT Object class the S-101 mandatory attribute visual prominence on the 
converted Landmark feature will be populated during the automated conversion process 
with value 2 (not visually conspicuous). Data Producers will be required to amend this value as appropriate.
*/
                visualProminence = 2
            };

            if (current.CATCTR_HasValue()) {
                var categoryOfLandmark = EnumHelper.GetEnumValues(current.CATCTR());
                if (categoryOfLandmark is not null)
                    instance.categoryOfLandmark = categoryOfLandmark;
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

