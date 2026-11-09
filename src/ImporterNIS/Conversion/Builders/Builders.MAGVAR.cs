using ArcGIS.Core.Data;
using S100FC.S101.FeatureTypes;
using S100Framework.Applications.Singletons;

namespace S100Framework.Applications
{
    using S100Framework.Applications.S57.esri;

    internal static partial class ImporterNIS
    {

        private static MagneticVariation MAGVAR(Feature current) {
            var instance = new MagneticVariation();

            /*  27.152 reference year for magnetic variation (RYRMGV)
                IHO Definition: REFERENCE YEAR FOR MAGNETIC VARIATION. The reference calendar year for magnetic
                variation values. (S-57 Edition 3.1, Appendix A – Chapter 2, Page 2.176, November 2000).
                Attribute Type: Truncated date
                Unit: Four digit year indication (YYYY)
                Format: YYYY----
                Example: 2009----

                Remarks:
                The dashes (----) must be included in all cases.
            */
            if (current.RYRMGV_HasValue()) {
                instance.referenceYearForMagneticVariation = current.RYRMGV() != "-32767" ? current.RYRMGV()!.PadRight(8, '-') : null;
            }

            if (current.VALACM_HasValue()) {
                instance.valueOfAnnualChangeInMagneticVariation = current.VALACM() != -32767 ? current.VALACM() : null;
            }

            if (current.VALMAG_HasValue()) {
                instance.valueOfMagneticVariation = current.VALMAG() != -32767m ? current.VALMAG() : null;
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

