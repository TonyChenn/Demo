using UnityEngine;

namespace UnityEditor.UI
{
	[CustomEditor(typeof(Base64Image), true)]
	public class Base64ImageInspector : GraphicEditor
	{
		SerializedProperty m_Texture;
		SerializedProperty m_Base64Texture;
		SerializedProperty m_IsSupportedImage;
		SerializedProperty m_IsValidBase64;

		private string newTextAreaContent = "";
		private readonly Rect previousRect = new Rect(20, 5, 98, 98);

		protected override void OnEnable()
		{
			base.OnEnable();
			m_Texture = serializedObject.FindProperty("m_Texture");
			m_Base64Texture = serializedObject.FindProperty("m_Base64Texture");
			m_IsSupportedImage = serializedObject.FindProperty("m_IsSupportedImage");
			m_IsValidBase64 = serializedObject.FindProperty("m_IsValidBase64");
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();
			
			GUILayout.BeginHorizontal();
			EditorGUI.DrawTextureTransparent(previousRect, (target as Base64Image).mainTexture);
			GUILayout.Space(105);

			EditorGUI.BeginChangeCheck();
			newTextAreaContent = EditorGUILayout.TextArea(m_Base64Texture.stringValue,
													   GUILayout.Width(EditorGUIUtility.currentViewWidth - 125),
													   GUILayout.Height(100));
			if (EditorGUI.EndChangeCheck())
			{
				Undo.RecordObject(target, "Base64ImageInspector_ChangedBase64ImageString");
				(target as Base64Image).base64Texture = newTextAreaContent.Replace("\n", "")
																	 .Replace(" ", "")
																	 .Replace("\t", "")
																	 .Replace("\r", "");
			}
			GUILayout.EndHorizontal();

			if (m_Base64Texture.stringValue.Length > 0 && !m_IsValidBase64.boolValue)
			{
				EditorGUILayout.HelpBox("base64 string is not valid", MessageType.Warning);
			}
			else if (m_Base64Texture.stringValue.Length > 0 && !m_IsSupportedImage.boolValue)
			{
				EditorGUILayout.HelpBox("not a base64 image string", MessageType.Warning);
			}

			AppearanceControlsGUI();
			RaycastControlsGUI();
			MaskableControlsGUI();
			serializedObject.ApplyModifiedProperties();
		}

		public override bool HasPreviewGUI() => true;
		public override void OnPreviewGUI(Rect r, GUIStyle background)
		{
			GUI.DrawTexture(r, (target as Base64Image).mainTexture, ScaleMode.ScaleToFit);
		}
	}
}
