using System.Collections;
using TweenKit;
using UnityEngine;

namespace TweenKit
{
    [AddComponentMenu("TweenKit/Tween Size")]
    public class TweenSize : UITweener
    {
        public Vector2 def = Vector2.one;
        public Vector2 from = Vector2.one;
        public Vector2 to = Vector2.one;

        private RectTransform mRect;
        public RectTransform cachedRect { get { if (mRect == null) mRect = GetComponent<RectTransform>(); return mRect; } }

        public Vector2 value { get { return cachedRect.sizeDelta; }set { cachedRect.sizeDelta = value; } }


        protected override void Awake() { value = def; base.Awake(); }
        protected override void OnUpdate(float factor, bool isFinished)
        {
            value = from * (1f - factor) + to * factor;
        }

        static public TweenSize Begin(RectTransform rect, float duration, Vector2 size)
        {
            TweenSize comp = UITweener.Begin<TweenSize>(rect.gameObject, duration);
            comp.from = rect.sizeDelta;
            comp.to = size;

            if (duration <= 0f)
            {
                comp.Sample(1f, true);
                comp.enabled = false;
            }
            return comp;
        }

        [ContextMenu("Set 'From' to current value")]
        public override void SetStartToCurrentValue() { from = value; }

        [ContextMenu("Set 'To' to current value")]
        public override void SetEndToCurrentValue() { to = value; }

        [ContextMenu("Assume value of 'From'")]
        void SetCurrentValueToStart() { value = from; }

        [ContextMenu("Assume value of 'To'")]
        void SetCurrentValueToEnd() { value = to; }
    }
}