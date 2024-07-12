using System;
using System.Collections.Generic;

public interface IDownloadAssets
{
	/// <summary>
	/// 异步下载热更资源
	/// </summary>
	/// <param name="list"></param>
	void DownloadAssetsAsync(List<ResManifest.ResUnit> list);

	/// <summary>
	/// 删除无用资源
	/// </summary>
	void DeleteUselessAssetsAsync(List<ResManifest.ResUnit> list);

	/// <summary>
	/// 下载成功回调
	/// </summary>
	Action DownloadSuccessCallback { get; }

	/// <summary>
	/// 下载失败回调
	/// </summary>
	Action DownloaFailCallback { get; }
}
