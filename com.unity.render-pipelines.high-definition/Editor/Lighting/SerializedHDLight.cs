using UnityEditor.Rendering;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Experimental.Rendering.HDPipeline;
using UnityEngine;

namespace UnityEditor.Experimental.Rendering.HDPipeline
{
    using LightShape = HDLightUI.LightShape;
    internal class SerializedHDLight
    {
        public sealed class SerializedLightData
        {
            public SerializedProperty intensity;
            public SerializedProperty enableSpotReflector;
            public SerializedProperty luxAtDistance;
            public SerializedProperty spotInnerPercent;
            public SerializedProperty lightDimmer;
            public SerializedProperty fadeDistance;
            public SerializedProperty affectDiffuse;
            public SerializedProperty affectSpecular;
            public SerializedProperty nonLightmappedOnly;
            public SerializedProperty lightTypeExtent;
            public SerializedProperty spotLightShape;
            public SerializedProperty shapeWidth;
            public SerializedProperty shapeHeight;
            public SerializedProperty aspectRatio;
            public SerializedProperty shapeRadius;
            public SerializedProperty maxSmoothness;
            public SerializedProperty applyRangeAttenuation;
            public SerializedProperty volumetricDimmer;
            public SerializedProperty lightUnit;
            public SerializedProperty displayAreaLightEmissiveMesh;
            public SerializedProperty renderingLayerMask;
            public SerializedProperty shadowNearPlane;
            public SerializedProperty shadowSoftness;
            public SerializedProperty blockerSampleCount;
            public SerializedProperty filterSampleCount;
            public SerializedProperty minFilterSize;
            public SerializedProperty areaLightCookie;   // We can't use default light cookies because the cookie gets reset by some safety measure on C++ side... :/
            public SerializedProperty areaLightShadowCone;
#if ENABLE_RAYTRACING
            public SerializedProperty useRayTracedShadows;
            public SerializedProperty numRayTracingSamples;
            public SerializedProperty filterTracedShadow;
            public SerializedProperty filterSizeTraced;
#endif
            public SerializedProperty evsmExponent;
            public SerializedProperty evsmLightLeakBias;
            public SerializedProperty evsmVarianceBias;
            public SerializedProperty evsmBlurPasses;

            // Improved moment shadows data
            public SerializedProperty lightAngle;
            public SerializedProperty kernelSize;
            public SerializedProperty maxDepthBias;

            // Editor stuff
            public SerializedProperty useOldInspector;
            public SerializedProperty showFeatures;
            public SerializedProperty showAdditionalSettings;
            public SerializedProperty useVolumetric;

            // Layers
            public SerializedProperty linkLightLayers;
            public SerializedProperty lightlayersMask;
        }

        public sealed class SerializedShadowData
        {
            public SerializedProperty shadowDimmer;
            public SerializedProperty volumetricShadowDimmer;
            public SerializedProperty fadeDistance;
            public SerializedProperty resolution;
            public SerializedProperty contactShadows;
            public SerializedProperty shadowTint;            

            // Bias control
            public SerializedProperty viewBiasMin;
            public SerializedProperty viewBiasMax;
            public SerializedProperty viewBiasScale;
            public SerializedProperty normalBiasMin;
            public SerializedProperty normalBiasMax;
            public SerializedProperty normalBiasScale;
            public SerializedProperty sampleBiasScale;
            public SerializedProperty edgeLeakFixup;
            public SerializedProperty edgeToleranceNormal;
            public SerializedProperty edgeTolerance;
        }

        public bool needUpdateAreaLightEmissiveMeshComponents = false;

        public SerializedObject serializedLightDatas;
        public SerializedObject serializedShadowDatas;

        public SerializedLightData serializedLightData;
        public SerializedShadowData serializedShadowData;

        //contain serialized property that are mainly used to draw inspector
        public LightEditor.Settings settings;

        // Used for UI only; the processing code must use LightTypeExtent and LightType
        public LightShape editorLightShape;

        public SerializedHDLight(HDAdditionalLightData[] lightDatas, AdditionalShadowData[] shadowDatas, LightEditor.Settings settings)
        {
            serializedLightDatas = new SerializedObject(lightDatas);
            serializedShadowDatas = new SerializedObject(shadowDatas);
            this.settings = settings;

            using (var o = new PropertyFetcher<HDAdditionalLightData>(serializedLightDatas))
                serializedLightData = new SerializedLightData
                {
                    intensity = o.Find(x => x.displayLightIntensity),
                    enableSpotReflector = o.Find(x => x.enableSpotReflector),
                    luxAtDistance = o.Find(x => x.luxAtDistance),
                    spotInnerPercent = o.Find(x => x.m_InnerSpotPercent),
                    lightDimmer = o.Find(x => x.lightDimmer),
                    volumetricDimmer = o.Find(x => x.volumetricDimmer),
                    lightUnit = o.Find(x => x.lightUnit),
                    displayAreaLightEmissiveMesh = o.Find(x => x.displayAreaLightEmissiveMesh),
                    fadeDistance = o.Find(x => x.fadeDistance),
                    affectDiffuse = o.Find(x => x.affectDiffuse),
                    affectSpecular = o.Find(x => x.affectSpecular),
                    nonLightmappedOnly = o.Find(x => x.nonLightmappedOnly),
                    lightTypeExtent = o.Find(x => x.lightTypeExtent),
                    spotLightShape = o.Find("m_SpotLightShape"),
                    shapeWidth = o.Find(x => x.shapeWidth),
                    shapeHeight = o.Find(x => x.shapeHeight),
                    aspectRatio = o.Find(x => x.aspectRatio),
                    shapeRadius = o.Find(x => x.shapeRadius),
                    maxSmoothness = o.Find(x => x.maxSmoothness),
                    applyRangeAttenuation = o.Find(x => x.applyRangeAttenuation),
                    shadowNearPlane = o.Find(x => x.shadowNearPlane),
                    shadowSoftness = o.Find(x => x.shadowSoftness),
                    blockerSampleCount = o.Find(x => x.blockerSampleCount),
                    filterSampleCount = o.Find(x => x.filterSampleCount),
                    minFilterSize = o.Find(x => x.minFilterSize),
                    areaLightCookie = o.Find(x => x.areaLightCookie),
                    areaLightShadowCone = o.Find(x => x.areaLightShadowCone),
#if ENABLE_RAYTRACING
                    useRayTracedShadows = o.Find(x => x.useRayTracedShadows),
                    numRayTracingSamples = o.Find(x => x.numRayTracingSamples),
                    filterTracedShadow = o.Find(x => x.filterTracedShadow),
                    filterSizeTraced = o.Find(x => x.filterSizeTraced),
#endif
                    evsmExponent = o.Find(x => x.evsmExponent),
                    evsmVarianceBias = o.Find(x => x.evsmVarianceBias),
                    evsmLightLeakBias = o.Find(x => x.evsmLightLeakBias),
                    evsmBlurPasses = o.Find(x => x.evsmBlurPasses),

                    // Moment light
                    lightAngle = o.Find(x => x.lightAngle),
                    kernelSize = o.Find(x => x.kernelSize),
                    maxDepthBias = o.Find(x => x.maxDepthBias),

                    // Editor stuff
                    useOldInspector = o.Find(x => x.useOldInspector),
                    showFeatures = o.Find(x => x.featuresFoldout),
                    showAdditionalSettings = o.Find(x => x.showAdditionalSettings),
                    useVolumetric = o.Find(x => x.useVolumetric),
                    renderingLayerMask = settings.renderingLayerMask,

                    // Layers
                    linkLightLayers = o.Find(x => x.linkShadowLayers),
                    lightlayersMask = o.Find(x => x.lightlayersMask)
                };

            // TODO: Review this once AdditionalShadowData is refactored
            using (var o = new PropertyFetcher<AdditionalShadowData>(serializedShadowDatas))
                serializedShadowData = new SerializedShadowData
                {
                    shadowDimmer = o.Find(x => x.shadowDimmer),
                    volumetricShadowDimmer = o.Find(x => x.volumetricShadowDimmer),
                    fadeDistance = o.Find(x => x.shadowFadeDistance),
                    resolution = o.Find(x => x.shadowResolution),
                    contactShadows = o.Find(x => x.contactShadows),
                    shadowTint = o.Find(x => x.shadowTint),                    

                    viewBiasMin = o.Find(x => x.viewBiasMin),
                    viewBiasMax = o.Find(x => x.viewBiasMax),
                    viewBiasScale = o.Find(x => x.viewBiasScale),
                    normalBiasMin = o.Find(x => x.normalBiasMin),
                    normalBiasMax = o.Find(x => x.normalBiasMax),
                    normalBiasScale = o.Find(x => x.normalBiasScale),
                    sampleBiasScale = o.Find(x => x.sampleBiasScale),
                    edgeLeakFixup = o.Find(x => x.edgeLeakFixup),
                    edgeToleranceNormal = o.Find(x => x.edgeToleranceNormal),
                    edgeTolerance = o.Find(x => x.edgeTolerance)
                };
        }

        public void Update()
        {
            serializedLightDatas.Update();
            serializedShadowDatas.Update();
            settings.Update();

            ResolveLightShape();
        }

        public void Apply()
        {
            serializedLightDatas.ApplyModifiedProperties();
            serializedShadowDatas.ApplyModifiedProperties();
            settings.ApplyModifiedProperties();
        }

        void ResolveLightShape()
        {
            var type = settings.lightType;

            // Special case for multi-selection: don't resolve light shape or it'll corrupt lights
            if (type.hasMultipleDifferentValues
                || serializedLightData.lightTypeExtent.hasMultipleDifferentValues)
            {
                editorLightShape = (LightShape)(-1);
                return;
            }

            var lightTypeExtent = (LightTypeExtent)serializedLightData.lightTypeExtent.enumValueIndex;
            switch (lightTypeExtent)
            {
                case LightTypeExtent.Punctual:
                    switch ((LightType)type.enumValueIndex)
                    {
                        case LightType.Directional:
                            editorLightShape = LightShape.Directional;
                            break;
                        case LightType.Point:
                            editorLightShape = LightShape.Point;
                            break;
                        case LightType.Spot:
                            editorLightShape = LightShape.Spot;
                            break;
                    }
                    break;
                case LightTypeExtent.Rectangle:
                    editorLightShape = LightShape.Rectangle;
                    break;
                case LightTypeExtent.Tube:
                    editorLightShape = LightShape.Tube;
                    break;
            }
        }
    }
}
