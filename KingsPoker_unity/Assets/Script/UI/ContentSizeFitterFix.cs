using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ContentSizeFitterFix : ContentSizeFitter
{
    public override void SetLayoutHorizontal()
    {
        base.SetLayoutHorizontal();
        var layoutGroup = transform.parent.GetComponent<LayoutGroup>();
        if (layoutGroup)
        {
            layoutGroup.CalculateLayoutInputHorizontal();
            layoutGroup.SetLayoutHorizontal();
        }
        var sizefitter = transform.parent.GetComponent<ContentSizeFitter>();
        if (sizefitter)
        {
            sizefitter.SetLayoutHorizontal();
        }
    }

    public override void SetLayoutVertical()
    {
        base.SetLayoutVertical();
        var layoutGroup = transform.parent.GetComponent<LayoutGroup>();
        if (layoutGroup)
        {
            layoutGroup.CalculateLayoutInputVertical();
            layoutGroup.SetLayoutVertical();
        }

        var sizefitter = transform.parent.GetComponent<ContentSizeFitter>();
        if (sizefitter)
        {
            sizefitter.SetLayoutVertical();
        }
    }
}
