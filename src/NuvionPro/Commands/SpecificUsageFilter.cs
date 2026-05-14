using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace NuvionPro.Commands
{
    internal abstract class SpecificUsageFilter : Button
    {
        protected abstract int SpecificUsage { get; }

        protected abstract string Name { get; }

        protected override void OnClick() {
            QueuedTask.Run(() => {
                var layer = MapView.Active.GetSelectedLayers().ToList().FirstOrDefault();

                if (layer is FeatureLayer) {
                    var featureLayer = (FeatureLayer)layer;

                    var fields = featureLayer.GetFieldDescriptions();
                    if (fields.Any(e => e.Name.Equals("specificUsage", StringComparison.InvariantCultureIgnoreCase)))
                        SpecificUsageFilter.SetDefinitionQuery(featureLayer, SpecificUsage, Name);
                }
                else if (layer is GroupLayer) {
                    var groupLayer = (GroupLayer)layer;
                    foreach (var l in groupLayer.GetLayersAsFlattenedList()) {
                        if (l is FeatureLayer) {
                            var featureLayer = (FeatureLayer)l;

                            var fields = featureLayer.GetFieldDescriptions();
                            if (fields.Any(e => e.Name.Equals("specificUsage", StringComparison.InvariantCultureIgnoreCase)))
                                SpecificUsageFilter.SetDefinitionQuery(featureLayer, SpecificUsage, Name);
                        }
                    }
                }
            });
        }

        private static void SetDefinitionQuery(FeatureLayer layer, int specificUsage, string name) {
            var queries = layer.DefinitionQueries;

            var query = string.Empty;

            if (layer.ActiveDefinitionQuery != default) {
                query = layer.ActiveDefinitionQuery.Name switch {
                    "Navigational Purpose Overview" => string.Empty,
                    "Navigational Purpose General" => string.Empty,
                    "Navigational Purpose Coastal" => string.Empty,
                    "Navigational Purpose Approach" => string.Empty,
                    "Navigational Purpose Harbour" => string.Empty,
                    "Navigational Purpose Berthing" => string.Empty,
                    "Default" => layer.ActiveDefinitionQuery.WhereClause,
                    _ => layer.ActiveDefinitionQuery.WhereClause,
                };

                if (string.IsNullOrEmpty(query)) {
                    var q = layer.DefinitionQueries.SingleOrDefault(e => e.Name.Equals("Default", StringComparison.CurrentCultureIgnoreCase));
                    if (q != default)
                        query = q.WhereClause;
                }
            }

            var whereClause = $"specificUsage = {specificUsage}";

            if (queries.Any(e => e.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase)))
                layer.SetActiveDefinitionQuery(name);
            else {
                if (string.IsNullOrEmpty(query))
                    layer.InsertDefinitionQuery(new DefinitionQuery(name, whereClause), true);
                else {
                    layer.InsertDefinitionQuery(new DefinitionQuery(name, $"({query}) AND ({whereClause})"), true);
                }
            }
        }
    }

    internal class ButtonOverview : SpecificUsageFilter
    {
        protected override int SpecificUsage => 1;

        protected override string Name => "Navigational Purpose Overview";
    }

    internal class ButtonGeneral : SpecificUsageFilter
    {
        protected override int SpecificUsage => 2;

        protected override string Name => "Navigational Purpose General";
    }

    internal class ButtonCoastal : SpecificUsageFilter
    {
        protected override int SpecificUsage => 3;

        protected override string Name => "Navigational Purpose Coastal";
    }

    internal class ButtonApproach : SpecificUsageFilter
    {
        protected override int SpecificUsage => 4;

        protected override string Name => "Navigational Purpose Approach";
    }

    internal class ButtonHarbour : SpecificUsageFilter
    {
        protected override int SpecificUsage => 5;

        protected override string Name => "Navigational Purpose Harbour";
    }

    internal class ButtonBerthing : SpecificUsageFilter
    {
        protected override int SpecificUsage => 6;

        protected override string Name => "Navigational Purpose Berthing";
    }

    internal class ButtonClear : Button
    {
        protected override async void OnClick() {
            await QueuedTask.Run(() => {
                var layer = MapView.Active.GetSelectedLayers().ToList().FirstOrDefault();

                if (layer is FeatureLayer) {
                    var featureLayer = (FeatureLayer)layer;

                    ClearDefinitionQuery(featureLayer);
                }
                else if (layer is GroupLayer) {
                    var groupLayer = (GroupLayer)layer;
                    foreach (var l in groupLayer.GetLayersAsFlattenedList()) {
                        if (l is FeatureLayer) {
                            var featureLayer = (FeatureLayer)l;
                            ClearDefinitionQuery(featureLayer);
                        }
                    }
                }
            });
        }

        private void ClearDefinitionQuery(FeatureLayer layer) {
            var fields = layer.GetFieldDescriptions();
            if (!fields.Any(e => e.Name.Equals("specificUsage", StringComparison.InvariantCultureIgnoreCase)))
                return;

            var queries = layer.DefinitionQueries;

            try {
                var q = queries.SingleOrDefault(e => e.Name.Equals("Default", StringComparison.CurrentCultureIgnoreCase));
                if (q != default) {
                    layer.SetActiveDefinitionQuery(q.Name);
                }
                else {
                    layer.RemoveActiveDefinitionQuery();
                }
            }
            catch (System.InvalidOperationException ex) {
                Logger.Current.Error(ex, "ClearDefinitionQuery({layer})", layer.Name);
            }
        }
    }

    internal class DynamicMenuSpecificUsage : DynamicMenu
    {
        protected override void OnPopup() {
            var layer = MapView.Active.GetSelectedLayers().ToList().FirstOrDefault();

            base.Enabled = layer != default;

            this.AddReference("NuvionPro_ButtonOverview");
            this.AddReference("NuvionPro_ButtonGeneral");
            this.AddReference("NuvionPro_ButtonCoastal");
            this.AddReference("NuvionPro_ButtonApproach");
            this.AddReference("NuvionPro_ButtonHarbour");
            this.AddReference("NuvionPro_ButtonBerthing");
            this.AddReference("NuvionPro_ButtonClear");
        }

        protected override void OnUpdate() {
            base.OnUpdate();
        }
    }
}
