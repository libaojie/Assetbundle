using UnityEngine;
using System.Collections;

public class Normal : MonoBehaviour 
{
	public string resourceName = "";

	// Use this for initialization
	void Start () 
	{
		ResourceManager.Instance.Init();

	}


	void OnGUI()
	{
		if (GUILayout.Button("Main Assetbundle"))
		{
			GameObject go = GameObject.Instantiate(ResourceManager.Instance.GetObjectDirectly<UnityEngine.Object>(resourceName)) as GameObject;
		}



	}


}
