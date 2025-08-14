using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuestSlot : MonoBehaviour
{

    public Image questimage;
    public Text questTitleText;
    public Text questText;
    public GameObject rewardChipObj;
    public Text rewardChipText;
    public GameObject rewardTiketObj;
    public Text rewardTicketText;
    public GameObject rewardGoldObj;
    public Text rewardGoldText;
    public Button rewardButton;
    public Text descriptionText; 
    private int questId;
    private int quetType;

    // Start is called before the first frame update
    public void SetQuest(int id, int type, string variable, int opcode, int current , int goal, bool isRicived, long rewardChip, long rewardGold,int rewardTicket, int kind, string description, Sprite sp = null)
    {
        questId = id;
        quetType = type;
        string localPre="";
        switch(kind)
        {
            case 1:
                localPre = "quest_day";
                break;
            case 2:
                localPre = "quest_weekly";
                break;
            case 3:
                localPre = "quest_monthly";
                break;
        }
        questTitleText.text = LocalizeManager.GetLocalString(string.Format("{0}_{1}", localPre, variable));
        questText.text = string.Format("(<color=#78C724>{0}</color>/{1})", current, goal);
        
        bool success = false;;
        switch(opcode)
        {
            case 1:
                success = current>=goal;
                break;
            case 2:
                success = current == goal;
                break;
            case 3:
                success = current <= goal;
                break;
        }
        if(sp != null)
        {
            questimage.sprite = sp;
        }
        rewardChipObj.SetActive(rewardChip != 0);
        rewardTiketObj.SetActive(rewardTicket != 0);
        rewardGoldObj.SetActive(rewardGold != 0);
        rewardChipText.text = string.Format("{0}",MoneyToString.Converting(rewardChip));
        rewardTicketText.text = string.Format("{0}", rewardTicket);
        rewardGoldText.text = string.Format("{0}", MoneyToString.Converting(rewardGold));
        rewardButton.gameObject.SetActive(success && !isRicived);
        if(!string.IsNullOrEmpty(description) && descriptionText)
        {
            descriptionText.text = LocalizeManager.GetLocalString(description);
        }
        gameObject.SetActive(true);
    }

    public void OnClick()
    {
        var p = new Packet((int)CPProtocol.CP_QUEST_CLEAR);
        p.Add("questid",questId);
        WebSocketManager.defaultCli.Send(p.ToJson());
        rewardButton.gameObject.SetActive(false);
    }
}
