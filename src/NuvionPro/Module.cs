using ArcGIS.Core.Data;
using ArcGIS.Core.Events;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.Mapping.Events;
//using S100FC.Catalogues;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace NuvionPro
{
    internal class Module : ArcGIS.Desktop.Framework.Contracts.Module
    {
        private static Module _this = null;

        internal static List<string> RegisteredFeatureLayers = [];

        /// <summary>
        /// Retrieve the singleton instance to this module here
        /// </summary>
        public static Module Current => _this ??= (Module)FrameworkApplication.FindModule("NuvionPro_Module");

        private SubscriptionToken _tokenActiveMapViewChangedEvent;

        public record FeatureCatalogue(string Name, string FullPath)
        {
            public string ID => $"S-{this.Name.Substring(0, 3)}";
        }

        private ImmutableArray<FeatureCatalogue> _featureCatalogues = ImmutableArray<FeatureCatalogue>.Empty;

        public FeatureCatalogue[] GetFeatureCatalogues() => this._featureCatalogues.ToArray();

        public string[] GetFeatureCatalogueNames() => this._featureCatalogues.Select(e => e.Name).ToArray();

        public FeatureCatalogue GetFeatureCatalogue(string name) => this._featureCatalogues.Single(e => e.ID.Equals(name));

        /// <summary>
        /// A new MapView is incoming
        /// </summary>
        /// <param name="args"></param>
        private static async void OnActiveMapViewChanged(ActiveMapViewChangedEventArgs args) {
            Map incomingMap = args?.IncomingView?.Map;
            if (incomingMap == null)
                return;

            foreach (var table in incomingMap.GetStandaloneTablesAsFlattenedList()) {
                if (table.Name.Equals("attributebinding", StringComparison.OrdinalIgnoreCase))
                    continue;
                await RegisterStandaloneTableGuidAsync(table);
            }

            // process each layer to see if the layer needs the custom attribute tab
            foreach (var featLayer in incomingMap.GetLayersAsFlattenedList().OfType<FeatureLayer>()) {
                await RegisterFeatureClassGuidAsync(featLayer);
            }
        }

        #region Overrides

        protected override bool Initialize() {
            this._tokenActiveMapViewChangedEvent = ActiveMapViewChangedEvent.Subscribe(OnActiveMapViewChanged);
            //this._featureCatalogues = FeatureCatalogue.Catalogues;

            string path = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            foreach (var catalogue in System.IO.Directory.GetFiles(System.IO.Path.Combine(path, "GeospatialInformationRegistry"), "*FC*.xml")) {
                this._featureCatalogues = [.. this._featureCatalogues, new FeatureCatalogue(System.IO.Path.GetFileNameWithoutExtension(catalogue), System.IO.Path.GetFullPath(catalogue))];
            }

            return base.Initialize();
        }

        protected override void Uninitialize() {
            ActiveMapViewChangedEvent.Unsubscribe(this._tokenActiveMapViewChangedEvent);
            base.Uninitialize();
        }

        /// <summary>
        /// Called by Framework when ArcGIS Pro is closing
        /// </summary>
        /// <returns>False to prevent Pro from closing, otherwise True</returns>
        protected override bool CanUnload() {
            //TODO - add your business logic
            //return false to ~cancel~ Application close
            return true;
        }

        #endregion Overrides

        #region Register Guid with Feature Class to add Custom Tab to Edit Attributes Dockpane

        /// <summary>
        /// Register Feature Class with GUID defined in config.daml categories
        /// </summary>
        /// <param name="layer"></param>
        internal static Task RegisterFeatureClassGuidAsync(FeatureLayer layer) {
            return QueuedTask.Run(() => {
                //note: These methods must be called on the Main CIM Thread. Use QueuedTask.Run.
                var fc = layer.GetFeatureClass();
                if (fc is null)
                    return;

                var metadata = layer.GetMetadata();                              
                if (!metadata.Contains("<keyword>vortex</keyword>"))
                    return;

                var fcName = fc.GetName();
                //if (!fcName.Equals(Module1.TaxParcelPolygonLayerName)) 
                //    return;

                var table = layer.GetTable();

                var join = table.IsJoinedTable() ? table.GetJoin() : default;


                // This Guid defines the "Custom Tab" in the config.daml                
                Guid extension = new("{981f7707-9c4e-4b1c-9ecd-9d63946e943d}");

                if (!fc.GetHasActivationExtension(extension)) {
                    CoreDataExtensions.AddActivationExtension(fc, extension);
                }

                // remember the registration
                if (!RegisteredFeatureLayers.Contains(fcName)) {
                    RegisteredFeatureLayers.Add(fcName);
                }
            });
        }

        /// <summary>
        /// Register Standalone Table with GUID defined in config.daml categories
        /// </summary>
        /// <param name="featureLayer"></param>
        internal static Task RegisterStandaloneTableGuidAsync(StandaloneTable layer) {
            return QueuedTask.Run(() => {
                var table = layer.GetTable();
                if (table is null)
                    return;

                var metadata = layer.GetMetadata();
                if (!metadata.Contains("<keyword>nuvion</keyword>"))
                    return;

                var fcName = table.GetName();

                var join = table.IsJoinedTable() ? table.GetJoin() : default;

                // This Guid defines the "Custom Tab" in the config.daml                
                Guid extension = new("{981f7707-9c4e-4b1c-9ecd-9d63946e943d}");

                if (!table.GetHasActivationExtension(extension)) {
                    CoreDataExtensions.AddActivationExtension(table, extension);
                }

                // remember the registration
                if (!RegisteredFeatureLayers.Contains(fcName)) {
                    RegisteredFeatureLayers.Add(fcName);
                }
            });
        }


        #endregion
    }
}
