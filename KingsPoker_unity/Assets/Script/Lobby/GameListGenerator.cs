using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;
using Newtonsoft;
using System.Linq;

public class GameListGenerator : MonoBehaviour
{
    private static List<GameListGenerator> generators = new List<GameListGenerator>();
    [SerializeField]
    private List<CHIP_TYPE> chipTypes = new List<CHIP_TYPE> { CHIP_TYPE.dc, CHIP_TYPE.cc };//new List<CHIP_TYPE>();

    public InfiniteScroll scroll;
    public ItemControllerServerCommunication itemController;
    public static int pagePer = 50;

    private int cafeIdx = -1;


    public void RequestGameList()
    {
        Packet p = new Packet((int)CPProtocol.CP_PLAY_GAME_LIST);
        p.Add("startGtn", -1);
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
    private JArray GetFilterdData(JArray list)
    {
        List<JObject> filterd = list.ToObject<List<JObject>>().Where(obj =>
        {
            var chipType = obj.ValueOrDefault("chip_type", CHIP_TYPE.nothing);
            return chipTypes.Contains(chipType);
            
        }).ToList();

            return JArray.FromObject(filterd);
    }
    private JArray GetSortedData(JArray list)
    {
        List<JObject> sorted = list.ToObject<List<JObject>>().OrderBy(obj => {

            // 2021-04-25 Alberto 
            // 1.  카페의 leftSeconds가 0 이면  맨 밑으로 간다.
            // 2.  테이블이 바로 게임에 참여할수 없다면(e.g. 아무도 없거나, 꽉찼거나 ), 아래로 내린다. 

            float player_count = (float)obj["player_count"];
            float personnel = (float)obj["personnel"];
            float score = player_count / personnel;

            if (score == 0 || score == 1) score = 1.0f;  // 아무도 없거나 꽉 차있으면, 맨 뒤로 미룬다. Alberto 2021-04-17
            else
            {
                Debug.Log(score);
            }

            // 모든 score값은 1.0보다 같거나 작은 값을 가지게 된다. 

            int temp_cafeIdx = (int)obj["cafeIdx"];
            CafeInfo cafeInfo = Cafe.instance.GetCafeList((int)temp_cafeIdx);

            if (cafeInfo != null && (int)cafeInfo.info["leftSeconds"] <= 0)
            {
                score += temp_cafeIdx; // 운영이 중지된 cafe는 cafeIdx 순서대로 밑으로 내린다. 
            }

            return score;

        }).ThenBy(obj => {
            int game_type = (int)obj["game_type"];
            return game_type;
        }).ToList();

        return JArray.FromObject(sorted);
    }


    public void SetGameList(JArray list, int cafeIdx = -1)//ReceivePlayGameList
    {
        this.cafeIdx = cafeIdx;

        list = GetFilterdData(list);

        list = GetSortedData(list);

        itemController.dataArray = list;
        itemController.Refresh();

    }

    public void Awake()
    {
        generators.Add(this);
        itemController.updateItemCallback = (go, data) =>
        {
            PlayGameInfo playInfo = null;
            PlayGameItem ci = go.GetComponent<PlayGameItem>();
            if (ci.playGameInfo == null)
                playInfo = new PlayGameInfo(null);
            else
                playInfo = ci.playGameInfo;
            playInfo.info = data as JObject;
            ci.Set(playInfo, this.cafeIdx);
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
        for(int i = 0; i < generators.Count; i++)
        {
            generators[i].RefreshGameList();
        }
    }

    public void AddGameList(JObject gameData)
    {
        var isTournament = gameData.ValueOrDefault("tn", 0) > 0;
        if(isTournament)
        {
            return;
        }
        var chipType = gameData.ValueOrDefault<CHIP_TYPE>("chip_type", CHIP_TYPE.nothing);
        if(chipTypes.Contains(chipType))
        {
            itemController.dataArray.Add(gameData);
            itemController.dataArray = GetSortedData(itemController.dataArray);
            itemController.Refresh();
        }
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
        for(int i = arr.Count - 1; i >= 0; i--)
        {
            if((arr[i] as JObject).ValueOrDefault("gtn",0) == roomNumber)
            {
                arr.RemoveAt(i);
            }
        }
        itemController.Refresh();
    }

    public void UpdatePlayGamePlayerCount(long gtn,int playerCount)
    {
        if(itemController.dataArray != null)
        {
            for(int i = 0; i < itemController.dataArray.Count; i++)
            {
                if((int)itemController.dataArray[i]["gtn"] == gtn)
                {
                    itemController.dataArray[i]["player_count"] = playerCount;
                }
            }
            itemController.dataArray = GetSortedData(itemController.dataArray);
        }
        itemController.Refresh();
        return;
    }
}
