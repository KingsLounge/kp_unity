using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.UI;

public class CustomUIIconText : CustomUIText
{
    public Image icon;

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
                case "icon_width":
                    {
                        Vector2 newSize = icon.rectTransform.sizeDelta;
                        newSize.x = float.Parse(value);
                        icon.rectTransform.sizeDelta = newSize;
                        break;
                    }
                case "icon_height":
                    {
                        Vector2 newSize = icon.rectTransform.sizeDelta;
                        newSize.y = float.Parse(value);
                        icon.rectTransform.sizeDelta = newSize;
                        break;
                    }
                case "icon_scale_x":
                    {
                        Vector2 newScale = icon.rectTransform.localScale;
                        newScale.x = float.Parse(value);
                        icon.rectTransform.localScale = newScale;
                        break;
                    }
                case "icon_scale_y":
                    {
                        Vector2 newScale = icon.rectTransform.localScale;
                        newScale.y = float.Parse(value);
                        icon.rectTransform.localScale = newScale;
                        break;
                    }
                case "icon_scale":
                    {
                        float parseValue = float.Parse(value);
                        icon.rectTransform.localScale = new Vector2(parseValue,parseValue);
                        break;
                    }
            }
        }

    }

    public override void SetUI(JObject data)
    {
        base.SetUI(data);
        Sprite sp = root.GetSprite(data["data"].ToString());
        icon.sprite = sp;
        if (sp)
        {
            icon.SetNativeSize();
        }
    }
}
