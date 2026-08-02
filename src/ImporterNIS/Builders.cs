using ArcGIS.Core.Data;
using S100FC;
using S100FC.S101.ComplexAttributes;
using S100FC.S101.FeatureTypes;
using S100Framework.Applications.Singletons;
using System.Text.RegularExpressions;

namespace S100Framework.Applications
{
    using ArcGIS.Core.Geometry;
    using ArcGIS.Core.Internal.CIM;
    using NetTopologySuite.GeometriesGraph;
    using S100Framework.Applications.S57.esri;
    using S100Framework.Applications.S57auto.esri;
    using static System.Runtime.InteropServices.JavaScript.JSType;

    internal static partial class ImporterNIS
    {
        private static readonly Dictionary<string, Func<Feature, RowBuffer, FeatureType>> _builders = new Dictionary<string, Func<Feature, RowBuffer, FeatureType>> {
            { "DISMAR", (current, buffer) => { return DISMAR(current, buffer); } },
            { "BERTHS", (current, buffer) => { return BERTHS(current, buffer); } },
            { "NAVLNE", (current, buffer) => { return NAVLNE(current, buffer); } },
            { "ADMARE", (current, buffer) => { return ADMARE(current, buffer); } },
            { "RESARE", (current, buffer) => { return RESARE(current, buffer); } },
            { "ACHARE", (current, buffer) => { return ACHARE(current, buffer); } },
            { "CTSARE", (current, buffer) => { return CTSARE(current, buffer); } },
            { "DWRTPT", (current, buffer) => { return DWRTPT(current, buffer); } },
            { "DRGARE", (current, buffer) => { return DRGARE(current, buffer); } },
            { "DMPGRD", (current, buffer) => { return DMPGRD(current, buffer); } },
            { "FAIRWY", (current, buffer) => { return FAIRWY(current, buffer); } },
            { "HRBFAC", (current, buffer) => { return HRBFAC(current, buffer); } },
            { "ISTZNE", (current, buffer) => { return ISTZNE(current, buffer); } },
            { "MARCUL", (current, buffer) => { return MARCUL(current, buffer); } },
            { "MIPARE", (current, buffer) => { return MIPARE(current, buffer); } },
            { "OSPARE", (current, buffer) => { return OSPARE(current, buffer); } },
            { "PRCARE", (current, buffer) => { return PRCARE(current, buffer); } },
            { "CGUSTA", (current, buffer) => { return CGUSTA(current, buffer); } },
            { "SPLARE", (current, buffer) => { return SPLARE(current, buffer); } },
            { "CBLARE", (current, buffer) => { return CBLARE(current, buffer); } },
            { "TSSLPT", (current, buffer) => { return TSSLPT(current, buffer); } },
            { "TSSRON", (current, buffer) => { return TSSRON(current, buffer); } },
            { "PIPARE", (current, buffer) => { return PIPARE(current, buffer); } },
            { "TESARE", (current, buffer) => { return TESARE(current, buffer); } },
            { "RSCSTA", (current, buffer) => { return RSCSTA(current, buffer); } },
            { "M_ACCY", (current, buffer) => { return M_ACCY(current, buffer); } },
            { "SOUNDG", (current, buffer) => { return SOUNDG(current, buffer); } },
            { "TS_PAD", (current, buffer) => { return TS_PAD(current, buffer); } },
            { "CURENT", (current, buffer) => { return CURENT(current, buffer); } },
            { "WEDKLP", (current, buffer) => { return WEDKLP(current, buffer); } },
            { "SNDWAV", (current, buffer) => { return SNDWAV(current, buffer); } },
            { "SBDARE", (current, buffer) => { return SBDARE(current, buffer); } },
            { "SWPARE", (current, buffer) => { return SWPARE(current, buffer); } },
            { "PIPSOL", (current, buffer) => { return PIPSOL(current, buffer); } },
            { "SUBTLN", (current, buffer) => { return SUBTLN(current, buffer); } },
            { "RADRNG", (current, buffer) => { return RADRNG(current, buffer); } },
            { "STSLNE", (current, buffer) => { return STSLNE(current, buffer); } },
            { "ACHBRT", (current, buffer) => { return ACHBRT(current, buffer); } },
            { "CONZNE", (current, buffer) => { return CONZNE(current, buffer); } },
            { "COSARE", (current, buffer) => { return COSARE(current, buffer); } },
            { "EXEZNE", (current, buffer) => { return EXEZNE(current, buffer); } },
            { "FSHZNE", (current, buffer) => { return FSHZNE(current, buffer); } },
            { "HRBARE", (current, buffer) => { return HRBARE(current, buffer); } },
        };

        private static readonly Regex regexWaterwayDistance = new Regex(@"(Waterway distance =)\s(?<value>\d+)\s(?<unit>\D+)", RegexOptions.IgnoreCase);

        private static readonly Regex regexMaximumDraughtPermitted = new Regex(@"(Maximum draught permitted =)\s(?<value>\d+\.?\d*)", RegexOptions.IgnoreCase);

        private static readonly Regex regexVesselTrafficServiceArea = new Regex(@"(Vessel Traffic Service Area)", RegexOptions.IgnoreCase);

        private static readonly Regex regexPilotageDistrict = new Regex(@"(Pilotage District)", RegexOptions.IgnoreCase);

        private static readonly Regex regexMaritimeRescue = new Regex(@"(Maritime Rescue)", RegexOptions.IgnoreCase);

        private static readonly Regex regexCoordinationCentre = new Regex(@"(Coordination Centre)", RegexOptions.IgnoreCase);

        private static readonly Regex regexMarinePollutionRegulationsArea = new Regex(@"(Marine Pollution Regulations Area)", RegexOptions.IgnoreCase);

        private static readonly Regex regexVesselSpeedLimit = new Regex(@"(Speed limit is)\s(?<value>\d+)\s(?<unit>\D+)", RegexOptions.IgnoreCase);   //  Speed limit is 5 knots

        private static FeatureType Build(string code, Feature feature, RowBuffer buffer) => _builders[code]?.Invoke(feature, buffer)!;

        private static QualityOfNonBathymetricData M_ACCY(Feature current, RowBuffer buffer) {
            var instance = new QualityOfNonBathymetricData();

            // TODO
            if (current.HORACC_HasValue()) {
                var horacc = current.HORACC();
                instance.horizontalPositionUncertainty = new() {
                };
            }

            var result = ImporterNIS.AddInformation(current.GetObjectID(), current.TableName(), current.NTXTDS(), current.TXTDSC(), current.INFORM(), current.NINFOM());
            instance.information = [.. result.information];
            instance.SetInformationBindings(result.InformationBindings.ToArray());

            buffer["ps"] = ps101;
            buffer["code"] = instance.GetType().Name;
            buffer["attributebindings"] = instance.Flatten();
            buffer["informationbindings"] = System.Text.Json.JsonSerializer.Serialize(instance.GetInformationBindings(), jsonSerializerOptions);

            buffer["sourceIdentifier"] = instance.sourceIdentifier;
            SetShape(buffer, current.SHAPE());
            SetUsageBand(buffer, current.PLTS_COMP_SCALE()!.Value);

            return instance;
        }

        private static TidalStreamPanelData TS_PAD(Feature current, RowBuffer buffer) {
            var instance = new TidalStreamPanelData();

            // TODO

            var featureName = GetFeatureName(current.OBJNAM(), current.NOBJNM());
            if (featureName is not null)
                instance.featureName = featureName;

            var result = ImporterNIS.AddInformation(current.GetObjectID(), current.TableName(), current.NTXTDS(), current.TXTDSC(), current.INFORM(), current.NINFOM());
            instance.information = [.. result.information];
            instance.SetInformationBindings(result.InformationBindings.ToArray());

            buffer["ps"] = ps101;
            buffer["code"] = instance.GetType().Name;
            buffer["attributebindings"] = instance.Flatten();
            buffer["informationbindings"] = System.Text.Json.JsonSerializer.Serialize(instance.GetInformationBindings(), jsonSerializerOptions);

            buffer["sourceIdentifier"] = instance.sourceIdentifier;
            SetShape(buffer, current.SHAPE());
            SetUsageBand(buffer, current.PLTS_COMP_SCALE()!.Value);

            return instance;
        }

        private static CurrentNonGravitational CURENT(Feature current, RowBuffer buffer) {
            var instance = new CurrentNonGravitational();

            // TODO

            var featureName = GetFeatureName(current.OBJNAM(), current.NOBJNM());
            if (featureName is not null)
                instance.featureName = featureName;

            var result = ImporterNIS.AddInformation(current.GetObjectID(), current.TableName(), current.NTXTDS(), current.TXTDSC(), current.INFORM(), current.NINFOM());
            instance.information = [.. result.information];
            instance.SetInformationBindings(result.InformationBindings.ToArray());

            buffer["ps"] = ps101;
            buffer["code"] = instance.GetType().Name;
            buffer["attributebindings"] = instance.Flatten();
            buffer["informationbindings"] = System.Text.Json.JsonSerializer.Serialize(instance.GetInformationBindings(), jsonSerializerOptions);

            buffer["sourceIdentifier"] = instance.sourceIdentifier;
            SetShape(buffer, current.SHAPE());
            SetUsageBand(buffer, current.PLTS_COMP_SCALE()!.Value);

            return instance;
        }

        private static SweptArea SWPARE(Feature current, RowBuffer buffer) {
            var instance = new SweptArea();

            // TODO

            if (current.PLTS_COMP_SCALE_HasValue() && current.SHAPE != null) {
                string subtype = "";

                if (!Subtypes.Instance.TryGetSubtype(current.TableName(), current.FCSUBTYPE()!.Value, out subtype))
                    throw new NotSupportedException($"Unknown subtype for {current.TableName}, {current.FCSUBTYPE()!.Value}");

                instance.scaleMinimum = Scamin.Instance.GetMinimumScale(current, subtype, current.PLTS_COMP_SCALE()!.Value, isRelatedToStructure: false);
            }

            var result = ImporterNIS.AddInformation(current.GetObjectID(), current.TableName(), current.NTXTDS(), current.TXTDSC(), current.INFORM(), current.NINFOM());
            instance.information = [.. result.information];
            instance.SetInformationBindings(result.InformationBindings.ToArray());

            buffer["ps"] = ps101;
            buffer["code"] = instance.GetType().Name;
            buffer["attributebindings"] = instance.Flatten();
            buffer["informationbindings"] = System.Text.Json.JsonSerializer.Serialize(instance.GetInformationBindings(), jsonSerializerOptions);

            buffer["sourceIdentifier"] = instance.sourceIdentifier;
            SetShape(buffer, current.SHAPE());
            SetUsageBand(buffer, current.PLTS_COMP_SCALE()!.Value);

            return instance;
        }

        private static PipelineSubmarineOnLand PIPSOL(Feature current, RowBuffer buffer) {
            var instance = new PipelineSubmarineOnLand();

            // TODO

            if (current.PLTS_COMP_SCALE_HasValue() && current.SHAPE != null) {
                string subtype = "";

                if (!Subtypes.Instance.TryGetSubtype(current.TableName(), current.FCSUBTYPE()!.Value, out subtype))
                    throw new NotSupportedException($"Unknown subtype for {current.TableName}, {current.FCSUBTYPE()!.Value}");

                instance.scaleMinimum = Scamin.Instance.GetMinimumScale(current, subtype, current.PLTS_COMP_SCALE()!.Value, isRelatedToStructure: false);
            }

            var result = ImporterNIS.AddInformation(current.GetObjectID(), current.TableName(), current.NTXTDS(), current.TXTDSC(), current.INFORM(), current.NINFOM());
            instance.information = [.. result.information];
            instance.SetInformationBindings(result.InformationBindings.ToArray());

            buffer["ps"] = ps101;
            buffer["code"] = instance.GetType().Name;
            buffer["attributebindings"] = instance.Flatten();
            buffer["informationbindings"] = System.Text.Json.JsonSerializer.Serialize(instance.GetInformationBindings(), jsonSerializerOptions);

            buffer["sourceIdentifier"] = instance.sourceIdentifier;
            SetShape(buffer, current.SHAPE());
            SetUsageBand(buffer, current.PLTS_COMP_SCALE()!.Value);

            return instance;
        }

        private static SubmarineTransitLane SUBTLN(Feature current, RowBuffer buffer) {
            var instance = new SubmarineTransitLane();

            // TODO

            if (current.PLTS_COMP_SCALE_HasValue() && current.SHAPE != null) {
                string subtype = "";

                if (!Subtypes.Instance.TryGetSubtype(current.TableName(), current.FCSUBTYPE()!.Value, out subtype))
                    throw new NotSupportedException($"Unknown subtype for {current.TableName}, {current.FCSUBTYPE()!.Value}");

                instance.scaleMinimum = Scamin.Instance.GetMinimumScale(current, subtype, current.PLTS_COMP_SCALE()!.Value, isRelatedToStructure: false);
            }

            var result = ImporterNIS.AddInformation(current.GetObjectID(), current.TableName(), current.NTXTDS(), current.TXTDSC(), current.INFORM(), current.NINFOM());
            instance.information = [.. result.information];
            instance.SetInformationBindings(result.InformationBindings.ToArray());

            buffer["ps"] = ps101;
            buffer["code"] = instance.GetType().Name;
            buffer["attributebindings"] = instance.Flatten();
            buffer["informationbindings"] = System.Text.Json.JsonSerializer.Serialize(instance.GetInformationBindings(), jsonSerializerOptions);

            buffer["sourceIdentifier"] = instance.sourceIdentifier;
            SetShape(buffer, current.SHAPE());
            SetUsageBand(buffer, current.PLTS_COMP_SCALE()!.Value);

            return instance;
        }

        private static RadarRange RADRNG(Feature current, RowBuffer buffer) {
            var instance = new RadarRange();

            // TODO

            if (current.PLTS_COMP_SCALE_HasValue() && current.SHAPE != null) {
                string subtype = "";

                if (!Subtypes.Instance.TryGetSubtype(current.TableName(), current.FCSUBTYPE()!.Value, out subtype))
                    throw new NotSupportedException($"Unknown subtype for {current.TableName}, {current.FCSUBTYPE()!.Value}");

                instance.scaleMinimum = Scamin.Instance.GetMinimumScale(current, subtype, current.PLTS_COMP_SCALE()!.Value, isRelatedToStructure: false);
            }

            var result = ImporterNIS.AddInformation(current.GetObjectID(), current.TableName(), current.NTXTDS(), current.TXTDSC(), current.INFORM(), current.NINFOM());
            instance.information = [.. result.information];
            instance.SetInformationBindings(result.InformationBindings.ToArray());

            buffer["ps"] = ps101;
            buffer["code"] = instance.GetType().Name;
            buffer["attributebindings"] = instance.Flatten();
            buffer["informationbindings"] = System.Text.Json.JsonSerializer.Serialize(instance.GetInformationBindings(), jsonSerializerOptions);

            buffer["sourceIdentifier"] = instance.sourceIdentifier;
            SetShape(buffer, current.SHAPE());
            SetUsageBand(buffer, current.PLTS_COMP_SCALE()!.Value);

            return instance;
        }

        private static StraightTerritorialSeaBaseline STSLNE(Feature current, RowBuffer buffer) {
            var instance = new StraightTerritorialSeaBaseline();

            if (current.NATION_HasValue()) {
                instance.nationality = GetNation(current.NATION()!);
            }

            if (current.PLTS_COMP_SCALE_HasValue() && current.SHAPE != null) {
                string subtype = "";

                if (!Subtypes.Instance.TryGetSubtype(current.TableName(), current.FCSUBTYPE()!.Value, out subtype))
                    throw new NotSupportedException($"Unknown subtype for {current.TableName}, {current.FCSUBTYPE()!.Value}");

                instance.scaleMinimum = Scamin.Instance.GetMinimumScale(current, subtype, current.PLTS_COMP_SCALE()!.Value, isRelatedToStructure: false);
            }

            var result = ImporterNIS.AddInformation(current.GetObjectID(), current.TableName(), current.NTXTDS(), current.TXTDSC(), current.INFORM(), current.NINFOM());
            instance.information = [.. result.information];
            instance.SetInformationBindings(result.InformationBindings.ToArray());

            buffer["ps"] = ps101;
            buffer["code"] = instance.GetType().Name;
            buffer["attributebindings"] = instance.Flatten();
            buffer["informationbindings"] = System.Text.Json.JsonSerializer.Serialize(instance.GetInformationBindings(), jsonSerializerOptions);

            buffer["sourceIdentifier"] = instance.sourceIdentifier;
            SetShape(buffer, current.SHAPE());
            SetUsageBand(buffer, current.PLTS_COMP_SCALE()!.Value);

            return instance;
        }

        private static AnchorBerth ACHBRT(Feature current, RowBuffer buffer) {
            var instance = new AnchorBerth();

            if (current.CATACH() == "8") {
                throw new NotSupportedException("Anchorage area category 8 not implemented. Create mooring area.");
            }

            if (current.CATACH_HasValue()) {
                var categoryOfAnchorage = EnumHelper.GetEnumValues(current.CATACH());
                if (categoryOfAnchorage is not null && categoryOfAnchorage.Any())
                    instance.categoryOfAnchorage = categoryOfAnchorage;
            }

            // new S-101
            //instance.categoryOfCargo
            var featureName = GetFeatureName(current.OBJNAM(), current.NOBJNM());
            if (featureName is not null)
                instance.featureName = featureName;

            DateHelper.TryGetFixedDateRange(current.DATSTA(), current.DATEND(), out var dateRange);
            if (dateRange != default) {
                instance.fixedDateRange = dateRange;
            }

            DateHelper.TryGetPeriodicDateRange(current.PERSTA(), current.PEREND(), out var periodicDateRange);
            if (periodicDateRange != default) {
                instance.periodicDateRange = periodicDateRange;
            }

            if (current.RADIUS_HasValue()) {
                instance.radius = current.RADIUS();
            }

            if (current.STATUS_HasValue()) {
                instance.status = GetStatus(current.STATUS()!);
            }

            if (current.PLTS_COMP_SCALE_HasValue() && current.SHAPE != null) {
                string subtype = "";

                if (!Subtypes.Instance.TryGetSubtype(current.TableName(), current.FCSUBTYPE()!.Value, out subtype))
                    throw new NotSupportedException($"Unknown subtype for {current.TableName}, {current.FCSUBTYPE()!.Value}");

                instance.scaleMinimum = Scamin.Instance.GetMinimumScale(current, subtype, current.PLTS_COMP_SCALE()!.Value, isRelatedToStructure: false);
            }

            var result = ImporterNIS.AddInformation(current.GetObjectID(), current.TableName(), current.NTXTDS(), current.TXTDSC(), current.INFORM(), current.NINFOM());
            instance.information = [.. result.information];
            instance.SetInformationBindings(result.InformationBindings.ToArray());

            buffer["ps"] = ps101;
            buffer["code"] = instance.GetType().Name;
            buffer["attributebindings"] = instance.Flatten();
            buffer["informationbindings"] = System.Text.Json.JsonSerializer.Serialize(instance.GetInformationBindings(), jsonSerializerOptions);

            buffer["sourceIdentifier"] = instance.sourceIdentifier;
            SetShape(buffer, current.SHAPE());
            SetUsageBand(buffer, current.PLTS_COMP_SCALE()!.Value);

            return instance;
        }

        private static ContiguousZone CONZNE(Feature current, RowBuffer buffer) {
            var instance = new ContiguousZone();

            DateHelper.TryGetFixedDateRange(current.DATSTA(), current.DATEND(), out var dateRange);
            if (dateRange != default) {
                instance.fixedDateRange = dateRange;
            }

            // TODO: interoperabilityIdentifier

            if (current.NATION_HasValue()) {
                instance.nationality = [GetNation(current.NATION()!)];
            }

            if (current.PLTS_COMP_SCALE_HasValue() && current.SHAPE != null) {
                string subtype = "";

                if (!Subtypes.Instance.TryGetSubtype(current.TableName(), current.FCSUBTYPE()!.Value, out subtype))
                    throw new NotSupportedException($"Unknown subtype for {current.TableName}, {current.FCSUBTYPE()!.Value}");

                instance.scaleMinimum = Scamin.Instance.GetMinimumScale(current, subtype, current.PLTS_COMP_SCALE()!.Value, isRelatedToStructure: false);
            }

            var result = ImporterNIS.AddInformation(current.GetObjectID(), current.TableName(), current.NTXTDS(), current.TXTDSC(), current.INFORM(), current.NINFOM());
            instance.information = [.. result.information];
            instance.SetInformationBindings(result.InformationBindings.ToArray());

            buffer["ps"] = ps101;
            buffer["code"] = instance.GetType().Name;
            buffer["attributebindings"] = instance.Flatten();
            buffer["informationbindings"] = System.Text.Json.JsonSerializer.Serialize(instance.GetInformationBindings(), jsonSerializerOptions);

            buffer["sourceIdentifier"] = instance.sourceIdentifier;
            SetShape(buffer, current.SHAPE());
            SetUsageBand(buffer, current.PLTS_COMP_SCALE()!.Value);

            return instance;
        }

        private static ContinentalShelfArea COSARE(Feature current, RowBuffer buffer) {
            var instance = new ContinentalShelfArea {
            };
            var featureName = GetFeatureName(current.OBJNAM(), current.NOBJNM());
            if (featureName is not null)
                instance.featureName = featureName;

            if (current.NATION_HasValue()) {
                instance.nationality = [GetNation(current.NATION()!)];
            }

            if (current.PLTS_COMP_SCALE_HasValue() && current.SHAPE != null) {
                string subtype = "";

                if (!Subtypes.Instance.TryGetSubtype(current.TableName(), current.FCSUBTYPE()!.Value, out subtype))
                    throw new NotSupportedException($"Unknown subtype for {current.TableName}, {current.FCSUBTYPE()!.Value}");

                instance.scaleMinimum = Scamin.Instance.GetMinimumScale(current, subtype, current.PLTS_COMP_SCALE()!.Value, isRelatedToStructure: false);
            }

            var result = ImporterNIS.AddInformation(current.GetObjectID(), current.TableName(), current.NTXTDS(), current.TXTDSC(), current.INFORM(), current.NINFOM());
            instance.information = [.. result.information];
            instance.SetInformationBindings(result.InformationBindings.ToArray());

            buffer["ps"] = ps101;
            buffer["code"] = instance.GetType().Name;
            buffer["attributebindings"] = instance.Flatten();
            buffer["informationbindings"] = System.Text.Json.JsonSerializer.Serialize(instance.GetInformationBindings(), jsonSerializerOptions);

            buffer["sourceIdentifier"] = instance.sourceIdentifier;
            SetShape(buffer, current.SHAPE());
            SetUsageBand(buffer, current.PLTS_COMP_SCALE()!.Value);

            return instance;
        }

        private static ExclusiveEconomicZone EXEZNE(Feature current, RowBuffer buffer) {
            var instance = new ExclusiveEconomicZone();

            // TODO: inDispute

            if (current.NATION_HasValue()) {
                instance.nationality = [GetNation(current.NATION()!)];
            }

            if (current.PLTS_COMP_SCALE_HasValue() && current.SHAPE != null) {
                string subtype = "";

                if (!Subtypes.Instance.TryGetSubtype(current.TableName(), current.FCSUBTYPE()!.Value, out subtype))
                    throw new NotSupportedException($"Unknown subtype for {current.TableName}, {current.FCSUBTYPE()!.Value}");

                instance.scaleMinimum = Scamin.Instance.GetMinimumScale(current, subtype, current.PLTS_COMP_SCALE()!.Value, isRelatedToStructure: false);
            }

            var result = ImporterNIS.AddInformation(current.GetObjectID(), current.TableName(), current.NTXTDS(), current.TXTDSC(), current.INFORM(), current.NINFOM());
            instance.information = [.. result.information];
            instance.SetInformationBindings(result.InformationBindings.ToArray());

            buffer["ps"] = ps101;
            buffer["code"] = instance.GetType().Name;
            buffer["attributebindings"] = instance.Flatten();
            buffer["informationbindings"] = System.Text.Json.JsonSerializer.Serialize(instance.GetInformationBindings(), jsonSerializerOptions);

            buffer["sourceIdentifier"] = instance.sourceIdentifier;
            SetShape(buffer, current.SHAPE());
            SetUsageBand(buffer, current.PLTS_COMP_SCALE()!.Value);

            return instance;
        }

        private static FisheryZone FSHZNE(Feature current, RowBuffer buffer) {
            var instance = new FisheryZone();

            var featureName = GetFeatureName(current.OBJNAM(), current.NOBJNM());
            if (featureName is not null)
                instance.featureName = featureName;

            // TODO: interoperabilityIdentifier

            if (current.NATION_HasValue()) {
                instance.nationality = GetNation(current.NATION()!);
            }

            if (current.STATUS_HasValue()) {
                instance.status = GetStatus(current.STATUS()!);
            }

            if (current.PLTS_COMP_SCALE_HasValue() && current.SHAPE != null) {
                string subtype = "";

                if (!Subtypes.Instance.TryGetSubtype(current.TableName(), current.FCSUBTYPE()!.Value, out subtype))
                    throw new NotSupportedException($"Unknown subtype for {current.TableName}, {current.FCSUBTYPE()!.Value}");

                instance.scaleMinimum = Scamin.Instance.GetMinimumScale(current, subtype, current.PLTS_COMP_SCALE()!.Value, isRelatedToStructure: false);
            }

            var result = ImporterNIS.AddInformation(current.GetObjectID(), current.TableName(), current.NTXTDS(), current.TXTDSC(), current.INFORM(), current.NINFOM());
            instance.information = [.. result.information];
            instance.SetInformationBindings(result.InformationBindings.ToArray());

            buffer["ps"] = ps101;
            buffer["code"] = instance.GetType().Name;
            buffer["attributebindings"] = instance.Flatten();
            buffer["informationbindings"] = System.Text.Json.JsonSerializer.Serialize(instance.GetInformationBindings(), jsonSerializerOptions);

            buffer["sourceIdentifier"] = instance.sourceIdentifier;
            SetShape(buffer, current.SHAPE());
            SetUsageBand(buffer, current.PLTS_COMP_SCALE()!.Value);

            return instance;
        }

        private static HarbourAreaAdministrative HRBARE(Feature current, RowBuffer buffer) {
            var instance = new HarbourAreaAdministrative();

            var featureName = GetFeatureName(current.OBJNAM(), current.NOBJNM());
            if (featureName is not null)
                instance.featureName = featureName;

            if (current.STATUS_HasValue()) {
                instance.status = GetStatus(current.STATUS()!);
            }

            if (current.PLTS_COMP_SCALE_HasValue() && current.SHAPE != null) {
                string subtype = "";

                if (!Subtypes.Instance.TryGetSubtype(current.TableName(), current.FCSUBTYPE()!.Value, out subtype))
                    throw new NotSupportedException($"Unknown subtype for {current.TableName}, {current.FCSUBTYPE()!.Value}");

                instance.scaleMinimum = Scamin.Instance.GetMinimumScale(current, subtype, current.PLTS_COMP_SCALE()!.Value, isRelatedToStructure: false);
            }

            var result = ImporterNIS.AddInformation(current.GetObjectID(), current.TableName(), current.NTXTDS(), current.TXTDSC(), current.INFORM(), current.NINFOM());
            instance.information = [.. result.information];
            instance.SetInformationBindings(result.InformationBindings.ToArray());

            buffer["ps"] = ps101;
            buffer["code"] = instance.GetType().Name;
            buffer["attributebindings"] = instance.Flatten();
            buffer["informationbindings"] = System.Text.Json.JsonSerializer.Serialize(instance.GetInformationBindings(), jsonSerializerOptions);

            buffer["sourceIdentifier"] = instance.sourceIdentifier;
            SetShape(buffer, current.SHAPE());
            SetUsageBand(buffer, current.PLTS_COMP_SCALE()!.Value);

            return instance;
        }

        private static OffshorePlatform OFSPLF(Feature current, RowBuffer buffer) {
            var instance = new OffshorePlatform();

            if (current.CATOFP_HasValue()) {
                instance.categoryOfOffshorePlatform = EnumHelper.GetEnumValue(current.CATOFP());
            }

            if (current.COLOUR_HasValue()) {
                var colour = GetColours(current.COLOUR()!);
                if (colour is not null && colour.Any())
                    instance.colour = colour;
            }

            if (current.COLPAT_HasValue()) {
                if (instance.colour is not null && instance.colour.Length > 1)
                    instance.colourPattern = GetColourPattern(current.COLPAT()!)!.value;
            }

            if (current.CONDTN_HasValue()) {
                instance.condition = GetCondition(current.CONDTN()!.Value)!.value;
            }

            var featureName = GetFeatureName(current.OBJNAM(), current.NOBJNM());
            if (featureName is not null)
                instance.featureName = featureName;

            DateHelper.TryGetFixedDateRange(current.DATSTA()!, current.DATEND(), out var dateRange);
            if (dateRange != default) {
                instance.fixedDateRange = dateRange;
            }

            if (current.HEIGHT_HasValue()) {
                instance.height = current.HEIGHT() != -32767m ? current.HEIGHT()!.Value : null;
            }

            if (current.PRODCT_HasValue()) {
                var product = EnumHelper.GetEnumValues(current.PRODCT());
                if (product is not null && product.Any())
                    instance.product = product;
            }

            if (current.CONRAD_HasValue()) {
                instance.radarConspicuous = current.CONRAD() == -32767 ? null : current.CONRAD()!.Value == 2 ? false : true;
            }
            if (current.SORDAT_HasValue())) {
                if (DateHelper.TryConvertSordat(current.SORDAT()!, out var reportedDate)) {
                    instance.reportedDate = reportedDate;
                }
                else {
                    Logger.Current.DataError(current.GetObjectID(), current.GetType().Name, current.LNAM() ?? "Unknown LNAM", $"Cannot convert date {current.SORDAT}");
                }
            }

            if (current.STATUS_HasValue()) {
                instance.status = GetStatus(current.STATUS()!);
            }

            if (current.VERLEN_HasValue()) {
                instance.verticalLength = current.VERLEN() != -32767m ? current.VERLEN()!.Value : null;
            }

            if (current.CONVIS_HasValue()) {
                instance.visualProminence = EnumHelper.GetEnumValue(current.CONVIS());
            }

            if (current.PLTS_COMP_SCALE_HasValue() && current.SHAPE != null) {
                string subtype = "";

                if (!Subtypes.Instance.TryGetSubtype(current.TableName(), current.FCSUBTYPE()!.Value, out subtype))
                    throw new NotSupportedException($"Unknown subtype for {current.TableName}, {current.FCSUBTYPE()!.Value}");

                instance.scaleMinimum = Scamin.Instance.GetMinimumScale(current, subtype, current.PLTS_COMP_SCALE()!.Value, isRelatedToStructure: false);
            }

            var result = ImporterNIS.AddInformation(current.GetObjectID(), current.TableName(), current.NTXTDS(), current.TXTDSC(), current.INFORM(), current.NINFOM());
            instance.information = [.. result.information];
            instance.SetInformationBindings(result.InformationBindings.ToArray());
            if (current.PICREP_HasValue()) {
                instance.pictorialRepresentation = FixFilename(current.PICREP()!);
            }

            buffer["ps"] = ps101;
            buffer["code"] = instance.GetType().Name;
            buffer["attributebindings"] = instance.Flatten();
            buffer["informationbindings"] = System.Text.Json.JsonSerializer.Serialize(instance.GetInformationBindings(), jsonSerializerOptions);

            buffer["sourceIdentifier"] = instance.sourceIdentifier;
            SetShape(buffer, current.SHAPE());
            SetUsageBand(buffer, current.PLTS_COMP_SCALE()!.Value);

            return instance;
        }

        private static SeabedArea SBDARE(Feature current, RowBuffer buffer) {
            var instance = new SeabedArea();

            var featureName = GetFeatureName(current.OBJNAM(), current.NOBJNM());
            if (featureName is not null)
                instance.featureName = featureName;

            // TODO: Verify this against action point 48

            surfaceCharacteristics[] surfaceCharacteristics = [];

            var natsur = string.IsNullOrEmpty(current.NATSUR()?.Trim()) ? [] : current.NATSUR()?.Split(",", StringSplitOptions.RemoveEmptyEntries);
            var natqua = string.IsNullOrEmpty(current.NATQUA()?.Trim()) ? [] : current.NATQUA()?.Split(",");

            int[] _ = [natsur!.Length, natqua!.Length];

            for (var i = 0; i < _.Max(); i++) {
                var s = new surfaceCharacteristics();

                if (i < natsur.Length)
                    s.natureOfSurface = EnumHelper.GetEnumValue(natsur[i]);
                if (i < natqua.Length) {
                    if (!string.IsNullOrEmpty(natqua[i]))
                        s.natureOfSurfaceQualifyingTerms = [EnumHelper.GetEnumValue(natqua[i])];
                }

                surfaceCharacteristics = [.. surfaceCharacteristics, s];
            }

            instance.surfaceCharacteristics = surfaceCharacteristics;

            if (current.WATLEV_HasValue()) {
                instance.waterLevelEffect = EnumHelper.GetEnumValue(current.WATLEV());
            }

            if (current.PLTS_COMP_SCALE_HasValue() && current.SHAPE != null) {
                string subtype = "";

                if (!Subtypes.Instance.TryGetSubtype(current.TableName(), current.FCSUBTYPE()!.Value, out subtype))
                    throw new NotSupportedException($"Unknown subtype for {current.TableName}, {current.FCSUBTYPE()!.Value}");

                instance.scaleMinimum = Scamin.Instance.GetMinimumScale(current, subtype, current.PLTS_COMP_SCALE()!.Value, isRelatedToStructure: false);
            }

            var result = ImporterNIS.AddInformation(current.GetObjectID(), current.TableName(), current.NTXTDS(), current.TXTDSC(), current.INFORM(), current.NINFOM());
            instance.information = [.. result.information];
            instance.SetInformationBindings(result.InformationBindings.ToArray());

            buffer["ps"] = ps101;
            buffer["code"] = instance.GetType().Name;
            buffer["attributebindings"] = instance.Flatten();
            buffer["informationbindings"] = System.Text.Json.JsonSerializer.Serialize(instance.GetInformationBindings(), jsonSerializerOptions);

            buffer["sourceIdentifier"] = instance.sourceIdentifier;
            SetShape(buffer, current.SHAPE());
            SetUsageBand(buffer, current.PLTS_COMP_SCALE()!.Value);

            return instance;
        }

        private static Sandwave SNDWAV(Feature current, RowBuffer buffer) {
            var instance = new Sandwave();

            if (current.VERLEN_HasValue()) {
                instance.verticalLength = current.VERLEN() != -32767m ? current.VERLEN() : null;
            }

            if (current.PLTS_COMP_SCALE_HasValue() && current.SHAPE != null) {
                string subtype = "";

                if (!Subtypes.Instance.TryGetSubtype(current.TableName(), current.FCSUBTYPE()!.Value, out subtype))
                    throw new NotSupportedException($"Unknown subtype for {current.TableName}, {current.FCSUBTYPE()!.Value}");

                instance.scaleMinimum = Scamin.Instance.GetMinimumScale(current, subtype, current.PLTS_COMP_SCALE()!.Value, isRelatedToStructure: false);
            }

            var result = ImporterNIS.AddInformation(current.GetObjectID(), current.TableName(), current.NTXTDS(), current.TXTDSC(), current.INFORM(), current.NINFOM());
            instance.information = [.. result.information];
            instance.SetInformationBindings(result.InformationBindings.ToArray());

            buffer["ps"] = ps101;
            buffer["code"] = instance.GetType().Name;
            buffer["attributebindings"] = instance.Flatten();
            buffer["informationbindings"] = System.Text.Json.JsonSerializer.Serialize(instance.GetInformationBindings(), jsonSerializerOptions);

            buffer["sourceIdentifier"] = instance.sourceIdentifier;
            SetShape(buffer, current.SHAPE());
            SetUsageBand(buffer, current.PLTS_COMP_SCALE()!.Value);

            return instance;
        }

        private static FeatureType WEDKLP(Feature current, RowBuffer buffer) {
            if (current.CATWED_HasValue() && current.CATWED() == 3) {
                var instance = new Seagrass();

                var featureName = GetFeatureName(current.OBJNAM(), current.NOBJNM());
                if (featureName is not null)
                    instance.featureName = featureName;

                if (current.PLTS_COMP_SCALE_HasValue() && current.SHAPE != null) {
                    string subtype = "";

                    if (!Subtypes.Instance.TryGetSubtype(current.TableName(), current.FCSUBTYPE()!.Value, out subtype))
                        throw new NotSupportedException($"Unknown subtype for {current.TableName}, {current.FCSUBTYPE()!.Value}");

                    instance.scaleMinimum = Scamin.Instance.GetMinimumScale(current, subtype, current.PLTS_COMP_SCALE()!.Value, isRelatedToStructure: false);
                }

                var result = ImporterNIS.AddInformation(current.GetObjectID(), current.TableName(), current.NTXTDS(), current.TXTDSC(), current.INFORM(), current.NINFOM());
                instance.information = [.. result.information];
                instance.SetInformationBindings(result.InformationBindings.ToArray());

                buffer["ps"] = ps101;
                buffer["code"] = instance.GetType().Name;
                buffer["attributebindings"] = instance.Flatten();
                buffer["informationbindings"] = System.Text.Json.JsonSerializer.Serialize(instance.GetInformationBindings(), jsonSerializerOptions);

                buffer["sourceIdentifier"] = instance.sourceIdentifier;
                SetShape(buffer, current.SHAPE());
                SetUsageBand(buffer, current.PLTS_COMP_SCALE()!.Value);

                return instance;
            }
            else {
                var instance = new WeedKelp();

                if (current.CATWED_HasValue()) {
                    instance.categoryOfWeedKelp = EnumHelper.GetEnumValue(current.CATWED());
                }

                var featureName = GetFeatureName(current.OBJNAM(), current.NOBJNM());
                if (featureName is not null)
                    instance.featureName = featureName;

                if (current.PLTS_COMP_SCALE_HasValue() && current.SHAPE != null) {
                    string subtype = "";

                    if (!Subtypes.Instance.TryGetSubtype(current.TableName(), current.FCSUBTYPE()!.Value, out subtype))
                        throw new NotSupportedException($"Unknown subtype for {current.TableName}, {current.FCSUBTYPE()!.Value}");

                    instance.scaleMinimum = Scamin.Instance.GetMinimumScale(current, subtype, current.PLTS_COMP_SCALE()!.Value, isRelatedToStructure: false);
                }

                var result = ImporterNIS.AddInformation(current.GetObjectID(), current.TableName(), current.NTXTDS(), current.TXTDSC(), current.INFORM(), current.NINFOM());
                instance.information = [.. result.information];
                instance.SetInformationBindings(result.InformationBindings.ToArray());

                buffer["ps"] = ps101;
                buffer["code"] = instance.GetType().Name;
                buffer["attributebindings"] = instance.Flatten();
                buffer["informationbindings"] = System.Text.Json.JsonSerializer.Serialize(instance.GetInformationBindings(), jsonSerializerOptions);

                buffer["sourceIdentifier"] = instance.sourceIdentifier;
                SetShape(buffer, current.SHAPE());
                SetUsageBand(buffer, current.PLTS_COMP_SCALE()!.Value);

                return instance;
            }
        }

        private static FeatureType SOUNDG(Feature current, RowBuffer buffer) {
            var shape = (MapPoint)current.SHAPE()!;

            //SetShape(buffer, MultipointBuilderEx.CreateMultipoint(mappoint));

            var depth = current.DEPTH()!.Value;
            var quasou = current.QUASOU() ?? default;
            var quapos = current.P_QUAPOS() ?? default;
            var tecsou = current.TECSOU() ?? default;

            var mappoint = MapPointBuilderEx.CreateMapPoint(shape.X, shape.Y, Convert.ToDouble(depth), shape.SpatialReference);

            if (quasou == default || string.IsNullOrEmpty(quasou) || !string.Equals(quasou, "5", StringComparison.OrdinalIgnoreCase)) {
                var instance = new Sounding();

                if (current.QUASOU_HasValue()) {
                    instance.qualityOfVerticalMeasurement = EnumHelper.GetEnumValues(current.QUASOU()!)!;
                }

                if (current.TECSOU_HasValue()) {
                    instance.techniqueOfVerticalMeasurement = EnumHelper.GetEnumValues(current.TECSOU()!)!;
                }

                if (current.QUASOU_HasValue()) {
                    instance.qualityOfVerticalMeasurement = EnumHelper.GetEnumValues(current.QUASOU()!)!;
                }

                var featureName = GetFeatureName(current.OBJNAM(), current.NOBJNM());
                if (featureName is not null)
                    instance.featureName = featureName;

                if (current.SORDAT_HasValue()) {
                    if (DateHelper.TryConvertSordat(current.SORDAT()!, out var reportedDate)) {
                        instance.reportedDate = reportedDate;
                    }
                    else {
                        Logger.Current.DataError(current.GetObjectID(), current.GetType().Name, current.LNAM() ?? "Unknown LNAM", $"Cannot convert date {current.SORDAT()}");
                    }
                }

                if (current.STATUS_HasValue()) {
                    instance.status = ImporterNIS.GetSingleStatus(current.STATUS()!).value;
                }

                if (current.TECSOU_HasValue()) {
                    var techniqueOfVerticalMeasurement = EnumHelper.GetEnumValues(current.TECSOU());
                    if (techniqueOfVerticalMeasurement is not null && techniqueOfVerticalMeasurement.Any())
                        instance.techniqueOfVerticalMeasurement = techniqueOfVerticalMeasurement;
                }

                if (current.PLTS_COMP_SCALE_HasValue() && current.SHAPE != null) {
                    string subtype = "";

                    if (!Subtypes.Instance.TryGetSubtype(current.TableName(), current.FCSUBTYPE()!.Value, out subtype))
                        throw new NotSupportedException($"Unknown subtype for {current.TableName}, {current.FCSUBTYPE()!.Value}");

                    instance.scaleMinimum = Scamin.Instance.GetMinimumScale(current, subtype, current.PLTS_COMP_SCALE()!.Value, isRelatedToStructure: false);
                }

                var result = ImporterNIS.AddInformation(current.GetObjectID(), current.TableName(), current.NTXTDS(), current.TXTDSC(), current.INFORM(), current.NINFOM());
                instance.information = [.. result.information];
                instance.SetInformationBindings(result.InformationBindings.ToArray());

                buffer["ps"] = ps101;
                buffer["code"] = instance.GetType().Name;
                buffer["attributebindings"] = instance.Flatten();
                buffer["informationbindings"] = System.Text.Json.JsonSerializer.Serialize(instance.GetInformationBindings(), jsonSerializerOptions);

                buffer["sourceIdentifier"] = instance.sourceIdentifier;
                SetShape(buffer, mappoint);
                SetUsageBand(buffer, current.PLTS_COMP_SCALE()!.Value);

                // TODO: Handle Spatialquality
                //if (quapos != default && quapos == 4) {
                //    /*  SOUNDG with attribute QUAPOS = 4 (approximate) will also be converted to an instance of the S101 Information _s101type Spatial Quality (see S-101 DCEG clause 24.5), attribute quality of horizontal
                //        measurement = 4 (approximate), associated to the geometry of the Sounding feature using the
                //        association Spatial Association. */
                //    using var information = informationtype.CreateRowBuffer();

                //    var row = new SpatialQuality {
                //        qualityOfHorizontalMeasurement = qualityOfHorizontalMeasurement.Approximate,
                //    };

                //    information["ps"] = ps101;
                //    information["code"] = row.GetType().Name;
                //    information["json"] = System.Text.Json.JsonSerializer.Serialize(row);
                //    using var _ = informationtype.CreateRow(information);
                //}

                return instance;
            }
            else {
                /*  SOUNDG with attribute QUASOU = 5 (no bottom found at value shown) will be converted to an
                    instance of the S-101 Feature _s101type Depth – No Bottom Found. Where this is the case, the attributes
                    EXPSOU, NOBJNM, OBJNAM, SOUACC and STATUS will not be converted. It is considered that
                    these attributes are not relevant for Depth – No Bottom Found in S-101. */
                var instance = new DepthNoBottomFound();

                // TODO: interoperabilityIdentifier

                if (current.TECSOU_HasValue()) {
                    instance.techniqueOfVerticalMeasurement = [.. current.TECSOU()!.Split(',').Select(e => EnumHelper.GetEnumValue(e))];
                }

                if (current.PLTS_COMP_SCALE_HasValue() && current.SHAPE != null) {
                    string subtype = "";

                    if (!Subtypes.Instance.TryGetSubtype(current.TableName(), current.FCSUBTYPE()!.Value, out subtype))
                        throw new NotSupportedException($"Unknown subtype for {current.TableName}, {current.FCSUBTYPE()!.Value}");

                    instance.scaleMinimum = Scamin.Instance.GetMinimumScale(current, subtype, current.PLTS_COMP_SCALE()!.Value, isRelatedToStructure: false);
                }

                var result = ImporterNIS.AddInformation(current.GetObjectID(), current.TableName(), current.NTXTDS(), current.TXTDSC(), current.INFORM(), current.NINFOM());
                instance.information = [.. result.information];
                instance.SetInformationBindings(result.InformationBindings.ToArray());

                buffer["ps"] = ps101;
                buffer["code"] = instance.GetType().Name;
                buffer["attributebindings"] = instance.Flatten();
                buffer["informationbindings"] = System.Text.Json.JsonSerializer.Serialize(instance.GetInformationBindings(), jsonSerializerOptions);

                buffer["sourceIdentifier"] = instance.sourceIdentifier;
                SetShape(buffer, mappoint);
                SetUsageBand(buffer, current.PLTS_COMP_SCALE()!.Value);
                return instance;
            }
        }

        private static DistanceMark DISMAR(Feature current, RowBuffer buffer) {
            var instance = new DistanceMark();

            /*
                The S-57 attribute CATDIS has been replaced in S-101 by the mandatory Boolean type attribute
                distance mark visible. Where CATDIS has not been populated, or has been populated with value
                1 (distance mark not physically installed) or an empty (null) value, distance mark visible will be set
                to False. Where CATDIS has been populated with a value other than 1, distance mark visible will
                be set to True.                             
            */
            if (!current.CATDIS_HasValue() || (current.CATDIS() == 1)) {
                instance.distanceMarkVisible = false;
            }
            else if (current.CATDIS_HasValue()) {
                instance.distanceMarkVisible = true;
            }

            var featureName = GetFeatureName(current.OBJNAM(), current.NOBJNM());
            if (featureName is not null)
                instance.featureName = featureName;

            DateHelper.TryGetFixedDateRange(current.DATSTA(), current.DATEND(), out var dateRange);
            if (dateRange is not null) {
                instance.fixedDateRange = dateRange;
            }

            var inform = current.INFORM();
            if (!string.IsNullOrEmpty(inform) && regexWaterwayDistance.IsMatch(inform)) {
                var _value = regexWaterwayDistance.Match(inform).Groups["value"]?.Value;
                var _unit = regexWaterwayDistance.Match(inform).Groups["unit"]?.Value.ToLowerInvariant();

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

            if (current.PLTS_COMP_SCALE_HasValue() && current.SHAPE() != null) {
                string subtype = "";
                if (current.TableName() != default && !Subtypes.Instance.TryGetSubtype(current.TableName(), current.FCSubtype()!.Value, out subtype))
                    throw new NotSupportedException($"Unknown subtype for {current.TableName()}, {current.FCSubtype}");
                var scaleMinimum = Scamin.Instance.GetMinimumScale(current, subtype, current.PLTS_COMP_SCALE()!.Value, isRelatedToStructure: false);
                if (scaleMinimum.HasValue)
                    instance.scaleMinimum = scaleMinimum;
            }
            var result = ImporterNIS.AddInformation(current.GetObjectID(), current.TableName(), current.NTXTDS(), current.TXTDSC(), current.INFORM(), current.NINFOM());
            instance.information = [.. result.information];
            instance.SetInformationBindings(result.InformationBindings.ToArray());

            buffer["ps"] = ps101;
            buffer["code"] = instance.GetType().Name;
            buffer["attributebindings"] = instance.Flatten();
            buffer["informationbindings"] = System.Text.Json.JsonSerializer.Serialize(instance.GetInformationBindings(), jsonSerializerOptions);

            buffer["sourceIdentifier"] = instance.sourceIdentifier;
            SetShape(buffer, current.SHAPE());
            SetUsageBand(buffer, current.PLTS_COMP_SCALE()!.Value);

            return instance;
        }

        private static Berth BERTHS(Feature current, RowBuffer buffer) {
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

            var featureName = GetFeatureName(current.OBJNAM(), current.NOBJNM());
            if (featureName is not null)
                instance.featureName = featureName;

            DateHelper.TryGetFixedDateRange(current.DATSTA(), current.DATEND(), out var dateRange);
            if (dateRange is not null) {
                instance.fixedDateRange = dateRange;
            }

            var inform = current.INFORM();
            if (!string.IsNullOrEmpty(inform) && regexMaximumDraughtPermitted.IsMatch(inform)) {
                var _value = regexMaximumDraughtPermitted.Match(inform).Groups["value"]?.Value;

                if (decimal.TryParse(_value, out decimal value)) {
                    instance.maximumPermittedDraught = value;
                }
            }

            if (current.DRVAL1_HasValue()) {
                instance.minimumBerthDepth = current.DRVAL1() == -32767m ? null : current.DRVAL1();
            }

            DateHelper.TryGetPeriodicDateRange(current.PERSTA(), current.PEREND(), out var periodicDateRange);
            if (periodicDateRange is not null) {
                instance.periodicDateRange = periodicDateRange;
            }

            if (current.QUASOU_HasValue()) {
                var qualityOfVerticalMeasurement = EnumHelper.GetEnumValues(current.QUASOU());
                if (qualityOfVerticalMeasurement is not null && qualityOfVerticalMeasurement.Any())
                    instance.qualityOfVerticalMeasurement = qualityOfVerticalMeasurement;
            }

            if (current.STATUS_HasValue()) {
                instance.status = GetStatus(current.STATUS());
            }

            if (current.SOUACC_HasValue()) {
                instance.verticalUncertainty = new() {
                    uncertaintyFixed = current.SOUACC()
                };
            }

            if (current.PLTS_COMP_SCALE_HasValue() && current.SHAPE() != null) {
                string subtype = "";
                if (current.TableName() != default && current.FCSUBTYPE_HasValue() && !Subtypes.Instance.TryGetSubtype(current.TableName(), current.FCSUBTYPE()!.Value, out subtype))
                    throw new NotSupportedException($"Unknown subtype for {current.TableName()}, {current.FCSUBTYPE()}");
                var scaleMinimum = Scamin.Instance.GetMinimumScale(current, subtype, current.PLTS_COMP_SCALE()!.Value, isRelatedToStructure: false);
                if (scaleMinimum.HasValue)
                    instance.scaleMinimum = scaleMinimum;
            }

            var result = ImporterNIS.AddInformation(current.GetObjectID(), current.TableName(), current.NTXTDS(), current.TXTDSC(), current.INFORM(), current.NINFOM());
            instance.information = [.. result.information];
            instance.SetInformationBindings(result.InformationBindings.ToArray());

            buffer["ps"] = ps101;
            buffer["code"] = instance.GetType().Name;
            buffer["attributebindings"] = instance.Flatten();
            buffer["informationbindings"] = System.Text.Json.JsonSerializer.Serialize(instance.GetInformationBindings(), jsonSerializerOptions);

            buffer["sourceIdentifier"] = instance.sourceIdentifier;
            SetShape(buffer, current.SHAPE());
            SetUsageBand(buffer, current.PLTS_COMP_SCALE()!.Value);

            return instance;
        }

        private static NavigationLine NAVLNE(Feature current, RowBuffer buffer) {
            var instance = new NavigationLine();

            if (current.CATNAV_HasValue()) {
                instance.categoryOfNavigationLine = EnumHelper.GetEnumValue(current.CATNAV());
            }

            DateHelper.TryGetFixedDateRange(current.DATSTA(), current.DATEND(), out var dateRange);
            if (dateRange is not null) {
                instance.fixedDateRange = dateRange;
            }

            // TODO: measured distance

            if (current.ORIENT_HasValue()) {
                instance.orientation = new S100FC.S101.ComplexAttributes.orientation() {
                    orientationValue = current.ORIENT() == -32767m ? null : current.ORIENT(),
                    // TODO: oriantationUncertainty
                    //orientationUncertainty = ,
                };
            }

            DateHelper.TryGetPeriodicDateRange(current.PERSTA(), current.PEREND(), out var periodicDateRange);
            if (periodicDateRange is not null) {
                instance.periodicDateRange = periodicDateRange;
            }

            if (current.STATUS_HasValue()) {
                instance.status = GetStatus(current.STATUS());
            }

            if (current.PLTS_COMP_SCALE_HasValue() && current.SHAPE() != null) {
                string subtype = "";
                if (current.TableName() != default && current.FCSUBTYPE_HasValue() && !Subtypes.Instance.TryGetSubtype(current.TableName(), current.FCSUBTYPE()!.Value, out subtype))
                    throw new NotSupportedException($"Unknown subtype for {current.TableName()}, {current.FCSUBTYPE()}");
                var scaleMinimum = Scamin.Instance.GetMinimumScale(current, subtype, current.PLTS_COMP_SCALE()!.Value, isRelatedToStructure: false);
                if (scaleMinimum.HasValue)
                    instance.scaleMinimum = scaleMinimum;
            }

            var result = ImporterNIS.AddInformation(current.GetObjectID(), current.TableName(), current.NTXTDS(), current.TXTDSC(), current.INFORM(), current.NINFOM());
            instance.information = [.. result.information];
            instance.SetInformationBindings(result.InformationBindings.ToArray());

            buffer["ps"] = ps101;
            buffer["code"] = instance.GetType().Name;
            buffer["attributebindings"] = instance.Flatten();
            buffer["informationbindings"] = System.Text.Json.JsonSerializer.Serialize(instance.GetInformationBindings(), jsonSerializerOptions);

            buffer["sourceIdentifier"] = instance.sourceIdentifier;
            SetShape(buffer, current.SHAPE());
            SetUsageBand(buffer, current.PLTS_COMP_SCALE()!.Value);

            return instance;
        }

        private static FeatureType ADMARE(Feature current, RowBuffer buffer) {
            if (!string.IsNullOrEmpty(current.INFORM()) && regexPilotageDistrict.IsMatch(current.INFORM()!)) {
                var instance = new PilotageDistrict {
                };

                var featureName = GetFeatureName(current.OBJNAM(), current.NOBJNM());
                if (featureName is not null)
                    instance.featureName = featureName;

                if (current.PLTS_COMP_SCALE_HasValue() && current.SHAPE() != null) {
                    string subtype = "";
                    if (current.TableName() != default && current.FCSUBTYPE_HasValue() && !Subtypes.Instance.TryGetSubtype(current.TableName(), current.FCSUBTYPE()!.Value, out subtype))
                        throw new NotSupportedException($"Unknown subtype for {current.TableName()}, {current.FCSUBTYPE()}");
                    var scaleMinimum = Scamin.Instance.GetMinimumScale(current, subtype, current.PLTS_COMP_SCALE()!.Value, isRelatedToStructure: false);
                    if (scaleMinimum.HasValue)
                        instance.scaleMinimum = scaleMinimum;
                }

                var result = ImporterNIS.AddInformation(current.GetObjectID(), current.TableName(), current.NTXTDS(), current.TXTDSC(), current.INFORM(), current.NINFOM());
                instance.information = [.. result.information];
                instance.SetInformationBindings(result.InformationBindings.ToArray());

                buffer["ps"] = ps101;
                buffer["code"] = instance.GetType().Name;
                buffer["attributebindings"] = instance.Flatten();
                buffer["informationbindings"] = System.Text.Json.JsonSerializer.Serialize(instance.GetInformationBindings(), jsonSerializerOptions);

                buffer["sourceIdentifier"] = instance.sourceIdentifier;
                SetShape(buffer, current.SHAPE());
                SetUsageBand(buffer, current.PLTS_COMP_SCALE()!.Value);
                return instance;
            }
            else if (!string.IsNullOrEmpty(current.INFORM()) && regexMarinePollutionRegulationsArea.IsMatch(current.INFORM()!)) {
                var instance = new MarinePollutionRegulationsArea();

                var featureName = GetFeatureName(current.OBJNAM(), current.NOBJNM());
                if (featureName is not null)
                    instance.featureName = featureName;

                buffer["ps"] = ps101;
                buffer["code"] = instance.GetType().Name;
                buffer["attributebindings"] = instance.Flatten();
                buffer["informationbindings"] = System.Text.Json.JsonSerializer.Serialize(instance.GetInformationBindings(), jsonSerializerOptions);

                buffer["sourceIdentifier"] = instance.sourceIdentifier;
                SetShape(buffer, current.SHAPE());
                SetUsageBand(buffer, current.PLTS_COMP_SCALE()!.Value);
                return instance;
            }
            else if (!string.IsNullOrEmpty(current.INFORM()) && regexVesselTrafficServiceArea.IsMatch(current.INFORM()!)) {
                var instance = new VesselTrafficServiceArea();

                var featureName = GetFeatureName(current.OBJNAM(), current.NOBJNM());
                if (featureName is not null)
                    instance.featureName = featureName;

                if (current.PLTS_COMP_SCALE_HasValue() && current.SHAPE() != null) {
                    string subtype = "";
                    if (!Subtypes.Instance.TryGetSubtype(current.TableName(), current.FCSUBTYPE()!.Value, out subtype))
                        throw new NotSupportedException($"Unknown subtype for {current.TableName()}, {current.FCSUBTYPE()}");
                    var scaleMinimum = Scamin.Instance.GetMinimumScale(current, subtype, current.PLTS_COMP_SCALE()!.Value, isRelatedToStructure: false);
                    if (scaleMinimum.HasValue)
                        instance.scaleMinimum = scaleMinimum;
                }

                var result = ImporterNIS.AddInformation(current.GetObjectID(), current.TableName(), current.NTXTDS(), current.TXTDSC(), current.INFORM(), current.NINFOM());
                instance.information = [.. result.information];
                instance.SetInformationBindings(result.InformationBindings.ToArray());

                buffer["ps"] = ps101;
                buffer["code"] = instance.GetType().Name;
                buffer["attributebindings"] = instance.Flatten();
                buffer["informationbindings"] = System.Text.Json.JsonSerializer.Serialize(instance.GetInformationBindings(), jsonSerializerOptions);

                buffer["sourceIdentifier"] = instance.sourceIdentifier;
                SetShape(buffer, current.SHAPE());
                SetUsageBand(buffer, current.PLTS_COMP_SCALE()!.Value);
                return instance;
            }
            else {
                var instance = new AdministrationArea {
                };

                if (current.JRSDTN_HasValue()) {
                    instance.jurisdiction = EnumHelper.GetEnumValue(current.JRSDTN());
                }

                var featureName = GetFeatureName(current.OBJNAM(), current.NOBJNM());
                if (featureName is not null)
                    instance.featureName = featureName;

                if (current.NATION_HasValue()) {
                    instance.nationality = [GetNation(current.NATION())];
                }

                if (current.PLTS_COMP_SCALE_HasValue() && current.SHAPE() != null) {
                    string subtype = "";
                    if (!Subtypes.Instance.TryGetSubtype(current.TableName(), current.FCSUBTYPE()!.Value, out subtype))
                        throw new NotSupportedException($"Unknown subtype for {current.TableName()}, {current.FCSUBTYPE()}");
                    var scaleMinimum = Scamin.Instance.GetMinimumScale(current, subtype, current.PLTS_COMP_SCALE()!.Value, isRelatedToStructure: false);
                    if (scaleMinimum.HasValue)
                        instance.scaleMinimum = scaleMinimum;
                }

                var result = ImporterNIS.AddInformation(current.GetObjectID(), current.TableName(), current.NTXTDS(), current.TXTDSC(), current.INFORM(), current.NINFOM());
                instance.information = [.. result.information];
                instance.SetInformationBindings(result.InformationBindings.ToArray());

                if (current.PICREP_HasValue()) {
                    instance.pictorialRepresentation = FixFilename(current.PICREP());
                }

                buffer["ps"] = ps101;
                buffer["code"] = instance.GetType().Name;
                buffer["attributebindings"] = instance.Flatten();
                buffer["informationbindings"] = System.Text.Json.JsonSerializer.Serialize(instance.GetInformationBindings(), jsonSerializerOptions);

                buffer["sourceIdentifier"] = instance.sourceIdentifier;
                SetShape(buffer, current.SHAPE());
                SetUsageBand(buffer, current.PLTS_COMP_SCALE()!.Value);
                return instance;
            }
        }

        private static RestrictedArea RESARE(Feature current, RowBuffer buffer) {
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
                instance.status = GetStatus(current.STATUS());
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
                    Logger.Current.Debug($"VesselSpeedLimit {current.TableName()}::{current.OBJECTID} {current.INFORM()}");
            }

            if (current.PLTS_COMP_SCALE_HasValue() && current.SHAPE() != null) {
                string subtype = "";

                if (!Subtypes.Instance.TryGetSubtype(current.TableName(), current.FCSUBTYPE()!.Value, out subtype))
                    throw new NotSupportedException($"Unknown subtype for {current.TableName()}, {current.FCSUBTYPE()}");

                var scaleMinimum = Scamin.Instance.GetMinimumScale(current, subtype, current.PLTS_COMP_SCALE()!.Value, isRelatedToStructure: false);
                if (scaleMinimum.HasValue)
                    instance.scaleMinimum = scaleMinimum;
            }

            var result = ImporterNIS.AddInformation(current.GetObjectID(), current.TableName(), current.NTXTDS(), current.TXTDSC(), current.INFORM(), current.NINFOM());
            instance.information = [.. result.information];
            instance.SetInformationBindings(result.InformationBindings.ToArray());

            buffer["ps"] = ps101;
            buffer["code"] = instance.GetType().Name;
            buffer["attributebindings"] = instance.Flatten();
            buffer["informationbindings"] = System.Text.Json.JsonSerializer.Serialize(instance.GetInformationBindings(), jsonSerializerOptions);

            buffer["sourceIdentifier"] = instance.sourceIdentifier;
            SetShape(buffer, current.SHAPE());
            SetUsageBand(buffer, current.PLTS_COMP_SCALE()!.Value);

            return instance;

        }

        private static FeatureType ACHARE(Feature current, RowBuffer buffer) {
            if ("8".Equals(current.CATACH())) {
                //throw new NotSupportedException("Anchorage area category 8 not implemented. Create mooring area.");

                var instance = new MooringArea();

                var featureName = GetFeatureName(current.OBJNAM(), current.NOBJNM());
                if (featureName is not null)
                    instance.featureName = featureName;

                DateHelper.TryGetFixedDateRange(current.DATSTA(), current.DATEND(), out var dateRange);
                if (dateRange is not null) {
                    instance.fixedDateRange = dateRange;
                }

                if (current.RESTRN_HasValue()) {
                    var restriction = EnumHelper.GetEnumValues(current.RESTRN());
                    if (restriction is not null && restriction.Any())
                        instance.restriction = restriction;
                }

                if (current.STATUS_HasValue()) {
                    instance.status = GetStatus(current.STATUS());
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
                        Logger.Current.Debug($"VesselSpeedLimit {current.TableName()}::{current.OBJECTID} {current.INFORM()}");
                }

                if (current.PLTS_COMP_SCALE_HasValue() && current.SHAPE() != null) {
                    string subtype = "";
                    if (!Subtypes.Instance.TryGetSubtype(current.TableName(), current.FCSUBTYPE()!.Value, out subtype))
                        throw new NotSupportedException($"Unknown subtype for {current.TableName()}, {current.FCSUBTYPE()}");
                    var scamin = Scamin.Instance.GetMinimumScale(current, subtype, current.PLTS_COMP_SCALE()!.Value, isRelatedToStructure: false);
                    if (scamin.HasValue) {
                        instance.scaleMinimum = scamin!.Value;
                    }
                }

                var result = ImporterNIS.AddInformation(current.GetObjectID(), current.TableName(), current.NTXTDS(), current.TXTDSC(), current.INFORM(), current.NINFOM());
                instance.information = [.. result.information];
                instance.SetInformationBindings(result.InformationBindings.ToArray());

                buffer["ps"] = ps101;
                buffer["code"] = instance.GetType().Name;
                buffer["attributebindings"] = instance.Flatten();
                buffer["informationbindings"] = System.Text.Json.JsonSerializer.Serialize(instance.GetInformationBindings(), jsonSerializerOptions);

                buffer["sourceIdentifier"] = instance.sourceIdentifier;
                SetShape(buffer, current.SHAPE());
                SetUsageBand(buffer, current.PLTS_COMP_SCALE()!.Value);
                return instance;
            }
            else {
                var instance = new AnchorageArea();

                if (current.CATACH_HasValue()) {
                    var categoryOfAnchorage = EnumHelper.GetEnumValues(current.CATACH());
                    if (categoryOfAnchorage is not null && categoryOfAnchorage.Any())
                        instance.categoryOfAnchorage = categoryOfAnchorage;
                }

                // new S-101
                //instance.categoryOfCargo
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
                    instance.status = GetStatus(current.STATUS());
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
                        Logger.Current.Debug($"VesselSpeedLimit {current.TableName()}::{current.OBJECTID} {current.INFORM()}");
                }

                if (current.PLTS_COMP_SCALE_HasValue() && current.SHAPE() != null) {
                    string subtype = "";
                    if (!Subtypes.Instance.TryGetSubtype(current.TableName(), current.FCSUBTYPE()!.Value, out subtype))
                        throw new NotSupportedException($"Unknown subtype for {current.TableName()}, {current.FCSUBTYPE()}");
                    var scamin = Scamin.Instance.GetMinimumScale(current, subtype, current.PLTS_COMP_SCALE()!.Value, isRelatedToStructure: false);
                    if (scamin.HasValue)
                        instance.scaleMinimum = scamin!.Value;
                }

                var result = ImporterNIS.AddInformation(current.GetObjectID(), current.TableName(), current.NTXTDS(), current.TXTDSC(), current.INFORM(), current.NINFOM());
                instance.information = [.. result.information];
                instance.SetInformationBindings(result.InformationBindings.ToArray());

                buffer["ps"] = ps101;
                buffer["code"] = instance.GetType().Name;
                buffer["attributebindings"] = instance.Flatten();
                buffer["informationbindings"] = System.Text.Json.JsonSerializer.Serialize(instance.GetInformationBindings(), jsonSerializerOptions);

                buffer["sourceIdentifier"] = instance.sourceIdentifier;
                SetShape(buffer, current.SHAPE());
                SetUsageBand(buffer, current.PLTS_COMP_SCALE()!.Value);
                return instance;
            }
        }

        private static CargoTranshipmentArea CTSARE(Feature current, RowBuffer buffer) {
            var instance = new CargoTranshipmentArea();

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
                instance.status = GetStatus(current.STATUS());
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
                    Logger.Current.Debug($"VesselSpeedLimit {current.TableName()}::{current.OBJECTID} {current.INFORM()}");
            }

            if (current.PLTS_COMP_SCALE_HasValue() && current.SHAPE() != null) {
                string subtype = "";
                if (!Subtypes.Instance.TryGetSubtype(current.TableName(), current.FCSUBTYPE()!.Value, out subtype))
                    throw new NotSupportedException($"Unknown subtype for {current.TableName()}, {current.FCSUBTYPE()}");
                var scamin = Scamin.Instance.GetMinimumScale(current, subtype, current.PLTS_COMP_SCALE()!.Value, isRelatedToStructure: false);
                if (scamin.HasValue)
                    instance.scaleMinimum = scamin!.Value;
            }

            var result = ImporterNIS.AddInformation(current.GetObjectID(), current.TableName(), current.NTXTDS(), current.TXTDSC(), current.INFORM(), current.NINFOM());
            instance.information = [.. result.information];
            instance.SetInformationBindings(result.InformationBindings.ToArray());

            buffer["ps"] = ps101;
            buffer["code"] = instance.GetType().Name;
            buffer["attributebindings"] = instance.Flatten();
            buffer["informationbindings"] = System.Text.Json.JsonSerializer.Serialize(instance.GetInformationBindings(), jsonSerializerOptions);

            buffer["sourceIdentifier"] = instance.sourceIdentifier;
            SetShape(buffer, current.SHAPE());
            SetUsageBand(buffer, current.PLTS_COMP_SCALE()!.Value);

            return instance;
        }

        private static DeepWaterRoutePart DWRTPT(Feature current, RowBuffer buffer) {
            var instance = new DeepWaterRoutePart();

            if (current.ORIENT_HasValue()) {
                instance.orientationValue = current.ORIENT() == -32767m ? null : current.ORIENT();
            }
            if (current.DRVAL1_HasValue()) {
                instance.depthRangeMinimumValue = current.DRVAL1() == -32767m ? null : current.DRVAL1();
            }
            if (current.TRAFIC_HasValue()) {
                instance.trafficFlow = EnumHelper.GetEnumValue(current.TRAFIC());
            }

            var featureName = GetFeatureName(current.OBJNAM(), current.NOBJNM());
            if (featureName is not null)
                instance.featureName = featureName;

            DateHelper.TryGetFixedDateRange(current.DATSTA(), current.DATEND(), out var dateRange);
            if (dateRange is not null) {
                instance.fixedDateRange = dateRange;
            }

            // TODO: imoAdopted
            //instance.iMOAdopted = null;

            if (current.QUASOU_HasValue()) {
                var qualityOfVerticalMeasurement = EnumHelper.GetEnumValues(current.QUASOU());
                if (qualityOfVerticalMeasurement is not null && qualityOfVerticalMeasurement.Any())
                    instance.qualityOfVerticalMeasurement = qualityOfVerticalMeasurement;
            }

            if (current.RESTRN_HasValue()) {
                var restriction = EnumHelper.GetEnumValues(current.RESTRN());
                if (restriction is not null && restriction.Any())
                    instance.restriction = restriction;
            }

            if (current.STATUS_HasValue()) {
                instance.status = GetStatus(current.STATUS());
            }

            if (current.TECSOU != null) {
                var techniqueOfVerticalMeasurement = EnumHelper.GetEnumValues(current.TECSOU());
                if (techniqueOfVerticalMeasurement is not null && techniqueOfVerticalMeasurement.Any())
                    instance.techniqueOfVerticalMeasurement = techniqueOfVerticalMeasurement;
            }

            if (current.TRAFIC_HasValue()) {
                instance.trafficFlow = EnumHelper.GetEnumValue(current.TRAFIC());
            }

            if (current.SOUACC_HasValue()) {
                instance.verticalUncertainty = new() {
                    uncertaintyFixed = current.SOUACC()
                };
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
                    Logger.Current.Debug($"VesselSpeedLimit {current.TableName()}::{current.OBJECTID} {current.INFORM()}");
            }

            if (current.PLTS_COMP_SCALE_HasValue() && current.SHAPE() != null) {
                string subtype = "";
                if (!Subtypes.Instance.TryGetSubtype(current.TableName(), current.FCSUBTYPE()!.Value, out subtype))
                    throw new NotSupportedException($"Unknown subtype for {current.TableName()}, {current.FCSUBTYPE()}");
                var scaleMinimum = Scamin.Instance.GetMinimumScale(current, subtype, current.PLTS_COMP_SCALE()!.Value, isRelatedToStructure: false);
                if (scaleMinimum.HasValue)
                    instance.scaleMinimum = scaleMinimum;
            }

            var result = ImporterNIS.AddInformation(current.GetObjectID(), current.TableName(), current.NTXTDS(), current.TXTDSC(), current.INFORM(), current.NINFOM());
            instance.information = [.. result.information];
            instance.SetInformationBindings(result.InformationBindings.ToArray());

            buffer["ps"] = ps101;
            buffer["code"] = instance.GetType().Name;
            buffer["attributebindings"] = instance.Flatten();
            buffer["informationbindings"] = System.Text.Json.JsonSerializer.Serialize(instance.GetInformationBindings(), jsonSerializerOptions);

            buffer["sourceIdentifier"] = instance.sourceIdentifier;
            SetShape(buffer, current.SHAPE());
            SetUsageBand(buffer, current.PLTS_COMP_SCALE()!.Value);

            return instance;
        }

        private static DredgedArea DRGARE(Feature current, RowBuffer buffer) {
            var drval1 = current.DRVAL1() ?? default;
            var drval2 = current.DRVAL2() ?? default(decimal?);
            var sordat = current.SORDAT() ?? default;

            var restrn = current.RESTRN() ?? default;
            var quasou = current.QUASOU() ?? default;
            var tecsou = current.TECSOU() ?? default;

            var instance = new DredgedArea {
                depthRangeMinimumValue = drval1,
            };

            if (drval2.HasValue)
                instance.depthRangeMaximumValue = drval2.GetValueOrDefault();

            if (!string.IsNullOrEmpty(current.SORDAT())) {
                if (DateHelper.TryConvertSordat(current.SORDAT(), out var reportedDate)) {
                    instance.dredgedDate = reportedDate;
                }
            }
            else {
                Logger.Current.DataError(current.GetObjectID(), current.GetType().Name, current.LNAM() ?? "Unknown LNAM", $"Cannot convert date {current.SORDAT()}");
            }


            var featureName = GetFeatureName(current.OBJNAM(), current.NOBJNM());
            if (featureName is not null)
                instance.featureName = featureName;

            if (current.RESTRN_HasValue()) {
                var restriction = EnumHelper.GetEnumValues(current.RESTRN());
                if (restriction is not null && restriction.Any())
                    instance.restriction = restriction;
            }

            // TODO: maximumPermittedDraught - From INFORM - No instances in GST - Not converted


            // The S-57 attribute QUASOU for DEPARE will not be converted. It is considered that this attribute is
            // not relevant for Depth Area in S-101.
            //if (current.QUASOU_HasValue()) {
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
            //if (current.SOUACC_HasValue()) {
            //    instance.verticalUncertainty = new DomainModel.S101.ComplexAttributes.verticalUncertainty() {
            //        uncertaintyFixed = current.SOUACC()
            //    };
            //}
            //

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
                    Logger.Current.Debug($"VesselSpeedLimit {current.TableName()}::{current.OBJECTID} {current.INFORM()}");
            }

            var result = ImporterNIS.AddInformation(current.GetObjectID(), current.TableName(), current.NTXTDS(), current.TXTDSC(), current.INFORM(), current.NINFOM());
            instance.information = [.. result.information];
            instance.SetInformationBindings(result.InformationBindings.ToArray());

            buffer["ps"] = ps101;
            buffer["code"] = instance.GetType().Name;
            buffer["attributebindings"] = instance.Flatten();
            buffer["informationbindings"] = System.Text.Json.JsonSerializer.Serialize(instance.GetInformationBindings(), jsonSerializerOptions);

            buffer["sourceIdentifier"] = instance.sourceIdentifier;
            SetShape(buffer, current.SHAPE());
            SetUsageBand(buffer, current.PLTS_COMP_SCALE()!.Value);

            return instance;
        }

        private static DumpingGround DMPGRD(Feature current, RowBuffer buffer) {
            var instance = new DumpingGround();

            if (current.CATDPG_HasValue()) {
                var categoryOfDumpingGround = EnumHelper.GetEnumValues(current.CATDPG());
                if (categoryOfDumpingGround is not null && categoryOfDumpingGround.Any())
                    instance.categoryOfDumpingGround = categoryOfDumpingGround;
            }

            // TODO: DateDisused

            var featureName = GetFeatureName(current.OBJNAM(), current.NOBJNM());
            if (featureName is not null)
                instance.featureName = featureName;

            if (current.RESTRN_HasValue()) {
                var restriction = EnumHelper.GetEnumValues(current.RESTRN());
                if (restriction is not null && restriction.Any())
                    instance.restriction = restriction;
            }


            if (current.STATUS_HasValue()) {
                instance.status = GetStatus(current.STATUS());
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
                    Logger.Current.Debug($"VesselSpeedLimit {current.TableName()}::{current.OBJECTID} {current.INFORM()}");
            }

            if (current.PLTS_COMP_SCALE_HasValue() && current.SHAPE() != null) {
                string subtype = "";
                if (!Subtypes.Instance.TryGetSubtype(current.TableName(), current.FCSUBTYPE()!.Value, out subtype))
                    throw new NotSupportedException($"Unknown subtype for {current.TableName()}, {current.FCSUBTYPE()}");
                var scamin = Scamin.Instance.GetMinimumScale(current, subtype, current.PLTS_COMP_SCALE()!.Value, isRelatedToStructure: false);
                if (scamin.HasValue)
                    instance.scaleMinimum = scamin!.Value;
            }

            var result = ImporterNIS.AddInformation(current.GetObjectID(), current.TableName(), current.NTXTDS(), current.TXTDSC(), current.INFORM(), current.NINFOM());
            instance.information = [.. result.information];
            instance.SetInformationBindings(result.InformationBindings.ToArray());

            buffer["ps"] = ps101;
            buffer["code"] = instance.GetType().Name;
            buffer["attributebindings"] = instance.Flatten();
            buffer["informationbindings"] = System.Text.Json.JsonSerializer.Serialize(instance.GetInformationBindings(), jsonSerializerOptions);

            buffer["sourceIdentifier"] = instance.sourceIdentifier;
            SetShape(buffer, current.SHAPE());
            SetUsageBand(buffer, current.PLTS_COMP_SCALE()!.Value);
            return instance;
        }

        private static Fairway FAIRWY(Feature current, RowBuffer buffer) {
            var instance = new Fairway();

            if (current.DRVAL1_HasValue()) {
                instance.depthRangeMinimumValue = current.DRVAL1() != -32767m ? current.DRVAL1() : null;
            }

            var featureName = GetFeatureName(current.OBJNAM(), current.NOBJNM());
            if (featureName is not null)
                instance.featureName = featureName;

            DateHelper.TryGetFixedDateRange(current.DATSTA(), current.DATEND(), out var dateRange);
            if (dateRange is not null) {
                instance.fixedDateRange = dateRange;
            }

            // TODO: maximumPermittedDraught

            if (current.ORIENT_HasValue()) {
                instance.orientationValue = current.ORIENT() != -32767m ? current.ORIENT() : null;
            }

            if (current.QUASOU_HasValue()) {
                var qualityOfVerticalMeasurement = EnumHelper.GetEnumValues(current.QUASOU());
                if (qualityOfVerticalMeasurement is not null && qualityOfVerticalMeasurement.Any())
                    instance.qualityOfVerticalMeasurement = qualityOfVerticalMeasurement;
            }

            if (current.RESTRN_HasValue()) {
                var restriction = EnumHelper.GetEnumValues(current.RESTRN());
                if (restriction is not null && restriction.Any())
                    instance.restriction = restriction;
            }

            if (current.STATUS_HasValue()) {
                instance.status = GetStatus(current.STATUS());
            }

            if (current.TRAFIC_HasValue()) {
                instance.trafficFlow = EnumHelper.GetEnumValue(current.TRAFIC());
            }

            if (current.SOUACC_HasValue()) {
                instance.verticalUncertainty = new() {
                    uncertaintyFixed = current.SOUACC()
                };
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
                    Logger.Current.Debug($"VesselSpeedLimit {current.TableName()}::{current.OBJECTID} {current.INFORM()}");
            }

            if (current.PLTS_COMP_SCALE_HasValue() && current.SHAPE() != null) {
                string subtype = "";
                if (!Subtypes.Instance.TryGetSubtype(current.TableName(), current.FCSUBTYPE()!.Value, out subtype))
                    throw new NotSupportedException($"Unknown subtype for {current.TableName()}, {current.FCSUBTYPE()}");
                var scaleMinimum = Scamin.Instance.GetMinimumScale(current, subtype, current.PLTS_COMP_SCALE()!.Value, isRelatedToStructure: false);
                if (scaleMinimum.HasValue)
                    instance.scaleMinimum = scaleMinimum;
            }

            var result = ImporterNIS.AddInformation(current.GetObjectID(), current.TableName(), current.NTXTDS(), current.TXTDSC(), current.INFORM(), current.NINFOM());
            instance.information = [.. result.information];
            instance.SetInformationBindings(result.InformationBindings.ToArray());

            buffer["ps"] = ps101;
            buffer["code"] = instance.GetType().Name;
            buffer["attributebindings"] = instance.Flatten();
            buffer["informationbindings"] = System.Text.Json.JsonSerializer.Serialize(instance.GetInformationBindings(), jsonSerializerOptions);

            buffer["sourceIdentifier"] = instance.sourceIdentifier;
            SetShape(buffer, current.SHAPE());
            SetUsageBand(buffer, current.PLTS_COMP_SCALE()!.Value);
            return instance;
        }

        private static HarbourFacility HRBFAC(Feature current, RowBuffer buffer) {
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

            if (!string.IsNullOrEmpty(current.SORDAT())) {
                if (DateHelper.TryConvertSordat(current.SORDAT(), out var reportedDate)) {
                    instance.reportedDate = reportedDate;
                }
                else {
                    Logger.Current.DataError(current.GetObjectID(), current.GetType().Name, current.LNAM() ?? "Unknown LNAM", $"Cannot convert date {current.SORDAT()}");
                }
            }

            // TODO: restriction

            if (current.STATUS_HasValue()) {
                instance.status = GetStatus(current.STATUS());
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
                    Logger.Current.Debug($"VesselSpeedLimit {current.TableName()}::{current.OBJECTID} {current.INFORM()}");
            }

            if (current.PLTS_COMP_SCALE_HasValue() && current.SHAPE() != null) {
                string subtype = "";
                if (!Subtypes.Instance.TryGetSubtype(current.TableName(), current.FCSUBTYPE()!.Value, out subtype))
                    throw new NotSupportedException($"Unknown subtype for {current.TableName()}, {current.FCSUBTYPE()}");
                var scaleMinimum = Scamin.Instance.GetMinimumScale(current, subtype, current.PLTS_COMP_SCALE()!.Value, isRelatedToStructure: false);
                if (scaleMinimum.HasValue)
                    instance.scaleMinimum = scaleMinimum;
            }

            var result = ImporterNIS.AddInformation(current.GetObjectID(), current.TableName(), current.NTXTDS(), current.TXTDSC(), current.INFORM(), current.NINFOM());
            instance.information = [.. result.information];
            instance.SetInformationBindings(result.InformationBindings.ToArray());

            buffer["ps"] = ps101;
            buffer["code"] = instance.GetType().Name;
            buffer["attributebindings"] = instance.Flatten();
            buffer["informationbindings"] = System.Text.Json.JsonSerializer.Serialize(instance.GetInformationBindings(), jsonSerializerOptions);

            buffer["sourceIdentifier"] = instance.sourceIdentifier;
            SetShape(buffer, current.SHAPE());
            SetUsageBand(buffer, current.PLTS_COMP_SCALE()!.Value);
            return instance;
        }

        private static InshoreTrafficZone ISTZNE(Feature current, RowBuffer buffer) {
            var instance = new InshoreTrafficZone();

            DateHelper.TryGetFixedDateRange(current.DATSTA(), current.DATEND(), out var dateRange);
            if (dateRange is not null) {
                instance.fixedDateRange = dateRange;
            }

            if (current.RESTRN_HasValue()) {
                var restriction = EnumHelper.GetEnumValues(current.RESTRN());
                if (restriction is not null && restriction.Any())
                    instance.restriction = restriction;
            }

            if (current.STATUS_HasValue()) {
                instance.status = GetStatus(current.STATUS());
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
                    Logger.Current.Debug($"VesselSpeedLimit {current.TableName()}::{current.OBJECTID} {current.INFORM()}");
            }

            if (current.PLTS_COMP_SCALE_HasValue() && current.SHAPE() != null) {
                string subtype = "";
                if (!Subtypes.Instance.TryGetSubtype(current.TableName(), current.FCSUBTYPE()!.Value, out subtype))
                    throw new NotSupportedException($"Unknown subtype for {current.TableName()}, {current.FCSUBTYPE()}");
                var scaleMinimum = Scamin.Instance.GetMinimumScale(current, subtype, current.PLTS_COMP_SCALE()!.Value, isRelatedToStructure: false);
                if (scaleMinimum.HasValue)
                    instance.scaleMinimum = scaleMinimum;
            }

            var result = ImporterNIS.AddInformation(current.GetObjectID(), current.TableName(), current.NTXTDS(), current.TXTDSC(), current.INFORM(), current.NINFOM());
            instance.information = [.. result.information];
            instance.SetInformationBindings(result.InformationBindings.ToArray());

            buffer["ps"] = ps101;
            buffer["code"] = instance.GetType().Name;
            buffer["attributebindings"] = instance.Flatten();
            buffer["informationbindings"] = System.Text.Json.JsonSerializer.Serialize(instance.GetInformationBindings(), jsonSerializerOptions);

            buffer["sourceIdentifier"] = instance.sourceIdentifier;
            SetShape(buffer, current.SHAPE());
            SetUsageBand(buffer, current.PLTS_COMP_SCALE()!.Value);
            return instance;
        }

        private static MarineFarmCulture MARCUL(Feature current, RowBuffer buffer) {
            var instance = new MarineFarmCulture();

            if (current.CATMFA != null) {
                instance.categoryOfMarineFarmCulture = EnumHelper.GetEnumValue(current.CATMFA());
            }

            if (current.EXPSOU_HasValue()) {
                instance.expositionOfSounding = EnumHelper.GetEnumValue(current.EXPSOU());
            }

            var featureName = GetFeatureName(current.OBJNAM(), current.NOBJNM());
            if (featureName is not null)
                instance.featureName = featureName;

            DateHelper.TryGetFixedDateRange(current.DATSTA(), current.DATEND(), out var dateRange);
            if (dateRange is not null) {
                instance.fixedDateRange = dateRange;
            }

            // TODO: interoperability identifier

            DateHelper.TryGetPeriodicDateRange(current.PERSTA(), current.PEREND(), out var periodicDateRange);
            if (periodicDateRange is not null) {
                instance.periodicDateRange = periodicDateRange;
            }

            if (current.QUASOU_HasValue()) {
                var qualityOfVerticalMeasurement = EnumHelper.GetEnumValues(current.QUASOU());
                if (qualityOfVerticalMeasurement is not null && qualityOfVerticalMeasurement.Any())
                    instance.qualityOfVerticalMeasurement = qualityOfVerticalMeasurement;
            }

            if (current.RESTRN_HasValue()) {
                var restriction = EnumHelper.GetEnumValues(current.RESTRN());
                if (restriction is not null && restriction.Any())
                    instance.restriction = restriction;
            }

            if (current.STATUS_HasValue()) {
                instance.status = GetStatus(current.STATUS());
            }

            if (current.VALSOU_HasValue()) {
                instance.valueOfSounding = current.VALSOU() != -32767m ? current.VALSOU() : null;
            }
            else {
                // Exactly one of the attributes height or value of sounding must be populated
                if (current.WATLEV_HasValue() && new int[] { 1, 2, -32767 }.Contains(current.WATLEV()!.Value)) {
                    instance.height = null;
                }
                else
                    instance.valueOfSounding = null;
            }


            if (current.VERLEN_HasValue()) {
                instance.verticalLength = current.VERLEN() != -32767m ? current.VERLEN() : null;
            }
            else if (current.VERLEN_HasValue() && current.VERLEN() == -32767m) {
                //instance.verticalLength = default(decimal?);
            }

            // TODO: VerticalUncertainty

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
                    Logger.Current.Debug($"VesselSpeedLimit {current.TableName()}::{current.OBJECTID} {current.INFORM()}");
            }

            if (current.WATLEV_HasValue()) {
                instance.waterLevelEffect = EnumHelper.GetEnumValue(current.WATLEV());
            }

            // TODO: HEIGHT                            
            if (instance.waterLevelEffect == 1 || instance.waterLevelEffect == 2) {
                /* The attribute height must be populated for Marine Farm/Culture features having attribute water level
                   effect = 1 (partly submerged at high water) or 2 (always dry). */


            }

            if (current.PLTS_COMP_SCALE_HasValue() && current.SHAPE() != null) {
                string subtype = "";

                if (!Subtypes.Instance.TryGetSubtype(current.TableName(), current.FCSUBTYPE()!.Value, out subtype))
                    throw new NotSupportedException($"Unknown subtype for {current.TableName()}, {current.FCSUBTYPE()}");

                var scamin = Scamin.Instance.GetMinimumScale(current, subtype, current.PLTS_COMP_SCALE()!.Value, isRelatedToStructure: false);
                if (scamin.HasValue)
                    instance.scaleMinimum = scamin!.Value;
            }


            var result = ImporterNIS.AddInformation(current.GetObjectID(), current.TableName(), current.NTXTDS(), current.TXTDSC(), current.INFORM(), current.NINFOM());
            instance.information = [.. result.information];
            instance.SetInformationBindings(result.InformationBindings.ToArray());

            buffer["ps"] = ps101;
            buffer["code"] = instance.GetType().Name;
            buffer["attributebindings"] = instance.Flatten();
            buffer["informationbindings"] = System.Text.Json.JsonSerializer.Serialize(instance.GetInformationBindings(), jsonSerializerOptions);

            buffer["sourceIdentifier"] = instance.sourceIdentifier;
            SetShape(buffer, current.SHAPE());
            SetUsageBand(buffer, current.PLTS_COMP_SCALE()!.Value);
            return instance;
        }

        private static MilitaryPracticeArea MIPARE(Feature current, RowBuffer buffer) {
            var instance = new MilitaryPracticeArea();

            if (current.CATMPA_HasValue()) {
                var categoryOfMilitaryPracticeArea = EnumHelper.GetEnumValues(current.CATMPA());
                if (categoryOfMilitaryPracticeArea is not null && categoryOfMilitaryPracticeArea.Any())
                    instance.categoryOfMilitaryPracticeArea = categoryOfMilitaryPracticeArea;
            }

            var featureName = GetFeatureName(current.OBJNAM(), current.NOBJNM());
            if (featureName is not null)
                instance.featureName = featureName;

            DateHelper.TryGetFixedDateRange(current.DATSTA(), current.DATEND(), out var dateRange);
            if (dateRange is not null) {
                instance.fixedDateRange = dateRange;
            }

            // TODO: nationality

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
                instance.status = GetStatus(current.STATUS());
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
                    Logger.Current.Debug($"VesselSpeedLimit {current.TableName()}::{current.OBJECTID} {current.INFORM()}");
            }

            if (current.PLTS_COMP_SCALE_HasValue() && current.SHAPE() != null) {
                string subtype = "";
                if (!Subtypes.Instance.TryGetSubtype(current.TableName(), current.FCSUBTYPE()!.Value, out subtype))
                    throw new NotSupportedException($"Unknown subtype for {current.TableName()}, {current.FCSUBTYPE()}");
                var scamin = Scamin.Instance.GetMinimumScale(current, subtype, current.PLTS_COMP_SCALE()!.Value, isRelatedToStructure: false);
                if (scamin.HasValue)
                    instance.scaleMinimum = scamin!.Value;
            }

            var result = ImporterNIS.AddInformation(current.GetObjectID(), current.TableName(), current.NTXTDS(), current.TXTDSC(), current.INFORM(), current.NINFOM());
            instance.information = [.. result.information];
            instance.SetInformationBindings(result.InformationBindings.ToArray());

            buffer["ps"] = ps101;
            buffer["code"] = instance.GetType().Name;
            buffer["attributebindings"] = instance.Flatten();
            buffer["informationbindings"] = System.Text.Json.JsonSerializer.Serialize(instance.GetInformationBindings(), jsonSerializerOptions);

            buffer["sourceIdentifier"] = instance.sourceIdentifier;
            SetShape(buffer, current.SHAPE());
            SetUsageBand(buffer, current.PLTS_COMP_SCALE()!.Value);
            return instance;
        }

        private static OffshoreProductionArea OSPARE(Feature current, RowBuffer buffer) {
            var instance = new OffshoreProductionArea();

            if (current.CATPRA_HasValue()) {
                // Windfarm
                if (current.CATPRA() == 9) {
                    instance.categoryOfOffshoreProductionArea = 1;  // categoryOfOffshoreProductionArea.WindFarm;
                }
                else if (current.CATPRA() == 8) {
                    instance.categoryOfOffshoreProductionArea = 4;  // categoryOfOffshoreProductionArea.TankFarm;
                }
                else {
                    Logger.Current.DataError(current.GetObjectID(), current.TableName(), current.LNAM() ?? "Unknown LNAM", $"Cannot convert OffshoreInstallation with CATPRA = {current.CATPRA()}");
                    //continue;
                    //throw new NotSupportedException($"Cannot convert offshoreproductionarea with CATPRA {current.CATPRA()}");
                    //instance.categoryOfOffshoreProductionArea = EnumHelper.GetEnumValue<categoryOfOffshoreProductionArea>(current.CATPRA());
                }
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

            if (current.HEIGHT_HasValue()) {
                instance.height = current.HEIGHT() != -32767m ? current.HEIGHT() : null;
            }
            else {

            }

            if (current.PRODCT != null) {
                var product = EnumHelper.GetEnumValues(current.PRODCT());
                if (product is not null && product.Any())
                    instance.product = product;
            }

            if (current.CONRAD_HasValue()) {
                instance.radarConspicuous = current.CONRAD() == 2 ? false : true;
            }
            if (!string.IsNullOrEmpty(current.SORDAT())) {
                if (DateHelper.TryConvertSordat(current.SORDAT(), out var reportedDate)) {
                    instance.reportedDate = reportedDate;
                }
                else {
                    Logger.Current.DataError(current.GetObjectID(), current.GetType().Name, current.LNAM() ?? "Unknown LNAM", $"Cannot convert date {current.SORDAT()}");
                }
            }

            if (current.RESTRN_HasValue()) {
                var restriction = EnumHelper.GetEnumValues(current.RESTRN());
                if (restriction is not null && restriction.Any())
                    instance.restriction = restriction;
            }

            if (current.STATUS_HasValue()) {
                instance.status = GetStatus(current.STATUS());
            }

            if (current.VERLEN_HasValue()) {
                instance.verticalLength = current.VERLEN() != -32767m ? current.VERLEN() : null;
            }
            else {
                //instance.verticalLength = default(decimal?);
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
                    Logger.Current.Debug($"VesselSpeedLimit {current.TableName()}::{current.OBJECTID} {current.INFORM()}");
            }

            if (current.CONVIS_HasValue()) {
                instance.visualProminence = EnumHelper.GetEnumValue(current.CONVIS());
            }

            // TODO: waterleveleffect

            if (current.PLTS_COMP_SCALE_HasValue() && current.SHAPE() != null) {
                string subtype = "";
                if (current.TableName() != default && current.FCSUBTYPE_HasValue() && !Subtypes.Instance.TryGetSubtype(current.TableName(), current.FCSUBTYPE()!.Value, out subtype))
                    throw new NotSupportedException($"Unknown subtype for {current.TableName()}, {current.FCSUBTYPE()}");
                var scaleMinimum = Scamin.Instance.GetMinimumScale(current, subtype, current.PLTS_COMP_SCALE()!.Value, isRelatedToStructure: false);
                if (scaleMinimum.HasValue)
                    instance.scaleMinimum = scaleMinimum;
            }

            var result = ImporterNIS.AddInformation(current.GetObjectID(), current.TableName(), current.NTXTDS(), current.TXTDSC(), current.INFORM(), current.NINFOM());
            instance.information = [.. result.information];
            instance.SetInformationBindings(result.InformationBindings.ToArray());

            buffer["ps"] = ps101;
            buffer["code"] = instance.GetType().Name;
            buffer["attributebindings"] = instance.Flatten();
            buffer["informationbindings"] = System.Text.Json.JsonSerializer.Serialize(instance.GetInformationBindings(), jsonSerializerOptions);

            buffer["sourceIdentifier"] = instance.sourceIdentifier;
            SetShape(buffer, current.SHAPE());
            SetUsageBand(buffer, current.PLTS_COMP_SCALE()!.Value);
            return instance;
        }

        private static PrecautionaryArea PRCARE(Feature current, RowBuffer buffer) {
            var instance = new PrecautionaryArea();

            var featureName = GetFeatureName(current.OBJNAM(), current.NOBJNM());
            if (featureName is not null)
                instance.featureName = featureName;


            DateHelper.TryGetFixedDateRange(current.DATSTA(), current.DATEND(), out var dateRange);
            if (dateRange is not null) {
                instance.fixedDateRange = dateRange;
            }

            // TODO: imoAdopted
            //instance.iMOAdopted = null;

            if (current.RESTRN_HasValue()) {
                var restriction = EnumHelper.GetEnumValues(current.RESTRN());
                if (restriction is not null && restriction.Any())
                    instance.restriction = restriction;
            }

            if (current.STATUS_HasValue()) {
                instance.status = GetStatus(current.STATUS());
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
                    Logger.Current.Debug($"VesselSpeedLimit {current.TableName()}::{current.OBJECTID} {current.INFORM()}");
            }

            if (current.PLTS_COMP_SCALE_HasValue() && current.SHAPE() != null) {
                string subtype = "";
                if (current.TableName() != default && current.FCSUBTYPE_HasValue() && !Subtypes.Instance.TryGetSubtype(current.TableName(), current.FCSUBTYPE()!.Value, out subtype))
                    throw new NotSupportedException($"Unknown subtype for {current.TableName()}, {current.FCSUBTYPE()}");
                var scaleMinimum = Scamin.Instance.GetMinimumScale(current, subtype, current.PLTS_COMP_SCALE()!.Value, isRelatedToStructure: false);
                if (scaleMinimum.HasValue)
                    instance.scaleMinimum = scaleMinimum;
            }

            var result = ImporterNIS.AddInformation(current.GetObjectID(), current.TableName(), current.NTXTDS(), current.TXTDSC(), current.INFORM(), current.NINFOM());
            instance.information = [.. result.information];
            instance.SetInformationBindings(result.InformationBindings.ToArray());

            buffer["ps"] = ps101;
            buffer["code"] = instance.GetType().Name;
            buffer["attributebindings"] = instance.Flatten();
            buffer["informationbindings"] = System.Text.Json.JsonSerializer.Serialize(instance.GetInformationBindings(), jsonSerializerOptions);

            buffer["sourceIdentifier"] = instance.sourceIdentifier;
            SetShape(buffer, current.SHAPE());
            SetUsageBand(buffer, current.PLTS_COMP_SCALE()!.Value);
            return instance;
        }

        private static FeatureType CGUSTA(Feature current, RowBuffer buffer) {
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
                instance.status = GetStatus(current.STATUS());
            }

            if (current.PLTS_COMP_SCALE_HasValue() && current.SHAPE() != null) {
                string subtype = "";
                if (current.TableName() != default && current.FCSUBTYPE_HasValue() && !Subtypes.Instance.TryGetSubtype(current.TableName(), current.FCSUBTYPE()!.Value, out subtype))
                    throw new NotSupportedException($"Unknown subtype for {current.TableName()}, {current.FCSUBTYPE()}");
                var scaleMinimum = Scamin.Instance.GetMinimumScale(current, subtype, current.PLTS_COMP_SCALE()!.Value, isRelatedToStructure: false);
                if (scaleMinimum.HasValue)
                    instance.scaleMinimum = scaleMinimum;
            }

            var result = ImporterNIS.AddInformation(current.GetObjectID(), current.TableName(), current.NTXTDS(), current.TXTDSC(), current.INFORM(), current.NINFOM());
            instance.information = [.. result.information];
            instance.SetInformationBindings(result.InformationBindings.ToArray());

            buffer["ps"] = ps101;
            buffer["code"] = instance.GetType().Name;
            buffer["attributebindings"] = instance.Flatten();
            buffer["informationbindings"] = System.Text.Json.JsonSerializer.Serialize(instance.GetInformationBindings(), jsonSerializerOptions);

            buffer["sourceIdentifier"] = instance.sourceIdentifier;
            SetShape(buffer, current.SHAPE());
            SetUsageBand(buffer, current.PLTS_COMP_SCALE()!.Value);
            return instance;
        }

        private static SeaplaneLandingArea SPLARE(Feature current, RowBuffer buffer) {
            var instance = new SeaplaneLandingArea();

            var featureName = GetFeatureName(current.OBJNAM(), current.NOBJNM());
            if (featureName is not null)
                instance.featureName = featureName;


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
                instance.status = GetStatus(current.STATUS());
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
                    Logger.Current.Debug($"VesselSpeedLimit {current.TableName()}::{current.OBJECTID} {current.INFORM()}");
            }

            if (current.PLTS_COMP_SCALE_HasValue() && current.SHAPE() != null) {
                string subtype = "";
                if (current.TableName() != default && current.FCSUBTYPE_HasValue() && !Subtypes.Instance.TryGetSubtype(current.TableName(), current.FCSUBTYPE()!.Value, out subtype))
                    throw new NotSupportedException($"Unknown subtype for {current.TableName()}, {current.FCSUBTYPE()}");
                var scamin = Scamin.Instance.GetMinimumScale(current, subtype, current.PLTS_COMP_SCALE()!.Value, isRelatedToStructure: false);
                if (scamin.HasValue)
                    instance.scaleMinimum = scamin!.Value;
            }

            var result = ImporterNIS.AddInformation(current.GetObjectID(), current.TableName(), current.NTXTDS(), current.TXTDSC(), current.INFORM(), current.NINFOM());
            instance.information = [.. result.information];
            instance.SetInformationBindings(result.InformationBindings.ToArray());

            buffer["ps"] = ps101;
            buffer["code"] = instance.GetType().Name;
            buffer["attributebindings"] = instance.Flatten();
            buffer["informationbindings"] = System.Text.Json.JsonSerializer.Serialize(instance.GetInformationBindings(), jsonSerializerOptions);

            buffer["sourceIdentifier"] = instance.sourceIdentifier;
            SetShape(buffer, current.SHAPE());
            SetUsageBand(buffer, current.PLTS_COMP_SCALE()!.Value);
            return instance;
        }

        private static CableArea CBLARE(Feature current, RowBuffer buffer) {
            var instance = new CableArea();

            if (current.CATCBL_HasValue()) {
                var categoryOfCable = EnumHelper.GetEnumValues(current.CATCBL());
                if (categoryOfCable is not null && categoryOfCable.Any())
                    instance.categoryOfCable = categoryOfCable;
            }

            var featureName = GetFeatureName(current.OBJNAM(), current.NOBJNM());
            if (featureName is not null)
                instance.featureName = featureName;

            DateHelper.TryGetFixedDateRange(current.DATSTA(), current.DATEND(), out var dateRange);
            if (dateRange is not null) {
                instance.fixedDateRange = dateRange;
            }

            if (current.RESTRN_HasValue()) {
                var restriction = EnumHelper.GetEnumValues(current.RESTRN());
                if (restriction is not null && restriction.Any())
                    instance.restriction = restriction;
            }

            if (current.STATUS_HasValue()) {
                instance.status = GetStatus(current.STATUS());
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
                    Logger.Current.Debug($"VesselSpeedLimit {current.TableName()}::{current.OBJECTID} {current.INFORM()}");
            }

            if (current.PLTS_COMP_SCALE_HasValue() && current.SHAPE() != null) {
                string subtype = "";
                if (current.TableName() != default && current.FCSUBTYPE_HasValue() && !Subtypes.Instance.TryGetSubtype(current.TableName(), current.FCSUBTYPE()!.Value, out subtype))
                    throw new NotSupportedException($"Unknown subtype for {current.TableName()}, {current.FCSUBTYPE()}");
                var scaleMinimum = Scamin.Instance.GetMinimumScale(current, subtype, current.PLTS_COMP_SCALE()!.Value, isRelatedToStructure: false);
                if (scaleMinimum.HasValue)
                    instance.scaleMinimum = scaleMinimum;
            }
            var result = ImporterNIS.AddInformation(current.GetObjectID(), current.TableName(), current.NTXTDS(), current.TXTDSC(), current.INFORM(), current.NINFOM());
            instance.information = [.. result.information];
            instance.SetInformationBindings(result.InformationBindings.ToArray());

            buffer["ps"] = ps101;
            buffer["code"] = instance.GetType().Name;
            buffer["attributebindings"] = instance.Flatten();
            buffer["informationbindings"] = System.Text.Json.JsonSerializer.Serialize(instance.GetInformationBindings(), jsonSerializerOptions);

            buffer["sourceIdentifier"] = instance.sourceIdentifier;
            SetShape(buffer, current.SHAPE());
            SetUsageBand(buffer, current.PLTS_COMP_SCALE()!.Value);
            return instance;
        }

        private static TrafficSeparationSchemeLanePart TSSLPT(Feature current, RowBuffer buffer) {
            var instance = new TrafficSeparationSchemeLanePart();

            DateHelper.TryGetFixedDateRange(current.DATSTA(), current.DATEND(), out var dateRange);
            if (dateRange is not null) {
                instance.fixedDateRange = dateRange;
            }

            if (current.ORIENT_HasValue()) {
                instance.orientationValue = current.ORIENT() == -32767m ? null : current.ORIENT();
            }

            if (current.RESTRN_HasValue()) {
                var restriction = EnumHelper.GetEnumValues(current.RESTRN());
                if (restriction is not null && restriction.Any())
                    instance.restriction = restriction;
            }

            if (current.STATUS_HasValue()) {
                instance.status = GetStatus(current.STATUS());
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
                    Logger.Current.Debug($"VesselSpeedLimit {current.TableName()}::{current.OBJECTID} {current.INFORM()}");
            }

            if (current.PLTS_COMP_SCALE_HasValue() && current.SHAPE() != null) {
                string subtype = "";
                if (current.TableName() != default && current.FCSUBTYPE_HasValue() && !Subtypes.Instance.TryGetSubtype(current.TableName(), current.FCSUBTYPE()!.Value, out subtype))
                    throw new NotSupportedException($"Unknown subtype for {current.TableName()}, {current.FCSUBTYPE()}");
                var scaleMinimum = Scamin.Instance.GetMinimumScale(current, subtype, current.PLTS_COMP_SCALE()!.Value, isRelatedToStructure: false);
                if (scaleMinimum.HasValue)
                    instance.scaleMinimum = scaleMinimum;
            }

            var result = ImporterNIS.AddInformation(current.GetObjectID(), current.TableName(), current.NTXTDS(), current.TXTDSC(), current.INFORM(), current.NINFOM());
            instance.information = [.. result.information];
            instance.SetInformationBindings(result.InformationBindings.ToArray());

            buffer["ps"] = ps101;
            buffer["code"] = instance.GetType().Name;
            buffer["attributebindings"] = instance.Flatten();
            buffer["informationbindings"] = System.Text.Json.JsonSerializer.Serialize(instance.GetInformationBindings(), jsonSerializerOptions);

            buffer["sourceIdentifier"] = instance.sourceIdentifier;
            SetShape(buffer, current.SHAPE());
            SetUsageBand(buffer, current.PLTS_COMP_SCALE()!.Value);
            return instance;
        }

        private static TrafficSeparationSchemeRoundabout TSSRON(Feature current, RowBuffer buffer) {
            var instance = new TrafficSeparationSchemeRoundabout();

            DateHelper.TryGetFixedDateRange(current.DATSTA(), current.DATEND(), out var dateRange);
            if (dateRange is not null) {
                instance.fixedDateRange = dateRange;
            }

            if (current.RESTRN_HasValue()) {
                var restriction = EnumHelper.GetEnumValues(current.RESTRN());
                if (restriction is not null && restriction.Any())
                    instance.restriction = restriction;
            }

            if (current.STATUS_HasValue()) {
                instance.status = GetStatus(current.STATUS());
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
                    Logger.Current.Debug($"VesselSpeedLimit {current.TableName()}::{current.OBJECTID} {current.INFORM()}");
            }

            if (current.PLTS_COMP_SCALE_HasValue() && current.SHAPE() != null) {
                string subtype = "";
                if (current.TableName() != default && current.FCSUBTYPE_HasValue() && !Subtypes.Instance.TryGetSubtype(current.TableName(), current.FCSUBTYPE()!.Value, out subtype))
                    throw new NotSupportedException($"Unknown subtype for {current.TableName()}, {current.FCSUBTYPE()}");
                var scamin = Scamin.Instance.GetMinimumScale(current, subtype, current.PLTS_COMP_SCALE()!.Value, isRelatedToStructure: false);
                if (scamin.HasValue)
                    instance.scaleMinimum = scamin!.Value;
            }

            var result = ImporterNIS.AddInformation(current.GetObjectID(), current.TableName(), current.NTXTDS(), current.TXTDSC(), current.INFORM(), current.NINFOM());
            instance.information = [.. result.information];
            instance.SetInformationBindings(result.InformationBindings.ToArray());

            buffer["ps"] = ps101;
            buffer["code"] = instance.GetType().Name;
            buffer["attributebindings"] = instance.Flatten();
            buffer["informationbindings"] = System.Text.Json.JsonSerializer.Serialize(instance.GetInformationBindings(), jsonSerializerOptions);

            buffer["sourceIdentifier"] = instance.sourceIdentifier;
            SetShape(buffer, current.SHAPE());
            SetUsageBand(buffer, current.PLTS_COMP_SCALE()!.Value);
            return instance;
        }

        private static SubmarinePipelineArea PIPARE(Feature current, RowBuffer buffer) {
            var instance = new SubmarinePipelineArea();

            if (current.CATPIP_HasValue()) {
                var categoryOfPipelinePipe = EnumHelper.GetEnumValues(current.CATPIP());
                if (categoryOfPipelinePipe is not null && categoryOfPipelinePipe.Any())
                    instance.categoryOfPipelinePipe = categoryOfPipelinePipe;
            }

            var featureName = GetFeatureName(current.OBJNAM(), current.NOBJNM());
            if (featureName is not null)
                instance.featureName = featureName;

            DateHelper.TryGetFixedDateRange(current.DATSTA(), current.DATEND(), out var dateRange);
            if (dateRange is not null) {
                instance.fixedDateRange = dateRange;
            }

            if (current.PRODCT != null) {
                var product = EnumHelper.GetEnumValues(current.PRODCT());
                if (product is not null && product.Any())
                    instance.product = product;
            }

            if (current.RESTRN_HasValue()) {
                var restriction = EnumHelper.GetEnumValues(current.RESTRN());
                if (restriction is not null && restriction.Any())
                    instance.restriction = restriction;
            }

            if (current.STATUS_HasValue()) {
                instance.status = GetStatus(current.STATUS());
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
                    Logger.Current.Debug($"VesselSpeedLimit {current.TableName()}::{current.OBJECTID} {current.INFORM()}");
            }

            if (current.PLTS_COMP_SCALE_HasValue() && current.SHAPE() != null) {
                string subtype = "";
                if (current.TableName() != default && current.FCSUBTYPE_HasValue() && !Subtypes.Instance.TryGetSubtype(current.TableName(), current.FCSUBTYPE()!.Value, out subtype))
                    throw new NotSupportedException($"Unknown subtype for {current.TableName()}, {current.FCSUBTYPE()}");
                var scaleMinimum = Scamin.Instance.GetMinimumScale(current, subtype, current.PLTS_COMP_SCALE()!.Value, isRelatedToStructure: false);
                if (scaleMinimum.HasValue)
                    instance.scaleMinimum = scaleMinimum;
            }

            var result = ImporterNIS.AddInformation(current.GetObjectID(), current.TableName(), current.NTXTDS(), current.TXTDSC(), current.INFORM(), current.NINFOM());
            instance.information = [.. result.information];
            instance.SetInformationBindings(result.InformationBindings.ToArray());

            buffer["ps"] = ps101;
            buffer["code"] = instance.GetType().Name;
            buffer["attributebindings"] = instance.Flatten();
            buffer["informationbindings"] = System.Text.Json.JsonSerializer.Serialize(instance.GetInformationBindings(), jsonSerializerOptions);

            buffer["sourceIdentifier"] = instance.sourceIdentifier;
            SetShape(buffer, current.SHAPE());
            SetUsageBand(buffer, current.PLTS_COMP_SCALE()!.Value);
            return instance;
        }

        private static TerritorialSeaArea TESARE(Feature current, RowBuffer buffer) {
            var instance = new TerritorialSeaArea();

            // TODO: inDispute

            if (current.NATION_HasValue()) {
                instance.nationality = [GetNation(current.NATION())];
            }

            if (current.RESTRN_HasValue()) {
                var restriction = EnumHelper.GetEnumValues(current.RESTRN());
                if (restriction is not null && restriction.Any())
                    instance.restriction = restriction;
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
                    Logger.Current.Debug($"VesselSpeedLimit {current.TableName()}::{current.OBJECTID} {current.INFORM()}");
            }

            if (current.PLTS_COMP_SCALE_HasValue() && current.SHAPE() != null) {
                string subtype = "";
                if (current.TableName() != default && current.FCSUBTYPE_HasValue() && !Subtypes.Instance.TryGetSubtype(current.TableName(), current.FCSUBTYPE()!.Value, out subtype))
                    throw new NotSupportedException($"Unknown subtype for {current.TableName()}, {current.FCSUBTYPE()}");
                var scamin = Scamin.Instance.GetMinimumScale(current, subtype, current.PLTS_COMP_SCALE()!.Value, isRelatedToStructure: false);
                if (scamin.HasValue)
                    instance.scaleMinimum = scamin!.Value;
            }

            var result = ImporterNIS.AddInformation(current.GetObjectID(), current.TableName(), current.NTXTDS(), current.TXTDSC(), current.INFORM(), current.NINFOM());
            instance.information = [.. result.information];
            instance.SetInformationBindings(result.InformationBindings.ToArray());

            buffer["ps"] = ps101;
            buffer["code"] = instance.GetType().Name;
            buffer["attributebindings"] = instance.Flatten();
            buffer["informationbindings"] = System.Text.Json.JsonSerializer.Serialize(instance.GetInformationBindings(), jsonSerializerOptions);

            buffer["sourceIdentifier"] = instance.sourceIdentifier;
            SetShape(buffer, current.SHAPE());
            SetUsageBand(buffer, current.PLTS_COMP_SCALE()!.Value);
            return instance;
        }

        private static RescueStation RSCSTA(Feature current, RowBuffer buffer) {
            var instance = new RescueStation();

            if (current.CATRSC_HasValue()) {
                var categoryOfRescueStation = EnumHelper.GetEnumValues(current.CATRSC());
                if (categoryOfRescueStation is not null && categoryOfRescueStation.Any())
                    instance.categoryOfRescueStation = categoryOfRescueStation;
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
                instance.status = GetStatus(current.STATUS()!);
            }

            if (current.PLTS_COMP_SCALE_HasValue() && current.SHAPE() != null) {
                string subtype = "";
                if (current.TableName() != default && current.FCSUBTYPE_HasValue() && !Subtypes.Instance.TryGetSubtype(current.TableName(), current.FCSUBTYPE()!.Value, out subtype))
                    throw new NotSupportedException($"Unknown subtype for {current.TableName()}, {current.FCSUBTYPE()}");
                var scaleMinimum = Scamin.Instance.GetMinimumScale(current, subtype, current.PLTS_COMP_SCALE()!.Value, isRelatedToStructure: false);
                if (scaleMinimum.HasValue)
                    instance.scaleMinimum = scaleMinimum;
            }

            var result = ImporterNIS.AddInformation(current.GetObjectID(), current.TableName(), current.NTXTDS(), current.TXTDSC(), current.INFORM(), current.NINFOM());
            instance.information = [.. result.information];
            instance.SetInformationBindings(result.InformationBindings.ToArray());

            buffer["ps"] = ps101;
            buffer["code"] = instance.GetType().Name;
            buffer["attributebindings"] = instance.Flatten();
            buffer["informationbindings"] = System.Text.Json.JsonSerializer.Serialize(instance.GetInformationBindings(), jsonSerializerOptions);

            buffer["sourceIdentifier"] = instance.sourceIdentifier;
            SetShape(buffer, current.SHAPE());
            SetUsageBand(buffer, current.PLTS_COMP_SCALE()!.Value);
            return instance;
        }
    }
}

