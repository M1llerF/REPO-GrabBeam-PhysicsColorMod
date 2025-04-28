using UnityEngine;

namespace DynamicRepoGrabBeam.ColorLogic
{
    /// <summary>
    /// Holds contextual data for modifying the color of a grab beam,
    /// including original color properties and force escalation factors.
    /// </summary>
    public class BeamContext
    {
        /// <summary>The object currently grabbed.</summary>
        public PhysGrabObject GrabbedObject { get; }

        /// <summary>The Renderer associated with the beam.</summary>
        public Renderer BeamRenderer { get; }

        /// <summary>Initial hue value (H) of the base color.</summary>
        public float H0 { get; }

        /// <summary>Initial saturation value (S) of the base color.</summary>
        public float S0 { get; }

        /// <summary>Initial brightness value (V) of the base color.</summary>
        public float V0 { get; }

        /// <summary>Maximum force value considered for color scaling.</summary>
        public float MaxForce { get; }

        /// <summary>Scaling factor for saturation adjustment based on impact force.</summary>
        public float SaturationScale { get; }

        /// <summary>Scaling factor for brightness adjustment based on impact force.</summary>
        public float BrightnessScale { get; }

        /// <summary>Scaling factor for transparency (alpha) adjustment based on impact force.</summary>
        public float AlphaScale { get; }

        /// <summary>Base starting color for the beam before modifications.</summary>
        public Color BaseColor { get; }

        /// <summary>
        /// Constructs a new BeamContext with the given initial beam and impact parameters.
        /// </summary>
        public BeamContext(
            PhysGrabObject obj,
            Renderer renderer,
            float h, float s, float v,
            float maxForce,
            float satScale,
            float brightScale,
            float alphaScale,
            Color baseColor)
        {
            GrabbedObject   = obj;
            BeamRenderer    = renderer;
            H0              = h;
            S0              = s;
            V0              = v;
            MaxForce        = maxForce;
            SaturationScale = satScale;
            BrightnessScale = brightScale;
            AlphaScale      = alphaScale;
            BaseColor       = baseColor;
        }
    }
}
