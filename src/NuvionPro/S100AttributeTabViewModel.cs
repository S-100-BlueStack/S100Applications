using ActiproSoftware.Windows.Extensions;
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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Nodes;
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

        internal record SelectedTemplate(string Schema, string Code)
        {
            public static SelectedTemplate Empty => new(string.Empty, string.Empty);
        }

        internal record SelectedType(string Code);

        private readonly NuvionPro.Module _module;

        private SelectedTemplate _selectedTemplate = SelectedTemplate.Empty;

        private SelectedType _selectedModelType = default;

        private ObservableCollection<string> _schemas = [];

        private string _selectedSchema = default;

        private XDocument? _featureCatalogue = null;

        //private JsonSerializerOptions _jsonOptions = new JsonSerializerOptions {
        //    WriteIndented = false,
        //    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        //    PropertyNameCaseInsensitive = true,
        //};

        private S100AttributeEditorViewModel _selectedProperty = default;

        private Boolean _isEditingEnabled = false;

        private Boolean _isVisible = false;

        private ObservableCollection<SelectedType> _modelTypes = [];

        private bool _isSelectedSchemaEnabled = true;

        private bool _isSelectedModelTypeEnabled = false;

        //private readonly string[] _catalogues;

        private readonly SubscriptionToken _tokenEditStarted;

        public S100AttributeTabViewModel(XElement options, bool canChangeOptions) : base(options, canChangeOptions) {
            this._module = NuvionPro.Module.Current;
            //this._catalogues = this._module.GetFeatureCatalogues();


            Project.Current.PropertyChanged += this.Current_PropertyChanged;
            this.IsEditingEnabled = Project.Current.IsEditingEnabled;

            this.Schemas.AddRange(this._module.GetFeatureCatalogues());

            this.CreateInstance = new ArcGIS.Desktop.Framework.RelayCommand(async () => {
                var inspector = base.Inspector;

                if (inspector != default) {
                    //if (!Project.Current.IsEditingEnabled) {
                    //    await Project.Current.SetIsEditingEnabledAsync(true);
                    //}

                    inspector["ps"] = this.SelectedSchema;
                    inspector["code"] = this.SelectedModelType.Code;

                    this.IsSelectedSchemaEnabled = false;
                    this.IsSelectedModelTypeEnabled = false;

                    await QueuedTask.Run(() => {
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

            base.NotifyPropertyChanged(name);

            switch (name) {
                case "SelectedSchema": {
                        this.SelectedModelType = default;
                        this.IsSelectedModelTypeEnabled = false;

                        if (this.SelectedSchema != default) {
                            var schema = this.SelectedSchema;

                            this.SelectedProperty = default;

                            if (this._featureCatalogue is not null) {
                                this.SelectedProperty = new S100AttributeEditorViewModel(this._featureCatalogue);
                                //var featureCatalogue = this._module.GetFeatureCatalogue(schema);

                                IEnumerable<string> types;
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

                                    types = this.SelectedProperty.GetFeaturesByPrimitive(primitive);
                                }
                                else {
                                    types = this.SelectedProperty.GetFeaturesByPrimitive(Primitives.noGeometry);
                                }

                                System.Windows.Application.Current.Dispatcher.Invoke(() => {
                                    this.ModelTypes.Clear();
                                    this.ModelTypes.AddRange(types.OrderBy(e => e).Select(e => new SelectedType(e)));
                                });

                                this.IsSelectedModelTypeEnabled = true;
                            }
                        }
                        this._selectedTemplate = SelectedTemplate.Empty;

                        this.NotifyPropertyChanged(() => this.IsCreateButtonEnabled);
                    }
                    break;

                case "SelectedModelType": {
                        if (this.SelectedModelType != default) {
                            var featuretype = this.SelectedModelType.Code;

                            if (featuretype != default) {
                                var featureCatalogue = this._module.GetFeatureCatalogue(this.SelectedSchema);

                                this._selectedTemplate = new SelectedTemplate(this.SelectedSchema, featuretype);

                                this.NotifyPropertyChanged(() => this.IsCreateButtonEnabled);
                            }
                        }
                    }
                    break;
            }
        }

        public override bool Applies(MapMember mapMember) {
            return true;
        }

        public override bool IsDefault => true;

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
                System.Windows.Application.Current.Dispatcher.Invoke(() => {
                    this.SelectedSchema = default;
                    this.SelectedModelType = default;
                    this.Schemas.Clear();
                    this.ModelTypes.Clear();
                });

                //TODO: TJEK KUN FOR OPDATERING!!!!! Eller blinker UI

                var catalogue = await QueuedTask.Run(() => {
                    var fc = inspector.MapMember switch {
                        FeatureLayer l => l.GetFeatureClass(),
                        StandaloneTable t => t.GetTable(),
                        _ => throw new InvalidOperationException(),
                    };

                    using var geodatabase = (Geodatabase)fc.GetDatastore();

                    var syntax = geodatabase.GetSQLSyntax();
                    var tableNames = syntax.ParseTableName(fc.GetName());

                    var ps = Convert.ToString(inspector["ps"]);
                    if (!string.IsNullOrEmpty(ps)) {
                        return [ps.ToUpperInvariant()];
                    }

                    return this._module.GetFeatureCatalogues();


                    using var configuration = geodatabase.OpenDataset<Table>(syntax.QualifyTableName(tableNames.Item1, tableNames.Item2, "configuration"));

                    string[] catalogues = new string[0];

                    using var cursor = configuration.Search(null, true);
                    while (cursor.MoveNext()) {
                        var settings = JsonSerializer.Deserialize<S100Horizon.Settings.Editor>(Convert.ToString(cursor.Current["json"]));
                        if (!settings.ExcludeInEditor)
                            catalogues = [.. catalogues, Convert.ToString(cursor.Current["ps"]).Split('.').First()];
                    }
                    if (catalogues.Any())
                        return catalogues;

                    return [];
                    //return this._catalogues;
                }, TaskCreationOptions.None);

                System.Windows.Application.Current.Dispatcher.Invoke(() => {
                    if (catalogue.Any()) {
                        this.Schemas.AddRange(catalogue);
                    }
                    //if (!string.IsNullOrEmpty(catalogue)) {
                    //    Schemas.Clear();
                    //    Schemas.Add(catalogue);
                    //}
                    //else {

                    //}
                });

                var schema = Convert.ToString(inspector["ps"]);
                if (!string.IsNullOrEmpty(schema))
                    this.SelectedSchema = schema;

                var code = Convert.ToString(inspector["code"]);
                if (!string.IsNullOrEmpty(code))
                    this.SelectedModelType = new SelectedType(code);

                this.SelectedProperty = await QueuedTask.Run(() => {
                    ;
                    if (string.IsNullOrEmpty(schema)) {
                        this.SelectedSchema = default;
                        this.SelectedModelType = default;

                        System.Windows.Application.Current.Dispatcher.Invoke(() => {
                            this.ModelTypes.Clear();
                        });

                        this._selectedTemplate = SelectedTemplate.Empty;
                        return default(S100AttributeEditorViewModel);
                    }

                    var featureCatalogue = this._module.GetFeatureCatalogue(schema);

                    var ps = XDocument.Load(featureCatalogue.FullPath);

                    var viewModel = new S100AttributeEditorViewModel(ps);

                    if (string.IsNullOrEmpty(code)) {
                        System.Windows.Application.Current.Dispatcher.Invoke(() => {
                            this.ModelTypes.Clear();
                            this.ModelTypes.AddRange(viewModel.GetFeaturesByPrimitive(primitime).Select(e => new SelectedType(e)));
                        });
                        return viewModel;
                    }
                    System.Windows.Application.Current.Dispatcher.Invoke(() => {
                        this.ModelTypes.Clear();
                        this.ModelTypes.Add(new SelectedType(code));
                    });


                    //var type = this._inspectorHandle.TypeSelector(inspector, schema);

                    if (!string.IsNullOrEmpty(code)) {
                        viewModel.Initialize(code);
                    }

                    var uid = $"{inspector.UID()}";

                    object? instance = null;
                    if (DBNull.Value.Equals(inspector["FLATTEN"]) || string.IsNullOrEmpty(Convert.ToString(inspector["FLATTEN"]))) {
                        //instance = Activator.CreateInstance(type);
                    }
                    else {
                        var json = Convert.ToString(inspector["FLATTEN"]);

                        viewModel = viewModel.LoadAttributeBindings(json);

                        //if (type.BaseType == typeof(S100FC.InformationType))
                        //    instance = S100FC.AttributeFlattenExtensions.Unflatten<S100FC.InformationType>(json, type);
                        //else if (type.BaseType == typeof(S100FC.FeatureType))
                        //    instance = S100FC.AttributeFlattenExtensions.Unflatten<S100FC.FeatureType>(json, type);
                        //else if (System.Diagnostics.Debugger.IsAttached)
                        //    System.Diagnostics.Debugger.Break();
                    }

                    return viewModel;
#if null
                    var type = this._inspectorHandle.TypeSelector(inspector, schema);

                    if (type is null) {
                        return default;
                    }

                    object? instance = null;
                    if (DBNull.Value.Equals(inspector["FLATTEN"]) || string.IsNullOrEmpty(Convert.ToString(inspector["FLATTEN"]))) {
                        instance = Activator.CreateInstance(type);
                    }
                    else {
                        var json = Convert.ToString(inspector["FLATTEN"]);

                        if (type.BaseType == typeof(S100FC.InformationType))
                            instance = S100FC.AttributeFlattenExtensions.Unflatten<S100FC.InformationType>(json, type);
                        else if (type.BaseType == typeof(S100FC.FeatureType))
                            instance = S100FC.AttributeFlattenExtensions.Unflatten<S100FC.FeatureType>(json, type);
                        else if (System.Diagnostics.Debugger.IsAttached)
                            System.Diagnostics.Debugger.Break();
                    }

                    S100AttributeEditorViewModel viewModel = default;

                    if (instance is S100FC.InformationType informationType) {
                        System.Windows.Application.Current.Dispatcher.Invoke(() => {
                            viewModel = new S100AttributeEditorViewModel(informationType, uid) {
                                RequestInformation = async (s, e) => {
                                    if (MapView.Active is null)
                                        return [];
                                    return await QueuedTask.Run(() => {
                                        string[] result = [];

                                        foreach (var layer in MapView.Active.Map.StandaloneTables.Where(table => table.Name.EndsWith("informationtype"))) {
                                            var selection = layer.GetSelection();
                                            if (selection.GetCount() == 0) continue;

                                            using var cursor = selection.Search(new QueryFilter {
                                                WhereClause = $"UPPER(PS) = '{this.SelectedSchema}' AND UPPER(CODE) = '{e.InformationType.ToUpperInvariant()}'"
                                            }, true);

                                            while (cursor.MoveNext()) {
                                                result = [.. result, Convert.ToString(cursor.Current["UID"])];
                                            }
                                        }
                                        return result;
                                    });
                                },
                            };
                        });
                        return viewModel;
                    }
                    if (instance is S100FC.FeatureType featureType) {
                        System.Windows.Application.Current.Dispatcher.Invoke(() => {
                            viewModel = new S100AttributeEditorViewModel(featureType, uid) {
                                RequestInformation = async (s, e) => {
                                    if (MapView.Active is null)
                                        return [];
                                    return await QueuedTask.Run(() => {
                                        string[] result = [];

                                        foreach (var layer in MapView.Active.Map.StandaloneTables.Where(table => table.Name.EndsWith("informationtype"))) {
                                            var selection = layer.GetSelection();
                                            if (selection.GetCount() == 0) continue;

                                            using var cursor = selection.Search(new QueryFilter {
                                                WhereClause = $"UPPER(PS) = '{this.SelectedSchema}' AND UPPER(CODE) = '{e.InformationType.ToUpperInvariant()}'"
                                            }, true);

                                            while (cursor.MoveNext()) {
                                                result = [.. result, Convert.ToString(cursor.Current["UID"])];
                                            }
                                        }
                                        return result;
                                    }, TaskCreationOptions.None);
                                },

                                SelectInformationTypes = async (s, e) => {
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
                                },

                                RequestFeatures = async (s, e) => {
                                    if (MapView.Active is null)
                                        return [];
                                    return await QueuedTask.Run(() => {
                                        string[] result = [];

                                        foreach (var layer in MapView.Active.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>()) {
                                            if (layer is FeatureLayer featureLayer) {
                                                var selection = layer.GetSelection();
                                                if (selection.GetCount() == 0) continue;

                                                using var cursor = selection.Search(new QueryFilter {
                                                    WhereClause = $"UPPER(PS) = '{this.SelectedSchema}' AND UPPER(CODE) = '{e.FeatureType.ToUpperInvariant()}'"
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
                                                WhereClause = $"UPPER(PS) = '{this.SelectedSchema}' AND UPPER(CODE) = '{e.FeatureType.ToUpperInvariant()}'"
                                            }, true);

                                            while (cursor.MoveNext()) {
                                                result = [.. result, Convert.ToString(cursor.Current["UID"])];
                                            }
                                        }
                                        return result;
                                    }, TaskCreationOptions.None);
                                },

                                SelectFeatureTypes = async (s, e) => {
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
                                },
                            };
                        });

                        return viewModel;
                    }
                    throw new NotImplementedException();
#endif
                }, TaskCreationOptions.None);

                if (this.SelectedProperty == default) {
                    this.SelectedSchema = default;
                    this.SelectedModelType = default;

                    this.IsSelectedSchemaEnabled = true;
                    this.IsSelectedModelTypeEnabled = this.SelectedSchema != default;

                    this.IsVisible = Visibility.Collapsed;
                }
                else {
                    System.Windows.Application.Current.Dispatcher.Invoke(() => {
                        //if (!this.Inspector.IsNull("informationBindings")) {
                        //    var informationBindings = System.Text.Json.JsonSerializer.Deserialize<informationBinding[]>(Convert.ToString(this.Inspector["informationBindings"]), this._jsonOptions);
                        //    foreach (var informationBinding in informationBindings)
                        //        this.SelectedProperty += informationBinding;
                        //}
                        //if (this.SelectedProperty?.Instance is S100FC.FeatureType) {
                        //    if (!this.Inspector.IsNull("featureBindings")) {
                        //        var featureBindings = System.Text.Json.JsonSerializer.Deserialize<featureBinding[]>(Convert.ToString(this.Inspector["featureBindings"]), this._jsonOptions);
                        //        foreach (var featureBinding in featureBindings)
                        //            this.SelectedProperty += featureBinding;
                        //    }
                        //}

                        this.IsSelectedSchemaEnabled = false;
                        this.IsSelectedModelTypeEnabled = false;

                        this.IsVisible = Visibility.Visible;
                    });
                }
                this.NotifyPropertyChanged(() => this.IsCreateButtonEnabled);
            }
            catch (System.Exception ex) {
                DiagnosticHelper.Error(ex);
                if (System.Diagnostics.Debugger.IsAttached) System.Diagnostics.Debugger.Break();
            }
        }

        private async void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            await QueuedTask.Run(() => {
                //if (sender is ICollection<FeatureBindingViewModel> featureBindings) {
                //    var f = featureBindings.Select(e => new featureBinding {
                //        association = e.association,
                //        associationId = e.associationId,
                //        featureId = e.featureId,
                //        role = e.role,
                //        roleType = e.roleType.HasValue ? Enum.GetName<roleType>(e.roleType.Value) : default,
                //    });
                //    Inspector["featurebindings"] = System.Text.Json.JsonSerializer.Serialize(f);
                //}
            }, TaskCreationOptions.None);
        }

        private async void OnCollectionItemChanged(object sender, object item, PropertyChangedEventArgs e) {
            await QueuedTask.Run(() => {
                var updated = false;

                //if (sender is S100AttributeEditorViewModel viewModel) {
                //    var json = viewModel.Flatten();
                //    if (this.Inspector.IsNull("flatten")) {
                //        this.Inspector["flatten"] = json;
                //        updated |= true;
                //    }
                //    else if (string.Compare(json, Convert.ToString(this.Inspector["flatten"]), true) != 0) {
                //        this.Inspector["flatten"] = json;
                //        updated |= true;
                //    }
                //}
            }, TaskCreationOptions.None);
        }

        private async void OnPropertyChanged(object sender, PropertyChangedEventArgs e) {
            await QueuedTask.Run(() => {
                var updated = false;

                if (sender is S100AttributeEditorViewModel viewModel) {
                    if (e.PropertyName.Equals(nameof(S100AttributeEditorViewModel.attributeBindings))) {
                        var flatten = JsonFlattener.Flatten([.. viewModel.attributeBindings.Select(e => e.attribute)], viewModel.attributeBindingsCatalogue);
                        if (this.Inspector.IsNull("flatten")) {
                            this.Inspector["flatten"] = flatten;
                            updated |= true;
                        }
                        else if (string.Compare(flatten, Convert.ToString(this.Inspector["flatten"]), true) != 0) {
                            this.Inspector["flatten"] = flatten;
                            updated |= true;
                        }
                    }
                    if (e.PropertyName.Equals(nameof(S100AttributeEditorViewModel.informationBindings))) {
                        //var informationBindings = (informationBinding[])viewModel;

                        //var json = System.Text.Json.JsonSerializer.Serialize(informationBindings, this._module.GetFeatureCatalogue(this.SelectedSchema).DefaultJsonOptions);

                        //if (this.Inspector.IsNull("informationBindings")) {
                        //    this.Inspector["informationBindings"] = json;
                        //    updated |= true;
                        //}
                        //else if (string.Compare(json, Convert.ToString(this.Inspector["informationBindings"]), true) != 0) {
                        //    this.Inspector["informationBindings"] = json;
                        //    updated |= true;
                        //}
                    }
                    if (e.PropertyName.Equals(nameof(S100AttributeEditorViewModel.featureBindings))) {
                        //var featureBindings = (featureBinding[])viewModel;

                        //var json = System.Text.Json.JsonSerializer.Serialize(featureBindings, this._module.GetFeatureCatalogue(this.SelectedSchema).DefaultJsonOptions);

                        //if (this.Inspector.IsNull("featureBindings")) {
                        //    this.Inspector["featureBindings"] = json;
                        //    updated |= true;
                        //}
                        //else if (string.Compare(json, Convert.ToString(this.Inspector["featureBindings"]), true) != 0) {
                        //    this.Inspector["featureBindings"] = json;
                        //    updated |= true;
                        //}
                    }
                }
            }, TaskCreationOptions.None);
        }

        private async void OnInformationBindingCollectionChanged(object sender, PropertyChangedEventArgs e) {
            await QueuedTask.Run(() => {
                var updated = false;

                //if (sender is FeatureViewModel viewModel) {
                //    var informationBindings = viewModel.GetInformationBindings();

                //    var json = System.Text.Json.JsonSerializer.Serialize(informationBindings, _module.GetFeatureCatalogue(SelectedSchema).DefaultJsonOptions);
                //    if (Inspector.IsNull("informationBindings") && informationBindings.Any()) {
                //        Inspector["informationBindings"] = json;
                //        updated |= true;
                //    }
                //    else if (string.Compare(json, Convert.ToString(Inspector["informationBindings"]), true) != 0) {
                //        Inspector["informationBindings"] = json;
                //        updated |= true;
                //    }
                //}
            }, TaskCreationOptions.None);
        }

        private async void OnFeatureBindingCollectionChanged(object sender, PropertyChangedEventArgs e) {
            var propertyName = e.PropertyName;

            await QueuedTask.Run(() => {
                var updated = false;

                //if (sender is FeatureViewModel viewModel) {                    
                //    //var featureBindings = (Collection<featureBindingViewModel>)viewModel.GetType().GetProperty(propertyName).GetValue(viewModel);
                //    var featureBindings = viewModel.GetFeatureBindings();

                //    var json = System.Text.Json.JsonSerializer.Serialize(featureBindings, _module.GetFeatureCatalogue(SelectedSchema).DefaultJsonOptions);
                //    if (Inspector.IsNull("featurebindings") && featureBindings.Any()) {
                //        Inspector["featurebindings"] = json;
                //        updated |= true;
                //    }
                //    else if (string.Compare(json, Convert.ToString(Inspector["featurebindings"]), true) != 0) {
                //        Inspector["featurebindings"] = json;
                //        updated |= true;
                //    }
                //}
            }, TaskCreationOptions.None);
        }

        //private Type FeatureTypeSelector(Inspector inspector, string schema) {
        //    var featureCatalogue = this._module.GetFeatureCatalogue(schema);

        //    var code = Convert.ToString(inspector["code"]);
        //    if (string.IsNullOrEmpty(code))
        //        return null;

        //    if (!this._selectedTemplate.Schema.Equals(schema) || !this._selectedTemplate.Code.Equals(code)) {
        //        this.SelectedSchema = schema;

        //        var types = featureCatalogue.FeatureTypes.Select(e => e.Code);

        //        System.Windows.Application.Current.Dispatcher.Invoke(() => {
        //            this.ModelTypes.Clear();
        //            this.ModelTypes.AddRange(types.OrderBy(e => e).Select(e => new SelectedType(e)));
        //        });

        //        this.SelectedModelType = this.ModelTypes.Single(e => e.Code == code);
        //    }

        //    var type = featureCatalogue.Assembly!.GetType($"{S100FC.Catalogues.FeatureCatalogue.Namespace(schema, "FeatureTypes")}.{code}", true);

        //    return type;
        //}

        //private Type InformationTypeSelector(Inspector inspector, string schema) {
        //    var featureCatalogue = this._module.GetFeatureCatalogue(schema);

        //    var code = Convert.ToString(inspector["code"]);
        //    if (string.IsNullOrEmpty(code))
        //        return null;

        //    if (!this._selectedTemplate.Schema.Equals(schema) || !this._selectedTemplate.Code.Equals(code)) {
        //        this.SelectedSchema = schema;

        //        var types = featureCatalogue.InformationTypes.Select(e => e.Code);

        //        System.Windows.Application.Current.Dispatcher.Invoke(() => {
        //            this.ModelTypes.Clear();
        //            this.ModelTypes.AddRange(types.OrderBy(e => e).Select(e => new SelectedType(e)));
        //        });

        //        this.SelectedModelType = this.ModelTypes.Single(e => e.Code == code);
        //    }

        //    var type = featureCatalogue.Assembly!.GetType($"{S100FC.Catalogues.FeatureCatalogue.Namespace(schema, "InformationTypes")}.{code}", true);

        //    return type;
        //}

        public ICommand CreateInstance { get; set; }

        public ObservableCollection<string> Schemas {
            get => this._schemas;
            set => this.SetProperty(ref this._schemas, value);
        }

        public string SelectedSchema {
            get => this._selectedSchema;
            set {
                this.SetProperty(ref this._selectedSchema, value);

                if (string.IsNullOrEmpty(value))
                    this._featureCatalogue = null;
                else {
                    var featureCatalogue = this._module.GetFeatureCatalogue(value);

                    this._featureCatalogue = XDocument.Load(featureCatalogue.FullPath);
                }

                //this._jsonOptions = value switch {
                //    "S-101" => S100FC.S101.Extensions.AppendTypeInfoResolver(new JsonSerializerOptions {
                //        WriteIndented = false,
                //        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                //        PropertyNameCaseInsensitive = true,
                //    }),
                //    "S-122" => S100FC.S122.Extensions.AppendTypeInfoResolver(new JsonSerializerOptions {
                //        WriteIndented = false,
                //        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                //        PropertyNameCaseInsensitive = true,
                //    }),
                //    "S-123" => S100FC.S123.Extensions.AppendTypeInfoResolver(new JsonSerializerOptions {
                //        WriteIndented = false,
                //        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                //        PropertyNameCaseInsensitive = true,
                //    }),
                //    "S-124" => S100FC.S124.Extensions.AppendTypeInfoResolver(new JsonSerializerOptions {
                //        WriteIndented = false,
                //        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                //        PropertyNameCaseInsensitive = true,
                //    }),
                //    "S-127" => S100FC.S127.Extensions.AppendTypeInfoResolver(new JsonSerializerOptions {
                //        WriteIndented = false,
                //        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                //        PropertyNameCaseInsensitive = true,
                //    }),
                //    "S-128" => S100FC.S128.Extensions.AppendTypeInfoResolver(new JsonSerializerOptions {
                //        WriteIndented = false,
                //        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                //        PropertyNameCaseInsensitive = true,
                //    }),
                //    "S-131" => S100FC.S131.Extensions.AppendTypeInfoResolver(new JsonSerializerOptions {
                //        WriteIndented = false,
                //        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                //        PropertyNameCaseInsensitive = true,
                //    }),
                //    null => new JsonSerializerOptions {
                //        WriteIndented = false,
                //        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                //        PropertyNameCaseInsensitive = true,
                //    },
                //    _ => throw new NotImplementedException(),
                //};
            }
        }

        public ObservableCollection<SelectedType> ModelTypes {
            get => this._modelTypes;
            set => this.SetProperty(ref this._modelTypes, value);
        }

        public SelectedType SelectedModelType {
            get => this._selectedModelType;
            set => this.SetProperty(ref this._selectedModelType, value);
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

        //public SelectedInformationTypeObjectViewModel SelectedInformationProperty {
        //    get => _selectedInformationProperty;
        //    set => SetProperty(ref _selectedInformationProperty, value);
        //}

        //public SelectedFeatureTypeObjectViewModel SelectedFeatureProperty {
        //    get => _selectedFeatureProperty;
        //    set => SetProperty(ref _selectedFeatureProperty, value);
        //}

        public bool IsSelectedSchemaEnabled {
            get => this._isSelectedSchemaEnabled;
            set => this.SetProperty(ref this._isSelectedSchemaEnabled, value);
        }

        public bool IsSelectedModelTypeEnabled {
            get => this._isSelectedModelTypeEnabled;
            set => this.SetProperty(ref this._isSelectedModelTypeEnabled, value);
        }

        public bool IsCreateButtonEnabled => this.IsSelectedSchemaEnabled && this.IsSelectedModelTypeEnabled && this._selectedTemplate != SelectedTemplate.Empty;
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