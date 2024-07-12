using NCore;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace UIKit
{
	public class UIManager : NormalSingleton<UIManager>
	{
		// 正在加载的界面
		private List<UIFlag> m_CurLoadingList = new(4);
		// 当前显示的所有界面
		private List<UIFlag> m_CurShowUIID = new(8);
		// 所有加载过的界面
		private Dictionary<UIFlag, UIWndBase> m_AllUIIDDict = new(16);

		public List<UIFlag> CurShowUIID { get { return m_CurShowUIID; } }
		public Dictionary<UIFlag, UIWndBase> AllUIWnd { get { return m_AllUIIDDict; } }


		private static BundleLoader loader = BundleLoader.Alloc();



		private UIManager() { }

		~UIManager() { loader.Recycle(); }
		private UIWndBase GetUIWnd(UIFlag flag)
		{
			if (flag == UIFlag.None) return null;
			if (!AllUIWnd.ContainsKey(flag)) return null;

			return AllUIWnd[flag];
		}

		private async void PrepareUIAsync(UIFlag flag)
		{
			if (flag == UIFlag.None) return;
			UIWndBase wnd = GetUIWnd(flag);
			if (wnd != null || Singleton.m_CurLoadingList.Contains(flag)) return;

			Singleton.m_CurLoadingList.Add(flag);
			UIPackage package = flag.GetUIPackage();
			BundleRes res = await loader.LoadAsync($"ui/prefabs/{package}/{flag}");
		}
		private void RegisterWnd(UIWndBase wnd)
		{
			if (AllUIWnd.ContainsKey(wnd.UIID))
				throw new Exception("ui is already register!!!!->" + wnd.UIID.ToString());
			else
			{
				AllUIWnd[wnd.UIID] = wnd;
				//m_LoadUIIDs.Remove(wnd.UIID);
			}
		}

		private UIWndBase loadUIWnd(UIFlag uiid, UIDeep deep)
		{
			UIWndBase wnd = GetUIWnd(uiid);
			if (wnd == null)
			{
			}
			//if (wnd == null && !m_LoadUIIDs.Contains(uiid))
			//{
			//    //TODO 加载资源

			//    m_LoadUIIDs.Add(uiid);
			//    string path = $"{Application.streamingAssetsPath}/ui/{pkg.ToString()}.u";
			//    AssetBundle bundle = AssetBundle.LoadFromFile(path);
			//    var obj = bundle.LoadAsset<GameObject>(uiid.ToString());
			//    obj.SetParent(GetWndGroup(deep).gameObject);

			//    return obj.GetComponent<UIWndBase>();
			//}
			return wnd;
		}


		public static void ShowUISync(UIFlag flag, UIDeep deep = UIDeep.CommonLayer, dynamic exData = null)
		{
			if (!Singleton.m_CurLoadingList.Contains(flag))
			{
			}
		}

		public static void ShowUIAsync(UIFlag flag, UIDeep deep = UIDeep.CommonLayer, dynamic exData = null)
		{
			UIManager.ShowUIWait(true);

			UIManager.ShowUIWait(false);
		}
		public static void HideUIWnd(UIFlag flag)
		{
			HideUIWnd(new UIFlag[] { flag });
		}
		public static void HideUIWnd(UIFlag[] flag)
		{
			if (flag == null || flag.Length == 0) return;
			for (int i = 0,iMax = flag.Length; i < iMax; i++)
			{
				if (!Singleton.CurShowUIID.Contains(flag[i])) return;
				
				UIWndBase wnd = Singleton.GetUIWnd(flag[i]);
				if (wnd == null) continue;

				Singleton.CurShowUIID.Remove(flag[i]);
				Singleton.AllUIWnd.Remove(flag[i]);

				wnd.OnHideWnd();
				GameObject.Destroy(wnd.CachedGameObject);
			}
		}


		public void Init()
		{
			if (UIDialogObj == null)
			{
				Transform parent = UIRoot.PopLayer;
				var bundle = loader.LoadSync("ui/prefabs/ui_common").Bundle;


				var prefab = bundle.LoadAsset<GameObject>("ui_dialog");
				UIDialogObj = GameObject.Instantiate(prefab, Vector3.zero, Quaternion.identity, parent);
				UIDialogObj.name = "ui_dialog";
				UIDialogPanel = UIDialogObj.GetComponent<DialogPanel>();

				prefab = bundle.LoadAsset<GameObject>("ui_wait");
				UIWaitObj = GameObject.Instantiate(prefab, Vector3.zero, Quaternion.identity,parent);
				UIWaitObj.name = "ui_wait";
				UIWaitPanel = Singleton.UIWaitObj.GetComponent<WaitPanel>();


				prefab = bundle.LoadAsset<GameObject>("ui_toast");
				UIToastObj = GameObject.Instantiate(prefab, Vector3.zero, Quaternion.identity, parent);
				UIToastObj.name = "ui_toast";
				UIToastPanel = Singleton.UIToastObj.GetComponent<ToastPanel>();
			}
		}

		#region UIDialog/UIWait/UIToast
		private GameObject UIDialogObj = null;
		private GameObject UIWaitObj = null;
		private GameObject UIToastObj = null;

		private DialogPanel UIDialogPanel = null;
		private WaitPanel UIWaitPanel = null;
		private ToastPanel UIToastPanel = null;

		// Dialog
		public static void ShowDialog(string content) => ShowDialog(null, null);
		public static void ShowDialog(string content, Action yesAction, string strYes = null)
		{
			ShowDialog(null, content, yesAction, strYes, null, null);
		}
		public static void ShowDialog(string content, Action yesAction, Action noAction)
		{
			ShowDialog(null, content, yesAction, null, noAction, null);
		}
		public static void ShowDialog(string content, Action yesAction, string strYes, Action noAction, string strNo)
		{
			ShowDialog(null, content, yesAction, strYes, noAction, strNo);
		}
		public static void ShowDialog(string title, string content)
		{
			ShowDialog(title, content, null);
		}
		public static void ShowDialog(string title, string content, Action yesAction, string strYes = null)
		{
			ShowDialog(title, content, yesAction, strYes, null, null);
		}
		public static void ShowDialog(string title, string content, Action yesAction, Action noAction)
		{
			ShowDialog(title, content, yesAction, null, noAction, null);
		}
		public static void ShowDialog(string title, string content, Action yesAction, string strYes, Action noAction, string strNo, bool mask = true)
		{
			Singleton.UIDialogPanel.ShowDialog(title, content, yesAction, strYes, noAction, strNo, mask);
		}

		// UIWait
		public static void ShowUIWait(bool show) => Singleton.UIWaitPanel.Show(show);
		public static void HideUIWaitForce() => Singleton.UIWaitPanel.HideForce();

		// Toast
		public static void ShowToast(string msg, int millisecondsDelay = 1000) => Singleton.UIToastPanel.Show(msg, millisecondsDelay);
		#endregion
	}
}

