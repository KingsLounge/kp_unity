using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json.Linq;

public class CashierPopup : MonoBehaviour
{
    [SerializeField]
    private InputField transferChipInput;
    [SerializeField]
    private Text chipText;

    [SerializeField]
    private GameObject ownerObj;
    [SerializeField]
    private GameObject memberObj;

    private long transferChip = 0;

    public void Init()
    {
        var cafeInfo = Cafe.instance.curEnterCafeInfo;
        var cafe = Cafe.instance.curEnterCafeInfo["cafe"] as JObject;
        JObject cafeMember = cafeInfo.ValueOrDefault("cafeMember", new JObject());
        chipText.text = MoneyToString.Converting(cafeMember.ValueOrDefault("cc", 0));
        
        
        CAFE_MEMBER_PERMIT permit = (CAFE_MEMBER_PERMIT)(int)cafeMember["permit"];

        bool bPermit = permit == CAFE_MEMBER_PERMIT.manager || permit == CAFE_MEMBER_PERMIT.owner;
        ownerObj?.SetActive(bPermit);
        memberObj?.SetActive(!bPermit);
    }
    public void ChangeInput(string value)
    {
        transferChip = long.Parse(value);
    }
    
    public void BuyCafeOrder()
    {
        var p = new Packet(CPProtocol.CP_CAFE_MEMBER_ORDER);
        p.Add("status", (int)TRANSFER_TYPE.buy);
        p.Add("amount", transferChip);
        p.Add("cafeIdx", Cafe.instance.curEnterCafeInfo["cafe"]["idx"]);
        p.Add("goods", 1);
        WebSocketManager.defaultCli.Send(p);
    }
    public void SellCafeOrder()
    {
        var p = new Packet(CPProtocol.CP_CAFE_MEMBER_ORDER);
        p.Add("status", (int)TRANSFER_TYPE.sell);
        p.Add("amount", transferChip);
        p.Add("cafeIdx", Cafe.instance.curEnterCafeInfo["cafe"]["idx"]);
        p.Add("goods", 1);
        WebSocketManager.defaultCli.Send(p);
    }
}
