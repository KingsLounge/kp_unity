using System;
using System.Collections;
using System.Collections.Generic;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    public Image characterProfileImag;
    public SpriteAtlas characterAtlas;
    public Text nickText = null;
    public PokerChipText chipText = null;
    public Transform gaugeAngelTr;
    protected Dictionary<Text, long> lastMoneyTextValue = new Dictionary<Text, long>();

    [ReadOnly]
    public string gid = "";
    protected string nick = "";
    protected string profileUrl = "";
    protected long _chip = 0;
    public long Chip
    {
        get { return _chip; }
        set
        {
            _chip = value;
            chipText.SetChip(this._chip);
        }
    }
    public Timer gaugeBar = null;
    public Image gaugeBarImg;
    private Sprite gaugeBarSprite;
    public Sprite timebankGaugeBarSprite;

    [ReadOnly]
    public int seat = -1;
    public GameObject leaveMark;
    public PlayerBetting betting;
    public PlayerRate rate;
    public PlayerCard playerCard;
    public PlayerResult playerResult;
    protected bool _myTurn = false;
    public bool myTurn
    {
        get { return _myTurn; }
    }
    public GameObject bossMark;
    public GameObject profileImageObj;
    public Text handRanking;
    public ROOM_USER_STATUS status;
    protected string[] cardStrings = new string[2];
    public Image emoImage;
    protected float fill = 1f;
    protected float fillMin = 0f;
    protected int iconNo = 1;
    public bool isDie = false;
    private bool isDeal = false;
    public bool IsDeal
    {
        get { return isDeal; }
        set { 
            isDeal = value; 
            if(isDealObj)
                isDealObj.SetActive(isDeal);
        }
    }   
    [SerializeField]
    private GameObject isDealObj;

    [SerializeField]
    private Variation tagVariation;
    

    [SerializeField]
    private GameObject straddleMark;

    [SerializeField]
    private float hurryTiming = 0.3f;

    [SerializeField]
    GameObject myTurnEffectObj = null;
    protected CHIP_VIEW_MODE chipViewMode = CHIP_VIEW_MODE.CHIP;
    protected long chipBaseValue = 1;

    private int coffee_break = 0;

    [SerializeField]
    private GameObject coffeObj = null;
    public int Coffee_break
    {
        get { return coffee_break; }
        set { SetCoffeBreak(value); }
    }

    public bool straddle { get; private set; }

    // Start is called before the first frame update
    public virtual void Awake()
    {
        gaugeBarSprite = gaugeBarImg.sprite;
        MyTagData.AddTagDataEvent(TagChange);
    }
    public virtual void OnDestroy()
    {
        MyTagData.RemoveTagDataEvent(TagChange);
    }

    [SerializeField]
    private Transform targetGraphic;

    private Graphic[] m_graphics;
    protected Graphic[] Graphics
    {
        get
        {
            if (m_graphics == null) //캐싱이 되지 않았다면
            {
                if (targetGraphic)
                {
                    m_graphics = targetGraphic.GetComponentsInChildren<Graphic>(true);
                }
                else
                {
                    m_graphics = transform.GetComponentsInChildren<Graphic>(true);
                }
            }

            return m_graphics;
        }
    }

    private void SetCoffeBreak(int coffee)
    {
        coffee_break = coffee;
        if (coffeObj)
        {
            coffeObj.SetActive(coffee_break > 0);
        }
    }

    private void CoffeBreakEnd() { }

    public void OnClick()
    {
        var tableManager = transform.GetComponentInParent<HoldemTableManager>();
        tableManager?.GetUserData(gid, nick);
    }

    private void ColorTween(Color targetColor)
    {
        for (int i = 0; i < Graphics.Length; ++i)
            Graphics[i].CrossFadeColor(targetColor, 0f, true, true);
    }

    // Update is called once per frame
    protected virtual void Update() { }

    public void ActiveStraddleMark(bool active)
    {
        straddle = active;
        if (straddleMark)
        {
            straddleMark.SetActive(active);
        }
    }

    public virtual void SetInfo(string nick, int icon_no, long gc)
    {
        this.nick = nick;
        this.Chip = gc;
        this.iconNo = icon_no;
        betting.iconNo = icon_no;
    }

    public void CardHighlight(string[] list, int type)
    {
        playerCard.CardHighlight(list, type);
    }

    public void CardHighlightOff(bool onlyHighlight = false)
    {
        playerCard.CardHighlightOff(onlyHighlight);
    }

    public bool CompareGid(string gid)
    {
        return this.gid == gid;
    }

    public void PlayEmo(Sprite sp, float time)
    {
        CancelInvoke("EndEmo");
        emoImage.sprite = sp;
        emoImage.SetNativeSize();
        emoImage.gameObject.SetActive(true);
        Invoke("EndEmo", time);
    }

    public void EndEmo()
    {
        emoImage.gameObject.SetActive(false);
    }

    public bool CompareSeat(int seat)
    {
        return this.seat == seat;
    }

    public virtual void Clear()
    {
        this.gid = "";
        this.nick = "";
        this.Chip = 0;
        isDie = false;
        nickText.text = "";
        chipText.SetChip(0);
        seat = -1;
        Coffee_break = 0;
        EndEmo();
        ForceBack();
        SetReservedLeave(false);
        this.gameObject.SetActive(false);
    }

    public void SetBoss(bool boss)
    {
        bossMark.SetActive(boss);
    }

    public virtual void ForceBack()
    {
        isDie = false;
        if (playerCard)
        {
            playerCard.ForceBack();
        }
        if (betting)
        {
            betting.ForceBack(true);
            betting.Clear();
        }
        if (playerResult)
        {
            playerResult.ForceBack();
        }
        ActiveStraddleMark(false);
        NotMyTurn();
        VariationBack();
        SetBoss(false);
    }

    public void VariationBack()
    {
        ColorTween(Color.white);
    }

    public void SetCard(string[] cardArr = null)
    {
        Console.Log(string.Format("{0} status : {1}", nickText.text, status));
        if (status == ROOM_USER_STATUS.ingame)
        {
            playerCard.SetCard(cardArr);
        }
    }

    public void SetCard(string cards)
    {
        SetCard(cards.Split(','));
    }

    public void SetCard(int index, string card)
    {
        playerCard.SetCard(index, card);
    }

    public List<string> GetCard()
    {
        return playerCard.GetCard();
    }

    public List<Card> GetCardComponent()
    {
        return playerCard.cards;
    }

    public long GetCurBet()
    {
        return betting.curBet;
    }

    public void ShowCard()
    {
        if (status == ROOM_USER_STATUS.ingame)
        {
            playerCard.SetCard(cardStrings);
        }
    }

    public virtual void PlayResult(
        string hands,
        long a,
        long w,
        long gc,
        bool win,
        bool split = false
    )
    {
        if (win)
        {
            //glow.gameObject.SetActive(true);
            //glow.AnimationState.SetAnimation(0,"03_glow_yellow",true);
        }
        betting.ForceBack();
        playerResult.PlayResult(hands, a, w, gc, win, split);
        this.Chip = gc;
    }

    public virtual void SetReservedLeave(bool leave)
    {
        leaveMark.SetActive(leave);
    }

    public void OpenCards(string[] cards, bool skipAni = false)
    {
        playerCard.OpenCard(cards, skipAni);
    }

    public void ShowCard(string[] cards)
    {
        playerCard.ShowCard(cards);
    }

    public void Betting(POKER_BETTYPE bettype, long chip, bool myTurnOff = true)
    {
        isDie = bettype == POKER_BETTYPE.die;
        if (myTurnOff)
        {
            NotMyTurn();
        }

        betting.Betting(bettype, chip);
        var variString = bettype.ToString();
        if (isDie)
        {
            ColorTween(Color.gray);
        }
        else
        {
            VariationBack();
        }
    }

    public void Betting(POKER_BETTYPE bettype, long chip, long gc, long total)
    {
        Betting(bettype, chip);
        this.Chip = gc - total;

        if (bettype == POKER_BETTYPE.straddle)
        {
            Debug.Log(this.Chip);
            Debug.Log(gc);
            Debug.Log(total);
        }
    }

    public void Betting(
        POKER_BETTYPE bettype,
        long chip,
        long total,
        RoomUserData playerData,
        bool myTurnOff = true
    )
    {
        Betting(bettype, chip, myTurnOff);
        this.Chip = playerData.gc - total;
    }

    public virtual void ShowRate(float winRate, float tieRate)
    {
        rate.ShowRate(winRate, tieRate);
    }

    public virtual void SetPlayer(
        string gid,
        string nick,
        long chip,
        int seat,
        int status,
        int icon_no,
        string url = null,
        bool useUrl = false
    )
    {
        this.gid = gid;
        this.nick = nick;
        this.Chip = chip;
        this.seat = seat;
        this.status = (ROOM_USER_STATUS)status;
        this.iconNo = icon_no;
        betting.iconNo = icon_no;
        isDie = false;
        nickText.text = nick;
        gameObject.SetActive(true);
        SetProfileUrl(url, useUrl);
        GetTagData();
    }
    public async void GetTagData()
    {
        var tagData = await MyTagData.GetTagData(gid);
        if(tagVariation)
        {
            tagVariation.SetVariation(tagData.tag.ToString());
        }
    }
    private void TagChange(string gid)
    {
        if(gid == this.gid)
        {
            GetTagData();
        }
    }

    public virtual void MyTurn(float time, bool timebankMode = false, Action<float> progress = null)
    {
        //glow.gameObject.SetActive(true);
        //glow.AnimationState.SetAnimation(0,"01_glow_blue",true);
        if (timebankGaugeBarSprite)
        {
            gaugeBarImg.sprite = timebankMode ? timebankGaugeBarSprite : gaugeBarSprite;
        }
        if (myTurnEffectObj)
        {
            myTurnEffectObj.SetActive(true);
        }
        _myTurn = true;
        gaugeBar.Clear();
        gaugeBar.SetTimer(true, time, NotMyTurn, progress);
    }

    public virtual void NotMyTurn()
    {
        gaugeBar.Clear();
        if (myTurnEffectObj)
        {
            myTurnEffectObj.SetActive(false);
        }
        gaugeBar.gameObject.SetActive(false);
        _myTurn = false;
    }

    public async void SetProfileUrl(string url, bool useUrl)
    {
        if (useUrl)
        {
            if (url != profileUrl && !string.IsNullOrEmpty(url))
            {
                SetProfileTexture(
                    await ImageDatabase.LoadImageTexture(
                        url,
                        Application.persistentDataPath + ",profileImg",
                        gid
                    )
                );
            }
            else if (string.IsNullOrEmpty(url))
            {
                profileImageObj.SetActive(false);
            }
        }
        else
        {
            var imageStr = "";
            try
            {
                imageStr = TableDataManager.characterTable[iconNo]["2d_index"].ToObject<string>();
            }
            catch { }

            var sprite = characterAtlas.GetSprite(imageStr);
            if (sprite != null)
            {
                SetProfileTexture(sprite);
            }
            else
            {
                profileImageObj.SetActive(false);
            }
        }
    }

    private void SetProfileTexture(Texture2D texture)
    {
        Rect rect = new Rect(0, 0, texture.width, texture.height);
        Sprite sprite = Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f));
        SetProfileTexture(sprite);
    }

    private void SetProfileTexture(Sprite sprite)
    {
        characterProfileImag.sprite = sprite;
        characterProfileImag.enabled = true;
        profileImageObj.SetActive(true);
    }

    public void ChipViewModeSetting(CHIP_VIEW_MODE mode, long calc)
    {
        chipBaseValue = calc;
        chipViewMode = mode;
        foreach (KeyValuePair<Text, long> data in lastMoneyTextValue)
        {
            data.Key.text = GetMoneyString(data.Value);
        }
        betting.ChipViewModeSetting(chipViewMode, chipBaseValue);
        playerResult.ChipViewModeSetting(chipViewMode, chipBaseValue);
    }

    public string GetMoneyString(long money)
    {
        if (chipViewMode == CHIP_VIEW_MODE.CHIP)
            return MoneyToString.Converting(money);
        if (chipViewMode == CHIP_VIEW_MODE.BB)
            return string.Format("{0:#,##0.##}", (double)money / chipBaseValue) + " BB";
        return "";
    }
}
