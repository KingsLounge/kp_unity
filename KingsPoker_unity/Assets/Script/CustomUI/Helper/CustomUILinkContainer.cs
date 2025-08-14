using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;
public class CustomUILinkContainer : MonoBehaviour
{
    private JArray ratio = null;
    private RectTransform myRect = null;

    private int childCount
    {
        get
        {
            int count = 0;
            for(int i = 0; i < transform.childCount; i++)
            {
                if (transform.GetChild(i).gameObject.activeSelf)
                    count++;
            }
            return count;
        }
    }

    private List<float> GetCurrentRatio()
    {
        List<float> curRatio = new List<float>();
        if (ratio != null)
        {
            for (int i = 0; i < ratio.Count; i++)
            {
                curRatio.Add((float)ratio[i]);
            }
            for (int i = curRatio.Count; i <= childCount; i++)
            {
                curRatio.Add(0f);
            }
        }
        else
        {
            for(int i = 0; i < childCount; i++)
            {
                curRatio.Add(1f / childCount);
            }
        }
        return curRatio;
    }

    public void SetRatio(JArray ratio)
    {
        this.ratio = ratio;
        Setting();
    }

    private void OnRectTransformDimensionsChange()
    {
        Setting();
    }

    public void Setting()
    {
        List<float> curRatio = GetCurrentRatio();
        Vector2 mySize = new Vector2(myRect.rect.width, myRect.rect.height);
        float total = 0f;
        for(int i = 0; i < curRatio.Count; i++)
        {
            total += curRatio[i];
        }
        float maxY = 0;
        float curX = 0;
        for(int i = 0; i < transform.childCount; i++)
        {
            RectTransform rect = transform.GetChild(i) as RectTransform;
            if (!rect.gameObject.activeSelf)
                continue;
            Vector2 childSize = new Vector2(rect.rect.width, rect.rect.height);
            childSize.x = mySize.x * (curRatio[i] / total);
            rect.sizeDelta = childSize;
            Vector2 newPos = rect.anchoredPosition;
            newPos.x = curX;
            newPos.y = 0;
            curX += childSize.x;
            rect.anchoredPosition = newPos;
            if (maxY < childSize.y) maxY = childSize.y;
        }
        mySize.y = maxY;
        myRect.sizeDelta = mySize;
    }

    private void Update()
    {
        float maxY = 0;
        for (int i = 0; i < transform.childCount; i++)
        {
            RectTransform rect = transform.GetChild(i) as RectTransform;
            if (!rect.gameObject.activeSelf)
                continue;
            if (maxY < rect.rect.height) maxY = rect.rect.height;
        }
        if(maxY != myRect.rect.height)
        {
            Setting();
        }
    }

    private void Awake()
    {
        myRect = transform as RectTransform;
    }

    public void OnTransformChildrenChanged()
    {
        Setting();
    }
}
