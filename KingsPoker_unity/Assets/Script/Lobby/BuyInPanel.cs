using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuyInPanel : MonoBehaviour
{

    [SerializeField] private Text blindText;
    [SerializeField] private Text gameTypeText;
    [SerializeField] private Text balanceText;
    [SerializeField] private Text minText;
    [SerializeField] private Text maxText;
    [SerializeField] private Text amountText;
    [SerializeField] private Slider amountSlider;
    [SerializeField] private InputField amountInput;

    [SerializeField] private Text timeBank2Count; // default = 0
    [SerializeField] private GameObject timeBank2Panel;
    [SerializeField] private Text timeBank2Price;

    [SerializeField] private Button buyinButton;
    
    private long reserveBuyinAmount = 0;
    public bool canBuyin = true;
    public delegate bool CanBuyinDelegate();
    public CanBuyinDelegate canBuyinFunc;
    private IEnumerator coroutine = null;

    private CHIP_TYPE chip_type = CHIP_TYPE.nothing;

    public bool reserved
    {
        get; private set;
    }
    private long gtn;

    private long min;
    private long max;

    public void InitPanel(long gtn)
    {
        RoomStatus roomData = InfoManager.Instance.GetRoom(gtn);
        chip_type = roomData.chip_type;
        long balance = 0;

        switch(chip_type)
        {
            case CHIP_TYPE.zc:
                balance = MyStatus.zc;
                break;

            case CHIP_TYPE.dc:
                balance = MyStatus.dc;
                break;
            case CHIP_TYPE.cc:
                balance = Cafe.instance.GetCafeList(roomData.cafeIdx).cafeMembers[0].cc;
                break;
        }


        long myChip = roomData.userList.Find(d => d.gid == MyStatus.gid).gc;
        long an = roomData.an;
        long sb = roomData.sm;
        long bb = roomData.bg;
        min = roomData.bi;
        max = 0;
        if(roomData.bil <= balance)
        {
            max = roomData.bil;
            bool unlimited = max > 1000000000;
            if (unlimited)
                max = 1000000000;
            else
                max -= myChip;
        }
        else
        {
            max = balance;
        }
        if (min <= myChip)
            gameObject.SetActive(false);
        min -= myChip;
        if (min < 0) min = 0;
        GAME_TYPE type = roomData.game_type;

        switch (type)
        {
            case GAME_TYPE.short_deck:
                {
                    string str_blind = string.Format("{0} ({1})", MoneyToString.Converting(bb), MoneyToString.Converting(bb));
                    blindText.text = str_blind;
                }

                break;
            default:
                {
                    string str_blind = string.Format("{0} ({1})", MoneyToString.Converting(sb), MoneyToString.Converting(bb));
                    blindText.text = $"{MoneyToString.Converting(sb)} / {MoneyToString.Converting(bb)}" + (an > 0 ? "(" + MoneyToString.Converting(an) + ")"  : "");
                }
                
                break;
        }
        

        gameTypeText.text = LocalizeManager.GetLocalString(type.ToString());
        minText.text = MoneyToString.Converting(min);// $"{min:#,##0.##}";
        maxText.text = MoneyToString.Converting(max);// $"{max:#,##0.##}";
        balanceText.text = MoneyToString.Converting(balance);// $"{balance:#,##0.##}";
        amountSlider.minValue = min;
        amountSlider.maxValue = max;
        amountSlider.value = Mathf.Ceil(min + ((max - min) * (int.Parse(PlayerPrefs.GetString("buyinsize","0")) / 100f)));

        timeBank2Panel.gameObject.SetActive(roomData.tb2);
        timeBank2Count.text = "0";

        timeBank2Price.text = roomData.tb2_price.ToString();

        this.gtn = gtn;
    }

    public void SetAmount(long amount)
    {
        amountSlider.value = amount;
    }

    private void OnDestroy()
    {
        if (coroutine != null)
        {
            CustomUpdateCaller.Instance.StopCoroutine(coroutine);
        }
    }

    public void SliderValueCange()
    {
        long amount = amountSlider.value.ToInt64();

        if (amount > max) amount = max;

        amountText.text = MoneyToString.Converting(amount);
        amountInput.text = amount.ToString();
        ValueChanged();
    }

    private void ValueChanged()
    {
        buyinButton.interactable = amountSlider.value.ToInt64() != 0;
    }

    public void OnClickBuyInButton()
    {
        gameObject.SetActive(false);
        long amount = amountSlider.value.ToInt64(); ;
        if (amount > max) amount = max;
        if (amount == 0) return;
        if (coroutine != null)
        {
            CustomUpdateCaller.Instance.StopCoroutine(coroutine);
        }
        reserveBuyinAmount = amount;
        coroutine = TryBuyIn();
        CustomUpdateCaller.Instance.StartCoroutine(coroutine);
    }

    private IEnumerator TryBuyIn()
    {
        reserved = true;
        while (true)
        {
            if(canBuyinFunc != null)
            {
                canBuyin = canBuyinFunc();
            }
            if (canBuyin)
                break;
            yield return null;
        }
        reserved = false;
        var roomData = InfoManager.Instance.GetRoom(gtn);
        RoomUserData userData = roomData.userList.Find(value => value.gid == MyStatus.gid);
        if (userData.gc + reserveBuyinAmount > roomData.bil)
        {
            NormalMessage.instance.AddSimpleMessage(LocalizeManager.GetLocalString("buyin_request_cancel"));
            //바이인 요청이 취소되었습니다.
            yield break;
        }
        long balance = 0;

        switch (roomData.chip_type)
        {
            case CHIP_TYPE.zc:
                balance = MyStatus.zc;
                break;
            case CHIP_TYPE.dc:
                balance = MyStatus.dc;
                break;
            case CHIP_TYPE.cc:
                balance = Cafe.instance.GetCafeList(roomData.cafeIdx).cafeMembers[0].cc;
                break;
        }


        if (reserveBuyinAmount > balance)
        {
            NormalMessage.instance.AddSimpleMessage(LocalizeManager.GetLocalString("short_of_balance_buyin"));
            //카페에 보유하고있는 칩이 부족합니다.
            yield break;
        }


        var p = new Packet(CPProtocol.CP_ROOM_BUYIN);
        p.Add("gtn", gtn);
        p.Add("howmuch", reserveBuyinAmount);
        p.Add("tb2", Int32.Parse(timeBank2Count.text));
        WebSocketManager.defaultCli.Send(p);
    }

    public void OnChangeInput()
    {
        string str = amountInput.text;
        if (string.IsNullOrEmpty(str)) str = "0";
        long money = long.Parse(str);
        if(amountSlider.value != money)
        {
            amountSlider.value = money;
        }
    }

    public void OnClickMaxButton()
    {
        amountSlider.value = amountSlider.maxValue;
    }

    public void OnClickMinButton()
    {
        amountSlider.value = amountSlider.minValue;
    }
    public void OnClickTimeBank2MinusButton()
    {
        int count = Int32.Parse(timeBank2Count.text);
        count--;
        if (count < 0)
        {
            count = 0;
        }
        timeBank2Count.text = count.ToString();
    }

    public void OnClickTimeBank2PlusButton()
    {
        int count = Int32.Parse(timeBank2Count.text);
        count++;
        if (count > 3)
        {
            count = 3;
        }
        timeBank2Count.text = count.ToString();
    }
}
