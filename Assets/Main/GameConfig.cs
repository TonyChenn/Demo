using UnityEngine;

public class GameConfig : MonoBehaviour
{
	[SerializeField] ChannelEnum channel;
    [SerializeField] PlayMode playMode;
    

    private static GameConfig _instance = null;
	private Boot boot = null;

    private void Awake()
    {
        _instance = this;
		boot = GetComponent<Boot>();

#if !UNITY_EDITOR
        playMode = PlayMode.HostMode;
#endif
		ChannelConfig.Init(channel);
	}

    public static GameConfig Singlton { get { return _instance; } }

    public static PlayMode PlayMode { get { return Singlton.playMode; } }
}

public enum PlayMode
{
    OfflineMode,        // 只使用StreammingAsset目录下的资源(无需热更)
    HostMode,           // 线上模式
	DevMode,			// 使用真实代码+prefab
}
