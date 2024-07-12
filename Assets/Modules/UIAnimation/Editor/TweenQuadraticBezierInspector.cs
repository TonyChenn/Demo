using UnityEngine;
using UnityEditor;

namespace UIAnimation.Editor
{
    [CustomEditor(typeof(Tween.TweenQuadraticBezier))]
    public class TweenQuadraticBezierInspector : UnityEditor.Editor
    {
        private Tween.TweenQuadraticBezier tw;

        void OnEnable()
        {
            tw = target as Tween.TweenQuadraticBezier;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            TweenActionCommonInspector.DrawTweenActionBase(tw);

            DrawTweenMoveTo();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawTweenMoveTo()
        {
            EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.MaxWidth(274f));
            {
                EditorGUIUtility.labelWidth = 90f;
                tw.EndPosition = EditorGUILayout.Vector3Field("End Position", tw.EndPosition);
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.MaxWidth(274f));
            {
                EditorGUIUtility.labelWidth = 90f;
                tw.ControlPosition = EditorGUILayout.Vector3Field("Control Position", tw.ControlPosition);
            }
            EditorGUILayout.EndVertical();
        }
    }
}