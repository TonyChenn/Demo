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

    private const string VERSIONFILENAME = "version.dat";
    private const string MD5FILENAME = "main.csv";

    private readonly string DownloadFolder = string.Empty;
    private readonly string TmpVersionFilePath = string.Empty;
    private readonly string TmpMD5FilePath = string.Empty;

    private readonly ChannelItem channel;
    private Version()
    {
        channel = ChannelConfig.Singleton.CurChannelConfig;
        DownloadFolder = PathUtil.GetAssetBundlePersistPath("download");

        // 版控临时文件
        TmpVersionFilePath = Path.Combine(DownloadFolder, VERSIONFILENAME);
        // md5临时文件
        TmpMD5FilePath = Path.Combine(DownloadFolder, MD5FILENAME);
    }

    public ChannelItem Channel { get { return channel; } }

    #region 整个热更、下载流程
    public async void CheckUpdate()
    {
        Messenger<UpdateState, int>.Broadcast(MessengerDef.CHECK_UPDATE, UpdateState.GetRemoteVersion, 0);
        string localVersion = Singleton.GetLocalVersion();
        await Task.Delay(100);
        Messenger<UpdateState, int>.Broadcast(MessengerDef.CHECK_UPDATE, UpdateState.GetRemoteVersion, 10);
        Messenger<string>.Broadcast(MessengerDef.REFRESH_LOCAL_VERSION, localVersion);
        Log.Info("[01] 获取本地版本号：{localVersion}");

        if (GameConfig.Singlton.PlayMode == PlayMode.OfflineMode)
        {
            Log.Info("[01] 离线模式，无需更新，进入游戏");
            Messenger<UpdateState, int>.Broadcast(MessengerDef.CHECK_UPDATE, UpdateState.CanEnterGame, 100);
            return;
        }


        var remoteVersionReq = await GetRemoteVersion();
        if (remoteVersionReq == null)
        {
            await Task.Delay(100);
            Log.Error("[02] 获取远程版本失败");
            Messenger<UpdateState, int>.Broadcast(MessengerDef.CHECK_UPDATE, UpdateState.GetRemoteVersionFail, 20);
            return;
        }
        var remoteVersion = remoteVersionReq.GetJsonNode()["version"];
        await Task.Delay(100);
        Messenger<string>.Broadcast(MessengerDef.REFRESH_REMOTE_VERSION, remoteVersion);
        Messenger<UpdateState, int>.Broadcast(MessengerDef.CHECK_UPDATE, UpdateState.GetRemoteVersion, 20);
        Log.Info($"[02] 获取远程版本号：{remoteVersion}");
        if (localVersion == remoteVersion && !NeedFixClient)
        {
            DeleteTmpDownloadFolder();
            Log.Info("[02] 版本号一致新进入游戏: 100%");
            await Task.Delay(100);
            Messenger<UpdateState, int>.Broadcast(MessengerDef.CHECK_UPDATE, UpdateState.CanEnterGame, 100);
            return;
        }


        Log.Info("[03] 获取远程资源列表: 25%");
        await Task.Delay(100);
        Messenger<UpdateState, int>.Broadcast(MessengerDef.CHECK_UPDATE, UpdateState.GetRemoteRes, 25);
        var success = await DownloaRemoteMD5File((progress) =>
        {
            int progresss = 25 + ((int)(progress * 100)) / 25;
            Messenger<UpdateState, int>.Broadcast(MessengerDef.CHECK_UPDATE, UpdateState.GetRemoteRes, progresss);
        });
        if (!success)
        {
            Log.Error("[03] 获取远程资源列表失败: 25%");
            Messenger<UpdateState, int>.Broadcast(MessengerDef.CHECK_UPDATE, UpdateState.GetRemoteResFail, 25);
            return;
        }

        // 版本号变更, 资源无变化
        await Task.Delay(100);
        Messenger<UpdateState, int>.Broadcast(MessengerDef.CHECK_UPDATE, UpdateState.CheckRes, 50);
        string tmp_md5 = MD5Helper.GetFileMD5(TmpMD5FilePath);
        Log.Info($"[04] 远程资源列表MD5: {tmp_md5}");
        await Task.Delay(100);
        Messenger<UpdateState, int>.Broadcast(MessengerDef.CHECK_UPDATE, UpdateState.CheckRes, 55);
        string local_md5 = MD5Helper.GetFileMD5(PathUtil.GetBundleFullPath(MD5FILENAME));
        Log.Info("[04] 本地资源列表MD5: " + local_md5);
        await Task.Delay(100);
        Messenger<UpdateState, int>.Broadcast(MessengerDef.CHECK_UPDATE, UpdateState.CheckRes, 60);
        if (tmp_md5 == local_md5 && !NeedFixClient)
        {
            Log.Info("[04] 版本号变更，资源无化，进入游戏");
            await Task.Delay(100);
            Messenger<UpdateState, int>.Broadcast(MessengerDef.CHECK_UPDATE, UpdateState.CanEnterGame, 95);
            SaveVersionFile();
            await Task.Delay(100);
            DeleteTmpDownloadFolder();
            Messenger<UpdateState, int>.Broadcast(MessengerDef.CHECK_UPDATE, UpdateState.CanEnterGame, 100);
            return;
        }

        await Task.Delay(100);
        Log.Info("[05] 开始对比变更资源");
        Messenger<UpdateState, int>.Broadcast(MessengerDef.CHECK_UPDATE, UpdateState.CheckRes, 75);
        List<string[]> modifyList = new List<string[]>(32);
        List<string[]> delList = new List<string[]>(32);
        ulong downloadSize = 0;

        CalNeedUpdateInfo(ref modifyList, ref delList, ref downloadSize, NeedFixClient);
        await Task.Delay(100);
        Log.Info($"[05] 变动_新增：{modifyList.Count}, 删除：{delList.Count}, 下载大小{GetByteLengthString(downloadSize)}");
        Messenger<UpdateState, int>.Broadcast(MessengerDef.CHECK_UPDATE, UpdateState.CheckRes, 99);


        // 没有资源变动，无需下载
        if (modifyList.Count == 0 && delList.Count == 0)
        {
            Log.Info("[05] 客户端资源无变动，进入游戏");
            SaveVersionFile();
            await Task.Delay(100);
            DeleteTmpDownloadFolder();
            Messenger<UpdateState, int>.Broadcast(MessengerDef.CHECK_UPDATE, UpdateState.CanEnterGame, 100);
            return;
        }

        // 下载
        //DownloadUpdate(modifyList, delList);
        DownloadUpdateBundleAsync(modifyList, delList, downloadSize);
        NeedFixClient = false;
    }
    #endregion


    #region 获取远程、本地版本号
    private string GetLocalVersion()
    {
        string json = File.ReadAllText(PathUtil.GetBundleFullPath(VERSIONFILENAME));
        JSONNode node = JSON.Parse(json);

        return node["version"];
    }
    private async Task<UnityWebRequest> GetRemoteVersion()
    {
        var request = await WebServer.Get($"{channel.CurAssetURL}/{VERSIONFILENAME}?{System.DateTime.Now}");
        if (request.result == UnityWebRequest.Result.Success)
        {
            request.SaveFile(TmpVersionFilePath);
            return request;
        }
        return null;
    }
    #endregion


    #region 下载远程MD5文件
    private async Task<bool> DownloaRemoteMD5File(Action<float> refreshAction)
    {
        bool result = false;

        string url = $"{channel.CurAssetURL}/{MD5FILENAME}?{System.DateTime.Now}";
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
    private void CalNeedUpdateInfo(ref List<string[]> modifyList, ref List<string[]> delList, ref ulong downloadSize, bool checkLocalAsset = false)
    {
        // 解析tmp数据
        string[] tmpArray = File.ReadAllLines(TmpMD5FilePath);
        Dictionary<string, string[]> tmpDict = new Dictionary<string, string[]>(256);
        for (int i = 0, iMax = tmpArray.Length; i < iMax; i++)
        {
            string[] item = tmpArray[i].Split(',');
            tmpDict[item[0]] = item;
        }

        // 解析本地资源MD5数据
        string[] localArray = File.ReadAllLines(PathUtil.GetBundleFullPath(MD5FILENAME));
        Dictionary<string, string[]> localDict = new Dictionary<string, string[]>(256);
        for (int i = 0, iMax = localArray.Length; i < iMax; i++)
        {
            string[] item = localArray[i].Split(',');
            localDict[item[0]] = item;
        }


        // 计算add,motify列表
        foreach (var item in tmpDict)
        {
            if (!localDict.ContainsKey(item.Key))
            {
                modifyList.Add(item.Value);
                downloadSize += ulong.Parse(item.Value[2]);
            }
            else
            {
                var localItem = localDict[item.Key];
                if (item.Value[1] != localItem[1])
                {
                    modifyList.Add(item.Value);
                    downloadSize += ulong.Parse(item.Value[2]);
                }
            }
        }

        // 剔除已下载过的文件
        for (int i = 0; i < modifyList.Count; i++)
        {
            var item = modifyList[i];
            var path = PathUtil.GetAssetBundlePersistPath(item[0]);
            if (!File.Exists(path)) continue;

            if (MD5Helper.GetFileMD5(path) == item[1])
            {
                downloadSize -= ulong.Parse(item[2]);
                modifyList.RemoveAt(i);
            }
        }

        // 检测本地文件是否丢失(修复客户端)
        if (checkLocalAsset)
        {
            foreach (var item in tmpDict)
            {
                string path = PathUtil.GetBundleFullPath(item.Key);
                string localMD5 = MD5Helper.GetFileMD5(path);
                if (localMD5 != item.Value[1])
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
    }


    #region 下载更新资源
    /// <summary>
    /// 下载AssetBundle
    /// </summary>
    /// <param name="modifyList"></param>
    private void DownloadUpdateBundleAsync(List<string[]> modifyList, List<string[]> deleteList, ulong downloadSize)
    {
        ulong curDownloadSize = 0;
        Dictionary<string, ulong> downloadFileSizeDict = new Dictionary<string, ulong>(modifyList.Count);
        bool needRestartClient = false;
        // 下载文件数量
        int downloadCount = modifyList.Count;

        Log.Info("[06] 开始下载更新资源");
        for (int i = 0, iMax = modifyList.Count; i < iMax; i++)
        {
            string bundleName = modifyList[i][0];
            string bundleMd5 = modifyList[i][1];
            ulong bundleSize = 0;
            ulong.TryParse(modifyList[i][2], out bundleSize);

            if (bundleName.Contains("dll")) needRestartClient = true;

            string url = $"{ChannelConfig.Singleton.CurChannelConfig.CurAssetURL}/{bundleName}";
            string savePath = PathUtil.GetAssetBundlePersistPath(bundleName);
            DownloadUnit unit = new DownloadUnit(bundleName, url, savePath, bundleMd5);
            unit.Size = bundleSize;

            unit.ErrorFun = (msg) =>
            {
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
                --downloadCount;
                if (downloadCount > 0) return;
                Log.Info("[07] 下载更新资源完成");
                DeleteUselessBundle(deleteList);
                Log.Info($"[07] 删除无用资源完成：{deleteList.Count}");

                SaveMd5File();
                SaveVersionFile();
                Messenger<UpdateState, int>.Broadcast(MessengerDef.CHECK_UPDATE, UpdateState.Download, 100);

                // 如果更新了dll程序集需要重新启动客户端
                // 免费版HybridCLR不支持热重载DLL
                if (needRestartClient)
                {
                    Log.Info("[10] 更新完成，重新启动游戏客户端");
                    GameMgr.Singlton.QuitApp();
                }
                else
                {
                    // 重启游戏
                    GameMgr.Singlton.RestartGame();
                }
            };

            downloadFileSizeDict[bundleName] = 0;

            Downloader.Singleton.DownLoadAsync(unit);
        }
    }

    // 删除无用资源(Bundle)
    private void DeleteUselessBundle(List<string[]> deleteList)
    {
        Log.Info("[07] 删除无用资源");
        for (int i = 0, iMax = deleteList.Count; i < iMax; i++)
        {
            var item = deleteList[i];
            string path = PathUtil.GetAssetBundlePersistPath(item[0]);
            if (File.Exists(path))
            {
                File.Delete(path);
                Log.RedInfo("[Update delete] > " + item[0]);
            }
        }
    }
    #endregion


    /// <summary>
    /// 删除版控文件，修复客户端
    /// </summary>
    public void DeleteVersionFile()
    {
        var path = PathUtil.GetAssetBundlePersistPath(VERSIONFILENAME);
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }
    // 将download/version.dat 复制到version.dat
    private void SaveVersionFile()
    {
        try
        {
            File.Copy(TmpVersionFilePath, PathUtil.GetAssetBundlePersistPath(VERSIONFILENAME), true);

            var version = GetLocalVersion();
            Messenger<string>.Broadcast(MessengerDef.REFRESH_LOCAL_VERSION, version);
        }
        catch (Exception ex)
        {
            Log.Error(string.Format("移动 version.dat 失败: {0}", ex.Message));
        }
    }

    private void SaveMd5File()
    {
        try
        {
            var path = PathUtil.GetAssetBundlePersistPath(MD5FILENAME);
            File.Copy(TmpMD5FilePath, path, true);
        }
        catch (Exception ex)
        {
            Log.Error(string.Format("移动 main.csv 失败: {0}", ex.Message));
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
}
