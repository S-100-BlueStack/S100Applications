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


        private static FeatureType M_NSYS(Feature current, RowBuffer buffer) {
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

                buffer["ps"] = ps101;
                buffer["code"] = instance.GetType().Name;
                buffer["attributebindings"] = instance.Flatten();
                buffer["informationbindings"] = System.Text.Json.JsonSerializer.Serialize(instance.GetInformationBindings(), jsonSerializerOptions);

                buffer["sourceIdentifier"] = instance.sourceIdentifier;
                SetShape(buffer, current.SHAPE());
                SetCompilationScale(buffer, current.PLTS_COMP_SCALE()!.Value);

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
}

