using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;
using Newtonsoft;
using System.Linq;

public class TnmtListGenerator : MonoBehaviour
{
    private static List<TnmtListGenerator> generators = new List<TnmtListGenerator>();

    public InfiniteScroll scroll;
    public ItemControllerServerCommunication itemController;
    public static int pagePer = 50;

    private int cafeIdx = -1;


    public void RequestGameList()
    {
        Packet p = new Packet((int)CPProtocol.CP_TNMT_LIST);
        p.Add("startTn", -1);
        p.Add("per", GameListGenerator.pagePer);
        WebSocketManager.defaultCli.Send(p.ToJson());

    }
    public ItemControllerServerCommunication.ChangePageDelegate changePageCallback
    {
        get
        {
            return itemController.changePageCallback;
        }
        set
        {
            itemController.changePageCallback = value;
        }
    }
    private int StateToSortOerder(TNMT_FLOW flow)
    {
        int order = (int)flow;
        switch(flow)
        {
            case TNMT_FLOW.none:
                order = 21;
                break;
            case TNMT_FLOW.open:
                order = 20;
                break;
            case TNMT_FLOW.start:
                order = 10;
                break;
        }
        return order;
    }
    private JArray GetSortedData(JArray list)
    {
        var sorted = list.ToObject<List<JObject>>();
        sorted.Sort((JObject a, JObject b) =>
        {

            var a_flow = StateToSortOerder(a.ValueOrDefault("state", TNMT_FLOW.none));
            var b_flow = StateToSortOerder(b.ValueOrDefault("state", TNMT_FLOW.none));
            var a_tn = a.ValueOrDefault("tn", 0);
            var b_tn = b.ValueOrDefault("tn", 0);
            if (a_flow <= (int)TNMT_FLOW.start && b_flow <= (int)TNMT_FLOW.start)
            {

                var a_tnmt = MyStatus.GetTnmt(a_tn);
                var b_tnmt = MyStatus.GetTnmt(b_tn);
                if (a_tnmt != null && b_tnmt == null)
                {
                    return -1;
                }
                if (a_tnmt == null && b_tnmt != null)
                {
                    return 1;
                }
                var a_tag = a.ValueOrDefault("t_tag", 0);
                var b_tag = b.ValueOrDefault("t_tag", 0);
                
                if (a_tag != b_tag)
                {
                    return TagSort(b_tag) - TagSort(a_tag);
                }
            }

            if (a_flow != b_flow)
            {
                return a_flow - b_flow;
            }


            var a_startTime_text = a.ValueOrDefault("t_start_time", string.Empty);
            var b_startTime_text = b.ValueOrDefault("t_start_time", string.Empty);
            var a_start_time = DateTimeParser.Parse(a_startTime_text);
            var b_start_time = DateTimeParser.Parse(b_startTime_text);
            if(a_start_time != b_start_time)
            {
                if (a_flow <= (int)TNMT_FLOW.start)
                {
                    if (a_start_time < b_start_time)
                    {
                        return -1;
                    }
                    else
                    {
                        return 1;
                    }
                }
                else
                {
                    if (a_start_time > b_start_time)
                    {
                        return -1;
                    }
                    else
                    {
                        return 1;
                    }
                }
            }
            return b_tn - a_tn;
           
        });
        return JArray.FromObject(sorted);

        //List<JObject> sorted = list.ToObject<List<JObject>>().OrderBy(obj => {

        //    // 2021-04-25 Alberto 
        //    // 1.  카페의 leftSeconds가 0 이면  맨 밑으로 간다.
        //    // 2.  테이블이 바로 게임에 참여할수 없다면(e.g. 아무도 없거나, 꽉찼거나 ), 아래로 내린다. 

        //    float player_count = (float)obj["player_count"];
        //    float personnel = (float)obj["personnel"];
        //    float score = player_count / personnel;

        //    if (score == 0 || score == 1) score = 1.0f;  // 아무도 없거나 꽉 차있으면, 맨 뒤로 미룬다. Alberto 2021-04-17
        //    else
        //    {
        //        Debug.Log(score);
        //    }

        //    // 모든 score값은 1.0보다 같거나 작은 값을 가지게 된다. 

        //    int temp_cafeIdx = (int)obj["cafeIdx"];
        //    CafeInfo cafeInfo = Cafe.instance.GetCafeList((int)temp_cafeIdx);

        //    if (cafeInfo != null && (int)cafeInfo.info["leftSeconds"] <= 0)
        //    {
        //        score += temp_cafeIdx; // 운영이 중지된 cafe는 cafeIdx 순서대로 밑으로 내린다. 
        //    }

        //    return score;

        //}).ThenBy(obj => {
        //    int game_type = (int)obj["game_type"];
        //    return game_type;
        //}).ToList();

        //return JArray.FromObject(sorted);


    }

    public int TagSort(int tag)
    {
        var temp_tag = 0;
        switch(tag)
        {
            case 1:
                temp_tag = 2;
                break;
            case 2:
                temp_tag = 1;
                break;
            default:
                temp_tag = tag;
                break;
        }
        return temp_tag;
    }
    
    public void SetGameList(JArray list, int cafeIdx = -1)//ReceivePlayGameList
    {
        this.cafeIdx = cafeIdx;

        list = GetSortedData(list);

        itemController.dataArray = list;
        itemController.Refresh();

    }

    public void Awake()
    {
        generators.Add(this);
        itemController.updateItemCallback = (go, data) =>
        {
            TournamentInfo tnmtInfo = null;
            TnmtItem ci = go.GetComponent<TnmtItem>();
            if (ci.tnmtInfo == null)
                tnmtInfo = new TournamentInfo(null);
            else
                tnmtInfo = ci.tnmtInfo;
            tnmtInfo.SetTournamentInfo(data as JObject);
            ci.Set(tnmtInfo, this.cafeIdx);
        };
    }

    public void OnDestroy()
    {
        generators.Remove(this);
    }

    public void RefreshGameList()
    {
        itemController.dataArray = GetSortedData(itemController.dataArray);
        itemController.Refresh();
    }

    public static void RefreshAllGenerators()
    {
        for (int i = 0; i < generators.Count; i++)
        {
            generators[i].RefreshGameList();
        }
    }

    public void AddGameList(JObject gameData)
    {
        itemController.dataArray.Add(gameData);
        itemController.dataArray = GetSortedData(itemController.dataArray);
        itemController.Refresh();
    }
    public void UpdateGame(JObject gameData)
    {
        int gtn = (int)gameData["gtn"];

        if (itemController.dataArray != null)
        {
            for (int i = 0; i < itemController.dataArray.Count; i++)
            {
                if ((int)itemController.dataArray[i]["gtn"] == gtn)
                {
                    itemController.dataArray[i] = gameData;
                    itemController.dataArray = GetSortedData(itemController.dataArray);
                    itemController.Refresh();
                    break;
                }
            }
        }
    }

    public void RemoveGame(int roomNumber)
    {
        JArray arr = itemController.dataArray;
        for (int i = arr.Count - 1; i >= 0; i--)
        {
            if ((arr[i] as JObject).ValueOrDefault("gtn", 0) == roomNumber)
            {
                arr.RemoveAt(i);
            }
        }
        itemController.Refresh();
    }

    public void UpdatePlayGamePlayerCount(long gtn, int playerCount)
    {
        if (itemController.dataArray != null)
        {
            for (int i = 0; i < itemController.dataArray.Count; i++)
            {
                if ((int)itemController.dataArray[i]["gtn"] == gtn)
                {
                    itemController.dataArray[i]["player_count"] = playerCount;
                }
            }
            itemController.dataArray = GetSortedData(itemController.dataArray);
        }
        itemController.Refresh();
        return;
    }
    public void UpdateTournamentApplyCount(long tn, long countAllUser)
    {
        if (itemController.dataArray != null)
        {
            for (int i = 0; i < itemController.dataArray.Count; i++)
            {
                if ((int)itemController.dataArray[i]["tn"] == tn)
                {
                    var data = itemController.dataArray[i] as JObject;
                    var live = data.CastOrEmpty<JObject>("live");
                    live["countEntry"] = countAllUser;
                    data["live"] = live;
                    data["countAllUser"] = countAllUser;
                }
            }
            itemController.dataArray = GetSortedData(itemController.dataArray);
        }
        itemController.Refresh();
    }
}
