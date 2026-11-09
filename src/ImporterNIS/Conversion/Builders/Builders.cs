using ArcGIS.Core.Data;
using S100FC;
using System.Text.RegularExpressions;

namespace S100Framework.Applications
{
    internal static partial class ImporterNIS
    {
        private static readonly Dictionary<string, Func<Feature, FeatureType>> _builders = new Dictionary<string, Func<Feature, FeatureType>> {
            { "DISMAR", (current) => { return DISMAR(current); } },
            { "BERTHS", (current) => { return BERTHS(current); } },
            { "NAVLNE", (current) => { return NAVLNE(current); } },
            { "ADMARE", (current) => { return ADMARE(current); } },
            { "RESARE", (current) => { return RESARE(current); } },
            { "ACHARE", (current) => { return ACHARE(current); } },
            { "CTSARE", (current) => { return CTSARE(current); } },
            { "DWRTPT", (current) => { return DWRTPT(current); } },
            { "DRGARE", (current) => { return DRGARE(current); } },
            { "DMPGRD", (current) => { return DMPGRD(current); } },
            { "FAIRWY", (current) => { return FAIRWY(current); } },
            { "HRBFAC", (current) => { return HRBFAC(current); } },
            { "ISTZNE", (current) => { return ISTZNE(current); } },
            { "MARCUL", (current) => { return MARCUL(current); } },
            { "MIPARE", (current) => { return MIPARE(current); } },
            { "OSPARE", (current) => { return OSPARE(current); } },
            { "PRCARE", (current) => { return PRCARE(current); } },
            { "CGUSTA", (current) => { return CGUSTA(current); } },
            { "SPLARE", (current) => { return SPLARE(current); } },
            { "CBLARE", (current) => { return CBLARE(current); } },
            { "TSSLPT", (current) => { return TSSLPT(current); } },
            { "TSSRON", (current) => { return TSSRON(current); } },
            { "PIPARE", (current) => { return PIPARE(current); } },
            { "TESARE", (current) => { return TESARE(current); } },
            { "RSCSTA", (current) => { return RSCSTA(current); } },
            { "M_ACCY", (current) => { return M_ACCY(current); } },
            { "SOUNDG", (current) => { return SOUNDG(current); } },
            { "TS_PAD", (current) => { return TS_PAD(current); } },
            { "CURENT", (current) => { return CURENT(current); } },
            { "WEDKLP", (current) => { return WEDKLP(current); } },
            { "SNDWAV", (current) => { return SNDWAV(current); } },
            { "SBDARE", (current) => { return SBDARE(current); } },
            { "SWPARE", (current) => { return SWPARE(current); } },
            //{ "PIPSOL", (current) => { return PIPSOL(current); } },
            { "SUBTLN", (current) => { return SUBTLN(current); } },
            { "RADRNG", (current) => { return RADRNG(current); } },
            { "STSLNE", (current) => { return STSLNE(current); } },
            { "ACHBRT", (current) => { return ACHBRT(current); } },
            { "CONZNE", (current) => { return CONZNE(current); } },
            { "COSARE", (current) => { return COSARE(current); } },
            { "EXEZNE", (current) => { return EXEZNE(current); } },
            { "FSHZNE", (current) => { return FSHZNE(current); } },
            { "HRBARE", (current) => { return HRBARE(current); } },
            { "M_NPUB", (current) => { return M_NPUB(current); } },
            { "M_NSYS", (current) => { return M_NSYS(current); } },
            //{ "M_QUAL", (current) => { return M_QUAL(current); } },
            { "M_SREL", (current) => { return M_SREL(current); } },
            { "DEPARE", (current) => { return DEPARE(current); } },
            { "UNSARE", (current) => { return UNSARE(current); } },
            { "DEPCNT", (current) => { return DEPCNT(current); } },
            { "SPRING", (current) => { return SPRING(current); } },
            { "OFSPLF", (current) => { return OFSPLF(current); } },
            { "CBLSUB", (current) => { return CBLSUB(current); } },
            { "LOGPON", (current) => { return LOGPON(current); } },
            { "CTRPNT", (current) => { return CTRPNT(current); } },
            { "GRIDRN", (current) => { return GRIDRN(current); } },
            { "SLCONS", (current) => { return SLCONS(current); } },
            { "COALNE", (current) => { return COALNE(current); } },
            { "CTNARE", (current) => { return CTNARE(current); } },
            { "FSHFAC", (current) => { return FSHFAC(current); } },
            //{ "OBSTRN", (current) => { return OBSTRN(current); } },
            { "WATTUR", (current) => { return WATTUR(current); } },
            //{ "WRECKS", (current) => { return WRECKS(current); } },
            { "OILBAR", (current) => { return OILBAR(current); } },
            //{ "UWTROC", (current) => { return UWTROC(current); } },
            { "LAKARE", (current) => { return LAKARE(current); } },
            { "LNDARE", (current) => { return LNDARE(current); } },
            { "LNDRGN", (current) => { return LNDRGN(current); } },
            { "RIVERS", (current) => { return RIVERS(current); } },
            { "SEAARE", (current) => { return SEAARE(current); } },
            { "SLOGRD", (current) => { return SLOGRD(current); } },
            { "VEGATN", (current) => { return VEGATN(current); } },
            { "LNDELV", (current) => { return LNDELV(current); } },
            { "SLOTOP", (current) => { return SLOTOP(current); } },
            { "LOCMAG", (current) => { return LOCMAG(current); } },
            { "MAGVAR", (current) => { return MAGVAR(current); } },
            { "TIDEWY", (current) => { return TIDEWY(current); } },
            { "TS_FEB", (current) => { return TS_FEB(current); } },
            { "FERYRT", (current) => { return FERYRT(current); } },
            { "RCTLPT", (current) => { return RCTLPT(current); } },
            { "TSEZNE", (current) => { return TSEZNE(current); } },
            { "TWRTPT", (current) => { return TWRTPT(current); } },
            { "RCRTCL", (current) => { return RCRTCL(current); } },
            { "RDOCAL", (current) => { return RDOCAL(current); } },
            { "RECTRC", (current) => { return RECTRC(current); } },
            { "TSELNE", (current) => { return TSELNE(current); } },
            { "TSSBND", (current) => { return TSSBND(current); } },

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

        private static FeatureType Build(string code, Feature feature) => _builders[code]?.Invoke(feature)!;
    }
}

