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

        private static River RIVERS(Feature current, RowBuffer buffer) {

            /* S-57 allows for RIVERS of geometric primitive area to be covered by the Group 1 objects LNDARE
               or UNSARE, however in S-101 all Rivers of geometric primitive area must be covered by the Skin
               of the Earth feature Land Area. During the automated conversion process, the converter may have
               the capability to convert UNSARE covering RIVERS to Land Area (taking into account the attribution
               of any adjoining LNDARE objects) and merge with any adjoining Land Area features. If the
               converter does not have this capability, Data Producers are advised to check their S-57 data
               holdings and amend their Group 1 coverage to have RIVERS of geometric primitive area covered
               by LNDARE (and merge with adjoining LNDARE as appropriate). */

            /* S-57 guidance recommends the encoding of intermittent lakes using an instance of the S-57 Object
               class RIVERS. Data Producers are advised to check all instances of RIVERS of geometric primitive
               area having attribute STATUS = 5 (periodic/intermittent) and if the real-world feature is a lake to
               amend to an instance of the S-101 Feature _s101type Lake (see S-101 DCEG clause 5.10). */

            var instance = new River();

            var featureName = GetFeatureName(current.OBJNAM(), current.NOBJNM());
            if (featureName is not null)
                instance.featureName = featureName;

            if (current.STATUS_HasValue()) {
                var status = EnumHelper.GetEnumValues(current.STATUS(), instance.attributeBindingDefinition("status")!.permitedValues!);
                if (status is not null && status.Any())
                    instance.status = status[0];
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

