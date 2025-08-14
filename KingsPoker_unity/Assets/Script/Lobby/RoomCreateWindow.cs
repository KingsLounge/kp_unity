using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;
using Constant;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;

public class RoomCreateWindow : MonoBehaviour
{
    [SerializeField]
    private GameObject togglePrefab;

    [SerializeField]
    private List<CHIP_TYPE> chipTypes = new List<CHIP_TYPE>{CHIP_TYPE.dc};

    [SerializeField]
    private GAME_TYPE gameType = GAME_TYPE.nlh;

    [SerializeField]
    private Transform toggleParent;

    private List<JObject> options;

    private int selectLevel = 0;
    private int cafeIdx = 0;
    private CHIP_TYPE chipType = CHIP_TYPE.nothing;

    private void Awake()
    {
        init();
    }
    public void init()
    {
        options = JsonDataParser.Parse<List<JObject>>(RoomOptions.rules[gameType + "_list"]);
        bool first = true;
        for(int i = 0; i < options.Count; ++i)
        {
            var option = options[i];
            var chipType = option.ValueOrDefault("chip_type", CHIP_TYPE.nothing);
            if(chipTypes.Contains(chipType))
            {
                var obj = Instantiate(togglePrefab, toggleParent);
                var slot = obj.GetComponent<RoomCreateOptionSlot>();
                slot.SetSlot(option, (isOn) => {
                    if (isOn)
                    {
                        selectLevel = option.ValueOrDefault("level", 0);
                        Debug.Log(selectLevel);
                    }
                });
                
                
                obj.SetActive(true);
                if(first)
                {
                    var toggle = slot.optionToggle;
                    toggle.isOn = true;
                    first = false;
                }
            }
        }
    }
    public void SetCafeIdx(int idx)
    {
        cafeIdx = idx;
    }
    public void SetChipType(int chipType)
    {
        this.chipType = (CHIP_TYPE)chipType;
    }
    public void OnCreateButton()
    {
        CP_CAFE_GAME_CREATE(selectLevel);
    }

    private void CP_CAFE_GAME_CREATE(int optionLevel)
    {

        string game_type = GAME_TYPE.nlh.ToString();
        var option = JsonDataParser.Parse<List<JObject>>(RoomOptions.rules[game_type + "_list"])[optionLevel - 1];
        var obj = JObject.Parse(GameConfig.defaltGameOption);
        obj["game_type"] = (int)GAME_TYPE.nlh;
        obj["chip_type"] = (int)chipType;
        var rd = new RoomData();
        obj["ante"] = option["ante"];
        obj["buyin_min"] = option["buyin_min"];
        obj["buyin_max"] = option["buyin_max"];
        obj["blind"] = option["blind"];
        obj["small_blind"] = option["small_blind"];
        obj["cafeIdx"] = cafeIdx;
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
        GameCreateFlow(obj);
    }

    private async void GameCreateFlow(JObject gameData)
    {
        Packet p = new Packet(CPProtocol.CP_CAFE_GAME_CREATE);
        p.Add("o", gameData);

        
        WebSocketManager.defaultCli.Send(p);
        WaitForPCProtocol wait = new WaitForPCProtocol(PCProtocol.PC_CAFE_GAME_CREATE);
        await wait;
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
}
