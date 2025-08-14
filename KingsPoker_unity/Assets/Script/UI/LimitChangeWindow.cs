using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class LimitChangeWindow : MonoBehaviour
{
    [Header("UI", order = 0)]
    public Text infoText;

    [Header("LimitChangeResultWindow", order = 1)]
    public GameObject LimitChangeResult;
    public List<Text> limitTexts;
    public Text limitChangeResultInfoText;
    
    [Header("LimitChangeFailWindow", order = 2)]
    public GameObject limitChangeFail;
    public Text limitChangeFailTitleText;
    public Text limitChangeFailInfoText;


    int limittype;
    private long[] limits;
    public void OnchangeLimit(int limit)
    {
        limittype = limit;
    }
    public void Init()
    {
        List<string> limitStringList = new List<string>();
        limits = MyStatus.limitData.ValueOrDefault("dailyLossLimits", new long[0]);
        for (int i = 0; i < limits.Length; i++)
        {
            string limitString = MoneyToString.Converting(limits[i]);
            limitStringList.Add(limitString);
            if (i < limitTexts.Count)
            {
                limitTexts[i].text = limitString;
            }

        }
        var limitStrings = string.Join(" ", limitStringList);
        infoText.text = string.Format("<color=yellow>1일 기준 손실 머니</color>를 {0} 중 선택하실 수 있습니다.\n게임을 더 오래 하고 싶으시면 {1}으로 변경해 주세요.", limitStrings, limitStringList[limitStringList.Count - 1]);
    }
    public void OnClickLimitChangeCheckButton()
    {
        long limit = 0;
        limit = limits[limittype];
        if (limit == MyStatus.lossLimit)
        {
            string title = LocalizeManager.GetLocalString("SYS_ERR_TRY_SAME_VALUE");
            string info = LocalizeManager.GetLocalString("SYS_ERR_NOT_CHANGE_SAME_VALUE");
            limitChangeFailTitleText.text = title;
            limitChangeFailInfoText.text = info;
            limitChangeFail.SetActive(true);
            return;
        }
        Packet p = new Packet((int)CPProtocol.CP_USER_DAILY_LOSS_LIMIT_UPDATE);
        p.Add("limit", limit);
        WebSocketManager.defaultCli.Send(p);
    }

    public void SuccessLimit()
    {
       
        var changedLimit = limits[(int)limittype];
        string beforeStr = MoneyToString.Converting(limits[(int)MyStatus.limittype]);
        string afterStr = MoneyToString.Converting(changedLimit);
        MyStatus.lossLimit = changedLimit;
        limitChangeResultInfoText.text = $"({beforeStr} -> {afterStr})";
        MyStatus.limittype = limittype;
        LimitChangeResult.SetActive(true);
        LobbyManager.Instance.GetUserInfo();
    }

    public void FailedLimit()
    {
        string title = LocalizeManager.GetLocalString("SYS_ERR_DAILY_LOSS_LIMIT_CHANGE_COUNT_OVER");
        string info = LocalizeManager.GetLocalString("SYS_ERR_DAILY_LOSS_LIMIT_CHANGE_COUNT_OVER_EXPLAN");
        limitChangeFailTitleText.text = title;
        limitChangeFailInfoText.text = info;
        limitChangeFail.SetActive(true);
        gameObject.SetActive(false);
    }

}
