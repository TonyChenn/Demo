using NCore;
//using ResKit;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UObject = UnityEngine.Object;

namespace SFramework.Runtime
{
    public class UIManager : NormalSingleton<UIManager>
    {
        // 加载过的所有UI
        static Dictionary<UIFlag, UIWndBase> allUIWndDict = new Dictionary<UIFlag, UIWndBase>(16);
        // 当前显示的UI
        static List<UIFlag> curShowUIList = new List<UIFlag>();

        static List<UIFlag> curCacheUIList = new List<UIFlag>();
        private UIManager() { }

        #region API
        public static void ShowUIAsync(UIPackage package, UIFlag uiFlag, object exData = null)
        {
            // 打开wait


            // 关闭await
        }
        async void prepareUIAsync(UIPackage package, UIFlag uiFlag)
        {
            if (package == UIPackage.None || uiFlag == UIFlag.None) return;

            UIWndBase wnd = GetUIWnd(uiFlag);
            if (wnd == null && !IsUICaching(uiFlag))
            {
                curCacheUIList.Add(uiFlag);
            }
        }



        public static void HideUIWnd(UIFlag uiFlag)
        {
            HideUIWnd(new UIFlag[] { uiFlag });
        }
        public static void HideUIWnd(UIFlag[] hideWndArray)
        {
            if (hideWndArray == null) return;

            for (int i = 0, iMax = hideWndArray.Length; i < iMax; i++)
            {
                UIFlag temp = hideWndArray[i];
                if (IsUIShowing(temp))
                {
                    curShowUIList.Remove(temp);
                    UIWndBase wnd = GetUIWnd(temp);
                    if (wnd != null)
                    {
                        wnd.OnHideWnd();
                        DestoryWnd(temp);
                    }
                }
            }
        }


        public static void DestoryWnd(UIFlag uiFlag)
        {
            if (allUIWndDict.ContainsKey(uiFlag))
            {
                GameObject.Destroy(allUIWndDict[uiFlag].CachedGameObject);
                allUIWndDict.Remove(uiFlag);
            }
        }

        public static UIWndBase GetUIWnd(UIFlag uiFlag)
        {
            if (uiFlag == UIFlag.None) return null;

            if (allUIWndDict.ContainsKey(uiFlag))
            {
                return allUIWndDict[uiFlag];
            }

            return null;
        }

        public static bool IsUIShowing(UIFlag uiFlag)
        {
            if (uiFlag == UIFlag.None) return false;

            return curShowUIList.Contains(uiFlag);
        }

        public static bool IsUICaching(UIFlag uiFlag)
        {
            if (uiFlag == UIFlag.None) return false;

            return curCacheUIList.Contains(uiFlag);
        }
        #endregion

        public static void LoadCommonWnd()
        {
            //ResLoader loader = new ResLoader();
            //loader.LoadAsync<GameObject>("ui_message", "ui_wait", (obj) =>
            //{

            //});
        }
    }
}