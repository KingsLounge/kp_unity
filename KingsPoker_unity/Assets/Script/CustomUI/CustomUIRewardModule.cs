using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class CustomUIRewardModule : CustomUI
{
    public ItemControllerServerCommunication itemController;
    private JArray rewardData = new JArray();
    private List<RewardItem> templates = new List<RewardItem>();

    public int userCount;
    public int tableNum;

    private void Awake()
    {
        itemController.updateItemCallback = (go, data) =>
        {
            RewardItem t = go.GetComponent<RewardItem>();
            if (!templates.Contains(t))
                templates.Add(t);
            t.SetRewardItem(data as JObject);
        };
    }

    public void TestButton()
    {
        SetValue(tableNum, userCount, 5000000);
    }

    public void SetValue(int tableNum, int userCount, long totalPrize = 0, int ticket_type = 0)
    {
        var rewards = TableDataManager.GetRewardData(tableNum, userCount, totalPrize, ticket_type);
        SetValue(rewards);
    }

    public void SetValue(JObject t_rewards, int userCount, int countEntry, long totalPrize = 0, int ticket_type = 0)
    {
        
        JArray rewards;
        var countType = t_rewards.ValueOrDefault("count_type", RWARD_COUNT_CHECK_TYPE.entry);
        var checkRewardCount = countType == RWARD_COUNT_CHECK_TYPE.entry ? countEntry : userCount;
        if (t_rewards.ContainsKey("custom_reward_rank_table"))
        {
            JObject table = t_rewards.Value<JObject>("custom_reward_rank_table");
            var custom_type = t_rewards.ValueOrDefault("custom_type", "normal");
           
            if(custom_type == "percent")
            {
                var tempTable = new JObject();
                foreach(var t in table)
                {
                    var tableData = t.Value as JObject;
                    var temp = new JObject();
                    foreach(var d in tableData) 
                    {
                        var ratio = float.Parse(d.Key);
                        var te = countEntry * ratio / (float)100;
                        var rank = Math.Round(te, MidpointRounding.AwayFromZero);
                        temp[rank.ToString()] = d.Value;
                    }
                    tempTable[t.Key] = temp;
                }
                table = tempTable;
            }

            rewards = TableDataManager.GetRewardList(table, checkRewardCount, totalPrize, ticket_type);
        }
        else
        {
            var reward_rank_table = t_rewards.ValueOrDefault("reward_rank_table", 1) - 1;
            rewards = TableDataManager.GetRewardData(
                reward_rank_table,
                checkRewardCount,
                totalPrize,
                ticket_type
            );
        }

        // kp_rank_table: 칩 분배 테이블과 독립적인 순위별 KP 보상 병합
        var kpTable = t_rewards.CastOrEmpty<JObject>("kp_rank_table");
        if (kpTable != null && kpTable.Count > 0)
        {
            MergeKpRankTable(rewards, kpTable);
        }

        SetValue(rewards);
    }

    private void MergeKpRankTable(JArray rewards, JObject kpTable)
    {
        // 기존 순위 행에 kp 필드 병합
        var maxRank = 0;
        foreach (var r in rewards)
        {
            var row = r as JObject;
            if (row == null)
            {
                continue;
            }
            var rank = row.ValueOrDefault("rank", 0);
            if (rank > maxRank)
            {
                maxRank = rank;
            }
            var kp = kpTable.ValueOrDefault<long>(rank.ToString(), 0);
            if (kp > 0)
            {
                row["kp"] = kp;
            }
        }

        // 칩 입상 범위 밖 순위의 KP는 행을 추가해서 표시 (예: 칩 6위까지 + KP 9위까지)
        var extraRanks = new List<int>();
        foreach (var d in kpTable)
        {
            if (int.TryParse(d.Key, out int rank) && rank > maxRank)
            {
                extraRanks.Add(rank);
            }
        }
        extraRanks.Sort();
        foreach (var rank in extraRanks)
        {
            var row = new JObject();
            row["rank"] = rank;
            row["kp"] = kpTable.ValueOrDefault<long>(rank.ToString(), 0);
            rewards.Add(row);
        }
    }




    public override void SetValue(JToken data)
    {
        rewardData = data as JArray;
        SetList();
    }

    private void SetList()
    {
        itemController.dataArray = rewardData;
        itemController.Refresh();
    }

    private void OnEnable()
    {
        StartCoroutine(ReSize());
    }

    private IEnumerator ReSize()
    {
        yield return new WaitForEndOfFrame();

        var rect = (transform as RectTransform);
        rect.sizeDelta = new Vector2(rect.sizeDelta.x, root.viewportSize.y);
    }
}
