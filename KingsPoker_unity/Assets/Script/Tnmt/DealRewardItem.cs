using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Newtonsoft.Json.Linq;

using UnityEngine;
using UnityEngine.UI;

public class DealRewardItem : MonoBehaviour
{
    public Text nickText;
    public Text rewardText;

    public Toggle agreeToggle;

    public async void SetDealRewardItem(JObject player)
    {
       
        string nick = player["nick"].ToString();
        bool agree = player["agree"].ToObject<bool>();
        int stack = player["stack"].ToObject<int>();
        JObject r = player["reward"] as JObject;

        string rewardstring = null;
        if(r != null)
        {
            long reward = r.ValueOrDefault<long>("reward", 0);
            int ticket_type = r.ValueOrDefault<int>("ticket_type", 0);
            int ticket_count = r.ValueOrDefault<int>("ticket_count", 0);

            if(reward > 0)
            {
                rewardstring = MoneyToString.Converting(reward);
            }

            
            if(ticket_count > 0)
            {
                string ticketstring = await KingshillInfo.GetTicketString(ticket_type);
                if(string.IsNullOrEmpty(rewardstring))
                {
                    rewardstring = ticketstring + " " + ticket_count;
                }
                else
                {
                    rewardstring += " + " + ticketstring + " " + ticket_count;
                }
            }
        }
        if(string.IsNullOrEmpty(rewardstring))
        {
            rewardstring = "0";
        }
        nickText.text = nick;
        rewardText.text = rewardstring;
        agreeToggle.isOn = agree;
        gameObject.SetActive(true);
    }
}

