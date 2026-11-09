using ArcGIS.Core.Data;
using S100FC.S101.ComplexAttributes;
using S100FC.S101.FeatureTypes;
using S100Framework.Applications.Singletons;

namespace S100Framework.Applications
{
    using S100Framework.Applications.S57.esri;

    internal static partial class ImporterNIS
    {

        private static RestrictedArea RESARE(Feature current) {
            var instance = new RestrictedArea();

            if (current.CATREA_HasValue()) {
                if (current.CATREA() != "26") {   // Water Skiing Area
                                                  // CATREA
                    var categoryOfRestrictedArea = EnumHelper.GetEnumValues(current.CATREA());
                    if (categoryOfRestrictedArea is not null && categoryOfRestrictedArea.Any())
                        instance.categoryOfRestrictedArea = categoryOfRestrictedArea;
                }
                else {
                    //S-57 Restricted Area – Water Skiing becomes S-101 RestrictedArea – Recreation Area according to S-65 Annex B Attribute tables
                    var categoryOfRestrictedArea = EnumHelper.GetEnumValues(32);
                    if (categoryOfRestrictedArea is not null && categoryOfRestrictedArea.Any())
                        instance.categoryOfRestrictedArea = categoryOfRestrictedArea;
                }
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

            if (current.RESTRN_HasValue()) {
                var restriction = EnumHelper.GetEnumValues(current.RESTRN());
                if (restriction is not null && restriction.Any())
                    instance.restriction = restriction;
            }

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

