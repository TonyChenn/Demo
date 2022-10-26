using NCore;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace SFramework.Runtime
{
    public class StartPanel : UIWndBase
    {
        [SerializeField] GameObject m_UIParent;
        [SerializeField] Button m_Logout;
        [SerializeField] Button m_SelectServer;
        [SerializeField] Button m_Start;
        [SerializeField] Button m_AgeTip;
        [SerializeField] Image m_Progress;
        [SerializeField] Text m_ProgressText;
        [SerializeField] Text m_TipInfo;
        [SerializeField] Text m_VersionInfo;

        private Button button;

        protected override UIFlag SetUIFlag()
        {
            return UIFlag.StartPanel;
        }

        protected override UIPackage SetUIPackage()
        {
            return UIPackage.MainPkg;
        }

        protected override void Awake()
        {
            base.Awake();
            button = GetComponent<Button>();
            button.onClick.AddListener(() =>
            {
                bool active = m_UIParent.gameObject.activeSelf;
                m_UIParent.gameObject.SetActive(!active);
            });
        }
        protected override async void Start()
        {
            base.Start();
            m_Start.gameObject.SetActive(false);
            m_Progress.transform.parent.gameObject.SetActive(true);

            m_TipInfo.text = "获取版本信息...";
            m_ProgressText.text = "0%";
            string version = await Version.Singleton.GetRemoteVersion();
            Debug.Log("远程版本号：" + version);
            m_VersionInfo.text = $"local version:123.456\nremote version: {version}";

            m_TipInfo.text = "开始检查更新..."; 
            await checkUpdate();
            Debug.Log("模拟更新完成");
            m_TipInfo.text = "检查更新完成";
            m_TipInfo.transform.SetActive(false);

            m_Start.gameObject.SetActive(true);
            m_Progress.transform.parent.gameObject.SetActive(false);
        }

        private async Task checkUpdate()
        {
            if (GameConfig.HotUpdate)
            {
                for (int i = 0; i < 100; i++)
                {
                    m_ProgressText.text = i + "%";
                    await Task.Delay(5000);
                }
            }
        }
    }
}

