//-------------------------------------------------
//            TweenKit
// Copyright © 2020 tonychenn.cn
//-------------------------------------------------

using UnityEngine;
using UnityEngine.UI;

namespace TweenKit
{
    /// <summary>
    /// Tween the sprite's fill.
    /// </summary>

    [RequireComponent(typeof(Image))]
    [AddComponentMenu("TweenKit/Tween Fill")]
    public class TweenFill : UITweener
    {
        [Range(0f, 1f)] public float def = 1f;
        [Range(0f, 1f)] public float from = 1f;
        [Range(0f, 1f)] public float to = 1f;

        bool mCached = false;
        Image mImage;

        void Cache()
        {
            mCached = true;
            mImage = GetComponent<Image>();
        }

        /// <summary>
        /// Tween's current value.
        /// </summary>

        public float value
        {
            get
            {
                if (!mCached) Cache();
                if (mImage != null) return mImage.fillAmount;
                return 0f;
            }
            set
            {
                if (!mCached) Cache();
                if (mImage != null) mImage.fillAmount = value;
            }
        }

        protected override void Awake() { value = def; base.Awake(); }
        protected override void OnUpdate(float factor, bool isFinished) { value = Mathf.Lerp(from, to, factor); }

        /// <summary>
        /// Start the tweening operation.
        /// </summary>

        static public TweenFill Begin(GameObject go, float duration, float fill)
        {
            TweenFill comp = UITweener.Begin<TweenFill>(go, duration);
            comp.def = comp.value;
            comp.from = comp.value;
            comp.to = fill;

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

