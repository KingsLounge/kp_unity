using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
using Newtonsoft.Json.Linq;



public class PopUpCafeGameCreateSelect : MonoBehaviour
{
    public Cafe manager;



    private void Awake()
    {
    }

    private void Start()
    {

    }

    public void CreateCafe()
    {
        Packet p = new Packet((int)CPProtocol.CP_CAFE_CREATE);
        var c = new JObject();
        System.Random rand = new System.Random();
        int random = rand.Next(10, 99);

        c.Add("title", "카페 " + random);
        c.Add("desc", "대박나세요");
        c.Add("tier", 0);
        p.Add("info", c);

        WebSocketManager.defaultCli.Send(p.ToJson());

        gameObject.SetActive(false);
    }

    public void OnClickOk()
    {
        CreateCafe();
    }
    public void OnClickCancel()
    {
        gameObject.SetActive(false);

    }
}
