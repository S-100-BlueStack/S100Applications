using System;
using System.Collections.Generic;
using System.Text;

namespace SelectorUI
{

    // NOTE: Generated code may require at least .NET Framework 4.5 or .NET Core/Standard 2.0.
    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class Rules
    {

        private RulesRule ruleField;

        /// <remarks/>
        public RulesRule Rule {
            get {
                return this.ruleField;
            }
            set {
                this.ruleField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RulesRule
    {

        private string productIdField;

        private string typeField;

        private string operatorField;

        private byte valueField;

        private subAttributeBinding[] subAttributeBindingField;

        private string refField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://www.iho.int/S100FC/5.2")]
        public string productId {
            get {
                return this.productIdField;
            }
            set {
                this.productIdField = value;
            }
        }

        /// <remarks/>
        public string Type {
            get {
                return this.typeField;
            }
            set {
                this.typeField = value;
            }
        }

        /// <remarks/>
        public string Operator {
            get {
                return this.operatorField;
            }
            set {
                this.operatorField = value;
            }
        }

        /// <remarks/>
        public byte Value {
            get {
                return this.valueField;
            }
            set {
                this.valueField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("subAttributeBinding", Namespace = "http://www.iho.int/S100FC/5.2")]
        public subAttributeBinding[] subAttributeBinding {
            get {
                return this.subAttributeBindingField;
            }
            set {
                this.subAttributeBindingField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string @ref {
            get {
                return this.refField;
            }
            set {
                this.refField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.iho.int/S100FC/5.2")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "http://www.iho.int/S100FC/5.2", IsNullable = false)]
    public partial class subAttributeBinding
    {

        private subAttributeBindingAttribute attributeField;

        /// <remarks/>
        public subAttributeBindingAttribute attribute {
            get {
                return this.attributeField;
            }
            set {
                this.attributeField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.iho.int/S100FC/5.2")]
    public partial class subAttributeBindingAttribute
    {

        private string refField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string @ref {
            get {
                return this.refField;
            }
            set {
                this.refField = value;
            }
        }
    }


}
