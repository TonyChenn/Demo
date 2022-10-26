using NDebug;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Toast
/// </summary>
public partial class NSDK
{
#if UNITY_EDITOR || UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || UNITY_STANDALONE_LINUX

#elif UNITY_ANDROID
#elif UNITY_IOS

#endif

    public static class Toast
    {
        public static void Show(string msg)
        {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || UNITY_STANDALONE_LINUX
            Log.Info("Toast: >>> " + msg);
#elif UNITY_ANDROID
            RunOnUIThread(new AndroidJavaRunnable(() =>
            {
                using(AndroidJavaClass toastClass = new AndroidJavaClass("android.widget.Toast"))
                {
                    toastClass.CallStatic<AndroidJavaObject>("makeText", CurrentActivity, msg, 0).Call("show");
                }
            }));
#elif UNITY_IOS
            Log.Info("Toast: >>> " + msg);
#else
            Log.Info("Toast: >>> " + msg);
#endif
        }
    }
}
