using UnityEngine;
using DynamicRepoGrabBeam.Patches;

namespace DynamicRepoGrabBeam.ColorLogic
{
    /// <summary>
    /// Strategy that overrides the beam color when grabbing another player.
    /// </summary>
    public class PlayerGrabColorStrategy : IBeamColorStrategy
    {
        public bool Applies(PhysGrabber grabber, PhysGrabObject obj)
            => obj.IsPlayer();

        public Color Modify(Color current, PhysGrabber grabber, PhysGrabObject obj)
            => Plugin.PlayerGrabColor.Value;

        public Color ModifyFast(
            Color current,
            PhysGrabber grabber,
            PhysGrabObject obj,
            float impact,
            BeamContext ctx
        ) => Plugin.PlayerGrabColor.Value;
    }

    /// <summary>
    /// Strategy that adjusts the beam color based on the mass of the grabbed object.
    /// </summary>
    public class MassColorStrategy : IBeamColorStrategy
    {
        public bool Applies(PhysGrabber grabber, PhysGrabObject obj)
        {
            if (obj.IsPlayer()) return false;
            return obj.TryGetComponent<Rigidbody>(out _);
        }

        public Color Modify(Color current, PhysGrabber grabber, PhysGrabObject obj)
        {
            if (!obj.TryGetComponent<Rigidbody>(out var rb))
                return current;

            float mass     = rb.mass;
            float lightMax = Plugin.LightMax.Value;
            float heavyMin = Plugin.HeavyMin.Value;

            if (mass < lightMax) return Plugin.LightColor.Value;
            if (mass > heavyMin) return Plugin.HeavyColor.Value;

            return Plugin.MediumColor.Value;
        }

        public Color ModifyFast(
            Color current,
            PhysGrabber grabber,
            PhysGrabObject obj,
            float impact,
            BeamContext ctx
        ) => Modify(current, grabber, obj);
    }

    /// <summary>
    /// Strategy that overrides the beam color while the object is being rotated.
    /// </summary>
    public class RotationLightenStrategy : IBeamColorStrategy
    {
        public bool Applies(PhysGrabber grabber, PhysGrabObject obj)
            => grabber.isRotating;

        public Color Modify(Color current, PhysGrabber grabber, PhysGrabObject obj)
            => Plugin.RotationModeColor.Value;

        public Color ModifyFast(Color current, PhysGrabber grabber, PhysGrabObject obj, float impact, BeamContext ctx)
        {
            if (!grabber.isRotating)
                return current;

            if (Plugin.EnableDebugLogs.Value)
                Plugin.Log.LogInfo("[Rotation] Overriding beam with RotationModeColor");

            return Plugin.RotationModeColor.Value;
        }
    }
}
