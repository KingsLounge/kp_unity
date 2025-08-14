using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json.Linq;

public class AddZuiceItemSlot : MonoBehaviour
{
    private long price;
    [SerializeField]
    private Text priceText;
    [SerializeField]
    private Text timeText;
    [SerializeField]
    private Text contentsText;
    private bool sale = false;
    public void SetData(JObject data)
    {
        price = (long)data["zuice_sale"];
        long originalPrice = (long)data["zuice"];
        sale = originalPrice != price;

        priceText.text = MoneyToString.Converting(price);

        long time = (long)data["sec"];
        timeText.text = SecondToString(time);

        string contents = LocalizeManager.GetLocalString("add_zuice_contents_text");

        if(contents == "add_zuice_contents_text")
        {
            contents = "Charge {1} using {0} zuice.";
        }
        contentsText.text = string.Format(contents, price,SecondToString(time));
    }

    private string SecondToString(long sec)
    {
        const long daySec = 24 * 60 * 60;
        const long hourSec = 60 * 60;
        const long minuteSec = 60;

        long day = sec / daySec;
        sec %= daySec;
        long hour = sec / hourSec;
        sec %= hourSec;
        long minute = sec / minuteSec;
        sec %= minuteSec;

        string result = "";
        if(day > 0)
        {
            result += day + " days";
        }
        if (hour > 0)
        {
            result += hour + " hour";
        }
        if (minute > 0)
        {
            result += minute + " minute";
        }
        if (sec > 0)
        {
            result += sec + " sec";
        }
        return result;
    }

    public void OnClickOK()
    {
        if(price > MyStatus.zc)
        {
            NormalMessage.instance.OnOneButtonMessagePopUp("insufficient_my_zuice");
            return;
        }
        NormalMessage.instance.OnMessagePopup(LocalizeManager.GetLocalString("add_zuice_confirm_pannel_title"), LocalizeManager.GetLocalString("add_zuice_confirm_pannel_contents"), () =>
         {
             StartCoroutine(SendPacket());
         });
    }

    private IEnumerator SendPacket()
    {
        JObject obj = new JObject();
        obj["zc"] = price;
        obj["cafeIdx"] = Cafe.instance.curEnterCafeInfo["cafe"]["idx"];
        WebSocketManager.defaultCli.Send(new Packet(CPProtocol.CP_CAFE_TRANS_ZUICE, obj));
        LoadingCircle.Instance.StartSpin();
        yield return new WaitForPCProtocol(PCProtocol.PC_CAFE_TRANS_ZUICE);
        LoadingCircle.Instance.StopSpin();
        NormalMessage.instance.OnOneButtonMessagePopUp(LocalizeManager.GetLocalString("add_zuice_complete"),()=> { gameObject.SetActive(false); });
    }
}
