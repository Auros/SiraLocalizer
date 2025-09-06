using System.IO;
using UnityEditor;
using UnityEngine;

public class BuildFontsAssetBundle
{
    [MenuItem("Assets/Export Asset Bundle", priority = 1100)]
    public static void BuildAssetBundle()
    {
        string resourcesPath = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "..", "..", "SiraLocalizer", "Resources"));
        string targetPath = EditorUtility.SaveFilePanel("Export Font Asset Bundle", resourcesPath, "Assets", string.Empty);

        if (string.IsNullOrEmpty(targetPath))
        {
            return;
        }

        AssetBundleBuild assetBundleBuild = new()
        {
            assetBundleName = "SiraLocalizerAssets",
            assetNames = Directory.GetFiles(Path.Combine("Assets", "Text Assets"), "*.asset", SearchOption.TopDirectoryOnly),
        };

        AssetBundleManifest manifest = BuildPipeline.BuildAssetBundles(Application.temporaryCachePath, new AssetBundleBuild[] { assetBundleBuild }, BuildAssetBundleOptions.ForceRebuildAssetBundle, BuildTarget.StandaloneWindows64);

        if (manifest == null)
        {
            EditorUtility.DisplayDialog("Failed to build asset bundle!", "Failed to build asset bundle! Check the console for details.", "OK");
            return;
        }

        string fileName = manifest.GetAllAssetBundles()[0];
        File.Copy(Path.Combine(Application.temporaryCachePath, fileName), targetPath, true);

        EditorUtility.DisplayDialog("Export Successful!", "Asset bundle exported successfully!", "OK");
    }
}
