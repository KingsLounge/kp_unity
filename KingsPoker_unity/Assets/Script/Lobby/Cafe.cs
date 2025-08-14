using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Security.Policy;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class CafeMemberInfo // 내가 가입한 카페의 내정보 ( 키페에 있는 나의 zc, dc  )
{
    public int idx;
    public int status;
    public int permit;
    public int type;
    public string nick;
    public long zc;
    public long dc;
    public long cc;
    public long point;
    public long debt;
    public int cafeIdx;
    public int gameUserIdx;
}

public class CafeInfo // 내가 가입한 카페
{
    //public int idx;
    //public int priv;
    //public int status;
    //public string name;
    //public string pw;
    //public string desc;
    //public int tier;
    //public int gameCount;
    //public int memberCount;
    //public long zc;
    //public long dc;

    public JObject info;

    public List<CafeMemberInfo> cafeMembers;

    public CafeInfo(JObject info)
    {
        this.info = info;
    }
}

public enum KingshillDataType
{
    number,
    str,
    agent,
    tickettype,
    ticketCounts
}

[System.Serializable]
public class LoungeDataText
{
    public string key;
    public string localKey;

    public KingshillDataType dataType = KingshillDataType.number;
    public List<Text> valText = new List<Text>();
    public List<LocalText> localText = new List<LocalText>();

    public void SetData(string val)
    {
        foreach (var txt in valText)
        {
            txt.text = val;
        }

        foreach (var txt in localText)
        {
            txt.SetLocalText(localKey, val);
        }
    }
}

public class Cafe : WebsocketListenBehaviour
{
    public static int pagePer = 50;
    public CafeLogoData logoData;
    public GameListGenerator gameList;
    public TnmtListGenerator tnmtList;
    private List<CafeItem> cafeItemList = new List<CafeItem>();
    private List<CafeInfo> cafeInfoList = new List<CafeInfo>(); // 내가 가입한 카페들의 리스트
    public CustomUIOpener customUIOpener;
    public static Cafe instance { get; private set; }
    public JObject curEnterCafeInfo { get; private set; }
    public Text cafePermit;
    public Text cafePermit2;
    public CAFE_TIER tier { get; private set; }
    public CAFE_MEMBER_PERMIT permit { get; private set; }
    public List<Variation> tierVariation = new List<Variation>();
    public List<Variation> permitVariation = new List<Variation>();
    public Text cafeCode;
    public Text cafeZuiceText;
    public List<Text> cafeNameTexts = new List<Text>();
    public Text cafeTierText;
    public Text cafeMemberCountText;
    public Text cafeGameCountText;
    public List<Text> dummyChipTexts;

    public Text cafeNickText;

    public List<LoungeDataText> loungeDataTexts = new List<LoungeDataText>();
    public Text dummyChipDebtText;
    public Image cafeLogo;
    public GameObject orderNoticeMark;
    public GameObject joinNoticeMark;

    public List<Timer> timeLeftTimer = new List<Timer>();
    public CustomUIOpener cashierChipPopup;
    public CustomUIOpener cafeOptionPopup;
    public CustomUIOpener popupOpener;
    public List<GameObject> hasBorrowedChipShow = new List<GameObject>();
    public long[,] cafeTierData = new long[,]
    {
        { 4, 1, 5000, 2, 1, 10000000000 }, // 2023-10-10
        { 100, 10, 5000, 19, 5, 10000000000 },
        { 500, 20, 5000, 49, 10, 10000000000 },
        { 1000, 30, 5000, 99, 10, 10000000000 },
        { 2000, 50, 5000, 199, 10, 10000000000 },

        // { 4, 1, 6, 1, 1, 1000000 },
        // { 100, 10, 20, 2, 5, 10000000 },
        // { 500, 20, 50, 3, 10, 100000000 },
        // { 1000, 30, 100, 4, 10, 1000000000 },
        // { 2000, 50, 200, 5, 10, 10000000000 }
    };
    public ItemControllerServerCommunication itemController;
    public delegate void BannedCafeDelegate(int cafeIdx);
    public BannedCafeDelegate bannedEvent = null;

    public GameObject pointPopup;
    public CashierPopup cashierPopup;

    public GameObject ticketPopup;

    [SerializeField]
    private List<GameObject> otherCafeItems;

    [SerializeField]
    private List<GameObject> loungeItems;

    [SerializeField]
    private RoomCreateWindow roomCreateWindow;

    [SerializeField]
    private TnmtApplyPopup tnmtApplyPopup;

    [SerializeField]
    private TnmtUserInfoPopup tnmtUserInfoPopup;

    [SerializeField]
    public CafeQuestPopup cafeQuestPopup;

    public TnmtApplyPopup TnmtApplyPopup
    {
        get { return tnmtApplyPopup; }
    }

    public TnmtUserInfoPopup TnmtUserInfoPopup
    {
        get { return tnmtUserInfoPopup; }
    }

    protected override void Awake()
    {
        base.Awake();
        instance = this;
        for (int i = 0; i < timeLeftTimer.Count; i++)
        {
            timeLeftTimer[i].textFormatter = (double time) =>
            {
                long timeSeconds = (long)time;
                const long monthSec = (24 * 60 * 60 * 30);
                const long daySec = (24 * 60 * 60);
                const long hourSec = (60 * 60);
                const long minuteSec = 60;
                long month = 0;
                long day = 0;
                long hour = 0;
                long minute = 0;
                string format;
                if (timeSeconds >= daySec)
                {
                    format = LocalizeManager.GetLocalString("cafe_zuice_time");
                }
                else
                {
                    format = LocalizeManager.GetLocalString("cafe_zuice_time_noday");
                }
                //{0} {0:D2} 둘다 인식하기위해 }를 닫지않음



                //if (format.Contains("{0"))  // month 는 제외한다. 2021-04-07
                //{
                //    month = timeSeconds / monthSec;
                //    timeSeconds %= monthSec;
                //}
                if (format.Contains("{0"))
                {
                    day = timeSeconds / daySec;
                    timeSeconds %= daySec;
                }
                if (format.Contains("{1"))
                {
                    hour = timeSeconds / hourSec;
                    timeSeconds %= hourSec;
                }
                if (format.Contains("{2"))
                {
                    minute = timeSeconds / minuteSec;
                    timeSeconds %= minuteSec;
                }

                string[] formats = format.Split(' ');

                for (int j = 0; j < formats.Length; j++)
                {
                    formats[j] = string.Format(formats[j], day, hour, minute, timeSeconds);
                }

                return string.Join(" ", formats);
            };
        }

        itemController.changePageCallback = (next) =>
        {
            if (next)
            {
                RequestCafeList();
            }
        };
        itemController.updateItemCallback = (go, data) =>
        {
            CafeItem t = go.GetComponent<CafeItem>();
            if (!cafeItemList.Contains(t))
            {
                t.manager = this;
                cafeItemList.Add(t);
            }
            t.Set(
                cafeInfoList.Find(value =>
                    (value.info as JObject)["idx"] == (data as JObject)["idx"]
                )
            );
        };
        RequestCafeList();
        // if( cafeItemList == null ) cafeItemList = new
    }

    public CafeInfo GetCafeData(int idx)
    {
        return cafeInfoList.Find(value => (int)value.info["idx"] == idx);
    }

    public void OnClickCreateRoomButton()
    {
        if (roomCreateWindow)
        {
            roomCreateWindow.SetCafeIdx((int)curEnterCafeInfo["cafe"]["idx"]);
            roomCreateWindow.SetChipType((int)CHIP_TYPE.cc);
            roomCreateWindow.gameObject.SetActive(true);
        }
        else
        {
            Console.Error("isNotEnable roomCreateWindow");
        }
    }

    public CafeInfo GetCafeList(long idx)
    {
        return cafeInfoList.Find(
            (i) =>
            {
                return i.info["idx"].ToObject<long>() == idx;
            }
        );
    }

    private async void SetCafeLogo(int icon, string url, int cafeIdx)
    {
        Sprite sprite = null;
        if (icon == 0)
        {
            if (!string.IsNullOrEmpty(url))
                sprite = (
                    await ImageDatabase.LoadImageTexture(
                        url,
                        Application.persistentDataPath + "/cafeLogo",
                        cafeIdx.ToString()
                    )
                ).ToSprite();
        }
        else
        {
            sprite = logoData.sprites[icon - 1];
        }
        cafeLogo.sprite = sprite;
    }

    protected override void ReceivePacket(Packet packet)
    {
        base.ReceivePacket(packet);
        JObject c = packet.c;
        switch ((PCProtocol)packet.p)
        {
            case PCProtocol.PC_CAFE_LIST:
                ReceiveCafeList(c);
                break;

            case PCProtocol.PC_CAFE_CREATE:
                {
                    int ecode = c.ValueOrDefault("ecode", 0);
                    if (ecode == 0)
                    {
                        NormalMessage.instance.OnOneButtonMessagePopUp("cafe_create_success");
                        RequestCafeList();
                    }
                    else
                    {
                        ErrorMessageManager.Instance.AddGameError(
                            ecode,
                            c["message"].ToString(),
                            c["message"].ToString() + "_BODY"
                        );
                    }
                }
                break;
            case PCProtocol.PC_CAFE_NOTICE:
                {
                    int cafeIdx = c.ValueOrDefault("cafeIdx", -1);
                    CafeInfo info = GetCafeList(cafeIdx);
                    if (info != null)
                    {
                        CAFE_NOTICE_TYPE type = (CAFE_NOTICE_TYPE)c.ValueOrDefault("type", 0);
                        int count = c.ValueOrDefault("count", 0);
                        if (type == CAFE_NOTICE_TYPE.cafe_chip_request)
                        {
                            info.info["orderReqCount"] = count;
                            if (count > 0)
                            {
                                //NormalMessage.instance.AddSimpleMessage(LocalizeManager.GetLocalString("receive_order_request"));
                            }
                        }
                        else if (type == CAFE_NOTICE_TYPE.cafe_join)
                        {
                            info.info["joinReqCount"] = count;
                            if (count > 0)
                            {
                                //NormalMessage.instance.AddSimpleMessage(LocalizeManager.GetLocalString("receive_join_request"));
                            }
                        }
                        if (
                            curEnterCafeInfo != null
                            && (int)curEnterCafeInfo["cafe"]["idx"] == cafeIdx
                        )
                        {
                            bool bPermit =
                                this.permit == CAFE_MEMBER_PERMIT.manager
                                || this.permit == CAFE_MEMBER_PERMIT.owner;

                            if (orderNoticeMark)
                                orderNoticeMark.SetActive(
                                    bPermit && info.info.ValueOrDefault("orderReqCount", 0) > 0
                                );
                            if (joinNoticeMark)
                                joinNoticeMark.SetActive(
                                    bPermit && info.info.ValueOrDefault("joinReqCount", 0) > 0
                                );
                        }
                        itemController.Refresh();
                    }
                }
                break;

            case PCProtocol.PC_CAFE_JOIN_UPDATE:
                {
                    int ecode = c.ValueOrDefault("ecode", 0);

                    if (ecode == 0)
                    {
                        if (c.ContainsKey("cafeList")) // member
                        {
                            var cafe = c.CastOrEmpty<JArray>("cafeList")[0] as JObject;
                            var member = cafe.CastOrEmpty<JArray>("cafeMembers")[0] as JObject;
                            CAFE_MEMBER_STATUS cms = (CAFE_MEMBER_STATUS)
                                member.ValueOrDefault("status", 0);
                            if (cms == CAFE_MEMBER_STATUS.accepted)
                            {
                                string str = LocalizeManager.GetLocalString("cafe_join_success_p");
                                NormalMessage.instance.AddSimpleMessage(str);
                                CafeListUpdate(c);
                                EnterCafe(c.ValueOrDefault("cafeIdx", 0));
                            }
                            //else if (cms == CAFE_MEMBER_STATUS.rejected)
                            //{
                            //    string str = LocalizeManager.GetLocalString("cafe_join_rejected_p");
                            //    NormalMessage.instance.AddSimpleMessage(str);
                            //}
                        }
                        else // manager
                        { }
                    }
                    else { }
                }
                break;

            case PCProtocol.PC_CAFE_RESERVE_REMOVE:
                {
                    if (c.ValueOrDefault("ecode", -1) != 0)
                        break;
                    int cafeIdx = c.ValueOrDefault("cafeIdx", -1);
                    if (cafeIdx < 0)
                        break;

                    CafeInfo cafeInfo = GetCafeList(cafeIdx);
                    if (cafeInfo != null)
                    {
                        // delete
                        cafeInfoList.Remove(cafeInfo);
                        itemController.Refresh();
                    }

                    if (curEnterCafeInfo != null && (int)curEnterCafeInfo["cafe"]["idx"] == cafeIdx)
                    {
                        popupOpener.CloseUI();
                        LobbyTabsManager.Instance.TabTypeSet(0);
                    }
                }
                break;
            case PCProtocol.PC_CAFE_UPDATE:
                {
                    var dataArray = itemController.dataArray;
                    var cafe = c.CastOrEmpty<JObject>("cafe");
                    var idx = cafe.ValueOrDefault<long>("idx", 0);

                    if (dataArray != null)
                    {
                        for (int i = dataArray.Count - 1; i >= 0; i--)
                        {
                            if ((dataArray[i] as JObject).ValueOrDefault("idx", -1) == idx)
                            {
                                var data = dataArray[i] as JObject;
                                var cafeMembers = data.CastOrEmpty<JArray>("cafeMembers");
                                cafe["cafeMembers"] = cafeMembers;
                                dataArray[i] = cafe;
                            }
                        }
                        RefreshCafeList(dataArray);
                    }
                }
                if (c.ValueOrDefault("ecode", -1) != 0)
                    break;
                if (curEnterCafeInfo == null)
                    break;
                if (c.ValueOrDefault("cafeIdx", -1) != (int)curEnterCafeInfo["cafe"]["idx"])
                    break;
                curEnterCafeInfo["cafe"] = c["cafe"];
                c = curEnterCafeInfo;
                goto case PCProtocol.PC_CAFE_INFO;
            case PCProtocol.PC_CAFE_INFO:
            {
                {
                    LoadingCircle.Instance.StopSpin();
                    if (c.ValueOrDefault("ecode", -1) != 0)
                        break;
                    curEnterCafeInfo = c;
                    CafeInfoSet();
                }
                break;
            }
            case PCProtocol.PC_CAFE_TIME_UPDATE:
                {
                    int cafeIdx = (int)c["cafeIdx"];

                    JArray dataArray = itemController.dataArray;
                    for (int i = 0; i < cafeInfoList.Count; i++)
                    {
                        if (cafeInfoList[i].info.ValueOrDefault("idx", -1) == cafeIdx)
                        {
                            cafeInfoList[i].info["leftSeconds"] = c["leftSeconds"];
                        }
                    }
                    if (dataArray != null)
                    {
                        for (int i = dataArray.Count - 1; i >= 0; i--)
                        {
                            if ((dataArray[i] as JObject).ValueOrDefault("idx", -1) == cafeIdx)
                            {
                                dataArray[i]["leftSeconds"] = c["leftSeconds"];
                            }
                        }
                        itemController.Refresh();
                    }
                    if (curEnterCafeInfo != null && (int)curEnterCafeInfo["cafe"]["idx"] == cafeIdx)
                    {
                        double leftSeconds = (double)c["leftSeconds"];
                        bool operation = (bool)c["operation"];
                        //DateTime expireAt = (c as JObject).ValueOrDefault("expireAt", DateTime.Now);
                        //DateTime origin = DateTime.Now;
                        //TimeSpan diff = expireAt.ToUniversalTime() - origin.ToUniversalTime();
                        //double leftSeconds = Math.Floor(diff.TotalSeconds);
                        long timeSeconds = (long)leftSeconds;
                        if (timeSeconds < 0)
                            timeSeconds = 0;

                        for (int i = 0; i < timeLeftTimer.Count; i++)
                        {
                            timeLeftTimer[i].SetTimer(operation, timeSeconds, 24 * 60 * 60);
                        }
                    }
                }
                break;
            case PCProtocol.PC_GAME_PLAYER_COUNT:
                {
                    JObject json = (JObject)c["json"];
                    long gtn = (long)c["gtn"];
                    int playerCount = (int)c["player_count"];
                    gameList.UpdatePlayGamePlayerCount(gtn, playerCount);
                }
                break;
            case PCProtocol.PC_ROOM_DELETE:
                {
                    if (c.ContainsKey("gtn"))
                        gameList.RemoveGame((int)c["gtn"]);
                }
                break;
            case PCProtocol.PC_CAFE_MEMBER_UPDATE:
                /*  var json = {
                        ecode: 0,
                        cafeIdx,
                        cafeMember,
                        memberCount,  // null or number
                    }
                */
                {
                    int ecode = c.ValueOrDefault("ecode", 0);
                    if (ecode == 0)
                    {
                        int cafeIdx = (int)c["cafeIdx"];
                        CafeInfo cafeInfo = GetCafeList(cafeIdx);
                        CafeMemberInfo memberInfo = SetCafeMember(c["cafeMember"]);
                        bool isMe = false;
                        for (int i = 0; cafeInfo != null && i < cafeInfo.cafeMembers.Count; i++)
                        {
                            if (cafeInfo.cafeMembers[i].idx == memberInfo.idx)
                            {
                                //cafeList.cafeMembers[i] = memberInfo;
                                if (i == 0)
                                    isMe = true;
                            }
                        }

                        if (isMe)
                        {
                            bool isCurEnterCafe = false;
                            bool isBanned = false;
                            bool isChangePermit =
                                GetCafeList((int)c["cafeIdx"]).cafeMembers[0].permit
                                != (int)c["cafeMember"]["permit"];

                            //GetCafeList((int)c["cafeIdx"]).cafeMembers[0] = SetCafeMember(c["cafeMember"]);
                            if (
                                curEnterCafeInfo != null
                                && (int)curEnterCafeInfo["cafe"]["idx"] == (int)c["cafeIdx"]
                            )
                            {
                                isCurEnterCafe = true;
                            }
                            if (memberInfo.status == (int)CAFE_MEMBER_STATUS.suspended)
                            {
                                JArray dataArray = itemController.dataArray;
                                if (dataArray != null)
                                {
                                    for (int i = dataArray.Count - 1; i >= 0; i--)
                                    {
                                        if (
                                            (dataArray[i] as JObject).ValueOrDefault("idx", -1)
                                            == cafeIdx
                                        )
                                        {
                                            dataArray.RemoveAt(i);
                                        }
                                    }
                                    itemController.Refresh();
                                }
                                string contents = LocalizeManager
                                    .GetLocalString("your_banned_cafe")
                                    .Replace(
                                        "{cafeName}",
                                        curEnterCafeInfo.ValueOrDefault("name", "")
                                    );
                                NormalMessage.instance.AddSimpleMessage(contents);
                                bannedEvent?.Invoke(cafeIdx);
                                isBanned = true;
                            }
                            else if (isChangePermit)
                            {
                                string contents = LocalizeManager
                                    .GetLocalString("your_permit_changed")
                                    .Replace(
                                        "{cafeName}",
                                        curEnterCafeInfo.ValueOrDefault("name", "")
                                    );
                                NormalMessage.instance.AddSimpleMessage(contents);

                                JArray dataArray = itemController.dataArray;
                                for (int i = 0; i < cafeInfoList.Count; i++)
                                {
                                    if (cafeInfoList[i].info.ValueOrDefault("idx", -1) == cafeIdx)
                                    {
                                        cafeInfoList[i].info["cafeMembers"][0] = c["cafeMember"];
                                        cafeInfoList[i].cafeMembers[0] = SetCafeMember(
                                            c["cafeMember"]
                                        );
                                    }
                                }
                                if (dataArray != null)
                                {
                                    for (int i = dataArray.Count - 1; i >= 0; i--)
                                    {
                                        if (
                                            (dataArray[i] as JObject).ValueOrDefault("idx", -1)
                                            == cafeIdx
                                        )
                                        {
                                            dataArray[i]["cafeMembers"][0] = c["cafeMember"];
                                        }
                                    }
                                    itemController.Refresh();
                                }
                            }
                            if (isCurEnterCafe)
                            {
                                curEnterCafeInfo["cafeMember"] = c["cafeMember"];
                                gameList.RefreshGameList();

                                foreach (var text in dummyChipTexts)
                                {
                                    text.text = MoneyToString.Converting(memberInfo.cc);
                                }

                                if (cafeNickText)
                                {
                                    cafeNickText.text = memberInfo.nick;
                                }
                                if (cafeIdx == 2)
                                {
                                    RequestKingshillUserInfo();
                                }
                                if (isBanned || isChangePermit)
                                {
                                    popupOpener.CloseUI();
                                    LobbyTabsManager.Instance.TabTypeSet(0);
                                }
                            }
                        }

                        for (int i = 0; cafeInfo != null && i < cafeInfo.cafeMembers.Count; i++)
                        {
                            if (cafeInfo.cafeMembers[i].idx == memberInfo.idx)
                            {
                                cafeInfo.cafeMembers[i] = memberInfo;
                                //if (i == 0) isMe = true;
                            }
                        }

                        if (c.ContainsKey("memberCount")) // 없을때도 있다.
                        {
                            int memberCount = (int)c["memberCount"];
                            // TODO:  cafe memberCount 가 변경한다.
                        }
                    }
                }
                break;

            case PCProtocol.PC_TNMT_UPDATE_APPLY_COUNT:
                {
                    UpdateTournamentApplyCount(c);
                }
                break;
            case PCProtocol.PC_CAFE_TRANS_ZUICE:
                if ((int)curEnterCafeInfo["cafe"]["idx"] == (int)c["cafeIdx"])
                {
                    Cafe.instance.curEnterCafeInfo["cafe"]["zc"] = c["cafe"]["zc"];
                    cafeZuiceText.text = MoneyToString.Converting((long)c["cafe"]["zc"]);
                }
                break;
            case PCProtocol.PC_CAFE_GAME_CREATE:
                gameList.AddGameList(c["playGame"] as JObject);
                break;
            case PCProtocol.PC_TNMT_CREATE:
                {
                    var cafeIdx = c.ValueOrDefault("cafeIdx", 0);
                    if (cafeIdx != (int)curEnterCafeInfo["cafe"]["idx"])
                        break;
                    if ((int)permit >= (int)CAFE_MEMBER_PERMIT.manager)
                    {
                        Debug.Log("ddd");
                    }
                    GetCafeInfo(cafeIdx);
                }
                break;
            case PCProtocol.PC_TNMT_UPDATE:
                {
                    TnmtUpdate(c);
                }
                break;
            case PCProtocol.PC_CAFE_GAME_UPDATE:
                {
                    JObject o = c["o"] as JObject;
                    if (o != null)
                    {
                        gameList.UpdateGame(o);
                        GameListGenerator.RefreshAllGenerators();
                    }
                }
                break;

            case PCProtocol.PC_CAFE_MEMBER_ORDER:
                {
                    ERR ecode = (ERR)c.ValueOrDefault("ecode", 0);
                    if (ecode == ERR.OK)
                    {
                        if (c.ContainsKey("cafeMemberOrder"))
                        {
                            var status = (TRANSFER_TYPE)(int)c["cafeMemberOrder"]["status"];
                            if (status == TRANSFER_TYPE.sell)
                            {
                                curEnterCafeInfo["cafeMember"]["cc"] = c["cafeMember"]["cc"];
                            }
                        }
                    }

                    LoadingCircle.Instance.StopSpin();
                }
                break;
            case PCProtocol.PC_CAFE_MEMBER_ORDER_UPDATE:
                {
                    int ecode = c.ValueOrDefault("ecode", 0);
                    if (ecode != 0)
                        break;
                    if (
                        curEnterCafeInfo != null
                        && (int)c["cafeIdx"] == (int)curEnterCafeInfo["cafe"]["idx"]
                    )
                    {
                        if (c.ContainsKey("cafeMember"))
                        {
                            curEnterCafeInfo["cafeMember"] = c["cafeMember"];
                            var memberInfo = SetCafeMember(curEnterCafeInfo["cafeMember"]);
                            GetCafeList((int)c["cafeIdx"]).cafeMembers[0] = SetCafeMember(
                                c["cafeMember"]
                            );
                            gameList.RefreshGameList();

                            foreach (var text in dummyChipTexts)
                            {
                                text.text = MoneyToString.Converting(
                                    (long)curEnterCafeInfo["cafeMember"]["cc"]
                                );
                            }

                            cashierChipPopup.ShowUI(cashierChipPopup.GetCurrentScene());
                            cashierPopup?.Init();
                        }
                    }
                }
                break;
            case PCProtocol.PC_KINGSHILL_GET_USER_INFO:
                {
                    MyStatus.loungeData = c;
                    SetLoungeData();
                    
                    KingshillInfo.SetKingshillInfo(c);
                    tnmtList.RefreshGameList();
                }
                break;

            case PCProtocol.PC_KINGSHILL_TICKET_TO_POINT:
                {
                    var ticketGbn = c.ValueOrDefault("ticketGbn", 0);
                    var changeTicket = c.ValueOrDefault("changeTicket", 0);
                    var changePoint = c.ValueOrDefault("changePoint", 0);
                    var ticketName = KingshillInfo.GetTicketString(ticketGbn);
                    ticketPopup.SetActive(false);
                    if(changeTicket > 0)
                    {
                        NormalMessage.instance.AddSimpleMessage(string.Format($"{ticketName} {changeTicket}개를 {MoneyToString.Converting(changePoint)}포인트로 교환했습니다."));
                    }
                    else
                    {
                        NormalMessage.instance.AddSimpleMessage(string.Format($"{ticketName} 교환 실패"));
                    }
                }
                break;
        }
    }

    private void UpdateTournamentApplyCount(JObject c)
    {
        int tn = (int)c["tn"];
        var countAllUser = (int)c.ValueOrDefault("countAllUser", 0);
        if (c.ContainsKey("countEntry"))
        {
            countAllUser = c.ValueOrDefault("countEntry", 0);
        }
        if (curEnterCafeInfo == null)
        {
            return;
        }
        var curCafeIdx = (int)curEnterCafeInfo["cafe"]["idx"];
        var cafeTnmtList = curEnterCafeInfo.CastOrEmpty<JArray>("cafeTnmtList");
        for (int i = 0; i < cafeTnmtList.Count; ++i)
        {
            var tnData = cafeTnmtList[i] as JObject;
            var listTn = tnData.ValueOrDefault("tn", 0);
            if (tn == listTn)
            {
                var live = tnData["live"];
                live["countEntry"] = countAllUser;
                break;
            }
        }
        tnmtList.SetGameList(cafeTnmtList, curCafeIdx);
    }

    private void TnmtUpdate(JObject obj)
    {
        if (curEnterCafeInfo == null)
        {
            return;
        }
        var curCafeIdx = (int)curEnterCafeInfo["cafe"]["idx"];
        var cafeIdx = obj.ValueOrDefault("cafeIdx", 0);
        if (curCafeIdx != cafeIdx)
            return;

        var cafeTnmtList = curEnterCafeInfo["cafeTnmtList"] as JArray;
        var tn = obj.ValueOrDefault("tn", 0);
        var idx = -1;
        for (int i = 0; i < cafeTnmtList.Count; ++i)
        {
            var tnData = cafeTnmtList[i] as JObject;
            var listTn = tnData.ValueOrDefault("tn", 0);
            if (tn == listTn)
            {
                idx = i;
                break;
            }
        }
        if (idx < 0)
        {
            cafeTnmtList.AddFirst(obj);
        }
        else
        {
            var state = obj.ValueOrDefault("state", 0);
            if (state < (int)TNMT_FLOW.cancel || permit >= CAFE_MEMBER_PERMIT.manager)
            {
                cafeTnmtList[idx] = obj;
            }
            else
            {
                cafeTnmtList.Remove(cafeTnmtList[idx]);
            }
        }
        tnmtList.SetGameList(cafeTnmtList);
    }

    private async void SetLoungeData()
    {
        var data = MyStatus.loungeData;

        

        foreach (var loungDataText in loungeDataTexts)
        {
            if(loungDataText.dataType == KingshillDataType.ticketCounts)
            {
                var keys = await KingshillInfo.GetTicketList();
                var ticketCounts = new List<long>();
                for(int i = 0; i < keys.Count; i++)
                {
                    ticketCounts.Add(data.ValueOrDefault<long>(keys[i], 0));
                }
                var dataString = string.Join(" ", ticketCounts);
                loungDataText.SetData(dataString);
                continue;
            }
           
                
                switch (loungDataText.dataType)
                {
                    case KingshillDataType.number:
                        {
                            var loungeData = data.ValueOrDefault<long>(loungDataText.key, 0);
                            var dataString = MoneyToString.Converting(loungeData);
                            loungDataText.SetData(dataString);
                        }
                        break;
                    case KingshillDataType.str:
                        {
                            var loungeData = data.ValueOrDefault(loungDataText.key, string.Empty);
                            
                            loungDataText.SetData(loungeData);
                        }
                        break;
                    case KingshillDataType.agent:
                        {
                            string loungeData = data.ValueOrDefault(loungDataText.key, string.Empty, true);
                            var agentData = await KingshillInfo.GetAgentData();
                            var dataString = agentData.ValueOrDefault(
                                loungeData.ToString(),
                                string.Empty
                            );
                            loungDataText.SetData(dataString);
                        }
                        break;
                    case KingshillDataType.tickettype: { }
                        break;
                    case KingshillDataType.ticketCounts:
                        break;

                }
                
            
            
        }
        
        

        
        

        //var ticket1 = data.ValueOrDefault<long>("ticket", 0);
        //var ticket2 = data.ValueOrDefault<long>("ticket2", 0);
        //var ticket3 = data.ValueOrDefault<long>("ticket3", 0);
        //dummyTicketText.text = (ticket1 + ticket2 + ticket3).ToString();

        //ticket1text.SetLocalText("ticket1_count", ticket1.ToString());
        //ticket2text.SetLocalText("ticket2_count", ticket2.ToString());
        //ticket3text.SetLocalText("ticket3_count", ticket3.ToString());
    }

    public void CafeInfoSet()
    {
        JObject cafe = curEnterCafeInfo.ValueOrDefault("cafe", new JObject());
        int idx = cafe.ValueOrDefault("idx", 0);
        JObject cafeMember = curEnterCafeInfo.ValueOrDefault("cafeMember", new JObject());
        var memberInfo = SetCafeMember(cafeMember);
        gameList.SetGameList(curEnterCafeInfo["cafeGameList"] as JArray, idx);
        var tnmtData = curEnterCafeInfo["cafeTnmtList"] as JArray;
        tnmtList.SetGameList(tnmtData, idx);
        InfoManager.Instance.SetTnmtList(tnmtData);
        CAFE_MEMBER_PERMIT permit = (CAFE_MEMBER_PERMIT)(int)memberInfo.permit;
        bool bPermit = permit == CAFE_MEMBER_PERMIT.manager || permit == CAFE_MEMBER_PERMIT.owner;
        if (bPermit)
        {
            if (orderNoticeMark)
                orderNoticeMark.SetActive(cafe.ValueOrDefault("orderReqCount", 0) > 0);
            if (joinNoticeMark)
                joinNoticeMark.SetActive(cafe.ValueOrDefault("joinReqCount", 0) > 0);
        }

        int tier = cafe.ValueOrDefault("tier", 0);
        cafeZuiceText.text = MoneyToString.Converting((long)curEnterCafeInfo["cafe"]["zc"]);
        cafeNameTexts.ForEach(t => t.text = cafe.ValueOrDefault("name", ""));
        cafeTierText.text = ((CAFE_TIER)tier).ToString();
        this.tier = (CAFE_TIER)tier;

        for (int i = 0; i < tierVariation.Count; i++)
        {
            tierVariation[i].SetVariation(((CAFE_TIER)tier).ToString());
        }

        this.permit = permit;

        for (int i = 0; i < permitVariation.Count; i++)
        {
            permitVariation[i].SetVariation(permit.ToString());
        }

        switch (permit)
        {
            case CAFE_MEMBER_PERMIT.owner:
                if (cafePermit2)
                    cafePermit2.text = "O";
                if (cafePermit)
                    cafePermit.text = "오너";
                cafeCode.text = cafe.ValueOrDefault("pw", "");
                break;
            case CAFE_MEMBER_PERMIT.manager:
                if (cafePermit2)
                    cafePermit2.text = "M";
                if (cafePermit)
                    cafePermit.text = "메니저";
                cafeCode.text = cafe.ValueOrDefault("pw", "");
                break;
            case CAFE_MEMBER_PERMIT.member:
                if (cafePermit2)
                    cafePermit2.text = "M";
                if (cafePermit)
                    cafePermit.text = "멤버";
                cafeCode.text = cafe.ValueOrDefault("pw", "");
                break;
        }

        foreach (var text in dummyChipTexts)
        {
            text.text = MoneyToString.Converting((long)curEnterCafeInfo["cafeMember"]["cc"]);
        }
        if (cafeNickText)
        {
            cafeNickText.text = memberInfo.nick;
        }

        long borrowedChip = (long)curEnterCafeInfo["cafeMember"]["debt"];
        dummyChipDebtText.text = MoneyToString.Converting(borrowedChip);
        for (int i = 0; i < hasBorrowedChipShow.Count; i++)
        {
            // hasBorrowedChipShow[i].gameObject.SetActive(borrowedChip > 0);
        }
        cafeMemberCountText.text =
            cafe.ValueOrDefault("memberCount", 0).ToString()
            + "/"
            + cafeTierData[tier - (int)CAFE_TIER.bronze, (int)CAFE_TIER_OPTION.member_max];
        cafeGameCountText.text =
            cafe.ValueOrDefault("gameCount", 0).ToString()
            + "/"
            + cafeTierData[tier - (int)CAFE_TIER.bronze, (int)CAFE_TIER_OPTION.table_max];

        string photoUrl = "";
        if (cafe.ContainsKey("photoUrl"))
            photoUrl = cafe["photoUrl"].ToString();
        else if (cafe.ContainsKey("photourl"))
            photoUrl = cafe["photourl"].ToString();

        SetCafeLogo((int)cafe["icon"], photoUrl, (int)cafe["idx"]);
        ChatManagerInCafe.instance.SetCafeIdx((int)cafe["idx"]);

        double leftSeconds = (double)curEnterCafeInfo["cafe"]["leftSeconds"];
        bool operation = (bool)curEnterCafeInfo["cafe"]["operation"];

        //DateTime expireAt = cafe.ValueOrDefault("expireAt", DateTime.Now);
        //DateTime origin = DateTime.Now;
        //TimeSpan diff = expireAt.ToUniversalTime() - origin.ToUniversalTime();
        //leftSeconds = Math.Floor(diff.TotalSeconds);
        long timeSeconds = (long)leftSeconds;
        if (timeSeconds < 0)
            timeSeconds = 0;

        for (int i = 0; i < timeLeftTimer.Count; i++)
        {
            timeLeftTimer[i].SetTimer(operation, timeSeconds, 24 * 60 * 60);
        }

        if (cafeOptionPopup)
        {
            cafeOptionPopup.ShowUI(cafeOptionPopup.GetCurrentScene());
        }

        foreach (var obj in otherCafeItems)
        {
            obj.SetActive(idx != 2);
        }

        foreach (var obj in loungeItems)
        {
            obj.SetActive(idx == 2);
        }

        if (idx == 2)
        {
            RequestKingshillUserInfo();
        }
    }

    public async void RequestKingshillUserInfo()
    {
        await KingshillInfo.GetKingshillInfo();
        var p = new Packet(CPProtocol.CP_KINGSHILL_GET_USER_INFO);
        WebSocketManager.defaultCli.Send(p.ToJson());
    }

    public void EnterCafe(CafeInfo info)
    {
        LoadingCircle.Instance.StartSpin(CPProtocol.CP_CAFE_INFO);
        Packet p = new Packet(CPProtocol.CP_CAFE_INFO);
        p.Add("cafeIdx", (int)info.info["idx"]);
        WebSocketManager.defaultCli.Send(p);
        StartCoroutine(WaitEnter());
    }

    public void EnterCafe(int cafeIdx)
    {
        GetCafeInfo(cafeIdx);
        StartCoroutine(WaitEnter());
    }

    public void GetCafeInfo(int cafeIdx)
    {
        LoadingCircle.Instance.StartSpin(CPProtocol.CP_CAFE_INFO);
        Packet p = new Packet(CPProtocol.CP_CAFE_INFO);
        p.Add("cafeIdx", cafeIdx);
        WebSocketManager.defaultCli.Send(p);
    }

    private IEnumerator WaitEnter()
    {
        WaitForPCProtocol waiting = new WaitForPCProtocol(PCProtocol.PC_CAFE_INFO);
        yield return waiting;
        if (waiting.Result.c.ValueOrDefault("ecode", -1) == 0)
            LobbyTabsManager.Instance.CafeSet("defaltCafe");
        LoadingCircle.Instance.StopSpin();
    }

    public void RequestCafeList()
    {
        Packet p = new Packet((int)CPProtocol.CP_CAFE_LIST);
        WebSocketManager.defaultCli.Send(p.ToJson());
    }

    public void OnClickAddChip()
    {
        var idx = (int)curEnterCafeInfo["cafe"]["idx"];
        if (idx == 2)
        {
            pointPopup?.SetActive(true);
        }
        else
        {
            cashierPopup?.gameObject.SetActive(true);
            cashierPopup?.Init();
        }
    }

    public void OnClickTicket()
    {
        ticketPopup?.SetActive(true);
    }

    public void ReceiveCafeList(JObject c)
    {
        JArray temp_cafeInfoList = c["cafeList"] as JArray;
        RefreshCafeList(temp_cafeInfoList);
    }

    public void CafeListUpdate(JObject c)
    {
        JArray temp_cafeInfoList = itemController.dataArray;

        foreach (var info in c["cafeList"] as JArray)
        {
            temp_cafeInfoList.Add(info);
        }

        RefreshCafeList(temp_cafeInfoList);
    }

    private void RefreshCafeList(JArray temp_cafeInfoList)
    {
        cafeInfoList.Clear();

        for (int i = 0; i < temp_cafeInfoList.Count; i++)
        {
            var temp_cafeInfo = temp_cafeInfoList[i] as JObject;
            CafeInfo cafeInfo = new CafeInfo(temp_cafeInfo);
            cafeInfo.cafeMembers = new List<CafeMemberInfo>();
            cafeInfoList.Add(cafeInfo);
            JArray temp_cafeMemberInfo = temp_cafeInfo["cafeMembers"] as JArray;
            for (int j = 0; j < temp_cafeMemberInfo.Count; j++)
            {
                cafeInfo.cafeMembers.Add(SetCafeMember(temp_cafeMemberInfo[j]));
            }
        }

        if (!Constant.GameConfig.SHOW_FIRST_CAFE && temp_cafeInfoList.Count > 0)
        {
            JObject delInfo = null;
            foreach (JObject info in temp_cafeInfoList)
            {
                var cafeIdx = info.ValueOrDefault("idx", 0);
                if (cafeIdx == 1)
                {
                    delInfo = info;
                }
                if (cafeIdx == 2)
                {
                    GetBanner();
                }
            }
            if (delInfo != null)
            {
                temp_cafeInfoList.Remove(delInfo);
            }
        }

        itemController.dataArray = temp_cafeInfoList;
        itemController.Refresh();
    }

    public CafeMemberInfo SetCafeMember(JToken member)
    {
        var cafeMemberInfo = new CafeMemberInfo();
        cafeMemberInfo.idx = (int)member["idx"];
        cafeMemberInfo.status = (int)member["status"];
        cafeMemberInfo.permit = (int)member["permit"];
        cafeMemberInfo.type = (int)member["type"];
        cafeMemberInfo.nick = (string)member["nick"];
        cafeMemberInfo.zc = (long)member["zc"];
        cafeMemberInfo.dc = (long)member["dc"];
        cafeMemberInfo.cc = (long)member["cc"];
        cafeMemberInfo.point = (long)member["point"];
        cafeMemberInfo.debt = (long)member["debt"];
        return cafeMemberInfo;
    }

    public void CafeQuestPopupOpen()
    {
        cafeQuestPopup.RequsetQuestList((int)curEnterCafeInfo["cafe"]["idx"]);
    }

    public void CreateCafe() { }

    public void SetCafeItem(JObject data)
    {
        //if(mails == null)
        //{
        //    mails = new List<MailSlot>();
        //}

        //var list = (JArray)data["list"];
        //Console.Log(list.Count.ToString());
        //for(int i = 0; i < list.Count; i++)
        //{
        //    MailSlot slot;
        //    if(i < mails.Count)
        //    {
        //        slot = mails[i];
        //    }
        //    else
        //    {
        //        slot = Instantiate(mailSlotPrefab, contentContainer).GetComponent<MailSlot>();
        //        mails.Add(slot);
        //    }
        //    slot.manager = this;
        //    slot.SetSlot((JObject)list[i]);
        //}

        //while(mails.Count > list.Count)
        //{
        //    var m = mails[list.Count];
        //    mails.RemoveAt(list.Count);
        //    Destroy(m.gameObject);
        //}
    }

    public void RemoveSlot(int idx)
    {
        //var slot = mails.Find((MailSlot sl)=>{return sl.idx == idx;});
        //mails.Remove(slot);
        //Destroy(slot.gameObject);
    }

    public void OnClickCloseButton()
    {
        //ani.Play("ShopCloseAni");
    }

    public void CloseWindow()
    {
        gameObject.SetActive(false);
    }

    /*
    public void OnGUI()
    {
        //
#if UNITY_EDITOR

        


        if (GUI.Button(new Rect(10, 170, 100, 50), "Sit In"))
        {
            Packet p = new Packet((int)CPProtocol.CP_ROOM_SEAT);
            p.Add("gtn", 463);
            p.Add("seat", UnityEngine.Random.Range(0, 10));

            WebSocketManager.defaultCli.Send(p.ToJson());
        }
        if (GUI.Button(new Rect(10, 270, 100, 50), "Sit Out"))
        {
            Packet p = new Packet((int)CPProtocol.CP_ROOM_SEAT);
            p.Add("gtn", 463);
            p.Add("seat", -1);

            WebSocketManager.defaultCli.Send(p.ToJson());
        }

        if (GUI.Button(new Rect(10, 370, 100, 50), "JoinLink"))
        {
            Packet p = new Packet((int)CPProtocol.CP_CAFE_AUTO_JOIN_CODE);
            p.Add("cafeIdx", 18);
            p.Add("type", 1);

            WebSocketManager.defaultCli.Send(p.ToJson());
        }

#endif

    }
    */
    private JArray bannerArr;
    private bool startBanner = false;

    [SerializeField]
    RawImage bannerImage;

    public async void GetBanner()
    {
        var www = await PublisherApiManager.Instance.PostWithoutToken(
            Constant.GameConfig.KingshillUrl + Constant.GameConfig.bannerAPILink
        );
        var result = JObject.Parse(www.downloadHandler.text);
        int resultCode = result.ValueOrDefault("resultCode", 0);
        if (resultCode == 0)
        {
            bannerArr = result.CastOrEmpty<JArray>("bannerList");
            StartBanner();
        }
    }

    Dictionary<string, Texture> bannerTextureDic = new Dictionary<string, Texture>();

    public async void StartBanner()
    {
        int cur = 0;
        if (!startBanner && bannerImage)
        {
            startBanner = true;
            while (bannerArr.Count > 0)
            {
                var imageUrl = bannerArr[cur].Value<string>("url");
                var succes = await loadImageTexture(imageUrl);
                if (!succes)
                    break;
                cur++;
                cur %= bannerArr.Count;
                await UniTask.WaitForSeconds(Constant.GameConfig.bannerDuration);
            }
            if (bannerImage)
            {
                bannerImage.gameObject.SetActive(false);
            }
            startBanner = false;
        }
    }

    public async UniTask<bool> loadImageTexture(string url)
    {
        if (!bannerTextureDic.ContainsKey(url))
        {
            UnityWebRequest www = UnityWebRequestTexture.GetTexture(url);
            await www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.Log(www.error);
                return false;
            }
            else
            {
                bannerTextureDic.Add(url, ((DownloadHandlerTexture)www.downloadHandler).texture);
                if (bannerImage)
                {
                    bannerImage.gameObject.SetActive(true);
                }
            }
        }
        if (!bannerImage)
        {
            return false;
        }
        bannerImage.texture = bannerTextureDic[url];
        return true;
    }
}
