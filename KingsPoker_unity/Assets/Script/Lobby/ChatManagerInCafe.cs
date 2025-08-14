using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json.Linq;
using Jeckl.InfinityScroll;
using System.Linq;

public class ChatManagerInCafe : WebsocketListenBehaviour
{
    public static readonly int pagePer = 50;
    [HideInInspector]
    public int roomNumber = -1;
    [SerializeField]
    private InputField input;
    [SerializeField]
    private InfinityScroll scroll;
    [SerializeField]
    private List<Emoticon> ignore_emoticons = new List<Emoticon>();
    private int cafeIdx = -1;
    public static ChatManagerInCafe instance
    {
        get; private set;
    }

    private RoomUserData DataToRoomUserData(JObject c)
    {
        RoomUserData data = new RoomUserData();
        data.gid = c.ValueOrDefault("to_gid","");
        data.nick = c.ValueOrDefault("nick", "");
        return data;
    }

    public void SetCafeIdx(int cafeIdx)
    {
        this.cafeIdx = cafeIdx;
        scroll.dataArray.Clear();
        RequestCafeChatList(-1);
    }

    private void RequestCafeChatList(int startIdx, string orderDir = "DESC")
    {
        Packet p = new Packet(CPProtocol.CP_CAFE_CHAT_LIST); //cafeIdx, startIdx, per, orderColumn, orderDir
        p.Add("cafeIdx", cafeIdx);
        p.Add("startIdx", startIdx);
        p.Add("per", pagePer);
        p.Add("orderDir", orderDir);
        WebSocketManager.defaultCli.Send(p);
    }

    protected override void Awake()
    {
        base.Awake();
        instance = this;
        scroll.updateItem = (rect, data) =>
        {
            JObject obj = data as JObject;
            rect.GetComponent<ChatBalloon>().SetData(DataToRoomUserData(obj),obj.ValueOrDefault("msg",""),obj.ValueOrDefault("type","msg"),obj.ValueOrDefault("createdAt",""));
            rect.localScale = Vector3.one;
        };
        scroll.lastUpdate = (first) =>
        {
            if (!first)
            {
                RequestCafeChatList((scroll.dataArray.Last as JObject).ValueOrDefault("idx", -1), "ASC");
            }
            else
            {
                RequestCafeChatList((scroll.dataArray.First as JObject).ValueOrDefault("idx", -1), "DESC");
            }
        };
        scroll.outOfRange = (first) =>
        {
            RequestCafeChatList((scroll.dataArray.Last as JObject).ValueOrDefault("idx", -1), "ASC");
        };
    }

    public void Send()
    {
        if (input.text == "") return;
        var p = new Packet(CPProtocol.CP_CAFE_CHAT);
        p.Add("cafeIdx", cafeIdx);
        p.Add("type", "msg");
        p.Add("to_gid", MyStatus.gid);
        p.Add("nick", MyStatus.nick);
        p.Add("msg", input.text);
        input.text = "";
        WebSocketManager.defaultCli.Send(p);
    }

    private void UpdateScroll()
    {
        scroll.dataArray = Sort(scroll.dataArray);
        scroll.Refresh();
    }

    private JArray Sort(JArray before)
    {
        return new JArray(before.OrderBy(obj => (obj as JObject).ValueOrDefault("idx",0)));
    }


    private void AddMessageData(params JObject[] chatList)
    {
        for(int i = 0; i < chatList.Length; i++)
        {
            int idx = -1;
            JObject chat = chatList[i];
            for (int j = 0; j < scroll.dataArray.Count; j++)
            {
                if ((scroll.dataArray[j] as JObject).ValueOrDefault("idx", 0) == chat.ValueOrDefault("idx", 0))
                {
                    idx = j;
                    scroll.dataArray[j] = chat;
                    break;
                }
            }
            if (idx == -1)
            {
                scroll.dataArray.Add(chat);
            }
        }
        UpdateScroll();
    }

    protected override void ReceivePacket(Packet packet)
    {
        base.ReceivePacket(packet);
        JObject c = packet.c;
        switch ((PCProtocol)packet.p)
        {
            case PCProtocol.PC_CAFE_CHAT:
                if (!c.ContainsKey("chat"))
                    break;
                JObject chat = c["chat"] as JObject;
                if (chat.ValueOrDefault("cafeIdx", 0) != cafeIdx)
                    break;
                if (chat.ValueOrDefault("type", "msg") == "emo")
                {
                    bool go_break = false;
                    for (int i = 0; i < ignore_emoticons.Count; i++)
                    {
                        if (ignore_emoticons[i].ToString() == chat.ValueOrDefault("msg", ""))
                        {
                            go_break = true;
                            break;
                        }
                    }
                    if (go_break)
                        break;
                }
                AddMessageData(chat);
                break;
            case PCProtocol.PC_CAFE_CHAT_LIST:
                if (c.ValueOrDefault("cafeIdx", 0) != cafeIdx)
                    break;
                if(c.ContainsKey("chatList"))
                {
                    JArray chatList = c["chatList"] as JArray;
                    if(chatList.Count > 0)
                    {
                        AddMessageData(chatList.ToObject<JObject[]>());
                    }
                }
                break;
        }
    }
}


