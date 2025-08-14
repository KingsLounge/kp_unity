using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ContentSizeFitterSinal : ContentSizeFitter
{
    private bool callSetting = false;
    protected override void OnRectTransformDimensionsChange()
    {
        base.OnRectTransformDimensionsChange();
        callSetting = true;
    }

    public override void SetLayoutHorizontal()
    {
        base.SetLayoutHorizontal();
        callSetting = true;
    }

    public override void SetLayoutVertical()
    {
        base.SetLayoutVertical();
        callSetting = true;
    }

    public void Update()
    {
        if(callSetting)
        {
            callSetting = false;
            StartSetting();
        }
    }

    public void StartSetting()
    {
        Transform parent = transform.parent;
        while(parent != null)
        {
            ContentSizeFitter content = parent.GetComponent<ContentSizeFitter>();
            if (content != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)parent);
            }
            parent = parent.parent;
        }
    }
}
