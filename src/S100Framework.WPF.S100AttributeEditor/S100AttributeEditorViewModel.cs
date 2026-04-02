using S100FC;
using System.Collections;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace S100Framework.WPF.ViewModel
{
    public class S100AttributeEditorViewModel : INotifyPropertyChanged, IAttributeBindingContainer, INotifyDataErrorInfo
    {
        public class RequestInformationsEventArgs(string? informationType) : EventArgs
        {
            public string? InformationType { get; } = informationType;
        }

        public class RequestFeaturesEventArgs(string? featureType) : EventArgs
        {
            public string? FeatureType { get; } = featureType;
        }
        public class SelectInformationTypesEvenArgs(InformationTypeID[] uids) : EventArgs
        {
            public InformationTypeID[] UIDs { get; } = uids;
        }

        public class SelectFeatureTypesEvenArgs(FeatureTypeID[] uids) : EventArgs
        {
            public FeatureTypeID[] UIDs { get; } = uids;
        }

        public delegate Task<string[]> RequestInformationsEventHandler(object? sender, RequestInformationsEventArgs e);

        public delegate Task<string[]> RequestFeaturesEventHandler(object? sender, RequestFeaturesEventArgs e);

        public delegate Task SelectInformationTypesEventHandler(object? sender, SelectInformationTypesEvenArgs e);

        public delegate Task SelectFeatureTypessEventHandler(object? sender, SelectFeatureTypesEvenArgs e);

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged = default;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null) {
            if (Equals(field, value))
                return false;

            field = value;
            this.OnPropertyChanged(propertyName);
            return true;
        }
        #endregion

        #region INotifyDataErrorInfo
        public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged = default;

        public bool HasErrors => this._errors.Any();

        public IEnumerable GetErrors(string? propertyName) {
            if (string.IsNullOrEmpty(propertyName)) return Enumerable.Empty<string>();

            if (!this._errors.ContainsKey(propertyName) || !this._errors[propertyName].Any()) return Enumerable.Empty<string>();

            return this._errors[propertyName];
        }

        private void Validate() {
            this._errors.Clear();

            //if (this.Instance is InformationType informationType) {
            //    this._errors[nameof(this.attributeBindings)] = [];

            //}
            //else if (this.Instance is FeatureType featureType) {
            //    this._errors[nameof(this.attributeBindings)] = [];

            //    featureType.Validate(this._errors[nameof(this.attributeBindings)]);

            //    this.ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(nameof(this.HasErrors)));
            //}
        }

        private readonly Dictionary<string, List<string>> _errors = [];
        #endregion

        #region IAttributeBindingContainer
        public bool HasCapacity(attributeBindingDefinition binding) {
            var count = this.attributeBindings.Count(e => e.code.Equals(binding.attribute));
            return binding.upper > count;
        }

        public bool HasCapacity(IGrouping<string, informationBindingDefinition> binding) {
            return true;
        }

        public bool HasCapacity(IGrouping<string, featureBindingDefinition> binding) {
            return true;
        }

        public void AddAttribute(AttributeViewModel attributeBinding) {
            this.attributeBindings.Add(attributeBinding);
        }
        #endregion

        public RequestInformationsEventHandler RequestInformation = async (s, e) => { return []; };

        public RequestFeaturesEventHandler RequestFeatures = async (s, e) => { return []; };

        public SelectInformationTypesEventHandler SelectInformationTypes = async (s, e) => { };

        public SelectFeatureTypessEventHandler SelectFeatureTypes = async (s, e) => { };

        public informationBindingContainer? informationBindingDefinitions { get; private set; } = null;

        public featureBindingContainer? featureBindingDefinitions { get; private set; } = null;

        public attributeBindingDefinition[] attributeBindingsCatalogue { get; private set; } = [];

        private informationBindingDefinition[] _informationBindingDefinitions { get; set; } = [];

        private featureBindingDefinition[] _featureBindingDefinitions { get; set; } = [];

        private XDocument _featureCatalogue { get; init; }

        private XmlNamespaceManager _namespaceManager { get; init; }

        private bool _isInitialized = false;

        public S100AttributeEditorViewModel(XDocument featureCatalogue) {
            this._featureCatalogue = featureCatalogue;

            var navigator = featureCatalogue.CreateNavigator();
            navigator.MoveToFollowing(XPathNodeType.Element);

            var scopes = navigator.GetNamespacesInScope(XmlNamespaceScope.All);

            this._namespaceManager = new XmlNamespaceManager(new NameTable());
            foreach (var s in scopes)
                this._namespaceManager.AddNamespace(s.Key, s.Value);

            this.permittedPrimitives = featureCatalogue.Descendants(XName.Get("S100_FC_FeatureType", scopes["S100FC"])).ToDictionary(
                e => e.Element(XName.Get("code", scopes["S100FC"]))!.Value,
                e => e.Elements(XName.Get("permittedPrimitives", scopes["S100FC"])).Select(e => e.Value).ToArray()).ToImmutableDictionary<string,string[]>();

            this.attributeBindings.CollectionChanged += (s, e) => {
                if (e.OldItems is not null) {
                    foreach (var item in e.OldItems) {
                        if (item is SimpleAttributeViewModel simpleAttributeViewModel) {
                            //this._featureType.RemoveAttribute(simpleAttributeViewModel.attribute);
                        }
                        if (item is ComplexAttributeViewModel complexAttributeViewModel) {
                            //this._featureType.RemoveAttribute(complexAttributeViewModel.attribute);
                        }

                        if (item is AttributeViewModel attribute) {
                            attribute.PropertyChanged -= this.Viewmodel_PropertyChanged;
                        }
                    }
                }
                if (e.NewItems is not null) {
                    foreach (var item in e.NewItems) {
                        if (item is SimpleAttributeViewModel simpleAttribute) {
                            simpleAttribute.PropertyChanged += this.Viewmodel_PropertyChanged;
                        }
                        else if (item is ComplexAttributeViewModel complexAttribute) {
                            complexAttribute.PropertyChanged += this.Viewmodel_PropertyChanged;
                        }
                    }
                }
            };

            this.informationBindings.CollectionChanged += (s, e) => {
                if (e.OldItems is not null) {
                    foreach (var item in e.OldItems) {
                        if (item is InformationBindingViewModel informationBinding) {
                            informationBinding.PropertyChanged -= this.Viewmodel_PropertyChanged;
                        }
                    }
                }
                if (e.NewItems is not null) {
                    foreach (var item in e.NewItems) {
                        if (item is InformationBindingViewModel informationBinding) {
                            informationBinding.PropertyChanged += this.Viewmodel_PropertyChanged;
                        }
                    }
                }
            };

            this.featureBindings.CollectionChanged += (s, e) => {
                if (e.OldItems is not null) {
                    foreach (var item in e.OldItems) {
                        if (item is FeatureBindingViewModel featureBinding) {
                            featureBinding.PropertyChanged -= this.Viewmodel_PropertyChanged;
                        }
                    }
                }
                if (e.NewItems is not null) {
                    foreach (var item in e.NewItems) {
                        if (item is FeatureBindingViewModel featureBinding) {
                            featureBinding.PropertyChanged += this.Viewmodel_PropertyChanged;
                        }
                    }
                }
            };
        }

        public S100AttributeEditorViewModel Initialize(string code) {
            if (string.IsNullOrEmpty(code) || string.IsNullOrWhiteSpace(code)) throw new System.ArgumentNullException(nameof(code));

            this.code = code;

            var scope = this._namespaceManager.LookupNamespace("S100FC")!;

            var simpleAttributes = this._featureCatalogue.Descendants(XName.Get("S100_FC_SimpleAttribute", scope)).ToDictionary(e => e.Element(XName.Get("code", scope))!.Value, e => e);

            var complexAttributes = this._featureCatalogue.Descendants(XName.Get("S100_FC_ComplexAttribute", scope)).ToDictionary(e => e.Element(XName.Get("code", scope))!.Value, e => e);

            int index = 0;
            this.attributeBindingsCatalogue = Parser.AttributeBindings(this._featureCatalogue, code, ref index, simpleAttributes, complexAttributes);
            this._informationBindingDefinitions = Parser.InformationBindings(this._featureCatalogue, code);
            this._featureBindingDefinitions = Parser.FeatureBindings(this._featureCatalogue, code);

            this._isInitialized = true;

            return this;
        }

        public S100AttributeEditorViewModel LoadAttributeBindings(string json) {            
            if (string.IsNullOrEmpty(json)) return this;
            if (!this._isInitialized) throw new InvalidOperationException();

            var structuredObject = JsonUnflattener.Unflatten(json)!;

            if (structuredObject is null) return this;

            var properties = JsonUnflattener.GetAllProperties(structuredObject).ToArray();

            var g = properties.GroupBy(e => e.Path.Split('.')[0]);

            attributeBinding[] attributeBindings = [];

            foreach (var property in properties.GroupBy(e => e.Path.Split('.')[0])) {
                var attributes = property.ToArray();
                var instance = Parser.CreateInstance(property.Key, attributes, this.attributeBindingsCatalogue);
                attributeBindings = [.. attributeBindings, instance];
            }

            var attributeBindingsCatalogue = this.attributeBindingsCatalogue.ToDictionary(e => e.attribute, e => e);
            foreach (var attributeBinding in attributeBindings) {
                if (attributeBinding is DateAttribute dateAttribute) {
                    var viewModel = new DateAttributeViewModel(ref dateAttribute, attributeBindingsCatalogue[dateAttribute.S100FC_code]);
                    this.attributeBindings.Add(viewModel);
                }
                else if (attributeBinding is DateTimeAttribute dateTimeAttribute) {
                    var viewModel = new DateTimeAttributeViewModel(ref dateTimeAttribute, attributeBindingsCatalogue[dateTimeAttribute.S100FC_code]);
                    this.attributeBindings.Add(viewModel);
                }
                else if (attributeBinding is SimpleAttribute simpleAttribute) {
                    var viewModel = new SimpleAttributeViewModel(ref simpleAttribute, attributeBindingsCatalogue[simpleAttribute.S100FC_code]);
                    this.attributeBindings.Add(viewModel);
                }
                else if (attributeBinding is ComplexAttribute complexAttribute) {
                    var viewModel = new ComplexAttributeViewModel(ref complexAttribute);
                    this.attributeBindings.Add(viewModel);
                }
                else
                    throw new NotImplementedException();
            }
            this.Validate();

            //note: Must be added right by the end!
            this.attributeBindings.CollectionChanged += (s, e) => {
                this.OnPropertyChanged("attributeBindings");

            };

            return this;
        }

        public S100AttributeEditorViewModel LoadInformationBindings(string json) {
            if (string.IsNullOrEmpty(json)) return this;
            if (!this._isInitialized) throw new InvalidOperationException();

            var structuredObject = System.Text.Json.JsonSerializer.Deserialize<informationBinding[]>(json);

            if (structuredObject is null) return this;

            this.informationBindingDefinitions = null;
            if (this._informationBindingDefinitions.Any())
                this.informationBindingDefinitions = new informationBindingContainer(this._informationBindingDefinitions);

            foreach (var informationBinding in structuredObject) {
                var definitions = this._informationBindingDefinitions.GroupBy(e => e.association).Single(e => e.Key.Equals(informationBinding.association?.S100FC_code));
                this.informationBindings.Add(new InformationBindingViewModel(definitions) {
                    roleType = informationBinding.roleType,
                    role = informationBinding.role,
                    informationType = informationBinding.informationType,
                    informationUID = new InformationTypeID(informationBinding.informationType!, informationBinding.informationId),
                });
            }
            this.Validate();

            //note: Must be added right by the end!
            this.informationBindings.CollectionChanged += (s, e) => {
                this.OnPropertyChanged("informationBindings");
                this.Validate();
            };

            return this;
        }

        public S100AttributeEditorViewModel LoadFeatureBindings(string json) {
            if (string.IsNullOrEmpty(json)) return this;
            if (!this._isInitialized) throw new InvalidOperationException();

            var structuredObject = System.Text.Json.JsonSerializer.Deserialize<featureBinding[]>(json);

            if (structuredObject is null) return this;

            this.featureBindingDefinitions = null;
            if (this._featureBindingDefinitions.Any())
                this.featureBindingDefinitions = new featureBindingContainer(this._featureBindingDefinitions);

            foreach (var featureBinding in structuredObject) {
                var definitions = this._featureBindingDefinitions.GroupBy(e => e.association).Single(e => e.Key.Equals(featureBinding.association?.S100FC_code));
                this.featureBindings.Add(new FeatureBindingViewModel(definitions) {
                    roleType = featureBinding.roleType,
                    role = featureBinding.role,
                    featureType = featureBinding.featureType,
                    featureUID = new FeatureTypeID(featureBinding.featureType!, featureBinding.featureId),
                });
            }
            this.Validate();

            //note: Must be added right by the end!
            this.featureBindings.CollectionChanged += (s, e) => {
                this.OnPropertyChanged("featureBindings");
                this.Validate();
            };

            return this;
        }

        #region Properties        
        private string _code = "UNKNOWN";

        public string code {
            get {
                return this._code;
            }
            set {
                this.SetProperty(ref this._code, value);
            }
        }

        public ObservableCollection<AttributeViewModel> attributeBindings { get; set; } = [];

        public ObservableCollection<InformationBindingViewModel> informationBindings { get; set; } = [];

        public ObservableCollection<FeatureBindingViewModel> featureBindings { get; set; } = [];

        public bool HasInformationBindings => this._informationBindingDefinitions.Any();

        public bool HasFeatureBindings => this._featureBindingDefinitions.Any();

        public ImmutableDictionary<string, string[]> permittedPrimitives { get; init; } = [];

        public string[] GetFeaturesByPrimitive(Primitives primitive) => this.permittedPrimitives.Where(e => e.Value.Contains($"{primitive}")).Select(e => e.Key).ToArray();
        #endregion

        #region Operators
        public static S100AttributeEditorViewModel operator +(S100AttributeEditorViewModel viewModel, informationBinding informationBinding) {
            var association = informationBinding.GetType().GetGenericArguments()[0].Name;

            //var definitions = viewModel.informationBindingDefinitions!.GroupBy.Single(e => e.Key.Equals(association));

            //viewModel.informationBindings.Add(new InformationBindingViewModel(definitions) {
            //    roleType = informationBinding.roleType,
            //    role = informationBinding.role,
            //    informationType = informationBinding.informationType,
            //    informationUID = new InformationTypeID(informationBinding.informationType!, informationBinding.informationId),
            //});
            return viewModel;
        }

        public static S100AttributeEditorViewModel operator +(S100AttributeEditorViewModel viewModel, featureBinding featureBinding) {
            var association = featureBinding.GetType().GetGenericArguments()[0].Name;

            //var definitions = viewModel.featureBindingDefinitions!.GroupBy.Single(e => e.Key.Equals(association));

            //viewModel.featureBindings.Add(new FeatureBindingViewModel(definitions) {
            //    roleType = featureBinding.roleType,
            //    role = featureBinding.role,
            //    featureType = featureBinding.featureType,
            //    featureUID = new FeatureTypeID(featureBinding.featureType!, featureBinding.featureId),
            //});
            return viewModel;
        }
        #endregion        

        private void Viewmodel_PropertyChanged(object? sender, PropertyChangedEventArgs e) {
            if (sender is AttributeViewModel attribute) {
                //if (!attribute.attribute.IsValid(this.attributeBindings.Select(e => e.attribute))) {
                //    this._errors[attribute.code] = new List<string> { "Dependency" };
                //    ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(attribute.code));
                //}

                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.attributeBindings)));

                this.Validate();
            }
            else if (sender is InformationBindingViewModel informationBinding) {
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.informationBindings)));
            }
            else if (sender is FeatureBindingViewModel featureBinding) {
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.featureBindings)));
            }
            else if (System.Diagnostics.Debugger.IsAttached)
                System.Diagnostics.Debugger.Break();
        }

        public class informationBindingContainer
        {
            public string[] associations => [.. this._informationBindingDefinitions.Select(e => e.Key)];

            public IEnumerable<IGrouping<string, informationBindingDefinition>> GroupBy => this._informationBindingDefinitions;

            private IEnumerable<IGrouping<string, informationBindingDefinition>> _informationBindingDefinitions { get; init; } = [];

            public informationBindingContainer(S100FC.informationBindingDefinition[] informationBindingDefinitions) {
                this._informationBindingDefinitions = informationBindingDefinitions.GroupBy(e => e.association);
            }
        }

        public class featureBindingContainer
        {
            public string[] associations => [.. this._featureBindingDefinitions.Select(e => e.Key)];

            public IEnumerable<IGrouping<string, featureBindingDefinition>> GroupBy => this._featureBindingDefinitions;

            private IEnumerable<IGrouping<string, featureBindingDefinition>> _featureBindingDefinitions { get; init; } = [];

            public featureBindingContainer(S100FC.featureBindingDefinition[] featureBindingDefinitions) {
                this._featureBindingDefinitions = featureBindingDefinitions.GroupBy(e => e.association);
            }
        }

        private static class Parser
        {
            public static attributeBindingDefinition[] AttributeBindings(XDocument featureCatalogue, string code, ref int index, IDictionary<string, XElement> simpleAttributes, IDictionary<string, XElement> complexAttributes) {
                var navigator = featureCatalogue.CreateNavigator();
                navigator.MoveToFollowing(XPathNodeType.Element);

                var scopes = navigator.GetNamespacesInScope(XmlNamespaceScope.All);

                var xmlNamespaceManager = new XmlNamespaceManager(new NameTable());
                foreach (var s in scopes)
                    xmlNamespaceManager.AddNamespace(s.Key, s.Value);

                XElement? element = null;
                if (featureCatalogue.Descendants(XName.Get("S100_FC_InformationType", scopes["S100FC"])).Any(e => e.Element(XName.Get("code", scopes["S100FC"]))!.Value.Equals(code))) {
                    element = featureCatalogue.Descendants(XName.Get("S100_FC_InformationType", scopes["S100FC"])).First(ft => ft.Element(XName.Get("code", scopes["S100FC"]))!.Value.Equals(code));
                }
                else if (featureCatalogue.Descendants(XName.Get("S100_FC_FeatureType", scopes["S100FC"])).Any(e => e.Element(XName.Get("code", scopes["S100FC"]))!.Value.Equals(code))) {
                    element = featureCatalogue.Descendants(XName.Get("S100_FC_FeatureType", scopes["S100FC"])).First(ft => ft.Element(XName.Get("code", scopes["S100FC"]))!.Value.Equals(code));
                }
                else
                    throw new InvalidOperationException($"Unsupported object type ({code})!");

                //if (element.Attribute("isAbstract") != default && bool.Parse(element.Attribute("isAbstract")!.Value)) {
                //    throw new InvalidOperationException($"Abstract types are not supported ({code})!");
                //}

                attributeBindingDefinition[] attributeBindingDefinitions = [];

                var superType = element.Elements(XName.Get("superType", scopes["S100FC"])).FirstOrDefault();
                if (superType != null) {
                    var superTypeAttributeBindingDefinitionsSuperType = AttributeBindings(featureCatalogue, superType.Value, ref index, simpleAttributes, complexAttributes);
                    if (superTypeAttributeBindingDefinitionsSuperType.Any())
                        attributeBindingDefinitions = [.. attributeBindingDefinitions, .. superTypeAttributeBindingDefinitionsSuperType];
                }

                var attributeBindings = element.XPathSelectElements("S100FC:attributeBinding", xmlNamespaceManager);
                foreach (var binding in attributeBindings) {
                    var referenceCode = binding.Element(XName.Get("attribute", scopes["S100FC"]))!.Attribute("ref")!.Value!;
                    var lower = int.Parse(binding.XPathSelectElement("S100FC:multiplicity/S100Base:lower", xmlNamespaceManager)!.Value);
                    var _ = binding.XPathSelectElement("S100FC:multiplicity/S100Base:upper", xmlNamespaceManager)!;
                    int upper = (_.Attribute(XName.Get("infinite")) != default && _.Attribute(XName.Get("infinite"))!.Value.Equals("true")) ? int.MaxValue : int.Parse(_.Value!);

                    var attributeBinding = CreateAttributeBinding(binding, xmlNamespaceManager, simpleAttributes, complexAttributes);
                    attributeBinding.attributeBindingDefinition.order = index++;

                    attributeBindingDefinitions = [.. attributeBindingDefinitions, attributeBinding.attributeBindingDefinition];
                }

                return attributeBindingDefinitions;
            }

            public static informationBindingDefinition[] InformationBindings(XDocument featureCatalogue, string code) {
                var navigator = featureCatalogue.CreateNavigator();
                navigator.MoveToFollowing(XPathNodeType.Element);

                var scopes = navigator.GetNamespacesInScope(XmlNamespaceScope.All);

                var xmlNamespaceManager = new XmlNamespaceManager(new NameTable());
                foreach (var s in scopes)
                    xmlNamespaceManager.AddNamespace(s.Key, s.Value);

                XElement? element = null;
                if (featureCatalogue.Descendants(XName.Get("S100_FC_InformationType", scopes["S100FC"])).Any(e => e.Element(XName.Get("code", scopes["S100FC"]))!.Value.Equals(code))) {
                    element = featureCatalogue.Descendants(XName.Get("S100_FC_InformationType", scopes["S100FC"])).First(ft => ft.Element(XName.Get("code", scopes["S100FC"]))!.Value.Equals(code));
                }
                else if (featureCatalogue.Descendants(XName.Get("S100_FC_FeatureType", scopes["S100FC"])).Any(e => e.Element(XName.Get("code", scopes["S100FC"]))!.Value.Equals(code))) {
                    element = featureCatalogue.Descendants(XName.Get("S100_FC_FeatureType", scopes["S100FC"])).First(ft => ft.Element(XName.Get("code", scopes["S100FC"]))!.Value.Equals(code));
                }
                else
                    throw new InvalidOperationException($"Unsupported object type ({code})!");

                informationBindingDefinition[] informationBindingDefinitions = [];

                var superType = element.Elements(XName.Get("superType", scopes["S100FC"])).FirstOrDefault();
                if (superType != null) {
                    var superTypeAttributeBindingDefinitionsSuperType = InformationBindings(featureCatalogue, superType.Value);
                    if (superTypeAttributeBindingDefinitionsSuperType.Any())
                        informationBindingDefinitions = [.. informationBindingDefinitions, .. superTypeAttributeBindingDefinitionsSuperType];
                }

                var informationBindings = element.XPathSelectElements("S100FC:informationBinding", xmlNamespaceManager);
                foreach (var binding in informationBindings) {
                    var association = binding.Element(XName.Get("association", scopes["S100FC"]))!.Attribute("ref")!.Value!;
                    var role = binding.Element(XName.Get("role", scopes["S100FC"]))!.Attribute("ref")!.Value!;
                    var roleType = binding.Attribute("roleType")!.Value!;

                    var lower = int.Parse(binding.XPathSelectElement("S100FC:multiplicity/S100Base:lower", xmlNamespaceManager)!.Value);
                    var _ = binding.XPathSelectElement("S100FC:multiplicity/S100Base:upper", xmlNamespaceManager)!;
                    int upper = (_.Attribute(XName.Get("infinite")) != default && _.Attribute(XName.Get("infinite"))!.Value.Equals("true")) ? int.MaxValue : int.Parse(_.Value!);

                    var informationTypes = binding.XPathSelectElements("S100FC:informationType", xmlNamespaceManager);

                    var informationBindingDefinition = new informationBindingDefinition {
                        association = association,
                        role = role,
                        roleType = roleType,
                        lower = lower,
                        upper = upper,
                        informationTypes = [.. informationTypes.Select(e => e.Attribute("ref")!.Value!)],
                        CreateInstance = () => new informationBinding() {
                            role = role,
                            roleType = roleType,
                        },
                    };
                    informationBindingDefinitions = [.. informationBindingDefinitions, informationBindingDefinition];
                }

                return informationBindingDefinitions;
            }

            public static featureBindingDefinition[] FeatureBindings(XDocument featureCatalogue, string code) {
                var navigator = featureCatalogue.CreateNavigator();
                navigator.MoveToFollowing(XPathNodeType.Element);

                var scopes = navigator.GetNamespacesInScope(XmlNamespaceScope.All);

                var xmlNamespaceManager = new XmlNamespaceManager(new NameTable());
                foreach (var s in scopes)
                    xmlNamespaceManager.AddNamespace(s.Key, s.Value);

                XElement? element = null;
                if (featureCatalogue.Descendants(XName.Get("S100_FC_FeatureType", scopes["S100FC"])).Any(e => e.Element(XName.Get("code", scopes["S100FC"]))!.Value.Equals(code))) {
                    element = featureCatalogue.Descendants(XName.Get("S100_FC_FeatureType", scopes["S100FC"])).First(ft => ft.Element(XName.Get("code", scopes["S100FC"]))!.Value.Equals(code));
                }
                else
                    throw new InvalidOperationException($"Unsupported object type ({code})!");

                featureBindingDefinition[] featureBindingDefinitions = [];

                var superType = element.Elements(XName.Get("superType", scopes["S100FC"])).FirstOrDefault();
                if (superType != null) {
                    var superTypeAttributeBindingDefinitionsSuperType = FeatureBindings(featureCatalogue, superType.Value);
                    if (superTypeAttributeBindingDefinitionsSuperType.Any())
                        featureBindingDefinitions = [.. featureBindingDefinitions, .. superTypeAttributeBindingDefinitionsSuperType];
                }

                var featureBindings = element.XPathSelectElements("S100FC:featureBinding", xmlNamespaceManager);
                foreach (var binding in featureBindings) {
                    var association = binding.Element(XName.Get("association", scopes["S100FC"]))!.Attribute("ref")!.Value!;
                    var role = binding.Element(XName.Get("role", scopes["S100FC"]))!.Attribute("ref")!.Value!;
                    var roleType = binding.Attribute("roleType")!.Value!;

                    var lower = int.Parse(binding.XPathSelectElement("S100FC:multiplicity/S100Base:lower", xmlNamespaceManager)!.Value);
                    var _ = binding.XPathSelectElement("S100FC:multiplicity/S100Base:upper", xmlNamespaceManager)!;
                    int upper = (_.Attribute(XName.Get("infinite")) != default && _.Attribute(XName.Get("infinite"))!.Value.Equals("true")) ? int.MaxValue : int.Parse(_.Value!);

                    var featureTypes = binding.XPathSelectElements("S100FC:featureType", xmlNamespaceManager);

                    var featureBindingDefinition = new featureBindingDefinition {
                        association = association,
                        role = role,
                        roleType = roleType,
                        lower = lower,
                        upper = upper,
                        featureTypes = [.. featureTypes.Select(e => e.Attribute("ref")!.Value!)],
                        CreateInstance = () => new featureBinding() {
                            role = role,
                            roleType = roleType,
                        },
                    };
                    featureBindingDefinitions = [.. featureBindingDefinitions, featureBindingDefinition];
                }

                return featureBindingDefinitions;
            }

            internal static attributeBinding CreateInstance(string path, (string Path, object Value)[]? attributes, attributeBindingDefinition[] catalogue) {
                var simplepath = _regexArray.Replace(path, string.Empty);

                var instance = catalogue.ToDictionary(e => e.attribute, e => e)[simplepath].CreateInstance()!;

                if (instance is SimpleAttribute simpleAttribute) {
                    simpleAttribute.SetValue((string)attributes!.Single(e => e.Path.Equals(path)).Value);
                    return simpleAttribute;
                }
                else if (instance is ComplexAttribute complexAttribute) {
                    if (attributes is not null) {
                        var g = attributes.GroupBy(e => _regexArray.Replace(e.Path, string.Empty).Substring(simplepath.Length + 1).Split('.')[0]).ToArray();

                        foreach (var property in g) {
                            var subattributes = property.ToArray();
                            for (int i = 0; i < subattributes.Length; i++) {
                                subattributes[i].Path = _regexArray.Replace(subattributes[i].Path, string.Empty).Substring(simplepath.Length + 1);
                            }
                            //var subpath = _regexArray.Replace(attribute.Path, string.Empty).Substring(path.Length + 1);
                            var subinstance = CreateInstance(property.Key, subattributes, complexAttribute.attributeBindingsCatalogue);
                            complexAttribute.SetAttribute(subinstance);
                        }
                    }

                    return complexAttribute;
                }
                else
                    throw new NotImplementedException();
            }

            private static Func<EnumerationAttribute> CreateEnumeration(XElement attributeBindingElement, XElement simpleAttributeElement, XmlNamespaceManager xmlNamespaceManager) {
                var scope = xmlNamespaceManager.LookupNamespace("S100FC")!;

                var permittedValues = attributeBindingElement.XPathSelectElement("S100FC:permittedValues", xmlNamespaceManager)?.Elements(XName.Get("value", scope)).Select(e => e.Value).ToArray();

                listedValue[] listedValues = [];

                foreach (var listedValue in simpleAttributeElement.Element(XName.Get("listedValues", scope))!.Elements()) {
                    var label = listedValue.Element(XName.Get("label", scope))!.Value!;
                    var definition = listedValue.Element(XName.Get("definition", scope))!.Value!;
                    var code = listedValue.Element(XName.Get("code", scope))!.Value!;

                    if (permittedValues is not null && !permittedValues.Contains(code)) continue;

                    definition = definition.Replace("\"", "\\\"");

                    listedValues = [.. listedValues, new listedValue(label, definition, int.Parse(code))];
                }

                return () => new EnumerationAttribute {
                    S100FC_code = simpleAttributeElement.Element(XName.Get("code", scope))!.Value,
                    S100FC_name = simpleAttributeElement.Element(XName.Get("name", scope))!.Value,
                    listedValues = listedValues,
                };
            }

            private static (Func<attributeBinding> creator, attributeBindingDefinition attributeBindingDefinition) CreateAttributeBinding(XElement binding, XmlNamespaceManager xmlNamespaceManager, IDictionary<string, XElement> simpleAttributes, IDictionary<string, XElement> complexAttributes) {
                var scope = xmlNamespaceManager.LookupNamespace("S100FC")!;

                var referenceCode = binding.Element(XName.Get("attribute", scope))!.Attribute("ref")!.Value!;
                var lower = int.Parse(binding.XPathSelectElement("S100FC:multiplicity/S100Base:lower", xmlNamespaceManager)!.Value);
                var _ = binding.XPathSelectElement("S100FC:multiplicity/S100Base:upper", xmlNamespaceManager)!;
                int upper = (_.Attribute(XName.Get("infinite")) != default && _.Attribute(XName.Get("infinite"))!.Value.Equals("true")) ? int.MaxValue : int.Parse(_.Value!);

                if (simpleAttributes.ContainsKey(referenceCode)) {
                    var simpleAttribute = simpleAttributes[referenceCode];

                    var valueType = simpleAttribute.Element(XName.Get("valueType", scope))!.Value;

                    Func<SimpleAttribute> attributeBinding = valueType switch {
                        "boolean" => () => new BooleanAttribute {
                            S100FC_code = simpleAttribute.Element(XName.Get("code", scope))!.Value,
                            S100FC_name = simpleAttribute.Element(XName.Get("name", scope))!.Value,
                        },
                        "real" => () => new RealAttribute {
                            S100FC_code = simpleAttribute.Element(XName.Get("code", scope))!.Value,
                            S100FC_name = simpleAttribute.Element(XName.Get("name", scope))!.Value,
                        },
                        "text" => () => new TextAttribute {
                            S100FC_code = simpleAttribute.Element(XName.Get("code", scope))!.Value,
                            S100FC_name = simpleAttribute.Element(XName.Get("name", scope))!.Value,
                        },
                        "S100_TruncatedDate" => () => new S100_TruncatedDateAttribute {
                            S100FC_code = simpleAttribute.Element(XName.Get("code", scope))!.Value,
                            S100FC_name = simpleAttribute.Element(XName.Get("name", scope))!.Value,
                        },
                        "date" => () => new DateAttribute {
                            S100FC_code = simpleAttribute.Element(XName.Get("code", scope))!.Value,
                            S100FC_name = simpleAttribute.Element(XName.Get("name", scope))!.Value,
                        },
                        "dataonly" => () => new DateAttribute {
                            S100FC_code = simpleAttribute.Element(XName.Get("code", scope))!.Value,
                            S100FC_name = simpleAttribute.Element(XName.Get("name", scope))!.Value,
                        },
                        "datetime" => () => new DateTimeAttribute {
                            S100FC_code = simpleAttribute.Element(XName.Get("code", scope))!.Value,
                            S100FC_name = simpleAttribute.Element(XName.Get("name", scope))!.Value,
                        },
                        "time" => () => new TimeAttribute {
                            S100FC_code = simpleAttribute.Element(XName.Get("code", scope))!.Value,
                            S100FC_name = simpleAttribute.Element(XName.Get("name", scope))!.Value,
                        },
                        "integer" => () => new IntegerAttribute {
                            S100FC_code = simpleAttribute.Element(XName.Get("code", scope))!.Value,
                            S100FC_name = simpleAttribute.Element(XName.Get("name", scope))!.Value,
                        },
                        "URN" => () => new UrnAttribute {
                            S100FC_code = simpleAttribute.Element(XName.Get("code", scope))!.Value,
                            S100FC_name = simpleAttribute.Element(XName.Get("name", scope))!.Value,
                        },
                        "URL" => () => new UrnAttribute {
                            S100FC_code = simpleAttribute.Element(XName.Get("code", scope))!.Value,
                            S100FC_name = simpleAttribute.Element(XName.Get("name", scope))!.Value,
                        },
                        "URI" => () => new UrnAttribute {
                            S100FC_code = simpleAttribute.Element(XName.Get("code", scope))!.Value,
                            S100FC_name = simpleAttribute.Element(XName.Get("name", scope))!.Value,
                        },
                        "enumeration" => CreateEnumeration(binding, simpleAttribute, xmlNamespaceManager),
                        _ => throw new NotImplementedException(),
                    };

                    var attributeBindingDefinition = new attributeBindingDefinition {
                        attribute = referenceCode,
                        lower = lower,
                        upper = upper,
                        //order = index++,
                        CreateInstance = () => attributeBinding(),
                    };

                    return (attributeBinding, attributeBindingDefinition);
                }
                else if (complexAttributes.ContainsKey(referenceCode)) {
                    var complexAttribute = complexAttributes[referenceCode];

                    attributeBinding[] attributeBindings = [];
                    attributeBindingDefinition[] attributeBindingDefinitions = [];

                    var subAttributeBindings = complexAttribute.XPathSelectElements("S100FC:subAttributeBinding", xmlNamespaceManager);
                    foreach (var subBinding in subAttributeBindings) {
                        var subAttributeBinding = CreateAttributeBinding(subBinding, xmlNamespaceManager, simpleAttributes, complexAttributes);
                        attributeBindingDefinitions = [.. attributeBindingDefinitions, subAttributeBinding.attributeBindingDefinition];
                    }

                    var attributeBinding = () => new ComplexAttribute {
                        S100FC_code = complexAttribute.Element(XName.Get("code", scope))!.Value,
                        S100FC_name = complexAttribute.Element(XName.Get("name", scope))!.Value,
                        attributeBindings = attributeBindings,
                        attributeBindingsCatalogue = attributeBindingDefinitions,
                    };

                    var attributeBindingDefinition = new attributeBindingDefinition {
                        attribute = referenceCode,
                        lower = lower,
                        upper = upper,
                        //order = index++,
                        CreateInstance = () => attributeBinding(),
                    };

                    return (attributeBinding, attributeBindingDefinition);
                }

                throw new NotImplementedException();
            }

            private static Regex _regexArray = new Regex(@"\[\d+\]", RegexOptions.Singleline | RegexOptions.IgnorePatternWhitespace);
        }
    }

#if null
    public class S100AttributeEditorViewModel : INotifyPropertyChanged, IAttributeBindingContainer, INotifyDataErrorInfo
    {
        public class RequestInformationsEventArgs(string? informationType) : EventArgs
        {
            public string? InformationType { get; } = informationType;
        }

        public class RequestFeaturesEventArgs(string? featureType) : EventArgs
        {
            public string? FeatureType { get; } = featureType;
        }
        public class SelectInformationTypesEvenArgs(InformationTypeID[] uids) : EventArgs
        {
            public InformationTypeID[] UIDs { get; } = uids;
        }

        public class SelectFeatureTypesEvenArgs(FeatureTypeID[] uids) : EventArgs
        {
            public FeatureTypeID[] UIDs { get; } = uids;
        }

        public delegate Task<string[]> RequestInformationsEventHandler(object? sender, RequestInformationsEventArgs e);

        public delegate Task<string[]> RequestFeaturesEventHandler(object? sender, RequestFeaturesEventArgs e);

        public delegate Task SelectInformationTypesEventHandler(object? sender, SelectInformationTypesEvenArgs e);

        public delegate Task SelectFeatureTypessEventHandler(object? sender, SelectFeatureTypesEvenArgs e);

        public class informationBindingContainer
        {
            public string[] associations => [.. this._informationBindingDefinitions.Select(e => e.Key)];

            public IEnumerable<IGrouping<string, informationBindingDefinition>> GroupBy => this._informationBindingDefinitions;

            private IEnumerable<IGrouping<string, informationBindingDefinition>> _informationBindingDefinitions { get; init; } = [];

            public informationBindingContainer(S100FC.informationBindingDefinition[] informationBindingDefinitions) {
                this._informationBindingDefinitions = informationBindingDefinitions.GroupBy(e => e.association);
            }
        }

        public class featureBindingContainer
        {
            public string[] associations => [.. this._featureBindingDefinitions.Select(e => e.Key)];

            public IEnumerable<IGrouping<string, featureBindingDefinition>> GroupBy => this._featureBindingDefinitions;

            private IEnumerable<IGrouping<string, featureBindingDefinition>> _featureBindingDefinitions { get; init; } = [];

            public featureBindingContainer(S100FC.featureBindingDefinition[] featureBindingDefinitions) {
                this._featureBindingDefinitions = featureBindingDefinitions.GroupBy(e => e.association);
            }
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged = default;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null) {
            if (Equals(field, value))
                return false;

            field = value;
            this.OnPropertyChanged(propertyName);
            return true;
        }
        #endregion

        #region INotifyDataErrorInfo
        public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged = default;

        public bool HasErrors => this._errors.Any();

        public IEnumerable GetErrors(string? propertyName) {
            if (string.IsNullOrEmpty(propertyName)) return Enumerable.Empty<string>();

            if (!this._errors.ContainsKey(propertyName) || !this._errors[propertyName].Any()) return Enumerable.Empty<string>();

            return this._errors[propertyName];
        }

        private void Validate() {
            this._errors.Clear();

            if (this.Instance is InformationType informationType) {
                this._errors[nameof(this.attributeBindings)] = [];
                //this._errors[nameof(informationBindings)] = new List<string>();

            }
            else if (this.Instance is FeatureType featureType) {
                this._errors[nameof(this.attributeBindings)] = [];
                //this._errors[nameof(informationBindings)] = new List<string>();
                //this._errors[nameof(featureBindings)] = new List<string>();

                featureType.Validate(this._errors[nameof(this.attributeBindings)]);

                //foreach(var informationBinding in this.informationBindings) {
                //    informationBinding.Validate(this._errors[nameof(informationBindings)]);
                //}

                //foreach (var featureBinding in this.featureBindings) {
                //    featureBinding.Validate(this._errors[nameof(featureBindings)]);
                //}

                this.ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(nameof(this.HasErrors)));
            }
        }

        private readonly Dictionary<string, List<string>> _errors = [];
        #endregion

        public RequestInformationsEventHandler RequestInformation = async (s, e) => { return []; };

        public RequestFeaturesEventHandler RequestFeatures = async (s, e) => { return []; };

        public SelectInformationTypesEventHandler SelectInformationTypes = async (s, e) => { };

        public SelectFeatureTypessEventHandler SelectFeatureTypes = async (s, e) => { };

        public S100AttributeEditorViewModel(S100FC.InformationType informationType, string uid) {
            this._informationType = informationType;
            this._uid = uid;
            this.code = this._informationType.S100FC_code;
            this.attributeBindingsCatalogue = this._informationType.attributeBindingsCatalogue;

            this.Flatten = () => this._informationType.Flatten();

            if (informationType is IInformationBindings informationBindings) {
                this.HasInformationBindings = true;

                this.informationBindingDefinitions = new informationBindingContainer(informationBindings.GetInformationBindingsDefinitions());
            }

            this.attributeBindings.CollectionChanged += (s, e) => {
                if (e.OldItems is not null) {
                    foreach (var item in e.OldItems) {
                        if (item is SimpleAttributeViewModel simpleAttributeViewModel) {
                            this._informationType.RemoveAttribute(simpleAttributeViewModel.attribute);
                        }
                        if (item is ComplexAttributeViewModel complexAttributeViewModel) {
                            this._informationType.RemoveAttribute(complexAttributeViewModel.attribute);
                        }

                        if (item is AttributeViewModel attribute) {
                            attribute.PropertyChanged -= this.Viewmodel_PropertyChanged;
                        }
                    }
                }
                if (e.NewItems is not null) {
                    foreach (var item in e.NewItems) {
                        if (item is SimpleAttributeViewModel simpleAttribute) {
                            simpleAttribute.PropertyChanged += this.Viewmodel_PropertyChanged;
                        }
                        else if (item is ComplexAttributeViewModel complexAttribute) {
                            complexAttribute.PropertyChanged += this.Viewmodel_PropertyChanged;
                        }
                    }
                }
                //this.OnPropertyChanged("attributeBindings");
            };

            this.informationBindings.CollectionChanged += (s, e) => {
                if (e.OldItems is not null) {
                    foreach (var item in e.OldItems) {
                        if (item is InformationBindingViewModel informationBinding) {
                            informationBinding.PropertyChanged -= this.Viewmodel_PropertyChanged;
                        }
                    }
                }
                if (e.NewItems is not null) {
                    foreach (var item in e.NewItems) {
                        if (item is InformationBindingViewModel informationBinding) {
                            informationBinding.PropertyChanged += this.Viewmodel_PropertyChanged;
                        }
                    }
                }
                this.OnPropertyChanged("informationBindings");

                this.Validate();
            };

            foreach (var e in this._informationType.attributeBindings.OrderBy(e => this.attributeBindingsCatalogue.Single(a => a.attribute.Equals(e.S100FC_code)).order)) {
                var attributeBindingDefinition = this.attributeBindingsCatalogue.Single(a => a.attribute.Equals(e.S100FC_code));

                if (e is DateAttribute dateAttribute)
                    this.attributeBindings.Add(new DateAttributeViewModel(ref dateAttribute, attributeBindingDefinition));
                else if (e is DateTimeAttribute dateTimeAttribute)
                    this.attributeBindings.Add(new DateTimeAttributeViewModel(ref dateTimeAttribute, attributeBindingDefinition));
                else if (e is SimpleAttribute simpleAttribute)
                    this.attributeBindings.Add(new SimpleAttributeViewModel(ref simpleAttribute, attributeBindingDefinition));
                else if (e is ComplexAttribute complexAttribute)
                    this.attributeBindings.Add(new ComplexAttributeViewModel(ref complexAttribute));
            }

            //note: Must be added right by the end!
            this.attributeBindings.CollectionChanged += (s, e) => {
                this.OnPropertyChanged("attributeBindings");
            };

            this.Validate();
        }

        public S100AttributeEditorViewModel(S100FC.FeatureType feature, string uid) {
            this._featureType = feature;
            this._uid = uid;
            this.code = this._featureType.S100FC_code;
            this.attributeBindingsCatalogue = this._featureType.attributeBindingsCatalogue;

            this.Flatten = () => this._featureType.Flatten();

            if (feature is IInformationBindings informationBindings) {
                var _informationBindingDefinitions = informationBindings.GetInformationBindingsDefinitions();

                if (_informationBindingDefinitions.Any())
                    this.informationBindingDefinitions = new informationBindingContainer(_informationBindingDefinitions);

                this.HasInformationBindings = this.informationBindingDefinitions is not null;
            }

            if (feature is IFeatureBindings featureBindings) {
                var _featureBindingDefinitions = featureBindings.GetFeatureBindingsDefinitions();

                if (_featureBindingDefinitions.Any())
                    this.featureBindingDefinitions = new featureBindingContainer(_featureBindingDefinitions);

                this.HasFeatureBindings = this.featureBindingDefinitions is not null;
            }

            this.attributeBindings.CollectionChanged += (s, e) => {
                if (e.OldItems is not null) {
                    foreach (var item in e.OldItems) {
                        if (item is SimpleAttributeViewModel simpleAttributeViewModel) {
                            this._featureType.RemoveAttribute(simpleAttributeViewModel.attribute);
                        }
                        if (item is ComplexAttributeViewModel complexAttributeViewModel) {
                            this._featureType.RemoveAttribute(complexAttributeViewModel.attribute);
                        }

                        if (item is AttributeViewModel attribute) {
                            attribute.PropertyChanged -= this.Viewmodel_PropertyChanged;
                        }
                    }
                }
                if (e.NewItems is not null) {
                    foreach (var item in e.NewItems) {
                        if (item is SimpleAttributeViewModel simpleAttribute) {
                            simpleAttribute.PropertyChanged += this.Viewmodel_PropertyChanged;
                        }
                        else if (item is ComplexAttributeViewModel complexAttribute) {
                            complexAttribute.PropertyChanged += this.Viewmodel_PropertyChanged;
                        }
                    }
                }
            };

            this.informationBindings.CollectionChanged += (s, e) => {
                if (e.OldItems is not null) {
                    foreach (var item in e.OldItems) {
                        if (item is InformationBindingViewModel informationBinding) {
                            informationBinding.PropertyChanged -= this.Viewmodel_PropertyChanged;
                        }
                    }
                }
                if (e.NewItems is not null) {
                    foreach (var item in e.NewItems) {
                        if (item is InformationBindingViewModel informationBinding) {
                            informationBinding.PropertyChanged += this.Viewmodel_PropertyChanged;
                        }
                    }
                }
                this.OnPropertyChanged("informationBindings");

                this.Validate();
            };

            this.featureBindings.CollectionChanged += (s, e) => {
                if (e.OldItems is not null) {
                    foreach (var item in e.OldItems) {
                        if (item is FeatureBindingViewModel featureBinding) {
                            featureBinding.PropertyChanged -= this.Viewmodel_PropertyChanged;
                        }
                    }
                }
                if (e.NewItems is not null) {
                    foreach (var item in e.NewItems) {
                        if (item is FeatureBindingViewModel featureBinding) {
                            featureBinding.PropertyChanged += this.Viewmodel_PropertyChanged;
                        }
                    }
                }
                this.OnPropertyChanged("featureBindings");

                this.Validate();
            };

            foreach (var e in this._featureType.attributeBindings.OrderBy(e => this.attributeBindingsCatalogue.Single(a => a.attribute.Equals(e.S100FC_code)).order)) {
                var attributeBindingDefinition = this.attributeBindingsCatalogue.Single(a => a.attribute.Equals(e.S100FC_code));

                if (e is DateAttribute dateAttribute)
                    this.attributeBindings.Add(new DateAttributeViewModel(ref dateAttribute, attributeBindingDefinition));
                else if (e is DateTimeAttribute dateTimeAttribute)
                    this.attributeBindings.Add(new DateTimeAttributeViewModel(ref dateTimeAttribute, attributeBindingDefinition));
                else if (e is SimpleAttribute simpleAttribute)
                    this.attributeBindings.Add(new SimpleAttributeViewModel(ref simpleAttribute, attributeBindingDefinition));
                else if (e is ComplexAttribute complexAttribute)
                    this.attributeBindings.Add(new ComplexAttributeViewModel(ref complexAttribute));
            }

            //note: Must be added right by the end!
            this.attributeBindings.CollectionChanged += (s, e) => {
                this.OnPropertyChanged("attributeBindings");
            };

            this.Validate();
        }

        public bool HasInformationBindings { get; init; } = false;

        public informationBindingContainer? informationBindingDefinitions { get; set; } = null;

        public bool HasFeatureBindings { get; init; } = false;

        public featureBindingContainer? featureBindingDefinitions { get; set; } = null;

        public bool HasCapacity(attributeBindingDefinition binding) {
            var count = this.attributeBindings.Count(e => e.code.Equals(binding.attribute));
            return binding.upper > count;
        }

        public bool HasCapacity(IGrouping<string, informationBindingDefinition> binding) {
            return true;
            //var count = this.informationBindings.Count(e => e.association.Equals(binding.association) && e.role!.Equals(binding.role));

            //var definition = this.informationBindingDefinitions!.GroupBy.Single(e => e.Key.Equals(binding.association)).Single(e => e.role.Equals(binding.role));

            //return definition.upper > count;
        }

        public bool HasCapacity(IGrouping<string, featureBindingDefinition> binding) {
            return true;
            //var count = this.featureBindings.Count(e => e.association.Equals(binding.association) && e.role!.Equals(binding.role));

            //var definition = this.featureBindingDefinitions!.GroupBy.Single(e => e.Key.Equals(binding.association)).Single(e => e.role.Equals(binding.role));

            //return definition.upper > count;
        }

        public void AddAttribute(AttributeViewModel attributeBinding) {
            this.attributeBindings.Add(attributeBinding);

            if (this.Instance is S100FC.InformationType informationType) {
                informationType.SetAttribute(attributeBinding.attribute);
            }
            if (this.Instance is S100FC.FeatureType featureType) {
                featureType.SetAttribute(attributeBinding.attribute);
            }
        }

        private void Viewmodel_PropertyChanged(object? sender, PropertyChangedEventArgs e) {
            if (sender is AttributeViewModel attribute) {
                //if (!attribute.attribute.IsValid(this.attributeBindings.Select(e => e.attribute))) {
                //    this._errors[attribute.code] = new List<string> { "Dependency" };
                //    ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(attribute.code));
                //}

                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.attributeBindings)));

                this.Validate();
            }
            else if (sender is InformationBindingViewModel informationBinding) {
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.informationBindings)));
            }
            else if (sender is FeatureBindingViewModel featureBinding) {
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.featureBindings)));
            }
            else if (System.Diagnostics.Debugger.IsAttached)
                System.Diagnostics.Debugger.Break();
        }


        #region Operators
        public static S100AttributeEditorViewModel operator +(S100AttributeEditorViewModel viewModel, informationBinding informationBinding) {
            var association = informationBinding.GetType().GetGenericArguments()[0].Name;

            var definitions = viewModel.informationBindingDefinitions!.GroupBy.Single(e => e.Key.Equals(association));

            viewModel.informationBindings.Add(new InformationBindingViewModel(definitions) {
                roleType = informationBinding.roleType,
                role = informationBinding.role,
                informationType = informationBinding.informationType,
                informationUID = new InformationTypeID(informationBinding.informationType!, informationBinding.informationId),
            });
            return viewModel;
        }

        public static S100AttributeEditorViewModel operator +(S100AttributeEditorViewModel viewModel, featureBinding featureBinding) {
            var association = featureBinding.GetType().GetGenericArguments()[0].Name;

            var definitions = viewModel.featureBindingDefinitions!.GroupBy.Single(e => e.Key.Equals(association));

            viewModel.featureBindings.Add(new FeatureBindingViewModel(definitions) {
                roleType = featureBinding.roleType,
                role = featureBinding.role,
                featureType = featureBinding.featureType,
                featureUID = new FeatureTypeID(featureBinding.featureType!, featureBinding.featureId),
            });
            return viewModel;
        }


        public static explicit operator informationBinding[](S100AttributeEditorViewModel viewmodel) {
            informationBinding[] informationBinding = [];
            if (viewmodel.informationBindings.Any()) {
                foreach (var binding in viewmodel.informationBindings.ToImmutableArray()) {
                    if (binding.roleType is null) continue;

                    var f = binding.informationBindingDefinition!.CreateInstance()!;
                    f.informationType = binding.informationType;
                    f.informationId = binding.informationUID?.UID!;

                    informationBinding = [.. informationBinding, f];
                }
            }
            return informationBinding;
        }

        public static explicit operator featureBinding[](S100AttributeEditorViewModel viewmodel) {
            featureBinding[] featureBindings = [];
            if (viewmodel.featureBindings.Any()) {
                foreach (var binding in viewmodel.featureBindings.ToImmutableArray()) {
                    if (binding.roleType is null) continue;

                    var f = binding.featureBindingDefinition!.CreateInstance()!;
                    f.featureType = binding.featureType;
                    f.featureId = binding.featureUID?.UID!;

                    featureBindings = [.. featureBindings, f];
                }
            }
            return featureBindings;
        }
        #endregion

        #region Properties        

        private string _code = "UNKNOWN";

        public string code {
            get {
                return this._code;
            }
            set {
                this.SetProperty(ref this._code, value);
            }
        }

        public ObservableCollection<AttributeViewModel> attributeBindings { get; set; } = [];

        public ObservableCollection<InformationBindingViewModel> informationBindings { get; set; } = [];

        public ObservableCollection<FeatureBindingViewModel> featureBindings { get; set; } = [];

        public attributeBindingDefinition[] attributeBindingsCatalogue { get; init; } = [];
        #endregion

        public object? Instance => this._informationType != default ? this._informationType : this._featureType;

        public Func<string?> Flatten { get; private set; }

        private readonly S100FC.InformationType? _informationType = default;
        private readonly S100FC.FeatureType? _featureType = default;
        private readonly string _uid;
    }
#endif
}
