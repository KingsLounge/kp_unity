using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
[RequireComponent(typeof(Text))]
public class SingleLineBestFit : MonoBehaviour
{
    public int min = 0;
    public int max = 30;
    private Text text;
    private string prev = "";
    private bool needFit = true;
    private void Awake()
    {
        text = GetComponent<Text>();
        prev = text.text;
    }

    private void OnRectTransformDimensionsChange()
    {
        needFit = true;
    }

    private int GetBestSize()
    {
        Rect rect = (transform as RectTransform).rect;
        float width = rect.width;
        float height = rect.height;
        for(int i = max; i > min; i--)
        {
            text.fontSize = i;
            if (text.preferredWidth <= width && text.preferredHeight <= height)
                return i;
        }
        return min;
    }

    private void LateUpdate()
    {
        if (prev != text.text)
            needFit = true;
        if(text.horizontalOverflow != HorizontalWrapMode.Overflow)
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
        if (text.verticalOverflow != VerticalWrapMode.Overflow)
            text.verticalOverflow = VerticalWrapMode.Overflow;
        if(needFit)
            text.fontSize = GetBestSize();
    }
}
