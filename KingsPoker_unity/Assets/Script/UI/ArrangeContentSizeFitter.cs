using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Arrange))]
[ExecuteAlways]
public class ArrangeContentSizeFitter : MonoBehaviour
{
    private Arrange _arrange;
    private Arrange arrange {
        get
        {
            if (_arrange == null)
                _arrange = GetComponent<Arrange>();
            return _arrange;
        }
    }
    private void Update()
    {
        bool changed = true;
        for (int i = 0; i < transform.childCount; i++)
        {
            RectTransform rect = transform.GetChild(i) as RectTransform;
            if (rect.hasChanged)
            {
                changed = true;
                rect.hasChanged = false;
            }
        }
        if (changed)
        {
            Resize();
        }
    }
    private void OnTransformChildrenChanged()
    {
        Resize();
    }

    private void Resize()
    {
        float totalSize = 0;
        bool vertical = arrange.vertical;
        for (int i = 0; i < transform.childCount; i++)
        {
            RectTransform rect = transform.GetChild(i) as RectTransform;
            if (!rect.gameObject.activeSelf) continue;
            if (i != 0) totalSize += vertical ? arrange.verticalInterval : arrange.horizontalInterval;
            totalSize += vertical ? rect.sizeDelta.y : rect.sizeDelta.x;
        }
        RectTransform rt = (transform as RectTransform);
        Vector2 size = rt.sizeDelta;
        if (vertical) size.y = totalSize;
        else size.x = totalSize;
        rt.sizeDelta = size;
    }
}
