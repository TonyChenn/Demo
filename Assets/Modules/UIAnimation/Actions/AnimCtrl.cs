using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UIAnimation.Actions
{
    public class AnimCtrl : MonoBehaviour
    {
        [SerializeField] ActionRunner mShowAnim;
        [SerializeField] ActionRunner mHideAnim;
        [SerializeField] bool mShowOnEnable = true;

        ActionRunner currActionRunner;

        Action showAction;  //the action after show animation play over
        Action hideAction;  //the action after hide animation play over

        public Action ShowAction{ set { showAction = value; } }
        public Action HideAction{ set { hideAction = value; } }

        private void Awake()
        {
            if (mShowAnim) mShowAnim.OnFinishedAllActionsEvent += ShowFinishedHandler;
            if (mHideAnim) mHideAnim.OnFinishedAllActionsEvent += HideFinishedHandler;
        }

        private void OnEnable()
        {
            if (mShowOnEnable) Play();
        }

        private void Update()
        {
            if (currActionRunner)
                if (currActionRunner.Run())
                    currActionRunner = null;
        }
        private void OnDisable()
        {
            if (mShowAnim) mShowAnim.Stop();
            if (mHideAnim) mHideAnim.Stop();
        }

        public void Play()
        {
            currActionRunner = mShowAnim;
        }
        public void PlayReverse()
        {
            currActionRunner = mHideAnim;
            if (mHideAnim == null) mShowAnim.Stop();
        }

        private void HideFinishedHandler()
        {
            if (hideAction != null) hideAction();
        }

        private void ShowFinishedHandler()
        {
            if (showAction != null) showAction();
        }
    }
}

