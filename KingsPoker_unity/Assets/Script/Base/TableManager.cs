using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class TableManager : WebsocketListenBehaviour
{
    protected long roomNumber;
    protected GAME_TYPE game_type = GAME_TYPE.nothing;
    protected CHIP_TYPE chipType = CHIP_TYPE.nothing;
    public bool activeTable = false;
    public bool activeOn = false;

    [SerializeField]
    protected List<Variation> chipTypeVariations;

    public virtual void SetRoomNumber(long gtn)
    {
        roomNumber = gtn;

        var roomData = InfoManager.Instance.GetRoom(roomNumber);
        SetChipType(roomData.chip_type);
    }

    public long GetRoomNumber()
    {
        return roomNumber;
    }

    public GAME_TYPE GetGameType()
    {
        return game_type;
    }

    public string GetTableVariationString()
    {
        int table_color = 0;
        string variationString = string.Empty;
        if (game_type == GAME_TYPE.mtt)
        {
            int tn = InfoManager.Instance.GetRoom(roomNumber).tn;
            var tnmtinfo = InfoManager.Instance.GetTournamentInfo(tn);
            var t_o = tnmtinfo.info.CastOrEmpty<JObject>("t_o");
            table_color = t_o.ValueOrDefault("table_color", 0);
        }
        if (table_color > 0)
        {
            variationString = $"table_{table_color:00}";
        }
        else
        {
            variationString = game_type.ToString();
        }
        return variationString;
    }

    public void LeaveTheRoom() { }

    public virtual void SetGameType(GAME_TYPE gameType)
    {
        game_type = gameType;
    }

    public virtual void SetChipType(CHIP_TYPE chipType)
    {
        this.chipType = chipType;
        chipTypeVariations.ForEach((obj) => obj.SetVariation(chipType.ToString()));
    }

    protected void Start()
    {
        SoundManager.Instance.PlayEffectSound(Sound_TableEnum.SFX_GAMEIN);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        SoundManager.Instance.PlayEffectSound(Sound_TableEnum.SFX_GAMEOUT);
    }
}
