using ArcGIS.Core.Data;
using S100FC.S101.ComplexAttributes;
using S100FC.S101.FeatureTypes;
using S100Framework.Applications.Singletons;

namespace S100Framework.Applications
{
    using S100Framework.Applications.S57.esri;

    internal static partial class ImporterNIS
    {

        private static HarbourFacility HRBFAC(Feature current) {
            var instance = new HarbourFacility();

            if (current.CATHAF_HasValue()) {
                var categoryOfHarbourFacility = EnumHelper.GetEnumValues(current.CATHAF());
                if (categoryOfHarbourFacility is not null)
                    instance.categoryOfHarbourFacility = categoryOfHarbourFacility;
            }

            if (current.COMCHA_HasValue()) {
                instance.communicationChannel = GetCommunicationChannel(current.COMCHA()!);
            }

            if (current.CONDTN_HasValue()) {
                instance.condition = GetCondition(current.CONDTN()!.Value).value;
            }

            var featureName = GetFeatureName(current.OBJNAM(), current.NOBJNM());
            if (featureName is not null)
                instance.featureName = featureName;

            DateHelper.TryGetFixedDateRange(current.DATSTA(), current.DATEND(), out var dateRange);
            if (dateRange is not null) {
                instance.fixedDateRange = dateRange;
            }

            if (current.NATCON_HasValue()) {
                var natureOfConstruction = EnumHelper.GetEnumValues(current.NATCON());
                if (natureOfConstruction is not null && natureOfConstruction.Any())
                    instance.natureOfConstruction = natureOfConstruction;
            }

            DateHelper.TryGetPeriodicDateRange(current.PERSTA(), current.PEREND(), out var periodicDateRange);
            if (periodicDateRange is not null) {
                instance.periodicDateRange = periodicDateRange;
            }

            // TODO: product

            if (current.SORDAT_HasValue()) {
                if (DateHelper.TryConvertSordat(current.SORDAT()!, out var reportedDate)) {
                    instance.reportedDate = reportedDate;
                }
                else {
                    Logger.Current.DataError(current.GetObjectID(), current.TableName(), current.LNAM() ?? "Unknown LNAM", $"Cannot convert date {current.SORDAT()}");
                }
            }

            // TODO: restriction

            if (current.STATUS_HasValue()) {
                var status = EnumHelper.GetEnumValues(current.STATUS(), instance.attributeBindingDefinition("status")!.permitedValues!);
                if (status is not null && status.Any())
                    instance.status = status;
            }

            if (!string.IsNullOrEmpty(current.INFORM()) && regexVesselSpeedLimit.IsMatch(current.INFORM()!)) {
                var _value = regexVesselSpeedLimit.Match(current.INFORM()!).Groups["value"]?.Value;
                var _unit = regexVesselSpeedLimit.Match(current.INFORM()!).Groups["unit"]?.Value.ToLowerInvariant();

                var vesselSpeedLimit = new vesselSpeedLimit {
                };
                if (decimal.TryParse(_value, out decimal value)) {
                    vesselSpeedLimit.speedLimit = value;
                }
                if (!string.IsNullOrEmpty(_unit)) {
                    if (_unit.StartsWith("kilometres per hour"))
                        vesselSpeedLimit.speedUnits = 2;
                    if (_unit.StartsWith("miles per hour"))
                        vesselSpeedLimit.speedUnits = 3;
                    if (_unit.StartsWith("knots"))
                        vesselSpeedLimit.speedUnits = 4;
                }
                instance.vesselSpeedLimit = [vesselSpeedLimit];

                var inform = $"Speed limit is {_value} {_unit}";
                if (inform.Equals(current.INFORM(), StringComparison.InvariantCultureIgnoreCase))
                    current["INFORM"] = DBNull.Value;
                else
                    Logger.Current.Debug($"VesselSpeedLimit {current.TableName()}::{current.OBJECTID()} {current.INFORM()}");
            }

            if (current.PLTS_COMP_SCALE_HasValue()) {
                string subtype = "";
                if (!Subtypes.Instance.TryGetSubtype(current.TableName(), current.FCSubtype(), out subtype))
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

