using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class BettingIconInfo {
    public POKER_BETTYPE bettype;
    public Sprite onSprite = null;
    public Sprite offSprite = null;
    
}
[CreateAssetMenu(fileName = "BettingIconData", menuName = "ScriptableObject/BettingIconData")]
public class BettingIconData : ScriptableObject
{
    public List<BettingIconInfo> spriteInfo = new List<BettingIconInfo>();


    public Sprite GetIcon(POKER_BETTYPE bettype,bool on = true) {
        BettingIconInfo iconData = spriteInfo.Find(delegate(BettingIconInfo item) {
            return item.bettype == bettype;
        });

        return on ? iconData.onSprite : iconData.offSprite;
    }
}