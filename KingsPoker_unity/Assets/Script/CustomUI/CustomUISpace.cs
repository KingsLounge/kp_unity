using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomUISpace : CustomUI
{
    protected override string defaultStyle => "height=75";

    protected override void SetStyle(List<KeyValuePair<string, string>> data)
    {
        base.SetStyle(data);
        for(int i = 0; i < data.Count; i++)
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
}
