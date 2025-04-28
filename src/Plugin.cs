using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using DynamicRepoGrabBeam.ColorLogic;

/// <summary>
/// Main entry point for the Grab Beam Mod.
/// Handles configuration loading, initialization, and Harmony patching.
/// </summary>
[BepInPlugin("com.repo.DynamicRepoGrabBeam", "Grab Beam Mod", "1.0.0")]
public class Plugin : BaseUnityPlugin
{
    // --- Global access ---
    public static Plugin Instance;
    public static ManualLogSource Log => Instance?.Logger;

    // --- Weight thresholds ---
    public static ConfigEntry<float> LightMax;
    public static ConfigEntry<float> HeavyMin;

    // --- Beam base colors ---
    public static ConfigEntry<Color> LightColor;
    public static ConfigEntry<Color> MediumColor;
    public static ConfigEntry<Color> HeavyColor;
    public static ConfigEntry<Color> PlayerGrabColor;
    public static ConfigEntry<Color> RotationModeColor;


    // --- Force escalation logic (saturation and alpha control) ---
    public static ConfigEntry<float> MaxStunVisualForce;
    public static ConfigEntry<float> ForceSaturationScale;
    public static ConfigEntry<float> ForceBrightnessScale;
    public static ConfigEntry<float> ForceAlphaScale;

    // --- Debugging ---
    public static ConfigEntry<bool> EnableDebugLogs;

    private void Awake()
    {
        Instance = this;

        // Initialize beam color manager (observer setup, caching, etc.)
        BeamColorManager.Initialize();

        // --- Configuration binding ---

        // Weight thresholds (mass in kilograms)
        LightMax = Config.Bind("Weight Thresholds", "LightMax", 2.0f, "Max mass for light objects");
        HeavyMin = Config.Bind("Weight Thresholds", "HeavyMin", 3.0f, "Min mass for heavy objects");

        // Beam base colors with alpha = 0.5f (50% opacity)
        LightColor = Config.Bind("Colors", "LightWeight",
            new Color(0f, 1f, 0f, 0.5f), "Beam color for light objects (50% opacity)");
        MediumColor = Config.Bind("Colors", "MediumWeight",
            new Color(1f, 1f, 0f, 0.5f), "Beam color for medium objects (50% opacity)");
        HeavyColor = Config.Bind("Colors", "HeavyWeight",
            new Color(1f, 0f, 0f, 0.5f), "Beam color for heavy objects (50% opacity)");

        PlayerGrabColor = Config.Bind("Colors", "PlayerGrabBeam",
            new Color(0f, 1f, 1f, 0.5f), "Beam color when grabbing another player (50% opacity)");
        RotationModeColor = Config.Bind("Colors", "RotationModeBeam",
            new Color(1f, 0.5f, 1f, 0.5f), "Beam color override while rotating (used instead of default purple)");
        
        // Force escalation behavior
        MaxStunVisualForce = Config.Bind("Force", "MaxStunVisualForce", 15f,
            "Force value that causes full saturation intensity");
        ForceSaturationScale = Config.Bind(
            "Force", "SaturationScale", 0.2f,
            "How much saturation increases with normalized impact force"
        );
        ForceBrightnessScale = Config.Bind(
            "Force", "BrightnessScale", 0.03f,
            "How much brightness increases with normalized impact force"
        );
        ForceAlphaScale = Config.Bind(
            "Force", "AlphaScale", 0.3f,
            "How much transparency (alpha) increases with normalized impact force"
        );

        // Debugging options
        EnableDebugLogs = Config.Bind("Debug", "EnableDebugLogs", false,
            "Enable force debug output");

        
        if (EnableDebugLogs.Value)
            Logger.LogInfo("Debug logging is ENABLED.");

        // Apply all Harmony patches
        new Harmony("com.repo.DynamicRepoGrabBeam").PatchAll();

        Logger.LogInfo("Grab Beam Mod v1.0.0 initialized successfully");
    }
}
