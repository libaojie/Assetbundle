using UnityEngine;
using System.Collections;

public class Download : MonoBehaviour {

	public string baseURL = "ftp://192.168.0.173/CXL";
	public string name = "AssetBundles_v1.0.zip";

	DownloadManager downloadManager; 

	// Use this for initialization
	void Start () 
	{
		downloadManager = gameObject.AddComponent<DownloadManager>();
		downloadManager.Init();
	}
	


	void OnGUI()
	{
		if (GUILayout.Button("Download Assetbundle"))
		{
			StartCoroutine(downloadManager.DownLoadAssetBundle(name, baseURL));
		}
	}
}
