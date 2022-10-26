using UnityEngine;

namespace SFramework
{
    public static class CommonUtil
    {
        /// <summary>
        /// 复制文本到剪切板
        /// </summary>
        public static void CopyText(string text)
        {
            GUIUtility.systemCopyBuffer = text;
        }
    }
}