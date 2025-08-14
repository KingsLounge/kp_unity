using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class CustomUIWebSocket : CustomUI
{
    private bool enabledBeforeFrame = false;
    protected virtual void OnEnable()
    {
        enabledBeforeFrame = true;
        WebSocketManager.defaultCli.OnMessage += WebSocketOnMessage;

    }
    protected virtual void OnDisable()
    {
        WebSocketManager.defaultCli.OnMessage -= WebSocketOnMessage;
    }

    protected virtual void CustomOnEnable()
    {

    }

    protected virtual void Update()
    {
        if(enabledBeforeFrame)
        {
            CustomOnEnable();
            enabledBeforeFrame = false;
        }
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
