using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;
using DynamicRepoGrabBeam.Patches;

namespace DynamicRepoGrabBeam.ColorLogic
{
    /// <summary>
    /// Manages all beam color updates, strategies, and dynamic recoloring
    /// for both the local player and remote players.
    /// </summary>
    public static class BeamColorManager
    {
        // List of all available beam color modification strategies
        private static readonly List<IBeamColorStrategy> allStrategies = new()
        {
            new PlayerGrabColorStrategy(),
            new MassColorStrategy(),
            new ForceEffectStrategy(),
            new RotationLightenStrategy()
        };

        private static BeamContext _ctx;
        private static List<IBeamColorStrategy> _active;
        private static PhysGrabObject lastObj = null;

        /// <summary>
        /// Initializes the BeamColorManager and spawns the runtime observer.
        /// </summary>
        public static void Initialize()
        {
            var observer = new GameObject("BeamColorObserver");
            observer.hideFlags = HideFlags.HideAndDontSave;
            observer.AddComponent<BeamColorObserver>();
            observer.SetActive(true);

            Plugin.Log.LogInfo("[DynamicRepoGrabBeam] BeamColorObserver GameObject created and observing.");
        }

        /// <summary>
        /// Harmony patch that monitors PhysGrabber.Update to trigger beam color updates
        /// for the local player based on grab changes.
        /// </summary>
        [HarmonyPatch(typeof(PhysGrabber), "Update")]
        static class Hook_LateUpdate
        {
            static void Postfix(PhysGrabber __instance)
            {
                var obj = __instance.GetGrabbedObject();

                if (obj == null)
                {
                    _ctx = null;
                    lastObj = null;
                    return;
                }

                // Only rebuild context if a new object is grabbed
                if (_ctx == null || obj != lastObj)
                {
                    lastObj = obj;
                    BuildContext(__instance, obj);
                }

                UpdateBeam(__instance);
            }
        }

        /// <summary>
        /// Builds a new BeamContext for the given grabber and object.
        /// </summary>
        private static void BuildContext(PhysGrabber grabber, PhysGrabObject obj)
        {
            var beamGO = grabber.GetBeamObject();
            var rend = beamGO?.GetComponentInChildren<Renderer>();
            if (rend == null) return;

            Color baseColor = Color.white;
            foreach (var strat in allStrategies)
            {
                if (strat is ForceEffectStrategy) break;
                if (strat.Applies(grabber, obj))
                    baseColor = strat.Modify(baseColor, grabber, obj);
            }

            Color.RGBToHSV(baseColor, out float h, out float s, out float v);

            _ctx = new BeamContext(
                obj, rend, h, s, v,
                Plugin.MaxStunVisualForce.Value,
                Plugin.ForceSaturationScale.Value,
                Plugin.ForceBrightnessScale.Value,
                Plugin.ForceAlphaScale.Value,
                baseColor
            );

            _active = allStrategies.ToList();
        }

        /// <summary>
        /// Updates the beam color for the local player's currently grabbed object,
        /// applying all active color strategies.
        /// </summary>
        private static void UpdateBeam(PhysGrabber grabber)
        {
            float impact = 0f;
            if (_ctx.GrabbedObject.TryGetComponent<PhysGrabObjectImpactDetector>(out var det))
                impact = ForceEffectStrategy.ComputeImpact(_ctx.GrabbedObject);

            Color current = Color.HSVToRGB(_ctx.H0, _ctx.S0, _ctx.V0);

            foreach (var strat in _active)
                current = strat.ModifyFast(current, grabber, _ctx.GrabbedObject, impact, _ctx);

            ApplyBeamColor(_ctx.BeamRenderer, current);
        }

        /// <summary>
        /// Applies the specified color to the given beam renderer.
        /// Updates both the base color and emission color.
        /// </summary>
        public static void ApplyBeamColor(Renderer rend, Color color)
        {
            var mat = rend.material;
            mat.SetColor("_Color", color);
            mat.SetColor("_EmissionColor", color);
        }

        /// <summary>
        /// Updates the beam color for a given grabber and object manually,
        /// typically used by external observers scanning all players.
        /// </summary>
        public static void UpdateBeamFor(PhysGrabber grabber, PhysGrabObject obj)
        {
            if (obj == null)
                return;

            var beamGO = grabber.GetBeamObject();
            var rend = beamGO?.GetComponentInChildren<Renderer>();
            if (rend == null)
                return;

            bool hasMassData = obj.TryGetComponent<Rigidbody>(out var rb);
            float mass = hasMassData ? rb.mass : -1f;

            Color baseColor;

            if (!hasMassData || mass <= 0f)
            {
                baseColor = Plugin.MediumColor.Value;
                Plugin.Log.LogWarning($"[DynamicRepoGrabBeam] Applying fallback color for '{obj.name}' grabbed by '{grabber.name}' (missing Rigidbody)");
            }
            else
            {
                baseColor = Color.white;
                foreach (var strat in allStrategies)
                {
                    if (strat is ForceEffectStrategy) break;
                    if (strat.Applies(grabber, obj))
                        baseColor = strat.Modify(baseColor, grabber, obj);
                }
            }

            Color.RGBToHSV(baseColor, out float h, out float s, out float v);

            var context = new BeamContext(
                obj, rend, h, s, v,
                Plugin.MaxStunVisualForce.Value,
                Plugin.ForceSaturationScale.Value,
                Plugin.ForceBrightnessScale.Value,
                Plugin.ForceAlphaScale.Value,
                baseColor
            );

            float impact = 0f;
            if (obj.TryGetComponent<PhysGrabObjectImpactDetector>(out var det))
                impact = ForceEffectStrategy.ComputeImpact(obj);

            Color final = Color.HSVToRGB(h, s, v);
            foreach (var strat in allStrategies)
                final = strat.ModifyFast(final, grabber, obj, impact, context);

            ApplyBeamColor(rend, final);
        }          
    }
}
