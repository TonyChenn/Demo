using UnityEngine;

namespace SFramework
{
    public class GameDefine
    {
        public static void Init()
        {
            // 屏幕常亮
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            
            // Other Init
        }
    }
}