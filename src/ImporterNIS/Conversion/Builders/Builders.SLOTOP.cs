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

        private static SlopeTopline SLOTOP(Feature current, RowBuffer buffer) {
            var instance = new SlopeTopline();

            if (current.CATSLO_HasValue()) {
                if (EnumHelper.GetEnumValue(current.CATSLO(), out int? categoryOfSlope, instance.attributeBindingDefinition("categoryOfSlope")!.permitedValues!))
                    instance.categoryOfSlope = categoryOfSlope;
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

            if (current.NATSUR_HasValue()) {
                var natureOfSurface = EnumHelper.GetEnumValues(current.NATSUR(), instance.attributeBindingDefinition("natureOfSurface")!.permitedValues!);
                if (natureOfSurface is not null && natureOfSurface.Any())
                    instance.natureOfSurface = natureOfSurface;
            }

            if (current.CONRAD_HasValue()) {
                instance.radarConspicuous = current.CONRAD() == 2 ? false : true;
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

