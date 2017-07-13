using UnityEngine;
using System.Collections;

public class Normal : MonoBehaviour 
{
	public string resourceName = "UI/Cube1";
	private ResourceManager resourceManager;
	// Use this for initialization
	void Start () 
	{
		resourceManager = gameObject.AddComponent<ResourceManager>();
		resourceManager.Init();
	}


	void OnGUI()
	{
		if (GUILayout.Button("Main Assetbundle"))
		{
			Object ob = ResourceManager.Instance.GetObjectDirectly<UnityEngine.Object>(resourceName);
			GameObject go = GameObject.Instantiate(ob) as GameObject;
			if (go != null)
			{
			
			}
			else
			{
				Debug.Log("GameObject == null");
			}
		}



	}

	void OnDestory()
	{
		if (resourceManager != null)
		{
			resourceManager.Release();
		}
	}


}
