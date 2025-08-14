using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using Newtonsoft.Json.Linq;
using Cysharp.Threading.Tasks;

public class MailSlot : MonoBehaviour
{
    public GameObject itemPrefab;
    public Text titleText;
    public Text contentText;
    public Text sendTimeText;
    public Text deleteTimeText;
    public int idx = 1;
    private int senderIdx = 0;
    private int receiverIdx = 0;
    private int type = 0;
    private bool read = false;
    private DateTime deleteDate;
    private List<MailItem> parcels;
    public Transform parcelContainer;
    [HideInInspector]
    public MailManager manager;
    private JObject mailData;
    private string content;
    private string code;
    public GameObject normalButtons;
    public GameObject cafeJoinButtons;
    public void SetSlot(JObject data)
    {
        mailData = data;
        SetData(data);
        SetType(data);
        SetTime(data);
        SetParcel(data);
        gameObject.SetActive(true);
    }
   
    protected virtual void SetData(JObject data)
    {
        idx = (int)data["idx"];
        senderIdx = (int)data["sender_idx"];
        receiverIdx = (int)data["user_idx"];
        type = (int)data["type"];
        content = (string)data["content"];
    }

    protected virtual void SetType(JObject data)
    {
        normalButtons.SetActive(false);
        cafeJoinButtons.SetActive(false);
        switch (type)
        {
            case 2:
                titleText.text = LocalizeManager.GetLocalString(data["title"].ToObject<string>());
                contentText.text = string.Format(LocalizeManager.GetLocalString("SYS_MAIL_INVITE_ROOM"), data["sender_nick"].ToObject<string>(), content);
                normalButtons.SetActive(true);
                break;
            case 4:
                titleText.text = LocalizeManager.GetLocalString(data["title"].ToObject<string>());
                contentText.text = (string)data["content"];
                cafeJoinButtons.SetActive(true);
                break;
            default:
                titleText.text = (string)data["title"];
                contentText.text = (string)data["content"];
                normalButtons.SetActive(true);
                break;
        }
    }

    protected virtual void SetTime(JObject data)
    {

        if(sendTimeText)
        {
            string sendTimeString = DateTimeParser.Parse(data["created_at"].ToObject<string>()).ToString();
            sendTimeText.text = sendTimeString;
        }
        TimeSpan span = DateTimeParser.Parse(data["deleted_at"].ToObject<string>()) - DateTime.UtcNow;
        deleteTimeText.text = TimeToString.SpanToString(span);

    }

    protected virtual void SetParcel(JObject data)
    {
        
            switch (type)
            {
                case 4:
                {
                    var p = data["parcel"].ToObject<JObject>();
                    code = p.ValueOrDefault("code", string.Empty);
                }
                    break;

                default:
                {
                    var p = data["parcel"].ToObject<JArray>();
                    if (p != null)
                    {
                        if (parcels == null)
                        {
                            parcels = new List<MailItem>();
                        }
                        for (int i = 0; i < p.Count; i++)
                        {
                            MailItem item;
                            if (parcels.Count > i)
                            {
                                item = parcels[i];
                            }
                            else
                            {
                                // 2021-10-26 이후  deprecated   
                                // https://fromthered.atlassian.net/wiki/spaces/SpaceAstar/pages/124518405

                                // item = Instantiate(itemPrefab, parcelContainer).GetComponent<MailItem>();
                                // parcels.Add(item);
                            }

                            JObject parcel = p[i] as JObject;
                            ulong tet = 10;
                            int refIdx = parcel.ValueOrDefault<int>("refIdx", -1);
                            long quantity = parcel.ValueOrDefault<long>("quantity", -1);

                            if (refIdx != -1 && quantity != -1)
                            {
                                JObject refItemObj;
                                InfoManager.itemData.TryGetValue(refIdx, out refItemObj);
                                if (refItemObj != null)
                                {
                                    // itemInfoText.text = refItemObj.ValueOrDefault<string>("description", "");
                                }
                                // item.Set((int)p[i]["refIdx"], (int)p[i]["quantity"]);
                            }
                            else
                            {
                                long gc = parcel.ValueOrDefault<long>("gc", -1);
                                long sc = parcel.ValueOrDefault<long>("sc", -1);

                                if (gc > 0)
                                {
                                    // itemInfoText.text = MoneyToString.Converting(gc) + " 칩";
                                }
                                if (sc > 0)
                                {
                                    // itemInfoText.text = MoneyToString.Converting(sc) + " 코인";
                                }
                            }

                        }
                    }
                }
                    
                    break;
            }

    }


   
    
    public void OnClickOkButton()
    {
        PublisherApiManager.Instance.ReadMailPubAPI(idx, ReadCallback);
    }
    public async void OnClickAcceptButton()
    {
        var p = new Packet(CPProtocol.CP_CAFE_JOIN);
        p.Add("cafeCode", code);
        p.Add("autoJoinCode", code);
        WebSocketManager.defaultCli.Send(p);
        await new WaitForPCProtocol(PCProtocol.PC_CAFE_JOIN);
        OnClickOkButton();
    }
    private void ReadCallback(long statusCode, JObject data)
    {
        switch(statusCode)
        {
            case 200:
                ReadSuccess(data);
            break;
            default:
            break;
        }
    }

    public async void ReadSuccess(JObject data)
    {
        //Console.Error(data);
        
        var gold = (long?)data["update"]["gold"];
        var silver = (long?)data["update"]["silver"];
        if(gold.HasValue)
        {
            await PublisherApiManager.Instance.PointInAPI("gold", gold.Value, PointInAPICallBack);
        }
        if(silver.HasValue)
        {
            await PublisherApiManager.Instance.PointInAPI("silver", silver.Value, PointInAPICallBack);
        }
        LobbyManager.Instance.GetUserInfo();
        manager.RemoveSlot(idx);
    }

     public void PointInAPICallBack(long statusCode, JObject res)
    {
        switch(statusCode) {
                case 200://success
                    Console.Log("PutIn Success");
                    SendUserInfoCheck();
                    break;
                case 400://BadRequest
                    Console.Log("BadRequest");
                    break;
                case 402://BadRequest
                    Console.Log("AbnormalReceipt");
                    break;
                case 409://BadRequest
                    Console.Log("UsedReceipt");
                    break;
            }
    }
    private void SendUserInfoCheck()
    {
        var packet = new Packet((int)CPProtocol.CP_CONSOLE_USERINFO);
        WebSocketManager.defaultCli.Send(packet.ToJson());
        InfoManager.Instance.GetMyItems();
    }
}

public struct parcel
{
    public int refIdx;
    public int quantity;
}