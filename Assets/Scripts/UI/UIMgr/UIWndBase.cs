using Messengers;
using UnityEngine;

namespace UIKit
{
	public abstract class UIWndBase : MonoBehaviour, ITheme
    {
        private RectTransform m_CachedTrans;
        private GameObject m_CachedObj;

        protected UIFlag m_UIID = UIFlag.None;
        protected UIDeep m_UIDeep = UIDeep.CommonLayer;

		public UIFlag UIID => m_UIID;

		public UIDeep UIDeepLayer => m_UIDeep;

		public RectTransform CachedTrans { get { if (m_CachedTrans == null) m_CachedTrans = GetComponent<RectTransform>(); return m_CachedTrans; } }
        public GameObject CachedGameObject { get { if (m_CachedObj == null) m_CachedObj = this.gameObject; return m_CachedObj; } }

		public UIWndBase(UIFlag flag) { m_UIID = flag; }

        #region virsual
        protected virtual void Awake()
        {
            m_CachedObj = gameObject;
            m_CachedTrans = GetComponent<RectTransform>();

            InitWndOnAwake();
        }
		public virtual void InitWndOnAwake() { }

        protected virtual void Start() { InitWndOnStart(); }
        public virtual void InitWndOnStart() { }

        public virtual void OnShowWnd(dynamic data) { RegisterMessage(); }
        public virtual void OnHideWnd() { UnRegisterMessage(); }

        protected virtual void RegisterMessage()
        {
            Messenger<bool>.AddListener(MessengerDef.APP_THEME_CHANGED, ThemeChanged);
        }
        protected virtual void UnRegisterMessage() {
            Messenger<bool>.RemoveListener(MessengerDef.APP_THEME_CHANGED, ThemeChanged);
        }


        public virtual void ThemeChanged(bool dayTime) { }
        public virtual void Close() { }
        #endregion
    }
}
