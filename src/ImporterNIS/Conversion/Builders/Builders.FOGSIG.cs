using ArcGIS.Core.Data;
using S100FC.S101.FeatureTypes;
using S100Framework.Applications.Singletons;

namespace S100Framework.Applications
{
    using S100Framework.Applications.S57.esri;

    internal static partial class ImporterNIS
    {

        private static FogSignal FOGSIG(Feature current) {
            var instance = new FogSignal();

            //var fogsignals = FeatureRelations.Instance.GetRelated<AidsToNavigationP>(typeof(FogSignal), structure.GlobalId);
            //AidsToNavigationP current = structure as AidsToNavigationP;
            //if (fogsignals.Count == 0) {
            //    Logger.Current.Error($"No related fogsignals. Returning empty fogsignal (GlobalId: {structure.GlobalId})");
            //    return instance;
            //}
            //else {
            //    current = fogsignals.First();
            // }

            if (current.CATFOG_HasValue()) {
                instance.categoryOfFogSignal = EnumHelper.GetEnumValue(current.CATFOG());
            }

            var featureName = GetFeatureName(current.OBJNAM(), current.NOBJNM());
            if (featureName is not null)
                instance.featureName = featureName;

            DateHelper.TryGetFixedDateRange(current.DATSTA(), current.DATEND(), out var dateRange);
            if (dateRange != default) {
                instance.fixedDateRange = dateRange;
            }

            // TODO: interoperabilityidentifier

            DateHelper.TryGetPeriodicDateRange(current.PERSTA(), current.PEREND(), out var periodicDateRange);
            if (periodicDateRange != default) {
                instance.periodicDateRange = periodicDateRange;
            }

            if (current.SIGFRQ_HasValue()) {
                instance.signalFrequency = current.SIGFRQ();
            }
            if (current.SIGGEN_HasValue()) {
                instance.signalGeneration = EnumHelper.GetEnumValue(current.SIGGEN());
            }
            if (current.SIGGRP_HasValue()) {
                instance.signalGroup = current.SIGGRP();
            }

            if (current.SIGPER_HasValue()) {
                instance.signalPeriod = current.SIGPER() == -32767m ? null : current.SIGPER() == 0 ? null : current.SIGPER();
            }

            if (current.SIGSEQ_HasValue()) {
                instance.signalSequence = ImporterNIS.GetSignalSequences(current.SIGSEQ());
            }

            if (current.STATUS_HasValue()) {
                var status = EnumHelper.GetEnumValues(current.STATUS(), instance.attributeBindingDefinition("status")!.permitedValues!);
                if (status is not null && status.Any())
                    instance.status = status;
            }

            if (current.VALMXR_HasValue()) {
                instance.valueOfMaximumRange = current.VALMXR();
            }

            if (current.PLTS_COMP_SCALE_HasValue()) {
                string subtype = "";

                if (!Subtypes.Instance.TryGetSubtype(current.TableName(), current.FCSubtype(), out subtype))
                    throw new NotSupportedException($"Unknown subtype for {current.TableName}, {current.FCSubtype()}");

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

