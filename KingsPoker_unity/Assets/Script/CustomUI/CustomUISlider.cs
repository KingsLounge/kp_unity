using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json.Linq;

public class CustomUISlider : CustomUI
{
    public Text title;
    public Slider slider;
    private JTokenType dataType = JTokenType.Integer;
    private float prev = -1;
    public Text valueText;
    private string format = "";
    protected override string defaultStyle => "unit={0:#,##0}%";
    public virtual void OnChangeValue()
    {
        CleanValue();
        EmitEvent("changed");
    }

    private void CleanValue()
    {
        slider.value = (long)slider.value;
        UpdateText();
        prev = slider.value;
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
                    format = style.Value.GetLocalizedFormatString();
                    UpdateText();
                    break;
            }
        }
    }

    public void UpdateText()
    {
        valueText.text = string.Format(format, slider.value);
    }
    public void Update()
    {
        if(prev != slider.value)
        {
            OnChangeValue();
        }
    }

    public override void SetUI(JObject data)
    {
        base.SetUI(data);
        if(title)
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
        if (data.ContainsKey("min"))
        {
            slider.minValue = (float)data["min"];
        }
        if (data.ContainsKey("max"))
        {
            slider.maxValue = (float)data["max"];
        }
        else {
            slider.maxValue = slider.minValue;
        }
        if(data.ContainsKey("default"))
        {
            slider.value = (float)(data["default"]);
        }
        else
        {
            slider.value = 0f;
        }
        if(data.ContainsKey("data_type"))
        {
            string type = data["data_type"].ToString();
            if (type == "float") dataType = JTokenType.Float;
            else if (type == "int") dataType = JTokenType.Integer;
            else dataType = JTokenType.Float;
        }
        else dataType = JTokenType.Float;
        CleanValue();
    }
    public override JToken GetValue()
    {
        float value = slider.value;
        if (dataType == JTokenType.Integer)
        {
            long val = (long)value;
            return val;
        }
        else
        {
            return value;
        }
    }

    public override void SetValue(JToken d) {
        base.SetValue(d);
        slider.value = (float)d;
    }
}
