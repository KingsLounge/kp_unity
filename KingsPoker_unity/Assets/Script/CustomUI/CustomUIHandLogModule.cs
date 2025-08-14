using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.UI;

public class CustomUIHandLogModule : CustomUIWebSocket
{
    public static int pagePer = 50;
    public ItemControllerServerCommunication itemController;
    private JArray handsLogData = new JArray();
    private List<HandLogModuleTemplate> templates = new List<HandLogModuleTemplate>();
    public List<Color> handLogColors = new List<Color>();
    private int colorIdx = -1;
    public ScrollRect scrollView = null;
    private CustomUIHandLogRoot handlogRoot
    {
        get
        {
            System.Type t = root.GetType();
            if (t.IsSubclassOf(typeof(CustomUIHandLogRoot)) || t == typeof(CustomUIHandLogRoot))
            {
                return root as CustomUIHandLogRoot;
            }
            return null;
        }
    }

    protected override string defaultStyle => "y-interval=0";
    private bool isCafeHandLog
    {
        get
        {
            if (handlogRoot == null)
                return false;
            return handlogRoot.isCafeHandLog;
        }
    }
    private long gtn
    {
        get
        {
            if (handlogRoot == null)
                return -1;
            TableManager tableManager = handlogRoot.tableManager;
            return tableManager == null ? 0 : tableManager.GetRoomNumber();
        }
    }
    private int cafeIdx
    {
        get { return (int)Cafe.instance.curEnterCafeInfo["cafe"]["idx"]; }
    }

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
            HandLogModuleTemplate t = go.GetComponent<HandLogModuleTemplate>();
            if (!templates.Contains(t))
            {
                if (handLogColors.Count > 0)
                {
                    colorIdx++;
                    if (colorIdx >= handLogColors.Count)
                        colorIdx = 0;
                    t.GetComponent<Image>().color = handLogColors[colorIdx];
                }
                templates.Add(t);
            }
            t.SetData(this, data as JObject);
        };
    }

    private void RequestNextData()
    {
        if (isCafeHandLog)
        {
            Packet packet = new Packet(CPProtocol.CP_HANDS_LOG_CAFE_LIST);
            packet.Add("cafeIdx", cafeIdx);
            packet.Add("startIdx", handsLogData.Count == 0 ? -1 : (int)handsLogData.Last["gn"]);
            packet.Add("per", pagePer);
            packet.Add("orderDir", "DESC");
            WebSocketManager.defaultCli.Send(packet);
        }
        else
        {
            Packet packet = new Packet(CPProtocol.CP_HANDS_LOG_MY_LIST);
            packet.Add("gtn", gtn);
            packet.Add("startIdx", handsLogData.Count == 0 ? -1 : (int)handsLogData.Last["gn"]);
            packet.Add("per", pagePer);
            packet.Add("orderDir", "DESC");
            WebSocketManager.defaultCli.Send(packet);
        }
    }

    protected override void CustomOnEnable()
    {
        base.CustomOnEnable();
        if (scrollView)
            scrollView.verticalNormalizedPosition = 0;
        if (handsLogData != null)
            handsLogData.Clear();
        itemController.dataArray = handsLogData;
        itemController.Refresh();
        if (isCafeHandLog)
        {
            Packet packet = new Packet(CPProtocol.CP_HANDS_LOG_CAFE_LIST);
            packet.Add("cafeIdx", cafeIdx);
            packet.Add("startIdx", -1);
            packet.Add("per", pagePer);
            packet.Add("orderDir", "DESC");
            WebSocketManager.defaultCli.Send(packet);
        }
        else
        {
            Packet packet = new Packet(CPProtocol.CP_HANDS_LOG_MY_LIST);
            packet.Add("gtn", gtn);
            packet.Add("startIdx", -1);
            packet.Add("per", pagePer);
            packet.Add("orderDir", "DESC");
            WebSocketManager.defaultCli.Send(packet);
        }
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

        if (
            packet.p == (int)PCProtocol.PC_HANDS_LOG_MY_LIST
            || packet.p == (int)PCProtocol.PC_HANDS_LOG_CAFE_LIST
        )
        {
            SetList(c["handsLog"] as JArray);
        }
        else if (packet.p == (int)PCProtocol.PC_HOLDEM_GAMERESULT)
        {
            RequestNextData();
        }
    }

    private void SetList(JArray arr)
    {
        int[] idxs = new int[handsLogData.Count];
        for (int i = 0; i < handsLogData.Count; i++)
        {
            idxs[i] = (int)handsLogData[i]["gn"];
        }
        for (int i = 0; i < arr.Count; i++)
        {
            int idx = System.Array.IndexOf(idxs, (int)arr[i]["gn"]);
            if (idx == -1)
            {
                handsLogData.Add(arr[i]);
            }
            else
            {
                handsLogData[idx] = arr[i];
            }
        }
        bool showLast = scrollView.verticalNormalizedPosition == 1f;
        itemController.dataArray = handsLogData;
        itemController.Refresh();
        if (showLast)
        {
            scrollView.verticalNormalizedPosition = 1f;
        }
    }
}
