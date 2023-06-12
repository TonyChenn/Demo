using HybridCLR;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


/// <summary>
/// AOT 无法热更
/// </summary>
public class Main : MonoBehaviour
{
    [SerializeField] ChannelEnum channel;

    private void Awake()
    {
        //Debug.Log($"资源系统运行模式：{playMode}");
        Application.targetFrameRate = 60;
        Application.runInBackground = true;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        DontDestroyOnLoad(this);
    }
    void Start()
    {
        ChannelConfig.Singleton.CurChannel = channel;
        LoadDLL();
    }

    void LoadDLL()
    {
#if !UNITY_EDITOR
        LoadMetadataForAOTAssemblies();
#endif
        System.Reflection.Assembly assembly = System.Reflection.Assembly.Load(LoadDllBytes($"dll/assembly-csharp.u"));
        System.Type gameMgrType = assembly.GetType("GameMgr");
        gameObject.AddComponent(gameMgrType);
    }

    /// <summary>
    /// 为aot assembly加载原始metadata， 这个代码放aot或者热更新都行。
    /// 一旦加载后，如果AOT泛型函数对应native实现不存在，则自动替换为解释模式执行
    /// </summary>
    private static void LoadMetadataForAOTAssemblies()
    {
        // AOT 补充元数据
        List<string> aotMetaAssemblyFiles = new List<string>()
        {
            "mscorlib",
            "system",
            "system.core",
        };
        /// 注意，补充元数据是给AOT dll补充元数据，而不是给热更新dll补充元数据。
        /// 热更新dll不缺元数据，不需要补充，如果调用LoadMetadataForAOTAssembly会返回错误
        /// 
        HomologousImageMode mode = HomologousImageMode.SuperSet;
        foreach (var aotDllName in aotMetaAssemblyFiles)
        {
            byte[] dllBytes = LoadDllBytes($"dll/{aotDllName}.u");
            // 加载assembly对应的dll，会自动为它hook。一旦aot泛型函数的native函数不存在，用解释器版本代码
            LoadImageErrorCode err = RuntimeApi.LoadMetadataForAOTAssembly(dllBytes, mode);
            Debug.Log($"LoadMetadataForAOTAssembly:{aotDllName}. mode:{mode} ret:{err}");
        }
    }
    private static byte[] LoadDllBytes(string dllName)
    {
        string path = PathUtil.GetBundleFullPath(dllName);
        return File.ReadAllBytes(path);
    }
}
