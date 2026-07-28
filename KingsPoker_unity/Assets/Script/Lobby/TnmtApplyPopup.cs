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
    private Variation[] kpVariations; // KP 토글 상태 연출 (미연결 시 무시)

    [SerializeField]
    private Text myKpText; // 보유 KP 표시 (미연결 시 무시)

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
    private LocalText totalBuyinChipText;

    [SerializeField]
    private LocalText totalBuyinTicketText;

    [SerializeField]
    private LocalText totalBuyinKpText; // KP 결제 시 총 KP 표시 (미연결 시 무시)

    [SerializeField]
    private Toggle ticketToggle;

    [SerializeField]
    private Toggle chipToggle;

    [SerializeField]
    private Toggle kpToggle; // 칩 대신 KP(1:1)로 바이인 — 2번카페 전용 (미연결 시 무시)

    [SerializeField]
    private GameObject passwordObj;

    [SerializeField]
    private InputField passwordInput;

    private int buyinScale;
    private long entryCost;
    private int entryTicketCount;
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
        var isChipTnmt = buyinChip > 0;

        buyinTicketObj.SetActive(isTicketTnmt);
        buyinChipObj.SetActive(isChipTnmt);

        chipToggle.isOn = isChipTnmt;
        chipToggle.interactable = !condition.Equals("and");

        ticketToggle.isOn = condition.Equals("and") && isTicketTnmt;
        ticketToggle.interactable = !condition.Equals("and");

        if (kpToggle != null)
        {
            // KP 바이인은 킹스라운지(2번카페) + 칩 바이인 토너먼트에서만 노출
            kpToggle.gameObject.SetActive(isLounge && isChipTnmt);
            kpToggle.SetIsOnWithoutNotify(false);
        }
        if (myKpText != null)
        {
            var myKp = MyStatus.loungeData != null ? MyStatus.loungeData.ValueOrDefault<long>("newKp", 0) : 0;
            myKpText.text = MoneyToString.Converting(myKp);
        }
        SetKpVariations();

        if (isTicketTnmt)
        {
            var ticketTypeString = await KingshillInfo.GetTicketString(ticket_type);
            ticketTypeText.SetLocalText(ticketTypeString);
            ticketCountText.text = MoneyToString.Converting(ticket_count);
        }

        if (isChipTnmt)
        {
            if (didEntry)
            {
                buyinChipText.text = MoneyToString.Converting(t_reentry_cost);
            }
            else
            {
                buyinChipText.text = MoneyToString.Converting(buyinChip);
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

        entryCost = didEntry ? t_reentry_cost : t_buyin + t_buyin_fee;
        var chipMaxScale = entryCost > 0 ? Mathf.FloorToInt(chip / entryCost) : t_buyin_scale_max;
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
        SetKpVariations();
    }

    private void SetKpVariations()
    {
        if (kpVariations == null)
        {
            return;
        }
        var isKpOn = kpToggle != null && kpToggle.isOn;
        foreach (var variation in kpVariations)
        {
            variation.SetVariation(isKpOn.ToString());
        }
    }

    public void OnchangeKpToggle(bool value)
    {
        // KP는 "칩 결제분을 KP(1:1)로 대신 지불" 스위치 — 칩/티켓 토글 상태는 건드리지 않는다
        SetKpVariations();
        SetBuyinScale(buyinScale);
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
        var chip = chipToggle.isOn ? entryCost * buyinScale : 0;
        var ticket = ticketToggle.isOn ? entryTicketCount * buyinScale : 0;
        totalBuyinChipText.SetLocalText("chip_count_text", chip);
        totalBuyinTicketText.SetLocalText("ticket_counting_text", ticket);
        if (totalBuyinKpText != null)
        {
            // KP 토글이 켜져 있으면 칩 결제분을 KP(1:1)로 지불
            var isKpOn = kpToggle != null && kpToggle.gameObject.activeSelf && kpToggle.isOn;
            var kp = isKpOn && (condition.Equals("and") || chipToggle.isOn) ? entryCost * buyinScale : 0;
            totalBuyinKpText.SetLocalText("kp_count_text", kp);
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

        if (kpToggle != null && kpToggle.gameObject.activeSelf && kpToggle.isOn)
        {
            // KP 바이인 사전 잔액 체크 (칩 결제분과 동일 금액, 1:1)
            var needKp = (condition.Equals("and") || chipToggle.isOn) ? entryCost * buyinScale : 0;
            var hasKp = MyStatus.loungeData != null ? MyStatus.loungeData.ValueOrDefault<long>("newKp", 0) : 0;
            if (needKp > 0 && hasKp < needKp)
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
        if (kpToggle != null && kpToggle.gameObject.activeSelf && kpToggle.isOn)
        {
            p.Add("useKp", 1); // 칩 대신 KP(1:1)로 바이인
        }
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
