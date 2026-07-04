using ArcGIS.Core.Data;
using S100FC;
using S100FC.S101.ComplexAttributes;
using S100FC.S101.FeatureTypes;
using S100Framework.Applications.S57.esri;
using S100Framework.Applications.Singletons;
using System.Text.RegularExpressions;

namespace S100Framework.Applications
{
    internal static partial class ImporterNIS
    {
        private static Dictionary<string, Func<S57Object, RowBuffer, FeatureType>> _builders = new Dictionary<string, Func<S57Object, RowBuffer, FeatureType>> {
            { "DISMAR", (current, buffer) => { return DISMAR((PortsAndServices)current, buffer); } },
            { "BERTHS", (current, buffer) => { return BERTHS((PortsAndServices)current, buffer); } },
            { "NAVLNE", (current, buffer) => { return NAVLNE((TracksAndRoutes)current, buffer); } },
            { "ADMARE", (current, buffer) => { return ADMARE((RegulatedAreasAndLimits)current, buffer); } },
            { "RESARE", (current, buffer) => { return RESARE((RegulatedAreasAndLimits)current, buffer); } },
            { "ACHARE", (current, buffer) => { return ACHARE((RegulatedAreasAndLimits)current, buffer); } },
            { "CTSARE", (current, buffer) => { return CTSARE((RegulatedAreasAndLimits)current, buffer); } },
            { "DWRTPT", (current, buffer) => { return DWRTPT((TracksAndRoutes)current, buffer); } },
            { "DRGARE", (current, buffer) => { return DRGARE((Depths)current, buffer); } },
            { "DMPGRD", (current, buffer) => { return DMPGRD((RegulatedAreasAndLimits)current, buffer); } },
            { "FAIRWY", (current, buffer) => { return FAIRWY((TracksAndRoutes)current, buffer); } },
            { "HRBFAC", (current, buffer) => { return HRBFAC((PortsAndServices)current, buffer); } },
            { "ISTZNE", (current, buffer) => { return ISTZNE((TracksAndRoutes)current, buffer); } },
            { "MARCUL", (current, buffer) => { return MARCUL((RegulatedAreasAndLimits)current, buffer); } },
            { "MIPARE", (current, buffer) => { return MIPARE((MilitaryFeatures)current, buffer); } },
            { "OSPARE", (current, buffer) => { return OSPARE((OffshoreInstallations)current, buffer); } },
            { "PRCARE", (current, buffer) => { return PRCARE((TracksAndRoutes)current, buffer); } },
            { "CGUSTA", (current, buffer) => { return CGUSTA((PortsAndServices)current, buffer); } },
            { "SPLARE", (current, buffer) => { return SPLARE((RegulatedAreasAndLimits)current, buffer); } },
            { "CBLARE", (current, buffer) => { return CBLARE((OffshoreInstallations)current, buffer); } },
            { "TSSLPT", (current, buffer) => { return TSSLPT((TracksAndRoutes)current, buffer); } },
            { "TSSRON", (current, buffer) => { return TSSRON((TracksAndRoutes)current, buffer); } },
            { "PIPARE", (current, buffer) => { return PIPARE((OffshoreInstallations)current, buffer); } },
            { "TESARE", (current, buffer) => { return TESARE((RegulatedAreasAndLimits)current, buffer); } },
            { "RSCSTA", (current, buffer) => { return RSCSTA((PortsAndServices)current, buffer); } },
        };

        private static Regex regexWaterwayDistance = new Regex(@"(Waterway distance =)\s(?<value>\d+)\s(?<unit>\.+)", RegexOptions.IgnoreCase);

        private static Regex regexMaximumDraughtPermitted = new Regex(@"(Maximum draught permitted =)\s(?<value>\d+\.?\d*)", RegexOptions.IgnoreCase);

        private static Regex regexVesselTrafficServiceArea = new Regex(@"(Vessel Traffic Service Area)", RegexOptions.IgnoreCase);

        private static Regex regexPilotageDistrict = new Regex(@"(Pilotage District)", RegexOptions.IgnoreCase);

        private static Regex regexMaritimeRescue = new Regex(@"(Maritime Rescue)", RegexOptions.IgnoreCase);

        private static Regex regexCoordinationCentre = new Regex(@"(Coordination Centre)", RegexOptions.IgnoreCase);

        private static Regex regexMarinePollutionRegulationsArea = new Regex(@"(Marine Pollution Regulations Area)", RegexOptions.IgnoreCase);

        private static Regex regexVesselSpeedLimit = new Regex(@"(Speed limit is)\s(?<value>\d+)\s(?<unit>\.+)", RegexOptions.IgnoreCase);   //  Speed limit is 5 knots

        private static FeatureType Build(string code, S57Object feature, RowBuffer buffer) => _builders[code]?.Invoke(feature, buffer)!;

        private static DistanceMark DISMAR(PortsAndServices current, RowBuffer buffer) {
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

        private static Berth BERTHS(PortsAndServices current, RowBuffer buffer) {
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

        private static NavigationLine NAVLNE(TracksAndRoutes current, RowBuffer buffer) {
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

        private static FeatureType ADMARE(RegulatedAreasAndLimits current, RowBuffer buffer) {
            if (!string.IsNullOrEmpty(current.INFORM) && regexPilotageDistrict.IsMatch(current.INFORM)) {
                var instance = new PilotageDistrict {
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
            else if (!string.IsNullOrEmpty(current.INFORM) && regexMarinePollutionRegulationsArea.IsMatch(current.INFORM)) {
                var instance = new MarinePollutionRegulationsArea();

                var featureName = GetFeatureName(current.OBJNAM, current.NOBJNM);
                if (featureName is not null)
                    instance.featureName = featureName;

                buffer["ps"] = ps101;
                buffer["code"] = instance.GetType().Name;
                buffer["attributebindings"] = instance.Flatten();
                buffer["informationbindings"] = System.Text.Json.JsonSerializer.Serialize(instance.GetInformationBindings(), jsonSerializerOptions);

                SetShape(buffer, current.SHAPE); buffer["sourceIdentifier"] = instance.sourceIdentifier;
                SetUsageBand(buffer, current.PLTS_COMP_SCALE!.Value);
                return instance;
            }
            else if (!string.IsNullOrEmpty(current.INFORM) && regexVesselTrafficServiceArea.IsMatch(current.INFORM)) {
                var instance = new VesselTrafficServiceArea();

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

        private static RestrictedArea RESARE(RegulatedAreasAndLimits current, RowBuffer buffer) {
            var instance = new RestrictedArea();

            if (current.CATREA != default) {
                if (current.CATREA != "26") { // Water Skiing Area
                                              // CATREA
                    var categoryOfRestrictedArea = EnumHelper.GetEnumValues(current.CATREA);
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

            var featureName = GetFeatureName(current.OBJNAM, current.NOBJNM);
            if (featureName is not null)
                instance.featureName = featureName;

            DateHelper.TryGetFixedDateRange(current.DATSTA, current.DATEND, out var dateRange);
            if (dateRange != default) {
                instance.fixedDateRange = dateRange;
            }

            // TODO: InteroperabilityIdentifier

            DateHelper.TryGetPeriodicDateRange(current.PERSTA, current.PEREND, out var periodicDateRange);
            if (periodicDateRange != default) {
                instance.periodicDateRange = periodicDateRange;
            }

            if (current.RESTRN != default) {
                var restriction = EnumHelper.GetEnumValues(current.RESTRN);
                if (restriction is not null && restriction.Any())
                    instance.restriction = restriction;
            }

            if (current.STATUS != default) {
                instance.status = GetStatus(current.STATUS);
            }

            if (!string.IsNullOrEmpty(current.INFORM) && regexVesselSpeedLimit.IsMatch(current.INFORM)) {
                var _value = regexVesselSpeedLimit.Match(current.INFORM).Groups["value"]?.Value;
                var _unit = regexVesselSpeedLimit.Match(current.INFORM).Groups["unit"]?.Value?.ToLowerInvariant();

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

            buffer["ps"] = ps101;
            buffer["code"] = instance.GetType().Name;
            buffer["attributebindings"] = instance.Flatten();
            buffer["informationbindings"] = System.Text.Json.JsonSerializer.Serialize(instance.GetInformationBindings(), jsonSerializerOptions);

            SetShape(buffer, current.SHAPE); buffer["sourceIdentifier"] = instance.sourceIdentifier;
            SetUsageBand(buffer, current.PLTS_COMP_SCALE!.Value);

            return instance;

        }

        private static FeatureType ACHARE(RegulatedAreasAndLimits current, RowBuffer buffer) {
            if (current.CATACH == "8") {
                //throw new NotSupportedException("Anchorage area category 8 not implemented. Create mooring area.");

                var instance = new MooringArea();

                var featureName = GetFeatureName(current.OBJNAM, current.NOBJNM);
                if (featureName is not null)
                    instance.featureName = featureName;

                DateHelper.TryGetFixedDateRange(current.DATSTA, current.DATEND, out var dateRange);
                if (dateRange != default) {
                    instance.fixedDateRange = dateRange;
                }

                if (current.RESTRN != default) {
                    var restriction = EnumHelper.GetEnumValues(current.RESTRN);
                    if (restriction is not null && restriction.Any())
                        instance.restriction = restriction;
                }

                if (current.STATUS != default) {
                    instance.status = GetStatus(current.STATUS);
                }

                if (!string.IsNullOrEmpty(current.INFORM) && regexVesselSpeedLimit.IsMatch(current.INFORM)) {
                    var _value = regexVesselSpeedLimit.Match(current.INFORM).Groups["value"]?.Value;
                    var _unit = regexVesselSpeedLimit.Match(current.INFORM).Groups["unit"]?.Value?.ToLowerInvariant();

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
                }

                if (current.PLTS_COMP_SCALE.HasValue && current.SHAPE != null) {
                    string subtype = "";
                    if (current.TableName != default && current.FCSUBTYPE.HasValue && !Subtypes.Instance.TryGetSubtype(current.TableName, current.FCSUBTYPE.Value, out subtype))
                        throw new NotSupportedException($"Unknown subtype for {current.TableName}, {current.FCSUBTYPE.Value}");
                    var scamin = Scamin.Instance.GetMinimumScale(current, subtype, current.PLTS_COMP_SCALE!.Value, isRelatedToStructure: false);
                    if (scamin.HasValue) {
                        instance.scaleMinimum = scamin.Value;
                    }
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
                var instance = new AnchorageArea();

                if (current.CATACH != default) {
                    var categoryOfAnchorage = EnumHelper.GetEnumValues(current.CATACH);
                    if (categoryOfAnchorage is not null && categoryOfAnchorage.Any())
                        instance.categoryOfAnchorage = categoryOfAnchorage;
                }

                // new S-101
                //instance.categoryOfCargo
                var featureName = GetFeatureName(current.OBJNAM, current.NOBJNM);
                if (featureName is not null)
                    instance.featureName = featureName;

                DateHelper.TryGetFixedDateRange(current.DATSTA, current.DATEND, out var dateRange);
                if (dateRange != default) {
                    instance.fixedDateRange = dateRange;
                }

                // TODO: interoperabilityIdentifier

                DateHelper.TryGetPeriodicDateRange(current.PERSTA, current.PEREND, out var periodicDateRange);
                if (periodicDateRange != default) {
                    instance.periodicDateRange = periodicDateRange;
                }

                if (current.RESTRN != default) {
                    var restriction = EnumHelper.GetEnumValues(current.RESTRN);
                    if (restriction is not null && restriction.Any())
                        instance.restriction = restriction;
                }

                if (current.STATUS != default) {
                    instance.status = GetStatus(current.STATUS);
                }

                if (!string.IsNullOrEmpty(current.INFORM) && regexVesselSpeedLimit.IsMatch(current.INFORM)) {
                    var _value = regexVesselSpeedLimit.Match(current.INFORM).Groups["value"]?.Value;
                    var _unit = regexVesselSpeedLimit.Match(current.INFORM).Groups["unit"]?.Value?.ToLowerInvariant();

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

                buffer["ps"] = ps101;
                buffer["code"] = instance.GetType().Name;
                buffer["attributebindings"] = instance.Flatten();
                buffer["informationbindings"] = System.Text.Json.JsonSerializer.Serialize(instance.GetInformationBindings(), jsonSerializerOptions);

                SetShape(buffer, current.SHAPE); buffer["sourceIdentifier"] = instance.sourceIdentifier;
                SetUsageBand(buffer, current.PLTS_COMP_SCALE!.Value);
                return instance;
            }
        }

        private static CargoTranshipmentArea CTSARE(RegulatedAreasAndLimits current, RowBuffer buffer) {
            var instance = new CargoTranshipmentArea();

            var featureName = GetFeatureName(current.OBJNAM, current.NOBJNM);
            if (featureName is not null)
                instance.featureName = featureName;


            DateHelper.TryGetFixedDateRange(current.DATSTA, current.DATEND, out var dateRange);
            if (dateRange != default) {
                instance.fixedDateRange = dateRange;
            }

            // TODO: interoperabilityIdentifier

            DateHelper.TryGetPeriodicDateRange(current.PERSTA, current.PEREND, out var periodicDateRange);
            if (periodicDateRange != default) {
                instance.periodicDateRange = periodicDateRange;
            }

            if (current.RESTRN != default) {
                var restriction = EnumHelper.GetEnumValues(current.RESTRN);
                if (restriction is not null && restriction.Any())
                    instance.restriction = restriction;
            }

            if (current.STATUS != default) {
                instance.status = GetStatus(current.STATUS);
            }

            if (!string.IsNullOrEmpty(current.INFORM) && regexVesselSpeedLimit.IsMatch(current.INFORM)) {
                var _value = regexVesselSpeedLimit.Match(current.INFORM).Groups["value"]?.Value;
                var _unit = regexVesselSpeedLimit.Match(current.INFORM).Groups["unit"]?.Value?.ToLowerInvariant();

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

            buffer["ps"] = ps101;
            buffer["code"] = instance.GetType().Name;
            buffer["attributebindings"] = instance.Flatten();
            buffer["informationbindings"] = System.Text.Json.JsonSerializer.Serialize(instance.GetInformationBindings(), jsonSerializerOptions);

            SetShape(buffer, current.SHAPE); buffer["sourceIdentifier"] = instance.sourceIdentifier;
            SetUsageBand(buffer, current.PLTS_COMP_SCALE!.Value);

            return instance;
        }

        private static DeepWaterRoutePart DWRTPT(TracksAndRoutes current, RowBuffer buffer) {
            var instance = new DeepWaterRoutePart();

            if (current.ORIENT.HasValue) {
                instance.orientationValue = current.ORIENT.Value == -32767m ? default : current.ORIENT.Value;
            }
            if (current.DRVAL1.HasValue) {
                instance.depthRangeMinimumValue = current.DRVAL1.Value == -32767m ? default : current.DRVAL1.Value;
            }
            if (current.TRAFIC.HasValue) {
                instance.trafficFlow = EnumHelper.GetEnumValue(current.TRAFIC!.Value);
            }

            var featureName = GetFeatureName(current.OBJNAM, current.NOBJNM);
            if (featureName is not null)
                instance.featureName = featureName;

            DateHelper.TryGetFixedDateRange(current.DATSTA, current.DATEND, out var dateRange);
            if (dateRange != default) {
                instance.fixedDateRange = dateRange;
            }

            // TODO: imoAdopted
            //instance.iMOAdopted = null;

            // TODO: InteroperabilityIdentifier

            if (current.QUASOU != default) {
                var qualityOfVerticalMeasurement = EnumHelper.GetEnumValues(current.QUASOU);
                if (qualityOfVerticalMeasurement is not null && qualityOfVerticalMeasurement.Any())
                    instance.qualityOfVerticalMeasurement = qualityOfVerticalMeasurement;
            }

            if (current.RESTRN != default) {
                var restriction = EnumHelper.GetEnumValues(current.RESTRN);
                if (restriction is not null && restriction.Any())
                    instance.restriction = restriction;
            }

            if (current.STATUS != default) {
                instance.status = GetStatus(current.STATUS);
            }

            if (current.TECSOU != null) {
                var techniqueOfVerticalMeasurement = EnumHelper.GetEnumValues(current.TECSOU);
                if (techniqueOfVerticalMeasurement is not null && techniqueOfVerticalMeasurement.Any())
                    instance.techniqueOfVerticalMeasurement = techniqueOfVerticalMeasurement;
            }

            if (current.TRAFIC.HasValue) {
                instance.trafficFlow = EnumHelper.GetEnumValue(current.TRAFIC.Value);
            }

            if (current.SOUACC.HasValue) {
                instance.verticalUncertainty = new() {
                    uncertaintyFixed = current.SOUACC.Value
                };
            }

            if (!string.IsNullOrEmpty(current.INFORM) && regexVesselSpeedLimit.IsMatch(current.INFORM)) {
                var _value = regexVesselSpeedLimit.Match(current.INFORM).Groups["value"]?.Value;
                var _unit = regexVesselSpeedLimit.Match(current.INFORM).Groups["unit"]?.Value?.ToLowerInvariant();

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

        private static DredgedArea DRGARE(Depths current, RowBuffer buffer) {
            var drval1 = current.DRVAL1 ?? default;
            var drval2 = current.DRVAL2 ?? default(decimal?);
            var sordat = current.SORDAT ?? default;

            var restrn = current.RESTRN ?? default;
            var quasou = current.QUASOU ?? default;
            var tecsou = current.TECSOU ?? default;

            var instance = new DredgedArea {
                depthRangeMinimumValue = drval1,
            };

            if (drval2.HasValue)
                instance.depthRangeMaximumValue = drval2.GetValueOrDefault();

            if (!string.IsNullOrEmpty(current.SORDAT)) {
                if (DateHelper.TryConvertSordat(current.SORDAT, out var reportedDate)) {
                    instance.dredgedDate = reportedDate;
                }
            }
            else {
                Logger.Current.DataError(current.OBJECTID ?? -1, current.GetType().Name, current.LNAM ?? "Unknown LNAM", $"Cannot convert date {current.SORDAT}");
            }


            var featureName = GetFeatureName(current.OBJNAM, current.NOBJNM);
            if (featureName is not null)
                instance.featureName = featureName;

            if (current.RESTRN != default) {
                var restriction = EnumHelper.GetEnumValues(current.RESTRN);
                if (restriction is not null && restriction.Any())
                    instance.restriction = restriction;
            }

            // TODO: InteroperabilityIdentifier

            // TODO: maximumPermittedDraught - From INFORM - No instances in GST - Not converted


            // The S-57 attribute QUASOU for DEPARE will not be converted. It is considered that this attribute is
            // not relevant for Depth Area in S-101.
            //if (current.QUASOU != default) {
            //    instance.qualityOfVerticalMeasurement = EnumHelper.GetEnumValue<qualityOfVerticalMeasurement>(current);
            //}

            if (!string.IsNullOrEmpty(restrn)) {
                var restriction = EnumHelper.GetEnumValues(restrn);
                if (restriction is not null && restriction.Any())
                    instance.restriction = restriction;
            }

            if (!string.IsNullOrEmpty(tecsou)) {
                var techniqueOfVerticalMeasurement = EnumHelper.GetEnumValues(tecsou);
                if (techniqueOfVerticalMeasurement is not null && techniqueOfVerticalMeasurement.Any())
                    instance.techniqueOfVerticalMeasurement = techniqueOfVerticalMeasurement;
            }

            //TODO: verticalUncertainty - Not converted
            //if (current.SOUACC.HasValue) {
            //    instance.verticalUncertainty = new DomainModel.S101.ComplexAttributes.verticalUncertainty() {
            //        uncertaintyFixed = current.SOUACC.Value
            //    };
            //}
            //

            if (!string.IsNullOrEmpty(current.INFORM) && regexVesselSpeedLimit.IsMatch(current.INFORM)) {
                var _value = regexVesselSpeedLimit.Match(current.INFORM).Groups["value"]?.Value;
                var _unit = regexVesselSpeedLimit.Match(current.INFORM).Groups["unit"]?.Value?.ToLowerInvariant();

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

        private static DumpingGround DMPGRD(RegulatedAreasAndLimits current, RowBuffer buffer) {
            var instance = new DumpingGround();

            if (current.CATDPG != default) {
                var categoryOfDumpingGround = EnumHelper.GetEnumValues(current.CATDPG);
                if (categoryOfDumpingGround is not null && categoryOfDumpingGround.Any())
                    instance.categoryOfDumpingGround = categoryOfDumpingGround;
            }

            // TODO: DateDisused

            var featureName = GetFeatureName(current.OBJNAM, current.NOBJNM);
            if (featureName is not null)
                instance.featureName = featureName;

            // TODO: interoperabilityIdentifier


            if (current.RESTRN != default) {
                var restriction = EnumHelper.GetEnumValues(current.RESTRN);
                if (restriction is not null && restriction.Any())
                    instance.restriction = restriction;
            }


            if (current.STATUS != default) {
                instance.status = GetStatus(current.STATUS);
            }

            if (!string.IsNullOrEmpty(current.INFORM) && regexVesselSpeedLimit.IsMatch(current.INFORM)) {
                var _value = regexVesselSpeedLimit.Match(current.INFORM).Groups["value"]?.Value;
                var _unit = regexVesselSpeedLimit.Match(current.INFORM).Groups["unit"]?.Value?.ToLowerInvariant();

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

            buffer["ps"] = ps101;
            buffer["code"] = instance.GetType().Name;
            buffer["attributebindings"] = instance.Flatten();
            buffer["informationbindings"] = System.Text.Json.JsonSerializer.Serialize(instance.GetInformationBindings(), jsonSerializerOptions);

            SetShape(buffer, current.SHAPE); buffer["sourceIdentifier"] = instance.sourceIdentifier;
            SetUsageBand(buffer, current.PLTS_COMP_SCALE!.Value);
            return instance;
        }

        private static Fairway FAIRWY(TracksAndRoutes current, RowBuffer buffer) {
            var instance = new Fairway();

            if (current.DRVAL1.HasValue) {
                instance.depthRangeMinimumValue = current.DRVAL1.Value != -32767m ? current.DRVAL1.Value : null;
            }

            var featureName = GetFeatureName(current.OBJNAM, current.NOBJNM);
            if (featureName is not null)
                instance.featureName = featureName;

            DateHelper.TryGetFixedDateRange(current.DATSTA, current.DATEND, out var dateRange);
            if (dateRange != default) {
                instance.fixedDateRange = dateRange;
            }

            // TODO: interoperabilityIdentifier

            // TODO: maximumPermittedDraught

            if (current.ORIENT.HasValue) {
                instance.orientationValue = current.ORIENT.Value != -32767m ? current.ORIENT.Value : null;
            }

            if (current.QUASOU != default) {
                var qualityOfVerticalMeasurement = EnumHelper.GetEnumValues(current.QUASOU);
                if (qualityOfVerticalMeasurement is not null && qualityOfVerticalMeasurement.Any())
                    instance.qualityOfVerticalMeasurement = qualityOfVerticalMeasurement;
            }

            if (current.RESTRN != default) {
                var restriction = EnumHelper.GetEnumValues(current.RESTRN);
                if (restriction is not null && restriction.Any())
                    instance.restriction = restriction;
            }

            if (current.STATUS != default) {
                instance.status = GetStatus(current.STATUS);
            }

            if (current.TRAFIC.HasValue) {
                instance.trafficFlow = EnumHelper.GetEnumValue(current.TRAFIC.Value);
            }

            if (current.SOUACC.HasValue) {
                instance.verticalUncertainty = new() {
                    uncertaintyFixed = current.SOUACC.Value
                };
            }

            if (!string.IsNullOrEmpty(current.INFORM) && regexVesselSpeedLimit.IsMatch(current.INFORM)) {
                var _value = regexVesselSpeedLimit.Match(current.INFORM).Groups["value"]?.Value;
                var _unit = regexVesselSpeedLimit.Match(current.INFORM).Groups["unit"]?.Value?.ToLowerInvariant();

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

        private static HarbourFacility HRBFAC(PortsAndServices current, RowBuffer buffer) {
            var instance = new HarbourFacility();

            if (current.CATHAF != default) {
                var categoryOfHarbourFacility = EnumHelper.GetEnumValues(current.CATHAF);
                if (categoryOfHarbourFacility is not null)
                    instance.categoryOfHarbourFacility = categoryOfHarbourFacility;
            }

            if (current.COMCHA != default) {
                instance.communicationChannel = GetCommunicationChannel(current.COMCHA);
            }

            if (current.CONDTN.HasValue) {
                instance.condition = GetCondition(current.CONDTN.Value)?.value;
            }

            var featureName = GetFeatureName(current.OBJNAM, current.NOBJNM);
            if (featureName is not null)
                instance.featureName = featureName;

            DateHelper.TryGetFixedDateRange(current.DATSTA, current.DATEND, out var dateRange);
            if (dateRange != default) {
                instance.fixedDateRange = dateRange;
            }

            // TODO: interoperabilityIdentifier

            if (current.NATCON != default) {
                var natureOfConstruction = EnumHelper.GetEnumValues(current.NATCON);
                if (natureOfConstruction is not null && natureOfConstruction.Any())
                    instance.natureOfConstruction = natureOfConstruction;
            }

            DateHelper.TryGetPeriodicDateRange(current.PERSTA, current.PEREND, out var periodicDateRange);
            if (periodicDateRange != default) {
                instance.periodicDateRange = periodicDateRange;
            }

            // TODO: product

            if (!string.IsNullOrEmpty(current.SORDAT)) {
                if (DateHelper.TryConvertSordat(current.SORDAT, out var reportedDate)) {
                    instance.reportedDate = reportedDate;
                }
                else {
                    Logger.Current.DataError(current.OBJECTID ?? -1, current.GetType().Name, current.LNAM ?? "Unknown LNAM", $"Cannot convert date {current.SORDAT}");
                }
            }

            // TODO: restriction

            if (current.STATUS != default) {
                instance.status = GetStatus(current.STATUS);
            }

            if (!string.IsNullOrEmpty(current.INFORM) && regexVesselSpeedLimit.IsMatch(current.INFORM)) {
                var _value = regexVesselSpeedLimit.Match(current.INFORM).Groups["value"]?.Value;
                var _unit = regexVesselSpeedLimit.Match(current.INFORM).Groups["unit"]?.Value?.ToLowerInvariant();

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

        private static InshoreTrafficZone ISTZNE(TracksAndRoutes current, RowBuffer buffer) {
            var instance = new InshoreTrafficZone();

            DateHelper.TryGetFixedDateRange(current.DATSTA, current.DATEND, out var dateRange);
            if (dateRange != default) {
                instance.fixedDateRange = dateRange;
            }

            // TODO: interoperabilityIdentifier

            if (current.RESTRN != default) {
                var restriction = EnumHelper.GetEnumValues(current.RESTRN);
                if (restriction is not null && restriction.Any())
                    instance.restriction = restriction;
            }

            if (current.STATUS != default) {
                instance.status = GetStatus(current.STATUS);
            }

            if (!string.IsNullOrEmpty(current.INFORM) && regexVesselSpeedLimit.IsMatch(current.INFORM)) {
                var _value = regexVesselSpeedLimit.Match(current.INFORM).Groups["value"]?.Value;
                var _unit = regexVesselSpeedLimit.Match(current.INFORM).Groups["unit"]?.Value?.ToLowerInvariant();

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

        private static MarineFarmCulture MARCUL(RegulatedAreasAndLimits current, RowBuffer buffer) {
            var instance = new MarineFarmCulture();

            if (current.CATMFA != null) {
                instance.categoryOfMarineFarmCulture = EnumHelper.GetEnumValue(current.CATMFA);
            }

            if (current.EXPSOU.HasValue) {
                instance.expositionOfSounding = EnumHelper.GetEnumValue(current.EXPSOU.Value);
            }

            var featureName = GetFeatureName(current.OBJNAM, current.NOBJNM);
            if (featureName is not null)
                instance.featureName = featureName;

            DateHelper.TryGetFixedDateRange(current.DATSTA, current.DATEND, out var dateRange);
            if (dateRange != default) {
                instance.fixedDateRange = dateRange;
            }

            // TODO: interoperability identifier

            DateHelper.TryGetPeriodicDateRange(current.PERSTA, current.PEREND, out var periodicDateRange);
            if (periodicDateRange != default) {
                instance.periodicDateRange = periodicDateRange;
            }

            if (current.QUASOU != default) {
                var qualityOfVerticalMeasurement = EnumHelper.GetEnumValues(current.QUASOU);
                if (qualityOfVerticalMeasurement is not null && qualityOfVerticalMeasurement.Any())
                    instance.qualityOfVerticalMeasurement = qualityOfVerticalMeasurement;
            }

            if (current.RESTRN != default) {
                var restriction = EnumHelper.GetEnumValues(current.RESTRN);
                if (restriction is not null && restriction.Any())
                    instance.restriction = restriction;
            }

            if (current.STATUS != default) {
                instance.status = GetStatus(current.STATUS);
            }

            if (current.VALSOU.HasValue) {
                instance.valueOfSounding = current.VALSOU.Value != -32767m ? current.VALSOU.Value : null;
            }
            else {
                // Exactly one of the attributes height or value of sounding must be populated
                if (current.WATLEV.HasValue && new int[] { 1, 2, -32767 }.Contains(current.WATLEV.Value)) {
                    instance.height = null;
                }
                else
                    instance.valueOfSounding = null;
            }


            if (current.VERLEN.HasValue) {
                instance.verticalLength = current.VERLEN.Value != -32767m ? current.VERLEN.Value : null;
            }
            else if (current.VERLEN.HasValue && current.VERLEN.Value == -32767m) {
                //instance.verticalLength = default(decimal?);
            }

            // TODO: VerticalUncertainty

            if (!string.IsNullOrEmpty(current.INFORM) && regexVesselSpeedLimit.IsMatch(current.INFORM)) {
                var _value = regexVesselSpeedLimit.Match(current.INFORM).Groups["value"]?.Value;
                var _unit = regexVesselSpeedLimit.Match(current.INFORM).Groups["unit"]?.Value?.ToLowerInvariant();

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
            }

            if (current.WATLEV.HasValue) {
                instance.waterLevelEffect = EnumHelper.GetEnumValue(current.WATLEV);
            }

            // TODO: HEIGHT                            
            if (instance.waterLevelEffect == 1 || instance.waterLevelEffect == 2) {
                /* The attribute height must be populated for Marine Farm/Culture features having attribute water level
                   effect = 1 (partly submerged at high water) or 2 (always dry). */


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

            buffer["ps"] = ps101;
            buffer["code"] = instance.GetType().Name;
            buffer["attributebindings"] = instance.Flatten();
            buffer["informationbindings"] = System.Text.Json.JsonSerializer.Serialize(instance.GetInformationBindings(), jsonSerializerOptions);

            SetShape(buffer, current.SHAPE); buffer["sourceIdentifier"] = instance.sourceIdentifier;
            SetUsageBand(buffer, current.PLTS_COMP_SCALE!.Value);
            return instance;
        }

        private static MilitaryPracticeArea MIPARE(MilitaryFeatures current, RowBuffer buffer) {
            var instance = new MilitaryPracticeArea();

            if (current.CATMPA != default) {
                var categoryOfMilitaryPracticeArea = EnumHelper.GetEnumValues(current.CATMPA);
                if (categoryOfMilitaryPracticeArea is not null && categoryOfMilitaryPracticeArea.Any())
                    instance.categoryOfMilitaryPracticeArea = categoryOfMilitaryPracticeArea;
            }

            var featureName = GetFeatureName(current.OBJNAM, current.NOBJNM);
            if (featureName is not null)
                instance.featureName = featureName;

            DateHelper.TryGetFixedDateRange(current.DATSTA, current.DATEND, out var dateRange);
            if (dateRange != default) {
                instance.fixedDateRange = dateRange;
            }

            // TODO: interoperabilityIdentifier

            // TODO: nationality

            DateHelper.TryGetPeriodicDateRange(current.PERSTA, current.PEREND, out var periodicDateRange);
            if (periodicDateRange != default) {
                instance.periodicDateRange = periodicDateRange;
            }

            if (current.RESTRN != default) {
                var restriction = EnumHelper.GetEnumValues(current.RESTRN);
                if (restriction is not null && restriction.Any())
                    instance.restriction = restriction;
            }

            if (current.STATUS != default) {
                instance.status = GetStatus(current.STATUS);
            }

            if (!string.IsNullOrEmpty(current.INFORM) && regexVesselSpeedLimit.IsMatch(current.INFORM)) {
                var _value = regexVesselSpeedLimit.Match(current.INFORM).Groups["value"]?.Value;
                var _unit = regexVesselSpeedLimit.Match(current.INFORM).Groups["unit"]?.Value?.ToLowerInvariant();

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

            buffer["ps"] = ps101;
            buffer["code"] = instance.GetType().Name;
            buffer["attributebindings"] = instance.Flatten();
            buffer["informationbindings"] = System.Text.Json.JsonSerializer.Serialize(instance.GetInformationBindings(), jsonSerializerOptions);

            SetShape(buffer, current.SHAPE); buffer["sourceIdentifier"] = instance.sourceIdentifier;
            SetUsageBand(buffer, current.PLTS_COMP_SCALE!.Value);
            return instance;
        }

        private static OffshoreProductionArea OSPARE(OffshoreInstallations current, RowBuffer buffer) {
            var instance = new OffshoreProductionArea();

            if (current.CATPRA.HasValue) {
                // Windfarm
                if (current.CATPRA.Value == 9) {
                    instance.categoryOfOffshoreProductionArea = 1;  // categoryOfOffshoreProductionArea.WindFarm;
                }
                else if (current.CATPRA.Value == 8) {
                    instance.categoryOfOffshoreProductionArea = 4;  // categoryOfOffshoreProductionArea.TankFarm;
                }
                else {
                    //Logger.Current.DataError(current.OBJECTID!.Value, tableName, current.LNAM ?? "Unknown LNAM", $"Cannot convert OffshoreInstallation with CATPRA = {current.CATPRA.Value}");
                    //continue;
                    throw new NotSupportedException($"Cannot convert offshoreproductionarea with CATPRA {current.CATPRA.Value}");
                    //instance.categoryOfOffshoreProductionArea = EnumHelper.GetEnumValue<categoryOfOffshoreProductionArea>(current.CATPRA.Value);
                }
            }

            if (current.CONDTN.HasValue) {
                instance.condition = GetCondition(current.CONDTN.Value)?.value;
            }

            var featureName = GetFeatureName(current.OBJNAM, current.NOBJNM);
            if (featureName is not null)
                instance.featureName = featureName;

            DateHelper.TryGetFixedDateRange(current.DATSTA, current.DATEND, out var dateRange);
            if (dateRange != default) {
                instance.fixedDateRange = dateRange;
            }

            if (current.HEIGHT.HasValue) {
                instance.height = current.HEIGHT.Value != -32767m ? current.HEIGHT.Value : null;
            }
            else {

            }

            // TODO: interoperabilityIdentifier

            if (current.PRODCT != null) {
                var product = EnumHelper.GetEnumValues(current.PRODCT);
                if (product is not null && product.Any())
                    instance.product = product;
            }

            if (current.CONRAD.HasValue) {
                instance.radarConspicuous = current.CONRAD.Value == 2 ? false : true;
            }
            if (!string.IsNullOrEmpty(current.SORDAT)) {
                if (DateHelper.TryConvertSordat(current.SORDAT, out var reportedDate)) {
                    instance.reportedDate = reportedDate;
                }
                else {
                    Logger.Current.DataError(current.OBJECTID ?? -1, current.GetType().Name, current.LNAM ?? "Unknown LNAM", $"Cannot convert date {current.SORDAT}");
                }
            }

            if (current.RESTRN != default) {
                var restriction = EnumHelper.GetEnumValues(current.RESTRN);
                if (restriction is not null && restriction.Any())
                    instance.restriction = restriction;
            }

            if (current.STATUS != default) {
                instance.status = GetStatus(current.STATUS);
            }

            if (current.VERLEN.HasValue) {
                instance.verticalLength = current.VERLEN.Value != -32767m ? current.VERLEN.Value : null;
            }
            else {
                //instance.verticalLength = default(decimal?);
            }

            if (!string.IsNullOrEmpty(current.INFORM) && regexVesselSpeedLimit.IsMatch(current.INFORM)) {
                var _value = regexVesselSpeedLimit.Match(current.INFORM).Groups["value"]?.Value;
                var _unit = regexVesselSpeedLimit.Match(current.INFORM).Groups["unit"]?.Value?.ToLowerInvariant();

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
            }

            if (current.CONVIS.HasValue) {
                instance.visualProminence = EnumHelper.GetEnumValue(current.CONVIS.Value);
            }

            // TODO: waterleveleffect

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

        private static PrecautionaryArea PRCARE(TracksAndRoutes current, RowBuffer buffer) {
            var instance = new PrecautionaryArea();

            var featureName = GetFeatureName(current.OBJNAM, current.NOBJNM);
            if (featureName is not null)
                instance.featureName = featureName;


            DateHelper.TryGetFixedDateRange(current.DATSTA, current.DATEND, out var dateRange);
            if (dateRange != default) {
                instance.fixedDateRange = dateRange;
            }

            // TODO: imoAdopted
            //instance.iMOAdopted = null;

            // TODO: interoperabilityIdentifier

            if (current.RESTRN != default) {
                var restriction = EnumHelper.GetEnumValues(current.RESTRN);
                if (restriction is not null && restriction.Any())
                    instance.restriction = restriction;
            }

            if (current.STATUS != default) {
                instance.status = GetStatus(current.STATUS);
            }

            if (!string.IsNullOrEmpty(current.INFORM) && regexVesselSpeedLimit.IsMatch(current.INFORM)) {
                var _value = regexVesselSpeedLimit.Match(current.INFORM).Groups["value"]?.Value;
                var _unit = regexVesselSpeedLimit.Match(current.INFORM).Groups["unit"]?.Value?.ToLowerInvariant();

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

        private static FeatureType CGUSTA(PortsAndServices current, RowBuffer buffer) {
            /*
                The S-101 Boolean attribute is MRCC has been introduced in S - 101 to indicate that a coast guard
                station also performs the function of a Maritime Rescue and Coordination Centres(MRCC). This
                information is encoded in S - 57 on CGUSTA using the attribute INFORM(see clause 2.3).In order
                for this information to be converted across to S - 101, the text string encoded in INFORM on the
                CGUSTA should be in a standardised format, such as Maritime Rescue and Coordination Centre.
            */

            var instance = new CoastGuardStation();

            if (!string.IsNullOrEmpty(current.INFORM) && regexMaritimeRescue.IsMatch(current.INFORM)) {
                instance.isMRCC = true;
            }
            else if (!string.IsNullOrEmpty(current.INFORM) && regexCoordinationCentre.IsMatch(current.INFORM)) {
                instance.isMRCC = true;
            }

            if (current.COMCHA != default) {
                instance.communicationChannel = current.COMCHA.Split(',').ToArray();
            }

            var featureName = GetFeatureName(current.OBJNAM, current.NOBJNM);
            if (featureName is not null)
                instance.featureName = featureName;

            DateHelper.TryGetFixedDateRange(current.DATSTA, current.DATEND, out var dateRange);
            if (dateRange != default) {
                instance.fixedDateRange = dateRange;
            }

            // TODO: interoperabilityIdentifier

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

        private static SeaplaneLandingArea SPLARE(RegulatedAreasAndLimits current, RowBuffer buffer) {
            var instance = new SeaplaneLandingArea();

            var featureName = GetFeatureName(current.OBJNAM, current.NOBJNM);
            if (featureName is not null)
                instance.featureName = featureName;


            // TODO: interoperabilityIdentifier

            DateHelper.TryGetPeriodicDateRange(current.PERSTA, current.PEREND, out var periodicDateRange);
            if (periodicDateRange != default) {
                instance.periodicDateRange = periodicDateRange;
            }

            if (current.RESTRN != default) {
                var restriction = EnumHelper.GetEnumValues(current.RESTRN);
                if (restriction is not null && restriction.Any())
                    instance.restriction = restriction;
            }

            if (current.STATUS != default) {
                instance.status = GetStatus(current.STATUS);
            }

            if (!string.IsNullOrEmpty(current.INFORM) && regexVesselSpeedLimit.IsMatch(current.INFORM)) {
                var _value = regexVesselSpeedLimit.Match(current.INFORM).Groups["value"]?.Value;
                var _unit = regexVesselSpeedLimit.Match(current.INFORM).Groups["unit"]?.Value?.ToLowerInvariant();

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

            buffer["ps"] = ps101;
            buffer["code"] = instance.GetType().Name;
            buffer["attributebindings"] = instance.Flatten();
            buffer["informationbindings"] = System.Text.Json.JsonSerializer.Serialize(instance.GetInformationBindings(), jsonSerializerOptions);

            SetShape(buffer, current.SHAPE); buffer["sourceIdentifier"] = instance.sourceIdentifier;
            SetUsageBand(buffer, current.PLTS_COMP_SCALE!.Value);
            return instance;
        }

        private static CableArea CBLARE(OffshoreInstallations current, RowBuffer buffer) {
            var instance = new CableArea();

            if (current.CATCBL.HasValue) {
                var categoryOfCable = EnumHelper.GetEnumValues(current.CATCBL.Value);
                if (categoryOfCable is not null && categoryOfCable.Any())
                    instance.categoryOfCable = categoryOfCable;
            }

            var featureName = GetFeatureName(current.OBJNAM, current.NOBJNM);
            if (featureName is not null)
                instance.featureName = featureName;

            DateHelper.TryGetFixedDateRange(current.DATSTA, current.DATEND, out var dateRange);
            if (dateRange != default) {
                instance.fixedDateRange = dateRange;
            }

            // TODO: interoperabilityIdentifier

            if (current.RESTRN != default) {
                var restriction = EnumHelper.GetEnumValues(current.RESTRN);
                if (restriction is not null && restriction.Any())
                    instance.restriction = restriction;
            }

            if (current.STATUS != default) {
                instance.status = GetStatus(current.STATUS);
            }

            if (!string.IsNullOrEmpty(current.INFORM) && regexVesselSpeedLimit.IsMatch(current.INFORM)) {
                var _value = regexVesselSpeedLimit.Match(current.INFORM).Groups["value"]?.Value;
                var _unit = regexVesselSpeedLimit.Match(current.INFORM).Groups["unit"]?.Value?.ToLowerInvariant();

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

        private static TrafficSeparationSchemeLanePart TSSLPT(TracksAndRoutes current, RowBuffer buffer) {
            var instance = new TrafficSeparationSchemeLanePart();

            DateHelper.TryGetFixedDateRange(current.DATSTA, current.DATEND, out var dateRange);
            if (dateRange != default) {
                instance.fixedDateRange = dateRange;
            }

            // TODO: interoperabilityIdentifier

            if (current.ORIENT.HasValue) {
                instance.orientationValue = current.ORIENT.Value == -32767m ? default : current.ORIENT.Value;
            }

            if (current.RESTRN != default) {
                var restriction = EnumHelper.GetEnumValues(current.RESTRN);
                if (restriction is not null && restriction.Any())
                    instance.restriction = restriction;
            }

            if (current.STATUS != default) {
                instance.status = GetStatus(current.STATUS);
            }

            if (!string.IsNullOrEmpty(current.INFORM) && regexVesselSpeedLimit.IsMatch(current.INFORM)) {
                var _value = regexVesselSpeedLimit.Match(current.INFORM).Groups["value"]?.Value;
                var _unit = regexVesselSpeedLimit.Match(current.INFORM).Groups["unit"]?.Value?.ToLowerInvariant();

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

        private static TrafficSeparationSchemeRoundabout TSSRON(TracksAndRoutes current, RowBuffer buffer) {
            var instance = new TrafficSeparationSchemeRoundabout();

            DateHelper.TryGetFixedDateRange(current.DATSTA, current.DATEND, out var dateRange);
            if (dateRange != default) {
                instance.fixedDateRange = dateRange;
            }

            // TODO: interoperabilityIdentifier

            if (current.RESTRN != default) {
                var restriction = EnumHelper.GetEnumValues(current.RESTRN);
                if (restriction is not null && restriction.Any())
                    instance.restriction = restriction;
            }

            if (current.STATUS != default) {
                instance.status = GetStatus(current.STATUS);
            }

            if (!string.IsNullOrEmpty(current.INFORM) && regexVesselSpeedLimit.IsMatch(current.INFORM)) {
                var _value = regexVesselSpeedLimit.Match(current.INFORM).Groups["value"]?.Value;
                var _unit = regexVesselSpeedLimit.Match(current.INFORM).Groups["unit"]?.Value?.ToLowerInvariant();

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

            buffer["ps"] = ps101;
            buffer["code"] = instance.GetType().Name;
            buffer["attributebindings"] = instance.Flatten();
            buffer["informationbindings"] = System.Text.Json.JsonSerializer.Serialize(instance.GetInformationBindings(), jsonSerializerOptions);

            SetShape(buffer, current.SHAPE); buffer["sourceIdentifier"] = instance.sourceIdentifier;
            SetUsageBand(buffer, current.PLTS_COMP_SCALE!.Value);
            return instance;
        }

        private static SubmarinePipelineArea PIPARE(OffshoreInstallations current, RowBuffer buffer) {
            var instance = new SubmarinePipelineArea();

            if (current.CATPIP != default) {
                var categoryOfPipelinePipe = EnumHelper.GetEnumValues(current.CATPIP);
                if (categoryOfPipelinePipe is not null && categoryOfPipelinePipe.Any())
                    instance.categoryOfPipelinePipe = categoryOfPipelinePipe;
            }

            var featureName = GetFeatureName(current.OBJNAM, current.NOBJNM);
            if (featureName is not null)
                instance.featureName = featureName;

            DateHelper.TryGetFixedDateRange(current.DATSTA, current.DATEND, out var dateRange);
            if (dateRange != default) {
                instance.fixedDateRange = dateRange;
            }

            // TODO: interoperabilityIdentifier

            if (current.PRODCT != null) {
                var product = EnumHelper.GetEnumValues(current.PRODCT);
                if (product is not null && product.Any())
                    instance.product = product;
            }

            if (current.RESTRN != default) {
                var restriction = EnumHelper.GetEnumValues(current.RESTRN);
                if (restriction is not null && restriction.Any())
                    instance.restriction = restriction;
            }

            if (current.STATUS != default) {
                instance.status = GetStatus(current.STATUS);
            }

            if (!string.IsNullOrEmpty(current.INFORM) && regexVesselSpeedLimit.IsMatch(current.INFORM)) {
                var _value = regexVesselSpeedLimit.Match(current.INFORM).Groups["value"]?.Value;
                var _unit = regexVesselSpeedLimit.Match(current.INFORM).Groups["unit"]?.Value?.ToLowerInvariant();

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

        private static TerritorialSeaArea TESARE(RegulatedAreasAndLimits current, RowBuffer buffer) {
            var instance = new TerritorialSeaArea();

            // TODO: inDispute

            // TODO: interoperabilityIdentifier

            if (current.NATION != default) {
                instance.nationality = [GetNation(current.NATION)];
            }

            if (current.RESTRN != default) {
                var restriction = EnumHelper.GetEnumValues(current.RESTRN);
                if (restriction is not null && restriction.Any())
                    instance.restriction = restriction;
            }

            if (!string.IsNullOrEmpty(current.INFORM) && regexVesselSpeedLimit.IsMatch(current.INFORM)) {
                var _value = regexVesselSpeedLimit.Match(current.INFORM).Groups["value"]?.Value;
                var _unit = regexVesselSpeedLimit.Match(current.INFORM).Groups["unit"]?.Value?.ToLowerInvariant();

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

            buffer["ps"] = ps101;
            buffer["code"] = instance.GetType().Name;
            buffer["attributebindings"] = instance.Flatten();
            buffer["informationbindings"] = System.Text.Json.JsonSerializer.Serialize(instance.GetInformationBindings(), jsonSerializerOptions);

            SetShape(buffer, current.SHAPE); buffer["sourceIdentifier"] = instance.sourceIdentifier;
            SetUsageBand(buffer, current.PLTS_COMP_SCALE!.Value);
            return instance;
        }

        private static RescueStation RSCSTA(PortsAndServices current, RowBuffer buffer) {
            var instance = new RescueStation();

            if (current.CATRSC != null) {
                var categoryOfRescueStation = EnumHelper.GetEnumValues(current.CATRSC);
                if (categoryOfRescueStation is not null && categoryOfRescueStation.Any())
                    instance.categoryOfRescueStation = categoryOfRescueStation;
            }

            if (current.COMCHA != default) {
                instance.communicationChannel = current.COMCHA.Split(',').ToArray();
            }

            var featureName = GetFeatureName(current.OBJNAM, current.NOBJNM);
            if (featureName is not null)
                instance.featureName = featureName;

            DateHelper.TryGetFixedDateRange(current.DATSTA, current.DATEND, out var dateRange);
            if (dateRange != default) {
                instance.fixedDateRange = dateRange;
            }

            // TODO: interoperabilityIdentifier            

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
    }
}
