using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public enum BettingMode
{
    Korean,
    International
}

public enum CHIP_VIEW_MODE
{
    CHIP,
    BB
}

public class BettingManager : WebsocketListenBehaviour
{
    // Start is called before the first frame update
    [HideInInspector]
    public long roomNumber;

    [HideInInspector]
    public GAME_TYPE gameType;
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
    public Animator buttonContainerAnim;
    public BettingButtons bettingButtons;
    public ReserveBettingToggles reserveBettingToggles;
    public BettingPopUp bettingPopUp;
    public GameObject betOkButton;
    private bool bettingButtonsActive = false;
    public PlayerManager playerManager;
    public BettingMode bettingMode = BettingMode.International;
    public NumberPad numberPad = null;

    public CHIP_VIEW_MODE chipViewMode = CHIP_VIEW_MODE.CHIP;
    protected Dictionary<Text, long> lastMoneyTextValue = new Dictionary<Text, long>();

    protected override void Awake()
    {
        base.Awake();

        roomNumber = InfoManager.enterRn;
        bettingButtons.bettingManager = this;
    }

    void Start()
    {
        ChangeChipViewMode(chipViewMode);
    }

    public virtual void OnClickBettingButton(string type)
    {
        POKER_BETTYPE bettype = (POKER_BETTYPE)System.Enum.Parse(typeof(POKER_BETTYPE), type);
    }

    // Update is called once per frame
    void Update() { }

    protected override void ReceivePacket(Packet packet)
    {
        base.ReceivePacket(packet);
        int p = packet.p;
        JObject c = packet.c;

        switch ((PCProtocol)p)
        {
            case PCProtocol.PC_ROOM_COMMAND: //PC_ROOM_COMMAND
                if (!RoomNumberCheck(c, roomNumber))
                {
                    break;
                }
                int command = (int)c["command"];
                if (
                    (ROOM_COMMAND)command == ROOM_COMMAND.end
                    || (ROOM_COMMAND)command == ROOM_COMMAND.start
                )
                {
                    GameEnd();
                }
                break;
        }
    }

    protected virtual void GameEnd() { }

    protected virtual void SetBettingButtonContainer(bool active)
    {
        if (bettingButtonsActive == active)
            return;
        bettingButtonsActive = active;
        bettingButtons.gameObject.SetActive(active);
        if (active == false)
        {
            if (bettingPopUp)
            {
                bettingPopUp.gameObject.SetActive(false);
            }
            if (numberPad)
            {
                numberPad.gameObject.SetActive(false);
            }
        }

        if (bettingMode == BettingMode.Korean)
        {
            if (buttonContainerAnim)
            {
                buttonContainerAnim.Play("ButtonContainer" + (active ? "Up" : "Down"));
            }
        }
        else { }
    }

    public virtual void ChangeChipViewMode(CHIP_VIEW_MODE mode)
    {
        if (chipViewMode == mode)
            return;
        chipViewMode = mode;
        playerManager.ChangeChipViewMode(mode);
        bettingButtons.ChangeChipViewMode(mode);
        if (numberPad)
        {
            double value = numberPad.GetValue();
            long bg = InfoManager.Instance.GetRoom(roomNumber).bg;
            if (mode == CHIP_VIEW_MODE.BB)
            {
                value /= bg;
            }
            else
            {
                value *= bg;
            }
            numberPad.SetValue(value);
        }
        foreach (KeyValuePair<Text, long> data in lastMoneyTextValue)
        {
            data.Key.text = GetMoneyString(data.Value);
        }
    }

    public string GetMoneyString(long money)
    {
        if (chipViewMode == CHIP_VIEW_MODE.CHIP)
            return MoneyToString.Converting(money);
        if (chipViewMode == CHIP_VIEW_MODE.BB)
        {
            long bg = InfoManager.Instance.GetRoom(roomNumber).bg;
            return string.Format("{0:#,##0.##}", ((double)money / bg)) + " BB";
        }
        return "";
    }

    protected virtual void SetReserveBettingToggleContainer(bool active)
    {
        Console.SpecialLog(string.Format("ToggleContainer?? {0}", active));
        reserveBettingToggles.toggles.SetActive(active);
    }

    public void OnclickBettingOkButton()
    {
        betOkButton.SetActive(false);
        OnClickBettingButton(bettingPopUp.betType);
    }

    public virtual void BettingStatusSetting(JObject c) { }

    public void OnClickBettingPopUp(string type)
    {
        bettingPopUp.betType = type;
        betOkButton.SetActive(true);
        bettingButtons.raiseButtonsParentObj.SetActive(false);
        bettingButtons.betButtonsParentObj.SetActive(false);
        //bettingButtons.bettingButtons[3].button.gameObject.SetActive(false);
        //bettingButtons.bettingButtons[4].button.gameObject.SetActive(false);
        bettingPopUp.gameObject.SetActive(true);
    }

    public void OnClickCloseBettiongPopUp()
    {
        bettingPopUp.gameObject.SetActive(false);

        if (bettingPopUp.betType == "bet")
        {
            bettingButtons.betButtonsParentObj.SetActive(true);
        }
        else if (bettingPopUp.betType == "raise")
        {
            bettingButtons.raiseButtonsParentObj.SetActive(true);
        }
    }

    protected bool RoomNumberCheck(JObject data, long gtn)
    {
        long n = data.ValueOrDefault<long>("gtn", 0);
        return n == gtn;
    }
}
