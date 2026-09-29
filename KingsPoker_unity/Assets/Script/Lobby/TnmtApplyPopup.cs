using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.UI;

public class TnmtApplyPopup : MonoBehaviour
{
    private int tn;
    private TournamentInfo tnmtInfo;

    [SerializeField]
    private Text tnmtNameText;

    [SerializeField]
    private LocalText entryOrReentryText;

    [Header("myInfo")]
    [SerializeField]
    private Text myChipText;

    [SerializeField]
    private GameObject myTicketObj;

    [SerializeField]
    private LocalText[] myTicketTexts;

    [SerializeField]
    private GameObject buyinTicketObj;

    [Header("ticket")]
    [SerializeField]
    private Variation[] ticketVariations;

    [SerializeField]
    private LocalText ticketTypeText;

    [SerializeField]
    private Text ticketCountText;

    [Header("chip")]
    [SerializeField]
    private GameObject buyinChipObj;

    [SerializeField]
    private Variation[] chipVariations;

    [SerializeField]
    private Text buyinChipText;

    [Header("kp")]
    [SerializeField]
    private GameObject myKpObj; // 보유 KP 행 — KP 전용 토너에서만 노출 (미연결 시 myKpText 오브젝트 기준)

    [SerializeField]
    private Text myKpText; // 보유 KP 표시 (미연결 시 무시)

    [SerializeField]
    private GameObject buyinKpObj; // KP 바이인 행 — KP 전용 토너에서만 노출 (미연결 시 칩 칸 재활용)

    [SerializeField]
    private LocalText buyinKpText; // KP 바이인 금액 표시 (kp_count_text 키)

    [Header("Scale")]
    [SerializeField]
    private LocalText buyinScaleMinText;

    [SerializeField]
    private LocalText buyinScaleMaxText;

    [SerializeField]
    private LocalText buyinMinButtnText;

    [SerializeField]
    private LocalText buyinMaxButtnText;

    [SerializeField]
    private InputField buyinScaleInput;

    [Header("Total")]
    [SerializeField]
    private GameObject totalBuyinChipObj; // 사용 칩 행 루트 (미연결 시 텍스트 오브젝트 기준)

    [SerializeField]
    private LocalText totalBuyinChipText;

    [SerializeField]
    private GameObject totalBuyinTicketObj; // 사용 티켓 행 루트 (미연결 시 텍스트 오브젝트 기준)

    [SerializeField]
    private LocalText totalBuyinTicketText;

    [SerializeField]
    private GameObject totalBuyinKpObj; // 사용 KP 행 루트 (미연결 시 텍스트 오브젝트 기준)

    [SerializeField]
    private LocalText totalBuyinKpText; // KP 결제 시 총 KP 표시 (미연결 시 무시)

    [SerializeField]
    private Toggle ticketToggle;

    [SerializeField]
    private Toggle chipToggle;

    [Header("options")]
    [SerializeField]
    private Dropdown buyinOptionDropdown; // 바이인 옵션 선택. 연결하면 옵션 모드(서버 buyin_options 기준), 미연결이면 기존 칩/티켓 토글 방식

    [SerializeField]
    private Text buyinOptionText; // 선택한 옵션 요약 (미연결 시 칩 칸 재활용)

    private List<BuyinOption> buyinOptions = new List<BuyinOption>();
    private int selectedOptionIndex = 0;
    private bool optionMode = false;
    private bool optionListenerAdded = false;

    [SerializeField]
    private GameObject passwordObj;

    [SerializeField]
    private InputField passwordInput;

    private int buyinScale;
    private long entryCost;
    private int entryTicketCount;
    private long kpOnlyBuyin; // KP 전용 바이인 토너 (t_o.buyin_kp) — 0이면 일반 토너
    private long kpOnlyReentry; // KP 전용 토너 리엔트리 비용 (t_o.reentry_kp, 미설정 시 buyin_kp)
    private string condition;

    [SerializeField]
    private CustomUIOpener opener;

    private bool isInit = false;

    private void OnEnable()
    {
        InfoManager.Instance.TnmtHistoryChangeAddListener(SetTnmtApplyPanel);
    }

    private void OnDisable()
    {
        InfoManager.Instance.TnmtHistoryChangeRemoveListener(SetTnmtApplyPanel);
    }

    private void Awake()
    {
        if (buyinOptionDropdown != null && !optionListenerAdded)
        {
            buyinOptionDropdown.onValueChanged.AddListener(OnOptionChanged);
            optionListenerAdded = true;
        }
        SetTnmtApplyPanel();
        isInit = true;
    }

    private BuyinOption CurrentOption =>
        optionMode && selectedOptionIndex >= 0 && selectedOptionIndex < buyinOptions.Count ? buyinOptions[selectedOptionIndex] : null;

    private static List<long> MyTicketCounts()
    {
        var data = MyStatus.loungeData;
        var counts = new List<long>();
        counts.Add(data.ValueOrDefault<long>("ticket", 0));
        counts.Add(data.ValueOrDefault<long>("ticket2", 0));
        counts.Add(data.ValueOrDefault<long>("ticket3", 0));
        counts.Add(data.ValueOrDefault<long>("ticket4", 0));
        counts.Add(data.ValueOrDefault<long>("ticket5", 0));
        return counts;
    }

    private static long MyKp()
    {
        return MyStatus.loungeData != null ? MyStatus.loungeData.ValueOrDefault<long>("newKp", 0) : 0;
    }

    // 옵션 모드: 서버가 준 buyin_options / reentry_options 중 하나를 골라 option id + scale 로 신청한다.
    private async UniTask SetupOptionMode(bool didEntry, int t_buyin_scale_min, int t_buyin_scale_max)
    {
        var labels = new List<string>();
        foreach (var opt in buyinOptions)
        {
            labels.Add(await opt.Label());
        }
        buyinOptionDropdown.ClearOptions();
        buyinOptionDropdown.AddOptions(labels);
        if (selectedOptionIndex >= buyinOptions.Count)
        {
            selectedOptionIndex = 0;
        }
        buyinOptionDropdown.SetValueWithoutNotify(selectedOptionIndex);
        buyinOptionDropdown.gameObject.SetActive(true);

        // 기존 토글은 옵션 모드에선 쓰지 않는다 (배율 계산은 칩 토글 경로)
        chipToggle.SetIsOnWithoutNotify(true);
        chipToggle.interactable = false;
        ticketToggle.SetIsOnWithoutNotify(false);
        ticketToggle.interactable = false;

        RefreshOptionRows(didEntry);

        buyinScaleMinText.SetLocalText("buyin_scale_min", t_buyin_scale_min);
        buyinScaleMaxText.SetLocalText("buyin_scale_max", t_buyin_scale_max);
        SetMinMaxText();
        SetVariations();
        SetBuyinScale(t_buyin_scale_max);
    }

    private void RefreshOptionRows(bool didEntry)
    {
        var opt = CurrentOption;
        if (opt == null)
        {
            return;
        }
        var label = opt.LabelCached();
        if (buyinOptionText != null)
        {
            buyinOptionText.text = label;
        }
        // 행 표시: 옵션에 들어 있는 재화만
        buyinTicketObj.SetActive(opt.HasTicket);
        var hasKpRow = buyinKpObj != null;
        buyinChipObj.SetActive(opt.HasChip || (opt.HasKp && !hasKpRow) || (buyinOptionText == null));
        if (hasKpRow)
        {
            buyinKpObj.SetActive(opt.HasKp);
        }
        if (buyinOptionText == null)
        {
            buyinChipText.text = label; // 요약 텍스트 미연결 시 칩 칸에 요약
        }
        else if (opt.HasChip)
        {
            buyinChipText.text = $"{MoneyToString.Converting(opt.chip)}칩";
        }
        if (opt.HasKp && hasKpRow && buyinKpText != null)
        {
            buyinKpText.SetLocalText("kp_count_text", MoneyToString.Converting(opt.kp));
        }
        if (opt.HasTicket)
        {
            var t = opt.tickets[0];
            ticketTypeText.SetLocalText(KingshillInfo.GetTicketStringCached(t.type));
            ticketCountText.text = MoneyToString.Converting(t.count);
        }
        SetRowActive(myKpObj, myKpText, opt.HasKp);
        if (myKpText != null && opt.HasKp)
        {
            myKpText.text = MoneyToString.Converting(MyKp());
        }
        entryOrReentryText.SetLocalText(didEntry ? "reentry" : "entry");
    }

    public void OnOptionChanged(int index)
    {
        if (!optionMode)
        {
            return;
        }
        selectedOptionIndex = index;
        RefreshOptionRows(IsDidEntry());
        SetMinMaxText();
        SetVariations();
        SetBuyinScale(buyinScale);
    }

    private bool IsDidEntry()
    {
        foreach (JObject his in InfoManager.TnmtHistory)
        {
            if (tnmtInfo != null && tnmtInfo.tn == his.ValueOrDefault("tn", 0))
            {
                return true;
            }
        }
        return false;
    }

    public void SetTnmtData(int tn)
    {
        this.tn = tn;
        if (isInit)
        {
            SetTnmtApplyPanel();
            passwordInput.text = string.Empty;
        }

        gameObject.SetActive(true);
    }

    public void OnClickCancelButton()
    {
        gameObject.SetActive(false);
        if (opener)
        {
            opener.ShowUI("mtt_nlh_info");
        }
    }

    public async void SetTnmtApplyPanel()
    {
        tnmtInfo = InfoManager.Instance.GetTournamentInfo(tn);
        var data = MyStatus.loungeData;

        var ticketCounts = new List<long>();
        ticketCounts.Add(data.ValueOrDefault<long>("ticket", 0));
        ticketCounts.Add(data.ValueOrDefault<long>("ticket2", 0));
        ticketCounts.Add(data.ValueOrDefault<long>("ticket3", 0));
        var chipType = tnmtInfo.info.ValueOrDefault<CHIP_TYPE>("t_chip_type", CHIP_TYPE.nothing);
        long chip = GetChip(chipType);

        var live = tnmtInfo.info.CastOrEmpty<JObject>("live");

        var didEntry = false; // = live.ContainsKey("my");
        var reentryCount = 0;
        //var my = live.CastOrEmpty<JObject>("my");
        //if (didEntry)
        //{
        //    reentryCount = my.ValueOrDefault("reentryCount", 0);
        //}

        var historys = InfoManager.TnmtHistory;
        JObject history = null;
        foreach (JObject his in historys)
        {
            if (tnmtInfo.tn == his.ValueOrDefault("tn", 0))
            {
                didEntry = true;
                history = his;
                break;
            }
        }
        if (history != null)
        {
            reentryCount = history.ValueOrDefault("reentryCount", 0);
        }

        var t_ticket = tnmtInfo.info.CastOrEmpty<JObject>("t_ticket");
        var ticket_type = t_ticket.ValueOrDefault("ticket_type", 0);
        var ticket_count = t_ticket.ValueOrDefault("ticket_count", 0);
        condition = t_ticket.ValueOrDefault("condition", "and");

        var t_buyin = tnmtInfo.info.ValueOrDefault("t_buyin", 0);
        var t_buyin_fee = tnmtInfo.info.ValueOrDefault("t_buyin_fee", 0);

        var t_reentry_cost = tnmtInfo.info.ValueOrDefault("t_reentry_cost", 0);

        var t_buyin_scale_min = tnmtInfo.info.ValueOrDefault("t_buyin_scale_min", 1);
        var t_buyin_scale_max = tnmtInfo.info.ValueOrDefault("t_buyin_scale_max", 1);

        var buyinChip = t_buyin + t_buyin_fee;

        var t_o = tnmtInfo.info.CastOrEmpty<JObject>("t_o");
        var is_password = t_o.ValueOrDefault("is_password", false);
        // KP 전용 바이인 토너 — 칩 대신 KP를 buyin_kp 만큼 차감 (서버 강제, 리엔트리는 reentry_kp)
        kpOnlyBuyin = t_o.ValueOrDefault<long>("buyin_kp", 0);
        kpOnlyReentry = t_o.ValueOrDefault<long>("reentry_kp", 0);
        if (kpOnlyReentry <= 0)
        {
            kpOnlyReentry = kpOnlyBuyin;
        }
        var isKpOnlyTnmt = kpOnlyBuyin > 0;

        passwordObj.SetActive(is_password);

        tnmtNameText.text = $"# {tnmtInfo.tn} {tnmtInfo.title}";

        entryOrReentryText.SetLocalText(didEntry ? "reentry" : "entry");
        myChipText.text = MoneyToString.Converting(chip);
        var cafeIdx = data.ValueOrDefault("cafeIdx", 0); //(int)Cafe.instance.curEnterCafeInfo["cafe"]["idx"];
        var isLounge = cafeIdx == 2;
        myTicketObj.SetActive(isLounge);
        if (isLounge)
        {
            for (int i = 0; i < myTicketTexts.Length; ++i)
            {
                myTicketTexts[i].SetLocalText($"ticket{i + 1}_count", ticketCounts[i]);
            }
        }
        // 옵션 모드: 드롭다운이 연결돼 있고 서버가 옵션 목록을 줬을 때. 리엔트리면 reentry_options(없으면 buyin_options)
        buyinOptions = BuyinOption.Parse(tnmtInfo.info, didEntry ? "reentry_options" : "buyin_options");
        if (didEntry && buyinOptions.Count == 0)
        {
            buyinOptions = BuyinOption.Parse(tnmtInfo.info, "buyin_options");
        }
        optionMode = buyinOptionDropdown != null && buyinOptions.Count > 0;
        if (buyinOptionDropdown != null)
        {
            buyinOptionDropdown.gameObject.SetActive(optionMode);
        }
        if (optionMode)
        {
            await SetupOptionMode(didEntry, t_buyin_scale_min, t_buyin_scale_max);
            return;
        }

        var isTicketTnmt = ticket_count > 0;
        var isChipTnmt = buyinChip > 0 && !isKpOnlyTnmt;
        var hasKpRow = buyinKpObj != null; // KP 행이 프리팹에 있으면 그 행 사용, 없으면 칩 칸 재활용

        buyinTicketObj.SetActive(isTicketTnmt);
        buyinChipObj.SetActive(isChipTnmt || (isKpOnlyTnmt && !hasKpRow));
        if (hasKpRow)
        {
            buyinKpObj.SetActive(isKpOnlyTnmt);
        }

        chipToggle.isOn = isChipTnmt || isKpOnlyTnmt; // KP 전용도 배율 계산은 칩 토글 경로 사용
        chipToggle.interactable = !condition.Equals("and");

        ticketToggle.isOn = condition.Equals("and") && isTicketTnmt;
        ticketToggle.interactable = !condition.Equals("and");

        // 보유 KP 는 KP 전용 토너에서만 표시
        SetRowActive(myKpObj, myKpText, isKpOnlyTnmt);
        if (myKpText != null && isKpOnlyTnmt)
        {
            var myKp = MyStatus.loungeData != null ? MyStatus.loungeData.ValueOrDefault<long>("newKp", 0) : 0;
            myKpText.text = MoneyToString.Converting(myKp);
        }

        if (isTicketTnmt)
        {
            var ticketTypeString = await KingshillInfo.GetTicketString(ticket_type);
            ticketTypeText.SetLocalText(ticketTypeString);
            ticketCountText.text = MoneyToString.Converting(ticket_count);
        }

        if (isKpOnlyTnmt)
        {
            var kpAmount = MoneyToString.Converting(didEntry ? kpOnlyReentry : kpOnlyBuyin);
            if (hasKpRow)
            {
                if (buyinKpText != null)
                {
                    buyinKpText.SetLocalText("kp_count_text", kpAmount);
                }
            }
            else
            {
                buyinChipText.text = $"{kpAmount}KP"; // KP 행 미연결 시 칩 칸 재활용
            }
        }
        else if (isChipTnmt)
        {
            if (didEntry)
            {
                buyinChipText.text = $"{MoneyToString.Converting(t_reentry_cost)}칩";
            }
            else
            {
                buyinChipText.text = $"{MoneyToString.Converting(buyinChip)}칩";
            }
        }

        //if(didEntry)
        //{
        //    t_buyin_scale_max = 1;
        //    t_buyin_scale_min = 1;
        //}

        buyinScaleMinText.SetLocalText("buyin_scale_min", t_buyin_scale_min);
        buyinScaleMaxText.SetLocalText("buyin_scale_max", t_buyin_scale_max);

        SetMinMaxText();
        SetVariations();
        SetBuyinScale(t_buyin_scale_max);
    }

    private int scaleMin;
    private int scaleMax;

    private void SetMinMaxText()
    {
        var data = MyStatus.loungeData;

        var ticketCounts = new List<long>();

        var live = tnmtInfo.info.CastOrEmpty<JObject>("live");

        var didEntry = live.ContainsKey("my");
        ticketCounts.Add(data.ValueOrDefault<long>("ticket", 0));
        ticketCounts.Add(data.ValueOrDefault<long>("ticket2", 0));
        ticketCounts.Add(data.ValueOrDefault<long>("ticket3", 0));
        ticketCounts.Add(data.ValueOrDefault<long>("ticket4", 0));
        ticketCounts.Add(data.ValueOrDefault<long>("ticket5", 0));
        var chipType = tnmtInfo.info.ValueOrDefault("t_chip_type", CHIP_TYPE.nothing);

        long chip = GetChip(chipType);

        var t_buyin_scale_min = tnmtInfo.info.ValueOrDefault("t_buyin_scale_min", 1);
        var t_buyin_scale_max = tnmtInfo.info.ValueOrDefault("t_buyin_scale_max", 1);
        var historys = InfoManager.TnmtHistory;
        foreach (JObject his in historys)
        {
            if (tnmtInfo.tn == his.ValueOrDefault("tn", 0))
            {
                didEntry = true;
                break;
            }
        }

        var t_buyin = tnmtInfo.info.ValueOrDefault("t_buyin", 0);
        var t_buyin_fee = tnmtInfo.info.ValueOrDefault("t_buyin_fee", 0);

        var t_reentry_cost = tnmtInfo.info.ValueOrDefault("t_reentry_cost", 0);

        var t_ticket = tnmtInfo.info.CastOrEmpty<JObject>("t_ticket");
        var ticket_type = t_ticket.ValueOrDefault("ticket_type", 0);
        entryTicketCount = t_ticket.ValueOrDefault("ticket_count", 0);

        if (optionMode)
        {
            var opt = CurrentOption;
            var affordable = opt != null ? opt.MaxAffordableScale(chip, MyKp(), ticketCounts, t_buyin_scale_max) : t_buyin_scale_max;
            entryCost = opt != null ? opt.chip : 0;
            entryTicketCount = opt != null && opt.HasTicket ? opt.tickets[0].count : 0;
            scaleMin = Mathf.Min(t_buyin_scale_min, affordable);
            scaleMax = Mathf.Min(t_buyin_scale_max, affordable);
            buyinMinButtnText.SetLocalText("buyin_min_button", scaleMin);
            buyinMaxButtnText.SetLocalText("buyin_max_button", scaleMax);
            return;
        }

        entryCost = didEntry ? t_reentry_cost : t_buyin + t_buyin_fee;
        var chipMaxScale = entryCost > 0 ? Mathf.FloorToInt(chip / entryCost) : t_buyin_scale_max;
        if (kpOnlyBuyin > 0)
        {
            // KP 전용 토너 — 비용·배율 한도를 KP 잔액 기준으로 (리엔트리는 reentry_kp)
            entryCost = didEntry ? kpOnlyReentry : kpOnlyBuyin;
            var myKp = MyStatus.loungeData != null ? MyStatus.loungeData.ValueOrDefault<long>("newKp", 0) : 0;
            chipMaxScale = Mathf.FloorToInt(myKp / entryCost);
        }
        var ticketMaxScale = Mathf.FloorToInt(
            (
                ticket_type > 0 && ticket_type < ticketCounts.Count
                    ? ticketCounts[ticket_type - 1] / entryTicketCount
                    : t_buyin_scale_max
            )
        );
        //if (didEntry)
        //{
        //    chipMaxScale = 1;
        //    ticketMaxScale = 1;
        //}
        if (!condition.Equals("and"))
        {
            if (chipToggle.isOn)
            {
                scaleMin = Mathf.Min(t_buyin_scale_min, chipMaxScale);
                scaleMax = Mathf.Min(t_buyin_scale_max, chipMaxScale);
            }

            if (ticketToggle.isOn)
            {
                scaleMin = Mathf.Min(t_buyin_scale_min, ticketMaxScale);
                scaleMax = Mathf.Min(t_buyin_scale_max, ticketMaxScale);
            }
        }
        else
        {
            scaleMin = Mathf.Min(t_buyin_scale_min, chipMaxScale);
            scaleMax = Mathf.Min(t_buyin_scale_max, chipMaxScale);
            scaleMin = Mathf.Min(scaleMin, ticketMaxScale);
            scaleMax = Mathf.Min(scaleMax, ticketMaxScale);
        }

        buyinMinButtnText.SetLocalText("buyin_min_button", scaleMin);
        buyinMaxButtnText.SetLocalText("buyin_max_button", scaleMax);
    }

    public void OnchangeTicketToggle(bool value)
    {
        if (!condition.Equals("and"))
        {
            chipToggle.SetIsOnWithoutNotify(!value);
        }
        SetMinMaxText();
        SetVariations();
        SetBuyinScale(buyinScale);
    }

    public long GetChip(CHIP_TYPE chipType)
    {
        switch (chipType)
        {
            case CHIP_TYPE.cc: //카페 칩
                return (long)Cafe.instance.curEnterCafeInfo["cafeMember"]["cc"];

            case CHIP_TYPE.dc:
                return MyStatus.dc;

            case CHIP_TYPE.zc:
                return MyStatus.zc;
            default:
                return 0;
        }
    }

    public void OnchangeChipToggle(bool value)
    {
        if (!condition.Equals("and"))
        {
            ticketToggle.SetIsOnWithoutNotify(!value);
        }
        SetMinMaxText();
        SetVariations();
        SetBuyinScale(buyinScale);
    }

    private void SetVariations()
    {
        foreach (var variation in chipVariations)
        {
            variation.SetVariation(chipToggle.isOn.ToString());
        }
        foreach (var variation in ticketVariations)
        {
            variation.SetVariation(ticketToggle.isOn.ToString());
        }
    }

    // 행 루트가 연결돼 있으면 행 전체(라벨 포함), 아니면 텍스트 오브젝트만 토글
    private static void SetRowActive(GameObject rowObj, Component fallbackText, bool active)
    {
        if (rowObj != null)
        {
            rowObj.SetActive(active);
        }
        else if (fallbackText != null)
        {
            fallbackText.gameObject.SetActive(active);
        }
    }

    public void BuyinScaleChanged(string str)
    {
        var scale = 0;
        try
        {
            scale = int.Parse(str);
        }
        catch
        {
            Console.Error($"숫자가 아닌 값이 들어 있음 {str}");
        }
        SetBuyinScale(scale);
    }

    private void SetBuyinScale(int scale)
    {
        buyinScale = Mathf.Clamp(scale, scaleMin, scaleMax);
        if (optionMode)
        {
            var opt = CurrentOption;
            var showChip = opt != null && opt.HasChip;
            SetRowActive(totalBuyinChipObj, totalBuyinChipText, showChip);
            if (showChip)
            {
                totalBuyinChipText.SetLocalText("chip_count_text", opt.chip * buyinScale);
            }
            var showTicket = opt != null && opt.HasTicket;
            SetRowActive(totalBuyinTicketObj, totalBuyinTicketText, showTicket);
            if (showTicket)
            {
                totalBuyinTicketText.SetLocalText("ticket_counting_text", opt.tickets[0].count * buyinScale);
            }
            var showKp = opt != null && opt.HasKp;
            SetRowActive(totalBuyinKpObj, totalBuyinKpText, showKp);
            if (totalBuyinKpText != null && showKp)
            {
                totalBuyinKpText.SetLocalText("kp_count_text", opt.kp * buyinScale);
            }
            buyinScaleInput.SetTextWithoutNotify(buyinScale.ToString());
            return;
        }
        var chip = chipToggle.isOn ? entryCost * buyinScale : 0;
        var ticket = ticketToggle.isOn ? entryTicketCount * buyinScale : 0;

        // 토탈 줄은 해당 재화를 쓰는 토너에서만 표시 (행 루트 연결 시 라벨까지 함께 토글)
        var showChipTotal = kpOnlyBuyin <= 0 && entryCost > 0;
        SetRowActive(totalBuyinChipObj, totalBuyinChipText, showChipTotal);
        if (showChipTotal)
        {
            totalBuyinChipText.SetLocalText("chip_count_text", chip);
        }

        var showTicketTotal = entryTicketCount > 0;
        SetRowActive(totalBuyinTicketObj, totalBuyinTicketText, showTicketTotal);
        if (showTicketTotal)
        {
            totalBuyinTicketText.SetLocalText("ticket_counting_text", ticket);
        }

        // 사용 KP 줄은 KP 전용 토너에서만 노출
        SetRowActive(totalBuyinKpObj, totalBuyinKpText, kpOnlyBuyin > 0);
        if (totalBuyinKpText != null && kpOnlyBuyin > 0)
        {
            totalBuyinKpText.SetLocalText("kp_count_text", entryCost * buyinScale);
        }
        buyinScaleInput.SetTextWithoutNotify(buyinScale.ToString());
    }

    public void OnClickBuyinMinButton()
    {
        SetBuyinScale(scaleMin);
    }

    public void OnClickBuyinMaxButton()
    {
        SetBuyinScale(scaleMax);
    }

    public async void OnClickTryApply()
    {
        LoadingCircle.Instance.LoadingStart("tnmtApply");
        Packet p = new Packet(CPProtocol.CP_PLAY_GAME_LEAVE_OBSERVER);

        var room = InfoManager.Instance.GetTourmentRoom(tn);
        if (room != null)
        {
            p.Add("gtn", room.gtn);
            WebSocketManager.defaultCli.Send(p);

            await new WaitForPCProtocol(PCProtocol.PC_PLAY_GAME_LEAVE_OBSERVER);
        }

        if (optionMode)
        {
            var opt = CurrentOption;
            if (opt == null)
            {
                LoadingCircle.Instance.StopSpin();
                return;
            }
            if (opt.HasKp && MyKp() < opt.kp * (long)Mathf.Max(1, buyinScale))
            {
                LoadingCircle.Instance.StopSpin();
                ErrorMessageManager.Instance.AddGameError(0, "KP 부족", "보유한 KP가 부족합니다.", ErrorHandlingType.NONE, null, null);
                return;
            }
            p = new Packet(CPProtocol.CP_TNMT_APPLY);
            p.Add("tn", tn);
            p.Add("option", opt.id); // 서버는 option 이 있으면 그 옵션으로 결제 (scale 키 추론 안 함)
            p.Add("scale", buyinScale);
            if (passwordObj.activeSelf)
            {
                p.Add("password", passwordInput.text);
            }
            WebSocketManager.defaultCli.Send(p);
            var waitOpt = new WaitForPCProtocol(PCProtocol.PC_TNMT_APPLY, PCProtocol.PC_TNMT_APPLY_FAIL);
            await waitOpt;
            LoadingCircle.Instance.StopSpin();
            if (waitOpt.Result.c.ValueOrDefault("ecode", 0) == 0)
            {
                NormalMessage.instance.OnOneButtonMessagePopUp("confirm_success");
            }
            else
            {
                NormalMessage.instance.OnOneButtonMessagePopUp(waitOpt.Result.c.ValueOrDefault("message", "tnmt_apply_fail"));
            }
            gameObject.SetActive(false);
            LoadingCircle.Instance.LoadingComplete("tnmtApply");
            return;
        }

        if (kpOnlyBuyin > 0)
        {
            // KP 전용 토너 — 신청 전 KP 잔액 사전 체크 (entryCost 는 신청/리엔트리에 맞는 KP 비용)
            var needKp = entryCost * (long)Mathf.Max(1, buyinScale);
            var hasKp = MyStatus.loungeData != null ? MyStatus.loungeData.ValueOrDefault<long>("newKp", 0) : 0;
            if (hasKp < needKp)
            {
                LoadingCircle.Instance.StopSpin();
                ErrorMessageManager.Instance.AddGameError(0, "KP 부족", "보유한 KP가 부족합니다.", ErrorHandlingType.NONE, null, null);
                return;
            }
        }

        p = new Packet(CPProtocol.CP_TNMT_APPLY);
        p.Add("tn", tn);

        var scaleString = "scale";
        if (!condition.Equals("and"))
        {
            scaleString = chipToggle.isOn ? "scale_chip" : "scale_ticket";
        }

        p.Add(scaleString, buyinScale);
        if (passwordObj.activeSelf)
        {
            p.Add("password", passwordInput.text);
        }

        //p.Add("double", 0);
        WebSocketManager.defaultCli.Send(p);
        WaitForPCProtocol wait = new WaitForPCProtocol(
            PCProtocol.PC_TNMT_APPLY,
            PCProtocol.PC_TNMT_APPLY_FAIL
        );
        await wait;
        LoadingCircle.Instance.StopSpin();
        if (wait.Result.c.ValueOrDefault("ecode", 0) == 0)
        {
            NormalMessage.instance.OnOneButtonMessagePopUp("confirm_success");
        }
        else
        {
            NormalMessage.instance.OnOneButtonMessagePopUp(
                wait.Result.c.ValueOrDefault("message", "tnmt_apply_fail")
            );
        }
        gameObject.SetActive(false);
        LoadingCircle.Instance.LoadingComplete("tnmtApply");
    }
}
