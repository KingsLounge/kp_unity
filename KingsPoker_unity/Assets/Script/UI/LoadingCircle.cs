using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingCircle : MonoBehaviour
{
    private static LoadingCircle instance = null;
    public static LoadingCircle Instance
    {
        get { return LoadingCircle.instance; }
    }
    private bool reconnect = false;
    private bool _spin = false;
    public bool spin
    {
        get => _spin;
    }
    bool timeOutCheck = false;
    public float rotPerSecond = 720f;
    public GameObject circle;

    [SerializeField]
    private GameObject loading;

    [SerializeField]
    private GameObject reconnectLoading;

    private string requestAPI = "";
    private CPProtocol requestProtocol;

    public float noreturnTime = 30f;

    public bool isProtocolLoading = false;

    public Text loadingText;
    public Text reasonsText;
    public float fadeTime = 1f;
    public CanvasGroup loadingCanvasGroup;

    [SerializeField]
    public CanvasGroup recLoadingCanvasGroup;

    private List<string> reasons = new List<string>();

    void Awake()
    {
        LoadingCircle.instance = this;
    }

    public void StartSpin()
    {
        requestAPI = "";
        _spin = true;
        if (loadingText != null)
        {
            loadingText.text = "";
        }
        CancelInvoke();
        if (isProtocolLoading == false)
        {
            Invoke("NoReturn", noreturnTime);
        }
    }

    public void StartSpin(string requestName)
    {
        requestAPI = requestName;
        _spin = true;
        isProtocolLoading = false;
        if (loadingText != null)
        {
            loadingText.text = "API " + requestAPI + " 로딩중";
        }
        CancelInvoke();
        if (isProtocolLoading == false)
        {
            Invoke("NoReturn", noreturnTime);
        }
        Console.Log("API " + requestAPI + " 로딩중");
    }

    public void LoadingStart(string reason)
    {
        if (!reasons.Contains(reason))
        {
            reasons.Add(reason);
            CancelInvoke();
            if (isProtocolLoading == false)
            {
                Invoke("NoReturn", noreturnTime);
            }
        }
        if (loadingText != null)
        {
            reasonsText.text = string.Join(" ", reasons);
        }
    }

    public void LoadingComplete(string reason)
    {
        reasons.Remove(reason);
        if (loadingText != null)
        {
            reasonsText.text = string.Join(" ", reasons);
        }
    }

    public void ReconnectStart(string reason = "")
    {
        reconnect = true;
        SoundManager.Instance.SetAllMute(true);
    }

    public async void ReconnectEnd()
    {
        reconnect = false;
        await UniTask.WaitForSeconds(2);
        SoundManager.Instance.ResetAllMute();
    }

    public void LoadingClear()
    {
        reasons.Clear();
        CancelInvoke();
        if (loadingText != null)
        {
            reasonsText.text = string.Join(" ", reasons);
        }
    }

    public void StartSpin(CPProtocol protocol)
    {
        requestProtocol = protocol;
        _spin = true;
        isProtocolLoading = true;
        if (loadingText != null)
        {
            loadingText.text = "Protocol " + requestProtocol + " 로딩중";
        }
        Console.Log("Protocol " + requestProtocol + " 로딩중");
    }

    public void StopSpin()
    {
        _spin = false;
        isProtocolLoading = false;
        if (loadingText != null)
        {
            loadingText.text = "";
        }
        Console.Log("로딩완료");
    }

    public void NoReturn()
    {
        reasons.Clear();
        NormalMessage.instance.AddSimpleMessage("SYS_ERR_WAITTIME_OVER");
        switch (requestAPI)
        {
            case Constant.API.Post.LOGIN:
            case Constant.API.Post.KAKAO_LOGIN:
            case Constant.API.Post.NAVER_LOGIN:
                LoginManager.recvBlock = true;
                break;
            case Constant.API.Post.CHECK_NICKNAME:
            case Constant.API.Post.REGISTER:
            case Constant.API.Get.GAME_IN:
                LoginManager.recvBlock = true;
                break;
        }

        if (requestProtocol != CPProtocol.CP_PROTOCOL_START)
        {
            WebSocketManager.defaultCli.recvProtocolBlock = true;
        }

        StopSpin();

        //         ErrorMessageManager.Instance.AddNetworkError(
        //             10,
        //             "SYS_ERR_NETWORK",
        //             "SYS_ERR_WAITTIME_OVER" + requestAPI,
        //             ErrorHandlingType.RECALL,
        //             () =>
        //             {
        //                 switch (requestAPI)
        //                 {
        //                     case Constant.API.Post.LOGIN:
        //                     case Constant.API.Post.KAKAO_LOGIN:
        //                     case Constant.API.Post.NAVER_LOGIN:
        //                         LoginManager.recvBlock = true;
        //                         break;
        //                     case Constant.API.Post.CHECK_NICKNAME:
        //                     case Constant.API.Post.REGISTER:
        //                     case Constant.API.Get.GAME_IN:
        //                         LoginManager.recvBlock = true;
        //                         break;
        //                 }

        //                 if (requestProtocol != CPProtocol.CP_PROTOCOL_START)
        //                 {
        //                     WebSocketManager.defaultCli.recvProtocolBlock = true;
        //                 }
        //                 //switch (requestProtocol)
        //                 //{
        //                 //    case CPProtocol.CP_CONSOLE_LOGIN:
        //                 //        WebSocketManager.defaultCli.recvProtocolBlock = true;
        //                 //        break;
        //                 //}
        //             },
        //             () =>
        //             {
        //                 if (requestAPI == "")
        //                 {
        //                     Console.Log("반응 초과");
        //                     requestAPI = "";
        //                     requestProtocol = CPProtocol.CP_PROTOCOL_START;
        //                     return;
        //                 }

        //                 switch (requestAPI)
        //                 {
        //                     case Constant.API.Post.LOGIN:
        //                     case Constant.API.Post.KAKAO_LOGIN:
        //                     case Constant.API.Post.NAVER_LOGIN:
        //                         DevManager.Instance.PubLogin = false;
        //                         LoginManager.recvBlock = true;
        //                         FirebaseManager.Instance.SignOut();
        //                         Console.Log("로그인 취소");
        //                         break;
        //                     case Constant.API.Post.CHECK_NICKNAME:
        //                     case Constant.API.Post.REGISTER:
        //                     case Constant.API.Get.GAME_IN:
        //                         LoginManager.recvBlock = true;
        //                         LoginManager.loginReset.Invoke();
        //                         Console.Log("게임서버 취소");
        //                         break;
        //                 }

        //                 if (requestProtocol != CPProtocol.CP_PROTOCOL_START)
        //                 {
        //                     DevManager.Instance.GsLogin = false;
        //                     WebSocketManager.defaultCli.recvProtocolBlock = true;

        //                     WebSocketManager.defaultCli.Close();
        //                     DevManager.Instance.WsConnect = false;
        //                     DevManager.Instance.WsDelegate = 0;

        //                     DevManager.Instance.PubLogin = false;
        //                     LoginManager.recvBlock = true;
        //                     FirebaseManager.Instance.SignOut();

        // #if Astar
        //                     if (SceneManager.GetActiveScene().name != "Login_Astar")
        //                     {
        //                         CustomSceneManager.LoadLoginScene();
        //                     }
        // #elif Kingdom
        //                     if (SceneManager.GetActiveScene().name != "Login_KH")
        //                     {
        //                         CustomSceneManager.LoadLoginScene();
        //                     }
        // #elif A9J
        //                     if (SceneManager.GetActiveScene().name != "Login")
        //                     {
        //                         CustomSceneManager.LoadLoginScene();
        //                     }
        // #endif
        //                     Console.Log("게임서버 로그인 취소");
        //                 }
        //                 //            switch (requestProtocol)
        //                 //            {
        //                 //                case CPProtocol.CP_CONSOLE_LOGIN:

        //                 //                    DevManager.Instance.GsLogin = false;
        //                 //                    WebSocketManager.defaultCli.recvProtocolBlock = true;

        //                 //                    WebSocketManager.defaultCli.Close();
        //                 //                    DevManager.Instance.WsConnect = false;
        //                 //                    DevManager.Instance.WsDelegate = 0;

        //                 //                    DevManager.Instance.PubLogin = false;
        //                 //                    LoginManager.recvBlock = true;
        //                 //                    FirebaseManager.Instance.SignOut();

        //                 //#if Astar
        //                 //                    if (SceneManager.GetActiveScene().name != "Login_Astar")
        //                 //                    {
        //                 //                        CustomSceneManager.LoadLoginScene();
        //                 //                    }
        //                 //#elif Kingdom
        //                 //                    if (SceneManager.GetActiveScene().name != "Login_KH")
        //                 //                    {
        //                 //                        CustomSceneManager.LoadLoginScene();
        //                 //                    }
        //                 //#endif

        //                 //                    Console.Log("게임서버 로그인 취소");
        //                 //                    break;
        //                 //            }

        //                 requestAPI = "";
        //                 requestProtocol = CPProtocol.CP_PROTOCOL_START;

        //                 StopSpin();
        //             }
        //         ); //"Has No Return.", "서버 연결 끊김");
    }

    private void Update()
    {
        //circle.transform.Rotate(0,0,-Time.deltaTime * rotPerSecond);
        if (reconnect || reconnectLoading.activeSelf)
        {
            if (reconnectLoading.activeSelf != reconnect)
            {
                if (reconnect)
                {
                    CancelInvoke();
                    loading.SetActive(false);
                    reconnectLoading.SetActive(true);
                }
            }
            if (reconnectLoading.activeSelf)
            {
                float alpha = 0;
                if (reconnect)
                {
                    alpha = Mathf.Lerp(
                        0f,
                        1f,
                        recLoadingCanvasGroup.alpha + Time.deltaTime / (fadeTime * 0.5f)
                    );
                    if (alpha > 1f)
                    {
                        alpha = 1f;
                    }
                }
                else
                {
                    alpha = Mathf.Lerp(
                        0f,
                        1f,
                        recLoadingCanvasGroup.alpha - Time.deltaTime / fadeTime
                    );
                    if (alpha <= 0)
                    {
                        alpha = 0;
                        reconnectLoading.SetActive(false);
                    }
                }
                recLoadingCanvasGroup.alpha = alpha;
            }
        }
        else
        {
            bool sp = (spin || reasons.Count > 0);
            if (loading.activeSelf != sp)
            {
                if (sp)
                {
                    loading.SetActive(true);
                }
                else
                {
                    CancelInvoke();
                }
            }
            if (loading.activeSelf)
            {
                float alpha = 0;
                if (sp)
                {
                    alpha = Mathf.Lerp(
                        0f,
                        1f,
                        loadingCanvasGroup.alpha + Time.deltaTime / fadeTime
                    );
                    if (alpha > 1f)
                    {
                        alpha = 1f;
                    }
                }
                else
                {
                    alpha = Mathf.Lerp(
                        0f,
                        1f,
                        loadingCanvasGroup.alpha - Time.deltaTime / fadeTime
                    );
                    if (alpha <= 0)
                    {
                        alpha = 0;
                        loading.SetActive(false);
                    }
                }
                loadingCanvasGroup.alpha = alpha;
            }
        }
    }
}
