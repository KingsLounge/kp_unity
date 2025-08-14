using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class CustomUICafeMemberJoinModule : CustomUIWebSocket
{
    public static int pagePer = 50;
    public ItemControllerServerCommunication itemController;
    private JArray joinData = new JArray();
    public CafeMemberJoinModuleTemplate template;
    private List<CafeMemberJoinModuleTemplate> templates = new List<CafeMemberJoinModuleTemplate>();
    protected override string defaultStyle => "y-interval=0";
    protected override void SetStyle(List<KeyValuePair<string, string>> data)
    {
        base.SetStyle(data);
        for (int i = 0; i < data.Count; i++)
        {
            switch (data[i].Key)
            {
                case "y-interval":
                    {
                        Vector2 size = root.viewportSize;
                        size.y -= float.Parse(data[i].Value);
                        (transform as RectTransform).sizeDelta = size;
                    }
                    break;
            }
        }
    }

    private void Awake()
    {
        itemController.changePageCallback = (next) =>
        {
            if (next)
            {
                RequestNextData();
            }
        };
        itemController.updateItemCallback = (go, data) =>
        {
            CafeMemberJoinModuleTemplate t = go.GetComponent<CafeMemberJoinModuleTemplate>();
            if (!templates.Contains(t)) templates.Add(t);
            t.SetData(this, data as JObject);
        };
    }

    private void RequestNextData()
    {
        var packet = new Packet(CPProtocol.CP_CAFE_JOIN_LIST);
        packet.Add("cafeIdx", Cafe.instance.curEnterCafeInfo["cafe"]["idx"]);
        int startIdx = -1;
        if (joinData.Count > 0)
        {
            startIdx = (int)joinData.Last["idx"];
        }
        packet.Add("startIdx", startIdx);
        packet.Add("per", pagePer);
        packet.Add("orderDir", "ASC");
        WebSocketManager.defaultCli.Send(packet);
    }

    protected override void CustomOnEnable()
    {
        base.CustomOnEnable();
        if (joinData != null) joinData.Clear();
        var packet = new Packet(CPProtocol.CP_CAFE_JOIN_LIST);
        packet.Add("cafeIdx", Cafe.instance.curEnterCafeInfo["cafe"]["idx"]);
        packet.Add("startIdx", -1);
        packet.Add("per", pagePer);
        packet.Add("orderDir", "DESC");
        WebSocketManager.defaultCli.Send(packet);
    }

    protected override void ReceivePacket(Packet packet)
    {
        base.ReceivePacket(packet);
        JObject c = packet.c;
        if(c.ContainsKey("ecode"))
        {
            if ((int)c["ecode"] != 0) return;
        }
       
        if (packet.p == (int)PCProtocol.PC_CAFE_JOIN_LIST)
        {
            SetList(c["cafeJoins"] as JArray);
        }
        else if (packet.p == (int)PCProtocol.PC_CAFE_JOIN_UPDATE)
        {
            for(int i = 0; i < joinData.Count; i++)
            {
                if((int)joinData[i]["idx"] == (int)c["cafeJoinIdx"])
                {
                    joinData.Remove(joinData[i]);
                    itemController.dataArray = Sort(joinData);
                    itemController.Refresh();
                    break;
                }
            }
            var status = (CAFE_JOIN_STATUS)(int)c["cafeJoin"]["status"];
            switch(status)
            {
                case CAFE_JOIN_STATUS.accepted:
                    NormalMessage.instance.OnOneButtonMessagePopUp("cafe_join_success_p");
                    break;
                case CAFE_JOIN_STATUS.rejected:
                    NormalMessage.instance.OnOneButtonMessagePopUp("decline_success_p");
                    break;
            }
        }
        else if(packet.p == (int)PCProtocol.PC_CAFE_NOTICE)
        {
            RequestNextData();
        }
    }
    private void SetList(JArray arr)
    {
        int[] idxs = new int[joinData.Count];
        for (int i = 0; i < joinData.Count; i++)
        {
            idxs[i] = (int)joinData[i]["idx"];
        }
        for (int i = 0; i < arr.Count; i++)
        {
            int idx = System.Array.IndexOf(idxs, (int)arr[i]["idx"]);
            if (idx == -1)
            {
                joinData.Add(arr[i]);
            }
            else
            {
                joinData[idx] = arr[i];
            }
        }
        itemController.dataArray = Sort(joinData);
        itemController.Refresh();
    }


    private JArray Sort(JArray arr)
    {
        return arr;
    }

    public void OnClickAccept(int idx)
    {
        Packet packet = new Packet(CPProtocol.CP_CAFE_JOIN_UPDATE);
        packet.Add("cafeIdx", Cafe.instance.curEnterCafeInfo["cafe"]["idx"]);
        packet.Add("cafeJoinIdx", idx);
        packet.Add("status", (int)CAFE_JOIN_STATUS.accepted);
        WebSocketManager.defaultCli.Send(packet);
    }

    public void OnClickReject(int idx)
    {
        Packet packet = new Packet(CPProtocol.CP_CAFE_JOIN_UPDATE);
        packet.Add("cafeIdx", Cafe.instance.curEnterCafeInfo["cafe"]["idx"]);
        packet.Add("cafeJoinIdx", idx);
        packet.Add("status", (int)CAFE_JOIN_STATUS.rejected);
        WebSocketManager.defaultCli.Send(packet);
    }

}
