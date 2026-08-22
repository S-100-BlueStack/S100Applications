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
    internal abstract class NominalScaleFilter : Button
    {
        protected abstract int SpecificUsage { get; }

        protected abstract string Name { get; }

        protected override void OnClick() {
            QueuedTask.Run(() => {
                var layer = MapView.Active.GetSelectedLayers().ToList().FirstOrDefault();

                if (layer is FeatureLayer) {
                    var featureLayer = (FeatureLayer)layer;

                    var fields = featureLayer.GetFieldDescriptions();
                    if (fields.Any(e => e.Name.Equals("nominalscale", StringComparison.InvariantCultureIgnoreCase)))
                        NominalScaleFilter.SetDefinitionQuery(featureLayer, SpecificUsage, Name);
                }
                else if (layer is GroupLayer) {
                    var groupLayer = (GroupLayer)layer;
                    foreach (var l in groupLayer.GetLayersAsFlattenedList()) {
                        if (l is FeatureLayer) {
                            var featureLayer = (FeatureLayer)l;

                            var fields = featureLayer.GetFieldDescriptions();
                            if (fields.Any(e => e.Name.Equals("nominalscale", StringComparison.InvariantCultureIgnoreCase)))
                                NominalScaleFilter.SetDefinitionQuery(featureLayer, SpecificUsage, Name);
                        }
                    }
                }
            });
        }

        private static void SetDefinitionQuery(FeatureLayer layer, int nominalscale, string name) {
            var queries = layer.DefinitionQueries;

            var query = string.Empty;

            if (layer.ActiveDefinitionQuery != default) {
                query = layer.ActiveDefinitionQuery.Name switch {
                    "1:1.000" => string.Empty,
                    "1:2.000" => string.Empty,
                    "1:3.000" => string.Empty,
                    "1:4.000" => string.Empty,
                    "1:8.000" => string.Empty,
                    "1:12.000" => string.Empty,
                    "1:22.000" => string.Empty,
                    "1:45.000" => string.Empty,
                    "1:90.000" => string.Empty,
                    "1:180.000" => string.Empty,
                    "1:350.000" => string.Empty,
                    "1:700.000" => string.Empty,
                    "1:1.500.000" => string.Empty,
                    "1:3.500.000" => string.Empty,
                    "1:10.000.000" => string.Empty,
                    "Default" => layer.ActiveDefinitionQuery.WhereClause,
                    _ => layer.ActiveDefinitionQuery.WhereClause,
                };

                if (string.IsNullOrEmpty(query)) {
                    var q = layer.DefinitionQueries.SingleOrDefault(e => e.Name.Equals("Default", StringComparison.CurrentCultureIgnoreCase));
                    if (q != default)
                        query = q.WhereClause;
                }
            }

            var whereClause = $"nominalscale = {nominalscale} OR nominalscale = 0";

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

    internal class Button1000 : NominalScaleFilter
    {
        protected override int SpecificUsage => 1000;

        protected override string Name => "1:1.000";
    }
    internal class Button2000 : NominalScaleFilter
    {
        protected override int SpecificUsage => 2000;

        protected override string Name => "1:2.000";
    }
    internal class Button3000 : NominalScaleFilter
    {
        protected override int SpecificUsage => 3000;

        protected override string Name => "1:3.000";
    }
    internal class Button4000 : NominalScaleFilter
    {
        protected override int SpecificUsage => 4000;

        protected override string Name => "1:4.000";
    }
    internal class Button8000 : NominalScaleFilter
    {
        protected override int SpecificUsage => 8000;

        protected override string Name => "1:8.000";
    }
    internal class Button12000 : NominalScaleFilter
    {
        protected override int SpecificUsage => 12000;

        protected override string Name => "1:12.000";
    }
    internal class Button22000 : NominalScaleFilter
    {
        protected override int SpecificUsage => 22000;

        protected override string Name => "1:22.000";
    }
    internal class Button45000 : NominalScaleFilter
    {
        protected override int SpecificUsage => 1;

        protected override string Name => "Navigational Purpose Overview";
    }
    internal class Button90000 : NominalScaleFilter
    {
        protected override int SpecificUsage => 90000;

        protected override string Name => "1:90.000";
    }
    internal class Button180000 : NominalScaleFilter
    {
        protected override int SpecificUsage => 180000;

        protected override string Name => "1:180.000";
    }
    internal class Button350000 : NominalScaleFilter
    {
        protected override int SpecificUsage => 350000;

        protected override string Name => "1:350.000";
    }
    internal class Button700000 : NominalScaleFilter
    {
        protected override int SpecificUsage => 700000;

        protected override string Name => "1:700.000";
    }
    internal class Button1500000 : NominalScaleFilter
    {
        protected override int SpecificUsage => 1500000;

        protected override string Name => "1:1.500.000";
    }
    internal class Button3500000 : NominalScaleFilter
    {
        protected override int SpecificUsage => 3500000;

        protected override string Name => "1:3.500.000";
    }
    internal class Button10000000 : NominalScaleFilter
    {
        protected override int SpecificUsage => 10000000;

        protected override string Name => "1:10.000.000";
    }

    internal class ButtonClearNominalScale : Button
    {
        protected override async void OnClick() {
            await QueuedTask.Run(() => {
                var layer = MapView.Active.GetSelectedLayers().ToList().FirstOrDefault();

                if (layer is FeatureLayer) {
                    var featureLayer = (FeatureLayer)layer;

                    ClearNominalScaleDefinitionQuery(featureLayer);
                }
                else if (layer is GroupLayer) {
                    var groupLayer = (GroupLayer)layer;
                    foreach (var l in groupLayer.GetLayersAsFlattenedList()) {
                        if (l is FeatureLayer) {
                            var featureLayer = (FeatureLayer)l;
                            ClearNominalScaleDefinitionQuery(featureLayer);
                        }
                    }
                }
            });
        }

        private void ClearNominalScaleDefinitionQuery(FeatureLayer layer) {
            var fields = layer.GetFieldDescriptions();
            if (!fields.Any(e => e.Name.Equals("nominalscale", StringComparison.InvariantCultureIgnoreCase)))
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
                Logger.Current.Error(ex, "ClearNominalScaleDefinitionQuery({layer})", layer.Name);
            }
        }
    }

    internal class DynamicMenuNominalScale : DynamicMenu
    {
        protected override void OnPopup() {
            var layer = MapView.Active.GetSelectedLayers().ToList().FirstOrDefault();

            base.Enabled = layer != default;

            this.AddReference("NuvionPro_Button1000");
            this.AddReference("NuvionPro_Button2000");
            this.AddReference("NuvionPro_Button3000");
            this.AddReference("NuvionPro_Button4000");
            this.AddReference("NuvionPro_Button8000");
            this.AddReference("NuvionPro_Button12000");
            this.AddReference("NuvionPro_Button22000");
            this.AddReference("NuvionPro_Button45000");
            this.AddReference("NuvionPro_Button90000");
            this.AddReference("NuvionPro_Button180000");
            this.AddReference("NuvionPro_Button350000");
            this.AddReference("NuvionPro_Button700000");
            this.AddReference("NuvionPro_Button1500000");
            this.AddReference("NuvionPro_Button3500000");
            this.AddReference("NuvionPro_Button10000000");            
            this.AddReference("NuvionPro_ButtonClearNominalScale");
        }

        protected override void OnUpdate() {
            base.OnUpdate();
        }
    }
}
