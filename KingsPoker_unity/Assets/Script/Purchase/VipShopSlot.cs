using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;
using UnityEngine.U2D;
using UnityEngine.UI;

public class VipShopSlot : ShopItemSlot
{
    public Text contentText;
    public override void SetSlot(JObject data, SpriteAtlas atals, ShopManager shop)
    {
        base.SetSlot(data, atals, shop);
        var refidx = data["ref_idx"].ToObject<int>();
        data = TableDataManager.vipTable[refidx];
        Sprite sp = atals.GetSprite(data["image"].ToObject<string>());
        if (sp == null)
        {
            Console.Error(string.Format("다음 이미지를 찾을 수 없습니다.: {0}", data["img"].ToString()));
        }
        else
        {
            itemImage.sprite = sp;
        }
        string str = LocalizeManager.GetLocalString(data["string"].ToObject<string>());
        //Debug.LogError(str);
        str = str.Replace("\"", "");
        str = str.Replace("\\n", "\n");
        contentText.text = str;
    }
}
