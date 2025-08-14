using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class ScrollViewHelper : MonoBehaviour
{
    private RectTransform tr;

    // Update is called once per frame
    void Awake() {
        tr = GetComponent<RectTransform>();
        Resize();
    }

    private void OnTransformChildrenChanged() {
        Resize();
    }

    private void Resize() {
        if(tr == null) {
            tr = GetComponent<RectTransform>();
        }
        float y = 0;
        for(int i = 0; i < gameObject.transform.childCount; i++) {
            Transform child = gameObject.transform.GetChild(i);
            if (child.gameObject.activeSelf)
            {
                y += child.GetComponent<RectTransform>().sizeDelta.y;
            }
        }
        tr.sizeDelta = new Vector2(tr.sizeDelta.x,y);
    }
}
