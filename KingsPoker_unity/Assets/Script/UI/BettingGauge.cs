using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.UI;

public enum BettingGaugeType
{
    Slider,
    Toggle
}

public class BettingGauge : MonoBehaviour
{
    public PlayerManager playerManager;
    public BettingPopUp bettingPopUp;

    public BettingGaugeToggle[] toggles;
    public List<GameObject> chips;
    public Slider slider;

    public PokerChipText betMoenyText;

    public float percent;

    public int currentIndex = 29;

    public BettingGaugeType bettingGaugeType = BettingGaugeType.Slider;
    public HoldemBettingManager bettingManager;

    [SerializeField]
    private GameObject betParentObj;

    [SerializeField]
    private GameObject raiseParentObj;

    public long defaultBet = 0;
    public long defaultRaise = 0;
    public long allIn;

    private void OnEnable()
    {
        Console.Log("Gauge Enablede : " + bettingPopUp.betType);
        SetGauge(bettingPopUp.betType);
        var status = InfoManager.Instance.GetRoom(bettingManager.roomNumber);
        long bet_chip;
        betParentObj.SetActive(false);
        raiseParentObj.SetActive(false);
        if (bettingPopUp.betType == "bet")
        {
            bet_chip = (long)Mathf.Clamp(status.bg, defaultBet, allIn);
            var v = (long)((bet_chip - defaultBet) * (slider.maxValue - 2) / (allIn - defaultBet));

            slider.value = 2;
            betParentObj.SetActive(true);
        }
        else if (bettingPopUp.betType == "raise")
        {
            bet_chip = (long)Mathf.Clamp(bettingManager.raise_chip, defaultRaise, allIn);

            var v = (long)(
                (bet_chip - defaultRaise) * (slider.maxValue - 2) / (allIn - defaultRaise)
            );
            slider.value = 2;
            raiseParentObj.SetActive(true);
        }
    }

    public void SetGauge(string type)
    {
        var status = InfoManager.Instance.GetRoom(bettingManager.roomNumber);
        bettingPopUp.betType = type;
        allIn = playerManager.myPlayer.Chip;
        if (type == "bet")
        {
            defaultBet = status.bg < allIn ? status.bg : allIn;
            SetBetMontyText(bettingManager.bet_chip);
        }
        else if (type == "raise")
        {
            var minimum = bettingManager.minimumRaise - bettingManager.myBetChip;
            defaultRaise = minimum < allIn ? minimum : allIn;
            bettingManager.raise_chip = defaultRaise;
            SetBetMontyText(defaultRaise + bettingManager.myBetChip);
        }
    }

    private void SetBetMontyText(long chip)
    {
        betMoenyText.SetChip(chip);
    }

    public void OnBettingGaugeChanged(int index)
    {
        if (bettingGaugeType == BettingGaugeType.Toggle)
        {
            currentIndex = index;

            for (int i = index - 1; i >= 0; i--)
            {
                toggles[i].OnBettingGaugeChanged(false, false);
            }

            for (int i = index + 1; i < toggles.Length; i++)
            {
                toggles[i].OnBettingGaugeChanged(false, true);
            }
        }
        else if (bettingGaugeType == BettingGaugeType.Slider)
        {
            if (slider.value < 2)
            {
                slider.value = 2;
            }
            var value = (int)slider.value - 2;
            if (bettingPopUp.betType == "bet")
            {
                if (slider.value == slider.maxValue)
                {
                    bettingManager.bet_chip = (long)allIn;
                }
                else
                {
                    bettingManager.bet_chip =
                        defaultBet
                        + (long)((allIn - defaultBet) * (value / (slider.maxValue - 2))) / 10 * 10; //+ RoomStatus.bg * value;
                }
                SetBetMontyText(bettingManager.bet_chip);
            }
            else if (bettingPopUp.betType == "raise")
            {
                if (slider.value == slider.maxValue)
                {
                    bettingManager.raise_chip = (long)allIn;
                }
                else
                {
                    bettingManager.raise_chip =
                        defaultRaise
                        + (long)((allIn - defaultRaise) * (value / (slider.maxValue - 2)))
                            / 10
                            * 10;
                }
                Console.Log(
                    $"{bettingManager.raise_chip}  {defaultRaise}  {value}  {bettingManager.myBetChip}"
                );
                SetBetMontyText(bettingManager.myBetChip + bettingManager.raise_chip);
            }

            // SetChip((int)slider.value);
        }
    }

    public void OnClickBettingButton(int index)
    {
        long bet_chip = 0;

        if (bettingPopUp.betType == "bet")
        {
            bet_chip = bettingManager.bettingButtons.betbuttons[index].price;
        }
        else if (bettingPopUp.betType == "raise")
        {
            bet_chip = bettingManager.bettingButtons.raisebuttons[index].price;
        }

        var v = (long)((bet_chip - defaultBet) * (slider.maxValue - 2) / (allIn - defaultBet)) + 2;
        slider.SetValueWithoutNotify(v);
        if (bettingPopUp.betType == "bet")
        {
            bettingManager.bet_chip = bet_chip;
            SetBetMontyText(bettingManager.bet_chip);
        }
        else if (bettingPopUp.betType == "raise")
        {
            bettingManager.raise_chip = bet_chip;
            SetBetMontyText(bettingManager.raise_chip + bettingManager.myBetChip);
        }
    }

    public void OnToggelChange(int type)
    {
        long bet_chip = 0;
        allIn = playerManager.myPlayer.Chip;
        switch ((BETTING_TOGGLE_TYPE)type)
        {
            case BETTING_TOGGLE_TYPE.dob:
                bet_chip = bettingManager.pot * 2;
                break;
            case BETTING_TOGGLE_TYPE.tri:
                bet_chip = bettingManager.pot * 3;
                break;
            case BETTING_TOGGLE_TYPE.four:
                bet_chip = bettingManager.pot * 4;
                break;
            case BETTING_TOGGLE_TYPE.fif:
                bet_chip = bettingManager.pot * 5;
                break;
            case BETTING_TOGGLE_TYPE.har:
                bet_chip = playerManager.myPlayer.Chip / 2;
                break;
            case BETTING_TOGGLE_TYPE.one_t:
                bet_chip = playerManager.myPlayer.Chip / 3;
                break;
            case BETTING_TOGGLE_TYPE.quarter:
                bet_chip = playerManager.myPlayer.Chip / 4;
                break;
            case BETTING_TOGGLE_TYPE.one_fif:
                bet_chip = playerManager.myPlayer.Chip / 5;
                break;
            case BETTING_TOGGLE_TYPE.all:
                bet_chip = playerManager.myPlayer.Chip;
                break;
        }

        bet_chip = (long)Mathf.Clamp(bet_chip, defaultBet, allIn);
        var v = (long)((bet_chip - defaultBet) * (slider.maxValue - 2) / (allIn - defaultBet)) + 2;
        slider.value = v;
        if (bettingPopUp.betType == "bet")
        {
            bettingManager.bet_chip = bet_chip;
            SetBetMontyText(bettingManager.bet_chip);
        }
        else if (bettingPopUp.betType == "raise")
        {
            bettingManager.raise_chip = bet_chip;
            SetBetMontyText(bettingManager.raise_chip);
        }
    }

    public void AddGauge()
    {
        if (bettingGaugeType == BettingGaugeType.Toggle)
        {
            if (currentIndex > 0)
            {
                toggles[currentIndex - 1].toggle.isOn = true;
            }
        }
        else if (bettingGaugeType == BettingGaugeType.Slider)
        {
            var status = InfoManager.Instance.GetRoom(bettingManager.roomNumber);
            if (slider.value < slider.maxValue)
            {
                long bet_chip;
                if (bettingPopUp.betType == "bet")
                {
                    bet_chip = (long)
                        Mathf.Clamp(bettingManager.bet_chip + status.bg, defaultBet, allIn);

                    var v = (long)(
                        (bet_chip - defaultBet) * (slider.maxValue - 2) / (allIn - defaultBet)
                    );
                    slider.value = v + 2;
                    bettingManager.bet_chip = bet_chip;
                    SetBetMontyText(bettingManager.bet_chip);
                }
                else if (bettingPopUp.betType == "raise")
                {
                    bet_chip = (long)
                        Mathf.Clamp(bettingManager.raise_chip + status.bg, defaultRaise, allIn);

                    var v = (long)(
                        (bet_chip - defaultBet) * (slider.maxValue - 2) / (allIn - defaultBet)
                    );
                    slider.value = v + 2;
                    bettingManager.raise_chip = bet_chip;
                    SetBetMontyText(bettingManager.raise_chip + bettingManager.myBetChip);
                }
            }
        }
    }

    public void SetChip(int count)
    {
        for (int i = 0; i < chips.Count; i++)
        {
            chips[i].SetActive(!(count <= i));
        }
    }

    public void SubGauge()
    {
        if (bettingGaugeType == BettingGaugeType.Toggle)
        {
            if (currentIndex < 29)
            {
                toggles[currentIndex + 1].toggle.isOn = false;
            }
        }
        else if (bettingGaugeType == BettingGaugeType.Slider)
        {
            if (slider.value >= 2) //slider.minValue)
            {
                var status = InfoManager.Instance.GetRoom(bettingManager.roomNumber);
                //slider.value--;
                long bet_chip;
                if (bettingPopUp.betType == "bet")
                {
                    bet_chip = (long)
                        Mathf.Clamp(bettingManager.bet_chip - status.bg, defaultBet, allIn);

                    var v = (long)(
                        (bet_chip - defaultBet) * (slider.maxValue - 2) / (allIn - defaultBet)
                    );
                    slider.value = v + 2;
                    bettingManager.bet_chip = bet_chip;
                    SetBetMontyText(bettingManager.bet_chip);
                }
                else if (bettingPopUp.betType == "raise")
                {
                    bet_chip = (long)
                        Mathf.Clamp(bettingManager.raise_chip - status.bg, defaultRaise, allIn);

                    var v = (long)(
                        (bet_chip - defaultBet) * (slider.maxValue - 2) / (allIn - defaultBet)
                    );
                    slider.value = v + 2;
                    bettingManager.raise_chip = bet_chip;
                    SetBetMontyText(bettingManager.raise_chip);
                }
                // if (bettingPopUp.betType == "bet")
                // {
                //     bettingManager.bet_chip -= RoomStatus.bg;
                //     SetBetMontyText(bettingManager.bet_chip.ToString());
                // }
                // else if (bettingPopUp.betType == "raise")
                // {
                //     bettingManager.raise_chip -= RoomStatus.bg;
                //     SetBetMontyText(bettingManager.raise_chip.ToString());
                // }
            }
        }
    }
}

public enum BETTING_TOGGLE_TYPE
{
    dob = 1,
    tri = 2,
    four = 3,
    fif = 4,
    har = 5,
    one_t = 6,
    quarter = 7,
    one_fif = 8,
    all = 9
}
