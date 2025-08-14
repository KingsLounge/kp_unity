using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class StretchHelper : MonoBehaviour
{
    public Vector2 leftBottom = Vector2.zero;
    public Vector2 rightTop = Vector2.zero;
    private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = transform as RectTransform;
    }
    private void Update()
    {
        Rect rect = (transform.parent as RectTransform).rect;
        Vector2 curSize = new Vector2(rect.width, rect.height);

        rectTransform.offsetMin = leftBottom * curSize;
        rectTransform.offsetMax = -rightTop * curSize;
    }
}
