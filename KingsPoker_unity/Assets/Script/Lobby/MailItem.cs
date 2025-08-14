using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MailItem : MonoBehaviour
{
    public Image itemIamge;
    public Text quantityText;
    private int refIdx;
    private int quantity;
    public void Set(int refIdx, int quantity)
    {
        this.refIdx = refIdx;
        this.quantity = quantity;
        var item = InfoManager.itemData[refIdx];
        if(item == null)
        {
            Console.Error(string.Format("존재하지 않는 아이템 코드 : {0}",refIdx));
            return;
        }
        var sp = LobbyManager.Instance.itemAtlas.GetSprite(item["img"].ToString());
        if(sp == null)
        {
            Console.Error(string.Format("아이템 이미지가 존재하지 않습니다. : {0}",item["img"].ToString()));
        }

        itemIamge.sprite = sp;
        quantityText.text = string.Format("x{0}",quantity);
    }
}
