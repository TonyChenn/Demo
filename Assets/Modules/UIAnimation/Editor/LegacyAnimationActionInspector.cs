using UnityEngine;
using UnityEditor;

namespace UIAnimation.Editor
{
    [CustomEditor(typeof(Actions.LegacyAnimationAction))]
    public class LegacyAnimationActionInspector : UnityEditor.Editor
    {
        private Actions.LegacyAnimationAction legacyAnimationAction;

        void OnEnable()
        {
            legacyAnimationAction = target as Actions.LegacyAnimationAction;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.MaxWidth(250f));
            {
                EditorGUILayout.BeginHorizontal(GUILayout.Height(120f));
                {
                    EditorGUILayout.BeginVertical(GUILayout.Width(220f));
                    {
                        EditorGUIUtility.labelWidth = 90f;
                        legacyAnimationAction.Description = EditorGUILayout.TextField("Description", legacyAnimationAction.Description);
                        legacyAnimationAction.Clip = EditorGUILayout.ObjectField("Clip", legacyAnimationAction.Clip, typeof(AnimationClip), true) as AnimationClip;
                        if (legacyAnimationAction.Clip != null)
                        {
                            legacyAnimationAction.ClipWrapMode = (WrapMode)EditorGUILayout.EnumPopup("Wrap Mode", legacyAnimationAction.ClipWrapMode);
                            EditorGUILayout.LabelField("Duration: " + string.Format("{0}秒", legacyAnimationAction.Clip.length / legacyAnimationAction.Speed));
                            EditorGUILayout.LabelField("FrameRate: " + string.Format("{0}fps", legacyAnimationAction.Clip.frameRate));
                        }
                        legacyAnimationAction.DelaySeconds = EditorGUILayout.FloatField("Delay Seconds", legacyAnimationAction.DelaySeconds);
                    }
                    EditorGUILayout.EndVertical();
                }
                EditorGUILayout.EndHorizontal();

                EditorGUIUtility.fieldWidth = 30f;
                legacyAnimationAction.Speed = EditorGUILayout.Slider("Speed", legacyAnimationAction.Speed, -10f, 10f);
            }
            EditorGUILayout.EndVertical();

            serializedObject.ApplyModifiedProperties();
        }

    }
}