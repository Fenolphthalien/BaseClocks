using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public static class ExportAssetBundles
{
	[MenuItem("Assets/Export Asset Bundles")]
	static void Export()
	{
		BuildPipeline.BuildAssetBundles ("AssetBundles",BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows);
	}
}
