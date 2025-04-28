using UnityEngine;

namespace DynamicRepoGrabBeam.ColorLogic
{
    /// <summary>
    /// Interface for defining beam color modification strategies.
    /// Strategies can adjust the beam color based on different conditions,
    /// such as object mass, player grabs, impact force, or rotation.
    /// </summary>
    public interface IBeamColorStrategy
    {
        /// <summary>
        /// Determines whether this strategy should apply to the given grabbed object.
        /// </summary>
        bool Applies(PhysGrabber grabber, PhysGrabObject obj);

        /// <summary>
        /// Modifies the current beam color using a non-optimized, general-purpose path.
        /// Typically used during initial recoloring.
        /// </summary>
        Color Modify(Color current, PhysGrabber grabber, PhysGrabObject obj);

        /// <summary>
        /// Modifies the beam color using a fast path with additional contextual data.
        /// Typically used during continuous updates for dynamic effects.
        /// </summary>
        Color ModifyFast(
            Color current,
            PhysGrabber grabber,
            PhysGrabObject obj,
            float impact,
            BeamContext ctx
        );
    }
}
