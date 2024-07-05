using NDebug;
using System.Collections;
using UnityEngine;

public class GameConfig : MonoBehaviour
{
	[SerializeField] ChannelEnum channel;
	[SerializeField] PlayMode playMode;
	[SerializeField] bool useLocalAsset;
	[SerializeField] bool openSDK;
	[SerializeField] bool runtimeLogViewer;


	private static GameConfig _instance = null;


	public static GameConfig Singlton { get { return _instance; } }
	public static PlayMode PlayMode { get { return Singlton.playMode; } }
	/// <summary>
	/// true  使用本地资源
	/// false 使用AssetBundle
	/// </summary>
	public static bool UseLocalAsset { get { return Singlton.useLocalAsset; } }
	public static bool IsOpenSDK { get { return Singlton.openSDK; } }


	private void Awake()
	{
		_instance = this;
		DontDestroyOnLoad(this);

		// 初始化
		Application.targetFrameRate = 60;
		Application.runInBackground = true;
		Screen.sleepTimeout = SleepTimeout.NeverSleep;
		Debug.unityLogger.logEnabled = true;
		QualitySettings.vSyncCount = 0;

		// 运行时日志模块
		if (runtimeLogViewer)
		{
			Instantiate(Resources.Load("[Reporter]"), Vector3.zero, Quaternion.identity, transform.parent).name = "[Reporter]";
		}
#if !UNITY_EDITOR
        playMode = PlayMode.HostMode;
		useLocalAsset = false;
#endif
	}

	private IEnumerator Start()
	{
		// 加载渠道信息
		ChannelConfig.Init(channel);
		yield return new WaitForEndOfFrame();

		Log.Info("当前渠道：" + channel);
		Log.Info("运行模式：" + playMode);

		StartHotUpdate();
	}

	private void StartHotUpdate()
	{
		var prefab = Resources.Load<GameObject>("UpdatePanel");
		Instantiate(prefab, Vector3.zero, Quaternion.identity, UIRoot.CommonLayer);
	}
}

public enum PlayMode
{
	OfflineMode,        // 只使用StreammingAsset目录下的资源(无需热更)
	HostMode,           // 线上模式(有热更)
}
