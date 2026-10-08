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
    private GameObject myChipObj; // 보유 칩 행 루트 — 옵션 모드에서 칩 옵션이 있을 때만 노출 (미연결 시 myChipText 오브젝트 기준)

    [SerializeField]
    private Text myChipText;

    [SerializeField]
    private GameObject myTicketObj;

    [SerializeField]
    private LocalText[] myTicketTexts;

    [SerializeField]
    private GameObject myOptionTicketObj; // 옵션 모드 보유 티켓 행 루트 (미연결 시 myOptionTicketText 오브젝트 기준)

    [SerializeField]
    private Text myOptionTicketText; // 옵션에 쓰이는 티켓만: "JOPT 1장 · 월간티켓 0장" (미연결 시 myTicketObj 로 대체)

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

    // ── 바이인 옵션 줄 나열 방식 ──
    // optionItemPrefab + optionContainer 를 연결하면 옵션 모드. 이 모드에서는 위쪽 필드가
    // 전부 선택 사항이라(없으면 건너뜀) 기존 칩/티켓 토글·행을 프리팹에서 지워도 된다.
    // 둘 중 하나라도 비어 있으면 기존 칩/티켓 토글 방식 그대로 동작한다.
    [Header("options (줄 나열)")]
    [SerializeField]
    private TnmtBuyinOptionItem optionItemPrefab; // 옵션 한 줄 프리팹

    [SerializeField]
    private Transform optionContainer; // 줄들이 들어갈 부모 (Vertical Layout Group 권장)

    [SerializeField]
    private ToggleGroup optionToggleGroup; // 미연결 시 optionContainer 에서 찾거나 새로 붙인다

    [SerializeField]
    private GameObject optionCaptionObj; // "결제 방법 선택 (택1)" 캡션 — 옵션이 2개 이상일 때만 노출 (미연결 시 생략)

    private readonly List<BuyinOption> buyinOptions = new List<BuyinOption>();
    private readonly List<TnmtBuyinOptionItem> optionItems = new List<TnmtBuyinOptionItem>();
    private int selectedOptionIndex = 0;
    private bool optionMode = false;
    private int optionPanelVersion = 0; // 비동기 갱신이 겹칠 때 늦게 끝난 쪽이 덮어쓰지 않게

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
        SetTnmtApplyPanel();
        isInit = true;
    }

    public void SetTnmtData(int tn)
    {
        this.tn = tn;
        if (isInit)
        {
            SetTnmtApplyPanel();
            if (passwordInput != null)
            {
                passwordInput.text = string.Empty;
            }
        }

        gameObject.SetActive(true);
    }

    // ───────────────────────── 옵션 모드 ─────────────────────────

    private BuyinOption CurrentOption =>
        optionMode && selectedOptionIndex >= 0 && selectedOptionIndex < buyinOptions.Count ? buyinOptions[selectedOptionIndex] : null;

    private static void SetTextSafe(Text t, string s)
    {
        if (t != null) t.text = s;
    }

    private static void SetLocalSafe(LocalText t, string key, params object[] par)
    {
        if (t != null) t.SetLocalText(key, par);
    }

    private static void SetActiveSafe(GameObject g, bool active)
    {
        if (g != null) g.SetActive(active);
    }

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

    private long MyChip()
    {
        return GetChip(tnmtInfo.info.ValueOrDefault("t_chip_type", CHIP_TYPE.nothing));
    }

    private bool IsDidEntry()
    {
        if (tnmtInfo == null)
        {
            return false;
        }
        foreach (JObject his in InfoManager.TnmtHistory)
        {
            if (tnmtInfo.tn == his.ValueOrDefault("tn", 0))
            {
                return true;
            }
        }
        return false;
    }

    private async UniTask SetOptionPanel()
    {
        var version = ++optionPanelVersion;
        if (tnmtInfo == null)
        {
            return;
        }
        var info = tnmtInfo.info;
        var didEntry = IsDidEntry();

        // 서버가 준 옵션, 없으면(구버전 서버) 기존 필드로 합성
        var resolved = BuyinOption.Resolve(info, didEntry);
        Debug.Log(
            $"[TnmtApplyPopup] tn={tn} optionMode reentry={didEntry} serverOptions={(info.ContainsKey("buyin_options") ? info["buyin_options"].ToString(Newtonsoft.Json.Formatting.None) : "(none)")} resolved={resolved.Count} ids={string.Join(",", resolved.ConvertAll(o => o.id + (o.fromServer ? "" : "*")))}"
        );

        var t_o = info.CastOrEmpty<JObject>("t_o");
        var scaleMinSetting = info.ValueOrDefault("t_buyin_scale_min", 1);
        var scaleMaxSetting = info.ValueOrDefault("t_buyin_scale_max", 1);
        var myChip = MyChip();
        var myKp = MyKp();
        var myTickets = MyTicketCounts();

        // 줄 문구는 티켓 이름을 받아와야 해서 비동기 — 먼저 다 만든 뒤 한 번에 그린다
        var prices = new List<string>();
        var holds = new List<string>();
        var affordables = new List<bool>();
        var anyChip = false;
        var anyKp = false;
        var ticketTypes = new List<int>(); // 옵션에 쓰이는 티켓 종류 (등장 순)
        foreach (var opt in resolved)
        {
            prices.Add(await opt.Label());
            holds.Add(await opt.HoldLabel(myChip, myKp, myTickets, Mathf.Max(1, scaleMinSetting)));
            affordables.Add(opt.MaxAffordableScale(myChip, myKp, myTickets, scaleMaxSetting) >= Mathf.Max(1, scaleMinSetting));
            anyChip |= opt.HasChip;
            anyKp |= opt.HasKp;
            foreach (var t in opt.tickets)
            {
                if (!ticketTypes.Contains(t.type)) ticketTypes.Add(t.type);
            }
        }
        // 보유 티켓 줄: 옵션에 쓰이는 종류만 "JOPT 1장 · 월간티켓 0장"
        var ticketHoldParts = new List<string>();
        foreach (var type in ticketTypes)
        {
            var have = type >= 1 && type <= myTickets.Count ? myTickets[type - 1] : 0;
            ticketHoldParts.Add($"{await KingshillInfo.GetTicketString(type)}{BuyinOption.NBSP}{have}장");
        }
        if (version != optionPanelVersion || this == null)
        {
            return; // 더 새로운 갱신이 시작됨
        }

        buyinOptions.Clear();
        buyinOptions.AddRange(resolved);

        // 머리말
        SetTextSafe(tnmtNameText, $"# {tnmtInfo.tn} {tnmtInfo.title}");
        SetLocalSafe(entryOrReentryText, didEntry ? "reentry" : "entry");
        SetActiveSafe(passwordObj, t_o.ValueOrDefault("is_password", false));

        // 내 보유 — 이 토너의 옵션에 등장하는 재화만 보여준다 (칩 옵션이 없는데 "보유 칩 0" 이 뜨면 칩이 필요한 줄 안다)
        SetRowActive(myChipObj, myChipText, anyChip);
        if (anyChip)
        {
            SetTextSafe(myChipText, MoneyToString.Converting(myChip));
        }
        SetRowActive(myKpObj, myKpText, anyKp);
        if (anyKp)
        {
            SetTextSafe(myKpText, MoneyToString.Converting(myKp));
        }
        // 보유 티켓: 옵션에 티켓이 쓰이면 보여준다. (예전 조건 loungeData.cafeIdx == 2 는 라운지 유저 정보에
        // 없는 키라 항상 false → 보유 티켓 줄이 한 번도 안 떴던 원인)
        var anyTicket = ticketTypes.Count > 0;
        if (myOptionTicketText != null)
        {
            // 옵션 티켓 전용 줄이 있으면 그걸 쓰고, 종류별 보유 줄(myTicketObj)은 숨긴다
            SetRowActive(myOptionTicketObj, myOptionTicketText, anyTicket);
            if (anyTicket)
            {
                SetTextSafe(myOptionTicketText, string.Join(" · ", ticketHoldParts));
            }
            SetActiveSafe(myTicketObj, false);
        }
        else
        {
            // 기존 종류별 줄(myTicketObj + myTicketTexts)을 재사용: 옵션에 쓰이는 종류만 켜고 실제 티켓 이름으로 표시.
            // 텍스트 칸이 모자라면 첫 칸을 복제한다 (프리팹 수정 없이 동작)
            SetActiveSafe(myTicketObj, anyTicket);
            if (anyTicket && myTicketTexts != null && myTicketTexts.Length > 0)
            {
                var slots = new List<LocalText>(myTicketTexts);
                while (slots.Count < ticketTypes.Count)
                {
                    var clone = Instantiate(slots[0], slots[0].transform.parent);
                    clone.gameObject.name = slots[0].gameObject.name + " (opt)";
                    slots.Add(clone);
                }
                myTicketTexts = slots.ToArray();
                for (int i = 0; i < slots.Count; i++)
                {
                    var used = i < ticketTypes.Count;
                    slots[i].gameObject.SetActive(used);
                    if (used)
                    {
                        // LocalText 는 Start/언어 변경 때 자기 키로 글자를 다시 쓰므로 .text 직접 대입은 덮어써진다
                        // → "{0}" 키(raw_text)로 넣어 유지되게 한다
                        slots[i].SetLocalText("raw_text", ticketHoldParts[i]);
                    }
                }
            }
        }

        // 옵션이 둘 이상일 때만 "결제 방법 선택 (택1)" 캡션
        SetActiveSafe(optionCaptionObj, resolved.Count > 1);

        // 기존 칩/티켓/KP 가격 행과 토글은 옵션 줄이 대신한다 (남아 있으면 숨김)
        SetActiveSafe(buyinChipObj, false);
        SetActiveSafe(buyinTicketObj, false);
        SetActiveSafe(buyinKpObj, false);
        if (chipToggle != null) chipToggle.gameObject.SetActive(false);
        if (ticketToggle != null) ticketToggle.gameObject.SetActive(false);

        // 옵션 줄
        if (optionToggleGroup == null)
        {
            optionToggleGroup = optionContainer.GetComponent<ToggleGroup>();
            if (optionToggleGroup == null)
            {
                optionToggleGroup = optionContainer.gameObject.AddComponent<ToggleGroup>();
            }
        }
        optionToggleGroup.allowSwitchOff = false;

        // 줄 템플릿이 에셋이 아니라 팝업 안에 놓인 오브젝트면(컨테이너 자식 등) 템플릿 자체는 숨긴다.
        // 복제본은 아래에서 SetActive(true) 로 켠다.
        if (optionItemPrefab.gameObject.scene.IsValid() && optionItemPrefab.gameObject.activeSelf)
        {
            optionItemPrefab.gameObject.SetActive(false);
        }

        foreach (var item in optionItems)
        {
            if (item != null)
            {
                Destroy(item.gameObject);
            }
        }
        optionItems.Clear();

        // 기본 선택: 이전 선택을 유지하되 살 수 없으면 살 수 있는 첫 옵션
        if (selectedOptionIndex >= buyinOptions.Count || !affordables[selectedOptionIndex])
        {
            var firstAffordable = affordables.IndexOf(true);
            selectedOptionIndex = firstAffordable >= 0 ? firstAffordable : 0;
        }

        for (int i = 0; i < buyinOptions.Count; i++)
        {
            var index = i;
            var item = Instantiate(optionItemPrefab, optionContainer);
            item.gameObject.SetActive(true);
            item.Set(prices[i], holds[i], affordables[i], optionToggleGroup, () => OnOptionSelected(index));
            optionItems.Add(item);
        }
        for (int i = 0; i < optionItems.Count; i++)
        {
            optionItems[i].SetSelected(i == selectedOptionIndex);
        }

        SetLocalSafe(buyinScaleMinText, "buyin_scale_min", scaleMinSetting);
        SetLocalSafe(buyinScaleMaxText, "buyin_scale_max", scaleMaxSetting);
        SetMinMaxText();
        SetBuyinScale(scaleMaxSetting);
    }

    private void OnOptionSelected(int index)
    {
        if (!optionMode || index == selectedOptionIndex)
        {
            return;
        }
        selectedOptionIndex = index;
        SetMinMaxText();
        SetBuyinScale(buyinScale);
    }

    private void SetMinMaxOption()
    {
        var t_buyin_scale_min = tnmtInfo.info.ValueOrDefault("t_buyin_scale_min", 1);
        var t_buyin_scale_max = tnmtInfo.info.ValueOrDefault("t_buyin_scale_max", 1);
        var opt = CurrentOption;
        var affordable = opt != null ? opt.MaxAffordableScale(MyChip(), MyKp(), MyTicketCounts(), t_buyin_scale_max) : t_buyin_scale_max;
        scaleMin = Mathf.Min(t_buyin_scale_min, affordable);
        scaleMax = Mathf.Min(t_buyin_scale_max, affordable);
        SetLocalSafe(buyinMinButtnText, "buyin_min_button", scaleMin);
        SetLocalSafe(buyinMaxButtnText, "buyin_max_button", scaleMax);
    }

    private void SetTotalsOption()
    {
        var opt = CurrentOption;

        var showChip = opt != null && opt.HasChip;
        SetRowActive(totalBuyinChipObj, totalBuyinChipText, showChip);
        if (showChip)
        {
            SetLocalSafe(totalBuyinChipText, "chip_count_text", opt.chip * buyinScale);
        }

        var showTicket = opt != null && opt.HasTicket;
        SetRowActive(totalBuyinTicketObj, totalBuyinTicketText, showTicket);
        if (showTicket)
        {
            long ticketTotal = 0;
            foreach (var t in opt.tickets)
            {
                ticketTotal += (long)t.count * buyinScale;
            }
            SetLocalSafe(totalBuyinTicketText, "ticket_counting_text", ticketTotal);
        }

        var showKp = opt != null && opt.HasKp;
        SetRowActive(totalBuyinKpObj, totalBuyinKpText, showKp);
        if (showKp)
        {
            SetLocalSafe(totalBuyinKpText, "kp_count_text", opt.kp * buyinScale);
        }

        if (buyinScaleInput != null)
        {
            buyinScaleInput.SetTextWithoutNotify(buyinScale.ToString());
        }
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

        // 옵션 줄 프리팹과 부모가 연결돼 있으면 옵션 모드 (아래 기존 방식은 타지 않는다)
        optionMode = optionItemPrefab != null && optionContainer != null;
        if (!optionMode)
        {
            Debug.Log($"[TnmtApplyPopup] tn={tn} legacyMode (optionItemPrefab={(optionItemPrefab != null)} optionContainer={(optionContainer != null)}) buyin_options in info={(tnmtInfo != null && tnmtInfo.info.ContainsKey("buyin_options"))}");
        }
        if (optionMode)
        {
            await SetOptionPanel();
            return;
        }

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
        if (optionMode)
        {
            SetMinMaxOption();
            return;
        }

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
        if (optionMode)
        {
            return;
        }
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
        if (optionMode)
        {
            return;
        }
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
        if (optionMode)
        {
            return;
        }
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
            // 행 루트가 연결돼 있지 않으면 텍스트의 부모가 "라벨 + 값" 묶음(자식 몇 개짜리 Line)일 때 그 줄째 끄고,
            // 큰 컨테이너면 텍스트만 끈다 (라벨만 덩그러니 남는 문제 방지)
            var parent = fallbackText.transform.parent;
            if (parent != null && parent.childCount <= 4 && parent.GetComponent<TnmtApplyPopup>() == null)
            {
                parent.gameObject.SetActive(active);
            }
            else
            {
                fallbackText.gameObject.SetActive(active);
            }
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
            SetTotalsOption();
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
            var applyScale = Mathf.Max(1, buyinScale);
            if (opt.HasKp && MyKp() < opt.kp * (long)applyScale)
            {
                LoadingCircle.Instance.StopSpin();
                ErrorMessageManager.Instance.AddGameError(0, "KP 부족", "보유한 KP가 부족합니다.", ErrorHandlingType.NONE, null, null);
                return;
            }

            p = new Packet(CPProtocol.CP_TNMT_APPLY);
            p.Add("tn", tn);
            if (opt.fromServer)
            {
                // 서버가 준 옵션 — id 로 지정 (서버는 scale 키 추론을 하지 않는다)
                p.Add("option", opt.id);
                p.Add("scale", applyScale);
            }
            else
            {
                // 구버전 서버 — 예전 방식 키 (scale / scale_chip / scale_ticket)
                p.Add(opt.legacyScaleKey, applyScale);
            }
            if (passwordObj != null && passwordObj.activeSelf && passwordInput != null)
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
