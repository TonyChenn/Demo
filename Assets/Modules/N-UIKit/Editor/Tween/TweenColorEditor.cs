//-------------------------------------------------
//            TweenKit
// Copyright © 2020 tonychenn.cn
//-------------------------------------------------
using UnityEngine;
using UnityEditor;

namespace TweenKit
{
    [CustomEditor(typeof(TweenColor))]
    public class TweenColorEditor : UITweenerEditor
    {
        public override void OnInspectorGUI()
        {
            GUILayout.Space(6f);
            TweenKitEditorTools.SetLabelWidth(120f);

            TweenColor tw = target as TweenColor;
            GUI.changed = false;

            Color def = EditorGUILayout.ColorField("Default", tw.def);
            Color from = EditorGUILayout.ColorField("From", tw.from);
            Color to = EditorGUILayout.ColorField("To", tw.to);

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

