using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Spine;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UI;

public class HoldemCardManager : CardManager
{
    [System.Serializable]
    public class CommCardList
    {
        [SerializeField]
        private List<Card> list = new List<Card>();
        public Card this[int idx]
        {
            get => list[idx];
        }
        public int Count
        {
            get => list.Count;
        }
    }

    [System.Serializable]
    public class OpenCardButtonData
    {
        public List<GAME_TYPE> game_type = new List<GAME_TYPE>();
        public Button button;
        public List<Card> cards = new List<Card>();
        public USER_SHOW_CARDS dir;
    }

    public GameObject cardEffectorFrefab;
    public List<CommCardList> cards = new List<CommCardList>();
    private bool showdown = false;
    private bool isOpened = false;
    private float openTime = 0;
    public Animator showdownAnim;
    public GameObject resultPanel;
    public SkeletonGraphic congratulations;
    public Text resultHands;
    public GameObject showdownWindow;
    public Player player;
    public float cardOpenDelay = 0f;
    public float cardGetDelay = 0f;

    [Space]
    [Header("3D매니저와 겹치지 않도록 주의")]
    public GameObject cardFocusingPanel = null;
    public CardFocusing cardFocusing = null;

    [Space]
    private POKER_FLOW stage = POKER_FLOW.none; // Pre-flop, turn, river만

    [Space]
    public List<OpenCardButtonData> openCardButtons = new List<OpenCardButtonData>();

    private Coroutine showDownCorou = null;

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

    public List<KeyValuePair<string, string>> hands = new List<KeyValuePair<string, string>>();

    [SerializeField]
    private string holeCards = "";

    [SerializeField]
    public string commCards { private set; get; } = "";

    protected override void Awake()
    {
        base.Awake();
        openCardButtons.ForEach(d =>
        {
            d.button.onClick.AddListener(() =>
            {
                OnClickOpenCardButton(d.dir);
            });
        });
        StatusSet();
    }

    protected override void ReceivePacket(Packet packet)
    {
        base.ReceivePacket(packet);
        int p = packet.p;
        JObject c = packet.c;

        switch ((PCProtocol)p)
        {
            case PCProtocol.PC_HOLDEM_DRAW_CARDS: // 203 PC_HOLDEM_DRAW_CARDS
                if (!RoomNumberCheck(c, roomNumber))
                {
                    break;
                }

                {
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
                        if (drawCardCo != null)
                        {
                            CancelDrawCards();
                        }
                        drawCardCo = StartCoroutine(GetUserCard(gid, cards));
                    }
                    else
                    {
                        showDownCorou = StartCoroutine(
                            ShowdownOpenCard(openTime, kind, cardArr, runItTwiceIdx, odds)
                        );
                        if (player)
                        {
                            //(player as HoldemPlayer).SetHandRank(holeCards, commCards);
                        }
                    }
                    if (showdown)
                    {
                        openTime += 2f;
                    }
                }
                break;
            case PCProtocol.PC_HOLDEM_OPEN_CARDS: // 카드깡
                if (!RoomNumberCheck(c, roomNumber))
                {
                    break;
                }

                //if(showdown)
                {
                    hands.Add(
                        new KeyValuePair<string, string>(
                            c["gid"].ToString(),
                            c["strHands"].ToString().Replace(',', ' ')
                        )
                    );
                }
                break;
            case PCProtocol.PC_ROOM_USER_SHOW_CARDS:
                if (!RoomNumberCheck(c, roomNumber))
                {
                    break;
                }
                if (c.ValueOrDefault("ecode", 0) != 0)
                {
                    break;
                }

                {
                    string gid = c.ValueOrDefault("gid", "");
                    if (!string.IsNullOrEmpty(gid))
                    {
                        Player player = playerManager.FindPlayerWithGid(gid);
                        if (player != null)
                        {
                            if (gid == MyStatus.gid)
                            {
                                player.ShowCard(c.ValueOrDefault("cards", "").Split(','));
                            }
                            else
                            {
                                player.OpenCards(c.ValueOrDefault("cards", "").Split(','));
                            }
                        }
                    }
                }
                break;
            case PCProtocol.PC_HOLDEM_COMMAND: //PC_HOLDEM_COMMAND
                if (!RoomNumberCheck(c, roomNumber))
                {
                    break;
                }
                if (true)
                {
                    int command = (int)c["command"];
                    HoldemCommand((POKER_FLOW)command);
                }
                break;
            case PCProtocol.PC_HOLDEM_GAMERESULT: //PC_HOLDEM_GAME_RESULT
                if (!RoomNumberCheck(c, roomNumber))
                {
                    break;
                }
                if (true)
                {
                    List<JObject>[] gameResults = JsonDataParser.Parse<List<JObject>[]>(
                        c["list"]["item"]
                    );
                    StartCoroutine(PlayResult(gameResults));
                }
                break;
            case PCProtocol.PC_HOLDEM_STATUS:
                if (!RoomNumberCheck(c, roomNumber))
                {
                    break;
                }
                StatusSet();

                break;
            case PCProtocol.PC_HOLDEM_EQUITY:
                if (!RoomNumberCheck(c, roomNumber))
                {
                    break;
                }

                {
                    JArray odds = c.CastOrEmpty<JArray>("odds");
                    for (int i = 0; i < odds.Count; i++)
                    {
                        //JObject curOdds = odds[i] as JObject;
                        //Player player = playerManager.FindPlayerWithSeat((int)curOdds["seat"]);
                        //if (player)
                        //{
                        //    player.ShowRate(curOdds.ValueOrDefault("win", 0f), curOdds.ValueOrDefault("tie", 0f));
                        //}
                    }
                }
                break;
            case PCProtocol.PC_ROOM_RABBIT:
                if (!RoomNumberCheck(c, roomNumber))
                {
                    break;
                }

                {
                    if (c.ValueOrDefault("ecode", -1) != 0)
                        break;
                    string[] cardList = c.ValueOrDefault("cards", "").Split(',');
                    switch (stage)
                    {
                        case POKER_FLOW.none:
                        case POKER_FLOW.betstage_preflop:
                            for (int i = 0; i < 3; i++)
                            {
                                cards[0][i].SetCard(cardList[i], false);
                                cards[0][i].Flip();
                            }
                            break;
                        case POKER_FLOW.betstage_flop:
                            cards[0][3].SetCard(cardList[3], false);
                            cards[0][3].Flip();
                            break;
                        case POKER_FLOW.betstage_turn:
                            cards[0][4].SetCard(cardList[4], false);
                            cards[0][4].Flip();
                            break;
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
        var rs = InfoManager.Instance.GetRoom(roomNumber);
        stage = POKER_FLOW.none;

        GameEnd();

        for (int i = 0; i <= rs.flow; i++)
        {
            POKER_FLOW flow = (POKER_FLOW)i;
            if (
                flow == POKER_FLOW.betstage_preflop
                || flow == POKER_FLOW.betstage_flop
                || flow == POKER_FLOW.betstage_turn
                || flow == POKER_FLOW.betstage_river
            )
                stage = flow;
        }
        StopAllCoroutines();
        var roomCommand = (ROOM_COMMAND)rs.room_command;
        if (roomCommand >= ROOM_COMMAND.start && roomCommand <= ROOM_COMMAND.calc_dividend) // 20
        {
            SetCommunityCard(rs.flow, rs.commcards, false); // 커뮤카드세팅

            hands.Clear();
            showdown = (POKER_FLOW)rs.flow == POKER_FLOW.show_down;

            var players = rs.playerList;
            JObject userData;

            bool showDownSkip =
                (players.Count > 0) && (!string.IsNullOrEmpty(commCards)) && showdown;

            //                    Debug.LogError(playersJson.ToJson());
            for (int i = 0; i < players.Count; i++) // 다른사람들 카드세팅
            {
                userData = players[i] as JObject;
                Player player = playerManager.FindPlayerWithGid(userData["gid"].ToString());
                if (!player)
                {
                    RoomReenter();
                    Console.Log("플레이어가 없음");
                    showDownSkip = false;
                    break;
                }

                //                        Debug.Log(string.Format("card player gid : {0}  :  {1}",playerJson["gid"].ToString(), player));
                var cards = userData.ValueOrDefault<string>("cards", "");
                string[] cardStr = cards.SplitAndTrimAll(',');
                bool backCard = false;
                bool validData = true;
                for (int j = 0; j < cardStr.Length; j++)
                {
                    if (string.IsNullOrEmpty(cardStr[j]))
                    {
                        validData = false;
                    }
                    else if (cardStr[j] == "**")
                    {
                        backCard = true;
                    }
                }
                if (validData)
                {
                    if (backCard || player.gid == MyStatus.gid)
                    {
                        player.SetCard(cardStr);
                    }
                    else
                    {
                        player.OpenCards(cardStr);
                    }
                }
                else
                {
                    (player as HoldemPlayer).ClearCard();
                }

                if (showdown)
                {
                    var pokerBetType = userData.ValueOrDefault("last_bettype", POKER_BETTYPE.none);
                    if (
                        !backCard
                        && pokerBetType != POKER_BETTYPE.die
                        && pokerBetType != POKER_BETTYPE.none
                    )
                    {
                        hands.Add(
                            new KeyValuePair<string, string>(player.gid, cards.Replace(",", " "))
                        );
                    }
                }
                //string[] cardStrArr = cardStr.SplitAndTrimAll(',');
            }

            if (showDownSkip)
            {
                Console.Log("------------ShowDownSkip-----------");

                if (showDownCorou != null)
                {
                    StopCoroutine(showDownCorou);
                }

                ShowPokerOddsRatio();
            }
        }
        else { }
    }

    private async void RoomReenter()
    {
        InfoManager.Instance.RoomReenter(roomNumber);
    }

    private void OnClickOpenCardButton(USER_SHOW_CARDS dir)
    {
        Packet p = new Packet(CPProtocol.CP_ROOM_USER_SHOW_CARDS);
        p.Add("gtn", roomNumber);
        p.Add("type", (int)dir);
        WebSocketManager.defaultCli.Send(p);

        openCardButtons.ForEach(d =>
        {
            if (d.button.gameObject.activeSelf)
                d.button.gameObject.SetActive(false);
        });
    }

    private IEnumerator PlayResult(List<JObject>[] gameResults)
    {
        showdownAnim.gameObject.SetActive(false);
        showdownWindow.SetActive(false);
        resultPanel.SetActive(true);

        GAME_TYPE game_type = InfoManager.Instance.GetRoom(roomNumber).game_type;

        if (playerManager.IsPlayingGamer(MyStatus.gid))
        {
            var finalPlayer = playerManager
                .GetPlayingGamer()
                .FindAll(
                    (p) =>
                    {
                        return !p.isDie;
                    }
                );

            Player player = playerManager.FindPlayerWithGid(MyStatus.gid);
            if (player.isDie || finalPlayer.Count == 1)
            {
                if (stage != POKER_FLOW.betstage_river)
                {
                    switch (stage)
                    {
                        case POKER_FLOW.none:
                        case POKER_FLOW.betstage_preflop:
                            for (int i = 0; i < 3; i++)
                            {
                                cards[0][i].RabbitMode(true);
                                cards[0][i].onClick = OnClickRabbitCard;
                                cards[0][i].gameObject.SetActive(true);
                            }
                            break;
                        case POKER_FLOW.betstage_flop:
                            cards[0][3].RabbitMode(true);
                            cards[0][3].onClick = OnClickRabbitCard;
                            cards[0][3].gameObject.SetActive(true);
                            break;
                        case POKER_FLOW.betstage_turn:
                            cards[0][4].RabbitMode(true);
                            cards[0][4].onClick = OnClickRabbitCard;
                            cards[0][4].gameObject.SetActive(true);
                            break;
                    }
                }
                openCardButtons.ForEach(
                    (OpenCardButtonData d) =>
                    {
                        d.button.gameObject.SetActive(
                            Array.IndexOf(d.game_type.ToArray(), game_type) != -1
                        );
                        List<string> cards = player.GetCard();
                        for (int i = 0; i < cards.Count; i++)
                        {
                            if (d.cards.Count > i && d.cards[i] != null)
                            {
                                d.cards[i].SetCard(cards[i]);
                            }
                        }
                    }
                );
            }
        }

        for (int i = 0; i < gameResults.Length; i++)
        {
            List<JObject> result = gameResults[i];
            if (result.Count == 0)
                continue;

            /*
            if(game_type != GAME_TYPE.plo)
            {
                RunItTwiceCardHighlight(i);
            }
            else
            {
                List<JObject> winners = result.FindAll(value => value.ValueOrDefault("winner", false));
                string sorted = "";
                for(int j = 0; j < winners.Count; j++)
                {
                    if (sorted != "") sorted += ",";
                    sorted += winners[j].ValueOrDefault("sorted", "");
                }
                if (sorted != "")
                    CardHighlight(0, sorted.Split(','));
            }
            */
            List<JObject> winners = result.FindAll(value => value.ValueOrDefault("winner", false));
            string sorted = "";
            for (int j = 0; j < winners.Count; j++)
            {
                if (sorted != "")
                    sorted += ",";
                sorted += winners[j].ValueOrDefault("sorted", "");
            }
            if (sorted != "")
                CardHighlight(0, sorted.Split(','));
            yield return new WaitForSeconds(1.8f);
            CardHighlightOff();
            if (i < gameResults.Length - 1)
                yield return new WaitForSeconds(0.2f);
        }
    }

    private void OnClickRabbitCard()
    {
        Packet p = new Packet(CPProtocol.CP_ROOM_RABBIT);
        p.Add("gtn", roomNumber);
        WebSocketManager.defaultCli.Send(p);
        for (int i = 0; i < cards[0].Count; i++)
        {
            cards[0][i].onClick = null;
        }
    }

    private void RunItTwiceCardHighlight(int idx)
    {
        List<Card> highlightCards = new List<Card>();
        for (int i = 0; i < cards[0].Count; i++)
        {
            highlightCards.Add(cards[0][i]);
        }
        for (int i = 0; i < cards[idx].Count; i++)
        {
            if (cards[idx][i].gameObject.activeInHierarchy)
            {
                highlightCards[i] = cards[idx][i];
            }
        }
        for (int i = 0; i < cards.Count; i++)
        {
            for (int j = 0; j < cards[i].Count; j++)
            {
                cards[i][j].gray = !highlightCards.Contains(cards[i][j]);
            }
        }
    }

    private void RunItTwiceCardHighlightOff()
    {
        for (int i = 0; i < cards.Count; i++)
        {
            for (int j = 0; j < cards[i].Count; j++)
            {
                cards[i][j].gray = false;
            }
        }
    }

    public void CardHighlight(int type, string[] list)
    {
        List<Player> players = playerManager.GetPlayingGamer();
        for (int i = 0; i < players.Count; i++)
        {
            players[i].CardHighlight(list, type);
        }
        for (int i = 0; i < cards.Count; i++)
        {
            for (int j = 0; j < cards[i].Count; j++)
            {
                cards[i][j].gray = System.Array.IndexOf(list, cards[i][j].GetCardData()) == -1;
                cards[i][j].Highlight(!cards[i][j].gray);
            }
        }
    }

    public void CardHighlightOff(bool onlyHighlight = false)
    {
        List<Player> players = playerManager.GetPlayingGamer();
        for (int i = 0; i < players.Count; i++)
        {
            players[i].CardHighlightOff(onlyHighlight);
        }
        for (int i = 0; i < cards.Count; i++)
        {
            //cards[i].Highlight(System.Array.IndexOf(list, cards[i].GetCardData()) != -1, type);
            for (int j = 0; j < cards[i].Count; j++)
            {
                cards[i][j].Highlight(false);
                if (!onlyHighlight)
                    cards[i][j].gray = false;
            }
        }
    }

    //    private void StartCardSet()
    //    {
    //        var data = InfoManager.Instance.GetRoom(roomNumber);
    //        if (!System.String.IsNullOrEmpty(data.commcards))
    //        {
    //            SetCommunityCard(data.room_command, data.commcards); // 커뮤카드세팅
    //        }

    //        if(data.room_command >= 20)
    //        {
    //            var players = data.userList;
    ////                    Debug.LogError(playersJson.ToJson());
    //            for (int i = 0; i < players.Count; i++) // 다른사람들 카드세팅
    //            {
    //                var playerData = players[i];
    //                Player player = playerManager.FindPlayerWithGid(playerData.gid);
    ////                        Debug.Log(string.Format("card player gid : {0}  :  {1}",playerJson["gid"].ToString(), player));
    //                if (player != null)
    //                {
    //                    string[] cardStr;
    //                    try{
    //                        cardStr = playerData.cards.Split(',');
    //                    }
    //                    catch
    //                    {
    //                        cardStr = new string[0];
    //                    }

    //                    //string[] cardStrArr = cardStr.Split(',');
    //                    if (player.CompareGid(MyStatus.gid))
    //                    {
    //                        this.player = playerManager.myPlayer;
    //                        if(cardStr.Length == 0)
    //                        {
    //                            this.player.SetCard(new string[] { "**", "**" });
    //                        }
    //                        else
    //                        {
    //                            this.player.SetCard(cardStr);
    //                            this.player.OpenCards(cardStr);

    //                        }

    //                    }
    //                    else
    //                    {
    //                        player.SetCard(new string[] { "**", "**" });
    //                    }
    //                }
    //            }
    //        }
    //    }
    private Coroutine drawCardCo = null;

    private void HoldemCommand(POKER_FLOW command)
    {
        for (int i = 0; i <= (int)command; i++)
        {
            POKER_FLOW flow = (POKER_FLOW)i;
            if (
                flow == POKER_FLOW.betstage_preflop
                || flow == POKER_FLOW.betstage_flop
                || flow == POKER_FLOW.betstage_turn
                || flow == POKER_FLOW.betstage_river
            )
                stage = flow;
        }
        switch (command)
        {
            case POKER_FLOW.show_down:
                showdown = true;
                showdownAnim.gameObject.SetActive(true);
                showdownWindow.SetActive(true);
                if (Tm.activeTable)
                    SoundManager.Instance.PlayEffectSound(Sound_TableEnum.SFX_SHOWDOWN);
                Invoke("ShowdownWindowClose", 1f);
                ShowPokerOddsRatio();
                break;
            case POKER_FLOW.draw_cards:
                if (!playerManager.IsPlayingGamer(MyStatus.gid)) //내가 게임에 참여한 유저가 아닌경우 PC_HOLDEM_DRAW_CARDS가 안오기때문에 다시 세팅
                {
                    List<Player> players = playerManager.GetPlayingGamer();
                    if (players.Count > 0)
                    {
                        if (drawCardCo != null)
                        {
                            CancelDrawCards();
                        }
                        drawCardCo = StartCoroutine(
                            GetUserCard(
                                players[0].gid,
                                gameType == GAME_TYPE.plo ? "**,**,**,**" : "**,**"
                            )
                        );
                    }
                    else
                    {
                        RoomReenter();
                    }
                }
                break;
        }
    }

    private void CancelDrawCards()
    {
        if (drawCardCo != null)
        {
            StopCoroutine(drawCardCo);
        }

        drawCardCo = null;
    }

    private void ShowdownWindowClose()
    {
        showdownWindow.SetActive(false);
    }

    private PokerRatio[] CalcPokerRatio()
    {
        HandsItem hi = new HandsItem();
        hi.gids = new string[hands.Count];
        hi.hands = new string[hands.Count];
        for (int i = 0; i < hands.Count; i++)
        {
            hi.gids[i] = hands[i].Key;
            hi.hands[i] = hands[i].Value;
        }
        hi.comm = commCards;
        Console.Log("COOMMMMMMMMMMMMMMM" + commCards);

        return PokerOddsManager.PostHandsItem(hi);
    }

    private void SetCommunityCard(int command, string[] commcardsArr, bool ani = true)
    {
        commCards = string.Join(" ", commcardsArr);
        commCards = commCards.Replace(",", " ").Trim();

        for (int i = 0; i < cards.Count; ++i)
        {
            for (int j = 0; j < cards[i].Count; ++j)
            {
                cards[i][j].gameObject.SetActive(false);
            }
        }
        for (int i = 0; i < commcardsArr.Length && i < cards.Count; i++)
        {
            string commcards = commcardsArr[i];
            string[] cardArr = commcards.Split(',');

            int flipCardCount = cardArr.Length;
            int openCardCount = 0;
            for (int j = 0; j < flipCardCount; j++)
            {
                if (cardArr[j] == "")
                {
                    break;
                }
                if (i == 0)
                {
                    openCardCount = j + 1;
                }

                {
                    cards[i][j].gameObject.SetActive(true);
                    var card = cards[i][j].GetCardData();
                    bool same = card.Equals(cardArr[j]);

                    if (!same && ani)
                    {
                        cards[i][j].SetCard(cardArr[j], false);
                        cards[i][j].Flip();
                    }
                    else
                    {
                        cards[i][j].SetCard(cardArr[j], true);
                    }
                }
                Console.SpecialLog($"commCards ({i}) : {commcards}");
            }
            Console.SpecialLog($"openCardCount : {openCardCount}");
            if (i == 0)
            {
                switch (openCardCount)
                {
                    case 0:
                        stage = POKER_FLOW.betstage_preflop;
                        break;
                    case 3:
                        stage = POKER_FLOW.betstage_flop;
                        break;
                    case 4:
                        stage = POKER_FLOW.betstage_turn;
                        break;
                    case 5:
                        stage = POKER_FLOW.betstage_river;
                        break;
                }
            }
        }
    }

    private IEnumerator GetUserCard(string gid, string cards)
    {
        List<Player> players = playerManager.GetPlayingGamer();
        var boss = playerManager.bossSeat;
        string[] cardArr = cards.Split(',');
        string[] otherPlayerCardArr = new string[cardArr.Length];
        var firstIdx =
            (
                players.FindIndex(
                    0,
                    0,
                    (p) =>
                    {
                        return p.seat == boss;
                    }
                ) + 1
            ) % players.Count;

        for (int i = 0; i < otherPlayerCardArr.Length; i++)
        {
            otherPlayerCardArr[i] = "**";
        }
        var count = 0;
        for (int i = 0; i < cardArr.Length; i++)
        {
            for (int j = 0; j < players.Count; j++)
            {
                string cardData = players[j].CompareGid(gid) ? cardArr[i] : otherPlayerCardArr[i];
                float delay = firstCardDrawIntaval * count;

                DrawCard(players[j].playerCard.cards[i], cardData, delay);
                count++;
            }
            count++;
        }

        yield return new WaitForSeconds(cardGetDelay);

        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].CompareGid(gid))
            {
                player = players[i];
                //holeCards = cards.Replace(',', ' ');
                players[i].SetCard(cardArr);
                if (!showdown)
                {
                    //(players[i] as HoldemPlayer).SetHandRank(holeCards);
                }
                //if(gid == MyStatus.gid && cardFocusing != null && cardFocusingPanel != null)
                //{
                //    cardFocusing.gameObject.SetActive(true);
                //    cardFocusingPanel.SetActive(true);
                //    cardFocusing.SetCard(cardArr);
                //    cardFocusing.Play(0, 5f, (isCancel) =>
                //    {
                //        if (!isCancel)
                //        {
                //            cardFocusingPanel.SetActive(false);
                //            cardFocusing.gameObject.SetActive(false);
                //        }
                //    });
                //}
                //string[] best_cards = PokerOddsManager.MyHandsInfo(holeCards, commCards).best_cards;
                //CardHighlight(0, best_cards);
            }
            else
            {
                players[i].SetCard(otherPlayerCardArr);
            }
        }
    }

    private IEnumerator ShowdownOpenCard(
        float time,
        int kind,
        string[] cards,
        int runItTwiceIdx,
        JArray odds
    )
    {
        yield return new WaitForSeconds(time + cardOpenDelay);
        int[] kindToIdx = new int[4] { -1, 0, 3, 4 };
        if (kind == 1)
        {
            //commCards = "";
        }
        bool wait = false;
        for (int i = runItTwiceIdx; i > 0; i--)
        {
            int first = kindToIdx[kind];
            if (
                this.cards[i][first].gameObject.activeSelf
                && this.cards[i][first].GetCardData() == cards[0]
            )
                break;
            for (int j = first; j < this.cards[i].Count; j++)
            {
                bool active_prev = this.cards[i - 1][j].gameObject.activeSelf;
                if (!active_prev)
                    continue;
                this.cards[i][j].gameObject.SetActive(true);
                this.cards[i][j].SetCard(this.cards[i - 1][j].GetCardData());
                this.cards[i][j].Comeback(this.cards[i - 1][j].transform.position, 0.5f);
                this.cards[i - 1][j].SetCard("**");
                this.cards[i - 1][j].gameObject.SetActive(false);
                wait = true;
            }
        }
        // if(wait)
        yield return new WaitForSeconds(0.5f);
        for (int i = 0; i < cards.Length; i++)
        {
            this.cards[0][i + kindToIdx[kind]].gameObject.SetActive(true);
            this.cards[0][i + kindToIdx[kind]].SetCard(cards[i], false);
            this.cards[0][i + kindToIdx[kind]].Flip();
            commCards += string.Format("{0}{1}", commCards.Length == 0 ? "" : " ", cards[i]);
            if (i == cards.Length - 1)
            {
                if (player)
                {
                    (player as HoldemPlayer).SetHandRank(holeCards, commCards);
                }
                if (holeCards != "")
                {
                    string[] best_cards = PokerOddsManager
                        .MyHandsInfo(holeCards, commCards)
                        .best_cards;
                    CardHighlight(0, best_cards);
                }
                if (showdown)
                {
                    ShowPokerOddsRatio();
                }
            }

            // if(i < cards.Length - 1)
            {
                yield return new WaitForSeconds(0.3f);
            }
        }

        yield return new WaitForSeconds(1.0f);

        if (odds != null)
        {
            //for (int i = 0; i < odds.Count; i++)
            //{
            //    JObject curOdds = odds[i] as JObject;
            //    Player p = playerManager.FindPlayerWithSeat((int)curOdds["seat"]);
            //    if (p)
            //    {
            //        p.ShowRate(curOdds.ValueOrDefault("win", 0f), curOdds.ValueOrDefault("tie", 0f));
            //    }
            //}
        }
    }

    private IEnumerator GameEnd_()
    {
        //CardHighlightOff();  //결과때 연출끝내도록 (여기에서 내려가는 애니메이션 해주면 ForceBack 에서 초기화시켜주는부분과 겹침)
        //yield return new WaitForSeconds(0.3f);
        //RunItTwiceCardHighlightOff();
        CancelDrawCards();

        for (int i = 0; i < cards.Count; i++)
        {
            for (int j = 0; j < cards[i].Count; j++)
            {
                if (cards[i][j].gameObject.activeSelf)
                {
                    cards[i][j].PlayIdle();
                }
            }
        }
        for (int i = 0; i < cards.Count; i++)
        {
            for (int j = 0; j < cards[i].Count; j++)
            {
                if (i == 0)
                {
                    cards[i][j].RabbitMode(false);
                }
                cards[i][j].onClick = null;
                cards[i][j].gameObject.SetActive(false);
            }
        }
        openCardButtons.ForEach(d =>
        {
            if (d.button.gameObject.activeSelf)
                d.button.gameObject.SetActive(false);
        });
        hands.Clear();
        holeCards = string.Empty;
        commCards = string.Empty;
        yield break;
    }

    private async void ShowPokerOddsRatio()
    {
        PokerRatio[] pr = CalcPokerRatio();
        if (pr != null)
        {
            string resultString = "Win Ratio\n";
            await UniTask.WaitForSeconds(0.5f);
            if (!showdown)
            {
                return;
            }

            for (int j = 0; j < pr.Length; j++)
            {
                HoldemPlayer pl = playerManager.FindPlayerWithGid(pr[j].gid) as HoldemPlayer;
                pl.ShowRate(pr[j].win, pr[j].tie);
                resultString += string.Format(
                    "{0} / win: {1} / tie: {2}\n",
                    pr[j].gid,
                    pr[j].win,
                    pr[j].tie
                );
            }
            Debug.Log(resultString);
        }
    }

    protected override void GameEnd()
    {
        base.GameEnd();
        openTime = 0;
        resultPanel.SetActive(false);
        showdown = false;
        isOpened = false;
        stage = POKER_FLOW.none;
        cardDrawer.KillAnimations();
        foreach (var player in playerManager.players)
        {
            player.playerCard.ForceBack();
        }
        StopAllCoroutines();
        StartCoroutine(GameEnd_());
    }
}
