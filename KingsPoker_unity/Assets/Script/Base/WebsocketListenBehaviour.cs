using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;

public class WebsocketListenBehaviour : MonoBehaviour
{
    protected virtual void Awake()
    {
        InfoManager.Instance.OnMessage += WebSocketOnMessage;

    }
    protected virtual void OnDestroy()
    {
        InfoManager.Instance.OnMessage -= WebSocketOnMessage;
    }

    private void WebSocketOnMessage(string msg)
    {
        JObject json = JObject.Parse(msg);
        Packet p = new Packet((int)json["p"]);
        p.c = json["c"] as JObject;
        JToken s = json.GetValue("s");
        if (s != null)
        {
            p.s = s.ToString();
        }
        ReceivePacket(p);
    }

    protected virtual void ReceivePacket(Packet packet)
    {

    }
}
