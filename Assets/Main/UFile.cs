using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

#if UNITY_EDITOR || UNITY_IOS || UNITY_STANDALONE || UNITY_ANDROID
public class UFile
{
	private static readonly string STREAMMING_PATH = Application.streamingAssetsPath;
	private static readonly string PERSISTENT_PATH = Application.persistentDataPath;


	public static void Copy(string sourceFileName, string destFileName)
	{
#if UNITY_ANDROID
		if(CustomStartsWith(destFileName, PERSISTENT_PATH))
		{
			byte[] buffer = ReadAllBytes(sourceFileName);
			WriteAllBytes(destFileName, buffer);
		}
#else
		File.Copy(sourceFileName, destFileName, true);
#endif
	}


	public static void Delete(string path)
	{
#if UNITY_ANDROID
		if(CustomStartsWith(path, PERSISTENT_PATH))
		{
			File.Delete(path);
		}
#else
		File.Delete(path);
#endif
	}

	public static bool Exists(string path)
	{
#if UNITY_ANDROID
		if(CustomStartsWith(path, PERSISTENT_PATH))
		{
			return File.Exists(path);
		}

		return false;
#else
		return File.Exists(path);
#endif
	}

	public static string ReadAllText(string path)
	{
#if UNITY_ANDROID
		return ReadAllText(path, Encoding.UTF8);
#else
		return File.ReadAllText(path);
#endif
	}

	public static string ReadAllText(string path, Encoding encoding)
	{
#if UNITY_ANDROID
		UnityWebRequest request = UnityWebRequest.Get(path);
		while (!request.isDone) { }
		return request.downloadHandler.text;
#else
		return File.ReadAllText(path, encoding);
#endif
	}

	public static byte[] ReadAllBytes(string path)
	{
#if UNITY_ANDROID
		UnityWebRequest request = UnityWebRequest.Get(path);
		while(!request.isDone) { }
		return request.downloadHandler.data;
#else
		return File.ReadAllBytes(path);
#endif
	}
	public static void WriteAllBytes(string path, byte[] bytes)
	{
#if UNITY_ANDROID
		if(CustomStartsWith(path, PERSISTENT_PATH))
		{
			File.WriteAllBytes(path, bytes);
		}
		
#else
		File.WriteAllBytes(path, bytes);
#endif
	}

	public static void Move(string sourceFileName, string destFileName)
	{
#if UNITY_ANDROID
		Copy(sourceFileName, destFileName);
		Delete(sourceFileName);
#else
		File.Move(sourceFileName, destFileName);
#endif
	}

	/// <summary>
	/// 提高string.StartsWith的性能
	/// </summary>
	private static bool CustomStartsWith(string str, string value)
	{
		int aLen = str.Length;
		int bLen = value.Length;

		int ap = 0; int bp = 0;
		while (ap < aLen && bp < bLen && str[ap] == value[bp]) { ap++; bp++; }

		return (bp == bLen);
	}
}
#endif
