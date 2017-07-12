using UnityEngine;
using System.Collections;
using System;
using ICSharpCode.SharpZipLib.Zip;
using System.IO;
using ICSharpCode.SharpZipLib.Checksums;

public class ZipTool 
{
	protected static string dirRootName = "";
	protected static int zipLevel = 0;
	protected static DateTime time = new DateTime(2016, 1, 1);
	#region 压缩

	/// <summary>   
	/// 递归压缩文件夹的内部方法   
	/// </summary>   
	/// <param name="folderToZip">要压缩的文件夹路径</param>   
	/// <param name="zipStream">压缩输出流</param>   
	/// <param name="parentFolderName">此文件夹的上级文件夹</param>   
	/// <returns></returns>   
	private static bool ZipDirectory(string folderToZip, ZipOutputStream zipStream, string parentFolderName)
	{
		bool result = true;
		string[] folders, files;
		ZipEntry ent = null;
		FileStream fs = null;
		Crc32 crc = new Crc32();

		if (string.IsNullOrEmpty(parentFolderName))
		{
			dirRootName = Path.GetFileName(folderToZip);
		}

		try
		{
			//ent = new ZipEntry(Path.Combine(parentFolderName, Path.GetFileName(folderToZip) + "/"));
			//zipStream.PutNextEntry(ent);
			//zipStream.Flush();

			files = Directory.GetFiles(folderToZip);
			foreach (string file in files)
			{
				if (file.EndsWith(".manifest"))
					continue;

				fs = File.OpenRead(file);

				byte[] buffer = new byte[fs.Length];
				fs.Read(buffer, 0, buffer.Length);
				ent = new ZipEntry(Path.Combine(parentFolderName, Path.GetFileName(folderToZip) + "/" + Path.GetFileName(file)));
				ent.DateTime = time;
				ent.Size = fs.Length;

				fs.Close();

				crc.Reset();
				crc.Update(buffer);

				ent.Crc = crc.Value;
				zipStream.PutNextEntry(ent);
				zipStream.Write(buffer, 0, buffer.Length);
			}

		}
		catch
		{
			result = false;
		}
		finally
		{
			if (fs != null)
			{
				fs.Close();
				fs.Dispose();
			}
			if (ent != null)
			{
				ent = null;
			}
			GC.Collect();
			GC.Collect(1);
		}

		folders = Directory.GetDirectories(folderToZip);
		int start = folderToZip.IndexOf(dirRootName);
		if (start == -1)
		{
			return false;
		}
		string folderParentName = folderToZip.Substring(start);
		foreach (string folder in folders)
		{
			if (!ZipDirectory(folder, zipStream, folderParentName))
			{
				return false;
			}
		}

		return result;
	}

	/// <summary>   
	/// 压缩文件夹    
	/// </summary>   
	/// <param name="folderToZip">要压缩的文件夹路径</param>   
	/// <param name="zipedFile">压缩文件完整路径</param>   
	/// <param name="password">密码</param>   
	/// <returns>是否压缩成功</returns>   
	public static bool ZipDirectory(string folderToZip, string zipedFile, string password)
	{
		bool result = false;
		if (!Directory.Exists(folderToZip))
			return result;

		ZipOutputStream zipStream = new ZipOutputStream(File.Create(zipedFile));
		zipStream.SetLevel(zipLevel);
		if (!string.IsNullOrEmpty(password)) zipStream.Password = password;

		result = ZipDirectory(folderToZip, zipStream, "");

		zipStream.Finish();
		zipStream.Close();

		return result;
	}

	/// <summary>   
	/// 压缩文件夹   
	/// </summary>   
	/// <param name="folderToZip">要压缩的文件夹路径</param>   
	/// <param name="zipedFile">压缩文件完整路径</param>   
	/// <returns>是否压缩成功</returns>   
	public static bool ZipDirectory(string folderToZip, string zipedFile)
	{
		bool result = ZipDirectory(folderToZip, zipedFile, null);
		return result;
	}

	/// <summary>
	/// 压缩多文件
	/// </summary>
	/// <param name="filesToZip">要压缩的文件名列表</param>
	/// <param name="zipedFile">压缩文件完整路径</param>
	/// <param name="password">密码</param>
	/// <returns></returns>
	public static bool ZipFiles(string[] filesToZip, string zipedFile, string password)
	{
		bool result = true;
		ZipOutputStream zipStream = null;
		ZipEntry ent = null;
		FileStream fs = null;
		Crc32 crc = new Crc32();

		try
		{
			zipStream = new ZipOutputStream(File.Create(zipedFile));
			zipStream.SetLevel(zipLevel);
			if (!string.IsNullOrEmpty(password)) zipStream.Password = password;
			foreach (string file in filesToZip)
			{
				if (!File.Exists(file))
					return false;

				fs = File.OpenRead(file);
				byte[] buffer = new byte[fs.Length];
				fs.Read(buffer, 0, buffer.Length);
				ent = new ZipEntry(Path.GetFileName(file));
				ent.DateTime = time;
				ent.Size = fs.Length;

				fs.Close();

				crc.Reset();
				crc.Update(buffer);

				ent.Crc = crc.Value;
				zipStream.PutNextEntry(ent);
				zipStream.Write(buffer, 0, buffer.Length);
			}
		}
		catch
		{
			result = false;
		}
		finally
		{
			if (zipStream != null)
			{
				zipStream.Finish();
				zipStream.Close();
			}
			if (fs != null)
			{
				fs.Close();
				fs.Dispose();
			}
			if (ent != null)
			{
				ent = null;
			}
		}
		GC.Collect();
		GC.Collect(1);

		return result;
	}

	/// <summary>
	/// 压缩多文件
	/// </summary>
	/// <param name="filesToZip">要压缩的文件名列表</param>
	/// <param name="zipedFile">压缩后文件名</param>
	/// <returns></returns>
	public static bool ZipFiles(string[] filesToZip, string zipedFile)
	{
		bool result = ZipFiles(filesToZip, zipedFile, null);
		return result;
	}

	/// <summary>   
	/// 压缩文件   
	/// </summary>   
	/// <param name="fileToZip">要压缩的文件全名</param>   
	/// <param name="zipedFile">压缩后的文件名</param>   
	/// <param name="password">密码</param>   
	/// <returns>压缩结果</returns>   
	public static bool ZipFile(string fileToZip, string zipedFile, string password)
	{
		bool result = true;
		ZipOutputStream zipStream = null;
		FileStream fs = null;
		ZipEntry ent = null;

		if (!File.Exists(fileToZip))
			return false;

		try
		{
			zipStream = new ZipOutputStream(File.Create(zipedFile));
			zipStream.SetLevel(zipLevel);
			if (!string.IsNullOrEmpty(password)) zipStream.Password = password;

			fs = File.OpenRead(fileToZip);
			byte[] buffer = new byte[fs.Length];
			fs.Read(buffer, 0, buffer.Length);
			ent = new ZipEntry(Path.GetFileName(fileToZip));
			ent.DateTime = time;
			ent.Size = fs.Length;

			fs.Close();

			Crc32 crc = new Crc32();
			crc.Update(buffer);

			ent.Crc = crc.Value;
			zipStream.PutNextEntry(ent);
			zipStream.Write(buffer, 0, buffer.Length);
		}
		catch
		{
			result = false;
		}
		finally
		{
			if (zipStream != null)
			{
				zipStream.Finish();
				zipStream.Close();
			}
			if (fs != null)
			{
				fs.Close();
				fs.Dispose();
			}
			if (ent != null)
			{
				ent = null;
			}
		}
		GC.Collect();
		GC.Collect(1);

		return result;
	}

	/// <summary>   
	/// 压缩文件   
	/// </summary>   
	/// <param name="fileToZip">要压缩的文件全名</param>   
	/// <param name="zipedFile">压缩后的文件名</param>   
	/// <returns>压缩结果</returns>   
	public static bool ZipFile(string fileToZip, string zipedFile)
	{
		bool result = ZipFile(fileToZip, zipedFile, null);
		return result;
	}

	/// <summary>   
	/// 压缩文件或文件夹   
	/// </summary>   
	/// <param name="fileToZip">要压缩的路径</param>   
	/// <param name="zipedFile">压缩后的文件名</param>   
	/// <param name="password">密码</param>   
	/// <returns>压缩结果</returns>   
	public static bool Zip(string fileToZip, string zipedFile, string password)
	{
		bool result = false;
		if (Directory.Exists(fileToZip))
			result = ZipDirectory(fileToZip, zipedFile, password);
		else if (File.Exists(fileToZip))
			result = ZipFile(fileToZip, zipedFile, password);

		return result;
	}

	/// <summary>   
	/// 压缩文件或文件夹   
	/// </summary>   
	/// <param name="fileToZip">要压缩的路径</param>   
	/// <param name="zipedFile">压缩后的文件名</param>   
	/// <returns>压缩结果</returns>   
	public static bool Zip(string fileToZip, string zipedFile)
	{
		bool result = Zip(fileToZip, zipedFile, null);
		return result;

	}

	#endregion

	#region 解压

	/// <summary>   
	/// 解压功能(解压压缩文件到指定目录)   
	/// </summary>   
	/// <param name="fileToUnZip">待解压的文件</param>   
	/// <param name="zipedFolder">指定解压目标目录</param>   
	/// <param name="password">密码</param>   
	/// <returns>解压结果</returns>   
	public static bool UnZip(string fileToUnZip, string zipedFolder, string password)
	{
		bool result = true;
		FileStream fs = null;
		ZipInputStream zipStream = null;
		ZipEntry ent = null;
		Crc32 crc = new Crc32();
		string fileName;

		if (!File.Exists(fileToUnZip))
			return false;

		if (!Directory.Exists(zipedFolder))
			Directory.CreateDirectory(zipedFolder);

		try
		{
			zipStream = new ZipInputStream(File.OpenRead(fileToUnZip));
			if (!string.IsNullOrEmpty(password)) zipStream.Password = password;
			while ((ent = zipStream.GetNextEntry()) != null)
			{
				if (!string.IsNullOrEmpty(ent.Name))
				{
					fileName = Path.Combine(zipedFolder, ent.Name);
					fileName = fileName.Replace('\\', '/');
					string dirName = fileName.Remove(fileName.LastIndexOf("/"));

					if (!Directory.Exists(dirName))
					{
						Directory.CreateDirectory(dirName);
					}

					fs = File.Create(fileName);
					long size = 2048;
					if (ent.Size > 0)
					{
						size = ent.Size;
					}
					byte[] data = new byte[size];
					while (true)
					{
						size = zipStream.Read(data, 0, data.Length);
						if (size > 0)
						{
							crc.Reset();
							crc.Update(data);
							if (ent.Crc > 0 && ent.Crc != crc.Value)
								return false;
							fs.Write(data, 0, data.Length);
							fs.Close();
						}
						else
							break;
					}
				}
			}
		}
		catch
		{
			result = false;
		}
		finally
		{
			if (fs != null)
			{
				fs.Close();
				fs.Dispose();
			}
			if (zipStream != null)
			{
				zipStream.Close();
				zipStream.Dispose();
			}
			if (ent != null)
			{
				ent = null;
			}
			GC.Collect();
			GC.Collect(1);
		}
		return result;
	}

	/// <summary>   
	/// 解压功能(解压压缩文件到指定目录)   
	/// </summary>   
	/// <param name="fileToUnZip">待解压的文件</param>   
	/// <param name="zipedFolder">指定解压目标目录</param>   
	/// <returns>解压结果</returns>   
	public static bool UnZip(string fileToUnZip, string zipedFolder)
	{
		bool result = UnZip(fileToUnZip, zipedFolder, null);
		return result;
	}

	#endregion  

}
