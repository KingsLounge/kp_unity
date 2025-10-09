using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class TableDataManager : MonoBehaviour
{
    static string SPLIT_RE = @",(?=(?:[^""]*""[^""]*"")*(?![^""]*""))";
    public static Dictionary<Sound_TableEnum, JObject> soundTable;

    public static Dictionary<int, JObject> characterTable;
    public static Dictionary<int, JObject> vipTable;
    public static Dictionary<string, JObject> localTable;
    public static List<string> lanKeyList;

    public static JObject table_list;

    public static Dictionary<int, JObject> blindTables;
    public static Dictionary<int, JObject> rankTables;

    public const int blindTableCount = 9;
    public const int rankTableCount = 3;

    private void Awake()
    {
        SetTables();
    }

    public void SetTables()
    {
        SetSoundTable();
        SetCharacterTable();
        SetVipTable();
        SetLocalTable();
    }

    public static JArray GetRewardData(
        int tableNum,
        int userCount,
        long totlaPrize = 0,
        int ticket_type = 0
    )
    {
        var tableData = rankTables[tableNum];
        var table = tableData.ValueOrDefault<JObject>("table", null);
        if (table == null)
        {
            table = tableData;
        }
        var rewardData = GetRewardList(table, userCount, totlaPrize, ticket_type);
        return rewardData;
    }

    public static JArray GetRewardList(
        JObject table,
        int userCount,
        long totlaPrize = 0,
        int ticket_type = 0
    )
    {
        JObject selectTable = null;
        if (table != null)
        {
            foreach (var d in table)
            {
                if (int.Parse(d.Key) <= userCount || selectTable == null)
                {
                    selectTable = d.Value as JObject;
                }
                else
                {
                    break;
                }
            }
        }
        else
        {
            Debug.LogError($"NotFound reward Table");
        }
        var rewardData = new JArray();
        int rank = 0;
        foreach (var t in selectTable)
        {
            for (int i = rank; i < int.Parse(t.Key); ++i)
            {
                rank++;
                JObject d;
                if (t.Value.Type == JTokenType.Float || t.Value.Type == JTokenType.Integer)
                {
                    d = new JObject();
                    var ratio = (double)t.Value * 100;
                    long chip = (long)ratio * (totlaPrize / 10000);
                    d.Add("chip", chip);
                }
                else
                {
                    d = new JObject(t.Value as JObject);
                    var c_ticket_type = d.ValueOrDefault<int>("ticket_type", 0);
                    if (ticket_type > 0 && c_ticket_type == 0)
                    {
                        d["ticket_type"] = ticket_type;
                    }
                }
                d.Add("rank", rank);

                rewardData.Add(d);
            }
        }
        Console.Log($"Selected reward data : {rewardData}");
        return rewardData;
    }

    public static async UniTask GetTableList()
    {
        var url = $"{LinkOptionConstant.storageUrl}/assets/game_table_json/table_list.json";
        UnityWebRequest www = UnityWebRequest.Get(url);
        await www.SendWebRequest();
        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
        }
        else
        {
            var job = JObject.Parse(www.downloadHandler.text);
            table_list = job;
        }
    }

    public static async UniTask GetBlindTables()
    {
        blindTables = new Dictionary<int, JObject>();
        JArray blindData = table_list.CastOrEmpty<JArray>("blind_up_tables");
        for (int i = 0; i < blindData.Count; i++)
        {
            //var url = $"{LinkOptionConstant.storageUrl}/assets/game_table_json/blind_up_{i:00}.json";
            var url = $"{LinkOptionConstant.storageUrl}/assets/game_table_json/{blindData[i]}";
            UnityWebRequest www = UnityWebRequest.Get(url);
            await www.SendWebRequest();
            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
            }
            else
            {
                var job = JObject.Parse(www.downloadHandler.text);
                blindTables.Add(i, job);
            }
        }
    }

    public static async UniTask GetRankTables()
    {
        rankTables = new Dictionary<int, JObject>();
        JArray rankData = table_list.CastOrEmpty<JArray>("rank_tables");
        var storageUrl = ServerConfigManager.GetConfig("assets_url");
        var urlString = storageUrl == null ? LinkOptionConstant.storageUrl : storageUrl.ToString();
        for (int i = 0; i < rankData.Count; i++)
        {
            var url = $"{urlString}/assets/game_table_json/{rankData[i]}";
            UnityWebRequest www = UnityWebRequest.Get(url);
            await www.SendWebRequest();
            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
            }
            else
            {
                var job = JObject.Parse(www.downloadHandler.text);
                rankTables.Add(i, job);
            }
        }
    }

    public void SetSoundTable()
    {
        var dataLines = CsvReader.ReadCSV("Sound_Table");
        soundTable = new Dictionary<Sound_TableEnum, JObject>();
        string[] header = null;
        for (int i = 0; i < dataLines.Length; i++)
        {
            var data = Reconfiguration(Regex.Split(dataLines[i], SPLIT_RE));
            if (i == 0)
            {
                header = data;
            }
            else
            {
                var obj = new JObject();
                Sound_TableEnum key = Sound_TableEnum.SFX_BUTTON_01;
                for (int j = 0; j < header.Length && j < data.Length; j++)
                {
                    if (header[j] == "key")
                    {
                        try
                        {
                            key = (Sound_TableEnum)
                                System.Enum.Parse(typeof(Sound_TableEnum), data[j]);
                            soundTable.Add(key, obj);
                        }
                        catch
                        {
                            continue;
                        }
                    }
                    obj.Add(header[j], data[j]);
                }
            }
        }
    }

    public void SetVipTable()
    {
        string[] dataLines = CsvReader.ReadCSV("Character_Table");

        characterTable = new Dictionary<int, JObject>();
        string[] header = null;
        for (int i = 0; i < dataLines.Length; i++)
        {
            string[] data = Reconfiguration(Regex.Split(dataLines[i], SPLIT_RE));

            if (i == 0)
            {
                header = data;
            }
            else
            {
                var obj = new JObject();
                for (int j = 0; j < header.Length && j < data.Length; j++)
                {
                    if (header[j] == "id")
                    {
                        int key;
                        if (int.TryParse(data[j], out key))
                        {
                            characterTable.Add(key, obj);
                        }
                        else
                        {
                            continue;
                        }
                    }
                    obj.Add(header[j], data[j]);
                }
            }
        }
        //var dataLines = CsvReader.ReadCSV("vip_string");

        //vipTable = new Dictionary<int, JObject>();
        //var header = Regex.Split(dataLines[0], SPLIT_RE);
        //for (int i = 1; i < dataLines.Length; i++)
        //{
        //    var data = Regex.Split(dataLines[i], SPLIT_RE);
        //    var obj = new JObject();
        //    for (int j = 0; j < header.Length && j < data.Length; j++)
        //    {
        //        if (header[j] == "ref_idx")
        //        {
        //            int key;

        //            if (int.TryParse(data[j], out key))
        //            {
        //                vipTable.Add(key, obj);
        //            }
        //            else
        //            {
        //                continue;
        //            }
        //            Console.SpecialLog(key);
        //        }
        //        obj.Add(header[j], data[j]);
        //    }
        //}
    }

    public void SetCharacterTable()
    {
        string[] dataLines = CsvReader.ReadCSV("Character_Table");

        characterTable = new Dictionary<int, JObject>();
        string[] header = null;
        for (int i = 0; i < dataLines.Length; i++)
        {
            string[] data = Reconfiguration(Regex.Split(dataLines[i], SPLIT_RE));
            if (i == 0)
            {
                header = data;
            }
            else
            {
                var obj = new JObject();
                for (int j = 0; j < header.Length && j < data.Length; j++)
                {
                    if (header[j] == "id")
                    {
                        int key;
                        if (int.TryParse(data[j], out key))
                        {
                            characterTable.Add(key, obj);
                        }
                        else
                        {
                            continue;
                        }
                    }
                    obj.Add(header[j], data[j]);
                }
            }
        }
    }

    public void SetLocalTable()
    {
        string[] dataLines = CsvReader.ReadCSV("StringTable");
        List<JObject> jsonDatas = CSVParser.Parse(dataLines);

        localTable = new Dictionary<string, JObject>();
        for (int i = 0; i < jsonDatas.Count; i++)
        {
            JObject data = jsonDatas[i];
            string key = data.ValueOrDefault("key", "undefined");
            localTable.Set(key, data);
        }
        lanKeyList = new List<string>();
        foreach (var property in jsonDatas[0].Properties())
        {
            if (property.Name == "type" || property.Name == "key")
                continue;
            lanKeyList.Add(property.Name);
        }
    }

    private string[] Reconfiguration(string[] arr)
    {
        for (int j = 0; j < arr.Length; j++)
        {
            string newText = arr[j];
            int count = 0;
            for (int k = 0; k < newText.Length; k++)
            {
                if (newText[k] == '"')
                {
                    count++;
                    if (count == 3)
                    {
                        count = 0;
                        newText = newText.Remove(k - 1, 2);
                        k -= 2;
                    }
                    else if (count == 1 && k == newText.Length - 1)
                    {
                        newText = newText.Remove(k - 1, 1);
                        k -= 1;
                    }
                }
                else
                {
                    if (count == 1)
                    {
                        newText = newText.Remove(k - 1, 1);
                        k -= 1;
                    }
                    count = 0;
                }
            }
            arr[j] = newText;
        }
        return arr;
    }
}
