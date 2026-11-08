using ArcGIS.Core.Data;
using S100FC;
using S100FC.S101.FeatureTypes;
using S100Framework.Applications.Singletons;

namespace S100Framework.Applications
{
    using S100Framework.Applications.S57.esri;

    internal static partial class ImporterNIS
    {

        private static FeatureType ADMARE(Feature current) {
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

                return instance;
            }
            else if (!string.IsNullOrEmpty(current.INFORM()) && regexMarinePollutionRegulationsArea.IsMatch(current.INFORM()!)) {
                var instance = new MarinePollutionRegulationsArea();

                var featureName = GetFeatureName(current.OBJNAM(), current.NOBJNM());
                if (featureName is not null)
                    instance.featureName = featureName;

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

                return instance;
            }
            else {
                var instance = new AdministrationArea {
                };

                if (current.JRSDTN_HasValue()) {
                    if (EnumHelper.GetEnumValue(current.JRSDTN(), out int? jurisdiction, instance.attributeBindingDefinition("jurisdiction")!.permitedValues!))
                        instance.jurisdiction = jurisdiction;
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

                return instance;
            }
        }
    }
}

