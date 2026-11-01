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
    using S100FC.S101.InformationTypes;
    using S100FC.S101.SimpleAttributes;
    using S100Framework.Applications.S57.esri;

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
            //{ "PIPSOL", (current, buffer) => { return PIPSOL(current, buffer); } },
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
            //{ "OBSTRN", (current, buffer) => { return OBSTRN(current, buffer); } },
            { "WATTUR", (current, buffer) => { return WATTUR(current, buffer); } },
            //{ "WRECKS", (current, buffer) => { return WRECKS(current, buffer); } },
            { "OILBAR", (current, buffer) => { return OILBAR(current, buffer); } },
            //{ "UWTROC", (current, buffer) => { return UWTROC(current, buffer); } },
            { "LAKARE", (current, buffer) => { return LAKARE(current, buffer); } },
            { "LNDARE", (current, buffer) => { return LNDARE(current, buffer); } },
            { "LNDRGN", (current, buffer) => { return LNDRGN(current, buffer); } },
            { "RIVERS", (current, buffer) => { return RIVERS(current, buffer); } },
            { "SEAARE", (current, buffer) => { return SEAARE(current, buffer); } },
            { "SLOGRD", (current, buffer) => { return SLOGRD(current, buffer); } },
            { "VEGATN", (current, buffer) => { return VEGATN(current, buffer); } },
            { "LNDELV", (current, buffer) => { return LNDELV(current, buffer); } },
            { "SLOTOP", (current, buffer) => { return SLOTOP(current, buffer); } },
            { "LOCMAG", (current, buffer) => { return LOCMAG(current, buffer); } },
            { "MAGVAR", (current, buffer) => { return MAGVAR(current, buffer); } },
            { "TIDEWY", (current, buffer) => { return TIDEWY(current, buffer); } },
            { "TS_FEB", (current, buffer) => { return TS_FEB(current, buffer); } },
            { "FERYRT", (current, buffer) => { return FERYRT(current, buffer); } },
            { "RCTLPT", (current, buffer) => { return RCTLPT(current, buffer); } },
            { "TSEZNE", (current, buffer) => { return TSEZNE(current, buffer); } },
            { "TWRTPT", (current, buffer) => { return TWRTPT(current, buffer); } },
            { "RCRTCL", (current, buffer) => { return RCRTCL(current, buffer); } },
            { "RDOCAL", (current, buffer) => { return RDOCAL(current, buffer); } },
            { "RECTRC", (current, buffer) => { return RECTRC(current, buffer); } },
            { "TSELNE", (current, buffer) => { return TSELNE(current, buffer); } },
            { "TSSBND", (current, buffer) => { return TSSBND(current, buffer); } },

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
    }
}

