using ArcGIS.Core.Data;
using S100FC;
using S100FC.S101.FeatureTypes;
using S100Framework.Applications.Singletons;

namespace S100Framework.Applications
{
    using S100Framework.Applications.S57.esri;

    internal static partial class ImporterNIS
    {


        private static FeatureType M_NSYS(Feature current) {
            if (current.ORIENT_HasValue()) {
                var instance = new LocalDirectionOfBuoyage();

                if (current.MARSYS_HasValue()) {
                    if (EnumHelper.GetEnumValue(current.MARSYS(), out int? marksNavigationalSystemOf, instance.attributeBindingDefinition("marksNavigationalSystemOf")!.permitedValues!))
                        instance.marksNavigationalSystemOf = marksNavigationalSystemOf;
                }

                instance.orientationValue = current.ORIENT() != -32767m ? current.ORIENT() : null;

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
            else {
                var instance = new NavigationalSystemOfMarks();

                if (current.MARSYS_HasValue()) {
                    if (EnumHelper.GetEnumValue(current.MARSYS(), out int? marksNavigationalSystemOf, instance.attributeBindingDefinition("marksNavigationalSystemOf")!.permitedValues!))
                        instance.marksNavigationalSystemOf = marksNavigationalSystemOf;
                }
                else {
                    Logger.Current.DataError(current.GetObjectID(), current.TableName() ?? "Unknown tablename", current.LNAM() ?? "Unknown LNAM", $"Missing MARSYS value for M_NSYS where globalid = '{{{current.GLOBALID}}}'");
                }
                var result = ImporterNIS.AddInformation(current.GetObjectID(), current.TableName(), current.NTXTDS(), current.TXTDSC(), current.INFORM(), current.NINFOM());
                instance.information = [.. result.information];
                instance.SetInformationBindings(result.InformationBindings.ToArray());

                return instance;
            }
        }
    }
}

