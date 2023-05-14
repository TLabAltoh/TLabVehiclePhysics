using System.Collections.Generic;
using System.IO;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

#if UNITY_EDITOR
public class TLabAssetBundleBuilder : Editor
{
    static string root_path = "AssetBundle";
    static string variant = "assetbundl";

    [MenuItem("Assets/Build AssetBundles/WebGL")]
    static void BuildAllAssetBundlesWebGL()
    {
        BuildAllAssetBundles(UnityEditor.BuildTarget.WebGL);
    }

    [MenuItem("Assets/Build AssetBundles/Win")]
    static void BuildAllAssetBundlesWin()
    {
        BuildAllAssetBundles(UnityEditor.BuildTarget.StandaloneWindows);
    }

    static void BuildAllAssetBundles(BuildTarget targetPlatform)
    {
        Debug.Log("Start Build Asset Bundle");

        var outputPath = System.IO.Path.Combine(root_path, targetPlatform.ToString());
        if (!Directory.Exists(outputPath))
        {
            Directory.CreateDirectory(outputPath);
        }

        var assetBundleBuildList = new List<UnityEditor.AssetBundleBuild>();
        foreach (string assetBundleName in UnityEditor.AssetDatabase.GetAllAssetBundleNames())
        {
            var builder = new AssetBundleBuild();
            builder.assetBundleName = assetBundleName;
            builder.assetNames = UnityEditor.AssetDatabase.GetAssetPathsFromAssetBundle(builder.assetBundleName);
            builder.assetBundleVariant = variant;
            assetBundleBuildList.Add(builder);
        }

        if (assetBundleBuildList.Count > 0)
        {
            UnityEditor.BuildPipeline.BuildAssetBundles(outputPath, assetBundleBuildList.ToArray(), UnityEditor.BuildAssetBundleOptions.ChunkBasedCompression, targetPlatform);
            Debug.Log("Asset Bundle Builded: " + outputPath);
        }

        Debug.Log("Finish Build Asset Bundle");
    }
}
#endif
