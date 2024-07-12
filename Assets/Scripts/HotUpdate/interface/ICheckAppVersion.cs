using System;

public interface ICheckAppVersion
{
	/// <summary>
	/// 远程版本文件下载路径
	/// </summary>
	string RemoteVersionFileUrl { get; }

	/// <summary>
	/// 下载版本文件失败
	/// </summary>
	void DownloadVersionFail();

	/// <summary>
	/// 加载本地版本文件数据
	/// </summary>
	byte[] LoadLocalVersionInfo();

	/// <summary>
	/// 是否需要热更新
	/// </summary>
	bool NeedUpdate(string localData, string remoteData);
}
