namespace UnityEngine.UI
{
    /// <summary>
    /// 实现图片的多边形合圆形的裁切
    /// </summary>
    [RequireComponent(typeof(CanvasRenderer))]
    public class PolygonImage : MaskableGraphic
    {
        [SerializeField] Texture m_Texture;
        [SerializeField] int m_PologyCount = 4;

        public override Texture mainTexture
        {
            get
            {
                if (m_Texture == null)
                {
                    if (material != null && material.mainTexture != null)
                    {
                        return material.mainTexture;
                    }
                    return s_WhiteTexture;
                }

                return m_Texture;
            }
        }

        public Texture texture
        {
            get { return m_Texture; }
            set
            {
                if (m_Texture == value) return;

                m_Texture = value;
                SetVerticesDirty();
                SetMaterialDirty();
            }
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            Texture texture = mainTexture;
            vh.Clear();

            if (texture != null)
            {
                if (m_PologyCount < 3) m_PologyCount = 3;
                // 每一份弧度
                float perRad = (float)Mathf.PI * 2 / m_PologyCount;

                // add vert
                for (int i = 0; i < m_PologyCount; i++)
                {
                    // 当前弧度
                    float rad = perRad * i;
                    float m_Radus = Mathf.Min(rectTransform.rect.width, rectTransform.rect.height) / 2;
                    //计算坐标
                    float x = m_Radus * Mathf.Sin(rad);
                    float y = m_Radus * Mathf.Cos(rad);

                    Vector2 uv = new Vector2(x / rectTransform.rect.width + 0.5f, y / rectTransform.rect.height + 0.5f);
                    vh.AddVert(new Vector2(x, y), color, uv);
                }
                //add trangle
                for (int i = 0; i < m_PologyCount - 1; i++)
                {
                    vh.AddTriangle(0, i, i + 1);
                }
            }
        }

        public override void SetNativeSize()
        {
            Texture tex = mainTexture;
            if (tex != null)
            {
                rectTransform.anchorMax = rectTransform.anchorMin;
                rectTransform.sizeDelta = new Vector2(tex.width, tex.height);
            }
        }
    }
}

