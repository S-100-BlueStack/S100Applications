using ArcGIS.Core.Internal.CIM;
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
    internal abstract class CompilationScaleFilter : Button
    {
        protected abstract int CompilationScale { get; }

        protected abstract string Name { get; }

        //protected virtual string WhereClause => $"compilationscale = {CompilationScale} Or compilationscale <= 0";

        protected virtual string WhereClause_NIS => $"((PLTS_COMP_SCALE = {CompilationScale}) Or (IS_CONFLATE = 1 And PLTS_COMP_SCALE >= {CompilationScale}))";

        protected virtual string WhereClause_S100 => $"compilationscale = {CompilationScale} Or compilationscale <= 0";

        internal static string[] _fields = ["plts_comp_scale", "compilationscale"];

        protected override void OnClick() {
            QueuedTask.Run(() => {
                var layer = MapView.Active.GetSelectedLayers().ToList().FirstOrDefault();

                if (layer is FeatureLayer) {
                    var featureLayer = (FeatureLayer)layer;

                    var fields = featureLayer.GetFieldDescriptions();
                    if (fields.Any(e => _fields.Contains(e.Name.ToLowerInvariant())))
                        CompilationScaleFilter.SetDefinitionQuery(featureLayer, this.GetWhereClause(featureLayer), Name);
                }
                else if (layer is GroupLayer) {
                    var groupLayer = (GroupLayer)layer;
                    foreach (var l in groupLayer.GetLayersAsFlattenedList()) {
                        if (l is FeatureLayer) {
                            var featureLayer = (FeatureLayer)l;

                            var fields = featureLayer.GetFieldDescriptions();
                            if (fields.Any(e => _fields.Contains(e.Name.ToLowerInvariant())))
                                CompilationScaleFilter.SetDefinitionQuery(featureLayer, this.GetWhereClause(featureLayer), Name);
                        }
                    }
                }
            });
        }

        private string GetWhereClause(FeatureLayer layer) {
            if (layer.GetFieldDescriptions().Any(e => "PLTS_COMP_SCALE".Equals(e.Name, StringComparison.CurrentCultureIgnoreCase)))
                return WhereClause_NIS;
            return WhereClause_S100;
        }

        private static void SetDefinitionQuery(FeatureLayer layer, string whereClause, string name) {
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

            if (queries.Any(e => e.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase)))
                layer.SetActiveDefinitionQuery(name);
            else {
                if (string.IsNullOrEmpty(query))
                    layer.InsertDefinitionQuery(new DefinitionQuery(name, whereClause), true);
                else {
                    layer.InsertDefinitionQuery(new DefinitionQuery(name, $"({query}) And ({whereClause})"), true);
                }
            }
        }
    }

    internal class ButtonOverview : CompilationScaleFilter
    {
        protected override int CompilationScale => 1;

        protected override string Name => "Navigational Purpose Overview";

        protected override string WhereClause_NIS => $"(PLTS_COMP_SCALE >= {700000})";
        protected override string WhereClause_S100 => $"(compilationscale >= {700000} Or compilationscale <= 0)";
    }

    internal class ButtonGeneral : CompilationScaleFilter
    {
        protected override int CompilationScale => 2;

        protected override string Name => "Navigational Purpose General";

        protected override string WhereClause_NIS => $"((PLTS_COMP_SCALE >= {180000} AND PLTS_COMP_SCALE < {700000}) Or (IS_CONFLATE = 1 And PLTS_COMP_SCALE >= {700000}))";
        protected override string WhereClause_S100 => $"((compilationscale >= {180000} And compilationscale < {700000}) Or compilationscale <= 0)";
    }

    internal class ButtonCoastal : CompilationScaleFilter
    {
        protected override int CompilationScale => 3;

        protected override string Name => "Navigational Purpose Coastal";

        protected override string WhereClause_NIS => $"((PLTS_COMP_SCALE >= {90000} AND PLTS_COMP_SCALE < {180000}) Or (IS_CONFLATE = 1 And PLTS_COMP_SCALE >= {180000}))";
        protected override string WhereClause_S100 => $"((compilationscale >= {90000} And compilationscale < {180000}) Or compilationscale <= 0)";
    }

    internal class ButtonApproach : CompilationScaleFilter
    {
        protected override int CompilationScale => 4;

        protected override string Name => "Navigational Purpose Approach";

        protected override string WhereClause_NIS => $"((PLTS_COMP_SCALE >= {22000} AND PLTS_COMP_SCALE < {90000}) Or (IS_CONFLATE = 1 And PLTS_COMP_SCALE >= {90000}))";
        protected override string WhereClause_S100 => $"((compilationscale >= {22000} And compilationscale < {90000}) Or compilationscale <= 0)";
    }

    internal class ButtonHarbour : CompilationScaleFilter
    {
        protected override int CompilationScale => 5;

        protected override string Name => "Navigational Purpose Harbour";

        protected override string WhereClause_NIS => $"((PLTS_COMP_SCALE < {22000}) Or (IS_CONFLATE = 1 And PLTS_COMP_SCALE >= {22000}))";
        protected override string WhereClause_S100 => $"(compilationscale < {22000} Or compilationscale <= 0)";
    }

    internal class ButtonBerthing : CompilationScaleFilter
    {
        protected override int CompilationScale => 6;

        protected override string Name => "Navigational Purpose Berthing";

        protected override string WhereClause_NIS => $"((PLTS_COMP_SCALE < {12000}) Or (IS_CONFLATE = 1 And PLTS_COMP_SCALE >= {12000}))";
        protected override string WhereClause_S100 => $"(compilationscale < {12000} Or compilationscale <= 0)";
    }

    internal class Button1000 : CompilationScaleFilter
    {
        protected override int CompilationScale => 1000;

        protected override string Name => "1:1.000";        
    }

    internal class Button2000 : CompilationScaleFilter
    {
        protected override int CompilationScale => 2000;

        protected override string Name => "1:2.000";        
    }

    internal class Button3000 : CompilationScaleFilter
    {
        protected override int CompilationScale => 3000;

        protected override string Name => "1:3.000";
    }
    internal class Button4000 : CompilationScaleFilter
    {
        protected override int CompilationScale => 4000;

        protected override string Name => "1:4.000";
    }
    internal class Button8000 : CompilationScaleFilter
    {
        protected override int CompilationScale => 8000;

        protected override string Name => "1:8.000";
    }
    internal class Button12000 : CompilationScaleFilter
    {
        protected override int CompilationScale => 12000;

        protected override string Name => "1:12.000";
    }
    internal class Button22000 : CompilationScaleFilter
    {
        protected override int CompilationScale => 22000;

        protected override string Name => "1:22.000";
    }
    internal class Button45000 : CompilationScaleFilter
    {
        protected override int CompilationScale => 1;

        protected override string Name => "Navigational Purpose Overview";
    }
    internal class Button90000 : CompilationScaleFilter
    {
        protected override int CompilationScale => 90000;

        protected override string Name => "1:90.000";
    }
    internal class Button180000 : CompilationScaleFilter
    {
        protected override int CompilationScale => 180000;

        protected override string Name => "1:180.000";
    }
    internal class Button350000 : CompilationScaleFilter
    {
        protected override int CompilationScale => 350000;

        protected override string Name => "1:350.000";
    }
    internal class Button700000 : CompilationScaleFilter
    {
        protected override int CompilationScale => 700000;

        protected override string Name => "1:700.000";
    }
    internal class Button1500000 : CompilationScaleFilter
    {
        protected override int CompilationScale => 1500000;

        protected override string Name => "1:1.500.000";
    }
    internal class Button3500000 : CompilationScaleFilter
    {
        protected override int CompilationScale => 3500000;

        protected override string Name => "1:3.500.000";
    }
    internal class Button10000000 : CompilationScaleFilter
    {
        protected override int CompilationScale => 10000000;

        protected override string Name => "1:10.000.000";
    }

    internal class ButtonClearCompilationScaleFiltere : Button
    {
        protected override async void OnClick() {
            await QueuedTask.Run(() => {
                var layer = MapView.Active.GetSelectedLayers().ToList().FirstOrDefault();

                if (layer is FeatureLayer) {
                    var featureLayer = (FeatureLayer)layer;

                    ClearCompilationScaleDefinitionQuery(featureLayer);
                }
                else if (layer is GroupLayer) {
                    var groupLayer = (GroupLayer)layer;
                    foreach (var l in groupLayer.GetLayersAsFlattenedList()) {
                        if (l is FeatureLayer) {
                            var featureLayer = (FeatureLayer)l;
                            ClearCompilationScaleDefinitionQuery(featureLayer);
                        }
                    }
                }
            });
        }

        private void ClearCompilationScaleDefinitionQuery(FeatureLayer layer) {
            var fields = layer.GetFieldDescriptions();
            if (!fields.Any(e => CompilationScaleFilter._fields.Contains(e.Name.ToLowerInvariant())))
                return;

            var queries = layer.DefinitionQueries;

            try {
                layer.RemoveActiveDefinitionQuery();
                var q = queries.SingleOrDefault(e => e.Name.Equals("Default", StringComparison.CurrentCultureIgnoreCase));
                if (q != default) {
                    layer.SetActiveDefinitionQuery(q.Name);
                }
                //else {
                //    layer.RemoveActiveDefinitionQuery();
                //}
            }
            catch (System.InvalidOperationException ex) {
                Logger.Current.Error(ex, "ClearCompilationScaleDefinitionQuery({layer})", layer.Name);
            }
        }
    }

    internal class DynamicMenuCompilationScale : DynamicMenu
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
            this.AddReference("NuvionPro_ButtonClearCompilationScale");
        }

        protected override void OnUpdate() {
            base.OnUpdate();
        }
    }
}
