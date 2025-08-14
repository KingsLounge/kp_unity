using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json.Linq;

public class SystemPreprocessing : MonoBehaviour
{
    public GamePreprocessing gamePreprocessing;
    public Text currentState;

    private WaitForSeconds waitForSeconds = new WaitForSeconds(0.3f);

    private int timeOut = 90;
    private int currentTime = 0;

    private void Start()
    {
        StartCoroutine("CheckNetworkReachability");
    }

    private IEnumerator CheckNetworkReachability()
    {
        currentState.text = "네트워크 연결 확인 중...";

        while (Application.internetReachability == NetworkReachability.NotReachable)
        {
            if (currentTime < timeOut)
            {
                currentTime++;
            }
            else
            {
                ErrorMessageManager.Instance.AddNetworkError(0, "SYS_ERR_NETWORK", "SYS_ERR_CHECKING_NETWORK",ErrorHandlingType.RECALL, null, ()=>{CustomSceneManager.LoadLoginScene();});
                yield break;
            }

            yield return waitForSeconds; 
        }

        currentTime = 0;
        StartCoroutine("LoadServerConfigFile");
    }

    private IEnumerator LoadServerConfigFile()
    {
        currentState.text = "서버 설정 정보 불러오는 중...";

        bool isLoadServerConfig = false;
        bool isError = false;
        //ServerConfigManager.LoadServerConfig("https://firebasestorage.googleapis.com/v0/b/jackpotholdem-984af.appspot.com/o/serverConfig.json?alt=media", () =>
        //{
        //    isLoadServerConfig = true;
        //});


        ServerConfigManager.Instance.LoadServerConfig(LinkOptionConstant.storageUrl + LinkOptionConstant.configPath, () =>
        {
            isLoadServerConfig = true;
        },
        () => {
            isError = true;
        });

        while (isLoadServerConfig == false && isError == false)
        {
            if (currentTime < timeOut)
            {
                currentTime++;
            }
            else
            {
                
                ErrorMessageManager.Instance.AddNetworkError(1, "SYS_ERR_NETWORK", "SYS_ERR_SVR_INFO_LOADING_FAILED_CHECKING_NETWORK", ErrorHandlingType.RECALL, null, ()=>
                {
                    CustomSceneManager.LoadLoginScene();
                });
                yield break;
            }

            yield return waitForSeconds;
        }

        currentTime = 0;

        if (isError == false)
        {
            gamePreprocessing.Init();
        }
        //StartCoroutine("CheckGameVersion");
    }

    private IEnumerator CheckGameVersion()
    {
        currentState.text = "게임 버전 정보 확인 중...";
        string version = (string)ServerConfigManager.GetConfig("version");
        var serverSp = version.Split('.');
        var clientSp = Application.version.Split('.');
        if (int.Parse(serverSp[1]) != int.Parse(clientSp[0]) || int.Parse(serverSp[2]) != int.Parse(clientSp[1]))
        {
            ErrorMessageManager.Instance.AddNetworkError(2, "SYS_ERR_NETWORK", "SYS_INVALID_CLIENT_VER_LOW");
            yield break;
        }
#if UNITY_EDITOR
        Console.Log("서버 버전" + version);
        Console.Log("클라이언트 버전" + UnityEditor.PlayerSettings.bundleVersion);
#endif
        yield return waitForSeconds;

        currentTime = 0;
        
        gamePreprocessing.Init();
    }
}
