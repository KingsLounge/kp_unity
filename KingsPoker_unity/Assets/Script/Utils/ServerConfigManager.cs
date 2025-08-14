using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BestHTTP;
using System;
using UnityEngine.Networking;
using Newtonsoft.Json.Linq;

public class ServerConfigManager : MonoBehaviour
{
    #region Singleton Initialize

    private static ServerConfigManager sInstance;

    public static ServerConfigManager Instance
    {
        get
        {
            sInstance = FindObjectOfType(typeof(ServerConfigManager)) as ServerConfigManager;
            if (sInstance == null)
            {
                GameObject newGameObject = new GameObject("ServerConfigManager");
                sInstance = newGameObject.AddComponent<ServerConfigManager>();
            }

            return sInstance;
        }
    }

    #endregion

    public delegate void ServerConfigLoadSuccessCallback();
    public delegate void ServerConfigLoadFailCallback();
    private static JArray serverConfig = null;

    public static void LoadServerConfig(string url,ServerConfigLoadSuccessCallback callback, ServerConfigLoadFailCallback failCallback) {
        HTTPRequest xml = new HTTPRequest(new Uri(url),delegate(HTTPRequest originalRequest, HTTPResponse response) {
            if(response.IsSuccess) {
                JArray data = JArray.Parse(response.DataAsText);
                serverConfig = data;
                callback();
            } else {
                failCallback();
            }
        });
        xml.Send();
    }


    public static void LoadServerConfig(string url,ServerConfigLoadSuccessCallback callback) {
        HTTPRequest xml = new HTTPRequest(new Uri(url),delegate(HTTPRequest originalRequest, HTTPResponse response) {
            if(response.IsSuccess) {
                JArray data = JArray.Parse(response.DataAsText);
                serverConfig = data;
                callback();
            }
            else
            {
                Debug.LogError(string.Format("{0} : {1}", response.StatusCode, response.DataAsText));
            }    
        });
        xml.Send();
        Debug.Log("서버설정데이터요청");
    }

    public static void LoadServerConfig(string url) {
        HTTPRequest xml = new HTTPRequest(new Uri(url),delegate(HTTPRequest originalRequest, HTTPResponse response) {
            if(response.IsSuccess) {
                JArray data = JArray.Parse(response.DataAsText);
                serverConfig = data;
            }
        });
        xml.Send();
    }

    public static bool Contains(string key)
    {
        JObject config = ServerConfigManager.serverConfig[DevOptionsManager.devOptions.serverIndex] as JObject;
        return config.ContainsKey(key);
    }

    public void LoadServerConfig(string url, Action successCallback, Action failCallback)
    {
        StartCoroutine(TryLoadServerConfig(url, successCallback, failCallback));
    }

    public IEnumerator TryLoadServerConfig(string url, Action successCallback, Action failCallback)
    {
        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            yield return www.SendWebRequest();

             if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                ErrorMessageManager.Instance.AddNetworkError(1, "SYS_ERR_NETWORK", www.error, ErrorHandlingType.RECALL, null, () =>
                {
                    CustomSceneManager.LoadLoginScene();
                });
                Console.Error("LoadServerConfigError: " + www.error);
                failCallback();
            }
            else
            {
                JArray data = JArray.Parse(www.downloadHandler.text);
                serverConfig = data;
                successCallback(); 
            }
        }
    }

    public static JToken GetConfig(string key) {
        JObject config = ServerConfigManager.serverConfig[DevOptionsManager.devOptions.serverIndex] as JObject;
        return config[key];
    }

    public static JToken GetConfig(int index,string key)
    {
        return ServerConfigManager.serverConfig[index][key];
    }

    public static int GetConfigSize()
    {
        return ServerConfigManager.serverConfig.Count;
    }
}
