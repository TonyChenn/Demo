//-------------------------------------------------
//            TweenKit
// Copyright © 2020 tonychenn.cn
//-------------------------------------------------

using UnityEngine;
using UnityEditor;

namespace TweenKit
{
    [CustomEditor(typeof(TweenRotation))]
    public class TweenRotationEditor : UITweenerEditor
    {
        public override void OnInspectorGUI()
        {
            GUILayout.Space(6f);
            TweenKitEditorTools.SetLabelWidth(120f);

            TweenRotation tw = target as TweenRotation;
            GUI.changed = false;

            Vector3 def = EditorGUILayout.Vector3Field("Default", tw.def);
            Vector3 from = EditorGUILayout.Vector3Field("From", tw.from);
            Vector3 to = EditorGUILayout.Vector3Field("To", tw.to);
            var quat = EditorGUILayout.Toggle("Quaternion", tw.quaternionLerp);

            if (GUI.changed)
            {
                TweenKitEditorTools.RegisterUndo("Tween Change", tw);
                tw.def = def;
                tw.from = from;
                tw.to = to;
                tw.quaternionLerp = quat;
                TweenKitTools.SetDirty(tw);
            }

            DrawCommonProperties();
        }
    }
}

