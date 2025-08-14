using System.Collections;
using System.Collections.Generic;
using Jeckl.InfinityScroll;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ChatManager : WebsocketListenBehaviour
{
    [HideInInspector]
    public long roomNumber = -1;

    [SerializeField]
    private InputField input;
    public int maxLength = 100;

    [SerializeField]
    private InfinityScroll scroll;

    [SerializeField]
    private List<Emoticon> ignore_emoticons = new List<Emoticon>();
    public static ChatManager instance { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        instance = this;
        scroll.updateItem = (rect, data) =>
        {
            Vector2 prevSize = rect.sizeDelta;
            rect.GetComponent<ChatBalloon>().InGameSetData(data as JObject);
            rect.localScale = Vector3.one;
        };
    }

    public void Send()
    {
        if (input.text == "")
            return;
        var p = new Packet((int)CPProtocol.CP_ROOM_CHAT);
        p.Add("gtn", roomNumber);
        p.Add("type", "msg");
        p.Add("to_gid", MyStatus.gid);
        p.Add("msg", input.text);
        input.text = "";
        WebSocketManager.defaultCli.Send(p);
    }

    public void OnGUI()
    {
        //if(GUI.Button(new Rect(0,0,100,100),"Test"))
        //{
        //    for(int i = 0; i < 10; i++)
        //    {
        //        string t = "";
        //        for (int j = 0; j < Random.Range(0, 20); j++)
        //            t += "a";
        //        input.text = t;
        //        Send();
        //    }
        //}
    }

    protected override void ReceivePacket(Packet packet)
    {
        base.ReceivePacket(packet);
        JObject c = packet.c;
        switch ((PCProtocol)packet.p)
        {
            case PCProtocol.PC_ROOM_CHAT:
                if (c.ValueOrDefault("type", "msg") == "emo")
                {
                    bool go_break = false;
                    for (int i = 0; i < ignore_emoticons.Count; i++)
                    {
                        if (ignore_emoticons[i].ToString() == c.ValueOrDefault("msg", ""))
                        {
                            go_break = true;
                            break;
                        }
                    }
                    if (go_break)
                        break;
                }
                scroll.dataArray.Add(c);
                scroll.Refresh();
                //scroll.MoveDown();
                break;
        }
    }

#if UNITY_EDITOR
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return) && input.runInEditMode)
        {
            Send();
        }
    }
#endif
}
