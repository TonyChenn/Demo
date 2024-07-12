using NCore;
using System.Collections.Generic;
using UIKit;
using UnityEngine;

/// <summary>
/// 热更层入口
/// </summary>
public class GameMgr : MonoBehaviour
{
    private static GameMgr _instance = null;
    public static GameMgr Singlton { get { return _instance; } }

	private GameObject MainPanel = null;
	private BundleLoader loader = null;
	private List<AssetBundle> bundleList = new List<AssetBundle>();

    private void Awake()
    {
        _instance = this;
    }
	
	private void Start()
    {
		/// ----------------------------------------------ui_start

		UIManager.Singleton.Init();
	}
	
	public void Restart()
	{
		MainPanel.Destory();

		loader.Recycle();
		loader = null;
		bundleList.ForEach((bundle) => bundle.Unload(true));
		bundleList.Clear();
	}

    Vector2 scrolllPos;
    private void OnGUI()
    {
        GUILayout.BeginHorizontal();
        GUILayout.BeginVertical();
		GUILayout.Label("资源加载清单: ");
		GUILayout.BeginScrollView(scrolllPos, GUILayout.Height(Screen.height / 2 - 15));
		foreach (var item in ResMgr.LoadedABDict)
		{
			GUILayout.Label($"{item.Key}: {item.Value.RefCount}");
		}
		GUILayout.EndScrollView();
        GUILayout.EndVertical();

        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
    }

	public void QuitApp()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }
}
