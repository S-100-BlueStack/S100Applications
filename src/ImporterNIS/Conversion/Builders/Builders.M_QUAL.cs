using ArcGIS.Core.Data;
using S100FC;
using S100FC.S101.ComplexAttributes;
using S100FC.S101.FeatureTypes;
using S100Framework.Applications.Singletons;

namespace S100Framework.Applications
{
    using S100FC.S101.InformationTypes;
    using S100Framework.Applications.S57.esri;

    internal static partial class ImporterNIS
    {

        private static QualityOfBathymetricData M_QUAL(Feature current, QueryFilter filter, Geodatabase target) {
            var instance = new QualityOfBathymetricData();

            /*
                Temporal Variation: The S-101 mandatory attribute category of temporal variation introduces the
                ability for the Data Producer to incorporate the temporal impact on bathymetric data quality in areas
                where the seabed is likely to change over time, or in the wake of an extreme event such as a hurricane
                S-57 ENC to S-101 Conversion Guidance 9
                S-65 Annex B April 2024 Edition 1.2.0
                or tsunami. During the automated conversion process, for all M_QUAL except those where CATZOC =
                6 (zone of confidence U (data not assessed)), the corresponding Quality of Bathymetric Data will
                have category of temporal variation populated with value 5 (unlikely to change). For full S-101
                functionality, Data Producers will be required to reassess the value of this attribute as required. For
                CATZOC = 6 (zone of confidence U (data not assessed)), category of temporal variation will be
                populated with value 6 (unassessed).
            */

            if (current.DRVAL1_HasValue()) {
                instance.depthRangeMinimumValue = current.DRVAL1() != -32767m ? current.DRVAL1() : null;
            }

            if (current.DRVAL2_HasValue()) {
                instance.depthRangeMaximumValue = current.DRVAL2() != -32767m ? current.DRVAL2() : null;
            }

            // TODO: featuresDetected (ed2.1.0)

            //Code Description
            //1   zone of confidence A1
            //2   zone of confidence A2
            //3   zone of confidence B
            //4   zone of confidence C
            //5   zone of confidence D
            //6   zone of confidence U(data not assessed)

            // During the automated conversion process, for all M_QUAL
            // except those where CATZOC = 6 (zone of confidence U(data not assessed)),
            // the corresponding Quality of Bathymetric Data will
            // have category of temporal variation populated with value 5(unlikely to change).

            /* S-65 Annex B p.8
                Data Assessment: The S-101 mandatory attribute data assessment introduces an option to reduce
                screen clutter in some ECDIS display modes through population of value 2 (assessed (oceanic)). This
                value is intended for use where an indication of the overall data quality is not considered to be required
                – generally in depths deeper the 200 metres. However, determination as to when this value may be
                populated cannot be made during the automated conversion process, therefore for all M_QUAL except
                those where CATZOC = 6 (zone of confidence U (data not assessed)), the corresponding Quality of
                Bathymetric Data will have data assessment populated with value 1 (assessed).
             */


            SpatialQuality? spatialQuality = default;

            if (current.CATZOC_HasValue()) { // A1
                int catzoc = current.CATZOC()!.Value;

                if (catzoc == 1) {
                    instance.categoryOfTemporalVariation = 5;   // categoryOfTemporalVariation.UnlikelyToChange;
                    instance.dataAssessment = 1;    // dataAssessment.Assessed;
                    instance.featuresDetected = new featuresDetected() {
                        significantFeaturesDetected = true,
                        leastDepthOfDetectedFeaturesMeasured = true,
                    };
                    instance.fullSeafloorCoverageAchieved = true;
                    instance.zoneOfConfidence = [new zoneOfConfidence() {
                                        categoryOfZoneOfConfidenceInData = 1,   //categoryOfZoneOfConfidenceInData.ZoneOfConfidenceA1
                                    }];

                    spatialQuality = new SpatialQuality {
                        spatialAccuracy = [new spatialAccuracy {
                                           horizontalPositionUncertainty = new horizontalPositionUncertainty{
                                               uncertaintyFixed = 5m,
                                               uncertaintyVariableFactor = 0.05m,   // 5% of depth
                                           },
                                           verticalUncertainty = new verticalUncertainty{
                                               uncertaintyFixed = 0.5m,
                                               uncertaintyVariableFactor = 0.01m,
                                           },
                                        }],
                    };
                }
                else if (catzoc == 2) { // A2
                    instance.categoryOfTemporalVariation = 5;   // categoryOfTemporalVariation.UnlikelyToChange;
                    instance.dataAssessment = 1;    // dataAssessment.Assessed;
                    instance.featuresDetected = new featuresDetected() {
                        significantFeaturesDetected = true,
                        leastDepthOfDetectedFeaturesMeasured = true,

                    };
                    instance.fullSeafloorCoverageAchieved = true;
                    instance.zoneOfConfidence = [new zoneOfConfidence() {
                                        categoryOfZoneOfConfidenceInData = 2,   //categoryOfZoneOfConfidenceInData.ZoneOfConfidenceA2,
                                    }];

                    spatialQuality = new SpatialQuality {
                        spatialAccuracy = [new spatialAccuracy {
                                           horizontalPositionUncertainty = new horizontalPositionUncertainty{
                                               uncertaintyFixed = 20m,
                                           },
                                           verticalUncertainty = new verticalUncertainty{
                                               uncertaintyFixed = 1m,
                                               uncertaintyVariableFactor = 0.02m,
                                           },
                                        }],
                    };
                }
                else if (catzoc == 3) { // B
                    instance.categoryOfTemporalVariation = 5;   // categoryOfTemporalVariation.UnlikelyToChange;
                    instance.dataAssessment = 1;    // dataAssessment.Assessed;
                    instance.featuresDetected = new featuresDetected() {
                        significantFeaturesDetected = false,
                        leastDepthOfDetectedFeaturesMeasured = false,
                    };
                    instance.fullSeafloorCoverageAchieved = false;
                    instance.zoneOfConfidence = [new zoneOfConfidence() {
                                        categoryOfZoneOfConfidenceInData = 3,   //categoryOfZoneOfConfidenceInData.ZoneOfConfidenceB,                                        
                                    }];

                    spatialQuality = new SpatialQuality {
                        spatialAccuracy = [new spatialAccuracy {
                                           horizontalPositionUncertainty = new horizontalPositionUncertainty{
                                               uncertaintyFixed = 50m,
                                           },
                                           verticalUncertainty = new verticalUncertainty{
                                               uncertaintyFixed = 1m,
                                               uncertaintyVariableFactor = 0.02m,
                                           },
                                        }],
                    };
                }
                else if (catzoc == 4) { // C
                    instance.categoryOfTemporalVariation = 5;   // categoryOfTemporalVariation.UnlikelyToChange;
                    instance.dataAssessment = 1;    // dataAssessment.Assessed;
                    instance.featuresDetected = new featuresDetected() {
                        significantFeaturesDetected = false,
                        leastDepthOfDetectedFeaturesMeasured = false,
                    };
                    instance.fullSeafloorCoverageAchieved = false;
                    instance.zoneOfConfidence = [new zoneOfConfidence() {
                                        categoryOfZoneOfConfidenceInData = 4,   //categoryOfZoneOfConfidenceInData.ZoneOfConfidenceC,                                        
                                    }];

                    spatialQuality = new SpatialQuality {
                        spatialAccuracy = [new spatialAccuracy {
                                           horizontalPositionUncertainty = new horizontalPositionUncertainty{
                                               uncertaintyFixed = 500m,
                                           },
                                           verticalUncertainty = new verticalUncertainty{
                                               uncertaintyFixed = 2m,
                                               uncertaintyVariableFactor = 0.05m,
                                           },
                                        }],
                    };
                }
                else if (catzoc == 5) { // D
                    instance.categoryOfTemporalVariation = 5;   // categoryOfTemporalVariation.UnlikelyToChange;
                    instance.dataAssessment = 1;    // dataAssessment.Assessed;
                    instance.featuresDetected = new featuresDetected() {
                        significantFeaturesDetected = false,
                        leastDepthOfDetectedFeaturesMeasured = false,

                    };
                    instance.fullSeafloorCoverageAchieved = false;
                    instance.zoneOfConfidence = [new zoneOfConfidence() {
                                        categoryOfZoneOfConfidenceInData = 5,   //categoryOfZoneOfConfidenceInData.ZoneOfConfidenceD,                                        
                                    }];

                    spatialQuality = new SpatialQuality {
                        spatialAccuracy = [new spatialAccuracy {
                                           horizontalPositionUncertainty = new horizontalPositionUncertainty{
                                               uncertaintyFixed = null,
                                           },
                                           verticalUncertainty = new verticalUncertainty{
                                               uncertaintyFixed = null,
                                           },
                                        }],
                    };
                }
                else if (catzoc == 6) { // U
                    instance.categoryOfTemporalVariation = 5;   // categoryOfTemporalVariation.Unassessed;
                    instance.dataAssessment = 1;    // dataAssessment.Unassessed;
                    instance.featuresDetected = new featuresDetected() {
                        significantFeaturesDetected = false,
                        leastDepthOfDetectedFeaturesMeasured = false,

                    };
                    instance.fullSeafloorCoverageAchieved = false;
                    instance.zoneOfConfidence = [new zoneOfConfidence() {
                                        categoryOfZoneOfConfidenceInData = 6,   //categoryOfZoneOfConfidenceInData.ZoneOfConfidenceU,                                        
                                    }];

                    spatialQuality = new SpatialQuality {
                        spatialAccuracy = [new spatialAccuracy {
                                           horizontalPositionUncertainty = new horizontalPositionUncertainty{
                                               uncertaintyFixed = null,
                                           },
                                           verticalUncertainty = new verticalUncertainty{
                                               uncertaintyFixed = null,
                                           },
                                        }],
                    };
                }
                else {
                    throw new NotSupportedException($"Unknown catzoc {catzoc}. objectid: {current.GetObjectID()} - {current.TableName()}");
                }
            }

            if (DateHelper.TryGetSurveyDateRange(current.SURSTA(), current.SUREND(), out var dateRange)) {
                instance.surveyDateRange = dateRange;
            }

            if (DateHelper.TryGetSurveyDateRange(current.SURSTA(), current.SUREND(), out var surveyDateRange)) {
                instance.surveyDateRange = surveyDateRange;
            }

            if (ImporterNIS.ContainsBarthyFeatures_UnsurveyedArea(current.SHAPE()!, (Geodatabase)current.GetTable().GetDatastore(), filter.WhereClause)) {
                if (ImporterNIS.IsCoveredByUNSARE_UnsurveyedArea(current.SHAPE()!)) {
                    instance.categoryOfTemporalVariation = 6;   //  Unassessed
                    instance.zoneOfConfidence[0]!.categoryOfZoneOfConfidenceInData = 5;  //categoryOfZoneOfConfidenceInData.ZoneOfConfidenceD,                                        
                }
            }

            //var informationBindings = instance.GetInformationBindings();
            informationBinding[] informationBindings = [];

            if (spatialQuality is not null) {
                var informationBinding = target.CreateInformationType(spatialQuality);

                if (informationBindings is null)
                    informationBindings = [];
                informationBindings = [.. informationBindings, informationBinding];
            }
            var result = ImporterNIS.AddInformation(current.GetObjectID(), current.TableName(), current.NTXTDS(), current.TXTDSC(), current.INFORM(), current.NINFOM());
            instance.information = [.. result.information];
            instance.SetInformationBindings([.. result.InformationBindings, .. informationBindings]);

            return instance;
        }
    }
}

