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

        private static LandRegion LNDRGN(Feature current, RowBuffer buffer) {
            var instance = new LandRegion();

            bool hasCATLND = false;
            bool hasFeatureName = false;

            if (current.CATLND_HasValue()) {
                var categoryOfLandRegion = EnumHelper.GetEnumValues(current.CATLND(), instance.attributeBindingDefinition("categoryOfLandRegion")!.permitedValues!);
                if (categoryOfLandRegion is not null && categoryOfLandRegion.Any()) {
                    instance.categoryOfLandRegion = categoryOfLandRegion;
                    hasCATLND = true;
                }
            }

            var featureName = GetFeatureName(current.OBJNAM(), current.NOBJNM());
            if (featureName is not null) {
                instance.featureName = featureName;
                hasFeatureName = true;
            }

            if (current.NATSUR_HasValue()) {
                var natureOfSurface = EnumHelper.GetEnumValues(current.NATSUR(), instance.attributeBindingDefinition("natureOfSurface")!.permitedValues!);
                if (natureOfSurface is not null && natureOfSurface.Any())
                    instance.natureOfSurface = natureOfSurface;
            }

            if (current.WATLEV_HasValue()) {
                if (EnumHelper.GetEnumValue(current.WATLEV(), out int? waterLevelEffect, instance.attributeBindingDefinition("waterLevelEffect")!.permitedValues!))
                    instance.waterLevelEffect = waterLevelEffect;
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

