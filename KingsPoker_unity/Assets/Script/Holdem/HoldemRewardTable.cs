using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Newtonsoft.Json.Linq;
public class HoldemRewardTable : MonoBehaviour
{
    private static List<TnmtRewardData> rewardDatas;
    
    
    public static void  SetTnmtRewardTable(JObject c)
    {
//        Console.Error(c.ToJson());
        JObject Jdata = c["data"] as JObject;
        List<string> stringKeys = new List<string>();
        foreach(JProperty j in Jdata.Properties())
        {
            stringKeys.Add(j.Name);
        }
        rewardDatas = new List<TnmtRewardData>();
        for(int i = 0; i < stringKeys.Count; i++)
        {
            var data = new TnmtRewardData();
            data.num = int.Parse(stringKeys[i]);
            rewardDatas.Add(data);
        }
        rewardDatas.Sort(Compare);
        
        for(int i = 0; i < rewardDatas.Count; i++)
        {
            TnmtRewardData data = rewardDatas[i];
            JObject tableData;
            tableData = Jdata[data.num.ToString()] as JObject;
            data.tableReward = new Dictionary<int, float>();
            List<string> rewardKeys = new List<string>();
            foreach (JProperty j in tableData.Properties())
            {
                rewardKeys.Add(j.Name);
            }

            for(int j = 0; j < rewardKeys.Count; j++)
            {
                float per = (float)tableData[rewardKeys[j]];
                data.tableReward.Add(int.Parse(rewardKeys[j]), per);
            }
        }
        
        int Compare(TnmtRewardData x, TnmtRewardData y)
        {
            return x.num>y.num?1:-1;
        }   
    }

    public static List<long> GetRewardTable(int num, long totalReward)
    {
        var list = new List<long>();
        TnmtRewardData data = rewardDatas[0];
        for(int i = 0; i < rewardDatas.Count; i++)
        {
            if(num < rewardDatas[i].num)
            {
                data = rewardDatas[i];
                break;
            }
        }
        var table = data.tableReward;
        var keys = table.Keys.ToList();
        
        keys.Sort();
        var k = 0;
        for(int i = 0; i < keys.Count; i++)
        {
            while(k < keys[i])
            {
                list.Add((long)(table[keys[i]] * totalReward / 100));
                k++;
            }
            
        }

        return list;
    }

    public class TnmtRewardData
    {
        public int num;
        public Dictionary<int,float> tableReward;
    }
}
