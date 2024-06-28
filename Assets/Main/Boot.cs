using HybridCLR;
using NDebug;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

/// <summary>
/// AOT 无法热更
/// </summary>
public class Boot : MonoBehaviour
{
	void Start()
	{
		LoadDLL();
	}

	void LoadDLL()
	{
		Assembly assembly = null;
#if UNITY_EDITOR
		assembly = System.AppDomain.CurrentDomain.GetAssemblies().First(a => a.GetName().Name == "Assembly-CSharp");
#else
		LoadAOTDlls();
		LoadHotfixDlls(ref assembly);
#endif
		System.Type gameMgr = assembly.GetType("GameMgr");
		GameObject.Find("[Main]").AddComponent(gameMgr);
	}

	/// <summary>
	/// 为aot assembly加载原始metadata， 这个代码放aot或者热更新都行。
	/// 一旦加载后，如果AOT泛型函数对应native实现不存在，则自动替换为解释模式执行
	/// 
	/// 注意，补充元数据是给AOT dll补充元数据，而不是给热更新dll补充元数据。
	/// 热更新dll不缺元数据，不需要补充，如果调用LoadMetadataForAOTAssembly会返回错误
	/// </summary>
	private static void LoadAOTDlls()
	{
		Log.Info("开始补充元数据");
		var aotDlls = Resources.LoadAll<TextAsset>("AotDll");
		foreach (var dll in aotDlls)
		{
			LoadImageErrorCode err = RuntimeApi.LoadMetadataForAOTAssembly(dll.bytes, HomologousImageMode.SuperSet);
			Log.Info($"补充元数据：{dll.name}, ret: {err == LoadImageErrorCode.OK}");
		}
	}

	private static void LoadHotfixDlls(ref Assembly assembly)
	{
		Log.Info("开始加载热更程序集");
		string hotfixDll = Application.persistentDataPath + "/code/code.u";
		if(!File.Exists(hotfixDll) || GameConfig.PlayMode == PlayMode.OfflineMode)
		{
			hotfixDll = Application.streamingAssetsPath + "/code/code.u";
		}

		AssetBundle bundle = AssetBundle.LoadFromFile(hotfixDll);
		TextAsset dll = bundle.LoadAsset<TextAsset>("Assembly-CSharp.bytes");
		assembly = Assembly.Load(dll.bytes);
		Log.Info($"热更程序集记载完毕: {dll != null}");
		bundle.Unload(true);
	}
}
