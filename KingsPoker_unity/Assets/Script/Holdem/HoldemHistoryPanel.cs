using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;
using UnityEngine.UI;

public class HoldemHistoryPanel : MonoBehaviour
{
    private int currentOffset = 1;
    public GameObject loadingPanel;
    public TableManager table;
    public Text dataText;
    public GameObject rightButton;
    public GameObject leftButton;
    public Text offsetText;
    public Text potMoneyText;

    public GameObject historySlotPrefab;
    public Transform container;
    public List<HistorySlot> historyList;

    private void Awake() {
        currentOffset = 0;
        WebSocketManager.defaultCli.OnMessage += WebSocketOnMessage;
    }

    private void OnEnable() {
        
        offsetText.text = string.Format("current offset : {0}", currentOffset);
        RequestRoomHistory();
    }

    private void WebSocketOnMessage(string msg)
    {
        JObject json = JObject.Parse(msg);
        int p = (int)json["p"];
        JObject c = json["c"] as JObject;
        switch((PCProtocol)p)
        {
            case PCProtocol.PC_ROOM_HISTORY:            
                var gtn = (long)c["data"]["gtn"];
                if(gtn == table.GetRoomNumber())
                {
                    SetHistoryData(c["data"]as JObject);
                    
                }
                else
                {
                    
                }
                
            break;
            case PCProtocol.PC_HOLDEM_GAMERESULT:
                currentOffset++;
                offsetText.text = string.Format("current offset : {0}", currentOffset);
                if(currentOffset>1)
                {
                    rightButton.SetActive(true); 
                }
            break;
        }
    }
    public void NextButton()
    {
        currentOffset --;
        RequestRoomHistory();
    }

    public void BeforeButton()
    {
        currentOffset ++;
        RequestRoomHistory();
    }
    public void SetHistoryData(JObject c)
    {
        //loadingPanel.SetActive(false);
        if((int)c["count"] <= 0)
        {
            currentOffset = 0;
            dataText.text = "히스토리 데이터가 없습니다.";
        }
        else
        {
            dataText.text = c.ToString();
            currentOffset = c["offset"].ToObject<int>();
            SetHistorySlot(c["history"] as JObject);
            leftButton.SetActive(currentOffset < (int)c["count"]-1);
        }
        rightButton.SetActive(currentOffset>0);
    }
    private void SetHistorySlot(JObject c)
    {
        if(historyList == null)
        {
            historyList = new List<HistorySlot>();
        }
//        Debug.LogError(c.ToString());
        var nicks = c["n"].ToObject<string[]>();
        var uids =  c["p"].ToObject<string[]>();
        var cards = c["c"].ToObject<string[]>();
        var cc =    c["cc"].ToObject<string[]>();
        var fold =  c["f"].ToObject<int[][]>();
        var win =   c["w"].ToObject<int[]>();
        var all = c["a"].ToObject<int[]>();
        var count = 0;
        int pot = 0;
        for(int i = 0; i < all.Length; i++)
        {
            pot += all[i];
        }
        potMoneyText.text = MoneyToString.Converting(pot);
        for(int i = 0; i < nicks.Length; i++)
        {
            if(!string.IsNullOrEmpty(nicks[i]))
            {
                HistorySlot go; 
                if(count < historyList.Count)
                {
                    go = historyList[count];
                }
                else
                {
                    go = Instantiate(historySlotPrefab, container).GetComponent<HistorySlot>();
                    historyList.Add(go);
                }
                bool f = false;
                for(int j = 0; j < fold.Length; j++)
                {
                    if(fold[j][i] != 0)
                    {
                        f = true;
                        break;
                    }
                }
                go.SetHistory(win[i], f, nicks[i], cards[i], cc, uids[i], null);
                count ++;
            }
        }
        for(int i = 0; i < historyList.Count; i++)
        {
            historyList[i].gameObject.SetActive(i<count);
        }

    }

    private void RequestRoomHistory()
    {
        if(currentOffset<0) currentOffset = 0;
        offsetText.text = string.Format("current offset : {0}", currentOffset);
        var p = new Packet((int)CPProtocol.CP_ROOM_HISTORY);
        p.Add("gtn",table.GetRoomNumber());
        p.Add("offset", currentOffset);
        WebSocketManager.defaultCli.Send(p.ToJson());
        
    }

    private void OnDestroy() {
        WebSocketManager.defaultCli.OnMessage -= WebSocketOnMessage;
    }


}
