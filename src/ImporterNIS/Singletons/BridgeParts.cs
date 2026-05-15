using S100FC.S101.ComplexAttributes;
using S100Framework.Applications.S57.esri;
using S100Framework.Applications.Singletons;
using System;
using System.Collections.Generic;
using System.Text;

namespace ImporterNIS.Singletons
{    
    internal sealed class BridgeParts
    {
        private static BridgeParts? _instance;

        private Dictionary<string, CulturalFeaturesA> _parts = new Dictionary<string, CulturalFeaturesA>();

        internal static void Initialize() {
            _instance = new BridgeParts();
        }

        public static BridgeParts Instance {
            get {
                if (_instance == null)
                    Initialize();
                return _instance!;
            }
        }

        public void Add(string UID, CulturalFeaturesA culturalFeaturesA) {
            this._parts.Add(UID, culturalFeaturesA);
        }

        public CulturalFeaturesA? Parts(string UID) => this._parts.ContainsKey(UID) ? this._parts[UID] : null;
    }
}
