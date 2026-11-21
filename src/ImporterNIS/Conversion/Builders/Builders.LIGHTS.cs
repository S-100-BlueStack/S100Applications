using ArcGIS.Core.Data;
using S100FC.S101.FeatureTypes;
using S100Framework.Applications.Singletons;

namespace S100Framework.Applications
{
    using S100FC;
    using S100FC.S101.ComplexAttributes;
    using S100Framework.Applications.S57.esri;

    internal static partial class ImporterNIS
    {

        private static FeatureType LIGHTS(Feature current, AidsToNavigationP[] lights) {
            var _ = new AidsToNavigationP(current);
            if (_.GetS101CatlitTypeFrom() == typeof(LightSectored)) {
                var instance = new LightSectored();

                //var lights = new List<AidsToNavigationP>();
                //foreach (var slave in slaves) {
                //    if (slave.S101Type == typeof(LightSectored)) {
                //        var obj = slave.S57Object as AidsToNavigationP;
                //        if (obj == null) {
                //            throw new NotSupportedException($"{slave.S57Object} is not an AidsToNavigationP");
                //        }
                //        lights.Add(obj);
                //    }
                //}
                

                //var light = lights.First();
                // TODO: evaluate light sectors based on height. Assume same height for now and take data from first.
                //var current = lights.First();

                //foreach (var lightN in lights) {
                //    if (lightN.CATLIT != default) {
                //        var list = EnumHelper.GetEnumValues<categoryOfLight>(lightN.CATLIT);
                //        instance.categoryOfLight = instance.categoryOfLight.Union(list).ToList<categoryOfLight>();
                //            //var it = (List<categoryOfLight>);
                //        //instance.categoryOfLight = null;
                //    }
                //     TODO: CATLITs
                //}

                if (current.EXCLIT_HasValue()) {
                    instance.exhibitionConditionOfLight = EnumHelper.GetEnumValue(current.EXCLIT());
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

                if (current.MARSYS_HasValue()) {
                    instance.marksNavigationalSystemOf = EnumHelper.GetEnumValue(current.MARSYS());
                }

                if (current.MLTYLT_HasValue()) {
                    instance.multiplicityOfFeatures = new multiplicityOfFeatures() {
                        multiplicityKnown = true,
                        numberOfFeatures = current.MLTYLT()
                    };
                }

                DateHelper.TryGetPeriodicDateRange(current.PERSTA(), current.PEREND(), out var periodicDateRange);
                if (periodicDateRange != default) {
                    instance.periodicDateRange = periodicDateRange;
                }

                var sectorCharacteristics = ImporterNIS.GetSectorCharacteristics<LightSectored>(lights);
                if (sectorCharacteristics is not null)
                    instance.sectorCharacteristics = sectorCharacteristics;

                if (current.SIGGEN_HasValue()) {
                    instance.signalGeneration = EnumHelper.GetEnumValue(current.SIGGEN());
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
            else if (_.GetS101CatlitTypeFrom() == typeof(LightAirObstruction)) {
                var instance = new LightAirObstruction();

                if (current.COLOUR_HasValue()) {
                    var colours = ImporterNIS.GetColours(current.COLOUR()!);
                    if (colours is not null)
                        instance.colour = colours;
                }

                if (current.EXCLIT_HasValue()) {
                    instance.exhibitionConditionOfLight = EnumHelper.GetEnumValue(current.EXCLIT());
                }

                var featureName = GetFeatureName(current.OBJNAM(), current.NOBJNM());
                if (featureName is not null)
                    instance.featureName = featureName;


                DateHelper.TryGetFixedDateRange(current.DATSTA(), current.DATEND(), out var dateRange);
                if (dateRange != default) {
                    instance.fixedDateRange = dateRange;
                }

                // flareBearing is not populated. New field.                     
                if (current.HEIGHT_HasValue()) {
                    instance.height = current.HEIGHT() != -32767m ? current.HEIGHT() : null;
                }

                if (current.LITVIS_HasValue()) {
                    var litvis = EnumHelper.GetEnumValues(current.LITVIS());
                    int[] permitedValues = instance.attributeBindingsCatalogue.Single(e => e.attribute.Equals("lightVisibility")).permitedValues!;

                    if (litvis != null) {
                        litvis = [.. litvis.Where(e => e.HasValue && permitedValues.Contains(e.Value))];
                        if (litvis.Any())
                            instance.lightVisibility = litvis;
                    }

                    //var lightVisibility = EnumHelper.GetEnumValues(current.LITVIS);
                    //if (lightVisibility != null && lightVisibility.Any())
                    //    instance.lightVisibility = lightVisibility;
                }

                if (current.MLTYLT_HasValue()) {
                    instance.multiplicityOfFeatures = new multiplicityOfFeatures() {
                        multiplicityKnown = true,
                        numberOfFeatures = current.MLTYLT()
                    };
                }

                DateHelper.TryGetPeriodicDateRange(current.PERSTA(), current.PEREND(), out var periodicDateRange);
                if (periodicDateRange != default) {
                    instance.periodicDateRange = periodicDateRange;
                }

                instance.rhythmOfLight = ImporterNIS.GetRythmOfLight<LightAirObstruction>(_);

                if (current.STATUS_HasValue()) {
                    var status = EnumHelper.GetEnumValues(current.STATUS(), instance.attributeBindingDefinition("status")!.permitedValues!);
                    if (status is not null && status.Any())
                        instance.status = status;
                }

                if (current.VALNMR_HasValue()) {
                    instance.valueOfNominalRange = current.VALNMR();
                }

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
            else if (_.GetS101CatlitTypeFrom() == typeof(LightFogDetector)) {
                var instance = new LightFogDetector();

                if (current.COLOUR_HasValue()) {
                    var colours = ImporterNIS.GetColours(current.COLOUR()!);
                    if (colours is not null)
                        instance.colour = colours;
                }

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

                instance.rhythmOfLight = ImporterNIS.GetRythmOfLight<LightFogDetector>(_);

                //if (instance.rhythmOfLight.attributeBindings.Count(e => e.S100FC_code.Equals("signalPeriod"))==0) System.Diagnostics.Debugger.Break();

                if (current.SIGGEN_HasValue()) {
                    instance.signalGeneration = EnumHelper.GetEnumValue(current.SIGGEN());
                }

                if (current.STATUS_HasValue()) {
                    var status = EnumHelper.GetEnumValues(current.STATUS(), instance.attributeBindingDefinition("status")!.permitedValues!);
                    if (status is not null && status.Any())
                        instance.status = status;
                }

                // covered by meta feature hence not to be set
                if (current.VERLEN_HasValue()) {
                    instance.verticalLength = current.VERLEN() != -32767m ? current.VERLEN() : null;
                    var verticalDatum = ImporterNIS.GetVerticalDatum(current.VERDAT(), current.SHAPE()!);
                    if (verticalDatum != default)
                        instance.verticalDatum = verticalDatum.value;
                }

                // flareBearing is not populated. New field.    
                if (current.HEIGHT_HasValue()) {
                    instance.height = current.HEIGHT() != -32767m ? current.HEIGHT() : null;
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
            else if (_.GetS101CatlitTypeFrom() == typeof(LightAllAround)) {
                var instance = new LightAllAround();

                if (current.CATLIT_HasValue()) {
                    var categoryOfLight = EnumHelper.GetEnumValues(current.CATLIT());
                    if (categoryOfLight is not null && categoryOfLight.Any())
                        instance.categoryOfLight = categoryOfLight;
                }

                if (current.COLOUR_HasValue()) {
                    var colours = ImporterNIS.GetColours(current.COLOUR()!);
                    if (colours is not null)
                        instance.colour = colours;
                }

                if (current.EXCLIT_HasValue()) {
                    instance.exhibitionConditionOfLight = EnumHelper.GetEnumValue(current.EXCLIT());
                }

                var featureName = GetFeatureName(current.OBJNAM(), current.NOBJNM());
                if (featureName is not null)
                    instance.featureName = featureName;

                DateHelper.TryGetFixedDateRange(current.DATSTA(), current.DATEND(), out var dateRange);
                if (dateRange != default) {
                    instance.fixedDateRange = dateRange;
                }

                // flareBearing is not populated. New field.                            
                if (current.HEIGHT_HasValue()) {
                    instance.height = current.HEIGHT() != -32767m ? current.HEIGHT() : null;
                }

                if (current.LITVIS_HasValue()) {
                    var litvis = EnumHelper.GetEnumValue(current.LITVIS());
                    int[] permitedValues = instance.attributeBindingsCatalogue.Single(e => e.attribute.Equals("lightVisibility")).permitedValues!;

                    if (!litvis.HasValue || permitedValues.Contains(litvis.Value))   //  UNKNOWN or permitted value
                        instance.lightVisibility = litvis;
                }

                /*
                    The S-101 Boolean _s101type attribute major light has been introduced in S-101 to aid in improved
                    portrayal of lights in ECDIS. This attribute will be populated as True during the automated conversion
                    process for all lights having a nominal range of 10 Nautical Miles or greater.
                */

                if (current.VALNMR_HasValue()) {
                    instance.valueOfNominalRange = current.VALNMR();

                    if (current.VALNMR() >= 10.0m) {
                        instance.majorLight = true;
                    }
                }

                if (current.MARSYS_HasValue()) {
                    instance.marksNavigationalSystemOf = EnumHelper.GetEnumValue(current.MARSYS());
                }

                if (current.MLTYLT_HasValue()) {
                    instance.multiplicityOfFeatures = new multiplicityOfFeatures() {
                        multiplicityKnown = true,
                        numberOfFeatures = current.MLTYLT()
                    };
                }

                DateHelper.TryGetPeriodicDateRange(current.PERSTA(), current.PEREND(), out var periodicDateRange);
                if (periodicDateRange != default) {
                    instance.periodicDateRange = periodicDateRange;
                }

                instance.rhythmOfLight = ImporterNIS.GetRythmOfLight<LightAllAround>(_);

                DateHelper.TryGetFixedDateRange(current.DATSTA(), current.DATEND(), out var fixedDateRange);
                if (dateRange != default) {
                    instance.fixedDateRange = fixedDateRange;
                }

                if (current.SIGGEN_HasValue()) {
                    instance.signalGeneration = EnumHelper.GetEnumValue(current.SIGGEN());
                }

                if (current.STATUS_HasValue()) {
                    var status = EnumHelper.GetEnumValues(current.STATUS(), instance.attributeBindingDefinition("status")!.permitedValues!);
                    if (status is not null && status.Any())
                        instance.status = status;
                }

                if (current.VALNMR_HasValue()) {
                    instance.valueOfNominalRange = current.VALNMR();
                }

                if (current.VERLEN_HasValue()) {
                    instance.verticalLength = current.VERLEN() != -32767m ? current.VERLEN() : null;
                    var verticalDatum = ImporterNIS.GetVerticalDatum(current.VERDAT(), current.SHAPE()!);
                    if (verticalDatum != default)
                        instance.verticalDatum = verticalDatum.value;
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
            else {
                throw new NotSupportedException($"{current.GetType()}");
            }
        }
    }
}

