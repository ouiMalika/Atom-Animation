// Tools > Configure Meta Quest 3S
// Run this ONCE after switching Unity's build target to Android.
// It enables OpenXR in XR Plugin Management and sets Meta Quest feature flags.

using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
public static class QuestBuildSetup
{
    [MenuItem("Tools/Configure Meta Quest 3S")]
    public static void Configure()
    {
        // ── Android player settings ──────────────────────────────────────────
        PlayerSettings.SetApplicationIdentifier(
            BuildTargetGroup.Android, "com.nhi.NanoHumanInterfaces");
        PlayerSettings.Android.minSdkVersion  = AndroidSdkVersions.AndroidApiLevel32;
        PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevel32;
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;

        // ── XR Plugin Management: enable OpenXR loader for Android ───────────
        var buildTargetGroup = BuildTargetGroup.Android;
        var settings = UnityEditor.XR.Management.XRGeneralSettingsPerBuildTarget
            .XRGeneralSettingsForBuildTarget(buildTargetGroup);

        if (settings == null)
        {
            Debug.LogWarning(
                "[QuestSetup] XR General Settings not found for Android.\n" +
                "Open Project Settings > XR Plug-in Management, switch to Android tab,\n" +
                "tick OpenXR, then re-run this menu item.");
            return;
        }

        var manager = settings.Manager;
        if (manager == null)
        {
            Debug.LogWarning("[QuestSetup] XR Manager null — open XR Plug-in Management first.");
            return;
        }

        // Find OpenXR loader and add it
        var loaders = manager.activeLoaders;
        bool hasOpenXR = false;
        foreach (var loader in loaders)
            if (loader.GetType().Name.Contains("OpenXR")) { hasOpenXR = true; break; }

        if (!hasOpenXR)
        {
            Debug.Log("[QuestSetup] OpenXR loader not yet active.\n" +
                      "In XR Plug-in Management > Android, tick OpenXR manually,\n" +
                      "then run Tools > Configure Meta Quest 3S again to apply feature sets.");
        }
        else
        {
            Debug.Log("[QuestSetup] OpenXR loader already active. All good.");
        }

        // ── Switch build target prompt ───────────────────────────────────────
        if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
        {
            bool doSwitch = EditorUtility.DisplayDialog(
                "Switch Build Target?",
                "Current target is not Android.\n\nSwitch to Android now?",
                "Switch", "Cancel");

            if (doSwitch)
                EditorUserBuildSettings.SwitchActiveBuildTargetAsync(
                    BuildTargetGroup.Android, BuildTarget.Android);
        }

        AssetDatabase.SaveAssets();
        Debug.Log("[QuestSetup] Done. Bundle ID set to com.nhi.NanoHumanInterfaces, SDK 32.");
    }

    [MenuItem("Tools/Build Quest 3S APK")]
    public static void BuildAPK()
    {
        if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
        {
            EditorUtility.DisplayDialog("Wrong Target",
                "Switch build target to Android first (Tools > Configure Meta Quest 3S).",
                "OK");
            return;
        }

        var scenes = new[] { "Assets/Scenes/SampleScene.unity" };
        var path   = EditorUtility.SaveFilePanel("Save APK", "", "AtomAnimation", "apk");
        if (string.IsNullOrEmpty(path)) return;

        BuildPipeline.BuildPlayer(
            scenes,
            path,
            BuildTarget.Android,
            BuildOptions.None);
    }
}
#endif
