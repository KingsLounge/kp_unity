using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.UI;

public class CustomUIText : CustomUI    
{
    public string originalText {
        get;private set;
    }
    public Text contents;
    private string format;
    protected override string defaultStyle => "align=left;color=#8d8d8d;font-size=26;format=";
    private bool isLocalstring;
    private string def = "";
    private void Awake()
    {
        LocalizeManager.AddEvent(ChangeLocal);
    }
    private void OnDestroy()
    {
        LocalizeManager.RemoveEvent(ChangeLocal);
    }
    protected override void SetStyle(List<KeyValuePair<string,string>> list)
    {
        base.SetStyle(list);
        for(int i = 0; i < list.Count; i++)
        {
            KeyValuePair<string, string> cur = list[i];
            string key = cur.Key;
            string value = cur.Value;
            switch(key)
            {
                case "align":
                    if (value == "left") contents.alignment = TextAnchor.MiddleLeft;
                    else if (value == "center") contents.alignment = TextAnchor.MiddleCenter;
                    else if (value == "right") contents.alignment = TextAnchor.MiddleRight;
                    break;
                case "font-size":
                    contents.fontSize = int.Parse(value);
                    break;
                case "color":
                    Color color;
                    if(ColorUtility.TryParseHtmlString(value, out color))
                        contents.color = color;
                    break;
                case "format":
                    format = value.GetLocalizedFormatString();
                    if (!string.IsNullOrEmpty(format) && contents.text != "")
                    {
                        contents.text = string.Format(format, contents.text);
                    }
                    break;
            }
        }
    }
    public override void SetValue(JToken d) 
    {
        base.SetValue(d);
        isLocalstring = false;
        if (!string.IsNullOrEmpty(format) && d.ToString() != "infinity")
        {
            contents.text = string.Format(format, Int64.Parse(d.ToString()));
        }
        else
        {
            contents.text = d.ToString();
        }
    }
    public override void SetUI(JObject data)
    {
        base.SetUI(data);
        isLocalstring = false;
        if(data.ContainsKey("default"))
        {
            def = data["default"].ToString();
            contents.text = LocalizeManager.GetLocalString(def);
            isLocalstring = true;
        }
        else
        {
            contents.text = "";
        }
        originalText = contents.text;
    }
    public void ChangeLocal()
    {
        if(isLocalstring)
        {
            contents.text = LocalizeManager.GetLocalString(def);
        }
    }

    public void SetText(string str)
    {
        isLocalstring = false;
        contents.text = str;
    }
}
