using System;
using UnityEngine;

namespace SFramework.Runtime
{
    public abstract class UIWndBase : MonoBehaviour
    {
        protected Transform m_Trans;
        protected GameObject m_Go;
        protected UIFlag m_UIID = UIFlag.None;
        protected UIPackage m_package = UIPackage.None;
        protected UILayer m_layer = UILayer.Default;


        public UIPackage Package
        {
            get
            {
                if (m_package == UIPackage.None)
                    SetUIPackage();
                return m_package;
            }
        }
        public UIFlag UIID
        {
            get
            {
                if (m_UIID == UIFlag.None)
                    SetUIFlag();
                return m_UIID;
            }
        }

        public UILayer UILayer => m_layer;

        public Transform CachedTrans
        {
            get
            {
                if (m_Trans == null)
                    m_Trans = this.transform;
                return m_Trans;
            }
        }
        public GameObject CachedGameObject
        {
            get
            {
                if (m_Go == null)
                    m_Go = this.gameObject;
                return m_Go;
            }
        }

        #region Life Method
        protected virtual void Awake()
        {
            m_Go = this.gameObject;
            m_Trans = this.transform;

            SetUIPackage();
            SetUIFlag();
            SetUILayer();

            InitWndOnAwake();
            
        }

        protected virtual void InitWndOnAwake()
        {
            
        }

        protected virtual void Start()
        {
            InitWndOnStart();
        }

        protected virtual void InitWndOnStart()
        {
        }
        
        public virtual void OnShowWnd()
        {
            RegisterMessage();
        }

        public virtual void OnHideWnd()
        {
            RemoveMessage();
        }

        #endregion

        #region abstract Method
        protected abstract UIPackage SetUIPackage();
        protected abstract UIFlag SetUIFlag();
        #endregion

        #region Messager
        protected virtual void RegisterMessage() { }
        protected virtual void RemoveMessage() { }
        #endregion

        protected virtual void SetActive(bool active)
        {
            m_Go?.SetActive(active);
        }

        protected virtual UILayer SetUILayer()
        {
            return UILayer.Default;
        }

    }
}