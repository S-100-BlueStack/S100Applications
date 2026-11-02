#define CREATE_SUBTYPES

using ArcGIS.Core.Data;
using ArcGIS.Core.Data.DDL;
using ArcGIS.Core.Geometry;

using ImporterNIS.Singletons;
using S100FC;
using S100FC.Catalogues;
using S100FC.S101;
using S100FC.S101.ComplexAttributes;
using S100FC.S101.FeatureAssociation;
using S100FC.S101.FeatureTypes;
using S100FC.S101.InformationAssociation;
using S100FC.S101.InformationTypes;
using S100FC.S101.SimpleAttributes;
using S100Framework.Applications.S57.esri;
using S100Framework.Applications.Singletons;
using System.Data;
using System.Globalization;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using VortexLoader;
using VortexLoader.Singletons;

using IO = System.IO;


namespace S100Framework.Applications
{
    internal static partial class ImporterNIS
    {
        private static void RegisterConverters()
        {
            //_converterRegistry.Register<AidsToNavigationP, CardinalBeacon>(Converters.CreateCardinalBeacon);
            _converterRegistry.Register<AidsToNavigationP, RadarTransponderBeacon>(Converters.CreateRadarTransponderBeacon);
            _converterRegistry.Register<AidsToNavigationP, LightAllAround>(Converters.CreateLightAllAround);
            _converterRegistry.Register<CulturalFeaturesP, LightSectored>(Converters.CreateLightSectored);
            _converterRegistry.Register<CulturalFeaturesP, Building>(Converters.CreateBuilding);
            _converterRegistry.Register<AidsToNavigationP, LightSectored>(Converters.CreateLightSectored);
            _converterRegistry.Register<AidsToNavigationP, LightAirObstruction>(Converters.CreateLightAirObstruction);
            _converterRegistry.Register<AidsToNavigationP, LightFogDetector>(Converters.CreateLightFogDetector);
            _converterRegistry.Register<AidsToNavigationP, Daymark>(Converters.CreateDaymark);
            _converterRegistry.Register<DangersP, Obstruction>(Converters.CreateObstruction);
            //_converterRegistry.Register<DangersA, Obstruction>(Converters.CreateObstruction);
            //_converterRegistry.Register<DangersL, Obstruction>(Converters.CreateObstruction);

            _converterRegistry.Register<AidsToNavigationP, Retroreflector>(Converters.CreateRetroreflector);
            _converterRegistry.Register<CulturalFeaturesA, LightSectored>(Converters.CreateLightSectored);
            _converterRegistry.Register<PortsAndServicesP, LightSectored>(Converters.CreateLightSectored);
            _converterRegistry.Register<PortsAndServicesP, SignalStationWarning>(Converters.CreateSignalStationWarning);
            _converterRegistry.Register<AidsToNavigationP, FogSignal>(Converters.CreateFogSignal);
            _converterRegistry.Register<AidsToNavigationP, RadarStation>(Converters.CreateRadarStation);
            _converterRegistry.Register<CulturalFeaturesP, WindTurbine>(Converters.CreateWindturbine);
            _converterRegistry.Register<PortsAndServicesP, SignalStationTraffic>(Converters.CreateSignalStationTraffic);
            _converterRegistry.Register<AidsToNavigationP, RadioStation>(Converters.CreateRadioStation);
            _converterRegistry.Register<OffshoreInstallationsP, LightSectored>(Converters.CreateLightSectored);
            _converterRegistry.Register<OffshoreInstallationsP, RadarTransponderBeacon>(Converters.CreateRadarTransponderBeacon);
            _converterRegistry.Register<OffshoreInstallationsP, FogSignal>(Converters.CreateFogSignal);


        }
    }
}
