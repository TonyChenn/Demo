//-------------------------------------------------
//            TweenKit
// Copyright © 2020 tonychenn.cn
//-------------------------------------------------
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TweenKit
{
    [RequireComponent(typeof(Button))]
    [AddComponentMenu("TweenKit/Button Scale")]
    public class ButtonScale : MonoBehaviour, IPointerDownHandler,IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
    {
        MaskableGraphic tweenTarget;
        public Vector3 hover = Vector3.one;
        public Vector3 pressed = new Vector3(.9f, .9f, .9f);
        public float duration = .2f;

        Vector3 mScale;
        bool mStarted = false;

        private void OnEnable()
        {
            if (mStarted) OnPointerEnter(null);
        }
        private void Start()
        {
            if(!mStarted)
            {
                mStarted = true;
                if (tweenTarget == null) tweenTarget = GetComponent<MaskableGraphic>();
                mScale = tweenTarget.transform.localScale;
            }
        }
        //鼠标按下
        public void OnPointerDown(PointerEventData eventData)
        {
            if(enabled)
            {
                if (!mStarted) Start();
                TweenScale.Begin(tweenTarget.gameObject, duration, pressed);
            }
        }

        //鼠标松开
        public void OnPointerUp(PointerEventData eventData)
        {
            if (enabled)
            {
                if (!mStarted) Start();
                TweenScale.Begin(tweenTarget.gameObject, duration, hover);
            }
        }
        //鼠标进入
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (enabled)
            {
                if (!mStarted) Start();
                TweenScale.Begin(tweenTarget.gameObject, duration, hover);
            }
        }
        //鼠标离开
        public void OnPointerExit(PointerEventData eventData)
        {
            if (enabled)
            {
                if (!mStarted) Start();
                TweenScale.Begin(tweenTarget.gameObject, duration, Vector3.one);
            }
        }
    }
}
