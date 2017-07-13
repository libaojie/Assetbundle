using UnityEngine;
using System.Collections;
using System.IO;
using System;

/// <summary>
/// 下载管理器
/// </summary>
public class DownloadManager : MonoBehaviour 
{
	private string tempPath = Application.streamingAssetsPath + "/Temp";


	private static DownloadManager instance = null;
	public static DownloadManager Instance
	{
		get
		{
			return instance;
		}
	}

	/// <summary>
	/// 初始化函数
	/// </summary>
	public void Init()
	{
		instance = this;
	}

	/// <summary>
	/// 释放函数
	/// </summary>
	public void Release()
	{
		instance = null;
	}

	/// <summary>
	/// 下载AssetBundle
	/// </summary>
	/// <param name="name"></param>
	/// <param name="baseUrl"></param>
	/// <returns></returns>
	public IEnumerator DownLoadAssetBundle(string name, string baseUrl)
	{
		string realName = baseUrl + "/"+ name;

		using (WWW www = new WWW(realName))
		{
			www.threadPriority = ThreadPriority.High;
			while(!www.isDone)
			{
				yield return www.progress;
				ShowLog("Download... "+ www.progress);
			}

			if (www.error == null)
			{
				// 下载成功
				ShowLog("Download Successfully ");
				CreateBundleFile(name, www.bytes);
			}
			else
			{
				ShowLog("Download Fail " + www.error);
			}
		}
	}


	private void CreateBundleFile(string name, byte[] info)
	{
		/*
		Stream sw;
		string[] dir = name.Split('/');
		string dirpath = tempPath;
		if (dir.Length > 1)
		{
			for (int i = 0; i < dir.Length - 1; i++)
			{
				dirpath += "/" + dir[i];
				if (!Directory.Exists(dirpath))
				{
					Directory.CreateDirectory(dirpath);
				}
			}
		}

		FileInfo file = new FileInfo(tempPath + "//" + name);
		if (!file.Exists)
		{
			sw = file.
		}
		else
		{
			sw = file.OpenWrite();
		}
		try
		{
			sw.Write(info, 0, info.Length);
			sw.Close();
			sw.Dispose();
		}
		catch (Exception e)
		{
		}*/

		string fileName = tempPath + "/" + name;
		string[] dir = fileName.Split('/');
		string dirpath = "";
		if (dir.Length > 1)
		{
			for (int i = 0; i < dir.Length - 1; i++)
			{
				if (i == 0)
				{
					dirpath = dir[i];
				}
				else
				{
					dirpath += "/" + dir[i];
				}
				
				if (!Directory.Exists(dirpath))
				{
					Directory.CreateDirectory(dirpath);
				}
			}
		}

		FileStream pFileStream = null;
		try
		{
			pFileStream = new FileStream(fileName, FileMode.OpenOrCreate);
			pFileStream.Write(info, 0, info.Length);
		}
		catch
		{
		}
		finally
		{
			if (pFileStream != null)
				pFileStream.Close();
		}
		
	}

	private void ShowLog(string log)
	{
		Debug.Log(Time.time+"	"+ log);
	}
}
