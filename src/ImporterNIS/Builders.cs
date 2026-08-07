using ArcGIS.Core.Data;
using S100FC;
using S100FC.S101.ComplexAttributes;
using S100FC.S101.FeatureTypes;
using S100Framework.Applications.Singletons;
using System.Text.RegularExpressions;

namespace S100Framework.Applications
{
    using ArcGIS.Core.Data.UtilityNetwork.Trace;
    using ArcGIS.Core.Geometry;
    using NetTopologySuite.GeometriesGraph;
    using S100FC.S101.InformationTypes;
    using S100FC.S101.SimpleAttributes;
    using S100Framework.Applications.S57.esri;
    using S100Framework.Applications.S57auto.esri;
    using System.Runtime.CompilerServices;
    using YamlDotNet.Core.Tokens;
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
            { "M_NPUB", (current, buffer) => { return M_NPUB(current, buffer); } },
            { "M_NSYS", (current, buffer) => { return M_NSYS(current, buffer); } },
            //{ "M_QUAL", (current, buffer) => { return M_QUAL(current, buffer); } },
            { "M_SREL", (current, buffer) => { return M_SREL(current, buffer); } },
            { "M_VDAT", (current, buffer) => { return M_VDAT(current, buffer); } },
            { "DEPARE", (current, buffer) => { return DEPARE(current, buffer); } },
            { "UNSARE", (current, buffer) => { return UNSARE(current, buffer); } },
            { "DEPCNT", (current, buffer) => { return DEPCNT(current, buffer); } },
            { "SPRING", (current, buffer) => { return SPRING(current, buffer); } },
            { "OFSPLF", (current, buffer) => { return OFSPLF(current, buffer); } },
            { "CBLSUB", (current, buffer) => { return CBLSUB(current, buffer); } },
            { "LOGPON", (current, buffer) => { return LOGPON(current, buffer); } },
            { "CTRPNT", (current, buffer) => { return CTRPNT(current, buffer); } },
            { "GRIDRN", (current, buffer) => { return GRIDRN(current, buffer); } },
            { "SLCONS", (current, buffer) => { return SLCONS(current, buffer); } },
            { "COALNE", (current, buffer) => { return COALNE(current, buffer); } },
            { "CTNARE", (current, buffer) => { return CTNARE(current, buffer); } },
            { "FSHFAC", (current, buffer) => { return FSHFAC(current, buffer); } },
            { "OBSTRN", (current, buffer) => { return OBSTRN(current, buffer); } },
            { "WATTUR", (current, buffer) => { return WATTUR(current, buffer); } },
            { "WRECKS", (current, buffer) => { return WRECKS(current, buffer); } },
            { "OILBAR", (current, buffer) => { return OILBAR(current, buffer); } },
            //{ "UWTROC", (current, buffer) => { return UWTROC(current, buffer); } },
            { "LAKARE", (current, buffer) => { return LAKARE(current, buffer); } },
            { "LNDARE", (current, buffer) => { return LNDARE(current, buffer); } },
            { "LNDRGN", (current, buffer) => { return LNDRGN(current, buffer); } },
            { "RIVERS", (current, buffer) => { return RIVERS(current, buffer); } },
            { "SEAARE", (current, buffer) => { return SEAARE(current, buffer); } },
            { "SLOGRD", (current, buffer) => { return SLOGRD(current, buffer); } },
            { "VEGATN", (current, buffer) => { return VEGATN(current, buffer); } },
            
        };

        private static readonly Regex regexWaterwayDistance = new Regex(@"(Waterway distance =)\s(?<value>\d+)\s(?<unit>\D+)", RegexOptions.IgnoreCase);

        private static readonly Regex regexMaximumDraughtPermitted = new Regex(@"(Maximum draught permitted =)\s(?<value>\d+\.?\d*)", RegexOptions.IgnoreCase);

        private static readonly Regex regexVesselTrafficServiceArea = new Regex(@"(Vessel Traffic Service Area)", RegexOptions.IgnoreCase);

        private static readonly Regex regexPilotageDistrict = new Regex(@"(Pilotage District)", RegexOptions.IgnoreCase);

        private static readonly Regex regexMaritimeRescue = new Regex(@"(Maritime Rescue)", RegexOptions.IgnoreCase);

        private static readonly Regex regexCoordinationCentre = new Regex(@"(Coordination Centre)", RegexOptions.IgnoreCase);

        private static readonly Regex regexMarinePollutionRegulationsArea = new Regex(@"(Marine Pollution Regulations Area)", RegexOptions.IgnoreCase);

        private static readonly Regex regexVesselSpeedLimit = new Regex(@"(Speed limit is)\s(?<value>\d+)\s(?<unit>\D+)", RegexOptions.IgnoreCase);   //  Speed limit is 5 knots

        private static readonly Regex regexDam = new Regex(@"(Submerged weir)", RegexOptions.IgnoreCase);

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

            if (current.PLTS_COMP_SCALE_HasValue()) {
                string subtype = "";

                if (!Subtypes.Instance.TryGetSubtype(current.TableName(), current.FCSubtype(), out subtype))
                    throw new NotSupportedException($"Unknown subtype for {current.TableName}, {current.FCSubtype()}");

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

        private static Gridiron GRIDRN(Feature current, RowBuffer buffer) {
            var instance = new Gridiron();

            // TODO

            if (current.PLTS_COMP_SCALE_HasValue()) {
                string subtype = "";

                if (!Subtypes.Instance.TryGetSubtype(current.TableName(), current.FCSubtype(), out subtype))
                    throw new NotSupportedException($"Unknown subtype for {current.TableName}, {current.FCSubtype()}");

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

        private static Vegetation VEGATN(Feature current, RowBuffer buffer) {
            var instance = new Vegetation();

            if (current.CATVEG_HasValue()) {
                instance.categoryOfVegetation = EnumHelper.GetEnumValue(current.CATVEG());
            }

            if (current.ELEVAT_HasValue()) {
                instance.elevation = current.ELEVAT() != -32767m ?current.ELEVAT() : null;
            }

            var featureName = GetFeatureName(current.OBJNAM(), current.NOBJNM());
            if (featureName is not null)
                instance.featureName = featureName;

            if (current.HEIGHT_HasValue()) {
                instance.height = current.HEIGHT() != -32767m ? current.HEIGHT() : null;
            }

            if (current.VERLEN_HasValue()) {
                instance.verticalLength = current.VERLEN() != -32767m ? current.VERLEN() : null;
            }

            if (current.CONVIS_HasValue()) {
                instance.visualProminence = EnumHelper.GetEnumValue(current.CONVIS());
            }

            if (current.PLTS_COMP_SCALE_HasValue()) {
                string subtype = "";

                if (!Subtypes.Instance.TryGetSubtype(current.TableName(), current.FCSubtype(), out subtype))
                    throw new NotSupportedException($"Unknown subtype for {current.TableName}, {current.FCSubtype()}");

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

        private static SlopingGround SLOGRD(Feature current, RowBuffer buffer) {
            var instance = new SlopingGround();

            if (current.CATSLO_HasValue()) {
                if ((current.CATSLO() == 3 || current.CATSLO() == 4) && (!string.IsNullOrEmpty(current.NATSUR()) && "4".Equals(current.NATSUR())))
                    throw new NotImplementedException();    //  If it is required to encode a sand dune or sand hill, it must be done using the feature Sloping Ground with attribute category of slope = 3 (dune) or 4 (hill) and attribute nature of surface = 4 (sand). If these features are positioned along the coastline, a Coastline feature must also be encoded.

                instance.categoryOfSlope = EnumHelper.GetEnumValue(current.CATSLO());
            }

            var featureName = GetFeatureName(current.OBJNAM(), current.NOBJNM());
            if (featureName is not null)
                instance.featureName = featureName;

            if (current.COLOUR_HasValue()) {
                var colours = GetColours(current.COLOUR()!);
                if (colours is not null && colours.Any())
                    instance.colour = colours;
            }

            if (current.NATSUR_HasValue()) {
                var natureOfSurface = EnumHelper.GetEnumValues(current.NATSUR(), instance.attributeBindingDefinition("natureOfSurface")!.permitedValues!);
                if (natureOfSurface is not null && natureOfSurface.Any())
                    instance.natureOfSurface = natureOfSurface;
            }
            if (current.CONRAD_HasValue()) {
                instance.radarConspicuous = current.CONRAD() switch {
                    1 => true,      //  radar conspicuous
                    2 => false,     //  not radar conspicuous
                    4 => true,      //  radar conspicuous (has radar reflector)
                    -32767 => default,
                    _ => throw new NotImplementedException(),
                };
            }

            if (current.CONVIS_HasValue()) {
                instance.visualProminence = EnumHelper.GetEnumValue(current.CONVIS()!);
            }

            if (current.PLTS_COMP_SCALE_HasValue()) {
                string subtype = "";

                if (!Subtypes.Instance.TryGetSubtype(current.TableName(), current.FCSubtype(), out subtype))
                    throw new NotSupportedException($"Unknown subtype for {current.TableName}, {current.FCSubtype()}");

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

        private static SeaAreaNamedWaterArea SEAARE(Feature current, RowBuffer buffer) {
            var instance = new SeaAreaNamedWaterArea();

            if (current.CATSEA_HasValue()) {
                instance.categoryOfSeaArea = EnumHelper.GetEnumValue(current.CATSEA());
            }

            var featureName = GetFeatureName(current.OBJNAM(), current.NOBJNM());
            if (featureName is not null)
                instance.featureName = featureName;

            if (current.PLTS_COMP_SCALE_HasValue()) {
                string subtype = "";

                if (!Subtypes.Instance.TryGetSubtype(current.TableName(), current.FCSubtype(), out subtype))
                    throw new NotSupportedException($"Unknown subtype for {current.TableName}, {current.FCSubtype()}");

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

        private static River RIVERS(Feature current, RowBuffer buffer) {

            /* S-57 allows for RIVERS of geometric primitive area to be covered by the Group 1 objects LNDARE
               or UNSARE, however in S-101 all Rivers of geometric primitive area must be covered by the Skin
               of the Earth feature Land Area. During the automated conversion process, the converter may have
               the capability to convert UNSARE covering RIVERS to Land Area (taking into account the attribution
               of any adjoining LNDARE objects) and merge with any adjoining Land Area features. If the
               converter does not have this capability, Data Producers are advised to check their S-57 data
               holdings and amend their Group 1 coverage to have RIVERS of geometric primitive area covered
               by LNDARE (and merge with adjoining LNDARE as appropriate). */

            /* S-57 guidance recommends the encoding of intermittent lakes using an instance of the S-57 Object
               class RIVERS. Data Producers are advised to check all instances of RIVERS of geometric primitive
               area having attribute STATUS = 5 (periodic/intermittent) and if the real-world feature is a lake to
               amend to an instance of the S-101 Feature _s101type Lake (see S-101 DCEG clause 5.10). */

            var instance = new River();

            var featureName = GetFeatureName(current.OBJNAM(), current.NOBJNM());
            if (featureName is not null)
                instance.featureName = featureName;

            if (current.STATUS_HasValue()) {
                var status = EnumHelper.GetEnumValues(current.STATUS(), instance.attributeBindingDefinition("status")!.permitedValues!);
                if (status is not null && status.Any())
                    instance.status = status[0];
            }

            if (current.PLTS_COMP_SCALE_HasValue()) {
                string subtype = "";

                if (!Subtypes.Instance.TryGetSubtype(current.TableName(), current.FCSubtype(), out subtype))
                    throw new NotSupportedException($"Unknown subtype for {current.TableName}, {current.FCSubtype()}");

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

        private static LandRegion LNDRGN(Feature current, RowBuffer buffer) {
            var instance = new LandRegion();

            if (current.CATLND_HasValue()) {
                var categoryOfLandRegion = EnumHelper.GetEnumValues(current.CATLND());
                if (categoryOfLandRegion is not null && categoryOfLandRegion.Any())
                    instance.categoryOfLandRegion = categoryOfLandRegion;
            }

            var featureName = GetFeatureName(current.OBJNAM(), current.NOBJNM());
            if (featureName is not null)
                instance.featureName = featureName;

            if (current.NATSUR_HasValue()) {
                var natureOfSurface = EnumHelper.GetEnumValues(current.NATSUR(), instance.attributeBindingDefinition("natureOfSurface")!.permitedValues!);
                if (natureOfSurface is not null && natureOfSurface.Any())
                    instance.natureOfSurface = natureOfSurface;
            }

            if (current.WATLEV_HasValue()) {
                instance.waterLevelEffect = EnumHelper.GetEnumValue(current.WATLEV());
            }

            if (current.PLTS_COMP_SCALE_HasValue()) {
                string subtype = "";

                if (!Subtypes.Instance.TryGetSubtype(current.TableName(), current.FCSubtype(), out subtype))
                    throw new NotSupportedException($"Unknown subtype for {current.TableName}, {current.FCSubtype()}");

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

        private static LandArea LNDARE(Feature current, RowBuffer buffer) {
            var instance = new LandArea();

            if (current.CONDTN_HasValue()) {
                instance.condition = GetCondition(current.CONDTN()!.Value)?.value;
            }

            var featureName = GetFeatureName(current.OBJNAM(), current.NOBJNM());
            if (featureName is not null)
                instance.featureName = featureName;

            if (current.SORDAT_HasValue()) {
                if (DateHelper.TryConvertSordat(current.SORDAT(), out var reportedDate)) {
                    instance.reportedDate = reportedDate;
                }
                else {
                    Logger.Current.DataError(current.GetObjectID(), current.GetType().Name, current.LNAM() ?? "Unknown LNAM", $"Cannot convert date {current.SORDAT}");
                }
            }

            if (current.STATUS_HasValue()) {
                var status = EnumHelper.GetEnumValues(current.STATUS(), instance.attributeBindingDefinition("status")!.permitedValues!);
                if (status is not null && status.Any())
                    instance.status = status[0];
            }

            if (current.PLTS_COMP_SCALE_HasValue()) {
                string subtype = "";

                if (!Subtypes.Instance.TryGetSubtype(current.TableName(), current.FCSubtype(), out subtype))
                    throw new NotSupportedException($"Unknown subtype for {current.TableName}, {current.FCSubtype()}");

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

        private static Lake LAKARE(Feature current, RowBuffer buffer) {
            /*  S-57 allows for LAKARE to be covered by the Group 1 objects LNDARE or UNSARE, however in
                S-101 all Lake features must be covered by the Skin of the Earth feature Land Area. During the
                automated conversion process, the converter may have the capability to convert UNSARE covering
                LAKARE to Land Area (taking into account the attribution of any adjoining LNDARE objects) and
                merge with any adjoining Land Area features. If the converter does not have this capability, Data
                Producers are advised to check their S-57 data holdings and amend their Group 1 coverage to have
                LAKARE covered by LNDARE (and merge with adjoining LNDARE as appropriate). */

            var instance = new Lake();

            if (current.ELEVAT_HasValue()) {
                instance.elevation = current.ELEVAT() != -32767m ? current.ELEVAT() : null;
            }

            var featureName = GetFeatureName(current.OBJNAM(), current.NOBJNM());
            if (featureName is not null)
                instance.featureName = featureName;

            if (current.STATUS_HasValue()) {
                var status = EnumHelper.GetEnumValues(current.STATUS(), instance.attributeBindingDefinition("status")!.permitedValues!);
                if (status is not null && status.Any())
                    instance.status = status[0];
            }

            if (current.PLTS_COMP_SCALE_HasValue()) {
                string subtype = "";

                if (!Subtypes.Instance.TryGetSubtype(current.TableName(), current.FCSubtype(), out subtype))
                    throw new NotSupportedException($"Unknown subtype for {current.TableName}, {current.FCSubtype()}");

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

        private static UnderwaterAwashRock UWTROC(Feature current, RowBuffer buffer, QueryFilter filter) {
            var instance = new UnderwaterAwashRock();

            if (current.EXPSOU_HasValue()) {
                instance.expositionOfSounding = EnumHelper.GetEnumValue(current.EXPSOU());
            }

            var featureName = GetFeatureName(current.OBJNAM(), current.NOBJNM());
            if (featureName is not null)
                instance.featureName = featureName;

            if (current.NATSUR_HasValue()) {
                if (EnumHelper.GetEnumValue(current.NATSUR(), out int? natureOfSurface, instance.attributeBindingDefinition("natureOfSurface")!.permitedValues!))
                    instance.natureOfSurface = natureOfSurface;
            }

            if (current.QUASOU_HasValue()) {
                var qualityOfVerticalMeasurement = EnumHelper.GetEnumValues(current.QUASOU());
                if (qualityOfVerticalMeasurement is not null && qualityOfVerticalMeasurement.Any())
                    instance.qualityOfVerticalMeasurement = qualityOfVerticalMeasurement;
            }

            if (current.SORDAT_HasValue()) {
                if (DateHelper.TryConvertSordat(current.SORDAT(), out var reportedDate)) {
                    instance.reportedDate = reportedDate;
                }
                else {
                    Logger.Current.DataError(current.GetObjectID(), current.GetType().Name, current.LNAM() ?? "Unknown LNAM", $"Cannot convert date {current.SORDAT}");
                }
            }

            if (current.STATUS_HasValue()) {                
                var status = EnumHelper.GetEnumValues(current.STATUS(), instance.attributeBindingDefinition("status")!.permitedValues!);
                if (status is not null && status.Any())
                    instance.status = status[0];
            }

            if (current.TECSOU_HasValue()) {
                var techniqueOfVerticalMeasurement = EnumHelper.GetEnumValues(current.TECSOU(), instance.attributeBindingDefinition("techniqueOfVerticalMeasurement")!.permitedValues!);
                if (techniqueOfVerticalMeasurement is not null && techniqueOfVerticalMeasurement.Any())
                    instance.techniqueOfVerticalMeasurement = techniqueOfVerticalMeasurement;
            }

            if (current.VALSOU_HasValue()) {
                instance.valueOfSounding = current.VALSOU() != -32767m ? current.VALSOU() : null;
            }

            if (current.WATLEV_HasValue()) {
                if (EnumHelper.GetEnumValue(current.WATLEV(), out int? waterLevelEffect, instance.attributeBindingDefinition("waterLevelEffect")!.permitedValues!))
                    instance.waterLevelEffect = waterLevelEffect;
            }


            bool coveredByUnsurveyedArea = false;
            bool coveredByDredgedArea = false;
            decimal? leastDepth = null;

            var surrindingDepth = ImporterNIS.GetSurrunding_DepthArea(current.Shape()!, (Geodatabase)current.GetTable().GetDatastore(), filter.WhereClause);
            if (surrindingDepth.HasValue) {
                leastDepth = surrindingDepth.Value.DRVAL1.HasValue ? surrindingDepth.Value.DRVAL1.Value : null;
                if (surrindingDepth.Value.FcSubtype == 15) {  // UNSARE
                    coveredByUnsurveyedArea = true;
                }
                if (surrindingDepth.Value.FcSubtype == 5) {  // DRGARE
                    coveredByDredgedArea = true;
                    instance.surroundingDepth = leastDepth != -32767m ? leastDepth : null;
                }
                if (surrindingDepth.Value.FcSubtype == 1) {  // DEPARE
                    instance.surroundingDepth = leastDepth != -32767m ? leastDepth : null;
                }

                instance.surroundingDepth = leastDepth != -32767m ? leastDepth : null;
            }

            bool allCoveringDepthRangeMinimumValuesAreKnown = instance.surroundingDepth is not null;

            bool unknownDepthCoveredByUnsurveyedArea = coveredByUnsurveyedArea && (current.VALSOU_HasValue() && current.VALSOU() == -32767m);

            bool depthDredgedAreaWhereDepthMinimumValueIsUnknown = coveredByDredgedArea && !(instance.surroundingDepth is not null && instance.surroundingDepth.HasValue);

            bool expositionOfSoundingIsUnknown = current.EXPSOU() is -32767;

            if (allCoveringDepthRangeMinimumValuesAreKnown) {
                if (!(current.VALSOU_HasValue() && current.VALSOU() != -32767m)) {
                    if (current.EXPSOU_HasValue() && (current.EXPSOU() == 1 || current.EXPSOU() == 3) &&
                        (current.VALSOU_HasValue() && current.VALSOU() == -32767m) &&
                        (current.WATLEV_HasValue() && (current.WATLEV() == 3))) {

                        instance.defaultClearanceDepth = instance.surroundingDepth;
                    }
                    else if (((current.EXPSOU_HasValue() && current.EXPSOU() == 2) || expositionOfSoundingIsUnknown || (!current.EXPSOU_HasValue())) &&
                       (current.VALSOU_HasValue() && current.VALSOU() == -32767m) &&
                       (current.WATLEV_HasValue() && (current.WATLEV() == 3))) {

                        instance.defaultClearanceDepth = 0.1m;
                    }
                    else if (((current.EXPSOU_HasValue() && current.EXPSOU() == 2) || expositionOfSoundingIsUnknown || (!current.EXPSOU_HasValue())) &&
                       (current.VALSOU_HasValue() && current.VALSOU() == -32767m) &&
                       (current.WATLEV_HasValue() && (current.WATLEV() == 5))) {

                        instance.defaultClearanceDepth = 0m;
                    }
                    else if (((current.EXPSOU_HasValue() && current.EXPSOU() == 2) || expositionOfSoundingIsUnknown || (!current.EXPSOU_HasValue())) &&
                       (current.VALSOU_HasValue() && current.VALSOU() == -32767m) &&
                       (current.WATLEV_HasValue() && (current.WATLEV() == 4 || current.WATLEV() == -32767m))) {

                        instance.defaultClearanceDepth = -15m;
                    }
                    else {
                        ;// Logger.Current.DataError(current.OBJECTID.Value, tableName, longname, $"Cannot convert defaultCleareanceDepth for underwater awash rock. Check S-101 Annex - A.");
                    }
                }
            }
            else if (unknownDepthCoveredByUnsurveyedArea || depthDredgedAreaWhereDepthMinimumValueIsUnknown) {
                if ((current.VALSOU_HasValue() && current.VALSOU() == -32767m) &&
                   (current.WATLEV_HasValue() && (current.WATLEV() == 3))) {
                    instance.defaultClearanceDepth = 0.1m;
                }
                else if ((current.VALSOU_HasValue() && current.VALSOU() == -32767m) &&
                   (current.WATLEV_HasValue() && (current.WATLEV() == 5))) {
                    instance.defaultClearanceDepth = 0m;
                }
                else if ((current.VALSOU_HasValue() && current.VALSOU() == -32767m) &&
                        (current.WATLEV_HasValue() && (current.WATLEV() == 4 || current.WATLEV() == -32767m))) {
                    instance.defaultClearanceDepth = -15m;
                }
                else {
                    ;// Logger.Current.DataError(current.OBJECTID.Value, tableName, longname, $"Cannot convert defaultCleareanceDepth for underwater awash rock. Check S-101 Annex - A.");
                }

            }
            else {
                Logger.Current.DataError(current.GetObjectID(), current.TableName(), current.LNAM() ?? "Unknown LNAM", $"Cannot set default clearance depth. Check loader.");
            }

            if (!instance.valueOfSounding.HasValue && instance.attributeBindings.Count(e => e.S100FC_code.Equals("defaultClearanceDepth")) == 0) {
                Logger.Current.Error("!instance.valueOfSounding.HasValue && !defaultClearanceDepth (OID:{objectid})", current.OBJECTID);
            }

            if (current.PLTS_COMP_SCALE_HasValue()) {
                string subtype = "";

                if (!Subtypes.Instance.TryGetSubtype(current.TableName(), current.FCSubtype(), out subtype))
                    throw new NotSupportedException($"Unknown subtype for {current.TableName}, {current.FCSubtype()}");

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

        private static OilBarrier OILBAR(Feature current, RowBuffer buffer) {
            var instance = new OilBarrier();

            var featureName = GetFeatureName(current.OBJNAM(), current.NOBJNM());
            if (featureName is not null)
                instance.featureName = featureName;

            if (current.CATOLB_HasValue()) {
                instance.categoryOfOilBarrier = EnumHelper.GetEnumValue(current.CATOLB());
            }

            if (current.CONDTN_HasValue()) {
                instance.condition = GetCondition(current.CONDTN()!.Value)?.value;
            }

            DateHelper.TryGetFixedDateRange(current.DATSTA(), current.DATEND(), out var dateRange);
            if (dateRange != default) {
                instance.fixedDateRange = dateRange;
            }

            if (current.SORDAT_HasValue()) {
                if (DateHelper.TryConvertSordat(current.SORDAT(), out var reportedDate)) {
                    instance.reportedDate = reportedDate;
                }
                else {
                    Logger.Current.DataError(current.GetObjectID(), current.GetType().Name, current.LNAM() ?? "Unknown LNAM", $"Cannot convert date {current.SORDAT}");
                }
            }

            if (current.STATUS_HasValue()) {
                var status = EnumHelper.GetEnumValues(current.STATUS(), instance.attributeBindingDefinition("status")!.permitedValues!);
                if (status is not null && status.Any())
                    instance.status = status;
            }

            if (current.PLTS_COMP_SCALE_HasValue()) {
                string subtype = "";

                if (!Subtypes.Instance.TryGetSubtype(current.TableName(), current.FCSubtype(), out subtype))
                    throw new NotSupportedException($"Unknown subtype for {current.TableName}, {current.FCSubtype()}");

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

        private static Wreck WRECKS(Feature current, RowBuffer buffer) {
            var instance = new Wreck();

            // action point #42 Attributes converted correctly but the combination of both is prohibited in S-101 (DCEG 13.5). Ignore/ drop CATWRK when VALSOU is populated on conversion.
            if (current.CATWRK_HasValue() && !current.VALSOU_HasValue()) {
                instance.categoryOfWreck = EnumHelper.GetEnumValue(current.CATWRK());
            }

            if (current.EXPSOU_HasValue()) {
                instance.expositionOfSounding = EnumHelper.GetEnumValue(current.EXPSOU());
            }

            var featureName = GetFeatureName(current.OBJNAM(), current.NOBJNM());
            if (featureName is not null)
                instance.featureName = featureName;

            if (current.HEIGHT_HasValue()) {
                instance.height = current.HEIGHT() != -32767m ? current.HEIGHT() : null;
            }

            if (current.QUASOU_HasValue()) {
                var qualityOfVerticalMeasurement = EnumHelper.GetEnumValues(current.QUASOU());
                if (qualityOfVerticalMeasurement is not null && qualityOfVerticalMeasurement.Any())
                    instance.qualityOfVerticalMeasurement = qualityOfVerticalMeasurement;
            }

            if (current.CONRAD_HasValue()) {
                instance.radarConspicuous = current.CONRAD() == 2 ? false : true;
            }
            if (current.SORDAT_HasValue()) {
                if (DateHelper.TryConvertSordat(current.SORDAT(), out var reportedDate)) {
                    instance.reportedDate = reportedDate;
                }
                else {
                    Logger.Current.DataError(current.GetObjectID(), current.GetType().Name, current.LNAM() ?? "Unknown LNAM", $"Cannot convert date {current.SORDAT}");
                }
            }

            if (current.STATUS_HasValue()) {
                var status = EnumHelper.GetEnumValues(current.STATUS(), instance.attributeBindingDefinition("status")!.permitedValues!);
                if (status is not null && status.Any())
                    instance.status = status;
            }

            if (current.TECSOU_HasValue()) {
                var techniqueOfVerticalMeasurement = EnumHelper.GetEnumValues(current.TECSOU(), instance.attributeBindingDefinition("techniqueOfVerticalMeasurement")!.permitedValues!);
                if (techniqueOfVerticalMeasurement is not null && techniqueOfVerticalMeasurement.Any())
                    instance.techniqueOfVerticalMeasurement = techniqueOfVerticalMeasurement;
            }

            if (current.VALSOU_HasValue()) {
                instance.valueOfSounding = current.VALSOU() != -32767m ? current.VALSOU() : null;
            }

            if (current.CONVIS_HasValue()) {
                instance.visualProminence = EnumHelper.GetEnumValue(current.CONVIS());
            }

            if (current.WATLEV_HasValue()) {
                instance.waterLevelEffect = EnumHelper.GetEnumValue(current.WATLEV());
            }

            if (current.PLTS_COMP_SCALE_HasValue()) {
                string subtype = "";

                if (!Subtypes.Instance.TryGetSubtype(current.TableName(), current.FCSubtype(), out subtype))
                    throw new NotSupportedException($"Unknown subtype for {current.TableName}, {current.FCSubtype()}");

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

        private static WaterTurbulence WATTUR(Feature current, RowBuffer buffer) {
            var instance = new WaterTurbulence();

            // action point #42 Attributes converted correctly but the combination of both is prohibited in S-101 (DCEG 13.5). Ignore/ drop CATWRK when VALSOU is populated on conversion.
            if (current.CATWAT_HasValue()) {
                instance.categoryOfWaterTurbulence = EnumHelper.GetEnumValue(current.CATWAT());
            }

            var featureName = GetFeatureName(current.OBJNAM(), current.NOBJNM());
            if (featureName is not null)
                instance.featureName = featureName;

            if (current.PLTS_COMP_SCALE_HasValue()) {
                string subtype = "";

                if (!Subtypes.Instance.TryGetSubtype(current.TableName(), current.FCSubtype(), out subtype))
                    throw new NotSupportedException($"Unknown subtype for {current.TableName}, {current.FCSubtype()}");

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

        private static FeatureType OBSTRN(Feature current, RowBuffer buffer) {
            if (!string.IsNullOrEmpty(current.INFORM()) && regexDam.IsMatch(current.INFORM()!)) {
                var instance = new Dam();

                if (current.CONDTN_HasValue()) {
                    instance.condition = GetCondition(current.CONDTN()!.Value)?.value;
                }

                var featureName = GetFeatureName(current.OBJNAM(), current.NOBJNM());
                if (featureName is not null)
                    instance.featureName = featureName;

                DateHelper.TryGetFixedDateRange(current.DATSTA(), current.DATEND(), out var dateRange);
                if (dateRange != default) {
                    instance.fixedDateRange = dateRange;
                }
                if (current.HEIGHT_HasValue()) {
                    instance.height = current.HEIGHT() != -32767m ? current.HEIGHT() : null;
                }

                if (current.NATCON_HasValue()) {
                    var natureOfConstruction = EnumHelper.GetEnumValues(current.NATCON);
                    if (natureOfConstruction is not null && natureOfConstruction.Any())
                        instance.natureOfConstruction = natureOfConstruction;
                }

                if (current.CONRAD_HasValue()) {
                    instance.radarConspicuous = current.CONRAD() == 2 ? false : true;
                }

                if (current.STATUS_HasValue()) {
                    var status = EnumHelper.GetEnumValues(current.STATUS(), instance.attributeBindingDefinition("status")!.permitedValues!);
                    if (status is not null && status.Any())
                        instance.status = status;
                }

                if (current.VERLEN_HasValue()) {
                    instance.verticalLength = current.VERLEN() != -32767m ? current.VERLEN() : null;
                }

                if (current.CONVIS_HasValue()) {
                    instance.visualProminence = EnumHelper.GetEnumValue(current.CONVIS()!);
                }

                if (current.PLTS_COMP_SCALE_HasValue()) {
                    string subtype = "";

                    if (!Subtypes.Instance.TryGetSubtype(current.TableName(), current.FCSubtype(), out subtype))
                        throw new NotSupportedException($"Unknown subtype for {current.TableName}, {current.FCSubtype()}");

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
            else if (current.CATOBS_HasValue() && current.CATOBS() == 7) {
                var instance = new FoulGround();

                var featureName = GetFeatureName(current.OBJNAM(), current.NOBJNM());
                if (featureName is not null)
                    instance.featureName = featureName;

                if (current.QUASOU_HasValue()) {
                    var qualityOfVerticalMeasurement = EnumHelper.GetEnumValues(current.QUASOU());
                    if (qualityOfVerticalMeasurement is not null && qualityOfVerticalMeasurement.Any())
                        instance.qualityOfVerticalMeasurement = qualityOfVerticalMeasurement;
                }
                if (current.SORDAT_HasValue()) {
                    if (DateHelper.TryConvertSordat(current.SORDAT()!, out var reportedDate)) {
                        instance.reportedDate = reportedDate;
                    }
                    else {
                        Logger.Current.DataError(current.GetObjectID(), current.GetType().Name, current.LNAM() ?? "Unknown LNAM", $"Cannot convert date {current.SORDAT}");
                    }
                }

                if (current.STATUS_HasValue()) {
                    var status = EnumHelper.GetEnumValues(current.STATUS(), instance.attributeBindingDefinition("status")!.permitedValues!);
                    if (status is not null && status.Any())
                        instance.status = status;
                }

                if (current.TECSOU_HasValue()) {
                    var techniqueOfVerticalMeasurement = EnumHelper.GetEnumValues(current.TECSOU(), instance.attributeBindingDefinition("techniqueOfVerticalMeasurement")!.permitedValues!);
                    if (techniqueOfVerticalMeasurement is not null && techniqueOfVerticalMeasurement.Any())
                        instance.techniqueOfVerticalMeasurement = techniqueOfVerticalMeasurement;
                }

                if (current.VALSOU_HasValue()) {
                    instance.valueOfSounding = current.VALSOU() != -32767m ? current.VALSOU() : null;
                }

                if (current.SOUACC_HasValue()) {
                    instance.verticalUncertainty = new() {
                        uncertaintyFixed = current.SOUACC() != -32767m ? current.SOUACC() : null,
                    };
                }

                if (current.PLTS_COMP_SCALE_HasValue()) {
                    string subtype = "";

                    if (!Subtypes.Instance.TryGetSubtype(current.TableName(), current.FCSubtype(), out subtype))
                        throw new NotSupportedException($"Unknown subtype for {current.TableName}, {current.FCSubtype()}");

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
                var instance = new Obstruction();

                if (current.CATOBS_HasValue()) {
                    instance.categoryOfObstruction = EnumHelper.GetEnumValue(current.CATOBS());
                }

                if (current.CONDTN_HasValue()) {
                    instance.condition = ImporterNIS.GetCondition(current.CONDTN()!.Value)?.value;
                }

                if (current.EXPSOU_HasValue()) {
                    instance.expositionOfSounding = EnumHelper.GetEnumValue(current.EXPSOU());
                }

                var featureName = GetFeatureName(current.OBJNAM(), current.NOBJNM());
                if (featureName is not null)
                    instance.featureName = featureName;

                if (current.HEIGHT_HasValue()) {
                    instance.height = current.HEIGHT() != -32767m ? current.HEIGHT() : null;
                }

                var inform = current.INFORM();
                if (!string.IsNullOrEmpty(inform) && regexMaximumDraughtPermitted.IsMatch(inform)) {
                    var _value = regexMaximumDraughtPermitted.Match(inform).Groups["value"]?.Value;

                    if (decimal.TryParse(_value, out decimal value)) {
                        instance.maximumPermittedDraught = value;
                    }
                }

                if (current.NATSUR_HasValue()) {
                    var natureOfSurface = EnumHelper.GetEnumValues(current.NATSUR(), instance.attributeBindingDefinition("natureOfSurface")!.permitedValues!);
                    if (natureOfSurface is not null && natureOfSurface.Any())
                        instance.natureOfSurface = natureOfSurface;
                }

                if (current.PRODCT_HasValue()) {
                    var product = EnumHelper.GetEnumValues(current.PRODCT());
                    if (product is not null && product.Any())
                        instance.product = product;
                }

                // TODO: QualityOfVerticalMeasurement
                if (current.SORDAT_HasValue()) {
                    if (DateHelper.TryConvertSordat(current.SORDAT(), out var reportedDate)) {
                        instance.reportedDate = reportedDate;
                    }
                    else {
                        Logger.Current.DataError(current.GetObjectID(), current.GetType().Name, current.LNAM() ?? "Unknown LNAM", $"Cannot convert date {current.SORDAT}");
                    }
                }

                if (current.STATUS_HasValue()) {
                    var status = EnumHelper.GetEnumValues(current.STATUS(), instance.attributeBindingDefinition("status")!.permitedValues!);
                    if (status is not null && status.Any())
                        instance.status = status;
                }

                if (current.TECSOU_HasValue()) {
                    var techniqueOfVerticalMeasurement = EnumHelper.GetEnumValues(current.TECSOU(), instance.attributeBindingDefinition("techniqueOfVerticalMeasurement")!.permitedValues!);
                    if (techniqueOfVerticalMeasurement is not null && techniqueOfVerticalMeasurement.Any())
                        instance.techniqueOfVerticalMeasurement = techniqueOfVerticalMeasurement;
                }

                if (current.VALSOU_HasValue()) {
                    instance.valueOfSounding = current.VALSOU() != -32767m ? current.VALSOU() : null;
                }

                if (current.VERLEN_HasValue()) {
                    instance.verticalLength = current.VERLEN() != -32767m ? current.VERLEN() : null;
                }

                if (current.WATLEV_HasValue()) {
                    instance.waterLevelEffect = EnumHelper.GetEnumValue(current.WATLEV());
                }

                if (current.PLTS_COMP_SCALE_HasValue()) {
                    string subtype = "";

                    if (!Subtypes.Instance.TryGetSubtype(current.TableName(), current.FCSubtype(), out subtype))
                        throw new NotSupportedException($"Unknown subtype for {current.TableName}, {current.FCSubtype()}");

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

        private static FishingFacility FSHFAC(Feature current, RowBuffer buffer) {
            var instance = new FishingFacility();

            if (current.CATFIF_HasValue()) {
                instance.categoryOfFishingFacility = EnumHelper.GetEnumValue(current.CATFIF());
            }

            if (current.CONDTN_HasValue()) {
                instance.condition = GetCondition(current.CONDTN()!.Value)?.value;
            }

            var featureName = GetFeatureName(current.OBJNAM(), current.NOBJNM());
            if (featureName is not null)
                instance.featureName = featureName;

            DateHelper.TryGetPeriodicDateRange(current.PERSTA(), current.PEREND(), out var periodicDateRange);
            if (periodicDateRange != default) {
                instance.periodicDateRange = periodicDateRange;
            }
            if (current.SORDAT_HasValue()) {
                if (DateHelper.TryConvertSordat(current.SORDAT(), out var reportedDate)) {
                    instance.reportedDate = reportedDate;
                }
                else {
                    Logger.Current.DataError(current.GetObjectID(), current.GetType().Name, current.LNAM() ?? "Unknown LNAM", $"Cannot convert date {current.SORDAT}");
                }
            }

            if (current.STATUS_HasValue()) {
                var status = EnumHelper.GetEnumValues(current.STATUS(), instance.attributeBindingDefinition("status")!.permitedValues!);
                if (status is not null && status.Any())
                    instance.status = status;
            }

            if (current.VERLEN_HasValue()) {
                instance.verticalLength = current.VERLEN() != -32767m ? current.VERLEN() : null;
            }

            if (current.PLTS_COMP_SCALE_HasValue()) {
                string subtype = "";

                if (!Subtypes.Instance.TryGetSubtype(current.TableName(), current.FCSubtype(), out subtype))
                    throw new NotSupportedException($"Unknown subtype for {current.TableName}, {current.FCSubtype()}");

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

        private static CautionArea CTNARE(Feature current, RowBuffer buffer) {
            var instance = new CautionArea();


            if (current.CONDTN_HasValue()) {
                instance.condition = GetCondition(current.CONDTN()!.Value)?.value;
            }

            DateHelper.TryGetFixedDateRange(current.DATSTA(), current.DATEND(), out var dateRange);
            if (dateRange != default) {
                instance.fixedDateRange = dateRange;
            }

            DateHelper.TryGetPeriodicDateRange(current.PERSTA(), current.PEREND(), out var periodicDateRange);
            if (periodicDateRange != default) {
                instance.periodicDateRange = periodicDateRange;
            }

            if (current.SORDAT_HasValue()) {
                if (DateHelper.TryConvertSordat(current.SORDAT(), out var reportedDate)) {
                    instance.reportedDate = reportedDate;
                }
                else {
                    Logger.Current.DataError(current.GetObjectID(), current.GetType().Name, current.LNAM() ?? "Unknown LNAM", $"Cannot convert date {current.SORDAT}");
                }
            }

            if (current.PLTS_COMP_SCALE_HasValue()) {
                string subtype = "";

                if (!Subtypes.Instance.TryGetSubtype(current.TableName(), current.FCSubtype(), out subtype))
                    throw new NotSupportedException($"Unknown subtype for {current.TableName}, {current.FCSubtype()}");

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


        private static Coastline COALNE(Feature current, RowBuffer buffer) {
            var instance = new Coastline();

            if (current.CATCOA_HasValue()) {
                if (EnumHelper.GetEnumValue(current.CATCOA(), out int? categoryOfCoastline, instance.attributeBindingDefinition("categoryOfCoastline")!.permitedValues!))
                    instance.categoryOfCoastline = categoryOfCoastline;
            }

            if (current.COLOUR_HasValue()) {
                var colour = GetColours(current.COLOUR());
                if (colour is not null && colour.Any())
                    instance.colour = colour;
            }

            if (current.ELEVAT_HasValue()) {
                instance.elevation = current.ELEVAT() != -32767m ? current.ELEVAT() : null;
            }

            var featureName = GetFeatureName(current.OBJNAM(), current.NOBJNM());
            if (featureName is not null)
                instance.featureName = featureName;

            /*
                • The attribute nature of surface has been included as an allowable attribute for Coastline in S-101.
                During the automated conversion process, the following COALNE/CATCOA encoding instances will
                be converted to the corresponding Coastline/nature of surface instances.
                CATCOA = 3 (sandy shore) -> nature of surface = 4 (sand)
                CATCOA = 4 (stony shore) -> nature of surface = 5 (stone)
                CATCOA = 5 (shingly shore) -> nature of surface = 7 (pebbles)
                CATCOA = 9 (coral reef) -> nature of surface = 14 (coral)
                CATCOA = 11 (shelly shore) -> nature of surface = 17 (shells)
            */
            if (current.CATCOA_HasValue()) {
                natureOfSurface? e = current.CATCOA() switch {
                    3 => 4, //natureOfSurface.Sand,
                    4 => 5, //natureOfSurface.Stone,
                    5 => 7, //natureOfSurface.Pebbles,
                    9 => 14, //natureOfSurface.Coral,
                    11 => 17,   //natureOfSurface.Shells,
                    -32767 => null,
                    _ => null //lthrow new IndexOutOfRangeException($"catcoa to natureOfSurface: {catcoa}")
                };
                if (e is not null) {
                    instance.natureOfSurface = [e.value];

                }
            }

            if (current.CONRAD_HasValue()) {
                instance.radarConspicuous = current.CONRAD() == 2 ? false : true;
            }

            if (current.CONVIS_HasValue()) {
                instance.visualProminence = EnumHelper.GetEnumValue(current.CONVIS());
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

        private static ShorelineConstruction SLCONS(Feature current, RowBuffer buffer) {
            var instance = new ShorelineConstruction();

            if (current.CATSLC_HasValue()) {
                instance.categoryOfShorelineConstruction = EnumHelper.GetEnumValue(current.CATSLC());
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
                instance.condition = GetCondition(current.CONDTN()!.Value)?.value;
            }

            var featureName = GetFeatureName(current.OBJNAM(), current.NOBJNM());
            if (featureName is not null)
                instance.featureName = featureName;

            DateHelper.TryGetFixedDateRange(current.DATSTA(), current.DATEND(), out var dateRange);
            if (dateRange != default) {
                instance.fixedDateRange = dateRange;
            }

            if (current.HEIGHT_HasValue()) {
                instance.height = current.HEIGHT() != -32767m ? current.HEIGHT() : null;
            }
            else {

            }

            var horclr = current.HORCLR() ?? default;
            var horacc = current.HORACC() ?? default;

            if (horclr != default) {
                instance.horizontalClearanceFixed = new() {
                    horizontalClearanceValue = horclr,
                    horizontalDistanceUncertainty = horacc,
                };
            }

            if (current.HORLEN_HasValue()) {
                instance.horizontalLength = current.HORLEN() != -32767m ? current.HORLEN() : null;
            }

            if (current.HORWID_HasValue()) {
                instance.horizontalWidth = current.HORWID() != -32767m ? current.HORWID() : null;
            }

            if (current.NATCON_HasValue()) {
                var natureOfConstruction = EnumHelper.GetEnumValues(current.NATCON());
                if (natureOfConstruction is not null && natureOfConstruction.Any())
                    instance.natureOfConstruction = natureOfConstruction;
            }

            if (current.CONRAD_HasValue()) {
                instance.radarConspicuous = current.CONRAD() == 2 ? false : true;
            }
            if (current.SORDAT_HasValue()) {
                if (DateHelper.TryConvertSordat(current.SORDAT()!, out var reportedDate)) {
                    instance.reportedDate = reportedDate;
                }
                else {
                    Logger.Current.DataError(current.GetObjectID(), current.GetType().Name, current.LNAM() ?? "Unknown LNAM", $"Cannot convert date {current.SORDAT}");
                }
            }

            if (current.STATUS_HasValue()) {
                var status = EnumHelper.GetEnumValues(current.STATUS(), instance.attributeBindingDefinition("status")!.permitedValues!);
                if (status is not null && status.Any())
                    instance.status = status;
            }

            if (current.VERLEN_HasValue()) {
                instance.verticalLength = current.VERLEN() != -32767m ? current.VERLEN() : null;
            }

            if (current.CONVIS_HasValue()) {
                instance.visualProminence = EnumHelper.GetEnumValue(current.CONVIS());
            }

            if (current.WATLEV_HasValue()) {
                instance.waterLevelEffect = EnumHelper.GetEnumValue(current.WATLEV());
            }

            if (current.PLTS_COMP_SCALE_HasValue()) {
                string subtype = "";

                if (!Subtypes.Instance.TryGetSubtype(current.TableName(), current.FCSubtype(), out subtype))
                    throw new NotSupportedException($"Unknown subtype for {current.TableName}, {current.FCSubtype()}");

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

            if (current.BURDEP_HasValue()) {
                instance.buriedDepth = current.BURDEP() != -32767m ? current.BURDEP() : null;
            }

            if (current.CATPIP_HasValue()) {
                var categoryOfPipelinePipe = EnumHelper.GetEnumValues(current.CATPIP());
                if (categoryOfPipelinePipe is not null && categoryOfPipelinePipe.Any())
                    instance.categoryOfPipelinePipe = categoryOfPipelinePipe;
            }

            if (current.CONDTN_HasValue()) {
                instance.condition = GetCondition(current.CONDTN()!.Value)?.value;
            }

            if (current.DRVAL1_HasValue()) {
                instance.depthRangeMinimumValue = current.DRVAL1() != -32767m ? current.DRVAL1() : null;
            }

            if (current.DRVAL2_HasValue()) {
                instance.depthRangeMaximumValue = current.DRVAL2() != -32767m ? current.DRVAL2() : null;
            }

            var featureName = GetFeatureName(current.OBJNAM(), current.NOBJNM());
            if (featureName is not null)
                instance.featureName = featureName;

            DateHelper.TryGetFixedDateRange(current.DATSTA(), current.DATEND(), out var dateRange);
            if (dateRange != default) {
                instance.fixedDateRange = dateRange;
            }

            // TODO: multiplicityOfFeatures

            if (current.PRODCT_HasValue()) {
                var product = EnumHelper.GetEnumValues(current.PRODCT());
                if (product is not null && product.Any())
                    instance.product = product;
            }
            if (current.SORDAT_HasValue()) {
                if (DateHelper.TryConvertSordat(current.SORDAT()!, out var reportedDate)) {
                    instance.reportedDate = reportedDate;
                }
                else {
                    Logger.Current.DataError(current.GetObjectID(), current.GetType().Name, current.LNAM() ?? "Unknown LNAM", $"Cannot convert date {current.SORDAT}");
                }
            }

            // TODO: restriction

            if (current.STATUS_HasValue()) {
                var status = EnumHelper.GetEnumValues(current.STATUS(), instance.attributeBindingDefinition("status")!.permitedValues!);
                if (status is not null && status.Any())
                    instance.status = status;
            }

            if (current.PLTS_COMP_SCALE_HasValue()) {
                string subtype = "";

                if (!Subtypes.Instance.TryGetSubtype(current.TableName(), current.FCSubtype(), out subtype))
                    throw new NotSupportedException($"Unknown subtype for {current.TableName}, {current.FCSubtype()}");

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

            if (current.PLTS_COMP_SCALE_HasValue()) {
                string subtype = "";

                if (!Subtypes.Instance.TryGetSubtype(current.TableName(), current.FCSubtype(), out subtype))
                    throw new NotSupportedException($"Unknown subtype for {current.TableName}, {current.FCSubtype()}");

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

            if (current.PLTS_COMP_SCALE_HasValue()) {
                string subtype = "";

                if (!Subtypes.Instance.TryGetSubtype(current.TableName(), current.FCSubtype(), out subtype))
                    throw new NotSupportedException($"Unknown subtype for {current.TableName}, {current.FCSubtype()}");

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

        private static Spring SPRING(Feature current, RowBuffer buffer) {
            var instance = new Spring();

            // TODO

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

        private static LogPond LOGPON(Feature current, RowBuffer buffer) {
            var instance = new LogPond();

            // TODO

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

        private static Landmark CTRPNT(Feature current, RowBuffer buffer) {
            var instance = new Landmark();

            /*
                When converting the S-57 CTRPNT Object class the S-101 mandatory attribute visual prominence on the 
                converted Landmark feature will be populated during the automated conversion process 
                with value 2 (not visually conspicuous). Data Producers will be required to amend this value as appropriate.
             */
            instance.visualProminence = 2;

            if (current.CATCTR_HasValue()) {
                var categoryOfLandmark = EnumHelper.GetEnumValues(current.CATCTR());
                if (categoryOfLandmark is not null)
                    instance.categoryOfLandmark = categoryOfLandmark;
            }

            if (current.PLTS_COMP_SCALE_HasValue()) {
                string subtype = "";

                if (!Subtypes.Instance.TryGetSubtype(current.TableName(), current.FCSubtype(), out subtype))
                    throw new NotSupportedException($"Unknown subtype for {current.TableName}, {current.FCSubtype()}");

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

            if (current.PLTS_COMP_SCALE_HasValue()) {
                string subtype = "";

                if (!Subtypes.Instance.TryGetSubtype(current.TableName(), current.FCSubtype(), out subtype))
                    throw new NotSupportedException($"Unknown subtype for {current.TableName}, {current.FCSubtype()}");

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
                var status = EnumHelper.GetEnumValues(current.STATUS(), instance.attributeBindingDefinition("status")!.permitedValues!);
                if (status is not null && status.Any())
                    instance.status = status;
            }

            if (current.PLTS_COMP_SCALE_HasValue()) {
                string subtype = "";

                if (!Subtypes.Instance.TryGetSubtype(current.TableName(), current.FCSubtype(), out subtype))
                    throw new NotSupportedException($"Unknown subtype for {current.TableName}, {current.FCSubtype()}");

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

            if (current.NATION_HasValue()) {
                instance.nationality = [GetNation(current.NATION()!)];
            }

            if (current.PLTS_COMP_SCALE_HasValue()) {
                string subtype = "";

                if (!Subtypes.Instance.TryGetSubtype(current.TableName(), current.FCSubtype(), out subtype))
                    throw new NotSupportedException($"Unknown subtype for {current.TableName}, {current.FCSubtype()}");

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

            if (current.PLTS_COMP_SCALE_HasValue()) {
                string subtype = "";

                if (!Subtypes.Instance.TryGetSubtype(current.TableName(), current.FCSubtype(), out subtype))
                    throw new NotSupportedException($"Unknown subtype for {current.TableName}, {current.FCSubtype()}");

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

            if (current.PLTS_COMP_SCALE_HasValue()) {
                string subtype = "";

                if (!Subtypes.Instance.TryGetSubtype(current.TableName(), current.FCSubtype(), out subtype))
                    throw new NotSupportedException($"Unknown subtype for {current.TableName}, {current.FCSubtype()}");

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

            if (current.NATION_HasValue()) {
                instance.nationality = GetNation(current.NATION()!);
            }

            if (current.STATUS_HasValue()) {
                var status = EnumHelper.GetEnumValues(current.STATUS(), instance.attributeBindingDefinition("status")!.permitedValues!);
                if (status is not null && status.Any())
                    instance.status = status;
            }

            if (current.PLTS_COMP_SCALE_HasValue()) {
                string subtype = "";

                if (!Subtypes.Instance.TryGetSubtype(current.TableName(), current.FCSubtype(), out subtype))
                    throw new NotSupportedException($"Unknown subtype for {current.TableName}, {current.FCSubtype()}");

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
                var status = EnumHelper.GetEnumValues(current.STATUS(), instance.attributeBindingDefinition("status")!.permitedValues!);
                if (status is not null && status.Any())
                    instance.status = status;
            }

            if (current.PLTS_COMP_SCALE_HasValue()) {
                string subtype = "";

                if (!Subtypes.Instance.TryGetSubtype(current.TableName(), current.FCSubtype(), out subtype))
                    throw new NotSupportedException($"Unknown subtype for {current.TableName}, {current.FCSubtype()}");

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
            if (current.SORDAT_HasValue()) {
                if (DateHelper.TryConvertSordat(current.SORDAT()!, out var reportedDate)) {
                    instance.reportedDate = reportedDate;
                }
                else {
                    Logger.Current.DataError(current.GetObjectID(), current.GetType().Name, current.LNAM() ?? "Unknown LNAM", $"Cannot convert date {current.SORDAT}");
                }
            }

            if (current.STATUS_HasValue()) {
                var status = EnumHelper.GetEnumValues(current.STATUS(), instance.attributeBindingDefinition("status")!.permitedValues!);
                if (status is not null && status.Any())
                    instance.status = status;
            }

            if (current.VERLEN_HasValue()) {
                instance.verticalLength = current.VERLEN() != -32767m ? current.VERLEN()!.Value : null;
            }

            if (current.CONVIS_HasValue()) {
                instance.visualProminence = EnumHelper.GetEnumValue(current.CONVIS());
            }

            if (current.PLTS_COMP_SCALE_HasValue()) {
                string subtype = "";

                if (!Subtypes.Instance.TryGetSubtype(current.TableName(), current.FCSubtype(), out subtype))
                    throw new NotSupportedException($"Unknown subtype for {current.TableName}, {current.FCSubtype()}");

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

        private static CableSubmarine CBLSUB(Feature current, RowBuffer buffer) {
            var instance = new CableSubmarine();

            if (current.BURDEP_HasValue()) {
                instance.buriedDepth = current.BURDEP() != -32767m ? current.BURDEP() : null;
            }

            if (current.CATCBL_HasValue()) {
                if (current.CATCBL() == 4) {
                    /*
                        S-65 Annex B
                        Amended guidance for conversion of CATCBL value 4 (telephone) to convert
                        to new value category of cable = 10 (telecommunications cable).
                        11.5.1, 11.5.3, A-2
                     */

                    instance.categoryOfCable = 10; //categoryOfCable.TelecommunicationsCable;
                }
                else {
                    instance.categoryOfCable = EnumHelper.GetEnumValue(current.CATCBL());
                }
            }

            if (current.CONDTN_HasValue()) {
                instance.condition = GetCondition(current.CONDTN()!.Value)?.value;
            }

            var featureName = GetFeatureName(current.OBJNAM(), current.NOBJNM());
            if (featureName is not null)
                instance.featureName = featureName;

            DateHelper.TryGetFixedDateRange(current.DATSTA(), current.DATEND(), out var dateRange);
            if (dateRange != default) {
                instance.fixedDateRange = dateRange;
            }

            if (current.STATUS_HasValue()) {
                var status = EnumHelper.GetEnumValues(current.STATUS(), instance.attributeBindingDefinition("status")!.permitedValues!);
                if (status is not null && status.Any())
                    instance.status = status;
            }

            if (current.PLTS_COMP_SCALE_HasValue()) {
                string subtype = "";

                if (!Subtypes.Instance.TryGetSubtype(current.TableName(), current.FCSubtype(), out subtype))
                    throw new NotSupportedException($"Unknown subtype for {current.TableName}, {current.FCSubtype()}");

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

        private static InformationArea M_NPUB(Feature current, RowBuffer buffer) {
            var instance = new InformationArea();

            if (current.SORDAT_HasValue()) {
                if (DateHelper.TryConvertSordat(current.SORDAT()!, out var reportedDate)) {
                    instance.reportedDate = reportedDate;
                }
                else {
                    Logger.Current.DataError(current.GetObjectID(), current.GetType().Name, current.LNAM() ?? "Unknown LNAM", $"Cannot convert date {current.SORDAT}");
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

                instance.scaleMinimum = Scamin.Instance.GetMinimumScale(current, subtype, current.PLTS_COMP_SCALE()!.Value, isRelatedToStructure: false);
            }

            var result = ImporterNIS.AddInformation(current.GetObjectID(), current.TableName(), current.NTXTDS(), current.TXTDSC(), current.INFORM(), current.NINFOM());
            var informations = result.information.ToArray();

            if (current.PUBREF_HasValue()) {
                informations = [..informations, new information {
                                    language = "eng",
                                    headline = "-32767".Equals(current.PUBREF()) ? null : current.PUBREF()!.Trim(),
                                }];
            }

            if (informations.Any())
                instance.information = informations;
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

        private static FeatureType M_NSYS(Feature current, RowBuffer buffer) {
            if (current.ORIENT_HasValue()) {
                var instance = new LocalDirectionOfBuoyage();

                if (current.MARSYS_HasValue()) {
                    instance.marksNavigationalSystemOf = EnumHelper.GetEnumValue(current.MARSYS()!);
                }

                instance.orientationValue = current.ORIENT() != -32767m ? current.ORIENT() : null;

                if (current.PLTS_COMP_SCALE_HasValue()) {
                    string subtype = "";

                    if (!Subtypes.Instance.TryGetSubtype(current.TableName(), current.FCSubtype(), out subtype))
                        throw new NotSupportedException($"Unknown subtype for {current.TableName}, {current.FCSubtype()}");

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
                var instance = new NavigationalSystemOfMarks();

                if (current.MARSYS_HasValue()) {
                    instance.marksNavigationalSystemOf = EnumHelper.GetEnumValue(current.MARSYS());
                }
                else {
                    Logger.Current.DataError(current.GetObjectID(), current.TableName() ?? "Unknown tablename", current.LNAM() ?? "Unknown LNAM", $"Missing MARSYS value for M_NSYS where globalid = '{{{current.GLOBALID}}}'");
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

        private static QualityOfBathymetricData M_QUAL(Feature current, RowBuffer buffer, QueryFilter filter, Geodatabase target) {
            var instance = new QualityOfBathymetricData();

            /*
                Temporal Variation: The S-101 mandatory attribute category of temporal variation introduces the
                ability for the Data Producer to incorporate the temporal impact on bathymetric data quality in areas
                where the seabed is likely to change over time, or in the wake of an extreme event such as a hurricane
                S-57 ENC to S-101 Conversion Guidance 9
                S-65 Annex B April 2024 Edition 1.2.0
                or tsunami. During the automated conversion process, for all M_QUAL except those where CATZOC =
                6 (zone of confidence U (data not assessed)), the corresponding Quality of Bathymetric Data will
                have category of temporal variation populated with value 5 (unlikely to change). For full S-101
                functionality, Data Producers will be required to reassess the value of this attribute as required. For
                CATZOC = 6 (zone of confidence U (data not assessed)), category of temporal variation will be
                populated with value 6 (unassessed).
            */

            if (current.DRVAL1_HasValue()) {
                instance.depthRangeMinimumValue = current.DRVAL1() != -32767m ? current.DRVAL1() : null;
            }

            if (current.DRVAL2_HasValue()) {
                instance.depthRangeMaximumValue = current.DRVAL2() != -32767m ? current.DRVAL2() : null;
            }

            // TODO: featuresDetected (ed2.1.0)

            //Code Description
            //1   zone of confidence A1
            //2   zone of confidence A2
            //3   zone of confidence B
            //4   zone of confidence C
            //5   zone of confidence D
            //6   zone of confidence U(data not assessed)

            // During the automated conversion process, for all M_QUAL
            // except those where CATZOC = 6 (zone of confidence U(data not assessed)),
            // the corresponding Quality of Bathymetric Data will
            // have category of temporal variation populated with value 5(unlikely to change).

            /* S-65 Annex B p.8
                Data Assessment: The S-101 mandatory attribute data assessment introduces an option to reduce
                screen clutter in some ECDIS display modes through population of value 2 (assessed (oceanic)). This
                value is intended for use where an indication of the overall data quality is not considered to be required
                – generally in depths deeper the 200 metres. However, determination as to when this value may be
                populated cannot be made during the automated conversion process, therefore for all M_QUAL except
                those where CATZOC = 6 (zone of confidence U (data not assessed)), the corresponding Quality of
                Bathymetric Data will have data assessment populated with value 1 (assessed).
             */


            SpatialQuality? spatialQuality = default;

            if (current.CATZOC_HasValue()) { // A1
                int catzoc = current.CATZOC()!.Value;

                if (catzoc == 1) {
                    instance.categoryOfTemporalVariation = 5;   // categoryOfTemporalVariation.UnlikelyToChange;
                    instance.dataAssessment = 1;    // dataAssessment.Assessed;
                    instance.featuresDetected = new featuresDetected() {
                        significantFeaturesDetected = true,
                        leastDepthOfDetectedFeaturesMeasured = true,
                    };
                    instance.fullSeafloorCoverageAchieved = true;
                    instance.zoneOfConfidence = [new zoneOfConfidence() {
                                        categoryOfZoneOfConfidenceInData = 1,   //categoryOfZoneOfConfidenceInData.ZoneOfConfidenceA1
                                    }];

                    spatialQuality = new SpatialQuality {
                        spatialAccuracy = [new spatialAccuracy {
                                           horizontalPositionUncertainty = new horizontalPositionUncertainty{
                                               uncertaintyFixed = 5m,
                                               uncertaintyVariableFactor = 0.05m,   // 5% of depth
                                           },
                                           verticalUncertainty = new verticalUncertainty{
                                               uncertaintyFixed = 0.5m,
                                               uncertaintyVariableFactor = 0.01m,
                                           },
                                        }],
                    };
                }
                else if (catzoc == 2) { // A2
                    instance.categoryOfTemporalVariation = 5;   // categoryOfTemporalVariation.UnlikelyToChange;
                    instance.dataAssessment = 1;    // dataAssessment.Assessed;
                    instance.featuresDetected = new featuresDetected() {
                        significantFeaturesDetected = true,
                        leastDepthOfDetectedFeaturesMeasured = true,

                    };
                    instance.fullSeafloorCoverageAchieved = true;
                    instance.zoneOfConfidence = [new zoneOfConfidence() {
                                        categoryOfZoneOfConfidenceInData = 2,   //categoryOfZoneOfConfidenceInData.ZoneOfConfidenceA2,
                                    }];

                    spatialQuality = new SpatialQuality {
                        spatialAccuracy = [new spatialAccuracy {
                                           horizontalPositionUncertainty = new horizontalPositionUncertainty{
                                               uncertaintyFixed = 20m,
                                           },
                                           verticalUncertainty = new verticalUncertainty{
                                               uncertaintyFixed = 1m,
                                               uncertaintyVariableFactor = 0.02m,
                                           },
                                        }],
                    };
                }
                else if (catzoc == 3) { // B
                    instance.categoryOfTemporalVariation = 5;   // categoryOfTemporalVariation.UnlikelyToChange;
                    instance.dataAssessment = 1;    // dataAssessment.Assessed;
                    instance.featuresDetected = new featuresDetected() {
                        significantFeaturesDetected = false,
                        leastDepthOfDetectedFeaturesMeasured = false,
                    };
                    instance.fullSeafloorCoverageAchieved = false;
                    instance.zoneOfConfidence = [new zoneOfConfidence() {
                                        categoryOfZoneOfConfidenceInData = 3,   //categoryOfZoneOfConfidenceInData.ZoneOfConfidenceB,                                        
                                    }];

                    spatialQuality = new SpatialQuality {
                        spatialAccuracy = [new spatialAccuracy {
                                           horizontalPositionUncertainty = new horizontalPositionUncertainty{
                                               uncertaintyFixed = 50m,
                                           },
                                           verticalUncertainty = new verticalUncertainty{
                                               uncertaintyFixed = 1m,
                                               uncertaintyVariableFactor = 0.02m,
                                           },
                                        }],
                    };
                }
                else if (catzoc == 4) { // C
                    instance.categoryOfTemporalVariation = 5;   // categoryOfTemporalVariation.UnlikelyToChange;
                    instance.dataAssessment = 1;    // dataAssessment.Assessed;
                    instance.featuresDetected = new featuresDetected() {
                        significantFeaturesDetected = false,
                        leastDepthOfDetectedFeaturesMeasured = false,
                    };
                    instance.fullSeafloorCoverageAchieved = false;
                    instance.zoneOfConfidence = [new zoneOfConfidence() {
                                        categoryOfZoneOfConfidenceInData = 4,   //categoryOfZoneOfConfidenceInData.ZoneOfConfidenceC,                                        
                                    }];

                    spatialQuality = new SpatialQuality {
                        spatialAccuracy = [new spatialAccuracy {
                                           horizontalPositionUncertainty = new horizontalPositionUncertainty{
                                               uncertaintyFixed = 500m,
                                           },
                                           verticalUncertainty = new verticalUncertainty{
                                               uncertaintyFixed = 2m,
                                               uncertaintyVariableFactor = 0.05m,
                                           },
                                        }],
                    };
                }
                else if (catzoc == 5) { // D
                    instance.categoryOfTemporalVariation = 5;   // categoryOfTemporalVariation.UnlikelyToChange;
                    instance.dataAssessment = 1;    // dataAssessment.Assessed;
                    instance.featuresDetected = new featuresDetected() {
                        significantFeaturesDetected = false,
                        leastDepthOfDetectedFeaturesMeasured = false,

                    };
                    instance.fullSeafloorCoverageAchieved = false;
                    instance.zoneOfConfidence = [new zoneOfConfidence() {
                                        categoryOfZoneOfConfidenceInData = 5,   //categoryOfZoneOfConfidenceInData.ZoneOfConfidenceD,                                        
                                    }];

                    spatialQuality = new SpatialQuality {
                        spatialAccuracy = [new spatialAccuracy {
                                           horizontalPositionUncertainty = new horizontalPositionUncertainty{
                                               uncertaintyFixed = null,
                                           },
                                           verticalUncertainty = new verticalUncertainty{
                                               uncertaintyFixed = null,
                                           },
                                        }],
                    };
                }
                else if (catzoc == 6) { // U
                    instance.categoryOfTemporalVariation = 5;   // categoryOfTemporalVariation.Unassessed;
                    instance.dataAssessment = 1;    // dataAssessment.Unassessed;
                    instance.featuresDetected = new featuresDetected() {
                        significantFeaturesDetected = false,
                        leastDepthOfDetectedFeaturesMeasured = false,

                    };
                    instance.fullSeafloorCoverageAchieved = false;
                    instance.zoneOfConfidence = [new zoneOfConfidence() {
                                        categoryOfZoneOfConfidenceInData = 6,   //categoryOfZoneOfConfidenceInData.ZoneOfConfidenceU,                                        
                                    }];

                    spatialQuality = new SpatialQuality {
                        spatialAccuracy = [new spatialAccuracy {
                                           horizontalPositionUncertainty = new horizontalPositionUncertainty{
                                               uncertaintyFixed = null,
                                           },
                                           verticalUncertainty = new verticalUncertainty{
                                               uncertaintyFixed = null,
                                           },
                                        }],
                    };
                }
                else {
                    throw new NotSupportedException($"Unknown catzoc {catzoc}. objectid: {current.GetObjectID()} - {current.TableName()}");
                }
            }

            if (DateHelper.TryGetSurveyDateRange(current.SURSTA(), current.SUREND(), out var dateRange)) {
                instance.surveyDateRange = dateRange;
            }

            if (DateHelper.TryGetSurveyDateRange(current.SURSTA(), current.SUREND(), out var surveyDateRange)) {
                instance.surveyDateRange = surveyDateRange;
            }

            if (ImporterNIS.ContainsBarthyFeatures_UnsurveyedArea(current.SHAPE()!, (Geodatabase)current.GetTable().GetDatastore(), filter.WhereClause)) {
                var isCoveredByUNSARE_UnsurveyedArea = false;

                if (ImporterNIS.IsCoveredByUNSARE_UnsurveyedArea(current.SHAPE()!)) {
                    instance.categoryOfTemporalVariation = 6;   //  Unassessed
                    instance.zoneOfConfidence[0]!.categoryOfZoneOfConfidenceInData = 5;  //categoryOfZoneOfConfidenceInData.ZoneOfConfidenceD,                                        
                    isCoveredByUNSARE_UnsurveyedArea = true;
                }
            }

            //var informationBindings = instance.GetInformationBindings();
            informationBinding[] informationBindings = [];

            if (spatialQuality is not null) {
                var informationBinding = target.CreateInformationType(spatialQuality);

                if (informationBindings is null)
                    informationBindings = [];
                informationBindings = [.. informationBindings, informationBinding];
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

        private static QualityOfSurvey M_SREL(Feature current, RowBuffer buffer) {
            var instance = new QualityOfSurvey();

            if (current.DRVAL1_HasValue()) {
                instance.depthRangeMinimumValue = current.DRVAL1() != -32767m ? current.DRVAL1() : null;
            }
            if (current.DRVAL2_HasValue()) {
                instance.depthRangeMaximumValue = current.DRVAL2() != -32767m ? current.DRVAL2() : null;
            }

            // TODO: featuresdetected

            // TODO: full seafloor covearge achieved

            // TODO: line spacing maximum

            // TODO: line spacing minimum

            if (current.SDISMX_HasValue()) {
                if (current.SDISMX() == -32767m) {
                    instance.measurementDistanceMaximum = null;
                }
                else {
                    if (current.SDISMX() % 1 == 0) {
                        instance.measurementDistanceMaximum = Convert.ToInt32(current.SDISMX());
                    }
                    else {
                        Logger.Current.DataError(current.GetObjectID(), current.LNAM() ?? "Empty LNAM", current.TableName() ?? "Unknown tablename", $"SDISMX on M_SREL: value is {current.SDISMX} and cannot be converted to an integer");
                    }
                }
            }

            if (current.SDISMN_HasValue()) {
                if (current.SDISMN() == -32767m) {
                    instance.measurementDistanceMaximum = null;
                }
                else {
                    if (current.SDISMN() % 1 == 0) {
                        instance.measurementDistanceMaximum = Convert.ToInt32(current.SDISMN());
                    }
                    else {
                        Logger.Current.DataError(current.GetObjectID(), current.LNAM() ?? "Empty LNAM", current.TableName() ?? "Unknown tablename", $"SDISMN on M_SREL: value is {current.SDISMN} and cannot be converted to an integer");
                    }
                }
            }

            if (current.QUAPOS_HasValue()) {
                instance.qualityOfHorizontalMeasurement = current.QUAPOS() switch {
                    4 => 4, //qualityOfHorizontalMeasurement.Approximate,
                    _ => default,
                };
            }

            if (current.QUASOU_HasValue()) {
                var qualityOfVerticalMeasurement = EnumHelper.GetEnumValues(current.QUASOU());
                if (qualityOfVerticalMeasurement is not null && qualityOfVerticalMeasurement.Any())
                    instance.qualityOfVerticalMeasurement = qualityOfVerticalMeasurement;
            }

            if (current.SCVAL1_HasValue()) {
                instance.scaleValueMaximum = current.SCVAL1() == -32767 ? null : current.SCVAL1();
            }

            if (current.SCVAL2_HasValue()) {
                instance.scaleValueMinimum = current.SCVAL2() == -32767 ? null : current.SCVAL2();
            }

            if (current.SURATH_HasValue()) {
                instance.surveyAuthority = "-32767".Equals(current.SURATH()) ? null : current.SURATH();
            }

            if (DateHelper.TryGetSurveyDateRange(current.SURSTA(), current.SUREND(), out var surveyDateRange)) {
                instance.surveyDateRange = surveyDateRange!;
            }

            if (current.SURTYP_HasValue()) {
                var surveyType = EnumHelper.GetEnumValues(current.SURTYP());
                if (surveyType is not null)
                    instance.surveyType = surveyType;
            }

            if (current.TECSOU_HasValue()) {
                var techniqueOfVerticalMeasurement = EnumHelper.GetEnumValues(current.TECSOU(), instance.attributeBindingDefinition("techniqueOfVerticalMeasurement")!.permitedValues!);
                if (techniqueOfVerticalMeasurement is not null && techniqueOfVerticalMeasurement.Any())
                    instance.techniqueOfVerticalMeasurement = techniqueOfVerticalMeasurement;
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

        private static VerticalDatumOfData M_VDAT(Feature current, RowBuffer buffer) {
            var instance = new VerticalDatumOfData();

            var verticalDatum = ImporterNIS.GetVerticalDatum(current.VERDAT(), current.SHAPE()!);
            if (verticalDatum != null) {
                var update = true;
                foreach (var elm in VerticalDatums.Instance.Touch(current.SHAPE()!)) {
                    if (elm.Item2.value == verticalDatum.value) {
                        update = false;
                    }
                }
                if (update)
                    instance.verticalDatum = verticalDatum.value;
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

        private static DepthArea DEPARE(Feature current, RowBuffer buffer) {
            var instance = new DepthArea();

            instance.depthRangeMinimumValue = current.DRVAL1()!.Value;

            if (current.DRVAL2_HasValue())
                instance.depthRangeMaximumValue = current.DRVAL2() != -32767m ? current.DRVAL2() : null;

            // TODO: Spatial association to Spatial Quality

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

        private static UnsurveyedArea UNSARE(Feature current, RowBuffer buffer) {
            var instance = new UnsurveyedArea();

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

        private static DepthContour DEPCNT(Feature current, RowBuffer buffer) {
            var instance = new DepthContour();

            if (current.VALDCO_HasValue()) {
                instance.valueOfDepthContour = current.VALDCO() != -32767m ? current.VALDCO() : null;
            }

            if (current.PLTS_COMP_SCALE_HasValue()) {
                string subtype = "";

                if (!Subtypes.Instance.TryGetSubtype(current.TableName(), current.FCSubtype(), out subtype))
                    throw new NotSupportedException($"Unknown subtype for {current.TableName}, {current.FCSubtype()}");

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

                if (i < natsur.Length && EnumHelper.GetEnumValue(natsur[i], out int? natureOfSurface, s.attributeBindingDefinition("natureOfSurface")!.permitedValues!))
                    s.natureOfSurface = natureOfSurface;
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

            if (current.PLTS_COMP_SCALE_HasValue()) {
                string subtype = "";

                if (!Subtypes.Instance.TryGetSubtype(current.TableName(), current.FCSubtype(), out subtype))
                    throw new NotSupportedException($"Unknown subtype for {current.TableName}, {current.FCSubtype()}");

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

            if (current.PLTS_COMP_SCALE_HasValue()) {
                string subtype = "";

                if (!Subtypes.Instance.TryGetSubtype(current.TableName(), current.FCSubtype(), out subtype))
                    throw new NotSupportedException($"Unknown subtype for {current.TableName}, {current.FCSubtype()}");

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

                if (current.PLTS_COMP_SCALE_HasValue()) {
                    string subtype = "";

                    if (!Subtypes.Instance.TryGetSubtype(current.TableName(), current.FCSubtype(), out subtype))
                        throw new NotSupportedException($"Unknown subtype for {current.TableName}, {current.FCSubtype()}");

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

                if (current.PLTS_COMP_SCALE_HasValue()) {
                    string subtype = "";

                    if (!Subtypes.Instance.TryGetSubtype(current.TableName(), current.FCSubtype(), out subtype))
                        throw new NotSupportedException($"Unknown subtype for {current.TableName}, {current.FCSubtype()}");

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
                    var techniqueOfVerticalMeasurement = EnumHelper.GetEnumValues(current.TECSOU(), instance.attributeBindingDefinition("techniqueOfVerticalMeasurement")!.permitedValues!);
                    if (techniqueOfVerticalMeasurement is not null && techniqueOfVerticalMeasurement.Any())
                        instance.techniqueOfVerticalMeasurement = techniqueOfVerticalMeasurement;
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
                    var status = EnumHelper.GetEnumValues(current.STATUS(), instance.attributeBindingDefinition("status")!.permitedValues!);
                    if (status is not null && status.Any())
                        instance.status = status[0];
                }

                if (current.TECSOU_HasValue()) {
                    var techniqueOfVerticalMeasurement = EnumHelper.GetEnumValues(current.TECSOU(), instance.attributeBindingDefinition("techniqueOfVerticalMeasurement")!.permitedValues!);
                    if (techniqueOfVerticalMeasurement is not null && techniqueOfVerticalMeasurement.Any())
                        instance.techniqueOfVerticalMeasurement = techniqueOfVerticalMeasurement;
                }

                if (current.PLTS_COMP_SCALE_HasValue()) {
                    string subtype = "";

                    if (!Subtypes.Instance.TryGetSubtype(current.TableName(), current.FCSubtype(), out subtype))
                        throw new NotSupportedException($"Unknown subtype for {current.TableName}, {current.FCSubtype()}");

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

                if (current.TECSOU_HasValue()) {
                    instance.techniqueOfVerticalMeasurement = [.. current.TECSOU()!.Split(',').Select(e => EnumHelper.GetEnumValue(e))];
                }

                if (current.PLTS_COMP_SCALE_HasValue()) {
                    string subtype = "";

                    if (!Subtypes.Instance.TryGetSubtype(current.TableName(), current.FCSubtype(), out subtype))
                        throw new NotSupportedException($"Unknown subtype for {current.TableName}, {current.FCSubtype()}");

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

            if (current.PLTS_COMP_SCALE_HasValue()) {
                string subtype = "";
                if (current.TableName() != default && !Subtypes.Instance.TryGetSubtype(current.TableName(), current.FCSubtype(), out subtype))
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
                instance.minimumBerthDepth = current.DRVAL1() != -32767m ? current.DRVAL1() : null;
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
                var status = EnumHelper.GetEnumValues(current.STATUS(), instance.attributeBindingDefinition("status")!.permitedValues!);
                if (status is not null && status.Any())
                    instance.status = status;
            }

            if (current.SOUACC_HasValue()) {
                instance.verticalUncertainty = new() {
                    uncertaintyFixed = current.SOUACC()
                };
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
                    orientationValue = current.ORIENT() != -32767m ? current.ORIENT() : null,
                    // TODO: oriantationUncertainty
                    //orientationUncertainty = ,
                };
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
                instance.orientationValue = current.ORIENT() != -32767m ? current.ORIENT() : null;
            }
            if (current.DRVAL1_HasValue()) {
                instance.depthRangeMinimumValue = current.DRVAL1() != -32767m ? current.DRVAL1() : null;
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
                var status = EnumHelper.GetEnumValues(current.STATUS(), instance.attributeBindingDefinition("status")!.permitedValues!);
                if (status is not null && status.Any())
                    instance.status = status;
            }

            if (current.TECSOU_HasValue()) {
                var techniqueOfVerticalMeasurement = EnumHelper.GetEnumValues(current.TECSOU(), instance.attributeBindingDefinition("techniqueOfVerticalMeasurement")!.permitedValues!);
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
                    Logger.Current.Debug($"VesselSpeedLimit {current.TableName()}::{current.OBJECTID()} {current.INFORM()}");
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
                var status = EnumHelper.GetEnumValues(current.STATUS(), instance.attributeBindingDefinition("status")!.permitedValues!);
                if (status is not null && status.Any())
                    instance.status = status;
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

            if (current.CATMFA_HasValue()) {
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
                var status = EnumHelper.GetEnumValues(current.STATUS(), instance.attributeBindingDefinition("status")!.permitedValues!);
                if (status is not null && status.Any())
                    instance.status = status;
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
                    Logger.Current.Debug($"VesselSpeedLimit {current.TableName()}::{current.OBJECTID()} {current.INFORM()}");
            }

            if (current.WATLEV_HasValue()) {
                instance.waterLevelEffect = EnumHelper.GetEnumValue(current.WATLEV());
            }

            // TODO: HEIGHT                            
            if (instance.waterLevelEffect == 1 || instance.waterLevelEffect == 2) {
                /* The attribute height must be populated for Marine Farm/Culture features having attribute water level
                   effect = 1 (partly submerged at high water) or 2 (always dry). */


            }

            if (current.PLTS_COMP_SCALE_HasValue()) {
                string subtype = "";

                if (!Subtypes.Instance.TryGetSubtype(current.TableName(), current.FCSubtype(), out subtype))
                    throw new NotSupportedException($"Unknown subtype for {current.TableName()}, {current.FCSubtype()}");

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

            if (current.PRODCT_HasValue()) {
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
                var status = EnumHelper.GetEnumValues(current.STATUS(), instance.attributeBindingDefinition("status")!.permitedValues!);
                if (status is not null && status.Any())
                    instance.status = status;
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
                    Logger.Current.Debug($"VesselSpeedLimit {current.TableName()}::{current.OBJECTID()} {current.INFORM()}");
            }

            if (current.CONVIS_HasValue()) {
                instance.visualProminence = EnumHelper.GetEnumValue(current.CONVIS());
            }

            // TODO: waterleveleffect

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
                if (current.TableName() != default && !Subtypes.Instance.TryGetSubtype(current.TableName(), current.FCSubtype(), out subtype))
                    throw new NotSupportedException($"Unknown subtype for {current.TableName()}, {current.FCSubtype()}");
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
                if (current.TableName() != default && !Subtypes.Instance.TryGetSubtype(current.TableName(), current.FCSubtype(), out subtype))
                    throw new NotSupportedException($"Unknown subtype for {current.TableName()}, {current.FCSubtype()}");
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
                if (current.TableName() != default && !Subtypes.Instance.TryGetSubtype(current.TableName(), current.FCSubtype(), out subtype))
                    throw new NotSupportedException($"Unknown subtype for {current.TableName()}, {current.FCSubtype()}");
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
                instance.orientationValue = current.ORIENT() != -32767m ? current.ORIENT() : null;
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
                if (current.TableName() != default && !Subtypes.Instance.TryGetSubtype(current.TableName(), current.FCSubtype(), out subtype))
                    throw new NotSupportedException($"Unknown subtype for {current.TableName()}, {current.FCSubtype()}");
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
                if (current.TableName() != default && !Subtypes.Instance.TryGetSubtype(current.TableName(), current.FCSubtype(), out subtype))
                    throw new NotSupportedException($"Unknown subtype for {current.TableName()}, {current.FCSubtype()}");
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

            if (current.PRODCT_HasValue()) {
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
                if (current.TableName() != default && !Subtypes.Instance.TryGetSubtype(current.TableName(), current.FCSubtype(), out subtype))
                    throw new NotSupportedException($"Unknown subtype for {current.TableName()}, {current.FCSubtype()}");
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
                    Logger.Current.Debug($"VesselSpeedLimit {current.TableName()}::{current.OBJECTID()} {current.INFORM()}");
            }

            if (current.PLTS_COMP_SCALE_HasValue()) {
                string subtype = "";
                if (current.TableName() != default && !Subtypes.Instance.TryGetSubtype(current.TableName(), current.FCSubtype(), out subtype))
                    throw new NotSupportedException($"Unknown subtype for {current.TableName()}, {current.FCSubtype()}");
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

