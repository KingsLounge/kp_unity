using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.UI;

public class CustomUIImage : CustomUI
{
    public Image img;

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
                case "width":
                    {
                        Vector2 newSize = img.rectTransform.sizeDelta;
                        newSize.x = float.Parse(value);
                        img.rectTransform.sizeDelta = newSize;
                        break;
                    }
                case "height":
                    {
                        Vector2 newSize = img.rectTransform.sizeDelta;
                        newSize.y = float.Parse(value);
                        img.rectTransform.sizeDelta = newSize;
                        break;
                    }
                case "scale_x":
                    {
                        Vector2 newScale = img.rectTransform.localScale;
                        newScale.x = float.Parse(value);
                        img.rectTransform.localScale = newScale;
                        break;
                    }
                case "scale_y":
                    {
                        Vector2 newScale = img.rectTransform.localScale;
                        newScale.y = float.Parse(value);
                        img.rectTransform.localScale = newScale;
                        break;
                    }
                case "scale":
                    {
                        float parseValue = float.Parse(value);
                        img.rectTransform.localScale = new Vector2(parseValue, parseValue);
                        break;
                    }
            }
        }

    }

    public override void SetUI(JObject data)
    {
        base.SetUI(data);
        Sprite sp = root.GetSprite(data["default"].ToString());
        img.sprite = sp;
        if(sp)
        {
            Debug.Log((new Vector2(sp.texture.width, sp.texture.height).ToString()));
            img.SetNativeSize();
        }
    }
}
