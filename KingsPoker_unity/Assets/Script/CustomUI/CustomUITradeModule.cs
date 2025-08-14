using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;
using System.Linq;
public class CustomUITradeModule : CustomUIWebSocket
{
    public static int pagePer = 50;
    public ItemControllerServerCommunication itemController;
    private JArray tradeData = new JArray();
    private List<TradeModuleTemplate> templates = new List<TradeModuleTemplate>();
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
                    Vector2 size = root.viewportSize;
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
            TradeModuleTemplate t = go.GetComponent<TradeModuleTemplate>();
            if (!templates.Contains(t)) templates.Add(t);
            t.SetData(this, data as JObject);
        };
    }

    private void RequestNextData()
    {
        var packet = new Packet(CPProtocol.CP_CAFE_MEMBER_ORDER_LIST);
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
        var packet = new Packet(CPProtocol.CP_CAFE_MEMBER_ORDER_LIST);
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
            
        if (packet.p == (int)PCProtocol.PC_CAFE_MEMBER_ORDER_LIST)
        {
            SetList(c["cafeMemberOrders"] as JArray);
        }
        else if (packet.p == (int)PCProtocol.PC_CAFE_MEMBER_ORDER_UPDATE)
        {
            if(c.ContainsKey("cafeMemberOrder"))
            {
                JObject data = c["cafeMemberOrder"] as JObject;
                bool cancelOrder = false;
                for (int i = 0; i < templates.Count; i++)
                {
                    if (templates[i].idx == (int)data["idx"])
                    {
                        templates[i].SetData(this, data);
                        TRANSFER_TYPE status = (TRANSFER_TYPE)data.ValueOrDefault("status", 0);
                        if (status == TRANSFER_TYPE.borrow_cancel || status == TRANSFER_TYPE.buy_cancel || status == TRANSFER_TYPE.sell_cancel)
                            cancelOrder = true;
                    }
                }
                for(int i = 0; i < tradeData.Count; i++)
                {
                    if((int)tradeData[i]["idx"] == (int)data["idx"])
                    {
                        tradeData[i] = data;
                    }
                }
                if (cancelOrder)
                {
                    itemController.dataArray = RefreshData(tradeData);
                    itemController.Refresh();
                }
            }
            else if (c.ContainsKey("cafeMemberOrderIdx"))
            {
                int idx = c.ValueOrDefault("cafeMemberOrderIdx", 0);
                TRANSFER_TYPE status = (TRANSFER_TYPE)c.ValueOrDefault("status", 0);
                if(status == TRANSFER_TYPE.borrow_cancel || status == TRANSFER_TYPE.buy_cancel || status == TRANSFER_TYPE.sell_cancel)
                {
                    bool refresh = false;
                    for (int i = 0; i < tradeData.Count; i++)
                    {
                        if ((int)tradeData[i]["idx"] == idx)
                        {
                            tradeData[i]["status"] = c["status"];
                            refresh = true;
                        }
                    }
                    if (refresh)
                    {
                        itemController.dataArray = RefreshData(tradeData);
                        itemController.Refresh();
                    }
                }
            }
        }
        else if(packet.p == (int)PCProtocol.PC_CAFE_NOTICE)
        {
            RequestNextData();
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
        itemController.dataArray = RefreshData(tradeData);
        itemController.Refresh();
    }


    private JArray Sort(JArray arr)
    {
        return new JArray(arr.OrderBy(obj => -(obj as JObject).ValueOrDefault("idx", 0)));
    }

    private JArray RefreshData(JArray data)
    {
        data = new JArray(data.Where(t => {
            TRANSFER_TYPE status = (TRANSFER_TYPE)((t as JObject).ValueOrDefault("status", 0));
            bool canceled = (status == TRANSFER_TYPE.buy_cancel || status == TRANSFER_TYPE.sell_cancel || status == TRANSFER_TYPE.borrow_cancel);
            return !canceled;
        }));
        return Sort(data);
    }

    public void OnClickAccept(int idx, TRANSFER_TYPE status)
    {
        // if (waitingResult) return;
        TRANSFER_TYPE n = TRANSFER_TYPE.none;
        switch(status)
        {
            case TRANSFER_TYPE.sell:
                n = TRANSFER_TYPE.sold;
                break;
            case TRANSFER_TYPE.buy:
                n = TRANSFER_TYPE.bought;
                break;
            case TRANSFER_TYPE.borrow:
                n = TRANSFER_TYPE.borrowed;
                break;
        }
        Packet packet = new Packet(CPProtocol.CP_CAFE_MEMBER_ORDER_UPDATE);
        packet.Add("cafeMemberOrderIdx", idx);
        packet.Add("cafeIdx", Cafe.instance.curEnterCafeInfo["cafe"]["idx"]);
        packet.Add("status", (int)n);
        WebSocketManager.defaultCli.Send(packet);
        StartCoroutine(WaitResultPacket());
    }

    public void OnClickReject(int idx, TRANSFER_TYPE status)
    {
        // if (waitingResult) return;
        TRANSFER_TYPE n = TRANSFER_TYPE.none;
        switch (status)
        {
            case TRANSFER_TYPE.sell:
                n = TRANSFER_TYPE.sell_rejected;
                break;
            case TRANSFER_TYPE.buy:
                n = TRANSFER_TYPE.buy_rejected;
                break;
            case TRANSFER_TYPE.borrow:
                n = TRANSFER_TYPE.borrow_rejected;
                break;
        }
        Packet packet = new Packet(CPProtocol.CP_CAFE_MEMBER_ORDER_UPDATE);
        packet.Add("cafeMemberOrderIdx", idx);
        packet.Add("cafeIdx", Cafe.instance.curEnterCafeInfo["cafe"]["idx"]);
        packet.Add("status", (int)n);
        WebSocketManager.defaultCli.Send(packet);
        StartCoroutine(WaitResultPacket());
    }

    private IEnumerator WaitResultPacket()
    {
        // waitingResult = true;
        var wait = new WaitForPCProtocol(PCProtocol.PC_CAFE_MEMBER_ORDER_UPDATE);
        yield return wait;
        var c = wait.Result.c;
        TRANSFER_TYPE status = TRANSFER_TYPE.none;
        if (c.ContainsKey("status"))
        {
            status = (TRANSFER_TYPE)(int)c["status"];
        }
        if (c.ContainsKey("cafeMemberOrder"))
        {
            status = (TRANSFER_TYPE)(int)c["cafeMemberOrder"]["status"];
        }

        switch (status)
        {
            case TRANSFER_TYPE.sold:
            case TRANSFER_TYPE.bought:
            case TRANSFER_TYPE.borrowed:
                NormalMessage.instance.OnOneButtonMessagePopUp("confirm_success");
                break;
            case TRANSFER_TYPE.sell_rejected:
            case TRANSFER_TYPE.buy_rejected:
            case TRANSFER_TYPE.borrow_rejected:
                NormalMessage.instance.OnOneButtonMessagePopUp("decline_success");
                break;
        }
        // waitingResult = false;
    }
}
