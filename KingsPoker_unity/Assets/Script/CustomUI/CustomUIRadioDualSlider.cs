using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class CustomUIRadioDualSlider : CustomUIDualSlider
{
    private JArray dataArr = new JArray();

    private void Awake()
    {
        dualSlider.textFormatter = (value) =>
        {
            int idx = ((float)value).ToInt32();
            if (dataArr.Count > idx)
            {
                if (dataArr[idx].Type == JTokenType.Integer && (long)dataArr[idx] >= 100000000)
                    return LocalizeManager.GetLocalString("infinite");
                return string.Format(textFormat, dataArr[idx]);
            }
            else
            {
                return string.Format(textFormat, idx);
            }
        };
    }

    public override void SetUI(JObject data)
    {
        base.SetUI(data);
        dataArr = data.ValueOrDefault("data", new JArray(new int[2] { 0, 1 }));
        dualSlider.SetLimits(0, dataArr.Count - 1);
        dualSlider.SetValues(0, dataArr.Count - 1);
        dataType = JTokenType.Integer;
    }

    public override JToken GetValue()
    {
        return new JArray(dataArr[(int)dualSlider.minValue], dataArr[(int)dualSlider.maxValue]);
    }

    public override void SetValue(JToken d)
    {
        JArray arr = JArray.Parse(d.ToString());
        if (arr.Count > 0)
        {
            int minIdx = 0;
            int maxIdx = 0;
            for (int i = 0; i < dataArr.Count; i++)
            {
                if (dataArr[i].Equals(arr[0]))
                    minIdx = i;
                if (dataArr[i].Equals(arr[1]))
                    maxIdx = i;
            }
            dualSlider.SetValues(minIdx, maxIdx);
        }
    }

    public JToken GetIndex()
    {
        return new JArray((int)dualSlider.minValue, (int)dualSlider.maxValue);
    }

    public void SetIndex(JToken d)
    {
        JArray arr = JArray.Parse(d.ToString());
        if (arr.Count > 0)
        {
            int minIdx = (int)arr[0];
            int maxIdx = (int)arr[1];
            dualSlider.SetValues(minIdx, maxIdx);
        }
    }
}
