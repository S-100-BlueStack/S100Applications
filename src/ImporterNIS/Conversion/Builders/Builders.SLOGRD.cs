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

        private static SlopingGround SLOGRD(Feature current, RowBuffer buffer) {
            var instance = new SlopingGround();

            if (current.CATSLO_HasValue()) {
                if ((current.CATSLO() == 3 || current.CATSLO() == 4) && (!string.IsNullOrEmpty(current.NATSUR()) && "4".Equals(current.NATSUR())))
                    throw new NotImplementedException();    //  If it is required to encode a sand dune or sand hill, it must be done using the feature Sloping Ground with attribute category of slope = 3 (dune) or 4 (hill) and attribute nature of surface = 4 (sand). If these features are positioned along the coastline, a Coastline feature must also be encoded.

                if (EnumHelper.GetEnumValue(current.CATSLO(), out int? categoryOfSlope, instance.attributeBindingDefinition("categoryOfSlope")!.permitedValues!))
                    instance.categoryOfSlope = categoryOfSlope;
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
                if (EnumHelper.GetEnumValue(current.CONVIS(), out int? visualProminence, instance.attributeBindingDefinition("visualProminence")!.permitedValues!))
                    instance.visualProminence = visualProminence;
            }

            if (current.PLTS_COMP_SCALE_HasValue()) {
                string subtype = "";

                if (!Subtypes.Instance.TryGetSubtype(current.TableName(), current.FCSubtype(), out subtype))
                    throw new NotSupportedException($"Unknown subtype for {current.TableName}, {current.FCSubtype()}");

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
            SetCompilationScale(buffer, current.PLTS_COMP_SCALE()!.Value);

            return instance;
        }
    }
}

