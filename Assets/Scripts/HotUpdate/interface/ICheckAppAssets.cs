using System.Collections.Generic;

public interface ICheckAppAssets
{
	/// <summary>
	/// 远程资源清单下载路径
	/// </summary>
	string RemoteMD5FileUrl { get; }

	/// <summary>
	/// 加载本地资源清单数据
	/// </summary>
	byte[] LoadLocalMD5FileInfo();

	/// <summary>
	/// 获取资源下载列表
	/// </summary>
	List<ResManifest.ResUnit> GetDownloadResUnitList();

	/// <summary>
	/// 获取要删除的资源清单
	/// </summary>
	List<ResManifest.ResUnit> GetDeleteResUnitList();
}
