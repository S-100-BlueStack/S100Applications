using S100FC;
using System.Collections;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using static S100Framework.WPF.ViewModel.S100AttributeEditorViewModel;

namespace S100Framework.WPF.ViewModel
{
    public class S100AttributeEditorViewModel : INotifyPropertyChanged, IAttributeBindingContainer, INotifyDataErrorInfo
    {
        #region Delegates
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
        #endregion

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

        public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

        private string _errorMessage = string.Empty;

        public string ErrorMessage {
            get {
                return this._errorMessage;
            }
            set {
                this.SetProperty(ref this._errorMessage, value);
                this.IsErrorMessageEnabled = !string.IsNullOrEmpty(value);
            }
        }

        private bool _isErrorMessageEnabled;

        public bool IsErrorMessageEnabled {
            get {
                return this._isErrorMessageEnabled;
            }
            set {
                this.SetProperty(ref this._isErrorMessageEnabled, value);
            }
        }

        private Action<AddError, IEnumerable<attributeBinding>>[] _validators { get; set; } = [];

        public bool HasErrors => this._errors.Any();

        public IEnumerable GetErrors(string? propertyName) {
            if (!nameof(this.attributeBindings).Equals(propertyName)) return Enumerable.Empty<string>();
            return this._errors;
        }

        private void Validate() {
            var hasErrors = this.HasErrors;

            this._errors = [];
            if (this._validators is not null && this._validators.Any())
                foreach (var action in this._validators) {
                    action.Invoke(this.AddError, this.attributeBindings.Select(e => e.attribute));
                }

            if (this.HasErrors) {
                this.ErrorMessage = string.Join(Environment.NewLine, this._errors);

                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.attributeBindings)));
            }

            if (this.IsErrorMessageEnabled != this.HasErrors) {
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.attributeBindings)));
                this.IsErrorMessageEnabled = this.HasErrors;
            }
        }

        public void AddError(string propertyName, string error) {
            this._errors = [.. this._errors, error];
        }

        private string[] _errors = [];

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

        public string[] InformationTypes = [];

        public string[] FeatureTypes = [];

        public XElement? GetElement(string code) => this._featureCatalogue?.XPathSelectElement($"//S100FC:*[S100FC:code='{code}']", this._namespaceManager);

        public S100AttributeEditorViewModel(XDocument featureCatalogue, ILookup<string, XElement> constraints) {
            this._featureCatalogue = featureCatalogue;
            this._constraints = constraints;

            var navigator = featureCatalogue.CreateNavigator();
            navigator.MoveToFollowing(XPathNodeType.Element);

            var scopes = navigator.GetNamespacesInScope(XmlNamespaceScope.All);

            this._namespaceManager = new XmlNamespaceManager(new NameTable());
            foreach (var s in scopes)
                this._namespaceManager.AddNamespace(s.Key, s.Value);

            this.InformationTypes = featureCatalogue.Descendants(XName.Get("S100_FC_InformationType", scopes["S100FC"])).Select(e => e.Element(XName.Get("code", scopes["S100FC"]))!.Value).ToArray();
            this.FeatureTypes = featureCatalogue.Descendants(XName.Get("S100_FC_FeatureType", scopes["S100FC"])).Select(e => e.Element(XName.Get("code", scopes["S100FC"]))!.Value).ToArray();

            this.permittedPrimitives = featureCatalogue.Descendants(XName.Get("S100_FC_FeatureType", scopes["S100FC"])).ToDictionary(
                e => e.Element(XName.Get("code", scopes["S100FC"]))!.Value,
                e => e.Elements(XName.Get("permittedPrimitives", scopes["S100FC"])).Select(e => e.Value).ToArray()).ToImmutableDictionary<string, string[]>();

            this.sourceIdentifiers = featureCatalogue.Descendants(XName.Get("S100_FC_FeatureType", scopes["S100FC"])).ToDictionary(
                e => e.Element(XName.Get("code", scopes["S100FC"]))!.Value,
                e => e.Element(XName.Get("definitionReference", scopes["S100FC"]))?.Element(XName.Get("sourceIdentifier", scopes["S100FC"]))?.Value).ToImmutableDictionary<string, string?>();


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

        public S100AttributeEditorViewModel Initialize(string code, string uid) {
            if (string.IsNullOrEmpty(code) || string.IsNullOrWhiteSpace(code)) throw new System.ArgumentNullException(nameof(code));

            this.code = code;
            this.UID = uid;

            var scope = this._namespaceManager.LookupNamespace("S100FC")!;

            var simpleAttributes = this._featureCatalogue.Descendants(XName.Get("S100_FC_SimpleAttribute", scope)).ToDictionary(e => e.Element(XName.Get("code", scope))!.Value, e => e);

            var complexAttributes = this._featureCatalogue.Descendants(XName.Get("S100_FC_ComplexAttribute", scope)).ToDictionary(e => e.Element(XName.Get("code", scope))!.Value, e => e);


            var element = this._featureCatalogue.XPathSelectElement($"//S100FC:*[S100FC:code='{code}']", this._namespaceManager);
            if (element is null) throw new KeyNotFoundException($"Code not found ({code})!");
            if (element.Attribute("isAbstract") != default && bool.Parse(element.Attribute("isAbstract")!.Value)) throw new InvalidOperationException($"Abstract types are not supported ({code})!");

            int index = 0;
            this.attributeBindingsCatalogue = Parser.AttributeBindings(this._featureCatalogue, code, ref index, simpleAttributes, complexAttributes);

            if (element.Name.LocalName.Equals("S100_FC_InformationType") || element.Name.LocalName.Equals("S100_FC_FeatureType"))
                this._informationBindingDefinitions = Parser.InformationBindings(this._featureCatalogue, code);
            if (element.Name.LocalName.Equals("S100_FC_FeatureType"))
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

            //  Object level validation
            {
                var rules = this._constraints.SelectMany(e => e).Where(e => e.Attribute("attribute") is null || e.Attribute("code")!.Value.Equals(code));

                foreach (var e in rules) {
                    var type = e.Element("type")!.Value;

                    if ("ConditionalMandatoryCount".Equals(type)) {
                        var subAttribute = e.Element("subAttributeBinding")!.Element("attribute")!.Attribute("ref")!.Value;

                        var condition = e.Element("condition");
                        if (condition is not null) {
                            var _attribute = e.Element("condition")!.Element("attribute")!.Value;
                            var _operator = e.Element("condition")!.Element("operator")!.Value;
                            var _value = e.Element("condition")!.Element("value")!.Value;

                            Action<AddError, IEnumerable<attributeBinding>> validator = (action, instance) => {
                                var _ = instance.Where(e => e.S100FC_code.Equals(_attribute));
                                var count = _.Count();

                                var match = _operator switch {
                                    "eq" => count.Equals(int.Parse(_value)),
                                    "ne" => !count.Equals(int.Parse(_value)),
                                    "gt" => count > int.Parse(_value),
                                    "lt" => count < int.Parse(_value),
                                    _ => false,
                                };

                                if (match) {
                                    var containsAttribute = false;
                                    foreach (var s in _) {
                                        if (default != _.SingleOrDefault(e => e.S100FC_code.Equals(subAttribute)))
                                            containsAttribute = true;
                                    }
                                    if (!containsAttribute) {
                                        var error = $"The sub-complex attribute {subAttribute} is mandatory if more than one instance of the complex attribute {_attribute} is encoded.";
                                        action(subAttribute, error);
                                    }
                                }
                            };

                            this._validators = [.. this._validators, validator];
                        }
                    }
                }
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
                    var subAttributes = complexAttribute.attributeBindingsCatalogue.Select(e => e.attribute).ToArray();

                    var rules = this._constraints.SelectMany(e => e).Where(e => e.Attribute("attribute") is null || e.Attribute("code")!.Value.Equals(code));

                    //var viewModel = new ComplexAttributeViewModel(ref complexAttribute, [..this._rules[code], .. this._rules.Where(e => e.Attribute.Contains(e.Key)).SelectMany(e=>e)]);
                    var viewModel = new ComplexAttributeViewModel(ref complexAttribute, rules);
                    this.attributeBindings.Add(viewModel);
                }
                else
                    throw new NotImplementedException();
            }
            this.Validate();

            //note: Must be added right by the end!
            this.attributeBindings.CollectionChanged += (s, e) => {
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.attributeBindings)));
                this.Validate();
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
        public string? ProductID => this._featureCatalogue.XPathSelectElement("/S100FC:S100_FC_FeatureCatalogue/S100FC:productId", this._namespaceManager)?.Value;

        private string _code = "UNKNOWN";

        public string code {
            get {
                return this._code;
            }
            set {
                this.SetProperty(ref this._code, value);
            }
        }

        private string _uid = "UNKNOWN";

        public string UID {
            get {
                return this._uid;
            }
            set {
                this.SetProperty(ref this._uid, value);
            }
        }

        public ObservableCollection<AttributeViewModel> attributeBindings { get; set; } = [];

        public ObservableCollection<InformationBindingViewModel> informationBindings { get; set; } = [];

        public ObservableCollection<FeatureBindingViewModel> featureBindings { get; set; } = [];

        public bool HasInformationBindings => this._informationBindingDefinitions.Any();

        public bool HasFeatureBindings => this._featureBindingDefinitions.Any();

        public ImmutableDictionary<string, string[]> permittedPrimitives { get; init; } = [];

        public ImmutableDictionary<string, string?> sourceIdentifiers { get; init; } = [];

        public informationBindingContainer? informationBindingDefinitions { get; private set; } = null;

        public featureBindingContainer? featureBindingDefinitions { get; private set; } = null;

        public attributeBindingDefinitionViewModel[] attributeBindingsCatalogue { get; private set; } = [];

        public string[] GetFeaturesByPrimitive(Primitives primitive) => this.permittedPrimitives.Where(e => e.Value.Contains($"{primitive}")).Select(e => e.Key).ToArray();
        #endregion

        #region Operators
        //public static S100AttributeEditorViewModel operator +(S100AttributeEditorViewModel viewModel, informationBinding informationBinding) {
        //    var association = informationBinding.GetType().GetGenericArguments()[0].Name;

        //    //var definitions = viewModel.informationBindingDefinitions!.GroupBy.Single(e => e.Key.Equals(association));

        //    //viewModel.informationBindings.Add(new InformationBindingViewModel(definitions) {
        //    //    roleType = informationBinding.roleType,
        //    //    role = informationBinding.role,
        //    //    informationType = informationBinding.informationType,
        //    //    informationUID = new InformationTypeID(informationBinding.informationType!, informationBinding.informationId),
        //    //});
        //    return viewModel;
        //}

        //public static S100AttributeEditorViewModel operator +(S100AttributeEditorViewModel viewModel, featureBinding featureBinding) {
        //    var association = featureBinding.GetType().GetGenericArguments()[0].Name;

        //    //var definitions = viewModel.featureBindingDefinitions!.GroupBy.Single(e => e.Key.Equals(association));

        //    //viewModel.featureBindings.Add(new FeatureBindingViewModel(definitions) {
        //    //    roleType = featureBinding.roleType,
        //    //    role = featureBinding.role,
        //    //    featureType = featureBinding.featureType,
        //    //    featureUID = new FeatureTypeID(featureBinding.featureType!, featureBinding.featureId),
        //    //});
        //    return viewModel;
        //}

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

        private informationBindingDefinition[] _informationBindingDefinitions { get; set; } = [];

        private featureBindingDefinition[] _featureBindingDefinitions { get; set; } = [];

        private XDocument _featureCatalogue { get; init; }

        public ILookup<string, XElement> Constraints => this._constraints;

        private ILookup<string, XElement> _constraints { get; init; }

        private XmlNamespaceManager _namespaceManager { get; init; }

        private bool _isInitialized = false;

        private void Viewmodel_PropertyChanged(object? sender, PropertyChangedEventArgs e) {
            var _internal = e.PropertyName switch {
                "ErrorMessage" => true,
                "IsErrorMessageEnabled" => true,
                _ => false,
            };

            if (_internal) {
                this.PropertyChanged?.Invoke(this, e);
                return;
            }

            if (sender is AttributeViewModel attribute) {
                //if (!attribute.attribute.IsValid(this.attributeBindings.Select(e => e.attribute))) {
                //    this._errors[attribute.code] = new List<string> { "Dependency" };
                //    ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(attribute.code));
                //}

                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.attributeBindings)));

                if (attribute is ComplexAttributeViewModel complexAttribute) {
                    var binding = this.attributeBindingsCatalogue.Single(e => e.attribute.Equals(complexAttribute.code));
                    if (binding.Validators.Any()) {
                        foreach (var action in binding.Validators)
                            action.Invoke(complexAttribute.AddError, complexAttribute.attribute);
                    }
                }

                if (attribute is INotifyDataErrorInfo notifyDataError) {
                    if (notifyDataError.HasErrors) {

                    }
                }

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
            public static attributeBindingDefinitionViewModel[] AttributeBindings(XDocument featureCatalogue, string code, ref int index, IDictionary<string, XElement> simpleAttributes, IDictionary<string, XElement> complexAttributes) {
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

                attributeBindingDefinitionViewModel[] attributeBindingDefinitions = [];

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

            private static Func<CodeListAttribute> CreateCodedList(XElement attributeBindingElement, XElement simpleAttributeElement, XmlNamespaceManager xmlNamespaceManager) {
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

                return () => new CodeListAttribute {
                    S100FC_code = simpleAttributeElement.Element(XName.Get("code", scope))!.Value,
                    S100FC_name = simpleAttributeElement.Element(XName.Get("name", scope))!.Value,
                    listedValues = listedValues,
                };
            }

            private static (Func<attributeBinding> creator, attributeBindingDefinitionViewModel attributeBindingDefinition) CreateAttributeBinding(XElement binding, XmlNamespaceManager xmlNamespaceManager, IDictionary<string, XElement> simpleAttributes, IDictionary<string, XElement> complexAttributes) {
                var scope = xmlNamespaceManager.LookupNamespace("S100FC")!;

                var referenceCode = binding.Element(XName.Get("attribute", scope))!.Attribute("ref")!.Value!;
                var lower = int.Parse(binding.XPathSelectElement("S100FC:multiplicity/S100Base:lower", xmlNamespaceManager)!.Value);
                var _ = binding.XPathSelectElement("S100FC:multiplicity/S100Base:upper", xmlNamespaceManager)!;
                int upper = (_.Attribute(XName.Get("infinite")) != default && _.Attribute(XName.Get("infinite"))!.Value.Equals("true")) ? int.MaxValue : int.Parse(_.Value!);

                if (simpleAttributes.ContainsKey(referenceCode)) {
                    var simpleAttribute = simpleAttributes[referenceCode];

                    var valueType = simpleAttribute.Element(XName.Get("valueType", scope))!.Value;

                    Func<attributeBinding> attributeBinding = valueType switch {
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
                        "S100_CodeList" => CreateCodedList(binding, simpleAttribute, xmlNamespaceManager),
                        _ => throw new NotImplementedException(),
                    };

                    var constraints = simpleAttribute.Element(XName.Get("constraints", scope))?.Elements();

                    var attributeBindingDefinition = new attributeBindingDefinitionViewModel {
                        attribute = referenceCode,
                        lower = lower,
                        upper = upper,
                        //order = index++,
                        CreateInstance = () => attributeBinding(),
                    };

                    if (constraints is not null && constraints.Any()) {
                        foreach (var constraint in constraints) {
                            if ("stringLength".Equals(constraint.Name.LocalName)) {
                                var stringLength = int.Parse(constraint.Value);

                                Action<AddError, attributeBinding> validator = (action, instance) => {
                                    if (instance is TextAttribute textAttribute) {
                                        if (stringLength < textAttribute.value?.Length) {
                                            action("", $"StringLengthConstraint: {stringLength}!");
                                        }
                                    }
                                };
                                attributeBindingDefinition.Validators = [.. attributeBindingDefinition.Validators, validator];
                            }
                            if ("precision".Equals(constraint.Name.LocalName)) {
                                var precision = int.Parse(constraint.Value);

                                Action<AddError, attributeBinding> validator = (action, instance) => {
                                    if (instance is RealAttribute realAttribute) {
                                        if (realAttribute.value.HasValue) {
                                            var rounded = Math.Round(realAttribute.value.Value, precision);
                                            if (rounded != realAttribute.value.Value)
                                                action("", $"PrecisionConstraint: {precision}!");
                                        }
                                    }
                                    if (instance is IntegerAttribute integerAttribute) {
                                        if (integerAttribute.value.HasValue) {
                                            if (Math.Pow(10, precision) < integerAttribute.value.Value)
                                                action("", $"PrecisionConstraint: {precision}!");
                                        }
                                    }
                                };
                                attributeBindingDefinition.Validators = [.. attributeBindingDefinition.Validators, validator];
                            }
                            if ("textPattern".Equals(constraint.Name.LocalName)) {
                                Action<AddError, attributeBinding> validator = (action, instance) => {
                                    if (instance is TextAttribute textAttribute) {
                                        var regex = new Regex(constraint.Value);
                                        if (string.IsNullOrEmpty(textAttribute.value)) return;
                                        if (!regex.IsMatch(textAttribute.value)) {
                                            action("", $"PatternConstraint: {constraint.Value}!");
                                        }
                                    }
                                };
                                attributeBindingDefinition.Validators = [.. attributeBindingDefinition.Validators, validator];
                            }
                            if ("range".Equals(constraint.Name.LocalName)) {
                                var lowerBound = constraint.Element(XName.Get("lowerBound", xmlNamespaceManager.LookupNamespace("S100Base")!));
                                var upperBound = constraint.Element(XName.Get("upperBound", xmlNamespaceManager.LookupNamespace("S100Base")!));
                                var closure = constraint.Element(XName.Get("closure", xmlNamespaceManager.LookupNamespace("S100Base")!))!.Value;

                                Action<AddError, attributeBinding> validator = (action, instance) => {
                                    if (instance is RealAttribute realAttribute) {
                                        if (realAttribute.value.HasValue) {
                                            var _lowerBound = lowerBound is null ? decimal.MinValue : decimal.Parse(lowerBound!.Value, CultureInfo.InvariantCulture);
                                            var _upperBound = upperBound is null ? decimal.MaxValue : decimal.Parse(upperBound!.Value, CultureInfo.InvariantCulture);

                                            var _ = realAttribute.value!.Value;
                                            var error = closure switch {
                                                "openInterval" => !(_ > _lowerBound && _ < _upperBound),        // The open interval, lower < x < upper
                                                "geLtInterval" => !(_ >= _lowerBound && _ < _upperBound),       // The right half-open interval, lower ≤ x < upper
                                                "gtLeInterval" => !(_ > _lowerBound && _ <= _upperBound),       // The left half-open interval, lower < x ≤ upper
                                                "closedInterval" => !(_ >= _lowerBound && _ <= _upperBound),    // The closed interval, lower ≤ x ≤ upper
                                                "gtSemiInterval" => !(_lowerBound < _),                         // The left half-open ray, lower < x
                                                "geSemiInterval" => !(_lowerBound <= _),                        // The left closed ray, lower ≤ x
                                                "ltSemiInterval" => !(_<_upperBound),                           // The right half-open ray, x < upper
                                                "leSemiInterval" => !(_<=_upperBound),                          // The right closed ray, x ≤ upper
                                                _ => throw new NotImplementedException(),
                                            };
                                            if (error)
                                                action("", $"RangeConstraint: {closure}, {_lowerBound}, {_upperBound}!");
                                        }
                                    }
                                    if (instance is IntegerAttribute integerAttribute) {
                                        if (integerAttribute.value.HasValue) {
                                            var _lowerBound = lowerBound is null ? int.MinValue : int.Parse(lowerBound!.Value);
                                            var _upperBound = upperBound is null ? int.MaxValue : int.Parse(upperBound!.Value);

                                            var _ = integerAttribute.value!.Value;
                                            var error = closure switch {
                                                "openInterval" => !(_ > _lowerBound && _ < _upperBound),        // The open interval, lower < x < upper
                                                "geLtInterval" => !(_ >= _lowerBound && _ < _upperBound),       // The right half-open interval, lower ≤ x < upper
                                                "gtLeInterval" => !(_ > _lowerBound && _ <= _upperBound),       // The left half-open interval, lower < x ≤ upper
                                                "closedInterval" => !(_ >= _lowerBound && _ <= _upperBound),    // The closed interval, lower ≤ x ≤ upper
                                                "gtSemiInterval" => !(_lowerBound < _),                         // The left half-open ray, lower < x
                                                "geSemiInterval" => !(_lowerBound <= _),                        // The left closed ray, lower ≤ x
                                                "ltSemiInterval" => !(_ < _upperBound),                         // The right half-open ray, x < upper
                                                "leSemiInterval" => !(_ <= _upperBound),                        // The right closed ray, x ≤ upper
                                                _ => throw new NotImplementedException(),
                                            };
                                            if (error)
                                                action("", $"RangeConstraint: {closure}, {_lowerBound}, {_upperBound}!");
                                        }
                                    }
                                };
                                attributeBindingDefinition.Validators = [.. attributeBindingDefinition.Validators, validator];
                            }

                            //if (constraints.Element(XName.Get("textPattern", scopes["S100CD"])) != default) {
                            //    var textPattern = constraints.Element(XName.Get("textPattern", scopes["S100CD"]))!.Value;
                            //    roslyn.AppendLine($"\t[TextPatternConstraint(@\"{textPattern}\")]"); //Replace("\\","\\\\")
                            //}                            
                        }
                    }

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

                    var attributeBindingDefinition = new attributeBindingDefinitionViewModel {
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

            private static readonly Regex _regexArray = new Regex(@"\[\d+\]", RegexOptions.Singleline | RegexOptions.IgnorePatternWhitespace);
        }
    }
}
