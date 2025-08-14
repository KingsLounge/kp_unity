using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ChipData {
    public Sprite sprite;
    public long price = 1;
    public bool isCircle = true;
}
public class ChipCounting
{
    public ChipData data;
    public int count;
    public ChipCounting(ChipData data, int count)
    {
        this.data = data;
        this.count = count;
    }
}

[CreateAssetMenu(fileName = "ChipDataList", menuName = "ChipDataList")]
public class ChipDataList : ScriptableObject
{
    public List<ChipData> list = new List<ChipData>();
    public int maxThrowChip = 15;

    public List<ChipData> MoneyToChip(long money) {
        List<ChipData> result = new List<ChipData>();
        list.Sort(delegate(ChipData a,ChipData b) {
            return (a.price < b.price) ? -1 : 1;
        });
        int counting = 0;
        bool goBreak = false;
        for(int i = list.Count - 1; i >= 0; i--) {
            long chipCount = money / list[i].price;
            money %= list[i].price;
            for(int j = 0; j < chipCount; j++)
            {
                result.Add(list[i]);
                counting++;
                if(counting >= maxThrowChip)
                {
                    goBreak = true;
                    break;
                }
            }
            if(goBreak)
            {
                break;
            }
        }
        return result;
    }

    public List<ChipCounting> MoneyToChipOfCount(long money)
    {
        List<ChipCounting> result = new List<ChipCounting>();
        list.Sort(delegate (ChipData a, ChipData b) {
            return (a.price < b.price) ? -1 : 1;
        });
        for (int i = list.Count - 1; i >= 0; i--)
        {
            int chipCount = (int)(money / list[i].price);
            money %= list[i].price;
            if (chipCount > 0) 
            {
                result.Add(new ChipCounting(list[i], chipCount));
            }
        }
        return result;
    }
}
