using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

/// <summary>
/// Injects the VisionOSSkipPresent constant into the generated Xcode project.
/// Unity's VisionOS package references this constant in UnitySwiftUIAppDelegate.swift
/// but does not define it — this script adds the definition automatically after each build.
/// Set to 'true' for PolySpatial/Mixed Reality mode, 'false' for VR/Metal mode.
/// </summary>
public static class VisionOSPostBuildProcessor
{
    private const bool SkipPresent = true; // true = PolySpatial MR mode

    private const string ConstantDefinition =
        "// Auto-injected by VisionOSPostBuildProcessor\n" +
        "import Foundation\n" +
        "let VisionOSSkipPresent: NSNumber = " + (SkipPresent ? "true" : "false") + "\n\n";

    [PostProcessBuild(1)]
    public static void OnPostProcessBuild(BuildTarget target, string buildPath)
    {
        if (target != BuildTarget.VisionOS)
            return;

        string swiftFile = Path.Combine(buildPath, "UnitySwiftUIAppDelegate.swift");

        if (!File.Exists(swiftFile))
        {
            // Search one level deeper (Xcode project subfolder)
            string[] candidates = Directory.GetFiles(buildPath, "UnitySwiftUIAppDelegate.swift", SearchOption.AllDirectories);
            if (candidates.Length == 0)
            {
                Debug.LogWarning("[VisionOSPostBuildProcessor] Could not find UnitySwiftUIAppDelegate.swift in build output. " +
                                 "You may need to manually define VisionOSSkipPresent in Xcode.");
                return;
            }
            swiftFile = candidates[0];
        }

        string contents = File.ReadAllText(swiftFile);

        if (contents.Contains("VisionOSSkipPresent"))
        {
            Debug.Log("[VisionOSPostBuildProcessor] VisionOSSkipPresent already defined, skipping.");
            return;
        }

        File.WriteAllText(swiftFile, ConstantDefinition + contents);
        Debug.Log($"[VisionOSPostBuildProcessor] Injected VisionOSSkipPresent into {swiftFile}");
    }
}
