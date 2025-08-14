using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.UI;

public class CustomUIToggle : CustomUI
{
    public Text title;
    public Toggle toggle;

    protected void Awake()
    {
        toggle.onValueChanged.AddListener(OnChangedToggle);
    }

    protected virtual void OnChangedToggle(bool check)
    {
        EmitEvent("changed");
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
        if (data.ContainsKey("default"))
        {
            toggle.isOn = ((bool)data["default"]);
        }
    }
    public override JToken GetValue()
    {
        return toggle.isOn;
    }

    public override void SetValue(JToken d)
    {
        base.SetValue(d);
        if(d.Type == JTokenType.Boolean)
        {
            toggle.isOn = (bool)d;
        }
        else if(d.Type == JTokenType.Integer || d.Type == JTokenType.Float)
        {
            toggle.isOn = (int)d != 0;
        }
        else if(d.Type == JTokenType.String)
        {
            toggle.isOn = d.ToString().ToLower() == "true";
        }
        else
        {
            Debug.LogError("this value is not bool",gameObject);
        }
    }
}
