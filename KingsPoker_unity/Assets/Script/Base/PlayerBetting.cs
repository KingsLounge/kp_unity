using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerBetting : MonoBehaviour
{
    public BettingIconData bettingIcon;
    public BettingSoundData bettingSound;
    public Image balloon;
    public Image icon;
    protected RectTransform iconTr;
    public Text chip;
    public float balloonActiveTime = 1f;
    protected Coroutine setBettingIconCoroutine = null;
    public POKER_BETTYPE currentBetType
    {
        get; protected set;
    }
    public int iconNo = 0;
    protected CHIP_VIEW_MODE chipViewMode = CHIP_VIEW_MODE.CHIP;
    protected long chipBaseValue = 1;
    protected Dictionary<Text, long> lastMoneyTextValue = new Dictionary<Text, long>();
    public long curBet = 0;

    private TableManager tm = null;
    protected TableManager Tm
    {
        get
        {
            if (!tm)
            {
                tm = GetComponentInParent<TableManager>();
            }
            return tm;
        }
    }
    void Awake() {
        iconTr = icon.gameObject.GetComponent<RectTransform>();
    }

    public virtual void ForceBack(bool restartGame = false) {
        if((currentBetType != POKER_BETTYPE.die && currentBetType != POKER_BETTYPE.allin)|| restartGame)
        {
            currentBetType = POKER_BETTYPE.none;
            curBet = 0;
            balloon.gameObject.SetActive(false);
        }
    }
 
    public virtual void Betting(POKER_BETTYPE bettype, long chip) {
        if(bettype == POKER_BETTYPE.none)
            return;
        if(bettype == POKER_BETTYPE.ante)
            return;
        curBet = chip;
        lastMoneyTextValue.Set(this.chip, chip);
        this.chip.text = chip == 0 ? "" : GetMoneyString(chip);
        this.balloon.gameObject.SetActive(true);
        if(setBettingIconCoroutine != null)
            StopCoroutine(setBettingIconCoroutine);
        setBettingIconCoroutine = StartCoroutine(SetBettingIcon(bettype,chip));
        var soundType = bettingSound.GetSoundType(bettype);
        if(Tm.activeTable)
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

    public virtual void Clear()
    {

    }

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

    protected virtual IEnumerator SetBettingIcon(POKER_BETTYPE bettype, long chip) {
        currentBetType = bettype;
        Sprite newIcon = bettingIcon.GetIcon(bettype);
        this.icon.sprite = newIcon;
        this.icon.SetNativeSize();//new Vector2(newIcon.texture.width,newIcon.texture.height);
        yield return new WaitForSeconds(balloonActiveTime);
        newIcon = bettingIcon.GetIcon(bettype,false);
        this.icon.sprite = newIcon;
        this.icon.SetNativeSize();
    }
}
