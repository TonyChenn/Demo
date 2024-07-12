using NCore;
using System;
using System.Drawing;
using System.Xml.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class SceneViewMenu
{

	[InitializeOnLoadMethod]
	static void Init()
	{
		SceneView.duringSceneGui += OnSceneGUI;
	}

	private static void OnSceneGUI(SceneView view)
	{
		if (Selection.gameObjects.Length != 1) { return; }

		GameObject gameObject = Selection.gameObjects[0];
		Component canvas = gameObject.GetComponentInParent<Canvas>();
		if (canvas == null) { return; }

		Event e = Event.current;
		if (e?.button != 1 || e?.type != EventType.MouseUp) { return; }

		GenericMenu menu = new GenericMenu();
		menu.AddItem(new GUIContent("Create/Text"), false, () => {  });
		menu.AddItem(new GUIContent("Create/Button"), false, () => { });
		menu.AddItem(new GUIContent("Create/UITexture"), false, () => { });
		menu.AddItem(new GUIContent("Create/UISprite"), false, () => { });
		menu.AddItem(new GUIContent("Create/Panel"), false, () => { });
		menu.AddItem(new GUIContent("Create/Toggle"), false, () => { });
		menu.AddItem(new GUIContent("Create/Slider"), false, () => { });
		menu.AddItem(new GUIContent("Create/ScrollView"), false, () => { });
		menu.ShowAsContext();
	}


	private const float kWidth = 160f;
	private const float kThickHeight = 30f;
	private const float kThinHeight = 20f;
	private static Vector2 s_ThickElementSize = new Vector2(kWidth, kThickHeight);
	private static Vector2 s_ThinElementSize = new Vector2(kWidth, kThinHeight);
	private static Vector2 s_ImageElementSize = new Vector2(100f, 100f);

	private static DefaultControls.Resources s_StandardResources;

	private static DefaultControls.Resources GetStandardResources()
	{
		if (s_StandardResources.standard == null)
		{
			s_StandardResources.standard = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
			s_StandardResources.background = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
			s_StandardResources.inputField = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/InputFieldBackground.psd");
			s_StandardResources.knob = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
			s_StandardResources.checkmark = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Checkmark.psd");
			s_StandardResources.dropdown = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/DropdownArrow.psd");
			s_StandardResources.mask = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UIMask.psd");
		}
		return s_StandardResources;
	}
}
