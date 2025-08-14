using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;
using UnityEngine.UI;
using UnityEngine.U2D;
using Newtonsoft.Json.Linq;

public class QuestPanel : MonoBehaviour
{
    public GameObject questSlotPrefab;
    private List<QuestSlot> questSlotList;
    public List<Transform> dailyContainer;
    public List<Transform> monthlyContainer;
    public SpriteAtlas atlas;
    public QuestSlot monthlyAllClearSlot;
    public QuestSlot dailyAllClearSlot;

   
    public void SetQuest(JObject data)
    {
        questSlotPrefab.SetActive(false);
        //var data = JObject.Parse(dt.ToJson());

        var dailyQuest = data["info"]["dailyQuest"]as JObject;
        var monthlyQuest = data["info"]["monthlyQuest"]as JObject;
        var questData = QuestData.GetQuestData();
        //var dailySuccess = SetClearData(dailyQuest);
        //var monthlySuccess = SetClearData(monthlyQuest);
        //Console.Log(dt.ToJson());
        if(questSlotList == null)
        {
            questSlotList = new List<QuestSlot>();
        }
        int dailyCount = 0;
        int monthlyCount = 0;
        for(int i = 0; i < questData.Count; i++)
        {
            var d = questData[i];
            QuestSlot qs;
            if(questSlotList.Count>i)
            {
                qs = questSlotList[i];
            }
            else
            {
                qs = Instantiate(questSlotPrefab).GetComponent<QuestSlot>();
                questSlotList.Add(qs);
            }
            Sprite sprite = null;
            if(atlas)
            {
                sprite = atlas.GetSprite(d.img);
            }
            int curSucc;
            switch((QUEST_KIND)d.kind)
            {
                case QUEST_KIND.DAILY:
                   // Debug.LogError(d.variable);
                    if (d.variable == "clearCount" && dailyAllClearSlot)
                    {
                        qs = dailyAllClearSlot;
                    }
                    else
                    {
                        qs.transform.SetParent(dailyContainer[dailyCount % dailyContainer.Count]);
                        qs.transform.localScale = Vector3.one;
                        var pos = qs.transform.localPosition;
                        pos.z = 0;
                        qs.transform.localPosition = pos;
                        dailyCount++;
                    }
                    List<int> dailyClears = new List<int>(); 
                    if (dailyQuest == null)
                    {
                        curSucc = 0;
                    }
                    else
                    {
                        Console.Log(d.variable);
                        curSucc = 0;
                        if(dailyQuest[d.variable]!=null)
                        {
                            curSucc = dailyQuest[d.variable].ToObject<int>();
                        }
                        if (dailyQuest["clear"] != null)
                        {
                            dailyClears = dailyQuest["clear"].ToObject<List<int>>();
                        }
                    }
                    
                    
                    qs.SetQuest(d.id, d.type, d.variable, d.opcode, curSucc, d.value, dailyClears.Contains(d.id), d.rewardChip, d.rewardGold, d.rewardTicket, d.kind, d.description, sprite);
                break;
                case QUEST_KIND.WEEKLY:
                break;
                case QUEST_KIND.MONTHLY:
                    if (d.variable == "clearCount" && monthlyAllClearSlot)
                    {
                        qs = monthlyAllClearSlot;
                    }
                    else
                    {
                        qs.transform.SetParent(monthlyContainer[monthlyCount % monthlyContainer.Count]);
                        qs.transform.localScale = Vector3.one;
                        var pos = qs.transform.localPosition;
                        pos.z = 0;
                        qs.transform.localPosition = pos;
                        monthlyCount++;
                    }


                    List<int> monthlyClears = new List<int>();
                    if (monthlyQuest == null)
                    {
                        curSucc = 0;
                    }
                    else
                    {
                        curSucc = 0;
                        if (monthlyQuest[d.variable] != null)
                        {
                            curSucc = monthlyQuest[d.variable].ToObject<int>();
                        }
                        if (dailyQuest["clear"] != null)
                        {
                            monthlyClears = monthlyQuest["clear"].ToObject<List<int>>();
                        }
                    }
                    
                   
                    
                    qs.SetQuest(d.id, d.type, d.variable, d.opcode, curSucc, d.value, monthlyClears.Contains(d.id), d.rewardChip, d.rewardGold, d.rewardTicket, d.kind, d.description, sprite);
                break;
            }
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
    }
    public successData SetClearData(JObject data)
    {
        var success = new successData();
        if(data == null)
        {
            success.clears = new List<int>();
            success.applyTnmtCount = 0;
            success.highHandsCount = 0;
            success.sendEmoJiCount = 0;
            success.winHoldemCount = 0;
            success.playholdemCount = 0;
            success.playMiniGameCount = 0;
            
        }
        else
        {
            success.clears = data["clear"].ToObject<List<int>>() ;

            Console.Error(data.ToString());
            success.applyTnmtCount = //data["applyTnmtCount"].ToObject<int>();
            success.highHandsCount = 
            success.sendEmoJiCount = //data["sendEmojiCount"].ToObject<int>();
            success.winHoldemCount = data["winHoldemCount"].ToObject<int>();
            success.playholdemCount = data["playHoldemCount"].ToObject<int>();
            success.playMiniGameCount = data["playMiniGameCount"].ToObject<int>();
            //success.clears = new List<int>(clears);
        }
        
        return success;
    }

    public void RequestQuestData()
    {
        var p = new Packet((int)CPProtocol.CP_USER_QUESTINFO);
        WebSocketManager.defaultCli.Send(p);
    }


    public struct successData
    {
        public List<int> clears;
        public int applyTnmtCount;
        public int highHandsCount;
        public int sendEmoJiCount;
        public int winHoldemCount;
        public int playholdemCount;
        public int playMiniGameCount;
    }
}

enum QUEST_KIND{DAILY = 1, WEEKLY = 2,MONTHLY = 3}
