using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerResult : MonoBehaviour
{
    public Text handsText;
    public Text rewardText;
    public Color winRewardTextColor = new Color(0.88f,0.6f,0.15f);
    public Color loseRewardTextColor = new Color(0.58f,0.05f,0);
    public GameObject winPanel;
    public GameObject losePanel;
    public PlayerStatus playerStatus;
    protected CHIP_VIEW_MODE chipViewMode = CHIP_VIEW_MODE.CHIP;
    protected long chipBaseValue = 1;
    protected Dictionary<Text, long> lastMoneyTextValue = new Dictionary<Text, long>();


    public void ChipViewModeSetting(CHIP_VIEW_MODE mode, long calc)
    {
        chipBaseValue = calc;
        chipViewMode = mode;
        foreach (KeyValuePair<Text, long> data in lastMoneyTextValue)
        {
            data.Key.text = GetMoneyString(data.Value);
        }
    }

    protected string GetMoneyString(long money)
    {
        if (chipViewMode == CHIP_VIEW_MODE.CHIP)
            return MoneyToString.Converting(money);
        if (chipViewMode == CHIP_VIEW_MODE.BB)
            return string.Format("{0:#,##0.##}", (double)money / chipBaseValue) + " BB";
        return "";
    }

    public virtual void PlayResult(string hands,long a,long w, long gc, bool win, bool split = false) {
        if(handsText)
        {
            handsText.text = hands;
        }
        winPanel.SetActive(win);
        losePanel.SetActive(!win);
        if(playerStatus != null)
        {
            playerStatus.PlayResult(false);
        }
        rewardText.text = win ? "+" + GetMoneyString(w) : "-" + GetMoneyString(a);
        rewardText.color = win ? winRewardTextColor : loseRewardTextColor;
        gameObject.SetActive(true);
    }

    public virtual void ForceBack() {
        if (playerStatus != null)
        {
            playerStatus.PlayResult(true);
        }
        gameObject.SetActive(false);
    }
}
