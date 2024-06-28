using UnityEngine;
using UnityEngine.UI;

public class UI_Update : MonoBehaviour
{
	[SerializeField] Text tipText;
	[SerializeField] Button btnYes;
	[SerializeField] Button btnNo;

#if UNITY_ANDROID
	private static AndroidJavaClass unityPlayer = null;
	private static AndroidJavaObject currentActivity = null;
	private class DialogInterfaceOnClickListener : AndroidJavaProxy
	{
		private readonly System.Action<AndroidJavaObject, int> action = null;
		public DialogInterfaceOnClickListener(System.Action<AndroidJavaObject, int> callback)
			: base("android.content.DialogInterface$OnClickListener")
		{ action = callback; }
		void onClick(AndroidJavaObject dialogInterface, int arg1) { action?.Invoke(dialogInterface, arg1); }
	}
#endif

	public static void ShowDialog(string tip, System.Action onYes=null, System.Action onNo = null)
	{
#if UNITY_ANDROID
		unityPlayer ??= new AndroidJavaClass("com.unity3d.player.UnityPlayer");
		currentActivity ??= unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
		using AndroidJavaObject builder = new AndroidJavaObject("android.app.AlertDialog$Builder", currentActivity);
		builder.Call<AndroidJavaObject>("setTitle", "更新提示");
		builder.Call<AndroidJavaObject>("setMessage", tip);
		builder.Call<AndroidJavaObject>("setPositiveButton", "确定", new DialogInterfaceOnClickListener((dialog, i) => { onYes?.Invoke(); dialog.Call("dismiss"); }));
		builder.Call<AndroidJavaObject>("setNegativeButton", "取消", new DialogInterfaceOnClickListener((dialog, i) => { onNo?.Invoke(); dialog.Call("dismiss"); }));
		
		currentActivity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
		{
			using AndroidJavaObject dialog = builder.Call<AndroidJavaObject>("create");
			dialog.Call("show");
		}));
#endif
		GameObject prefab = Resources.Load<GameObject>("UI_Update");
		UI_Update ui = Instantiate(prefab).GetComponent<UI_Update>();
		ui.tipText.text = tip;
		ui.btnYes.onClick.AddListener(() => { onYes?.Invoke(); Destroy(ui.gameObject); });
		ui.btnNo.onClick.AddListener(() => { onNo?.Invoke(); Destroy(ui.gameObject); });
	}
}
