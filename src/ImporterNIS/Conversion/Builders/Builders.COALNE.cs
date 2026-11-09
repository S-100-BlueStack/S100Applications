using ArcGIS.Core.Data;
using S100FC.S101.FeatureTypes;

namespace S100Framework.Applications
{
    using S100FC.S101.SimpleAttributes;
    using S100Framework.Applications.S57.esri;

    internal static partial class ImporterNIS
    {


        private static Coastline COALNE(Feature current) {
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
                if (EnumHelper.GetEnumValue(current.CONVIS(), out int? visualProminence, instance.attributeBindingDefinition("visualProminence")!.permitedValues!))
                    instance.visualProminence = visualProminence;
            }

            var result = ImporterNIS.AddInformation(current.GetObjectID(), current.TableName(), current.NTXTDS(), current.TXTDSC(), current.INFORM(), current.NINFOM());
            instance.information = [.. result.information];
            instance.SetInformationBindings(result.InformationBindings.ToArray());

            return instance;
        }
    }
}

