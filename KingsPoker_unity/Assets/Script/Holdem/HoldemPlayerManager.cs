using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using PokerOdds;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UI;

public class HoldemPlayerManager : PlayerManager
{
    //private void Awake()
    //{
    //    GetCurrntGameState();
    //}

    //public override void GetCurrntGameState()
    //{
    //    Packet p = new Packet(202);
    //    p.Add("gtn", roomNumber);
    //    WebSocketManager.defaultCli.Send(p.ToJson());
    //}
    [SerializeField]
    private BuyInPanel buyinPanel;

    [SerializeField]
    private EmoticonData emoSets;

    [SerializeField]
    private TournamentResultPanel resultPanel;

    [SerializeField]
    private GameObject EmoWindow;

    [SerializeField]
    private int runItTwiceOKUserCount = 0;

    [SerializeField]
    private GameObject timebank_panel;

    [SerializeField]
    private Toggle timebank_toggle;

    public Text timebank_text_OffLabel;
    public Text timebank_text_OnLabel;

    public GameObject timebank2_panel;
    public Button timebank2_button;
    public Text timebank2_text_OffLabel;
    public Text timebank2_text_OnLabel;

    [SerializeField]
    private List<Timer> timebank_timers = new List<Timer>();
    private TIME_BANK_MODE timebank_mode = TIME_BANK_MODE.none;
    private float timebank_prev_progress = 1f;
    public float timebank_active_progress = 0.5f;

    private HoldemTableManager tableManager;
    private HoldemTableManager TableManager
    {
        get
        {
            if (!tableManager)
            {
                tableManager = GetComponent<HoldemTableManager>();
            }
            return tableManager;
        }
    }

    [SerializeField]
    private DealAgreePanel dealAgreePanel;

    [SerializeField]
    private GameObject dealFailPanel;
    [SerializeField]
    private GameObject dealSuccessPanel;
    [SerializeField]
    private DealPanel dealPanel;
  

    protected override void Start()
    {
        base.Start();
        buyinPanel.canBuyinFunc = () =>
        {
            Player myPlayer = FindPlayerWithGid(MyStatus.gid);
            if (myPlayer == null) //내가 앉지 않았을때
                return false;
            if (playingGame && !myPlayer.isDie && playingSeatList.Contains(myPlayer.seat)) //게임이 진행중이고, 나도 참여중이고, 내가 폴드한게 아닐ㄷ
                return false;
            return true;
        };

        //Packet p = new Packet(CPProtocol.CP_ROOM_USER_STATUS_SEAT);
        //p.Add("gtn", roomNumber);
        //p.Add("status_seat", (int)ROOM_USER_STATUS_SEAT.seated);
        //WebSocketManager.defaultCli.Send(p);
        //MoveTableSet();
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
                    int seat = (int)c["seat"];

                    HoldemPlayer pl = FindPlayerWithSeat(seat) as HoldemPlayer;
                    if (pl == null)
                    {
                        RoomReenter();
                    }
                    if (pl.CompareGid(MyStatus.gid))
                    {
                        timebank_panel.SetActive(false);
                        timebank2_panel.SetActive(false);
                    }
                }
                break;
            case PCProtocol.PC_HOLDEM_WHO_IS_TURN: //PC_HOLDEM_WHO_IS_TURN
                if (!RoomNumberCheck(c, roomNumber))
                {
                    break;
                }

                {
                    int seat = (int)c["seat"];
                    float time = (float)c["time"];
                    PlayerTurnSetting(seat, time);
                }
                break;
            case PCProtocol.PC_USER_INFO_UPDATE:
                {
                    Player player = FindPlayerWithGid(c["gid"].ToString());
                    if (player != null)
                    {
                        player.SetInfo(c["nick"].ToString(), (int)c["icon"], (long)c["gc"]);
                    }
                }
                break;
            case PCProtocol.PC_HOLDEM_GAMERESULT: //PC_HOLDEM_GAME_RESULT
                if (!RoomNumberCheck(c, roomNumber))
                {
                    break;
                }
                JObject[][] gameResults = JsonDataParser.Parse<JObject[][]>(c["list"]["item"]);

                if (gameResults.Length > 1)
                {
                    Console.Log("run_it_twice = 2 ");
                }
                else
                {
                    Console.Log("run_it_twice = 1 ");
                }

                JObject[] arr = gameResults[0];

                int winnerCount = 0;
                for (int i = 0; i < arr.Length; i++)
                {
                    bool winner = JsonDataParser.Parse<bool>(arr[i]["winner"]);
                    winnerCount += winner ? 1 : 0;
                }
                int notDieCount = 0;
                for (int i = 0; i < arr.Length; i++)
                {
                    bool d = arr[i]["die"].ToObject<bool>();
                    //JsonDataParser.Parse(arr[i]["die"],out d);
                    if (!d)
                    {
                        notDieCount++;
                    }
                }
                for (int i = 0; i < arr.Length; i++)
                {
                    string gid = (string)arr[i]["gid"];
                    bool winner = (bool)arr[i]["winner"];
                    bool die = (bool)arr[i]["die"];
                    string hands = (string)arr[i]["hands"];
                    long a = (long)arr[i]["a"];
                    long w = (long)arr[i]["w"];
                    long gc = (long)arr[i]["gc"];

                    { //--------------클라이언트 족보 계산
                        var cardManager = TableManager.cardManager as HoldemCardManager;
                        var commCards = cardManager.commCards;
                        var cards = FindPlayerWithGid(gid).GetCard();
                        var jokbov = CalculateCardsValue.calc(
                            $"{commCards} {string.Join(" ", cards)}"
                        );
                        var jockbo = CalculateCardsValue.jokboName(jokbov);
                        hands = $"{jockbo[0]},{jockbo[1]},{jockbo[2]}"; // string.Join(",", CalculateCardsValue.jokboName(jokbov));
                    }

                    //Console.Error(hands);
                    if (die)
                    {
                        hands = LocalizeManager.GetLocalString("ingame_holdem_fold_lose");
                    }
                    else
                    {
                        if (notDieCount > 1)
                        {
                            var handArr = hands.Split(',');
                            handArr[0] = LocalizeManager.GetLocalString(handArr[0]);
                            hands = string.Join(" ", handArr);
                            hands = hands.ToUpper();
                        }
                        else
                        {
                            hands = LocalizeManager.GetLocalString("ingame_holdem_fold_win");
                        }
                    }
                    winner = a < w; //내가 칩을 획득한 경우에 win표시
                    FindPlayerWithGid(gid).PlayResult(hands, a, w, gc, winner, winnerCount > 1);
                    InfoManager.Instance.GetRoom(roomNumber).userList.Find(d => d.gid == gid).gc =
                        gc;
                }
                if (InfoManager.Instance.GetRoom(roomNumber).tn == 0)
                {
                    //if (false)//myPlayer.Chip == 0)
                    //{
                    //    AutoRebuy();
                    //}
                }
                break;
            case PCProtocol.PC_HOLDEM_WHO_IS_BOSS: //PC_HOLDEM_WHO_IS_BOSS
                if (!RoomNumberCheck(c, roomNumber))
                {
                    break;
                }
                bossSeat = (int)c["boss_seat"];
                List<Player> list = GetPlayingGamer();
                SetBoss(bossSeat);

                break;
            case PCProtocol.PC_HOLDEM_OPEN_CARDS:
                if (!RoomNumberCheck(c, roomNumber))
                {
                    break;
                }
                if (true)
                {
                    string gid = (string)c["gid"];
                    string strHands = (string)c["strHands"];
                    FindPlayerWithGid(gid).OpenCards(strHands.Split(','));
                    break;
                }
            case PCProtocol.PC_HOLDEM_STATUS:
                if (!RoomNumberCheck(c, roomNumber))
                {
                    break;
                }

                {
                    StatusSet();
                    var deal_rewards = c.CastOrEmpty<JArray>("deal_rewards",true);
                    var room_command = c.ValueOrDefault("room_command",ROOM_COMMAND.none);
                    if(room_command == ROOM_COMMAND.check_tournament_deal)
                    {
                        SetDealAgreePanel(c);
                    }
                }

                break;

            case PCProtocol.PC_ROOM_COMMAND: //PC_ROOM_COMMAND
                if (!RoomNumberCheck(c, roomNumber))
                {
                    break;
                }

                roomCommand = JsonDataParser.Parse<ROOM_COMMAND>(c["command"]);

                if (roomCommand == ROOM_COMMAND.start)
                {
                    timebank_mode = (TIME_BANK_MODE)
                        int.Parse(PlayerPrefs.GetString("auto_timebank", "3"));
                }
                break;
            case PCProtocol.PC_TNMT_RANKING:
                //if(!RoomNumberCheck(c, roomNumber)){break;}
                //RankingPopup(c);
                break;
            case PCProtocol.PC_TNMT_MY_RESULT:
                if (!RoomNumberCheck(c, roomNumber))
                {
                    break;
                }
                TnmtResult(c);
                break;
            case PCProtocol.PC_ROOM_CHAT:
                if (!RoomNumberCheck(c, roomNumber))
                {
                    break;
                }
                string type = c["type"].ToObject<string>();
                if (type == "emo")
                {
                    ReceiveEmo(c);
                }

                break;
            case PCProtocol.PC_ROOM_BUYIN:
                if (!RoomNumberCheck(c, roomNumber))
                {
                    break;
                }

                {
                    var gid = c["gid"].ToObject<string>();
                    var pl = FindPlayerWithGid(gid);
                    List<RoomUserData> userList = InfoManager.Instance.GetRoom(roomNumber).userList;
                    for (int i = 0; i < userList.Count; i++)
                    {
                        if (userList[i].gid == gid)
                        {
                            userList[i].gc = c.ValueOrDefault("gc", userList[i].gc);
                        }
                    }
                    if (pl != null)
                    {
                        var chip = c["gc"].ToObject<long>();
                        pl.Chip = chip;
                    }
                    if (MyStatus.gid == gid)
                    {
                        buyinPanel.gameObject.SetActive(false);
#if UNITY_WEBGL && !UNITY_EDITOR
                        WebGLWindowMessage.OnUpdatedFlow("buyin", "buyin");
#endif
                    }
                }
                break;
            //case PCProtocol.PC_ROOM_USER_STATUS_SEAT:
            //    if (!RoomNumberCheck(c, roomNumber)) { break; }
            //    {
            //        if (c["status_seat"].ToObject<int>() == 1)
            //        {
            //            bool autoBuyin = bool.Parse(PlayerPrefs.GetString("autobuyin", false.ToString()));
            //            if(autoBuyin)
            //            {
            //                AutoBuyIn();
            //            }
            //            else
            //            {
            //                OpenBuyInPopup();
            //            }
            //        }
            //    }


            //    break;
            case PCProtocol.PC_CAFE_MEMBER_ORDER_UPDATE:
                if (c.ContainsKey("cafeMember"))
                {
                    //curEnterCafeInfo["cafeMember"]["cc"] = c["cafeMember"]["cc"];
                    Cafe.instance.GetCafeList((int)c["cafeIdx"]).cafeMembers[0] =
                        Cafe.instance.SetCafeMember(c["cafeMember"]);
                    buyinPanel.InitPanel(roomNumber);
                }

                break;
            case PCProtocol.PC_RUN_IT_TWICE_START:
                if (!RoomNumberCheck(c, roomNumber))
                {
                    break;
                }
                runItTwiceOKUserCount = 0;
                break;
            case PCProtocol.PC_RUN_IT_TWICE:
                if (!RoomNumberCheck(c, roomNumber))
                {
                    break;
                }

                // TOOD: PC_RUN_IT_TWICE.seats[]에 myPlayer.seat 가 있으면, 나도 런잇트와이스 결정에 참여한 것이니. SendEmo() 호출 할수 있다.


                int howmany = (int)c["howmany"];
                Emoticon playEmo = Emoticon.run_it_twice_not_pick;
                if (howmany == 2)
                {
                    runItTwiceOKUserCount++;
                    if (runItTwiceOKUserCount == 1)
                    {
                        playEmo = Emoticon.run_it_twice_pick_twice_first;
                    }
                    else
                    {
                        playEmo = Emoticon.run_it_twice_pick_twice_second;
                    }
                }
                else if (howmany == 1)
                {
                    playEmo = Emoticon.run_it_twice_pick_once;
                }
                if (c["gid"].ToString() == MyStatus.gid)
                {
                    SendEmo((int)playEmo);
                }
                break;
            case PCProtocol.PC_STRADDLE:
                if (!RoomNumberCheck(c, roomNumber))
                {
                    break;
                }

                {
                    Player player = FindPlayerWithGid(c["gid"].ToString());
                    player.ActiveStraddleMark((bool)c["on"]);
                }
                break;
            case PCProtocol.PC_ROOM_TIME_BANK:
                if (!RoomNumberCheck(c, roomNumber))
                {
                    break;
                }

                {
                    string gid = c["gid"].ToString();
                    TIME_BANK_MODE mode = (TIME_BANK_MODE)c.ValueOrDefault("mode", 0);
                    TIME_BANK_COMMAND cmd = (TIME_BANK_COMMAND)c.ValueOrDefault("command", 0);
                    double time = c.ValueOrDefault<double>("time", 0);
                    ReceiveTimeBank(gid, mode, cmd, time);
                }
                break;
            case PCProtocol.PC_ROOM_TIME_BANK2:
                if (!RoomNumberCheck(c, roomNumber))
                {
                    break;
                }

                {
                    string gid = c["gid"].ToString();
                    bool canuse = c.ValueOrDefault<bool>("canuse", false);
                    int count = c.ValueOrDefault<int>("count", 0);
                    ReceiveTimeBank2(gid, canuse, count);
                }
                break;
            case PCProtocol.PC_ROOM_TIME_BANK2_USE:
                if (!RoomNumberCheck(c, roomNumber))
                {
                    break;
                }

                {
                    string gid = c["gid"].ToString();
                    bool canuse = c.ValueOrDefault<bool>("canuse", false);
                    float time = c.ValueOrDefault<float>("time", 0);

                    FindPlayerWithGid(gid).MyTurn((float)time, true);
                }
                break;

            case PCProtocol.PC_ROOM_PLAYER_INFO_CHANGE:
                if (!RoomNumberCheck(c, roomNumber))
                {
                    break;
                }
                if (c.ValueOrDefault("ecode", 0) == 0)
                {
                    string gid = c.ValueOrDefault("gid", "");
                    long gc = c.ValueOrDefault<long>("gc", 0);
                    long old_gc = c.ValueOrDefault<long>("old_gc", 0);
                    long gc_gap = gc - old_gc;
                    FindPlayerWithGid(gid).Chip = gc;
                    RoomStatus room = InfoManager.Instance.GetRoom(roomNumber);
                    RoomUserData data = room.userList.Find(player => player.gid == gid);
                    if (data == null)
                    {
                        RoomReenter();
                    }
                    data.gc = gc;

                    if (myPlayer.CompareGid(gid))
                        break;

                    PLAYER_INFO_CHANGE reason = (PLAYER_INFO_CHANGE)c.ValueOrDefault("reason", 0);
                    string notice = LocalizeManager.GetLocalString(reason.ToString());
                    try
                    {
                        notice = notice.Replace("{hand}", "{0}");
                        notice = notice.Replace("{time}", "{0}");
                        notice = notice.Replace("{passive_rake}", "{1}");
                        notice = notice.Replace("{stack_removal}", "{1}");

                        if (reason == PLAYER_INFO_CHANGE.player_info_change_passive_rake_hands)
                        {
                            notice = string.Format(notice, room.pr2, room.pr1);
                        }
                        else if (reason == PLAYER_INFO_CHANGE.player_info_change_passive_rake_time)
                        {
                            notice = string.Format(notice, room.pr4, room.pr3);
                        }
                        else if (
                            reason == PLAYER_INFO_CHANGE.player_info_change_stack_removal_hands
                        )
                        {
                            notice = string.Format(
                                notice,
                                room.stack_removal_option1,
                                room.stack_removal_bb1
                            );
                        }
                        else if (reason == PLAYER_INFO_CHANGE.player_info_change_stack_removal_time)
                        {
                            notice = string.Format(
                                notice,
                                room.stack_removal_option2,
                                room.stack_removal_bb1
                            );
                        }
                        else if (reason == PLAYER_INFO_CHANGE.player_info_change_surplus_bet_chip)
                        {
                            break;
                        }
                    }
                    catch { }
                    NormalMessage.instance.AddSimpleMessage(notice);
                }
                break;
            case PCProtocol.PC_TNMT_DEAL:
                if (!RoomNumberCheck(c, roomNumber))
                {
                    break;
                }   
                SetDealPlayers(c);
                break;
            case PCProtocol.PC_TNMT_DEAL_AGREE:
            
                if (!RoomNumberCheck(c, roomNumber))
                {
                    break;
                }
                SetDealAgreePanel(c);
                break;
            case PCProtocol.PC_TNMT_DEAL_AGREE_START:
                if (!RoomNumberCheck(c, roomNumber))
                {
                    break;
                }
                SetDealAgreePanelStart(c);
                
                GameEnd();
                break;
        }
    }

    private void SetDealPlayers(JObject c)
    {
        JArray players = c["agreed_players"] as JArray;
        for(int i = 0; i < players.Count; i++)
        {
            JObject p = players[i] as JObject;
            string gid = p["gid"].ToString();
            bool agree = p["agree"].ToObject<bool>();
            Player player = FindPlayerWithGid(gid);

            if(player != null)
            {
                player.IsDeal = agree;
            }
            if(gid == MyStatus.gid)
            {
                dealPanel.OnDeal(agree);
            }
        }

    }

    private void ResetDealPlayers()
    {
        foreach(var player in players)
        {
            player.IsDeal = false;
        }
    }

    private void SetDealAgreePanel(JObject c)
    {
       
        bool agree = c.ValueOrDefault<bool>("agree", true);
        bool allAgreed = c.ValueOrDefault<bool>("allAgreed", false);
        if(!agree)
        {

            dealFailPanel.SetActive(true);  
            dealAgreePanel.gameObject.SetActive(false);
            //ResetDealPlayers();
        }
        else if(allAgreed)
        {
            dealSuccessPanel.SetActive(true);
            dealAgreePanel.gameObject.SetActive(false);
        }
        else
        {
            if(FindPlayerWithGid(MyStatus.gid) != null)
            {
                dealAgreePanel.SetDealAgreePanel(c, roomNumber);
                dealAgreePanel.gameObject.SetActive(true);
                dealPanel.gameObject.SetActive(false);
                dealFailPanel.gameObject.SetActive(false);
                dealSuccessPanel.gameObject.SetActive(false);
            }

        }

    }

    private void SetDealAgreePanelStart(JObject c)
    {
        SetDealAgreePanel(c);
    }


    protected override void StatusSet()
    {
        GameEnd();
        base.StatusSet();
        var rs = InfoManager.Instance.GetRoom(roomNumber);
        roomCommand = (ROOM_COMMAND)rs.room_command;

        if (playingGame)
        {
            SetBoss(rs.boss_seat);
            POKER_FLOW flow = (POKER_FLOW)rs.flow;
            try
            {
                if (flow == POKER_FLOW.bet || flow == POKER_FLOW.bet_start)
                {
                    int turn = rs.who_turn_seat;
                    int nextTime = rs.turn_nexttime;
                    PlayerTurnSetting(turn, nextTime, rs);
                }
            }
            catch
            {
                Console.Log("is not enable : who_turn_seat");
            }
            RoomInfoTextSet();

            JArray players = rs.playerList;
            POKER_BETTYPE bettype = POKER_BETTYPE.none;
            if (players != null)
            {
                for (int i = 0; i < players.Count; i++)
                {
                    string gid = players[i]["gid"].ToString();

                    ReceiveTimeBank(
                        gid,
                        (TIME_BANK_MODE)(int)players[i]["time_bank"]["mode"],
                        (TIME_BANK_COMMAND)(int)players[i]["time_bank"]["command"],
                        (double)players[i]["time_bank"]["time"]
                    );

                    if (gid == MyStatus.gid)
                    {
                        bettype = (POKER_BETTYPE)(
                            (players[i] as JObject).ValueOrDefault("last_bettype", 0)
                        );
                    }
                }
            }
        }
        else { }

        //Packet readyPacket = new Packet(CPProtocol.CP_ROOM_USER_STATUS_SEAT);
        //readyPacket.Add("gtn", roomNumber);
        //readyPacket.Add("status_seat", (int)ROOM_USER_STATUS_SEAT.standing);
        //WebSocketManager.defaultCli.Send(readyPacket);
        //MoveTableSet();

        Debug.Log("홀덤 플레이어 매니저에서 홀덤 상태 체크함");
    }

    private void ReceiveTimeBank(
        string gid,
        TIME_BANK_MODE mode,
        TIME_BANK_COMMAND command,
        double time
    )
    {
        if (gid == MyStatus.gid)
        {
            timebank_mode = mode;
            bool toggle_on =
                mode == TIME_BANK_MODE.alwaysbank
                || mode == TIME_BANK_MODE.excludingbank
                || mode == TIME_BANK_MODE.includingbank
                || mode == TIME_BANK_MODE.on;
            timebank_toggle.isOn = toggle_on;

            timebank_text_OffLabel.text = "+" + time.ToString();
            timebank_text_OnLabel.text = "+" + time.ToString();

            bool playing = false;
            if (mode != TIME_BANK_MODE.none)
            {
                if (command == TIME_BANK_COMMAND.popup)
                {
                    timebank_panel.SetActive(true);
                }
                else if (command == TIME_BANK_COMMAND.count_down)
                {
                    playing = true;
                    timebank_panel.SetActive(true);
                    FindPlayerWithGid(gid).MyTurn((float)time, true);
                }
            }

            timebank_timers.ForEach(t => t.SetTimer(playing, time));
        }
        else
        {
            if (mode != TIME_BANK_MODE.none && command == TIME_BANK_COMMAND.count_down)
            {
                FindPlayerWithGid(gid).MyTurn((float)time, true);
            }
        }
    }

    private void RoomReenter()
    {
        InfoManager.Instance.RoomReenter(roomNumber);
    }

    private void ReceiveTimeBank2(string gid, bool canuse, int count)
    {
        if (gid == MyStatus.gid)
        {
            if (canuse && count > 0)
            {
                RoomStatus room = InfoManager.Instance.GetRoom(roomNumber);

                string s =
                    "+"
                    + room.tb2_time
                    + System.Environment.NewLine
                    + "( "
                    + count.ToString()
                    + " )";
                timebank2_text_OffLabel.text = s;
                timebank2_text_OnLabel.text = s;
                timebank2_panel.SetActive(true);
            }
        }
    }

    public void OnClickTimeBankButton()
    {
        if (timebank_mode == TIME_BANK_MODE.off || timebank_mode == TIME_BANK_MODE.neverbank)
        {
            Packet p = new Packet(CPProtocol.CP_ROOM_TIME_BANK);
            int mode = (int)TIME_BANK_MODE.on;
            p.Add("gtn", roomNumber);
            p.Add("mode", mode);
            WebSocketManager.defaultCli.Send(p);
        }
    }

    public void OnClickTimeBank2Button()
    {
        Packet p = new Packet(CPProtocol.CP_ROOM_TIME_BANK2_USE);
        p.Add("gtn", roomNumber);
        WebSocketManager.defaultCli.Send(p);

        timebank2_panel.SetActive(false);
    }

    protected override void MyTurnProgress(float progress)
    {
        base.MyTurnProgress(progress);
        if (
            progress <= timebank_active_progress
            && timebank_prev_progress > timebank_active_progress
        )
        {
            RoomStatus room = InfoManager.Instance.GetRoom(roomNumber);

            if (room.timebank)
            {
                Packet p = new Packet(CPProtocol.CP_ROOM_TIME_BANK);
                int mode = (int)timebank_mode;
                if (mode == 0)
                {
                    mode = int.Parse(PlayerPrefs.GetString("auto_timebank", "3"));
                }
                p.Add("gtn", roomNumber);
                p.Add("mode", mode);
                WebSocketManager.defaultCli.Send(p);
            }

            if (room.tb2)
            {
                Packet p = new Packet(CPProtocol.CP_ROOM_TIME_BANK2);
                p.Add("gtn", roomNumber);
                WebSocketManager.defaultCli.Send(p);
            }
        }
        timebank_prev_progress = progress;
    }

    public override void PlayerTurnSetting(int seat, float time, RoomStatus rs = null)
    {
        //base.PlayerTurnSetting(seat, time, c);
        List<Player> all = GetPlayingGamer();
        Console.SpecialLog("Playing Gamer Count : " + all.Count);
        JArray players = null;
        POKER_FLOW flow = POKER_FLOW.none;
        if (rs != null)
        {
            players = rs.playerList;
            flow = (POKER_FLOW)rs.flow;
        }

        for (int i = 0; i < all.Count; i++)
        {
            int pokerBetType = 0;
            if (rs != null)
            {
                for (int j = 0; j < players.Count; j++)
                {
                    if (players[j]["gid"].ToString() == all[i].gid)
                    {
                        var playerJson = players[j] as JObject;
                        pokerBetType = (int)playerJson["last_bettype"];

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
                        && (flow == POKER_FLOW.bet || flow == POKER_FLOW.bet_start)
                    )
                    || rs == null
                )
                && all[i].Coffee_break <= 0
            )
            {
                if (all[i].CompareGid(MyStatus.gid))
                    all[i].MyTurn(time, false, MyTurnProgress);
                else
                    all[i].MyTurn(time);
            }
            else if (all[i].myTurn)
            {
                all[i].NotMyTurn();
            }
        }
    }

    protected override void GameEnd()
    {
        base.GameEnd();
        Player player = FindPlayerWithGid(MyStatus.gid);
        RoomStatus info = InfoManager.Instance.GetRoom(roomNumber);
        if (player && info != null && info.tn > 0)
        {
            if (!buyinPanel.reserved)
            {
                bool autoRebuy = bool.Parse(PlayerPrefs.GetString("autorebuy", false.ToString()));
                if (autoRebuy)
                {
                    JArray autoRebuyChip = JArray.Parse(
                        PlayerPrefs.GetString(
                            "autorebuy_chip",
                            (new JArray(new int[2] { 0, 100 })).ToString()
                        )
                    );
                    long min = (long)
                        Mathf.Ceil((info.bil - info.bi) * ((int)autoRebuyChip[0] / 100f) + info.bi);
                    if (player.Chip <= min)
                    {
                        long topup = (long)
                            Mathf.Ceil(
                                (info.bil - info.bi) * ((int)autoRebuyChip[1] / 100f) + info.bi
                            );
                        long amount = topup - player.Chip;
                        BuyIn(amount);
                    }
                }
                else if (player.Chip == 0 && info.tn == 0)
                {
                    OpenBuyInPopup();
                }
            }
        }
    }

    public void OnClickDealButton()
    {
        RoomStatus room = InfoManager.Instance.GetRoom(roomNumber);
        if (room == null)
            return;

        Packet p = new Packet(CPProtocol.CP_TNMT_DEAL);
        p.Add("tn", room.tn);
        p.Add("gtn", room.gtn);
        p.Add("agree", true);
        WebSocketManager.defaultCli.Send(p);
    }
    public void OnClickDealCancelButton()
    {
        RoomStatus room = InfoManager.Instance.GetRoom(roomNumber);
        if (room == null)
            return;
        Packet p = new Packet(CPProtocol.CP_TNMT_DEAL);
        p.Add("tn", room.tn);
        p.Add("gtn", room.gtn);
        p.Add("agree", false);
        WebSocketManager.defaultCli.Send(p);
    }

    public void OnClickDealAgreeButton()
    {
        RoomStatus room = InfoManager.Instance.GetRoom(roomNumber);
        if (room == null)
            return;


        Packet p = new Packet(CPProtocol.CP_TNMT_DEAL_AGREE);
        p.Add("tn", room.tn);
        p.Add("gtn", room.gtn);
        p.Add("agree", true);
        WebSocketManager.defaultCli.Send(p);
    }

    public override void MoveTableSet()
    {
        base.MoveTableSet();
        // var list = RoomOptions.rules[gameType + "_list"].ToObject<List<JObject>>();

        //for (int i = 0; i < list.Count; i++)
        //{
        //    //Console.SpecialLog(list[i].ToString());
        //    var sb = list[i]["sm"].ToObject<long>();
        //    if (sb == this.blind)
        //    {
        //        if (i != 0)
        //        {
        //            beforeBl = list[i - 1]["sm"].ToObject<long>();
        //            beforeBlText.text = string.Format("blind : {0}", MoneyToString.Converting(beforeBl));
        //            beforeBlText.enabled = true;
        //            beforeTableMoveButton.interactable = true;
        //        }
        //        else
        //        {
        //            beforeBl = 0;
        //            beforeBlText.enabled = false;
        //            beforeTableMoveButton.interactable = false;
        //            //beforeObj.SetActive(false);
        //        }
        //        if (i < list.Count - 1)
        //        {
        //            nextBl = list[i + 1]["sm"].ToObject<long>();
        //            nextBlText.text = string.Format("blind : {0}", MoneyToString.Converting(nextBl));
        //            nextBlText.enabled = true;
        //            nextTableMoveButton.interactable = true;
        //        }
        //        else
        //        {
        //            nextBl = 0;
        //            nextBlText.enabled = false;
        //            nextTableMoveButton.interactable = false;
        //            //nextObj.SetActive(false);
        //        }
        //        break;
        //    }
        //}
    }

    //private void AutoRebuy()
    //{
    //    Packet p = new Packet((int)CPProtocol.CP_ROOM_REBUY);
    //    p.Add("tn", 0);
    //    p.Add("gtn",roomNumber);
    //    p.Add("multiple", 1);
    //    WebSocketManager.defaultCli.Send(p);
    //}

    private void ReceiveEmo(JObject c)
    {
        string gid = c["to_gid"].ToObject<string>();
        string msg = c["msg"].ToObject<string>();
        string attacked_gid = c.ValueOrDefault("attacked_gid", "");

        Player player = FindPlayerWithGid(gid);
        EmoticonInfo info = emoSets.Find(msg);

        if (info == null)
        {
            return;
        }
        if (player == null)
        {
            return;
        }
        if (!string.IsNullOrEmpty(info.soundEnumKey))
        {
            var soundEnum = (Sound_TableEnum)(-1);
            try
            {
                soundEnum = (Sound_TableEnum)Enum.Parse(typeof(Sound_TableEnum), info.soundEnumKey);
            }
            catch (Exception e)
            {
                Debug.LogError($"enum값에 없는 키값 : {info.soundEnumKey}");
            }
            if ((int)soundEnum != -1)
            {
                SoundManager.Instance.PlayEffectSound(soundEnum);
            }
        }

        if (!info.moveable)
        {
            player.PlayEmo(info.texture, info.playTime);
            //SoundManager.Instance.PlayEffectSound(Sound_TableEnum.SFX_USE_IMOTICON_01);
        }
        else
        {
            if (attacked_gid == "")
                return;
            Player target = FindPlayerWithGid(attacked_gid);

            StartCoroutine(StartActiveEmo_(player, target, info));

            //움직이는 이모티콘 재생
        }
    }

    public IEnumerator StartActiveEmo_(Player player, Player target, EmoticonInfo info)
    {
        GameObject go = Instantiate(info.spine.gameObject, player.transform);
        go.transform.localPosition = new Vector3(0, 64, 0);
        go.transform.localScale = new Vector3(1, 1, 1);

        SkeletonAnimation spine = go.GetComponent<SkeletonAnimation>();

        spine.state.SetAnimation(0, info.startAnim, true);

        yield return new WaitForSecondsRealtime(0.3f);

        spine.state.SetAnimation(0, info.moveAnim, true);

        // go.transform.position = target.transform.position;

        Vector3[] wayPoints = new Vector3[2];
        wayPoints.SetValue(go.transform.position, 0);
        wayPoints.SetValue(target.transform.position, 1);

        go.transform.DOPath(wayPoints, 0.3f, PathType.Linear)
            .SetLookAt(new Vector3(0, 0, 0))
            .SetEase(Ease.OutCirc);

        yield return new WaitForSecondsRealtime(0.3f);

        spine.state.SetAnimation(0, info.endAnim, false);

        yield return new WaitForSecondsRealtime(1.0f);

        GameObject.Destroy(go);

        yield break;
    }

    public void SendEmo(int emo)
    {
        // TODO: 자리에 앉지 않으면 클릭이 소용없어야 한다.

        var p = new Packet((int)CPProtocol.CP_ROOM_CHAT);
        p.Add("gtn", roomNumber);
        p.Add("type", "emo");
        p.Add("to_gid", MyStatus.gid);
        p.Add("msg", ((Emoticon)emo).ToString());
        WebSocketManager.defaultCli.Send(p);
    }

    public void TnmtResult(JObject c)
    {
        int tn = c.ValueOrDefault("tn", 0);
        int gtn = c.ValueOrDefault("gtn", 0);
        long reward = c.ValueOrDefault("reward", 0);
        long zc = c.ValueOrDefault("zc", 0);
        long dc = c.ValueOrDefault("dc", 0);
        long cc = c.ValueOrDefault("cc", 0);
        int ticket = c.ValueOrDefault("ticket", 0);
        int ticket_amount = c.ValueOrDefault("ticket_amount", 0);
        long kp = c.ValueOrDefault<long>("kp", 0);
        int rank = c.ValueOrDefault("rank", 0);
        string my = c.ValueOrDefault("my", string.Empty);
        string enemy = c.ValueOrDefault("enemy", string.Empty);

        if (my == myPlayer.gid)
        {
            resultPanel?.SetRewardPanel(rank, reward, enemy, kp);
        }
        Debug.Log(myPlayer.gid);
    }

    public void RankPopUpButton()
    {
        var data = InfoManager.Instance.GetRoom(roomNumber);
        if (data == null)
            return;
        var tn = data.tn;
        Packet p = new Packet((int)CPProtocol.CP_TNMT_RANKING);

        p.Add("tn", tn);
        WebSocketManager.defaultCli.Send(p.ToJson());
    }

    public void RankingPopup(JObject c)
    {
        SetRankSlot(c);
        SetRewardSlot();
        rankPopup.Popup();
    }

    public void OpenBuyInPopup()
    {
        //if (myPlayer.Chip == 0 && (roomCommand == ROOM_COMMAND.none || roomCommand == ROOM_COMMAND.calc_dividend
        //    || roomCommand == ROOM_COMMAND.waiting_player ||myPlayer.status ==ROOM_USER_STATUS.die || myPlayer.status == ROOM_USER_STATUS.nothing))
        RoomStatus info = InfoManager.Instance.GetRoom(roomNumber);

        if (info != null)
        {
            if (myPlayer.Chip > 0)
            {
                NormalMessage.instance.AddSimpleMessage("cannot_buyin_already_chips");
            }
            else if (
                IsPlayingGamer(MyStatus.gid)
                && (roomCommand >= ROOM_COMMAND.start && roomCommand <= ROOM_COMMAND.playing)
            )
            {
                NormalMessage.instance.AddSimpleMessage("cannot_buyin_in_playing");
            }
            else
            {
                buyinPanel.InitPanel(roomNumber);
                buyinPanel.gameObject.SetActive(true);
            }
        }
    }

    public void BuyIn(long amount)
    {
        if (amount <= 0)
            return;
        RoomStatus info = InfoManager.Instance.GetRoom(roomNumber);
        long chip = 0;
        var mem = Cafe.instance.GetCafeList(info.cafeIdx).cafeMembers[0];
        switch (info.chip_type)
        {
            case CHIP_TYPE.cc:
                chip = mem.cc;
                break;
            case CHIP_TYPE.dc:
                chip = mem.dc;
                break;
            case CHIP_TYPE.zc:
                chip = mem.zc;
                break;
        }

        if ((myPlayer.Chip < info.bil) && (chip >= info.bi))
        {
            buyinPanel.InitPanel(roomNumber);
            buyinPanel.SetAmount(amount);
            buyinPanel.OnClickBuyInButton();
        }
    }

    public void AutoBuyIn()
    {
        RoomStatus info = InfoManager.Instance.GetRoom(roomNumber);
        long dc = Cafe.instance.GetCafeList(info.cafeIdx).cafeMembers[0].dc;
        if ((myPlayer.Chip < info.bil) && (dc >= info.bi))
        {
            buyinPanel.InitPanel(roomNumber);
            buyinPanel.OnClickBuyInButton();
        }
    }

    public void SetRankSlot(JObject c)
    {
        JObject[] dataArr = JsonDataParser.Parse<JObject[]>(c["data"]["ranking"]);
        for (int i = 0; i < dataArr.Length; i++)
        {
            RankSlot go;
            if (rankSlotList == null)
            {
                rankSlotList = new List<RankSlot>();
            }
            if (rankSlotList.Count > i)
            {
                go = rankSlotList[i];
            }
            else
            {
                go = Instantiate(rankPopup.rankPrefab, rankPopup.rankParent)
                    .GetComponent<RankSlot>();
                rankSlotList.Add(go);
            }
            JObject data = dataArr[i];
            long chip = (long)data["chip"];
            go.rankText.text = string.Format("{0}위", data["rank"]);
            go.nickNameText.text = string.Format("{0}", data["name"]);
            lastMoneyTextValue.Set(go.nickNameText, chip);
            go.chipText.text = string.Format("{0}", bettingManager.GetMoneyString(chip));
            if (MyStatus.nick == (string)data["name"])
            {
                MyRank.nickNameText.text = string.Format("{0}", data["name"]);
                MyRank.rankText.text = string.Format("{0}위", data["rank"]);
                lastMoneyTextValue.Set(MyRank.nickNameText, chip);
                MyRank.chipText.text = string.Format("{0}", bettingManager.GetMoneyString(chip));
                var follow = MyRank.GetComponent<FollowMyRank>();
                follow.SetMyRnakTrans(go.transform);
            }
        }

        while (rankSlotList.Count > dataArr.Length)
        {
            var go = rankSlotList[dataArr.Length];
            rankSlotList.RemoveAt(dataArr.Length);
            Destroy(go);
        }
    }

    private void SetRewardSlot()
    {
        var room = InfoManager.Instance.GetRoom(roomNumber);
        var tnmtData = InfoManager.Instance.GetTournamentInfo(room.tn);
        var rwList = HoldemRewardTable.GetRewardTable(tnmtData.countAllUser, tnmtData.totalPrize);
        if (rewardList == null)
        {
            rewardList = new List<RewardSlot>();
        }
        for (int i = 0; i < rwList.Count; i++)
        {
            RewardSlot go;
            var rw = rwList[i];
            if (rewardList.Count > i)
            {
                go = rewardList[i];
            }
            else
            {
                go = Instantiate(rankPopup.rewardPrefab, rankPopup.rewardParent)
                    .GetComponent<RewardSlot>();
                rewardList.Add(go);
            }
            go.SetSlot(i, rwList[i]);
        }
        while (rewardList.Count > rwList.Count)
        {
            var go = rewardList[rwList.Count];
            rewardList.RemoveAt(rwList.Count);
            Destroy(go);
        }
    }

    public override void OnClickSeatInButton(Button seatInButton)
    {
        base.OnClickSeatInButton(seatInButton);
        StartCoroutine(TrySeatIn());
    }

    private IEnumerator TrySeatIn()
    {
        WaitForPCProtocol wait = new WaitForPCProtocol(PCProtocol.PC_ROOM_USER_STATUS);
        yield return wait;
        bool autoBuyin = bool.Parse(PlayerPrefs.GetString("autobuyin", false.ToString()));
        if (autoBuyin)
        {
            AutoBuyIn();
        }
        else
        {
            OpenBuyInPopup();
        }
    }

    public override void ResetPlayers()
    {
        HoldemPlayer hmp = (HoldemPlayer)myPlayer;
        hmp.isUTG = false;

        for (int i = 0; i < players.Count; i++)
        {
            HoldemPlayer hp = (HoldemPlayer)players[i];
            hp.isUTG = false;
        }
    }

    public void SetBoss(int bossSeat)
    {
        if (dealerButtonsManager)
        {
            dealerButtonsManager.SetBoss(bossSeat);
        }
        else
        {
            List<Player> list = GetPlayingGamer();

            for (int i = 0; i < list.Count; i++)
            {
                list[i].SetBoss(list[i].CompareSeat(bossSeat));
            }
        }
    }

    public void SetPlayUsers() { }

    // Update is called once per frame
    void Update() { }

    private void LateUpdate() { }
}

[Serializable]
public class EmoSet
{
    public Emoticon e;
    public Sprite sp;
}
