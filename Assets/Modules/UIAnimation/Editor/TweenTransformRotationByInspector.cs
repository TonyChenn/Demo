using UnityEngine;
using UnityEditor;

namespace UIAnimation.Editor
{
    [CustomEditor(typeof(Tween.TweenTransformRotationBy))]
    public class TweenTransformRotationByInspector : UnityEditor.Editor
    {
        private Tween.TweenTransformRotationBy tw;

        void OnEnable()
        {
            tw = target as Tween.TweenTransformRotationBy;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            TweenActionCommonInspector.DrawTweenActionBase(tw);

            DrawTweenRotation();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawTweenRotation()
        {
            EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.MaxWidth(274f));
            {
                EditorGUIUtility.labelWidth = 90f;
                tw.DeltaVec3 = EditorGUILayout.Vector3Field("Delta Vec3", tw.DeltaVec3);
            }
            EditorGUILayout.EndVertical();
        }

    }

}