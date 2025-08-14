using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using BestHTTP.JSON;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngineInternal;

public class InfoManager
{
    public static GAME_TYPE enterGameType = GAME_TYPE.nlh;
    public static int[] bettingRate
    {
        get
        {
            int[] result = new int[4];
            result[0] = Int32.Parse(PlayerPrefs.GetString("betsizeafter_1", "33"));
            result[1] = Int32.Parse(PlayerPrefs.GetString("betsizeafter_2", "50"));
            result[2] = Int32.Parse(PlayerPrefs.GetString("betsizeafter_3", "75"));
            result[3] = 0; //Int32.Parse(PlayerPrefs.GetString("betsizeafter_4", "100"));
            return result;
        }
    }
    public static float[] bettingBB
    {
        get
        {
            float[] result = new float[4];
            result[0] = float.Parse(PlayerPrefs.GetString("betsizepreflop_1", "2"));
            result[1] = float.Parse(PlayerPrefs.GetString("betsizepreflop_2", "3"));
            result[2] = float.Parse(PlayerPrefs.GetString("betsizepreflop_3", "4"));
            result[3] = 0; //float.Parse(PlayerPrefs.GetString("betsizepreflop_4", "0"));
            return result;
        }
    }
    public static int enterRn;
    public static int enterHn;
    public static bool canChangeNick;
    private static InfoManager instance;
    public static InfoManager Instance
    {
        get
        {
            if (instance == null)
                Init();

            return instance;
        }
    }
    public RoomEnterEvent roomEnterEvent = new RoomEnterEvent();
    public JsonEvent roomLeaveEvent = new JsonEvent();
    public static Dictionary<int, JObject> itemData;
    private Dictionary<long, RoomStatus> roomStatusDic;
    private Dictionary<long, RoomStatus> hallStatusDic;
    private List<TournamentInfo> tournamentInfoList;
    private JArray zcCountArr = null;
    private Event zcCountUpdate = new Event();
    private Event tnmtUpdateEvent = new Event();
    private List<long> enterdRoomList = new List<long>();

    public bool IsEnteredRoom(long gtn)
    {
        var isEnterd = enterdRoomList.Contains(gtn);
        return isEnterd;
    }

    public void EnterdClear()
    {
        enterdRoomList.Clear();
    }

    public static StringEvent valueChange = new StringEvent();

    public static void AddValueChangeLisener(UnityAction<string> action)
    {
        valueChange.RemoveListener(action);
        valueChange.AddListener(action);
    }

    public static void RemoveValueChangeLisener(UnityAction<string> action)
    {
        valueChange.RemoveListener(action);
    }

    public JArray ZcCountArr
    {
        get
        {
            if (zcCountArr == null)
            {
                zcCountArr = new JArray();
            }
            return zcCountArr;
        }
        set
        {
            if (zcCountArr == null)
            {
                zcCountArr = value;
            }
            else
            {
                foreach (JObject obj in value)
                {
                    bool found = false;
                    for (int i = 0; i < zcCountArr.Count; ++i)
                    {
                        var fobj = zcCountArr[i] as JObject;

                        if (
                            fobj.ValueOrDefault("option_level", 0)
                            == obj.ValueOrDefault("option_level", 0)
                        )
                        {
                            found = true;
                            zcCountArr[i] = obj;
                            break;
                        }
                    }
                    if (!found)
                    {
                        zcCountArr.Add(obj);
                    }
                }
            }
            zcCountUpdate.Invoke();
        }
    }

    private static JArray myItems = null;

    public static JArray MyItems
    {
        get { return myItems; }
        set
        {
            myItems = value;
            valueChange.Invoke("myItems");
        }
    }

    public static JObject GetItem(int refIdx)
    {
        foreach (JObject item in myItems)
        {
            var idx = item.ValueOrDefault("ref_idx", 0);
            if (idx == refIdx)
            {
                return item;
            }
        }
        return null;
    }

    private static JArray tnmtHistory = null;
    public static JArray TnmtHistory
    {
        get
        {
            if (tnmtHistory == null)
            {
                tnmtHistory = new JArray();
            }
            return tnmtHistory;
        }
        set { tnmtHistory = value; }
    }

    private Event tnmtHistryEvent = new();

    public void TnmtHistoryChangeAddListener(UnityAction action)
    {
        tnmtHistryEvent.AddListener(action);
    }

    public void TnmtHistoryChangeRemoveListener(UnityAction action)
    {
        tnmtHistryEvent.RemoveListener(action);
        action.Invoke();
    }

    public async void GetMyItems()
    {
        var www = await PublisherApiManager.Instance.GetMyItemApi();
        if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.Log(www.error);
            ErrorMessageManager.Instance.AddNetworkError(0, www.error, www.downloadHandler.text);
        }
        else
        {
            JObject json = JObject.Parse(www.downloadHandler.text);

            if (www.responseCode == 200)
            {
                //callback(www.responseCode, json);
                MyItems = json.ValueOrDefault<JArray>("myItems", null);
            }
        }
    }

    public void ZcCountChangeAddLisener(UnityAction action)
    {
        zcCountUpdate.AddListener(action);
        action.Invoke();
    }

    public void ZcCountChangeRemoveLisener(UnityAction action)
    {
        zcCountUpdate.RemoveListener(action);
    }

    public void TnmtUpdateAddLisener(UnityAction action)
    {
        tnmtUpdateEvent.AddListener(action);
        action.Invoke();
    }

    public void TnmtUpdateRemoveLisener(UnityAction action)
    {
        tnmtUpdateEvent.RemoveListener(action);
    }

    public List<TournamentInfo> TournamentInfoList
    {
        get
        {
            if (tournamentInfoList == null)
            {
                tournamentInfoList = new List<TournamentInfo>();
            }
            return tournamentInfoList;
        }
    }
    public List<TournamentInfo> tnmtInfoList = new List<TournamentInfo>();
    public static JObject Membership { get; private set; }

    private List<RefQuest> refQuestList;

    public static void Init()
    {
        if (instance == null)
        {
            instance = new InfoManager();
            instance.roomStatusDic = new Dictionary<long, RoomStatus>();
            instance.tournamentInfoList = new List<TournamentInfo>();
            instance.refQuestList = new List<RefQuest>();
            RequestItemData();
            RequestGameConfig();
        }
        WebSocketManager.defaultCli.OnMessage -= instance.WebSocketOnMessage;
        WebSocketManager.defaultCli.OnMessage += instance.WebSocketOnMessage;
        instance.enterdRoomList.Clear();
    }

    private static void RequestItemData()
    {
        PublisherApiManager.Instance.GetItemListPubApi(GetItemCallback);
    }

    private static void RequestGameConfig()
    {
        PublisherApiManager.Instance.GetGameConfigPubApi(GetGameConfigCallback);
    }

    public static void GetItemCallback(long statusCode, JObject data)
    {
        if (statusCode == 200)
        {
            itemData = new Dictionary<int, JObject>();
            Console.Log(data.ToString());
            var items = data["refItems"].ToObject<JArray>();
            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                if (!itemData.ContainsKey(item["idx"].ToObject<int>()))
                {
                    itemData.Add(item["idx"].ToObject<int>(), item.ToObject<JObject>());
                }
                else
                {
                    Console.Log(string.Format("이미 추가된 아이템 입니다. {0}", item["idx"]));
                }
            }
        }
    }

    public static void GetGameConfigCallback(long statusCode, JObject data)
    {
        if (statusCode == 200)
        {
            Membership = data.CastOrEmpty<JObject>("membership");
            Console.Log(Membership.ToString());
        }
    }

    public RefQuest GetRefQuestWithDay(int day)
    {
        return refQuestList.Find(
            (data) =>
            {
                return data.loginCountContinued == day;
            }
        );
    }

    public delegate void MessageDelegate(string msg);
    public MessageDelegate OnMessage;

    private void WebSocketOnMessage(string msg)
    {
        JObject json = JObject.Parse(msg);
        // Debug.Log(json.ToString());
        Packet packet = new Packet((int)json["p"]);
        packet.c = json["c"] as JObject;

        //if(MissingUserList(packet))
        {
            ReceivePacket(packet);
            OnMessage?.Invoke(msg);
        }
    }

    private bool MissingUserList(Packet packet)
    {
        int p = packet.p;

        switch ((PCProtocol)p)
        {
            case PCProtocol.PC_ROOM_USER_LIST:

                if (DevOptionsManager.devOptions.mode == MODE.dev)
                {
                    var ran = UnityEngine.Random.Range(0f, 10f);
                    if (9 < ran)
                    {
                        return false;
                    }
                }
                break;
        }

        return true;
    }

    public void SetRefQuest(JObject data)
    {
        var result = data["result"] as JArray;
        refQuestList.Clear();
        for (int i = 0; i < result.Count; i++)
        {
            // if((int)data["type"]== 4)
            {
                var rd = result[i] as JObject;
                var go = new RefQuest();
                go.id = (int)rd["id"];
                go.name = (string)rd["name"];
                go.kind = (int)rd["kind"];
                go.loginCountContinued = (int)rd["require"]["login"]["loginCountContinued"];

                go.rewardChip = (long)rd["reward"]["silver"];

                refQuestList.Add(go);
            }
        }
    }

    public void SetNotiPanel(JObject c)
    {
        NoticeTypeEnum type = c.ValueOrDefault<NoticeTypeEnum>("type", NoticeTypeEnum.normal);
        string message = c.ValueOrDefault<string>("message", string.Empty);

        NoticeManager.Instance.AddNotice(new NoticeData(type, message));
    }

    public RoomStatus GetRoom(long gtn)
    {
        RoomStatus status;

        if (roomStatusDic.ContainsKey(gtn))
        {
            if (!roomStatusDic.TryGetValue(gtn, out status))
                return null;
        }
        else
        {
            return null;
        }
        return status;
    }

    public RoomStatus GetTourmentRoom(long tn)
    {
        foreach (var room in roomStatusDic.Values)
        {
            if (room.tn == tn)
            {
                return room;
            }
        }

        return null;
    }

    private void ReceivePacket(Packet packet)
    {
        int p = packet.p;

        JObject c = packet.c;
        if (c.ContainsKey("ecode"))
        {
            int ecode = c["ecode"].ToObject<int>();
            if (ecode != 0)
            {
                string msg = c.ValueOrDefault("message", "");
                if (!string.IsNullOrEmpty(msg))
                {
                    NormalMessage.instance.OnOneButtonMessagePopUp(msg.ToLower());
                }
                return;
            }
        }
        switch ((PCProtocol)p)
        {
            case PCProtocol.PC_HOLDEM_DRAW_CARDS:
                DrawCard(c);
                break;
            case PCProtocol.PC_PLAY_GAME_ENTER:
            case PCProtocol.PC_ROOM_ENTER:
                RoomEnter(c);
                break;
            case PCProtocol.PC_PLAY_GAME_ENTER_OBSERVER:
                {
                    long tn = (long)c["data"]["tn"];
                    var roomStatus = GetTourmentRoom(tn);

                    RoomEnter(c, true);

                    if (roomStatus != null)
                    {
                        ObserverRoomMove(roomStatus.gtn);
                    }
                }
                break;
            case PCProtocol.PC_ROOM_RESERVE_LEAVE:
                {
                    int gtn = (int)c["gtn"];
                    //roomLeaveEvent.Invoke(c);
                }
                break;
            case PCProtocol.PC_ROOM_LEAVE:
                RoomLeave(c, false);
                break;
            case PCProtocol.PC_ROOM_DELETE:
                {
                    long gtn = (long)c["gtn"];
                    RoomStatus status = GetRoom(gtn);
                    if (status != null && status.isObserve)
                    {
                        NormalMessage.instance.AddSimpleMessage("delete_ob_table");
                        ObserverRoomLeave(gtn);
                    }
                }
                break;
            case PCProtocol.PC_PLAY_GAME_LEAVE_OBSERVER:
                {
                    RoomLeave(c, true);
                }
                break;
            //case PCProtocol.PC_BANG_ROOM_LEAVE:
            //    BangRoomLeave(c);
            //    break;
            case PCProtocol.PC_HOLDEM_STATUS:
                HoldemStatus(c);
                break;
            case PCProtocol.PC_ROOM_USER_LIST:
                HoldemUserList(c);
                break;
            case PCProtocol.PC_TNMT_MY:
                if (c.ContainsKey("tournament"))
                {
                    MyStatus.ClearTnmt();
                    JArray arr = c["tournament"] as JArray;
                    for (int i = 0; i < arr.Count; i++)
                    {
                        JObject obj = arr[i] as JObject;
                        TnmtData data = MyStatus.AddTnmt(obj.ValueOrDefault("tn", 0));
                        data.gtn = (int)obj.ValueOrDefault("gtn", 0);
                        //data.cafeIdx = (int)arr[i]["cafeIdx"];
                    }
                }

                if (c.ContainsKey("history"))
                {
                    TnmtHistory = c["history"] as JArray;
                }

                break;
            case PCProtocol.PC_TNMT_LIST:
                SetTournamentList(c);
                ReceiveTnmtList(c);
                break;
            case PCProtocol.PC_TNMT_UPDATE:
                SetTournamentInfo(c);
                tnmtUpdateEvent?.Invoke();
                break;
            case PCProtocol.PC_TNMT_UPDATE_APPLY_COUNT:
                UpdateTnmtApllyCount(c);
                break;
            //case PCProtocol.PC_ROOM_COMMAND:
            //    //RoomCommand(c);
            //break;
            //case PCProtocol.PC_HOLDEM_COMMAND:
            //    RoomCommand(c);
            //break;
            case PCProtocol.PC_USER_NOTIFICATION:
                int code = c["code"].ToObject<int>();
                if (code == 100)
                {
                    string message = c.ValueOrDefault("message", "");
                    switch (message)
                    {
                        case "DAILY_LOSS_LIMIT_OVER":
                            ErrorMessageManager.Instance.AddGameErrorBodyNotLocal(
                                100,
                                "SYS_ERR_OVER_LIMIT_LOSS_DAILY",
                                string.Format(
                                    LocalizeManager.GetLocalString(
                                        "SYS_EXIT_CLIENT_COZ_OVER_LIMIT_LOSS_DAILY"
                                    ),
                                    MyStatus.lossLimits[MyStatus.limittype]
                                ),
                                ErrorHandlingType.RECALL,
                                null,
                                () =>
                                {
                                    WebSocketManager.defaultCli.OnExitOnce += (rseon) =>
                                    {
                                        DevManager.Instance.GsLogin = false;
                                        DevManager.Instance.WsDelegate -= 1;
                                        DevManager.Instance.WsConnect = false;
                                        FirebaseManager.Instance.SignOut();
                                        CustomSceneManager.LoadLoginScene();
                                    };
                                    WebSocketManager.defaultCli.Close();
                                }
                            );
                            break;
                    }
                }
                break;
            case PCProtocol.PC_USER_NOTICE:
                SetNotiPanel(c);
                break;
            case PCProtocol.PC_COUNT_OF_TABLES_ZC:
                ZcCountArr = c.CastOrEmpty<JArray>("zc");
                break;
        }
    }

    //private void RoomCommand(JObject c)
    //{
    //    var gtn = c["gtn"].ToObject<int>();
    //    var roomData = GetRoom(gtn);
    //    roomData.holdem_command = c["command"].ToObject<int>();

    //}

    private void DrawCard(JObject c)
    {
        long gtn = c.ValueOrDefault<long>("gtn", 0);
        var rs = GetRoom(gtn);
        if (rs == null)
        {
            Debug.LogError($"방 정보 없음 (나간방) gtn : {gtn}");
            return;
        }
        int kind = (int)c["kind"];
        string cards = (string)c["cards"];
        string[] cardArr = cards.Split(',');
        string gid = (string)c["gid"];
        int runItTwiceIdx = 0;
        JArray odds = c.ValueOrDefault("odds", new JArray());
        if (c.ContainsKey("cur_run_it_twice"))
        {
            runItTwiceIdx = (int)c["cur_run_it_twice"];
        }
        if (kind == 0)
        {
            var user = rs.userList.Find(user => user.gid == gid);
            if (user != null)
            {
                user.cards = cards;
            }
        }
        else
        {
            rs.commcards = cardArr;
        }
    }

    private void SetTournamentList(JObject c, string key = "playTournamentList")
    {
        if (!c.ContainsKey(key))
            return; // data가 없는경우는 토너먼트에 참여하지 않은경우임
        JArray list = c[key] as JArray;

        for (int i = 0; i < list.Count; i++)
        {
            var data = list[i];
            SetTournamentInfo(data as JObject);
        }
    }

    private void UpdateTnmtApllyCount(JObject c)
    {
        int tn = (int)c["tn"];
        var info = GetTournamentInfo(tn);
        var countAllUser = (int)c.ValueOrDefault("countAllUser", 0);
        if (c.ContainsKey("countEntry"))
        {
            countAllUser = c.ValueOrDefault("countEntry", 0);
        }
        var live = info.info.CastOrEmpty<JObject>("live");
        live["countEntry"] = countAllUser;
        info.info["countAllUser"] = countAllUser;
        info.countAllUser = countAllUser;
        tnmtUpdateEvent?.Invoke();
    }

    private void SetTournamentInfo(JObject data)
    {
        var tn = (int)data["tn"];
        var info = GetTournamentInfo(tn);
        info.SetTournamentInfo(data);
    }

    public TournamentInfo GetTournamentInfo(int tn)
    {
        var info = tournamentInfoList.Find(
            (TournamentInfo t) =>
            {
                return t.tn == tn;
            }
        );
        if (info == null)
        {
            info = new TournamentInfo();
            info.tn = tn;
            tournamentInfoList.Add(info);
        }
        return info;
    }

    public void HoldemUserList(JObject c)
    {
        int gtn = (int)c["gtn"];
        RoomStatus status = GetRoom(gtn);

        if (status == null)
            return;
        JArray players = c["list"]["item"] as JArray;

        if (status.userList == null)
        {
            status.userList = new List<RoomUserData>();
        }
        for (int i = 0; i < players.Count; i++)
        {
            var player = players[i];

            var user = status.userList.Find(
                delegate(RoomUserData data)
                {
                    return data.gid == player["gid"].ToString();
                }
            );
            if (user == null)
            {
                user = new RoomUserData();
                status.userList.Add(user);
            }
            user.gid = (string)player["gid"];
            user.nick = (string)player["nick"];
            user.icon_no = (int)player["icon_no"];
            user.gc = (long)player["gc"];
            user.status = (int)player["status"];
            user.seat = (int)player["seat"];
            try
            {
                var option = player["option"];
                user.useUrlPhoto = option["usePhoto"].ToObject<bool>();
            }
            catch
            {
                Console.Log("notContain usePhoto");
            }

            try
            {
                user.photourl = (string)player["photourl"];
            }
            catch
            {
                user.photourl = null;
            }
        }
    }

    public void RoomReenter(long gtn)
    {
        var rd = GetRoom(gtn);
        Packet p;
        if (rd.isObserve)
        {
            p = new Packet(CPProtocol.CP_PLAY_GAME_ENTER_OBSERVER);
        }
        else
        {
            p = new Packet(CPProtocol.CP_PLAY_GAME_ENTER);
        }
        p.Add("gtn", rd.gtn);
        p.Add("tn", rd.tn);
        WebSocketManager.defaultCli.Send(p);
    }

    private void HoldemStatus(JObject c)
    {
        int gtn = (int)c["gtn"];
        RoomStatus status = GetRoom(gtn);
        if (status.userList == null)
        {
            RoomReenter(gtn);
            Debug.LogError("아직 유저 리스트 셋팅 안됨");
            return;
        }
        if (status == null)
            return;
        status.gn = (long)c["gn"];
        status.room_command = (int)c["room_command"];
        status.flow = c.ValueOrDefault("flow", 0);
        status.boss_seat = (int)c["boss_seat"];
        status.commcards = c["commcards"].ToObject<string[]>();
        status.who_turn = (int)c["who_turn"];
        status.who_turn_seat = c.ValueOrDefault("who_turn_seat", -1);
        status.turn_nexttime = c.ValueOrDefault("turn_nexttime", 0);
        status.money_total = (long)c["money_total"];
        status.money_call = (long)c["money_call"];
        status.money_minimum_raise = c.ValueOrDefault<long>("money_minimum_raise", 0);
        status.playerList = c["players"] as JArray;

        //if(status.playerList== null)
        //{
        //    status.playerList = new List<RoomUserData>();
        //}

        //for(int i = 0; i < players.Count; i++)
        //{
        //    JObject player = players[i] as JObject;

        //    RoomUserData user = status.playerList.Find(delegate(RoomUserData data) {return data.gid == player["gid"].ToString();});
        //    if(user == null)
        //    {
        //        user = new RoomUserData();
        //        status.playerList.Add(user);
        //    }

        //    user.gid = (string)player["gid"];
        //    try
        //    {
        //        user.cards = player["cards"].ToObject<string>();
        //    }
        //    catch
        //    {
        //        user.cards = String.Join(",", JsonDataParser.Parse<string[]>(player["cards"]));
        //    }

        //    user.money_bet_thistime = (int)player["money_bet_thistime"];
        //    user.money_total = (int)player["money_total"];
        //    user.last_bettype = (int)player["last_bettype"];
        //    user.bettype = player.ValueOrDefault("bettype",0);
        //    user.time_bank = player.CastOrEmpty<JObject>("time_bank");


        //}
    }

    private void BangRoomLeave(JObject c)
    {
        if (c["gid"].ToString() == MyStatus.gid)
        {
            ERR reason = c.ValueOrDefault<ERR>("reason", ERR.OK);
#if UNITY_WEBGL && !UNITY_EDITOR
            WebGLWindowMessage.OnUpdatedFlow("leavedRoom", reason.ToString());
#endif
        }
    }

    private async void RoomLeave(JObject c, bool isObserve)
    {
        int gtn = (int)c["gtn"];
        enterdRoomList.Remove(gtn);
        RoomStatus status = GetRoom(gtn);
        if (status == null)
            return;

        if (isObserve)
        {
            roomStatusDic.Remove(gtn);
            roomLeaveEvent.Invoke(c, true);
            return;
        }

        status.userList.Remove(
            status.userList.Find(
                delegate(RoomUserData data)
                {
                    return data.gid == c["gid"].ToString();
                }
            )
        );

        if (c["gid"].ToString() == MyStatus.gid)
        {
            ERR reason = c.ValueOrDefault<ERR>("reason", ERR.OK);

            switch (reason)
            {
                case ERR.OK:
                case ERR.TNMT_UNAPPLY:
                    break;
                case ERR.CANCEL_TNMT:
                    NormalMessage.instance.AddSimpleMessage("leave_cancel_tnmt");
                    break;
                case ERR.MOVE_ROOM:
                    await new WaitForPCProtocol(PCProtocol.PC_PLAY_GAME_ENTER);
                    Console.Log("RemoveTableAfterNewTable - 2초 대기");
                    await UniTask.Delay(TimeSpan.FromSeconds(2.0));
                    Console.Log("RemoveTableAfterNewTable - 2초 끝");
                    break;
                default:

                    break;
            }
            roomStatusDic.Remove(gtn);
        }

        roomLeaveEvent.Invoke(c, false);

        int ecode = c.ValueOrDefault<int>("ecode", 0);
        switch ((ERR)(ecode)) {
            //case ERR.EXCEEDED_CHIP_HOLDING_AMOUNT:
            //    ErrorMessageManager.Instance.addApplicationError(0, "SYS_MSG_OVERLIMIT_POPUP_TITLE", "SYS_MSG_OVERLIMIT_POPUP_KICK", ErrorHandlingType.NONE);
            //    break;
            //case ERR.ROOM_LEAVE_ALL_USER_OBSERVER:
            //    NormalMessage.instance.AddSimpleMessage("더이상 게임이 없어서, 관전중인 방에서 퇴장하였습니다.");
            //    break;
            //case ERR.ROOM_LEAVE_FORCE_BY_BANGJANG:
            //    NormalMessage.instance.AddSimpleMessage("방에서 퇴장 당했습니다.");
            //    break;
        }
    }

    private void ObserverRoomLeave(long gtn)
    {
        var p = new Packet(CPProtocol.CP_PLAY_GAME_LEAVE_OBSERVER);
        p.Add("gtn", gtn);
        WebSocketManager.defaultCli.Send(p);
    }

    private async void ObserverRoomMove(long gtn)
    {
        await UniTask.Delay(TimeSpan.FromSeconds(1f));

        ObserverRoomLeave(gtn);
    }

    private void ReceiveTnmtList(JObject c)
    {
        if (!c.ContainsKey("playTournamentList"))
            return;
        JArray arr = c["playTournamentList"] as JArray;
        SetTnmtList(arr);
    }

    public void SetTnmtList(JArray arr)
    {
        for (int i = 0; i < arr.Count; i++)
        {
            SetTournamentInfo(arr[i] as JObject);
        }
    }

    private void RoomEnter(JObject c, bool isObserve = false)
    {
        //ERR ecode = (ERR)(int)c["ecode"];
        //if( ecode != ERR.OK)
        //{
        //    Debug.LogError(c["message"]);
        //    return;
        //}

        JObject data = c["data"] as JObject;
        Debug.Log(c.ToString());
        int gtn = (int)data["gtn"];
        int ecode = c.ValueOrDefault<int>("ecode", 0);
        if (!enterdRoomList.Contains(gtn))
        {
            enterdRoomList.Add(gtn);
        }

        switch ((ERR)ecode) {
            //case ERR.EXCEEDED_CHIP_HOLDING_AMOUNT:
            //    ErrorMessageManager.Instance.addApplicationError(0, "SYS_MSG_OVERLIMIT_POPUP_TITLE", "SYS_MSG_OVERLIMIT_POPUP_KICK", ErrorHandlingType.NONE);
            //    break;
        }
        RoomStatus status;
        enterRn = gtn;
        enterGameType = (GAME_TYPE)(int)data["game_type"];

        if (!roomStatusDic.ContainsKey(gtn))
        {
            status = new RoomStatus();
            roomStatusDic.Add(gtn, status);
        }
        else
        {
            if (!roomStatusDic.TryGetValue(gtn, out status))
                return;
            status.userList.Clear();
        }

        status.info = data;

        status.game_type = (GAME_TYPE)(int)data["game_type"];
        status.chip_type = (CHIP_TYPE)(int)data["chip_type"];

        status.gtn = (int)data["gtn"];
        // status.ttl = (string)c["ttl"];
        // status.pw = (bool)c["pw"];
        // status.gid = (string)room["gid"];
        // status.nk = (string)room["nk"];
        // status.lv = (int)room["lv"];
        // status.an = (long)room["ante"];
        // status.mx = (long)room["mx"];

        status.bi = (long)data["buyin_min"];
        status.bil = (long)data.ValueOrDefault("buyin_max", long.MaxValue);
        status.mx = data.ValueOrDefault("max", long.MaxValue);
        status.personnel = data.ValueOrDefault("personnel", 9);
        status.random_sit_in = data.ValueOrDefault("random_sit_in", false);
        status.timebank = false; // data.ValueOrDefault("timebank", false); // deprecated 사용하지 않기로 한다.
        // status.bil = (long)room["bil"];
        // status.emn = (long)room["emn"];
        // status.emx = (long)room["emx"];
        status.bg = (long)data["blind"];
        // status.sm = status.bg / 2;
        status.sm = (long)data.ValueOrDefault<long>("small_blind", 0);

        status.an = (long)data["ante"];

        status.passive_rake = data.ValueOrDefault("passive_rake", false);
        status.passive_rake_select = data.ValueOrDefault("passive_rake_select", 0);
        status.pr1 = data.ValueOrDefault<long>("pr1", 0);
        status.pr2 = data.ValueOrDefault<long>("pr2", 0);
        status.pr3 = data.ValueOrDefault<long>("pr3", 0);
        status.pr4 = data.ValueOrDefault<long>("pr4", 0);

        status.stack_removal = data.ValueOrDefault<bool>("pr4", false);
        status.stack_removal_option1 = data.ValueOrDefault<long>("stack_removal_option1", 0);
        status.stack_removal_option2 = data.ValueOrDefault<long>("stack_removal_option2", 0);

        status.stack_removal_bb1 = data.ValueOrDefault<long>("stack_removal_bb1", 0);
        status.stack_removal_bb2 = data.ValueOrDefault<long>("stack_removal_bb2", 0);

        status.tb2 = data.ValueOrDefault("tb2", false);
        status.tb2_time = data.ValueOrDefault("tb2_time", 0);
        status.tb2_price = data.ValueOrDefault("tb2_price", 0);

        status.cafeIdx = (long)data["cafeIdx"];

        status.isObserve = isObserve;

        try
        {
            status.tn = (int)data["tn"];
        }
        catch (Exception e)
        {
            Debug.Log(e.ToString());
        }
        if ((GAME_TYPE)(int)data["game_type"] == GAME_TYPE.nlh)
        {
            GetHoldemStatus(gtn);
        }
        else if ((GAME_TYPE)(int)data["game_type"] == GAME_TYPE.plo)
        {
            GetHoldemStatus(gtn);
        }
        else if ((GAME_TYPE)(int)data["game_type"] == GAME_TYPE.sng)
        {
            GetHoldemStatus(gtn);
        }
        else if ((GAME_TYPE)(int)data["game_type"] == GAME_TYPE.short_deck)
        {
            GetHoldemStatus(gtn);
        }
        else if ((GAME_TYPE)(int)data["game_type"] == GAME_TYPE.mtt)
        {
            GetHoldemStatus(gtn);
        }
        //else if((string)c["game_type"] == "badugi")
        //{
        //    GetBadugiStatus(gtn);
        //}
        // if ((GAME_TYPE)(int)c["game_type"] == GAME_TYPE.nlh) {
        //     GetHoldemStatus(gtn);
        //     if (DevOptionsManager.devOptions.bettingMode == 0)
        //     {
        //         CustomSceneManager.LoadScene("Holdem");
        //     }
        //     else if (DevOptionsManager.devOptions.bettingMode == 1)
        //     {
        //         CustomSceneManager.LoadScene("Holdem 1");
        //     }
        // }
        // else if((string)c["game_type"] == "badugi") {
        //     CustomSceneManager.LoadScene("Badugi");
        // }
        // else if((string)c["game_type"] == "poker7") {
        //     CustomSceneManager.LoadScene("Poker7");
        // }
        roomEnterEvent?.Invoke(gtn, (GAME_TYPE)(int)data["game_type"]);
    }

    //public void GetMyInvenToryList()
    //{
    //    PublisherApiManager.Instance.GetInvenListPubAPI(GetMyInvenToryListCallBack);
    //}
    //public void GetMyInvenToryListCallBack(long statusCode, JObject res)
    //{
    //    switch(statusCode)
    //    {
    //        case 200:
    //            var arr = res["myItems"] as JArray;
    //            Console.Log(string.Format("my item Count : {0}",arr.Count));
    //            Console.Log(arr.ToString());
    //            MyStatus.invenList= new List<JObject>();

    //            for(int i = 0; i < arr.Count; i++)
    //            {
    //                MyStatus.invenList.Add(arr[i] as JObject);
    //                //Console.SpecialLog(arr[i].ToString());
    //            }
    //            break;
    //    }
    //}


    public async void GetHoldemStatus(int gtn)
    {
        await UniTask.WaitForSeconds(0.1f);
        Packet p = new Packet((int)CPProtocol.CP_HOLDEM_STATUS);
        p.Add("gtn", gtn);
        WebSocketManager.defaultCli.Send(p.ToJson());
    }

    private static int countProtocol = 0;
    private static Dictionary<string, string> dApiAndProtocol = new Dictionary<string, string>();
    private static List<LogPair> logPairList = new List<LogPair>();

    public static void PushApiAndProtocol(string key, string value)
    {
#if UNITY_EDITOR
        countProtocol++;

        if (value == null)
        {
            return;
        }

        key = key.Replace(PublisherApiManager.Instance.url, "");

        string keyOther = countProtocol.ToString() + "," + key;
        logPairList.Add(new LogPair(keyOther, value));
        if (dApiAndProtocol.ContainsKey(keyOther)) { }
        else
        {
            dApiAndProtocol.Add(keyOther, value);
        }

        // 저장


        string pathKey = Path.Combine(Application.persistentDataPath, "ApiAndProtocol_Key.txt");
        string pathValue = Path.Combine(Application.persistentDataPath, "ApiAndProtocol_Value.txt");

        var linesKey = new List<string>();
        var linesValue = new List<string>();

        foreach (var t in logPairList)
        {
            linesKey.Add(t.Key);
            linesValue.Add(t.Value);
        }

        File.WriteAllLines(pathKey, linesKey.ToArray());
        File.WriteAllLines(pathValue, linesValue.ToArray());
#endif
    }

    private struct LogPair
    {
        public string Key;
        public string Value;

        public LogPair(string key, string value)
        {
            this.Key = key;
            this.Value = value;
        }
    }
}

public class RoomEnterEvent : UnityEvent<int, GAME_TYPE> { }

public class JsonEvent : UnityEvent<JObject, bool> { }

public class Event : UnityEvent { }

public class StringEvent : UnityEvent<string> { };

public class MyIntEvent : UnityEvent<int> { }

public class RefQuest
{
    public int id;
    public string name;
    public int kind;
    public int loginCountContinued;
    public long rewardChip;
}

public class TournamentInfo
{
    public JObject info;
    public int tn;
    public int state;
    public long totalPrize;
    public string title;
    public long buyIn;
    public long buyInFee;
    public string startTime;
    public string closeTime;
    public int gameType;
    public long startChip;
    public int rebuyCount;
    public long rebuyCost;
    public long rebuyChip;
    public int addOnCount;
    public long addOnCost;
    public long addOnChip;
    public int countAllUser;
    public int myTable;

    public TournamentInfo(JObject info)
    {
        if (info != null)
            SetTournamentInfo(info);
    }

    public TournamentInfo() { }

    public void SetTournamentInfo(JObject data)
    {
        tn = (int)data["tn"];

        info = data;
        var live = data.CastOrEmpty<JObject>("live");
        if (live.ContainsKey("countEntry"))
        {
            data["countAllUser"] = live["countEntry"];
        }
        state = data.ValueOrDefault("state", 0);
        totalPrize = data.ValueOrDefault("t_reward_all_chip", 0);
        title = data.ValueOrDefault("t_title", string.Empty);
        buyIn = data.ValueOrDefault("t_buyin", 0);
        buyInFee = data.ValueOrDefault("t_buyin_fee", 0);
        startTime = data.ValueOrDefault("t_start_time", string.Empty);
        closeTime = data.ValueOrDefault("t_close_time", string.Empty);
        gameType = data.ValueOrDefault("t_game_type", 0);
        startChip = data.ValueOrDefault("t_starting_chip", 0);
        rebuyCount = data.ValueOrDefault("t_rebuy_count", 0);
        rebuyCost = data.ValueOrDefault("t_rebuy_cost", 0);
        rebuyChip = data.ValueOrDefault("t_rebuy_chip", 0);
        addOnCount = data.ValueOrDefault("t_add_on_count", 0);
        addOnCost = data.ValueOrDefault("t_add_on_cost", 0);
        addOnChip = data.ValueOrDefault("t_add_on_chip", 0);
        countAllUser = data.ValueOrDefault("countAllUser", 0);
    }
}
public static class MyTagData
{
    private static JObject data;
    public static JObject Data {
        get { return data; }
        set { data = value; MyTagDataEvent.Invoke(); }
    }

    public static Dictionary<string, TagData> tagDic = new Dictionary<string, TagData>();
    public struct TagData
    {
        public int tag;
        public string memo;
        public TagData(int tag, string memo)
        {
            this.tag = tag;
            this.memo = memo;
        }
    }
    private static UnityEvent MyTagDataEvent = new UnityEvent();
    private static UnityEvent<string> TagDataEvent = new UnityEvent<string>();
    public static void AddMyTagDataEvent(UnityAction action)
    {
        MyTagDataEvent.AddListener(action);
    }

    public static void AddTagDataEvent(UnityAction<string> action)
    {
        TagDataEvent.AddListener(action);
    }

    public static void RemoveMyTagDataEvent(UnityAction action)
    {
        MyTagDataEvent.RemoveListener(action);
    }

    public static void RemoveTagDataEvent(UnityAction<string> action)
    {
        TagDataEvent.RemoveListener(action);
    }

    public static void GetMyTagData()
    {
        PublisherApiManager.Instance.RequestTagMyGet(SetMyTagData);
    }

    public static void ChangeTagData(JObject data)
    {
        Data = data;
        PublisherApiManager.Instance.RequestTagMySet(SetMyTagData, data);
    }

    public static void SetMyTagData(bool success, JObject data)
    {
        if(success)
        {
            var TagData = data.CastOrEmpty<JObject>("tagData");
            //var TagData = JObject.Parse(TagDataString);
            var value = TagData["value"];
            if(value.Type == JTokenType.String)
            {
                var valueString = TagData.ValueOrDefault("value", "{}");
                Data = JObject.Parse(valueString);
            }
            else if(value.Type == JTokenType.Object)
            {
                Data = value as JObject;
            }
            else
            {
                Debug.LogError("태그 데이터 타입 오류");
            }
        }
        else
        {
            Debug.LogError("태그 데이터 가져오기 실패");
        }
    }

  
    public static void SetTagData(bool success, JObject data)
    {
        if(success)
        {
            var gid = data.ValueOrDefault<string>("gid", string.Empty);
            var tag = data.ValueOrDefault<int>("tag", 0);
            var memo = data.ValueOrDefault<string>("memo", string.Empty);
            if(string.IsNullOrEmpty(gid))
            {
                Debug.LogError("gid이 비어있습니다.");
                return;
            }
            tagDic[gid] = new TagData { tag = tag, memo = memo };
            TagDataEvent.Invoke(gid);
        }
        else
        {
            Debug.LogError("태그 데이터 가져오기 실패");
        }
    }
    public static void GetTagDataCallBack(bool success, JObject data)
    {
        if(success)
        {
            var gid = data.ValueOrDefault<string>("gid", string.Empty);
            var tagData = data.CastOrEmpty<JObject>("tagData");
            var tag = tagData.ValueOrDefault<int>("tag", 0);
            var memo = tagData.ValueOrDefault<string>("memo", string.Empty);
            if(string.IsNullOrEmpty(gid))
            {
                Debug.LogError("gid이 비어있습니다.");
                return;
            }
            tagDic[gid] = new TagData(tag, memo);
            
            TagDataEvent.Invoke(gid);
        }
        else
        {
            Debug.LogError("태그 데이터 가져오기 실패");
        }
    }
    public static async UniTask<TagData> GetTagData(string gid)
    {
        if(!tagDic.ContainsKey(gid))
        {
            await PublisherApiManager.Instance.RequestTagGet(GetTagDataCallBack, gid);
            
        }
        if(tagDic.ContainsKey(gid))
        {
            return tagDic[gid];
        }
        else
        {
            return new TagData(0, string.Empty);
        }
    }
    public static void ChangeTagData(string gid, int tag, string memo)
    {
        PublisherApiManager.Instance.RequestTagSet(null, gid, tag, memo);
        tagDic[gid] = new TagData(tag, memo);
        
        TagDataEvent.Invoke(gid);
    }
}
