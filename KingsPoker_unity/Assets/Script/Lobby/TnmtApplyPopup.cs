using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 토너먼트 입장(신청) 팝업.
///
/// 결제 방법은 서버가 주는 바이인 옵션 목록(buyin_options / reentry_options)으로 표시한다 — 옵션 한 줄씩, 택1.
/// 옵션 배열이 없는 구버전 서버라도 BuyinOption.Resolve 가 기존 필드로 합성하므로 경로는 하나다.
///
/// 보유 블록은 템플릿 행(myChipText 의 부모 = "보유 칩" 행, 라벨 + 값) 하나를 필요한 재화 수만큼 복제해 만든다.
/// 프리팹에서는 그 템플릿 행 하나만 관리하면 된다 (칩 → KP → 티켓 종류 순, 없는 재화는 행이 생기지 않음).
/// </summary>
public class TnmtApplyPopup : MonoBehaviour
{
    private int tn;
    private TournamentInfo tnmtInfo;

    [SerializeField]
    private Text tnmtNameText;

    [SerializeField]
    private LocalText entryOrReentryText;

    [Header("보유 — 템플릿 행")]
    [SerializeField]
    private Text myChipText; // "보유 칩" 행의 값 텍스트. 이 텍스트의 부모(라벨 + 값 한 줄)가 보유 행 템플릿이다

    [Header("옵션 (줄 나열)")]
    [SerializeField]
    private TnmtBuyinOptionItem optionItemPrefab; // 옵션 한 줄 프리팹 (컨테이너 안에 두면 템플릿은 자동으로 숨김)

    [SerializeField]
    private Transform optionContainer; // 줄들이 들어갈 부모 (Vertical Layout Group 권장)

    [SerializeField]
    private ToggleGroup optionToggleGroup; // 미연결 시 optionContainer 에서 찾거나 새로 붙인다

    [SerializeField]
    private GameObject optionCaptionObj; // "결제 방법 선택 (택1)" 캡션 — 옵션이 2개 이상일 때만 노출 (미연결 시 생략)

    [Header("배율")]
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

    [Header("사용 (합계)")]
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
    private LocalText totalBuyinKpText;

    [Header("기타")]
    [SerializeField]
    private GameObject passwordObj;

    [SerializeField]
    private InputField passwordInput;

    [SerializeField]
    private CustomUIOpener opener;

    // ── 상태 ──
    private readonly List<BuyinOption> buyinOptions = new List<BuyinOption>();
    private readonly List<TnmtBuyinOptionItem> optionItems = new List<TnmtBuyinOptionItem>();
    private int selectedOptionIndex = 0;
    private int optionPanelVersion = 0; // 비동기 갱신이 겹칠 때 늦게 끝난 쪽이 덮어쓰지 않게

    // 보유 블록 복제본 (재사용 — 팝업을 다시 열어도 늘어나지 않음)
    private readonly List<GameObject> holdRows = new List<GameObject>();
    private readonly List<LocalText> holdRowLabels = new List<LocalText>();
    private readonly List<LocalText> holdRowValues = new List<LocalText>();

    private int buyinScale;
    private int scaleMin;
    private int scaleMax;

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

    // ───────────────────────── 헬퍼 ─────────────────────────

    private BuyinOption CurrentOption =>
        selectedOptionIndex >= 0 && selectedOptionIndex < buyinOptions.Count ? buyinOptions[selectedOptionIndex] : null;

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

    // 행 루트가 연결돼 있으면 행 전체, 아니면 텍스트의 부모가 "라벨 + 값" 묶음(자식 몇 개짜리 Line)일 때 그 줄째,
    // 큰 컨테이너면 텍스트만 토글한다 (라벨만 덩그러니 남는 문제 방지)
    private static void SetRowActive(GameObject rowObj, Component fallbackText, bool active)
    {
        if (rowObj != null)
        {
            rowObj.SetActive(active);
        }
        else if (fallbackText != null)
        {
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

    // ───────────────────────── 패널 구성 ─────────────────────────

    public async void SetTnmtApplyPanel()
    {
        tnmtInfo = InfoManager.Instance.GetTournamentInfo(tn);
        if (optionItemPrefab == null || optionContainer == null)
        {
            Debug.LogError($"[TnmtApplyPopup] tn={tn} optionItemPrefab / optionContainer 가 연결돼 있지 않습니다 — 옵션 줄을 그릴 수 없습니다.");
            return;
        }
        await SetOptionPanel();
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
            $"[TnmtApplyPopup] tn={tn} reentry={didEntry} serverOptions={(info.ContainsKey("buyin_options") ? info["buyin_options"].ToString(Newtonsoft.Json.Formatting.None) : "(none)")} resolved={resolved.Count} ids={string.Join(",", resolved.ConvertAll(o => o.id + (o.fromServer ? "" : "*")))}"
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
        var ticketNames = new List<string>();
        var ticketHaves = new List<long>();
        foreach (var type in ticketTypes)
        {
            ticketNames.Add(await KingshillInfo.GetTicketString(type));
            ticketHaves.Add(type >= 1 && type <= myTickets.Count ? myTickets[type - 1] : 0);
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

        // 내 보유 — 이 토너의 옵션에 등장하는 재화만 (칩 옵션이 없는데 "보유 칩 0" 이 뜨면 칩이 필요한 줄 안다)
        var holdEntries = new List<(string labelKey, string labelParam, string value)>();
        if (anyChip) holdEntries.Add(("hold_chips", null, MoneyToString.Converting(myChip)));
        if (anyKp) holdEntries.Add(("hold_kps", null, MoneyToString.Converting(myKp)));
        for (int i = 0; i < ticketTypes.Count; i++)
        {
            // 라벨 "보유 JOPT": 줄바꿈 불가 공백으로 라벨 폭에서 두 줄로 갈리지 않게 (Best Fit 이 한 줄로 축소)
            holdEntries.Add(("hold_ticket_label", BuyinOption.NBSP + ticketNames[i], ticketHaves[i].ToString()));
        }
        BuildHoldRows(holdEntries);

        // 옵션이 둘 이상일 때만 "결제 방법 선택 (택1)" 캡션
        SetActiveSafe(optionCaptionObj, resolved.Count > 1);

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
        SetMinMax();
        SetBuyinScale(scaleMaxSetting);
    }

    /// <summary>
    /// 보유 블록을 템플릿 행(보유 칩 행)의 복제본으로 구성한다. entries 순서대로 행을 켜고 라벨/값을 채우며
    /// 템플릿 행 자체는 숨긴다. 복제본은 재사용한다.
    /// </summary>
    private void BuildHoldRows(List<(string labelKey, string labelParam, string value)> entries)
    {
        var template = myChipText != null ? myChipText.transform.parent : null;
        if (template == null)
        {
            return;
        }
        var templateObj = template.gameObject;
        var container = template.parent;
        var valueIndex = myChipText.transform.GetSiblingIndex();

        while (holdRows.Count < entries.Count)
        {
            var row = Instantiate(templateObj, container);
            row.name = templateObj.name + " (hold " + (holdRows.Count + 1) + ")";
            row.transform.SetSiblingIndex(template.GetSiblingIndex() + 1 + holdRows.Count); // 템플릿 바로 뒤에 순서대로

            // 값 칸 = 칩 값 텍스트와 같은 자리의 자식, 라벨 = 그 외 첫 Text
            var valueTr = valueIndex < row.transform.childCount ? row.transform.GetChild(valueIndex) : null;
            var valueText = valueTr != null ? valueTr.GetComponent<Text>() : null;
            LocalText valueLocal = null;
            if (valueText != null)
            {
                valueLocal = valueText.GetComponent<LocalText>() ?? valueText.gameObject.AddComponent<LocalText>();
            }
            LocalText labelLocal = null;
            foreach (var txt in row.GetComponentsInChildren<Text>(true))
            {
                if (txt == valueText) continue;
                labelLocal = txt.GetComponent<LocalText>() ?? txt.gameObject.AddComponent<LocalText>();
                break;
            }
            holdRows.Add(row);
            holdRowLabels.Add(labelLocal);
            holdRowValues.Add(valueLocal);
        }

        for (int i = 0; i < holdRows.Count; i++)
        {
            var show = i < entries.Count;
            holdRows[i].SetActive(show);
            if (!show) continue;
            var e = entries[i];
            if (holdRowLabels[i] != null)
            {
                if (e.labelParam != null) holdRowLabels[i].SetLocalText(e.labelKey, e.labelParam);
                else holdRowLabels[i].SetLocalText(e.labelKey);
            }
            // LocalText 는 자기 키로 글자를 다시 쓰므로 .text 직접 대입은 덮어써진다 → "{0}" 키(raw_text)로
            if (holdRowValues[i] != null) holdRowValues[i].SetLocalText("raw_text", e.value);
        }

        templateObj.SetActive(false); // 템플릿은 숨기고 복제본만 보인다
    }

    private void OnOptionSelected(int index)
    {
        if (index == selectedOptionIndex)
        {
            return;
        }
        selectedOptionIndex = index;
        SetMinMax();
        SetBuyinScale(buyinScale);
    }

    // ───────────────────────── 배율 / 합계 ─────────────────────────

    private void SetMinMax()
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

    private void SetBuyinScale(int scale)
    {
        buyinScale = Mathf.Clamp(scale, scaleMin, scaleMax);
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

    public void OnClickBuyinMinButton()
    {
        SetBuyinScale(scaleMin);
    }

    public void OnClickBuyinMaxButton()
    {
        SetBuyinScale(scaleMax);
    }

    // ───────────────────────── 버튼 ─────────────────────────

    public void OnClickCancelButton()
    {
        gameObject.SetActive(false);
        if (opener)
        {
            opener.ShowUI("mtt_nlh_info");
        }
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
        var wait = new WaitForPCProtocol(PCProtocol.PC_TNMT_APPLY, PCProtocol.PC_TNMT_APPLY_FAIL);
        await wait;
        LoadingCircle.Instance.StopSpin();
        if (wait.Result.c.ValueOrDefault("ecode", 0) == 0)
        {
            NormalMessage.instance.OnOneButtonMessagePopUp("confirm_success");
        }
        else
        {
            NormalMessage.instance.OnOneButtonMessagePopUp(wait.Result.c.ValueOrDefault("message", "tnmt_apply_fail"));
        }
        gameObject.SetActive(false);
        LoadingCircle.Instance.LoadingComplete("tnmtApply");
    }
}
