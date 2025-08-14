using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.UI;

public enum HOLDEM_BETTING_ROUND
{
    PRE_FLOP,
    FLOP,
    TURN,
    RIVER
}

public enum HOLDEM_CHIP_BETTING_MODE
{
    NONE,
    THROW
}

public class HoldemBettingManager : BettingManager
{
    private bool prevMyTurn = false;
    private int betstep = 0;
    public long pot;
    public PokerChipText potText;
    public PokerChipText resultPotText;
    public long call_chip = 0;
    public long bet_chip = 0;
    public long raise_chip = 0;
    public long myBetChip = 0;
    public long myBetTotal = 0;
    private long tableCall = 0;
    private long money_minimum_raise = 0; // from PC_HOLDEM_BET_MONEY
    public long minimumRaise = 0;

    [SerializeField]
    private float chipMoveTime = 0.5f;

    public bool isAction = false;
    public bool isFold = true;
    private bool raiseOrBet = false;
    public Button raiseButton;
    public Button betButton;
    public POKER_BETTYPE prevAction = POKER_BETTYPE.none;
    public HOLDEM_BETTING_ROUND bettingRound = HOLDEM_BETTING_ROUND.PRE_FLOP;
    public ChipContainerManager chipContainer;
    private POKER_BETTYPE myBetType;
    public HOLDEM_CHIP_BETTING_MODE chipBettingMode;
    public GameObject reserveButtonLock;
    public float autobetDelayTime = 0.5f;
    public ChipVer2 collectChip = null;
    private ObjectPool2<ChipVer2> collectChipPool = new ObjectPool2<ChipVer2>();
    public GameObject runItTwicePanel;
    public Timer runItTwiceTimer;
    public GameObject insurancePanel;
    public Timer insuranceTimer;
    public Text insuranceButtonOkText;

    public GameObject waitForBlindPanel;

    public Toggle straddleToggle;
    public Button straddleButton;
    public GameObject straddlePanel;
    public Timer straddleTimer;
    public Text straddleTitleText;
    public Text straddleAmountText;
    public Text noLookAllinText;
    public Button straddleBetButton;
    public Button noLookAllinButton;
    private long currentStraddleAmount = 0;
    private POKER_BETTYPE betOrRaise = POKER_BETTYPE.none;
    private long numberPadMin = 0;
    public Button numberPadOKButton = null;
    private List<POKER_BETTYPE> stageBetList = new List<POKER_BETTYPE>();
    public Toggle reserveFoldToggle;

    protected override void Awake()
    {
        if (collectChip)
        {
            collectChip.bettingManager = this;
            collectChipPool.generator = () =>
            {
                ChipVer2 instance = Instantiate(collectChip);
                instance.transform.SetParent(collectChip.transform.parent);
                instance.transform.CleanIdentity();
                return instance;
            };
            collectChipPool.activator = (obj) =>
            {
                obj.gameObject.SetActive(true);
            };
            collectChipPool.deactivator = (obj) =>
            {
                obj.gameObject.SetActive(false);
            };
            collectChipPool.remover = (obj) =>
            {
                Destroy(obj.gameObject);
            };
        }
        base.Awake();
        if (numberPad)
        {
            numberPad.onValueChanged = (double value) =>
            {
                long chip = 0;
                RoomStatus rs = InfoManager.Instance.GetRoom(roomNumber);
                long bg = rs.bg;
                long max = rs.mx;
                if (chipViewMode == CHIP_VIEW_MODE.CHIP)
                    chip = (long)value;
                else if (chipViewMode == CHIP_VIEW_MODE.BB)
                    chip = (long)(value * bg);
                long myChip = playerManager.myPlayer.Chip;
                long allinChip = myChip + myBetChip;
                long limit = allinChip < max ? allinChip : max;

                bool interactable = false;

                if (numberPadMin > limit && chip == limit) //미니멈 레이즈가 내돈보다 커서 올인밖에 못할때 입력한값이 올인인 경우
                    interactable = true;
                else if (chip >= numberPadMin && chip <= limit) //입력한 값이 미니멈 레이즈보다 크고 올인칩보다 작을경우
                    interactable = true;

                numberPadOKButton.interactable = interactable;

                numberPad.slider.SetValueWithoutNotify((float)chip / limit);
                // Debug.Log(string.Format("chip {0} >= numberPadMin {1} && chip {0} <= myChip {2}  + myBetChip {3}", chip, numberPadMin, myChip, myBetChip));
            };

            numberPad.slider.onValueChanged.AddListener(
                (float value) =>
                {
                    long chip = 0;
                    RoomStatus rs = InfoManager.Instance.GetRoom(roomNumber);
                    long bg = rs.bg;
                    long max = rs.mx;

                    long myChip = playerManager.myPlayer.Chip;
                    long allinChip = myChip + myBetChip;
                    value *= allinChip < max ? allinChip : max;

                    if (chipViewMode == CHIP_VIEW_MODE.CHIP)
                        chip = (long)(value);
                    else if (chipViewMode == CHIP_VIEW_MODE.BB)
                        chip = (long)(value * bg);

                    numberPad.SetValue(chip);
                }
            );

            numberPad.clamper = (double value) =>
            {
                RoomStatus rs = InfoManager.Instance.GetRoom(roomNumber);
                double clamped = value;
                var gameType = rs.game_type;
                long myChip = playerManager.myPlayer.Chip;
                long bg = rs.bg;
                long max = rs.mx;
                // long curPot = pot + (call_chip * 2);

                // long curPot = pot + (call_chip + myBetChip) - myBetChip;

                long basic_call = call_chip + myBetChip;
                long basic_pot = pot + basic_call;
                long myPotSize = basic_pot - myBetChip;
                long allInChip = myChip + myBetChip;

                //long limit = gameType != GAME_TYPE.plo ? allInChip :
                //(myChip < curPot ? allInChip : curPot); //plo가 아니면 myChip이 최대, plo면 myChip과 pot중 작은값이 최대

                long limit = 0;
                if (gameType == GAME_TYPE.plo)
                {
                    if (myChip < myPotSize) // myChip = 600    660   myPotSize = 840 - 60 = 780
                    {
                        limit = allInChip; // 660
                    }
                    else
                    {
                        limit = basic_pot + call_chip;
                    }
                }
                else
                {
                    limit = allInChip < max ? allInChip : max;
                }

                if (chipViewMode == CHIP_VIEW_MODE.CHIP && (long)value > limit)
                    clamped = limit;
                else if (chipViewMode == CHIP_VIEW_MODE.BB && (long)(value * bg) > limit)
                    clamped = ((long)(limit / bg * 10) / 10.0);
                return clamped;
            };
        }
        StatusSet();
    }

    protected override void ReceivePacket(Packet packet)
    {
        base.ReceivePacket(packet);
        int p = packet.p;
        JObject c = packet.c;

        switch ((PCProtocol)p)
        {
            case PCProtocol.PC_HOLDEM_USER_MONEY: // PC_HOLDEM_USER_MONEY 배팅 209
                if (!RoomNumberCheck(c, roomNumber))
                {
                    break;
                }

                {
                    int bettypeInt = (int)c["bettype"];
                    POKER_BETTYPE bettype = (POKER_BETTYPE)bettypeInt;
                    stageBetList.Add(bettype);
                    long gc = (long)c["gc"];
                    long total = c["total"].ToObject<long>();
                    long bet = (long)c["bet"];
                    long betThisStage = c["bet_this_stage"].ToObject<long>();
                    int seat = (int)c["seat"];
                    long gtn = c["gtn"].ToObject<long>();

                    HoldemPlayer pl = (HoldemPlayer)playerManager.FindPlayerWithSeat(seat);

                    if (pl != null)
                    {
                        var user = InfoManager
                            .Instance.GetRoom(roomNumber)
                            .userList.Find(d => d.gid == pl.gid);
                        if (user == null)
                        {
                            RoomReenter();
                        }

                        if (pl.CompareGid(MyStatus.gid)) // 배팅한 유저가 나
                        {
                            myBetType = bettype;
                            SetBettingButtonContainer(false);
                            raiseOrBet = false;
                            myBetChip = betThisStage;
                            myBetTotal = total;
                            if (bettingRound == HOLDEM_BETTING_ROUND.PRE_FLOP) // 현재 배팅라운드가 프리플랍
                            {
                                if (bettype == POKER_BETTYPE.small)
                                {
                                    pl.position = HOLDEM_POSITION.SB;
                                }
                                else if (bettype == POKER_BETTYPE.big)
                                {
                                    pl.position = HOLDEM_POSITION.BB;
                                }
                                else
                                {
                                    pl.position = HOLDEM_POSITION.NONE;
                                }
                            }

                            if (bettype == POKER_BETTYPE.die || bettype == POKER_BETTYPE.allin)
                            {
                                isFold = true;
                            }
                            reserveBettingToggles.Init();
                        }
                        else // 배팅한 유저가 상대방
                        {
                            if (bettingRound == HOLDEM_BETTING_ROUND.PRE_FLOP)
                            {
                                if (bettype == POKER_BETTYPE.big) // 상대방이 BB를 냈다면
                                {
                                    pl.position = HOLDEM_POSITION.BB; // 상대방은 BB
                                }
                                else if (bettype == POKER_BETTYPE.small)
                                {
                                    pl.position = HOLDEM_POSITION.SB;
                                }
                                else
                                {
                                    pl.position = HOLDEM_POSITION.NONE;
                                }
                            }

                            if (bettype != POKER_BETTYPE.none)
                            {
                                //SetReserveToggle(bettype, false);
                            }

                            //if (bettype == POKER_BETTYPE.die)
                            //{
                            //    SetReserveBettingToggleContainer(false);
                            //}

                            prevAction = bettype;
                        }

                        pl.Betting(bettype, betThisStage, gc, total);
                        switch (chipBettingMode)
                        {
                            case HOLDEM_CHIP_BETTING_MODE.NONE:
                                break;
                            case HOLDEM_CHIP_BETTING_MODE.THROW:
                                chipContainer.Throw(pl.transform.position, bet);
                                break;
                        }
                    }
                }
                break;
            case PCProtocol.PC_HOLDEM_WHO_IS_TURN: //PC_HOLDEM_WHO_IS_TURN 208
                if (!RoomNumberCheck(c, roomNumber))
                {
                    break;
                }
                int turnSeat = (int)c["seat"];
                WhoseTurn(c);

                break;
            case PCProtocol.PC_HOLDEM_BET_MONEY: //PC_HOLDEM_BET_MONEY 207
                if (!RoomNumberCheck(c, roomNumber))
                {
                    break;
                }
                if (true)
                {
                    RoomStatus status = InfoManager.Instance.GetRoom(roomNumber);
                    long money_total = (long)c["money_total"];
                    long money_call = (long)c["money_call"];
                    tableCall = money_call;
                    money_minimum_raise = (long)c["money_minimum_raise"];

                    long myTotal = 0;
                    pot = money_total;
                    //lastMoneyTextValue.Set(potText, money_total);
                    //potText.text = GetMoneyString(money_total);
                    potText.SetChip(money_total);
                    long myChip = playerManager.myPlayer.Chip;
                    long call = (long)Mathf.Min(money_call - myBetChip, myChip);
                    call_chip = call < 0 ? 0 : call;
                    bet_chip = (long)Mathf.Min(call_chip + status.bg, myChip);
                    raise_chip = (long)Mathf.Min(call_chip + status.bg, myChip);
                    long dadang = (long)Mathf.Min(money_call * 2 - myBetChip, myChip);
                    long half = money_total / 2; //(money_total + call_chip) / 2;
                    long quarter = half / 2;
                    long max = status.mx - myTotal;
                    bettingButtons.SetBettingButtonPriceText(POKER_BETTYPE.ante, status.bg);
                    bettingButtons.SetBettingButtonPriceText(
                        POKER_BETTYPE.call,
                        call_chip <= playerManager.myPlayer.Chip
                            ? call_chip
                            : playerManager.myPlayer.Chip
                    );
                    bettingButtons.SetBettingButtonPriceText(POKER_BETTYPE.dadang, dadang);
                    bettingButtons.SetBettingButtonPriceText(POKER_BETTYPE.half, half);
                    bettingButtons.SetBettingButtonPriceText(POKER_BETTYPE.max, max);
                    bettingButtons.SetBettingButtonPriceText(POKER_BETTYPE.raise, raise_chip);
                    List<POKER_BETTYPE> disables = new List<POKER_BETTYPE>();
                    if (
                        dadang > playerManager.myPlayer.Chip
                        || dadang > status.mx - myTotal
                        || dadang < 0
                    )
                    {
                        disables.Add(POKER_BETTYPE.dadang);
                    }
                    if (
                        half > playerManager.myPlayer.Chip
                        || half > status.mx - myTotal
                        || half < 0
                        || half <= call_chip
                    )
                    {
                        disables.Add(POKER_BETTYPE.half);
                    }
                    if (
                        quarter > playerManager.myPlayer.Chip
                        || quarter > status.mx - myTotal
                        || quarter < 0
                        || quarter <= call_chip
                    )
                    {
                        disables.Add(POKER_BETTYPE.quarter);
                    }
                    if (max <= 0)
                    {
                        disables.Add(POKER_BETTYPE.max);
                    }
                    bettingButtons.SetDisableBettingButton(disables);
                    raiseOrBet = false;
                }
                break;
            case PCProtocol.PC_HOLDEM_GAMERESULT: //PC_HOLDEM_GAME_RESULT
                if (!RoomNumberCheck(c, roomNumber))
                {
                    break;
                }
                if (true)
                {
                    resultPotText.SetChip(potText.GetChip());
                    List<JObject>[] gameResults = JsonDataParser.Parse<List<JObject>[]>(
                        c["list"]["item"]
                    );
                    //List<JObject> result = gameResults[0];

                    // TODO: run_it_twice 인경우 처리

                    if (gameResults.Length > 1)
                    {
                        Console.Log("run_it_twice = 2 ");
                    }
                    else
                    {
                        Console.Log("run_it_twice = 1 ");
                    }

                    StartCoroutine(PlayResult(gameResults));
                }
                break;
            case PCProtocol.PC_HOLDEM_DRAW_CARDS: // PC_HOLDEM_DRAW_CARDS
                if (!RoomNumberCheck(c, roomNumber))
                {
                    break;
                }

                int kind = (int)c["kind"];

                bettingRound = (HOLDEM_BETTING_ROUND)kind;
                isAction = false;
                reserveBettingToggles.alwaysFold = false;
                prevAction = POKER_BETTYPE.none;
                playerManager.OffBettingBalloon();
                tableCall = 0;
                if (kind == 0)
                {
                    isFold = false;
                }
                else if (kind >= 1)
                {
                    reserveBettingToggles.Init();
                    myBetChip = 0;
                }

                break;
            case PCProtocol.PC_HOLDEM_COMMAND:
                if (!RoomNumberCheck(c, roomNumber))
                {
                    break;
                }

                {
                    var flow = (POKER_FLOW)c["command"].ToObject<int>();

                    POKER_FLOW[] end_betstage = new POKER_FLOW[]
                    {
                        POKER_FLOW.betstage_preflop,
                        POKER_FLOW.betstage_flop,
                        POKER_FLOW.betstage_turn,
                        POKER_FLOW.betstage_river
                    };
                    POKER_FLOW[] betstage = new POKER_FLOW[]
                    {
                        POKER_FLOW.show_down,
                        POKER_FLOW.goto_result,
                        POKER_FLOW.open_cards,
                        POKER_FLOW.end_of_game
                    };
                    if (System.Array.IndexOf(end_betstage, flow) != -1)
                    {
                        stageBetList.Clear();
                    }

                    if (
                        System.Array.IndexOf(end_betstage, flow) != -1
                        || System.Array.IndexOf(betstage, flow) != -1
                    )
                    {
                        CollectChip();
                        call_chip = 0;
                    }

                    if (flow == POKER_FLOW.show_down)
                    {
                        runItTwicePanel.SetActive(false);
                        runItTwiceTimer.Clear();
                        insurancePanel.SetActive(false);
                        insuranceTimer.Clear();
                    }

                    if (flow == POKER_FLOW.draw_cards)
                    {
                        straddlePanel.SetActive(false);
                        if (reserveFoldToggle)
                        {
                            reserveFoldToggle.gameObject.SetActive(
                                playerManager.IsPlayingGamer(MyStatus.gid)
                            );
                        }
                    }
                }
                break;
            case PCProtocol.PC_HOLDEM_STATUS: // PC_HOLDEM_STATUS
                if (!RoomNumberCheck(c, roomNumber))
                {
                    break;
                }
                StatusSet();
                Debug.Log("홀덤 배팅 매니저에서 홀덤 상태 체크함");
                break;
            case PCProtocol.PC_RUN_IT_TWICE_START:
                /*
                 * {"p":217,"c":{"gtn":637,"time":10,"seats":[1,2]}}
                 */
                if (!RoomNumberCheck(c, roomNumber) || !IsPlayingGamer(MyStatus.gid))
                {
                    break;
                }

                {
                    Player player = playerManager.FindPlayerWithGid(MyStatus.gid);
                    if (player.isDie)
                        break;

                    JArray seats = c["seats"] as JArray; // 여기 있는 유저만 럿잇트와이스에 참여한다.  Alberto 2021-04-20
                    bool openWindow = false;
                    for (int i = 0; i < seats.Count; i++)
                    {
                        if ((int)seats[i] == player.seat)
                        {
                            openWindow = true;
                            break;
                        }
                    }
                    if (!openWindow) // 런잇트와이스르
                    {
                        break;
                    }

                    runItTwicePanel.SetActive(true);
                    runItTwiceTimer.Clear();
                    runItTwiceTimer.SetTimer(
                        true,
                        (float)c["time"],
                        () =>
                        {
                            runItTwicePanel.SetActive(false);
                        }
                    );
                }

                break;
            case PCProtocol.PC_RUN_IT_TWICE:
                if (!RoomNumberCheck(c, roomNumber))
                {
                    break;
                }

                {
                    int howmany = (int)c["howmany"];
                    if (howmany == 1 && runItTwicePanel.activeSelf)
                    {
                        runItTwicePanel.SetActive(false);
                        runItTwiceTimer.Clear();
                    }
                }
                break;
            case PCProtocol.PC_ROOM_INSURANCE_START:
                /* {"p":134,"c":{"gtn":637,"time":10,"seats":[7,0],"insurance_chips":[320,436]}}*/

                if (!RoomNumberCheck(c, roomNumber) || !IsPlayingGamer(MyStatus.gid))
                {
                    break;
                }

                {
                    Player player = playerManager.FindPlayerWithGid(MyStatus.gid);
                    if (player.isDie)
                        break;

                    int seat_index = 0;
                    JArray seats = c["seats"] as JArray; // 여기 있는 유저만 럿잇트와이스에 참여한다.  Alberto 2021-04-20
                    bool openWindow = false;
                    for (int i = 0; i < seats.Count; i++)
                    {
                        if ((int)seats[i] == player.seat)
                        {
                            openWindow = true;
                            seat_index = i;
                            break;
                        }
                    }
                    if (!openWindow) // 런잇트와이스르
                    {
                        break;
                    }

                    JArray insurance_chips = c["insurance_chips"] as JArray; // 여기 있는 유저만 럿잇트와이스에 참여한다.  Alberto 2021-04-20
                    int insurance_chip = (int)insurance_chips[seat_index];
                    if (insurance_chip <= 1) //
                    {
                        //Packet pkt = new Packet(CPProtocol.CP_ROOM_INSURANCE);
                        //pkt.Add("gtn", roomNumber);
                        //pkt.Add("on", false);
                        //WebSocketManager.defaultCli.Send(pkt);
                        break;
                    }

                    insurancePanel.SetActive(true);
                    string format =
                        "{0}"
                        + System.Environment.NewLine
                        + LocalizeManager.GetLocalString("insurance_ok");

                    insuranceButtonOkText.text = MoneyToString.Converting(insurance_chip);
                    insuranceTimer.Clear();
                    insuranceTimer.SetTimer(
                        true,
                        (float)c["time"],
                        () =>
                        {
                            insurancePanel.SetActive(false);
                        }
                    );
                }

                break;
            case PCProtocol.PC_ROOM_INSURANCE:
                /*
                 */
                if (!RoomNumberCheck(c, roomNumber))
                {
                    break;
                }

                // TODO: '단순메시지'로 보험 신청한것을 알린다.  또는 캐릭터 위체 '보험 마크를 붙힌다.

                if (c["gid"].ToString() == MyStatus.gid)
                {
                    insurancePanel.SetActive(false);
                    insuranceTimer.Clear();
                }
                else
                {
                    bool on = (bool)c.ValueOrDefault("on", false);
                    string nick = (string)c.ValueOrDefault("nick", "");
                    string msg = LocalizeManager.GetLocalString(
                        on ? "take_out_insurance" : "not_take_out_insurance"
                    );

                    NormalMessage.instance.AddSimpleMessage(string.Format("'{0} {1}", nick, msg));
                }

                break;
            case PCProtocol.PC_STRADDLE:
                if (!RoomNumberCheck(c, roomNumber))
                {
                    break;
                }
                straddleToggle.isOn = (bool)c["on"] && c["gid"].ToString() == MyStatus.gid;
                straddleButton.interactable =
                    !((bool)c["on"]) || c["gid"].ToString() == MyStatus.gid;
                break;
            case PCProtocol.PC_STRADDLE_BET_START:
                if (!RoomNumberCheck(c, roomNumber))
                {
                    break;
                }

                {
                    straddleToggle.isOn = false;
                    straddleButton.interactable = true;

                    if (IsPlayingGamer(MyStatus.gid))
                    {
                        Player player = playerManager.FindPlayerWithGid(MyStatus.gid);
                        long myChip = player ? player.Chip : 0;
                        if (straddlePanel)
                        {
                            straddlePanel.SetActive(true);
                            straddleTimer.Clear();
                            straddleTimer.SetTimer(
                                true,
                                (float)c["time"],
                                () =>
                                {
                                    straddlePanel.SetActive(false);
                                }
                            );
                            currentStraddleAmount = (long)c["howmuch"];
                            straddleTitleText.text =
                                "Straddle to " + GetMoneyString(currentStraddleAmount);
                            straddleAmountText.text = GetMoneyString(currentStraddleAmount);
                            straddleBetButton.interactable = currentStraddleAmount < myChip;
                            noLookAllinText.text = GetMoneyString(myChip);
                            noLookAllinButton.interactable = myChip > 0;
                        }
                    }
                }
                break;
            case PCProtocol.PC_ROOM_USER_STATUS:
                if (!RoomNumberCheck(c, roomNumber))
                {
                    break;
                }

                {
                    string gid = (string)c["gid"];
                    var tn = InfoManager.Instance.GetRoom(roomNumber).tn;
                    if (MyStatus.gid == gid && tn == 0)
                    {
                        waitForBlindPanel.SetActive((bool)c["wait_for_blind_popup"]);
                    }
                }
                break;

            case PCProtocol.PC_ROOM_PLAYER_SEAT_LIST:
                /*
                 * {"p":131,"c":{"gtn":637,"list":[2,4,5,0]}}
                 */
                if (!RoomNumberCheck(c, roomNumber))
                {
                    break;
                }

                {
                    JArray seats = c["list"] as JArray;

                    Player myPlayer;
                    for (int i = 0; i < seats.Count; i++)
                    {
                        myPlayer = playerManager.FindMyPlayerWithSeat((int)seats[i]);
                        if (myPlayer && myPlayer.gid == MyStatus.gid)
                        {
                            waitForBlindPanel.SetActive(false);
                            break;
                        }
                    }
                }
                break;
            case PCProtocol.PC_TNMT_DEAL_AGREE_START:
            {
                if (!RoomNumberCheck(c, roomNumber))
                {
                    break;
                }
                GameEnd();
                break;  
            }
                
        }
    }

    private void StatusSet()
    {
        GameEnd();
        var rs = InfoManager.Instance.GetRoom(roomNumber);
        var roomCommand = (ROOM_COMMAND)(int)rs.room_command;
        var isPlaying =
            roomCommand >= ROOM_COMMAND.start && roomCommand <= ROOM_COMMAND.calc_dividend;
        if (reserveFoldToggle)
        {
            reserveFoldToggle.gameObject.SetActive(
                isPlaying && playerManager.IsPlayingGamer(MyStatus.gid)
            );
        }
        if (isPlaying) // 20
        {
            long potMoney = rs.money_total;
            long collectedPodMoney = potMoney;
            money_minimum_raise = rs.money_minimum_raise;
            long myChip = playerManager.myPlayer.Chip;
            RoomStatus status = InfoManager.Instance.GetRoom(roomNumber);

            pot = potMoney;
            //lastMoneyTextValue.Set(potText, potMoney);
            //potText.text = GetMoneyString(potMoney);
            potText.SetChip(potMoney);
            POKER_FLOW flow = (POKER_FLOW)rs.flow;
            int curTurnSeat = rs.who_turn_seat;
            var originPlayers = rs.playerList;

            JArray players = new JArray(
                originPlayers.OrderBy(obj =>
                {
                    string gid = obj["gid"].ToString();
                    var player = playerManager.FindPlayerWithGid(gid);
                    if (player)
                    {
                        int sort = playerManager.FindPlayerWithGid(gid).seat;
                        sort -= curTurnSeat;
                        if (sort < 0)
                            sort += originPlayers.Count;
                        return sort;
                    }
                    return 1000;
                })
            );

            for (int i = players.Count - 1; i >= 0; i--)
            {
                JObject playerJson = players[i] as JObject;
                Player player = playerManager.FindPlayerWithGid(playerJson["gid"].ToString()); //FindPlayerWithSeat(seat);
                long betThisStage = playerJson.ValueOrDefault<long>("money_bet_this_stage", 0);

                if (player != null)
                {
                    var playerData = status.userList.Find(
                        (RoomUserData) =>
                        {
                            return RoomUserData.gid == player.gid;
                        }
                    );
                    //if (player.CompareGid(MyStatus.gid))
                    //{
                    //    SetBettingButtonContainer(false);
                    //    myBettingHistory.Add(c);
                    //}
                    int pokerBetType = (int)playerJson["last_bettype"];
                    stageBetList.Add((POKER_BETTYPE)pokerBetType);

                    int betThisTimeChip = (int)playerJson["money_bet_thistime"];

                    int totlaBet = (int)playerJson["money_total"];
                    long post_straddle = playerJson.ValueOrDefault<long>("post_straddle", 0);
                    player.ActiveStraddleMark(post_straddle != 0);
                    //player.Betting((POKER_BETTYPE)pokerBetType, betThisTimeChip, totlaBet, playerData, false);
                    if (flow == POKER_FLOW.bet || flow == POKER_FLOW.bet_start)
                    {
                        collectedPodMoney -= betThisStage;
                        if (roomCommand == ROOM_COMMAND.calc_dividend)
                        {
                            playerData.gc += totlaBet;
                        }
                        player.Betting(
                            (POKER_BETTYPE)pokerBetType,
                            betThisStage,
                            totlaBet,
                            playerData,
                            false
                        );
                        if (player.gid == MyStatus.gid)
                        {
                            myBetType = (POKER_BETTYPE)pokerBetType;
                            myBetChip = betThisStage;
                            myBetTotal = totlaBet;
                            long call = (long)Mathf.Min(rs.money_call - myBetChip, myChip);
                            call_chip = call < 0 ? 0 : call;
                            bet_chip = call_chip + status.bg;
                            raise_chip = call_chip + status.bg;
                        }

                        //----------------------내턴----------------------
                        try
                        {
                            if (
                                player.gid == MyStatus.gid
                                && player.CompareSeat(rs.who_turn_seat)
                                && player.Coffee_break <= 0
                            )
                            {
                                if (
                                    (pokerBetType & (int)POKER_BETTYPE.allin) == 0
                                    && (pokerBetType & (int)POKER_BETTYPE.die) == 0
                                )
                                {
                                    prevMyTurn = true;
                                    int activeBettype = playerJson["bettype"].ToObject<int>();
                                    long betThisTime = playerJson["money_bet_thistime"]
                                        .ToObject<long>();
                                    bettingButtons.SetActiveBettingButton(activeBettype);
                                    SetReserveBettingToggleContainer(false);
                                    SetBettingButtonContainer(true);
                                    bettingButtons.SetBettingButtonPriceText(
                                        POKER_BETTYPE.ante,
                                        status.bg
                                    );
                                    bettingButtons.SetBettingButtonPriceText(
                                        POKER_BETTYPE.call,
                                        call_chip <= playerManager.myPlayer.Chip
                                            ? call_chip
                                            : playerManager.myPlayer.Chip
                                    );
                                    bettingButtons.SetBettingButtonPriceText(
                                        POKER_BETTYPE.raise,
                                        raise_chip - betThisStage
                                    );
                                    SetBettingButtons(activeBettype);
                                    bettingPopUp.gameObject.SetActive(false);
                                }
                            }
                        }
                        catch
                        {
                            Console.Log("is not enable who_turn_seat");
                        }
                    }
                    else if ((pokerBetType & (int)POKER_BETTYPE.die) != 0)
                    {
                        player.Betting((POKER_BETTYPE)pokerBetType, 0, totlaBet, playerData, false);
                    }
                    else if ((pokerBetType & (int)POKER_BETTYPE.allin) != 0)
                    {
                        player.Betting((POKER_BETTYPE)pokerBetType, 0, totlaBet, playerData, false);
                    }
                }
            }
            collectChip.SetChip(collectedPodMoney);
        }
        else { }
    }

    private void RoomReenter()
    {
        InfoManager.Instance.RoomReenter(roomNumber);
    }

    private bool IsPlayingGamer(string gid)
    {
        List<Player> playingGamers = playerManager.GetPlayingGamer();
        return (playingGamers.FindIndex(player => player.CompareGid(MyStatus.gid)) != -1);
    }

    private IEnumerator PlayResult(List<JObject>[] gameResults)
    {
        for (int i = 0; i < gameResults.Length; i++)
        {
            List<JObject> result = gameResults[i];
            if (result.Count == 0)
                continue;
            List<JObject> winners = result.FindAll(
                delegate(JObject data)
                {
                    bool winner = (bool)data["winner"];
                    return winner;
                }
            );

            string winnerGid = (string)winners[0]["gid"];

            Player player = playerManager.FindPlayerWithGid(winnerGid);
            if (chipBettingMode == HOLDEM_CHIP_BETTING_MODE.THROW)
            {
                ChipContainerManager.Instance.MoveAndClear(player.transform.position);
            }
            long nextPot = 0;
            for (int j = i + 1; j < gameResults.Length; j++)
            {
                List<JObject> nextResult = gameResults[j];
                for (int k = 0; k < nextResult.Count; k++)
                    nextPot += nextResult[k].ValueOrDefault<long>("w", 0);
            }

            yield return new WaitForSeconds(1.5f);

            collectChip.SetChip(nextPot);

            for (int j = 0; j < result.Count; j++)
            {
                long reward = result[j].ValueOrDefault("w", 0);
                if (reward <= 0)
                    continue;
                Player curPlayer = playerManager.FindPlayerWithGid(
                    result[j].ValueOrDefault("gid", "")
                );
                if (curPlayer)
                {
                    ChipVer2 dummyChip = collectChipPool.GetObject();
                    dummyChip.SetChip(reward);
                    dummyChip.MoveChip(
                        curPlayer.transform.position,
                        chipMoveTime,
                        () =>
                        {
                            collectChipPool.ReturnObject(dummyChip);
                        }
                    );
                }
            }

            isAction = false;
            isFold = false;

            playerManager.OffRateBalloon();

            reserveBettingToggles.alwaysFold = false;
            prevAction = POKER_BETTYPE.none;
            reserveBettingToggles.Init();
            HoldemPlayerManager hpm = (HoldemPlayerManager)playerManager;
            hpm.ResetPlayers();
            if (Tm.activeTable)
            {
                SoundManager.Instance.PlayEffectSound(Sound_TableEnum.SFX_WIN_RESULT);
            }

            //myPosition = HOLDEM_POSITION.NONE;
            yield return new WaitForSeconds(2f);
        }
    }

    public void OnChangeChipViewTypeToggle(Toggle bbModeToggle)
    {
        ChangeChipViewMode(bbModeToggle.isOn ? CHIP_VIEW_MODE.BB : CHIP_VIEW_MODE.CHIP);
    }

    public void OnToggleWaitForBlind(bool toggle)
    {
        // waitForBlindPanel.SetActive(false);
        Packet p = new Packet(CPProtocol.CP_ROOM_WAIT_FOR_BLIND);
        p.Add("gtn", roomNumber);
        p.Add("wait_for_blind", toggle);
        WebSocketManager.defaultCli.Send(p);
    }

    public void OnClickRunItTwice(int count)
    {
        runItTwicePanel.SetActive(false);
        runItTwiceTimer.Clear();
        Packet p = new Packet(CPProtocol.CP_RUN_IT_TWICE);
        p.Add("gtn", roomNumber);
        p.Add("howmany", count);
        WebSocketManager.defaultCli.Send(p);
    }

    public void OnClickInsurance(bool ok)
    {
        insurancePanel.SetActive(false);
        insuranceTimer.Clear();

        Packet p = new Packet(CPProtocol.CP_ROOM_INSURANCE);
        p.Add("gtn", roomNumber);
        p.Add("on", ok);
        WebSocketManager.defaultCli.Send(p);
    }

    public void CollectChip()
    {
        List<Player> players = playerManager.GetPlayingGamer();
        foreach (var player in players)
        {
            var hp = player as HoldemPlayer;
            hp.MoveChip(collectChip.transform.position, chipMoveTime);
        }
        Invoke("CollectPotChange", chipMoveTime);
    }

    public void CollectPotChange()
    {
        collectChip.SetChip(pot);
    }

    public void OnChangeStraddleStatus()
    {
        if (!IsPlayingGamer(MyStatus.gid))
            return;
        Packet p = new Packet(CPProtocol.CP_STRADDLE);
        p.Add("gtn", roomNumber);
        p.Add("on", !straddleToggle.isOn);
        WebSocketManager.defaultCli.Send(p);
    }

    public void OnClickNoLookAllin()
    {
        Player player = playerManager.FindPlayerWithGid(MyStatus.gid);
        if (player)
        {
            Packet p = new Packet(CPProtocol.CP_STRADDLE_BET);
            p.Add("gtn", roomNumber);
            p.Add("nolookallin", true);
            p.Add("howmuch", player.Chip);
            WebSocketManager.defaultCli.Send(p);
        }
        straddlePanel.SetActive(false);
    }

    public void OnClickStraddleCancel()
    {
        Player player = playerManager.FindPlayerWithGid(MyStatus.gid);
        if (player)
        {
            Packet p = new Packet(CPProtocol.CP_STRADDLE_BET);
            p.Add("gtn", roomNumber);
            p.Add("nolookallin", false);
            p.Add("howmuch", 0); // 취소
            WebSocketManager.defaultCli.Send(p);
        }
        straddlePanel.SetActive(false);
    }

    public void OnClickStraddleBet()
    {
        Player player = playerManager.FindPlayerWithGid(MyStatus.gid);
        if (player)
        {
            Packet p = new Packet(CPProtocol.CP_STRADDLE_BET);
            p.Add("gtn", roomNumber);
            p.Add("nolookallin", player.Chip == currentStraddleAmount);
            p.Add("howmuch", currentStraddleAmount);
            WebSocketManager.defaultCli.Send(p);
        }
        straddlePanel.SetActive(false);
    }

    public void SetReserveToggle(POKER_BETTYPE bettype, bool thatsMe)
    {
        // if (bettype == POKER_BETTYPE.none)
        // {
        //     // none이 앤티임, 앤티는 예약배팅 뜨면 안됨
        //     return;
        // }
        // HoldemPlayer hmp = (HoldemPlayer)playerManager.myPlayer;

        // if (thatsMe == true)
        // {
        //     Debug.LogError("현재 내 포지션 : " + hmp.position);

        //     if (hmp.position == HOLDEM_POSITION.BB && bettingRound == HOLDEM_BETTING_ROUND.PRE_FLOP) // 내가 BB라면
        //     {
        //         reserveBettingToggles.SetReserveToggles(POKER_RESERVE_TYPE.CHECK, POKER_RESERVE_TYPE.RAISE);
        //         Debug.LogError("BB의 예약 : 체크 레이즈");
        //     }
        //     else if (hmp.position == HOLDEM_POSITION.SB && bettingRound == HOLDEM_BETTING_ROUND.PRE_FLOP) // 내가 SB라면
        //     {
        //         if (hmp.isUTG == true)
        //         {
        //             return;
        //         }

        //         reserveBettingToggles.SetReserveToggles(POKER_RESERVE_TYPE.CALL, POKER_RESERVE_TYPE.RAISE);
        //         Debug.LogError("SB의 예약 : 콜 레이즈");
        //     }
        //     else
        //     {
        //         return;
        //     }
        // }
        // else
        // {
        //     if (bettingRound == HOLDEM_BETTING_ROUND.PRE_FLOP)
        //     {
        //         if (hmp.position != HOLDEM_POSITION.NONE) // SB나 BB는 해당안됨
        //         {
        //             return;
        //         }

        //         if (bettype == POKER_BETTYPE.big) // 프리플랍때 UTG는 예약안함
        //         {
        //             return;
        //         }

        //         if (isAction == false)
        //         {
        //             if (bettype == POKER_BETTYPE.bet || bettype == POKER_BETTYPE.raise) // 앞사람이 벳 레이즈를 했다면
        //             {
        //                 if (reserveBettingToggles.currentReserveType == POKER_RESERVE_TYPE.FOLDCHECK)
        //                 {
        //                     reserveBettingToggles.alwaysFold = true;
        //                 }

        //                 reserveBettingToggles.Init();
        //                 reserveBettingToggles.SetReserveToggles(POKER_RESERVE_TYPE.FOLD, POKER_RESERVE_TYPE.CALL, POKER_RESERVE_TYPE.RAISE);
        //                 Debug.LogError("일반의 예약 : 폴드 콜 레이즈");
        //             }
        //             else
        //             {
        //                 reserveBettingToggles.SetReserveToggles(POKER_RESERVE_TYPE.FOLDCHECK, POKER_RESERVE_TYPE.CHECK, POKER_RESERVE_TYPE.BET);
        //                 Debug.LogError("일반의 예약 : 폴드체크 체크 벳");
        //             }
        //         }
        //         else
        //         {
        //             if (bettype == POKER_BETTYPE.bet || bettype == POKER_BETTYPE.raise) // 앞사람이 벳 레이즈를 했다면
        //             {
        //                 reserveBettingToggles.Init();
        //                 reserveBettingToggles.SetReserveToggles(POKER_RESERVE_TYPE.FOLD, POKER_RESERVE_TYPE.CALL, POKER_RESERVE_TYPE.RAISE);
        //                 Debug.LogError("일반의 예약 : 폴드 콜 레이즈");
        //             }
        //             else
        //             {
        //                 Debug.LogError("이미 베팅함");
        //             }
        //         }

        //     }
        //     else
        //     {
        //         if (isAction == false) // 내가 배팅을 안했다면
        //         {
        //             if (hmp.position == HOLDEM_POSITION.SB) // 플랍, 턴, 리버때 SB는 예약안함
        //             {
        //                 return;
        //             }

        //             if (bettype == POKER_BETTYPE.bet || bettype == POKER_BETTYPE.raise) // 앞사람이 벳 레이즈를 했다면
        //             {
        //                 if (reserveBettingToggles.currentReserveType == POKER_RESERVE_TYPE.FOLDCHECK)
        //                 {
        //                     reserveBettingToggles.alwaysFold = true;
        //                 }

        //                 reserveBettingToggles.Init();
        //                 reserveBettingToggles.SetReserveToggles(POKER_RESERVE_TYPE.FOLD, POKER_RESERVE_TYPE.CALL, POKER_RESERVE_TYPE.RAISE);
        //                 Debug.LogError("일반의 예약 : 폴드 콜 레이즈");
        //             }
        //             else
        //             {
        //                 reserveBettingToggles.SetReserveToggles(POKER_RESERVE_TYPE.FOLDCHECK, POKER_RESERVE_TYPE.CHECK, POKER_RESERVE_TYPE.BET);
        //                 Debug.LogError("일반의 예약 : 폴드체크 체크 벳");
        //             }
        //         }
        //         else // 내가 배팅을 했다면
        //         {
        //             if (bettype == POKER_BETTYPE.bet || bettype == POKER_BETTYPE.raise) // 앞사람이 벳 레이즈를 했다면
        //             {
        //                 reserveBettingToggles.Init();
        //                 reserveBettingToggles.SetReserveToggles(POKER_RESERVE_TYPE.FOLD, POKER_RESERVE_TYPE.CALL, POKER_RESERVE_TYPE.RAISE);
        //                 Debug.LogError("일반의 예약 : 폴드 콜 레이즈");
        //             }
        //             else
        //             {
        //                 Debug.LogError("이미 베팅함");
        //             }
        //         }
        //     }
        // }

        // SetReserveBettingToggleContainer(true);
    }

    private void WhoseTurn(JObject c)
    {
        int turnSeat = (int)c["seat"];
        var player = playerManager.FindPlayerWithSeat(turnSeat);
        if (player.CompareGid(MyStatus.gid) && player.Coffee_break <= 0)
        {
            prevMyTurn = true;
            int activeBettype = (int)c["bettype"];
            ViveManager.Vibrate(200);
            // 예약베팅
            AutoBetting(activeBettype);
            //if ((prevAction == POKER_BETTYPE.bet || prevAction == POKER_BETTYPE.raise) && (reserveBettingToggles.currentReserveType != POKER_RESERVE_TYPE.FOLD && reserveBettingToggles.currentReserveType != POKER_RESERVE_TYPE.FOLDCHECK))
            //{
            //    bettingButtons.SetActiveBettingButton(activeBettype);
            //    SetBettingButtonContainer(true);
            //    SoundManager.Instance.PlayEffectSound(Sound_TableEnum.SFX_MY_TURN);
            //    Debug.Log("배팅 하세요");
            //}
            //else if (reserveBettingToggles.currentReserveType != POKER_RESERVE_TYPE.NONE)
            //{

            //    var type = reserveBettingToggles.currentReserveType;

            //    bettingButtons.SetActiveBettingButton(activeBettype);
            //    AutoBetting(activeBettype);
            //    reserveBettingToggles.Init();

            //}
            //else
            //{
            //    bettingButtons.SetActiveBettingButton(activeBettype);
            //    SetBettingButtonContainer(true);
            //    SoundManager.Instance.PlayEffectSound(Sound_TableEnum.SFX_MY_TURN);
            //    Debug.Log("배팅 하세요");
            //}
        }
        else
        {
            if (prevMyTurn)
            {
                SetBettingButtonContainer(false);
            }
            prevMyTurn = false;

            if (!playerManager.imObserver)
            {
                SetReserveToggleVer2();
            }
        }
    }

    public void SetReserveToggleVer2()
    {
        HoldemPlayer hmp = (HoldemPlayer)playerManager.myPlayer;
        var prevBetorRaise = (prevAction == POKER_BETTYPE.bet || prevAction == POKER_BETTYPE.raise);

        if (!raiseOrBet)
        {
            raiseOrBet = prevBetorRaise;
        }
        if (
            prevBetorRaise
            && reserveBettingToggles.currentReserveType != POKER_RESERVE_TYPE.FOLDCHECK
            && reserveBettingToggles.currentReserveType != POKER_RESERVE_TYPE.FOLD
        ) // 바로 앞 사람이 벳 or 레이즈
        {
            //reserveBettingToggles.Init();
        }

        if (hmp.status != 0 && myBetType != POKER_BETTYPE.allin && POKER_BETTYPE.die != myBetType)
        {
            if (myBetType == POKER_BETTYPE.big)
            {
                if (tableCall <= myBetChip)
                {
                    if (hmp.Chip + myBetChip <= tableCall)
                    {
                        reserveBettingToggles.SetReserveToggles(
                            POKER_RESERVE_TYPE.FOLDCHECK,
                            POKER_RESERVE_TYPE.CHECK
                        );
                    }
                    else
                    {
                        reserveBettingToggles.SetReserveToggles(
                            POKER_RESERVE_TYPE.FOLDCHECK,
                            POKER_RESERVE_TYPE.CHECK,
                            POKER_RESERVE_TYPE.RAISE
                        );
                    }
                }
                else
                {
                    if (hmp.Chip + myBetChip <= tableCall)
                    {
                        reserveBettingToggles.SetReserveToggles(
                            POKER_RESERVE_TYPE.FOLD,
                            POKER_RESERVE_TYPE.CALL
                        );
                    }
                    else
                    {
                        reserveBettingToggles.SetReserveToggles(
                            POKER_RESERVE_TYPE.FOLD,
                            POKER_RESERVE_TYPE.CALL,
                            POKER_RESERVE_TYPE.RAISE
                        );
                    }
                }
            }
            else
            {
                if (tableCall <= 0)
                {
                    if (hmp.Chip + myBetChip <= tableCall)
                    {
                        reserveBettingToggles.SetReserveToggles(
                            POKER_RESERVE_TYPE.FOLDCHECK,
                            POKER_RESERVE_TYPE.CHECK
                        );
                    }
                    else
                    {
                        reserveBettingToggles.SetReserveToggles(
                            POKER_RESERVE_TYPE.FOLDCHECK,
                            POKER_RESERVE_TYPE.CHECK,
                            POKER_RESERVE_TYPE.BET
                        );
                    }
                }
                else
                {
                    if (hmp.Chip + myBetChip <= tableCall)
                    {
                        reserveBettingToggles.SetReserveToggles(
                            POKER_RESERVE_TYPE.FOLD,
                            POKER_RESERVE_TYPE.CALL
                        );
                    }
                    else
                    {
                        reserveBettingToggles.SetReserveToggles(
                            POKER_RESERVE_TYPE.FOLD,
                            POKER_RESERVE_TYPE.CALL,
                            POKER_RESERVE_TYPE.RAISE
                        );
                    }
                }
            }
            //SetReserveBettingToggleContainer(true);
        }
        else
        {
            SetReserveBettingToggleContainer(false);
            Debug.Log("게임 참여 안함");
        }
    }

    protected override void GameEnd()
    {
        StopAllCoroutines();
        SetBettingButtonContainer(false);
        //lastMoneyTextValue.Set(potText, 0);
        //potText.text = GetMoneyString(0);
        collectChip.SetChip(0);
        potText.SetChip(0);
        myBetChip = 0;
        myBetTotal = 0;
        myBetType = POKER_BETTYPE.none;
        betOrRaise = POKER_BETTYPE.none;
        if (reserveFoldToggle)
        {
            reserveFoldToggle.isOn = false;
            reserveFoldToggle.gameObject.SetActive(false);
        }
        stageBetList.Clear();
    }

    public void AutoBetting(int activeBettype)
    {
        POKER_BETTYPE bettype = POKER_BETTYPE.none;
        long chip;
        if (reserveFoldToggle && reserveFoldToggle.isOn)
        {
            if ((activeBettype & (int)POKER_BETTYPE.check) > 0)
            {
                bettype = POKER_BETTYPE.check;
            }
            else
            {
                bettype = POKER_BETTYPE.die;
            }
        }
        if (reserveBettingToggles.foldCheck)
        {
            if ((activeBettype & (int)POKER_BETTYPE.check) > 0)
            {
                bettype = POKER_BETTYPE.check;
            }
            else
            {
                bettype = POKER_BETTYPE.die;
            }
        }
        else if (reserveBettingToggles.fold)
        {
            bettype = POKER_BETTYPE.die;
        }
        else if (reserveBettingToggles.check)
        {
            if ((activeBettype & (int)POKER_BETTYPE.check) > 0)
            {
                bettype = POKER_BETTYPE.check;
            }
            else
            {
                bettype = POKER_BETTYPE.none;
            }
        }
        else if (reserveBettingToggles.call)
        {
            if ((activeBettype & (int)POKER_BETTYPE.call) > 0)
            {
                bettype = POKER_BETTYPE.call;
            }
            else
            {
                bettype = POKER_BETTYPE.none;
            }
        }
        else if (reserveBettingToggles.bet)
        {
            if ((activeBettype & (int)POKER_BETTYPE.bet) > 0)
            {
                bettype = POKER_BETTYPE.bet;
            }
            else if ((activeBettype & (int)POKER_BETTYPE.raise) > 0)
            {
                bettype = POKER_BETTYPE.raise;
            }
            else
            {
                bettype = POKER_BETTYPE.none;
            }
        }
        else if (reserveBettingToggles.raise)
        {
            if ((activeBettype & (int)POKER_BETTYPE.raise) > 0)
            {
                bettype = POKER_BETTYPE.raise;
            }
            else
            {
                bettype = POKER_BETTYPE.none;
            }
        }

        switch (bettype)
        {
            case POKER_BETTYPE.bet:
                chip = bet_chip;
                break;
            case POKER_BETTYPE.raise:
                chip = raise_chip;
                break;
            default:
                chip = 0;
                break;
        }
        if (
            bettype != POKER_BETTYPE.none
            && bettype != POKER_BETTYPE.bet
            && bettype != POKER_BETTYPE.raise
        )
        {
            isAction = true;
            reserveButtonLock.SetActive(true);
            StartCoroutine(DelayAutoBet(autobetDelayTime, bettype, chip));
        }
        else
        {
            SetBettingButtons(activeBettype);
        }
    }

    public void SetBettingButtons(int activeBettype)
    {
        bettingButtons.SetActiveBettingButton(activeBettype);
        var myChip = playerManager.myPlayer.Chip;
        var data = InfoManager.Instance.GetRoom(roomNumber);
        var bb = data.bg;
        if ((activeBettype & (int)POKER_BETTYPE.raise) == (int)POKER_BETTYPE.raise)
        {
            //myBetType != POKER_BETTYPE.big &&
            betOrRaise = POKER_BETTYPE.raise;
            //bool lastBetIsCall = false;
            //for(int i = stageBetList.Count - 1; i >= 0; i--)
            //{
            //    if(stageBetList[i] == POKER_BETTYPE.call)
            //    {
            //        lastBetIsCall = true;
            //        break;
            //    }
            //}

            long basic_call = call_chip + myBetChip;
            long basic_pot = pot + basic_call;
            Debug.Log(
                string.Format(
                    "basic_pot {0}  = pot {1} + basic_call {2} ( call_chip {3}  + myBetChip {4} ) ",
                    basic_pot,
                    pot,
                    basic_call,
                    call_chip,
                    myBetChip
                )
            );

            minimumRaise = basic_call + money_minimum_raise;
            Debug.Log(
                string.Format(
                    "minimumRaise {0} = basic_call {1} + money_minimum_raise {2} ",
                    minimumRaise,
                    basic_call,
                    money_minimum_raise
                )
            );

            if (bettingRound == HOLDEM_BETTING_ROUND.PRE_FLOP && basic_call <= bb) // && !lastBetIsCall)
            {
                bettingButtons.RaiseBBSet(
                    call_chip,
                    bb,
                    basic_pot,
                    myChip,
                    myBetChip,
                    myBetTotal,
                    minimumRaise,
                    data.game_type == GAME_TYPE.plo
                );
            }
            else
            {
                bettingButtons.RaiseRateSet(
                    call_chip,
                    bb,
                    basic_pot,
                    myChip,
                    myBetChip,
                    myBetTotal,
                    minimumRaise,
                    data.game_type == GAME_TYPE.plo
                );
            }

            numberPadMin = minimumRaise;
        }
        if ((activeBettype & (int)POKER_BETTYPE.bet) == (int)POKER_BETTYPE.bet)
        {
            long basic_call = call_chip + myBetChip;
            long basic_pot = pot + call_chip;

            numberPadMin = (bb * 1);

            betOrRaise = POKER_BETTYPE.bet;
            bettingButtons.BetRateSet(
                call_chip,
                bb,
                basic_pot,
                myChip,
                myBetChip,
                myBetTotal,
                numberPadMin,
                data.game_type == GAME_TYPE.plo
            );

            // Bet 최소베팅
        }

        SetReserveBettingToggleContainer(false);
        SetBettingButtonContainer(true);
        //if (bettype == POKER_BETTYPE.bet)
        //{
        //    SetBettingButtonContainer(true);
        //    OnClickBettingPopUp("bet");
        //}
        //else if (bettype == POKER_BETTYPE.raise)
        //{
        //    SetBettingButtonContainer(true);
        //    OnClickBettingPopUp("raise");
        //}
        if (Tm.activeTable)
        {
            SoundManager.Instance.PlayEffectSound(Sound_TableEnum.SFX_MY_TURN);
        }
    }

    public IEnumerator DelayAutoBet(float seconds, POKER_BETTYPE bettype, long chip)
    {
        yield return new WaitForSecondsRealtime(seconds);
        reserveButtonLock.SetActive(false);
        SetReserveBettingToggleContainer(false);
        WebSocketManager.defaultCli.Send(
            "{\"p\":201,\"c\":{\"gtn\":"
                + roomNumber
                + ",\"gid\":\""
                + MyStatus.gid
                + "\", \"betstep\":"
                + betstep
                + ", \"bettype\":"
                + (int)bettype
                + ", \"howmuch\":"
                + chip
                + "}}"
        );
    }

    public override void OnClickBettingButton(string type)
    {
        POKER_BETTYPE bettype = (POKER_BETTYPE)System.Enum.Parse(typeof(POKER_BETTYPE), type);

        long chip;
        switch (bettype)
        {
            case POKER_BETTYPE.bet:
                chip = bet_chip; //RoomStatus.bg;
                break;
            case POKER_BETTYPE.raise:
                chip = raise_chip; //RoomStatus.bg * 2;
                break;
            default:
                chip = 0;
                break;
        }
        isAction = true;
        raiseOrBet = false;
        Debug.Log(string.Format("배팅 / 타입: {0} / 금액: {1}", bettype, chip));
        WebSocketManager.defaultCli.Send(
            "{\"p\":201,\"c\":{\"gtn\":"
                + roomNumber
                + ",\"gid\":\""
                + MyStatus.gid
                + "\", \"betstep\":"
                + betstep
                + ", \"bettype\":"
                + (int)bettype
                + ", \"howmuch\":"
                + chip
                + "}}"
        );
    }

    public void OnClickNumberPadBetButton()
    {
        if (betOrRaise != POKER_BETTYPE.bet && betOrRaise != POKER_BETTYPE.raise)
        {
            Debug.LogError("betOrRaise Error");
            return;
        }
        long amount = 0;
        double value = numberPad.GetValue();
        if (chipViewMode == CHIP_VIEW_MODE.CHIP)
        {
            amount = (long)(value);
        }
        else
        {
            long bg = InfoManager.Instance.GetRoom(roomNumber).bg;
            amount = (long)(value * bg);
        }

        amount -= myBetChip;

        Packet p = new Packet(CPProtocol.CP_HOLDEM_BET);
        p.Add("gtn", roomNumber);
        p.Add("betstep", betstep);
        p.Add("bettype", (int)betOrRaise);
        p.Add("howmuch", amount);
        WebSocketManager.defaultCli.Send(p);
    }

    public void OnClickRaiseButton(int idx)
    {
        raise_chip = bettingButtons.raisebuttons[idx].price;
        OnClickBettingButton(POKER_BETTYPE.raise.ToString());
    }

    public void OnClickBetButton(int idx)
    {
        bet_chip = bettingButtons.betbuttons[idx].price;
        OnClickBettingButton(POKER_BETTYPE.bet.ToString());
    }
}
