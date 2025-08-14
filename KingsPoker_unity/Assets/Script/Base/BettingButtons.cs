using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class BettingButtonData
{
    public POKER_BETTYPE bettype;
    public Button button;
    public PokerChipText priceText;
    public bool disableInteractable = false;
}

[System.Serializable]
public class BetOrRaiseButtonData
{
    public Button button;
    public PokerChipText priceTxt;
    public Text rateText;
    public long price;
}

public class BettingButtons : MonoBehaviour
{
    public List<BettingButtonData> bettingButtons = new List<BettingButtonData>();
    public GameObject[] bettingOkButtons;
    private List<POKER_BETTYPE> disableLists = new List<POKER_BETTYPE>();
    public GameObject raiseButtonsParentObj;
    public GameObject betButtonsParentObj;
    public GameObject otherBettingButtonsParentObj; // numberPad 또는 나중에 만들 slide betting buttons을 담아둔다.

    public List<BetOrRaiseButtonData> betbuttons;
    public List<BetOrRaiseButtonData> raisebuttons;
    public BettingManager bettingManager;
    public Dictionary<Text, long> lastMoneyTextValue = new Dictionary<Text, long>();

    public LocalText devMinimumRaiseText; // for development;

    public virtual void ChangeChipViewMode(CHIP_VIEW_MODE mode)
    {
        foreach (KeyValuePair<Text, long> data in lastMoneyTextValue)
        {
            data.Key.text = bettingManager.GetMoneyString(data.Value);
        }
    }

    public string GetMoneyString(long money)
    {
        if (bettingManager != null)
        {
            return bettingManager.GetMoneyString(money);
        }
        else
        {
            return MoneyToString.Converting(money);
        }
    }

    public void RaiseRateSet(
        long call,
        long bb,
        long basicPot,
        long myChip,
        long myBetChip,
        long myBetTotal,
        long minimumRaise,
        bool potLimit = false
    )
    {
        long max = InfoManager.Instance.GetRoom(bettingManager.roomNumber).mx - myBetTotal;
        long limit = 0;
        string limitBetType = "";
        if (myChip < max)
        {
            limit = myChip;
            limitBetType = "ingame_holdem_allin";
        }
        else
        {
            limit = max;
            limitBetType = "ingame_holdem_max";
        }

        if (limit <= call)
        {
            raiseButtonsParentObj.SetActive(false);
            otherBettingButtonsParentObj.SetActive(false);
            return;
        }
        else
        {
            raiseButtonsParentObj.SetActive(true);
            otherBettingButtonsParentObj.SetActive(true);
        }
        int[] bettingRate = InfoManager.bettingRate;
        for (int i = 0; i < raisebuttons.Count; i++)
        {
            var data = raisebuttons[i];
            // var rate = bettingRate[i];
            // long price = (long)(System.Math.Ceiling(pot * rate) + call);

            long basicCall = call + myBetChip;
            long myPotSize = basicPot - myBetChip;
            Debug.Log(
                string.Format(
                    "myPotSize {0} = basicPot {1}- myBetChip {2}",
                    myPotSize,
                    basicPot,
                    myBetChip
                )
            );

            long rate = bettingRate[i];
            long raise = (basicCall + (myPotSize * rate) / 100);
            Debug.Log(
                string.Format(
                    "raise {0}= basicCall {1} + (myPotSize {2} * rate {3})",
                    raise,
                    basicCall,
                    myPotSize,
                    rate
                )
            );
            long price = raise - myBetChip; // 서버에 보내야 하는 실제 레이즈 값.
            Debug.Log(
                string.Format("price {0} = raise{1} -  myBetChip {2}", price, raise, myBetChip)
            );
            if (rate == 0)
            {
                price = limit; //
            }

            if (price + myBetChip < minimumRaise)
            {
                price = minimumRaise - myBetChip;
                rate = (minimumRaise - basicCall) * 100 / myPotSize;
            }
            data.price = price;

            if (price >= limit)
            {
                data.price = limit;
                //data.priceTxt.text = GetMoneyString(data.price + myBetChip);
                if (data.priceTxt)
                {
                    data.priceTxt.SetChip(data.price + myBetChip);
                }
                data.rateText.text = LocalizeManager.GetLocalString(limitBetType);
            }
            else
            {
                //data.priceTxt.text = GetMoneyString(data.price + myBetChip);
                if (data.priceTxt)
                {
                    data.priceTxt.SetChip(data.price + myBetChip);
                }
                data.rateText.text = $"{rate:0}%";
            }

            if (potLimit)
            {
                // pot = 240   앞사람이 150 레이즈 나는 20을 postBlind bet 상태

                // basicCall = call + myBetChip;   150  = 130 + 20
                // myPotSize = basicPot - myBetChip;    370 = 390 - 20
                // raise = basicCall + (myPotSize * 100% )    150 + 370 * 100% = 520
                // raise = basicCall + (myPotSize *  75% )    150 + 370 *  75% = 427
                // raise = basicCall + (myPotSize *  50% )    150 + 370 *  50% = 335

                //data.price = 500, myPotSize = 370, call = 130, myBetChip = 20

                //(data.price <= myPotSize + call) = (500 <= 370 + 130) = true

                Debug.Log(
                    string.Format(
                        "data.price:{0} myBetChip:${1} basicPot:{2} + call:{3} minimumRaise: {4}",
                        data.price,
                        myBetChip,
                        basicPot,
                        call,
                        minimumRaise
                    )
                );

                if (
                    (data.price + myBetChip <= basicPot + call)
                    && // pot 보다는 작거나 같고
                    (
                        data.price + myBetChip >= minimumRaise
                        || // numberPadMin 보다 크거나,
                        data.price >= limit
                    )
                ) // 올인 이거나.
                {
                    data.button.enabled = true;
                    data.priceTxt.color = new Color(1.0f, 0.9f, 0.4f);
                }
                else //
                {
                    data.button.enabled = false;
                    data.priceTxt.color = new Color(0.37f, 0.31f, 0f);
                }
            }
            else
            {
                if (data.price + myBetChip >= minimumRaise || data.price >= limit) // numberPadMin 보다 크거나,  올인 이거나.
                {
                    data.button.enabled = true;
                    if (data.priceTxt)
                    {
                        data.priceTxt.color = new Color(1.0f, 0.9f, 0.4f);
                    }
                }
                else //
                {
                    data.button.enabled = false;
                    if (data.priceTxt)
                    {
                        data.priceTxt.color = new Color(0.37f, 0.31f, 0f);
                    }
                }
            }

            devMinimumRaiseText.SetLocalText("min_raise", minimumRaise);
        }
    }

    public void RaiseBBSet(
        long call,
        long bb,
        long basicPot,
        long myChip,
        long myBetChip,
        long myBetTotal,
        long minimumRaise,
        bool potLimit = false
    )
    {
        long max = InfoManager.Instance.GetRoom(bettingManager.roomNumber).mx - myBetTotal;
        long limit = 0;
        string limitBetType = "";
        if (myChip < max)
        {
            limit = myChip;
            limitBetType = "ingame_holdem_allin";
        }
        else
        {
            limit = max;
            limitBetType = "ingame_holdem_max";
        }

        if (limit <= call)
        {
            raiseButtonsParentObj.SetActive(false);
            otherBettingButtonsParentObj.SetActive(false);
            return;
        }
        else
        {
            raiseButtonsParentObj.SetActive(true);
            otherBettingButtonsParentObj.SetActive(true);
        }
        float[] bettingBB = InfoManager.bettingBB;
        //long limit = potLimit ? (basicPot + call > myChip ? myChip : basicPot + call) : myChip; //팟 제한이 걸려있으면 제한금액은 팟과 내 칩중 적은금액, 팟 제한이 없으면 제한금액은 내 칩
        for (int i = 0; i < raisebuttons.Count; i++)
        {
            var data = raisebuttons[i];
            float rate = bettingBB[i];
            long price = (long)(bb * rate) - myBetChip;
            //var rate = InfoManager.bettingRate[i];

            long myPotSize = basicPot - myBetChip;
            Debug.Log(
                string.Format(
                    "myPotSize {0} = basicPot {1}- myBetChip {2}",
                    myPotSize,
                    basicPot,
                    myBetChip
                )
            );

            devMinimumRaiseText.SetLocalText("min_raise", minimumRaise); // = "min raise: " + minimumRaise;

            if (price > 0)
            {
                if (price >= limit)
                {
                    data.price = limit;
                    //data.priceTxt.text = GetMoneyString(data.price + myBetChip);
                    if (data.priceTxt)
                    {
                        data.priceTxt.SetChip(data.price + myBetChip);
                    }

                    data.rateText.text = LocalizeManager.GetLocalString(limitBetType);
                }
                else
                {
                    data.price = price;
                    //data.priceTxt.text = GetMoneyString(data.price + myBetChip);
                    if (data.priceTxt)
                    {
                        data.priceTxt.SetChip(data.price + myBetChip);
                    }
                    data.rateText.text = $"{bettingBB[i]}BB";
                }
            }
            else
            {
                //data.price = basicPot + call - myBetChip;
                //data.priceTxt.text = GetMoneyString(data.price + myBetChip);
                //data.rateText.text = $"pot";
                //if (data.price > limit)
                {
                    data.price = limit;
                    //data.priceTxt.text = GetMoneyString(data.price + myBetChip);
                    if (data.priceTxt)
                    {
                        data.priceTxt.SetChip(data.price + myBetChip);
                    }
                    data.rateText.text = LocalizeManager.GetLocalString(limitBetType);
                }
            }

            if (potLimit)
            {
                // pot = 240   앞사람이 150 레이즈 나는 20을 postBlind bet 상태

                // basicCall = call + myBetChip;   150  = 130 + 20
                // myPotSize = basicPot - myBetChip;    370 = 390 - 20
                // raise = basicCall + (myPotSize * 100% )    150 + 370 * 100% = 520
                // raise = basicCall + (myPotSize *  75% )    150 + 370 *  75% = 427
                // raise = basicCall + (myPotSize *  50% )    150 + 370 *  50% = 335

                //data.price = 500, myPotSize = 370, call = 130, myBetChip = 20

                //(data.price <= myPotSize + call) = (500 <= 370 + 130) = true
                if (
                    (data.price + myBetChip <= basicPot + call)
                    && // pot 보다는 작거나 같고
                    (
                        data.price + myBetChip >= minimumRaise
                        || // numberPadMin 보다 크거나,
                        data.price >= myChip
                    )
                ) // 올인 이거나.
                {
                    data.button.enabled = true;
                    if (data.priceTxt)
                    {
                        data.priceTxt.color = new Color(1.0f, 0.9f, 0.4f);
                    }
                }
                else //
                {
                    data.button.enabled = false;
                    if (data.priceTxt)
                    {
                        data.priceTxt.color = new Color(0.37f, 0.31f, 0f);
                    }
                }
            }
            else
            {
                if (data.price + myBetChip >= minimumRaise || data.price >= myChip) // numberPadMin 보다 크거나,  올인 이거나.
                {
                    data.button.enabled = true;
                    if (data.priceTxt)
                    {
                        data.priceTxt.color = new Color(1.0f, 0.9f, 0.4f);
                    }
                }
                else //
                {
                    data.button.enabled = false;
                    if (data.priceTxt)
                    {
                        data.priceTxt.color = new Color(0.37f, 0.31f, 0f);
                    }
                }
            }
        }
    }

    public void BetRateSet(
        long call,
        long bb,
        long basicPot,
        long myChip,
        long myBetChip,
        long myBetTotal,
        long minimumRaise,
        bool potLimit = false
    )
    {
        devMinimumRaiseText.SetLocalText("");

        long max = InfoManager.Instance.GetRoom(bettingManager.roomNumber).mx - myBetTotal;
        long limit = 0;
        string limitBetType = "";
        if (myChip < max)
        {
            limit = myChip;
            limitBetType = "ingame_holdem_allin";
        }
        else
        {
            limit = max;
            limitBetType = "ingame_holdem_max";
        }

        if (limit <= call)
        {
            betButtonsParentObj.SetActive(false);
            otherBettingButtonsParentObj.SetActive(false);
            return;
        }
        else
        {
            betButtonsParentObj.SetActive(true);
            otherBettingButtonsParentObj.SetActive(true);
        }
        int[] bettingRate = InfoManager.bettingRate;
        for (int i = 0; i < betbuttons.Count; i++)
        {
            //             var rate = bettingRate[i];
            //             long price = (call + System.Math.Ceiling(pot * rate)).ToInt64();
            //long rate = (int)(bettingRate[i] * 100);
            //long price = (call + (long)System.Math.Round((double)((pot * rate) / 100)));

            var data = betbuttons[i];

            long basicCall = call + myBetChip;
            long myPotSize = basicPot - myBetChip;
            Debug.Log(
                string.Format(
                    "myPotSize {0} = basicPot {1}- myBetChip {2}",
                    myPotSize,
                    basicPot,
                    myBetChip
                )
            );

            long rate = (bettingRate[i]);
            long raise = (basicCall + (myPotSize * rate) / 100); // made 되어야 하는 레이즈값.
            Debug.Log(
                string.Format(
                    "raise {0}= basicCall {1} + (myPotSize {2} * rate {3})",
                    raise,
                    basicCall,
                    myPotSize,
                    rate
                )
            );
            long price = raise - myBetChip; // 서버에 보내야 하는 실제 레이즈 값.
            Debug.Log(
                string.Format("price {0} = raise{1} -  myBetChip {2}", price, raise, myBetChip)
            );
            if (rate == 0)
            {
                price = limit; //
            }

            // long price = (long)(System.Math.Ceiling((pot - bb) * rate) + bb); // YR 2021-04-27

            if (price >= limit)
            {
                data.price = limit;
                //data.priceTxt.text = GetMoneyString(data.price + myBetChip);
                if (data.priceTxt)
                {
                    data.priceTxt.SetChip(data.price + myBetChip);
                }
                data.rateText.text = LocalizeManager.GetLocalString(limitBetType);
            }
            else
            {
                if (price + myBetChip < minimumRaise)
                {
                    price = minimumRaise - myBetChip;
                    rate = minimumRaise * 100 / myPotSize;
                }
                data.price = price;
                //data.priceTxt.text = GetMoneyString(data.price + myBetChip);
                if (data.priceTxt)
                {
                    data.priceTxt.SetChip(data.price + myBetChip);
                }
                data.rateText.text = $"{rate:0}%";
            }

            if (potLimit)
            {
                // pot = 240   앞사람이 150 레이즈 나는 20을 postBlind bet 상태

                // basicCall = call + myBetChip;   150  = 130 + 20
                // myPotSize = basicPot - myBetChip;    370 = 390 - 20
                // raise = basicCall + (myPotSize * 100% )    150 + 370 * 100% = 520
                // raise = basicCall + (myPotSize *  75% )    150 + 370 *  75% = 427
                // raise = basicCall + (myPotSize *  50% )    150 + 370 *  50% = 335

                //data.price = 500, myPotSize = 370, call = 130, myBetChip = 20

                //(data.price <= myPotSize + call) = (500 <= 370 + 130) = true
                if (
                    (data.price + myBetChip <= basicPot + call)
                    && // pot 보다는 작거나 같고
                    (
                        data.price + myBetChip >= minimumRaise
                        || // numberPadMin 보다 크거나,
                        data.price >= limit
                    )
                ) // 올인 이거나.
                {
                    data.button.enabled = true;
                    if (data.priceTxt)
                    {
                        data.priceTxt.color = new Color(1.0f, 0.9f, 0.4f);
                    }
                }
                else //
                {
                    data.button.enabled = false;
                    if (data.priceTxt)
                    {
                        data.priceTxt.color = new Color(0.37f, 0.31f, 0f);
                    }
                }
            }
            else
            {
                if (data.price + myBetChip >= minimumRaise || data.price >= limit) // numberPadMin 보다 크거나,  올인 이거나.
                {
                    data.button.enabled = true;
                    if (data.priceTxt)
                    {
                        data.priceTxt.color = new Color(1.0f, 0.9f, 0.4f);
                    }
                }
                else //
                {
                    data.button.enabled = false;
                    if (data.priceTxt)
                    {
                        data.priceTxt.color = new Color(0.37f, 0.31f, 0f);
                    }
                }
            }
        }
    }

    public void SetActiveBettingButton(int activeData)
    {
        for (int i = 0; i < bettingOkButtons.Length; i++)
        {
            bettingOkButtons[i].SetActive(false);
        }
        raiseButtonsParentObj.SetActive(false);
        betButtonsParentObj.SetActive(false);
        otherBettingButtonsParentObj.SetActive(false);

        //if (!Constant.GameConfig.FOLD_WIDTH_CHECK && (activeData & (int)POKER_BETTYPE.check) > 0)
        //{
        //    activeData = activeData & (~(int)POKER_BETTYPE.die);

        //}


        foreach (int value in (int[])System.Enum.GetValues(typeof(POKER_BETTYPE)))
        {
            if ((value & activeData) == value)
            {
                BettingButtonData data = bettingButtons.Find(
                    delegate(BettingButtonData d)
                    {
                        return d.bettype == (POKER_BETTYPE)value;
                    }
                );
                if (data != null)
                {
                    bool active = disableLists.IndexOf((POKER_BETTYPE)value) == -1;
                    if (data.disableInteractable)
                    {
                        data.button.interactable = active;
                        if (!active)
                        {
                            data.priceText.transform.parent.gameObject.SetActive(false);
                        }
                    }
                    else
                    {
                        data.button.gameObject.SetActive(active);
                    }

                    if (value == (int)POKER_BETTYPE.bet || value == (int)POKER_BETTYPE.raise)
                    {
                        //TODO Bet Raise 예외 처리
                    }
                }
            }
            else
            {
                BettingButtonData data = bettingButtons.Find(
                    delegate(BettingButtonData d)
                    {
                        return d.bettype == (POKER_BETTYPE)value;
                    }
                );
                if (data != null)
                {
                    if (data.disableInteractable)
                    {
                        data.button.interactable = false;
                        data.priceText.transform.parent.gameObject.SetActive(false);
                    }
                    else
                    {
                        data.button.gameObject.SetActive(false);
                    }
                }
            }
        }

        if (!Constant.GameConfig.FOLD_WIDTH_CHECK)
        {
            BettingButtonData data = bettingButtons.Find(
                delegate(BettingButtonData d)
                {
                    return d.bettype == (POKER_BETTYPE.die);
                }
            );
            data.button.interactable = (activeData & (int)POKER_BETTYPE.check) == 0;
        }
    }

    public void SetBettingButtonPriceText(POKER_BETTYPE bettype, long chip)
    {
        BettingButtonData d = bettingButtons.Find(
            delegate(BettingButtonData data)
            {
                return data.bettype == bettype;
            }
        );
        if (d != null)
        {
            if (d.priceText != null)
            {
                //Debug.Log(string.Format("{0} : {1}", bettype, chip));
                if (bettingManager != null)
                {
                    //lastMoneyTextValue.Set(d.priceText, chip);
                    //d.priceText.text = bettingManager.GetMoneyString(chip);

                    d.priceText.SetChip(chip);
                }
                else
                {
                    //d.priceText.text = MoneyToString.Converting(chip);
                    d.priceText.SetChip(chip);
                }
                d.priceText.transform.parent.gameObject.SetActive(chip > 0);
            }
        }
    }

    public void SetDisableBettingButton(List<POKER_BETTYPE> bettypes)
    {
        disableLists.Clear();
        for (int i = 0; i < bettypes.Count; i++)
        {
            POKER_BETTYPE bettype = bettypes[i];
            BettingButtonData d = bettingButtons.Find(
                delegate(BettingButtonData data)
                {
                    return data.bettype == bettype;
                }
            );
            if (d != null)
            {
                if (d.disableInteractable)
                {
                    d.button.interactable = false;
                    d.priceText.transform.parent.gameObject.SetActive(false);
                }
                else
                {
                    d.button.gameObject.SetActive(false);
                }
            }
            disableLists.Add(bettype);
        }
    }
}
