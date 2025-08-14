using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Permissions;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.UI;

public class GamePreprocessing : MonoBehaviour
{
    public static GamePreprocessing instance;

    public Text currentState;

    private WaitForSeconds waitForSeconds = new WaitForSeconds(0.3f);

    public GameObject DevWindow;

    private int timeOut = 9;

    public void Init()
    {
        instance = this;

        StartPreProcessing();
    }

    public async void StartPreProcessing()
    {
        if (DevOptionsManager.devOptions.mode == MODE.dev)
        {
            DevWindow.SetActive(true);
        }
        if (!DevWindow)
            return;
        while (DevWindow && DevWindow.activeSelf)
        {
            await UniTask.WaitForSeconds(0.1f);
        }
        ServerIndexText.serverIndexText.text = DevOptionsManager.devOptions.serverIndex.ToString();
        await SetPubAPI();
        var needAppUpdate = await CheckGameVersion();
        if (needAppUpdate)
        {
            return;
        }
        await CheckGameUpdate();

        bool maintenace = await CheckMaintenance();
        if (maintenace)
        {
            return;
        }
        GetTables();
        LoginManager.instance.OnLoginButton();
    }

    private async void GetTables()
    {
        await TableDataManager.GetTableList();
        await TableDataManager.GetBlindTables();
        await TableDataManager.GetRankTables();
    }

    private void SelectServer()
    {
        if (DevOptionsManager.devOptions.mode == MODE.dev)
        {
            DevWindow.SetActive(true);
        }
        else
        {
            StartPup();
        }
    }

    public async void StartPup()
    {
        await SetPubAPI();
        ServerIndexText.serverIndexText.text = DevOptionsManager.devOptions.serverIndex.ToString();
    }

    private async UniTask SetPubAPI()
    {
        if (currentState)
        {
            currentState.text = LocalizeManager.GetLocalString("SYS_PUB_SRV_SETTING");
        }

        //string url = (string)ServerConfigManager.GetConfig("publisher_url");
        //int port = (int)ServerConfigManager.GetConfig("publisher_port");
        PublisherApiManager.Instance.SetURL((string)ServerConfigManager.GetConfig("pub_url"));

        await UniTask.WaitForSeconds(0.1f);

        //StartCoroutine("ConnectGameServer");
    }

    private async UniTask<bool> CheckGameVersion()
    {
        currentState.text = LocalizeManager.GetLocalString("SYS_GAME_VER_INFO_CHECKING");
        string version = (string)ServerConfigManager.GetConfig("version");

#if UNITY_ANDROID
        version = (string)ServerConfigManager.GetConfig("version_android"); //.ValueOrDefault("version_android", version);
#elif UNITY_IOS
        version = (string)ServerConfigManager.GetConfig("version_ios");
#endif
        var serverSp = version.Replace("v.", "").SplitAndTrimAll('.');
        var clientSp = Application.version.Replace("v.", "").SplitAndTrimAll('.');
        int matchCount = 2;

        bool needUpdate = false;

        for (int i = 0; i < matchCount; i++)
        {
            try
            {
                if (int.Parse(serverSp[i]) > int.Parse(clientSp[i]))
                {
                    needUpdate = true;
                    break;
                }
                else if (int.Parse(serverSp[i]) < int.Parse(clientSp[i]))
                {
                    //ErrorMessageManager.Instance.AddNetworkError(2, "SYS_ERR_NETWORK", "SYS_INVALID_CLIENT_VER_HI", ErrorHandlingType.NONE);
                }
            }
            catch
            {
                ErrorMessageManager.Instance.AddNetworkError(
                    2,
                    "SYS_ERR",
                    "SYS_ERR_CLIENT_VER_CHECK",
                    ErrorHandlingType.NONE
                );
            }
        }
#if UNITY_EDITOR
        Console.Log("서버 버전" + version);
        Console.Log("클라이언트 버전" + UnityEditor.PlayerSettings.bundleVersion);
#endif

        if (needUpdate)
        {
            ErrorMessageManager.Instance.AddNetworkError(
                2,
                "SYS_VERSION_UPDATE_TITLE",
                "SYS_INVALID_CLIENT_VER_LOW",
                ErrorHandlingType.RECALL,
                null,
                OpenAppStore
            );

            await UniTask.WaitForSeconds(30f);
            OpenAppStore();
            return true;
        }
        await UniTask.WaitForSeconds(0.1f);
        return false;
    }

    public static async void OpenAppStore()
    {
#if UNITY_ANDROID
#if ONE_STORE
        Application.OpenURL(Constant.URL.ONE_STORE_UPDATE_URL);
#else
        Application.OpenURL(Constant.URL.ANDROID_UPDATE_URL);
#endif
#elif UNITY_IOS
        //int iosCode = 1610089270;

        Application.OpenURL(Constant.URL.IOS_UPDATE_URL);
#else
#endif
        await UniTask.WaitForSeconds(0.1f);
        Application.Quit();
    }
    public static void ServerVersionCheck(Action successCallback, Action failCallback)
    {
        ServerConfigManager.Instance.LoadServerConfig(LinkOptionConstant.storageUrl + LinkOptionConstant.configPath, () =>
        {
            successCallback();
        },
        failCallback);  
    }
    public static async UniTask<bool> CheckGameVersionNonUi()
    {
        string version = (string)ServerConfigManager.GetConfig("version");

#if UNITY_ANDROID
        version = (string)ServerConfigManager.GetConfig("version_android"); //.ValueOrDefault("version_android", version);
#elif UNITY_IOS
        version = (string)ServerConfigManager.GetConfig("version_ios");
#endif
        var serverSp = version.Replace("v.", "").SplitAndTrimAll('.');
        var clientSp = Application.version.Replace("v.", "").SplitAndTrimAll('.');
        int matchCount = 2;

        bool needUpdate = false;

        for (int i = 0; i < matchCount; i++)
        {
            try
            {
                if (int.Parse(serverSp[i]) > int.Parse(clientSp[i]))
                {
                    needUpdate = true;
                    break;
                }
                else if (int.Parse(serverSp[i]) < int.Parse(clientSp[i]))
                {
                    //ErrorMessageManager.Instance.AddNetworkError(2, "SYS_ERR_NETWORK", "SYS_INVALID_CLIENT_VER_HI", ErrorHandlingType.NONE);
                }
            }
            catch
            {
                ErrorMessageManager.Instance.AddNetworkError(
                    2,
                    "SYS_ERR",
                    "SYS_ERR_CLIENT_VER_CHECK",
                    ErrorHandlingType.NONE
                );
            }
        }
#if UNITY_EDITOR
        Console.Log("서버 버전" + version);
        Console.Log("클라이언트 버전" + UnityEditor.PlayerSettings.bundleVersion);
#endif

        if (needUpdate)
        {
            ErrorMessageManager.Instance.AddNetworkError(
                2,
                "SYS_VERSION_UPDATE_TITLE",
                "SYS_INVALID_CLIENT_VER_LOW",
                ErrorHandlingType.RECALL,
                null,
                GamePreprocessing.OpenAppStore
            );

            await UniTask.WaitForSeconds(30f);
            GamePreprocessing.OpenAppStore();
            return true;
        }
        return false;
    }

    private async UniTask CheckGameUpdate()
    {
        currentState.text = LocalizeManager.GetLocalString("SYS_GAME_UPDATE_INFO_CHECKING");

        if (false)
        {
            ErrorMessageManager.Instance.AddNetworkError(
                2,
                "SYS_ERR_NETWORK",
                "SYS_ERR_UPDATE_FAILED"
            );
            return;
        }
#if UNITY_EDITOR
        Console.Log("어드레서블 에셋 만들어주세요~");
#endif
        await UniTask.WaitForSeconds(0f);
    }

    private async UniTask<bool> CheckMaintenance()
    {
        currentState.text = LocalizeManager.GetLocalString("SYS_GAME_MAINTAIN_INFO_CHECKING");

        await UniTask.WaitForSeconds(0.1f);

        if (ServerConfigManager.Contains("maintenance"))
        {
            JObject maintenance = ServerConfigManager.GetConfig("maintenance") as JObject;
            string block_login_time = maintenance.ValueOrDefault<string>("block_login_time", "");
            string end_time = maintenance.ValueOrDefault<string>("end_time", "");
            if (!string.IsNullOrEmpty(block_login_time) && !string.IsNullOrEmpty(end_time))
            {
                DateTime now = DateTime.UtcNow;

                DateTime start = DateTime.MinValue;
                DateTime end = DateTime.MinValue;

                start = DateTimeParser.Parse(block_login_time);
                end = DateTimeParser.Parse(end_time);

                int startToNow = DateTime.Compare(start, now);
                int nowToEnd = DateTime.Compare(now, end);
                if (startToNow <= 0 && nowToEnd <= 0)
                {
                    string message = maintenance.ValueOrDefault<string>("message", "점검중입니다.");
                    if (DevOptionsManager.devOptions.mode == MODE.dev)
                    {
                        ErrorMessageManager.Instance.AddNetworkError(
                            0,
                            LocalizeManager.GetLocalString("SYS_MAINTENANCE_TITLE"),
                            message + "\n(개발자 모드입니다.)",
                            ErrorHandlingType.NONE
                        );
                    }
                    else
                    {
                        ErrorMessageManager.Instance.AddNetworkError(
                            0,
                            LocalizeManager.GetLocalString("SYS_MAINTENANCE_TITLE"),
                            message,
                            ErrorHandlingType.SHUTDOWN
                        );
                        return true;
                    }
                }
            }
        }

        //GoToLogin();
        //StartCoroutine("ConnectGameServer");

        gameObject.SetActive(false);
        return false;
    }

    private IEnumerator ConnectGameServer()
    {
        currentState.text = LocalizeManager.GetLocalString("SYS_GAME_SRV_CONNECTING");

        string url = (string)ServerConfigManager.GetConfig("gameServer_url");
        int port = (int)ServerConfigManager.GetConfig("gameServer_port");
        WebSocketManager.Init(string.Format("http://{0}:{1}/", url, port));

        yield return waitForSeconds;

        gameObject.SetActive(false);

        //loginManager.OnLoginButton();
        //GetRefQuest();
    }

    //private IEnumerator NetworkProcessing()
    //{
    //    int maxTimeOutCount = 9;
    //    int timeOutCount = 0;

    //    currentState.text = "네트워크 연결 확인 중...";

    //    while (Application.internetReachability == NetworkReachability.NotReachable)
    //    {
    //        if (timeOutCount < maxTimeOutCount)
    //        {
    //            timeOutCount++;
    //        }
    //        else
    //        {
    //            SystemMessageManager.Instance.InvokeSystemError(0, "SYS_ERR_NETWORK", "네트워크 상태를 확인해주세요.");
    //            yield break;
    //        }

    //        yield return waitForSeconds;
    //    }

    //    timeOutCount = 0;
    //    yield return waitForSeconds;

    //    currentState.text = "서버 설정 정보 불러오는 중...";

    //    bool isLoadServerConfig = false;

    //    ServerConfigManager.LoadServerConfig("https://firebasestorage.googleapis.com/v0/b/jackpotholdem-984af.appspot.com/o/serverConfig.json?alt=media&token=38e5c344-7839-47fe-9263-63ee2f6f8959", () =>
    //    {
    //        isLoadServerConfig = true;
    //    });

    //    while (isLoadServerConfig == false)
    //    {
    //        if (timeOutCount < maxTimeOutCount)
    //        {
    //            timeOutCount++;
    //        }
    //        else
    //        {
    //            SystemMessageManager.Instance.InvokeSystemError(1, "SYS_ERR_NETWORK", "SYS_ERR_SVR_INFO_LOADING_FAILED_CHECKING_NETWORK");
    //            yield break;
    //        }

    //        yield return waitForSeconds;
    //    }

    //    timeOutCount = 0;
    //    yield return waitForSeconds;

    //    currentState.text = "게임 버전 정보 확인 중...";

    //    string version;
    //    JsonDataParser.Parse(ServerConfigManager.GetConfig("version"), out version);
    //    if (version != "v.1.1.200310.001")
    //    {
    //        SystemMessageManager.Instance.InvokeSystemError(2, "SYS_ERR_NETWORK", "SYS_INVALID_CLIENT_VER_LOW");
    //        yield break;
    //    }

    //    timeOutCount = 0;
    //    yield return waitForSeconds;

    //    currentState.text = "게임 서버에 연결 중...";

    //    string url;
    //    int port;
    //    JsonDataParser.Parse(ServerConfigManager.GetConfig("gameServer_url"), out url);
    //    JsonDataParser.Parse(ServerConfigManager.GetConfig("gameServer_port"), out port);
    //    WebSocketManager.Init(string.Format("http://{0}:{1}/", url, port));

    //    yield return waitForSeconds;

    //    gameObject.SetActive(false);
    //    loginManager.OnLoginButton();
    //}
}
