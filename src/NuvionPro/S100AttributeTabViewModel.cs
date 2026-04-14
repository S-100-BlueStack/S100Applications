using ArcGIS.Core;
using ArcGIS.Core.Data;
using ArcGIS.Core.Events;

using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Editing.Attributes;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using S100FC;
using S100Framework.WPF.ViewModel;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Xml.Linq;

namespace NuvionPro
{
    //  https://github.com/esri/arcgis-pro-sdk/wiki/ProConcepts-Editing#customizing-the-attributes-dockpane

    internal class S100AttributeTabViewModel : AttributeTabEmbeddableControl
    {
        const string S100AttributesUpdate = "S100AttributesUpdate";

        private static readonly CultureInfo culture = new("en-GB", false);

        private readonly NuvionPro.Module _module;

        private Module.FeatureCatalogue _ps = default;

        private string? _code = default;

        private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions {
            WriteIndented = false,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            PropertyNameCaseInsensitive = true,
        };

        private S100AttributeEditorViewModel _selectedProperty = default;

        private Boolean _isEditingEnabled = false;

        private Boolean _isVisible = false;

        private bool _isLocked = false;

        private readonly SubscriptionToken _tokenEditStarted;

        public S100AttributeTabViewModel(XElement options, bool canChangeOptions) : base(options, canChangeOptions) {
            this._module = NuvionPro.Module.Current;

            Project.Current.PropertyChanged += this.Current_PropertyChanged;
            this.IsEditingEnabled = Project.Current.IsEditingEnabled;

            this.FeatureCatalogues = this._module.GetFeatureCatalogues();

            this.CreateInstance = new ArcGIS.Desktop.Framework.RelayCommand(async () => {
                var inspector = base.Inspector;

                if (inspector != default) {
                    await QueuedTask.Run(() => {
                        var sourceidentifier = this.SelectedProperty.GetElement(this.Code).GetSourceIdentifier();

                        if (sourceidentifier.HasValue && sourceidentifier.Value != Convert.ToInt32(inspector["sourceidentifier"])) {
                            var changed = inspector.ChangeSubtype(sourceidentifier.Value, true);
                        }
                        inspector["ps"] = this.PS.ID;
                        inspector["code"] = this.Code;
                        inspector["attributebindings"] = "{}";
                        if (inspector.HasAttribute("informationbindings"))
                            inspector["informationbindings"] = "[]";
                        if (inspector.HasAttribute("featurebindings"))
                            inspector["featurebindings"] = "[]";

                        inspector.Apply();
                    }, TaskCreationOptions.None);
                }
            });
        }

        private void Current_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            if (e.PropertyName == "IsEditingEnabled") {
                this.IsEditingEnabled = Project.Current.IsEditingEnabled;
            }
        }

        protected override void NotifyPropertyChanged([CallerMemberName] string name = "") {
            var inspector = base.Inspector;

            if (string.IsNullOrEmpty(name)) return;

            base.NotifyPropertyChanged(name);

            switch (name) {
                default:
                    return; // SKIP ALL OTHERS

                case nameof(this.SelectedProperty):
                    this.NotifyPropertyChanged(() => this._psSelectorIsEnabled);
                    this.NotifyPropertyChanged(() => this._codeSelectorIsEnabled);
                    break;

                case nameof(this.PS):
                    this.PS_Inspector(inspector);
                    this.NotifyPropertyChanged(() => this._psSelectorIsEnabled);

                    this.Code = default;
                    break;

                case nameof(this.Code):
                    this.Code_Inspector(inspector);
                    this.NotifyPropertyChanged(() => this._codeSelectorIsEnabled);
                    break;
            }
            this.NotifyPropertyChanged(() => this._createButtonIsEnabled);
        }

        private void PS_Inspector(Inspector inspector) {
            if (this.PS != default) {
                var schema = this.PS;

                if (this.SelectedProperty is null) {
                    var ps = XDocument.Load(this.PS.FullPath);
                    this.SelectedProperty = new S100AttributeEditorViewModel(ps);
                }
                else if (this.SelectedProperty is not null && string.IsNullOrEmpty(this.SelectedProperty.ProductID)) {
                    if (!this.PS.Equals(this.SelectedProperty.ProductID)) {
                        var ps = XDocument.Load(this.PS.FullPath);
                        this.SelectedProperty = new S100AttributeEditorViewModel(ps);
                    }
                }

                var sourceIdentifiers = this.SelectedProperty.sourceIdentifiers;

                if (inspector.HasGeometry) {
                    var geometryType = inspector.MapMember switch {
                        FeatureLayer l => l.ShapeType,
                        _ => throw new InvalidOperationException(),
                    };

                    var primitive = geometryType switch {
                        ArcGIS.Core.CIM.esriGeometryType.esriGeometryPolygon => Primitives.surface,
                        ArcGIS.Core.CIM.esriGeometryType.esriGeometryPolyline => Primitives.curve,
                        ArcGIS.Core.CIM.esriGeometryType.esriGeometryPoint => Primitives.point,
                        ArcGIS.Core.CIM.esriGeometryType.esriGeometryMultipoint => Primitives.pointSet,
                        _ => throw new InvalidOperationException(),
                    };

                    var items = this.SelectedProperty.GetFeaturesByPrimitive(primitive).OrderBy(e => e);
                    this.Codes = items.ToArray();
                }
                else {
                    if (inspector.HasAttribute("featurebindings")) {
                        var items = this.SelectedProperty.GetFeaturesByPrimitive(Primitives.noGeometry).OrderBy(e => e);
                        this.Codes = items.ToArray();
                    }
                    else {
                        var items = this.SelectedProperty.InformationTypes.OrderBy(e => e);
                        this.Codes = items.ToArray();
                    }
                }

                //System.Windows.Application.Current.Dispatcher.Invoke(() => {
                //    this.Codes = codes;
                //    this.Codes.AddRange(codes.OrderBy(e => e));
                //});
            }

        }

        private void Code_Inspector(Inspector inspector) {

        }

        public override bool Applies(MapMember mapMember) {
            return true;
        }

        public override bool IsDefault => true;

        public override async Task CancelAsync() {
            base.Inspector.Cancel();

            await this.LoadFromFeaturesAsync();
            await base.CancelAsync();
        }

        public override async Task LoadFromFeaturesAsync() {
            var inspector = base.Inspector;

            var model = base.Model;

            if (!inspector.HasAttributes)
                return;

            var primitime = inspector.Shape?.GeometryType switch {
                ArcGIS.Core.Geometry.GeometryType.Point => Primitives.point,
                ArcGIS.Core.Geometry.GeometryType.Multipoint => Primitives.pointSet,
                ArcGIS.Core.Geometry.GeometryType.Polyline => Primitives.curve,
                ArcGIS.Core.Geometry.GeometryType.Polygon => Primitives.surface,
                _ => Primitives.noGeometry,
            };

            try {
                var catalogues = await QueuedTask.Run(() => {
                    var fc = inspector.MapMember switch {
                        FeatureLayer l => l.GetFeatureClass(),
                        StandaloneTable t => t.GetTable(),
                        _ => throw new InvalidOperationException(),
                    };

                    using var geodatabase = (Geodatabase)fc.GetDatastore();

                    var syntax = geodatabase.GetSQLSyntax();
                    var tableNames = syntax.ParseTableName(fc.GetName());

                    Module.FeatureCatalogue[] featureCatalogues = [];

                    using var configuration = geodatabase.OpenDataset<Table>(syntax.QualifyTableName(tableNames.Item1, tableNames.Item2, "configuration"));

                    using var cursor = configuration.Search(null, true);
                    while (cursor.MoveNext()) {
                        var settings = JsonSerializer.Deserialize<S100BlueStack.Settings.Editor>(Convert.ToString(cursor.Current["json"]));
                        if (!settings.ExcludeInEditor && System.IO.File.Exists(settings.FullPath))
                            featureCatalogues = [.. featureCatalogues, new Module.FeatureCatalogue(Convert.ToString(cursor.Current["ps"]).Split('.').First().Substring(2), settings.FullPath)];
                    }
                    if (!featureCatalogues.Any())
                        featureCatalogues = this._module.GetFeatureCatalogues();  //  DEFAULT

                    return featureCatalogues;

                }, TaskCreationOptions.None);

                System.Windows.Application.Current.Dispatcher.Invoke(() => {
                    this.FeatureCatalogues = catalogues;

                });

                var ps = this.Inspector.IsNull("ps") ? null : Convert.ToString(inspector["ps"]);

                var update = (this.PS is null && ps is null) || (this.PS is not null && ps is null) || (!ps.Equals(this.PS?.ID));
                if (update) {
                    if (ps is null)
                        this.PS = default;
                    else {
                        var _ = this.FeatureCatalogues.SingleOrDefault(e => e.ID.Equals(ps, StringComparison.InvariantCultureIgnoreCase));
                        if (_ is not null) {
                            this.PS = this.FeatureCatalogues[Array.IndexOf(this.FeatureCatalogues, _)];
                        }
                    }
                }

                var code = Convert.ToString(inspector["code"]);
                if (!string.IsNullOrEmpty(code)) {
                    this.Code = code;
                }

                this.SelectedProperty = await QueuedTask.Run(() => {
                    if (this.PS is null) {
                        return default(S100AttributeEditorViewModel);
                    }

                    var xDocument = XDocument.Load(this.PS.FullPath);

                    var viewModel = new S100AttributeEditorViewModel(xDocument);

                    if (this.Code is null)
                        return viewModel;

                    var uid = $"{inspector.UID()}";

                    viewModel.Initialize(this.Code, uid);

                    if (!inspector.IsNull("attributebindings")) {
                        var json = Convert.ToString(inspector["attributebindings"]);

                        viewModel = viewModel.LoadAttributeBindings(json);
                    }

                    viewModel.RequestInformation = async (s, e) => {
                        if (MapView.Active is null)
                            return [];
                        return await QueuedTask.Run(() => {
                            string[] result = [];

                            foreach (var layer in MapView.Active.Map.StandaloneTables.Where(table => table.Name.EndsWith("informationtype"))) {
                                var selection = layer.GetSelection();
                                if (selection.GetCount() == 0) continue;

                                using var cursor = selection.Search(new QueryFilter {
                                    WhereClause = $"UPPER(PS) = '{this.PS}' AND UPPER(CODE) = '{e.InformationType.ToUpperInvariant()}'"
                                }, true);

                                while (cursor.MoveNext()) {
                                    result = [.. result, Convert.ToString(cursor.Current["UID"])];
                                }
                            }
                            return result;
                        });
                    };
                    viewModel.SelectInformationTypes = async (s, e) => {
                        var mapView = MapView.Active;
                        if (mapView is null) return;

                        if (e.UIDs.Any()) {
                            await QueuedTask.Run(() => {
                                var query = new QueryFilter {
                                    WhereClause = $"UID IN ({string.Join(',', e.UIDs.Select(e => $"'{e.UID}'"))})",
                                };
                                foreach (var layer in mapView.Map.StandaloneTables) {
                                    layer.Select(query, SelectionCombinationMethod.Add);
                                }
                            }, TaskCreationOptions.None);
                        }
                    };
                    if (inspector.HasAttribute("informationbindings")) {
                        if (!inspector.IsNull("informationbindings"))
                            viewModel.LoadInformationBindings(Convert.ToString(this.Inspector["informationbindings"]));
                    }

                    viewModel.RequestFeatures = async (s, e) => {
                        if (MapView.Active is null)
                            return [];
                        return await QueuedTask.Run(() => {
                            string[] result = [];

                            foreach (var layer in MapView.Active.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>()) {
                                if (layer is FeatureLayer featureLayer) {
                                    var selection = layer.GetSelection();
                                    if (selection.GetCount() == 0) continue;

                                    using var cursor = selection.Search(new QueryFilter {
                                        WhereClause = $"UPPER(PS) = '{this.PS}' AND UPPER(CODE) = '{e.FeatureType.ToUpperInvariant()}'"
                                    }, true);

                                    while (cursor.MoveNext()) {
                                        result = [.. result, Convert.ToString(cursor.Current["UID"])];
                                    }
                                }
                            }
                            foreach (var layer in MapView.Active.Map.StandaloneTables.Where(table => table.Name.EndsWith("featuretype"))) {
                                var selection = layer.GetSelection();
                                if (selection.GetCount() == 0) continue;

                                using var cursor = selection.Search(new QueryFilter {
                                    WhereClause = $"UPPER(PS) = '{this.PS}' AND UPPER(CODE) = '{e.FeatureType.ToUpperInvariant()}'"
                                }, true);

                                while (cursor.MoveNext()) {
                                    result = [.. result, Convert.ToString(cursor.Current["UID"])];
                                }
                            }
                            return result;
                        }, TaskCreationOptions.None);
                    };
                    viewModel.SelectFeatureTypes = async (s, e) => {
                        var mapView = MapView.Active;
                        if (mapView is null) return;

                        if (e.UIDs.Any()) {
                            await QueuedTask.Run(() => {
                                var query = new QueryFilter {
                                    WhereClause = $"UID IN ({string.Join(',', e.UIDs.Select(e => $"'{e.UID}'"))})",
                                };
                                foreach (var layer in mapView.Map.Layers.OfType<FeatureLayer>()) {
                                    layer.Select(query, SelectionCombinationMethod.Add);
                                }
                                foreach (var layer in mapView.Map.StandaloneTables) {
                                    layer.Select(query, SelectionCombinationMethod.Add);
                                }
                            });
                        }
                    };
                    if (inspector.HasAttribute("featurebindings")) {
                        if (!inspector.IsNull("featurebindings"))
                            viewModel.LoadFeatureBindings(Convert.ToString(this.Inspector["featurebindings"]));
                    }

                    return viewModel;
                }, TaskCreationOptions.None);

                this.IsVisible = this.SelectedProperty is null ? Visibility.Collapsed : Visibility.Visible;

                this.NotifyPropertyChanged(() => this._createButtonIsEnabled);
            }
            catch (System.Exception ex) {
                DiagnosticHelper.Error(ex);
                if (System.Diagnostics.Debugger.IsAttached) System.Diagnostics.Debugger.Break();
            }
        }

        private async void OnPropertyChanged(object sender, PropertyChangedEventArgs e) {
            await QueuedTask.Run(() => {
                var updated = false;

                if (sender is S100AttributeEditorViewModel viewModel) {
                    if (e.PropertyName.Equals(nameof(S100AttributeEditorViewModel.attributeBindings))) {
                        var flatten = JsonFlattener.Flatten([.. viewModel.attributeBindings.Select(e => e.attribute)], viewModel.attributeBindingsCatalogue);
                        if (this.Inspector.IsNull("attributebindings")) {
                            this.Inspector["attributebindings"] = flatten;
                            updated |= true;
                        }
                        else if (string.Compare(flatten, Convert.ToString(this.Inspector["attributebindings"]), true) != 0) {
                            this.Inspector["attributebindings"] = flatten;
                            updated |= true;
                        }
                    }
                    if (this.Inspector.HasAttribute("informationbindings") && e.PropertyName.Equals(nameof(S100AttributeEditorViewModel.informationBindings))) {
                        var informationBindings = (informationBinding[])viewModel;

                        var json = System.Text.Json.JsonSerializer.Serialize(informationBindings, this._jsonOptions);

                        if (this.Inspector.IsNull("informationbindings")) {
                            this.Inspector["informationbindings"] = json;
                            updated |= true;
                        }
                        else if (string.Compare(json, Convert.ToString(this.Inspector["informationbindings"]), true) != 0) {
                            this.Inspector["informationbindings"] = json;
                            updated |= true;
                        }
                    }
                    if (this.Inspector.HasAttribute("featurebindings") && e.PropertyName.Equals(nameof(S100AttributeEditorViewModel.featureBindings))) {
                        var featureBindings = (featureBinding[])viewModel;

                        var json = System.Text.Json.JsonSerializer.Serialize(featureBindings, this._jsonOptions);

                        if (this.Inspector.IsNull("featurebindings")) {
                            this.Inspector["featurebindings"] = json;
                            updated |= true;
                        }
                        else if (string.Compare(json, Convert.ToString(this.Inspector["featurebindings"]), true) != 0) {
                            this.Inspector["featurebindings"] = json;
                            updated |= true;
                        }
                    }
                }
            }, TaskCreationOptions.None);
        }

        public ICommand CreateInstance { get; set; }

        private Module.FeatureCatalogue[] _featureCatalogues = [];

        public Module.FeatureCatalogue[] FeatureCatalogues {
            get => this._featureCatalogues;
            set => this.SetProperty(ref this._featureCatalogues, value);
        }

        public bool IsLocked {
            get => this._isLocked;
            set => this.SetProperty(ref this._isLocked, value);
        }

        public Module.FeatureCatalogue PS {
            get => this._ps;
            set {
                this.SetProperty(ref this._ps, value);
            }
        }

        private string[] _codes = [];

        public string[] Codes {
            get => this._codes;
            set => this.SetProperty(ref this._codes, value);
        }

        public string? Code {
            get => this._code;
            set => this.SetProperty(ref this._code, value);
        }

        public S100AttributeEditorViewModel SelectedProperty {
            get => this._selectedProperty;
            set {
                if (this._selectedProperty is not null) {
                    this._selectedProperty.PropertyChanged -= this.OnPropertyChanged;
                }
                this.SetProperty(ref this._selectedProperty, value);
                if (this._selectedProperty is not null) {
                    this._selectedProperty.PropertyChanged += this.OnPropertyChanged;
                }
            }
        }

        public Visibility IsVisible {
            get => this._isVisible ? Visibility.Visible : Visibility.Collapsed;
            set => this.SetProperty(ref this._isVisible, value == Visibility.Visible);
        }

        public Boolean IsEditingEnabled {
            get => this._isEditingEnabled;
            set => this.SetProperty(ref this._isEditingEnabled, value);
        }

        public bool _psSelectorIsEnabled => (this.Inspector is null || this.Inspector.IsNull("attributebindings")) ? true : "{}".Equals(Convert.ToString(this.Inspector["attributebindings"]).Trim());

        public bool _codeSelectorIsEnabled => (this.Inspector is null || this.Inspector.IsNull("ps")) ? true : this.Inspector.IsNull("attributebindings") ? false : "{}".Equals(Convert.ToString(this.Inspector["attributebindings"]).Trim());

        public bool _createButtonIsEnabled => (this.Inspector is null || (this.PS is null || string.IsNullOrEmpty(this.Code))) ? false : this.Inspector.IsNull("attributebindings") ? true : "{}".Equals(Convert.ToString(this.Inspector["attributebindings"]).Trim());
    }
}

namespace ArcGIS.Desktop.Editing.Attributes
{
    public static class Extension
    {
        public static T OpenDataset<T>(this Inspector inspector, string name) where T : Dataset {
            using var fc = inspector.MapMember switch {
                FeatureLayer l => l.GetFeatureClass(),
                StandaloneTable t => t.GetTable(),
                _ => throw new InvalidOperationException(),
            };
            using var geodatabase = (Geodatabase)fc.GetDatastore();

            var syntax = geodatabase.GetSQLSyntax();
            var tableNames = syntax.ParseTableName(fc.GetName());

            return geodatabase.OpenDataset<T>(syntax.QualifyTableName(tableNames.Item1, tableNames.Item2, name));
        }
    }
}