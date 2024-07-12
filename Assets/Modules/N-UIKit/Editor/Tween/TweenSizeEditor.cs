//-------------------------------------------------
//            TweenKit
// Copyright © 2020 tonychenn.cn
//-------------------------------------------------

using UnityEngine;
using UnityEditor;

namespace TweenKit
{
    [CustomEditor(typeof(TweenSize))]
    public class TweenSizeEditor : UITweenerEditor
    {
        public override void OnInspectorGUI()
        {
            GUILayout.Space(6f);
            TweenKitEditorTools.SetLabelWidth(120f);

            TweenPosition tw = target as TweenPosition;
            GUI.changed = false;
            Vector2 def = EditorGUILayout.Vector3Field("default", tw.def);
            Vector2 from = EditorGUILayout.Vector3Field("From", tw.from);
            Vector2 to = EditorGUILayout.Vector3Field("To", tw.to);

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