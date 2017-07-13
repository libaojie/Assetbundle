
using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.IO;

public class ResourceManager : MonoBehaviour 
{

	private string MainBundle = "AssetBundles";
	private string BundleSuffix = ".bundle";
	private string PersistentDataPath = "";
	private string StreamingAssetsPath = "";

	private List<string> allBundleNames = null;
	private AssetBundleManifest assetBundleManifest = null;

	/// <summary>
	/// 加载对象字典
	/// </summary>
	private Dictionary<string, Dictionary<string, UnityEngine.Object>> objectMaps = null;

	/// <summary>
	/// 依赖项
	/// </summary>
	private Dictionary<string, AssetBundle> dependedBundle = null;

	//单例
	private static ResourceManager instance = null;
	public static ResourceManager Instance
	{
		get
		{
			return instance;
		}
	}

	/// <summary>
	/// 是否使用Bundle加载
	/// </summary>
	public bool UseBundle = true;

	

	/// <summary>
	/// 初始化
	/// </summary>
	public void Init()
	{
		instance = this;

		StreamingAssetsPath =
		#if UNITY_EDITOR
		 Application.streamingAssetsPath + "/" + MainBundle + "/";
		#elif UNITY_IPHONE
						Application.streamingAssetsPath + "/" + MainBundle + "/";
		#elif UNITY_ANDROID
						Application.dataPath + "!assets/" + MainBundle + "/";
		#elif UNITY_STANDALONE_WIN
						Application.streamingAssetsPath + "/" + MainBundle + "/";
		#else
						string.Empty;
		#endif

		PersistentDataPath = Application.persistentDataPath + "/" + MainBundle + "/";

		allBundleNames = new List<string>();
		assetBundleManifest = null;
		objectMaps = new Dictionary<string, Dictionary<string, UnityEngine.Object>>();
		dependedBundle = new Dictionary<string, AssetBundle>();

		InitAllBundleNames();

	}

	public void Release()
	{
		instance = null;
	}

	#region 公共接口

	/// <summary>
	/// 
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="name"></param>
	/// <returns></returns>
	public T GetObjectDirectly<T>(string name) where T : UnityEngine.Object
	{
		name = name.ToLower();
		T reObject = null;
		Type type = typeof(T);
		UnityEngine.Object obj;

		// 是否已经加载过
		if (GetValue(name, type, out obj))
		{
			if (obj != null)
			{
				reObject = obj as T;
			}
		}

		if (reObject != null)
		{
			return reObject;
		}

		if (UseBundle)
		{
			// 从Bundle中加载
			reObject = (T)GetDirectlyFromBundle(name, type);
		}
		
		if (reObject == null)
		{
			// 从Resource中加载
			reObject = Resources.Load<T>(name) as T;
		}


		if (reObject != null)
		{
			SetValue(name, type, reObject);
			return reObject;
		}

		return null;
	}
	#endregion

	#region 对象字典 操作
	/// <summary>
	/// 字典中取相应对象
	/// </summary>
	/// <param name="name"></param>
	/// <param name="type"></param>
	/// <param name="obj"></param>
	/// <returns></returns>
	private bool GetValue(string name, Type type, out UnityEngine.Object obj)
	{
		obj = null;
		Dictionary<string, UnityEngine.Object> objectMap;
		if (objectMaps.TryGetValue(name, out objectMap))
		{
			if (objectMap.TryGetValue(type.FullName, out obj))
			{
				return true;
			}
		}

		return false;
	}

	/// <summary>
	/// 将对象放入字典中
	/// </summary>
	/// <param name="name"></param>
	/// <param name="type"></param>
	/// <param name="obj"></param>
	private void SetValue(string name, Type type, UnityEngine.Object obj)
	{
		Dictionary<string, UnityEngine.Object> objectMap = null;
		if (objectMaps.ContainsKey(name))
		{
			objectMap = objectMaps[name];
		}

		if (objectMap == null)
		{
			objectMap = new Dictionary<string, UnityEngine.Object>();
		}

		objectMap[type.FullName] = obj;
		objectMaps[name] = objectMap;
	}

	/// <summary>
	/// 清理
	/// </summary>
	private	void ClearValue()
	{
		var itr = objectMaps.Values.GetEnumerator();
		while (itr.MoveNext())
		{
			itr.Current.Clear();
		}
		objectMaps.Clear();
	}

	#endregion

	#region BundleName相关

	/// <summary>
	/// 初始化AllBundleName
	/// </summary>
	private void InitAllBundleNames()
	{
		if (!UseBundle)
		{
			return;
		}

		AssetBundle bundle = GetAssetBundle(MainBundle);

		if (bundle != null)
		{
			assetBundleManifest = (AssetBundleManifest)bundle.LoadAsset("AssetBundleManifest");
			bundle.Unload(false);
		}

		if (assetBundleManifest != null)
		{
			string[] bundleNames = assetBundleManifest.GetAllAssetBundles();
			for (int i = 0; i < bundleNames.Length; i++)
			{
				if (string.IsNullOrEmpty(bundleNames[i]))
					continue;

				allBundleNames.Add(bundleNames[i]);
			}
		}
	}

	#endregion

	#region 加载bundle相关

	/// <summary>
	/// bundle中加载name
	/// </summary>
	/// <param name="name"></param>
	/// <param name="type"></param>
	/// <returns></returns>
	private UnityEngine.Object GetDirectlyFromBundle(string name, Type type)
	{
		string bundleName = name;
		string shortName = name.Substring(name.LastIndexOf("/") + 1);

		if (!bundleName.EndsWith(BundleSuffix))
		{
			bundleName += BundleSuffix;
		}

		if (!allBundleNames.Contains(bundleName))
			return null;

		// 加载依赖项
		if (assetBundleManifest != null)
		{
			string[] dps = assetBundleManifest.GetAllDependencies(bundleName.ToLower());
			for (int i = 0; i < dps.Length; i++)
			{
				if (string.IsNullOrEmpty(dps[i]))
					continue;

				if (dependedBundle.ContainsKey(dps[i]) && dependedBundle[dps[i]] != null)
				{
					continue;
				}

				AssetBundle dBundle = GetAssetBundle(dps[i]);
				dependedBundle[dps[i]] = dBundle;
			}
		}

		// 加载本体
		AssetBundle ab = null;
		if (dependedBundle.ContainsKey(bundleName) && dependedBundle[bundleName] != null) // 属于bundle已被加载
		{
			ab = dependedBundle[bundleName];
		}
		else
		{
			ab = GetAssetBundle(bundleName);
			dependedBundle[bundleName] = ab;
		}

		if (ab == null)
		{
			return null;
		}

		UnityEngine.Object gobj;
		gobj = ab.LoadAsset(name, type) as UnityEngine.Object;

		return gobj;
	}

	/// <summary>
	/// 得到单纯的AssetBundle
	/// </summary>
	/// <param name="name"></param>
	/// <returns></returns>
	private AssetBundle GetAssetBundle(string name)
	{
		//PersistentDataPath加载
		AssetBundle dBundle = GetAssetBundle(name, PersistentDataPath);

		if (dBundle == null)
		{
			// StreamingAssetsPath加载
			dBundle = GetAssetBundle(name, StreamingAssetsPath);
		}

		if (dBundle == null)
		{

		}

		return dBundle;
	}

	/// <summary>
	/// 得到单纯的AssetBundle
	/// </summary>
	/// <param name="name"></param>
	/// <returns></returns>
	private AssetBundle GetAssetBundle(string name, string path)
	{
		if (name.EndsWith("AssetBundles") == false && name.EndsWith(".bundle") == false)
		{
			name += ".bundle";
		}
		string url = path + name;

		if (path == PersistentDataPath && !File.Exists(url))
		{
			return null;
		}

		AssetBundle bundle = AssetBundle.LoadFromFile(url);
		return bundle;
	}
	

	#endregion
}
