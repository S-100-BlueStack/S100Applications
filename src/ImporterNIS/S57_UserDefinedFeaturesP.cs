using ArcGIS.Core.Data;
using S100FC;
using S100FC.S101.ComplexAttributes;
using S100FC.S101.FeatureTypes;
using S100FC.S101.SimpleAttributes;
using S100Framework.Applications.S57.esri;
using S100Framework.Applications.Singletons;
using System.Globalization;

namespace S100Framework.Applications
{
    internal static partial class ImporterNIS
    {
        private static void S57_UserDefinedFeaturesP(Geodatabase source, Geodatabase target, QueryFilter filter) {
            var tableName = "UserDefinedFeaturesP";

            using var metadataa = source.OpenDataset<FeatureClass>(source.GetName("MetaDataA"));

            using var aidstonavigationp = source.OpenDataset<FeatureClass>(source.GetName(tableName));

            using var featureClass = target.OpenDataset<FeatureClass>(target.GetName("point"));

            using var buffer = featureClass.CreateRowBuffer();


            var orgWhereClause = filter.WhereClause.Clone().ToString();

            using var cursor = aidstonavigationp.Search(filter, true);

            int recordCount = 0;
            var _slaves = new Dictionary<Guid, string>();

            while (cursor.MoveNext()) {
                recordCount += 1;
                var feature = (Feature)cursor.Current;
                var current = new UserDefinedFeaturesP(feature);

                var objectid = current.OBJECTID ?? default;
                var globalid = current.GLOBALID;
                var fcSubtype = current.FCSUBTYPE ?? default;

                if (ConversionAnalytics.Instance.IsConverted(globalid)) {
                    continue;
                }

                var plts_comp_scale = current.PLTS_COMP_SCALE ?? default;
                var longname = current.LNAM ?? Strings.UNKNOWN;

                if (FeatureRelations.Instance.IsSlave(globalid)) {
                    Subtypes.Instance.TryGetSubtype(tableName, current.FCSUBTYPE!.Value, out var subtype);
                    _slaves.Add(globalid, $"{tableName}::{globalid}::{subtype}");
                    continue;
                }

                var inform = Convert.ToString(feature["INFORM"])?.Split(',');

                if (inform is not null && inform.Any()) {
                    switch (inform[0].ToLowerInvariant()) {
                        case "virtual aton": {
                                var instance = new VirtualAISAidToNavigation();

                                #region aidstonavigation

                                var featureName = GetFeatureName(current.OBJNAM, current.NOBJNM);
                                if (featureName is not null)
                                    instance.featureName = featureName;

                                DateHelper.TryGetFixedDateRange(current.DATSTA, current.DATEND, out var dateRange);
                                if (dateRange != default) {
                                    instance.fixedDateRange = dateRange;
                                }

                                DateHelper.TryGetPeriodicDateRange(current.PERSTA, current.PEREND, out var periodicDateRange);
                                if (periodicDateRange != default) {
                                    instance.periodicDateRange = periodicDateRange;
                                }

                                if (current.STATUS != default) {
                                    instance.status = GetStatus(current.STATUS)?.First();
                                }

                                var clsdef = Convert.ToString(current.CLSDEF)?.Trim()?.ToLowerInvariant();
                                var clsnam = Convert.ToString(current.CLSNAM)?.Trim()?.ToLowerInvariant();

                                int virtualAtonPortLateral = 5;
                                int virtualAtonStarboardLateral = 6;

                                using (var search = metadataa.Search(new SpatialQueryFilter {
                                    WhereClause = "FCSubType = 35",  //  M_NSYS
                                    SpatialRelationship = SpatialRelationship.Within,
                                    FilterGeometry = current.SHAPE,
                                }, true)) {
                                    search.MoveNext();

                                    var marsys = Convert.ToInt32(search.Current["marsys"]);
                                    if (marsys == 1) {
                                        virtualAtonPortLateral = 5;
                                        virtualAtonStarboardLateral = 6;
                                    }
                                    else if (marsys == 2) {
                                        virtualAtonPortLateral = 7;
                                        virtualAtonStarboardLateral = 8;
                                    }
                                    else
                                        System.Diagnostics.Debugger.Break();
                                }

                                int? virtualAISAidToNavigationType = (clsdef, clsnam) switch {
                                    ("a virtual object which indicates navigable water lies northwards", "virtual aton, north cardinal") => 1,
                                    ("a virtual object which indicates navigable water lies eastwards", "virtual aton, east cardinal") => 2,
                                    ("a virtual object which indicates navigable water lies southwards", "virtual aton, south cardinal") => 3,
                                    ("a virtual object which indicates navigable water lies westhwards", "virtual aton, west cardinal") => 4,
                                    ("a virtual object marking the port side of a channel", "virtual aton, port lateral") => virtualAtonPortLateral,
                                    ("a virtual object marking the starboard side of a channel", "virtual aton, starboard lateral") => virtualAtonStarboardLateral,
                                    ("a virtual object marking an isolated danger", "virtual aton, isolated danger") => 9,
                                    ("a virtual object marking safe water", "virtual aton, safe water") => 10,
                                    ("a virtual object used to mark an area or feature referred to in nautical documents", "virtual aton, special purpose") => 11,
                                    ("a virtual object marking a wreck", "virtual aton, wreck marking") => 12,
                                    _ => null,
                                };
                                if (virtualAISAidToNavigationType.HasValue)
                                    instance.virtualAISAidToNavigationType = virtualAISAidToNavigationType;

                                if (current.PLTS_COMP_SCALE.HasValue && current.SHAPE != null) {
                                    string subtype = "";

                                    if (current.TableName != default && current.FCSUBTYPE.HasValue && !Subtypes.Instance.TryGetSubtype(current.TableName, current.FCSUBTYPE.Value, out subtype))
                                        throw new NotSupportedException($"Unknown subtype for {current.TableName}, {current.FCSUBTYPE.Value}");

                                    var scamin = Scamin.Instance.GetMinimumScale(current, subtype, current.PLTS_COMP_SCALE!.Value, isRelatedToStructure: false);
                                    if (scamin.HasValue)
                                        instance.scaleMinimum = scamin.Value;
                                }

                                var result = ImporterNIS.AddInformation(current.OBJECTID!.Value, current.TableName!, current.NTXTDS, current.TXTDSC, current.INFORM, current.NINFOM);
                                instance.information = result.information.ToArray();
                                instance.SetInformationBindings(result.InformationBindings.ToArray());

                                buffer["ps"] = ps101;
                                buffer["code"] = instance.GetType().Name;

                                buffer["attributebindings"] = instance.Flatten();
                                buffer["informationbindings"] = System.Text.Json.JsonSerializer.Serialize(instance.GetInformationBindings(), ImporterNIS.jsonSerializerOptions);

                                SetShape(buffer, current.SHAPE); buffer["sourceIdentifier"] = instance.sourceIdentifier;
                                SetCompilationScale(buffer, current.PLTS_COMP_SCALE!.Value);

                                using var featureN = featureClass.CreateRow(buffer);
                                var name = featureN.UID();


                                ConversionAnalytics.Instance.AddConverted(tableName, current.GlobalId, name);

                                Logger.Current.DataObject((int)featureN.GetObjectID(), tableName, name, System.Text.Json.JsonSerializer.Serialize(instance, ImporterNIS.jsonSerializerOptions));

                                #endregion aidstonavigation

                                if (FeatureRelations.Instance.HasSlaves(current.GLOBALID)) {
                                    relatedEquipment?.CreateRelatedPointEquipment(current, instance, featureN, instance.scaleMinimum);
                                }
                            }
                            break;

                        default:
                            throw new Exception($"Missing subtype in S57_AidsToNavigation: {fcSubtype}");
                    }
                }
            }

            Logger.Current.DataTotalCount(tableName, recordCount, ConversionAnalytics.Instance.GetConvertedCount(tableName));

            var _nonConvertedSlaves = new List<Guid>();

            foreach (var slaveId in _slaves.Keys) {
                if (!ConversionAnalytics.Instance.IsConverted(slaveId)) {
                    _nonConvertedSlaves.Add(slaveId);
                    Logger.Current.DataError(-1, tableName, "These slaves are missing the structure and therefore not converted.", $"{_slaves[slaveId]}");
                }
            }
        }
    }
}

