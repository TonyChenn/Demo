/// <summary>
/// 本文件中的代码为生成的代码，不允许手动修改
/// Generate by TonyChenn @
/// </summary>

using System;
using UnityEngine;

[Serializable]
public partial class Item_t_test
{
	/// <summary>
	/// f_id(ID)
	/// <summary>
	public uint f_id;
	/// <summary>
	/// f_Name(名称)
	/// <summary>
	public string f_Name;

}

public partial class Config_t_test : ScriptableObject
{
    private static Config_t_test _instence = null;
    public Item_t_test[] Array;
    public static Config_t_test Singleton
    {
        get
        {
            if (_instence == null) Init();
            return _instence;
        }
    }

    private static void Init()
    {
#if UNITY_EDITOR
        if (GameConfig.UseLocalAsset)
            LoadFromLocal();
        else
            LoadFromBundle();
#else
            LoadFromBundle();
#endif
    }

    private static void LoadFromBundle()
    {
		string path = $"{Application.streamingAssetsPath}/asset/table/t_test.u";
		if(GameConfig.PlayMode == PlayMode.HostMode)
		{
			string temp = $"{Application.persistentDataPath}/asset/table/t_test.u";
			if (System.IO.File.Exists(temp))
			{
				path = temp;
			}
		}
		AssetBundle bundle = AssetBundle.LoadFromFile(path);
		_instence = bundle.LoadAsset<Config_t_test>("asset/table/t_test.u");
    }

#if UNITY_EDITOR
    private static void LoadFromLocal()
	{
		string path = "Assets/BuildBundle/Asset/Table/t_test.asset";
		var obj = UnityEditor.AssetDatabase.LoadAssetAtPath<Config_t_test>(path);
		_instence = obj;
	}
#endif
}
