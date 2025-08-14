using System;
using System.Collections;
using System.Collections.Generic;
using Constant;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.U2D;
using UnityEngine.UI;

public class LobbyManager : WebsocketListenBehaviour
{
    //public Dictionary<long, string> unit = new Dictionary<long, string>();
    private static bool noticeView = false;
    private static GAME_TYPE pickGameTypeTab = GAME_TYPE.nlh;

    [SerializeField]
    private GameObject addCafeButton;

    [Header("UserInfo")]
    public GameObject userInfoWindow;
    public Image[] profileCharacterImage;
    public Text[] nicknameText;
    public Text[] chipText;
    public Text[] goldText;
    public Text[] safeChipText;
    public Text[] safeGoldText;
    public LimitChangeWindow limitChangeWindow;

    public GAME_TYPE curGameType = GAME_TYPE.nlh;

    [Header("Item List")]
    public SpriteAtlas itemAtlas;
    public GameObject gameEnterButtonContainer = null;
    public ScrollRect gameEnterScrollView = null;

    public LobbyRoomList lobbyRoomList;

    [Header("TournamentDetailPopup")]
    public GameObject tournamentDetailPopup;
    public Transform rewardParent;
    public GameObject rewardRowPrefab;
    private List<RewardRow> rewardList;

    [Space]
    public int tn;

    [Space]
    public Text titleText;
    public Text buyInText;

    [Space]
    public Text startTimeText;
    public Text closeTimeText;
    public Text totalPlayerText;
    public Text startChipText;
    public Text rebuyText;
    public Text addOnText;
    public Text userCountText;

    [Space]
    public Button applyButton;
    public Button unapplyButton;
    public Button inProgressButton;

    [Header("History")]
    public InfoTab totalInfo;
    public InfoTab holdemInfo;
    public InfoTab tournamentInfo;
    public InfoTab badugiInfo;
    public InfoTab baccaratInfo;
    public QuestPanel questPanel;
    public LoginCheckQuest loginCheck;

    [Space]
    [Header("GameTypeToggles")]
    public GameTypeToggle[] gameTypeToggles;
    public CustomUIOpener customUIOpener;

    private static LobbyManager instance;
    public static LobbyManager Instance
    {
        get { return instance; }
    }
    public NotiPanel noti;

    [HideInInspector]
    public JObject rankData;
    public UnityEvent rankEvent;

    [Header("LostLimit")]
    public Text getLostText;
    public Text limitButtonText;
    public Text changeResultText;
    public Text limitFailedTileText;
    public Text limitFailedText;

    public int limittype = 0;
    public GameObject limitHistoryPanel;
    public GameObject limitChangeResultWindow;
    public GameObject limitSetWindow;
    public GameObject limitChangeFailWindow;
    public GameObject limitHistorySlotPrefab;

    public List<LimitChangeHistorySlot> limitChangeHistoryList = new List<LimitChangeHistorySlot>();
    public Transform limitHistoryContainer;
    public GameObject userInfoPanel;

    [Header("Vip")]
    public GameObject VipPanel;

    [Header("minigame")]
    [SerializeField]
    private Button minigameButton;

    [SerializeField]
    private Text minigameCount;

    [SerializeField]
    private GameObject textBack;

    [Header("UserProfileImage")]
    [SerializeField]
    private Toggle useUrlToggle;

    [SerializeField]
    private SpriteAtlas characterAtlas;

    public UnityEvent roomUpdate = new UnityEvent();

    public Play play;
    public Cafe cafe;

    //public Shop shop;
    //public MyInfo myInfo;

    public PlayGameInfo currentPlayInfo;
    public NickNameChangePanel nickChangePanel;

    public TnmtResultPopup tnmtResultPopup;

    protected override void Awake()
    {
        base.Awake();
        instance = this;
        addCafeButton?.SetActive(GameConfig.SHOW_FIRST_CAFE);
        // unit.Add(10000, "만");
        // unit.Add(100000000, "억");
        // unit.Add(1000000000000, "조");
    }

    private void Notice()
    {
        if (!noticeView)
        {
            var notice = ServerConfigManager.GetConfig("notice");
            noti.Notice(notice as JArray);
            noticeView = true;
        }
    }

    // Start is called before the first frame update
    private void Start()
    {
        Initialize();
    }

    public async void Initialize()
    {
        Debug.Log("lobby start");
        InitInfo();
        GetUserInfo();
        RequestChanelCount();
        RequestInPlayingGame();
        await new WaitForPCProtocol(PCProtocol.PC_ROOM_IN_PLAYING);
        RequestGetTournamentList();
        RequestMyTournament();
        RequestQuestData();
        RequestMySkillList();
        RequestUserGameInfo();
        InfoManager.Instance.GetMyItems();
        bool menualChangeTab = false;
        for (int i = 0; i < gameTypeToggles.Length; i++)
        {
            if (gameTypeToggles[i].GetGameTypeString() == pickGameTypeTab)
            {
                menualChangeTab = gameTypeToggles[i].toggle.isOn;
                gameTypeToggles[i].toggle.isOn = true;
                break;
            }
        }
        if (menualChangeTab)
        {
            OnChangedGameTypeTab(pickGameTypeTab);
        }

        LobbyTabsManager.Instance?.TabReset();

        //PublisherApiManager.Instance.GetUserInfoPubAPI(UserInfoCallBack);
    }

    public void RequestChanelCount()
    {
        Packet p = new Packet(CPProtocol.CP_COUNT_OF_TABLES_ZC);
        WebSocketManager.defaultCli.Send(p.ToJson());
    }

    private async void RequestInPlayingGame()
    {
        LoadingCircle.Instance.StartSpin();
        if (GamesManager.instance)
        {
            //GamesManager.instance.ClearGames();
        }
        await UniTask.WaitForSeconds(0.1f);
        var p = new Packet(CPProtocol.CP_ROOM_IN_PLAYING);
        WebSocketManager.defaultCli.Send(p.ToJson());
    }

    private void DeepLinkSetting()
    {
        if (!string.IsNullOrEmpty(DeepLink.CafeCode))
        {
            customUIOpener.ShowUI("cafe_join_1");
        }
        else if (!string.IsNullOrEmpty(DeepLink.AutoJoinCode))
        {
            customUIOpener.ShowUI("cafe_join_2");
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        DeepLink.updated = null;
    }

    public async void GetUserInfo()
    {
        LoadingCircle.Instance.StartSpin();
        var info = await PublisherApiManager.Instance.TryGetAPI(
            PublisherApiManager.Instance.url + API.Get.INFO
        );
        await UserInfoResult(info);
        var packet = new Packet((int)CPProtocol.CP_CONSOLE_USERINFO);
        WebSocketManager.defaultCli.Send(packet.ToJson());
        await new WaitForPCProtocol(PCProtocol.PC_CONSOLE_USERINFO);

        LobbyEnterPopup();
        LoadingCircle.Instance.StopSpin();
    }

    public async void LobbyEnterPopup()
    {
        while (loginCheck.gameObject.activeSelf)
        {
            await UniTask.WaitForSeconds(0.1f);
        }

        if (loginCheck.SetLoginCheckData())
        {
            return;
        }

        if (await RequestChip())
        {
            return;
        }

        if (InfoManager.canChangeNick)
        {
            InfoManager.canChangeNick = false;
            nickChangePanel.gameObject.SetActive(true);
            while (nickChangePanel.gameObject.activeSelf)
            {
                await UniTask.WaitForSeconds(0.1f);
            }
            DeepLinkSetting();
            DeepLink.updated = DeepLinkSetting;
        }

        Notice();
    }

    public async UniTask<bool> RequestChip()
    {
        JObject memberShip = InfoManager.Membership[MyStatus.vipRate.ToString()] as JObject;
        var result = false;
        string[] currency_list = new string[] { "gold", "silver" };
        for (int i = 0; i < currency_list.Length; i++)
        {
            string currency = currency_list[i];
            long current_chip = 0;

            if (currency == "silver")
                current_chip = MyStatus.zc + MyStatus.safeSc;
            if (currency == "gold")
                current_chip = MyStatus.dc + MyStatus.safeGc;

            if (MyStatus.request == null)
                MyStatus.request = new JObject();

            long currentCount = MyStatus.request.ValueOrDefault<long>(currency, 0);
            long limit = memberShip.ValueOrDefault<long>(currency + "_free_chip_count", 0); // gold_request --> gold_free_chip_count  변경
            long min_chip = memberShip.ValueOrDefault<long>(currency + "_free_chip_min", 0); // gold_min --> gold_free_chip_min 변경
            if (currentCount < limit && current_chip < min_chip)
            {
                LoadingCircle.Instance.StartSpin();
                var www = await PublisherApiManager.Instance.RequestChip(currency);
                RequestChipCallback(www);
                result = true;
            }
        }

        return result;
    }

    private void RequestChipCallback(UnityWebRequest www)
    {
        JObject data = JObject.Parse(www.downloadHandler.text);

        if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
        {
            int ecode = 0;
            try
            {
                ecode = data["ecode"].ToObject<int>();
            }
            catch { }
            if (ecode == 402)
            {
                PublisherApiManager.Instance.NetWorkErrorLogOut();
            }
            else
            {
                Debug.Log(string.Format("{0} : {1}", www.error, www.downloadHandler.text));
                ErrorMessageManager.Instance.AddNetworkError(
                    0,
                    www.error,
                    www.downloadHandler.text
                );
            }
        }
        else
        {
            if (www.responseCode == 200)
            {
                string[] currency_list = new string[] { "gold", "silver" };
                JObject update = data.ValueOrDefault<JObject>("update", new JObject());
                JObject request = data.CastOrEmpty<JObject>("request");
                if (request != null)
                {
                    MyStatus.request = request;
                }
                for (int i = 0; i < currency_list.Length; i++)
                {
                    string currency = currency_list[i];
                    long amount = update.ValueOrDefault<long>(currency, 0);
                    if (amount > 0)
                    {
                        // JObject requestData = InfoManager.GameConfig["free_chip"][MyStatus.vipString] as JObject;
                        JObject memberShip =
                            InfoManager.Membership[MyStatus.vipRate.ToString()] as JObject;

                        int limit = memberShip.ValueOrDefault<int>(
                            currency + "_free_chip_count",
                            0
                        ); // gold_request --> gold_free_chip_count  변경
                        int currentCount = 0;
                        var myRequest = MyStatus.request;
                        if (myRequest != null)
                        {
                            currentCount = myRequest.ValueOrDefault<int>(currency, 0);
                        }

                        //MyStatus.request[currency] = currentCount + 1;

                        long v = amount;
                        string errorMessage = string.Format(
                            LocalizeManager.GetLocalString(
                                "SYS_MSG_FREE_CHARGE_" + currency.ToUpper()
                            ),
                            MoneyToString.Converting(v),
                            limit - currentCount
                        );
                        ;
                        ErrorMessageManager.Instance.AddGameErrorBodyNotLocal(
                            0,
                            "SYS_MSG_USERCARE_TITLE",
                            errorMessage,
                            ErrorHandlingType.NONE
                        );
                    }
                }
            }
        }
        Debug.Log(data.ToString());
    }

    public async void UserInfoCallBack(long statusCode, JObject data)
    {
        long gold = data["gold"].ToObject<long>();
        long silver = data["silver"].ToObject<long>();
        if (gold > 0)
        {
            await PublisherApiManager.Instance.PointInAPI("gold", gold, null);
        }
        if (silver > 0)
        {
            await PublisherApiManager.Instance.PointInAPI("silver", silver, null);
        }
        loginCheck.SetLoginCheckData();
    }

    private async UniTask UserInfoResult(UnityWebRequest www)
    {
        Console.Log("GetUserInfo Web Load Success");
        if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
        {
            int ecode = 0;
            try
            {
                JObject json = JObject.Parse(www.downloadHandler.text);
                ecode = json["ecode"].ToObject<int>();
            }
            catch { }
            if (ecode == 402)
            {
                PublisherApiManager.Instance.NetWorkErrorLogOut();
            }
            else
            {
                Debug.Log(www.error);
                Debug.Log(www.downloadHandler.text);
                ErrorMessageManager.Instance.AddNetworkError(
                    ecode,
                    www.error,
                    www.downloadHandler.text
                );
            }
        }
        else
        {
            JObject json = JObject.Parse(www.downloadHandler.text);
            Debug.Log("PubUserInfo : " + json.ToString());

            if (www.responseCode == 200)
            {
                var login = json["property"]["login"] as JObject;
                MyStatus.loginContinued = (int)login["loginCountContinued"];
                MyStatus.isLoginRewarded = (bool)login["isRecvAttendanceReward"];
                MyStatus.safeGc = json["safe_gold"].ToObject<long>();
                MyStatus.safeSc = json["safe_silver"].ToObject<long>();
                MyStatus.safeRu = json["safe_ruby"].ToObject<long>();
                int vip_rate = json.ValueOrDefault<int>("vip_rate", 0);
                MyStatus.vipRate = vip_rate;
                try
                {
                    long gold = json["gold"].ToObject<long>();
                    long silver = json["silver"].ToObject<long>();
                    if (gold > 0)
                    {
                        await PublisherApiManager.Instance.PointInAPI("gold", gold, null);
                    }
                    if (silver > 0)
                    {
                        await PublisherApiManager.Instance.PointInAPI("silver", silver, null);
                    }
                }
                catch
                {
                    Console.Log("failed to Point In");
                }

                Console.Log("MyData : " + json.ToString());
                MyStatus.request = null;
                if ((json["property"] as JObject).ContainsKey("request"))
                {
                    long lastUpdateAt = (
                        json["property"]["request"] as JObject
                    ).ValueOrDefault<long>("lastUpdateAt", 0);
                    DateTime lastUpdateDate = new DateTime(1970, 1, 1).AddSeconds(lastUpdateAt);
                    //if (lastUpdateDate.Date == DateTime.UtcNow.Date)
                    {
                        MyStatus.request = json["property"]["request"] as JObject;
                    }
                }
            }
        }
    }

    public void UpdateChipText()
    {
        try
        {
            for (int i = 0; i < chipText.Length; i++)
            {
                chipText[i].text = MoneyToString.Converting(MyStatus.zc);
            }
        }
        catch (Exception e)
        {
            Console.Log(e.Message);
        }

        try
        {
            for (int i = 0; i < goldText.Length; i++)
            {
                goldText[i].text = MoneyToString.Converting(MyStatus.dc);
            }
        }
        catch (Exception e)
        {
            Console.Log(e.Message);
        }
        roomUpdate.Invoke();
    }

    private void RequestMySkillList()
    {
        PublisherApiManager.Instance.RequestSkillList(MySkillListCallback);
    }

    private void MySkillListCallback(long statusCode, JObject data)
    {
        JArray list = data["list"].ToObject<JArray>();
        for (int i = 0; i < list.Count; i++)
        {
            JToken skill = list[i];
            switch (skill["ref_id"].ToObject<int>())
            {
                case 1:
                    StartCoroutine(SpinCoolDown(skill["cooldown_date"].ToObject<string>()));
                    break;
            }
        }
    }

    public void StartMiniGameCountDown(string time)
    {
        StartCoroutine(SpinCoolDown(time));
    }

    private IEnumerator SpinCoolDown(string time)
    {
        var targetTime = DateTimeParser.Parse(time);
        Console.SpecialLog(time);
        DateTime currTime = DateTime.UtcNow;
        minigameButton.interactable = false;
        textBack.SetActive(true);
        currTime = DateTime.UtcNow;
        TimeSpan span = targetTime - currTime;
        minigameCount.text = TimeToString.SpanToString(span);
        while (targetTime > currTime)
        {
            yield return new WaitForSeconds(1);
            currTime = DateTime.UtcNow;
            span = targetTime - currTime;
            minigameCount.text = TimeToString.SpanToString(span);
        }

        minigameButton.interactable = true;
        textBack.SetActive(false);
    }

    //private string SpanToString(TimeSpan span)
    //{
    //    string str = "";
    //    if (span.TotalHours > 0)
    //    {
    //        str = string.Format("{0}:{1}:{2}", span.Hours, span.Minutes, span.Seconds);
    //    }
    //    else if (span.TotalMinutes > 0)
    //    {
    //        str = string.Format("{0}:{1}", span.Minutes, span.Seconds);
    //    }
    //    else if (span.TotalSeconds > 0)
    //    {
    //        str = string.Format("{0}", span.Seconds);
    //    }

    //    return str;
    //}
    private async void InitInfo()
    {
        for (int i = 0; i < nicknameText.Length; i++)
        {
            nicknameText[i].text = MyStatus.nick;
        }
        //var chip = long.Parse(string.Format("{0:0000}",MyStatus.gc));
        UpdateChipText();

        if (!string.IsNullOrEmpty(MyStatus.photoURL) && MyStatus.usePhotoURL)
        {
            SetProfile(
                await ImageDatabase.LoadImageTexture(
                    MyStatus.photoURL,
                    Application.persistentDataPath + "/profileImg",
                    MyStatus.uid
                )
            );
        }
        else
        {
            string imageName = "";
            try
            {
                imageName = TableDataManager
                    .characterTable[MyStatus.icon_no]["2d_index"]
                    .ToObject<string>();
            }
            catch { }
            Console.SpecialLog(imageName);

            Sprite sp = characterAtlas.GetSprite(imageName);
            if (sp != null && !MyStatus.usePhotoURL)
            {
                SetProfile(sp);
            }
            else { }
        }
    }

    public void SetProfile(Texture2D texture)
    {
        Rect rect = new Rect(0, 0, texture.width, texture.height);
        Sprite sprite = Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f));
        sprite.name = "scriptCreating";
        SetProfile(sprite);
    }

    public void SetProfile(Sprite sprite)
    {
        for (int i = 0; i < profileCharacterImage.Length; i++)
        {
            profileCharacterImage[i].enabled = true;
            profileCharacterImage[i].sprite = sprite;
        }
    }

    public void NickNamePopupButton()
    {
        nickChangePanel.gameObject.SetActive(true);
    }

    public void OnClickUserInfoButton()
    {
        userInfoWindow.SetActive(true);
    }

    public void OnTournamentDetailPopup(
        int tn,
        string title,
        string buyIn,
        string startTime,
        string closeTime,
        string totalPlayer,
        string startChip,
        string rebuy,
        string addOn,
        List<long> rewards
    )
    {
        this.tn = tn;
        titleText.text = title;
        buyInText.text = buyIn;
        startTimeText.text = startTime;
        closeTimeText.text = closeTime;
        totalPlayerText.text = totalPlayer;
        startChipText.text = startChip;
        rebuyText.text = rebuy;
        addOnText.text = addOn;
        userCountText.text = totalPlayer;

        if (rewardList == null)
        {
            rewardList = new List<RewardRow>();
        }

        for (int i = 0; i < rewards.Count; i++)
        {
            if (rewardList.Count <= i)
            {
                var go = Instantiate(rewardRowPrefab, rewardParent);
                rewardList.Add(go.GetComponent<RewardRow>());
            }
            rewardList[i].RankText.text = string.Format("{0}등", i + 1);
            rewardList[i].RewardText.text = MoneyToString.Converting(rewards[i]);
        }

        while (rewardList.Count > rewards.Count)
        {
            var go = rewardList[rewards.Count].gameObject;
            rewardList.RemoveAt(rewards.Count);
            Destroy(go);
        }

        UpdateTournamentDetailPopup();

        tournamentDetailPopup.SetActive(true);
    }

    private void PointInAPICallBack(long statusCode, JObject res)
    {
        switch (statusCode)
        {
            case 200: //success
                Console.Log("PutIn Success");
                GetUserInfo();
                break;
            case 400: //BadRequest
                Console.Log("BadRequest");
                break;
            case 402: //BadRequest
                Console.Log("AbnormalReceipt");
                break;
            case 409: //BadRequest
                Console.Log("UsedReceipt");
                break;
        }
    }

    public void SetSafeText()
    {
        //Debug.LogError(string.Format("chip : {0} \ngold : {1}", MyStatus.safeSc, MyStatus.safeGc));
        for (int i = 0; i < safeChipText.Length; i++)
        {
            var monyString = MoneyToString.Converting(MyStatus.safeSc);
            if (safeChipText[i])
            {
                safeChipText[i].text = monyString;
            }
        }
        for (int i = 0; i < safeGoldText.Length; i++)
        {
            var monyString = MoneyToString.Converting(MyStatus.safeGc);
            if (safeGoldText[i])
            {
                safeGoldText[i].text = monyString;
            }
        }
        roomUpdate.Invoke();
    }

    public void UpdateTournamentDetailPopup()
    {
        //if (tn == MyStatus.tn)
        //{
        //    applyButton.gameObject.SetActive(false);
        //    unapplyButton.gameObject.SetActive(true);
        //}
        //else
        //{
        //    unapplyButton.gameObject.SetActive(false);
        //    applyButton.gameObject.SetActive(true);
        //}
    }

    public void OnClickTournamentApply()
    {
        LoadingCircle.Instance.StartSpin(CPProtocol.CP_TNMT_APPLY);
        Packet p = new Packet((int)CPProtocol.CP_TNMT_APPLY);
        p.Add("tn", tn);
        p.Add("ticket", 0);
        p.Add("rebuy", 0);
        p.Add("double", 0);
        WebSocketManager.defaultCli.Send(p.ToJson());
    }

    //private void RequestPubNicknameCheck()
    //{
    //    PublisherApiManager.Instance.CheckNickname(nicknameCheckText.text, (statusCode) =>
    //    {
    //        if (statusCode == 200)
    //        {
    //            SetNicknameCheckNotification("", Color.red);
    //            RequestPubRegister(nicknameCheckText.text);
    //        }
    //        else if (statusCode == 422)
    //        {
    //            LoadingCircle.Instance.StopSpin();
    //            SetNicknameCheckNotification(LocalizeManager.GetLocalString("duplicate_nickname"), Color.red);
    //        }
    //        else
    //        {
    //            LoadingCircle.Instance.StopSpin();
    //            ErrorMessageManager.Instance.AddGameError((int)statusCode, "SYS_ERR_NICK_CHECKING", "SYS_ERR_NICK_CHECKING_NOT_DEFINED");
    //        }
    //    });
    //}
    //public void OnClickNicknameCheckButton()
    //{
    //    Debug.Log("ChangeNickName");
    //    LoadingCircle.Instance.StartSpin();
    //    RequestPubNicknameCheck();
    //}

    //private void RequestPubRegister(string nickName)
    //{
    //    PublisherApiManager.Instance.UpdateNickname(nickName, (statusCode) =>
    //    {
    //        if (statusCode == 200)
    //        {
    //            MyStatus.nick = nickName;
    //            for(int i = 0; i < nicknameText.Length; i++)
    //            {
    //                nicknameText[i].text = MyStatus.nick;
    //            }
    //            LoadingCircle.Instance.StopSpin();
    //            nicknameCheckPanel.SetActive(false);
    //        }
    //        else
    //        {
    //            LoadingCircle.Instance.StopSpin();
    //            ErrorMessageManager.Instance.AddGameError((int)statusCode, "SYS_ERR_JOIN", "SYS_ERR_NICK_CHANGE");
    //        }
    //    });
    //}

    public void RequestQuestData()
    {
        var p = new Packet((int)CPProtocol.CP_USER_QUESTINFO);
        WebSocketManager.defaultCli.Send(p);
    }

    //public void SetNicknameCheckNotification(string text, Color color)
    //{
    //    nicknameCheckNotificationText.color = color;
    //    nicknameCheckNotificationText.text = text;
    //}

    //public void OnNicknameCheck()
    //{
    //    int nickByte = System.Text.ASCIIEncoding.ASCII.GetByteCount(nicknameCheckText.text);
    //    if (nickByte < 3)
    //    {
    //        string message = LocalizeManager.GetLocalString("SYS_ERR_SHORT_NICKNAME");

    //        SetNicknameCheckNotification(message, Color.red);
    //        Console.Log("닉네임이 너무 짧습니다.");
    //        return;
    //    }
    //    else if (nickByte > 19)
    //    {
    //        string message = LocalizeManager.GetLocalString("SYS_ERR_LONG_NICKNAME");

    //        SetNicknameCheckNotification(message, Color.red);
    //        Console.Log("닉네임이 너무 깁니다.");
    //        return;
    //    }
    //    else
    //    {
    //        SetNicknameCheckNotification("", Color.red);
    //    }
    //}

    //public void OpenNicknameCheckPanel()
    //{
    //    string message = LocalizeManager.GetLocalString("SYS_ERR_SHORT_NICKNAME");

    //    SetNicknameCheckNotification(message, Color.red);
    //    nicknameCheckText.text = "";
    //    nicknameCheckPanel.SetActive(true);
    //}

    public void OnClickTournamentUnapply()
    {
        LoadingCircle.Instance.StartSpin(CPProtocol.CP_TNMT_UNAPPLY);
        Packet p = new Packet((int)CPProtocol.CP_TNMT_UNAPPLY);
        p.Add("tn", tn);
        WebSocketManager.defaultCli.Send(p.ToJson());
    }

    public void OnClickGetTournamentListButton()
    {
        curGameType = GAME_TYPE.mtt;
        lobbyRoomList.GetTournamentList();
    }

    public void RequestGetTournamentList()
    {
        Packet p = new Packet((int)CPProtocol.CP_TNMT_LIST);
        WebSocketManager.defaultCli.Send(p.ToJson());
    }

    public void OnClickLogOutButton()
    {
        WebSocketManager.defaultCli.OnExitOnce += (reson) =>
        {
            DevManager.Instance.GsLogin = false;
            DevManager.Instance.WsDelegate -= 1;
            DevManager.Instance.WsConnect = false;
            FirebaseManager.Instance.SignOut();
            PublisherApiManager.Instance.token = "";
            ErrorMessageManager.Instance.AddGameError(
                0,
                "SYS_ERR_LOGOUT_SUCCESS",
                "SYS_CLIENT_LOGOUT_SUCCESS",
                ErrorHandlingType.RECALL,
                null,
                () =>
                {
                    CustomSceneManager.LoadLoginScene();
                }
            );
        };
        WebSocketManager.defaultCli.Close();
    }

    public void OnClickEnterGameButton(EnterChipButton cb)
    {
        WebSocketManager.defaultCli.Send(
            "{\"p\":103,\"c\":{\"gtn\":0,\"game_type\":\""
                + curGameType
                + "\",\"blind\":"
                + cb.bng
                + "}}"
        );
    }

    /// <summary>
    /// 임시용임
    /// </summary>
    private void RequestMyTournament()
    {
        Packet p = new Packet((int)CPProtocol.CP_TNMT_MY);
        WebSocketManager.defaultCli.Send(p.ToJson());
    }

    private void SendTnmtNotification(int tn, DateTime time)
    {
        string notiKey = "tnmt_noti_" + tn;
        if (PlayerPrefs.GetInt(notiKey, 0) == 0 && (time - DateTime.UtcNow).TotalSeconds > 0)
        {
            int notiId = LocalNotificationManager.SendNotification(
                LocalizeManager.GetLocalString("start_tnmt_soon"),
                "",
                time
            );
            PlayerPrefs.SetInt(notiKey, notiId);
        }
    }

    private void CancelTnmtNotification(int tn)
    {
        string notiKey = "tnmt_noti_" + tn;
        int notiId = PlayerPrefs.GetInt(notiKey);
        LocalNotificationManager.CancelNotification(notiId);
        PlayerPrefs.DeleteKey(notiKey);
    }

    private void RequestHoldemStatus(int gtn)
    {
        Packet p = new Packet((int)CPProtocol.CP_HOLDEM_STATUS);
        p.Add("gtn", gtn);
        WebSocketManager.defaultCli.Send(p.ToJson());
    }

    protected override void ReceivePacket(Packet packet)
    {
        base.ReceivePacket(packet);
        int p = packet.p;
        JObject c = packet.c;
        if (c.ContainsKey("ecode"))
        {
            if ((int)c["ecode"] != 0)
            {
                return;
            }
        }
        switch ((PCProtocol)p)
        {
            case PCProtocol.PC_PLAY_GAME_ENTER:
            case PCProtocol.PC_ROOM_ENTER:
                {
                    if (LoadingCircle.Instance.spin)
                    {
                        LoadingCircle.Instance.StopSpin();
                    }
                    ERR ecode = (ERR)(int)c["ecode"];
                    if (ecode == ERR.OK)
                    {
                        JObject data = c["data"] as JObject;
                        if ((GAME_TYPE)(int)data["game_type"] == GAME_TYPE.nlh)
                        {
                            //if (DevOptionsManager.devOptions.bettingMode == 0)
                            //{
                            //    CustomSceneManager.LoadScene("Holdem");
                            //}
                            //else if (DevOptionsManager.devOptions.bettingMode == 1)
                            //{
                            //    CustomSceneManager.LoadScene("Games");
                            //}

                            //GamesManager.instance.SetTable()
                        }
                    }
                    else if (ecode == ERR.CAFE_STATUS_NOT_NORMAL)
                    {
                        ErrorMessageManager.Instance.AddGameError(
                            (int)ecode,
                            "SYS_ERR_GAME_IN_FAILED",
                            "play_game_enter_err_cafe_status_not_normal"
                        );
                    }
                    else if (ecode == ERR.TNMT_ALREADY_ELIMINATED) { }
                    else
                    {
                        ErrorMessageManager.Instance.AddGameError(
                            (int)ecode,
                            "SYS_ERR_GAME_IN_FAILED",
                            "SYS_CLIENT_GAME_IN_FAILED"
                        );
                    }
                }
                break;
            case PCProtocol.PC_ROOM_RABBIT:
                if (c.ValueOrDefault("ecode", -1) == 0 && c.ValueOrDefault("use_zc", 0) > 0)
                {
                    MyStatus.zc = c.ValueOrDefault("zc", MyStatus.zc);
                    UpdateChipText();
                }
                break;
            case PCProtocol.PC_QUEST_CLEAR:
                GetUserInfo();
                RequestQuestData();
                break;

            case PCProtocol.PC_USER_QUESTINFO:
                questPanel.SetQuest(c);
                break;
            case PCProtocol.PC_ROOM_USER_LIST: //유저리스트
                List<JObject> userList = JsonDataParser.Parse<List<JObject>>(c["list"]["item"]);
                //Store.userList = userList;
                Debug.Log("UserList Check Complete");
                break;
            case PCProtocol.PC_ROOM_ENTER_FAIL: //FailToEndterRoom
                {
                    ErrorMessageManager.Instance.AddGameError(
                        107,
                        "SYS_ERR_GAME_IN_FAILED",
                        "SYS_CLIENT_GAME_IN_FAILED"
                    );
                }
                break;
            case PCProtocol.PC_TNMT_LIST:
                //lobbyRoomList.GetTournamentList(c["data"]);
                break;
            case PCProtocol.PC_USER_INFO_OPTION_UPDATE:
                GetUserInfo();
                break;
            case PCProtocol.PC_TNMT_UPDATE:
                Debug.Log("토너먼트 룸 업데이트 됨");
                //RequestTnmtList();
                break;
            case PCProtocol.PC_TNMT_APPLY:
            {
                Debug.Log("토너먼트 등록 완료");
                int tn = (int)c["tn"];
                RequestMyTournament();
                UpdateTournamentDetailPopup();
                var tnmt = InfoManager.Instance.GetTournamentInfo(tn);
                SendTnmtNotification(tn, DateTimeParser.Parse(tnmt.startTime).AddSeconds(-60));

                break;
            }
            case PCProtocol.PC_TNMT_UNAPPLY: // PC_TNMT_UNAPPLY
            {
                Debug.Log("토너먼트 취소 완료");
                int tn = 0;
                if (c.ContainsKey("tn"))
                {
                    tn = (int)c["tn"];
                }
                else
                {
                    var data = c.CastOrEmpty<JObject>("data");
                    tn = data.ValueOrDefault("tn", 0);
                }
                MyStatus.RemoveTnmt(tn);
                MyTournamentCount();
                UpdateTournamentDetailPopup();
                CancelTnmtNotification(tn);
                break;
            }
            case PCProtocol.PC_TNMT_MY: // PC_TNMT_MY 임시
                JArray arr = c["tournament"] as JArray;
                MyStatus.RemoveAllTnmt();
                for (int i = 0; i < arr.Count; i++)
                {
                    var da = arr[i] as JObject;
                    TnmtData data = MyStatus.AddTnmt(da.ValueOrDefault("tn", 0));
                    data.gtn = da.ValueOrDefault("gtn", 0);
                    data.cafeIdx = da.ValueOrDefault("cafeIdx", 0);
                    data.state = da.ValueOrDefault("state", TNMT_FLOW.none);
                    var strartTimeText = da.ValueOrDefault("t_start_time", string.Empty);
                    if (!string.IsNullOrEmpty(strartTimeText))
                    {
                        data.startTime = DateTimeParser.Parse(strartTimeText);
                        data.enterTime = DateTimeParser.Parse(strartTimeText).AddSeconds(-60);
                        SendTnmtNotification(data.tn, data.enterTime);
                    }
                }
                if (Cafe.instance)
                {
                    Cafe.instance.tnmtList.RefreshGameList();
                }

                MyTournamentCount();
                break;
            case PCProtocol.PC_RANKING:
                rankData = c;
                rankEvent.Invoke();
                break;
            case PCProtocol.PC_CONSOLE_USERINFO:
                MyStatus.type = (int)c["type"];
                MyStatus.zc = (long)c["zc"];
                MyStatus.dc = (long)c["dc"];
                MyStatus.cc = (long)c.ValueOrDefault<long>("cc", MyStatus.cc);

                MyStatus.uid = (string)c["uid"];
                MyStatus.icon_no = c["icon"].ToObject<int>();
                try
                {
                    JObject data = c["limit"].ToObject<JObject>();
                    //Console.SpecialLog(data.ToString());
                    LimitSet(data);
                }
                catch (Exception e)
                {
                    Console.Error(e.Message);
                }

                bool usePhoto = false;
                try
                {
                    JObject option = c["option"] as JObject;
                    usePhoto = option.ValueOrDefault("usePhoto", false);
                    MyStatus.usePhotoURL = usePhoto;
                }
                catch (Exception e)
                {
                    Console.Log(e.Message);
                }

                useUrlToggle.isOn = usePhoto;

                InitInfo();

                break;
            case PCProtocol.PC_USER_GAMEINFO:
                UserGameInfo(c);
                break;
            case PCProtocol.PC_TNMT_UPDATE_APPLY_COUNT:
                {
                    int tn = (int)c["tn"];
                    long countAllUser = (long)c["countAllUser"];
                    //lobbyRoomList.UpdateTournamentApplyCount(tn, countAllUser);
                }

                break;
            case PCProtocol.PC_ROOM_LEAVE:
                {
                    var gid = c["gid"].ToObject<string>();
                    if (gid == MyStatus.gid)
                    {
                        GetUserInfo();
                    }
                }

                break;
            case PCProtocol.PC_USER_DAILY_LOSS_LIMIT_UPDATE:
                limitChangeWindow.SuccessLimit();
                break;
            case PCProtocol.PC_USER_DAILY_LOSS_LIMIT_UPDATE_FAIL:
                limitChangeWindow.FailedLimit();
                break;

            case PCProtocol.PC_CAFE_JOIN:
                CafeJoinResult(c);
                break;
            case PCProtocol.PC_CAFE_TRANS_ZUICE:
                MyStatus.zc = (long)c["zc"];
                UpdateChipText();
                break;
            case PCProtocol.PC_CAFE_MEMBER_ORDER:
                {
                    if (!c.ContainsKey("cafeMemberOrder"))
                        break;
                    var status = (TRANSFER_TYPE)(int)c["cafeMemberOrder"]["status"];
                    switch (status)
                    {
                        case TRANSFER_TYPE.transfer:
                            NormalMessage.instance.OnOneButtonMessagePopUp("transfer_success");
                            break;
                        case TRANSFER_TYPE.transfer2:
                            NormalMessage.instance.OnOneButtonMessagePopUp("transfer2_success");
                            break;

                        case TRANSFER_TYPE.sell:
                            NormalMessage.instance.OnOneButtonMessagePopUp("sell_success");
                            break;
                        case TRANSFER_TYPE.buy:
                            NormalMessage.instance.OnOneButtonMessagePopUp("buy_success");
                            break;
                        case TRANSFER_TYPE.borrow:
                            NormalMessage.instance.OnOneButtonMessagePopUp("borrow_success");
                            break;
                    }
                }
                break;
            case PCProtocol.PC_CAFE_MEMBER_ORDER_UPDATE:
                {
                    bool isMe = c.ContainsKey("cafeMember");
                    if (isMe)
                    {
                        TRANSFER_TYPE status = TRANSFER_TYPE.none;
                        if (c.ContainsKey("status"))
                        {
                            status = (TRANSFER_TYPE)(int)c["status"];
                        }
                        if (c.ContainsKey("cafeMemberOrder"))
                        {
                            status = (TRANSFER_TYPE)(int)c["cafeMemberOrder"]["status"];
                        }

                        switch (status)
                        {
                            case TRANSFER_TYPE.sold:
                            case TRANSFER_TYPE.bought:
                            case TRANSFER_TYPE.borrowed:
                                NormalMessage.instance.AddSimpleMessage(
                                    LocalizeManager.GetLocalString("confirm_success")
                                );
                                break;
                            case TRANSFER_TYPE.sell_rejected:
                            case TRANSFER_TYPE.buy_rejected:
                            case TRANSFER_TYPE.borrow_rejected:
                                NormalMessage.instance.AddSimpleMessage(
                                    LocalizeManager.GetLocalString("decline_success")
                                );
                                break;
                        }
                        //RequestGameList();
                        //RequestTnmtList();
                        //ReauestCafeList();
                    }
                }
                break;
            case PCProtocol.PC_ROOM_IN_PLAYING:
                if (c.ValueOrDefault("ecode", 0) == 0)
                {
                    ReenterFlow(c);
                }
                LoadingCircle.Instance.StopSpin();
                break;
            case PCProtocol.PC_CAFE_UPDATE:
            case PCProtocol.PC_CAFE_CREATE:
                if (c.ValueOrDefault("ecode", 0) == 0)
                {
                    long zc = (long)c.ValueOrDefault("zc", MyStatus.zc);
                    MyStatus.zc = zc;

                    UpdateChipText();
                }
                break;

            case PCProtocol.PC_CAFE_RESERVE_REMOVE:
                {
                    if (c.ValueOrDefault("ecode", -1) != 0)
                        break;

                    ReauestCafeList();
                }
                break;

            case PCProtocol.PC_MESSAGE:
                {
                    //            gid = c["gid"].ToString();


                    //            int cafeIdx = (long)c["money_total"];
                    //            int 					gtn;
                    //char[STRING_32_LEN+1] 	gid;
                    //int 					type;
                    //char[STRING_64_LEN+1] 	msg;
                    //char[STRING_32_LEN+1] 	time;
                }
                break;
            case PCProtocol.PC_TNMT_MY_RESULT:
                {
                    tnmtResultPopup?.SetTnmtResult(c);
                    var tn = c.ValueOrDefault("tn", 0);
                    MyStatus.RemoveTnmt(tn);
                    //(customUIOpener as CustomUIOpenerLobby).tnmtResult = c;
                    //customUIOpener.ShowUI("mtt_result");
                }
                break;
            case PCProtocol.PC_TNMT_MOVE_ROOM:
                {
                    var tn = c.ValueOrDefault("tn", 0);
                    TnmtData tournament = MyStatus.GetTnmt(tn);
                    if (tournament == null)
                    {
                        var exception = new Exception($"MyStatus에 토너먼트 정보 없음 {MyStatus.nick} {tn}");
                        Debug.LogException(exception);
                        tournament = MyStatus.AddTnmt(tn);
                        var tInfo = InfoManager.Instance.GetTournamentInfo(tn);
                        if (tInfo != null)
                        {
                            tournament.cafeIdx = tInfo.info.ValueOrDefault("cafeIdx", 0);
                            tournament.state = tInfo.info.ValueOrDefault("state", TNMT_FLOW.none);
                            var strartTimeText = tInfo.info.ValueOrDefault(
                                "t_start_time",
                                string.Empty
                            );
                            if (!string.IsNullOrEmpty(strartTimeText))
                            {
                                tournament.startTime = DateTimeParser.Parse(strartTimeText);
                                tournament.enterTime = DateTimeParser
                                    .Parse(strartTimeText)
                                    .AddSeconds(-60);
                            }
                        }
                    }

                    tournament.gtn = (int)c["gtn"];
                    Packet CP_PLAY_GAME_ENTER = new Packet((int)CPProtocol.CP_PLAY_GAME_ENTER);
                    CP_PLAY_GAME_ENTER.Add("gtn", tournament.gtn);
                    CP_PLAY_GAME_ENTER.Add("tn", tournament.tn);
                    WebSocketManager.defaultCli.Send(CP_PLAY_GAME_ENTER);
                }
                break;

            case PCProtocol.PC_KINGSHILL_POINT_IN: { }
                break;
            case PCProtocol.PC_KINGSHILL_POINT_OUT: { }
                break;
        }
    }

    public async void ReenterFlow(JObject c)
    {
        List<long> enteredRoomList;
        if (GamesManager.instance)
        {
            enteredRoomList = GamesManager.instance.GetTableList();
        }
        else
        {
            enteredRoomList = new List<long>();
        }

        JArray game_list;
        if (c.ContainsKey("game_list"))
            game_list = c["game_list"] as JArray;
        else
            game_list = new JArray();
        if (game_list.Count == 0)
        {
            LoadingCircle.Instance.StopSpin();
        }
        for (int i = 0; i < game_list.Count && i < 4; i++)
        {
            JObject game = game_list[i] as JObject;
            int tn = game.ValueOrDefault("tn", 0);
            int gtn = game.ValueOrDefault("gtn", 0);
            if (tn == 0)
            {
                if (gtn != 0)
                {
                    Packet CP_PLAY_GAME_ENTER = new Packet((int)CPProtocol.CP_PLAY_GAME_ENTER);
                    CP_PLAY_GAME_ENTER.Add("gtn", gtn);

                    WebSocketManager.defaultCli.Send(CP_PLAY_GAME_ENTER);
                    await new WaitForPCProtocol(PCProtocol.PC_PLAY_GAME_ENTER);
                }
            }
            enteredRoomList.Remove(gtn);
        }

        foreach (var gtn in enteredRoomList)
        {
            GamesManager.instance.RemoveGameTable(gtn);
        }

        GameManager.instance.LoadEnd("ent");
        var observerRoomList = GamesManager.instance.GetObserverRooms();

        foreach (var gtn in observerRoomList)
        {
            Packet CP_PLAY_GAME_ENTER_OBSERVER = new Packet(
                (int)CPProtocol.CP_PLAY_GAME_ENTER_OBSERVER
            );
            CP_PLAY_GAME_ENTER_OBSERVER.Add("gtn", gtn);
            WebSocketManager.defaultCli.Send(CP_PLAY_GAME_ENTER_OBSERVER);

            enteredRoomList.Remove(gtn);
            await new WaitForPCProtocol(PCProtocol.PC_PLAY_GAME_ENTER_OBSERVER);
        }

        GamesManager.instance.ClearObserverRooms();

        GameManager.instance.LoadEnd("ob");
    }

    public void CafeJoinResult(JObject c)
    {
        CAFE_JOIN_STATUS status = c.ValueOrDefault<CAFE_JOIN_STATUS>(
            "status",
            CAFE_JOIN_STATUS.none
        );
        switch (status)
        {
            case CAFE_JOIN_STATUS.applied:
                NormalMessage.instance.OnOneButtonMessagePopUp("cafe_join_success");
                break;
            case CAFE_JOIN_STATUS.rejected:
                NormalMessage.instance.OnOneButtonMessagePopUp("cafe_join_rejected_p");
                break;
            case CAFE_JOIN_STATUS.accepted:
                NormalMessage.instance.OnOneButtonMessagePopUp("cafe_join_success_p");
                break;
        }
    }

    public void RequestGameList()
    {
        var p = new Packet((int)CPProtocol.CP_PLAY_GAME_LIST);
        WebSocketManager.defaultCli.Send(p.ToJson());
    }

    public void RequestTnmtList()
    {
        var p = new Packet((int)CPProtocol.CP_TNMT_LIST);
        WebSocketManager.defaultCli.Send(p.ToJson());
    }

    public void ReauestCafeList()
    {
        var p = new Packet((int)CPProtocol.CP_CAFE_LIST);
        WebSocketManager.defaultCli.Send(p.ToJson());
    }

    public void OnChangedGameTypeTab(GAME_TYPE gameType)
    {
        this.curGameType = gameType;
        pickGameTypeTab = gameType;
        //List<JsonData> rules;
        //JsonDataParser.Parse(RoomOptions.rules[gameType + "_list"],out rules);
        //EnterChipButton button = this.gameEnterButtonContainer.transform.GetChild(0).GetComponent<EnterChipButton>();
        // long bng,sb,bb,emn;
        // JsonDataParser.Parse(rules[0]["bng"],out bng);
        // JsonDataParser.Parse(rules[0]["sm"],out sb);
        // JsonDataParser.Parse(rules[0]["bg"],out bb);
        // JsonDataParser.Parse(rules[0]["emn"],out emn);
        // button.Setting(bng,emn,sb,bb);
        this.gameEnterScrollView.StopMovement();
        this.gameEnterScrollView.horizontalNormalizedPosition = 0;
        //if(gameType == "baccarat") {

        //} else if(gameType == GAME_TYPE.NLH){
        //    // lobbyRoomList.GetPlayGameList(gameType);
        //} else if(gameType == "badugi")
        //{
        //}
    }

    public Text countDownText;
    public GameObject CountDownPanel;

    private IEnumerator CountDownCo()
    {
        var notEnterTnmts = MyStatus.GetNotEnterTnmt();
        var enterTime = DateTime.UtcNow.AddSeconds(-60);
        var wait = new WaitForSeconds(0.1f);
        GameManager.instance.LoadEnd("tnmt");
        while (notEnterTnmts.Count > 0)
        {
            CountDownPanel.SetActive(true);
            enterTime = notEnterTnmts[0].enterTime;
            if (DateTime.UtcNow > enterTime)
            {
                EnterTnmtRoom(notEnterTnmts[0]);
                notEnterTnmts.RemoveAt(0);
            }
            else
            {
                var count = notEnterTnmts[0].startTime - DateTime.UtcNow;
                countDownText.text = string.Format(
                    "토너먼트 남은 시간 : {0}",
                    TimeToString.SpanToString(count)
                );
                CountDownPanel.SetActive(count.TotalSeconds > 0);
            }
            yield return wait;
        }
        CountDownPanel.SetActive(false);
    }

    public void EnterTnmtRoom(TnmtData tnmtData)
    {
        Packet p = new Packet((int)CPProtocol.CP_PLAY_GAME_ENTER);
        p.Add("gtn", tnmtData.gtn);
        p.Add("tn", tnmtData.tn);
        WebSocketManager.defaultCli.Send(p.ToJson());
    }

    private void LoginCallBack(long statusCode)
    {
        Debug.Log("RequestPupLogin CallBack");
        if (statusCode == 200) // 게임 로그인
        {
            RequestGameLoginData();
        }
        else if (statusCode == 418) // 회원가입
        {
            Debug.Log("회원가입");
            //LoadingCircle.Instance.StopSpin();
            //RandomNicknameSet();
            //OpenNicknameCheckPanel();
        }
        else // 에러
        {
            LoadingCircle.Instance.StopSpin();

            ErrorMessageManager.Instance.AddGameError(
                (int)statusCode,
                "SYS_ERR_LOGIN",
                "SYS_ERR_LOGIN_NOT_DEFINED"
            );
            //OnClickLogOut();
            //login.SetActive(true);
        }
    }

    private async void RequestGameLoginData()
    {
        var response = await PublisherApiManager.Instance.GetGameLoginDataPubAPI();

        if (response.code == 200)
        {
            string uid = response.json.GetValue("userid").ToString();
            string pw = response.json.GetValue("password").ToString();
            Debug.Log(string.Format("{0} / {1}", uid, pw));
            StartCoroutine(RequestLogin(uid, pw));
        }
        else
        {
            LoadingCircle.Instance.StopSpin();

            ErrorMessageManager.Instance.AddGameError(
                (int)response.code,
                "SYS_ERR_LOGIN",
                "SYS_ERR_LOGIN_NOT_DEFINED"
            );
        }
    }

    public void RequestUserGameInfo()
    {
        Packet p = new Packet((int)CPProtocol.CP_USER_GAMEINFO);
        p.Add("gid", MyStatus.gid);
        WebSocketManager.defaultCli.Send(p.ToJson());
    }

    public void UserGameInfo(JObject data)
    {
        JObject info = data["info"] as JObject;
        JObject holdem = null;
        JObject tournament = null;
        JObject baccarat = null;
        JObject badugi = null;
        try
        {
            holdem = info["holdem"] as JObject;
        }
        catch { }
        try
        {
            tournament = info["tournament"] as JObject;
        }
        catch { }
        try
        {
            baccarat = info["baccarat"] as JObject;
        }
        catch { }
        try
        {
            badugi = info["badugi"] as JObject;
        }
        catch { }

        holdemInfo?.SetInfo(holdem);
        tournamentInfo?.SetInfo(tournament);
        totalInfo?.SetTotalInfo(holdem, tournament);
        baccaratInfo?.SetInfo(baccarat);
        badugiInfo?.SetInfo(badugi);

        //int hwin;
        //int hflop;
        //int hturn;
        //int hriver;
        //int hpreflop;
        //int hlose;
        //int hcall;
        //int hfold;
        //int hcheck;
        //int hraise;
        //string hbest;
        //int hwinMoney;
        //int hloseMoney;
        //int hMaxWin;
        //long hBestValue;
        //if(holdem != null)
        //{
        //hwin = holdem["win"].ToObject<int>();
        //hflop = holdem["fold"]["flop"].ToObject<int>();
        //hturn = holdem["fold"]["turn"].ToObject<int>();
        //hriver = holdem["fold"]["river"].ToObject<int>();
        //hpreflop = holdem["fold"]["preflop"].ToObject<int>();
        //hlose = holdem["lose"].ToObject<int>();
        //hcall = holdem["betting"]["call"].ToObject<int>();
        //hfold = holdem["betting"]["fold"].ToObject<int>();
        //hcheck = holdem["betting"]["check"].ToObject<int>();
        //hraise = holdem["betting"]["raise"].ToObject<int>();
        //hbest = holdem["bestHand"].ToObject<string>();
        //hwinMoney = holdem["winMoney"].ToObject<int>();
        //hloseMoney = holdem["loseMoney"].ToObject<int>();
        //hMaxWin = holdem["maxWinMoney"].ToObject<int>();
        //hBestValue = holdem["bestHandValue"].ToObject<long>();


        //    holdemInfo.SetInfo(holdem);

        //}
        //else
        //{
        //    holdemInfo.SetInfo(null);
        //}

        //int twin;
        //int tturn;
        //int tflop;
        //int triver;
        //int tpreflop;
        //int tlose;
        //int tcall;
        //int tfold;
        //int tcheck;
        //int traise;
        //string tbest;
        //int twinMoney;
        //int tloseMoney;
        //int tMaxWin;
        //long tBestValue;
        //if(tournament != null)
        //{
        //twin = tournament["win"].ToObject<int>();
        //tflop = tournament["fold"]["flop"].ToObject<int>();
        //tturn = tournament["fold"]["turn"].ToObject<int>();
        //triver = tournament["fold"]["river"].ToObject<int>();
        //tpreflop = tournament["fold"]["preflop"].ToObject<int>();
        //tlose = tournament["lose"].ToObject<int>();
        //tcall = tournament["betting"]["call"].ToObject<int>();
        //tfold = tournament["betting"]["fold"].ToObject<int>();
        //tcheck = tournament["betting"]["check"].ToObject<int>();
        //traise = tournament["betting"]["raise"].ToObject<int>();
        //tbest = tournament["bestHand"].ToObject<string>();
        //twinMoney = tournament["winMoney"].ToObject<int>();
        //tloseMoney = tournament["loseMoney"].ToObject<int>();
        //tMaxWin = tournament["maxWinMoney"].ToObject<int>();
        //tBestValue = tournament["bestHandValue"].ToObject<long>();
        //tournamentInfo.SetInfo(tournament);
        //if(holdem == null)
        //{
        //    totalInfo.SetInfo(tournament);
        //}
        //else
        //{
        //hwin = holdem["win"].ToObject<int>();
        //hflop = holdem["fold"]["flop"].ToObject<int>();
        //hturn = holdem["fold"]["turn"].ToObject<int>();
        //hriver = holdem["fold"]["river"].ToObject<int>();
        //hpreflop = holdem["fold"]["preflop"].ToObject<int>();
        //hlose = holdem["lose"].ToObject<int>();
        //hcall = holdem["betting"]["call"].ToObject<int>();
        //hfold = holdem["betting"]["fold"].ToObject<int>();
        //hcheck = holdem["betting"]["check"].ToObject<int>();
        //hraise = holdem["betting"]["raise"].ToObject<int>();
        //hbest = holdem["bestHand"].ToObject<string>();
        //hwinMoney = holdem["winMoney"].ToObject<int>();
        //hloseMoney = holdem["loseMoney"].ToObject<int>();
        //hMaxWin = holdem["maxWinMoney"].ToObject<int>();
        //hBestValue = holdem["bestHandValue"].ToObject<long>();
        //        totalInfo.SetInfo(tournament);
        //    }
        //}
        //else
        //{
        //tournamentInfo.SetInfo(0,0,0,0,0,0,0,0,0,0,"",0,0,0);
        //}

        //if(holdem == null && tournament == null)
        //{
        //totalInfo.SetInfo(0,0,0,0,0,0,0,0,0,0,"",0,0,0);
        //}
    }

    private IEnumerator RequestLogin(string uid, string password)
    {
        //Debug.Log(string.Format("{{\"p\":{0}, \"c\":{{ \"os\":\"{1}\", \"browser\":\"{2}\", \"client_type\":\"{3}\", \"token\":\"{4}\" }}}}", 20, "android", "test", "unity", FirebaseManager.Instance.token));
        //WebSocketManager.defaultCli.Send(string.Format("{{\"p\":{0}, \"c\":{{ \"os\":\"{1}\", \"browser\":\"{2}\", \"client_type\":\"{3}\", \"token\":\"{4}\" }}}}", 20, "android", "test", "unity", FirebaseManager.Instance.token));
        //isLoginFirebase = true;

        yield return new WaitForSeconds(0.3f);

        Packet p = new Packet((int)CPProtocol.CP_CONSOLE_LOGIN);
        p.Add("os", "android");
        p.Add("browser", "android");
        p.Add("client_type", "basic");
        p.Add("acc", uid); //Firebase.Auth.FirebaseAuth.DefaultInstance.CurrentUser.UserId);
        p.Add("pass", password);
        WebSocketManager.defaultCli.Send(p.ToJson());
    }

    private Coroutine tnCo;

    public void MyTournamentCount()
    {
        if (tnCo != null)
        {
            StopCoroutine(tnCo);
        }
        tnCo = StartCoroutine(CountDownCo());
    }

    public void RequestRanking(int type)
    {
        var p = new Packet((int)CPProtocol.CP_RANKING);
        p.Add("type", type);
        WebSocketManager.defaultCli.Send(p);
    }

    public void OnClickChangeLimit()
    {
        long limit = 0;
        if (limittype == MyStatus.limittype)
        {
            string title = LocalizeManager.GetLocalString("SYS_ERR_TRY_SAME_VALUE");
            string info = LocalizeManager.GetLocalString("SYS_ERR_NOT_CHANGE_SAME_VALUE");
            limitFailedTileText.text = title;

            limitFailedText.text = info;
            limitChangeFailWindow.SetActive(true);
            return;
        }
        limit = MyStatus.lossLimits[limittype];
        Packet p = new Packet((int)CPProtocol.CP_USER_DAILY_LOSS_LIMIT_UPDATE);
        p.Add("limit", limit);
        WebSocketManager.defaultCli.Send(p);
    }

    public void LimitSet(JObject data)
    {
        MyStatus.limitData = data;

        long dailyLose = data["dailyLoss"].ToObject<long>();
        long dailyLossLimit = data["dailyLossLimit"].ToObject<long>();
        long[] dailyLossLimits = data.ValueOrDefault("dailyLossLimits", new long[0]);
        JArray history = data["dailyLossLimitUpdateDate"].ToObject<JArray>();

        MyStatus.lossLimit = dailyLossLimit;
        MyStatus.lossLimits = dailyLossLimits;
        for (int i = 0; i < dailyLossLimits.Length; i++)
        {
            if (dailyLossLimits[i] == dailyLossLimit)
            {
                MyStatus.limittype = i;
                break;
            }
        }

        //getLostText.text = string.Format("{0}{1}", dailyLose>0?"-":"",MoneyToString.Converting(dailyLose));
        //limitButtonText.text = string.Format("-{0}", MoneyToString.Converting(dailyLossLimit));
        //for(int i = 0; i < history.Count; i++)
        //{
        //    JObject d = history[i].ToObject<JObject>();
        //    LimitChangeHistorySlot slot;
        //    if(i < limitChangeHistoryList.Count)
        //    {
        //        slot = limitChangeHistoryList[i];
        //    }
        //    else
        //    {
        //        slot = Instantiate(limitHistorySlotPrefab, limitHistoryContainer).GetComponent<LimitChangeHistorySlot>();
        //        limitChangeHistoryList.Add(slot);
        //    }
        //    slot.historyChipText.text = d["limit"].ToObject<string>();
        //    slot.historyTimeText.text = d["date"].ToObject<string>();
        //    slot.historyButton.interactable = i == 0;
        //    slot.buttonText.text = i == 0 ? "변경하기" : "변경완료";
        //}
    }

    public void OnchangeLimit(int limit)
    {
        limittype = limit;
    }

    public void SuccessLimit()
    {
        //changeResultText.text = limittype == LIMITTYPE.MAX_LIMIT ? "(1000억 -> 5000억)" : "(5000억 -> 1000억)";
        //MyStatus.limittype = limittype;
        //limitButtonText.text = string.Format("1일 손실 기준 : -{0}", limittype == LIMITTYPE.MAX_LIMIT? "5000억":"1000억");
        //limitChangeResultWindow.SetActive(true);
        //limitSetWindow.SetActive(false);
        //limitHistoryPanel.SetActive(false);
        GetUserInfo();
    }

    public void FailedLimit()
    {
        limitSetWindow.SetActive(false);
        limitHistoryPanel.SetActive(false);
        string title = LocalizeManager.GetLocalString("SYS_ERR_DAILY_LOSS_LIMIT_CHANGE_COUNT_OVER");

        string info = LocalizeManager.GetLocalString(
            "SYS_ERR_DAILY_LOSS_LIMIT_CHANGE_COUNT_OVER_EXPLAN"
        );

        limitFailedTileText.text = title;
        limitFailedText.text = info;
        limitChangeFailWindow.SetActive(true);
    }

    bool show_debug_info = true;

    private async void OnGUI()
    {
        //if(DevOptionsManager.devOptions.mode == MODE.prod)
        //{
        //    return;
        //}

        //GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
        //buttonStyle.fontSize = 20; // 폰트 크기를 20으로 설정

        //int xl = 200;
        //int yl = 80;
        //int y = 500;
        //int yspace = 20;
        //int x = Screen.width - xl;

        //if (GUI.Button(new Rect(x, y,  xl, yl), "킹스 userLink", buttonStyle))
        //{
        //    Packet p = new Packet((int)CPProtocol.CP_KINGSHILL_USER_LINK);
        //    p.Add("phoneNum", "01026952104");
        //    WebSocketManager.defaultCli.Send(p.ToJson());
        //}
        //y += yl + yspace;
        //if (GUI.Button(new Rect(x, y,  xl, yl), "킹스 code", buttonStyle))
        //{
        //    Packet p = new Packet((int)CPProtocol.CP_KINGSHILL_USER_LINK);
        //    p.Add("phoneNum", "01026952104");
        //    p.Add("code", "765515");
        //    WebSocketManager.defaultCli.Send(p.ToJson());
        //}
        //y += yl + yspace;
        //if (GUI.Button(new Rect(x, y,  xl, yl), "킹스 유저정보", buttonStyle))
        //{
        //    Packet p = new Packet((int)CPProtocol.CP_KINGSHILL_GET_USER_INFO);
        //    p.Add("cafeIdx", 1);
        //    WebSocketManager.defaultCli.Send(p.ToJson());
        //}
        //y += yl + yspace;
        //if (GUI.Button(new Rect(x, y,  xl, yl), "킹스 pontIn", buttonStyle))
        //{
        //    for( int i = 0 ; i < 1 ; i ++){
        //        Packet p = new Packet((int)CPProtocol.CP_KINGSHILL_POINT_IN);
        //        p.Add("cafeIdx", 1);
        //        p.Add("point",100);
        //        WebSocketManager.defaultCli.Send(p.ToJson());
        //    }
        //}
        //y += yl + yspace;
        //if (GUI.Button(new Rect(x, y,  xl, yl), "킹스 pontOut", buttonStyle))
        //{
        //    Packet p = new Packet((int)CPProtocol.CP_KINGSHILL_POINT_OUT);
        //    p.Add("cafeIdx", 1);
        //    p.Add("point",100);
        //    WebSocketManager.defaultCli.Send(p.ToJson());
        //}
        //y += yl + yspace;

        //if (GUI.Button(new Rect(x, y,  xl, yl), "카페생성", buttonStyle))
        //{
        //    Packet p = new Packet((int)CPProtocol.CP_CAFE_CREATE);
        //    JObject o = new JObject();
        //    o.Add("name", "testCafe");
        //    o.Add("desc", "testCafe");
        //    o.Add("icon", 1);
        //    o.Add("tier", 5);
        //    p.Add("o", o);
        //    WebSocketManager.defaultCli.Send(p.ToJson());
        //}
        //y += yl + yspace;

        //return;


        //if (GUI.Button(new Rect(x, y,  xl, yl), "유료칩 방입장", buttonStyle))
        //{
        //    Packet p = new Packet((int)CPProtocol.CP_PLAY_GAME_ENTER);
        //    p.Add("gtn", 0);
        //    p.Add("chip_type", (int)CHIP_TYPE.zc);
        //    p.Add("option_level", 1);

        //    WebSocketManager.defaultCli.Send(p.ToJson());
        //}
        //y += yl + yspace;
        //if (GUI.Button(new Rect(x, y, xl, yl), "무료칩 방입", buttonStyle))
        //{
        //    Packet p = new Packet((int)CPProtocol.CP_PLAY_GAME_ENTER);
        //    p.Add("gtn", 0);
        //    p.Add("chip_type", (int)CHIP_TYPE.dc);
        //    p.Add("option_level", 8);

        //    WebSocketManager.defaultCli.Send(p.ToJson());
        //}
        //y += yl + yspace;
        //if (GUI.Button(new Rect(x, y, xl, yl), "silverr가져오기", buttonStyle))
        //{
        //    UnityWebRequest pubwww = await PublisherApiManager.Instance.GetUserInfoPubAPIAsync(PublisherApiManager.Instance.url + API.Get.INFO);
        //    if (pubwww.responseCode == 200)
        //    {
        //        JObject data = JObject.Parse(pubwww.downloadHandler.text);

        //        var login = data["property"]["login"] as JObject;
        //        MyStatus.loginContinued = (int)login["loginCountContinued"];
        //        MyStatus.isLoginRewarded = (bool)login["isRecvAttendanceReward"];
        //        MyStatus.safeGc = data["safe_gold"].ToObject<long>();
        //        MyStatus.safeSc = data["safe_silver"].ToObject<long>();
        //        MyStatus.safeRu = data["safe_ruby"].ToObject<long>();

        //        try
        //        {
        //            long gold = data["gold"].ToObject<long>();
        //            long silver = data["silver"].ToObject<long>();
        //            if (gold > 0)
        //            {
        //                await PublisherApiManager.Instance.PointInAPI("gold", gold, PointInAPICallBack);
        //            }
        //            if (silver > 0)
        //            {
        //                await PublisherApiManager.Instance.PointInAPI("silver", silver, PointInAPICallBack);
        //            }
        //        }
        //        catch(Exception e)
        //        {

        //        }
        //    }
        //}

        //y += yl + yspace;
        //if( GUI.Button( new Rect(x,y, xl, yl), "/mail", buttonStyle)){
        //    int page = 1;
        //    int per = 100;
        //    PublisherApiManager.Instance.GetMailListPubApi((long statusCode, JObject data) =>   {
        //        switch(statusCode)
        //        {
        //            case 200:
        //                Console.Log("mailList: " + data.ToString());
        //            break;
        //            default:
        //                Console.Error("Faild Get Mails");
        //            break;
        //        }
        //    },
        //    page, per);
        //}

        //y += yl + yspace;
        //if( GUI.Button( new Rect(x,y, xl, yl), "유료 테이블수", buttonStyle)){
        //    Packet p = new Packet((int)CPProtocol.CP_COUNT_OF_TABLES_ZC);
        //    WebSocketManager.defaultCli.Send(p.ToJson());
        //}

        //y += yl + yspace;
        //if( GUI.Button( new Rect(x,y, xl, yl), "/item", buttonStyle)){
        //    PublisherApiManager.Instance.GetInvenListPubAPI((long statusCode, JObject data) =>   {
        //        switch(statusCode)
        //        {
        //            case 200:
        //                Console.Log("My itemList: " + data.ToString());
        //            break;
        //            default:
        //                Console.Error("Faild Get Mails");
        //            break;
        //        }
        //    });
        //}

        //y += yl + yspace;
        //if (GUI.Button(new Rect(x, y, xl, yl), "Create Game", buttonStyle))
        //{
        //    CP_CAFE_GAME_CREATE(1);
        //}

        //GUI.Button(new Rect(0, 0, 100, 50), avgFrameRate.ToString());
    }

    private void CP_CAFE_GAME_CREATE(int optionLevel)
    {
        string game_type = GAME_TYPE.nlh.ToString();
        var option = JsonDataParser.Parse<List<JObject>>(RoomOptions.rules[game_type + "_list"])[
            optionLevel - 1
        ];
        var obj = JObject.Parse(GameConfig.defaltGameOption);
        obj["game_type"] = (int)GAME_TYPE.nlh;
        obj["chip_type"] = (int)option.ValueOrDefault("chip_type", 0);
        var rd = new RoomData();
        obj["ante"] = option["ante"];
        obj["buyin_min"] = option["buyin_min"];
        obj["buyin_max"] = option["buyin_max"];
        obj["blind"] = option["blind"];
        obj["small_blind"] = option["small_blind"];
        obj["cafeIdx"] = 1;
        //roomDataList[gameType.ToString()].Add(rd);
        //rd.ante =
        //rd.sb = option["small_blind"];
        //rd.bb = option["blind"];
        //rd.emn = option["buyin_min"];
        //rd.level = rules[i].ValueOrDefault("level", 0);
        //rd.chip_type = rules[i].ValueOrDefault("chip_type", CHIP_TYPE.cc);
        //obj["cafeIdx"] = 1;



        //switch (System.Enum.Parse(typeof(GAME_TYPE), game_type))
        //{
        //    case GAME_TYPE.short_deck:
        //        obj["ante"] = (long)(obj["blind"]);
        //        break;
        //    default:
        //        obj["ante"] = (long)((double)obj["ante"] * bb);
        //        break;
        //}

        //obj.Remove("buyin");
        StartCoroutine(GameCreateFlow(obj));
    }

    private IEnumerator GameCreateFlow(JObject gameData)
    {
        Packet p = new Packet(CPProtocol.CP_CAFE_GAME_CREATE);
        p.Add("o", gameData);
        WebSocketManager.defaultCli.Send(p);
        WaitForPCProtocol wait = new WaitForPCProtocol(PCProtocol.PC_CAFE_GAME_CREATE);
        yield return wait;
        if (wait.Result.c.ValueOrDefault("ecode", 0) == 0)
        {
            var game = wait.Result.c.CastOrEmpty<JObject>("playGame");
            var gtn = game.ValueOrDefault("gtn", -1);
            if (gtn >= 0)
            {
                p = new Packet(CPProtocol.CP_PLAY_GAME_ENTER);
                p.Add("gtn", gtn);
                WebSocketManager.defaultCli.Send(p);
            }
        }
        else
            NormalMessage.instance.OnOneButtonMessagePopUp("table_fail");
    }

    public int avgFrameRate;

    public void Update()
    {
        float current = 0;
        current = Time.frameCount / Time.time;
        avgFrameRate = (int)current;
    }
}

public enum LIMITTYPE
{
    MAX_LIMIT = 0,
    MINLIMIT = 1
};
