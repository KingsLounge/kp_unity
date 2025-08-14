using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class Arrange : MonoBehaviour
{
    [Header("Horizontal Setting")]
    public bool horizontal = false;
    public bool right = true;
    public float horizontalInterval = 0f;
    [Header("Vertical Setting")]
    public bool vertical = false;
    public bool up = true;
    public float verticalInterval = 0f;

    private void OnRectTransformDimensionsChange()
    {
        Setting();
    }

    private void OnTransformChildrenChanged()
    {
        Setting();
    }

    private void Update()
    {
        Setting();
    }

    public void Setting()
    {
        Vector3 cur = Vector3.zero;
        for (int i = 0; i < transform.childCount; i++)
        {
            RectTransform rect = transform.GetChild(i) as RectTransform;
            if (!rect.gameObject.activeInHierarchy) continue;
            rect.anchoredPosition = cur;
            if (horizontal)
                cur.x += (rect.rect.width + horizontalInterval) * (right ? 1 : -1);
            if (vertical)
                cur.y += (rect.rect.height + verticalInterval) * (up ? 1 : -1);
        }
    }
}
