using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class ChatBalloonBackground : MonoBehaviour
{
    public Text text;
    private RectTransform rect;
    public bool limitWidth = true;
    public bool limitHeight = true;
    public Vector2 interval = Vector2.zero;

    private void Awake()
    {
        rect = transform as RectTransform;
    }
    void Update()
    {
        if(text != null)
        {
            float width = text.preferredWidth;
            float height = text.preferredHeight;
            Rect r = text.rectTransform.rect;
            if (limitWidth && width > r.width)
                width = r.width;
            if (limitHeight && height > r.height)
                height = r.height;
            rect.sizeDelta = new Vector2(interval.x + width, interval.y + height);
        }
        else
        {
            rect.sizeDelta = interval;
        }
    }
}
