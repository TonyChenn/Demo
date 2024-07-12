//-------------------------------------------------
//            TweenKit
// Copyright © 2020 tonychenn.cn
//-------------------------------------------------

using UnityEngine;

namespace TweenKit
{
    /// <summary>
    /// Tween the object's position.
    /// </summary>
    [AddComponentMenu("TweenKit/Tween Position")]
    public class TweenPosition : UITweener
    {
        public Vector3 def;
        public Vector3 from;
        public Vector3 to;

        private RectTransform cachedTrans;

        public RectTransform CachedTrans
        {
            get { if (cachedTrans == null) cachedTrans = GetComponent<RectTransform>(); return cachedTrans; }
        }
        /// <summary>
        /// Tween's current value.
        /// </summary>

        public Vector3 value
        {
            get => CachedTrans.anchoredPosition;
            set => CachedTrans.anchoredPosition = value;
        }


        protected override void Awake()
        {
            base.Awake();
            value = def;
        }

        /// <summary>
        /// Tween the value.
        /// </summary>

        protected override void OnUpdate(float factor, bool isFinished) { value = from * (1f - factor) + to * factor; }

        /// <summary>
        /// Start the tweening operation.
        /// </summary>

        static public TweenPosition Begin(GameObject go, float duration, Vector3 pos)
        {
            TweenPosition comp = UITweener.Begin<TweenPosition>(go, duration);
            comp.from = comp.value;
            comp.to = pos;

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

