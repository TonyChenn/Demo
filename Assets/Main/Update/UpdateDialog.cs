using UnityEngine;
using UnityEngine.UI;
using System;




#if UNITY_IOS || UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
using System.Runtime.InteropServices;
#endif

public class UpdateDialog : MonoBehaviour
{
	[SerializeField] Text tipText;
	[SerializeField] Transform group1;
	[SerializeField] Button btnOK;
	[SerializeField] Transform group2;
	[SerializeField] Button btnYes;
	[SerializeField] Button btnNo;


#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
	[DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
	private static extern IntPtr GetForegroundWindow();

	/// <summary>
	/// 0		0 确定
	/// 1		1 确定 2 取消
	/// 2		3 终止 4 重试 5 忽略
	/// 3		6 是 7 否 2 取消
	/// 4		6 是 7 否
	/// 5		4 重试 2 取消
	/// 6		2 取消 10重试 11继续
	/// </summary>
	[DllImport("User32.dll", SetLastError = true, ThrowOnUnmappableChar = true, CharSet = CharSet.Auto)]
	private static extern int MessageBox(IntPtr handle, String message, String title, int type);

	private static int ShowMessageBox(String message, String title, int type) => MessageBox(GetForegroundWindow(), message, title, type);
#elif UNITY_ANDROID && !UNITY_EDITOR
	private static AndroidJavaClass unityPlayer = null;
	private static AndroidJavaObject unityActivity = null;
	private class DialogInterfaceOnClickListener : AndroidJavaProxy
	{
		private readonly Action<AndroidJavaObject, int> action = null;
		public DialogInterfaceOnClickListener(Action<AndroidJavaObject, int> callback)
			: base("android.content.DialogInterface$OnClickListener")
		{ action = callback; }
		void onClick(AndroidJavaObject dialogInterface, int arg1) { action?.Invoke(dialogInterface, arg1); }
	}
#endif

	public static void ShowDialog(string tip, Action onYes = null, Action onNo = null)
	{
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
		if (onYes != null && onNo != null)
		{
			int result = ShowMessageBox(tip, "提示", 1);
			Action action = result == 1 ? onYes : onNo;
			action?.Invoke();
		}
		else
		{
			int result = ShowMessageBox(tip, "提示", 0);
			if (result == 0) onYes?.Invoke();
		}
#elif UNITY_ANDROID && !UNITY_EDITOR
		unityPlayer ??= new AndroidJavaClass("com.unity3d.player.UnityPlayer");
		unityActivity ??= unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
		AndroidJavaObject builder = new AndroidJavaObject("android.app.AlertDialog$Builder", unityActivity);
		builder.Call<AndroidJavaObject>("setTitle", "提示");
		builder.Call<AndroidJavaObject>("setMessage", tip);
		builder.Call<AndroidJavaObject>("setCancelable", false);
		if (onYes != null)
		{
			builder.Call<AndroidJavaObject>("setPositiveButton", "确定", new DialogInterfaceOnClickListener((dialog, i) => { onYes?.Invoke(); dialog.Call("dismiss"); }));
		}
		if(onNo != null)
		{
			builder.Call<AndroidJavaObject>("setNegativeButton", "取消", new DialogInterfaceOnClickListener((dialog, i) => { onNo?.Invoke(); dialog.Call("dismiss"); }));
		}

		unityActivity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
		{
			builder.Call<AndroidJavaObject>("create").Call("show");
		}));
#else
		GameObject prefab = Resources.Load<GameObject>("UpdateDialog");
		UpdateDialog ui = Instantiate(prefab, Vector3.zero, Quaternion.identity, UIRoot.CommonLayer).GetComponent<UpdateDialog>();
		ui.tipText.text = tip;

		if (onYes != null && onNo != null)
		{
			ui.group1.gameObject.SetActive(false);
			ui.group2.gameObject.SetActive(true);
			ui.btnYes.onClick.AddListener(() => { onYes?.Invoke(); Destroy(ui.gameObject); });
			ui.btnNo.onClick.AddListener(() =>
			{
				onNo?.Invoke();
				Destroy(ui.gameObject);
			});
		}
		else
		{
			ui.group1.gameObject.SetActive(true);
			ui.group2.gameObject.SetActive(false);
			ui.btnOK.onClick.AddListener(() => { onYes?.Invoke(); Destroy(ui.gameObject); });
		}
#endif
	}
}
