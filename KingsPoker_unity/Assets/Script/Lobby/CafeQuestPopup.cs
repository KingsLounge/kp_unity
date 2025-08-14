using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.UI;

public class CafeQuestPopup : MonoBehaviour
{
    public Text testText;
    public GameObject questPrefab;
    public Transform dailyQuestListParent;
    public Transform weeklyQuestListParent;
    public List<CafeQuestItem> dailyItemList = new List<CafeQuestItem>();
    public List<CafeQuestItem> weeklyItemList = new List<CafeQuestItem>();
    private int cafeIdx = 0;

    public async void RequsetQuestList(int cafeIdx)
    {
        this.cafeIdx = cafeIdx;
        var p = new Packet(CPProtocol.CP_CAFE_QUEST_LIST);
        p.Add("cafeIdx", cafeIdx);
        WebSocketManager.defaultCli.Send(p);
        var wait = new WaitForPCProtocol(PCProtocol.PC_CAFE_QUEST_LIST);
        await wait;
        var data = wait.Result.c.CastOrEmpty<JArray>("data");
        SetQuestList(data);
        gameObject.SetActive(true);

        Debug.Log(wait.Result.ToString());
    }

    public void SetQuestList(JArray list)
    {
        var sortedList = new JArray(list.OrderBy(obj => GetSortOrder(obj as JObject)));
        var dailyList = new JArray(sortedList.Where(obj => GetQuestType(obj as JObject) == 0));
        var weeklyList = new JArray(sortedList.Where(obj => GetQuestType(obj as JObject) == 2));
        for (int i = 0; i < dailyList.Count; i++)
        {
            CafeQuestItem item = null;
            if (i < dailyItemList.Count)
            {
                item = dailyItemList[i];
            }
            else
            {
                var obj = Instantiate(questPrefab, dailyQuestListParent);
                item = obj.GetComponent<CafeQuestItem>();
                dailyItemList.Add(item);
            }
            item.SetQuestData(dailyList[i] as JObject, cafeIdx);
            item.gameObject.SetActive(true);
        }
        for (int i = dailyList.Count; i < dailyItemList.Count; i++)
        {
            dailyItemList[i].gameObject.SetActive(false);
        }
        for (int i = 0; i < weeklyList.Count; i++)
        {
            CafeQuestItem item = null;
            if (i < weeklyItemList.Count)
            {
                item = weeklyItemList[i];
            }
            else
            {
                var obj = Instantiate(questPrefab, weeklyQuestListParent);
                item = obj.GetComponent<CafeQuestItem>();
                weeklyItemList.Add(item);
            }
            item.SetQuestData(weeklyList[i] as JObject, cafeIdx);
            item.gameObject.SetActive(true);
        }
        for (int i = weeklyList.Count; i < weeklyItemList.Count; i++)
        {
            weeklyItemList[i].gameObject.SetActive(false);
        }
    }

    public int GetSortOrder(JObject obj)
    {
        var result = obj.CastOrEmpty<JObject>("result");
        var info = result.CastOrEmpty<JObject>("info");
        var sort = info.ValueOrDefault("sort", 0);
        return sort;
    }

    public int GetQuestType(JObject obj)
    {
        var result = obj.CastOrEmpty<JObject>("result");
        var info = result.CastOrEmpty<JObject>("info");
        var type = info.ValueOrDefault("type", 0);
        return type;
    }
}
