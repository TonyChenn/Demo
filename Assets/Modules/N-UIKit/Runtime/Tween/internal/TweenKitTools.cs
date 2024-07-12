//-------------------------------------------------
//            TweenKit
// Copyright © 2020 tonychenn.cn
//-------------------------------------------------

using UnityEngine;
using System;

namespace TweenKit
{
    public class DoNotObfuscateNGUI : Attribute { }

    /// <summary>
    /// Helper class containing generic functions used throughout the UI library.
    /// </summary>

    static public class TweenKitTools
    {
        /// <summary>
        /// Convenience function that converts Class + Function combo into Class.Function representation.
        /// </summary>

        static public string GetFuncName(object obj, string method)
        {
            if (obj == null) return "<null>";
            string type = obj.GetType().ToString();
            int period = type.LastIndexOf('/');
            if (period > 0) type = type.Substring(period + 1);
            return string.IsNullOrEmpty(method) ? type : type + "/" + method;
        }
        /// <summary>
        /// Extension for the game object that checks to see if the component already exists before adding a new one.
        /// If the component is already present it will be returned instead.
        /// </summary>

        static public T AddMissingComponent<T>(this GameObject go) where T : Component
        {
#if UNITY_FLASH
		    object comp = go.GetComponent<T>();
#else
            T comp = go.GetComponent<T>();
#endif
            if (comp == null)
            {
#if UNITY_EDITOR
                if (!Application.isPlaying)
                    RegisterUndo(go, "Add " + typeof(T));
#endif
                comp = go.AddComponent<T>();
            }
#if UNITY_FLASH
		    return (T)comp;
#else
            return comp;
#endif
        }
        /// <summary>
        /// Convenience method that works without warnings in both Unity 3 and 4.
        /// </summary>

        static public void RegisterUndo(UnityEngine.Object obj, string name)
        {
#if UNITY_EDITOR
            UnityEditor.Undo.RecordObject(obj, name);
            TweenKitTools.SetDirty(obj);
#endif
        }

        /// <summary>
        /// Convenience function that marks the specified object as dirty in the Unity Editor.
        /// </summary>

        static public void SetDirty(UnityEngine.Object obj, string undoName = "last change")
        {
#if UNITY_EDITOR
#if UNITY_2018_3_OR_NEWER
            if (obj)
            {
                UnityEditor.EditorUtility.SetDirty(obj);

                if (!UnityEditor.AssetDatabase.Contains(obj) && !Application.isPlaying)
                {
                    if (obj is Component)
                    {
                        var component = (Component)obj;
                        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(component.gameObject.scene);
                    }
                    else if (!(obj is UnityEditor.EditorWindow || obj is ScriptableObject))
                    {
                        UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
                    }
                }
            }
#else
		    if (obj) UnityEditor.EditorUtility.SetDirty(obj);
#endif
#endif
        }
    }
}

