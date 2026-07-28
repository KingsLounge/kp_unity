using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json.Linq;

public class RewardItem : MonoBehaviour
{
    
    [SerializeField]
    private Text rankText;

    [SerializeField]
    private Text rewardText;


    public async void SetRewardItem(JObject data)
    {
        var rank = data.ValueOrDefault("rank", 0);
        var ticket_type = data.ValueOrDefault("ticket_type", 0);
        var ticket_count = data.ValueOrDefault("ticket_count", 0);
        var name = data.ValueOrDefault("name", "");
        var chip = data.ValueOrDefault("chip", 0);
        var kp = data.ValueOrDefault<long>("kp", 0);
        rankText.text = rank.ToString();
        string rewardString = string.Empty;
        if (ticket_type > 0 && ticket_count > 0)
        {
            var ticket_string = await KingshillInfo.GetTicketString(ticket_type);
            rewardString += $"{ticket_string} : {ticket_count}";
        }
        if (chip > 0)
        {
            if(rewardString.Length > 0)
            {
                rewardString += ", ";
            }
            rewardString += $"{LocalizeManager.GetLocalString("chip")} : {MoneyToString.Converting(chip)}";
        }
        if (kp > 0)
        {
            if(rewardString.Length > 0)
            {
                rewardString += ", ";
            }
            rewardString += $"KP : {MoneyToString.Converting(kp)}";
        }

        rewardText.text = rewardString;
    }
}
