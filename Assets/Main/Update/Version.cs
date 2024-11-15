using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine.Networking;
using System.Collections.Generic;
using UnityEngine;

public static class Version
{
	public enum UpdateState
	{
		GetRemoteVersion, GetRemoteVersionFail,
		GetRemoteRes, GetRemoteResFail,
		CheckRes,
		Download, DownloadFail,
		CanEnterGame
	};

	private const string VERSIONFILENAME = "version.data";
	private const string MD5FILENAME = "res_manifest.csv";

	private static readonly string DownloadFolder = string.Empty;
	private static readonly string TmpVersionFilePath = string.Empty;
	private static readonly string TmpMD5FilePath = string.Empty;

	private static Action<string> refreshLocalVersionAction = null;
	private static Action<string> refreshRemoteVersionAction = null;
	private static Action<UpdateState, int> refreshProgressValueAction = null;

	static Version()
	{
		DownloadFolder = $"{Application.persistentDataPath}/download";
		if (!Directory.Exists(DownloadFolder)) Directory.CreateDirectory(DownloadFolder);

		// 版控临时文件
		TmpVersionFilePath = Path.Combine(DownloadFolder, VERSIONFILENAME);
		// md5临时文件
		TmpMD5FilePath = Path.Combine(DownloadFolder, MD5FILENAME);
	}

	#region 整个热更、下载流程
	public static async Task CheckUpdate(Action<string> refreshLocalVersionAction,
										 Action<string> refreshRemoteVersionAction,
										 Action<UpdateState, int> refreshProgressValueAction)
	{
		Version.refreshLocalVersionAction = refreshLocalVersionAction;
		Version.refreshRemoteVersionAction = refreshRemoteVersionAction;
		Version.refreshProgressValueAction = refreshProgressValueAction;

		UpdateLog.Info($"[00] 检查网络状态：{Application.internetReachability}");
		if (Application.internetReachability == NetworkReachability.NotReachable)
		{
			UpdateLog.Info("[00] 没有网络连接");
			UpdateDialog.ShowDialog("没有网络连接重新尝试", async () =>
			{
				await Task.Delay(2);
				await CheckUpdate(refreshLocalVersionAction, refreshRemoteVersionAction, refreshProgressValueAction);
			});
			return;
		}


		UpdateLog.Info("[01] 开始检查更新流程");
		refreshProgressValueAction(UpdateState.GetRemoteVersion, 0);
		VersionInfo localVersionInfo = await GetLocalVersionInfo();
		int localSmall = localVersionInfo.smallVersion;
		int localBig = localVersionInfo.bigVersion;
		string str_local_version = $"{localBig}.{localSmall}";
		refreshProgressValueAction(UpdateState.GetRemoteVersion, 10);
		refreshLocalVersionAction(str_local_version);


		UpdateLog.Info($"[01] 获取本地版本号：{str_local_version}");
		if (GameConfig.PlayMode == PlayMode.OfflineMode)
		{
			UpdateLog.Info("[01] 离线模式，无需更新，进入游戏");
			refreshRemoteVersionAction("offline");
			refreshProgressValueAction(UpdateState.CanEnterGame, 100);
			return;
		}

		UpdateLog.Info($"[02] 开始获取远程版本号");
		var remoteVersionReq = await GetRemoteVersion();
		if (remoteVersionReq == null)
		{
			UpdateDialog.ShowDialog("获取远程版本失败,请重新尝试", async () =>
			{
				await CheckUpdate(refreshLocalVersionAction, refreshRemoteVersionAction, refreshProgressValueAction);
			},
			() =>
			{
#if UNITY_EDITOR
				UnityEditor.EditorApplication.isPlaying = false;
#else
				Application.Quit();
#endif
			});
			refreshProgressValueAction(UpdateState.GetRemoteVersionFail, 20);
			return;
		}

		byte[] datas = remoteVersionReq.downloadHandler.data;
		VersionInfo remoteVersionInfo = JsonUtility.FromJson<VersionInfo>(System.Text.Encoding.UTF8.GetString(datas));
		int remoteSmall = remoteVersionInfo.smallVersion;
		int remoteBig = remoteVersionInfo.bigVersion;

		string str_remote_version = $"{remoteBig}.{remoteSmall}";
		refreshRemoteVersionAction(str_remote_version);
		refreshProgressValueAction(UpdateState.GetRemoteVersion, 20);
		UpdateLog.Info($"[02] 获取远程版本号：{str_remote_version}");

		// 强更，需要更新安装包
		if (localBig < remoteBig)
		{
			UpdateLog.Info("[02] 请下载最新安装包");
			UpdateDialog.ShowDialog("检测到最新安装包,请到应用商店进行下载", () => { Application.OpenURL("https://blog.tonychenn.cn"); });
			return;
		}

#if UNITY_IOS  // Apple提审中              
		string versionName = Application.version;
		if (versionName == remoteVersionInfo.appleExamVersion)
		{
			Log.Info("[02] iOS提审模式，无需更新，进入游戏");
			refreshRemoteVersionAction("apple exam");
			refreshProgressValueAction(UpdateState.CanEnterGame, 100);
			return;
		}
#endif


		if (localSmall >= remoteSmall && !NeedFixClient)
		{
			DeleteTmpDownloadFolder();
			UpdateLog.Info("[02] 版本号一致新进入游戏: 100%");
			refreshProgressValueAction(UpdateState.CanEnterGame, 100);
			return;
		}


		UpdateLog.Info("[03] 开始获取远程资源列表: 25%");
		refreshProgressValueAction(UpdateState.GetRemoteRes, 25);
		string md5Name = remoteVersionInfo.md5;
		var success = await DownloaRemoteMD5File(md5Name);

		if (!success)
		{
			refreshProgressValueAction(UpdateState.GetRemoteResFail, 50);

			UpdateDialog.ShowDialog("获取远程资源清单失败,请重新尝试", async () =>
			{
				await CheckUpdate(refreshLocalVersionAction, refreshRemoteVersionAction, refreshProgressValueAction);
			},
			() =>
			{
#if UNITY_EDITOR
				UnityEditor.EditorApplication.isPlaying = false;
#else
				Application.Quit();
#endif
			});
			return;
		}
		// 版本号变更, 资源无变化
		refreshProgressValueAction(UpdateState.CheckRes, 50);
		string tmp_md5 = await getFileMD5(TmpMD5FilePath);
		UpdateLog.Info($"[03] 远程资源列表MD5: {tmp_md5}\t55%");
		refreshProgressValueAction(UpdateState.CheckRes, 55);
		string local_md5 = await getFileMD5(PathUtil.GetBundlePath(MD5FILENAME, true));
		UpdateLog.Info($"[03] 本地资源列表MD5: {local_md5}\t60%");
		refreshProgressValueAction(UpdateState.CheckRes, 60);

		if (tmp_md5 == local_md5 && !NeedFixClient)
		{
			UpdateLog.Info("[04] 版本号变更，资源无化，进入游戏 100%");
			refreshProgressValueAction(UpdateState.CanEnterGame, 95);
			await SaveVersionFile();
			DeleteTmpDownloadFolder();
			refreshProgressValueAction(UpdateState.CanEnterGame, 100);
			return;
		}

		UpdateLog.Info("[05] 开始对比变更资源\t75%");
		refreshProgressValueAction(UpdateState.CheckRes, 75);
		var modifyList = new List<ResManifest.ResUnit>(32);
		var delList = new List<ResManifest.ResUnit>(0);
		ulong downloadSize = 0;

		CalNeedUpdateInfo(ref modifyList, ref delList, ref downloadSize, NeedFixClient);
		UpdateLog.Info($"[05] 变动_新增：{modifyList.Count}, 删除：{delList.Count}, 下载大小{GetByteLengthString(downloadSize)}");
		refreshProgressValueAction(UpdateState.CheckRes, 99);


		// 没有资源变动，无需下载
		if (modifyList.Count == 0 && delList.Count == 0)
		{
			UpdateLog.Info("[05] 客户端资源无变动，进入游戏\t100%");
			await SaveVersionFile();
			DeleteTmpDownloadFolder();
			refreshProgressValueAction(UpdateState.CanEnterGame, 100);
			return;
		}

		// wifi连接直接开始下载
		if (Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork && false)
		{
			UpdateLog.Info("[06] wifi 网络直接下载");
			await DownloadUpdateBundleAsync(modifyList, delList, downloadSize);
			NeedFixClient = false;
			return;
		}

		string _content = string.Format("检测到更新{0},是否使用流量进行下载", GetByteLengthString(downloadSize));
		UpdateDialog.ShowDialog(_content,
							async () => { await DownloadUpdateBundleAsync(modifyList, delList, downloadSize); NeedFixClient = false; },
							() => Application.Quit());
	}
	#endregion


	#region 获取远程、本地版本号
	private static async Task<VersionInfo> GetLocalVersionInfo()
	{
		string path = PathUtil.GetBundlePath(VERSIONFILENAME, true);
		string info = await readFileAllText(path);
		UpdateLog.Info($"[01] 读取LocalVersionInfo: {info}");
		VersionInfo result = JsonUtility.FromJson<VersionInfo>(info);
		return result;
	}

	private static async Task<UnityWebRequest> GetRemoteVersion()
	{
		string remoteUrl = PathUtil.GetBundleCDNUrl(VERSIONFILENAME, true);
		Debug.Log($"[UnityWebRequest] >>> <color=green>{remoteUrl}</color>");
		UnityWebRequest request = UnityWebRequest.Get($"{remoteUrl}?{DateTime.Now}");
		await request.SendWebRequest();
		if (request.error != null)
		{
			Debug.Log($"[UnityWebRequest] <<< <color=red>{request.error}</color>");
			return null;
		}

		if (request.result == UnityWebRequest.Result.Success)
		{
			byte[] data = request.downloadHandler.data;
			var fs = new FileStream(TmpVersionFilePath, FileMode.Create);
			await fs.WriteAsync(data, 0, data.Length);
			fs.Close();
			Debug.Log("[UnityWebRequest] <<< <color=green>success</color>");
			return request;
		}
		else
		{
			Debug.Log($"[UnityWebRequest] <<< <color=red>{request.result.ToString()}</color>");
			return null;
		}
	}
	#endregion


	#region 下载远程MD5文件
	private static async Task<bool> DownloaRemoteMD5File(string md5)
	{
		string url = $"{ChannelConfig.CurCDNUrl}/{md5}.csv";
		UnityWebRequest request = UnityWebRequest.Get(url);
		await request.SendWebRequest();

		if (request.result != UnityWebRequest.Result.Success)
		{
			Debug.Log($"[UnityWebRequest] <<< <color=red>{request.error}</color>");
			return false;
		}

		byte[] data = request.downloadHandler.data;
		var fs = new FileStream(TmpMD5FilePath, FileMode.Create);
		await fs.WriteAsync(data, 0, data.Length);
		fs.Close();

		Debug.Log("[UnityWebRequest] <<< <color=green>success</color>");
		return true;
	}
	#endregion

	/// <summary>
	/// 计算需要下载、删除的资源信息
	/// </summary>
	/// <param name="modifyList">add,modify列表</param>
	/// <param name="delList">delete列表</param>
	/// <param name="downloadSize">需要下载的文件大小</param>
	private static void CalNeedUpdateInfo(ref List<ResManifest.ResUnit> modifyList, ref List<ResManifest.ResUnit> delList,
								   ref ulong downloadSize, bool checkLocalAsset = false)
	{
		// 解析本地资源MD5数据
		var localDict = new Dictionary<string, ResManifest.ResUnit>(512);
		ResManifest.ParseResManifest(PathUtil.GetBundlePath(MD5FILENAME, true), localDict);


		// 解析tmp数据(保存一份解析)
		ResManifest.ResManifestDict = new Dictionary<string, ResManifest.ResUnit>(512);
		ResManifest.ParseResManifest(TmpMD5FilePath, ResManifest.ResManifestDict);
		var tmpDict = ResManifest.ResManifestDict;



		// 计算add,motify列表
		foreach (var item in tmpDict)
		{
			if (!localDict.ContainsKey(item.Key))
			{
				modifyList.Add(item.Value);
				downloadSize += ulong.Parse(item.Value.size);
			}
			else
			{
				var localItem = localDict[item.Key];
				if (item.Value.md5 != localItem.md5)
				{
					modifyList.Add(item.Value);
					downloadSize += ulong.Parse(item.Value.size);
				}
			}
		}

		// 剔除已下载过的文件
		for (int i = modifyList.Count - 1; i >= 0; i--)
		{
			var item = modifyList[i];
			var path = PathUtil.GetBundlePersistPath(item.bundleName);
			if (!File.Exists(path)) continue;

			Debug.Log("------------------->");
			Debug.Log("开启子线程计算MD5值");
			
			string realMD5 = Task.Run(() => getFileMD5(path)).Result;
			Debug.Log(realMD5+" ----- "+item.md5);
			Debug.Log("<------------------");
			if (realMD5 == item.md5)
			{
				downloadSize -= ulong.Parse(item.size);
				modifyList.Remove(item);
			}
		}

		// 检测本地文件是否丢失(修复客户端)
		if (checkLocalAsset)
		{
			foreach (var item in tmpDict)
			{
				string path = PathUtil.GetBundlePath(item.Key);
				string localMD5 = Task.Run(() => getFileMD5(path)).Result;
				if (localMD5 != item.Value.md5)
				{
					modifyList.Add(item.Value);
				}
			}
		}

		// 计算出要删除的文件
		foreach (var item in localDict)
		{
			if (!tmpDict.ContainsKey(item.Key))
				delList.Add(item.Value);
		}

		localDict.Clear();
	}


	#region 下载更新资源
	/// <summary>
	/// 下载AssetBundle
	/// </summary>
	/// <param name="modifyList"></param>
	private static async Task DownloadUpdateBundleAsync(List<ResManifest.ResUnit> modifyList, List<ResManifest.ResUnit> deleteList, ulong downloadSize)
	{
		await BundleDownloader.Singleton.DownloadAsync(modifyList, (percent) =>
		{
			refreshProgressValueAction(UpdateState.Download, percent);
		},
		async () =>
		{
			UpdateLog.Info("[07] 下载更新资源完成");
			DeleteUselessBundle(deleteList);
			UpdateLog.Info($"[07] 删除无用资源完成：{deleteList.Count}");

			SaveMd5File();
			await SaveVersionFile();
			refreshProgressValueAction(UpdateState.Download, 100);

			UpdateLog.Info("[10] 更新完成，进入游戏(无需重启)");
			refreshProgressValueAction(UpdateState.CanEnterGame, 100);
		});
	}

	// 删除无用资源(Bundle)
	private static void DeleteUselessBundle(List<ResManifest.ResUnit> deleteList)
	{
		UpdateLog.Info("[07] 删除无用资源");
		for (int i = 0, iMax = deleteList.Count; i < iMax; i++)
		{
			var item = deleteList[i];
			string path = PathUtil.GetBundlePersistPath(item.bundleName);
			if (File.Exists(path))
			{
				File.Delete(path);
				UpdateLog.RedInfo("[Update delete] > " + item.bundleName);
			}
		}
	}
	#endregion


	/// <summary>
	/// 删除版控文件/清单文件，修复客户端
	/// </summary>
	public static void FixClient()
	{
		var path = $"{Application.persistentDataPath}/{VERSIONFILENAME}";
		if (File.Exists(path)) { File.Delete(path); }

		var md5_path = $"{Application.persistentDataPath}/{MD5FILENAME}";
		if (File.Exists(md5_path)) { File.Delete(md5_path); }
	}
	// 将download/version.data 复制到version.data
	private static async Task SaveVersionFile()
	{
		try
		{
			File.Copy(TmpVersionFilePath, $"{Application.persistentDataPath}/{VERSIONFILENAME}", true);

			VersionInfo v = await GetLocalVersionInfo();
			refreshLocalVersionAction($"{v.bigVersion}.{v.smallVersion}");
		}
		catch (Exception ex)
		{
			UpdateLog.Error(string.Format("移动 version.data 失败: {0}", ex.Message));
		}
	}

	private static void SaveMd5File()
	{
		try
		{
			var path = $"{Application.persistentDataPath}/{MD5FILENAME}";
			File.Copy(TmpMD5FilePath, path, true);
		}
		catch (Exception ex)
		{
			UpdateLog.Error(string.Format("移动 res_manifest 失败: {0}", ex.Message));
		}
	}

	/// <summary>
	/// 清理临时下载目录
	/// </summary>
	private static void DeleteTmpDownloadFolder()
	{
		if (Directory.Exists(DownloadFolder))
		{
			Directory.Delete(DownloadFolder, true);
		}
	}

	#region
	/// <summary>
	///本地记录是否需要修复客户端
	/// </summary>
	public static bool NeedFixClient
	{
		get { return PlayerPrefs.GetInt("NEED_FIX_CLIENT", 0) == 1; }
		set { PlayerPrefs.SetInt("NEED_FIX_CLIENT", value ? 1 : 0); }
	}
	private static string GetByteLengthString(ulong length)
	{
		if (length < 1024)
			return $"{length} B";
		else if (length < 1048576)
			return $"{(length / 1024f):F2}KB";
		else if (length < 1073741824)
			return $"{(length / 1048576f):F2}MB";
		else
			return $"{(length / 1073741824f):F2} GB";
	}

	private static async Task<string> getFileMD5(string filePath)
	{
		byte[] buffer = await readFileAllBytes(filePath);
		if (buffer == null || buffer.Length < 1) return null;

		var builder = new System.Text.StringBuilder();
		byte[] hash = System.Security.Cryptography.MD5.Create().ComputeHash(buffer);
		foreach (var b in hash)
		{
			builder.Append(b.ToString("x2"));
		}
		return builder.ToString();
	}
	#endregion

	#region FileUtil
	private static readonly string STREAMMING_PATH = Application.streamingAssetsPath;
	private static readonly string PERSISTENT_PATH = Application.persistentDataPath;

	private static bool isStartWith(string str, string value)
	{
		int aLen = str.Length;
		int bLen = value.Length;

		int ap = 0; int bp = 0;
		while (ap < aLen && bp < bLen && str[ap] == value[bp]) { ap++; bp++; }

		return (bp == bLen);
	}

	private static async Task<string> readFileAllText(string path)
	{
		if(Application.platform == RuntimePlatform.Android && isStartWith(path, STREAMMING_PATH))
		{
			UnityWebRequest request = UnityWebRequest.Get(path);
			await request.SendWebRequest();
			return request.downloadHandler.text;
		}
		return await File.ReadAllTextAsync(path);
	}

	private static async Task<byte[]> readFileAllBytes(string path)
	{
		if (Application.platform == RuntimePlatform.Android && isStartWith(path, STREAMMING_PATH))
		{
			UnityWebRequest request = UnityWebRequest.Get(path);
			await request.SendWebRequest();
			return request.downloadHandler.data;
		}
		return await File.ReadAllBytesAsync(path);
	}
	#endregion

	#region Support await UnityWebRequestAsyncOperation
	private static System.Runtime.CompilerServices.TaskAwaiter<object> GetAwaiter(this UnityWebRequestAsyncOperation operation)
	{
		var tcs = new TaskCompletionSource<object>();
		operation.completed += (obj) => { tcs.SetResult(null); };
		return tcs.Task.GetAwaiter();
	}
	#endregion

	public struct VersionInfo
	{
		public int smallVersion;
		public int bigVersion;
		public string appleExamVersion;
		public string md5;
		public string cdn1;
		public string cdn2;
		public long time;
	}
}
