using ArcGIS.Core.Data;
using S100FC.S101.FeatureTypes;
using S100Framework.Applications.Singletons;

namespace S100Framework.Applications
{
    using S100Framework.Applications.S57.esri;

    internal static partial class ImporterNIS
    {

        private static RadioStation RDOSTA(Feature current) {
            var instance = new RadioStation();

            if (current.CALSGN_HasValue()) {
                instance.callSign = current.CALSGN();
            }

            if (current.CATROS_HasValue()) {
                var category = current.CATROS() switch {
                    "1" => null,
                    "2" => null,
                    "3" => null,
                    "4" => null,
                    "5" => "5",
                    "6" => null,
                    "7" => null,
                    "8" => null,
                    "9" => null,
                    "10" => "10",
                    "11" => "11",
                    "12" => "11",
                    "13" => "11",
                    "14" => "14",
                    "19" => "19",
                    "20" => "20",
                    "-32767" => null,
                    _ => throw new NotSupportedException($"Cannot convert radiostation category {current.CATROS} aton: globalid:{current.GLOBALID}")
                };

                if (category != null) {
                    var categoryOfRadioStation = EnumHelper.GetEnumValues(category);
                    if (categoryOfRadioStation is not null && categoryOfRadioStation.Any())
                        instance.categoryOfRadioStation = categoryOfRadioStation;
                }
                else {
                    Logger.Current.DataError(current.OBJECTID() ?? -1, current.GetType().Name, current.LNAM() ?? "Unknown LNAM", $"Radiostation of type {current.FCSubtype()} is not converted.");
                }
            }

            if (current.COMCHA_HasValue()) {
                instance.communicationChannel = ImporterNIS.GetCommunicationChannel(current.COMCHA()!);
            }

            if (current.ESTRNG_HasValue()) {
                instance.estimatedRangeOfTransmission = current.ESTRNG();
            }

            var featureName = GetFeatureName(current.OBJNAM(), current.NOBJNM());
            if (featureName is not null)
                instance.featureName = featureName;

            DateHelper.TryGetFixedDateRange(current.DATSTA(), current.DATEND(), out var dateRange);
            if (dateRange != default) {
                instance.fixedDateRange = dateRange;
            }

            if (current.SIGFRQ_HasValue()) {
                instance.frequencyPair = ImporterNIS.GetFrequencyPair(current.SIGFRQ()!.Value);
            }

            DateHelper.TryGetPeriodicDateRange(current.PERSTA(), current.PEREND(), out var periodicDateRange);
            if (periodicDateRange != default) {
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

