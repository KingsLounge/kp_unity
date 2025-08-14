using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.UI;

public class HandLogDetailViewer : WebsocketListenBehaviour
{
    public class UserInfoMap
    {
        public JToken p;
        public JToken c;
        public JToken i;
        public JToken n;
        public JToken u;
        public JToken s;
    }

    [System.Serializable]
    public class CommCardList
    {
        [SerializeField]
        private List<Image> list = new List<Image>();
        public Image this[int idx]
        {
            get => list[idx];
        }
        public int Count
        {
            get => list.Count;
        }
    }

    public List<CommCardList> cards = new List<CommCardList>();
    public List<Player> players = new List<Player>();
    public List<HandLogFlowInfo> flowInfos = new List<HandLogFlowInfo>();
    public RectTransform scrollRectContents = null;
    public float baseHeight = 1444;
    private List<UserInfoMap> mappingData = new List<UserInfoMap>();
    public Text text_game_type_gtn_gn;

    public static HandLogDetailViewer Instance { get; private set; }

    public HandLogDetailViewer()
    {
        Instance = this;
    }

    public void SetGame(bool cafeHandlog, int gn)
    {
        if (cafeHandlog)
        {
            Packet packet = new Packet(CPProtocol.CP_HANDS_LOG_CAFE);
            packet.Add("gn", gn);
            WebSocketManager.defaultCli.Send(packet);
        }
        else
        {
            Packet packet = new Packet(CPProtocol.CP_HANDS_LOG_MY);
            packet.Add("gn", gn);
            WebSocketManager.defaultCli.Send(packet);
        }
        gameObject.SetActive(true);
    }

    protected override void ReceivePacket(Packet packet)
    {
        base.ReceivePacket(packet);
        PCProtocol p = (PCProtocol)packet.p;
        JObject c = packet.c;
        switch (p)
        {
            case PCProtocol.PC_HANDS_LOG_CAFE:
                SetHandsLog(c["handsLog"] as JObject);
                break;
            case PCProtocol.PC_HANDS_LOG_MY:
                SetHandsLog(c["handsLog"] as JObject);
                break;
        }
    }

    public UserInfoMap GetUserData(string gid)
    {
        return mappingData.Find(value => value.p.ToString() == gid);
    }

    public UserInfoMap GetUserData(int seat)
    {
        return mappingData.Find(value => (int)value.s == seat);
        //if (mappingData.Count > seat)
        //    return mappingData[seat];
        //else
        //    return null;
    }

    private void SetHandsLog(JObject handsLog)
    {
        JObject log = handsLog["log"] as JObject;

        mappingData.Clear();

        long gtn = (int)handsLog["gtn"];
        long gn = (long)handsLog["gn"];
        int gt = (int)handsLog["gt"];
        text_game_type_gtn_gn.text = string.Format(
            "{0} #{1}.{2}",
            LocalizeManager.GetLocalString(((GAME_TYPE)gt).ToString()),
            gtn,
            gn
        );

        int cardSetNum =
            CardTextureSetter.Instance != null ? CardTextureSetter.Instance.cardSetIndex : 0;
        JArray cc = log["cc"] as JArray;
        for (int i = 0; i < cards.Count; i++)
        {
            JArray comm = cc[i] as JArray;
            for (int j = 0; j < cards[i].Count; j++)
            {
                bool active = j < comm.Count && !string.IsNullOrEmpty(comm[j].ToString());
                cards[i][j].gameObject.SetActive(active);
                if (active)
                    cards[i][j].sprite = CardSets.GetCard(comm[j].ToString(), cardSetNum);
            }
        }
        int d = log.ValueOrDefault("d", 0);
        JArray p = log["p"] as JArray;
        JArray n = log["n"] as JArray;
        JArray icon = log.CastOrEmpty<JArray>("i");
        JArray s = log["s"] as JArray;
        JArray u = log["u"] as JArray;
        JArray c = log["c"] as JArray;
        JArray a = log["a"] as JArray;
        JArray w = log["w"] as JArray;
        JArray f = log["f"] as JArray;
        int notFoldCount = 0;
        for (int i = 0; i < p.Count; ++i)
        {
            if (!string.IsNullOrEmpty(p[i].ToString()))
            {
                bool fold = false;
                for (int j = 0; j < f.Count; ++j)
                {
                    JArray folds = f[j] as JArray;
                    if ((int)folds[i] > 0)
                    {
                        fold = true;
                        break;
                    }
                }
                if (!fold)
                {
                    notFoldCount++;
                }
            }
        }

        JArray bb_ante = log.ValueOrDefault<JArray>("bb_ante", null);
        if (bb_ante == null)
        {
            bb_ante = new JArray(0, 0, 0, 0, 0, 0, 0, 0, 0);
        }
        long[] totalW = new long[(w[0] as JArray).Count];
        for (int i = 0; i < totalW.Length; i++)
            totalW[i] = 0;
        for (int i = 0; i < w.Count; i++)
        {
            JArray cur = w[i] as JArray;
            for (int j = 0; j < cur.Count; j++)
            {
                if (cur[j].Type == JTokenType.Null) { }
                else
                {
                    totalW[j] += (long)cur[j];
                }
            }
        }

        for (int i = 0; i < players.Count; i++)
        {
            string gid = p.Count > i ? p[i].ToString() : "";
            players[i].Clear();
            if (!string.IsNullOrEmpty(gid))
            {
                UserInfoMap user = new UserInfoMap();
                int icon_no = icon.Count > i ? (int)icon[i] : 0;
                user.p = p[i];
                user.u = u[i];
                user.i = icon_no;
                user.c = c[i];
                user.n = n[i];
                user.s = i;
                mappingData.Add(user);
                string url = u[i].ToString();
                long rs = (long)s[i];
                long ra = (long)a[i];

                long rw = totalW[i];

                var card = c[i].ToString();
                if (!gid.Equals(MyStatus.gid) && notFoldCount <= 1)
                {
                    card = "**,**";
                }
                players[i]
                    .SetPlayer(
                        gid,
                        n[i].ToString(),
                        rs,
                        i,
                        (int)ROOM_USER_STATUS.ingame,
                        icon_no,
                        url,
                        false
                    );

                players[i].SetCard(card);
                players[i].PlayResult("", ra, rw, rs - ra + rw, ra < rw);
                players[i].SetBoss(i == d);
            }
        }
        JArray bet = log["bet"] as JArray;
        for (int i = 0; i < flowInfos.Count; i++)
        {
            JArray flowData = (bet.Count <= i) ? new JArray() : bet[i] as JArray;
            flowInfos[i].SetBetFlow(flowData);
        }
        StartCoroutine(SetResize());
    }

    private IEnumerator SetResize()
    {
        yield return 0;
        float height = 0;
        float max_height = 0;
        for (int i = 0; i < flowInfos.Count; i++)
        {
            if (max_height < flowInfos[i].height)
            {
                max_height = flowInfos[i].height;
            }
            Debug.Log("flowInfos[i].height = " + flowInfos[i].height);
        }
        Vector2 size = scrollRectContents.sizeDelta;
        if (max_height > baseHeight)
        {
            size.y = max_height + (max_height / 20);
        }
        else
        {
            size.y = baseHeight;
        }
        size.y = max_height > baseHeight ? max_height : baseHeight;
        scrollRectContents.sizeDelta = size;

        Debug.Log("scrollRectContents.sizeDelta =  " + size);
    }
}
