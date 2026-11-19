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
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

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

            Logger.Current.Debug("S-100 attribute tab initialized with {CatalogueCount} feature catalogues; editing enabled: {IsEditingEnabled}.",
                this.FeatureCatalogues.Length, this.IsEditingEnabled);

            this.CreateInstance = new ArcGIS.Desktop.Framework.RelayCommand(async () => {
                var inspector = base.Inspector;

                try {
                    if (inspector != default) {
                        Logger.Current.Debug("Creating S-100 instance for catalogue {CatalogueId}, code {FeatureCode}.", this.PS?.ID, this.Code);
                        await QueuedTask.Run(() => {
                            var ps = XDocument.Load(this.PS.FullPath);

                            var navigator = ps.CreateNavigator();
                            navigator.MoveToFollowing(XPathNodeType.Element);

                            var scopes = navigator.GetNamespacesInScope(XmlNamespaceScope.All);

                            var namespaceManager = new XmlNamespaceManager(new NameTable());
                            foreach (var s in scopes)
                                namespaceManager.AddNamespace(s.Key, s.Value);

                            var xElement = ps.XPathSelectElement($"//S100FC:*[S100FC:code='{this.Code}']", namespaceManager);

                            var sourceidentifier = xElement.GetSourceIdentifier();

                            var constraints = this.PS.Constraints;

                            var viewModel = new S100AttributeEditorViewModel(ps, constraints.ToLookup(e => e.Attribute("code").Value));

                            viewModel.Initialize(this.Code, "UNKNOWN");

                            var flatten = JsonFlattener.Flatten([.. viewModel.attributeBindings.Select(e => e.attribute)], viewModel.attributeBindingsCatalogue);

                            if (sourceidentifier.HasValue && inspector.IsNull("sourceidentifier")) {
                                if (inspector.SubtypeAttribute is null)
                                    inspector["sourceidentifier"] = sourceidentifier.Value;
                                else {
                                    var changed = inspector.ChangeSubtype(sourceidentifier.Value, true);
                                }
                            }
                            else if (sourceidentifier.HasValue && sourceidentifier.Value != Convert.ToInt32(inspector["sourceidentifier"])) {
                                if (inspector.SubtypeAttribute is null)
                                    inspector["sourceidentifier"] = sourceidentifier.Value;
                                else {
                                    var changed = inspector.ChangeSubtype(sourceidentifier.Value, true);
                                }
                            }
                            inspector["ps"] = this.PS.ID;
                            inspector["code"] = this.Code;
                            inspector["attributebindings"] = flatten;
                            if (inspector.HasAttribute("informationbindings"))
                                inspector["informationbindings"] = "{}";
                            if (inspector.HasAttribute("featurebindings"))
                                inspector["featurebindings"] = "{}";

                            inspector.Apply();
                            Logger.Current.Info("Applied S-100 instance for catalogue {CatalogueId}, code {FeatureCode}, source identifier {SourceIdentifier}.",
                                this.PS.ID, this.Code, sourceidentifier);

                        }, TaskCreationOptions.None);
                    }
                    else {
                        Logger.Current.Debug("S-100 instance creation skipped because no inspector is available.");
                    }
                }
                catch (Exception ex) {
                    Logger.Current.Error(ex, "Failed to create S-100 instance for catalogue {CatalogueId}, code {FeatureCode}.", this.PS?.ID, this.Code);
                    throw;
                }
            });
        }

        private void Current_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            if (e.PropertyName == "IsEditingEnabled") {
                this.IsEditingEnabled = Project.Current.IsEditingEnabled;
                Logger.Current.Debug("Project editing enabled changed to {IsEditingEnabled}.", this.IsEditingEnabled);
            }
        }

        protected override void NotifyPropertyChanged([CallerMemberName] string name = "") {
            var inspector = base.Inspector;

            if (string.IsNullOrEmpty(name)) return;

            base.NotifyPropertyChanged(name);

            switch (name) {
                case nameof(this.PS): {
                        if (this.PS is not null) {
                            Logger.Current.Debug("Loading available S-100 types from catalogue {CatalogueId} at {CataloguePath}.", this.PS.ID, this.PS.FullPath);
                            var ps = XDocument.Load(this.PS.FullPath);

                            var navigator = ps.CreateNavigator();
                            navigator.MoveToFollowing(XPathNodeType.Element);

                            var scopes = navigator.GetNamespacesInScope(XmlNamespaceScope.All);

                            var namespaceManager = new XmlNamespaceManager(new NameTable());
                            foreach (var s in scopes)
                                namespaceManager.AddNamespace(s.Key, s.Value);

                            var constraints = this.PS.Constraints;

                            if (inspector is not null) {
                                var permittedPrimitives = ps.Descendants(XName.Get("S100_FC_FeatureType", scopes["S100FC"])).ToDictionary(
                                    e => e.Element(XName.Get("code", scopes["S100FC"]))!.Value,
                                    e => e.Elements(XName.Get("permittedPrimitives", scopes["S100FC"])).Select(e => e.Value).ToArray());

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

                                    var items = permittedPrimitives.Where(e => e.Value.Contains($"{primitive}")).Select(e => e.Key);
                                    this.Codes = items.ToArray();
                                }
                                else {
                                    if (inspector.HasAttribute("featurebindings")) {
                                        var items = permittedPrimitives.Where(e => e.Value.Contains($"{Primitives.noGeometry}")).Select(e => e.Key);
                                        this.Codes = items.ToArray();
                                    }
                                    else {
                                        var items = ps.Descendants(XName.Get("S100_FC_InformationType", scopes["S100FC"])).Select(e => e.Element(XName.Get("code", scopes["S100FC"]))!.Value);
                                        this.Codes = items.ToArray();
                                    }
                                }
                            }
                        }

                        if (this.PS is not null && inspector is not null) {
                            Logger.Current.Debug("S-100 catalogue selection processed: {CatalogueId}; available type count: {TypeCount}.", this.PS.ID, this.Codes.Length);
                        }
                        this.NotifyPropertyChanged(() => this.CREATE_ButtonIsEnabled);
                    }
                    break;

                case nameof(this.Code): {
                        this.NotifyPropertyChanged(() => this.CREATE_ButtonIsEnabled);
                    }
                    break;

                case nameof(this.SelectedProperty): {
                        this.IsVisible = this.SelectedProperty is null ? Visibility.Collapsed : Visibility.Visible;
                    }
                    break;

                default:
                    return; // SKIP ALL OTHERS              
            }
        }

        public override bool Applies(MapMember mapMember) {
            return true;
        }

        public override bool IsDefault => true;

        public override async Task CancelAsync() {
            Logger.Current.Debug("Cancelling S-100 attribute edits for catalogue {CatalogueId}, code {FeatureCode}.", this.PS?.ID, this.Code);
            try {
                base.Inspector.Cancel();

                await this.LoadFromFeaturesAsync();
                await base.CancelAsync();
                Logger.Current.Debug("S-100 attribute edit cancellation completed.");
            }
            catch (Exception ex) {
                Logger.Current.Error(ex, "Failed to cancel S-100 attribute edits.");
                throw;
            }
        }

        public override async Task LoadFromFeaturesAsync() {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            Logger.Current.Debug("Loading S-100 attributes from the current inspector.");

            var inspector = base.Inspector;

            var model = base.Model;

            if (!inspector.HasAttributes) {
                Logger.Current.Debug("S-100 attribute loading skipped because the inspector has no attributes.");
                this.SelectedProperty = null;
                this.Code = null;
                this.PS = null;
                return;
            }

            if (inspector.MapMember is null) {
                Logger.Current.Debug("S-100 attribute loading skipped because no map member is available.");
                this.SelectedProperty = null;
                this.Code = null;
                this.PS = null;
                return;
            }

            var primitime = inspector.Shape?.GeometryType switch {
                ArcGIS.Core.Geometry.GeometryType.Point => Primitives.point,
                ArcGIS.Core.Geometry.GeometryType.Multipoint => Primitives.pointSet,
                ArcGIS.Core.Geometry.GeometryType.Polyline => Primitives.curve,
                ArcGIS.Core.Geometry.GeometryType.Polygon => Primitives.surface,
                _ => Primitives.noGeometry,
            };

            try {
                var catalogues = inspector.MapMember is null ? this._module.GetFeatureCatalogues() : await QueuedTask.Run(() => {
                    var fc = inspector.MapMember switch {
                        FeatureLayer l => l.GetFeatureClass(),
                        StandaloneTable t => t.GetTable(),
                        _ => throw new InvalidOperationException(),
                    };

                    using var geodatabase = (Geodatabase)fc.GetDatastore();

                    var syntax = geodatabase.GetSQLSyntax();
                    var tableNames = syntax.ParseTableName(fc.GetName());

                    Module.FeatureCatalogue[] featureCatalogues = [];

                    var definitions = geodatabase.GetDefinitions<TableDefinition>().ToLookup(e => e.GetName());

                    using var configuration = geodatabase.OpenDataset<Table>(definitions.Single(e => e.Key.EndsWith("configuration")).Key);

                    using var cursor = configuration.Search(null, true);
                    while (cursor.MoveNext()) {
                        var settings = JsonSerializer.Deserialize<S100BlueStack.Settings.Editor>(Convert.ToString(cursor.Current["json"]));
                        if (!settings.ExcludeInEditor && !System.IO.File.Exists(settings.FullPath)) {
                            Logger.Current.Warn("Configured S-100 feature catalogue is unavailable at {CataloguePath}.", settings.FullPath);
                        }
                        if (!settings.ExcludeInEditor && System.IO.File.Exists(settings.FullPath)) {
                            var featureCatalogue = new Module.FeatureCatalogue(Convert.ToString(cursor.Current["ps"]).Split('.').First().Substring(2), settings.FullPath);
                            var constraints = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(settings.FullPath), $"{System.IO.Path.GetFileNameWithoutExtension(settings.FullPath)}.constraints.xml");
                            if (System.IO.File.Exists(constraints)) {
                                featureCatalogue.Constraints = [.. XDocument.Load(constraints).Descendants("Rule")];
                            }

                            featureCatalogues = [.. featureCatalogues, featureCatalogue];
                        }
                    }
                    if (!featureCatalogues.Any()) {
                        Logger.Current.Debug("No usable S-100 catalogues found in geodatabase configuration; using module defaults.");
                        featureCatalogues = this._module.GetFeatureCatalogues();  //  DEFAULT
                    }
                    Logger.Current.Debug("Resolved {CatalogueCount} S-100 feature catalogues.", featureCatalogues.Length);

                    return featureCatalogues;

                }, TaskCreationOptions.None);

                System.Windows.Application.Current.Dispatcher.Invoke(() => {
                    this.FeatureCatalogues = catalogues;

                });

                var ps = this.Inspector.IsNull("ps") ? null : Convert.ToString(inspector["ps"]);

                if (ps is null)
                    this.PS = default;
                else {
                    var _ = this.FeatureCatalogues.FirstOrDefault(e => e.ID.Equals(ps, StringComparison.InvariantCultureIgnoreCase));
                    if (_ is not null) {
                        this.PS = this.FeatureCatalogues[Array.IndexOf(this.FeatureCatalogues, _)];
                    }
                    else {
                        Logger.Current.Warn("Cannot load S-100 attributes: catalogue {CatalogueId} is not available among {CatalogueCount} configured catalogues.",
                            ps, this.FeatureCatalogues.Length);
                        this.SelectedProperty = null;
                        return;
                    }
                }

                var code = Convert.ToString(inspector["code"]);
                if (!string.IsNullOrEmpty(code)) {
                    this.Code = code;
                }

                if (this.PS is not null && this.Code is not null) {
                    this.SelectedProperty = await QueuedTask.Run(() => {
                        try {
                            var ps = XDocument.Load(this.PS.FullPath);

                            var navigator = ps.CreateNavigator();
                            navigator.MoveToFollowing(XPathNodeType.Element);

                            var scopes = navigator.GetNamespacesInScope(XmlNamespaceScope.All);

                            var namespaceManager = new XmlNamespaceManager(new NameTable());
                            foreach (var s in scopes)
                                namespaceManager.AddNamespace(s.Key, s.Value);

                            var constraints = this.PS.Constraints;

                            var viewModel = new S100AttributeEditorViewModel(ps, constraints.ToLookup(e => e.Attribute("code").Value));

                            if (this.Code is null)
                                return viewModel;

                            var uid = $"{inspector.UID()}";

                            viewModel.Initialize(this.Code, uid);

                            if (!inspector.IsNull("attributebindings")) {
                                var json = Convert.ToString(inspector["attributebindings"]);

                                viewModel = viewModel.LoadAttributeBindings(json);
                            }

                            viewModel.RequestInformation = async (s, e) => {
                                if (MapView.Active is null) {
                                    Logger.Current.Debug("Information binding lookup skipped because no map view is active.");
                                    return [];
                                }
                                return await QueuedTask.Run(() => {
                                    string[] result = [];

                                    foreach (var layer in MapView.Active.Map.GetStandaloneTablesAsFlattenedList().Where(table => table.Name.EndsWith("informationtype"))) {
                                        var selection = layer.GetSelection();
                                        if (selection.GetCount() == 0) continue;

                                        using var cursor = selection.Search(new QueryFilter {
                                            WhereClause = $"UPPER(PS) = '{this.PS.ID}' AND UPPER(CODE) = '{e.InformationType.ToUpperInvariant()}'"
                                        }, true);

                                        while (cursor.MoveNext()) {
                                            result = [.. result, $"{cursor.Current.GetGlobalID():B}"];
                                        }
                                    }
                                    Logger.Current.Debug("Information binding lookup for type {InformationType} returned {MatchCount} selected records.",
                                        e.InformationType, result.Length);
                                    return result;
                                });
                            };

                            viewModel.SelectInformationTypes = async (s, e) => {
                                var mapView = MapView.Active;
                                if (mapView is null) return;

                                if (e.UIDs.Any()) {
                                    await QueuedTask.Run(() => {
                                        var query = new QueryFilter {
                                            WhereClause = $"GlobalID IN ({string.Join(',', e.UIDs.Select(e => $"'{e.UID}'"))})",
                                        };
                                        foreach (var layer in mapView.Map.GetStandaloneTablesAsFlattenedList()) {
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
                                if (MapView.Active is null) {
                                    Logger.Current.Debug("Feature binding lookup skipped because no map view is active.");
                                    return [];
                                }
                                return await QueuedTask.Run(() => {
                                    string[] result = [];

                                    foreach (var layer in MapView.Active.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>()) {
                                        if (layer is FeatureLayer featureLayer) {
                                            var selection = layer.GetSelection();
                                            if (selection.GetCount() == 0) continue;

                                            using var cursor = selection.Search(new QueryFilter {
                                                WhereClause = $"UPPER(PS) = '{this.PS.ID}' AND UPPER(CODE) = '{e.FeatureType.ToUpperInvariant()}'"
                                            }, true);

                                            while (cursor.MoveNext()) {
                                                result = [.. result, $"{cursor.Current.GetGlobalID():B}"];
                                            }
                                        }
                                    }
                                    foreach (var layer in MapView.Active.Map.GetStandaloneTablesAsFlattenedList().Where(table => table.Name.EndsWith("featuretype"))) {
                                        var selection = layer.GetSelection();
                                        if (selection.GetCount() == 0) continue;

                                        using var cursor = selection.Search(new QueryFilter {
                                            WhereClause = $"UPPER(PS) = '{this.PS.ID}' AND UPPER(CODE) = '{e.FeatureType.ToUpperInvariant()}'"
                                        }, true);

                                        while (cursor.MoveNext()) {
                                            result = [.. result, $"{cursor.Current.GetGlobalID():B}"];
                                        }
                                    }
                                    Logger.Current.Debug("Feature binding lookup for type {FeatureType} returned {MatchCount} selected records.",
                                        e.FeatureType, result.Length);
                                    return result;
                                }, TaskCreationOptions.None);
                            };

                            viewModel.SelectFeatureTypes = async (s, e) => {
                                var mapView = MapView.Active;
                                if (mapView is null) return;

                                if (e.UIDs.Any()) {
                                    await QueuedTask.Run(() => {
                                        var query = new QueryFilter {
                                            WhereClause = $"GlobalID IN ({string.Join(',', e.UIDs.Select(e => $"'{e.UID}'"))})",
                                        };
                                        foreach (var layer in mapView.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>()) {
                                            layer.Select(query, SelectionCombinationMethod.Add);
                                        }
                                        foreach (var layer in mapView.Map.GetStandaloneTablesAsFlattenedList()) {
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
                        }
                        catch (Exception ex) {
                            Logger.Current.Error(ex, "Failed to initialize the S-100 attribute editor for catalogue {CatalogueId}, code {FeatureCode}.",
                                this.PS?.ID, this.Code);
                            DiagnosticHelper.Error(ex);
                            return null;
                        }
                    }, TaskCreationOptions.None);
                    if (this.SelectedProperty is not null) {
                        Logger.Current.Debug("Loaded S-100 attribute editor for catalogue {CatalogueId}, code {FeatureCode} in {ElapsedMilliseconds} ms.",
                            this.PS.ID, this.Code, stopwatch.ElapsedMilliseconds);
                    }
                }
                else {
                    Logger.Current.Debug("S-100 attribute editor initialization skipped: catalogue {CatalogueId}, code {FeatureCode}.", this.PS?.ID, this.Code);
                }
            }
            catch (System.Exception ex) {
                Logger.Current.Error(ex, "Failed to load S-100 attributes for catalogue {CatalogueId}, code {FeatureCode}.", this.PS?.ID, this.Code);
                DiagnosticHelper.Error(ex);
                if (System.Diagnostics.Debugger.IsAttached) System.Diagnostics.Debugger.Break();
            }
        }

        private async void OnPropertyChanged(object sender, PropertyChangedEventArgs e) {
            try {
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
                    if (updated) {
                        Logger.Current.Debug("Updated inspector binding field for property {PropertyName}, catalogue {CatalogueId}, code {FeatureCode}.",
                            e.PropertyName, this.PS?.ID, this.Code);
                    }
                }, TaskCreationOptions.None);
            }
            catch (Exception ex) {
                Logger.Current.Error(ex, "Failed to update inspector bindings for property {PropertyName}, catalogue {CatalogueId}, code {FeatureCode}.",
                    e.PropertyName, this.PS?.ID, this.Code);
                throw;
            }
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
                this.NotifyPropertyChanged(() => this.PS_SelectorIsEnabled);
                this.NotifyPropertyChanged(() => this.CODE_SelectorIsEnabled);
            }
        }

        private string[] _codes = [];

        public string[] Codes {
            get => this._codes;
            set => this.SetProperty(ref this._codes, [.. value.Order()]);
        }

        public string? Code {
            get => this._code;
            set {
                this.SetProperty(ref this._code, value);
                this.NotifyPropertyChanged(() => this.CODE_SelectorIsEnabled);
            }
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

        public bool PS_SelectorIsEnabled {
            get {
                if (this.Inspector is null) return false;

                return this.Inspector.IsNull("ps") || this.Inspector.IsNull("code");

                return this.PS is null;

                if (this.Inspector is null) return true;

                if (this.Inspector.IsNull("ps")) {
                    //if (this.PS is null) return false;
                    return true;
                }
                return false;

                if (this.SelectedProperty is null)
                    return false;

                if ("{}".Equals(Convert.ToString(this.Inspector["attributebindings"]).Trim()) &&
                    "{}".Equals(Convert.ToString(this.Inspector["informationbindings"]).Trim()) &&
                    "{}".Equals(Convert.ToString(this.Inspector["featurebindings"]).Trim()))
                    return true;

                return false; // "{}".Equals(Convert.ToString(this.Inspector["attributebindings"]).Trim());
            }
        }

        public bool CODE_SelectorIsEnabled {
            get {
                if (this.Inspector is null) return false;

                if (!this.Inspector.IsNull("code")) return false;

                if (this.PS is null)
                    return false;
                return this.Codes.Any();

                return this.PS != null;

                if (this.Inspector is null) return false;

                if (this.Inspector.IsNull("ps")) {
                    //if (!(this.PS is null)) {
                    //    return true; 
                    //}
                    return false;
                }
                if (this.Inspector.IsNull("code"))
                    return true;
                return false;

                if ("{}".Equals(Convert.ToString(this.Inspector["attributebindings"]).Trim()) &&
                    "{}".Equals(Convert.ToString(this.Inspector["informationbindings"]).Trim()) &&
                    "{}".Equals(Convert.ToString(this.Inspector["featurebindings"]).Trim()))
                    return true;

                return false; // "{}".Equals(Convert.ToString(this.Inspector["attributebindings"]).Trim());
            }
        }

        public bool CREATE_ButtonIsEnabled {
            get {
                if (this.Inspector is null) return false;

                if (this.PS is null)
                    return false;
                if (this.Code is null)
                    return false;

                if (this.Inspector.IsNull("ps") || this.Inspector.IsNull("ps"))
                    return true;
                return false;

                if (this.PS is not null && !(string.IsNullOrEmpty(this.Code))) {
                    if ("{}".Equals(Convert.ToString(this.Inspector["attributebindings"]).Trim()) &&
                        "{}".Equals(Convert.ToString(this.Inspector["informationbindings"]).Trim()) &&
                        "{}".Equals(Convert.ToString(this.Inspector["featurebindings"]).Trim())) {
                        if (this.SelectedProperty is null || !("UNKNOWN".Equals(this.SelectedProperty.UID))) {
                            return false;
                        }
                        else {
                            return true;
                        }

                    }
                    return false;
                }



                //if ("{}".Equals(Convert.ToString(this.Inspector["attributebindings"]).Trim()) &&
                //    "{}".Equals(Convert.ToString(this.Inspector["informationbindings"]).Trim()) &&
                //    "{}".Equals(Convert.ToString(this.Inspector["featurebindings"]).Trim()))
                //    return false;

                //if (this.Inspector.IsNull("attributebindings") && this.Inspector.IsNull("informationbindings") && this.Inspector.IsNull("featurebindings"))
                //    return true;

                return false; // "{}".Equals(Convert.ToString(this.Inspector["attributebindings"]).Trim());
            }
        }
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