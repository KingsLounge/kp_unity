using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Newtonsoft.Json.Linq;

public class CustomUICafeCashierTargetModule : CustomUIWebSocket
{
    public static int pagePer = 50;
    private int curCafeIdx = -1;
    public ItemControllerServerCommunication itemController;
    private JArray memberData = new JArray();
    private List<CafeCashierTargetTemplate> templates = new List<CafeCashierTargetTemplate>();
    protected override string defaultStyle => "height=350";
    public int selectedIdx = 0;

    private void Awake()
    {
        itemController.changePageCallback = (next) =>
        {
            if (next)
            {
                var packet = new Packet(CPProtocol.CP_CAFE_MEMBER_LIST);
                packet.Add("cafeIdx", Cafe.instance.curEnterCafeInfo["cafe"]["idx"]);
                packet.Add("startIdx", (int)memberData.Last["idx"]);
                packet.Add("per", pagePer);
                WebSocketManager.defaultCli.Send(packet);
            }
        };
        itemController.updateItemCallback = (go, data) =>
        {
            CafeCashierTargetTemplate t = go.GetComponent<CafeCashierTargetTemplate>();
            if (!templates.Contains(t)) templates.Add(t);
            t.SetData(this, data as JObject);
        };
    }

    public override void SetUI(JObject data)
    {
        base.SetUI(data);
    }

    protected override void CustomOnEnable()
    {
        base.CustomOnEnable();
        memberData.Clear();
        curCafeIdx = (int)Cafe.instance.curEnterCafeInfo["cafe"]["idx"];
        var packet = new Packet(CPProtocol.CP_CAFE_MEMBER_LIST);
        packet.Add("cafeIdx", curCafeIdx);
        packet.Add("startIdx", -1);
        packet.Add("per", pagePer);
        WebSocketManager.defaultCli.Send(packet);
    }

    protected override void SetStyle(List<KeyValuePair<string, string>> data)
    {
        base.SetStyle(data);
        for (int i = 0; i < data.Count; i++)
        {
            switch (data[i].Key)
            {
                case "height":
                    RectTransform rect = (transform as RectTransform);
                    Vector2 size = rect.sizeDelta;
                    size.y = float.Parse(data[i].Value);
                    rect.sizeDelta = size;
                    break;
            }
        }
    }

    protected override void ReceivePacket(Packet packet)
    {
        base.ReceivePacket(packet);
        JObject c = packet.c;
        if (c.ContainsKey("ecode"))
        {
            if ((int)c["ecode"] != 0) return;
        }

        if (packet.p == (int)PCProtocol.PC_CAFE_MEMBER_LIST)
        {
            JArray arr = c["cafeMembers"] as JArray;
            JObject dummy = new JObject();
            dummy.Add("idx", 0);
            dummy.Add("nick", "Cafe");
            arr.Add(dummy);
            SetList(arr);
        }
        if (packet.p == (int)PCProtocol.PC_CAFE_MEMBER_UPDATE)
        {
            // var json = {
            //        ecode: 0,
            //        cafeIdx,
            //        cafeMember,
            //        memberCount,  // null or number
            // }

            if (curCafeIdx == c.ValueOrDefault("cafeIdx", -1))
            {
                var cafeMember = c["cafeMember"] as JObject;
                int cafeMemberIdx = (int)cafeMember["idx"];
                CAFE_MEMBER_STATUS status = (CAFE_MEMBER_STATUS)(int)cafeMember["status"];
                CAFE_MEMBER_PERMIT permit = (CAFE_MEMBER_PERMIT)(int)cafeMember["permit"];

                if (c.ContainsKey("memberCount")) // 없을때도 있다. 
                {
                    int memberCount = (int)c["memberCount"];
                    // TODO:  cafe memberCount 가 변경되었다. 
                }

                if (status == CAFE_MEMBER_STATUS.suspended)
                {
                    for (int i = memberData.Count - 1; i >= 0; i--)
                    {
                        if ((memberData[i] as JObject).ValueOrDefault("idx", -1) == cafeMemberIdx)
                        {
                            memberData.RemoveAt(i);
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < memberData.Count; i++)
                    {
                        if ((memberData[i] as JObject).ValueOrDefault("idx", -1) == cafeMemberIdx)
                        {
                            memberData[i]["permit"] = (int)permit;
                        }
                    }
                }
                itemController.dataArray = memberData;
                itemController.Refresh();
            }
        }
        //else if (packet.p == (int)PCProtocol.PC_CAFE_JOIN_UPDATE)
        //{
        //    for (int i = 0; i < usingTemplate.Count; i++)
        //    {
        //        if (usingTemplate[i].idx == (int)c["Idx"])
        //        {
        //            objectPool.ReturnObject(usingTemplate[i]);
        //        }
        //    }
        //}
    }
    private void SetList(JArray arr)
    {
        int[] idxs = new int[memberData.Count];
        for (int i = 0; i < memberData.Count; i++)
        {
            idxs[i] = (int)memberData[i]["idx"];
        }
        for (int i = 0; i < arr.Count; i++)
        {
            int idx = System.Array.IndexOf(idxs, (int)arr[i]["idx"]);
            if (idx == -1)
            {
                memberData.Add(arr[i]);
            }
            else
            {
                memberData[idx] = arr[i];
            }
        }
        itemController.dataArray = Sort(memberData);
        itemController.Refresh();
    }

    private JArray Sort(JArray arr)
    {
        return new JArray(arr.OrderBy(a => (a as JObject).ValueOrDefault("idx", 0)));
    }

    //public void OnClickAccept(int idx)
    //{
    //    Packet packet = new Packet(CPProtocol.CP_CAFE_JOIN_UPDATE);
    //    packet.Add("cafeIdx", Cafe.instance.curEnterCafeInfo["cafe"]["idx"]);
    //    packet.Add("cafeJoinIdx", idx);
    //    packet.Add("status", (int)CAFE_JOIN_STATUS.accepted);
    //    WebSocketManager.defaultCli.Send(packet);
    //}

    //public void OnClickReject(int idx)
    //{
    //    Packet packet = new Packet(CPProtocol.CP_CAFE_JOIN_UPDATE);
    //    packet.Add("cafeIdx", Cafe.instance.curEnterCafeInfo["cafe"]["idx"]);
    //    packet.Add("cafeJoinIdx", idx);
    //    packet.Add("status", (int)CAFE_JOIN_STATUS.rejected);
    //    WebSocketManager.defaultCli.Send(packet);
    //}
    public override JToken GetValue()
    {
        return selectedIdx;
    }

}
