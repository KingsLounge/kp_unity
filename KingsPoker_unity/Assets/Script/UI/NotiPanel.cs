using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Newtonsoft.Json.Linq;

public class NotiPanel : MonoBehaviour
{
    UniWebView webView; 
    public RectTransform viewBack;
    private int idx = 0;
    public Text notiCount;
    JArray noti;
    public LobbyManager lobby;
    public RawImage notiImage;
    [SerializeField] Toggle[] noShowThisDaysToggles;

    private void Awake()
    {
        webView = new GameObject("Uniwebview").AddComponent<UniWebView>();
        notiCount.text = string.Format("{0} / {1}", idx + 1, noti.Count);
        webView.transform.SetParent(viewBack);
        var size = new Vector2(viewBack.rect.width, viewBack.rect.height) * viewBack.lossyScale;
        var pos = (Vector2)viewBack.transform.position;
        pos.y = Screen.height - pos.y - size.y;
        var rect = new Rect(pos, size);
        webView.Frame = rect;
    }

    private void Start()
    {
        NotiLoad(noti[0].ToString());
    }

    public void Notice(JArray notice)
    {
    #if UNITY_EDITOR_OSX
    #else
        if(notice != null && notice.Count != 0)
        {
            noti = notice;
            gameObject.SetActive(true);
        }
    #endif

    
    }

    public void Next()
    {
        idx ++;
        if(idx < noti.Count)
        {
            NotiLoad(noti[idx].ToString());
            notiCount.text = string.Format("{0} / {1}", idx+1, noti.Count);
        }
        else
        {
            
            webView.Stop();
            webView.Hide();
            gameObject.SetActive(false);
            Console.Log("Noti Panel Close");
            //lobby.loginCheck.SetLoginCheckData();
            //PublisherApiManager.Instance.GetUserInfoPubAPI(lobby.UserInfoCallBack);
        }
    }
    public void NotiLoad(string url)
    {

        var noShowTimeStr = PlayerPrefs.GetString(string.Format("{0}_{1}", noti[idx].ToString(), "noShow"), "");
        if (!string.IsNullOrEmpty(noShowTimeStr))
        {
            var noShowTime = DateTimeParser.Parse(noShowTimeStr);
            var today = DateTime.Today;
            if (noShowTime >= today)
            {
                Next();
                return;
            }
        }

        foreach (var noShowToggle in noShowThisDaysToggles)
        {
            if (noShowToggle)
            {
                noShowToggle.isOn = false;
            }
        }
        string extension = url.Substring(url.LastIndexOf('.') + 1);
        
        if (extension.ToLower() == "jpeg" || extension.ToLower() == "png")
        {
            webView.Hide();
            StartCoroutine(CoLoadImageTexture(url));
        }
        else
        {
            webView.Load(url);
            webView.Show();
            notiImage.gameObject.SetActive(false);
        }
    }

    public IEnumerator CoLoadImageTexture(string url)
    {
        UnityWebRequest www = UnityWebRequestTexture.GetTexture(url);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.Log(www.error);
            Next();
        }
        else
        {
            notiImage.texture = ((DownloadHandlerTexture)www.downloadHandler).texture;
            notiImage.gameObject.SetActive(true);
        }
    }
}
