using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DevManager : MonoBehaviour
{
    public static DevManager Instance;

    public Text loginStatus;
    public GameObject LogButton;

    private bool pubLogin = false;
    private bool wsConnect = false;
    private bool gsLogin = false;
    private int wsDelegate = 0;

    private void Awake()
    {
        Instance = this;
        var dev = DevOptionsManager.devOptions.mode == MODE.dev;
        loginStatus.gameObject.SetActive(dev);
        LogButton.SetActive(dev);
        if (dev)
        {
            UpdateText();
        }
    }

    public void ClickLogout()
    {
        PublisherApiManager.Instance.token = "foijqwoeihfpoaiwjehfoiq;jw12o09i3u902";
        //var success = await PublisherApiManager.Instance.Logout();
        Console.Log($"Log out : {true}");
    }

    public bool PubLogin
    {
        get { return pubLogin; }
        set
        {
            pubLogin = value;
            UpdateText();
        }
    }

    public bool WsConnect
    {
        get { return wsConnect; }
        set
        {
            wsConnect = value;
            UpdateText();
        }
    }

    public bool GsLogin
    {
        get { return gsLogin; }
        set
        {
            gsLogin = value;
            UpdateText();
        }
    }

    public int WsDelegate
    {
        get { return wsDelegate; }
        set
        {
            wsDelegate = value;
            UpdateText();
        }
    }

    public void UpdateText()
    {
        loginStatus.text = string.Format(
            "pl: {0}  wc: {1}  wd: {2}  gl: {3}",
            pubLogin,
            wsConnect,
            wsDelegate,
            gsLogin
        );
    }
}
