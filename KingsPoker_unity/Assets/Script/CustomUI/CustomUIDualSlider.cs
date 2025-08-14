using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.UI;

public class CustomUIDualSlider : CustomUI
{
    public Text title;
    public DualSlider dualSlider;
    protected JTokenType dataType = JTokenType.None;
    protected string textFormat = "";
    protected override string defaultStyle => "unit={0:0}";

    public void Update()
    {
        if(dualSlider.handling)
        {
            if(Input.GetMouseButtonUp(0))
                CleanValue();
            OnChangeValue();
        }
    }

    protected virtual void OnChangeValue()
    {
        EmitEvent("changed");
    }

    protected override void SetStyle(List<KeyValuePair<string, string>> data)
    {
        base.SetStyle(data);
        for (int i = 0; i < data.Count; i++)
        {
            KeyValuePair<string, string> style = data[i];
            switch (style.Key)
            {
                case "unit":
                    textFormat = style.Value.GetLocalizedFormatString();
                    dualSlider.textFormat = style.Value.GetLocalizedFormatString();
                    break;
            }
        }
    }

    public override void SetUI(JObject data)
    {
        base.SetUI(data);
        if (title)
        {
            if (data.ContainsKey("title"))
            {
                title.text = LocalizeManager.GetLocalString(data["title"].ToString());
            }
            else
            {
                title.text = key;
            }
        }
        double min = 0;
        double max = 1;
        min = data.ValueOrDefault<double>("min", min);
        max = data.ValueOrDefault<double>("max", min + 1);
        //if (data.ContainsKey("min"))
        //{
           
        //}
        //if (data.ContainsKey("max"))
        //{
        //    max = (double)data["max"];
        //}
        //else
        //{
        //    max = min + 1;
        //}
        dualSlider.SetLimits(min, max);
        if (data.ContainsKey("default"))
        {
            JArray minMax = data["default"] as JArray;
            dualSlider.SetValues((double)minMax[0], (double)minMax[1]);
        }
        else
        {
            dualSlider.SetValues(min,max);
        }
        if (data.ContainsKey("data_type"))
        {
            string type = data["data_type"].ToString();
            if (type == "double")
            {
                dataType = JTokenType.Float;
                dualSlider.textFormat = "0.00";
            }
            else if (type == "int")
            {
                dataType = JTokenType.Integer;
                dualSlider.textFormat = "0";
            }
            else
            {
                dataType = JTokenType.Float;
                dualSlider.textFormat = "0.00";
            }
        }
        else
        {
            dataType = JTokenType.Float;
            dualSlider.textFormat = "0.00";
        }
        CleanValue();
    }

    protected void CleanValue()
    {
        double min = dualSlider.minValue;
        double max = dualSlider.maxValue;
        if(dataType == JTokenType.Integer)
        {
            min = min.ToInt64();
            max = max.ToInt64();
        }
        dualSlider.SetValues(min, max);
    }

    public override void CompleteSetting()
    {
        base.CompleteSetting();
    }

    public override void SetMin(JToken d)
    {
        base.SetMin(d);
        double minPer = 0;
        double maxPer = 1;
        double newMin = (double)d;
        dualSlider.SetLimits(newMin, dualSlider.maxLimit);
        dualSlider.SetValues(((dualSlider.maxLimit - newMin) * minPer) + newMin, ((dualSlider.maxLimit - newMin) * maxPer) + newMin);
    }

    public override void SetMax(JToken d)
    {
        base.SetMax(d);
        double minPer = 0;
        double maxPer = 1;
        double newMax = (double)d;
        dualSlider.SetLimits(dualSlider.minLimit, newMax);
        dualSlider.SetValues(((newMax - dualSlider.minLimit) * minPer) + dualSlider.minLimit, ((newMax - dualSlider.minLimit) * maxPer) + dualSlider.minLimit);
    }

    public override JToken GetValue()
    {
        if(dataType == JTokenType.Integer)
        {
            return new JArray(new long[2] { (long)dualSlider.minValue, (long)dualSlider.maxValue });
        }
        return new JArray(new double[2] { dualSlider.minValue, dualSlider.maxValue });
    }

    public override void SetValue(JToken d)
    {
        base.SetValue(d);
        JArray arr = JArray.Parse(d.ToString());
        if(arr.Count > 0)
        {
            dualSlider.SetValues((double)arr[0], (double)arr[1]);
        }
    }
}
