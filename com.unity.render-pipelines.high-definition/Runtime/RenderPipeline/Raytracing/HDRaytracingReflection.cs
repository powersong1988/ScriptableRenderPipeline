using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;

namespace UnityEngine.Experimental.Rendering.HDPipeline
{
#if ENABLE_RAYTRACING
    using RTHandle = RTHandleSystem.RTHandle;

    public partial class HDRenderPipeline
    {
        // Intermediate buffer for computing the effect
        RTHandle m_ReflIntermediateTexture0 = null;
        RTHandle m_ReflIntermediateTexture1 = null;

        // String values
        const string m_RayGenReflectionHalfResName = "RayGenReflectionHalfRes";
        const string m_RayGenReflectionFullResName = "RayGenReflectionFullRes";
        const string m_RayGenIntegrationName = "RayGenIntegration";

        public void InitRayTracedReflections()
        {
            m_ReflIntermediateTexture0 = RTHandles.Alloc(Vector2.one, TextureXR.slices, colorFormat: GraphicsFormat.R16G16B16A16_SFloat, dimension: TextureXR.dimension, enableRandomWrite: true, useDynamicScale: true, useMipMap: false, autoGenerateMips: false, name: "ReflIntermediateTexture0");
            m_ReflIntermediateTexture1 = RTHandles.Alloc(Vector2.one, TextureXR.slices, colorFormat: GraphicsFormat.R16G16B16A16_SFloat, dimension: TextureXR.dimension, enableRandomWrite: true, useDynamicScale: true, useMipMap: false, autoGenerateMips: false, name: "ReflIntermediateTexture1");
        }

        static RTHandleSystem.RTHandle ReflectionHistoryBufferAllocatorFunction(string viewName, int frameIndex, RTHandleSystem rtHandleSystem)
        {
            return rtHandleSystem.Alloc(Vector2.one, TextureXR.slices, colorFormat: GraphicsFormat.R16G16B16A16_SFloat, dimension: TextureXR.dimension,
                                        enableRandomWrite: true, useMipMap: false, autoGenerateMips: false,
                                        name: string.Format("ReflectionHistoryBuffer{0}", frameIndex));
        }

        public void ReleaseRayTracedReflections()
        {
            RTHandles.Release(m_ReflIntermediateTexture1);
            RTHandles.Release(m_ReflIntermediateTexture0);
        }

        public void RenderRayTracedReflections(HDCamera hdCamera, CommandBuffer cmd, RTHandleSystem.RTHandle outputTexture, ScriptableRenderContext renderContext, int frameCount)
        {
            RenderPipelineSettings.RaytracingTier currentTier = m_Asset.currentPlatformRenderPipelineSettings.supportedRaytracingTier;
            switch (currentTier)
            {
                case RenderPipelineSettings.RaytracingTier.Tier1:
                {
                    RenderReflectionsT1(hdCamera, cmd, outputTexture, renderContext, frameCount);
                }
                break;
                case RenderPipelineSettings.RaytracingTier.Tier2:
                {
                    RenderReflectionsT2(hdCamera, cmd, outputTexture, renderContext, frameCount);
                }
                break;
            }
        }

        public void BindRayTracedReflectionData(CommandBuffer cmd, HDCamera hdCamera, HDRaytracingEnvironment rtEnvironment, RaytracingShader reflectionShader, ScreenSpaceReflection settings, LightCluster lightClusterSettings)
        {
            // Grab the acceleration structures and the light cluster to use
            RaytracingAccelerationStructure accelerationStructure = m_RayTracingManager.RequestAccelerationStructure(rtEnvironment.reflLayerMask);
            HDRaytracingLightCluster lightCluster = m_RayTracingManager.RequestLightCluster(rtEnvironment.reflLayerMask);

            // Define the shader pass to use for the reflection pass
            cmd.SetRaytracingShaderPass(reflectionShader, "IndirectDXR");

            // Set the acceleration structure for the pass
            cmd.SetRaytracingAccelerationStructure(reflectionShader, HDShaderIDs._RaytracingAccelerationStructureName, accelerationStructure);

            // Inject the ray-tracing sampling data
            cmd.SetRaytracingTextureParam(reflectionShader, HDShaderIDs._OwenScrambledTexture, m_Asset.renderPipelineResources.textures.owenScrambledTex);
            cmd.SetRaytracingTextureParam(reflectionShader, HDShaderIDs._ScramblingTexture, m_Asset.renderPipelineResources.textures.scramblingTex);

            // Global reflection parameters
            cmd.SetRaytracingFloatParams(reflectionShader, HDShaderIDs._RaytracingIntensityClamp, settings.clampValue.value);
            cmd.SetRaytracingFloatParams(reflectionShader, HDShaderIDs._RaytracingReflectionMinSmoothness, settings.minSmoothness.value);
            cmd.SetRaytracingIntParams(reflectionShader, HDShaderIDs._RaytracingReflectSky, settings.reflectSky.value ? 1 : 0);

            // Inject the ray generation data
            cmd.SetGlobalFloat(HDShaderIDs._RaytracingRayBias, rtEnvironment.rayBias);
            cmd.SetGlobalFloat(HDShaderIDs._RaytracingRayMaxLength, settings.rayLength.value);
            cmd.SetRaytracingIntParams(reflectionShader, HDShaderIDs._RaytracingNumSamples, settings.numSamples.value);
            int frameIndex = hdCamera.IsTAAEnabled() ? hdCamera.taaFrameIndex : (int)m_FrameCount % 8;
            cmd.SetRaytracingIntParam(reflectionShader, HDShaderIDs._RaytracingFrameIndex, frameIndex);

            // Set the data for the ray generation
            cmd.SetRaytracingTextureParam(reflectionShader, HDShaderIDs._SsrLightingTextureRW, m_ReflIntermediateTexture0);
            cmd.SetRaytracingTextureParam(reflectionShader, HDShaderIDs._SsrHitPointTexture, m_ReflIntermediateTexture1);
            cmd.SetRaytracingTextureParam(reflectionShader, HDShaderIDs._DepthTexture, m_SharedRTManager.GetDepthStencilBuffer());
            cmd.SetRaytracingTextureParam(reflectionShader, HDShaderIDs._NormalBufferTexture, m_SharedRTManager.GetNormalBuffer());

            // Set ray count tex
            cmd.SetRaytracingIntParam(reflectionShader, HDShaderIDs._RayCountEnabled, m_RayTracingManager.rayCountManager.RayCountIsEnabled());
            cmd.SetRaytracingTextureParam(reflectionShader, HDShaderIDs._RayCountTexture, m_RayTracingManager.rayCountManager.rayCountTexture);

            // Compute the pixel spread value
            float pixelSpreadAngle = Mathf.Atan(2.0f * Mathf.Tan(hdCamera.camera.fieldOfView * Mathf.PI / 360.0f) / Mathf.Min(hdCamera.actualWidth, hdCamera.actualHeight));
            cmd.SetRaytracingFloatParam(reflectionShader, HDShaderIDs._RaytracingPixelSpreadAngle, pixelSpreadAngle);

            // LightLoop data
            cmd.SetGlobalBuffer(HDShaderIDs._RaytracingLightCluster, lightCluster.GetCluster());
            cmd.SetGlobalBuffer(HDShaderIDs._LightDatasRT, lightCluster.GetLightDatas());
            cmd.SetGlobalVector(HDShaderIDs._MinClusterPos, lightCluster.GetMinClusterPos());
            cmd.SetGlobalVector(HDShaderIDs._MaxClusterPos, lightCluster.GetMaxClusterPos());
            cmd.SetGlobalInt(HDShaderIDs._LightPerCellCount, lightClusterSettings.maxNumLightsPercell.value);
            cmd.SetGlobalInt(HDShaderIDs._PunctualLightCountRT, lightCluster.GetPunctualLightCount());
            cmd.SetGlobalInt(HDShaderIDs._AreaLightCountRT, lightCluster.GetAreaLightCount());

            // Note: Just in case, we rebind the directional light data (in case they were not)
            cmd.SetGlobalBuffer(HDShaderIDs._DirectionalLightDatas, directionalLightDatas);
            cmd.SetGlobalInt(HDShaderIDs._DirectionalLightCount, m_lightList.directionalLights.Count);

            // Evaluate the clear coat mask texture based on the lit shader mode
            RenderTargetIdentifier clearCoatMaskTexture = hdCamera.frameSettings.litShaderMode == LitShaderMode.Deferred ? m_GbufferManager.GetBuffersRTI()[2] : TextureXR.GetBlackTexture();
            cmd.SetRaytracingTextureParam(reflectionShader, HDShaderIDs._SsrClearCoatMaskTexture, clearCoatMaskTexture);
        }

        public void RenderReflectionsT1(HDCamera hdCamera, CommandBuffer cmd, RTHandleSystem.RTHandle outputTexture, ScriptableRenderContext renderContext, int frameCount)
        {
            // First thing to check is: Do we have a valid ray-tracing environment?
            HDRaytracingEnvironment rtEnvironment = m_RayTracingManager.CurrentEnvironment();
            // If no ray tracing environment, then we do not want to evaluate this effect
            if (rtEnvironment == null)
                return;

            // Fetch the required resources
            BlueNoise blueNoise = m_RayTracingManager.GetBlueNoiseManager();
            RaytracingShader reflectionShaderRT = m_Asset.renderPipelineRayTracingResources.reflectionRaytracingRT;
            ComputeShader reflectionShaderCS = m_Asset.renderPipelineRayTracingResources.reflectionRaytracingCS; 
            ComputeShader reflectionFilter = m_Asset.renderPipelineRayTracingResources.reflectionBilateralFilterCS;

            // Fetch all the settings
            var settings = VolumeManager.instance.stack.GetComponent<ScreenSpaceReflection>();
            LightCluster lightClusterSettings = VolumeManager.instance.stack.GetComponent<LightCluster>();

            if (settings.deferredMode.value)
            {
                // Fetch the new sample kernel
                int currentKernel = reflectionShaderCS.FindKernel(settings.fullResolution.value ? "RaytracingReflectionsFullRes" : "RaytracingReflectionsHalfRes");

                // Bind all the required textures
                cmd.SetComputeTextureParam(reflectionShaderCS, currentKernel, HDShaderIDs._OwenScrambledTexture, m_Asset.renderPipelineResources.textures.owenScrambledTex);
                cmd.SetComputeTextureParam(reflectionShaderCS, currentKernel, HDShaderIDs._ScramblingTexture, m_Asset.renderPipelineResources.textures.scramblingTex);
                cmd.SetComputeTextureParam(reflectionShaderCS, currentKernel, HDShaderIDs._DepthTexture, m_SharedRTManager.GetDepthStencilBuffer());
                cmd.SetComputeTextureParam(reflectionShaderCS, currentKernel, HDShaderIDs._NormalBufferTexture, m_SharedRTManager.GetNormalBuffer());
                RenderTargetIdentifier clearCoatMaskTexture = hdCamera.frameSettings.litShaderMode == LitShaderMode.Deferred ? m_GbufferManager.GetBuffersRTI()[2] : TextureXR.GetBlackTexture();
                cmd.SetComputeTextureParam(reflectionShaderCS, currentKernel, HDShaderIDs._SsrClearCoatMaskTexture, clearCoatMaskTexture);

                // Bind all the required scalars
                cmd.SetComputeFloatParam(reflectionShaderCS, HDShaderIDs._RaytracingIntensityClamp, settings.clampValue.value);
                cmd.SetComputeFloatParam(reflectionShaderCS, HDShaderIDs._RaytracingReflectionMinSmoothness, settings.minSmoothness.value);
                cmd.SetComputeIntParam(reflectionShaderCS, HDShaderIDs._RaytracingReflectSky, settings.reflectSky.value ? 1 : 0);

                // Bind the sampling data
                int frameIndex = hdCamera.IsTAAEnabled() ? hdCamera.taaFrameIndex : (int)m_FrameCount % 8;
                cmd.SetComputeIntParam(reflectionShaderCS, HDShaderIDs._RaytracingFrameIndex, frameIndex);

                // Bind the output buffers
                cmd.SetComputeTextureParam(reflectionShaderCS, currentKernel, HDShaderIDs._RaytracingDirectionBuffer, m_ReflIntermediateTexture1);

                // Texture dimensions
                int texWidth = hdCamera.actualWidth;
                int texHeight = hdCamera.actualHeight;

                if (settings.fullResolution.value)
                {
                    // Evaluate the dispatch parameters
                    int areaTileSize = 8;
                    int numTilesXHR = (texWidth + (areaTileSize - 1)) / areaTileSize;
                    int numTilesYHR = (texHeight + (areaTileSize - 1)) / areaTileSize;

                    // Compute the directions
                    cmd.DispatchCompute(reflectionShaderCS, currentKernel, numTilesXHR, numTilesYHR, 1);
                }
                else
                {
                    // Evaluate the dispatch parameters
                    int areaTileSize = 8;
                    int numTilesXHR = (texWidth / 2 + (areaTileSize - 1)) / areaTileSize;
                    int numTilesYHR = (texHeight / 2 + (areaTileSize - 1)) / areaTileSize;

                    // Compute the directions
                    cmd.DispatchCompute(reflectionShaderCS, currentKernel, numTilesXHR, numTilesYHR, 1);
                }
                // Evaluate the deferred lighting
                cmd.SetGlobalInt(HDShaderIDs._RaytracingReflectSky, settings.reflectSky.value ? 1 : 0);
                RenderRaytracingDeferredLighting(cmd, hdCamera, rtEnvironment, m_ReflIntermediateTexture1, settings.rayBinning.value, rtEnvironment.reflLayerMask, settings.rayLength.value, m_ReflIntermediateTexture0, disableSpecularLighting: true, halfResolution: !settings.fullResolution.value);
            }
            else
            {
                // Bind all the required data for ray tracing
                BindRayTracedReflectionData(cmd, hdCamera, rtEnvironment, reflectionShaderRT, settings, lightClusterSettings);

                // Set the data for the ray miss
                cmd.SetRaytracingTextureParam(reflectionShaderRT, HDShaderIDs._SkyTexture, m_SkyManager.skyReflection);

                // Force to disable specular lighting
                cmd.SetGlobalInt(HDShaderIDs._EnableSpecularLighting, 0);

                // Run the computation
                if (settings.fullResolution.value)
                {
                    cmd.DispatchRays(reflectionShaderRT, m_RayGenReflectionFullResName, (uint)hdCamera.actualWidth, (uint)hdCamera.actualHeight, 1);
                }
                else
                {
                    // Run the computation
                    cmd.DispatchRays(reflectionShaderRT, m_RayGenReflectionHalfResName, (uint)(hdCamera.actualWidth / 2), (uint)(hdCamera.actualHeight / 2), 1);
                }

                // Restore the previous state of specular lighting
                cmd.SetGlobalInt(HDShaderIDs._EnableSpecularLighting, hdCamera.frameSettings.IsEnabled(FrameSettingsField.SpecularLighting) ? 1 : 0);
            }

            using (new ProfilingSample(cmd, "Filter Reflection", CustomSamplerId.RaytracingFilterReflection.GetSampler()))
            {
                // Fetch the right filter to use
                int currentKernel = 0;
                if (settings.fullResolution.value)
                {
                    currentKernel = reflectionFilter.FindKernel("ReflectionIntegrationUpscaleFullRes"); 
                }
                else
                {
                    currentKernel = reflectionFilter.FindKernel("ReflectionIntegrationUpscaleHalfRes");
                }

                // Inject all the parameters for the compute
                cmd.SetComputeTextureParam(reflectionFilter, currentKernel, HDShaderIDs._SsrLightingTextureRW, m_ReflIntermediateTexture0);
                cmd.SetComputeTextureParam(reflectionFilter, currentKernel, HDShaderIDs._SsrHitPointTexture, m_ReflIntermediateTexture1);
                cmd.SetComputeTextureParam(reflectionFilter, currentKernel, HDShaderIDs._DepthTexture, m_SharedRTManager.GetDepthStencilBuffer());
                cmd.SetComputeTextureParam(reflectionFilter, currentKernel, HDShaderIDs._NormalBufferTexture, m_SharedRTManager.GetNormalBuffer());
                cmd.SetComputeTextureParam(reflectionFilter, currentKernel, "_NoiseTexture", blueNoise.textureArray16RGB);
                cmd.SetComputeTextureParam(reflectionFilter, currentKernel, "_RaytracingReflectionTexture", outputTexture);
                cmd.SetComputeTextureParam(reflectionFilter, currentKernel, HDShaderIDs._ScramblingTexture, m_Asset.renderPipelineResources.textures.scramblingTex);
                cmd.SetComputeIntParam(reflectionFilter, HDShaderIDs._SpatialFilterRadius, settings.spatialFilterRadius.value);
                cmd.SetComputeFloatParam(reflectionFilter, HDShaderIDs._RaytracingReflectionMinSmoothness, settings.minSmoothness.value);

                // Texture dimensions
                int texWidth = hdCamera.actualWidth;
                int texHeight = hdCamera.actualHeight;

                // Evaluate the dispatch parameters
                int areaTileSize = 8;
                int numTilesXHR = (texWidth  + (areaTileSize - 1)) / areaTileSize;
                int numTilesYHR = (texHeight + (areaTileSize - 1)) / areaTileSize;

                // Bind the right texture for clear coat support
                RenderTargetIdentifier clearCoatMaskTexture = hdCamera.frameSettings.litShaderMode == LitShaderMode.Deferred ? m_GbufferManager.GetBuffersRTI()[2] : TextureXR.GetBlackTexture();
                cmd.SetComputeTextureParam(reflectionFilter, currentKernel, HDShaderIDs._SsrClearCoatMaskTexture, clearCoatMaskTexture);

                // Compute the texture
                cmd.DispatchCompute(reflectionFilter, currentKernel, numTilesXHR, numTilesYHR, 1);

                using (new ProfilingSample(cmd, "Filter Reflection", CustomSamplerId.RaytracingFilterReflection.GetSampler()))
                {
                    if (settings.enableFilter.value)
                    {
                        // Grab the history buffer
                        RTHandleSystem.RTHandle reflectionHistory = hdCamera.GetCurrentFrameRT((int)HDCameraFrameHistoryType.RaytracedReflection)
                            ?? hdCamera.AllocHistoryFrameRT((int)HDCameraFrameHistoryType.RaytracedReflection, ReflectionHistoryBufferAllocatorFunction, 1);

                        HDSimpleDenoiser simpleDenoiser = m_RayTracingManager.GetSimpleDenoiser();
                        simpleDenoiser.DenoiseBuffer(cmd, hdCamera, outputTexture, reflectionHistory, m_ReflIntermediateTexture0, settings.filterRadius.value, singleChannel: false);
                        HDUtils.BlitCameraTexture(cmd, m_ReflIntermediateTexture0, outputTexture);
                    }
                }
            }
        }

        public void RenderReflectionsT2(HDCamera hdCamera, CommandBuffer cmd, RTHandleSystem.RTHandle outputTexture, ScriptableRenderContext renderContext, int frameCount)
        {
            // First thing to check is: Do we have a valid ray-tracing environment?
            HDRaytracingEnvironment rtEnvironment = m_RayTracingManager.CurrentEnvironment();
            // If no acceleration structure available, end it now
            if (rtEnvironment == null)
                return;

            // Fetch the shaders that we will be using
            ComputeShader reflectionFilter = m_Asset.renderPipelineRayTracingResources.reflectionBilateralFilterCS;
            RaytracingShader reflectionShader = m_Asset.renderPipelineRayTracingResources.reflectionRaytracingRT;

            var settings = VolumeManager.instance.stack.GetComponent<ScreenSpaceReflection>();
            LightCluster lightClusterSettings = VolumeManager.instance.stack.GetComponent<LightCluster>();

            // Bind all the required data for ray tracing
            BindRayTracedReflectionData(cmd, hdCamera, rtEnvironment, reflectionShader, settings, lightClusterSettings);

            // Force to disable specular lighting
            cmd.SetGlobalInt(HDShaderIDs._EnableSpecularLighting, 0);

            // Run the computation
            cmd.DispatchRays(reflectionShader, m_RayGenIntegrationName, (uint)hdCamera.actualWidth, (uint)hdCamera.actualHeight, 1);

            // Restore the previous state of specular lighting
            cmd.SetGlobalInt(HDShaderIDs._EnableSpecularLighting, hdCamera.frameSettings.IsEnabled(FrameSettingsField.SpecularLighting) ? 1 : 0);

            using (new ProfilingSample(cmd, "Filter Reflection", CustomSamplerId.RaytracingFilterReflection.GetSampler()))
            {
                if (settings.enableFilter.value)
                {
                    // Grab the history buffer
                    RTHandleSystem.RTHandle reflectionHistory = hdCamera.GetCurrentFrameRT((int)HDCameraFrameHistoryType.RaytracedReflection)
                        ?? hdCamera.AllocHistoryFrameRT((int)HDCameraFrameHistoryType.RaytracedReflection, ReflectionHistoryBufferAllocatorFunction, 1);

                    HDSimpleDenoiser simpleDenoiser = m_RayTracingManager.GetSimpleDenoiser();
                    simpleDenoiser.DenoiseBuffer(cmd, hdCamera, m_ReflIntermediateTexture0, reflectionHistory, outputTexture, settings.filterRadius.value, singleChannel: false);
                }
                else
                {
                    HDUtils.BlitCameraTexture(cmd, m_ReflIntermediateTexture0, outputTexture);
                }
            }
        }
    }
#endif
}
