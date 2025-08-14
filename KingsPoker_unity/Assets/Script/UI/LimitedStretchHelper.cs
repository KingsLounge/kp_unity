using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[ExecuteAlways]
public class LimitedStretchHelper : MonoBehaviour
{
    public RectTransform top;
    public RectTransform left;
    public RectTransform right;
    public RectTransform bottom;
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

        Vector2 tempLeftBottom= new Vector2(left == null ? 0 : rect.width + left.offsetMax.x, top == null ? 0 : rect.height + bottom.offsetMax.y);
        Vector2 tempRightTop = new Vector2(right == null ? 0 : rect.width - right.offsetMin.x, bottom == null ? 0 : rect.height - top.offsetMin.y);
        Vector2 curSize = new Vector2(rect.width, rect.height);
        curSize -= tempLeftBottom + tempRightTop;

        rectTransform.offsetMin = tempLeftBottom + (leftBottom * curSize);
        rectTransform.offsetMax = -(tempRightTop + (rightTop * curSize));
    }
}
