using System;
using System.Collections.Generic;
using UnityEngine;

namespace SFramework
{
    [CreateAssetMenu(fileName = "ChannelConfig", menuName = "创建Asset/渠道配置", order = 1)]
    public class ChannelConfig : ScriptableObject
    {
        public List<ChannelItem> ChannelList = new List<ChannelItem>();
        
        private ChannelItem mCurChannel;

        #region Singleton
        private static ChannelConfig _instance;
        public static ChannelConfig Singleton
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Resources.Load<ChannelConfig>("ChannelConfig");
                    _instance.mCurChannel = null;
                    // TODO
                        // _instance.ChannelList.Find((p) => p.PackageType == GameConfig.CurPackageEnum);
                }
                return _instance;
            }
        }

        public ChannelItem CurChannel => Singleton.mCurChannel;

        #endregion
    }

    [System.Serializable]
    public class ChannelItem
    {
        /// <summary>
        /// 渠道类型
        /// </summary>
        public PackageEnum PackageType;
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
        /// 资源地址
        /// </summary>
        public string AssetURL;


        /// <summary>
        /// 启用分享
        /// </summary>
        public bool ShareEnable;
    }
    
    /// <summary>
    /// 渠道枚举
    /// </summary>
    public enum PackageEnum
    {
        Develop, //开发服
        Android,
        IOS,
        BiLi,
    }
}