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
using System.Threading;
using System.Threading.Tasks;

namespace NuvionPro
{
    internal class PickReportDockpaneViewModel : DockPane
    {
        private const string _dockPaneID = "NuvionPro_PickReportDockpane";

        private readonly object _cancellationLock = new object();

        private CancellationTokenSource? _cancellationTokenSource = default;

        private bool _isBusy;

        protected PickReportDockpaneViewModel() { }

        /// <summary>
        /// Show the DockPane.
        /// </summary>
        internal static PickReportDockpaneViewModel? Show() {
            DockPane pane = FrameworkApplication.DockPaneManager.Find(_dockPaneID);
            if (pane == null)
                return null;

            pane.Activate();

            return pane as PickReportDockpaneViewModel;
        }

        /// <summary>
        /// Text shown near the top of the DockPane.
        /// </summary>
        private string _heading = "Feature Information";
        public string Heading {
            get => _heading;
            set => SetProperty(ref _heading, value);
        }

        public bool IsBusy {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        public async Task IdentifyAsync(Geometry sketchGeometry) {
            CancellationTokenSource cts;
            lock (_cancellationLock) {
                _cancellationTokenSource?.Cancel();
                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = cts = new CancellationTokenSource();
            }

            var token = cts.Token;

            IsBusy = true;
            try {
                var mapView = MapView.Active;

                var identifyResult = await QueuedTask.Run(() => {
                    var sb = new StringBuilder();

                    // Get the features that intersect the sketch geometry.
                    var features = mapView.GetFeatures(sketchGeometry);

                    var dictionary = features.ToDictionary();

                    // Get all layer definitions.
                    var lyrs = mapView.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>();
                    foreach (var lyr in lyrs) {
                        var fCnt = dictionary.ContainsKey(lyr) ? features[lyr].Count : 0;
                        if (fCnt > 0)
                            sb.AppendLine($@"{fCnt} {(fCnt == 1 ? "record" : "records")} for {lyr.Name}");
                    }
                    return sb.ToString();
                });
            }
            catch (OperationCanceledException) {
                // Superseded by a newer click: leave the pane showing whatever arrives next.
            }
            finally {
                if (!token.IsCancellationRequested) {
                    IsBusy = false;
                }
            }
        }
    }

    /// <summary>
    /// Button implementation to show the DockPane.
    /// </summary>
    internal class PickReportDockpane_ShowButton : Button
    {
        protected override void OnClick() {
            PickReportDockpaneViewModel.Show();
        }
    }
}
