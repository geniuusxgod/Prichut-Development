#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

// Zostavenie hry z menu aj z prikazoveho riadku (batchmode -executeMethod BuildCommands.WebGL).
public static class BuildCommands
{
    private static readonly string[] Scenes = { "Assets/Scenes/Main.unity" };

    [MenuItem("Resort Empire/Build/WebGL")]
    public static void WebGL()
    {
        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;
        PlayerSettings.WebGL.decompressionFallback = false;
        Build(BuildTarget.WebGL, "Builds/WebGL");
    }

    private static void Build(BuildTarget target, string path)
    {
        var options = new BuildPlayerOptions
        {
            scenes = Scenes,
            target = target,
            locationPathName = path,
            options = BuildOptions.None
        };
        BuildReport report = BuildPipeline.BuildPlayer(options);
        Debug.Log("Build " + target + ": " + report.summary.result + " size " + report.summary.totalSize);
        if (Application.isBatchMode && report.summary.result != BuildResult.Succeeded) EditorApplication.Exit(1);
    }
}
#endif
