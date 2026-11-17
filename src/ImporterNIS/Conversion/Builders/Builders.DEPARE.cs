using ArcGIS.Core.Data;
using S100FC.S101.FeatureTypes;

namespace S100Framework.Applications
{
    using S100Framework.Applications.S57.esri;

    internal static partial class ImporterNIS
    {

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

