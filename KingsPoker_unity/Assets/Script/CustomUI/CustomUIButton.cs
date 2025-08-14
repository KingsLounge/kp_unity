using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.UI;

public class CustomUIButton : CustomUI
{
    public Text text;
    public Button btn;

    public void Awake()
    {
        btn.onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        EmitEvent("clicked");
    }

    public override void SetUI(JObject data)
    {
        base.SetUI(data);
        if(data.ContainsKey("title"))
        {
            text.text = LocalizeManager.GetLocalString(data["title"].ToString());
        }
        else if(!string.IsNullOrEmpty(key))
        {
            text.text = key;
        }
        else
        {
            text.text = "";
        }
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
               
                case "color":
                    Color color;
                    if (ColorUtility.TryParseHtmlString(value, out color))
                        btn.image.color = color;
                    break;
                
            }
        }
    }
}
