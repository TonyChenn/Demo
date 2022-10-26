using System;
using UnityEngine;

/// <summary>
/// Dialog
/// </summary>
public partial class NSDK
{

    public static class Dialog
    {
        public static void Show(string title, string msg, Action yesAction = null, Action noAction = null)
        {
            Show(title, msg, "确定", yesAction, "取消", noAction, false);
        }
        public static void Show(string title, string msg, string str_yes, Action yesAction, string str_no, Action noAction, bool cancelble)
        {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
            Alert.MessageBox(IntPtr.Zero, msg, title, 1);
#elif UNITY_ANDROID
            RunOnUIThread(new AndroidJavaRunnable(() =>
            {
                AndroidJavaObject dialog = null;
                using (AndroidJavaObject builder = new AndroidJavaObject("android.app.AlertDialog$Builder", CurrentActivity))
                {
                    builder.Call<AndroidJavaObject>("setTitle", title).Dispose();
                    builder.Call<AndroidJavaObject>("setMessage", msg).Dispose();
                    builder.Call<AndroidJavaObject>("setCancelable", cancelble).Dispose();
                    if (!string.IsNullOrEmpty(str_yes) || yesAction != null)
                        builder.Call<AndroidJavaObject>("setPositiveButton", str_yes, new OnClickListener(yesAction)).Dispose();

                    if (!string.IsNullOrEmpty(str_no) || noAction != null)
                        builder.Call<AndroidJavaObject>("setNegativeButton", str_no, new OnClickListener(noAction)).Dispose();

                    dialog = builder.Call<AndroidJavaObject>("create");
                }
                dialog.Call("show");
                dialog.Dispose();
            }));
#endif
        }


        #region 内部类(不同平台的工具类)
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
        class Alert
        {
            [System.Runtime.InteropServices.DllImport("User32.dll", SetLastError = true, ThrowOnUnmappableChar = true, CharSet = System.Runtime.InteropServices.CharSet.Auto)]
            public static extern int MessageBox(IntPtr handle, String message, String title, int type);
        }
#elif UNITY_ANDROID
        class OnClickListener : AndroidJavaProxy
        {
            public readonly Action Callback;
            public OnClickListener(Action callback) : base("android.content.DialogInterface$OnClickListener")
            {
                Callback = callback;
            }
            public void onClick(AndroidJavaObject dialog, int id)
            {
                dialog.Call("dismiss");
                Callback?.Invoke();
            }
        }
#endif
        #endregion
    }
}
