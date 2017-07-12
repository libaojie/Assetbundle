using UnityEngine;
using System.Collections;
using System.IO;
using System;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// 平台管理
/// </summary>
public class AssetPlatform 
{
	/// <summary>
	/// 资源目录
	/// </summary>
	protected static string ResourcesName = "Assets/Resources/";

	/// <summary>
	/// Bundle后缀
	/// </summary>
	protected static string BundleSuffix = ".bundle";

	/// <summary>
	/// StreamingAssetsPath
	/// </summary>
	public static readonly string StreamingAssetsPath =
	#if UNITY_ANDROID
			"jar:file://" + Application.dataPath + "!/assets/";
	#elif UNITY_IPHONE
			Application.dataPath + "/Raw/";
	#elif UNITY_STANDALONE_WIN || UNITY_EDITOR
			"file://" + Application.dataPath + "/StreamingAssets/";
	#else
			string.Empty;
	#endif

	/// <summary>
	/// 资源版本号
	/// </summary>
	protected static string ResourceVersion = "";

	/// <summary>
	/// 资源文件夹结构
	/// </summary>
	protected static string[] ResourceDir = new string[]
	{
		"UI",
		"Data",
		"Effect"
	};


	/// <summary>
	/// 临时Bundle文件夹
	/// </summary>
	protected static string tempBundlePath = "";

	protected static string platformDetail = "";


	#region 打包函数

	/// <summary>
	/// 打包过程
	/// </summary>
	public virtual void Process()
	{
		if (PreBuild() == false)
		{
			return;
		}

		if (Build() == false)
		{
			return;
		}

		if (AfterBuild() == false)
		{
			return;
		}

		if (PostBuild() == false)
		{
			return;
		}

		EditorUtility.DisplayDialog("Build Success!", platformDetail, "ok");
	}

	protected virtual bool PreBuild()
	{
		// 新建临时Bundle文件夹
		tempBundlePath = Application.dataPath + "/AssetBundles";
		MakeDirectoryExist(tempBundlePath);
		ResourceVersion = "v1.0";
		return true;
	}

	protected virtual bool Build() { return true; }
	protected virtual bool AfterBuild() { return true; }
	protected virtual bool PostBuild() { return true; }

	#endregion

	#region 工具函数
	/// <summary>
	/// 确定文件夹存在
	/// </summary>
	/// <param name="name"></param>
	protected static void MakeDirectoryExist(string name)
	{
		if (Directory.Exists(name) == false)
		{
			Directory.CreateDirectory(name);
		}
	}

	/// <summary>
	/// 压缩文件夹
	/// </summary>
	/// <param name="path"></param>
	/// <param name="zipedFileName"></param>
	protected static void ZipDir(string path, string zipedFileName)
	{
		if (File.Exists(zipedFileName))
		{
			File.Delete(zipedFileName);
		}

		if (!ZipTool.Zip(path, zipedFileName))
		{
			throw new ArgumentException("Zip files failed! zipedFileName: " + zipedFileName);
		}
	}

	protected static AssetBundleBuild[] GetBundleMap(string[] content)
	{
		string[] path = new string[content.Length];
		for (int i = 0; i < content.Length; i++)
		{
			path[i] = ResourcesName + content[i];
		}

		List<string> files = new List<string>();
		var result = AssetDatabase.FindAssets(null, path);
		foreach (var file in result)
		{
			string fileName = AssetDatabase.GUIDToAssetPath(file);
			if (!files.Contains(fileName))
			{
				files.Add(fileName);
			}
		}


		AssetBundleBuild[] buildMap = new AssetBundleBuild[files.Count];
		for (int i = 0; i < files.Count; i++)
		{
			string[] assets1 = new string[1];
			assets1[0] = files[i];
			buildMap[i].assetNames = assets1;
			string bundleName = files[i].Replace(ResourcesName, "");
			string dirName = Path.GetDirectoryName(bundleName);
			if (dirName.Length != 0)
			{
				dirName += "/";
			}
			bundleName = dirName + Path.GetFileNameWithoutExtension(bundleName) + BundleSuffix;
			buildMap[i].assetBundleName = bundleName;
		}

		return buildMap;
	}

	#endregion

}

/// <summary>
/// 编辑器打包
/// </summary>
public class AssetEditor : AssetPlatform
{
	protected override bool Build()
	{
		platformDetail = "Editor Resource Bundle";
		AssetBundleBuild[] buildMap1 = AssetPlatform.GetBundleMap(AssetPlatform.ResourceDir);
		AssetBundleBuild[] buildMap = new AssetBundleBuild[buildMap1.Length];
		buildMap1.CopyTo(buildMap, 0);
		BuildPipeline.BuildAssetBundles(tempBundlePath, buildMap, BuildAssetBundleOptions.ChunkBasedCompression, BuildTarget.StandaloneWindows);

		return true;
	}
	protected override bool AfterBuild()
	{
		return true;
	}
	protected override bool PostBuild()
	{

		string zipedFileName = tempBundlePath + "_" + ResourceVersion + ".zip";
		ZipDir(tempBundlePath, zipedFileName);
		return true;
	}
}

