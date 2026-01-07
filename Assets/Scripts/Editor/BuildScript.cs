using UnityEditor;
using UnityEngine;
using System;
using System.IO;

/// <summary>
/// Unity Editor build script for MDPro3
/// Contains methods to build the game for different platforms
/// </summary>
public class BuildScript
{
    /// <summary>
    /// Build for macOS platform
    /// This method is called from command line with -executeMethod BuildScript.BuildMac
    /// </summary>
    [MenuItem("Build/Build Mac")]
    public static void BuildMac()
    {
        Debug.Log("Starting macOS build...");

        // Get build options from command line or use defaults
        string buildPath = GetCommandLineArgValue("-buildPath", "build/StandaloneOSX/MDPro3.app");
        string buildTarget = GetCommandLineArgValue("-buildTarget", "StandaloneOSX");

        // Ensure build directory exists
        string buildDir = Path.GetDirectoryName(buildPath);
        if (!string.IsNullOrEmpty(buildDir) && !Directory.Exists(buildDir))
        {
            Directory.CreateDirectory(buildDir);
            Debug.Log($"Created build directory: {buildDir}");
        }

        // Get all scenes in build settings
        string[] scenes = GetEnabledScenes();
        
        if (scenes.Length == 0)
        {
            Debug.LogError("No scenes found in build settings! Please add scenes to build.");
            EditorApplication.Exit(1);
            return;
        }

        Debug.Log($"Building {scenes.Length} scenes:");
        foreach (string scene in scenes)
        {
            Debug.Log($"  - {scene}");
        }

        // Configure build options
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = buildPath,
            target = BuildTarget.StandaloneOSX,
            options = BuildOptions.None
        };

        // Check if this is a development build
        if (HasCommandLineArg("-development"))
        {
            buildPlayerOptions.options |= BuildOptions.Development;
            Debug.Log("Development build enabled");
        }

        // Check if autoconnect profiler is requested
        if (HasCommandLineArg("-autoconnectprofiler"))
        {
            buildPlayerOptions.options |= BuildOptions.ConnectWithProfiler;
            Debug.Log("Autoconnect profiler enabled");
        }

        // Perform the build
        Debug.Log($"Building to: {buildPath}");
        Debug.Log($"Target platform: {buildPlayerOptions.target}");

        try
        {
            var report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            
            if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
            {
                Debug.Log($"Build succeeded! Size: {report.summary.totalSize} bytes");
                Debug.Log($"Build time: {report.summary.totalTime}");
                Debug.Log($"Output: {buildPath}");
                EditorApplication.Exit(0);
            }
            else
            {
                Debug.LogError($"Build failed! Result: {report.summary.result}");
                Debug.LogError($"Errors: {report.summary.totalErrors}");
                Debug.LogError($"Warnings: {report.summary.totalWarnings}");
                EditorApplication.Exit(1);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Build exception: {e.Message}");
            Debug.LogError(e.StackTrace);
            EditorApplication.Exit(1);
        }
    }

    /// <summary>
    /// Get all enabled scenes from build settings
    /// </summary>
    private static string[] GetEnabledScenes()
    {
        var scenes = new System.Collections.Generic.List<string>();
        
        foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
        {
            if (scene.enabled)
            {
                scenes.Add(scene.path);
            }
        }
        
        return scenes.ToArray();
    }

    /// <summary>
    /// Check if a command line argument exists
    /// </summary>
    private static bool HasCommandLineArg(string name)
    {
        string[] args = Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == name)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Get value of a command line argument
    /// </summary>
    private static string GetCommandLineArgValue(string name, string defaultValue = "")
    {
        string[] args = Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == name && i + 1 < args.Length)
            {
                return args[i + 1];
            }
        }
        return defaultValue;
    }
}
