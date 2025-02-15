using System;
using UnityEngine.Rendering;

namespace UnityEngine.Experimental.Rendering.HDPipeline
{
    [Serializable, VolumeComponentMenu("Post-processing/Motion Blur")]
    public sealed class MotionBlur : VolumeComponent, IPostProcessComponent
    {
        public MinIntParameter sampleCount = new MinIntParameter(8, 2);

        [Tooltip("Sets the intensity of the motion blur effect. Acts as a multiplier for velocities.")]
        public MinFloatParameter intensity = new MinFloatParameter(0.0f, 0.0f);
        [Tooltip("Sets the maximum velocity, in pixels, for everything except Camera rotation. Larger values result in a wider blur for fast objects. Increasing this value can hamper performance.")]
        public ClampedFloatParameter maximumVelocity = new ClampedFloatParameter(250.0f, 0.0f, 1500.0f);
        [Tooltip("Sets the minimum velocity, in pixels, that a GameObject must have to contribute to the motion blur effect.")]
        public ClampedFloatParameter minimumVelocity = new ClampedFloatParameter(2.0f, 0.0f, 64.0f);
        [Tooltip("Sets the maximum length, as a fraction of the screen's full resolution, that the velocity resulting from Camera rotation can have.")]
        public ClampedFloatParameter cameraRotationVelocityClamp = new ClampedFloatParameter(0.03f, 0.0f, 0.2f);

        public bool IsActive()
        {
            return intensity.value > 0.0f;
        }
    }
}
