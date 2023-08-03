using NCore;
using System;
using NDebug;
using System.IO;
using Messengers;
using System.Threading.Tasks;
using UnityEngine.Networking;
using System.Collections.Generic;
using System.Collections;
using NCore.Networking;
using UIKit;

public class Version : NormalSingleton<Version>
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

	private readonly string DownloadFolder = string.Empty;
	private readonly string TmpVersionFilePath = string.Empty;
	private readonly string TmpMD5FilePath = string.Empty;

	private Version()
	{
		DownloadFolder = PathUtil.GetBundlePersistPath("download", true);

		// 版控临时文件
		TmpVersionFilePath = Path.Combine(DownloadFolder, VERSIONFILENAME);
		// md5临时文件
		TmpMD5FilePath = Path.Combine(DownloadFolder, MD5FILENAME);
	}

	#region 整个热更、下载流程
	public async void CheckUpdate()
	{
		Messenger<UpdateState, int>.Broadcast(MessengerDef.CHECK_UPDATE, UpdateState.GetRemoteVersion, 0);
		VersionInfo localVersionInfo = Singleton.GetLocalVersionInfo();
		int localSmall = localVersionInfo.smallVersion;
		int localBig = localVersionInfo.bigVersion;
		string str_local_version = $"{localBig}.{localSmall}";

		Messenger<UpdateState, int>.Broadcast(MessengerDef.CHECK_UPDATE, UpdateState.GetRemoteVersion, 10);
		Messenger<string>.Broadcast(MessengerDef.REFRESH_LOCAL_VERSION, str_local_version);
		Log.Info($"[01] 获取本地版本号：{str_local_version}");

		if (GameConfig.PlayMode == PlayMode.OfflineMode)
		{
			Log.Info("[01] 离线模式，无需更新，进入游戏");
			Messenger<string>.Broadcast(MessengerDef.REFRESH_REMOTE_VERSION, "offline");
			Messenger<UpdateState, int>.Broadcast(MessengerDef.CHECK_UPDATE, UpdateState.CanEnterGame, 100);
			return;
		}

		var remoteVersionReq = await GetRemoteVersion();
		if (remoteVersionReq == null)
		{
			Log.Error("[02] 获取远程版本失败");
			UIManager.ShowDialog(Localization.Get("获取远程版本失败,请重新尝试"),
				() => CheckUpdate(), Localization.Get("重试"),
				() => GameMgr.Singlton.QuitApp(), Localization.Get("取消")
			);
			Messenger<UpdateState, int>.Broadcast(MessengerDef.CHECK_UPDATE, UpdateState.GetRemoteVersionFail, 20);
			return;
		}

		VersionInfo remoteVersionInfo = GetVersionInfo(remoteVersionReq.GetBytes());
		int remoteSmall = remoteVersionInfo.smallVersion;
		int remoteBig = remoteVersionInfo.bigVersion;

		string str_remote_version = $"{remoteBig}.{remoteSmall}";
		Messenger<string>.Broadcast(MessengerDef.REFRESH_REMOTE_VERSION, str_remote_version);
		Messenger<UpdateState, int>.Broadcast(MessengerDef.CHECK_UPDATE, UpdateState.GetRemoteVersion, 20);
		Log.Info($"[02] 获取远程版本号：{str_remote_version}");

		// 强更，需要更新安装包
		if (localBig < remoteBig)
		{
			Log.Info("[02] 请下载最新安装包");
			UIManager.ShowDialog(Localization.Get("检测到最新安装包,请到应用商店进行下载"), () => UnityEngine.Application.OpenURL("https://blog.tonychenn.cn"));
			return;
		}

#if UNITY_IOS
		string versionName = NSDK.GetVersionName();
		if (versionName == remoteVersionInfo.appleExamVersion)
		{
			Log.Info("[01] iOS提审模式，无需更新，进入游戏");
			Messenger<string>.Broadcast(MessengerDef.REFRESH_REMOTE_VERSION, "apple exam");
			Messenger<UpdateState, int>.Broadcast(MessengerDef.CHECK_UPDATE, UpdateState.CanEnterGame, 100);
			return;
		}
#endif


		if (localSmall >= remoteSmall && !NeedFixClient)
		{
			DeleteTmpDownloadFolder();
			Log.Info("[02] 版本号一致新进入游戏: 100%");
			Messenger<UpdateState, int>.Broadcast(MessengerDef.CHECK_UPDATE, UpdateState.CanEnterGame, 100);
			return;
		}


		Log.Info("[03] 开始获取远程资源列表: 25%");
		Messenger<UpdateState, int>.Broadcast(MessengerDef.CHECK_UPDATE, UpdateState.GetRemoteRes, 25);
		var success = await DownloaRemoteMD5File(remoteVersionInfo.md5, (progress) =>
		{
			int progresss = 25 + (int)(progress * 25);
			Messenger<UpdateState, int>.Broadcast(MessengerDef.CHECK_UPDATE, UpdateState.GetRemoteRes, progresss);
		});
		if (!success)
		{
			Log.Error("[03] 获取远程资源列表失败: 50%");
			Messenger<UpdateState, int>.Broadcast(MessengerDef.CHECK_UPDATE, UpdateState.GetRemoteResFail, 50);
			return;
		}
		// 版本号变更, 资源无变化
		Messenger<UpdateState, int>.Broadcast(MessengerDef.CHECK_UPDATE, UpdateState.CheckRes, 50);
		string tmp_md5 = MD5Helper.GetFileMD5(TmpMD5FilePath);
		Log.Info($"[03] 远程资源列表MD5: {tmp_md5}\t55%");
		Messenger<UpdateState, int>.Broadcast(MessengerDef.CHECK_UPDATE, UpdateState.CheckRes, 55);
		string local_md5 = MD5Helper.GetFileMD5(PathUtil.GetBundleFullPath(MD5FILENAME, true));
		Log.Info($"[03] 本地资源列表MD5: {local_md5}\t60%");
		Messenger<UpdateState, int>.Broadcast(MessengerDef.CHECK_UPDATE, UpdateState.CheckRes, 60);

		if (tmp_md5 == local_md5 && !NeedFixClient)
		{
			Log.Info("[04] 版本号变更，资源无化，进入游戏 100%");
			Messenger<UpdateState, int>.Broadcast(MessengerDef.CHECK_UPDATE, UpdateState.CanEnterGame, 95);
			SaveVersionFile();
			DeleteTmpDownloadFolder();
			Messenger<UpdateState, int>.Broadcast(MessengerDef.CHECK_UPDATE, UpdateState.CanEnterGame, 100);
			return;
		}

		Log.Info("[05] 开始对比变更资源\t75%");
		Messenger<UpdateState, int>.Broadcast(MessengerDef.CHECK_UPDATE, UpdateState.CheckRes, 75);
		var modifyList = new List<ResManifest.ResUnit>(32);
		var delList = new List<ResManifest.ResUnit>(0);
		ulong downloadSize = 0;

		CalNeedUpdateInfo(ref modifyList, ref delList, ref downloadSize, NeedFixClient);
		Log.Info($"[05] 变动_新增：{modifyList.Count}, 删除：{delList.Count}, 下载大小{GetByteLengthString(downloadSize)}");
		Messenger<UpdateState, int>.Broadcast(MessengerDef.CHECK_UPDATE, UpdateState.CheckRes, 99);


		// 没有资源变动，无需下载
		if (modifyList.Count == 0 && delList.Count == 0)
		{
			Log.Info("[05] 客户端资源无变动，进入游戏\t100%");
			SaveVersionFile();
			DeleteTmpDownloadFolder();
			Messenger<UpdateState, int>.Broadcast(MessengerDef.CHECK_UPDATE, UpdateState.CanEnterGame, 100);
			return;
		}
		string _title = Localization.Get("更新提示");
		string _content = string.Format(Localization.Get("检测到更新{0}"), GetByteLengthString(downloadSize));
		UIManager.ShowDialog(_title, _content,
			() =>
			{
				DownloadUpdateBundleAsync(modifyList, delList, downloadSize);
				NeedFixClient = false;
			},
			() => GameMgr.Singlton.QuitApp());
	}
	#endregion


	#region 获取远程、本地版本号
	private VersionInfo GetLocalVersionInfo()
	{
		string path = PathUtil.GetBundleFullPath(VERSIONFILENAME, true);
		return GetVersionInfo(File.ReadAllBytes(path));
	}
	private VersionInfo GetVersionInfo(byte[] bytes)
	{
		using (BinaryReader reader = new BinaryReader(new MemoryStream(bytes)))
		{
			return new VersionInfo()
			{
				smallVersion = reader.ReadInt32(),
				bigVersion = reader.ReadInt32(),
				appleExamVersion = reader.ReadString(),
				md5 = reader.ReadString(),
				time = reader.ReadInt64()
			};
		}
	}

	private async Task<UnityWebRequest> GetRemoteVersion()
	{
		string remoteUrl = PathUtil.GetBundleCDNUrl(VERSIONFILENAME, true);
		var request = await WebServer.Get($"{remoteUrl}?{DateTime.Now}");
		if (request.result == UnityWebRequest.Result.Success)
		{
			request.SaveFile(TmpVersionFilePath);
			return request;
		}
		return null;
	}
	#endregion


	#region 下载远程MD5文件
	private async Task<bool> DownloaRemoteMD5File(string md5, Action<float> refreshAction)
	{
		bool result = false;

		string url = $"{ChannelConfig.CurCDNUrl}/{md5}.csv";
		await DownloaRemoteMD5FileHandler(url, refreshAction, (success) =>
		{
			result = success;
		});

		return result;
	}
	IEnumerator DownloaRemoteMD5FileHandler(string url, Action<float> refreshAction, Action<bool> resultAction)
	{
		UnityWebRequest request = UnityWebRequest.Get(url);
		request.SendWebRequest();

		while (!request.isDone)
		{
			refreshAction?.Invoke(request.downloadProgress);
			yield return null;
		}
		if (request.result == UnityWebRequest.Result.Success)
		{
			request.SaveFile(TmpMD5FilePath);
			resultAction?.Invoke(true);
		}
		else
		{
			resultAction?.Invoke(false);
		}
	}
	#endregion

	/// <summary>
	/// 计算需要下载、删除的资源信息
	/// </summary>
	/// <param name="modifyList">add,modify列表</param>
	/// <param name="delList">delete列表</param>
	/// <param name="downloadSize">需要下载的文件大小</param>
	private void CalNeedUpdateInfo(ref List<ResManifest.ResUnit> modifyList, ref List<ResManifest.ResUnit> delList,
								   ref ulong downloadSize, bool checkLocalAsset = false)
	{
		// 解析本地资源MD5数据
		var localDict = new Dictionary<string, ResManifest.ResUnit>(256);
		ResManifest.ParseResManifest(PathUtil.GetBundleFullPath(MD5FILENAME, true), localDict);


		// 解析tmp数据(保存一份解析)
		ResManifest.ResManifestDict = new Dictionary<string, ResManifest.ResUnit>(256);
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

			if (MD5Helper.GetFileMD5(path) == item.md5)
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
				string path = PathUtil.GetBundleFullPath(item.Key);
				string localMD5 = MD5Helper.GetFileMD5(path);
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
	private void DownloadUpdateBundleAsync(List<ResManifest.ResUnit> modifyList, List<ResManifest.ResUnit> deleteList, ulong downloadSize)
	{
		Downloader.Singleton.DeleteAll();

		ulong curDownloadSize = 0;
		Dictionary<string, ulong> downloadFileSizeDict = new Dictionary<string, ulong>(modifyList.Count);
		// 下载文件数量
		int downloadCount = modifyList.Count;

		// 下载完成回调
		Action downloadCompleteAction = () =>
		{
			Log.Info("[07] 下载更新资源完成");
			DeleteUselessBundle(deleteList);
			Log.Info($"[07] 删除无用资源完成：{deleteList.Count}");

			SaveMd5File();
			SaveVersionFile();
			Messenger<UpdateState, int>.Broadcast(MessengerDef.CHECK_UPDATE, UpdateState.Download, 100);

			Log.Info("[10] 更新完成，重新启动游戏客户端");
			UIManager.ShowDialog(Localization.Get("更新已完成，请重启客户端"), () => GameMgr.Singlton.QuitApp());
		};

		if (downloadCount == 0)
		{
			Log.Info("[06] 下载数量为0, 跳过下载步骤");
			downloadCompleteAction();
			return;
		}

		Log.Info("[06] 开始下载更新资源");
		for (int i = 0, iMax = modifyList.Count; i < iMax; i++)
		{
			string bundleName = modifyList[i].bundleName;
			string bundleMd5 = modifyList[i].md5;
			ulong bundleSize = 0;
			ulong.TryParse(modifyList[i].size, out bundleSize);

			// 使用MD5Name请求远程文件
			string md5Name = ResManifest.GetBundleMD5Name(bundleName);
			string url = $"{ChannelConfig.CurChannelConfig.CurCDNUrl}/{md5Name}";
			string savePath = PathUtil.GetBundlePersistPath(bundleName);
			DownloadUnit unit = new(bundleName, url, savePath, bundleMd5);
			unit.Size = bundleSize;

			unit.ErrorFun = (msg) =>
			{
				Downloader.Singleton.Delete(unit);
				Log.Error($"多次下载失败：{unit.Name}\n {msg}");
			};
			unit.ProgressFun = (curSize, totalSize) =>
			{
				curDownloadSize = curDownloadSize + curSize - downloadFileSizeDict[bundleName];
				downloadFileSizeDict[bundleName] = curSize;
				Messenger<UpdateState, int>.Broadcast(MessengerDef.CHECK_UPDATE, UpdateState.Download, (int)((curDownloadSize) * 99 / downloadSize));
			};
			unit.CompleteFun = () =>
			{
				Log.Info($"{unit.Name} 下载完成");
				--downloadCount;
				if (downloadCount > 0) return;

				downloadCompleteAction();
			};
			downloadFileSizeDict[bundleName] = 0;
			Downloader.Singleton.Add(unit);
		}
		Downloader.Singleton.StartAsync();
	}

	// 删除无用资源(Bundle)
	private void DeleteUselessBundle(List<ResManifest.ResUnit> deleteList)
	{
		Log.Info("[07] 删除无用资源");
		for (int i = 0, iMax = deleteList.Count; i < iMax; i++)
		{
			var item = deleteList[i];
			string path = PathUtil.GetBundlePersistPath(item.bundleName);
			if (File.Exists(path))
			{
				File.Delete(path);
				Log.RedInfo("[Update delete] > " + item.bundleName);
			}
		}
	}
	#endregion


	/// <summary>
	/// 删除版控文件/清单文件，修复客户端
	/// </summary>
	public void FixClient()
	{
		var path = PathUtil.GetBundlePersistPath(VERSIONFILENAME, true);
		if (File.Exists(path)) { File.Delete(path); }

		var md5_path = PathUtil.GetBundlePersistPath(MD5FILENAME, true);
		if (File.Exists(md5_path)) { File.Delete(md5_path); }
	}
	// 将download/version.data 复制到version.data
	private void SaveVersionFile()
	{
		try
		{
			File.Copy(TmpVersionFilePath, PathUtil.GetBundlePersistPath(VERSIONFILENAME, true), true);

			VersionInfo v = GetLocalVersionInfo();
			Messenger<string>.Broadcast(MessengerDef.REFRESH_LOCAL_VERSION, $"{v.bigVersion}.{v.smallVersion}");
		}
		catch (Exception ex)
		{
			Log.Error(string.Format("移动 version.data 失败: {0}", ex.Message));
		}
	}

	private void SaveMd5File()
	{
		try
		{
			var path = PathUtil.GetBundlePersistPath(MD5FILENAME, true);
			File.Copy(TmpMD5FilePath, path, true);
		}
		catch (Exception ex)
		{
			Log.Error(string.Format("移动 res_manifest 失败: {0}", ex.Message));
		}
	}

	/// <summary>
	/// 清理临时下载目录
	/// </summary>
	private void DeleteTmpDownloadFolder()
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
	public bool NeedFixClient
	{
		get { return PlayerPrefsHelper.GetBool("NEED_FIX_CLIENT", false); }
		set { PlayerPrefsHelper.SetBool("NEED_FIX_CLIENT", value); }
	}
	private string GetByteLengthString(ulong length)
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
	#endregion

	public struct VersionInfo
	{
		public int smallVersion;
		public int bigVersion;
		public string appleExamVersion;
		public string md5;
		public long time;
	}
}
