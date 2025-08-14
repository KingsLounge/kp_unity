using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.UI;

public class RankListItem : MonoBehaviour
{
    [SerializeField]
    private Text nickText;

    [SerializeField]
    private Text rankText;

    [SerializeField]
    private Text chipText;

    [SerializeField]
    private Text agentText;

    public async void SetRankItem(JObject data)
    {
        var rank = data.ValueOrDefault("rank", 0);
        var name = data.ValueOrDefault("name", "");
        var agentCode = data.ValueOrDefault("agentCode", 0);
        var chip = data.ValueOrDefault("chip", 0);
        if (chip < 0)
            chip = 0;
        rankText.text = rank.ToString();
        nickText.text = name;
        chipText.text = MoneyToString.Converting(chip);
        if (agentText)
        {
            var agentData = await KingshillInfo.GetAgentData();
            var agentStrings = agentData
                .ValueOrDefault(agentCode.ToString(), string.Empty)
                .Split(' ');

            agentText.text = agentStrings[agentStrings.Length - 1];
        }
    }
}
