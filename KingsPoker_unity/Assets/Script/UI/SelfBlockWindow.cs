using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelfBlockWindow : MonoBehaviour
{
    private double blockTime = 6;

    // Start is called before the first frame update

    private void Awake()
    {
        init();
    }

    public void init()
    {
        SetBlockTime(6);
    }
    public void SetBlockTime(int hours)
    {
        blockTime = new System.TimeSpan(hours, 0, 0).TotalSeconds;
    }
    public void OnClickBlockCunfirmButton()
    {
        PublisherApiManager.Instance.SelfBlock(blockTime, SelfBlockCallBack);
    }

    private void SelfBlockCallBack(long statusCode)
    {
        if (statusCode == 200)
        {
            LogOut();
        }
    }
    public void LogOut()
    {
        WebSocketManager.defaultCli.OnExitOnce += (reson) =>
        {
            DevManager.Instance.GsLogin = false;
            DevManager.Instance.WsDelegate -= 1;
            DevManager.Instance.WsConnect = false;
            FirebaseManager.Instance.SignOut();
            CustomSceneManager.LoadLoginScene();
        };
        WebSocketManager.defaultCli.Close();
    }

}
