using UIAnimation.Core;
using System.Collections.Generic;
using UIAnimation.Actions;
using UnityEngine;
using UnityEngine.UI;

namespace UIAnimation.Tween
{
    [AddComponentMenu("UIAnimation/Tween/Tween Alpha To")]
    public class TweenAlphaTo : TweenActionBase
    {
        [SerializeField]
        float toAlpha;

        List<MaskableGraphic> mMaskableGraphic = new List<MaskableGraphic>();
        List<Color> m_colors = new List<Color>();
        List<float> m_alphas = new List<float>();

        protected override void Awake()
        {
            base.Awake();

            var roowWidget = GetComponent<MaskableGraphic>();

            if (roowWidget != null)
            {
                mMaskableGraphic.Add(roowWidget);
                m_colors.Add(roowWidget.color);
                m_alphas.Add(roowWidget.color.a);
            }

            foreach (var childWidget in transform.GetComponentsInChildren<MaskableGraphic>())
            {
                mMaskableGraphic.Add(childWidget);
                m_colors.Add(childWidget.color);
                m_alphas.Add(childWidget.color.a);
            }
        }

        public override void ResetStatus()
        {
            base.ResetStatus();
            for (int i = 0; i < mMaskableGraphic.Count; i++)
            {
                if (mMaskableGraphic[i] != null)
                    mMaskableGraphic[i].color = m_colors[i];
            }
        }

        public override void Prepare()
        {
            base.Prepare();

            for (int i = 0; i < mMaskableGraphic.Count; i++)
            {
                if (mMaskableGraphic[i] != null)
                    m_alphas[i] = mMaskableGraphic[i].color.a;
            }
        }

        protected override void Lerp(float normalizedTime)
        {
            for (int i = 0; i < mMaskableGraphic.Count; i++)
            {
                var col = m_colors[i];
                col.a = Mathematics.LerpFloat(m_alphas[i], ToAlpha, normalizedTime);
                if (mMaskableGraphic[i] != null)
                    mMaskableGraphic[i].color = col;
            }
        }

        protected override void OnActionIsDone()
        {
            base.OnActionIsDone();
            for (int i = 0; i < mMaskableGraphic.Count; i++)
            {
                var imageColor = m_colors[i];
                imageColor.a = ToAlpha;
                if (mMaskableGraphic[i] != null)
                    mMaskableGraphic[i].color = imageColor;
            }
        }

        public float ToAlpha
        {
            get { return toAlpha; }
            set { toAlpha = value; }
        }
    }
}
