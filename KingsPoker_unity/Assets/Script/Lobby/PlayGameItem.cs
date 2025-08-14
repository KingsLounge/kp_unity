using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PlayGameItem : MonoBehaviour
{
    public PlayGameInfo playGameInfo;

    public PlayGameItemGuage playGameItemGuage;
    public Text txt_game_title;
    public Text txt_game_type;
    public Text txt_gtn;

    public Text txt_cafe_name;
    public GameObject obj_cafe_stop;
    public Text txt_blind;
    public Text txt_buyin;
    public Text txt_cafeMember_dc;
    public Image img_cafeMember_dc;

    public Text txt_cafeIdx;
    public Text txt_averPot;
    public Image chip_icon;

    public GameObject borderLine = null;
    public List<Variation> chipTypeVariation;
    public List<Variation> permitVariations = new List<Variation>();
    public List<Variation> gameTypeVariations = new List<Variation>();

    public CustomUIOpener customUIOpener;
    private GAME_TYPE gameType;
    private CHIP_TYPE chipType;
    public bool isPlayTabItem = false;

    private void Awake() { }

    private void OnDestroy()
    {
        // LobbyManager.Instance.roomUpdate.RemoveListener(UpdateCanJoin);
    }

    public void Set(PlayGameInfo info, int cafeIdx = -1)
    {
        playGameInfo = info;

        playGameItemGuage.Set(playGameInfo);

        int game_type = (int)playGameInfo.info["game_type"];
        chipType = playGameInfo.info.ValueOrDefault<CHIP_TYPE>("chip_type", CHIP_TYPE.nothing);

        for (int i = 0; i < gameTypeVariations.Count; i++)
        {
            gameTypeVariations[i].SetVariation(((GAME_TYPE)game_type).ToString());
        }
        if (txt_game_title)
        {
            var title = playGameInfo.info.ValueOrDefault("title", string.Empty);
            txt_game_title.gameObject.SetActive(!string.IsNullOrEmpty(title));
            txt_game_title.text = playGameInfo.info.ValueOrDefault("title", string.Empty);
        }
        txt_game_type.text = LocalizeManager.GetLocalString(((GAME_TYPE)game_type).ToString());
        gameType = (GAME_TYPE)game_type;
        txt_gtn.text = "#" + (string)playGameInfo.info["gtn"];

        txt_cafe_name.text = cafeIdx == -1 ? (string)playGameInfo.info["cafe_name"] : ""; // cafeIdx != -1 이면 cafeGameList 이다. 여기서는 cafe_name 을 출력하지 않는다.
        if (chip_icon)
        {
            chip_icon.gameObject.SetActive(cafeIdx == -1); // 특정 카페에 들어가 있지 않으면,
        }
        txt_averPot.gameObject.SetActive(true);
        string str_txt_averPot = "no infomation yet";
        if (playGameInfo.info.ContainsKey("log")) // 없을때도 있다.
        {
            JObject log = playGameInfo.info["log"] as JObject;

            if (log.ContainsKey("c"))
            {
                int c = log.ValueOrDefault<int>("c", 0);

                JArray pot = log["pot"] as JArray;
                JArray flop = log["flop"] as JArray;
                JArray hhrs = log["hhrs"] as JArray;

                long avg_pot = 0;
                long avg_flop = 0;
                long avg_hhrs = 0;
                for (int i = 0; i < pot.Count; i++)
                {
                    avg_pot += (long)pot[i];
                    avg_flop += (long)flop[i];
                    avg_hhrs += (long)hhrs[i];
                }

                avg_pot /= pot.Count;
                avg_flop /= flop.Count;
                avg_hhrs /= hhrs.Count;

                str_txt_averPot = string.Format(
                    "Avg Pot:<color=#B0B0B0>{0:#,##0.##}</color>  Flop:<color=#B0B0B0>{1}%</color>  H/hrs:<color=#B0B0B0>{2}</color>",
                    avg_pot,
                    avg_flop,
                    avg_hhrs
                );
            }
        }
        txt_averPot.text = str_txt_averPot;
        foreach (var vari in chipTypeVariation)
        {
            vari.SetVariation(chipType.ToString());
        }

        // deprecated   2021-04-09 Alberto
        //long total_pot = log.ValueOrDefault<long>("total_pot", 0);
        //long play_count = (playGameInfo.info["log"] as JObject).ValueOrDefault<long>("play_count", 1);

        //txt_averPot.gameObject.SetActive(playGameInfo.info.ContainsKey("log"));
        //txt_averPot.text = LocalizeManager.GetLocalString("avg_pot") + " " + (total_pot / play_count).ToString();


        int ante = (int)playGameInfo.info["ante"];
        int sb = (int)playGameInfo.info["small_blind"];
        int bb = (int)playGameInfo.info["blind"];

        string str_blind = "";
        switch (gameType)
        {
            case GAME_TYPE.short_deck:
                str_blind = string.Format(
                    "{0} ({1})",
                    MoneyToString.Converting(bb),
                    MoneyToString.Converting(bb)
                );
                break;
            default:
                str_blind = string.Format(
                    "{0} / {1}",
                    MoneyToString.Converting(sb),
                    MoneyToString.Converting(bb)
                );
                if (ante > 0)
                {
                    str_blind += string.Format(" ({0})", MoneyToString.Converting(ante));
                }
                break;
        }

        txt_blind.text = str_blind;

        long buyin = (long)playGameInfo.info["buyin_min"];
        // txt_buyin.text = $"{buyin:#,##0.##}";
        txt_buyin.text = MoneyToString.Converting(buyin);

        CafeInfo cafeInfo = Cafe.instance.GetCafeList((int)playGameInfo.info["cafeIdx"]);
        if (cafeInfo != null)
        {
            long dc = (long)cafeInfo.cafeMembers[0].dc;
            txt_cafeMember_dc.text = cafeIdx == -1 ? $"{dc:#,##0.##}" : ""; // cafeIdx != -1 이면 cafeGameList 이다. 여기서는 cafe_name 을 출력하지 않는다.

            if (borderLine)
            {
                borderLine.SetActive(dc > 0);
            }
            if (cafeIdx == -1) // 특정 카페에 들어가 있지 않으면,
            {
                obj_cafe_stop.SetActive((long)cafeInfo.info["leftSeconds"] <= 0); // 시간이 모두 0이 되면 '시계+스톱' icon이 활성화된다.  Alberto 2021-04-25
            }
            else
            {
                obj_cafe_stop.SetActive(false);
            }

            txt_cafeIdx.text = "cafeIdx: " + playGameInfo.info["cafeIdx"];

            int permit = (int)
                Cafe.instance.GetCafeList((int)playGameInfo.info["cafeIdx"]).cafeMembers[0].permit;
            for (int i = 0; i < permitVariations.Count; i++)
            {
                permitVariations[i].SetVariation(((CAFE_MEMBER_PERMIT)permit).ToString());
            }
        }
    }

    public void UpdateGamePlayerCount(int playerCount)
    {
        playGameInfo.info["player_count"] = playerCount.ToString();
        playGameItemGuage.Set(playGameInfo);
    }

    //public virtual void SetEnable(bool en)
    //{
    //    GetComponent<Button>().interactable = en;
    //}


    public void PlayGameEnter()
    {
        if (GamesManager.instance.CanJoinInspection((int)playGameInfo.info["gtn"]))
        {
            //Packet p = new Packet((int)CPProtocol.CP_PLAY_GAME_ENTER);
            //p.Add("gtn", (int)playGameInfo.info["gtn"]);

            //WebSocketManager.defaultCli.Send(p.ToJson());
            if (customUIOpener == null)
            {
                Packet p = new Packet((int)CPProtocol.CP_PLAY_GAME_ENTER);
                p.Add("gtn", (int)playGameInfo.info["gtn"]);

                WebSocketManager.defaultCli.Send(p.ToJson());
            }
            else
            {
                (customUIOpener as CustomUIOpenerLobby).selectedGame = playGameInfo;
                (customUIOpener as CustomUIOpenerLobby).selectedGameIsPlayTabItem = isPlayTabItem;
                customUIOpener.ShowUI(gameType.ToString() + "_info");
            }
        }
    }

    public void OnClickEnterButton()
    {
        PlayGameEnter();
    }
}
