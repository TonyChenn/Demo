using UIAnimation.Core;
using System.Collections.Generic;
using UIAnimation.Actions;
using UnityEngine;
using UnityEngine.UI;

namespace UIAnimation.Tween
{
    [AddComponentMenu("UIAnimation/Tween/Tween Alpha By")]
    public class TweenAlphaBy : TweenActionBase
    {
        [SerializeField]
        float deltaAlpha;

        List<MaskableGraphic> mMaskableGraphic = new List<MaskableGraphic>();
        List<Color> m_colors = new List<Color>();
        List<float> m_fromAlphas = new List<float>();
        List<float> m_toAlphas = new List<float>();

        protected override void Awake()
        {
            base.Awake();

            MaskableGraphic rootWidget = transform.GetComponent<MaskableGraphic>();

            if (rootWidget != null)
            {
                mMaskableGraphic.Add(rootWidget);
                m_colors.Add(rootWidget.color);
                m_fromAlphas.Add(rootWidget.color.a);
                m_toAlphas.Add(rootWidget.color.a);
            }

            foreach (var childWidget in transform.GetComponentsInChildren<MaskableGraphic>())
            {
                mMaskableGraphic.Add(childWidget);
                m_colors.Add(childWidget.color);
                m_fromAlphas.Add(childWidget.color.a);
                m_toAlphas.Add(childWidget.color.a);
            }
        }

        public override void ResetStatus()
        {
            base.ResetStatus();
            for (int i = 0; i < mMaskableGraphic.Count; i++)
                mMaskableGraphic[i].color = m_colors[i];
        }

        public override void Prepare()
        {
            base.Prepare();

            for (int i = 0; i < mMaskableGraphic.Count; i++)
            {
                m_fromAlphas[i] = mMaskableGraphic[i].color.a;
                m_toAlphas[i] = Mathf.Clamp01(m_fromAlphas[i] + DeltaAlpha);
            }
        }

        protected override void Lerp(float normalizedTime)
        {
            for (int i = 0; i < mMaskableGraphic.Count; i++)
            {
                var col = m_colors[i];
                col.a = Mathematics.LerpFloat(m_fromAlphas[i], m_toAlphas[i], normalizedTime);
                mMaskableGraphic[i].color = col;
            }
        }

        protected override void OnActionIsDone()
        {
            base.OnActionIsDone();
            for (int i = 0; i < mMaskableGraphic.Count; i++)
            {
                var imageColor = m_colors[i];
                imageColor.a = m_toAlphas[i];
                mMaskableGraphic[i].color = imageColor;
            }
        }

        public float DeltaAlpha
        {
            get { return deltaAlpha; }
            set { deltaAlpha = value; }
        }
    }
}
