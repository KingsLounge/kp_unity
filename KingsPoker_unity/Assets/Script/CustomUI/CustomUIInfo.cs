using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.UI;

public class CustomUIInfo : CustomUIText
{
    public Text title;
    public Text subText;
    public Image line;

    protected override string defaultStyle => "sub-text=;align=right;color=#8d8d8d;font-size=26";


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
    }


    public override void SetValue(JToken d)
    {
        int temp_value = 0;

        if (Int32.TryParse(d.ToString(), out temp_value))
        {
            if (temp_value >= 100000000)
            {
                d = LocalizeManager.GetLocalString("infinity");
                subText.text = "";
            }
            else
            {
                //subText.gameObject.SetActive(false);
            }
        }

        base.SetValue(d);
    }

    protected override void SetStyle(List<KeyValuePair<string, string>> list)
    {
        base.SetStyle(list);
        for (int i = 0; i < list.Count; i++)
        {
            KeyValuePair<string, string> cur = list[i];
            string key = cur.Key;
            string value = cur.Value;
            switch (key)
            {
                case "height":
                    RectTransform rt = gameObject.transform.GetComponent<RectTransform>();
                    rt.sizeDelta = new Vector2(rt.sizeDelta.x, (float)int.Parse(value));
                    break;

                case "sub-text":
                    if (subText != null)
                    {
                        subText.text = " " + LocalizeManager.GetLocalString(value);
                    }
                    break;
                case "font-size":
                    title.fontSize = int.Parse(value);
                    subText.fontSize = int.Parse(value);
                    break;
                case "color":
                    {
                        Color color;
                        if (ColorUtility.TryParseHtmlString(value, out color))
                            subText.color = color;
                    }
                    break;
                case "line":
                    line?.gameObject.SetActive(bool.Parse(value));
                    break;
                case "line-color":
                    {
                        Color color;
                        if (ColorUtility.TryParseHtmlString(value, out color) && line != null)
                        {
                            line.color = color;
                        }
                    }
                    break;
            }
        }
    }

}
