using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DealAgreePanel : MonoBehaviour
{
    public GameObject dealRewardItem;
    public Transform dealRewardItemParent;

    public List<DealRewardItem> dealRewardItems = new List<DealRewardItem>();

    public Button agreeButton;

    public Button cancelButton;

    public DateTime dealEndTime;

    public long gtn;

    public Text dealEndTimerText;

    void Start()
    {
        agreeButton.onClick.AddListener(OnAgreeButtonClick);
        cancelButton.onClick.AddListener(OnDisagreeButtonClick);
    }

    void Update()
    {
        if(dealEndTime < DateTime.Now)
        {
            dealEndTimerText.text = "0초";
        }
        else
        {
            TimeSpan ts = dealEndTime - DateTime.Now;
            dealEndTimerText.text = $"{((int)ts.TotalSeconds).ToString("D")}초";
        }
    }

    public void OnAgreeButtonClick()
    {
        RoomStatus room = InfoManager.Instance.GetRoom(gtn);
        if (room == null)
            return;


        Packet p = new Packet(CPProtocol.CP_TNMT_DEAL_AGREE);
        p.Add("tn", room.tn);
        p.Add("gtn", room.gtn);
        p.Add("agree", true);
        WebSocketManager.defaultCli.Send(p);
        if(agreeButton)
        {
            agreeButton.interactable = false;
        }
    }

    public void OnDisagreeButtonClick()
    {
        RoomStatus room = InfoManager.Instance.GetRoom(gtn);
        if (room == null)
            return;


        Packet p = new Packet(CPProtocol.CP_TNMT_DEAL_AGREE);
        p.Add("tn", room.tn);
        p.Add("gtn", room.gtn);
        p.Add("agree", false);
        WebSocketManager.defaultCli.Send(p);
    }

    public void SetDealAgreePanel(JObject c, long gtn)
    {
        this.gtn = gtn;
        JArray players = c["agreed_players"] as JArray;
        JObject my = null;
        for(int i = 0; i < players.Count; i++)
        {
            JObject p = players[i] as JObject;
            if(p["gid"].ToString() == MyStatus.gid)
            {
                my = p;
            }
            if(dealRewardItems.Count <= i)  
            {
                GameObject item = Instantiate(dealRewardItem, dealRewardItemParent);
                dealRewardItems.Add(item.GetComponent<DealRewardItem>());
            }
            dealRewardItems[i].SetDealRewardItem(p);
        }
        for(int i = players.Count; i < dealRewardItems.Count; i++)
        {
            dealRewardItems[i].gameObject.SetActive(false);
        }
        var dealEndTimeMs = c.ValueOrDefault<long>("deal_end_time", 0);
        if(dealEndTimeMs == 0)
        {
            dealEndTimeMs = c.ValueOrDefault<long>("turn_nexttime", 0);
        }
        DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds(dealEndTimeMs);
        dealEndTime = dateTimeOffset.LocalDateTime;

        bool imAgree = my["agree"].ToObject<bool>();
        if(agreeButton)
        {
            agreeButton.interactable = !imAgree;
        }
        
    }
}

