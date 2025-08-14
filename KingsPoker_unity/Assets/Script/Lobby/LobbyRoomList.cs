using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using Newtonsoft.Json.Linq;

public class LobbyRoomList : MonoBehaviour
{
    public ScrollRect scrollView = null;

    public Transform content;

    public RoomInfoItem roomInfoItemPrefab;
    public BaccaratInfoItem baccaratInfoItemPrefab;
    public TournamentInfoItem tournamentInfoItemPrefab;
    public BadugiInfoItem badugiInfoItemPrefab;

    public List<RoomInfoItem> roomInfoItemList = new List<RoomInfoItem>();
    public List<RoomInfoItem> cafeRoomItemList = new List<RoomInfoItem>();
    public List<BadugiInfoItem> badugiInfoItemList = new List<BadugiInfoItem>();
    public List<TournamentInfoItem> tournamentInfoItemList = new List<TournamentInfoItem>();
    public List<BaccaratInfoItem> baccaratInfoItemList = new List<BaccaratInfoItem>();

    private Dictionary<int, TournamentInfoItem> tournamentItems = new Dictionary<int, TournamentInfoItem>();
    //private HashSet<int> tournamentNumbers = new HashSet<int>();
    

    public int tournamentCount = 0;

    private Dictionary<string,List<RoomData>> roomDataList = new Dictionary<string, List<RoomData>>();
    [SerializeField]
    private GAME_TYPE gameType;
    [SerializeField]
    private List<CHIP_TYPE> chipTypes = new List<CHIP_TYPE> { CHIP_TYPE.zc };

    private void Awake()
    {
        GetPlayGameList(gameType);
    }
    private void RoomListDisabler(GAME_TYPE except)
    {
        //if(except != "tournament")
        //{
        //    for (int i = 0; i < tournamentInfoItemList.Count; i++)
        //    {
        //        tournamentInfoItemList[i].gameObject.SetActive(false);
        //    }
        //}
        //if (except != "baccarat")
        //{
        //    for (int i = 0; i < baccaratInfoItemList.Count; i++)
        //    {
        //        baccaratInfoItemList[i].gameObject.SetActive(false);
        //    }
        //}
        //if (except != GAME_TYPE.nlh)
        //{
        //    for (int i = 0; i < roomInfoItemList.Count; i++)
        //    {
        //        roomInfoItemList[i].gameObject.SetActive(false);
        //    }
        //}
        //if (except != "badugi")
        //{
        //    for (int i = 0; i < badugiInfoItemList.Count; i++)
        //    {
        //        badugiInfoItemList[i].gameObject.SetActive(false);
        //    }
        //}
    }

    public void GetHoldemList()
    {
        GetPlayGameList(GAME_TYPE.nlh);
    }
    public void GetPlayGameList(GAME_TYPE gameType)
    {
        this.gameType = gameType;
        RoomListDisabler(gameType);
        List<RoomData> list;
        if(!roomDataList.ContainsKey(gameType.ToString()))
        {
            Debug.Log("RoomList Load");
            var rules = JsonDataParser.Parse<List<JObject>>(RoomOptions.rules[gameType + "_list"]);
            roomDataList[gameType.ToString()] = new List<RoomData>();
            for (int i = 0; i < rules.Count; i++)
            {
                var rd = new RoomData();
                roomDataList[gameType.ToString()].Add(rd);
                rd.ante = (long)rules[i]["ante"];
                rd.sb = (long)rules[i]["small_blind"];
                rd.bb = (long)rules[i]["blind"];
                rd.emn = (long)rules[i]["buyin_min"];
                rd.level = rules[i].ValueOrDefault("level", 0);
                rd.chip_type = rules[i].ValueOrDefault("chip_type", CHIP_TYPE.cc);
                rd.gameType = gameType;
            }
        }
        list = roomDataList[gameType.ToString()];


        // long bng, sb, bb, emn;

        for (int i = 0; i < list.Count; i++)
        {
//            var time = DateTime.UtcNow.Ticks;
            // JsonDataParser.Parse(rules[i]["bng"], out bng);
            // JsonDataParser.Parse(rules[i]["sm"], out sb);
            // JsonDataParser.Parse(rules[i]["bg"], out bb);
            // JsonDataParser.Parse(rules[i]["emn"], out emn);
            var rd = list[i];
            if(chipTypes.Contains( rd.chip_type))
            {
                if (roomInfoItemList.Count > i)
                {
                    UpdateRoomInfoItem(i, rd, gameType);
                }
                else
                {
                    CreateRoomInfoItem(rd, gameType);
                }
            }
            
//            Debug.Log(string.Format("GameRoom {0} Load Time : {1}ms", i,new TimeSpan(DateTime.UtcNow.Ticks -time).Milliseconds));
        }

        if (list.Count < roomInfoItemList.Count)
        {
            for (int i = list.Count; i < roomInfoItemList.Count; i++)
            {
                roomInfoItemList[i].gameObject.SetActive(false);
            }
        }

        scrollView.StopMovement();
        scrollView.horizontalNormalizedPosition = 0;
        
    }

    public void GetBadugiRoomList(GAME_TYPE gameType)
    {
        //this.gameType = gameType;
        //RoomListDisabler(gameType);
        //List<RoomData> list;
        //if (!roomDataList.ContainsKey(gameType))
        //{
        //    Debug.Log("RoomList Load");
        //    List<JObject> rules = JsonDataParser.Parse<List<JObject>>(RoomOptions.rules[gameType + "_list"]);
        //    roomDataList[gameType] = new List<RoomData>();
        //    for (int i = 0; i < rules.Count; i++)
        //    {
        //        var rd = new RoomData();
        //        roomDataList[gameType].Add(rd);
        //        rd.bng = (long)rules[i]["bng"];
        //        rd.sb = (long)rules[i]["sm"];
        //        rd.bb = (long)rules[i]["bg"];
        //        rd.emn = (long)rules[i]["emn"];
        //    }
        //}
        //list = roomDataList[gameType];


        //// long bng, sb, bb, emn;

        //for (int i = 0; i < list.Count; i++)
        //{
        //    //            var time = DateTime.UtcNow.Ticks;
        //    // JsonDataParser.Parse(rules[i]["bng"], out bng);
        //    // JsonDataParser.Parse(rules[i]["sm"], out sb);
        //    // JsonDataParser.Parse(rules[i]["bg"], out bb);
        //    // JsonDataParser.Parse(rules[i]["emn"], out emn);
        //    var rd = list[i];
        //    if (badugiInfoItemList.Count > i)
        //    {
        //        UpdateBadugiRoomInfoItem(i, rd.bng, rd.emn, rd.sb, rd.bb, gameType);
        //    }
        //    else
        //    {
        //        CreateBadugiRoomInfoItem(rd.bng, rd.emn, rd.sb, rd.bb, gameType);
        //    }
        //    //            Debug.Log(string.Format("GameRoom {0} Load Time : {1}ms", i,new TimeSpan(DateTime.UtcNow.Ticks -time).Milliseconds));
        //}

        //if (list.Count < badugiInfoItemList.Count)
        //{
        //    for (int i = list.Count; i < badugiInfoItemList.Count; i++)
        //    {
        //        badugiInfoItemList[i].gameObject.SetActive(false);
        //    }
        //}

        //scrollView.StopMovement();
        //scrollView.horizontalNormalizedPosition = 0;

    }

    public void GetTournamentList()
    {
        gameType = GAME_TYPE.mtt;
        RoomListDisabler(gameType);

        tournamentItems.Clear();

        int tn;
        int state;
        long totalPrize;
        string title;
        long buyIn, startingChip;

        string startTime;
        string closeTime;

        long rebuyCount, rebuyCost, rebuyChip;
        long addOnCount, addOnCost, addOnChip;
        int countAllUser;
        var tournaments = InfoManager.Instance.TournamentInfoList;
        

        tournamentCount = tournaments.Count;
        
//        Debug.LogError(tournaments.Count);

        for (int i = 0; i < tournaments.Count; i++)
        {
            tn = tournaments[i].tn;
            state = tournaments[i].state;
            totalPrize = tournaments[i].totalPrize;
            title = tournaments[i].title;
            buyIn = tournaments[i].buyIn;
            startingChip = tournaments[i].startChip;
            startTime = tournaments[i].startTime;
            closeTime = tournaments[i].closeTime;
            rebuyCount = tournaments[i].rebuyCount;
            rebuyCost = tournaments[i].rebuyCount;
            rebuyChip = tournaments[i].rebuyChip;
            addOnCount = tournaments[i].addOnCount;
            addOnCost = tournaments[i].addOnCost;
            addOnChip = tournaments[i].addOnChip;
            countAllUser = tournaments[i].countAllUser;

            if (tournamentInfoItemList.Count > i)
            {
                var item = UpdateTournamentInfoItem(i, tn, (TNMT_FLOW)state, totalPrize, title, buyIn, startingChip, startTime, closeTime, rebuyCount, rebuyCost, rebuyChip, addOnCount, addOnCost, addOnChip, countAllUser);
                tournamentItems.Add(tn, item);
            }
            else
            {
                var item = CreateTournamentInfoItem(tn, (TNMT_FLOW)state, totalPrize, title, buyIn, startingChip, startTime, closeTime, rebuyCount, rebuyCost, rebuyChip, addOnCount, addOnCost, addOnChip, countAllUser);
                tournamentItems.Add(tn, item);
            }
        }

        if (tournaments.Count < tournamentInfoItemList.Count)
        {
            for (int i = tournaments.Count; i < tournamentInfoItemList.Count; i++)
            {
                tournamentInfoItemList[i].gameObject.SetActive(false);
            }
        }

        scrollView.StopMovement();
        scrollView.horizontalNormalizedPosition = 0;
    }

    public void UpdateTournamentApplyCount(int tn, long applyCount)
    {
        if(tournamentItems.ContainsKey(tn))
        {
            TournamentInfoItem item;
            tournamentItems.TryGetValue(tn, out item);
            item.countAllUserText.text = applyCount.ToString();
        }
    }

    public void UpdateTournamentList(JObject data)
    {
        if(gameType != GAME_TYPE.mtt)
            return;
        int tn = (int)data["tn"];
        int state = (int)data["state"];
        long totalPrize = (long)data["totalPrize"];
        string title = (string)data["title"];
        long buyIn = (long)data["buyin"];
        long startingChip = (long)data["startChip"];

        string startTime = (string)data["startTime"];
        string closeTime = (string)data["closeTime"];

        long rebuyCount = (long)data["rebuyCount"];
        long rebuyCost = (long)data["rebuyCost"];
        long rebuyChip = (long)data["rebuyChip"];


        long addOnCount = (long)data["addOnCount"];
        long addOnCost = (long)data["addOnCost"];
        long addOnChip = (long)data["addOnChip"];
        int countAllUser = (int)data["countAllUser"];

        if (tournamentItems.ContainsKey(tn) == true)
        {
            TournamentInfoItem item;
            tournamentItems.TryGetValue(tn, out item);
            item.Setting(tn, (TNMT_FLOW)state, totalPrize, title, buyIn, startingChip, startTime, closeTime, rebuyCount, rebuyCost, rebuyChip, addOnCount, addOnCost, addOnChip, countAllUser);
        }
        else
        {
            tournamentCount += 1;
            if (tournamentInfoItemList.Count < tournamentCount)
            {
                var item = CreateTournamentInfoItem(tn, (TNMT_FLOW)state, totalPrize, title, buyIn, startingChip, startTime, closeTime, rebuyCount, rebuyCost, rebuyChip, addOnCount, addOnCost, addOnChip, countAllUser);
                tournamentItems.Add(tn, item);

            }
            else
            {
                var item = UpdateTournamentInfoItem(tournamentCount - 1, tn, (TNMT_FLOW)state, totalPrize, title, buyIn, startingChip, startTime, closeTime, rebuyCount, rebuyCost, rebuyChip, addOnCount, addOnCost, addOnChip, countAllUser);
                tournamentItems.Add(tn, item);
            }
        }
    }


    public void UpdateTournamentApplyOrUnapply(bool isApply, int tn)
    {
        if (isApply)
        {
            MyStatus.AddTnmt(tn);
        }
        else
        {
            MyStatus.RemoveTnmt(tn);
        }

        TournamentInfoItem item;
        if (tournamentItems.TryGetValue(tn, out item))
        {
            item.UpdateApplyOrUnapply();
        }
    }
    private void CreateRoomInfoItem(RoomData rd, GAME_TYPE gameType)
    {
        RoomInfoItem item = Instantiate(roomInfoItemPrefab, content);
        roomInfoItemList.Add(item);
        item.Setting(rd, gameType);
        StartCoroutine(DelayActive(item.gameObject));
    }
    private IEnumerator DelayActive(GameObject obj)
    {
        yield return 0;
        obj.SetActive(true);
    }
    //private void CreateRoomInfoItem(long bng, long emn, long sb, long bb, GAME_TYPE gameType)
    //{
    //    RoomInfoItem item = Instantiate(roomInfoItemPrefab, content);
    //    roomInfoItemList.Add(item);
    //    item.Setting(bng, emn, sb, bb, gameType);
    //}

    private void UpdateRoomInfoItem(int index, RoomData rd, GAME_TYPE gameType)
    {
        roomInfoItemList[index].Setting(rd, gameType);
        roomInfoItemList[index].gameObject.SetActive(true);
    }
    //private void UpdateRoomInfoItem(int index, long bng, long emn, long sb, long bb, GAME_TYPE gameType)
    //{
    //    roomInfoItemList[index].Setting(bng, emn, sb, bb, gameType);
    //    roomInfoItemList[index].gameObject.SetActive(true);
    //}

    private void CreateBadugiRoomInfoItem(long bng, long emn, long sb, long bb, GAME_TYPE gameType)
    {
        BadugiInfoItem item = Instantiate(badugiInfoItemPrefab, content);
        badugiInfoItemList.Add(item);
        item.Setting(bng, emn, sb, bb, gameType);
    }

    private void UpdateBadugiRoomInfoItem(int index, long bng, long emn, long sb, long bb, GAME_TYPE gameType)
    {
        badugiInfoItemList[index].Setting(bng, emn, sb, bb, gameType);
        badugiInfoItemList[index].gameObject.SetActive(true);
    }

    private void CreateBaccaratInfoItem(int tn,int mn, int mx)
    {
        BaccaratInfoItem item = Instantiate(baccaratInfoItemPrefab, content);
        baccaratInfoItemList.Add(item);
        item.Setting(tn,mn,mx);
    }

    private void UpdateBaccaratInfoItem(int index, int tn, int mn, int mx)
    {
        baccaratInfoItemList[index].Setting(tn,mn,mx);
        baccaratInfoItemList[index].gameObject.SetActive(true);
    }

    private TournamentInfoItem CreateTournamentInfoItem(int tn, TNMT_FLOW state, long totalPrize, string title, long buyIn, long startingChip, string startTime, string closeTime, long rebuyCount, long rebuyCost, long rebuyChip, long addOnCount, long addOnCost, long addOnChip, int countAllUser)
    {
        TournamentInfoItem item = Instantiate(tournamentInfoItemPrefab, content);
        tournamentInfoItemList.Add(item);
        item.Setting(tn, state, totalPrize, title, buyIn, startingChip, startTime, closeTime, rebuyCount, rebuyCost, rebuyChip, addOnCount, addOnCost, addOnChip, countAllUser);
        return item;
    }

    private TournamentInfoItem UpdateTournamentInfoItem(int index, int tn, TNMT_FLOW state, long totalPrize, string title, long buyIn, long startingChip, string startTime, string closeTime, long rebuyCount, long rebuyCost, long rebuyChip, long addOnCount, long addOnCost, long addOnChip, int countAllUser)
    {
        tournamentInfoItemList[index].Setting(tn, state, totalPrize, title, buyIn, startingChip, startTime, closeTime, rebuyCount, rebuyCost, rebuyChip, addOnCount, addOnCost, addOnChip, countAllUser);
        //tournamentInfoItemList[index].gameObject.SetActive(true);
        
        return tournamentInfoItemList[index];
    }
}

public class RoomData
{
    public long ante, sb, bb, emn;
    public int level;
    public CHIP_TYPE chip_type;
    public GAME_TYPE gameType;
}