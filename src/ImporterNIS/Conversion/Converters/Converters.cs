using ArcGIS.Core.Data;
using S100FC.S101.FeatureTypes;
using S100Framework.Applications.S57.esri;
using S100Framework.Applications.Singletons;

namespace VortexLoader
{

    public class ConverterRegistry
    {
        // The extra object is an additional parameter to the converter
        private readonly Dictionary<(Type from, Type to), Func<object, int?, Geodatabase, object>> _converters = [];

        public bool Exist(Type TFrom, Type TTo) => this._converters.ContainsKey((TFrom, TTo));

        public bool Exist<TFrom, TTo>() => this._converters.ContainsKey((typeof(TFrom), typeof(TTo)));


        public void Register<TFrom, TTo>(Func<TFrom, int?, Geodatabase, TTo> converter) {
            if (converter == null) {
                throw new ArgumentNullException(nameof(converter));
            }

            this._converters[(typeof(TFrom), typeof(TTo))] = (input, scaleMinimum, geodatabase) => converter((TFrom)input, scaleMinimum, geodatabase)!;
        }

        public TOut Convert<TOut>(object value, int? scaleMinimum = default(int?), Geodatabase geodatabase = null!) {
            var fromType = value.GetType();
            var toType = typeof(TOut);

            if (this._converters.TryGetValue((fromType, toType), out var converter)) {
                return (TOut)converter(value, scaleMinimum, geodatabase);
            }

            throw new InvalidOperationException($"No converter registered from {fromType.Name} to {toType.Name}");
        }

        public object Convert(object value, Type toType, int? scaleMinimum/* = default(int?)*/, Geodatabase geodatabase = null!) {
            var fromType = value.GetType();

            if (this._converters.TryGetValue((fromType, toType), out var converter)) {
                return converter(value, scaleMinimum, geodatabase);
            }

            //return null;
            // TODO: 'No converter registered from PortsAndServicesP to SignalStationTraffic'
            throw new InvalidOperationException($"No converter registered from {fromType.Name} to {toType.Name}");
        }
    }


    //public TOut ConvertList<TOut>(IList<S100Framework.Applications.S57.esri.AidsToNavigationP> related, IList<object> values) {
    //    var fromType = values.First().GetType();
    //    var toType = typeof(TOut);

    //    if (!_listConverters.ContainsKey((fromType, toType))) {
    //        throw new InvalidOperationException($"No converter registered from {fromType.Name} to {toType.Name}");
    //    }

    //    var converter = _listConverters[(fromType, toType)];
    //    return (TOut)converter(values);
    //}

    //public object ConvertList(IList<object> values, Type toType) {
    //    var fromType = values.First().GetType();

    //    if (!_listConverters.ContainsKey((fromType, toType))) {
    //        throw new InvalidOperationException($"No converter registered from {fromType.Name} to {toType.Name}");
    //    }

    //    var converter = _listConverters[(fromType, toType)];
    //    return converter(values);
    //}
}



//public class ConverterRegistry
//{
//    private readonly Dictionary<(Type from, Type to), Func<object, object>> _converters = new();

//    public void Register<TFrom, TTo>(Func<TFrom, TTo> converter) {
//        _converters[(typeof(TFrom), typeof(TTo))] = input => converter((TFrom)input);
//    }

//    public TOut Convert<TOut, TIn>(object value) {
//        var fromType = typeof(TIn);
//        var toType = typeof(TOut);

//        if (_converters.TryGetValue((fromType, toType), out var converter)) {
//            return (TOut)converter((TIn)value);
//        }

//        throw new InvalidOperationException($"No converter registered from {fromType.Name} to {toType.Name}");
//    }
//    public object Convert(object value, Type fromType, Type toType) {
//        if (_converters.TryGetValue((fromType, toType), out var converter)) {
//            return converter(value);
//        }

//        throw new InvalidOperationException($"No converter registered from {fromType.Name} to {toType.Name}");
//    }
//}




namespace S100Framework.Applications
{
    internal static partial class Converters
    {
        internal static Building CreateBuilding(CulturalFeaturesP current, int? scaleMinimum, Geodatabase source) {
            var instance = new Building();

            if (current.BUISHP != null) {
                instance.buildingShape = EnumHelper.GetEnumValue(current.BUISHP);
            }

            if (current.COLOUR != default) {
                var colour = ImporterNIS.GetColours(current.COLOUR);
                if (colour is not null && colour.Any())
                    instance.colour = colour;
            }

            if (current.COLPAT != default) {
                if (instance.colour is not null && instance.colour.Length > 1)
                    instance.colourPattern = ImporterNIS.GetColourPattern(current.COLPAT)!.value;
            }

            if (current.CONDTN.HasValue) {
                instance.condition = ImporterNIS.GetCondition(current.CONDTN.Value)?.value;
            }

            if (current.ELEVAT.HasValue) {
                instance.elevation = current.ELEVAT.Value == -32767m ? null : current.ELEVAT.Value;
            }

            var featureName = ImporterNIS.GetFeatureName(current.OBJNAM, current.NOBJNM);
            if (featureName is not null)
                instance.featureName = featureName;

            if (current.FUNCTN != default) {
                var function = EnumHelper.GetEnumValues(current.FUNCTN);
                if (function is not null && function.Any())
                    instance.function = function;
            }
            if (current.HEIGHT.HasValue) {
                instance.height = current.HEIGHT.Value != -32767m ? current.HEIGHT.Value : null;
            }
            else {

            }

            // TODO: interoperabilityIdentifier

            // TODO: multiplicity of features

            if (current.NATCON != default) {
                var natureOfConstruction = EnumHelper.GetEnumValues(current.NATCON);
                if (natureOfConstruction is not null && natureOfConstruction.Any())
                    instance.natureOfConstruction = natureOfConstruction;
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

            if (current.STATUS != default) {
                instance.status = ImporterNIS.GetStatus(current.STATUS);
            }

            if (current.VERLEN.HasValue) {
                instance.verticalLength = current.VERLEN.Value != -32767m ? current.VERLEN.Value : null;
            }

            if (current.CONVIS.HasValue /*&& current.CONVIS.Value != -32767*/) {
                instance.visualProminence = EnumHelper.GetEnumValue(current.CONVIS.Value);
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
                instance.pictorialRepresentation = ImporterNIS.FixFilename(current.PICREP);
            }

            instance.inTheWater = !LandAreas.Instance.Touch(current!.SHAPE!).Any();

            return instance;
        }
    }
}