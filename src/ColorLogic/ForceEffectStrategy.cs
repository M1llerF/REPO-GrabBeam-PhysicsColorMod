using UnityEngine;
using DynamicRepoGrabBeam.Patches;

namespace DynamicRepoGrabBeam.ColorLogic
{
    /// <summary>
    /// Strategy that adjusts the beam's visual intensity based on the impact force of the grabbed object.
    /// </summary>
    public class ForceEffectStrategy : IBeamColorStrategy
    {
        /// <summary>
        /// Computes an approximate impact force value for a given grabbed object,
        /// based on its break force and fragility properties.
        /// </summary>
        public static float ComputeImpact(PhysGrabObject obj)
        {
            if (!obj.TryGetComponent(out PhysGrabObjectImpactDetector det))
                return 0f;

            float breakForce = det.GetBreakForce(); // Retrieved via reflection
            float fragility = det.GetFragility();

            // Approximate impact force model
            return (breakForce / 8f) * fragility;
        }

        /// <summary>
        /// Determines if the strategy applies to the given object (requires an impact detector).
        /// </summary>
        public bool Applies(PhysGrabber grabber, PhysGrabObject obj)
            => obj.TryGetComponent<PhysGrabObjectImpactDetector>(out _);

        /// <summary>
        /// No modification is applied in the non-optimized Modify path.
        /// </summary>
        public Color Modify(Color current, PhysGrabber grabber, PhysGrabObject obj)
            => current;

        /// <summary>
        /// Applies fast color adjustments based on computed impact force and context scaling factors.
        /// Increases saturation, brightness, and alpha proportional to the impact strength.
        /// </summary>
        public Color ModifyFast(
            Color current,
            PhysGrabber grabber,
            PhysGrabObject obj,
            float ignoredImpact,
            BeamContext ctx)
        {
            float impact = ComputeImpact(obj);
            float strength = Mathf.Clamp01(impact / ctx.MaxForce);

            if (Plugin.EnableDebugLogs.Value)
                Plugin.Log.LogInfo($"[ForceEffectStrategy] Impact = {impact:F2}, Strength = {strength:F2}");

            float h = ctx.H0, s = ctx.S0, v = ctx.V0;

            // Adjust saturation and brightness based on normalized impact strength
            s = Mathf.Clamp01(s + strength * ctx.SaturationScale);
            v = Mathf.Clamp01(v + strength * ctx.BrightnessScale);
            Color result = Color.HSVToRGB(h, s, v);

            // Adjust transparency (alpha) based on strength
            result.a = Mathf.Lerp(0.1f, 1.0f, strength * ctx.AlphaScale);
            return result;
        }
    }
}
