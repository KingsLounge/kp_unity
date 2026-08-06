using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;
using UnityEngine.UI;
using System.Threading.Tasks;

public class TnmtResultPopup : MonoBehaviour
{
    [SerializeField]
    private List<Variation> resultVariations;

    [SerializeField]
    private GameObject rewardObj;
    
    
    [SerializeField]
    private LocalText rewardText;

    [SerializeField]
    private LocalText rankText;


    [SerializeField]
    private GameObject rewardTicketObj;
    [SerializeField]
    private LocalText rewardTicketText;
    // Update is called once per frame

    [SerializeField]
    private GameObject rewardKpObj; // KP 보상 표시 (미연결 시 무시)
    [SerializeField]
    private LocalText rewardKpText; // get_tnmt_kp 키 사용 (다른 보상 줄과 동일 컨벤션)

    [SerializeField]
    private GameObject rankInObj;

    public async Task SetTnmtResult(JObject data)
    {
        var tn = data.ValueOrDefault("tn", 0);
        var gtn = data.ValueOrDefault("gtn", 0);
        var reward = data.ValueOrDefault("reward", 0);
        var ticket = data.ValueOrDefault("ticket", 0);
        var ticket_amount = data.ValueOrDefault("ticket_amount", 0);
        var kp = data.ValueOrDefault("kp", 0);
        var rank = data.ValueOrDefault("rank", 0);
        var rankVariation = "rank_in";
        var tnmtData = InfoManager.Instance.GetTournamentInfo(tn);
        var t_rewards = tnmtData.info.CastOrEmpty<JObject>("t_rewards");
        var quit_player_count = 0;
        if(t_rewards != null)
        {
            quit_player_count = t_rewards.ValueOrDefault("quit_player_count", 0);
        }

        if (reward == 0 && ticket_amount == 0 && kp == 0 && quit_player_count < rank)
        {
            rankVariation = "rank_out";
        }
        foreach(var variation in resultVariations)
        {
            variation.SetVariation(rankVariation);
        }

        rankText.SetLocalText($"{rankVariation}_text",rank.ToString());

        rewardObj.SetActive(reward > 0);
        rewardText.SetLocalText("reward_chip", MoneyToString.Converting(reward));

        rewardTicketObj.SetActive(ticket_amount > 0);
        string ticketString = await KingshillInfo.GetTicketString(ticket);
        rewardTicketText.SetLocalText($"획득 티켓 : {ticketString} {ticket_amount}개");

        if (rewardKpObj != null)
        {
            rewardKpObj.SetActive(kp > 0);
            if (rewardKpText != null)
            {
                rewardKpText.SetLocalText("get_tnmt_kp", MoneyToString.Converting(kp));
            }
        }

        rankInObj.SetActive(quit_player_count > 1 && rank <= quit_player_count);
        gameObject.SetActive(true);
    }
}
