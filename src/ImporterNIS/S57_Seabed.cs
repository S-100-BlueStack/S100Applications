using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using S100FC;
using S100FC.S101.FeatureTypes;
using S100Framework.Applications.S57.esri;
using S100Framework.Applications.Singletons;
using surfaceCharacteristics = S100FC.S101.ComplexAttributes.surfaceCharacteristics;

namespace S100Framework.Applications
{
    internal static partial class ImporterNIS
    {
        private static void S57_Seabed(string tableName, Func<string, FeatureClass> source, QueryFilter filter, Func<FeatureClass> target, Action<RowBuffer, Geometry> setShape) {
            var ps101 = "S-101";

            using var seabed = source(tableName);   // source.OpenDataset<FeatureClass>(source.GetName(tableName));
            Subtypes.Instance.RegisterSubtypes(seabed);

            using var featureClass = target();    // target.OpenDataset<FeatureClass>(target.GetName("point"));

            using var buffer = featureClass.CreateRowBuffer();

            using var cursor = seabed.Search(filter, true);
            int recordCount = 0;

            while (cursor.MoveNext()) {
                recordCount += 1;

                var feature = (Feature)cursor.Current;

                var current = new Seabed(feature);

                var objectid = current.OBJECTID ?? default;
                var globalid = current.GLOBALID;

                if (FeatureRelations.Instance.IsSlave(globalid)) {
                    continue;
                }

                if (ConversionAnalytics.Instance.IsConverted(globalid)) {
                    throw new Exception("Ups. Not supported");
                }

                var fcSubtype = current.FCSUBTYPE ?? default;
                var watlev = current.WATLEV ?? default;
                var plts_comp_scale = current.PLTS_COMP_SCALE ?? default;
                var longname = current.LNAM ?? Strings.UNKNOWN;

                switch (fcSubtype) {
                    case 15: { // SBDARE_SeabedArea                            
                            var instance = new SeabedArea {                                
                            };

                            var featureName = GetFeatureName(current.OBJNAM, current.NOBJNM);
                            if (featureName is not null)
                                instance.featureName = featureName;

                            //if (current.OBJECTID == 6399) System.Diagnostics.Debugger.Break();

                            // TODO: interoperabilityIdentifier

                            // TODO: Verify this against action point 48

                            //surfaceCharacteristics surfaceChars = new();

                            surfaceCharacteristics[] surfaceCharacteristics = [];

                            var natsur = string.IsNullOrEmpty(current.NATSUR?.Trim()) ? [] : current.NATSUR.Split(",", StringSplitOptions.RemoveEmptyEntries);
                            var natqua = string.IsNullOrEmpty(current.NATQUA?.Trim()) ? [] : current.NATQUA.Split(",");

                            int[] _ = [natsur.Length, natqua.Length];

                            for (var i = 0; i < _.Max(); i++) {
                                var s = new surfaceCharacteristics();

                                if (i < natsur.Length)
                                    s.natureOfSurface = EnumHelper.GetEnumValue(natsur[i]);
                                if (i < natqua.Length) {
                                    if(!string.IsNullOrEmpty(natqua[i]))
                                    s.natureOfSurfaceQualifyingTerms = [EnumHelper.GetEnumValue(natqua[i])];
                                }

                                surfaceCharacteristics = [.. surfaceCharacteristics, s];
                            }
                            
                            //if (!surfaceCharacteristics.Any()) System.Diagnostics.Debugger.Break();

                            instance.surfaceCharacteristics = surfaceCharacteristics;                            

                            if (current.WATLEV.HasValue) {
                                instance.waterLevelEffect = EnumHelper.GetEnumValue(current.WATLEV);
                            }

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
                            buffer["code"] = instance.GetType().Name; buffer["sourceIdentifier"] = instance.sourceIdentifier;


                            buffer["attributebindings"] = instance.Flatten();
                            buffer["informationbindings"] = System.Text.Json.JsonSerializer.Serialize(instance.GetInformationBindings(), jsonSerializerOptions);
                            setShape(buffer, current.SHAPE!);
                            ImporterNIS.SetUsageBand(buffer, current.PLTS_COMP_SCALE!.Value);

                            var featureN = featureClass.CreateRow(buffer);
                            var name = featureN.UID();

                            if (FeatureRelations.Instance.HasSlaves(current.GLOBALID)) {
                                relatedEquipment!.CreateRelatedPointEquipment(current, instance, featureN, instance.scaleMinimum);
                            }

                            ConversionAnalytics.Instance.AddConverted(tableName, current.GLOBALID, name);

                            Logger.Current.DataObject(objectid, tableName, longname, System.Text.Json.JsonSerializer.Serialize(instance, ImporterNIS.jsonSerializerOptions));

                            JsonUnflattener.Unflatten(Convert.ToString(buffer["attributebindings"])!);
                        }
                        break;
                    case 25: { // SNDWAV_SandWaves
                            var instance = new Sandwave();


                            // TODO: interoperabilityIdentifier

                            if (current.VERLEN.HasValue) {
                                instance.verticalLength = current.VERLEN.Value != -32767m ? current.VERLEN.Value : null;
                            }
                            else if (current.VERLEN.HasValue && current.VERLEN.Value == -32767m) {
                                //instance.verticalLength = default(decimal?);
                            }

                            if (current.PLTS_COMP_SCALE.HasValue && current.SHAPE != null) {
                                string subtype = "";
                                if (current.TableName != default && current.FCSUBTYPE.HasValue && !Subtypes.Instance.TryGetSubtype(current.TableName, current.FCSUBTYPE.Value, out subtype))
                                    throw new NotSupportedException($"Unknown subtype for {current.TableName}, {current.FCSUBTYPE.Value}");
                                instance.scaleMinimum = Scamin.Instance.GetMinimumScale(current, subtype, current.PLTS_COMP_SCALE.Value, isRelatedToStructure: false);
                            }

                            var result = ImporterNIS.AddInformation(current.OBJECTID!.Value, current.TableName!, current.NTXTDS, current.TXTDSC, current.INFORM, current.NINFOM);
                            instance.information = result.information.ToArray();
                            instance.SetInformationBindings(result.InformationBindings.ToArray());

                            buffer["ps"] = ps101;
                            buffer["code"] = instance.GetType().Name; buffer["sourceIdentifier"] = instance.sourceIdentifier;


                            buffer["attributebindings"] = instance.Flatten();
                            buffer["informationbindings"] = System.Text.Json.JsonSerializer.Serialize(instance.GetInformationBindings(), jsonSerializerOptions);
                            setShape(buffer, current.SHAPE!);
                            ImporterNIS.SetUsageBand(buffer, current.PLTS_COMP_SCALE!.Value);

                            var featureN = featureClass.CreateRow(buffer);
                            var name = featureN.UID();

                            if (FeatureRelations.Instance.HasSlaves(current.GLOBALID)) {
                                relatedEquipment!.CreateRelatedAreaEquipment(current, instance, featureN, instance.scaleMinimum);
                            }

                            ConversionAnalytics.Instance.AddConverted(tableName, current.GLOBALID, name);

                            Logger.Current.DataObject(objectid, tableName, longname, System.Text.Json.JsonSerializer.Serialize(instance, ImporterNIS.jsonSerializerOptions));
                            JsonUnflattener.Unflatten(Convert.ToString(buffer["attributebindings"])!);
                        }
                        break;

                    case 30: { // SNDWAV_SandWaves
                            var instance = new Sandwave();

                            // TODO: interoperabilityIdentifier

                            if (current.VERLEN.HasValue) {
                                instance.verticalLength = current.VERLEN.Value != -32767m ? current.VERLEN.Value : null;
                            }
                            else if (current.VERLEN.HasValue && current.VERLEN.Value == -32767m) {
                                //instance.verticalLength = default(decimal?);
                            }

                            if (current.PLTS_COMP_SCALE.HasValue && current.SHAPE != null) {
                                string subtype = "";
                                if (current.TableName != default && current.FCSUBTYPE.HasValue && !Subtypes.Instance.TryGetSubtype(current.TableName, current.FCSUBTYPE.Value, out subtype))
                                    throw new NotSupportedException($"Unknown subtype for {current.TableName}, {current.FCSUBTYPE.Value}");
                                instance.scaleMinimum = Scamin.Instance.GetMinimumScale(current, subtype, current.PLTS_COMP_SCALE.Value, isRelatedToStructure: false);
                            }

                            var result = ImporterNIS.AddInformation(current.OBJECTID!.Value, current.TableName!, current.NTXTDS, current.TXTDSC, current.INFORM, current.NINFOM);
                            instance.information = result.information.ToArray();
                            instance.SetInformationBindings(result.InformationBindings.ToArray());

                            buffer["ps"] = ps101;
                            buffer["code"] = instance.GetType().Name; buffer["sourceIdentifier"] = instance.sourceIdentifier;


                            buffer["attributebindings"] = instance.Flatten();
                            buffer["informationbindings"] = System.Text.Json.JsonSerializer.Serialize(instance.GetInformationBindings(), jsonSerializerOptions);
                            setShape(buffer, current.SHAPE!);
                            ImporterNIS.SetUsageBand(buffer, current.PLTS_COMP_SCALE!.Value);

                            var featureN = featureClass.CreateRow(buffer);
                            var name = featureN.UID();

                            if (FeatureRelations.Instance.HasSlaves(current.GLOBALID)) {
                                relatedEquipment!.CreateRelatedAreaEquipment(current, instance, featureN, instance.scaleMinimum);
                            }

                            ConversionAnalytics.Instance.AddConverted(tableName, current.GLOBALID, name);

                            Logger.Current.DataObject(objectid, tableName, longname, System.Text.Json.JsonSerializer.Serialize(instance, ImporterNIS.jsonSerializerOptions));
                        }
                        break;

                    case 35: { // WEDKLP_WeedKelp

                            if (current.CATWED.HasValue && current.CATWED.Value == 3) {
                                var seagrass = new Seagrass {                                    
                                };

                                var featureName = GetFeatureName(current.OBJNAM, current.NOBJNM);
                                if (featureName is not null)
                                    seagrass.featureName = featureName;

                                // TODO: interoperabilityIdentifier

                                if (current.PLTS_COMP_SCALE.HasValue && current.SHAPE != null) {
                                    string subtype = "";

                                    if (current.TableName != default && current.FCSUBTYPE.HasValue && !Subtypes.Instance.TryGetSubtype(current.TableName, current.FCSUBTYPE.Value, out subtype))
                                        throw new NotSupportedException($"Unknown subtype for {current.TableName}, {current.FCSUBTYPE.Value}");

                                    seagrass.scaleMinimum = Scamin.Instance.GetMinimumScale(current, subtype, current.PLTS_COMP_SCALE!.Value, isRelatedToStructure: false);
                                }

                                var result = ImporterNIS.AddInformation(current.OBJECTID!.Value, current.TableName!, current.NTXTDS, current.TXTDSC, current.INFORM, current.NINFOM);
                                seagrass.information = result.information.ToArray();
                                seagrass.SetInformationBindings(result.InformationBindings.ToArray());

                                buffer["ps"] = ps101;
                                buffer["code"] = seagrass.GetType().Name;

                                buffer["attributebindings"] = seagrass.Flatten();
                                buffer["informationbindings"] = System.Text.Json.JsonSerializer.Serialize(seagrass.GetInformationBindings(), ImporterNIS.jsonSerializerOptions);

                                setShape(buffer, current.SHAPE!);
                                SetUsageBand(buffer, current.PLTS_COMP_SCALE!.Value);

                                var featureN = featureClass.CreateRow(buffer);
                                var name = featureN.UID();

                                if (FeatureRelations.Instance.HasSlaves(current.GLOBALID)) {
                                    relatedEquipment?.CreateRelatedPointEquipment(current, seagrass, featureN, seagrass.scaleMinimum);
                                }

                                ConversionAnalytics.Instance.AddConverted(tableName, current.GLOBALID, name);

                                Logger.Current.DataObject(objectid, tableName, longname, System.Text.Json.JsonSerializer.Serialize(seagrass, ImporterNIS.jsonSerializerOptions));
                                JsonUnflattener.Unflatten(Convert.ToString(buffer["attributebindings"])!);
                            }
                            else {

                                var instance = new WeedKelp();
                                if (current.CATWED.HasValue) {
                                    instance.categoryOfWeedKelp = EnumHelper.GetEnumValue(current.CATWED.Value);
                                }

                                var featureName = GetFeatureName(current.OBJNAM, current.NOBJNM);
                                if (featureName is not null)
                                    instance.featureName = featureName;

                                // TODO: interoperabilityIdentifier

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
                                buffer["code"] = instance.GetType().Name; buffer["sourceIdentifier"] = instance.sourceIdentifier;


                                buffer["attributebindings"] = instance.Flatten();
                                buffer["informationbindings"] = System.Text.Json.JsonSerializer.Serialize(instance.GetInformationBindings(), jsonSerializerOptions);

                                setShape(buffer, current.SHAPE!);
                                SetUsageBand(buffer, current.PLTS_COMP_SCALE!.Value);

                                var featureN = featureClass.CreateRow(buffer);
                                var name = featureN.UID();

                                if (FeatureRelations.Instance.HasSlaves(current.GLOBALID)) {
                                    relatedEquipment!.CreateRelatedPointEquipment(current, instance, featureN, instance.scaleMinimum);
                                }

                                ConversionAnalytics.Instance.AddConverted(tableName, current.GLOBALID, name);

                                Logger.Current.DataObject(objectid, tableName, longname, System.Text.Json.JsonSerializer.Serialize(instance, ImporterNIS.jsonSerializerOptions));
                                JsonUnflattener.Unflatten(Convert.ToString(buffer["attributebindings"])!);
                            }
                        }
                        break;

                    case 40: { // WEDKLP_WeedKelp

                            if (current.CATWED.HasValue && current.CATWED.Value == 3) {
                                var seagrass = new Seagrass();

                                var featureName = GetFeatureName(current.OBJNAM, current.NOBJNM);
                                if (featureName is not null)
                                    seagrass.featureName = featureName;

                                // TODO: interoperabilityIdentifier

                                if (current.PLTS_COMP_SCALE.HasValue && current.SHAPE != null) {
                                    string subtype = "";

                                    if (current.TableName != default && current.FCSUBTYPE.HasValue && !Subtypes.Instance.TryGetSubtype(current.TableName, current.FCSUBTYPE.Value, out subtype))
                                        throw new NotSupportedException($"Unknown subtype for {current.TableName}, {current.FCSUBTYPE.Value}");

                                    seagrass.scaleMinimum = Scamin.Instance.GetMinimumScale(current, subtype, current.PLTS_COMP_SCALE!.Value, isRelatedToStructure: false);
                                }

                                var result = ImporterNIS.AddInformation(current.OBJECTID!.Value, current.TableName!, current.NTXTDS, current.TXTDSC, current.INFORM, current.NINFOM);
                                seagrass.information = result.information.ToArray();
                                seagrass.SetInformationBindings(result.InformationBindings.ToArray());

                                buffer["ps"] = ps101;
                                buffer["code"] = seagrass.GetType().Name;

                                buffer["attributebindings"] = seagrass.Flatten();
                                buffer["informationbindings"] = System.Text.Json.JsonSerializer.Serialize(seagrass.GetInformationBindings(), ImporterNIS.jsonSerializerOptions);

                                setShape(buffer, current.SHAPE!);
                                SetUsageBand(buffer, current.PLTS_COMP_SCALE!.Value);

                                var featureN = featureClass.CreateRow(buffer);
                                var name = featureN.UID();

                                if (FeatureRelations.Instance.HasSlaves(current.GLOBALID)) {
                                    relatedEquipment!.CreateRelatedAreaEquipment(current, seagrass, featureN, seagrass.scaleMinimum);
                                }

                                ConversionAnalytics.Instance.AddConverted(tableName, current.GLOBALID, name);

                                Logger.Current.DataObject(objectid, tableName, longname, System.Text.Json.JsonSerializer.Serialize(seagrass, ImporterNIS.jsonSerializerOptions));

                            }
                            else {
                                var instance = new WeedKelp();

                                if (current.CATWED.HasValue) {
                                    instance.categoryOfWeedKelp = EnumHelper.GetEnumValue(current.CATWED.Value);
                                }

                                var featureName = GetFeatureName(current.OBJNAM, current.NOBJNM);
                                if (featureName is not null)
                                    instance.featureName = featureName;

                                // TODO: interoperabilityIdentifier

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
                                buffer["code"] = instance.GetType().Name; buffer["sourceIdentifier"] = instance.sourceIdentifier;


                                buffer["attributebindings"] = instance.Flatten();
                                buffer["informationbindings"] = System.Text.Json.JsonSerializer.Serialize(instance.GetInformationBindings(), jsonSerializerOptions);

                                setShape(buffer, current.SHAPE!);
                                SetUsageBand(buffer, current.PLTS_COMP_SCALE!.Value);

                                var featureN = featureClass.CreateRow(buffer);
                                var name = featureN.UID();

                                if (FeatureRelations.Instance.HasSlaves(current.GLOBALID)) {
                                    relatedEquipment!.CreateRelatedAreaEquipment(current, instance, featureN, instance.scaleMinimum);
                                }

                                ConversionAnalytics.Instance.AddConverted(tableName, current.GLOBALID, name);

                                Logger.Current.DataObject(objectid, tableName, longname, System.Text.Json.JsonSerializer.Serialize(instance, ImporterNIS.jsonSerializerOptions));

                            }
                        }
                        break;

                    default:
                        // code block
                        System.Diagnostics.Debugger.Break();
                        break;
                }
            }
            Logger.Current.DataTotalCount(tableName, recordCount, ConversionAnalytics.Instance.GetConvertedCount(tableName));
        }


    }
}
