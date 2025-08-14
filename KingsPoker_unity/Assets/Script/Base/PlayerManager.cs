using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerManager : WebsocketListenBehaviour
{
    public GameObject gameTabButton;

    [HideInInspector]
    public GAME_TYPE gameType;

    [HideInInspector]
    public long blind;
    public List<Player> players = new List<Player>();
    public Player myPlayer = null;
    public List<Button> seatInButtons = new List<Button>();
    public List<Button> seatInActiveEmos = new List<Button>();
    private Emoticon active_emo;
    private EmoticonInfo active_emo_info;
    private bool _im_seat = false;
    public int bossSeat { get; protected set; } = 0;
    public bool im_seat
    {
        get { return _im_seat; }
        set
        {
            _im_seat = value;
            if (_im_seat)
            {
                reserveSitOut = false;
            }
            SeatInButtonSetting();
        }
    }
    public bool playingGame { get; private set; } = false;
    public Text[] roomInfoTexts;
    public Text roomNumberText;
    private int gn;
    public GameObject tournamentUI;
    public GameObject holdemUI;
    public GameObject rankWindow;
    public GameObject rankSlotPrefab;
    public GameObject rewardSlotPrefab;
    public Transform rankParentTr;
    public Transform rewardParentTr;
    public RankSlot MyRank;
    public TournamentRankPopup rankPopup;
    protected List<RewardSlot> rewardList;
    protected List<RankSlot> rankSlotList;
    protected ROOM_COMMAND roomCommand;

    public long beforeBl;
    public long nextBl;

    public Text beforeBlText;
    public Text nextBlText;

    public Button beforeTableMoveButton;
    public Button nextTableMoveButton;
    public GameObject tableMoveWindow;
    public bool imObserver = false;

    [SerializeField]
    public Button addChipButton;

    [SerializeField]
    protected BettingManager bettingManager;
    protected List<int> playingSeatList = new List<int>();

    [SerializeField]
    private GameObject coffeBreakObj;

    [ReadOnly]
    public long roomNumber;
    private int tn;

    protected Dictionary<Text, long> lastMoneyTextValue = new Dictionary<Text, long>();

    // private static PlayerManager instance;
    // public static PlayerManager Instance {
    //     get {
    //         return instance;
    //     }
    // }
    private bool tableMove;
    public bool reserveSitOut { get; protected set; } = false;
    public bool reserveLeave { get; protected set; } = false;

    [SerializeField]
    protected DealerButtonsManager dealerButtonsManager;

    protected virtual void Start()
    {
        //if(users != null)
        //    PlayerSetting();
        RoomInfoTextSet();
        StatusSet();
    }

    protected override void Awake()
    {
        base.Awake();
        var data = InfoManager.Instance.GetRoom(roomNumber);
        if (data != null)
        {
            blind = data.sm;
            tn = data.tn;
            PlayerSetting();
            StatusSet();
        }
    }

    public virtual void PlayerTurnSetting(int seat, float time, RoomStatus rs = null)
    {
        List<Player> all = GetPlayingGamer();
        Console.SpecialLog("Playing Gamer Count : " + all.Count);
        List<RoomUserData> players = null;

        POKER_FLOW flow = POKER_FLOW.none;
        if (rs != null)
        {
            players = rs.userList;
            flow = (POKER_FLOW)rs.flow;
        }

        for (int i = 0; i < all.Count; i++)
        {
            int pokerBetType = 0;
            if (rs != null)
            {
                for (int j = 0; j < players.Count; j++)
                {
                    if (players[j].gid == all[i].gid)
                    {
                        pokerBetType = players[j].last_bettype;
                        //Console.Error("bettype : " + pokerBetType);
                        break;
                    }
                }
            }

            if (
                all[i].CompareSeat(seat)
                && (
                    (
                        (pokerBetType & (int)POKER_BETTYPE.allin) == 0
                        && (pokerBetType & (int)POKER_BETTYPE.die) == 0
                        && flow != POKER_FLOW.show_down
                    )
                    || rs == null
                )
            )
            {
                if (all[i].CompareGid(MyStatus.gid))
                    all[i].MyTurn(time);
                else
                    all[i].MyTurn(time, false, MyTurnProgress);
            }
            else if (all[i].myTurn)
            {
                all[i].NotMyTurn();
            }
        }
    }

    protected virtual void MyTurnProgress(float progress) { }

    public virtual void OnClickSeatInButton(Button seatInButton)
    {
        if (im_seat)
            return;
        int seat = seatInButtons.IndexOf(seatInButton);
        if (seat == -1)
            return;
        int first_seat = 0;
        if (players[0].seat >= 0 && players[0].seat <= 9)
            first_seat = players[0].seat;
        seat -= first_seat;
        if (seat < 0)
            seat += 9;
        Packet p = new Packet(CPProtocol.CP_ROOM_SEAT);
        p.Add("gtn", roomNumber);
        p.Add("seat", seat);
        WebSocketManager.defaultCli.Send(p);
    }

    public void OnClickSeatOutCancel()
    {
        if (reserveLeave)
        {
            var packet = new Packet((int)CPProtocol.CP_ROOM_RESERVE_LEAVE);
            var json = new JObject();
            packet.Add("gtn", roomNumber);
            packet.Add("leave", false);
            tableMove = false;
            WebSocketManager.defaultCli.Send(packet.ToJson());
        }
        if (reserveSitOut)
        {
            Packet p = new Packet(CPProtocol.CP_ROOM_USER_STATUS_SEAT);
            p.Add("gtn", roomNumber);
            p.Add("status_seat", (int)ROOM_USER_STATUS_SEAT.seated);
            WebSocketManager.defaultCli.Send(p);
        }
    }

    public bool IsPlayingGamer(string gid)
    {
        Player player = FindPlayerWithGid(gid);
        if (player == null)
            return false;
        return playingSeatList.Contains(player.seat);
    }

    public void OnClickSeatOutButton()
    {
        if (!im_seat)
            return;
        Packet p = new Packet(CPProtocol.CP_ROOM_USER_STATUS_SEAT);
        p.Add("gtn", roomNumber);
        p.Add("status_seat", (int)ROOM_USER_STATUS_SEAT.standing_up_after_round);
        WebSocketManager.defaultCli.Send(p);
    }

    public void OnClickCoffeEndButton()
    {
        Packet p = new Packet(CPProtocol.CP_ROOM_USER_STATUS_SEAT);
        p.Add("gtn", roomNumber);
        p.Add("status_seat", (int)ROOM_USER_STATUS_SEAT.seated);
        p.Add("coffee_break_end", true);
        WebSocketManager.defaultCli.Send(p);
    }

    public virtual void ChangeChipViewMode(CHIP_VIEW_MODE mode)
    {
        foreach (KeyValuePair<Text, long> data in lastMoneyTextValue)
        {
            data.Key.text = bettingManager.GetMoneyString(data.Value);
        }
        for (int i = 0; i < players.Count; i++)
        {
            if (bettingManager.chipViewMode == CHIP_VIEW_MODE.CHIP)
            {
                players[i].ChipViewModeSetting(bettingManager.chipViewMode, 1);
            }
            if (bettingManager.chipViewMode == CHIP_VIEW_MODE.BB)
            {
                RoomStatus room = InfoManager.Instance.GetRoom(roomNumber);
                players[i].ChipViewModeSetting(bettingManager.chipViewMode, room.bg);
            }
        }
    }

    public void OnClickBeforeTableMoveButton()
    {
        blind = beforeBl;
        OnClickTableMoveButton();
    }

    public void OnClickNextTableMoveButton()
    {
        blind = nextBl;
        OnClickTableMoveButton();
    }

    public void TableMoveOnOff()
    {
        tableMoveWindow.SetActive(!tableMoveWindow.activeSelf);
    }

    protected void PlayerTurnSetting(string gid, float time)
    {
        List<Player> all = GetPlayingGamer();
        for (int i = 0; i < all.Count; i++)
        {
            if (all[i].CompareGid(gid))
            {
                all[i].MyTurn(time);
            }
            else if (all[i].myTurn)
            {
                all[i].NotMyTurn();
            }
        }
        if (myPlayer.CompareGid(gid))
        {
            myPlayer.MyTurn(time);
        }
    }

    public virtual void PlayerSetting()
    {
        int mySeat = 0;
        var room = InfoManager.Instance.GetRoom(roomNumber);
        if (room == null)
        {
            return;
        }
        List<RoomUserData> userList = room.userList;
        imObserver = true;
        bool[] activeSeat = new bool[players.Count];
        for (int i = 0; i < userList.Count; i++)
        {
            if (MyStatus.gid == userList[i].gid)
            {
                //userList[i]["seat"].SetJsonType(JsonType.Int);
                int seat = userList[i].seat;
                if (seat != 99 && seat != -1)
                {
                    mySeat = seat;
                    imObserver = false;
                }
            }
        }

        dealerButtonsManager.SetMySeat(mySeat);
        if (addChipButton)
        {
            addChipButton.interactable = !imObserver;
        }

        List<Player> playingGamers = GetPlayingGamer();
        Dictionary<string, string> cardData = new Dictionary<string, string>();
        Dictionary<string, long> betData = new Dictionary<string, long>();
        Dictionary<string, POKER_BETTYPE> bettypeData = new Dictionary<string, POKER_BETTYPE>();
        string curTurn = "";
        double hasTime = 0;
        double maxTime = 0;
        string straddleUser = "";
        playingGamers.ForEach(p =>
        {
            List<Card> cards = p.GetCardComponent();
            string cardStr = "";
            for (int i = 0; i < cards.Count; i++)
            {
                if (cards[i].gameObject.activeInHierarchy)
                {
                    if (cardStr != "")
                        cardStr += ",";
                    cardStr += cards[i].GetCardData();
                }
            }
            cardData.Set(p.gid, cardStr);

            betData.Set(p.gid, p.GetCurBet());
            bettypeData.Set(p.gid, p.betting.currentBetType);

            if (p.myTurn)
            {
                curTurn = p.gid;
                hasTime = p.gaugeBar.time;
                maxTime = p.gaugeBar.maxTime;
            }

            if (p.straddle)
            {
                straddleUser = p.gid;
            }
        });
        for (int i = 0; i < userList.Count; i++)
        {
            //userList[i]["seat"].SetJsonType(JsonType.Int);
            int seat = (int)userList[i].seat;
            if (seat == 99 || seat == -1)
            {
                continue;
            }
            seat -= mySeat;

            if (seat < 0)
            {
                seat += players.Count;
            }
            string gid = userList[i].gid;
            activeSeat[seat] = true;
            Player player = players[seat];
            if (seat == 0 && !imObserver)
            {
                player = myPlayer;
            }
            string nick = userList[i].nick;
            long gc = userList[i].gc;
            int status = userList[i].status;
            string url = userList[i].photourl;
            int icon_no = userList[i].icon_no;
            bool useUrlPhoto = userList[i].useUrlPhoto;

            if (!player.CompareGid(gid))
            {
                Console.SpecialLog(nick + " - " + status);
                player.Clear();
                player.SetPlayer(
                    gid,
                    nick,
                    gc,
                    (int)(userList[i].seat),
                    status,
                    icon_no,
                    url,
                    useUrlPhoto
                );
                if (roomCommand >= ROOM_COMMAND.start && roomCommand <= ROOM_COMMAND.calc_dividend)
                {
                    if (cardData.ContainsKey(gid) && cardData[gid] != "")
                        player.SetCard(cardData[gid]);
                    if (bettypeData.ContainsKey(gid) && bettypeData[gid] != POKER_BETTYPE.none)
                        player.Betting(bettypeData[gid], betData[gid]);
                    if (gid == curTurn)
                        player.MyTurn((float)hasTime);
                    if (gid == straddleUser)
                        player.ActiveStraddleMark(true);
                }

                //                Debug.Log(userList[i].ToJson());
            }
            else
            {
                player.status = (ROOM_USER_STATUS)status;
                player.seat = (int)(userList[i].seat);
            }
        }
        for (int i = 0; i < players.Count; i++)
        {
            if (!activeSeat[i])
            {
                players[i].Clear();
            }
        }
        im_seat = !imObserver; //im_seat의 값을 변경하는 순간 SitIn버튼들을 한번 재정리 하기 때문에, 꼭 플레이어 초기화가 끝난 상태에서 값을 변경해야 함.
    }

    private void SeatInButtonSetting()
    {
        RoomStatus room = InfoManager.Instance.GetRoom(roomNumber);

        for (int i = 0; i < players.Count; i++)
        {
            if (seatInButtons.Count > i)
            {
                seatInButtons[i]
                    .gameObject.SetActive(
                        !players[i].gameObject.activeSelf
                            && !im_seat
                            && !room.random_sit_in
                            && i < room.personnel
                    );
                //플레이어가 앉아있지 않고,
                //내가 앉아있지 않고,
                //랜덤 싯 인이 아니고,
                //방 인원수에 어긋나지 않는 버튼일때 활성화
            }
        }
    }

    public void SeatInActiveEmosSetting(Emoticon emoticon, EmoticonInfo info)
    {
        active_emo = emoticon;
        active_emo_info = info;

        SeatInActiveEmosUnSetting();

        for (int i = 0; i < players.Count; i++)
        {
            if (seatInActiveEmos.Count > i)
            {
                // 자리에 누가 있으면서, 나는 아니어야 한다.
                seatInActiveEmos[i]
                    .gameObject.SetActive(
                        players[i].gameObject.activeSelf && players[i].gid != MyStatus.gid
                    );

                // TODO:  이미지를 넣자.
                // seatInActiveEmos[i].gameObject.GetComponent<Image>().sprite = active_emo_info.iconTexture;
            }
        }
    }

    public void SeatInActiveEmosUnSetting()
    {
        for (int i = 0; i < seatInActiveEmos.Count; i++)
        {
            seatInActiveEmos[i].gameObject.SetActive(false);
        }
    }

    public virtual void OnClickSeatInActiveEmo(Button seatInActiveEmo)
    {
        int seat = seatInActiveEmos.IndexOf(seatInActiveEmo);
        if (seat == -1)
            return;

        //Player player = players[seat];

        //int first_seat = 0;
        //if (players[0].seat >= 0 && players[0].seat <= 9)
        //    first_seat = players[0].seat;
        //seat -= first_seat;
        //if (seat < 0) seat += 9;

        Player target = players[seat];

        var p = new Packet((int)CPProtocol.CP_ROOM_CHAT);
        p.Add("gtn", roomNumber);
        p.Add("type", "emo");
        p.Add("to_gid", MyStatus.gid);
        p.Add("msg", active_emo.ToString());
        p.Add("attacked_gid", target.gid);
        WebSocketManager.defaultCli.Send(p);

        SeatInActiveEmosUnSetting();
    }

    public virtual void MoveTableSet() { }

    public void RoomInfoTextSet()
    {
        var status = InfoManager.Instance.GetRoom(roomNumber);
        if (status == null)
            return;
        if (tn == 0)
        {
            roomNumberText.text = string.Format(
                "{2}: {0} \n{3}: {1}",
                roomNumber,
                gn,
                LocalizeManager.GetLocalString("com_ingame_roomnum"),
                LocalizeManager.GetLocalString("com_ingame_gamenum")
            );
        }
        else
        {
            roomNumberText.text = string.Format(
                "{3}: {0} \n{4}: {1}\n토너먼트번호: {2}",
                roomNumber,
                gn,
                tn,
                LocalizeManager.GetLocalString("com_ingame_roomnum"),
                LocalizeManager.GetLocalString("com_ingame_gamenum")
            );
        }

        if (status.game_type == GAME_TYPE.short_deck)
        {
            for (int i = 0; i < roomInfoTexts.Length; i++)
            {
                string str_blind = string.Format(
                    "{0} ({1})",
                    MoneyToString.Converting(status.bg),
                    MoneyToString.Converting(status.bg)
                );
                roomInfoTexts[i].text = str_blind;
            }
        }
        else
        {
            for (int i = 0; i < roomInfoTexts.Length; i++)
            {
                string str_blind = string.Format(
                    "{0}/{1}",
                    MoneyToString.Converting(status.sm),
                    MoneyToString.Converting(status.bg)
                );
                if (status.an > 0)
                {
                    str_blind += string.Format(" ({0})", MoneyToString.Converting(status.an));
                }
                roomInfoTexts[i].text = str_blind;
            }
        }

        if (status.tn == 0)
        {
            holdemUI.SetActive(true);
            tournamentUI.SetActive(false);
        }
        else
        {
            holdemUI.SetActive(false);
            tournamentUI.SetActive(true);
        }
    }

    public void RoomInfoTextSet(long an, long sm, long bg)
    {
        var status = InfoManager.Instance.GetRoom(roomNumber);
        if (status == null)
            return;

        status.an = an;
        status.sm = sm;
        status.bg = bg;
    }

    public virtual List<Player> GetPlayingGamer()
    {
        List<Player> result = new List<Player>();
        for (int i = 0; i < players.Count; i++)
        {
            if (playingSeatList.Contains(players[i].seat))
            {
                result.Add(players[i]);
            }
        }

        if (!players.Contains(myPlayer))
        {
            if (playingSeatList.Contains(myPlayer.seat))
            {
                result.Add(myPlayer);
            }
        }
        return result;
    }

    public void OnClickLeaveButton()
    {
        var packet = new Packet((int)CPProtocol.CP_ROOM_RESERVE_LEAVE);
        var json = new JObject();
        packet.Add("gtn", roomNumber);
        packet.Add("leave", true);
        tableMove = false;
        WebSocketManager.defaultCli.Send(packet.ToJson());
        //WebSocketManager.defaultCli.Send("{\"p\":104,\"c\":{\"gtn\":"+RoomStatus.gtn+",\"leave\":true}}");
    }

    public void OnClickObserverLeaveButton()
    {
        var packet = new Packet((int)CPProtocol.CP_PLAY_GAME_LEAVE_OBSERVER);
        var json = new JObject();
        packet.Add("gtn", roomNumber);
        tableMove = false;
        WebSocketManager.defaultCli.Send(packet.ToJson());
        //WebSocketManager.defaultCli.Send("{\"p\":104,\"c\":{\"gtn\":"+RoomStatus.gtn+",\"leave\":true}}");
    }

    public void OnClickTableMoveButton()
    {
        var packet = new Packet((int)CPProtocol.CP_ROOM_RESERVE_LEAVE);
        var json = new JObject();
        packet.Add("gtn", roomNumber);
        packet.Add("leave", true);
        tableMove = true;
        WebSocketManager.defaultCli.Send(packet.ToJson());
    }

    public virtual Player FindPlayerWithGid(string gid)
    {
        if (myPlayer.CompareGid(gid))
            return myPlayer;
        return players.Find(
            delegate(Player player)
            {
                return player.CompareGid(gid);
            }
        );
    }

    public virtual Player FindPlayerWithSeat(int seat)
    {
        if (myPlayer.CompareSeat(seat))
            return myPlayer;

        return players.Find(
            delegate(Player player)
            {
                return player.CompareSeat(seat);
            }
        );
    }

    public virtual Player FindMyPlayerWithSeat(int seat)
    {
        if (myPlayer.CompareSeat(seat))
        {
            return myPlayer;
        }
        else
        {
            return null;
        }
    }

    public virtual void ResetPlayers() { }

    public void MoveTable()
    {
        //tableMove = false;
        //Packet p = new Packet((int)CPProtocol.CP_ROOM_ENTER);
        //p.Add("gtn", 0);
        //p.Add("game_type", gameType);
        //p.Add("blind", blind);
        //WebSocketManager.defaultCli.Send(p.ToJson());
    }

    public virtual void OffBettingBalloon()
    {
        myPlayer.betting.ForceBack();
        for (int i = 0; i < players.Count; i++)
        {
            players[i].betting.ForceBack();
        }
    }

    public virtual void OffRateBalloon()
    {
        (myPlayer as HoldemPlayer).rate.ForceBack();
        for (int i = 0; i < players.Count; i++)
        {
            (players[i] as HoldemPlayer).rate.ForceBack();
        }
    }

    /// <summary>
    /// 현재 게임의 상태를 요청합니다.
    /// </summary>
    //public virtual void GetCurrntGameState() { }

    protected override void ReceivePacket(Packet packet)
    {
        base.ReceivePacket(packet);
        int p = packet.p;
        JObject c = packet.c;
        switch ((PCProtocol)p)
        {
            case PCProtocol.PC_ROOM_USER_LIST: //유저리스트
                {
                    if (!RoomNumberCheck(c, roomNumber))
                    {
                        break;
                    }
                    List<JObject> userList = JsonDataParser.Parse<List<JObject>>(c["list"]["item"]);

                    //Store.userList = userList;
                    InfoManager.Instance.HoldemUserList(c);
                    Console.SpecialLog("유저 세팅 시도");
                    PlayerSetting();
                }

                break;
            case PCProtocol.PC_HOLDEM_STATUS:
                if (!RoomNumberCheck(c, roomNumber))
                    break;

                {
                    StatusSet();
                }
                break;
            case PCProtocol.PC_ROOM_PLAYER_SEAT_LIST:
                if (!RoomNumberCheck(c, roomNumber))
                    break;

                {
                    playingSeatList.Clear();
                    JArray list = c["list"] as JArray;
                    for (int i = 0; i < list.Count; i++)
                    {
                        playingSeatList.Add((int)list[i]);
                    }

                    // TODO: 만약 내가 list에 있다면 HoldemBettingManager.'waitForBlindPanel' 이 필요 없다. 닫자. 2021-04-25
                }
                break;
            case PCProtocol.PC_ROOM_LEAVE: //PC_ROOM_LEAVE
                if (!RoomNumberCheck(c, roomNumber))
                    break;

                {
                    // var userList = InfoManager.Instance.GetRoom(roomNumber).userList;
                    // userList.Remove(userList.Find(delegate(RoomUserData data) {
                    //     return data.gid.ToString() == c["gid"].ToString();
                    // }));
                    if (c["gid"].ToString() == MyStatus.gid)
                    {
                        if (tableMove)
                        {
                            MoveTable();
                        }
                        else if (tn != 0)
                        {
                            ERR reason = c.ValueOrDefault<ERR>("reason", ERR.OK);
                            switch (reason)
                            {
                                case ERR.CANCEL_TNMT:
                                    break;
                                case ERR.MOVE_ROOM:
                                    break;
                                case ERR.TNMT_UNAPPLY:
                                    break;
                            }

                            // reason == "cancel_tournament"
                            // reason == "move_room"

                            //
                            //TODO 토너먼트 퇴장 스킵 나중에 추가사항 시 필요
                        }
                        else
                        {
#if Kingdom
                            CustomSceneManager.LoadScene("Lobby_KH");
#else
                            //CustomSceneManager.LoadScene("Lobby");
                            //Destroy(gameObject);
                            //Destroy(gameTabButton);
#endif
                        }
                    }
                    else
                    {
                        PlayerSetting();
                    }
                }

                break;
            case PCProtocol.PC_ROOM_DELETE:
                if (!RoomNumberCheck(c, roomNumber))
                {
                    break;
                }

                {
                    if (tn != 0)
                    {
                        //TnmtMy();
                    }
                }
                break;
            case PCProtocol.PC_ROOM_COMMAND: //PC_ROOM_COMMAND
                if (!RoomNumberCheck(c, roomNumber))
                {
                    break;
                }

                roomCommand = JsonDataParser.Parse<ROOM_COMMAND>(c["command"]);
                gn = JsonDataParser.Parse<int>(c["gn"]);
                RoomInfoTextSet();
                if (roomCommand == ROOM_COMMAND.start)
                {
                    playingGame = true;
                }

                if (roomCommand == ROOM_COMMAND.end || roomCommand == ROOM_COMMAND.start)
                {
                    playingGame = false;
                    GameEnd();
                }
                break;

            case PCProtocol.PC_CAFE_GAME_UPDATE:
                {
                    JObject o = c["o"] as JObject;
                    if (o != null)
                    {
                        if (!RoomNumberCheck(o, roomNumber))
                        {
                            break;
                        }

                        long an = o.ValueOrDefault<long>("ante", 0);
                        long sm = o.ValueOrDefault<long>("small_blind", 0);
                        long bg = o.ValueOrDefault<long>("blind", 0);
                        RoomInfoTextSet(an, sm, bg);
                    }
                }
                break;

            case PCProtocol.PC_ROOM_RESERVE_LEAVE: //PC_ROOM_RESERVE_LEAVE
                if (!RoomNumberCheck(c, roomNumber))
                {
                    break;
                }

                if (true)
                {
                    string gid = JsonDataParser.Parse<string>(c["gid"]);
                    bool leave;
                    try
                    {
                        leave = (bool)c["leave"];
                    }
                    catch (InvalidCastException e)
                    {
                        Console.Error(e);
                        var l = (int)c["leave"];
                        leave = Convert.ToBoolean(l);
                    }

                    //JsonDataParser.Parse(c["leave"],out leave);
                    Debug.Log(leave);
                    Player pl = FindPlayerWithGid(gid);
                    if (pl != null)
                    {
                        pl.SetReservedLeave(leave);
                    }
                    if (c["gid"].ToString() == MyStatus.gid)
                    {
                        reserveLeave = leave;
                        if (leave)
                        {
                            if (tableMove)
                            {
                                MoveTable();
                            }
                            else
                            {
#if Kingdom
                                CustomSceneManager.LoadScene("Lobby_KH");
#else
                                //CustomSceneManager.LoadScene("Lobby");
                                //Destroy(gameObject);
                                //Destroy(gameTabButton);
#endif
                            }
                        }
                    }
                }
                break;
            case PCProtocol.PC_ROOM_ENTER:
                RoomEnter(c);
                break;
            case PCProtocol.PC_ROOM_RESERVE_LEAVE_FAIL: // PC_ROOM_RESERVE_LEAVE_FAIL
                if (!RoomNumberCheck(c, roomNumber))
                {
                    break;
                }
                int code = JsonDataParser.Parse<int>(c["code"]);
                if (code == 305 || code == 1)
                {
#if Kingdom
                    CustomSceneManager.LoadScene("Lobby_KH");
#else
                    //CustomSceneManager.LoadScene("Lobby");
                    if (code == 305)
                    {
                        NormalMessage.instance.AddSimpleMessage(
                            LocalizeManager.GetLocalString(c.ValueOrDefault("reason", ""))
                        );
                    }
#endif
                }
                break;
            case PCProtocol.PC_ROOM_USER_STATUS:
                if (!RoomNumberCheck(c, roomNumber))
                {
                    break;
                }

                {
                    string gid = (string)c["gid"];
                    int coffee_break = c.ValueOrDefault("coffee_break", 0);
                    Player pl = FindPlayerWithGid(gid);
                    if (pl != null)
                    {
                        pl.Coffee_break = coffee_break;
                        //Debug.Log(string.Format("{0} player Room_user_status : {1}", pl.nickText.text, pl.status));
                    }
                    var roomData = InfoManager.Instance.GetRoom(roomNumber);
                    RoomUserData userData = roomData.userList.Find(value => value.gid == gid);
                    if (userData != null)
                    {
                        userData.seat = (int)c["seat"];
                    }
                    if (gid.Equals(MyStatus.gid) && coffeBreakObj)
                    {
                        coffeBreakObj.SetActive(coffee_break > 0);
                    }
                    PlayerSetting();
                }
                break;
            case PCProtocol.PC_ROOM_USER_STATUS_SEAT:
                if (!RoomNumberCheck(c, roomNumber))
                {
                    break;
                }

                {
                    string gid = (string)c["gid"];
                    ROOM_USER_STATUS_SEAT status = c.ValueOrDefault(
                        "status_seat",
                        ROOM_USER_STATUS_SEAT.seated
                    );
                    var coffee_break = c.ValueOrDefault("coffee_break", 0);
                    if (gid.Equals(MyStatus.gid))
                    {
                        reserveSitOut = false;
                        switch (status)
                        {
                            case ROOM_USER_STATUS_SEAT.standing_up_after_round:
                                reserveSitOut = true;
                                break;
                        }
                        if (gid.Equals(MyStatus.gid) && coffeBreakObj)
                        {
                            coffeBreakObj.SetActive(coffee_break > 0);
                        }
                    }
                }
                break;
            // case PCProtocol.PC_TNMT_MY:
            //     {
            //         JsonDataParser.Parse(c["tn"], out MyStatus.tn);
            //         JsonDataParser.Parse(c["gtn"], out MyStatus.tnrn);
            //         TnmtRoomEnter();
            //     }
            //     break;
        }
    }

    protected void TnmtMy()
    {
        var p = new Packet((int)CPProtocol.CP_TNMT_MY);
        WebSocketManager.defaultCli.Send(p.ToJson());
    }

    protected void RoomCommand() { }

    protected virtual void StatusSet()
    {
        var rs = InfoManager.Instance.GetRoom(roomNumber);
        roomCommand = (ROOM_COMMAND)rs.room_command;

        playingGame = (
            roomCommand >= ROOM_COMMAND.start && roomCommand <= ROOM_COMMAND.calc_dividend
        );

        var playingGamers = rs.playerList;
        playingSeatList.Clear();

        if (playingGamers != null)
        {
            for (int i = 0; i < playingGamers.Count; i++)
            {
                var p = playingGamers[i] as JObject;
                string gid = p.ValueOrDefault("gid", string.Empty);
                Player player = FindPlayerWithGid(gid);

                if (player != null)
                {
                    if (playingGame)
                    {
                        playingSeatList.Add(player.seat);
                    }

                    int coffee_break = p.ValueOrDefault("coffee_break", 0);
                    bool isDeal = p.ValueOrDefault("icm", false);

                    if (player != null)
                    {
                        player.Coffee_break = coffee_break;
                        player.IsDeal = isDeal;
                        //Debug.Log(string.Format("{0} player Room_user_status : {1}", pl.nickText.text, pl.status));
                    }
                    var roomData = InfoManager.Instance.GetRoom(roomNumber);
                    RoomUserData userData = roomData.userList.Find(value => value.gid == gid);
                    if (userData != null)
                    {
                        userData.seat = player.seat;
                    }
                    if (gid.Equals(MyStatus.gid) && coffeBreakObj)
                    {
                        coffeBreakObj.SetActive(coffee_break > 0);
                        OnClickCoffeEndButton();
                    }
                }
            }
            PlayerSetting();
        }
    }

    protected virtual void GameEnd()
    {
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].gameObject.activeSelf)
            {
                players[i].ForceBack();
            }
        }
    }

    protected bool RoomNumberCheck(JObject data, long gtn)
    {
        long n = data.ValueOrDefault<long>("gtn", 0);
        return n == gtn;
    }

    protected void RoomTableSet()
    {
        //LobbyRoomList.
    }

    protected void RoomEnter(JObject c)
    {
        JObject room = c["room"] as JObject;
        // JsonDataParser.Parse(c["game_type"],out RoomStatus.game_type);
        // JsonDataParser.Parse(room["gtn"],out RoomStatus.gtn);
        // JsonDataParser.Parse(room["ttl"],out RoomStatus.ttl);
        // JsonDataParser.Parse(room["pw"],out RoomStatus.pw);
        // JsonDataParser.Parse(room["gid"],out RoomStatus.gid);
        // JsonDataParser.Parse(room["nk"],out RoomStatus.nk);
        // JsonDataParser.Parse(room["lv"],out RoomStatus.lv);
        // JsonDataParser.Parse(room["an"],out RoomStatus.an);
        // JsonDataParser.Parse(room["mx"],out RoomStatus.mx);
        // JsonDataParser.Parse(room["bi"],out RoomStatus.bi);
        // JsonDataParser.Parse(room["bil"],out RoomStatus.bil);
        // JsonDataParser.Parse(room["emn"],out RoomStatus.emn);
        // JsonDataParser.Parse(room["emx"],out RoomStatus.emx);
        // JsonDataParser.Parse(room["bg"], out RoomStatus.bg);
        // JsonDataParser.Parse(room["sm"], out RoomStatus.sm);
        // try
        // {
        //     JsonDataParser.Parse(room["tn"], out RoomStatus.tn);
        // }
        // catch(Exception e){Debug.Log(e.ToString());}

        if ((GAME_TYPE)(int)c["game_type"] == GAME_TYPE.nlh)
        {
            if (DevOptionsManager.devOptions.bettingMode == 0)
            {
                CustomSceneManager.LoadScene("Holdem");
            }
            else if (DevOptionsManager.devOptions.bettingMode == 1)
            {
#if Kingdom
                CustomSceneManager.LoadScene("Games_KH");
#else
                CustomSceneManager.LoadScene("Games");
#endif
            }
        }
    }

    public void SetPopUpActiveEmoticon(Emoticon emoticon, string gid, EmoticonInfo info, int gtn)
    {
        PopUpActiveEmoticon(emoticon, gid, info, gtn);
    }

    public void PopUpActiveEmoticon(Emoticon emoticon, string gid, EmoticonInfo info, int gtn)
    {
        var p = new Packet((int)CPProtocol.CP_ROOM_CHAT);
        p.Add("gtn", gtn);
        p.Add("type", "emo");
        p.Add("to_gid", gid);
        p.Add("attacked_gid", gid);
        p.Add("msg", emoticon.ToString());
        WebSocketManager.defaultCli.Send(p);
    }
}
