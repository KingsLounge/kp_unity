using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.UI;

public class CustomUIFulldown : CustomUI
{
    public Dropdown dropdown;
    private ScrollRect scroll;
    private JArray optionsArr;
    public RectTransform item;
    private int prev = 0;
    protected override string defaultStyle => "height=75";
    private float dropdownMaxHeight = 0;

    private void Awake()
    {
        dropdownMaxHeight = dropdown.template.rect.height;
        scroll = dropdown.template.GetComponent<ScrollRect>();
    }

    public override JToken GetValue()
    {
        return optionsArr[dropdown.value];
    }

    public override void SetValue(JToken d)
    {
        base.SetValue(d);
        int idx = -1;
        for(int i = 0; i < optionsArr.Count; i++)
        {
            if(JToken.DeepEquals(optionsArr[i], d))
            {
                idx = i;
            }
        }
        if(idx != -1)
        {
            dropdown.value = idx;
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
                case "height":
                    RectTransform rt = (transform as RectTransform);
                    Vector2 size = rt.sizeDelta;
                    size.y = float.Parse(style.Value);
                    rt.sizeDelta = size;
                    break;
            }
        }
    }

    public override void SetUI(JObject data)
    {
        base.SetUI(data);
        if(data.ContainsKey("data"))
        {
            JArray optionArr = data["data"].DeepClone() as JArray;
            ChangeOptions(optionArr);
        }
        if(data.ContainsKey("default"))
        {
            dropdown.value = (int)data["default"];
        }
    }

    public void ChangeOptions(JArray optionArr)
    {
        this.optionsArr = optionArr;
        dropdown.ClearOptions();
        List<string> options = new List<string>();
        for (int i = 0; i < optionArr.Count; i++)
        {
            options.Add(LocalizeManager.GetLocalString(optionArr[i].ToString()));
        }
        dropdown.AddOptions(options);
        Vector2 size = dropdown.template.sizeDelta;
        float itemHeight = item.sizeDelta.y * optionArr.Count;
        size.y = itemHeight < dropdownMaxHeight ? itemHeight : dropdownMaxHeight;
        dropdown.template.sizeDelta = size;
        dropdown.value = 0;
    }

    protected virtual void OnChangeValue()
    {
        prev = dropdown.value;
        EmitEvent("changed");
    }

    private void Update()
    {
        if(dropdown.value != prev)
        {
            OnChangeValue();
        }
    }
}
