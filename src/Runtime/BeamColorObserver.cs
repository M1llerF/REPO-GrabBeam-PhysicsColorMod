using UnityEngine;
using System.Collections.Generic;
using DynamicRepoGrabBeam.Patches;
using DynamicRepoGrabBeam.ColorLogic;

/// <summary>
/// Continuously monitors active PhysGrabbers and triggers beam recoloring
/// when a new object is grabbed or dropped.
/// </summary>
public class BeamColorObserver : MonoBehaviour
{
    // Tracks the last known grabbed object for each grabber
    private readonly Dictionary<PhysGrabber, int> lastGrabs = new();

    // Cached list of all PhysGrabbers in the scene
    private readonly List<PhysGrabber> cachedGrabbers = new();

    private float updateInterval = 0.2f;
    private float timer;

    private float rescanInterval = 10f;
    private float rescanTimer;

    /// <summary>
    /// Called when the observer is first created. Initializes grabber cache.
    /// </summary>
    void Awake()
    {
        RescanGrabbers();
    }

    /// <summary>
    /// Runs every frame. Detects grab changes and triggers beam color updates.
    /// </summary>
    void Update()
    {
        timer -= Time.deltaTime;
        rescanTimer -= Time.deltaTime;

        if (rescanTimer <= 0f)
        {
            RescanGrabbers();
            rescanTimer = rescanInterval;
        }

        if (timer > 0f) return;
        timer = updateInterval;

        foreach (var grabber in cachedGrabbers)
        {
            if (grabber == null) continue; // Handles disconnected players

            var obj = grabber.GetGrabbedObject();
            if (obj == null)
                continue; // Skip disconnected or destroyed players

            bool hasRigidbody = obj.TryGetComponent<Rigidbody>(out var rb);

            if (!hasRigidbody)
            {
                Plugin.Log.LogWarning($"[DynamicRepoGrabBeam] Delaying recolor: Rigidbody missing for object '{obj.name}' (grabber '{grabber.name}')");
                continue;
            }

            int objId = obj.GetInstanceID();
            if (lastGrabs.TryGetValue(grabber, out int prevObjId) && prevObjId == objId)
                continue; // No change detected

            lastGrabs[grabber] = objId;
            BeamColorManager.UpdateBeamFor(grabber, obj);
        }
    }

    /// <summary>
    /// Refreshes the cached list of all active PhysGrabbers in the scene.
    /// </summary>
    private void RescanGrabbers()
    {
        cachedGrabbers.Clear();
        cachedGrabbers.AddRange(FindObjectsOfType<PhysGrabber>());
    }
}
