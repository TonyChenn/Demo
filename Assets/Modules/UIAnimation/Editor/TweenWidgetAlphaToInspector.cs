using UnityEditor;
using UnityEngine;

namespace UIAnimation.Editor
{
    [CustomEditor(typeof(Tween.TweenWidgetAlphaTo))]
    public class TweenWidgetAlphaToInspector : UnityEditor.Editor
    {
        private Tween.TweenWidgetAlphaTo tw;

        void OnEnable()
        {
            tw = target as Tween.TweenWidgetAlphaTo;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            TweenActionCommonInspector.DrawTweenActionBase(tw);

            DrawTweenAlphaTo();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawTweenAlphaTo()
        {
            EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.MaxWidth(274f));
            {
                EditorGUIUtility.labelWidth = 90f;
                tw.ToAlpha = EditorGUILayout.Slider("To Alpha", tw.ToAlpha, 0f, 1f);
            }
            EditorGUILayout.EndVertical();
        }
    }
}
