using UIAnimation.Actions;
using UIAnimation.Core;
using UnityEngine;
using UnityEngine.UI;

namespace UIAnimation.Tween
{
    [AddComponentMenu("UIAnimation/Tween/Tween Widget Alpha To")]
    public class TweenWidgetAlphaTo : TweenActionBase
    {
        [SerializeField]
        float toAlpha;
        MaskableGraphic mMaskableGraphic = null;
        float fromAlpha;
        Color mColor;

        protected override void Awake()
        {
            base.Awake();
            mMaskableGraphic = transform.GetComponent<MaskableGraphic>();
            mColor = mMaskableGraphic.color;
            fromAlpha = mColor.a;
        }

        public override void ResetStatus()
        {
            base.ResetStatus();
            mColor = new Color(mColor.r, mColor.g, mColor.b, fromAlpha);
        }

        public override void Prepare()
        {
            base.Prepare();
            mColor = new Color(mColor.r, mColor.g, mColor.b, fromAlpha);
        }

        protected override void Lerp(float normalizedTime)
        {
            float a = Mathematics.LerpFloat(fromAlpha, ToAlpha, normalizedTime);
            mColor = new Color(mColor.r, mColor.g, mColor.b, a);
        }

        protected override void OnActionIsDone()
        {
            base.OnActionIsDone();
        }

        public float ToAlpha
        {
            get { return toAlpha; }
            set { toAlpha = value; }
        }
    }
}
