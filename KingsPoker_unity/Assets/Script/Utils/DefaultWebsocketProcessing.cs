using System.Collections;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class DefaultWebSocketProcessing
{
    public static void OnMessage(byte[] bytes)
    {
        string msg = Encoding.UTF8.GetString(bytes);
        OnMessage(msg);
    }

    public static void OnMessage(string msg)
    {
        JObject json = JObject.Parse(msg);
        int p = (int)json["p"];
        JObject c = JObject.Parse(json["c"].ToString());
        switch ((PCProtocol)p)
        {
            case PCProtocol.PC_KICK:
                switch (c.ValueOrDefault("reason", KICK_REASON.UNKNOWN))
                {
                    case KICK_REASON.ANOTHER_LOGIN:
                        {
                            ErrorMessageManager.Instance.AddNetworkError(
                                (int)c["reason"],
                                "SYS_ERR_NETWORK",
                                "SYS_ERR_CONNECTING_OTHER_CLIENT",
                                ErrorHandlingType.RECALL,
                                () =>
                                {
                                    DevManager.Instance.GsLogin = false;
                                    DevManager.Instance.WsConnect = false;
                                    DevManager.Instance.WsDelegate -= 1;

                                    FirebaseManager.Instance.SignOut();
                                    WebSocketManager.defaultCli.Close();
                                    DevManager.Instance.PubLogin = false;
                                    CustomSceneManager.LoadLoginScene();
                                },
                                null
                            );
                        }
                        break;
                    default:
                        {
                            ErrorMessageManager.Instance.AddNetworkError(
                                (int)c["reason"],
                                "SYS_ERR_NETWORK",
                                "SYS_CLIENT_DISCONNECTING",
                                ErrorHandlingType.RECALL,
                                () =>
                                {
                                    DevManager.Instance.GsLogin = false;
                                    DevManager.Instance.WsConnect = false;
                                    DevManager.Instance.WsDelegate -= 1;

                                    FirebaseManager.Instance.SignOut();
                                    WebSocketManager.defaultCli.Close();
                                    DevManager.Instance.PubLogin = false;
                                    CustomSceneManager.LoadLoginScene();
                                },
                                null
                            );
                        }
                        break;
                }

                break;
        }

        if (DevOptionsManager.devOptions.mode == MODE.dev)
        {
            // Debug Console에서 안 보이게 하고 싶은 패킷
            switch ((PCProtocol)p)
            {
                case PCProtocol.PC_HOLDEM_COMMAND:
                    if ((POKER_FLOW)(int)c["command"] == POKER_FLOW.draw_cards)
                    {
                        Console.Log(
                            "<color=#007ACC> " + ((PCProtocol)p).ToString() + "</color>" + msg
                        );
                    }

                    break;

                case PCProtocol.PC_ROOM_COMMAND:
                    {
                        ROOM_COMMAND command = (ROOM_COMMAND)(int)c["command"];
                        if (
                            command == ROOM_COMMAND.start
                            || command == ROOM_COMMAND.calc_dividend
                            || command == ROOM_COMMAND.end
                        )
                        {
                            Console.Log(
                                "<color=#007ACC> " + ((PCProtocol)p).ToString() + "</color>" + msg
                            );
                        }
                    }

                    break;
                case PCProtocol.PC_PING:
                    break;
                default:
                    Console.Log("<color=#007ACC> " + ((PCProtocol)p).ToString() + "</color>" + msg);
                    break;
            }

            InfoManager.PushApiAndProtocol(",,," + ((PCProtocol)p).ToString(), msg);
        }
    }

    public static void OnClose(string reson)
    {
        Console.Error("WebSocket OnClosed - " + reson);
        string[] no_try_reson = new string[] { "Normal connection closure" };
        if (System.Array.IndexOf(no_try_reson, reson) == -1)
        {
            GameManager.instance.VersionCheck();
        }
    }

    public static void OnError(string reson)
    {
        Console.Error("WebSocket OnError - " + reson);
        GameManager.instance.VersionCheck();
        //if (reson.Contains("Exception: TCP Stream closed unexpectedly by the remote server") || reson.Contains("Exception: Unable to write data to the transport connection"))
        //{
        //    Console.Error("Server is closed!");
        //    //ErrorMessageManager.Instance.AddNetworkError(100, "SYS_ERR_DISCONNECTING", "SYS_CLIENT_DISCONNECTING", ErrorHandlingType.RECALL, null, () => {
        //    //    DevManager.Instance.GsLogin = false;
        //    //    DevManager.Instance.WsConnect = false;
        //    //    DevManager.Instance.WsDelegate -= 1;

        //    //    FirebaseManager.Instance.SignOut();
        //    //    DevManager.Instance.PubLogin = false;

        //    //    CustomSceneManager.LoadLoginScene();
        //    //});
        //}
        //CustomUpdateCaller.Instance.PlayCoroutine(TryReconnect());
    }

    //public static IEnumerator TryReconnect()
    //{
    //    yield return new WaitForSeconds(3f);
    //    WebSocketManager.defaultCli.TryReconnect();
    //}

    public static void OnOpen() { }

    public static void OnReconnecting(int count, bool last)
    {
        Console.SpecialLog("재연결 시도중.." + count);
        if (last)
        {
            Console.SpecialLog("마지막 재연결 시도");
        }
    }

    public static void OnReconnectingFail()
    {
        Console.SpecialLog("재연결 실패");
    }

    public static void OnReconnectingSuccess()
    {
        Console.SpecialLog("재연결 성공");
    }
}
