using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;



public class QuestData
{
    private static List<Quest> quests;

    public static void SetQuestData(JObject data)
    {
        
        var questArr = data["list"].ToObject<JArray>();
        quests = new List<Quest>();
        
        for(int i = 0; i < questArr.Count; i++)
        {
            var qData = questArr[i];
            var go = new Quest();
            go.id = qData["id"].ToObject<int>();
            go.name = qData["name"].ToObject<string>();
            go.img = qData["img"].ToObject<string>();
            go.type = qData["type"].ToObject<int>();
            go.kind = qData["kind"].ToObject<int>();
            go.variable = qData["variable"].ToObject<string>();
            go.opcode = qData["opcode"].ToObject<int>();
            go.value = qData["value"].ToObject<int>();
            go.rewardChip = qData["reward_zc"].ToObject<long>();
            go.rewardGold = qData["reward_dc"].ToObject<long>();
            go.rewardTicket = qData["reward_ticket"].ToObject<int>();
            if(qData["description"]!= null)
            {
                go.description = qData["description"].ToObject<string>();
            }
            quests.Add(go);
        }
    } 
    public static List<Quest> GetQuestData()
    {
        return new List<Quest>(quests);
    }
 
}

public struct Quest
{
    public int id;
    public string name;
    public string img;
    public int kind;
    public int type;
    public string variable;
    public int opcode;
    public int value;
    public long rewardChip;
    public long rewardGold;
    public int rewardTicket;
    public string description;
}
