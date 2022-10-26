using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class NSDK
{
#if UNITY_EDITOR || UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || UNITY_STANDALONE_LINUX

#elif UNITY_ANDROID
    private static AndroidJavaClass unityPlayer = null;

    public static AndroidJavaClass UnityPlayer
    {
        get
        {
            if (unityPlayer == null)
                unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");

            return unityPlayer;
        }
    }


    private static AndroidJavaObject currentActivity = null;
    public static AndroidJavaObject CurrentActivity
    {
        get
        {
            if (currentActivity == null)
                currentActivity = UnityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

            return currentActivity;
        }
    }

    public static void RunOnUIThread(AndroidJavaRunnable runnable)
    {
        CurrentActivity.Call("runOnUiThread", runnable);
    }
#elif UNITY_IOS

#endif
}
