//-------------------------------------------------
//            TweenKit
// Copyright © 2020 tonychenn.cn
//-------------------------------------------------

using UnityEngine;
using UnityEditor;

namespace TweenKit
{
    [CustomEditor(typeof(TweenScale))]
    public class TweenScaleEditor : UITweenerEditor
    {
        public override void OnInspectorGUI()
        {
            GUILayout.Space(6f);
            TweenKitEditorTools.SetLabelWidth(120f);

            TweenScale tw = target as TweenScale;
            GUI.changed = false;

            Vector3 def = EditorGUILayout.Vector3Field("From", tw.def);
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

