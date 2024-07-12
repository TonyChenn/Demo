//-------------------------------------------------
//            TweenKit
// Copyright © 2020 tonychenn.cn
//-------------------------------------------------

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TweenKit
{
    /// <summary>
    /// Tools for the editor
    /// </summary>

    static public class TweenKitEditorTools
    {
        /// <summary>
        /// Unity 4.3 changed the way LookLikeControls works.
        /// </summary>

        static public void SetLabelWidth(float width)
        {
            EditorGUIUtility.labelWidth = width;
        }
        /// <summary>
        /// Draw a distinctly different looking header label
        /// </summary>

        static public bool DrawMinimalisticHeader(string text)
        {
            return DrawHeader(text, text, false, true);
        }

        /// <summary>
        /// Draw a distinctly different looking header label
        /// </summary>

        static public bool DrawHeader(string text)
        {
            return DrawHeader(text, text, false, TweenKitSettings.minimalisticLook);
        }

        /// <summary>
        /// Draw a distinctly different looking header label
        /// </summary>

        static public bool DrawHeader(string text, string key)
        {
            return DrawHeader(text, key, false, TweenKitSettings.minimalisticLook);
        }

        /// <summary>
        /// Draw a distinctly different looking header label
        /// </summary>

        static public bool DrawHeader(string text, bool detailed)
        {
            return DrawHeader(text, text, detailed, !detailed);
        }

        /// <summary>
        /// Draw a distinctly different looking header label
        /// </summary>

        static public bool DrawHeader(string text, string key, bool forceOn, bool minimalistic)
        {
            bool state = EditorPrefs.GetBool(key, true);

            if (!minimalistic) GUILayout.Space(3f);
            if (!forceOn && !state) GUI.backgroundColor = new Color(0.8f, 0.8f, 0.8f);
            GUILayout.BeginHorizontal();
            GUI.changed = false;

            if (minimalistic)
            {
                if (state) text = "\u25BC" + (char)0x200a + text;
                else text = "\u25BA" + (char)0x200a + text;

                GUILayout.BeginHorizontal();
                GUI.contentColor = EditorGUIUtility.isProSkin ? new Color(1f, 1f, 1f, 0.7f) : new Color(0f, 0f, 0f, 0.7f);
                if (!GUILayout.Toggle(true, text, "PreToolbar2", GUILayout.MinWidth(20f))) state = !state;
                GUI.contentColor = Color.white;
                GUILayout.EndHorizontal();
            }
            else
            {
                text = "<b><size=11>" + text + "</size></b>";
                if (state) text = "\u25BC " + text;
                else text = "\u25BA " + text;
                if (!GUILayout.Toggle(true, text, "dragtab", GUILayout.MinWidth(20f))) state = !state;
            }

            if (GUI.changed) EditorPrefs.SetBool(key, state);

            if (!minimalistic) GUILayout.Space(2f);
            GUILayout.EndHorizontal();
            GUI.backgroundColor = Color.white;
            if (!forceOn && !state) GUILayout.Space(3f);
            return state;
        }
        /// <summary>
        /// Begin drawing the content area.
        /// </summary>

        static public void BeginContents()
        {
            BeginContents(TweenKitSettings.minimalisticLook);
        }

        private static bool mEndHorizontal = false;

    #if UNITY_4_7 || UNITY_5_5 || UNITY_5_6
	    static public string textArea = "AS TextArea";
    #else
        static public string textArea = "TextArea";
    #endif

        /// <summary>
        /// Begin drawing the content area.
        /// </summary>

        static public void BeginContents(bool minimalistic)
        {
            if (!minimalistic)
            {
                mEndHorizontal = true;
                GUILayout.BeginHorizontal();
                EditorGUILayout.BeginHorizontal(textArea, GUILayout.MinHeight(10f));
            }
            else
            {
                mEndHorizontal = false;
                EditorGUILayout.BeginHorizontal(GUILayout.MinHeight(10f));
                GUILayout.Space(10f);
            }
            GUILayout.BeginVertical();
            GUILayout.Space(2f);
        }

        /// <summary>
        /// End drawing the content area.
        /// </summary>

        static public void EndContents()
        {
            GUILayout.Space(3f);
            GUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

            if (mEndHorizontal)
            {
                GUILayout.Space(3f);
                GUILayout.EndHorizontal();
            }

            GUILayout.Space(3f);
        }
        /// <summary>
        /// Draw a list of fields for the specified list of delegates.
        /// </summary>

        static public void DrawEvents(string text, Object undoObject, List<EventDelegate> list)
        {
            DrawEvents(text, undoObject, list, null, null, false);
        }

        /// <summary>
        /// Draw a list of fields for the specified list of delegates.
        /// </summary>

        static public void DrawEvents(string text, Object undoObject, List<EventDelegate> list, bool minimalistic)
        {
            DrawEvents(text, undoObject, list, null, null, minimalistic);
        }

        /// <summary>
        /// Draw a list of fields for the specified list of delegates.
        /// </summary>

        static public void DrawEvents(string text, Object undoObject, List<EventDelegate> list, string noTarget, string notValid, bool minimalistic)
        {
            if (!TweenKitEditorTools.DrawHeader(text, text, false, minimalistic)) return;

            if (!minimalistic)
            {
                TweenKitEditorTools.BeginContents(minimalistic);
                GUILayout.BeginHorizontal();
                GUILayout.BeginVertical();

                EventDelegateEditor.Field(undoObject, list, notValid, notValid, minimalistic);

                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
                TweenKitEditorTools.EndContents();
            }
            else EventDelegateEditor.Field(undoObject, list, notValid, notValid, minimalistic);
        }
        /// <summary>
        /// Create an undo point for the specified object.
        /// </summary>

        static public void RegisterUndo(string name, Object obj) { if (obj != null) UnityEditor.Undo.RecordObject(obj, name); }

        static public void DrawPadding()
        {
            if (!TweenKitSettings.minimalisticLook)
                GUILayout.Space(18f);
        }

        /// <summary>
        /// Load the asset at the specified path.
        /// </summary>
        static public Object LoadAsset(string path)
        {
            if (string.IsNullOrEmpty(path)) return null;
            return AssetDatabase.LoadMainAssetAtPath(path);
        }
        /// <summary>
        /// Convenience function to load an asset of specified type, given the full path to it.
        /// </summary>
        static public T LoadAsset<T>(string path) where T : Object
        {
            Object obj = LoadAsset(path);
            if (obj == null) return null;

            T val = obj as T;
            if (val != null) return val;

            if (typeof(T).IsSubclassOf(typeof(Component)))
            {
                if (obj.GetType() == typeof(GameObject))
                {
                    GameObject go = obj as GameObject;
                    return go.GetComponent(typeof(T)) as T;
                }
            }
            return null;
        }
    }
}
