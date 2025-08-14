using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class BettingSoundInfo
{
    public POKER_BETTYPE bettype;
    public Sound_TableEnum soundType;
}

[CreateAssetMenu(fileName = "BettingSoundData", menuName = "BettingSoundData")]
public class BettingSoundData : ScriptableObject
{
    public List<BettingSoundInfo> spriteInfo = new List<BettingSoundInfo>();


    public Sound_TableEnum GetSoundType(POKER_BETTYPE bettype,bool on = true) {
        BettingSoundInfo iconData = spriteInfo.Find(delegate(BettingSoundInfo item) {
            return item.bettype == bettype;
        });


        if(iconData == null) // TODO:   없을 경우, 빈 사운드 enum 필요.
        {
            return Sound_TableEnum.SFX_BUTTON_01;
        }

        return iconData.soundType;
    }
}
