using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using Constant;
using Cysharp.Threading.Tasks;
using distriqt.plugins.share;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.UI;

public class CustomUIOpenerLobby : CustomUIOpener
{
    [SerializeField]
    private CafeLogoSelector logoSelector;

    [SerializeField]
    private bool ingame = false;
    public PlayGameInfo selectedGame = null;
    public bool selectedGameIsPlayTabItem;
    public TournamentInfo selectedTnmt = null;
    public JObject tnmtResult;

    public bool selectedTnmtIsPlayTabItem;
    public Button backButton;
    private Button.ButtonClickedEvent originBackButtonEvent;
    private Button.ButtonClickedEvent subBackButtonEvent;
    private bool backButtonOriginActive = false;
    private JArray backButtonEvents;

    [SerializeField]
    private CustomUIOpener popup_opener;
    private DataTable calculator = new DataTable();
    public GameObject changeNickPanel;
    public string sceneLoadOnAwake = "";
    public GameObject recomWindow = null;

    private bool setting_push = false;

    public bool InGame
    {
        get { return ingame; }
    }

    protected override void Awake()
    {
        base.Awake();
        if (backButton)
        {
            backButtonOriginActive = backButton.gameObject.activeSelf;
            originBackButtonEvent = backButton.onClick;
            subBackButtonEvent = new Button.ButtonClickedEvent();
            subBackButtonEvent.AddListener(OnClickBackButton);
        }
        if (!string.IsNullOrEmpty(sceneLoadOnAwake))
        {
            ShowUI(sceneLoadOnAwake);
        }
    }

    private void OnEnable()
    {
        if (init)
        {
            ShowUI(sceneLoadOnAwake);
        }
    }

    protected override void SetEvent()
    {
        base.SetEvent();
        root.events.Add("set_default", SetDefault); // deprecated
        root.events.Add("set_max", SetMax);
        root.events.Add("change_scene", ChangeScene);
        root.events.Add("scene_change", ChangeScene);
        root.events.Add("close", CloseEvent);
        root.events.Add("CP_TNMT_FIX", CP_TNMT_FIX);
        root.events.Add("CP_CAFE_CREATE", CP_CAFE_CREATE);
        root.events.Add("create_game_scene_change", CreateGameSceneChange);
        root.events.Add("CP_CAFE_GAME_CREATE", CP_CAFE_GAME_CREATE);
        root.events.Add("CP_CAFE_JOIN", CP_CAFE_JOIN);
        root.events.Add("trans_zuice_calculate", TransZuiceCalculate);
        root.events.Add("CP_CAFE_TRANS_ZUICE", CP_CAFE_TRANS_ZUICE);
        root.events.Add("change_scene_with_fulldown", ChangeSceneWithFulldown);
        root.events.Add("calculate_sell_buy_borrow", CalculateSellBuyBorrow);
        root.events.Add("CP_CAFE_MEMBER_ORDER", CP_CAFE_MEMBER_ORDER);
        root.events.Add("buyin_dual_slide_min_max_change", BuyinDualSliderLimitChange);
        root.events.Add("set_local_key", SetLocalKey);
        root.events.Add("set_radio_with_prefs", SetRadioWithPrefs);
        root.events.Add("show_only_manager", OnlyStaff);
        root.events.Add("show_only_owner", OnlyOwner);
        root.events.Add("BlindChange", BlindChange);
        root.events.Add("ButtonBlindChange", ButtonBlindChange);
        root.events.Add("save_prefs", SavePrefs);
        root.events.Add("load_prefs", LoadPrefs);
        root.events.Add("dual_limit", DualLimit);
        root.events.Add("mute", MuteAll);
        root.events.Add("vive",Vive);
        root.events.Add("bb_text", BBText);
        root.events.Add("limit_player_count", LimitPlayerCount);
        root.events.Add("value_text", ValueText);
        root.events.Add("invite_cafe", InviteCafe);
        root.events.Add("CP_CAFE_LEAVE", CP_CAFE_LEAVE);
        root.events.Add("load_deep_link", LoadDeepLink);
        root.events.Add("delete_deep_link", DeleteDeepLink);
        root.events.Add("subscribe_logo_selector", SubscribeLogoSelector);
        root.events.Add("unsubscribe_logo_selector", UnSubscribeLogoSelector);
        root.events.Add("selector_open", OpenSelector);
        root.events.Add("logo_selector_random", LogoSelectorRandom);
        root.events.Add("betsize_limit", BetSizeLimit);
        root.events.Add("show_game_info", ShowGameInfo);
        root.events.Add("show_tnmt_info", ShowTnmtInfo);
        root.events.Add("enter_table", EnterTable);
        root.events.Add("edit_table", EditTable);
        root.events.Add("delete_table", DeleteTable);
        root.events.Add("show_only_in_play", ShowOnlyInPlay);
        root.events.Add("show_only_in_cafe", ShowOnlyInCafe);
        root.events.Add("btn_back_active", BackButtonActive);
        root.events.Add("btn_back_set", BackButtonSet);
        root.events.Add("popup_open", PopupOpen);
        root.events.Add("calculate_value", CalculateValue);
        root.events.Add("condition_active", ConditionActive);
        root.events.Add("override_cafe_data", OverrideCafeData);
        root.events.Add("CP_CAFE_UPDATE", CP_CAFE_UPDATE);
        root.events.Add("CP_CAFE_RESERVE_REMOVE", CP_CAFE_RESERVE_REMOVE);
        root.events.Add("logout", Logout);
        root.events.Add("change_nick", ChangeNick);
        root.events.Add("open_url", OpenUrl);
        root.events.Add("version", Version);
        root.events.Add("account_email", Account_email);
        root.events.Add("set_push", SetPush);
        root.events.Add("edit_push", EditPush);
        root.events.Add("CP_TNMT_CREATE", CP_TNMT_CREATE);
        root.events.Add("CP_TNMT_APPLY", CP_TNMT_APPLY);
        root.events.Add("CP_TNMT_UNAPPLY", CP_TNMT_UNAPPLY);
        root.events.Add("show_tnmt_result", ShowTnmtResult);
        root.events.Add("open_recom", OpenRecomWindow);
        root.events.Add("delete_account", DeleteAcount);
        root.events.Add("tnmt_room_enter", TnmtRoomEnter);
        root.events.Add("BuyinScaleChanged", BuyinScaleChanged);
    }

    private void DeleteAcount(CustomUI ui, JObject data, JObject evt)
    {
        NormalMessage.instance.OnMessagePopup(
            "회원 탈퇴",
            "탈퇴 시 7일간의 유예 기간이 주어지며, 기간 중 탈퇴 철회가 가능합니다.\n\n7일이 경과할 경우 모든 게임 데이터는 삭제되어 복구가 되지 않습니다.\n\n정말 탈퇴하시겠습니까?",
            DropOut
        );
    }

    public void DropOut()
    {
        PublisherApiManager.Instance.RequestDropout(DropOutCallBack);
    }

    public void DropOutCallBack(bool success, Newtonsoft.Json.Linq.JObject jobj)
    {
        if (success)
        {
            LogOut();
        }
    }
   

    public void LogOut()
    {
        WebSocketManager.defaultCli.OnExitOnce += (reson) =>
        {
            DevManager.Instance.GsLogin = false;
            DevManager.Instance.WsDelegate -= 1;
            DevManager.Instance.WsConnect = false;
            FirebaseManager.Instance.SignOut();
            CustomSceneManager.LoadLoginScene();
        };
        WebSocketManager.defaultCli.Close();
    }

    private void OpenRecomWindow(CustomUI ui, JObject data, JObject evt)
    {
        if (recomWindow)
        {
            recomWindow.SetActive(true);
        }
        else
        {
            Debug.LogError("Not assign recomWindow");
        }
    }

    private void SubscribeLogoSelector(CustomUI ui, JObject data, JObject evt)
    {
        logoSelector.onSelected = (sprite) =>
        {
            List<Sprite> sprites = new List<Sprite>();
            sprites.Add(sprite);
            (ui as CustomUIImageButton).ChangeSprite(sprites);
        };
    }

    public void SetTn(int tn)
    {
        selectedTnmt = InfoManager.Instance.GetTournamentInfo(tn);
    }

    private void UnSubscribeLogoSelector(CustomUI ui, JObject data, JObject evt)
    {
        logoSelector.onSelected = null;
    }

    private void OpenSelector(CustomUI ui, JObject data, JObject evt)
    {
        logoSelector.gameObject.SetActive(true);
    }

    private void LogoSelectorRandom(CustomUI ui, JObject data, JObject evt)
    {
        logoSelector.RandomPick();
    }

    private void SetDefault(CustomUI ui, JObject data, JObject evt)
    {
        if (evt.ContainsKey("param"))
        {
            string param = evt["param"].ToString();
            switch (param)
            {
                case "cafe_zuice":
                    data["default"] = Cafe.instance.curEnterCafeInfo["cafe"]["zc"];
                    break;
                case "cafe_my_zuice":
                    data["default"] = Cafe.instance.curEnterCafeInfo["cafeMember"]["zc"];
                    break;
                case "my_zuice":
                    data["default"] = MyStatus.zc;
                    break;
                case "cafe_my_dc":
                    data["default"] = Cafe.instance.curEnterCafeInfo["cafeMember"]["cc"];
                    break;
                case "cafe_dc":
                    data["default"] = Cafe.instance.curEnterCafeInfo["cafe"]["cc"];
                    break;
                case "cafe_my_debt":
                    data["default"] = Cafe.instance.curEnterCafeInfo["cafeMember"]["debt"];
                    break;
                case "result_my_surplus":
                    data["default"] =
                        (long)Cafe.instance.curEnterCafeInfo["cafeMember"]["cc"]
                        - (long)Cafe.instance.curEnterCafeInfo["cafeMember"]["debt"];
                    break;
            }
        }
    }

    private void SetMax(CustomUI ui, JObject data, JObject evt)
    {
        string formula = ReplaceFormulaString(root.GetJObject(), evt.ValueOrDefault("formula", ""));
        try
        {
            object result = calculator.Compute(formula, null);
            ui.SetMax(System.Convert.ToDouble(result));
        }
        catch
        {
            Debug.LogError("Calculating Error!");
        }
    }

    private void ChangeScene(CustomUI ui, JObject data, JObject evt)
    {
        ShowUI(evt["param"].ToString());
    }

    private void CloseEvent(CustomUI ui, JObject data, JObject evt)
    {
        CloseUI();
    }

    private void CP_TNMT_FIX(CustomUI ui, JObject data, JObject evt)
    {
        NormalMessage.instance.OnMessagePopup(
            $"토너먼트 취소",
            $"{selectedTnmt.tn}번 토너먼트를 취소합니다.",
            TnmtCancel,
            null
        );
    }

    private void TnmtCancel()
    {
        var tn = selectedTnmt.tn;
        var p = new Packet(CPProtocol.CP_TNMT_FIX);
        p.Add("tn", tn);
        p.Add("state", (int)TNMT_FLOW.cancel);
        WebSocketManager.defaultCli.Send(p);
        CloseUI();
    }

    private void CP_CAFE_CREATE(CustomUI ui, JObject data, JObject evt)
    {
        JObject obj = root.GetJObject();
        string name = obj.ValueOrDefault("name", "");

        if (name == "")
        {
            Debug.LogError("카페 이름칸이 비어있습니다.");
            ErrorMessageManager.Instance.AddGameError(
                0,
                "please_input_cafe_name_title",
                "please_input_cafe_name_contents"
            );
            //카페 이름을 입력해주세요 팝업 표시
            return;
        }
        else
        {
            bool validCafeName = Regex.IsMatch(name, @"[a-zA-Z가-힣0-9 ]{2,16}$");
            if (!validCafeName)
            {
                ErrorMessageManager.Instance.AddGameError(
                    0,
                    "cant_use_cafename_title",
                    "cant_use_cafename_contents"
                );
                //카페 이름을 입력해주세요 팝업 표시
                return;
            }
        }

        if (obj["password"] != null) // 2021-03-25 카페코드는 서버에서 강제로 만들어진다. 또한 변경할수 없다.
        {
            if (!((bool)obj["password"]))
            {
                obj["pw"] = "";
            }
        }
        else
        {
            obj["pw"] = "";
        }
        obj["icon"] = logoSelector.selected;
        if (logoSelector.selected == 0)
        {
            Texture2D texture = logoSelector.photo;
            texture = texture.DeCompress();
            byte[] bytes = texture.EncodeToPNG();
            string base64String = System.Convert.ToBase64String(bytes);
            PublisherApiManager.Instance.UploadCafeLogo(
                0,
                base64String,
                (statusCode, result) =>
                {
                    if (statusCode == 200)
                    {
                        obj["photourl"] = result.ValueOrDefault("url", "");
                    }
                    Packet p = new Packet(CPProtocol.CP_CAFE_CREATE);
                    p.Add("o", obj);
                    WebSocketManager.defaultCli.Send(p);
                }
            );
        }
        else
        {
            Packet p = new Packet(CPProtocol.CP_CAFE_CREATE);
            p.Add("o", obj);
            WebSocketManager.defaultCli.Send(p);
        }
        CloseUI();
    }

    private void CP_CAFE_GAME_CREATE(CustomUI ui, JObject data, JObject evt)
    {
        JObject obj = root.GetJObject();
        string game_type = GAME_TYPE.nlh.ToString();
        if (evt.ContainsKey("game_type"))
        {
            game_type = evt["game_type"].ToString();
        }
        long bb = (long)obj["blind"];
        obj["game_type"] = (int)System.Enum.Parse(typeof(GAME_TYPE), game_type);
        obj["buyin_min"] = (long)obj["buyin"][0] * bb;
        obj["buyin_max"] = (long)obj["buyin"][1] * bb;
        obj["cafeIdx"] = Cafe.instance.curEnterCafeInfo["cafe"]["idx"];
        obj["alwaysOpen"] = true;

        switch (System.Enum.Parse(typeof(GAME_TYPE), game_type))
        {
            case GAME_TYPE.short_deck:
                obj["ante"] = (long)(obj["blind"]);
                break;
            default:
                obj["ante"] = (long)((double)obj["ante"] * bb);
                break;
        }

        obj.Remove("buyin");
        StartCoroutine(GameCreateFlow(obj));
        CloseUI();
    }

    private IEnumerator GameCreateFlow(JObject gameData)
    {
        Packet p = new Packet(CPProtocol.CP_CAFE_GAME_CREATE);
        p.Add("o", gameData);
        WebSocketManager.defaultCli.Send(p);
        WaitForPCProtocol wait = new WaitForPCProtocol(PCProtocol.PC_CAFE_GAME_CREATE);
        yield return wait;
        if (wait.Result.c.ValueOrDefault("ecode", 0) == 0)
            NormalMessage.instance.OnOneButtonMessagePopUp("table_success");
        else
            NormalMessage.instance.OnOneButtonMessagePopUp("table_fail");
    }

    private void CreateGameSceneChange(CustomUI ui, JObject data, JObject evt)
    {
        ShowUI(root.GetJObject()["game_type"].ToString());
    }

    private void CP_CAFE_JOIN(CustomUI ui, JObject data, JObject evt)
    {
        JObject obj = root.GetJObject();
        if (!obj.ContainsKey("cafeCode"))
            obj.Add("cafeCode", "");
        if (!obj.ContainsKey("autoJoinCode"))
            obj.Add("autoJoinCode", "");
        WebSocketManager.defaultCli.Send(new Packet(CPProtocol.CP_CAFE_JOIN, obj));
    }

    private void TransZuiceCalculate(CustomUI ui, JObject data, JObject evt)
    {
        JObject obj = root.GetJObject();
        List<CustomUI> result_cafe = root.GetGroup(evt["result_cafe"].ToString());
        List<CustomUI> result_my = root.GetGroup(evt["result_my"].ToString());
        long myZc = MyStatus.zc;
        long cafeZc = (long)Cafe.instance.curEnterCafeInfo["cafe"]["zc"];
        long transZc = (long)obj["zc"];
        for (int i = 0; i < result_my.Count; i++)
        {
            (result_my[i] as CustomUIInfo).SetValue((myZc - transZc).ToString());
        }
        for (int i = 0; i < result_cafe.Count; i++)
        {
            (result_cafe[i] as CustomUIInfo).SetValue((cafeZc + transZc).ToString());
        }
    }

    private void CP_CAFE_TRANS_ZUICE(CustomUI ui, JObject data, JObject evt)
    {
        JObject obj = root.GetJObject();
        obj["cafeIdx"] = Cafe.instance.curEnterCafeInfo["cafe"]["idx"];
        WebSocketManager.defaultCli.Send(new Packet(CPProtocol.CP_CAFE_TRANS_ZUICE, obj));
        CloseUI();
    }

    private void BuyinScaleChanged(CustomUI ui, JObject data, JObject evt)
    {
        var info = selectedTnmt.info;
        var t_buyin_scale_min = info.ValueOrDefault("t_buyin_scale_min", 1);
        var t_buyin_scale_max = info.ValueOrDefault("t_buyin_scale_max", 1);
        var scale = (int)ui.GetValue();
        ui.SetValue(Mathf.Clamp(scale, t_buyin_scale_min, t_buyin_scale_max));
    }

    private void ChangeSceneWithFulldown(CustomUI ui, JObject data, JObject evt)
    {
        ShowUI((ui as CustomUIFulldown).GetProperty().Value.ToString());
    }

    private void CalculateSellBuyBorrow(CustomUI ui, JObject data, JObject evt)
    {
        JObject obj = root.GetJObject();
        TRANSFER_TYPE type = (TRANSFER_TYPE)
            System.Enum.Parse(typeof(TRANSFER_TYPE), obj["status"].ToString());
        long my_cc = (long)Cafe.instance.curEnterCafeInfo["cafeMember"]["cc"];
        long my_debt = (long)Cafe.instance.curEnterCafeInfo["cafeMember"]["debt"];
        long amount = (long)obj["amount"];
        List<CustomUI> result_my_dc = root.GetGroup(evt["result_my_dc"].ToString());
        List<CustomUI> result_my_debt = root.GetGroup(evt["result_my_debt"].ToString());
        List<CustomUI> result_my_surplus = root.GetGroup(evt["result_my_surplus"].ToString());
        long calculate_my_cc = 0;
        long calculate_my_debt = 0;
        switch (type)
        {
            case TRANSFER_TYPE.sell:
                calculate_my_cc = my_cc - amount;
                calculate_my_debt = (my_debt - amount) > 0 ? (my_debt - amount) : 0;
                break;
            case TRANSFER_TYPE.buy:
                calculate_my_cc = my_cc + amount;
                break;
            case TRANSFER_TYPE.borrow:
                calculate_my_cc = my_cc + amount;
                calculate_my_debt = my_debt + amount;
                break;
        }

        for (int i = 0; i < result_my_dc.Count; i++)
        {
            result_my_dc[i].SetValue(calculate_my_cc);
        }
        for (int i = 0; i < result_my_debt.Count; i++)
        {
            result_my_debt[i].SetValue(calculate_my_debt);
        }
        for (int i = 0; i < result_my_surplus.Count; i++)
        {
            result_my_surplus[i].SetValue(calculate_my_cc - calculate_my_debt);
        }
    }

    private void CP_CAFE_MEMBER_ORDER(CustomUI ui, JObject data, JObject evt)
    {
        JObject obj = root.GetJObject();
        if (obj.ContainsKey("status"))
        {
            obj["status"] = (int)System.Enum.Parse(typeof(TRANSFER_TYPE), obj["status"].ToString());
        }
        else if (evt.ContainsKey("status"))
        {
            obj["status"] = (int)System.Enum.Parse(typeof(TRANSFER_TYPE), evt["status"].ToString());
        }
        if ((long)obj["amount"] <= 0)
        {
            Debug.LogError("0 보다 큰 수를 입력하세요.");
            return;
        }
        obj["cafeIdx"] = Cafe.instance.curEnterCafeInfo["cafe"]["idx"];
        obj["goods"] = 1;
        WebSocketManager.defaultCli.Send(new Packet(CPProtocol.CP_CAFE_MEMBER_ORDER, obj));
        LoadingCircle.Instance.StartSpin();
    }

    private void BuyinDualSliderLimitChange(CustomUI ui, JObject data, JObject evt)
    {
        long blind = (long)ui.GetValue();
        var groups = root.GetGroup(evt["group"].ToString());
        for (int i = 0; i < groups.Count; i++)
        {
            groups[i].SetMin(10);
            groups[i].SetMax(100);
        }
    }

    private void SetLocalKey(CustomUI ui, JObject data, JObject evt)
    {
        SystemLanguage language = (SystemLanguage)
            System.Enum.Parse(
                typeof(SystemLanguage),
                (ui as CustomUIRadio).GetOnlyValue().ToString()
            );
        LocalizeManager.SetLocal(language.ToString());
    }

    private void SetRadioWithPrefs(CustomUI ui, JObject data, JObject evt)
    {
        switch (evt["type"].ToString())
        {
            case "string":
                string str = PlayerPrefs.GetString(evt["key"].ToString(), "");
                JArray arr = data["data"] as JArray;
                for (int i = 0; i < arr.Count; i++)
                {
                    if (arr[i].ToString() == str)
                    {
                        (ui as CustomUIRadio).SetIndex(i);
                    }
                }
                break;
        }
    }

    public void OnlyStaff(CustomUI ui, JObject data, JObject evt)
    {
        if (!ui.gameObject.activeSelf)
            return;
        CAFE_MEMBER_PERMIT myPermit = Cafe.instance.permit;
        CAFE_MEMBER_PERMIT[] permits = new CAFE_MEMBER_PERMIT[2]
        {
            CAFE_MEMBER_PERMIT.manager,
            CAFE_MEMBER_PERMIT.owner
        };
        System.Type type = ui.GetType();
        if (type == typeof(CustomUIFulldown))
        {
            CustomUIFulldown fulldown = (ui as CustomUIFulldown);
            JArray arr = data["data"].DeepClone() as JArray;
            List<int> indexes = new List<int>();
            for (int i = 0; i < permits.Length; i++)
            {
                if (myPermit < permits[i] && evt.ContainsKey(permits[i].ToString()))
                {
                    List<int> temp = evt[permits[i].ToString()].ToObject<List<int>>();
                    temp.ForEach(t =>
                    {
                        if (!indexes.Contains(t))
                            indexes.Add(t);
                    });
                }
            }
            indexes.Sort();
            for (int i = indexes.Count - 1; i >= 0; i--)
            {
                arr.RemoveAt(indexes[i]);
            }
            fulldown.ChangeOptions(arr);
        }
        else if (type == typeof(CustomUIActiveFulldown))
        {
            CustomUIActiveFulldown fulldown = (ui as CustomUIActiveFulldown);
            JArray arr = data["data"].DeepClone() as JArray;
            JArray active_group = data["active_groups"].DeepClone() as JArray;
            List<int> indexes = new List<int>();
            for (int i = 0; i < permits.Length; i++)
            {
                if (myPermit < permits[i] && evt.ContainsKey(permits[i].ToString()))
                {
                    List<int> temp = evt[permits[i].ToString()].ToObject<List<int>>();
                    temp.ForEach(t =>
                    {
                        if (!indexes.Contains(t))
                            indexes.Add(t);
                    });
                }
            }
            indexes.Sort();
            for (int i = indexes.Count - 1; i >= 0; i--)
            {
                arr.RemoveAt(indexes[i]);
                active_group.RemoveAt(indexes[i]);
            }
            fulldown.ChangeOptions(arr, active_group);
        }
        else
        {
            ui.gameObject.SetActive(System.Array.IndexOf(permits, myPermit) != -1);
        }
    }

    public void OnlyOwner(CustomUI ui, JObject data, JObject evt)
    {
        if (!ui.gameObject.activeSelf)
            return;
        CAFE_MEMBER_PERMIT myPermit = Cafe.instance.permit;
        CAFE_MEMBER_PERMIT[] permits = new CAFE_MEMBER_PERMIT[] { CAFE_MEMBER_PERMIT.owner };
        System.Type type = ui.GetType();
        if (type == typeof(CustomUIFulldown))
        {
            CustomUIFulldown fulldown = (ui as CustomUIFulldown);
            JArray arr = data["data"].DeepClone() as JArray;
            List<int> indexes = new List<int>();
            for (int i = 0; i < permits.Length; i++)
            {
                if (myPermit < permits[i] && evt.ContainsKey(permits[i].ToString()))
                {
                    List<int> temp = evt[permits[i].ToString()].ToObject<List<int>>();
                    temp.ForEach(t =>
                    {
                        if (!indexes.Contains(t))
                            indexes.Add(t);
                    });
                }
            }
            indexes.Sort();
            for (int i = indexes.Count - 1; i >= 0; i--)
            {
                arr.RemoveAt(indexes[i]);
            }
            fulldown.ChangeOptions(arr);
        }
        else if (type == typeof(CustomUIActiveFulldown))
        {
            CustomUIActiveFulldown fulldown = (ui as CustomUIActiveFulldown);
            JArray arr = data["data"].DeepClone() as JArray;
            JArray active_group = data["active_groups"].DeepClone() as JArray;
            List<int> indexes = new List<int>();
            for (int i = 0; i < permits.Length; i++)
            {
                if (myPermit < permits[i] && evt.ContainsKey(permits[i].ToString()))
                {
                    List<int> temp = evt[permits[i].ToString()].ToObject<List<int>>();
                    temp.ForEach(t =>
                    {
                        if (!indexes.Contains(t))
                            indexes.Add(t);
                    });
                }
            }
            indexes.Sort();
            for (int i = indexes.Count - 1; i >= 0; i--)
            {
                arr.RemoveAt(indexes[i]);
                active_group.RemoveAt(indexes[i]);
            }
            fulldown.ChangeOptions(arr, active_group);
        }
        else
        {
            ui.gameObject.SetActive(System.Array.IndexOf(permits, myPermit) != -1);
        }
    }

    public void BlindChange(CustomUI ui, JObject data, JObject evt)
    {
        CustomUIInput sbInput = root.GetGroup("sb")[0] as CustomUIInput;
        CustomUIInput bbInput = root.GetGroup("bb")[0] as CustomUIInput;
        // CustomUIInput ebInput = root.GetGroup("eb")[0] as CustomUIInput;

        long sb = (long)sbInput.GetValue();
        long bb = (long)bbInput.GetValue();
        // long eb = (long)ebInput.GetValue();
        if (bb < sb)
            bbInput.SetValue(sb * 2);
        //if (eb < bb)
        //    ebInput.SetValue(bb * 2);
    }

    public void ButtonBlindChange(CustomUI ui, JObject data, JObject evt)
    {
        // CustomUIInput sbInput = root.GetGroup("sb")[0] as CustomUIInput;
        CustomUIInput bbInput = root.GetGroup("bb")[0] as CustomUIInput;
        // CustomUIInput ebInput = root.GetGroup("eb")[0] as CustomUIInput;

        /// long sb = (long)sbInput.GetValue();
        long bb = (long)bbInput.GetValue();
        // long eb = (long)ebInput.GetValue();
        // if (bb < sb)
        // bbInput.SetValue(sb * 2);
        //if (eb < bb)
        //    ebInput.SetValue(bb * 2);
    }

    public void LoadPrefs(CustomUI ui, JObject data, JObject evt)
    {
        if (evt.ContainsKey("key"))
        {
            string key = evt["key"].ToString();
            string defaultValue = evt.ValueOrDefault("load_prefs", ui.GetValue().ToString());
            string value = PlayerPrefs.GetString(key, defaultValue);
            ui.SetValue((JToken)value);
            PlayerPrefs.SetString(key, value);
            PlayerPrefs.Save();
        }
    }

    public void SavePrefs(CustomUI ui, JObject data, JObject evt)
    {
        if (evt.ContainsKey("key"))
        {
            string key = evt["key"].ToString();
            string value = ui.GetValue().ToString();
            PlayerPrefs.SetString(key, value);
            PlayerPrefs.Save();
        }
    }

    public void DualLimit(CustomUI ui, JObject data, JObject evt)
    {
        System.Type type = ui.GetType();
        if (type == typeof(CustomUIRadioDualSlider))
        {
            CustomUIRadioDualSlider slider = ui as CustomUIRadioDualSlider;
            JArray values = slider.GetIndex() as JArray;
            bool hasChanged = false;
            if (evt.ContainsKey("min_maximum"))
            {
                long min_maximum = (long)evt["min_maximum"];
                if ((long)values[1] < min_maximum)
                {
                    values[1] = min_maximum;
                    hasChanged = true;
                }
            }
            if (evt.ContainsKey("max_minimum"))
            {
                long max_minimum = (long)evt["max_minimum"];
                if ((long)values[0] > max_minimum)
                {
                    values[0] = max_minimum;
                    hasChanged = true;
                }
            }
            if (hasChanged)
                slider.SetIndex(values);
        }
        else if (type == typeof(CustomUIDualSlider))
        {
            CustomUIDualSlider slider = ui as CustomUIDualSlider;
            JArray values = slider.GetValue() as JArray;
            if (evt.ContainsKey("min_maximum"))
            {
                long min_maximum = (long)evt["min_maximum"];
                if ((long)values[1] < min_maximum)
                {
                    values[1] = min_maximum;
                }
            }
            if (evt.ContainsKey("max_minimum"))
            {
                long max_minimum = (long)evt["max_minimum"];
                if ((long)values[0] > max_minimum)
                {
                    values[0] = max_minimum;
                }
            }
            slider.SetValue(values);
        }
    }

    public void MuteAll(CustomUI ui, JObject data, JObject evt)
    {
        bool soundPlay = (bool)ui.GetValue();
        SoundManager.Instance.SetAllMute(!soundPlay, true);
    }
     public void Vive(CustomUI ui, JObject data, JObject evt)
    {
        bool vive = (bool)ui.GetValue();
        ViveManager.SetVive(vive);
    }

    public void BBText(CustomUI ui, JObject data, JObject evt)
    {
        bool bbSet = (bool)ui.GetValue();
        MoneyToString.SetBBText(bbSet);
    }

    public void LimitPlayerCount(CustomUI ui, JObject data, JObject evt)
    {
        JObject obj = root.GetJObject();
        int personnel = obj.ValueOrDefault("personnel", 2);
        List<CustomUI> groups = root.GetGroup(evt.ValueOrDefault("group", "null"));
        if (groups.Count > 0)
        {
            CustomUIRadioSlider slider = groups[0] as CustomUIRadioSlider;
            if ((int)slider.GetValue() > personnel)
            {
                slider.SetValue(personnel);
            }
        }
    }

    public void ValueText(CustomUI ui, JObject data, JObject evt)
    {
        JToken values = ui.GetValue();
        JArray arr = null;
        if (values.Type == JTokenType.Array)
        {
            arr = values as JArray;
        }
        else
        {
            arr = new JArray(values);
        }

        List<CustomUI> texts = root.GetGroup(evt.ValueOrDefault("group", ""));
        string[] strings = new string[arr.Count];
        for (int i = 0; i < arr.Count; i++)
        {
            strings[i] = arr[i].ToString();
        }
        for (int i = 0; i < texts.Count; i++)
        {
            CustomUIText text = texts[i] as CustomUIText;
            text.SetText(string.Format(text.originalText, strings));
        }
    }

    public void InviteCafe(CustomUI ui, JObject data, JObject evt)
    {
        JObject obj = root.GetJObject();
        int invite_type = obj.ValueOrDefault("invite_type", 0);

        if (invite_type == 0)
        {
            string url = URL.CAFE_INVITE + "?cafeCode=" + Cafe.instance.cafeCode.text;
            ShareMessage(UrlToInviteMessage(url));
        }
        else
        {
            StartCoroutine(InviteWithAutoJoinCode(invite_type - 1));
        }
    }

    private IEnumerator InviteWithAutoJoinCode(int type)
    {
        Packet request = new Packet(CPProtocol.CP_CAFE_AUTO_JOIN_CODE);
        int cafeIdx = (int)Cafe.instance.curEnterCafeInfo["cafe"]["idx"];
        request.Add("cafeIdx", cafeIdx);
        request.Add("type", type);
        WebSocketManager.defaultCli.Send(request);
        WaitForPCProtocol wait = new WaitForPCProtocol(
            (p, c) =>
            {
                return p == PCProtocol.PC_CAFE_AUTO_JOIN_CODE
                    && c.ValueOrDefault("cafeIdx", 0) == cafeIdx;
            }
        );
        yield return wait;
        string url =
            URL.CAFE_INVITE + "?autoJoinCode=" + wait.Result.c.ValueOrDefault("autoJoinCode", "");
        ShareMessage(UrlToInviteMessage(url));
    }

    private string UrlToInviteMessage(string url)
    {
        Debug.Log(url);
        string invite_text = LocalizeManager.GetLocalString("invite_message");
        if (invite_text.Contains("{url}"))
        {
            invite_text = invite_text.Replace("{url}", url);
        }
        else
        {
            invite_text += " " + url;
        }
        invite_text = invite_text.Replace("{nick}", MyStatus.nick);
        invite_text = invite_text.Replace(
            "{cafe_name}",
            Cafe.instance.curEnterCafeInfo["cafe"]["name"].ToString()
        );
        return invite_text;
    }

    private void ShareMessage(string message)
    {
        if (Share.isSupported)
        {
            Share.Instance.share(message);
        }
        else
        {
            Debug.Log("this device is not support share service");
        }
    }

    public void CP_CAFE_LEAVE(CustomUI ui, JObject data, JObject evt)
    {
        NormalMessage.instance.OnMessagePopup(
            LocalizeManager.GetLocalString("really_unjoin_cafe_title"),
            LocalizeManager.GetLocalString("really_unjoin_cafe_body"),
            () =>
            {
                int cafeIdx = (int)Cafe.instance.curEnterCafeInfo["cafe"]["idx"];
                Packet p = new Packet(CPProtocol.CP_CAFE_LEAVE);
                p.Add("cafeIdx", cafeIdx);
                WebSocketManager.defaultCli.Send(p);
                root.gameObject.SetActive(false);
            }
        );
    }

    public void LoadDeepLink(CustomUI ui, JObject data, JObject evt)
    {
        string key = evt.ValueOrDefault("key", "");
        JToken value = null;
        if (key == "cafeCode" && !string.IsNullOrEmpty(DeepLink.CafeCode))
            value = DeepLink.CafeCode;
        if (key == "autoJoinCode" && !string.IsNullOrEmpty(DeepLink.AutoJoinCode))
            value = DeepLink.AutoJoinCode;
        if (value != null)
        {
            ui.SetValue(value);
        }
    }

    public void DeleteDeepLink(CustomUI ui, JObject data, JObject evt)
    {
        string key = evt.ValueOrDefault("key", "");
        if (key == "cafeCode" && !string.IsNullOrEmpty(DeepLink.CafeCode))
            DeepLink.CafeCode = null;
        if (key == "autoJoinCode" && !string.IsNullOrEmpty(DeepLink.AutoJoinCode))
            DeepLink.AutoJoinCode = null;
    }

    public void BetSizeLimit(CustomUI ui, JObject data, JObject evt)
    {
        float value = (float)ui.GetValue();
        if (value < 1.0f)
        {
            ui.SetValue(0f);
        }
    }

    public void ShowGameInfo(CustomUI ui, JObject data, JObject evt)
    {
        if (selectedGame == null)
            return;
        bool edit_mode = evt.ValueOrDefault("edit_mode", true);
        JObject o = selectedGame.info;
        JProperty[] properties = o.Properties().ToArray();
        string[] booleanArray = null;
        Dictionary<string, string> key_active_group = null;
        Dictionary<string, string> key_active_group_reverse = null;
        if (!edit_mode)
        {
            booleanArray = new string[]
            {
                "tb2",
                "tbs",
                "extra_blind_option",
                "rake",
                "passive_rake",
                "insurance",
                "stack_removal",
                "straddle",
                "nolook_allin",
                "run_it_multi",
                "random_sit_in",
                "anonymous",
                "community_ban",
                "spectator_ban",
                "device_ban"
            };

            key_active_group = new Dictionary<string, string>();
            key_active_group.Add("tb2", "tb2");
            key_active_group.Add("rake", "rake");
            key_active_group.Add("extra_blind_option", "extra_blind_option");
            key_active_group.Add("passive_rake_select", "passive_rake_select2");
            key_active_group.Add("stack_removal_select", "stack_removal_select2");
            key_active_group.Add("insurance", "insurance");
            key_active_group.Add("passive_rake", "passive_rake_selects");
            key_active_group.Add("stack_removal", "stack_removal_selects");

            key_active_group_reverse = new Dictionary<string, string>();
            key_active_group_reverse.Add("passive_rake_select", "passive_rake_select1");
            key_active_group_reverse.Add("stack_removal_select", "stack_removal_select1");
        }

        List<CustomUI> disables = new List<CustomUI>();

        for (int i = 0; i < properties.Length; i++) //값 설정부분
        {
            string key = properties[i].Name;
            JToken value = properties[i].Value;
            List<CustomUI> list = root.GetChildUIsWithKey(key);
            if (!edit_mode)
            {
                string value_str = "";
                if (System.Array.IndexOf(booleanArray, key) != -1)
                    value_str = (int)value == 0 ? "OFF" : "ON";
                else
                {
                    switch (key)
                    {
                        case "ante": // 칩을 BB 로 변경.
                        case "buyin_min":
                        case "buyin_max":
                            if ((long)value == 1000000000)
                            {
                                value_str = value.ToString();
                            }
                            else
                            {
                                value_str = (
                                    (float)value / o.ValueOrDefault<long>("blind", 1)
                                ).ToString();
                            }

                            break;
                        default:
                            value_str = value.ToString();
                            break;
                    }
                }

                list.ForEach(item => item.SetValue(value_str));

                if (key_active_group.ContainsKey(key))
                {
                    List<CustomUI> groups = root.GetGroup(key_active_group[key]);
                    bool active = false;
                    if (value.Type == JTokenType.Integer)
                        active = (int)value != 0;
                    else if (value.Type == JTokenType.Boolean)
                        active = (bool)value;
                    if (!active)
                        disables.AddRange(groups);
                }
                if (key_active_group_reverse.ContainsKey(key))
                {
                    List<CustomUI> groups = root.GetGroup(key_active_group_reverse[key]);
                    bool active = false;
                    if (value.Type == JTokenType.Integer)
                        active = (int)value != 0;
                    else if (value.Type == JTokenType.Boolean)
                        active = (bool)value;
                    if (active)
                        disables.AddRange(groups);
                }
            }
            else
            {
                if (key == "ante") // 칩을 BB 로 변경.
                    value = (float)value / o.ValueOrDefault<long>("blind", 1);
                if (key == "buyin_min")
                {
                    long blind = o.ValueOrDefault<long>("blind", 1);
                    long buyin_min_value = o.ValueOrDefault("buyin_min", (long)value);
                    long buyin_max_value = o.ValueOrDefault("buyin_max", (long)value);

                    List<CustomUI> buyin = root.GetChildUIsWithKey("buyin");
                    buyin.ForEach(t =>
                    {
                        buyin_min_value /= blind;

                        if (buyin_max_value == 1000000000) { }
                        else
                        {
                            buyin_max_value /= blind;
                        }

                        JArray jarray = new JArray(
                            new long[2] { buyin_min_value, buyin_max_value }
                        );
                        t.SetValue(jarray);
                    });
                }

                list.ForEach(item => item.SetValue(value));
            }
        }
        disables.ForEach(item => item.gameObject.SetActive(false));
    }

    public void EnterTable(CustomUI ui, JObject data, JObject evt)
    {
        if (selectedGame == null)
            return;

        Packet p = new Packet((int)CPProtocol.CP_PLAY_GAME_ENTER);
        p.Add("gtn", (int)selectedGame.info["gtn"]);

        WebSocketManager.defaultCli.Send(p.ToJson());
        CloseUI();
    }

    public void EditTable(CustomUI ui, JObject data, JObject evt)
    {
        if (selectedGame == null)
            return;

        JObject obj = root.GetJObject();
        string game_type = GAME_TYPE.nlh.ToString();
        if (evt.ContainsKey("game_type"))
        {
            game_type = evt["game_type"].ToString();
        }

        GAME_TYPE gametype = (GAME_TYPE)System.Enum.Parse(typeof(GAME_TYPE), game_type);

        long bb = (long)obj["blind"];
        // obj["game_type"] = (int)System.Enum.Parse(typeof(GAME_TYPE), game_type); // GAME_TYPE 수정할수 없음. 수정할 필요 없음.
        obj["buyin_min"] = (long)obj["buyin"][0] * bb;
        obj["buyin_max"] = (long)obj["buyin"][1] * bb;
        obj["cafeIdx"] = Cafe.instance.curEnterCafeInfo["cafe"]["idx"];

        switch (gametype)
        {
            case GAME_TYPE.short_deck:
                break;
            default:
                obj["ante"] = (long)((double)obj["ante"] * bb);
                break;
        }

        obj.Remove("buyin");
        StartCoroutine(GameEditFlow(obj));
        CloseUI();
    }

    private IEnumerator GameEditFlow(JObject gameData)
    {
        Packet p = new Packet(CPProtocol.CP_CAFE_GAME_UPDATE);
        p.Add("o", gameData);
        p.Add("gtn", selectedGame.info["gtn"]);
        p.Add("cafeIdx", selectedGame.info["cafeIdx"]);
        WebSocketManager.defaultCli.Send(p);
        WaitForPCProtocol wait = new WaitForPCProtocol(PCProtocol.PC_CAFE_GAME_UPDATE);
        yield return wait;
        if (wait.Result.c.ValueOrDefault("ecode", 0) == 0)
        {
            NormalMessage.instance.OnOneButtonMessagePopUp("table_edit_success");
        }
        else
        {
            NormalMessage.instance.OnOneButtonMessagePopUp("table_edit_fail");
        }
    }

    public void DeleteTable(CustomUI ui, JObject data, JObject evt)
    {
        if (selectedGame == null)
            return;

        StartCoroutine(GameDeleteFlow());
        CloseUI();
    }

    private IEnumerator GameDeleteFlow()
    {
        Packet p = new Packet(CPProtocol.CP_ROOM_DELETE);
        p.Add("gtn", selectedGame.info["gtn"]);
        p.Add("cafeIdx", selectedGame.info["cafeIdx"]);
        WebSocketManager.defaultCli.Send(p);
        WaitForPCProtocol wait = new WaitForPCProtocol(
            (protocol, c) =>
                protocol == PCProtocol.PC_ROOM_DELETE || protocol == PCProtocol.PC_ROOM_DELETE_FAIL
        );
        yield return wait;
        if (wait.Result.p == (int)PCProtocol.PC_ROOM_DELETE)
            NormalMessage.instance.OnOneButtonMessagePopUp("table_delete_success");
        else
            NormalMessage.instance.OnOneButtonMessagePopUp("table_delete_fail");
    }

    public void ShowOnlyInPlay(CustomUI ui, JObject data, JObject evt)
    {
        // ui.gameObject.SetActive(selectedGameIsPlayTabItem);

        CAFE_MEMBER_PERMIT myPermit = Cafe.instance.permit;
        ui.gameObject.SetActive(
            selectedGameIsPlayTabItem || (myPermit == CAFE_MEMBER_PERMIT.member)
        );
    }

    public void ShowOnlyInCafe(CustomUI ui, JObject data, JObject evt)
    {
        // ui.gameObject.SetActive(!selectedGameIsPlayTabItem);

        CAFE_MEMBER_PERMIT myPermit = Cafe.instance.permit;
        ui.gameObject.SetActive(
            !selectedGameIsPlayTabItem
                && (myPermit == CAFE_MEMBER_PERMIT.manager || myPermit == CAFE_MEMBER_PERMIT.owner)
        );
    }

    public void BackButtonSet(CustomUI ui, JObject data, JObject evt)
    {
        bool isDefault = true;
        if (evt != null && evt.ContainsKey("on_click"))
        {
            isDefault = evt["on_click"].ToString() == "default";
        }
        if (isDefault)
        {
            backButton.onClick = originBackButtonEvent;
        }
        else
        {
            backButtonEvents = evt["on_click"] as JArray;
            backButton.onClick = subBackButtonEvent;
        }
    }

    public void BackButtonActive(CustomUI ui, JObject data, JObject evt)
    {
        if (backButton)
            backButton.gameObject.SetActive(evt.ValueOrDefault("active", backButtonOriginActive));
    }

    private void OnClickBackButton()
    {
        for (int i = 0; i < backButtonEvents.Count; i++)
        {
            JObject evt = (backButtonEvents[i] as JObject);
            string key = evt.ValueOrDefault("event", "");
            root.events[key].Invoke(null, null, evt);
        }
    }

    public void PopupOpen(CustomUI ui, JObject data, JObject evt)
    {
        if (popup_opener)
            popup_opener.ShowUI(evt["param"].ToString());
    }

    public override void ShowUI(string scene)
    {
        if (backButton)
        {
            //씬 로드할때마다 백버튼 기능 초기화
            BackButtonSet(null, null, null);
        }
        base.ShowUI(scene);
    }

    public void CalculateValue(CustomUI ui, JObject data, JObject evt)
    {
        JObject obj = root.GetJObject();
        string formula = ReplaceFormulaString(obj, evt.ValueOrDefault("formula", ""));
        string value = calculator.Compute(formula, null).ToString();

        string target = evt.ValueOrDefault("target", "");
        if (target == "")
        {
            ui.SetValue(value);
        }
        else
        {
            root.GetGroup(target).ForEach(u => u.SetValue(value));
        }
    }

    public void ConditionActive(CustomUI ui, JObject data, JObject evt)
    {
        JObject obj = root.GetJObject();
        string formula = ReplaceFormulaString(obj, evt.ValueOrDefault("formula", ""));

        bool active = (bool)calculator.Compute(formula, null);

        string target = evt.ValueOrDefault("target", "");
        if (target == "")
        {
            ui.gameObject.SetActive(active);
        }
        else
        {
            root.GetGroup(target).ForEach(u => u.gameObject.SetActive(active));
        }
    }

    public void OverrideCafeData(CustomUI ui, JObject data, JObject evt)
    {
        if (Cafe.instance.curEnterCafeInfo == null)
            return;
        JObject obj = root.GetJObject();
        JObject cafeData = Cafe.instance.curEnterCafeInfo["cafe"] as JObject;
        Debug.Log(cafeData.ToString());
        foreach (JProperty prop in obj.Properties())
        {
            root.GetChildUIsWithKey(prop.Name).ForEach(u => u.SetValue(cafeData[prop.Name]));
        }
        int icon = cafeData.ValueOrDefault("icon", 0);
        if (icon != 0)
            logoSelector.OnClickSetLogoButton(icon);
        else
            logoSelector.SetPhoto(
                cafeData.ValueOrDefault("photoUrl", ""),
                cafeData.ValueOrDefault("idx", "0")
            );
    }

    private void CP_CAFE_UPDATE(CustomUI ui, JObject data, JObject evt)
    { // cafeIdx, o
        // JObject obj = root.GetJObject();

        JObject obj = new JObject();
        JObject o = root.GetJObject();
        o["icon"] = logoSelector.selected;

        obj.Add("cafeIdx", Cafe.instance.curEnterCafeInfo["cafe"]["idx"]);

        string name = o.ValueOrDefault("name", "");
        if (name == "")
        {
            Debug.LogError("카페 이름칸이 비어있습니다.");
            ErrorMessageManager.Instance.AddGameError(
                0,
                "please_input_cafe_name_title",
                "please_input_cafe_name_contents"
            );
            //카페 이름을 입력해주세요 팝업 표시
            return;
        }
        else
        {
            bool validCafeName = Regex.IsMatch(name, @"[a-zA-Z가-힣0-9 ]{2,16}$");
            if (!validCafeName)
            {
                ErrorMessageManager.Instance.AddGameError(
                    0,
                    "cant_use_cafename_title",
                    "cant_use_cafename_contents"
                );
                //카페 이름을 입력해주세요 팝업 표시
                return;
            }
        }

        if (logoSelector.selected == 0)
        {
            Texture2D texture = logoSelector.photo;
            texture = texture.DeCompress();
            byte[] bytes = texture.EncodeToPNG();
            string base64String = System.Convert.ToBase64String(bytes);
            PublisherApiManager.Instance.UploadCafeLogo(
                0,
                base64String,
                (statusCode, result) =>
                {
                    if (statusCode == 200)
                    {
                        o["photourl"] = result.ValueOrDefault("url", "");
                    }
                    obj.Add("o", o);
                    WebSocketManager.defaultCli.Send(new Packet(CPProtocol.CP_CAFE_UPDATE, obj));
                    StartCoroutine(WaitCafeUpdate());
                }
            );
        }
        else
        {
            obj.Add("o", o);
            WebSocketManager.defaultCli.Send(new Packet(CPProtocol.CP_CAFE_UPDATE, obj));
            StartCoroutine(WaitCafeUpdate());
        }
    }

    IEnumerator WaitCafeUpdate()
    {
        LoadingCircle.Instance.StartSpin();

        WaitForPCProtocol wait = new WaitForPCProtocol(PCProtocol.PC_CAFE_UPDATE);
        yield return wait;

        CloseUI();
        LoadingCircle.Instance.StopSpin();
    }

    private void CP_CAFE_RESERVE_REMOVE(CustomUI ui, JObject data, JObject evt)
    {
        JObject obj = new JObject();
        obj.Add("cafeIdx", Cafe.instance.curEnterCafeInfo["cafe"]["idx"]);
        obj.Add("reserveRemove", evt.ValueOrDefault("reserveRemove", 0));
        WebSocketManager.defaultCli.Send(new Packet(CPProtocol.CP_CAFE_RESERVE_REMOVE, obj));
        StartCoroutine(WaitCafeReserveRemove());
    }

    IEnumerator WaitCafeReserveRemove()
    {
        LoadingCircle.Instance.StartSpin();

        WaitForPCProtocol wait = new WaitForPCProtocol(PCProtocol.PC_CAFE_UPDATE);
        yield return wait;
        CloseUI();
        LoadingCircle.Instance.StopSpin();
    }

    private string ReplaceFormulaString(JObject obj, string formula)
    {
        if (formula.Contains("{balance}"))
            formula = formula.Replace(
                "{balance}",
                Cafe.instance.curEnterCafeInfo["cafeMember"]["cc"].ToString()
            );
        if (formula.Contains("{debt}"))
            formula = formula.Replace(
                "{debt}",
                Cafe.instance.curEnterCafeInfo["cafeMember"]["debt"].ToString()
            );
        if (formula.Contains("{reserveRemove}"))
            formula = formula.Replace(
                "{reserveRemove}",
                Cafe.instance.curEnterCafeInfo["cafe"]["reserveRemove"].ToString()
            );

        List<string> keys = new List<string>();
        bool started = false;
        string temp = "";
        for (int i = 0; i < formula.Length; i++)
        {
            if (formula[i] == '{')
                started = true;
            else if (formula[i] == '}')
            {
                started = false;
                keys.Add(temp);
                temp = "";
            }
            else if (started)
            {
                temp += formula[i];
            }
        }
        keys.ForEach(k =>
        {
            formula = formula.Replace("{" + k + "}", obj.ValueOrDefault(k, "0"));
        });
        return formula;
    }

    private void Logout(CustomUI ui, JObject data, JObject evt)
    {
        LobbyManager.Instance.OnClickLogOutButton();
    }

    private void ChangeNick(CustomUI ui, JObject data, JObject evt)
    {
        if (changeNickPanel)
            changeNickPanel.SetActive(true);
    }

    private void OpenUrl(CustomUI ui, JObject data, JObject evt)
    {
        Application.OpenURL(evt.ValueOrDefault("url", ""));
    }

    private void Version(CustomUI ui, JObject data, JObject evt)
    {
        ui.SetValue(Application.version);
    }

    private void Account_email(CustomUI ui, JObject data, JObject evt)
    {
        ui.SetValue(LoginManager.instance.email);
    }

    public void SetPush(CustomUI ui, JObject data, JObject evt)
    {
        this.setting_push = true;

        if (MyStatus.push == null)
        {
            MyStatus.push = new JObject();
            MyStatus.push.Add("mute_all_push", true);
            MyStatus.push.Add("nightly_noti_push", true);
            MyStatus.push.Add("cafe_noti_push", true);
            MyStatus.push.Add("approve_push", true);
            MyStatus.push.Add("chip_approve_push", true);
            MyStatus.push.Add("create_delete_push", true);
            MyStatus.push.Add("tournament_push", true);
            MyStatus.push.Add("stop_operation_push", true);
            MyStatus.push.Add("approve_push_2", true);
            MyStatus.push.Add("chip_push", true);
            MyStatus.push.Add("stop_operation_owner_push", true);
        }

        JProperty[] properties = MyStatus.push.Properties().ToArray();
        for (int i = 0; i < properties.Length; i++) //값 설정부분
        {
            string key = properties[i].Name;
            JToken value = properties[i].Value;

            List<CustomUI> list = root.GetChildUIsWithKey(key);
            list.ForEach(item => item.SetValue(value));
        }

        this.setting_push = false;
    }

    public void EditPush(CustomUI ui, JObject data, JObject evt)
    {
        if (this.setting_push == true)
            return; // ui setting 중에는 업데이트 할필요 없다.

        JObject obj = root.GetJObject();

        if (JToken.DeepEquals(obj, MyStatus.push))
            return; // 같으면 보낼필요 없다.

        Packet packet = new Packet(CPProtocol.CP_SET_NOTIFICATION);
        packet.Add("push", obj);
        WebSocketManager.defaultCli.Send(packet);

        MyStatus.push = obj;
    }

    private void CP_TNMT_CREATE(CustomUI ui, JObject data, JObject evt)
    {
        JObject obj = root.GetJObject();

        //string game_type = GAME_TYPE.nlh.ToString();
        //if (evt.ContainsKey("game_type"))
        //{
        //    game_type = evt["game_type"].ToString();
        //}
        //obj["game_type"] = (int)System.Enum.Parse(typeof(GAME_TYPE), game_type);
        int type = 1; // 1은 MTT / 2는 SNG
        if (evt.ContainsKey("t_type"))
        {
            string t_type = evt["t_type"].ToString();
            if (t_type == "mtt")
            {
                type = 1;
            }
            else if (t_type == "sng")
            {
                type = 2;
            }
            else
            {
                type = 1;
                Debug.LogWarning("t_type 값이 잘못 설정됨");
            }
        }
        obj["t_type"] = type;

        obj["t_close_time_min"] = (int)obj["t_close_time"]; //t_close_time에는 DateTime, t_close_time_min에는 int값으로 분단위 나오게 하기위함
        string game_type = GAME_TYPE.nlh.ToString();
        if (obj.ContainsKey("t_game_type"))
        {
            game_type = obj["t_game_type"].ToString();
        }
        obj["t_game_type"] = (int)System.Enum.Parse(typeof(GAME_TYPE), game_type);

        obj["t_min_player"] = (long)obj["t_min_max_player"][0];
        obj["t_max_player"] = (long)obj["t_min_max_player"][1];

        if (obj.ContainsKey("t_blind_up_structure"))
        {
            string bus = obj["t_blind_up_structure"].ToString();

            if (bus == "normal")
            {
                obj["t_blind_up_structure"] = 1;
            }
            else if (bus == "turbo")
            {
                obj["t_blind_up_structure"] = 1;
            }
            else if (bus == "hyper")
            {
                obj["t_blind_up_structure"] = 1;
            }
            else
            {
                obj["t_blind_up_structure"] = 1;
                Debug.LogWarning("t_blind_up_structure 값이 잘못 설정됨");
            }
        }
        System.DateTime curTime = System.DateTime.UtcNow;
        string t_open_time = obj.ValueOrDefault("t_open_time", "");
        string t_start_time = obj.ValueOrDefault("t_start_time", "");

        //   t_open_time은 curTime 보다 적어도 5분 늦어야 하고
        //   t_start_time은 curTime 보다 적어도 10분 늦어야 한다.

        System.DateTime parsedOpenTime;
        System.DateTime parsedStartTime;

        // t_open_time 파싱
        if (System.DateTime.TryParse(t_open_time, out parsedOpenTime))
        {
            parsedOpenTime = parsedOpenTime.ToUniversalTime();
            if (parsedOpenTime <= curTime.AddMinutes(3))
            {
                parsedOpenTime = curTime.AddMinutes(3);
                t_open_time = parsedOpenTime.ToString("MM/dd/yyyy HH:mm:ss"); // 또는 원하는 포맷으로 변경
                obj["t_open_time"] = t_open_time;
            }
        }
        else
        {
            Console.Error("Invalid t_open_time format.");
        }

        // t_start_time 파싱
        if (System.DateTime.TryParse(t_start_time, out parsedStartTime))
        {
            parsedStartTime = parsedStartTime.ToUniversalTime();
            if (parsedStartTime <= curTime.AddMinutes(5))
            {
                parsedStartTime = curTime.AddMinutes(5);
                t_start_time = parsedStartTime.ToString("MM/dd/yyyy HH:mm:ss"); // 또는 원하는 포맷으로 변경
                obj["t_start_time"] = t_start_time;
            }
        }
        else
        {
            Console.Error("Invalid t_start_time format.");
        }

        //    ///////////////////////////////////////////////


        double late_reg_time_turm = obj.ValueOrDefault("t_close_time", 0);

        System.DateTime lateRegTime = parsedOpenTime.AddMinutes(late_reg_time_turm);

        obj["t_close_time"] = lateRegTime.ToString("yyyy'/'MM'/'dd HH:" + "mm:ss");
        if (!obj.ContainsKey("t_start_time"))
            obj["t_start_time"] = obj["t_close_time"];
        obj.Remove("guarantee");
        obj.Remove("tb2");
        obj.Remove("t_min_max_player");
        obj.Add("t_chip_type", (int)CHIP_TYPE.cc);
        StartCoroutine(TnmtCreateFlow(obj));
        CloseUI();
    }

    private IEnumerator TnmtCreateFlow(JObject gameData)
    {
        Packet p = new Packet(CPProtocol.CP_TNMT_CREATE);
        p.Add("cafeIdx", Cafe.instance.curEnterCafeInfo["cafe"]["idx"]);
        p.Add("o", gameData);
        WebSocketManager.defaultCli.Send(p);
        WaitForPCProtocol wait = new WaitForPCProtocol(PCProtocol.PC_TNMT_CREATE);
        yield return wait;
        if (wait.Result.c.ValueOrDefault("ecode", 0) == 0)
            NormalMessage.instance.OnOneButtonMessagePopUp("table_success");
        else
            NormalMessage.instance.OnOneButtonMessagePopUp("table_fail");
    }

    private void CP_TNMT_APPLY(CustomUI ui, JObject data, JObject evt)
    {
        //LoadingCircle.Instance.StartSpin(CPProtocol.CP_TNMT_UNAPPLY);
        //TryApply(selectedTnmt.info.ValueOrDefault("tn",0), selectedTnmt.info.ValueOrDefault("cafeIdx", 0));
        Cafe.instance.TnmtApplyPopup.SetTnmtData(selectedTnmt.tn);
        CloseUI();
    }

    private async void TryApply(int tn, int cafeIdx)
    {
        Packet p = new Packet(CPProtocol.CP_TNMT_APPLY);
        p.Add("tn", tn);
        p.Add("cafeIdx", cafeIdx);
        p.Add("ticket", 0);
        p.Add("rebuy", 0);
        int scale = root.GetChildUIWithKey("scale").GetValue().ToObject<int>();
        p.Add("scale", scale);
        //p.Add("double", 0);
        WebSocketManager.defaultCli.Send(p);
        WaitForPCProtocol wait = new WaitForPCProtocol(
            PCProtocol.PC_TNMT_APPLY,
            PCProtocol.PC_TNMT_APPLY_FAIL
        );
        await wait;
        if (wait.Result.c.ValueOrDefault("ecode", 0) == 0)
        {
            NormalMessage.instance.OnOneButtonMessagePopUp("confirm_success");
            //Packet enter = new Packet(CPProtocol.CP_PLAY_GAME_ENTER);
            //enter.Add("gtn", wait.Result.c.ValueOrDefault("gtn",0));
            //WebSocketManager.defaultCli.Send(enter);
            //WaitForPCProtocol enter_wait = new WaitForPCProtocol(PCProtocol.PC_PLAY_GAME_ENTER);
            //yield return enter_wait;

            LoadingCircle.Instance.StopSpin();
            Cafe.instance.GetCafeInfo(cafeIdx);
            CloseUI();
            //ShowUI(GetCurrentScene());
        }
        else
        {
            LoadingCircle.Instance.StopSpin();
            NormalMessage.instance.OnOneButtonMessagePopUp(
                wait.Result.c.ValueOrDefault("message", "tnmt_apply_fail")
            );
        }
    }

    private async void TnmtRoomEnter(CustomUI ui, JObject data, JObject evt)
    {
        LoadingCircle.Instance.StartSpin();
        Packet enter = new Packet(CPProtocol.CP_PLAY_GAME_ENTER);
        enter.Add("gtn", selectedTnmt.myTable);
        enter.Add("tn", selectedTnmt.tn);
        WebSocketManager.defaultCli.Send(enter);
        WaitForPCProtocol enter_wait = new WaitForPCProtocol(PCProtocol.PC_PLAY_GAME_ENTER);
        await enter_wait;
        CloseUI();
        LoadingCircle.Instance.StopSpin();
    }

    private void CP_TNMT_UNAPPLY(CustomUI ui, JObject data, JObject evt)
    {
        LoadingCircle.Instance.StartSpin(CPProtocol.CP_TNMT_UNAPPLY);
        StartCoroutine(
            TryUnApply(
                selectedTnmt.info.ValueOrDefault("tn", 0),
                selectedTnmt.info.ValueOrDefault("cafeIdx", 0)
            )
        );
    }

    private IEnumerator TryUnApply(int tn, int cafeIdx)
    {
        Packet p = new Packet(CPProtocol.CP_TNMT_UNAPPLY);
        p.Add("tn", tn);
        p.Add("cafeIdx", cafeIdx);
        WebSocketManager.defaultCli.Send(p);
        WaitForPCProtocol wait = new WaitForPCProtocol(
            PCProtocol.PC_TNMT_UNAPPLY,
            PCProtocol.PC_TNMT_UNAPPLY_FAIL
        );
        yield return wait;
        LoadingCircle.Instance.StopSpin();
        Cafe.instance.GetCafeInfo(cafeIdx);
        CloseUI();
        if (wait.Result.c.ValueOrDefault("ecode", 0) == 0)
            NormalMessage.instance.OnOneButtonMessagePopUp("confirm_success");
        else
            NormalMessage.instance.OnOneButtonMessagePopUp("table_fail");
    }

    public async void ShowTnmtInfo(CustomUI ui, JObject data, JObject evt)
    {
        if (selectedTnmt == null)
            return;
        bool edit_mode = evt.ValueOrDefault("edit_mode", true);
        JObject o = selectedTnmt.info;
        int ticket_type = 0;
        {
            var t_ticket = o.CastOrEmpty<JObject>("t_ticket");
            bool ticket_active = false;

            if (t_ticket != null)
            {
                ticket_type = t_ticket.ValueOrDefault("ticket_type", 0);
                ticket_active = ticket_type != 0;
            }
            var ticket_type_ui = root.GetChildUIWithKey("ticket_type");
            var ticket_count_ui = root.GetChildUIWithKey("ticket_count");
            if (ticket_active)
            {
                var ticket_string = await KingshillInfo.GetTicketString(ticket_type);
                ticket_type_ui.SetValue(LocalizeManager.GetLocalString(ticket_string));
                ticket_count_ui.SetValue(t_ticket.ValueOrDefault("ticket_count", 0));
            }
            else
            {
                root.RemoveUI(ticket_type_ui);
                root.RemoveUI(ticket_count_ui);
            }
        }
        {
            var t_rewards = o.CastOrEmpty<JObject>("t_rewards");

            bool r_ticket_active = false;
            int r_ticket_type = 0;
            if (t_rewards != null)
            {
                r_ticket_type = t_rewards.ValueOrDefault("ticket_in_rank", 0);
                r_ticket_active = r_ticket_type != 0;
            }
            var reward_ticket_type = root.GetChildUIWithKey("reward_ticket_type");
            var ticket_in_rank = root.GetChildUIWithKey("ticket_in_rank");
            if (r_ticket_active)
            {
                reward_ticket_type.SetValue($"ticket{r_ticket_type}");
                ticket_in_rank.SetValue(t_rewards.ValueOrDefault("ticket_in_rank", 0));
            }
            else
            {
                root.RemoveUI(reward_ticket_type);
                root.RemoveUI(ticket_in_rank);
            }
        }

        JsonMergeSettings jsonMergeSettings = new JsonMergeSettings();
        jsonMergeSettings.MergeArrayHandling = MergeArrayHandling.Merge;
        o.Merge(o["live"], jsonMergeSettings);

        //JObject obj = new JObject();
        //obj.Add("t_minmax_player", $"{o.ValueOrDefault<long>("t_min_player", 0)}/{o.ValueOrDefault<long>("t_max_player", 0)}");
        //obj.Add("mtt_ob_table_1", o["live"]["tables"] as JArray);
        //o.Merge(obj, jsonMergeSettings);

        JProperty[] properties = o.Properties().ToArray();
        string[] booleanArray = null;
        Dictionary<string, string> key_active_group = null;
        Dictionary<string, string> key_active_group_reverse = null;

        if (!edit_mode)
        {
            booleanArray = new string[] { "t_is_hide_nickname", "t_is_allow_chat" };

            key_active_group = new Dictionary<string, string>();

            key_active_group_reverse = new Dictionary<string, string>();
        }

        for (int i = 0; i < properties.Length; i++) //값 설정부분
        {
            string key = properties[i].Name;
            JToken value = properties[i].Value;
            List<CustomUI> list = root.GetChildUIsWithKey(key);
            if (!edit_mode)
            {
                string value_str = "";
                if (System.Array.IndexOf(booleanArray, key) != -1)
                    value_str = (int)value == 0 ? "OFF" : "ON";
                else
                {
                    switch (key)
                    {
                        case "ante": // 칩을 BB 로 변경.
                        case "buyin_min":
                        case "buyin_max":
                            if ((long)value == 1000000000)
                            {
                                value_str = value.ToString();
                            }
                            else
                            {
                                value_str = (
                                    (float)value / o.ValueOrDefault<long>("blind", 1)
                                ).ToString();
                            }

                            break;
                        case "t_blind_up_structure":
                            SetBlindUpTable(o);
                            break;
                        default:
                            if (value.Type == JTokenType.Date)
                            {
                                value_str = value
                                    .ToObject<System.DateTime>()
                                    .ToLocalTime()
                                    .ToString("yyyy'/'MM'/'dd'/' HH:mm:ss");
                            }
                            else
                            {
                                value_str = value.ToString();
                            }
                            break;
                    }
                }

                list.ForEach(item => item.SetValue(value_str));

                if (key_active_group.ContainsKey(key))
                {
                    List<CustomUI> groups = root.GetGroup(key_active_group[key]);
                    groups.ForEach(item => item.gameObject.SetActive((int)value != 0));
                }
                else if (key_active_group_reverse.ContainsKey(key))
                {
                    List<CustomUI> groups = root.GetGroup(key_active_group_reverse[key]);
                    groups.ForEach(item => item.gameObject.SetActive((int)value == 0));
                }
            }
            else
            {
                if (key == "mtt_blind_1")
                    value = TableDataManager.blindTables[
                        o.ValueOrDefault("t_blind_up_structure", 0)
                    ];
                if (key == "ante") // 칩을 BB 로 변경.
                    value = (float)value / o.ValueOrDefault<long>("blind", 1);
                if (key == "buyin_min")
                {
                    long blind = o.ValueOrDefault<long>("blind", 1);
                    long buyin_min_value = o.ValueOrDefault("buyin_min", (long)value);
                    long buyin_max_value = o.ValueOrDefault("buyin_max", (long)value);

                    List<CustomUI> buyin = root.GetChildUIsWithKey("buyin");
                    buyin.ForEach(t =>
                    {
                        buyin_min_value /= blind;

                        if (buyin_max_value == 1000000000) { }
                        else
                        {
                            buyin_max_value /= blind;
                        }

                        JArray jarray = new JArray(
                            new long[2] { buyin_min_value, buyin_max_value }
                        );
                        t.SetValue(jarray);
                    });
                }

                list.ForEach(item => item.SetValue(value));
            }
        }
        //root.GetChildUIWithKey("scale").gameObject.SetActive(false);
        root.GetChildUIWithKey("enter_request")?.gameObject.SetActive(false);
        root.GetChildUIWithKey("cancel_enter_request")?.gameObject.SetActive(false);
        root.GetChildUIWithKey("tnmt_room_enter")?.gameObject.SetActive(false);
        root.GetChildUIWithKey("tnmt_btn_space")?.gameObject.SetActive(false);
        root.GetChildUIWithKey("tnmt_cancel")?.gameObject.SetActive(false);

        root.GetChildUIWithKey("mtt_ob_table_1").SetValue(o["live"]["tables"]);
        root.GetChildUIWithKey("t_minmax_player")
            .SetValue(
                $"{o.ValueOrDefault<long>("t_min_player", 0)}/{o.ValueOrDefault<long>("t_max_player", 0)}"
            );

        RewardTableSet(ticket_type);
        StartCoroutine(TournamentApplyButtonToggle());
        StartCoroutine(CurrentRankSetting());
    }

    private void SetBlindUpTable(JObject o)
    {
        var blindTable = new JObject(
            TableDataManager.blindTables[o.ValueOrDefault("t_blind_up_structure", 0)]
        );
        var blind_up = blindTable.CastOrEmpty<JArray>("blind_up");
        var blindUpDurationTime = o.ValueOrDefault("t_blind_up_duration_time", 0);
        if (blindUpDurationTime > 0)
        {
            foreach (JObject data in blind_up)
            {
                if (!data.ValueOrDefault("is_breaktime", false))
                {
                    data["next_second"] = blindUpDurationTime;
                }
            }
        }
        var breakTime = o.ValueOrDefault("t_blind_up_break_time", 0);
        if (breakTime > 0)
        {
            foreach (JObject data in blind_up)
            {
                if (data.ValueOrDefault("is_breaktime", false))
                {
                    data["next_second"] = breakTime;
                }
            }
        }
        var module = root.GetChildUIWithKey("mtt_blind_1") as CustomUiBlindTableModule;

        module?.SetBlindLevel(o.ValueOrDefault("blind_level", 0));
        module?.SetValue(blindTable);
    }

    private IEnumerator TournamentApplyButtonToggle()
    {
        Packet p = new Packet(CPProtocol.CP_TNMT_MY);
        WebSocketManager.defaultCli.Send(p);
        WaitForPCProtocol wait = new WaitForPCProtocol(PCProtocol.PC_TNMT_MY);
        yield return wait;
        JArray tnmts = wait.Result.c["tournament"] as JArray;
        bool applied = false;
        for (int i = 0; i < tnmts.Count; i++)
        {
            var tnmt = tnmts[i] as JObject;
            if ((int)tnmt.ValueOrDefault("tn", 0) == selectedTnmt.info.ValueOrDefault("tn", -1))
            {
                applied = true;
                selectedTnmt.myTable = tnmt.ValueOrDefault("gtn", 0);
            }
        }

        if (!ingame)
        {
            if (
                selectedTnmt.state < (int)TNMT_FLOW.close
                && selectedTnmt.countAllUser < selectedTnmt.info.ValueOrDefault("t_max_player", 0)
            )
            {
                root.GetChildUIWithKey("enter_request")?.gameObject.SetActive(!applied);
                //root.GetChildUIWithKey("scale").gameObject.SetActive(!applied);
            }

            if (
                selectedTnmt.state < (int)TNMT_FLOW.end
                && DevOptionsManager.devOptions.mode == MODE.dev
            )
            {
                root.GetChildUIWithKey("tnmt_room_enter")?.gameObject.SetActive(applied);
            }
            if (selectedTnmt.state < (int)TNMT_FLOW.start)
            {
                root.GetChildUIWithKey("cancel_enter_request")?.gameObject.SetActive(applied);
            }

            var cafeidx = selectedTnmt.info.ValueOrDefault("cafeIdx", 0);
            var cafe = Cafe.instance.GetCafeData(cafeidx);

            // 카페 정보/멤버가 누락이면 한 번 재요청해서 받아온다.
            bool needRefetch = cafe == null
                            || cafe.info == null
                            || cafe.cafeMembers == null
                            || cafe.cafeMembers.Count == 0;

            if (needRefetch && cafeidx > 0)
            {
                Cafe.instance.GetCafeInfo(cafeidx);
                var cafeWait = new WaitForPCProtocol(PCProtocol.PC_CAFE_INFO);
                yield return cafeWait;
                cafe = Cafe.instance.GetCafeData(cafeidx);
            }

            // 재요청 후에도 정보가 없으면 권한 의존 버튼을 모두 끄고 종료 — 잘못된 버튼이 노출되는 것보다 안전.
            if (cafe == null || cafe.cafeMembers == null || cafe.cafeMembers.Count == 0)
            {
                ResetTnmtAuthButtons();
                yield break;
            }

            try
            {
                var permit = (CAFE_MEMBER_PERMIT)cafe.cafeMembers[0].permit;
                if (
                    selectedTnmt.state < (int)TNMT_FLOW.cancel
                    && (permit == CAFE_MEMBER_PERMIT.manager || permit == CAFE_MEMBER_PERMIT.owner)
                )
                {
                    root.GetChildUIWithKey("tnmt_cancel")?.gameObject.SetActive(true);
                    root.GetChildUIWithKey("tnmt_btn_space")?.gameObject.SetActive(true);
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[TournamentApplyButtonToggle] permit eval failed: {ex}");
                ResetTnmtAuthButtons();
            }
        }
    }

    // Directive: 가드를 모두 통과하지 못한 경우 권한·참가 의존 버튼을 모두 OFF — "잘못 노출되는 것 < 안 보이는 것".
    private void ResetTnmtAuthButtons()
    {
        root.GetChildUIWithKey("enter_request")?.gameObject.SetActive(false);
        root.GetChildUIWithKey("cancel_enter_request")?.gameObject.SetActive(false);
        root.GetChildUIWithKey("tnmt_room_enter")?.gameObject.SetActive(false);
        root.GetChildUIWithKey("tnmt_btn_space")?.gameObject.SetActive(false);
        root.GetChildUIWithKey("tnmt_cancel")?.gameObject.SetActive(false);
    }

    private void RewardTableSet(int ticket_type = 0)
    {
        var info = selectedTnmt.info;
        var t_rewards = info.CastOrEmpty<JObject>("t_rewards");

        var t_reward_all_chip = info.ValueOrDefault<long>("t_reward_all_chip", 0);
        var accrue_buyin = info.ValueOrDefault<long>("accrue_buyin", 0);
        var countAllUser = info.ValueOrDefault("countAllUser", 0);

        var totalPrize = t_reward_all_chip > accrue_buyin ? t_reward_all_chip : accrue_buyin;
        var module = root.GetChildUIWithKey("mtt_reward_1") as CustomUIRewardModule;
        var countEntry = info.ValueOrDefault("countEntry", 0);
        module.SetValue(t_rewards, countAllUser, countEntry, totalPrize, ticket_type);
        //module.SetValue(reward_rank_table - 1, countAllUser, totalPrize, ticket_type);
    }

    private IEnumerator CurrentRankSetting()
    {
        Packet p = new Packet(CPProtocol.CP_TNMT_RANKING);
        p.Add("tn", selectedTnmt.info["tn"]);
        WebSocketManager.defaultCli.Send(p);
        WaitForPCProtocol wait = new WaitForPCProtocol(PCProtocol.PC_TNMT_RANKING);
        yield return wait;
        var my = wait.Result.c.CastOrEmpty<JObject>("my");
        //if(my != null)
        //{
        //    root.GetChildUIWithKey("rank")?.SetValue(my.ValueOrDefault("rank", "-"));
        //}
        var c = wait.Result.c;
        c.Add("cafeIdx", selectedTnmt.info.ValueOrDefault("cafeIdx", 0));
        {
            root.GetChildUIWithKey("mtt_list_1")?.SetValue(c);
        }
    }

    private void ShowTnmtResult(CustomUI ui, JObject data, JObject evt)
    {
        int rank = tnmtResult.ValueOrDefault("rank", -1);
        root.GetChildUIWithKey("rank_data").SetValue(rank);
        root.GetChildUIWithKey("my_rank").SetValue(rank);
        root.GetChildUIWithKey("my_money").SetValue(tnmtResult.ValueOrDefault("reward", 0));
    }
}
