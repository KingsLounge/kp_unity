using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BestHTTP.WebSocket;
using Newtonsoft.Json.Linq;
using UnityEngine;

struct WebSocketEvent
{
    public WebSocketEventType evt;
    public object[] param;
}

enum WebSocketEventType
{
    Open,
    Close,
    Error,
    Message,
    Exit,
    Reconnect,
    ReconnectFail,
    ReconnectSuccess
}

public class WebSocketClient
{
    private static int instanceID = 0;
    private int instanceID_ = 0;
    public int InstanceID
    {
        get { return instanceID_; }
    }
    private WebSocket ws;
    public delegate void OpenDelegate();
    public delegate void CloseDelegate(string reson);
    public delegate void ExitDelegate(string reson);
    public delegate void MessageDelegate(string msg);
    public delegate void ErrorDelegate(string reson);
    public delegate void ReconnectDelegate(int tryCount, bool isLast);
    public delegate void ReconnectFailDelegate();
    public delegate void ReconnectSuccessDelegate();
    public ReconnectDelegate OnReconnect;
    public ReconnectDelegate OnReconnectOnce;
    public ReconnectFailDelegate OnReconnectFail;
    public ReconnectFailDelegate OnReconnectFailOnce;
    public ReconnectSuccessDelegate OnReconnectSuccess;
    public ReconnectSuccessDelegate OnReconnectSuccessOnce;
    public OpenDelegate OnOpen;
    public CloseDelegate OnClose;
    public MessageDelegate OnMessage;
    public ErrorDelegate OnError;
    public ExitDelegate OnExit;
    public OpenDelegate OnOpenOnce;
    public CloseDelegate OnCloseOnce;
    public MessageDelegate OnMessageOnce;
    public ErrorDelegate OnErrorOnce;
    public ExitDelegate OnExitOnce;
    private int reconnectMaxCount = 4;
    private int reconnectCount = 0;
    private float reconnectDelay = 2f;
    private bool reconnecting = false;
    private bool _isOpened = false;
    private string url;
    public bool isOpened
    {
        get { return this._isOpened; }
    }
    private Queue<WebSocketEvent> queue = new Queue<WebSocketEvent>();

    public bool recvProtocolBlock = false;

    public WebSocketClient(string url)
    {
        this.url = url;
        instanceID_ = instanceID++;
        GenerateWebSocket(url);
        CustomUpdateCaller.Instance.PreUpdate += Update;
    }

    public void Update(double dt)
    {
        while (queue.Count > 0 && WebSocketManager.canTransitionPacket)
        {
            WebSocketEvent evt = queue.Dequeue();
            try
            {
                switch (evt.evt)
                {
                    case WebSocketEventType.Open:
                        if (OnOpen != null)
                            OnOpen();
                        if (OnOpenOnce != null)
                        {
                            OnOpenOnce();
                            OnOpenOnce = null;
                        }
                        break;
                    case WebSocketEventType.Close:
                        if (OnClose != null)
                            OnClose((string)evt.param[0]);
                        if (OnCloseOnce != null)
                        {
                            OnCloseOnce((string)evt.param[0]);
                            OnCloseOnce = null;
                        }
                        break;
                    case WebSocketEventType.Message:

                        if (OnMessage != null)
                            OnMessage((string)evt.param[0]);
                        if (OnMessageOnce != null)
                        {
                            OnMessageOnce((string)evt.param[0]);
                            OnMessageOnce = null;
                        }
                        break;
                    case WebSocketEventType.Error:
                        if (OnError != null)
                            OnError((string)evt.param[0]);
                        if (OnErrorOnce != null)
                        {
                            OnErrorOnce((string)evt.param[0]);
                            OnErrorOnce = null;
                        }
                        break;
                    case WebSocketEventType.Exit:
                        if (OnExit != null)
                            OnExit((string)evt.param[0]);
                        if (OnExitOnce != null)
                        {
                            OnExitOnce((string)evt.param[0]);
                            OnExitOnce = null;
                        }
                        break;
                    case WebSocketEventType.Reconnect:
                        if (OnReconnect != null)
                            OnReconnect((int)evt.param[0], (bool)evt.param[1]);
                        if (OnReconnectOnce != null)
                        {
                            OnReconnectOnce((int)evt.param[0], (bool)evt.param[1]);
                            OnReconnectOnce = null;
                        }
                        break;
                    case WebSocketEventType.ReconnectFail:
                        if (OnReconnectFail != null)
                            OnReconnectFail();
                        if (OnReconnectFailOnce != null)
                        {
                            OnReconnectFailOnce();
                            OnReconnectFailOnce = null;
                        }
                        break;
                    case WebSocketEventType.ReconnectSuccess:
                        if (OnReconnectSuccess != null)
                            OnReconnectSuccess();
                        if (OnReconnectSuccessOnce != null)
                        {
                            OnReconnectSuccessOnce();
                            OnReconnectSuccessOnce = null;
                        }
                        break;
                }
            }
            catch (System.Exception e)
            {
                if (evt.param != null && evt.param.Length > 0)
                {
                    var ex = new Exception("에러 발생 패킷 : " + evt.param[0], e);
                    Debug.LogException(ex);
                }
                else
                {
                    Debug.LogException(e);
                }
            }
        }
    }

    private void GenerateWebSocket(string url)
    {
        ws = new WebSocket(new Uri(url), "", "echo-protocol");
        ws.OnOpen = WebSocketOpen;
        ws.OnClosed = WebSocketClose;
        ws.OnMessage = WebSocketMessage;
        ws.OnError = WebSocketError;
        ws.Open();
    }

    public bool Close()
    {
        if (!_isOpened)
        {
            return false;
        }
        ws.OnClosed = WebSocketExit;
        ws.Close();
        return true;
    }

    public void Clear()
    {
        OnReconnect = null;
        OnReconnectOnce = null;
        OnReconnectFail = null;
        OnReconnectFailOnce = null;
        OnReconnectSuccess = null;
        OnReconnectSuccessOnce = null;
        OnOpen = null;
        OnClose = null;
        OnMessage = null;
        OnError = null;
        OnExit = null;
        OnOpenOnce = null;
        OnCloseOnce = null;
        OnMessageOnce = null;
        OnErrorOnce = null;
    }

    public void Send(string message)
    {
        message = String.Concat(message.Where(c => !(Char.IsWhiteSpace(c) && c != ' ')));

        recvProtocolBlock = false;

        if (DevOptionsManager.devOptions != null && DevOptionsManager.devOptions.mode == MODE.dev)
        {
            JObject json = JObject.Parse(message);
            CPProtocol p = (CPProtocol)(int)json["p"];
            if (p != CPProtocol.CP_PING)
            {
                Console.Log("<color=red> " + p + " </color>" + message);
            }

            InfoManager.PushApiAndProtocol(",," + p.ToString() + ",", message);
        }

        ws.Send(message);
    }

    public void Send(Packet packet)
    {
        Send(packet.ToJson());
    }

    public static string JsonStringConvert(string st)
    {
        return String.Concat(st.Where(c => !char.IsWhiteSpace(c)));
    }

    private void WebSocketOpen(WebSocket ws)
    {
        _isOpened = true;
        if (reconnecting)
        {
            WebSocketEvent reconnectSuccess = new WebSocketEvent();
            reconnectSuccess.evt = WebSocketEventType.ReconnectSuccess;
            reconnectSuccess.param = new object[] { };
            queue.Enqueue(reconnectSuccess);
        }
        reconnectCount = 0;
        reconnecting = false;
        WebSocketEvent evt = new WebSocketEvent();
        evt.evt = WebSocketEventType.Open;
        queue.Enqueue(evt);
        Console.Log(string.Format("OpenWebSoket : {0}", url));
    }

    private void WebSocketClose(WebSocket ws, UInt16 code, string message)
    { //유저가 웹소켓을 닫은게 아닌데 웹소켓이 닫히면 호출
        _isOpened = false;
        WebSocketEvent evt = new WebSocketEvent();
        evt.evt = WebSocketEventType.Close;
        evt.param = new object[] { message };
        queue.Enqueue(evt);
    }

    public void TryReconnect()
    {
        reconnectCount += 1;
        if (reconnectCount <= reconnectMaxCount)
        {
            reconnecting = true;
            WebSocketEvent reconnectEvt = new WebSocketEvent();
            reconnectEvt.evt = WebSocketEventType.Reconnect;
            reconnectEvt.param = new object[]
            {
                reconnectCount,
                reconnectCount >= reconnectMaxCount
            };
            queue.Enqueue(reconnectEvt);
            GenerateWebSocket(url);
        }
        else if (reconnecting)
        {
            WebSocketEvent reconnectFail = new WebSocketEvent();
            reconnectFail.evt = WebSocketEventType.ReconnectFail;
            reconnectFail.param = new object[] { };
            queue.Enqueue(reconnectFail);
        }
        return;
    }

    private void WebSocketExit(WebSocket ws, UInt16 code, string message)
    { //유저가 웹소켓을 닫아서 웹소켓이 닫히면 호출
        _isOpened = false;
        WebSocketEvent evt = new WebSocketEvent();
        evt.evt = WebSocketEventType.Exit;
        evt.param = new object[] { message };
        queue.Enqueue(evt);
    }

    private void WebSocketMessage(WebSocket ws, string msg)
    {
        WebSocketEvent evt = new WebSocketEvent();
        evt.evt = WebSocketEventType.Message;
        evt.param = new object[] { msg };
        queue.Enqueue(evt);
    }

    private void WebSocketError(WebSocket ws, string reson)
    {
        _isOpened = false;
        if (ws.IsOpen)
            ws.Close();
        WebSocketEvent evt = new WebSocketEvent();
        evt.evt = WebSocketEventType.Error;
        evt.param = new object[] { reson };
        queue.Enqueue(evt);
    }

    public void QueueClear()
    {
        queue.Clear();
    }

    public int QueueCountChekc()
    {
        return queue.Count();
    }
}
