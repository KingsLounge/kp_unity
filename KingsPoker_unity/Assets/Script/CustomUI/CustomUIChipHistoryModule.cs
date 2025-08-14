using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
public class CustomUIChipHistoryModule : CustomUIWebSocket
{
    public static int pagePer = 50;
    public ItemControllerServerCommunication itemController;
    private JArray tradeData = new JArray();
    private List<ChipHistoryModuleTemplate> templates = new List<ChipHistoryModuleTemplate>();

    public Sprite[] img_chips;
    public Color[] color_chips;
    public string[] prefixs = new string[] { 
        "",  "",  "",  "-", "+", 
        "+", "-", "+", "+", "-", 
        "+", "+", "-", "+", "+", 
        "-", "-", "+", "",  "-", 
        "-", "-", "-", "-" };

    protected override string defaultStyle => "y-interval=0";
    private bool waitingResult = false;
    protected override void SetStyle(List<KeyValuePair<string, string>> data)
    {
        base.SetStyle(data);
        for (int i = 0; i < data.Count; i++)
        {
            switch (data[i].Key)
            {
                case "y-interval":
                    Vector2 size = root.containerSize;
                    size.y -= float.Parse(data[i].Value);
                    (transform as RectTransform).sizeDelta = size;
                    break;
            }
        }
    }
    private void Awake()
    {
        itemController.changePageCallback = (next) =>
        {
            if(next)
            {
                RequestNextData();
            }
        };
        itemController.updateItemCallback = (go, data) =>
        {
            ChipHistoryModuleTemplate t = go.GetComponent<ChipHistoryModuleTemplate>();
            if (!templates.Contains(t)) templates.Add(t);
            t.SetData(this, data as JObject);
        };
    }

    private void RequestNextData()
    {
        var packet = new Packet(CPProtocol.CP_CAFE_MEMBER_CHIP_HISTORY);
        packet.Add("cafeIdx", Cafe.instance.curEnterCafeInfo["cafe"]["idx"]);
        packet.Add("startIdx", (int)tradeData.Last["idx"]);
        packet.Add("per", pagePer);
        packet.Add("orderDir", "ASC");
        WebSocketManager.defaultCli.Send(packet);
    }

    protected override void CustomOnEnable()
    {
        base.CustomOnEnable();
        if(tradeData != null)
            tradeData.Clear();
        var packet = new Packet(CPProtocol.CP_CAFE_MEMBER_CHIP_HISTORY);
        packet.Add("cafeIdx", Cafe.instance.curEnterCafeInfo["cafe"]["idx"]);
        packet.Add("startIdx", -1);
        packet.Add("per", pagePer);
        WebSocketManager.defaultCli.Send(packet);
    }

    protected override void ReceivePacket(Packet packet)
    {
        base.ReceivePacket(packet);
        JObject c = packet.c;
        ERR ecode = ERR.OK;
        if (c.ContainsKey("ecode"))
        {
            ecode = (ERR)(int)c["ecode"];
            if (ecode != ERR.OK)
            {
                return;
            }
        }
            
        if (packet.p == (int)PCProtocol.PC_CAFE_MEMBER_CHIP_HISTORY)
        {
            SetList(c["cafeMemberHistorys"] as JArray);
        }
    }
    private void SetList(JArray arr)
    {
        int[] idxs = new int[tradeData.Count];
        for (int i = 0; i < tradeData.Count; i++)
        {
            idxs[i] = (int)tradeData[i]["idx"];
        }
        for (int i = 0; i < arr.Count; i++)
        {
            int idx = System.Array.IndexOf(idxs, (int)arr[i]["idx"]);
            if (idx == -1)
            {
                tradeData.Add(arr[i]);
            }
            else
            {
                tradeData[idx] = arr[i];
            }
        }
        itemController.dataArray = Sort(tradeData);
        itemController.Refresh();
    }


    private JArray Sort(JArray arr)
    {
        return new JArray(arr.OrderBy(obj => -(obj as JObject).ValueOrDefault("idx", 0)));
    }
}
