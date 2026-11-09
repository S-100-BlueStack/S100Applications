using ArcGIS.Core.Data;
using S100FC.S101.FeatureTypes;

namespace S100Framework.Applications
{
    using S100Framework.Applications.S57.esri;

    internal static partial class ImporterNIS
    {

        //private static VerticalDatumOfData M_VDAT(Feature current) {
        //    var instance = new VerticalDatumOfData();

        //    var verticalDatum = ImporterNIS.GetVerticalDatum(current.VERDAT(), current.SHAPE()!);
        //    if (verticalDatum != null) {
        //        var update = true;
        //        foreach (var elm in VerticalDatums.Instance.Touch(current.SHAPE()!)) {
        //            if (elm.Item2.value == verticalDatum.value) {
        //                update = false;
        //            }
        //        }
        //        if (update)
        //            instance.verticalDatum = verticalDatum.value;
        //    }

        //    var result = ImporterNIS.AddInformation(current.GetObjectID(), current.TableName(), current.NTXTDS(), current.TXTDSC(), current.INFORM(), current.NINFOM());
        //    instance.information = [.. result.information];
        //    instance.SetInformationBindings(result.InformationBindings.ToArray());

        //    buffer["ps"] = ps101;
        //    buffer["code"] = instance.GetType().Name;
        //    buffer["attributebindings"] = instance.Flatten();
        //    buffer["informationbindings"] = System.Text.Json.JsonSerializer.Serialize(instance.GetInformationBindings(), jsonSerializerOptions);

        //    buffer["sourceIdentifier"] = instance.sourceIdentifier;
        //    SetShape(buffer, current.SHAPE());
        //    SetUsageBand(buffer, current.PLTS_COMP_SCALE()!.Value);

        //    return instance;
        //}

        private static DepthArea DEPARE(Feature current) {
            var instance = new DepthArea {
                depthRangeMinimumValue = current.DRVAL1()!.Value
            };

            if (current.DRVAL2_HasValue())
                instance.depthRangeMaximumValue = current.DRVAL2() != -32767m ? current.DRVAL2() : null;

            // TODO: Spatial association to Spatial Quality

            var result = ImporterNIS.AddInformation(current.GetObjectID(), current.TableName(), current.NTXTDS(), current.TXTDSC(), current.INFORM(), current.NINFOM());
            instance.information = [.. result.information];
            instance.SetInformationBindings(result.InformationBindings.ToArray());

            return instance;
        }
    }
}

