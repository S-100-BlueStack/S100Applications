using ArcGIS.Core.Data;
using S100FC.S101.FeatureTypes;
using S100Framework.Applications.Singletons;

namespace S100Framework.Applications
{
    using S100Framework.Applications.S57.esri;

    internal static partial class ImporterNIS
    {

        private static Daymark DAYMAR(Feature current) {
            var instance = new Daymark {
            };

            if (current.CATSPM_HasValue()) {
                var categoryOfSpecialPurposeMark = EnumHelper.GetEnumValues(current.CATSPM());
                if (categoryOfSpecialPurposeMark is not null)
                    instance.categoryOfSpecialPurposeMark = categoryOfSpecialPurposeMark;
            }

            if (current.COLOUR_HasValue()) {
                var colours = ImporterNIS.GetColours(current.COLOUR()!);
                if (colours is not null)
                    instance.colour = colours;
            }

            if (current.COLPAT_HasValue()) {
                if (instance.colour is not null && instance.colour.Length > 1) {
                    //if (current.COLPAT.Contains(",")) {
                    //    var colpats = current.COLPAT.Split(',');
                    //    Logger.Current.DataError(current.OBJECTID ?? -1, current.TableName!, current.LNAM ?? "Unknown LNAM", $"Illegal COLPAT: {current.COLPAT}. Only {colpats[0]} is used. The colors needs reviewing.");
                    //    instance.colourPattern = ImporterNIS.GetColourPattern(colpats[0])?.value;
                    //}
                    //else {
                    instance.colourPattern = "-32767".Equals(current.COLPAT()) ? null : ImporterNIS.GetColourPattern(current.COLPAT()!)!.value;
                    //}
                }
            }

            if (current.ELEVAT_HasValue()) {
                instance.elevation = current.ELEVAT() == -32767m ? null : current.ELEVAT();
            }

            var featureName = GetFeatureName(current.OBJNAM(), current.NOBJNM());
            if (featureName is not null)
                instance.featureName = featureName;

            DateHelper.TryGetFixedDateRange(current.DATSTA(), current.DATEND(), out var dateRange);
            if (dateRange != default) {
                instance.fixedDateRange = dateRange;
            }

            if (current.HEIGHT_HasValue()) {
                instance.height = current.HEIGHT() != -32767m ? current.HEIGHT() : null;
            }

            // TODO: interoperabilityidentifier

            if (current.NATCON_HasValue()) {
                var natureOfConstruction = EnumHelper.GetEnumValues(current.NATCON());
                if (natureOfConstruction != null && natureOfConstruction.Any())
                    instance.natureOfConstruction = natureOfConstruction;
            }

            DateHelper.TryGetPeriodicDateRange(current.PERSTA(), current.PEREND(), out var periodicDateRange);
            if (periodicDateRange != default) {
                instance.periodicDateRange = [.. periodicDateRange];
            }

            if (current.CONRAD_HasValue()) {
                instance.radarConspicuous = current.CONRAD() == 2 ? false : true;
            }

            if (current.STATUS_HasValue()) {
                var status = EnumHelper.GetEnumValues(current.STATUS(), instance.attributeBindingDefinition("status")!.permitedValues!);
                if (status is not null && status.Any())
                    instance.status = status;
            }

            if (current.TOPSHP_HasValue()) {
                instance.topmarkDaymarkShape = EnumHelper.GetEnumValue(current.TOPSHP());
            }

            if (current.VERLEN_HasValue()) {
                instance.verticalLength = current.VERLEN() != -32767m ? current.VERLEN() : null;
            }

            // TODO: shapeInformation

            if (current.PICREP_HasValue()) {
                instance.pictorialRepresentation = ImporterNIS.FixFilename(current.PICREP()!);
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

