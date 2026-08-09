using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Extensions;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.KnowledgeGraph;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuvionPro
{
    internal class PickReportTool : MapTool
    {
        public PickReportTool() {
            IsSketchTool = true;
            SketchType = SketchGeometryType.Point;
            SketchOutputMode = SketchOutputMode.Map;
            UseSnapping = false;
        }

        protected override Task OnToolActivateAsync(bool active) {
            return base.OnToolActivateAsync(active);
        }

        protected override async Task<bool> OnSketchCompleteAsync(Geometry geometry) {
            var viewModel = PickReportDockpaneViewModel.Show();
            if (viewModel is null) {
                return true;
            }

            // The view model owns cancellation, so rapid clicking supersedes rather than queues.
            await viewModel.IdentifyAsync(geometry);
            return true;




            var mv = MapView.Active;
            var identifyResult = await QueuedTask.Run(() => {
                var sb = new StringBuilder();

                // Get the features that intersect the sketch geometry.
                var features = mv.GetFeatures(geometry);

                // Get all layer definitions.
                var lyrs = mv.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>();
                foreach (var lyr in lyrs) {
                    var fCnt = features.ToDictionary().ContainsKey(lyr) ? features[lyr].Count : 0;
                    if (fCnt > 0)
                        sb.AppendLine($@"{fCnt} {(fCnt == 1 ? "record" : "records")} for {lyr.Name}");
                }
                return sb.ToString();
            });
            MessageBox.Show(identifyResult);
            return true;
        }
    }
}
