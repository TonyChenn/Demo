/// <summary>
/// 本文件中的代码为生成的代码，不允许手动修改
/// Generate by TonyChenn @
/// </summary>

using System;
using UnityEngine;

[Serializable]
public partial class Item_t_equip
{
	/// <summary>
	/// n_id(ID)
	/// <summary>
	public uint n_id;
	/// <summary>
	/// n_Name(名称)
	/// <summary>
	public string n_Name;
	/// <summary>
	/// n_Model(模型路径)
	/// <summary>
	public string n_Model;
	/// <summary>
	/// n_HasWalkAnim(是否有行走动画)
	/// <summary>
	public bool n_HasWalkAnim;
	/// <summary>
	/// n_EnterAnim(入场特效资源)
	/// <summary>
	public string n_EnterAnim;
	/// <summary>
	/// n_IdleAnim(待机动画)
	/// <summary>
	public string n_IdleAnim;
	/// <summary>
	/// n_FaceType(表情类型)
	/// <summary>
	public string n_FaceType;

}

public partial class Config_t_equip : ScriptableObject
{
    private static Config_t_equip _instence = null;
    public Item_t_equip[] Array;
    public static Config_t_equip Singleton
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
		string path = $"{Application.streamingAssetsPath}/asset/table/t_equip.u";
		if(GameConfig.PlayMode == PlayMode.HostMode)
		{
			string temp = $"{Application.persistentDataPath}/asset/table/t_equip.u";
			if (System.IO.File.Exists(temp))
			{
				path = temp;
			}
		}
		AssetBundle bundle = AssetBundle.LoadFromFile(path);
		_instence = bundle.LoadAsset<Config_t_equip>("asset/table/t_equip.u");
    }

#if UNITY_EDITOR
    private static void LoadFromLocal()
	{
		string path = "Assets/BuildBundle/Asset/Table/t_equip.asset";
		var obj = UnityEditor.AssetDatabase.LoadAssetAtPath<Config_t_equip>(path);
		_instence = obj;
	}
#endif
}
