using UnityEngine;
using UIAnimation.Actions;
using UIAnimation.Core;

namespace UIAnimation.Tween
{
    [AddComponentMenu("UIAnimation/Tween/Tween Transform Scale To")]
    public class TweenTransformScaleTo : TweenActionBase
    {               
        [SerializeField]
        private float toScaleX = 1;
        public float ToScaleX { get { return toScaleX; } set { toScaleX = value; } }
        
        [SerializeField]
        private float toScaleY = 1;
        public float ToScaleY { get { return toScaleY; } set { toScaleY = value; } }
        
        [SerializeField]
        private float toScaleZ = 1;
        public float ToScaleZ { get { return toScaleZ; } set { toScaleZ = value; } }

        [SerializeField]
        private Vector2 pivot = new Vector2(0.5f, 0.5f);
        public Vector2 Pivot {
            get {
                return pivot;
            }
            set {
                pivot = value;
            }
        }

        private RectTransform rectTrans;
        public RectTransform RectTrans {
            get {
                if (rectTrans == null) {
                    rectTrans = GetComponent<RectTransform>();
                }
                return rectTrans;
            }
        }
        
        private float fromScaleX;
        private float fromScaleY;
        private float fromScaleZ;
        
        private float valueX;
        private float valueY;
        private float valueZ;

        private Vector3 originalScale;
        private Vector2 originalPivot;
        private Vector2 originalAnchoredPosition;

        private bool hasOriginalValues = false;
        
        protected override void Awake()
        {
            base.Awake();
            if (transform == null) {
                throw new System.Exception("Transformコンポーネントが見つかりません");
            }

            originalScale = transform.localScale;
        }

        void Start()
        {
            if (RectTrans != null) {
                originalAnchoredPosition = RectTrans.anchoredPosition;
                originalPivot = RectTrans.pivot;
            }
            hasOriginalValues = RectTrans != null;
        }
        
        public override void ResetStatus()
        {
            base.ResetStatus();

            transform.localScale = originalScale;

            if (RectTrans != null && hasOriginalValues) {
                RectTrans.pivot = originalPivot;
                RectTrans.anchoredPosition = originalAnchoredPosition;
            }
        }
        
        public override void Prepare()
        {
            base.Prepare();

            fromScaleX = transform.localScale.x;
            fromScaleY = transform.localScale.y;
            fromScaleZ = transform.localScale.z;

            if (RectTrans != null) {
                Mathematics.ChangePivot(RectTrans, Pivot);
            }
        }
        
        #region implemented abstract members of TweenerBase
        protected override void Lerp(float normalizedTime)
        {
            valueX = Mathematics.LerpFloat(fromScaleX, ToScaleX, normalizedTime);
            valueY = Mathematics.LerpFloat(fromScaleY, ToScaleY, normalizedTime);
            valueZ = Mathematics.LerpFloat(fromScaleZ, ToScaleZ, normalizedTime);
            transform.localScale = new Vector3(valueX, valueY, valueZ);
        }
        #endregion

        protected override void OnActionIsDone ()
		{
			base.OnActionIsDone ();
			transform.localScale = new Vector3(ToScaleX, ToScaleY, ToScaleZ);
		}
    }
}