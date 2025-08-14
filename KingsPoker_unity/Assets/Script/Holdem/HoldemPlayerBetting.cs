using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HoldemPlayerBetting : PlayerBetting
{
    public List<ColorVariation> bettypeColorVariations = new List<ColorVariation>();
    public LocalText balloonLeftText = null;
    public LocalText balloonCenterText = null;
    public ChipVer2 chipObj;

    void Awake()
    {
        iconTr = icon.gameObject.GetComponent<RectTransform>();
    }

    public override void ForceBack(bool restartGame = false)
    {
        base.ForceBack(restartGame);
        if (
            (currentBetType != POKER_BETTYPE.die && currentBetType != POKER_BETTYPE.allin)
            || restartGame
        )
        {
            SetBettypeColorVariations(null);
        }
    }

    public override void Clear()
    {
        curBet = 0;
        chipObj.SetChip(0);
    }

    private void SetBettypeColorVariations(string key)
    {
        bettypeColorVariations.ForEach(value => value.SetVariation(key));
    }

    public override void Betting(POKER_BETTYPE bettype, long chip)
    {
        if (bettype == POKER_BETTYPE.none)
            return;

        if (bettype == POKER_BETTYPE.bing || bettype == POKER_BETTYPE.ante)
            return;

        curBet = chip;
        SetBettypeColorVariations(bettype.ToString());
        if (bettype != POKER_BETTYPE.die)
            chipObj.SetChip(chip);
        this.balloon.gameObject.SetActive(true);

        // SB BB

        if (bettype == POKER_BETTYPE.big || bettype == POKER_BETTYPE.small)
        {
            this.balloon.gameObject.GetComponent<Image>().enabled = false;
        }
        else
        {
            this.balloon.gameObject.GetComponent<Image>().enabled = true;
        }

        if (setBettingIconCoroutine != null)
            StopCoroutine(setBettingIconCoroutine);
        setBettingIconCoroutine = StartCoroutine(SetBettingIcon(bettype, chip));
        var soundType = bettingSound.GetSoundType(bettype);
        if (Tm.activeTable)
        {
            SoundManager.Instance.PlayCharacterVoice(soundType, iconNo);
            switch (bettype)
            {
                case POKER_BETTYPE.allin:
                    SoundManager.Instance.PlayEffectSound(Sound_TableEnum.SFX_ALLIN);
                    break;
                case POKER_BETTYPE.check:
                    SoundManager.Instance.PlayEffectSound(Sound_TableEnum.SFX_CHECK);
                    break;
                case POKER_BETTYPE.die:
                    SoundManager.Instance.PlayEffectSound(Sound_TableEnum.SFX_FOLD);
                    break;
            }
        }
    }

    public void MoveChip(Vector3 pos, float time)
    {
        if (chipObj.gameObject.activeSelf)
        {
            chipObj.MoveChip(pos, time);
        }
    }

    protected override IEnumerator SetBettingIcon(POKER_BETTYPE bettype, long chip)
    {
        currentBetType = bettype;
        POKER_BETTYPE[] dontShowBetAmountBets = new POKER_BETTYPE[]
        {
            POKER_BETTYPE.check,
            POKER_BETTYPE.die
        };
        bool center = System.Array.IndexOf(dontShowBetAmountBets, bettype) != -1 || chip == 0;
        string bettypeStr = bettype == POKER_BETTYPE.die ? "fold" : bettype.ToString();
        balloonLeftText.LocalKey = "ingame_holdem_" + bettypeStr;
        balloonCenterText.LocalKey = "ingame_holdem_" + bettypeStr;

        if (bettype == POKER_BETTYPE.big || bettype == POKER_BETTYPE.small)
        {
            balloonLeftText.LocalKey = "";
            balloonCenterText.LocalKey = "";
        }
        else { }
        balloonCenterText.gameObject.SetActive(center);
        balloonLeftText.gameObject.SetActive(!center);
        SetBettypeColorVariations(bettype.ToString() + "_on");
        yield return new WaitForSeconds(balloonActiveTime);
        SetBettypeColorVariations(bettype.ToString() + "_off");
    }
}
