using UnityEngine;
using System.Collections;

public class Main : MonoBehaviour 
{
	//不同平台下StreamingAssets的路径是不同的，这里需要注意一下。
	public static readonly string PathURL =
#if UNITY_ANDROID
		"jar:file://" + Application.dataPath + "!/assets/";
#elif UNITY_IPHONE
		Application.dataPath + "/Raw/";
#elif UNITY_STANDALONE_WIN || UNITY_EDITOR
 "file://" + Application.dataPath + "/StreamingAssets/";
#else
        string.Empty;
#endif

	void OnGUI()
	{
		if (GUILayout.Button("Main Assetbundle"))
		{
			StartCoroutine(LoadMainGameObject(PathURL + "Cube1.assetbundle"));
			StartCoroutine(LoadMainGameObject(PathURL + "Cube2.assetbundle"));
		}

		if (GUILayout.Button("ALL Assetbundle"))
		{
			StartCoroutine(LoadALLGameObject(PathURL + "ALL.assetbundle"));
		}

	}

	//读取一个资源

	private IEnumerator LoadMainGameObject(string path)
	{
		WWW bundle = new WWW(path);

		yield return bundle;

		//加载到游戏中
		yield return Instantiate(bundle.assetBundle.mainAsset);

		bundle.assetBundle.Unload(false);
	}

	//读取全部资源

	private IEnumerator LoadALLGameObject(string path)
	{
		WWW bundle = new WWW(path);

		yield return bundle;

		//通过Prefab的名称把他们都读取出来
		Object obj0 = bundle.assetBundle.LoadAsset("Cube1");
		Object obj1 = bundle.assetBundle.LoadAsset("Cube2");

		//加载到游戏中	
		yield return Instantiate(obj0);
		yield return Instantiate(obj1);
		bundle.assetBundle.Unload(false);
	}
}
