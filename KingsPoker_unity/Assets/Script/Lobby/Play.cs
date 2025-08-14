using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;


public class PlayGameInfo  // Play 탭에 있는 Game Info
{
    public JObject info;
    public PlayGameInfo(JObject info) {
        this.info = info;
    }
}





public class Play : WebsocketListenBehaviour
{
    public GameListGenerator gameList;
    public TnmtListGenerator tnmtList;
    private JArray playGameList;
    private JArray playTnmtList;
    protected override void Awake()
    {
        base.Awake();
        gameList.changePageCallback = (next) =>
        {
            if (next)
            {
                // 이미 모든것을 받았기 때문에 제거 한다. Alberto 2021 - 04 - 17
                var p = new Packet((int)CPProtocol.CP_PLAY_GAME_LIST);
                p.Add("startGtn", (int)playGameList.Last["gtn"]);
                p.Add("per", GameListGenerator.pagePer);
                WebSocketManager.defaultCli.Send(p.ToJson());
            }
        };
        tnmtList.changePageCallback = (next) =>
        {
            if (next)
            {
                // 이미 모든것을 받았기 때문에 제거 한다. Alberto 2021 - 04 - 17
                var p = new Packet((int)CPProtocol.CP_TNMT_LIST);
                p.Add("startTn", (int)playGameList.Last["tn"]);
                p.Add("per", GameListGenerator.pagePer);
                WebSocketManager.defaultCli.Send(p.ToJson());
            }
        };
    }

    protected void Start()
    {
        Cafe.instance.bannedEvent += (int cafeIdx) =>
        {
            if(playGameList != null)
            {
                for(int i = playGameList.Count - 1; i >= 0; i--)
                {
                    JObject t = playGameList[i] as JObject;
                    if (t.ValueOrDefault("cafeIdx", -1) == cafeIdx)
                    {
                        playGameList.RemoveAt(i);
                    }
                }
                gameList.SetGameList(playGameList);
            }
            if (playTnmtList != null)
            {
                for (int i = playTnmtList.Count - 1; i >= 0; i--)
                {
                    JObject t = playTnmtList[i] as JObject;
                    if (t.ValueOrDefault("cafeIdx", -1) == cafeIdx)
                    {
                        playTnmtList.RemoveAt(i);
                    }
                }
                tnmtList.SetGameList(playTnmtList);
            }
        };
    }

    protected override void ReceivePacket(Packet packet)
    {
        base.ReceivePacket(packet);
        JObject c = packet.c;
        switch ((PCProtocol)packet.p)
        {
            case PCProtocol.PC_PLAY_GAME_LIST:
                ReceivePlayGameList(c);
                JArray playGameList = (c["playGameList"] as JArray);
                if (c.ValueOrDefault("per",GameListGenerator.pagePer) <= playGameList.Count)
                {
                    var p = new Packet((int)CPProtocol.CP_PLAY_GAME_LIST);
                    p.Add("startGtn", (int)playGameList.Last["gtn"]);
                    p.Add("per", GameListGenerator.pagePer);
                    WebSocketManager.defaultCli.Send(p.ToJson());
                }
                break;
            case PCProtocol.PC_TNMT_LIST:
                ReceivePlayTnmtList(c);
                if (!c.ContainsKey("playTournamentList")) break;
                JArray tnmtList = (c["playTournamentList"] as JArray);
                if (c.ValueOrDefault("per", GameListGenerator.pagePer) <= tnmtList.Count)
                {
                    var p = new Packet((int)CPProtocol.CP_TNMT_LIST);
                    p.Add("startTn", (int)tnmtList.Last["tn"]);
                    p.Add("per", GameListGenerator.pagePer);
                    WebSocketManager.defaultCli.Send(p.ToJson());
                }
                break;
            case PCProtocol.PC_GAME_PLAYER_COUNT:
                UpdatePlayGamePlayerCount(c);
                break;

            case PCProtocol.PC_ROOM_DELETE:
                {
                    gameList.RemoveGame(c.ValueOrDefault("gtn", 0));
                }
                break;
            case PCProtocol.PC_CAFE_GAME_UPDATE:
                {
                    JObject o = c["o"] as JObject;
                    if(o != null)
                    {
                        GAME_TYPE game_type = (GAME_TYPE)System.Enum.Parse(typeof(GAME_TYPE), o["game_type"].ToString());
                        int gtn = (int)o["gtn"];
                        string msg = LocalizeManager.GetLocalString("game_option_changed");
                        var room = InfoManager.Instance.GetRoom(gtn);
                        if (room != null && room.tn == 0)
                            NormalMessage.instance.AddSimpleMessage(string.Format("${0} #{1} {2}", LocalizeManager.GetLocalString(game_type.ToString()), gtn, msg));

                        gameList.UpdateGame(o);
                        GameListGenerator.RefreshAllGenerators();   
                    }
                }
                break;
            case PCProtocol.PC_CAFE_GAME_CREATE:
                gameList.AddGameList(c["playGame"] as JObject);
                break;

        }
    }

    private void ReceivePlayGameList(JObject c)
    {
        JArray arr = c["playGameList"] as JArray;

        if (playGameList == null)
        {
            playGameList = new JArray();
        }
        int[] gtns = new int[playGameList.Count];
        for(int i = 0; i < playGameList.Count; i++)
        {
            gtns[i] = (int)playGameList[i]["gtn"];
        }
        for(int i = 0; i < arr.Count; i++)
        {
            int idx = System.Array.IndexOf(gtns, (int)arr[i]["gtn"]);
            if(idx == -1)
            {
                playGameList.Add(arr[i]);
            }
            else
            {
                playGameList[idx] = arr[i];
            }
        }
        gameList.SetGameList(playGameList);
    }

    private void ReceivePlayTnmtList(JObject c)
    {
        if (!c.ContainsKey("playTournamentList"))
            return;
        JArray arr = c["playTournamentList"] as JArray;

        if (playTnmtList == null)
        {
            playTnmtList = new JArray();
        }
        int[] tns = new int[playTnmtList.Count];
        for (int i = 0; i < playTnmtList.Count; i++)
        {
            tns[i] = (int)playTnmtList[i]["tn"];
        }
        for (int i = 0; i < arr.Count; i++)
        {
            int idx = System.Array.IndexOf(tns, (int)arr[i]["tn"]);
            if (idx == -1)
            {
                playTnmtList.Add(arr[i]);
            }
            else
            {
                playTnmtList[idx] = arr[i];
            }
        }
        tnmtList.SetGameList(playTnmtList);





        //TnmtUI.playTournamentList.Clear();
        //for (int i = 0; i < arr.Count; i++)
        //{
        //    Tnmt tnmt = new Tnmt();
        //    tnmt.tn = (int)arr[i]["tn"];
        //    tnmt.cafeIdx = (int)arr[i]["cafeIdx"];
        //    long buyin = (long)arr[i]["t_buyin"];
        //    long buyin_fee = (long)arr[i]["t_buyin_fee"];
        //    tnmt.buyIn = buyin + buyin_fee;
        //    tnmt.startTime = (string)arr[i]["t_start_time"];
        //    tnmt.closeTime = (string)arr[i]["t_close_time"];
        //    TnmtUI.playTournamentList.Add(tnmt);
        //}
    }

    private void ReceiveCafeMemberList(JObject c)
    {

    }

    private void UpdatePlayGamePlayerCount(JObject c)
    {

        long gtn = (long)c["gtn"];
        int  playerCount = (int)c["player_count"];

        gameList.UpdatePlayGamePlayerCount(gtn, playerCount);
    }


    public void OnClickCloseButton()
    {
        //ani.Play("ShopCloseAni");
    }
    public void CloseWindow()
    {
        gameObject.SetActive(false);
    }
}
