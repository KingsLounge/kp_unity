using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowMyRank : MonoBehaviour
{
    public RectTransform parentRect;
    private Transform myRankTransform;
    private RectTransform thisTransform;
    
    private Vector2 pos;
    private float height;
    private void Awake() {
        thisTransform = GetComponent<RectTransform>();
        height = parentRect.rect.height;
        pos = parentRect.position;
    }
    public void SetMyRnakTrans(Transform tr)
    {
        myRankTransform = tr;
    }

    private void LateUpdate() {
        if(!myRankTransform)
            return;
        var yScale = parentRect.lossyScale.y;
        var pos = parentRect.position;
        var top = pos.y;
        var bottom = pos.y - height *yScale;
        var myheight = thisTransform.rect.height * thisTransform.lossyScale.y;
        pos.y = Mathf.Clamp(myRankTransform.position.y, bottom + myheight*0.5f, top - myheight*0.5f);
//        Debug.Log(string.Format("{0} : {1} : {2}", pos.y, bottom , top));
        thisTransform.position = pos;
    }
}
