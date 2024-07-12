//-------------------------------------------------
//            TweenKit
// Copyright © 2020 tonychenn.cn
//-------------------------------------------------

using UnityEngine;
using UnityEngine.UI;

namespace TweenKit
{
    /// <summary>
    /// Tween the object's alpha. Works with both UI widgets as well as renderers.
    /// </summary>

    [AddComponentMenu("TweenKit/Tween Alpha")]
    public class TweenAlpha : UITweener
    {
        [Range(0f, 1f)] public float def = 1f;
        [Range(0f, 1f)] public float from = 1f;
        [Range(0f, 1f)] public float to = 1f;

        MaskableGraphic mMaskableGraphic;
        Color mColor;

        MaskableGraphic Cache
        {
            get
            {
                if (mMaskableGraphic == null)
                {
                    mMaskableGraphic = GetComponent<MaskableGraphic>();
                    if (mMaskableGraphic == null) mMaskableGraphic = GetComponentInChildren<MaskableGraphic>();
                }
                mColor = mMaskableGraphic.color;
                return mMaskableGraphic;
            }
        }

        /// <summary>
        /// Tween's current value.
        /// </summary>

        public float value
        {
            get { return Cache.color.a; }
            set { Cache.color = new Color(mColor.r, mColor.g, mColor.b, value); }
        }

        /// <summary>
        /// Tween the value.
        /// </summary>

        protected override void Awake()
        {
            value = def;
            base.Awake();
        }
        protected override void OnUpdate(float factor, bool isFinished) { value = Mathf.Lerp(from, to, factor); }

        /// <summary>
        /// Start the tweening operation.
        /// </summary>

        static public TweenAlpha Begin(GameObject go, float duration, float alpha, float delay = 0f)
        {
            TweenAlpha comp = UITweener.Begin<TweenAlpha>(go, duration, delay);
            comp.from = comp.value;
            comp.to = alpha;

            if (duration <= 0f)
            {
                comp.Sample(1f, true);
                comp.enabled = false;
            }
            return comp;
        }

        public override void SetStartToCurrentValue() { from = value; }
        public override void SetEndToCurrentValue() { to = value; }
    }
}

