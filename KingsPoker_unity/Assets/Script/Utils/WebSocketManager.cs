using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WebSocketManager
{
    private static WebSocketClient _defaultCli = null;
    public static WebSocketClient defaultCli
    {
        get { return WebSocketManager._defaultCli; }
    }
    private static string url = "";

    public static bool canTransitionPacket = true;

    public static WebSocketClient Init(string url)
    {
        if (_defaultCli != null)
        {
            _defaultCli.Clear();
            _defaultCli.Close();
        }
        WebSocketManager.url = url;

        WebSocketClient result = GenerateClient();
        WebSocketManager._defaultCli = result;
        return result;
    }

    public static WebSocketClient GenerateClient()
    {
        return new WebSocketClient(WebSocketManager.url);
    }
}
