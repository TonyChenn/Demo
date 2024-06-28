using System;
using System.Collections.Generic;
using UnityEngine;

public class UIRoot : MonoBehaviour
{
	[SerializeField] Camera uiCamera;
	[SerializeField] Canvas uiCanvas;

	private Type[] types = new Type[] { typeof(RectTransform) };

	private static Dictionary<string, RectTransform> layerTransDict;
	private static UIRoot _instance = null;


	private void Awake()
	{
		_instance = this;
		layerTransDict = new Dictionary<string, RectTransform>(6)
		{
			{ "ButtomLayer", createLayer("ButtomLayer") },
			{ "CommonLayer", createLayer("CommonLayer") },
			{ "UnderPopLayer", createLayer("UnderPopLayer") },
			{ "PopLayer", createLayer("PopLayer") },
			{ "UpPopLayer", createLayer("UpPopLayer") },
			{ "TopLayer", createLayer("TopLayer") }
		};
	}

	public static Canvas UICanvas
	{
		get
		{
			if (_instance.uiCanvas == null)
			{
				_instance.uiCanvas = GameObject.Find("[UIRoot]/UICanvas").GetComponent<Canvas>();
			}
			return _instance.uiCanvas;
		}
	}
	public static Camera Camera
	{
		get
		{
			if (_instance.uiCamera == null)
			{
				_instance.uiCamera = GameObject.Find("[UIRoot]/UICamera").GetComponent<Camera>();
			}
			return _instance.uiCamera;
		}
	}

	public static RectTransform ButtomLayer => layerTransDict["ButtomLayer"];
	public static RectTransform CommonLayer => layerTransDict["CommonLayer"];
	public static RectTransform UnderPopLayer => layerTransDict["UnderPopLayer"];
	public static RectTransform PopLayer => layerTransDict["PopLayer"];
	public static RectTransform UpPopLayer => layerTransDict["UpPopLayer"];
	public static RectTransform TopLayer => layerTransDict["TopLayer"];

	private RectTransform createLayer(string layerName)
	{
		var go = new GameObject($"[{layerName}]", types);
		go.transform.SetParent(UICanvas.transform);
		var rect = go.GetComponent<RectTransform>();
		rect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0, 0);
		rect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, 0);
		rect.anchorMin = Vector2.zero;
		rect.anchorMax = Vector2.one;
		rect.localScale = Vector3.one;
		rect.localPosition = Vector3.zero;
		return rect;
	}
}
