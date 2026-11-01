using ArcGIS.Core.Data;
using S100FC;
using S100FC.S101.ComplexAttributes;
using S100FC.S101.FeatureTypes;
using S100Framework.Applications.Singletons;
using System.Text.RegularExpressions;

namespace S100Framework.Applications
{
    using ArcGIS.Core.Data.UtilityNetwork.Trace;
    using ArcGIS.Core.Geometry;
    using S100FC.S101.InformationTypes;
    using S100FC.S101.SimpleAttributes;
    using S100Framework.Applications.S57.esri;

    internal static partial class ImporterNIS
    {

        private static LocalMagneticAnomaly LOCMAG(Feature current, RowBuffer buffer) {
            /*  S-65 Annex B -> LOCMAG
                The S-57 mandatory attribute VALLMA has been remodelled in S-101 as the mandatory complex
                attribute value of local magnetic anomaly, having sub-attributes magnetic anomaly value
                (mandatory) and reference direction, where:
                - magnetic anomaly value is intended to indicate both the positive (easterly) and negative
                (westerly) values where only a single instance of value of local magnetic anomaly is encoded,
                having no populated value for reference direction; or
                - magnetic anomaly value is intended to indicate an anomaly in a single direction, where only a
                single instance of value of local magnetic anomaly is encoded and reference direction is
                populated; or
                - magnetic anomaly value is intended to indicate an anomaly that is different in a positive
                (easterly) and negative (westerly) direction, where two instances of value of local magnetic
                anomaly are encoded and reference direction is populated for both instances.

                ** During the automated conversion process, the value populated in VALLMA will be converted across
                to magnetic anomaly value, noting that the value of VALLMA will be converted from minutes to
                decimal degrees for magnetic anomaly value. 

                Data Producers will be required to confirm whether
                the value populated in VALLMA is intended to indicate both the positive (easterly) and negative
                (westerly) values of the anomaly, or a disparate range; noting that S-57 guidance recommends
                encoding the values of a range in INFORM for the LOCMAG. Where the anomaly is a disparate
                range, Data Producers will be required to adjust value of local magnetic anomaly in accordance
                with the guidance above; and if the information contained in INFORM relates only to the range of
                anomaly values, remove the associated instance of the complex attribute information (see clause
                2.3).
           */
            var instance = new LocalMagneticAnomaly();


            var featureName = GetFeatureName(current.OBJNAM(), current.NOBJNM());
            if (featureName is not null)
                instance.featureName = featureName;

            if (current.SORDAT_HasValue()) {
                if (DateHelper.TryConvertSordat(current.SORDAT()!, out var reportedDate)) {
                    instance.reportedDate = reportedDate;
                }
                else {
                    Logger.Current.DataError(current.GetObjectID(), current.TableName(), current.LNAM() ?? "Unknown LNAM", $"Cannot convert date {current.SORDAT()}");
                }
            }

            if (current.VALLMA_HasValue()) {
                instance.valueOfLocalMagneticAnomaly = [new() {
                                    magneticAnomalyValue = current.VALLMA() != -32767m ? current.VALLMA() / 60 : null,
                                }];
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

            buffer["ps"] = ps101;
            buffer["code"] = instance.GetType().Name;
            buffer["attributebindings"] = instance.Flatten();
            buffer["informationbindings"] = System.Text.Json.JsonSerializer.Serialize(instance.GetInformationBindings(), jsonSerializerOptions);

            buffer["sourceIdentifier"] = instance.sourceIdentifier;
            SetShape(buffer, current.SHAPE());
            SetCompilationScale(buffer, current.PLTS_COMP_SCALE()!.Value);

            return instance;
        }
    }
}

