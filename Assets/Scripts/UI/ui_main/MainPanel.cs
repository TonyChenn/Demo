using NCore;
using NDebug;
using UnityEngine;
using Messengers;
using UnityEngine.UI;

public class MainPanel : MonoBehaviour
{
    [SerializeField] Text AppVersion;
    [SerializeField] Text ClientVersion;
    [SerializeField] Text RemoteVersion;

    // Update
    [SerializeField] Image ProgressBar;
    [SerializeField] Text ProgressValue;
    [SerializeField] Text TipInfo;


    [SerializeField] Transform RightGroup;
    [SerializeField] Button BtnAgeTip;
    [SerializeField] Button BtnFixClient;
    [SerializeField] Button BtnChangeAccount;
    [SerializeField] Button BtnAdvert;

    [SerializeField] Transform EnterGroup;
    [SerializeField] Toggle TglAllow;
    [SerializeField] Button BtnEnterGame;

    private void Awake()
    {
        EnterGroup.SetActive(false);
        RightGroup.gameObject.SetActive(false);
    }
    void Start()
    {
        AppVersion.text = "v" + Application.version;
        ProgressBar.fillAmount = 0f;

        Messenger<string>.AddListener(MessengerDef.REFRESH_LOCAL_VERSION, RefreshLocalVersion);
        Messenger<string>.AddListener(MessengerDef.REFRESH_REMOTE_VERSION, RefreshRemoteVersion);
        Messenger<Version.UpdateState, int>.AddListener(MessengerDef.CHECK_UPDATE, RefreshProgressValue);
        Messenger.AddListener(MessengerDef.RESTART_CLIENT, RestartClient);

        BtnFixClient.onClick.AddListener(FixClientClickHandler);
        TglAllow.onValueChanged.AddListener((isOn) =>
        {
            BtnEnterGame.enabled = isOn;
        });

        Version.Singleton.CheckUpdate();
    }


    private void OnDestroy()
    {
        Messenger<string>.RemoveListener(MessengerDef.REFRESH_LOCAL_VERSION, RefreshLocalVersion);
        Messenger<string>.RemoveListener(MessengerDef.REFRESH_REMOTE_VERSION, RefreshRemoteVersion);
        Messenger<Version.UpdateState, int>.RemoveListener(MessengerDef.CHECK_UPDATE, RefreshProgressValue);
        Messenger.AddListener(MessengerDef.RESTART_CLIENT, RestartClient);
    }

    private void RefreshRemoteVersion(string version)
    {
        RemoteVersion.text = "Remote: " + version;
    }

    private void RefreshLocalVersion(string version)
    {
        ClientVersion.text = "Client: " + version;
    }

    private void FixClientClickHandler()
    {
        Version.Singleton.DeleteVersionFile();
        Version.Singleton.NeedFixClient = true;
        GameMgr.Singlton.RestartGame();
    }

    private void RestartClient()
    {
        Log.Info("请重启客户端");
    }

    private void RefreshProgressValue(Version.UpdateState state, int rate)
    {
        if (rate < 0) { rate = 0; }
        else if (rate > 100) { rate = 100; }

        switch (state)
        {
            case Version.UpdateState.GetRemoteVersion:
                TipInfo.text = "正在获取远程版本号";
                break;
            case Version.UpdateState.GetRemoteVersionFail:
                Log.Error("获取远程版本号失败");
                break;
            case Version.UpdateState.GetRemoteRes:
                TipInfo.text = string.Format("正在获取远程资源列表{0}%", rate);
                break;
            case Version.UpdateState.GetRemoteResFail:
                TipInfo.text = string.Format("获取远程资源列表失败");
                break;
            case Version.UpdateState.CheckRes:
                TipInfo.text = string.Format("正在分析变动资源{0}%", rate);
                break;
            case Version.UpdateState.Download:
                TipInfo.text = string.Format("正在下载{0}%", rate);
                break;
            case Version.UpdateState.DownloadFail:
                Log.Error("下载失败");
                break;
            case Version.UpdateState.CanEnterGame:
                TipInfo.text = string.Format("");
                TipInfo.gameObject.SetActive(false);
                EnterGroup.SetActive(true);
                RightGroup.gameObject.SetActive(true);
                break;
            default:
                break;
        }
        ProgressValue.text = rate + "%";
        ProgressBar.fillAmount = rate / 100.0f;
    }
}
