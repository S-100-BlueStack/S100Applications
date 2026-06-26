using ArcGIS.Core.Data;
using Microsoft.AspNetCore.Components.Routing;
using S100FC;
using S100FC.S101.FeatureTypes;
using S100Framework.Applications;
using S100Framework.Applications.S57.esri;
using S100Framework.Applications.Singletons;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Windows.Storage.Streams;

namespace S100Framework.Applications
{
    internal static partial class ImporterNIS
    {
        private static Dictionary<string, Func<S57Object, RowBuffer, FeatureType>> _builders = new Dictionary<string, Func<S57Object, RowBuffer, FeatureType>> {
            { "DISMAR", (current, buffer) => { return DistanceMark((PortsAndServices)current, buffer); } },
            { "BERTHS", (current, buffer) => { return Berth((PortsAndServices)current, buffer); } },
            { "NAVLNE", (current, buffer) => { return NavigationLine((TracksAndRoutes)current, buffer); } },
            { "ADMARE", (current, buffer) => { return AdministrationArea((RegulatedAreasAndLimits)current, buffer); } },
        };

        private static Regex regexWaterwayDistance = new Regex(@"(Waterway distance =)\s(?<value>\d+)\s(?<unit>\.+)", RegexOptions.IgnoreCase);

        private static Regex regexMaximumDraughtPermitted = new Regex(@"(Maximum draught permitted =)\s(?<value>\d+\.?\d*)", RegexOptions.IgnoreCase);

        private static Regex regexVesselTrafficServiceArea = new Regex(@"(Vessel Traffic Service Area)", RegexOptions.IgnoreCase);

        private static FeatureType Build(string code, S57Object feature, RowBuffer buffer) => _builders[code]?.Invoke(feature, buffer)!;

        private static DistanceMark DistanceMark(PortsAndServices current, RowBuffer buffer) {
            var instance = new DistanceMark();

            /*
                The S-57 attribute CATDIS has been replaced in S-101 by the mandatory Boolean type attribute
                distance mark visible. Where CATDIS has not been populated, or has been populated with value
                1 (distance mark not physically installed) or an empty (null) value, distance mark visible will be set
                to False. Where CATDIS has been populated with a value other than 1, distance mark visible will
                be set to True.                             
            */
            if (!current.CATDIS.HasValue || (current.CATDIS.HasValue && current.CATDIS.Value == 1) || (current.CATDIS.HasValue && current.CATDIS.Value == -32767)) {
                instance.distanceMarkVisible = false;
            }
            else if (current.CATDIS.HasValue) {
                instance.distanceMarkVisible = true;
            }

            var featureName = GetFeatureName(current.OBJNAM, current.NOBJNM);
            if (featureName is not null)
                instance.featureName = featureName;


            DateHelper.TryGetFixedDateRange(current.DATSTA, current.DATEND, out var dateRange);
            if (dateRange != default) {
                instance.fixedDateRange = dateRange;
            }

            // TODO: interoperabilityIdentifier


            if (!string.IsNullOrEmpty(current.INFORM) && regexWaterwayDistance.IsMatch(current.INFORM)) {
                var _value = regexWaterwayDistance.Match(current.INFORM).Groups["value"]?.Value;
                var _unit = regexWaterwayDistance.Match(current.INFORM).Groups["unit"]?.Value?.ToLowerInvariant();

                instance.measuredDistanceValue = new() {
                    waterwayDistance = default,
                    distanceUnitOfMeasurement = default,
                };
                if (decimal.TryParse(_value, out decimal value)) {
                    instance.measuredDistanceValue.waterwayDistance = value;
                }
                if (!string.IsNullOrEmpty(_unit)) {
                    if (_unit.StartsWith("metres"))
                        instance.measuredDistanceValue.distanceUnitOfMeasurement = 1;
                    if (_unit.StartsWith("yards"))
                        instance.measuredDistanceValue.distanceUnitOfMeasurement = 2;
                    if (_unit.StartsWith("kilometres"))
                        instance.measuredDistanceValue.distanceUnitOfMeasurement = 3;
                    if (_unit.StartsWith("statute miles"))
                        instance.measuredDistanceValue.distanceUnitOfMeasurement = 4;
                    if (_unit.StartsWith("nautical miles"))
                        instance.measuredDistanceValue.distanceUnitOfMeasurement = 5;
                }
            }

            if (current.PLTS_COMP_SCALE.HasValue && current.SHAPE != null) {
                string subtype = "";
                if (current.TableName != default && current.FCSUBTYPE.HasValue && !Subtypes.Instance.TryGetSubtype(current.TableName, current.FCSUBTYPE.Value, out subtype))
                    throw new NotSupportedException($"Unknown subtype for {current.TableName}, {current.FCSUBTYPE.Value}");
                var scaleMinimum = Scamin.Instance.GetMinimumScale(current, subtype, current.PLTS_COMP_SCALE.Value, isRelatedToStructure: false);
                if (scaleMinimum.HasValue)
                    instance.scaleMinimum = scaleMinimum.Value;
            }
            var result = ImporterNIS.AddInformation(current.OBJECTID!.Value, current.TableName!, current.NTXTDS, current.TXTDSC, current.INFORM, current.NINFOM);
            instance.information = result.information.ToArray();
            instance.SetInformationBindings(result.InformationBindings.ToArray());

            buffer["ps"] = ps101;
            buffer["code"] = instance.GetType().Name;


            buffer["attributebindings"] = instance.Flatten();
            buffer["informationbindings"] = System.Text.Json.JsonSerializer.Serialize(instance.GetInformationBindings(), jsonSerializerOptions);

            SetShape(buffer, current.SHAPE); buffer["sourceIdentifier"] = instance.sourceIdentifier;
            SetUsageBand(buffer, current.PLTS_COMP_SCALE!.Value);

            return instance;
        }

        private static Berth Berth(PortsAndServices current, RowBuffer buffer) {
            var instance = new Berth {
            };

            /* S-57 ENC to S-101 Conversion Guidance ed 1.2.0

                The attribute category of cargo has been introduced in S-101 to encode the type of vessel cargo
                allowed at the berth, in particular the fact that a berth is a berth for dangerous or hazardous cargo
                (category of cargo = 7). This information is encoded in S-57 on BERTHS using the attribute
                INFORM (see clause 2.3). In order for this information to be converted across to S-101, the text
                string encoded in INFORM on the BERTHS should be in a standardised format, such as Dangerous
                or hazardous cargo.
            */

            var featureName = GetFeatureName(current.OBJNAM, current.NOBJNM);
            if (featureName is not null)
                instance.featureName = featureName;

            DateHelper.TryGetFixedDateRange(current.DATSTA, current.DATEND, out var dateRange);
            if (dateRange != default) {
                instance.fixedDateRange = dateRange;
            }

            // TODO: interoperabilityIdentifier

            if (!string.IsNullOrEmpty(current.INFORM) && regexMaximumDraughtPermitted.IsMatch(current.INFORM)) {
                var _value = regexMaximumDraughtPermitted.Match(current.INFORM).Groups["value"]?.Value;

                if (decimal.TryParse(_value, out decimal value)) {
                    instance.maximumPermittedDraught = value;
                }
            }

            if (current.DRVAL1.HasValue) {
                instance.minimumBerthDepth = current.DRVAL1.Value == -32767m ? default : current.DRVAL1.Value;
            }

            DateHelper.TryGetPeriodicDateRange(current.PERSTA, current.PEREND, out var periodicDateRange);
            if (periodicDateRange != default) {
                instance.periodicDateRange = periodicDateRange;
            }

            if (current.QUASOU != default) {
                var qualityOfVerticalMeasurement = EnumHelper.GetEnumValues(current.QUASOU);
                if (qualityOfVerticalMeasurement is not null && qualityOfVerticalMeasurement.Any())
                    instance.qualityOfVerticalMeasurement = qualityOfVerticalMeasurement;
            }

            if (current.STATUS != default) {
                instance.status = GetStatus(current.STATUS);
            }

            if (current.SOUACC.HasValue) {
                instance.verticalUncertainty = new() {
                    uncertaintyFixed = current.SOUACC.Value
                };
            }

            if (current.PLTS_COMP_SCALE.HasValue && current.SHAPE != null) {
                string subtype = "";
                if (current.TableName != default && current.FCSUBTYPE.HasValue && !Subtypes.Instance.TryGetSubtype(current.TableName, current.FCSUBTYPE.Value, out subtype))
                    throw new NotSupportedException($"Unknown subtype for {current.TableName}, {current.FCSUBTYPE.Value}");
                var scaleMinimum = Scamin.Instance.GetMinimumScale(current, subtype, current.PLTS_COMP_SCALE.Value, isRelatedToStructure: false);
                if (scaleMinimum.HasValue)
                    instance.scaleMinimum = scaleMinimum.Value;
            }

            var result = ImporterNIS.AddInformation(current.OBJECTID!.Value, current.TableName!, current.NTXTDS, current.TXTDSC, current.INFORM, current.NINFOM);
            instance.information = result.information.ToArray();
            instance.SetInformationBindings(result.InformationBindings.ToArray());

            buffer["ps"] = ps101;
            buffer["code"] = instance.GetType().Name;


            buffer["attributebindings"] = instance.Flatten();
            buffer["informationbindings"] = System.Text.Json.JsonSerializer.Serialize(instance.GetInformationBindings(), jsonSerializerOptions);

            SetShape(buffer, current.SHAPE); buffer["sourceIdentifier"] = instance.sourceIdentifier;
            SetUsageBand(buffer, current.PLTS_COMP_SCALE!.Value);

            return instance;
        }

        private static NavigationLine NavigationLine(TracksAndRoutes current, RowBuffer buffer) {
            var instance = new NavigationLine();

            if (current.CATNAV.HasValue) {
                instance.categoryOfNavigationLine = EnumHelper.GetEnumValue(current.CATNAV.Value);
            }

            DateHelper.TryGetFixedDateRange(current.DATSTA, current.DATEND, out var dateRange);
            if (dateRange != default) {
                instance.fixedDateRange = dateRange;
            }

            // TODO: interoperabilityIdentifier

            // TODO: measured distance

            if (current.ORIENT.HasValue) {
                instance.orientation = new S100FC.S101.ComplexAttributes.orientation() {
                    orientationValue = current.ORIENT.Value == -32767m ? default : current.ORIENT.Value,
                    // TODO: oriantationUncertainty
                    //orientationUncertainty = ,
                };
            }

            DateHelper.TryGetPeriodicDateRange(current.PERSTA, current.PEREND, out var periodicDateRange);
            if (periodicDateRange != default) {
                instance.periodicDateRange = periodicDateRange;
            }

            if (current.STATUS != default) {
                instance.status = GetStatus(current.STATUS);
            }

            if (current.PLTS_COMP_SCALE.HasValue && current.SHAPE != null) {
                string subtype = "";
                if (current.TableName != default && current.FCSUBTYPE.HasValue && !Subtypes.Instance.TryGetSubtype(current.TableName, current.FCSUBTYPE.Value, out subtype))
                    throw new NotSupportedException($"Unknown subtype for {current.TableName}, {current.FCSUBTYPE.Value}");
                var scaleMinimum = Scamin.Instance.GetMinimumScale(current, subtype, current.PLTS_COMP_SCALE.Value, isRelatedToStructure: false);
                if (scaleMinimum.HasValue)
                    instance.scaleMinimum = scaleMinimum.Value;
            }

            var result = ImporterNIS.AddInformation(current.OBJECTID!.Value, current.TableName!, current.NTXTDS, current.TXTDSC, current.INFORM, current.NINFOM);
            instance.information = result.information.ToArray();
            instance.SetInformationBindings(result.InformationBindings.ToArray());

            buffer["ps"] = ps101;
            buffer["code"] = instance.GetType().Name;


            buffer["attributebindings"] = instance.Flatten();
            buffer["informationbindings"] = System.Text.Json.JsonSerializer.Serialize(instance.GetInformationBindings(), jsonSerializerOptions);

            SetShape(buffer, current.SHAPE); buffer["sourceIdentifier"] = instance.sourceIdentifier;
            SetUsageBand(buffer, current.PLTS_COMP_SCALE!.Value);

            return instance;
        }

        private static FeatureType AdministrationArea(RegulatedAreasAndLimits current, RowBuffer buffer) {
            if (!string.IsNullOrEmpty(current.INFORM) && regexVesselTrafficServiceArea.IsMatch(current.INFORM)) {
                var instance = new VesselTrafficServiceArea {
                };

                var featureName = GetFeatureName(current.OBJNAM, current.NOBJNM);
                if (featureName is not null)
                    instance.featureName = featureName;

                // TODO: interoperabilityIdentifier

                if (current.PLTS_COMP_SCALE.HasValue && current.SHAPE != null) {
                    string subtype = "";
                    if (current.TableName != default && current.FCSUBTYPE.HasValue && !Subtypes.Instance.TryGetSubtype(current.TableName, current.FCSUBTYPE.Value, out subtype))
                        throw new NotSupportedException($"Unknown subtype for {current.TableName}, {current.FCSUBTYPE.Value}");
                    var scamin = Scamin.Instance.GetMinimumScale(current, subtype, current.PLTS_COMP_SCALE!.Value, isRelatedToStructure: false);
                    if (scamin.HasValue)
                        instance.scaleMinimum = scamin.Value;
                }

                var result = ImporterNIS.AddInformation(current.OBJECTID!.Value, current.TableName!, current.NTXTDS, current.TXTDSC, current.INFORM, current.NINFOM);
                instance.information = result.information.ToArray();
                instance.SetInformationBindings(result.InformationBindings.ToArray());

                buffer["ps"] = ps101;
                buffer["code"] = instance.GetType().Name;
                buffer["attributebindings"] = instance.Flatten();
                buffer["informationbindings"] = System.Text.Json.JsonSerializer.Serialize(instance.GetInformationBindings(), jsonSerializerOptions);

                SetShape(buffer, current.SHAPE); buffer["sourceIdentifier"] = instance.sourceIdentifier;
                SetUsageBand(buffer, current.PLTS_COMP_SCALE!.Value);
                return instance;
            }
            else {
                var instance = new AdministrationArea {
                };

                if (current.JRSDTN.HasValue) {
                    instance.jurisdiction = EnumHelper.GetEnumValue(current.JRSDTN.Value);
                }

                var featureName = GetFeatureName(current.OBJNAM, current.NOBJNM);
                if (featureName is not null)
                    instance.featureName = featureName;

                // TODO: interoperabilityIdentifier

                if (current.NATION != default) {
                    instance.nationality = [GetNation(current.NATION)];
                }

                if (current.PLTS_COMP_SCALE.HasValue && current.SHAPE != null) {
                    string subtype = "";
                    if (current.TableName != default && current.FCSUBTYPE.HasValue && !Subtypes.Instance.TryGetSubtype(current.TableName, current.FCSUBTYPE.Value, out subtype))
                        throw new NotSupportedException($"Unknown subtype for {current.TableName}, {current.FCSUBTYPE.Value}");
                    var scamin = Scamin.Instance.GetMinimumScale(current, subtype, current.PLTS_COMP_SCALE!.Value, isRelatedToStructure: false);
                    if (scamin.HasValue)
                        instance.scaleMinimum = scamin.Value;
                }

                var result = ImporterNIS.AddInformation(current.OBJECTID!.Value, current.TableName!, current.NTXTDS, current.TXTDSC, current.INFORM, current.NINFOM);
                instance.information = result.information.ToArray();
                instance.SetInformationBindings(result.InformationBindings.ToArray());

                if (current.PICREP != default) {
                    instance.pictorialRepresentation = FixFilename(current.PICREP);
                }

                buffer["ps"] = ps101;
                buffer["code"] = instance.GetType().Name;
                buffer["attributebindings"] = instance.Flatten();
                buffer["informationbindings"] = System.Text.Json.JsonSerializer.Serialize(instance.GetInformationBindings(), jsonSerializerOptions);

                SetShape(buffer, current.SHAPE); buffer["sourceIdentifier"] = instance.sourceIdentifier;
                SetUsageBand(buffer, current.PLTS_COMP_SCALE!.Value);
                return instance;
            }
        }
    }
}
