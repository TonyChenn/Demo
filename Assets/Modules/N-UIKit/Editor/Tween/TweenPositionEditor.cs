//-------------------------------------------------
//            TweenKit
// Copyright © 2020 tonychenn.cn
//-------------------------------------------------

using UnityEngine;
using UnityEditor;

namespace TweenKit
{
    [CustomEditor(typeof(TweenPosition))]
    public class TweenPositionEditor : UITweenerEditor
    {
        public override void OnInspectorGUI()
        {
            GUILayout.Space(6f);
            TweenKitEditorTools.SetLabelWidth(120f);

            TweenPosition tw = target as TweenPosition;
            GUI.changed = false;
            Vector3 def = EditorGUILayout.Vector3Field("default", tw.def);
            Vector3 from = EditorGUILayout.Vector3Field("From", tw.from);
            Vector3 to = EditorGUILayout.Vector3Field("To", tw.to);

            if (GUI.changed)
            {
                TweenKitEditorTools.RegisterUndo("Tween Change", tw);
                tw.def = def;
                tw.from = from;
                tw.to = to;
                TweenKitTools.SetDirty(tw);
            }

            DrawCommonProperties();
        }
    }
}

