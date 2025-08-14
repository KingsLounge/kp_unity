using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json.Linq;
using NUnit.Framework;


[System.Serializable]
public class UserStateRow
{
    public Text titleText;
    public Text valueText;
   
    public void SetText(string title, string value)
    {
        titleText.text = title;
        valueText.text = value;
    }
}

public class UserState : MonoBehaviour
{
    [SerializeField]
    private Text userName = null;

    [SerializeField]
    private GameObject graphsParent = null;

    [SerializeField]
    private List<CircleRate> circleRates = new List<CircleRate>();

    [SerializeField]
    private List<UserStateRow> userStateRow = null;

    [SerializeField]
    private Text tagText;
    
    [SerializeField]
    private InputField memoInputField;
    [SerializeField]
    private List<InputField> tagTexts = new List<InputField>();
    [SerializeField]
    private List<Toggle> tagToggles = new List<Toggle>();
    private int tagIndex = 0;
    private string memo = string.Empty;
    [SerializeField]
    private Variation tagVariation;
    private string gid = string.Empty;

    private void Awake()
    {
        MyTagData.AddMyTagDataEvent(GetMyTagData);
        MyTagData.AddTagDataEvent(SetUserTag);
        MyTagData.GetMyTagData();
    }
    private void OnDestroy()
    {
        MyTagData.RemoveMyTagDataEvent(GetMyTagData);
        MyTagData.RemoveTagDataEvent(SetUserTag);
    }

    public void GetMyTagData()
    {
        SetTagUI();
    }

    public void SetTag(int index)
    {
        tagIndex = index;
        SetTagUI();
    }

    private void SetTagUI()
    {
        var tagData = MyTagData.Data;
        if(tagTexts != null)
        {
            for(int i = 0; i < tagTexts.Count; ++i)
            {
                var input = tagTexts[i];
                input.text = tagData.ValueOrDefault((i+1).ToString(), string.Empty);
            }
        }
        if(tagText != null) 
        {
            if(tagIndex == 0)
            {
                tagText.text = "태그 없음";
            }
            else
            {
                var tagString = tagData.ValueOrDefault(tagIndex.ToString(), string.Empty);
                if(string.IsNullOrEmpty(tagString))
                {
                    tagString = "태그를 입력해 주세요";
                }
                tagText.text = tagString;
            }
            tagVariation.SetVariation(tagIndex.ToString());
        }
        if(memoInputField != null)
        {
            memoInputField.text = memo;
        }
        for(int i = 0; i < tagToggles.Count; ++i)
        {
            tagToggles[i].SetIsOnWithoutNotify(tagIndex == i);
        }
    }


    public void SetUserTagData()
    {
        memo = memoInputField.text;
        MyTagData.ChangeTagData(gid, tagIndex, memo);
    }

    public void SetMyTagData()
    {
        JObject data = new JObject();
        if(tagTexts != null)
        {
            for(int i = 0; i < tagTexts.Count; ++i)
            {
                data.Add((i+1).ToString(),tagTexts[i].text);
                Debug.LogError(tagTexts[i].text);
            }
        }
        MyTagData.ChangeTagData(data);
    }

    public void SetGid(string gid)
    {
        this.gid = gid;
        
        
        GetUserTag();
    }

    public void SetUserTag(string gid)
    {
        if(gid == this.gid)
        {
            GetUserTag();
        }
    }
    private async void GetUserTag()
    {
        var tagData = await MyTagData.GetTagData(gid);
        tagIndex = tagData.tag;
        memo = tagData.memo;
        SetTagUI();
    }

    public void SetMemo(string memo)
    {
        this.memo = memo;
        SetTagUI();
    }


    private readonly tendencyType[] types = new tendencyType[] { tendencyType.vpip, tendencyType.pfr, tendencyType.c_bet, tendencyType.bet_3, tendencyType.wmsd, tendencyType.wtsd };


    public enum Type
    {
        Holdem,
        Tnmt,
        Bang
    }

    // Start is called before the first frame update



    public void SetState(Type type, string name, JObject data)
    {
        var info = data.CastOrEmpty<JObject>("info");

        userName.text = name;

        switch (type)
        {
            case Type.Holdem:
                var holdem = info.CastOrEmpty<JObject>("holdem");
                SetHoldemState(holdem);
                break;
            case Type.Tnmt:
                var tnmt = info.CastOrEmpty<JObject>("tournament");
                SetTnmtState(tnmt);
                break;
            case Type.Bang:
                var bang = info.CastOrEmpty<JObject>("bang");
                SetBangState(bang);
                break;
        }
    }

    private void SetHoldemState(JObject data)
    {
        var win = data.ValueOrDefault("win", 0);
        var lose = data.ValueOrDefault("lose", 0);
        var maxWinMoney = data.ValueOrDefault<long>("maxWinMoney", 0);

        var rateStr = string.Format("총 {0}승 {1}패 승률({2}%)", win, lose, (win + lose) > 0 ? Mathf.RoundToInt((float)win / (win + lose) * 100) : 0);
        userStateRow[0]?.SetText("전체 전적", rateStr);
        var chipStr = string.Format("{0}칩", maxWinMoney);
        userStateRow[1]?.SetText("최고 획득 칩", chipStr);

        graphsParent.SetActive(true);

        //rateText.text = string.Format(rateTextFormat, win, lose, (win + lose) > 0 ? Mathf.RoundToInt((float)win / (win + lose) * 100) : 0);
        //chipText.text = maxWinMoney.ToString();

        var tendency = data.CastOrEmpty<JObject>("tendency");

        var vpip = tendency.ValueOrDefault("vpip", 0);
        var pfr = tendency.ValueOrDefault("pfr", 0);
        var c_bet = tendency.ValueOrDefault("c_bet", 0);
        var bet_3 = tendency.ValueOrDefault("bet_3", 0);
        var wtsd = tendency.ValueOrDefault("wtsd", 0);
        var wmsd = tendency.ValueOrDefault("wmsd", 0);
        var total = tendency.ValueOrDefault("total", 0);

        // string[] tends = { "nlh", "tnmt_nlh" };
        //foreach (var ten in tends)
        //{
        //    var t = tendency.CastOrEmpty<JObject>(ten);
        //    vpip += t.ValueOrDefault("vpip", 0);
        //    pfr += t.ValueOrDefault("pfr", 0);
        //    c_bet += t.ValueOrDefault("c_bet", 0);
        //    bet_3 += t.ValueOrDefault("bet_3", 0);
        //    wtsd += t.ValueOrDefault("wtsd", 0);
        //    wmsd += t.ValueOrDefault("wmsd", 0);
        //    total += t.ValueOrDefault("total", 0);
        //}

        for (int i = 0; i < types.Length; ++i)
        {
            CircleRate rate = null;
            if (i < circleRates.Count)
            {
                rate = circleRates[i];
            }
            if (!rate)
            {
                continue;
            }
            switch (types[i])
            {
                case tendencyType.vpip:
                {
                    rate.SetValue(vpip, total, "VPIP");
                }
                break;
                case tendencyType.pfr:
                {
                    rate.SetValue(pfr, total, "PFR");
                }
                break;
                case tendencyType.c_bet:
                {
                    rate.SetValue(c_bet, pfr, "C-BET");
                }
                break;
                case tendencyType.bet_3:
                {
                    rate.SetValue(bet_3, total, "3-BET");
                }
                break;

                case tendencyType.wtsd:
                {
                    rate.SetValue(wtsd, total, "WTSD");
                }
                break;
                case tendencyType.wmsd:
                {
                    rate.SetValue(wmsd, wtsd, "WMSD");
                }
                break;
            }

        }
    }

    private void SetTnmtState(JObject data)
    {
        var applyCount = data.ValueOrDefault("applyCount", 0);
        var win = data.ValueOrDefault("tnmtWin", 0);
        var moneyIn = data.ValueOrDefault("moneyIn", 0);


        userStateRow[0]?.SetText("토너먼트 참여 횟수", applyCount.ToString());
        var winStr = string.Format("{0}/{1}", win, moneyIn);
        userStateRow[1]?.SetText("우승/입상", winStr);

        graphsParent.SetActive(true);

        var tendency = data.CastOrEmpty<JObject>("tendency");

        var vpip = tendency.ValueOrDefault("vpip", 0);
        var pfr = tendency.ValueOrDefault("pfr", 0);
        var c_bet = tendency.ValueOrDefault("c_bet", 0);
        var bet_3 = tendency.ValueOrDefault("bet_3", 0);
        var wtsd = tendency.ValueOrDefault("wtsd", 0);
        var wmsd = tendency.ValueOrDefault("wmsd", 0);
        var total = tendency.ValueOrDefault("total", 0);

        for (int i = 0; i < types.Length; ++i)
        {
            CircleRate rate = null;
            if (i < circleRates.Count)
            {
                rate = circleRates[i];
            }
            if (!rate)
            {
                continue;
            }
            switch (types[i])
            {
                case tendencyType.vpip:
                {
                    rate.SetValue(vpip, total, "VPIP");
                }
                break;
                case tendencyType.pfr:
                {
                    rate.SetValue(pfr, total, "PFR");
                }
                break;
                case tendencyType.c_bet:
                {
                    rate.SetValue(c_bet, pfr, "C-BET");
                }
                break;
                case tendencyType.bet_3:
                {
                    rate.SetValue(bet_3, total, "3-BET");
                }
                break;

                case tendencyType.wtsd:
                {
                    rate.SetValue(wtsd, total, "WTSD");
                }
                break;
                case tendencyType.wmsd:
                {
                    rate.SetValue(wmsd, wtsd, "WMSD");
                }
                break;
            }

        }
    }


    private void SetBangState(JObject data)
    {
        var applyCount = data.ValueOrDefault("join", 0);
        var bang = data.ValueOrDefault("bang", 0);

        userStateRow[0]?.SetText("참여 횟수", applyCount.ToString());
        userStateRow[1]?.SetText("걸린 횟수", bang.ToString());

        graphsParent.SetActive(false);
    }

    enum tendencyType { vpip, pfr, c_bet, bet_3, wmsd, wtsd, total }
}

