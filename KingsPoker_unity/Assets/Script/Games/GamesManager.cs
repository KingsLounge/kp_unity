using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.UI;

public class GamesManager : WebsocketListenBehaviour
{
    public Transform gamesParent;
    public Transform disableParent;
    public Transform tabCenter;
    public Transform playTab;
    public Transform playParent;

    //public GameObject gameTabPrefab;
    public GameObject addGameButton;
    public GameObject addGameCloseButton;
    public int currentGame;
    public static GamesManager instance { get; private set; }
    public List<GameObject> tabsList;
    public List<Game> gameList;
    private Dictionary<long, string[]> cardsList = new Dictionary<long, string[]>();
    private Dictionary<long, bool> turnList = new Dictionary<long, bool>();
    public List<GameObject> turnNotice = new List<GameObject>();
    public List<GameObject> observerIcon = new List<GameObject>();
    private List<GameObject> tableList = new List<GameObject>();

    private List<long> observeRoomList = new List<long>();

    public GameObject roomLeavePanel;
    private bool activeAddGamePanel = false;
    public Image tableHighlighter;
    private Coroutine tableHighlightCoroutine = null;

    [SerializeField]
    private float cardSetDelay = 2f;

    protected override void Awake()
    {
        base.Awake();
        instance = this;
        InfoManager.Instance.roomEnterEvent.AddListener(SetTable);
        InfoManager.Instance.roomLeaveEvent.AddListener(RoomLeave);
        //SetGames();
    }

    public void SetGames()
    {
        //SetTable(InfoManager.enterRn);
    }

    protected override void ReceivePacket(Packet packet)
    {
        base.ReceivePacket(packet);
        int p = packet.p;
        JObject c = packet.c;

        switch ((PCProtocol)p)
        {
            case PCProtocol.PC_HOLDEM_DRAW_CARDS: // 203 PC_HOLDEM_DRAW_CARDS
                {
                    int kind = (int)c["kind"];
                    string gid = (string)c["gid"];
                    if (kind == 0 && gid == MyStatus.gid)
                    {
                        CardSet(c);
                    }
                }
                break;
            case PCProtocol.PC_ROOM_COMMAND:
                {
                    int command = (int)c["command"];
                    int gtn = (int)c["gtn"];
                    if ((ROOM_COMMAND)command == ROOM_COMMAND.end)
                    {
                        cardsList.Remove(gtn);
                        SetTabs();
                    }
                }
                break;
            case PCProtocol.PC_HOLDEM_COMMAND:
                {
                    int gtn = (int)c["gtn"];
                    turnList.Set(gtn, false);
                }
                break;
            case PCProtocol.PC_HOLDEM_WHO_IS_TURN:
                {
                    int gtn = (int)c["gtn"];
                    int seat = (int)c["seat"];
                    RoomStatus room = InfoManager.Instance.GetRoom(gtn);
                    if (room != null && room.userList.Count > 0)
                    {
                        RoomUserData info = room.userList.Find(value => value.seat == seat);
                        if (info != null)
                        {
                            turnList.Set(gtn, info.gid == MyStatus.gid);
                        }
                    }
                    SetTabs();
                }
                break;
            case PCProtocol.PC_ROOM_LEAVE:
                {
                    string gid = c["gid"].ToString();
                    int gtn = (int)c["gtn"];
                    if (gid == MyStatus.gid)
                    {
                        ERR reason = c.ValueOrDefault<ERR>("reason", ERR.OK);
                        switch (reason)
                        {
                            case ERR.OK:
                            case ERR.CANCEL_TNMT:
                            case ERR.TNMT_UNAPPLY:
                                RoomRemove(gtn);
                                break;
                            case ERR.MOVE_ROOM:
                                RemoveTableAfterNewTable(gtn);
                                break;
                            default:
                                RoomRemove(gtn);
                                break;
                        }
                    }
                }
                break;
            case PCProtocol.PC_HOLDEM_STATUS:
                {
                    var room_command = (int)c["room_command"];
                    if (
                        room_command >= (int)ROOM_COMMAND.start
                        && room_command <= (int)ROOM_COMMAND.calc_dividend
                    )
                    {
                        try
                        {
                            int gtn = (int)c["gtn"];
                            int idx = -1;
                            for (int i = 0; i < tableList.Count; i++)
                            {
                                if (!tableList[i])
                                {
                                    Console.Log($"tablelist {i} is null");
                                    continue;
                                }

                                var table = tableList[i].GetComponentInChildren<TableManager>();
                                if (table)
                                {
                                    if (table.GetRoomNumber() == gtn)
                                    {
                                        idx = i;
                                    }
                                }
                            }
                            if (idx != -1)
                            {
                                JArray playersJson = c["players"] as JArray;
                                for (int i = 0; i < playersJson.Count; i++) // 다른사람들 카드세팅
                                {
                                    JObject playerJson = playersJson[i] as JObject;
                                    if (playerJson.ValueOrDefault("gid", "") == MyStatus.gid)
                                    {
                                        cardsList.Set(
                                            gtn,
                                            playerJson
                                                .ValueOrDefault<string>("cards", "")
                                                .SplitAndTrimAll(',')
                                        );
                                        SetTabs();
                                        break;
                                    }
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            Debug.LogError(e);
                        }
                    }
                }
                break;
        }
    }

    public void RoomRemove(int gtn)
    {
        cardsList.Remove(gtn);
        turnList.Remove(gtn);
        SetTabs();
    }

    public async void RemoveTableAfterNewTable(int gtn)
    {
        await new WaitForPCProtocol(PCProtocol.PC_PLAY_GAME_ENTER);

        Console.Log("RemoveTableAfterNewTable - 2초 대기");
        await UniTask.Delay(TimeSpan.FromSeconds(2.0));
        Console.Log("RemoveTableAfterNewTable - 2초 끝");

        // 무한루프에 빠질수 있음..
        // 1. PC_PLAY_GAME_ENTER 패킷을 받지 못할 경우    ==>

        RoomRemove(gtn);
    }

    public void SetTable(int gtn, GAME_TYPE game_type)
    {
        var idx = GetTableIdx(gtn);

        if (idx < 0)
        {
            var roomStatus = InfoManager.Instance.GetRoom(gtn);
            var tn = roomStatus.tn;
            GAME_TYPE gameType = GAME_TYPE.nothing;
            if (tn == 0)
            {
                gameType = game_type;
            }
            else
            {
                gameType = GAME_TYPE.mtt;
            }
            GameObject prefab = gameList
                .Find(
                    (Game g) =>
                    {
                        return g.gameType == gameType;
                    }
                )
                .gamePrefab;
            GameObject game = Instantiate(prefab, gamesParent);

            TableManager tableManager = game.GetComponentInChildren<TableManager>();
            tableManager.SetRoomNumber(gtn);
            tableManager.SetGameType(gameType);

            tableList.Add(game);
            SetTabs();
            tabsList[tableList.Count - 1].GetComponent<Toggle>().isOn = true;
        }
        else
        {
            tabsList[idx].GetComponent<Toggle>().isOn = true;
        }
        OnClickCloseAddGamePanel();
    }

    public int GetTableIdx(int gtn)
    {
        var idx = tableList.FindIndex(
            (table) =>
            {
                var tm = table.GetComponent<TableManager>();
                return tm.GetRoomNumber() == gtn;
            }
        );
        return idx;
    }

    public List<long> GetTableList()
    {
        var gtns = new List<long>();
        foreach (var table in tableList)
        {
            var tm = table.GetComponent<TableManager>();
            gtns.Add(tm.GetRoomNumber());
        }
        return gtns;
    }

    public bool CanJoinInspection(int gtn)
    {
        bool contains = false;
        int idx = -1;
        for (int i = 0; i < tableList.Count; i++)
        {
            if (tableList[i].GetComponentInChildren<TableManager>().GetRoomNumber() == gtn)
            {
                idx = i;
                contains = true;
            }
        }
        if (contains)
        {
            //못들어감 연출
            if (tableHighlightCoroutine != null)
            {
                StopCoroutine(tableHighlightCoroutine);
            }
            tableHighlightCoroutine = StartCoroutine(TableButtonHighlight(idx));
        }
        return !contains;
    }

    private async void CardSet(JObject c)
    {
        await UniTask.WaitForSeconds(cardSetDelay);
        cardsList.Set((int)c["gtn"], ((string)c["cards"]).Split(','));
        SetTabs();
    }

    private IEnumerator TableButtonHighlight(int idx)
    {
        tableHighlighter.gameObject.SetActive(true);
        tableHighlighter.transform.position = tabsList[idx].transform.position;
        tableHighlighter.fillAmount = 0f;
        tableHighlighter.transform.localScale = new Vector3(1f, 1f, 1f);
        while (tableHighlighter.fillAmount < 1f)
        {
            tableHighlighter.fillAmount += Time.deltaTime * 4;
            yield return null;
        }
        tableHighlighter.fillAmount = 1f;
        tableHighlighter.transform.localScale = new Vector3(-1f, 1f, 1f);
        while (tableHighlighter.fillAmount > 0f)
        {
            tableHighlighter.fillAmount -= Time.deltaTime * 4;
            yield return null;
        }
        tableHighlighter.fillAmount = 0f;
        tableHighlighter.gameObject.SetActive(false);
    }

    public void RoomLeave(JObject c, bool isObserve)
    {
        int gtn = c["gtn"].ToObject<int>();
        string gid = isObserve ? MyStatus.gid : c["gid"].ToObject<string>();
        bool leave = isObserve ? true : c.ValueOrDefault("leave", true); //leave가 없을경우, PC_ROOM_RESERVED_LEAVE가 아니라 PC_ROOM_LEAVE이므로 default값은 true

        //if(isObserve)
        //{
        //    RoomRemove(gtn);
        //}

        if (leave)
        {
            if (gid == MyStatus.gid)
            {
                RemoveGameTable(gtn);
            }
        }
    }

    public void RemoveGameTable(long gtn)
    {
        turnList.Remove(gtn);
        cardsList.Remove(gtn);
        for (int i = 0; i < tableList.Count; i++)
        {
            var table = tableList[i].GetComponent<TableManager>();
            if (gtn == table.GetRoomNumber())
            {
                tableList.RemoveAt(i);
                Destroy(table.gameObject);
                if (tableList.Count <= 0)
                {
                    LobbyTabsManager.Instance.LobbyEnable();
                }
                else
                {
                    if (i == currentGame)
                    {
                        var idx = currentGame - 1;
                        if (idx < 0)
                        {
                            idx = tableList.Count - 1;
                        }
                        tabsList[idx].GetComponent<Toggle>().isOn = true;
                        EnableGame(idx);
                    }
                }
                SetTabs();
                return;
            }
        }
    }

    public void SetTabs()
    {
        for (int i = 0; i < tabsList.Count; i++)
        {
            bool active = i < tableList.Count;
            var tab = tabsList[i];
            if (!tab)
            {
                continue;
            }
            tab.SetActive(active);
            if (active)
            {
                var table = tableList[i];
                if (!table)
                {
                    continue;
                }
                TableManager tableManager = table.GetComponent<TableManager>();
                if (!tableManager)
                {
                    continue;
                }

                Variation[] variations = tab.GetComponentsInChildren<Variation>();
                for (int j = 0; j < variations.Length; j++)
                {
                    variations[j].SetVariation(tableManager.GetTableVariationString());
                }
                long gtn = tableManager.GetRoomNumber();
                Card[] dummyCards = tab.GetComponentsInChildren<Card>(true);
                string[] cards = cardsList.ContainsKey(gtn) ? cardsList[gtn] : new string[0];
                for (int j = 0; j < dummyCards.Length; j++)
                {
                    bool cardActive = j < cards.Length && !string.IsNullOrEmpty(cards[j]);
                    dummyCards[j].gameObject.SetActive(cardActive);
                    if (cardActive)
                    {
                        dummyCards[j].SetCard(cards[j]);
                    }
                }

                bool turn = false;
                if (turnList.ContainsKey(gtn))
                {
                    turn = turnList[gtn];
                }

                if (turnNotice[i])
                {
                    turnNotice[i].SetActive(turn);
                }

                var roomStatus = InfoManager.Instance.GetRoom(gtn);

                if (roomStatus != null)
                {
                    observerIcon[i].SetActive(roomStatus.isObserve);
                }
            }
        }
    }

    public bool CheckGameIndex(int idx)
    {
        return idx < tableList.Count && idx >= 0;
    }

    public void SelectGame(int idx)
    {
        if (currentGame == idx)
        {
            if (!activeAddGamePanel)
            {
                roomLeavePanel.SetActive(true);
                var tr = roomLeavePanel.GetComponent<RectTransform>();
                var pos = tr.anchoredPosition;
                pos.x = tabsList[idx].transform.localPosition.x;
                if (pos.x - (tr.rect.width / 2) < 0)
                {
                    pos.x = (tr.rect.width / 2);
                }
                tr.anchoredPosition = pos;
                var pm = tableList[idx].GetComponent<PlayerManager>();
                string state = string.Empty;
                if (pm.im_seat)
                {
                    if (pm.reserveLeave)
                    {
                        state = "exit";
                    }
                    else if (pm.reserveSitOut)
                    {
                        state = "reserve_sit_out";
                    }
                    else
                    {
                        state = "sit_in";
                    }
                }
                else
                {
                    state = "sit_out";
                }
                var room = InfoManager.Instance.GetRoom(pm.roomNumber);
                if (room?.tn > 0)
                {
                    state = "tnmt";

                    if (room.isObserve)
                    {
                        state = "tnmt_ob";
                    }
                }
                var variations = tr.GetComponentsInChildren<Variation>(true);
                foreach (var variation in variations)
                {
                    variation.SetVariation(state);
                }
            }
            //Debug.LogWarning("sameTable");
        }
        else
        {
            roomLeavePanel.SetActive(false);
            StartCoroutine(MoveGame(idx < currentGame, 0.1f, idx));
        }
        OnClickCloseAddGamePanel();
    }

    public IEnumerator MoveGame(bool left, float time, int idx)
    {
        Transform nex = tableList[idx].transform;
        Transform cur = null;
        nex.SetParent(gamesParent);
        if (CheckGameIndex(currentGame))
        {
            cur = tableList[currentGame].transform;
        }

        float to = left ? Screen.width : -Screen.width;
        float from = left ? -Screen.width : Screen.width;
        float t = 0;
        while (t < time)
        {
            yield return 0;
            t += Time.deltaTime;
            if (cur)
            {
                var pos1 = cur.localPosition;
                pos1.y = 0;
                pos1.x = Mathf.Lerp(0, to, t / time);
                cur.localPosition = pos1;
            }

            var pos2 = nex.localPosition;
            pos2.y = 0;
            pos2.x = Mathf.Lerp(from, 0, t / time);
            nex.localPosition = pos2;
        }

        if (CheckGameIndex(currentGame))
        {
            DisableGame(currentGame);
        }

        EnableGame(idx);
    }

    public void RoomLeaveButton()
    {
        tableList[currentGame].GetComponent<PlayerManager>().OnClickLeaveButton();
    }

    public void ObserverLeaveButton()
    {
        tableList[currentGame].GetComponent<PlayerManager>().OnClickObserverLeaveButton();
    }

    public void SeatOutButton()
    {
        tableList[currentGame].GetComponent<PlayerManager>().OnClickSeatOutButton();
    }

    public void SeatInButton()
    {
        tableList[currentGame].GetComponent<PlayerManager>().OnClickSeatOutCancel();
    }

    public void AddObserverRooms()
    {
        tableList.ForEach(
            (g) =>
            {
                var gtn = g.GetComponent<TableManager>().GetRoomNumber();
                var roomStatus = InfoManager.Instance.GetRoom(gtn);

                if (roomStatus.isObserve)
                {
                    Packet p = new Packet((int)CPProtocol.CP_PLAY_GAME_LEAVE_OBSERVER);
                    p.Add("gtn", gtn);
                    WebSocketManager.defaultCli.Send(p.ToJson());

                    observeRoomList.Add(gtn);
                }
                // Destroy(g);
            }
        );

        //cardsList.Clear();
        //tableList.Clear();
        //currentGame = 0;
        //if (LobbyTabsManager.Instance)
        //{
        //    LobbyTabsManager.Instance.LobbyEnable();
        //}
    }

    public List<long> GetObserverRooms()
    {
        List<long> rooms = new List<long>();
        rooms = observeRoomList.ToList();
        return rooms;
    }

    public void ClearObserverRooms()
    {
        //foreach (var gtn in observeRoomList)
        //{
        //    Packet CP_PLAY_GAME_ENTER = new Packet((int)CPProtocol.CP_PLAY_GAME_ENTER_OBSERVER);
        //    CP_PLAY_GAME_ENTER.Add("gtn", gtn);

        //    WebSocketManager.defaultCli.Send(CP_PLAY_GAME_ENTER);
        //}

        observeRoomList.Clear();
    }

    public void DisableGame(int idx)
    {
        var tr = tableList[idx].transform;
        tr.SetParent(disableParent);
        tr.localPosition = Vector3.zero;
        var table = tr.GetComponent<TableManager>();
        table.activeOn = false;
        table.activeTable = false;
    }

    public async void EnableGame(int idx)
    {
        var beforeIdx = currentGame;
        currentGame = idx;
        var tr = tableList[idx].transform;
        tr.SetParent(gamesParent);
        tr.localPosition = Vector3.zero;
        var table = tr.GetComponent<TableManager>();
        table.activeOn = true;
        await UniTask.WaitForSeconds(0.5f);
        table.activeTable = table.activeOn;
        if (addGameButton)
        {
            addGameButton.SetActive(tableList.Count < tabsList.Count);
            if (beforeIdx != idx)
            {
                roomLeavePanel.SetActive(false);
            }
        }
    }

    public void SetHall()
    {
        GAME_TYPE gameType = InfoManager.enterGameType;
        GameObject prefab = gameList
            .Find(
                (Game g) =>
                {
                    return g.gameType == gameType;
                }
            )
            .gamePrefab;
        GameObject game = Instantiate(prefab, Vector3.zero, Quaternion.identity);
    }

    protected override void OnDestroy()
    {
        InfoManager.Instance.roomEnterEvent.RemoveListener(SetTable);
        InfoManager.Instance.roomLeaveEvent.RemoveListener(RoomLeave);
    }

    public void OnClickAddGame()
    {
        LobbyTabsManager.Instance.LobbyEnable(0f, 0f, 150f, 0f);
        activeAddGamePanel = true;
        //playTab.SetParent(playParent);
        //playTab.localPosition = Vector3.zero;
        if (addGameCloseButton)
            addGameCloseButton.SetActive(true);
        if (currentGame < tableList.Count && currentGame >= 0)
        {
            DisableGame(currentGame);
        }
    }

    public void OnClickCloseAddGamePanel()
    {
        LobbyTabsManager.Instance.LobbyDisable();
        activeAddGamePanel = false;
        if (addGameCloseButton)
            addGameCloseButton.SetActive(false);
        if (currentGame < tableList.Count && currentGame >= 0)
        {
            EnableGame(currentGame);
        }
        //playTab.SetParent(LobbyTabsManager.Instance.tabType == 0?tabCenter: null);
        //playTab.localPosition = Vector3.zero;
    }
}

[Serializable]
public struct Game
{
    public GAME_TYPE gameType;

    public GameObject gamePrefab;
}
