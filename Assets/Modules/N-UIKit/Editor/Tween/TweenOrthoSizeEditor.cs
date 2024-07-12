//-------------------------------------------------
//            TweenKit
// Copyright © 2020 tonychenn.cn
//-------------------------------------------------

using UnityEngine;
using UnityEditor;

namespace TweenKit
{
    [CustomEditor(typeof(TweenOrthoSize))]
    public class TweenOrthoSizeEditor : UITweenerEditor
    {
        public override void OnInspectorGUI()
        {
            GUILayout.Space(6f);
            TweenKitEditorTools.SetLabelWidth(120f);

            TweenOrthoSize tw = target as TweenOrthoSize;
            GUI.changed = false;

            float from = EditorGUILayout.FloatField("From", tw.from);
            float to = EditorGUILayout.FloatField("To", tw.to);

            if (from < 0f) from = 0f;
            if (to < 0f) to = 0f;

            if (GUI.changed)
            {
                TweenKitEditorTools.RegisterUndo("Tween Change", tw);
                tw.from = from;
                tw.to = to;
                TweenKitTools.SetDirty(tw);
            }

            DrawCommonProperties();
        }
    }
}

