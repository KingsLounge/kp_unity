using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.UI;

public class TnmtItem : MonoBehaviour
{
    public PlayGameItemGuage gauge;
    public TournamentInfo tnmtInfo;

    public Text txt_game_type;
    public Text txt_tn;
    public LocalText txt_tnmt_type;
    public Text txt_tnmt_title;
    public GameObject obj_cafe_stop;
    public Text txt_buyin;
    public LocalText txt_time;
    public Text txt_prize;
    public Text txt_cafeMember_dc;

    public Image img_cafeMember_dc;

    public Text txt_cafeIdx;
    public Image chip_icon;

    private DateTime startTime;
    private DateTime openTime;
    private DateTime closeTime;

    public GameObject date;
    public Text month;
    public Text week;
    public Text day;

    [Space]
    public GameObject titleContents;
    public Text title;
    public LocalText titleLocal;
    public Text contents;
    public Image time_panel;

    [Space, Header("Time Panel Sprites")]
    public Sprite registering;
    public Sprite running;
    public Sprite latereg;

    public List<Variation> permitVariations = new List<Variation>();
    public List<Variation> gameTypeVariations = new List<Variation>();

    public CustomUIOpener customUIOpener;
    private GAME_TYPE gameType;
    public bool isPlayTabItem = false;
    private Coroutine playingCoroutine;
    private bool reservedCoroutine = false;

    [SerializeField]
    private List<Variation> myTnmtVariations = new List<Variation>();

    [SerializeField]
    private List<Variation> tagVariations = new List<Variation>();

    [SerializeField]
    private LocalText tagText;

    [SerializeField]
    private Variation stateVariation;
    private Variation chipTypeVariation;

    [SerializeField]
    private string defaltOutColor = "#62C6D5";

    [SerializeField]
    private Image outlineImage = null;

    private void Awake() { }

    private void OnDestroy()
    {
        // LobbyManager.Instance.roomUpdate.RemoveListener(UpdateCanJoin);
    }

    private enum TNMT_TYPE
    {
        none,
        t_blind_up_structure_1,
        t_blind_up_structure_2,
        t_blind_up_structure_3,
        t_blind_up_structure_4,
        t_blind_up_structure_5,
        t_blind_up_structure_6,
        t_blind_up_structure_7,
        t_blind_up_structure_8,
        t_blind_up_structure_9
    }

    private enum T_TAG
    {
        none,
        HOT,
        EVENT,
    }

    public void SetTag()
    {
        if (tagText)
        {
            var tag = tnmtInfo.info.ValueOrDefault("t_tag", T_TAG.none);
            tagText.SetLocalText(tag.ToString());
            foreach (var vari in tagVariations)
            {
                vari.SetVariation(tag.ToString());
            }
        }
    }

    public void MyTnmt()
    {
        var tnmt = MyStatus.GetTnmt(tnmtInfo.tn);
        foreach (var vari in myTnmtVariations)
        {
            var str = (tnmt != null).ToString();
            vari.SetVariation((tnmt != null).ToString());
        }
    }

    private void SetOutlineColor()
    {
        if (outlineImage)
        {
            var t_o = tnmtInfo.info.CastOrEmpty<JObject>("t_o");
            var colorCode = t_o.ValueOrDefault("t_out_color", defaltOutColor);
            if (colorCode[0] != '#')
            {
                colorCode = "#" + colorCode;
            }
            var color = ExtensionMethod.HexColor(colorCode);
            outlineImage.color = color;
        }
    }

    public async void Set(TournamentInfo info, int cafeIdx = -1)
    {
        gauge.Set(info);
        tnmtInfo = info;


        SetTag();
        MyTnmt();
        SetOutlineColor();

        int game_type = (int)tnmtInfo.info["t_game_type"];

        for (int i = 0; i < gameTypeVariations.Count; i++)
        {
            gameTypeVariations[i].SetVariation(((GAME_TYPE)game_type).ToString());
        }

        string type = tnmtInfo.info.ValueOrDefault("t_type", 1) == 1 ? "mtt" : "sng";
        txt_game_type.text = LocalizeManager.GetLocalString(
            type + "_" + ((GAME_TYPE)game_type).ToString()
        );
        gameType = (GAME_TYPE)game_type;
        txt_tn.text = "#" + (string)tnmtInfo.info["tn"];

        txt_tnmt_title.text = tnmtInfo.info.ValueOrDefault("t_title", string.Empty); // cafeIdx != -1 이면 cafeGameList 이다. 여기서는 cafe_name 을 출력하지 않는다.
        //
        var builndUpType = tnmtInfo.info.ValueOrDefault(
            "t_event",
            TNMT_TYPE.t_blind_up_structure_1
        );
        txt_tnmt_type.SetLocalText(builndUpType.ToString()); // 게임 이름 출력하도록 수정
        if (chip_icon)
        {
            chip_icon.gameObject.SetActive(cafeIdx == -1); // 특정 카페에 들어가 있지 않으면,
        }

        txt_prize.text = MoneyToString.Converting(
            tnmtInfo.info.ValueOrDefault<long>("t_reward_all_chip", 0)
        );
        startTime = tnmtInfo.info.ValueOrDefault("t_start_time", DateTime.UtcNow).ToLocalTime();
        openTime = tnmtInfo.info.ValueOrDefault("t_open_time", DateTime.UtcNow).ToLocalTime();
        closeTime = tnmtInfo.info.ValueOrDefault("t_close_time", DateTime.UtcNow).ToLocalTime();
        txt_time.SetLocalText("mtt_start_time", startTime.ToString("yyyy.MM.dd HH:mm"));

        // deprecated   2021-04-09 Alberto
        //long total_pot = log.ValueOrDefault<long>("total_pot", 0);
        //long play_count = (tnmtInfo.info["log"] as JObject).ValueOrDefault<long>("play_count", 1);

        //txt_averPot.gameObject.SetActive(tnmtInfo.info.ContainsKey("log"));
        //txt_averPot.text = LocalizeManager.GetLocalString("avg_pot") + " " + (total_pot / play_count).ToString();

        long buyin = tnmtInfo.info.ValueOrDefault("t_buyin", 0);
        long t_buyin_fee = tnmtInfo.info.ValueOrDefault("t_buyin_fee", 0);
        var t_ticket = tnmtInfo.info.CastOrEmpty<JObject>("t_ticket");
        bool ticket_active = false;
        int ticket_type = 0;
        int ticket_count = 0;
        string condition = "";
        if (t_ticket != null)
        {
            ticket_type = t_ticket.ValueOrDefault("ticket_type", 0);
            ticket_count = t_ticket.ValueOrDefault("ticket_count", 0);
            condition = t_ticket.ValueOrDefault("condition", "and");
            ticket_active = ticket_type != 0;
        }
        // txt_buyin.text = $"{buyin:#,##0.##}";
        List<string> buyins = new List<string>();
        bool buyinActive = buyin + t_buyin_fee > 0;

        if (buyinActive)
        {
            buyins.Add($"{MoneyToString.Converting(buyin + t_buyin_fee)}칩");
        }
        if (ticket_active)
        {
            if (buyinActive)
            {
                buyins.Add(LocalizeManager.GetLocalString(condition.Equals("and") ? "and" : "or"));
            }
            var ticketString = await KingshillInfo.GetTicketString(ticket_type);
            buyins.Add(
                $"{ticketString}:{ticket_count}"
            );


        }
        txt_buyin.text = buyins.Count > 0 ? string.Join("\n", buyins) : 0.ToString();

        CafeInfo cafeInfo = Cafe.instance.GetCafeList((int)tnmtInfo.info["cafeIdx"]);
        if (cafeInfo != null)
        {
            long dc = (long)cafeInfo.cafeMembers[0].dc;
            txt_cafeMember_dc.text = cafeIdx == -1 ? $"{dc:#,##0.##}" : ""; // cafeIdx != -1 이면 cafeGameList 이다. 여기서는 cafe_name 을 출력하지 않는다.

            if (cafeIdx == -1) // 특정 카페에 들어가 있지 않으면,
            {
                obj_cafe_stop.SetActive((long)cafeInfo.info["leftSeconds"] <= 0); // 시간이 모두 0이 되면 '시계+스톱' icon이 활성화된다.  Alberto 2021-04-25
            }
            else
            {
                obj_cafe_stop.SetActive(false);
            }

            txt_cafeIdx.text = "cafeIdx: " + tnmtInfo.info["cafeIdx"];

            int permit = (int)
                Cafe.instance.GetCafeList((int)tnmtInfo.info["cafeIdx"]).cafeMembers[0].permit;
            for (int i = 0; i < permitVariations.Count; i++)
            {
                permitVariations[i].SetVariation(((CAFE_MEMBER_PERMIT)permit).ToString());
            }
        }
        StartTimeSetCoroutine();
    }

    private void OnEnable()
    {
        if (tnmtInfo != null)
        {
            StartTimeSetCoroutine();
        }
    }

    private void StartTimeSetCoroutine()
    {
        if (gameObject.activeInHierarchy)
        {
            if (playingCoroutine != null)
            {
                StopCoroutine(playingCoroutine);
            }
            playingCoroutine = StartCoroutine(TimeTextSetting());
        }
    }

    private IEnumerator TimeTextSetting()
    {
        while (true)
        {
            if (tnmtInfo.state >= (int)TNMT_FLOW.cancel)
            {
                stateVariation.SetVariation("CANCELED");
                titleLocal.LocalKey = "canceled";
                titleContents.SetActive(false);
                date.SetActive(false);
                contents.text = LocalizeManager.GetLocalString("game_canceled");
                yield break;
            }
            if (tnmtInfo.state == (int)TNMT_FLOW.close)
            {
                stateVariation.SetVariation("PLAYING");

                titleContents.SetActive(true);
                date.SetActive(false);
                titleLocal.LocalKey = "playing";
                contents.text = LocalizeManager.GetLocalString("regi_end"); // $"{tnmtInfo.countAllUser}/{tnmtInfo.info.ValueOrDefault("t_max_player", 0)}";
                yield break;
            }
            if (tnmtInfo.state == (int)TNMT_FLOW.end)
            {
                stateVariation.SetVariation("DONE");
                titleContents.SetActive(true);
                date.SetActive(false);
                titleLocal.LocalKey = "done";
                contents.text = LocalizeManager.GetLocalString("game_end"); //$"{tnmtInfo.countAllUser}/{tnmtInfo.info.ValueOrDefault("t_max_player", 0)}";
                yield break;
            }

            DateTime curTime = DateTime.Now;

            if (openTime > curTime)
            {
                TimeSpan turm = openTime - curTime;

                stateVariation.SetVariation("NOTOPEN");
                titleLocal.LocalKey = "not_open";
                TimeSet(turm);
            }
            else if (startTime > curTime) // 토너먼트 시작 전
            {
                TimeSpan turm = startTime - curTime;

                stateVariation.SetVariation("OPEN");
                titleLocal.LocalKey = "open";
                TimeSet(turm);
            }
            else if (closeTime > curTime) // 토너먼트 시작 후, 모집 종료 전
            {
                TimeSpan turm = closeTime - curTime;

                stateVariation.SetVariation("LATER");
                titleLocal.LocalKey = "later";

                TimeSet(turm);
            }
            else // 토너먼트 시작 후, 모집 종료 후
            {
                stateVariation.SetVariation("PLAYING");

                titleContents.SetActive(true);
                date.SetActive(false);
                titleLocal.LocalKey = "playing";
                contents.text = LocalizeManager.GetLocalString("regi_end"); //$"{tnmtInfo.countAllUser}/{tnmtInfo.info.ValueOrDefault("t_max_player", 0)}";
            }
            if (tnmtInfo.countAllUser >= tnmtInfo.info.ValueOrDefault("t_max_player", 0))
            {
                contents.text = LocalizeManager.GetLocalString("regi_end");
            }
            yield return new WaitForSeconds(0.1f);
        }
    }

    public void TimeSet(TimeSpan turm)
    {
        if (turm.Days > 0)
        {
            titleContents.SetActive(false);
            date.SetActive(true);
            string providerStr = "en-US";
            string weekUnit = "";
            if (LocalizeManager.LanguageKey == SystemLanguage.Korean.ToString())
            {
                providerStr = "ko-KR";
                weekUnit = "요일";
            }
            IFormatProvider provider = new System.Globalization.CultureInfo(providerStr);
            month.text = startTime.ToString("MMM", provider).ToUpper();
            week.text = startTime.ToString("ddd", provider).ToUpper() + weekUnit;
            day.text = startTime.ToString("dd");
        }
        else if (turm.Hours > 0)
        {
            titleContents.SetActive(true);
            date.SetActive(false);
            contents.text = $"{turm.Hours}h {turm.Minutes}m";
        }
        else if (turm.Minutes > 0)
        {
            titleContents.SetActive(true);
            date.SetActive(false);
            contents.text = $"{turm.Minutes}m {turm.Seconds}s";
        }
        else
        {
            titleContents.SetActive(true);
            date.SetActive(false);
            contents.text = $"{turm.Seconds}s";
        }
    }

    public void PlayGameEnter()
    {
        (customUIOpener as CustomUIOpenerLobby).SetTn(tnmtInfo.tn); // = tnmtInfo;
        (customUIOpener as CustomUIOpenerLobby).selectedTnmtIsPlayTabItem = isPlayTabItem;
        string type = tnmtInfo.info.ValueOrDefault("t_type", 1) == 1 ? "mtt" : "sng";
        customUIOpener.ShowUI(type + "_" + gameType.ToString() + "_info");
    }

    public void OnClickEnterButton()
    {
        PlayGameEnter();
    }
}
