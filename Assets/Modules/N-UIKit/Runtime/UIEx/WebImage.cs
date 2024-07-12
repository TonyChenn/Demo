using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace UnityEngine.UI
{
    [RequireComponent(typeof(CanvasRenderer))]
    [AddComponentMenu("UI/Web Image", 12)]
    public class WebImage : MaskableGraphic
    {
        [SerializeField] string m_Url;
        [SerializeField] Texture m_Texture;

        protected WebImage()
        {
            useLegacyMeshGeneration = false;
        }


        public string imageUrl
        {
            get { return m_Url; }
            set
            {
#if !UNITY_EDITOR
                if (m_Url == value) return;
#endif
                m_Url = value;
                StartCoroutine(fetchImage());
            }
        }

        private Texture texture
        {
            get { return m_Texture; }
            set
            {
                m_Texture = value;
                SetVerticesDirty();
                SetMaterialDirty();
            }
        }

        protected override void Start()
        {
            base.Start();
            StartCoroutine(fetchImage());
        }

        protected override void OnDestroy()
        {
            DestroyImmediate(m_Texture);
            m_Texture = null;
            base.OnDestroy();
        }

        public override Texture mainTexture
        {
            get { return m_Texture == null ? s_WhiteTexture : m_Texture; }
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            if (mainTexture != null)
            {
                var r = GetPixelAdjustedRect();
                var v = new Vector4(r.x, r.y, r.x + r.width, r.y + r.height);
                var scaleX = mainTexture.width * mainTexture.texelSize.x;
                var scaleY = mainTexture.height * mainTexture.texelSize.y;
                {
                    var color32 = color;
                    vh.AddVert(new Vector3(v.x, v.y), color32, new Vector2(0, 0));
                    vh.AddVert(new Vector3(v.x, v.w), color32, new Vector2(0, 1));
                    vh.AddVert(new Vector3(v.z, v.w), color32, new Vector2(scaleX, 1));
                    vh.AddVert(new Vector3(v.z, v.y), color32, new Vector2(scaleX, 0));

                    vh.AddTriangle(0, 1, 2);
                    vh.AddTriangle(2, 3, 0);
                }
            }
        }

        public override void SetNativeSize()
        {
            Texture texture = mainTexture;
            if (texture != null)
            {
                int w = Mathf.RoundToInt(texture.width);
                int h = Mathf.RoundToInt(texture.height);
                rectTransform.anchorMax = rectTransform.anchorMin;
                rectTransform.sizeDelta = new Vector2(w, h);
            }
        }

        private IEnumerator fetchImage()
        {
            if (string.IsNullOrEmpty(m_Url) || m_Texture != null)
            {
                DestroyImmediate(m_Texture);
                texture = null;
                yield break;
            }

            UnityWebRequest request = UnityWebRequestTexture.GetTexture(m_Url);
            yield return request.SendWebRequest();
#if UNITY_2020_2_OR_NEWER
            if (request.result == UnityWebRequest.Result.ProtocolError)
#else
        if (request.isHttpError)
#endif
            {
                Debug.LogError(string.Format("HTTP Error: {0} - {1}", m_Url, request.error));
            }
#if UNITY_2020_2_OR_NEWER
            else if (request.result == UnityWebRequest.Result.ConnectionError)
#else
        else if(request.isNetworkError)
#endif
            {
                Debug.LogError(string.Format("Network Error: {0} - {1}", m_Url, request.error));
            }
            else
            {
                texture = DownloadHandlerTexture.GetContent(request);
                SetNativeSize();
                string[] tmp = m_Url.Split('?');
                if (tmp.Length == 1) yield break;
                string[] param = tmp[1].Split('&');
            }
        }

#if UNITY_EDITOR
        [ContextMenu("Refresh")]
        public void RefreshImage()
        {
            StartCoroutine(fetchImage());
        }
#endif
    }
}

