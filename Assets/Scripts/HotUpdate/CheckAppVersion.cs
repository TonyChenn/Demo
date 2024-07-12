using LitJson;
using NDebug;
using System;
using System.IO;
using System.Text;
using UnityEngine;

public partial class CheckAppVersion : ICheckAppVersion
{
	public const string VERSIONFILENAME = "version.data";

	public string LocalVersionFilePath => PathUtil.GetBundlePath(VERSIONFILENAME, true);

	public string RemoteVersionFileUrl { get { return PathUtil.GetBundleCDNUrl(VERSIONFILENAME, true); } }

	public void DownloadVersionFail()
	{
		//GameObject hotObj = GameObject.Find("[HotUpdate]");
		//if (hotObj == null) { Log.Error("Cant find GameObject: [HotUpdate]"); }
		//HotUpdate hotUpdate = hotObj.GetComponent<HotUpdate>();
		//if (hotUpdate == null) { Log.Error("Cant find Component in GameObject [HotUpdate]"); }
		//hotUpdate.DownloadVersionFileFail?.Invoke();
	}

	public byte[] LoadLocalVersionInfo()
	{
		string path = PathUtil.GetBundlePath(VERSIONFILENAME, true);
		return File.ReadAllBytes(path);
	}

	public virtual bool NeedUpdate(string localData, string remoteData)
	{
		var localVersion = JsonUtility.FromJson<Version.VersionInfo>(localData);
		var remoteVersion = JsonUtility.FromJson<Version.VersionInfo>(remoteData);

		if (localVersion.bigVersion < remoteVersion.bigVersion)
		{
			Log.Info($"[CheckAppVersion] 请下载最新安装包: {remoteVersion.bigVersion}.{remoteVersion.smallVersion}");
			return false;
		}

#if UNITY_IOS
		if(remoteVersion.appleExamVersion == Application.version)
		{
			Log.Info("[CheckAppVersion] iOS提审模式，无需更新，进入游戏");
			return false;
		}
#endif



		return true;
	}
}
