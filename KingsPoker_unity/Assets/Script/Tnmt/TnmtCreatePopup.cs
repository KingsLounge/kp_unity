using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Newtonsoft.Json.Linq;
using Cysharp.Threading.Tasks;

public class TnmtCreateDefaultValue
{
    private string _title;
    private int _anteType;
    private string _closeBlindupLevel;
    private string _buyin;
    private int _buyinPercent;
    private int _ticketCondition;
    private int _ticketType;
    private int _ticketCount;
    private string _buyinScaleMin;
    private string _buyinScaleMax;
    private int _earlyBirdOpenPercent;
    private int _beforeLevel;
    private int _beforeLevelPercent;
    private string _rewardAllChip;
    private int _rewardRankTable;
    private string _bettingTime;
    private int _blindupStructure;
    private int _blindupDuration;
    private string _startingChip;
    private string _reentryCount;
    private string _reentryCost;
    private string _reentryChip;
    private string _minPlayer;
    private string _maxPlayer;
    private string _quitPlayerCount;
    private string _jackpot;
    private int _tableColor;
    
    public string Title { get => _title; }
    public int AnteType { get => _anteType; }
    public string CloseBlindupLevel { get => _closeBlindupLevel; }
    public string Buyin { get => _buyin; }
    public int BuyinPercent { get => _buyinPercent; }
    public int TicketCondition { get => _ticketCondition; }
    public int TicketType { get => _ticketType; }
    public int TicketCount { get => _ticketCount; }
    public string BuyinScaleMin { get => _buyinScaleMin; }
    public string BuyinScaleMax { get => _buyinScaleMax; }
    public int EarlyBirdOpenPercent { get => _earlyBirdOpenPercent; }
    public int BeforeLevel { get => _beforeLevel; }
    public int BeforeLevelPercent { get => _beforeLevelPercent; }
    public string RewardAllChip { get => _rewardAllChip; }
    public int RewardRankTable { get => _rewardRankTable; }
    public string BettingTime { get => _bettingTime; }
    public int BlindupStructure { get => _blindupStructure; }
    public int BlindupDuration { get => _blindupDuration; }
    public string StartingChip { get => _startingChip; }
    public string ReentryCount { get => _reentryCount; }
    public string ReentryCost { get => _reentryCost; }
    public string ReentryChip { get => _reentryChip; }
    public string MinPlayer { get => _minPlayer; }
    public string MaxPlayer { get => _maxPlayer; }
    public string QuitPlayerCount { get => _quitPlayerCount; }
    public string Jackpot { get => _jackpot; }
    public int TableColor { get => _tableColor; }

    public TnmtCreateDefaultValue(string title, int anteType, string closeBlindupLevel, string buyin, int buyinPercent, int ticketCondition, int ticketType, int ticketCount, string buyinScaleMin, string buyinScaleMax, int earlyBirdOpenPercent, int beforeLevel, int beforeLevelPercent, string rewardAllChip, int rewardRankTable, string bettingTime, int blindupStructure, int blindupDuration, string startingChip, string reentryCount, string reentryCost, string reentryChip, string minPlayer, string maxPlayer, string quitPlayerCount, string jackpot, int tableColor)
    {
        _title = title;
        _anteType = anteType;
        _closeBlindupLevel = closeBlindupLevel;
        _buyin = buyin;
        _buyinPercent = buyinPercent;
        _ticketCondition = ticketCondition;
        _ticketType = ticketType;
        _ticketCount = ticketCount;
        _buyinScaleMin = buyinScaleMin;
        _buyinScaleMax = buyinScaleMax;
        _earlyBirdOpenPercent = earlyBirdOpenPercent;
        _beforeLevel = beforeLevel;
        _beforeLevelPercent = beforeLevelPercent;
        _rewardAllChip = rewardAllChip;
        _rewardRankTable = rewardRankTable;
        _bettingTime = bettingTime;
        _blindupStructure = blindupStructure;
        _blindupDuration = blindupDuration;
        _startingChip = startingChip;
        _reentryCount = reentryCount;
        _reentryCost = reentryCost;
        _reentryChip = reentryChip;
        _minPlayer = minPlayer;
        _maxPlayer = maxPlayer;
        _quitPlayerCount = quitPlayerCount;
        _jackpot = jackpot;
        _tableColor = tableColor;
    }
}

public class TnmtCreatePopup : MonoBehaviour
{
    private readonly string[] defaultSettingOptions = { "데일리", "Monster", "멀티테이블게임", "시티투어", "네셔널", "챔피언쉽", "시드권세틀", "시드권세틀2", "프리롤" };
    private readonly string[] anteTypeOptions = { "모두 앤티", "BB 앤티" };
    private readonly string[] buyinPercentOptions = { "0%", "5%", "10%", "15%", "20%", "25%" };
    private readonly double[] buyinPercentValues = { 0, 0.05, 0.1, 0.15, 0.2, 0.25 };
    private readonly string[] ticketConditionOptions = { "AND", "OR" };
    private readonly string[] ticketConditionValues = { "and", "or" };
    private readonly string[] ticketTypeOptions = { "-", "ticket", "ticket2", "ticket3" };
    private readonly string[] ticketCountOptions = { "-", "1장", "2장", "3장", "4장", "5장" };
    private readonly string[] earlyBirdOpenPercentOptions = { "0%", "5%", "10%", "15%", "20%", "25%" };
    private readonly double[] earlyBirdOpenPercentValues = { 0, 5, 10, 15, 20, 25 };
    private readonly string[] beforeLevelOptions = { "0", "3", "4", "5", "6", "7", "8", "10", "12" };
    private readonly double[] beforeLevelValues = { 0, 3, 4, 5, 6, 7, 8, 10, 12 };
    private readonly string[] beforeLevelPercentOptions = { "0%", "5%", "10%", "15%", "20%", "25%" };
    private readonly double[] beforeLevelPercentValues = { 0, 5, 10, 15, 20, 25 };
    private readonly string[] rewardRankTableOptions = { "-", "기본 자동 칩", "1등12만 2등 4만", "1등25만 2등 5만", "1등 6장 2등 3장", "1등 13장 2등 5장", "자동 칩" };
    private readonly int[] rewardRankTableValues = { 0, 1, 4, 5, 7, 8, 9 };
    private readonly string[] blindupStructureOptions = { "데일리", "Monster", "멀티테이블게임", "월간 시티투어" };
    private readonly int[] blindupStructureValues = { 1, 2, 3, 4 };
    private readonly string[] blindupDurationOptions = { "2분", "4분", "6분", "8분", "10분", "15분", "20분", "30분" };
    private readonly double[] blindupDurationValues = { 120, 240, 360, 480, 600, 900, 1200, 1800,};
    private readonly string[] tableColorOptions = { "기본", "블랙", "옐로우", "레드", "그린", "블루" };
    private readonly string[] tagOptions = { "안함", "HOT", "이벤트"};

    private readonly TnmtCreateDefaultValue[] tnmtCreateDefaultValues =
    {
        new TnmtCreateDefaultValue("Daily! 데일리",    1, "0", "20000",    0, 0, 0, 0, "1", "1",   2, 2, 1, "0", 2, "20", 0, 1, "12000", "2", "20000", "18000", "5", "18", "1", "20000", 0),        
        new TnmtCreateDefaultValue("Monster",          1, "0", "40000",    0, 0, 0, 0, "1", "1",   2, 2, 1, "0", 3, "20", 0, 1, "30000", "2", "40000", "40000", "5", "18", "1", "40000", 0),
        new TnmtCreateDefaultValue("멀티테이블게임",   1, "0", "30000",    0, 0, 0, 0, "1", "1",   2, 2, 1, "0", 1, "20", 0, 1, "25000", "2", "30000", "30000", "5", "1000", "1", "0", 0),
        new TnmtCreateDefaultValue("시티투어",         1, "0", "60000",    0, 0, 0, 0, "1", "3",   2, 2, 1, "0", 1, "20", 0, 2, "30000", "2", "60000", "40000", "5", "1000", "1", "0", 0),
        new TnmtCreateDefaultValue("네셔널",           1, "0", "100000",   0, 0, 0, 0, "1", "3",   2, 2, 1, "0", 1, "20", 0, 3, "40000", "2", "100000", "50000", "5", "1000", "1", "0", 0),
        new TnmtCreateDefaultValue("챔피언쉽",         1, "0", "250000",   0, 0, 0, 0, "1", "3",   2, 2, 1, "0", 1, "20", 0, 4, "50000", "2", "250000", "60000", "5", "1000", "1", "0", 0),
        new TnmtCreateDefaultValue("시드권세틀",       1, "0", "10000",    0, 0, 1, 1, "1", "100", 2, 2, 1, "0", 4, "20", 0, 1, "15000", "2", "60000", "40000", "5", "9", "1", "30000", 0),
        new TnmtCreateDefaultValue("시드권세틀2",      1, "0", "15000",    0, 0, 1, 2, "1", "50",  2, 2, 1, "0", 5, "20", 0, 1, "20000", "2", "15000", "25000", "5", "9", "1", "40000", 0),
        new TnmtCreateDefaultValue("프리롤",           1, "0", "0",        0, 0, 1, 1, "1", "100", 0, 0, 0, "0", 6, "20", 0, 1, "10000", "0", "0", "0", "5", "1000", "30", "0", 0),
    };


    [SerializeField]
    private Dropdown defaultSettingDropdown;
    [SerializeField]
    private InputField titleText;
    [SerializeField]
    private Dropdown anteTypeDropdown;
    [SerializeField]
    private InputField openTimeText;
    [SerializeField]
    private InputField startTimeText;
    [SerializeField]
    private InputField closeTimeText;
    [SerializeField]
    private InputField closeBlindupLevelText;
    [SerializeField]
    private InputField buyinText;
    [SerializeField]
    private Dropdown buyinPercentDropdown;
    [SerializeField]
    private Dropdown ticketConditionDropdown;
    [SerializeField]
    private Dropdown ticketTypeDropdown;
    [SerializeField]
    private Dropdown ticketCountDropdown;
    [SerializeField]
    private InputField buyinScaleMinText;
    [SerializeField]
    private InputField buyinScaleMaxText;
    [SerializeField]
    private Dropdown earlyBirdOpenPercentDropdown;
    [SerializeField]
    private Dropdown beforeLevelDropdown;
    [SerializeField]
    private Dropdown beforeLevelPercentDropdown;
    [SerializeField]
    private InputField rewardAllChipText;
    [SerializeField]
    private Dropdown rewardRankTableDropdown;
    [SerializeField]
    private InputField bettingTimeText;
    [SerializeField]
    private Dropdown blindupStructureDropdown;
    [SerializeField]
    private Dropdown blindupDurationDropdown;
    [SerializeField]
    private InputField startingChipText;
    [SerializeField]
    private InputField reentryCountText;
    [SerializeField]
    private InputField reentryCostText;
    [SerializeField]
    private InputField reentryChipText;
    [SerializeField]
    private InputField minPlayerText;
    [SerializeField]
    private InputField maxPlayerText;
    [SerializeField]
    private GameObject quitPlayerCountObj;
    [SerializeField]
    private InputField quitPlayerCountText;
    [SerializeField]
    private InputField jackpotText;
    [SerializeField]
    private Dropdown tableColorDropdown;

    [SerializeField]
    private Dropdown tagDropdown;
    [SerializeField]
    private Toggle passwordToggle;
    [SerializeField]
    private InputField passwordInput;

    [Header("LoungeOption")]
    [SerializeField]
    private List<Variation> loungeVariations;


    private int cafeIdx;

    private void Awake()
    {
        Init();
        InitValues();
    }

    private void OnEnable()
    {
        InitValues();
    }
    
    private void Init()
    {
        InitDropdown(defaultSettingDropdown, defaultSettingOptions);
        InitDropdown(anteTypeDropdown, anteTypeOptions);
        InitDropdown(buyinPercentDropdown, buyinPercentOptions);
        InitDropdown(ticketConditionDropdown, ticketConditionOptions);
        InitDropdown(ticketTypeDropdown, ticketTypeOptions);
        InitDropdown(ticketCountDropdown, ticketCountOptions);
        InitDropdown(earlyBirdOpenPercentDropdown, earlyBirdOpenPercentOptions);
        InitDropdown(beforeLevelDropdown, beforeLevelOptions);
        InitDropdown(beforeLevelPercentDropdown, beforeLevelPercentOptions);
        InitDropdown(rewardRankTableDropdown, rewardRankTableOptions);
        InitDropdown(blindupStructureDropdown, blindupStructureOptions);
        InitDropdown(blindupDurationDropdown, blindupDurationOptions);
        InitDropdown(tableColorDropdown, tableColorOptions);
        InitDropdown(tagDropdown, tagOptions);
        passwordInput.characterLimit = 4;
    }

    private void InitDropdown(Dropdown dropdown, string[] options)
    {
        dropdown.options.Clear();
        for (int i = 0; i < options.Length; i++)
        {
            Dropdown.OptionData newData = new Dropdown.OptionData();
            newData.text = options[i];
            dropdown.options.Add(newData);
        }
    }

    public void InitValues()
    {
        cafeIdx = (int)Cafe.instance.curEnterCafeInfo["cafe"]["idx"];
        
        defaultSettingDropdown.value = 0;
        var is_lounge = cafeIdx == 2;
        loungeVariations.ForEach((variation) => {
            variation.SetVariation(is_lounge.ToString().ToLower());
        });


        SetDefaultValue(0);
    }

    public void OnChangedDefaultSetting()
    {
        SetDefaultValue(defaultSettingDropdown.value);
    }
    public void OnChangeTimeText(InputField input)
    {
        input.text = DateTimeParser.Parse(input.text).ToString("yyyy-MM-dd HH:mm:00");
    }

    private void SetDefaultValue(int index)
    {
        TnmtCreateDefaultValue defaultValue = tnmtCreateDefaultValues[index];
        if (defaultValue == null)
        {
            defaultValue = tnmtCreateDefaultValues[0];
        }

        DateTime openTime = DateTime.Now + TimeSpan.FromMinutes(1);
        DateTime startTime = openTime.AddMinutes(5);
        DateTime closeTime = openTime.AddHours(3);
        openTimeText.text = openTime.ToString("yyyy-MM-dd HH:mm:00");
        startTimeText.text = startTime.ToString("yyyy-MM-dd HH:mm:00");
        closeTimeText.text = closeTime.ToString("yyyy-MM-dd HH:mm:00");
        
        titleText.text = defaultValue.Title;
        anteTypeDropdown.value = defaultValue.AnteType;
        closeBlindupLevelText.text = defaultValue.CloseBlindupLevel;
        buyinText.text = defaultValue.Buyin;
        buyinPercentDropdown.value = defaultValue.BuyinPercent;
        ticketConditionDropdown.value = defaultValue.TicketCondition;
        ticketTypeDropdown.value = defaultValue.TicketType;
        ticketCountDropdown.value = defaultValue.TicketCount;
        buyinScaleMinText.text = defaultValue.BuyinScaleMin;
        buyinScaleMaxText.text = defaultValue.BuyinScaleMax;
        earlyBirdOpenPercentDropdown.value = defaultValue.EarlyBirdOpenPercent;
        beforeLevelDropdown.value = defaultValue.BeforeLevel;
        beforeLevelPercentDropdown.value = defaultValue.BeforeLevelPercent;
        rewardAllChipText.text = defaultValue.RewardAllChip;
        rewardRankTableDropdown.value = defaultValue.RewardRankTable;
        bettingTimeText.text = defaultValue.BettingTime;
        blindupStructureDropdown.value = defaultValue.BlindupStructure;
        blindupDurationDropdown.value = defaultValue.BlindupDuration;
        startingChipText.text = defaultValue.StartingChip;
        reentryCountText.text = defaultValue.ReentryCount;
        reentryCostText.text = defaultValue.ReentryCost;
        reentryChipText.text = defaultValue.ReentryChip;
        minPlayerText.text = defaultValue.MinPlayer;
        maxPlayerText.text = defaultValue.MaxPlayer;
        quitPlayerCountText.text = defaultValue.QuitPlayerCount;
        jackpotText.text = defaultValue.Jackpot;
        tableColorDropdown.value = defaultValue.TableColor;
        quitPlayerCountText.text = defaultValue.QuitPlayerCount;
        passwordToggle.isOn = false;
        passwordInput.text = string.Empty;
        if(quitPlayerCountObj)
            quitPlayerCountObj.gameObject.SetActive(index == 8);
    }


    public async void OnClickTnmtCreateButton()
    {
        var o = new JObject();
        
        o.Add("make_type",(int)TNMT_MAKE_TYPE.user);
        o.Add("t_type", 1);
        o.Add("t_title", titleText.text);
        o.Add("t_game_type", (int)GAME_TYPE.nlh);

        DateTime openTime = DateTimeParser.Parse(openTimeText.text).ToUniversalTime();
        DateTime startTime = DateTimeParser.Parse(startTimeText.text).ToUniversalTime();
        DateTime closeTime = DateTimeParser.Parse(closeTimeText.text).ToUniversalTime();
        DateTime now = DateTime.UtcNow;
        if (openTime > startTime)// && passwordInput.text.Length <= 6)
        {
            ErrorMessageManager.Instance.AddGameError(0, "시간 설정 오류", "등록 시간보다 시작시간이 과거입니다.");
            return;
        }
        else if(openTime > closeTime)
        {
            ErrorMessageManager.Instance.AddGameError(0, "시간 설정 오류", "등록 시간보다 등록 마감 시간이 과거입니다.");
            return;
        }
        else if(now > closeTime)
        {
            ErrorMessageManager.Instance.AddGameError(0, "시간 설정 오류", "현재 시간보다 등록 마감 시간이 과거입니다.");
            return;
        }
        if (now > startTime)
        {
            ErrorMessageManager.Instance.AddGameError(0, "시간 설정 오류", "시작 시간이 현재 시간보다 과거입니다.");
            return;
        }
       

        o.Add("t_open_time", openTime.ToString("u"));
        o.Add("t_start_time",startTime.ToString("u"));
        o.Add("t_close_time", closeTime.ToString("u"));
        o.Add("t_chip_type", (int)CHIP_TYPE.cc);
        
        var buyinPercent = (long)(buyinPercentValues[buyinPercentDropdown.value]*100);
        var buyin = long.Parse(buyinText.text);
        var buyinFee = (buyin * buyinPercent)/100;
        buyin = buyin - buyinFee;
        o.Add("t_buyin", buyin);
        o.Add("t_buyin_fee_percent", buyinPercent);

        o.Add("t_buyin_fee", buyinFee);


        var t_buyin_earlybird = new JObject();
        t_buyin_earlybird.Add("before_level", beforeLevelValues[beforeLevelDropdown.value]);
        t_buyin_earlybird.Add("before_level_percent", beforeLevelPercentValues[beforeLevelPercentDropdown.value]);
        t_buyin_earlybird.Add("ante_type", 1);
        t_buyin_earlybird.Add("only_open_percent",earlyBirdOpenPercentValues[earlyBirdOpenPercentDropdown.value]);
        o.Add("t_buyin_earlybird", t_buyin_earlybird);


        var t_ticket = new JObject();
        t_ticket.Add("ticket_type", ticketTypeDropdown.value);
        t_ticket.Add("ticket_count", ticketCountDropdown.value);
        t_ticket.Add("condition", ticketConditionValues[ticketConditionDropdown.value]);
        o.Add("t_ticket", t_ticket);

        o.Add("t_buyin_scale_min", int.Parse(buyinScaleMinText.text));
        o.Add("t_buyin_scale_max",int.Parse(buyinScaleMaxText.text));
        o.Add("t_action_time", 10);
        o.Add("t_time_bank_sec", 30);
        o.Add("t_starting_chip", long.Parse(startingChipText.text));
        o.Add("t_reward_all_chip", long.Parse(rewardAllChipText.text));

        var t_rewards = new JObject();
        o.Add("t_rewards", t_rewards);

        t_rewards.Add("reward_rank_table",rewardRankTableValues[cafeIdx == 2 ? rewardRankTableDropdown.value : 1]);
        t_rewards.Add("ticket_rake", 0.05);
        t_rewards.Add("quit_player_count", int.Parse(quitPlayerCountText.text));
        t_rewards.Add("jackpot", long.Parse(jackpotText.text));


        o.Add("t_blind_up_structure", 1);// blindupStructureValues[cafeIdx == 2 ? blindupStructureDropdown.value : 2]);
        o.Add("t_blind_up_duration_time",blindupDurationValues[blindupDurationDropdown.value]);
        o.Add("t_reentry_count", int.Parse(reentryCountText.text));
        o.Add("t_reentry_cost", long.Parse(reentryCostText.text));
        o.Add("t_reentry_chip", long.Parse(reentryChipText.text));
        o.Add("t_rank_type", 1);// 사용하지 않음
        o.Add("t_is_hide_nickname", false);
        o.Add("t_is_allow_chat", false);
        o.Add("t_is_allow_observer", false);
        o.Add("t_close_blind_up_level", int.Parse(closeBlindupLevelText.text));
        o.Add("t_min_player",int.Parse(minPlayerText.text));
        o.Add("t_max_player", int.Parse(maxPlayerText.text));
        
        o.Add("t_tag", tagDropdown.value);
        o.Add("t_event", defaultSettingDropdown.value + 1);

        var t_o = new JObject();
        t_o.Add("is_password", passwordToggle.isOn);
        t_o.Add("table_color", tableColorDropdown.value);
        t_o.Add("tag", 0);
        o.Add("t_o", t_o);

        if (passwordToggle.isOn)
        {
            if (passwordInput.text.Length == 4)// && passwordInput.text.Length <= 6)
            {
                o.Add("t_password", passwordInput.text);
            }
            else
            {
                ErrorMessageManager.Instance.AddGameError(0, "비밀번호 설정 오류", "비밀번호를 4자리로 입력해 주세요");
                return;
            }
        }
        else
        {
            o.Add("t_password", "");
        }


        var p = new Packet(CPProtocol.CP_TNMT_CREATE);
        p.Add("cafeIdx", cafeIdx);

        
        p.Add("o", o);

        WebSocketManager.defaultCli.Send(p);
        WaitForPCProtocol wait = new WaitForPCProtocol(PCProtocol.PC_TNMT_CREATE);
        await wait;
        if (wait.Result.c.ValueOrDefault("ecode", 0) == 0)
            NormalMessage.instance.OnOneButtonMessagePopUp("table_success");
        else
            NormalMessage.instance.OnOneButtonMessagePopUp("table_fail");

        gameObject.SetActive(false);
    }
}
