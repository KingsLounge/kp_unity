using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.SimpleAndroidNotifications;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.UI;

public class CafeQuestItem : MonoBehaviour
{
    private JObject questData;

    [SerializeField]
    private LocalText titleText;
    public GameObject questPrefab;
    public Transform questListParent;
    public List<CafeQuestDetailItem> itemList = new List<CafeQuestDetailItem>();

    public Text rewardText;
    public Text rewardCountText;

    private int rewardCount;

    public GameObject rewardButton;
    private int cafeIdx = 0;

    public async void SetQuestData(JObject data, int cafeIdx)
    {
        this.cafeIdx = cafeIdx;
        questData = data;
        var result = data.CastOrEmpty<JObject>("result");
        Console.Log(result);
        var progress = result.CastOrEmpty<JObject>("progress");
        var info = result.CastOrEmpty<JObject>("info");

        var requirements = progress.CastOrEmpty<JArray>("requirements");
        SetQuestList(requirements);

        var rewardable = progress.ValueOrDefault("rewardable", 0);
        rewardCount = rewardable;
        rewardCountText.text = rewardCount.ToString();

        rewardButton.SetActive(rewardable > 0);
        var title = info.ValueOrDefault("title", string.Empty);
        titleText.SetLocalText(title);
        var reward = info.CastOrEmpty<JObject>("reward");
        var amount = reward.ValueOrDefault("amount", 0);
        var item = await RewardItemString(reward);

        rewardText.text = $"[보상] {amount}{item}";
    }

    public async UniTask<string> RewardItemString(JObject reward)
    {
        var item = reward.ValueOrDefault("item", string.Empty);
        String itemString = string.Empty;
        switch (item)
        {
            case "token":
                itemString = "칩";
                break;
            case "kings_ticket":
                var t_type = reward.ValueOrDefault("t_type", 1);
                itemString = await KingshillInfo.GetTicketString(t_type);
                break;
            case "kp":
                itemString = "KP";
                break;
        }
        return itemString;
    }

    public void SetQuestList(JArray list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            CafeQuestDetailItem item = null;
            if (i < itemList.Count)
            {
                item = itemList[i];
            }
            else
            {
                var obj = Instantiate(questPrefab, questListParent);
                item = obj.GetComponent<CafeQuestDetailItem>();
                itemList.Add(item);
            }
            item.SetQuestData(list[i] as JObject);
            item.gameObject.SetActive(true);
        }
        for (int i = list.Count; i < itemList.Count; i++)
        {
            itemList[i].gameObject.SetActive(false);
        }
    }

    public async void RequestReward()
    {
        var questKey = questData.Value<String>("questkey");
        if (string.IsNullOrEmpty(questKey))
        {
            Console.Error("not contain questKey");
            return;
        }
        var p = new Packet(CPProtocol.CP_CAFE_REQUEST_QUEST_REWARD);
        p.Add("questkey", questKey);
        p.Add("cafeIdx", cafeIdx);
        WebSocketManager.defaultCli.Send(p);

        var wait = new WaitForPCProtocol(PCProtocol.PC_CAFE_REQUEST_QUEST_REWARD);
        await wait;
        var result = wait.Result;
        var data = result.c.CastOrEmpty<JArray>("data")[0] as JObject;
        var succes = data.ValueOrDefault("success", false);
        if (succes)
        {
            rewardCount--;
            rewardButton.SetActive(rewardCount > 0);
            rewardCountText.text = rewardCount.ToString();
            var resultObj = data.CastOrEmpty<JObject>("result");
            var item = resultObj.ValueOrDefault("item", string.Empty);
            var rewardString = string.Empty;
            var amount = resultObj.ValueOrDefault("amount", 0);
            switch (item)
            {
                case "kings_ticket":
                    Cafe.instance.RequestKingshillUserInfo();
                    var t_type = resultObj.ValueOrDefault("t_type", 1);
                    
                    var itemString = await KingshillInfo.GetTicketString(t_type);
                    rewardString = $"{itemString} ({amount})";
                    break;
                case "token":
                    rewardString = $"토큰 ({amount})";
                    break;
            }
            NormalMessage.instance.AddSimpleMessage($"{rewardString}을 받았습니다.");
        }
    }
}
