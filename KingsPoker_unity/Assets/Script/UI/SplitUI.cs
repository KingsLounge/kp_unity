using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class SplitUI : MonoBehaviour
{
    private RectTransform rect;
    [Range(-0.5f,0.5f)]
    public float oneChildNodePos = 0f;
    private float prevOneChildNodePos = 0;
    private Vector2 prevSize = Vector2.zero;
    private bool changedChildCount = false;
    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        Setting();
    }
    private void Update()
    {
        if(changedChildCount || oneChildNodePos != prevOneChildNodePos || !prevSize.Equals(rect.sizeDelta))
        {
            prevOneChildNodePos = oneChildNodePos;
            Setting();
        }
        changedChildCount = false;
    }

    private void OnEnable()
    {
        Setting();
    }

    private void OnTransformChildrenChanged()
    {
        changedChildCount = true;
    }

    private void Setting()
    {
        int count = transform.childCount;
        float interval = count > 1 ? 1f / (count - 1) : 0;
        float left = count > 1 ? -0.5f : oneChildNodePos;
        Vector2 size = rect.sizeDelta;
        prevSize = size;
        for(int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).localPosition = new Vector3(size.x * (left + (interval * i)), 0, 0);
        }
    }
}
