//-------------------------------------------------
//            TweenKit
// Copyright © 2020 tonychenn.cn
//-------------------------------------------------

using UnityEngine;
using UnityEditor;

namespace TweenKit
{
    [CustomEditor(typeof(UITweener), true)]
    public class UITweenerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            GUILayout.Space(6f);
            TweenKitEditorTools.SetLabelWidth(110f);
            base.OnInspectorGUI();
            DrawCommonProperties();
        }

        protected void DrawCommonProperties()
        {
            UITweener tw = target as UITweener;

            if (TweenKitEditorTools.DrawHeader("Tweener"))
            {
                TweenKitEditorTools.BeginContents();
                TweenKitEditorTools.SetLabelWidth(110f);

                GUI.changed = false;
                
                GUILayout.BeginHorizontal();
                bool autoPlay = EditorGUILayout.Toggle("Auto Play", tw.autoPlay, GUILayout.Width(170));
                GUILayout.EndHorizontal();

                UITweener.Style style = (UITweener.Style)EditorGUILayout.EnumPopup("Play Style", tw.style);
                AnimationCurve curve = EditorGUILayout.CurveField("Animation Curve", tw.animationCurve, GUILayout.Width(170f), GUILayout.Height(62f));
                //UITweener.Method method = (UITweener.Method)EditorGUILayout.EnumPopup("Play Method", tw.method);

                GUILayout.BeginHorizontal();
                float dur = EditorGUILayout.FloatField("Duration", tw.duration, GUILayout.Width(170f));
                GUILayout.Label("seconds");
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                float del = EditorGUILayout.FloatField("Start Delay", tw.delay, GUILayout.Width(170f));
                GUILayout.Label("seconds");
                GUILayout.EndHorizontal();

                int tg = EditorGUILayout.IntField("Tween Group", tw.tweenGroup, GUILayout.Width(170f));
                bool ts = EditorGUILayout.Toggle("Ignore TimeScale", tw.ignoreTimeScale);
                bool fx = EditorGUILayout.Toggle("Use Fixed Update", tw.useFixedUpdate);

                if (GUI.changed)
                {
                    TweenKitEditorTools.RegisterUndo("Tween Change", tw);
                    tw.autoPlay = autoPlay;
                    tw.animationCurve = curve;
                    //tw.method = method;
                    tw.style = style;
                    tw.ignoreTimeScale = ts;
                    tw.tweenGroup = tg;
                    tw.duration = dur;
                    tw.delay = del;
                    tw.useFixedUpdate = fx;
                    TweenKitTools.SetDirty(tw);
                }
                TweenKitEditorTools.EndContents();
            }

            TweenKitEditorTools.SetLabelWidth(80f);
            TweenKitEditorTools.DrawEvents("On Finished", tw, tw.onFinished);
        }
    }
}


