using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.UI;

public class CustomUIRadioSlider : CustomUI
{
    private JArray arr;
    public Slider slider;
    public Text title;
    public GameObject splitmark;
    public Text valueText;
    private string format = "";
    private float prev = -1;
    private bool handling = false;
    protected override string defaultStyle => "unit={0}";

    public override JToken GetValue()
    {
        int idx = (slider.value / (1f / (arr.Count - 1))).ToInt32();
        return arr[idx];
    }

    public override void SetValue(JToken d)
    {
        base.SetValue(d);
        int idx = -1;
        for(int i = 0; i < arr.Count; i++)
        {
            if(JToken.DeepEquals(arr[i], d))
            {
                idx = i;
            }
        }
        if(idx != -1)
        {
            slider.value = idx * (1f / (arr.Count - 1));
            UpdateText();
        }
    }

    public void Update()
    {
        if (prev != slider.value)
        {
            if(!handling && Input.GetMouseButton(0))
            {
                handling = true;
            }
            else
            {
                try
                {
                    OnChangeValue();
                }
                catch
                {
                    //Debug.Log("here");
                }
            }
        }
        if(handling)
        {
            if(Input.GetMouseButtonUp(0))
            {
                handling = false;
            }
            try
            {
                OnChangeValue();
            }
            catch
            {
                //Debug.Log("here");
            }
        }
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

    public virtual void OnChangeValue()
    {
        int idx = (slider.value / (1f / (arr.Count - 1))).ToInt32();
        slider.value = idx * (1f / (arr.Count - 1));
        prev = slider.value;
        UpdateText();
        EmitEvent("changed");
    }

    public void UpdateText()
    {
        int idx = (slider.value / (1f / (arr.Count - 1))).ToInt32();
        float value = arr[idx].ToObject<float>();
        switch (key)
        {
            case "blind":
                valueText.text = $"{string.Format(format, value / 2)}/{string.Format(format, value)}";
                break;
            default:

                long temp_value = arr[idx].ToObject<long>();

                if ( temp_value >= 1000000000) // 10억.
                {
                    valueText.text = LocalizeManager.GetLocalString("infinity");
                }
                else
                {
                    valueText.text = string.Format(format, value);
                }
                
                break;
        }
    }

    public override void SetUI(JObject data)
    {
        base.SetUI(data);
        if (data.ContainsKey("title"))
        {
            title.text = LocalizeManager.GetLocalString(data["title"].ToString());
        }
        else
        {
            title.text = key;
        }
        if (data.ContainsKey("data"))
        {
            arr = data["data"] as JArray;
            if(splitmark)
            {
                Transform parent = splitmark.transform.parent;
                for(int i = parent.childCount - 1; i > arr.Count; i--)
                {
                    GameObject.Destroy(parent.GetChild(i).gameObject);
                }
                for (int i = parent.childCount; i < arr.Count; i++)
                {
                    GameObject go = Instantiate(splitmark,parent);
                }
            }
        }
        if(data.ContainsKey("default"))
        {
            slider.value = data.ValueOrDefault<int>("default",0) * (1f / arr.Count);
        }
        OnChangeValue();
    }
    
}
