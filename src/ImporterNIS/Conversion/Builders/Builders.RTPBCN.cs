using ArcGIS.Core.Data;
using S100FC.S101.FeatureTypes;
using S100Framework.Applications.Singletons;

namespace S100Framework.Applications
{
    using S100FC.S101.ComplexAttributes;
    using S100Framework.Applications.S57.esri;

    internal static partial class ImporterNIS
    {

        private static RadarTransponderBeacon RTPBCN(Feature current) {
            var instance = new RadarTransponderBeacon();

            //var beacons = FeatureRelations.Instance.GetRelated<AidsToNavigationP>(typeof(RadarTransponderBeacon), structure.GlobalId);
            //AidsToNavigationP current = structure as AidsToNavigationP;
            //if (beacons.Count == 0) {
            //    Logger.Current.Error($"No related RadarTransponderBeacons. Returning empty RadarTransponderBeacon (GlobalId: {structure.GlobalId})");
            //    return instance;
            //}
            //else {
            //    current = beacons.First();
            //}

            if (current.CATRTB_HasValue()) {
                instance.categoryOfRadarTransponderBeacon = EnumHelper.GetEnumValue(current.CATRTB());
            }

            var featureName = GetFeatureName(current.OBJNAM(), current.NOBJNM());
            if (featureName is not null)
                instance.featureName = featureName;

            DateHelper.TryGetFixedDateRange(current.DATSTA(), current.DATEND(), out var dateRange);
            if (dateRange != default) {
                instance.fixedDateRange = dateRange;
            }

            DateHelper.TryGetPeriodicDateRange(current.PERSTA(), current.PEREND(), out var periodicDateRange);
            if (periodicDateRange != default) {
                instance.periodicDateRange = periodicDateRange;
            }

            if (current.RADWAL_HasValue()) {
                if (ImporterNIS.TryGetRadarWaveLengths(current.RADWAL()!, out var lengths)) {
                    instance.radarWaveLength = lengths;
                }
            }

            if (current.SECTR1_HasValue() && current.SECTR2_HasValue()) {
                instance.sectorLimit = new sectorLimit() {
                    sectorLimitOne = new sectorLimitOne {
                        sectorBearing = current.SECTR1(),
                    },
                    sectorLimitTwo = new sectorLimitTwo {
                        sectorBearing = current.SECTR2(),
                    }
                };
            }

            var rhythmOfLight = ImporterNIS.GetRythmOfLight<RadarTransponderBeacon>(new AidsToNavigationP(current));

            if (current.SIGGRP_HasValue()) {
                instance.signalGroup = current.SIGGRP();
            }

            if (current.SIGSEQ_HasValue()) {
                instance.signalSequence = rhythmOfLight.signalSequence;
            }

            if (current.STATUS_HasValue()) {
                var status = EnumHelper.GetEnumValues(current.STATUS(), instance.attributeBindingDefinition("status")!.permitedValues!);
                if (status is not null && status.Any())
                    instance.status = status;
            }

            if (current.PLTS_COMP_SCALE_HasValue()) {
                string subtype = "";
                if (current.TableName() != default && !Subtypes.Instance.TryGetSubtype(current.TableName(), current.FCSubtype(), out subtype))
                    throw new NotSupportedException($"Unknown subtype for {current.TableName()}, {current.FCSubtype()}");
                var scaleMinimum = Scamin.Instance.GetMinimumScale(current, subtype, current.PLTS_COMP_SCALE()!.Value, isRelatedToStructure: false);
                if (scaleMinimum.HasValue)
                    instance.scaleMinimum = scaleMinimum;
            }

            var result = ImporterNIS.AddInformation(current.GetObjectID(), current.TableName(), current.NTXTDS(), current.TXTDSC(), current.INFORM(), current.NINFOM());
            instance.information = [.. result.information];
            instance.SetInformationBindings(result.InformationBindings.ToArray());

            return instance;
        }
    }
}

