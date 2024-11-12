using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasRenderer))]
[AddComponentMenu("UI/Base64Image", 12)]
public class Base64Image : MaskableGraphic
{
	private const string PNG_HEADER = "iVB";        // png
	private const string JPG_HEADER = "/9j";        // jpg

	[SerializeField] string m_Base64Texture;
	[SerializeField] Texture m_Texture;

#if UNITY_EDITOR
	[SerializeField, HideInInspector] bool m_IsSupportedImage = false;
	[SerializeField, HideInInspector] bool m_IsValidBase64 = false;
#else
	private bool m_IsSupportedImage = false;
	private bool m_IsValidBase64 = false;
#endif

	protected Base64Image()
	{
		useLegacyMeshGeneration = false;
	}
	public override Texture mainTexture
	{
		get
		{
			if (m_Texture == null)
			{
				return s_WhiteTexture;
			}
			return m_Texture;
		}
	}

	public string base64Texture
	{
		get { return m_Base64Texture; }
		set
		{
			if (m_Base64Texture == value) return;

			m_IsSupportedImage = value.StartsWith(PNG_HEADER) || value.StartsWith(JPG_HEADER);
			m_IsValidBase64 = value.Length % 4 == 0;

			Texture2D tmpTexture = new Texture2D(1, 1);
			m_Base64Texture = value;

			if (m_IsSupportedImage && m_IsValidBase64)
			{
				if (!decodeBase64Texture(ref tmpTexture, value))
				{
					tmpTexture = s_WhiteTexture;
				}
			}

			m_Texture = tmpTexture;
			SetVerticesDirty();
			SetMaterialDirty();
		}
	}



	protected override void Start()
	{
		base.Start();
		//base64Texture = m_Base64Texture;
	}
	protected override void OnDestroy()
	{
		DestroyImmediate(m_Texture);
		m_Texture = null;
		base.OnDestroy();
	}

	public override void SetNativeSize()
	{
		Texture tex = mainTexture;
		if (tex != null)
		{
			int w = Mathf.RoundToInt(tex.width);
			int h = Mathf.RoundToInt(tex.height);
			rectTransform.anchorMax = rectTransform.anchorMin;
			rectTransform.sizeDelta = new Vector2(w, h);

		}
	}

	private bool decodeBase64Texture(ref Texture2D texture, string base64)
	{
		byte[] bytes = System.Convert.FromBase64String(base64);
		return texture.LoadImage(bytes);
	}
}
