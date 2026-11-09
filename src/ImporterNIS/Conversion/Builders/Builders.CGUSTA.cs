using ArcGIS.Core.Data;
using S100FC;
using S100FC.S101.FeatureTypes;
using S100Framework.Applications.Singletons;

namespace S100Framework.Applications
{
    using S100Framework.Applications.S57.esri;

    internal static partial class ImporterNIS
    {

        private static FeatureType CGUSTA(Feature current) {
            /*
                The S-101 Boolean attribute is MRCC has been introduced in S - 101 to indicate that a coast guard
                station also performs the function of a Maritime Rescue and Coordination Centres(MRCC). This
                information is encoded in S - 57 on CGUSTA using the attribute INFORM(see clause 2.3).In order
                for this information to be converted across to S - 101, the text string encoded in INFORM on the
                CGUSTA should be in a standardised format, such as Maritime Rescue and Coordination Centre.
            */

            var instance = new CoastGuardStation();

            if (!string.IsNullOrEmpty(current.INFORM()) && regexMaritimeRescue.IsMatch(current.INFORM()!)) {
                instance.isMRCC = true;
            }
            else if (!string.IsNullOrEmpty(current.INFORM()) && regexCoordinationCentre.IsMatch(current.INFORM()!)) {
                instance.isMRCC = true;
            }

            if (current.COMCHA_HasValue()) {
                instance.communicationChannel = current.COMCHA()!.Split(',').ToArray();
            }

            var featureName = GetFeatureName(current.OBJNAM(), current.NOBJNM());
            if (featureName is not null)
                instance.featureName = featureName;

            DateHelper.TryGetFixedDateRange(current.DATSTA(), current.DATEND(), out var dateRange);
            if (dateRange is not null) {
                instance.fixedDateRange = dateRange;
            }

            DateHelper.TryGetPeriodicDateRange(current.PERSTA(), current.PEREND(), out var periodicDateRange);
            if (periodicDateRange is not null) {
                instance.periodicDateRange = periodicDateRange;
            }

            if (current.STATUS_HasValue()) {
                var status = EnumHelper.GetEnumValues(current.STATUS(), instance.attributeBindingDefinition("status")!.permitedValues!);
                if (status is not null && status.Any())
                    instance.status = status;
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

            return instance;
        }
    }
}

