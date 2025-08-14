using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.UI;

public class HoldemTableManager : TableManager
{
    public PlayerManager playerManager;
    public BettingManager bettingManager;
    public ChatManager chatManager;
    public CardManager cardManager;

    [SerializeField]
    private GameObject countDownObj;

    [SerializeField]
    private Text countdownTitle;

    [SerializeField]
    private Text countdownText;
    public List<Variation> gameTypeVariation = new List<Variation>();

    [SerializeField]
    private List<GameObject> tournamentDisableObjects = new List<GameObject>();
    private Coroutine countdownCoroutine;

    [Header("NEXT BLIND")]
    [SerializeField]
    private Text nextTimeText;
    private Coroutine nextBlindCo;
    private Coroutine blindUpCo;

    [Header("BLIND_UP")]
    [SerializeField]
    private List<Variation> blindUpVariations;

    [SerializeField]
    private GameObject blindUpObj;

    [SerializeField]
    private LocalText blindUpText;

    [SerializeField]
    CustomUIOpener customUIOpener;

    [Header("TNMT")]
    [SerializeField]
    private LocalText next_blind;

    [SerializeField]
    private LocalText prize;

    [SerializeField]
    private LocalText late_reg;

    [SerializeField]
    private LocalText my_rank;

    [SerializeField]
    private GameObject tnmtTab;

    [SerializeField]
    private Text tnmtTitleText;

    [SerializeField]
    private Text tnmtTableAndUserCountText;
    private string titleString;
    private int level;
    private int userCount = 0;
    private int tableCount = 0;

    [SerializeField]
    private UserState userState;
    private bool canDeal = false;
    public bool CanDeal
    {
        get { return canDeal; }
        set
        {
            canDeal = value;
            if (dealButton)
                dealButton.SetActive(canDeal);
        }
    }
    private BlindData next = null;

    [SerializeField]
    private GameObject dealButton;

    protected override void Awake()
    {
        base.Awake();
        InfoManager.Instance.TnmtUpdateAddLisener(TnmtTableCount);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        InfoManager.Instance.TnmtUpdateRemoveLisener(TnmtTableCount);
    }

    public override void SetRoomNumber(long gtn)
    {
        base.SetRoomNumber(gtn);
        playerManager.roomNumber = gtn;
        bettingManager.roomNumber = gtn;
        cardManager.roomNumber = gtn;
        if (chatManager)
            chatManager.roomNumber = gtn;
        CheckStartTimeCountDown(gtn);
        RequestTnmtRanking();
        TnmtDataSet();
    }

    public void TnmtDataSet()
    {
        var rd = InfoManager.Instance.GetRoom(roomNumber);
        if (rd.tn > 0)
        {
            var blind = rd.info.CastOrEmpty<JObject>("blindUp");
            var tnmt = InfoManager.Instance.GetTournamentInfo(rd.tn);
            var info = tnmt.info;
            if (blind != null && tnmt != null)
            {
                titleString = info.ValueOrDefault("t_title", string.Empty);
                var live = info.ValueOrDefault("live", new JObject());
                var delay = live.ValueOrDefault<double>("blindUpDelay", 0);

                level = blind.ValueOrDefault("level", 0) + 1;
                TnmtTitleSet();
                TnmtTableCount();

                var structureIdx = info.ValueOrDefault("t_blind_up_structure", 0);
                var structuredata = TableDataManager
                    .blindTables[structureIdx]
                    .CastOrEmpty<JArray>("blind_up");
                var start_time_string = info.ValueOrDefault("t_start_time", string.Empty);
                DateTime next_time = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                    .AddMilliseconds(blind.ValueOrDefault("next_time", (double)0))
                    .ToLocalTime();
                if (!string.IsNullOrEmpty(start_time_string))
                {
                    DateTime start_time = DateTimeParser.Parse(start_time_string).ToLocalTime();
                    double seconds = delay;

                    var t_blind_up_duration_time = info.ValueOrDefault(
                        "t_blind_up_duration_time",
                        0
                    );
                    var t_blind_up_break_time = info.ValueOrDefault("t_blind_up_break_time", 0);
                    bool durationTime = t_blind_up_duration_time != 0;
                    bool breakTime = t_blind_up_break_time != 0;
                    for (int i = 0; i < level; ++i)
                    {
                        var data = structuredata[i] as JObject;
                        var nextSeconds = data.ValueOrDefault<int>("next_second", 0);
                        if (data.ValueOrDefault("is_breaktime", false))
                        {
                            seconds += breakTime ? t_blind_up_break_time : nextSeconds;
                        }
                        else
                        {
                            seconds += durationTime ? t_blind_up_duration_time : nextSeconds;
                        }
                    }
                    next_time = start_time.AddSeconds(seconds);
                }

                JObject nextData = null;
                if (level < structuredata.Count)
                {
                    nextData = structuredata[level] as JObject;
                }
                //for (int i = 0; i < structuredata.Count; ++i)
                //{
                //    var id = structuredata[i].Value<int>("id");
                //    if (id == level + 1)
                //    {
                //        nextData = structuredata[i] as JObject;
                //        break;
                //    }
                //}
                if (nextData != null)
                {
                    var next_sb = nextData.ValueOrDefault("small", 0);
                    var next_bb = nextData.ValueOrDefault("big", 0);
                    var next_ante = nextData.ValueOrDefault("ante", 0);
                    var next_break_time = nextData.ValueOrDefault("is_breaktime", false);
                    next = new BlindData(
                        next_time,
                        next_sb,
                        next_bb,
                        next_ante,
                        next_break_time
                    );
                    SetNext();
                }
                var isBreak = blind.ValueOrDefault("isBreak", false);
                if (isBreak)
                {
                    var sb = blind.ValueOrDefault("small", 0);
                    var bb = blind.ValueOrDefault("big", 0);
                    var ante = blind.ValueOrDefault("ante", 0);
                    var blindData = new BlindData(next_time, sb, bb, ante, isBreak);
                    BlindUpPopup(blindData);
                }
            }
        }
    }

    private void TnmtTableCount()
    {
        var rd = InfoManager.Instance.GetRoom(roomNumber);
        if (rd != null && rd.tn > 0)
        {
            var tnmt = InfoManager.Instance.GetTournamentInfo(rd.tn);
            var live = tnmt.info.CastOrEmpty<JObject>("live");
            if (live != null)
            {
                var tables = live.ValueOrDefault("tables", new int[0]);
                tableCount = tables.Length;
                userCount = live.ValueOrDefault("countPlayable", 0);
            }
            SetUserAndTableCount();
        }
    }

    private void TnmtTitleSet()
    {
        if (tnmtTitleText)
            tnmtTitleText.text = $"{titleString} (레벨 {level})";
    }

    public override void SetGameType(GAME_TYPE gameType)
    {
        base.SetGameType(gameType);
        playerManager.gameType = gameType;
        bettingManager.gameType = gameType;
        cardManager.gameType = gameType;

        for (int i = 0; i < gameTypeVariation.Count; i++)
        {
            gameTypeVariation[i].SetVariation(GetTableVariationString());
        }
    }

    private async void CheckStartTimeCountDown(long gtn)
    {
        var info = InfoManager.Instance.GetRoom(gtn);
        if (info.tn != 0)
        {
            tournamentDisableObjects.ForEach(obj => obj.SetActive(false));
            countdownTitle.text = LocalizeManager.GetLocalString("tnmt_start_time");
            TournamentInfo tnInfo = InfoManager.Instance.GetTournamentInfo(info.tn);
            if (tnInfo.startTime == null)
            {
                LoadingCircle.Instance.StartSpin(CPProtocol.CP_CAFE_INFO);
                Packet p = new Packet(CPProtocol.CP_CAFE_INFO);
                p.Add("cafeIdx", (int)info.info["cafeIdx"]);
                WebSocketManager.defaultCli.Send(p);

                await new WaitForPCProtocol(PCProtocol.PC_CAFE_INFO);
            }
            DateTime startTime = DateTimeParser.Parse(tnInfo.startTime).ToLocalTime();
            DateTime closeTime = DateTimeParser.Parse(tnInfo.closeTime).ToLocalTime();
            prize.SetLocalText("prize_text", MoneyToString.Converting(tnInfo.totalPrize));
            late_reg.SetLocalText("late_reg", closeTime.ToString("MM'/'dd HH:mm"));
            if (countdownCoroutine != null)
            {
                StopCoroutine(countdownCoroutine);
            }
            countdownCoroutine = StartCoroutine(CountDownCo(startTime));
            if (tnmtTab)
                tnmtTab.SetActive(true);
            if (customUIOpener)
                (customUIOpener as CustomUIOpenerLobby).SetTn(info.tn);
        }
        else
        {
            if (countDownObj)
                countDownObj.SetActive(false);
            if (tnmtTab)
                tnmtTab.SetActive(false);
        }
    }

    private void RequestTnmtRanking()
    {
        var info = InfoManager.Instance.GetRoom(roomNumber);

        if (info != null && info.tn != 0)
        {
            Packet p = new Packet(CPProtocol.CP_TNMT_RANKING);
            p.Add("tn", info.tn);
            WebSocketManager.defaultCli.Send(p);
        }
    }

    private IEnumerator CountDownCo(DateTime targetTime)
    {
        if (countDownObj)
            countDownObj.SetActive(true);
        var wait = new WaitForSeconds(0.1f);
        DateTime now = DateTime.Now;

        while (now < targetTime)
        {
            now = DateTime.Now;
            countdownText.text = "[" + TimeToString.SpanToString(targetTime - now) + "]";
            yield return wait;
        }

        if (countDownObj)
            countDownObj.SetActive(false);
    }

    public void TournamentResultOkButton()
    {
        CustomSceneManager.LoadScene("Lobby");
    }

    protected override void ReceivePacket(Packet packet)
    {
        base.ReceivePacket(packet);
        JObject c = packet.c;
        switch ((PCProtocol)packet.p)
        {
            case PCProtocol.PC_TNMT_BLIND_UP:
                if (roomNumber == c.ValueOrDefault("gtn", 0))
                {
                    bool isBreak = c.ValueOrDefault("isBreak", false);

                    DateTime nextTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                        .AddMilliseconds(c.ValueOrDefault("next_time", (double)0))
                        .ToLocalTime();
                    long next_bb = c.ValueOrDefault("next_big", 0);
                    long next_sb = c.ValueOrDefault("next_small", 0);
                    long next_ante = c.ValueOrDefault("next_ante", 0);

                    long bb = c.ValueOrDefault("big", 0);
                    long sb = c.ValueOrDefault("small", 0);
                    long ante = c.ValueOrDefault("ante", 0);

                    var rd = InfoManager.Instance.GetRoom(roomNumber);
                    rd.sm = sb;
                    rd.bi = bb;
                    rd.an = ante;

                    var tnmt = InfoManager.Instance.GetTournamentInfo(rd.tn);
                    var structureIdx = tnmt.info.ValueOrDefault("t_blind_up_structure", 0);
                    var structuredata = TableDataManager
                        .blindTables[structureIdx]
                        .CastOrEmpty<JArray>("blind_up");

                    MoneyToString.OnChangeBB(roomNumber);

                    for (int i = 0; i < structuredata.Count; ++i)
                    {
                        var data = structuredata[i] as JObject;
                        var blind = data.ValueOrDefault("big", 0);
                        var br = data.ValueOrDefault("is_breaktime", false);
                        if (bb == blind && br == isBreak)
                        {
                            level = i + 1;
                            TnmtTitleSet();
                            break;
                        }
                    }

                    next = new BlindData(nextTime, next_sb, next_bb, next_ante);
                    var blindData = new BlindData(nextTime, sb, bb, ante, isBreak);
                    SetNext();
                    BlindUpPopup(blindData);
                }
                break;
            case PCProtocol.PC_TNMT_RANKING:
                var roomData = InfoManager.Instance.GetRoom(roomNumber);
                if (roomData != null)
                {
                    if (InfoManager.Instance.GetRoom(roomNumber).tn == c.ValueOrDefault("tn", 0))
                        my_rank.SetLocalText(
                            "my_rank_text",
                            c["my"]["rank"],
                            ((JArray)c["list"]).Count
                        );
                }

                break;
            case PCProtocol.PC_ROOM_COMMAND: //PC_ROOM_COMMAND
                if (c.ValueOrDefault("gtn", 0) != roomNumber)
                    break;
                ROOM_COMMAND roomCommand = JsonDataParser.Parse<ROOM_COMMAND>(c["command"]);
                if (roomCommand == ROOM_COMMAND.start) { }
                if (roomCommand == ROOM_COMMAND.end)
                {
                    RequestTnmtRanking();
                }
                break;
            case PCProtocol.PC_HOLDEM_STATUS:
            {
                if (c.ValueOrDefault("gtn", 0) != roomNumber)
                    break;
                TnmtDataSet();
                var imPlaying = playerManager.FindPlayerWithGid(MyStatus.gid);
                if (imPlaying)
                {
                    var canDeal = c.ValueOrDefault("tnmt_can_deal", false);
                    CanDeal = canDeal;
                }
                break;
            }
            case PCProtocol.PC_TNMT_CAN_DEAL:
            {
                if (c.ValueOrDefault("gtn", 0) != roomNumber)
                    break;
                var imPlaying = playerManager.FindPlayerWithGid(MyStatus.gid);
                if (imPlaying)
                {
                    CanDeal = true;
                }

                break;
            }
            case PCProtocol.PC_TNMT_DEAL_AGREE:
            {
                if (c.ValueOrDefault("gtn", 0) != roomNumber)
                    break;
                var imPlaying = playerManager.IsPlayingGamer(MyStatus.gid);
                if (imPlaying)
                {
                    var allAgreed = c.ValueOrDefault("allAgreed", false);
                    CanDeal = !allAgreed;
                }
                var blind_up_nexttime = c.ValueOrDefault<double>("blind_up_nexttime", 0);
                DateTime nextTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                        .AddMilliseconds(blind_up_nexttime)
                        .ToLocalTime();
                next.time = nextTime;
                SetNext();
                break;
            }
        }
    }

    private void SetUserAndTableCount()
    {
        if (tnmtTableAndUserCountText)
        {
            tnmtTableAndUserCountText.text = $"남은 인원:{userCount} 테이블:{tableCount}";
        }
    }

    private void BlindUpPopup(BlindData blindData)
    {
        if (blindUpCo != null)
        {
            StopCoroutine(blindUpCo);
        }
        blindUpCo = StartCoroutine(BlindUp(blindData));
    }

    private IEnumerator BlindUp(BlindData blindData)
    {
        blindUpObj.SetActive(true);
        var variationString = blindData.isBreak ? "break" : "blind_up";
        foreach (var vari in blindUpVariations)
        {
            vari.SetVariation(variationString);
        }
        string str = blindData.ante == 0 ? "blind_up" : "blind_up_ante";
        if (blindData.isBreak)
        {
            var wait = new WaitForSeconds(0.1f);
            DateTime now = DateTime.Now;

            while (now < blindData.time)
            {
                now = DateTime.Now;
                blindUpText.SetLocalText(
                    "break_time_count",
                    TimeToString.SpanToString(blindData.time - now)
                );
                yield return wait;
            }
        }
        else
        {
            blindUpText.SetLocalText(str, blindData.sb, blindData.bb, blindData.ante);
            yield return new WaitForSeconds(3f);
        }
        blindUpObj.SetActive(false);
    }

    private void SetNext()
    {
        if (next == null)
            return;
        string str = next.ante == 0 ? "next_blind" : "next_blind_anti";

        next_blind.SetLocalText(
            str,
            MoneyToString.Converting(next.sb),
            MoneyToString.Converting(next.bb),
            MoneyToString.Converting(next.ante)
        );
        if (nextBlindCo != null)
        {
            StopCoroutine(nextBlindCo);
        }
        nextBlindCo = StartCoroutine(NextCountCo(next.time));
    }

    private IEnumerator NextCountCo(DateTime time)
    {
        var wait = new WaitForSeconds(0.1f);
        DateTime now = DateTime.Now;

        while (now < time)
        {
            now = DateTime.Now;
            nextTimeText.text = "[" + TimeToString.SpanToString(time - now) + "]";
            yield return wait;
        }

        nextTimeText.text = string.Empty;
    }

    public void GetUserData(string gid, string nick)
    {
        RoomStatus rs = InfoManager.Instance.GetRoom(roomNumber);
        var cafeIdx = rs.cafeIdx;
        PublisherApiManager.Instance.RequestUserScore(
            (success, data) =>
            {
                JObject d = data.CastOrEmpty<JObject>("data");
                var rd = InfoManager.Instance.GetRoom(roomNumber);
                if (rd != null && rd.tn > 0)
                {
                    userState.SetState(UserState.Type.Tnmt, nick, d);
                }
                else
                {
                    userState.SetState(UserState.Type.Holdem, nick, d);
                }
                userState.SetGid(gid);
                userState.GetMyTagData();

                userState.gameObject.SetActive(true);
            },
            gid,
            (int)cafeIdx
        );
    }
}

public class BlindData
{
    public DateTime time;
    public long bb;
    public long sb;
    public long ante;
    public bool isBreak;

    public BlindData(
        DateTime dateTime,
        long next_sb,
        long next_bb,
        long next_ante,
        bool isBreak = false
    )
    {
        this.time = dateTime;
        this.sb = next_sb;
        this.bb = next_bb;
        this.ante = next_ante;
        this.isBreak = isBreak;
    }
}
