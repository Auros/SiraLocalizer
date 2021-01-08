using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

public class BuildFontsAssetBundle : MonoBehaviour
{
    [MenuItem("Assets/Export Font Asset Bundle...", false, 20)]
    public static void BuildAssetBundle()
    {
        string path = EditorUtility.SaveFilePanel("Export Font Asset Bundle", "", "fonts.assets", "assets");

        if (string.IsNullOrEmpty(path)) return;
        
        string fileName = Path.GetFileName(path);
        string folderPath = Path.GetDirectoryName(path);

        AssetBundleBuild assetBundleBuild = new AssetBundleBuild {
            assetBundleName = fileName,
            assetNames = Directory.GetFiles(Path.Combine("Assets", "Text Assets"), "*.asset", SearchOption.TopDirectoryOnly)
        };

        BuildTargetGroup selectedBuildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
        BuildTarget activeBuildTarget = EditorUserBuildSettings.activeBuildTarget;

        AssetBundleManifest manifest = BuildPipeline.BuildAssetBundles(Application.temporaryCachePath, new[] { assetBundleBuild }, BuildAssetBundleOptions.None, EditorUserBuildSettings.activeBuildTarget);

        if (manifest == null)
        {
            EditorUtility.DisplayDialog("Export Failed", "Failed to create asset bundle! Please check the Unity console for more information.", "OK");
            return;
        }

        // switch back to what it was before creating the asset bundle
        EditorUserBuildSettings.SwitchActiveBuildTarget(selectedBuildTargetGroup, activeBuildTarget);

        File.Copy(Application.temporaryCachePath + "/" + fileName, path, true);

        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Export Successful!", "Export Successful!", "OK");
    }
}
