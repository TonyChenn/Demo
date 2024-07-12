//-------------------------------------------------
//            TweenKit
// Copyright © 2020 tonychenn.cn
//-------------------------------------------------
using UnityEngine;
using UnityEngine.UI;

namespace TweenKit
{
    [RequireComponent(typeof(Text))]
    [AddComponentMenu("TweenKit/Tween Number")]
    public class TweenNumber : UITweener
    {
        public float from;
        public float to;
        public int digits;

        float mValue;

        Text mText;
        public Text CachedText
        {
            get
            {
                if (mText == null) mText = GetComponent<Text>();
                return mText;
            }
        }
        public float value
        {
            get { return mValue; }
            set { mValue = value; }
        }

        protected override void OnUpdate(float factor, bool isFinished)
        {
            value = from + factor * (to - from);
            CachedText.text = (System.Math.Round(value, digits)).ToString();
        }

        static public TweenNumber Begin(Text label, float duration, float delay, float from, float to)
        {
            TweenNumber comp = UITweener.Begin<TweenNumber>(label.gameObject, duration);
            comp.from = from;
            comp.to = to;
            comp.delay = delay;

            if (duration <= 0)
            {
                comp.Sample(1, true);
                comp.enabled = false;
            }
            return comp;
        }
    }
}

