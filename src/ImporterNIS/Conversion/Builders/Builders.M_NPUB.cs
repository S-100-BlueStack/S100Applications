using ArcGIS.Core.Data;
using S100FC.S101.FeatureTypes;
using S100Framework.Applications.Singletons;

namespace S100Framework.Applications
{
    using S100Framework.Applications.S57.esri;

    internal static partial class ImporterNIS
    {

        private static InformationArea M_NPUB(Feature current) {
            var instance = new InformationArea();

            if (current.SORDAT_HasValue()) {
                if (DateHelper.TryConvertSordat(current.SORDAT()!, out var reportedDate)) {
                    instance.reportedDate = reportedDate;
                }
                else {
                    Logger.Current.DataError(current.GetObjectID(), current.TableName(), current.LNAM() ?? "Unknown LNAM", $"Cannot convert date {current.SORDAT()}");
                }
            }

            if (current.PICREP_HasValue()) {
                instance.pictorialRepresentation = FixFilename(current.PICREP()!);
            }

            var featureName = GetFeatureName(current.OBJNAM(), current.NOBJNM());
            if (featureName is not null)
                instance.featureName = featureName;

            if (current.PLTS_COMP_SCALE_HasValue()) {
                string subtype = "";

                if (!Subtypes.Instance.TryGetSubtype(current.TableName(), current.FCSubtype(), out subtype))
                    throw new NotSupportedException($"Unknown subtype for {current.TableName}, {current.FCSubtype()}");

                var scaleMinimum = Scamin.Instance.GetMinimumScale(current, subtype, current.PLTS_COMP_SCALE()!.Value, isRelatedToStructure: false);
                if (scaleMinimum.HasValue)
                    instance.scaleMinimum = scaleMinimum;
            }

            var result = ImporterNIS.AddInformation(current.GetObjectID(), current.TableName(), current.NTXTDS(), current.TXTDSC(), current.INFORM(), current.NINFOM(), current.PUBREF());
            var informations = result.information.ToArray();

            if (informations.Any())
                instance.information = informations;
            instance.SetInformationBindings(result.InformationBindings.ToArray());

            return instance;
        }

        private static InformationArea M_NPUB(string? SORDAT, string? PICREP, string? OBJNAM, string? NOBJNM) {
            var instance = new InformationArea();

            if (!string.IsNullOrEmpty(SORDAT)) {
                if (DateHelper.TryConvertSordat(SORDAT!, out var reportedDate)) {
                    instance.reportedDate = reportedDate;
                }
            }

            if (!string.IsNullOrEmpty(PICREP)) {
                instance.pictorialRepresentation = FixFilename(PICREP!);
            }

            var featureName = GetFeatureName(OBJNAM, NOBJNM);
            if (featureName is not null)
                instance.featureName = featureName;

            return instance;
        }
    }
}

