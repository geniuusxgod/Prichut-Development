#if UNITY_EDITOR
using UnityEditor;

// Pri prvom otvoreni projektu nastavi hru na sirku (landscape) a nazov hry.
[InitializeOnLoad]
public static class ProjectSetup
{
    static ProjectSetup()
    {
        EditorApplication.delayCall += Apply;
    }

    private static void Apply()
    {
        PlayerSettings.productName = "Resort Empire";
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.AutoRotation;
        PlayerSettings.allowedAutorotateToPortrait = false;
        PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
        PlayerSettings.allowedAutorotateToLandscapeLeft = true;
        PlayerSettings.allowedAutorotateToLandscapeRight = true;
    }
}
#endif
