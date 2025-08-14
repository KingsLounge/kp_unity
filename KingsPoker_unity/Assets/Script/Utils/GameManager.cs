using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class GameManager : MonoBehaviour
{
    static string ConvertToCustomBinary(int decimalNumber)
    {
        // 10진수를 2진수 문자열로 변환
        string binaryString = Convert.ToString(decimalNumber, 2);
        // 최소 6자리까지 표시하기 위해 앞에 0을 추가
        binaryString = binaryString.PadLeft(6, '0');
        // 2진수 문자열을 커스텀 형식으로 변환
        string customBinaryString = "";
        foreach (char c in binaryString)
        {
            if (c == '0')
            {
                customBinaryString += "_";
            }
            else if (c == '1')
            {
                customBinaryString += "-";
            }
        }

        return customBinaryString;
    }

    private enum Login_Flow
    {
        Auto_Login = 0,
        API_Login = 1,
        Websocket_login_Check = 2,
        Api_Ping_Check = 3,
        Websocket_Ping_Check = 4,
    }

    public static GameManager instance { get; private set; }

    private void Awake()
    {
        instance = this;

        PingCheckManager.failure.AddListener((UnityEngine.Events.UnityAction)(() =>
        {
            this.VersionCheck();
        }));
    }

    private bool prevPause = false;
    private int tryCount = 5;

    private void Update()
    {
#if UNITY_EDITOR
        if (DevOptionsManager.devOptions.mode == MODE.dev && Input.GetKeyDown(KeyCode.PageDown))
        {
            VersionCheck();
        }
#endif
    }

    private void OnApplicationPause(bool pause)
    {
        Debug.Log("Application is Paused " + pause.ToString());
        if (prevPause != pause)
        {
            if (pause)
            {
                //LoadingCircle.Instance.ReconnectStart();
            }
            else
            {
                //WebSocketManager.defaultCli.QueueClear();
                VersionCheck();
            }
        }
        prevPause = pause;
    }

    bool isTryLogin = false;
    public void VersionCheck()
    {
        GamePreprocessing.ServerVersionCheck(
            async () =>
            {
                var needUpdate = await GamePreprocessing.CheckGameVersionNonUi();
                if (!needUpdate)
                {
                    TryReLogin();
                }
                else
                {
                    GamePreprocessing.OpenAppStore();
                }
            },
            () =>
            {
                TryReLogin();
            }
        );
    }
    public async void TryReLogin()
    {
        if (!isTryLogin)
        {
            isTryLogin = true;

            
            
            try
            {
                await StartPingCheck();
            }
            catch (Exception e)
            {
                Debug.LogException(new Exception($"PingCheckFails On Game Manager {e}"));
                LoadingCircle.Instance.LoadingClear();
                CustomSceneManager.LoadLoginScene();
            }

            isTryLogin = false;
        }
    }

    private int loginRequestCount = 0;

   
    
        
    
    
    private async UniTask StartPingCheck()
    {
        
        SoundManager.Instance.SetAllMute(true);
        if (CustomSceneManager.GetCurrentSceneName() == CustomSceneManager.GetLoginSceneName())
        {
            return;
        }

        Console.Log("--------TryReLogin--------");

        if (string.IsNullOrEmpty(PublisherApiManager.Instance.token))
        {
            Console.Log("--------IsNullOrEmpty Token--------");

            LoadingCircle.Instance.LoadingStart(ConvertToCustomBinary((int)Login_Flow.Auto_Login));
            if (await FirebaseManager.Instance.CheckAutoLoginAsync())
            {
                LoadingCircle.Instance.LoadingComplete(
                    ConvertToCustomBinary((int)Login_Flow.Auto_Login)
                );
                Console.Log("--------Successed AutoLogin--------");
                LoadingCircle.Instance.LoadingStart(
                    ConvertToCustomBinary((int)Login_Flow.API_Login)
                );
                await LoginManager.instance.RequestPubLogin();
                LoadingCircle.Instance.LoadingComplete(
                    ConvertToCustomBinary((int)Login_Flow.API_Login)
                );
                LoadingCircle.Instance.LoadingStart(
                    ConvertToCustomBinary((int)Login_Flow.Websocket_login_Check)
                );
                if (await PingCheckManager.CheckWebsocketLogin())
                {
                    Console.Log("--------Successed WsLogin--------");
                    LoadingCircle.Instance.LoadingComplete(
                        ConvertToCustomBinary((int)Login_Flow.Websocket_login_Check)
                    );
                    StartReLoad();
                }
                else
                {
                    Console.Log("--------Failed WS Login--------");
                    LoadingCircle.Instance.LoadingComplete(
                        ConvertToCustomBinary((int)Login_Flow.Websocket_login_Check)
                    );
                    ErrorMessageManager.Instance.ReconnectErrorPopup(
                        0,
                        "network_error",
                        "ws_failed",
                        ErrorHandlingType.RECALL,
                        null,
                        (Action)(() =>
                        {
                            this.VersionCheck();
                        })
                    );
                }
            }
            else
            {
                LoadingCircle.Instance.LoadingComplete(
                    ConvertToCustomBinary((int)Login_Flow.Auto_Login)
                );
                CustomSceneManager.LoadLoginScene();
            }
        }
        else
        {
            Console.Log("--------Has Token--------");

            LoadingCircle.Instance.LoadingStart(
                ConvertToCustomBinary((int)Login_Flow.Api_Ping_Check)
            );
            var www = await PingCheckManager.CheckApiPing();
            LoadingCircle.Instance.LoadingComplete(
                ConvertToCustomBinary((int)Login_Flow.Api_Ping_Check)
            );
            Console.Log($"--------Api Ping Result : {www.result}--------");

            if (www.result == UnityWebRequest.Result.Success)
            {
                LoadingCircle.Instance.LoadingStart(
                    ConvertToCustomBinary((int)Login_Flow.Websocket_Ping_Check)
                );
                if (!await PingCheckManager.CheckWebSocketPing())
                {
                    LoadingCircle.Instance.LoadingComplete(
                        ConvertToCustomBinary((int)Login_Flow.Websocket_Ping_Check)
                    );
                    LoadingCircle.Instance.LoadingStart(
                        ConvertToCustomBinary((int)Login_Flow.Auto_Login)
                    );

                    if (await FirebaseManager.Instance.CheckAutoLoginAsync())
                    {
                        LoadingCircle.Instance.LoadingComplete(
                            ConvertToCustomBinary((int)Login_Flow.Auto_Login)
                        );
                        Console.Log("--------Successed AutoLogin--------");
                        LoadingCircle.Instance.LoadingStart(
                            ConvertToCustomBinary((int)Login_Flow.API_Login)
                        );

                        await LoginManager.instance.RequestPubLogin();

                        LoadingCircle.Instance.LoadingComplete(
                            ConvertToCustomBinary((int)Login_Flow.API_Login)
                        );
                        Console.Log("--------Null Or Closed Ws--------");

                        LoadingCircle.Instance.LoadingStart(
                            ConvertToCustomBinary((int)Login_Flow.Websocket_login_Check)
                        );
                        if (await PingCheckManager.CheckWebsocketLogin())
                        {
                            LoadingCircle.Instance.LoadingComplete(
                                ConvertToCustomBinary((int)Login_Flow.Websocket_login_Check)
                            );
                            Console.Log("--------Successed WsLogin--------");

                            StartReLoad();
                        }
                        else
                        {
                            LoadingCircle.Instance.LoadingComplete(
                                ConvertToCustomBinary((int)Login_Flow.Websocket_login_Check)
                            );
                            Console.Log("--------Failed WS Login--------");

                            ErrorMessageManager.Instance.ReconnectErrorPopup(
                                0,
                                "network_error",
                                "ws_failed",
                                ErrorHandlingType.RECALL,
                                null,
                                (Action)(() =>
                                {
                                    this.VersionCheck();
                                })
                            );
                        }
                    }
                    else
                    {
                        LoadingCircle.Instance.LoadingComplete(
                            ConvertToCustomBinary((int)Login_Flow.Auto_Login)
                        );
                        CustomSceneManager.LoadLoginScene();
                    }
                }
                else
                {
                    LoadingCircle.Instance.LoadingComplete(
                        ConvertToCustomBinary((int)Login_Flow.Websocket_Ping_Check)
                    );
                    StartReLoad();
                }

                return;
            }
            else
            {
                if (www.downloadHandler != null && !string.IsNullOrEmpty(www.downloadHandler.text))
                {
                    JObject json = JObject.Parse(www.downloadHandler.text);
                    var ecode = json?.ValueOrDefault("ecode", 0);
                    Console.Log($"--------Api Ping Failed : {json}--------");
                    // if (ecode == 400) // Invalid Token
                    // {
                    //     PublisherApiManager.Instance.token = string.Empty;
                    //     isTryLogin = false;
                    //     TryReLogin();
                    //     return;
                    // }
                    // else if (ecode == 402) // Expired Session
                    // {
                    //     PublisherApiManager.Instance.token = string.Empty;
                    //     isTryLogin = false;
                    //     TryReLogin();
                    //     return;
                    // }

                    if (ecode == 400 || ecode == 402)
                    {
                        LoadingCircle.Instance.LoadingStart(
                            ConvertToCustomBinary((int)Login_Flow.Auto_Login)
                        );

                        if (await FirebaseManager.Instance.CheckAutoLoginAsync())
                        {
                            LoadingCircle.Instance.LoadingComplete(
                                ConvertToCustomBinary((int)Login_Flow.Auto_Login)
                            );
                            Console.Log("--------Successed AutoLogin--------");
                            LoadingCircle.Instance.LoadingStart(
                                ConvertToCustomBinary((int)Login_Flow.API_Login)
                            );

                            await LoginManager.instance.RequestPubLogin();

                            LoadingCircle.Instance.LoadingComplete(
                                ConvertToCustomBinary((int)Login_Flow.API_Login)
                            );
                            Console.Log("--------Null Or Closed Ws--------");

                            LoadingCircle.Instance.LoadingStart(
                                ConvertToCustomBinary((int)Login_Flow.Websocket_login_Check)
                            );
                            if (await PingCheckManager.CheckWebsocketLogin())
                            {
                                LoadingCircle.Instance.LoadingComplete(
                                    ConvertToCustomBinary((int)Login_Flow.Websocket_login_Check)
                                );
                                Console.Log("--------Successed WsLogin--------");

                                StartReLoad();
                            }
                            else
                            {
                                LoadingCircle.Instance.LoadingComplete(
                                    ConvertToCustomBinary((int)Login_Flow.Websocket_login_Check)
                                );
                                Console.Log("--------Failed WS Login--------");

                                ErrorMessageManager.Instance.ReconnectErrorPopup(
                                    0,
                                    "network_error",
                                    "ws_failed",
                                    ErrorHandlingType.RECALL,
                                    null,
                                    (Action)(() =>
                                    {
                                        this.VersionCheck();
                                    })
                                );
                            }
                        }
                        else
                        {
                            LoadingCircle.Instance.LoadingComplete(
                                ConvertToCustomBinary((int)Login_Flow.Auto_Login)
                            );
                            CustomSceneManager.LoadLoginScene();
                        }
                    }
                }
                else { }
            }

            if (await PingCheckManager.CheckNetworkPing())
            {
                if (WebSocketManager.defaultCli.isOpened)
                {
                    return;
                }
                ErrorMessageManager.Instance.ReconnectErrorPopup(
                    0,
                    "network_error",
                    "api_failed",
                    ErrorHandlingType.RECALL,
                    null,
                    (Action)(() =>
                    {
                        this.VersionCheck();
                    })
                );

                return;
            }

            if (PingCheckManager.CheckNetworkConnection())
            {
                ErrorMessageManager.Instance.ReconnectErrorPopup(
                    0,
                    "network_error",
                    "ping_failed",
                    ErrorHandlingType.RECALL,
                    null,
                    (Action)(() =>
                    {
                        this.VersionCheck();
                    })
                );

                return;
            }

            ErrorMessageManager.Instance.ReconnectErrorPopup(
                0,
                "network_error",
                "network_not_connected",
                ErrorHandlingType.RECALL,
                null,
                (Action)(() =>
                {
                    this.VersionCheck();
                })
            );
        }

        SoundManager.Instance.ResetAllMute();

        //Debug.Log("api token = " + PublisherApiManager.Instance.token);
        //if (!string.IsNullOrEmpty(PublisherApiManager.Instance.token))
        //{
        //    Debug.Log("Try API Connecting");
        //    LoadingCircle.Instance.StartSpin();
        //    while ( loginRequestCount < tryCount)
        //    {
        //        loginRequestCount++;
        //        UnityWebRequest www = await PublisherApiManager.Instance.PingCheckAsync();


        //        if (!(www.isNetworkError || www.isHttpError) && www.responseCode == 200)
        //        {
        //            Debug.Log("API Connecting Success");
        //            if (WebSocketManager.defaultCli != null && WebSocketManager.defaultCli.isOpened)
        //            {
        //                Debug.Log("WebSocket Connecting Success");
        //                LoadingCircle.Instance.StopSpin();
        //                StartReLoad();
        //                loginRequestCount = 0;

        //            }
        //            else
        //            {
        //                Debug.Log("WebSocket Connecting Fail");
        //                LoadingCircle.Instance.StopSpin();
        //                if(CustomSceneManager.GetCurrentSceneName() != CustomSceneManager.GetLoginSceneName()) //현재 로그인씬이 아니면
        //                    LoginManager.instance.RequestPubLogin();
        //            }
        //            return;
        //        }
        //        else
        //        {
        //            Debug.Log("API Connecting Fail");
        //            if (loginRequestCount >= tryCount - 1)
        //            {

        //            }
        //            else
        //            {
        //                await UniTask.WaitForSeconds(2f);
        //                Debug.Log("API Connecting Retry " + (loginRequestCount + 1).ToString());
        //            }
        //        }
        //    }
        //    LoadingCircle.Instance.StopSpin();
        //    CustomSceneManager.LoadLoginScene();
        //    loginRequestCount = 0;
        //}
    }

    private List<string> resetList = new List<string>();
    private bool reloading = false;

    private bool game = false;

    private async void StartReLoad()
    {
        if (reloading)
        {
            return;
        }
        while (WebSocketManager.defaultCli.QueueCountChekc() > 0)
        {
            await UniTask.WaitForSeconds(0.01f);
        }

        ErrorMessageManager.Instance.ReconnectSuccess();
        LoadingCircle.Instance.ReconnectStart();
        resetList.Clear();
        resetList.Add("ent");
        resetList.Add("tnmt");
        resetList.Add("ob");
        await UniTask.WaitForSeconds(0.2f);
        InfoManager.Instance.EnterdClear();
        MyStatus.GetUserInfo();
        GamesManager.instance?.AddObserverRooms();
        WebSocketManager.defaultCli.Send(new Packet(CPProtocol.CP_ROOM_IN_PLAYING));
        RequestMyTournament();
        RequestCafeList();
        LobbyTabsManager.Instance?.TabReset();
    }

    public async void LoadEnd(string endstr)
    {
        resetList.Remove(endstr);
        if (resetList.Count == 0)
        {
            ErrorMessageManager.Instance.ReconnectSuccess();
            await UniTask.WaitForSeconds(0.2f);
            LoadingCircle.Instance.ReconnectEnd();
        }
    }

    public void RequestMyTournament()
    {
        Packet p = new Packet((int)CPProtocol.CP_TNMT_MY);
        WebSocketManager.defaultCli.Send(p.ToJson());
    }

    public void RequestCafeList()
    {
        Packet p = new Packet((int)CPProtocol.CP_CAFE_LIST);
        WebSocketManager.defaultCli.Send(p.ToJson());
    }
    //public void Update() {
    //    if(Input.GetKeyDown(KeyCode.Q))
    //    {
    //        OnApplicationPause(!prevPause);
    //    }
    //}
}
