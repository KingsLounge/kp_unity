using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UniformParentSize : UniformSize
{
    protected override void Awake()
    {
        base.Awake();
        target = (RectTransform)transform.parent;
        Setting();
    }

    protected override void Update()
    {
        base.Update();
        if(target != transform.parent)
        {
            target = (RectTransform)transform.parent;
            Setting();
        }
    }

    private void OnTransformParentChanged()
    {
        target = (RectTransform)transform.parent;
        Setting();
    }
}
