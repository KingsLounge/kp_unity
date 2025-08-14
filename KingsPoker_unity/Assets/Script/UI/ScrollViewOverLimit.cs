using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScrollViewOverLimit : MonoBehaviour
{
    private ScrollRect scroll;
    public float limitY = 100f;
    void Awake()
    {
        scroll = GetComponent<ScrollRect>();
    }

    void Update()
    {
        float range = limitY / scroll.content.rect.height;
        Vector2 cur = scroll.normalizedPosition;
        if (cur.y < -range)
        {
            cur.y = -range;
            scroll.verticalNormalizedPosition = cur.y;
        }
        else if(cur.y > 1 + range)
        {
            cur.y = 1 + range;
            scroll.verticalNormalizedPosition = cur.y;
        }
    }
}
