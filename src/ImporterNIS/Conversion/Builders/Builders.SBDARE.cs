using ArcGIS.Core.Data;
using S100FC.S101.ComplexAttributes;
using S100FC.S101.FeatureTypes;
using S100Framework.Applications.Singletons;

namespace S100Framework.Applications
{
    using S100Framework.Applications.S57.esri;

    internal static partial class ImporterNIS
    {

        private static SeabedArea SBDARE(Feature current) {
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

            return instance;
        }
    }
}

