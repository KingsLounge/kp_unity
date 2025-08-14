using System;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MyInfoBasicInfomation : MonoBehaviour
{
    [Header("UI", order = 0)]

    public Text totalCoinText;
    public Text totalCoinLimitText;

    public Text holdingCoinText;
    public Text holdingCoinLimitText;
    public Text safeCoinText;
    public Text safeCoinLimitText;

    public Text totalChipText;
    public Text totalChipLimitText;

    public Text holdingChipText;
    public Text holdingChipLimitText;
    public Text safeChipText;
    public Text safeChipLimitText;

    public Text insuranceText;

    public Text lossCoinText;
    public Text lossCoinLimitText;

    public Text coinFreeChargeText;
    public Text chipFreeChargeText;
    public Text membersText;
    public Text membersDayText;

    [Header("Popup", order = 3)]
    public LimitChangeHistoryWindow limitChangeHistroyWindow;
    public GameObject limitChangeResult;
    public GameObject limitChangeFail;
    public GameObject limitChangeWindow;


    [Header("Data")]
    [SerializeField]
    private LobbyManager lobbyManager;
    private long dailyLose;
    private long dailyLossLimit;
    private JArray history;

    
    public void Init()
    {
        limitChangeHistroyWindow.gameObject.SetActive(false);
        limitChangeResult.SetActive(false);
        limitChangeFail.SetActive(false);
        if(limitChangeWindow)
        {
            limitChangeWindow.SetActive(false);
        }
        LimitDataSet(MyStatus.limitData);
        SetUI();
    }

    public void SetUI()
    {

        if (totalCoinText) totalCoinText.text = MoneyToString.Converting(MyStatus.zc + MyStatus.safeSc);
        //if (totalCoinLimitText) totalCoinLimitText.text = "(한도 " + GetLimit("silver", "_max") + ")";

        if (holdingCoinText)        holdingCoinText.text = getShowValue("silver");;
        //if (holdingCoinLimitText)   holdingCoinLimitText.text = "(한도 " + GetLimit("silver", "_user_show_max") + ")";
        if (safeCoinText)           safeCoinText.text = MoneyToString.Converting(MyStatus.safeSc);
        //if (safeCoinLimitText)      safeCoinLimitText.text = "(한도 " + GetLimit("silver", "_safe_max") + ")";


        if (totalChipText) totalChipText.text = MoneyToString.Converting(MyStatus.dc + MyStatus.safeGc);
        //if (totalChipLimitText) totalChipLimitText.text = "(한도 " + GetLimit("gold", "_max") + ")";

        if (holdingChipText)        holdingChipText.text = getShowValue("gold");
        //if (holdingChipLimitText)   holdingChipLimitText.text = "(한도 " + GetLimit("gold", "_user_show_max") + ")";
        if (safeChipText)           safeChipText.text = MoneyToString.Converting(MyStatus.safeGc);
        //if (safeChipLimitText)      safeChipLimitText.text = "(한도 " + GetLimit("gold", "_safe_max") + ")";


        //if (insuranceText)        insuranceText.text = MyStatus.insurance;
        if (lossCoinText)           lossCoinText.text = MoneyToString.Converting(-dailyLose);
        if (lossCoinLimitText)      lossCoinLimitText.text = string.Format("-{0}", MoneyToString.Converting(dailyLossLimit));
        //if (coinFreeChargeText)     coinFreeChargeText.text;
        //if (chipFreeChargeText)     chipFreeChargeText.text;
        //if (membersText)            membersText.text = (MyStatus.VipRate == 0 ? "-" : LocalizeManager.GetLocalString("members_" + MyStatus.vipString));

        //if (membersDayText)
        //{
        //    if(MyStatus.VipRate > 0)
        //    {
        //        TimeSpan span = MyStatus.vipExpire - DateTime.UtcNow;
        //        membersDayText.text = TimeSpanToText(span);                                      // ItemSlot.TimeSpanToText(TimeSpan span)  참고 했음.  2021-12-21 Alberto
        //    }
        //    else
        //    {
        //        membersDayText.text = "-"; 
        //    }
        //}
    }

    public string getShowValue(string safeChipType)
    {
        long limit = long.MaxValue;//InfoManager.GetCharacterLimit(safeChipType);
        long money = 0;
        if (safeChipType == "gold")
        {
            money = MyStatus.dc > limit ? limit : MyStatus.dc;

        }
        else if (safeChipType == "silver")
        {
            money = MyStatus.zc > limit ? limit : MyStatus.zc;
        }
        return MoneyToString.Converting(money);
    }

    private string TimeSpanToText(TimeSpan span)
    {
        string timeString;
        if ((long)span.TotalDays > 18250)
        {
            timeString = "무기한";
        }
        else if ((long)span.TotalDays > 0)
        {
            timeString = string.Format("{0}일 {1}시간", span.Days, span.Hours);
        }
        else if ((long)span.TotalHours > 0)
        {
            timeString = string.Format("{0}시간", (int)span.TotalHours);
        }
        else if ((long)span.TotalMinutes > 0)
        {
            timeString = "만료 임박";
        }
        else
        {
            timeString = "만료";
        }

        return timeString;
    }



    public void LimitDataSet(JObject data)
    {
        dailyLose = data.ValueOrDefault<long>("dailyLoss", 0);
        dailyLossLimit = data.ValueOrDefault<long>("dailyLossLimit", 0);
        history = data.ValueOrDefault<JArray>("dailyLossLimitUpdateDate", null);
    }

    public void OnClickLossLimitChangeButton()
    {
        limitChangeHistroyWindow.Init(history);
        limitChangeHistroyWindow.gameObject.SetActive(true);
    }
    public void OnClickDropOut()
    {
        NormalMessage.instance.OnMessagePopup("회원 탈퇴","탈퇴 시 7일간의 유예 기간이 주어지며, 기간 중 탈퇴 철회가 가능합니다.\n\n7일이 경과할 경우 모든 게임 데이터는 삭제되어 복구가 되지 않습니다.\n\n정말 탈퇴하시겠습니까?", DropOut);
    }
    public void DropOut()
    {
        PublisherApiManager.Instance.RequestDropout(DropOutCallBack);
    }

    public void DropOutCallBack(bool success, Newtonsoft.Json.Linq.JObject jobj)
    {
        if (success)
        {
            LogOut();
        }
    }


    public void LogOut()
    {
        WebSocketManager.defaultCli.OnExitOnce += (reson) =>
        {
            DevManager.Instance.GsLogin = false;
            DevManager.Instance.WsDelegate -= 1;
            DevManager.Instance.WsConnect = false;
            FirebaseManager.Instance.SignOut();
            CustomSceneManager.LoadLoginScene();
        };
        WebSocketManager.defaultCli.Close();
    }
    //public string GetLimit(string ct, string option)
    //{
    //    JObject memberShip = InfoManager.GameConfig[MyStatus.VipRate.ToString()] as JObject;
    //    long n = memberShip.ValueOrDefault<long>(ct + option, 0);
    //    return MoneyToString.Converting(n);
    //}
}
