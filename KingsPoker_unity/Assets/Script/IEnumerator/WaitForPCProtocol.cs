using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;
using System;

public class WaitForPCProtocol : CustomYieldInstruction
{
    private bool complete = false;
    private bool registration = false;
    public delegate bool WaitForPCProtocolCondition(PCProtocol p, JObject c);
    private WaitForPCProtocolCondition condition = null;
    public Packet Result
    {
        get;private set;
    }
    public WaitForPCProtocol(PCProtocol p)
    {
        condition = (packet,c) =>
        {
            return packet == p;
        };
        WebSocketManager.defaultCli.OnMessage += OnMessage;
        registration = true;
    }

    public WaitForPCProtocol(PCProtocol p1, PCProtocol p2)
    {
        condition = (packet, c) =>
        {
            return packet == p1 || packet == p2;
        };
        WebSocketManager.defaultCli.OnMessage += OnMessage;
        registration = true;
    }

    public WaitForPCProtocol(WaitForPCProtocolCondition condition)
    {
        this.condition = condition;
        WebSocketManager.defaultCli.OnMessage += OnMessage;
        registration = true;
    }

    ~WaitForPCProtocol()
    {
        Unregistration();
    }

    private void Unregistration()
    {
        if(registration)
        {
            registration = false;
            WebSocketManager.defaultCli.OnMessage -= OnMessage;
        }
    }
    private void OnMessage(string msg)
    {
        JObject json = JObject.Parse(msg);
        if (!complete)
        {
            PCProtocol p = (PCProtocol)(json.ValueOrDefault("p", 0));
            JObject c = json["c"] as JObject;
            complete = condition(p,c);
            if (complete)
            {
                Result = new Packet(p, c);
                Unregistration();
            }
        }
    }

    public override bool keepWaiting
    {
        get
        {
            return !complete;
        }
    }
}
