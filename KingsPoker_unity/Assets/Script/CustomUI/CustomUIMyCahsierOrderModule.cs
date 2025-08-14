using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;
using System.Linq;
public class CustomUIMyCahsierOrderModule : CustomUIWebSocket
{
    public static int pagePer = 50;
    public ItemControllerServerCommunication itemController;
    private JArray orderData = new JArray();
    private List<MyCashierOrderTemplate> templates = new List<MyCashierOrderTemplate>();

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
            MyCashierOrderTemplate t = go.GetComponent<MyCashierOrderTemplate>();
            if (!templates.Contains(t)) templates.Add(t);
            t.SetData(this, data as JObject);
        };
    }

    public override void CompleteSetting()
    {
        base.CompleteSetting();
        StartCoroutine(SetHeight());
    }
    private IEnumerator SetHeight()
    {
        yield return 0;
        RectTransform rectTr = transform as RectTransform;
        Vector2 newSize = rectTr.sizeDelta;
        newSize.y = root.viewportSize.y - (root.containerSize.y - rectTr.rect.height);
        if (newSize.y < 300)
            newSize.y = 300;
        rectTr.sizeDelta = newSize;
    }
    private void RequestNextData()
    {
        var packet = new Packet(CPProtocol.CP_CAFE_MEMBER_MY_ORDER_LIST);
        packet.Add("cafeIdx", Cafe.instance.curEnterCafeInfo["cafe"]["idx"]);
        if (orderData != null && orderData.Count > 0 )
        {
            try
            {
                packet.Add("startIdx", (int)orderData.Last["idx"]);
            }
            catch (System.Exception e)
            {
                Debug.Log(e.ToString());
            }
        }
        else
        {
            packet.Add("startIdx", -1);
        }
        
        packet.Add("per", pagePer);
        packet.Add("orderDir", "ASC");
        WebSocketManager.defaultCli.Send(packet);
    }

    protected override void CustomOnEnable()
    {
        base.CustomOnEnable();
        if(orderData != null)
            orderData.Clear();
        var packet = new Packet(CPProtocol.CP_CAFE_MEMBER_MY_ORDER_LIST);
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
            
        if (packet.p == (int)PCProtocol.PC_CAFE_MEMBER_MY_ORDER_LIST)
        {
            SetList(c["cafeMemberOrders"] as JArray);
        }
        else if (packet.p == (int)PCProtocol.PC_CAFE_MEMBER_ORDER_UPDATE)
        {
            if(c.ContainsKey("cafeMemberOrder"))
            {
                JObject data = c["cafeMemberOrder"] as JObject;
                bool refresh = false;
                for(int i = 0; i < orderData.Count; i++)
                {
                    if((int)orderData[i]["idx"] == (int)data["idx"])
                    {
                        orderData[i] = data;
                        refresh = true;
                    }
                }
                if(refresh)
                {
                    itemController.dataArray = RefreshData(orderData);
                    itemController.Refresh();
                }
            }
            else if(c.ContainsKey("cafeMemberOrderIdx"))
            {
                int idx = c.ValueOrDefault("cafeMemberOrderIdx", 0);
                bool refresh = false;
                for (int i = 0; i < orderData.Count; i++)
                {
                    if ((int)orderData[i]["idx"] == idx)
                    {
                        orderData[i]["status"] = c["status"];
                        refresh = true;
                    }
                }
                if (refresh)
                {
                    itemController.dataArray = RefreshData(orderData);
                    itemController.Refresh();
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
        int[] idxs = new int[orderData.Count];
        for (int i = 0; i < orderData.Count; i++)
        {
            idxs[i] = (int)orderData[i]["idx"];
        }
        for (int i = 0; i < arr.Count; i++)
        {
            int idx = System.Array.IndexOf(idxs, (int)arr[i]["idx"]);
            if (idx == -1)
            {
                orderData.Add(arr[i]);
            }
            else
            {
                orderData[idx] = arr[i];
            }
        }
        itemController.dataArray = RefreshData(orderData);
        itemController.Refresh();
    }

    private JArray RefreshData(JArray data)
    {
        data = new JArray(data.Where(t => {
            TRANSFER_TYPE status = (TRANSFER_TYPE)((t as JObject).ValueOrDefault("status",0));
            bool complete = (status != TRANSFER_TYPE.buy && status != TRANSFER_TYPE.sell && status != TRANSFER_TYPE.borrow);
            return !complete;
        }));
        return Sort(data);
    }


    private JArray Sort(JArray arr)
    {
        return new JArray(arr.OrderBy(obj => -(obj as JObject).ValueOrDefault("idx", 0)));
    }

    public void OnClickCancel(int idx, TRANSFER_TYPE status)
    {
        // if (waitingResult) return;
        TRANSFER_TYPE n = TRANSFER_TYPE.none;
        switch(status)
        {
            case TRANSFER_TYPE.sell:
                n = TRANSFER_TYPE.sell_cancel;
                break;
            case TRANSFER_TYPE.buy:
                n = TRANSFER_TYPE.buy_cancel;
                break;
            case TRANSFER_TYPE.borrow:
                n = TRANSFER_TYPE.borrow_cancel;
                break;
        }
        Packet packet = new Packet(CPProtocol.CP_CAFE_MEMBER_ORDER);
        packet.Add("cafeMemberOrderIdx", idx);
        packet.Add("cafeIdx", Cafe.instance.curEnterCafeInfo["cafe"]["idx"]);
        packet.Add("status", (int)n);
        WebSocketManager.defaultCli.Send(packet);
        StartCoroutine(WaitResultPacket());
    }

    private IEnumerator WaitResultPacket()
    {
        // waitingResult = true;
        var wait = new WaitForPCProtocol(PCProtocol.PC_CAFE_MEMBER_ORDER);
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
            case TRANSFER_TYPE.sell_cancel:
            case TRANSFER_TYPE.buy_cancel:
            case TRANSFER_TYPE.borrow_cancel:
                NormalMessage.instance.OnOneButtonMessagePopUp("cancel_success");
                break;

            case TRANSFER_TYPE.sell:
                /* TODO:  
                 * 
                 * 
                 *  보유칩:1000  빌린칩: 200 상황에서,  100 을 팔기 할때,  바로 처리되므로  dc, debt을 바로 업데이트 해야 한다. 
                 * 
                    PC_CAFE_MEMBER_ORDER
                    {   "p":65,
                        "c":{"ecode":0,"cafeIdx":34,
                                "cafeMemberOrder":{"zc":0,"idx":548,"goods":2,"gid":"al0000001","nick":"al0000001","status":15,"dc":100,"cafeMemberIdx":206,"cafeIdx":34,"updatedAt":"2021-06-02T14:51:53.087Z","createdAt":"2021-06-02T14:51:53.087Z"},
                                "cafeMember":{"idx":206,"status":2,"permit":3,"type":0,"nick":"al0000001","zc":"0","dc":5600,"debt":700,"cafeIdx":34,"gameUserIdx":131}}
                    }

                    dc, debt 을 업데이트 해야 한다. 

                 */
                break;
        }
        // waitingResult = false;
    }
}
