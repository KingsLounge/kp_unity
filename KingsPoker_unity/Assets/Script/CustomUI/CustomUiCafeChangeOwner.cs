using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;

public class CustomUiCafeChangeOwner : CustomUIWebSocket
{
    public static int pagePer = 50;
    private int curCafeIdx = -1;
    public ItemControllerServerCommunication itemController;
    private JArray memberData = new JArray();
    private List<CafeChangeOwnerTemplate> templates = new List<CafeChangeOwnerTemplate>();
    public List<Variation> myPermitVariation = new List<Variation>();
    protected override string defaultStyle => "y-interval=0";

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
            CafeChangeOwnerTemplate t = go.GetComponent<CafeChangeOwnerTemplate>();
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


    public override void SetUI(JObject data)
    {
        base.SetUI(data);
        myPermitVariation.ForEach(v => v.SetVariation(Cafe.instance.permit.ToString()));
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
        for(int i = 0; i < data.Count; i++)
        {
            switch(data[i].Key)
            {
                case "y-interval":
                    Vector2 size = root.containerSize;
                    size.y -= float.Parse(data[i].Value);
                    (transform as RectTransform).sizeDelta = size;
                    break;
            }
        }
    }

    protected override void ReceivePacket(Packet packet)
    {
        base.ReceivePacket(packet);
        JObject c = packet.c;
        if(c.ContainsKey("ecode"))
        {
            if ((int)c["ecode"] != 0) return;
        }
        
        if (packet.p == (int)PCProtocol.PC_CAFE_MEMBER_LIST)
        {
            SetList(c["cafeMembers"] as JArray);
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
                    for (int i = memberData.Count - 1; i >= 0 ; i--)
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
        itemController.dataArray = memberData;
        itemController.Refresh();
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

}
