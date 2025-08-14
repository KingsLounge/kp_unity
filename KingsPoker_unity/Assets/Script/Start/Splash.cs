using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

public class Splash : MonoBehaviour
{
    
    void Awake()
    {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
#if UNITY_EDITOR
        Debug.Log("<color=red>Start In Unity Editor</color>");
#endif

#if UNITY_ANDROID
        Debug.Log(string.Format("<color=red>BuildTarget</color> : {0}", "Android"));
#elif UNITY_IOS
        Debug.Log(string.Format("<color=red>BuildTarget</color> : {0}", "IOS"));
#elif UNITY_STANDALON
        Debug.Log(string.Format("<color=red>BuildTarget</color> : {0}", "STANDALON"));
#endif
        
        Invoke("OnLoad", 1f);
        //ServerConfigManager.LoadServerConfig("https://firebasestorage.googleapis.com/v0/b/jackpotholdem-984af.appspot.com/o/serverConfig.json?alt=media&token=38e5c344-7839-47fe-9263-63ee2f6f8959", OnLoad);
    }

    void OnLoad() {
        //string url;
        //int port;
        //JsonDataParser.Parse(ServerConfigManager.GetConfig("gameServer_url"), out url);
        //JsonDataParser.Parse(ServerConfigManager.GetConfig("gameServer_port"), out port);
        //WebSocketManager.Init(string.Format("http://{0}:{1}/", url, port));
        //CustomSceneManager.LoadLoginScene();
    }
}
