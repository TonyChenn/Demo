using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ChannelConfig", menuName = "创建Asset/渠道配置", order = 1)]
public class ChannelConfig : ScriptableObject
{
    public List<ChannelItem> ChannelList = new List<ChannelItem>();

    #region Singleton

    private ChannelEnum curChannel = ChannelEnum.Develop;
    private ChannelItem curChannelConfig = null;
    public ChannelEnum CurChannel
    {
        get { return curChannel; }
        set { curChannel = value; curChannelConfig = ChannelList.Find((c) => { return c.ChannelType == curChannel; }); }
    }
    public ChannelItem CurChannelConfig
    {
        get
        {
            if (curChannelConfig == null)
            {
                curChannelConfig = ChannelList.Find((c) => { return c.ChannelType == ChannelEnum.Develop; });
            }

            return curChannelConfig;
        }
    }

    private static ChannelConfig _instance;
    public static ChannelConfig Singleton
    {
        get
        {
            if (_instance == null)
            {
                _instance = Resources.Load<ChannelConfig>("ChannelConfig");
            }
            return _instance;
        }
    }

    /// <summary>
    /// 是否启用备用CDN
    /// </summary>
    public static bool UseBakCDN
    {
        get
        {
            if (PlayerPrefs.HasKey("USE_BAK_CDN"))
            {
                return PlayerPrefs.GetInt("USE_BAK_CDN") == 1;
            }
            return false;
        }
        set
        {
            PlayerPrefs.SetInt("USE_BAK_CDN", value ? 1 : 0);
        }
    }
    #endregion
}

[System.Serializable]
public class ChannelItem
{
    [SerializeField]
    private string curAssetURL = null;
    /// <summary>
    /// 渠道类型
    /// </summary>
    public ChannelEnum ChannelType;
    /// <summary>
    /// 服务器地址
    /// </summary>
    public string ServerURL;
    /// <summary>
    /// 版本文件地址
    /// </summary>
    [NonSerialized]
    public string VersionURL;
    /// <summary>
    /// 资源地址1(CDN1)
    /// </summary>
    public string AssetURL1;

    /// <summary>
    /// 资源地址2(CDN2)
    /// </summary>
    public string AssetURL2;



    public string CurAssetURL { get { return ChannelConfig.UseBakCDN ? AssetURL2 : AssetURL1; } }


    /// <summary>
    /// 启用分享
    /// </summary>
    public bool ShareEnable;
}

/// <summary>
/// 渠道枚举
/// </summary>
public enum ChannelEnum
{
    Develop, //开发服
    Android,
    IOS
}