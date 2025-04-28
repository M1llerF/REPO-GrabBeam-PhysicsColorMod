using System;
using System.Linq.Expressions;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace DynamicRepoGrabBeam.Patches
{
    /// <summary>
    /// Provides fast accessor methods for internal fields of PhysGrabber and PhysGrabObject.
    /// Avoids repeated reflection lookups during runtime.
    /// </summary>
    public static class PhysGrabberAccessors
    {
        private static readonly FieldInfo grabbedObjField =
            AccessTools.Field(typeof(PhysGrabber), "grabbedPhysGrabObject");

        private static readonly FieldInfo beamField =
            AccessTools.Field(typeof(PhysGrabber), "physGrabBeam");

        private static readonly FieldInfo isPlayerField =
            AccessTools.Field(typeof(PhysGrabObject), "isPlayer");

        private static readonly Func<PhysGrabObjectImpactDetector, float> BreakForceGetter =
            CreateFieldGetter<float>("breakForce");

        private static readonly Func<PhysGrabObjectImpactDetector, float> FragilityGetter =
            CreateFieldGetter<float>("impactFragilityMultiplier");

        // Static initializer to validate delegate creation
        static PhysGrabberAccessors()
        {
            if (BreakForceGetter == null)
                Debug.LogError("[DynamicRepoGrabBeam] BreakForceGetter delegate failed!");

            if (FragilityGetter == null)
                Debug.LogError("[DynamicRepoGrabBeam] FragilityGetter delegate failed!");
        }

        /// <summary>
        /// Determines if the given PhysGrabObject represents a player.
        /// </summary>
        public static bool IsPlayer(this PhysGrabObject obj)
            => isPlayerField != null && (bool)isPlayerField.GetValue(obj);

        /// <summary>
        /// Retrieves the currently grabbed object from a PhysGrabber instance.
        /// </summary>
        public static PhysGrabObject GetGrabbedObject(this PhysGrabber instance)
            => grabbedObjField.GetValue(instance) as PhysGrabObject;

        /// <summary>
        /// Retrieves the grab beam GameObject from a PhysGrabber instance.
        /// </summary>
        public static GameObject GetBeamObject(this PhysGrabber instance)
            => beamField.GetValue(instance) as GameObject;

        /// <summary>
        /// Retrieves the break force value from a PhysGrabObjectImpactDetector instance.
        /// </summary>
        public static float GetBreakForce(this PhysGrabObjectImpactDetector det)
            => BreakForceGetter(det);

        /// <summary>
        /// Retrieves the fragility multiplier from a PhysGrabObjectImpactDetector instance.
        /// </summary>
        public static float GetFragility(this PhysGrabObjectImpactDetector det)
            => FragilityGetter(det);

        /// <summary>
        /// Dynamically generates a fast field accessor delegate for the given field name.
        /// Used for performance-sensitive access to private fields.
        /// </summary>
        private static Func<PhysGrabObjectImpactDetector, T> CreateFieldGetter<T>(string fieldName)
        {
            try
            {
                var param = Expression.Parameter(typeof(PhysGrabObjectImpactDetector), "target");
                var field = Expression.Field(param, fieldName);
                var lambda = Expression.Lambda<Func<PhysGrabObjectImpactDetector, T>>(field, param);
                return lambda.Compile();
            }
            catch (Exception e)
            {
                Debug.LogError($"[DynamicRepoGrabBeam] Failed to create delegate for '{fieldName}': {e.Message}");
                return _ => default;
            }
        }
    }
}
