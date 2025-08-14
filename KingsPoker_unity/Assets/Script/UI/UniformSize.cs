using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways()]
public class UniformSize : MonoBehaviour
{
    private RectTransform myRect;
    public RectTransform target;
    private bool prevUniformWidth = false;
    public bool uniformWidth = false;
    private bool prevUniformHeight = false;
    public bool uniformHeight = false;
    private Vector2 prevSpecing = Vector2.zero;
    public Vector2 specing = Vector2.zero;
    private Vector2 targetPrevSize = Vector2.zero;
    private Vector2 targetCurSize = Vector2.zero;
    private Vector2 myPrevSize = Vector2.zero;
    private Vector2 myCurSize = Vector2.zero;
    protected virtual void Awake()
    {
        myRect = GetComponent<RectTransform>();
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        myCurSize.Set(myRect.rect.width, myRect.rect.height);
        if(target != null)
        {
            targetCurSize.Set(target.rect.width, target.rect.height);
            if(targetCurSize != targetPrevSize || 
                myCurSize != myPrevSize ||
                prevUniformHeight != uniformHeight ||
                prevUniformWidth != uniformWidth ||
                prevSpecing != specing)
            {
                Setting();
            }
        }
    }

    protected void Setting()
    {
        prevUniformHeight = uniformHeight;
        prevUniformWidth = uniformWidth;
        prevSpecing = specing;
        targetPrevSize.Set(targetCurSize.x,targetCurSize.y);
        myPrevSize.Set(myRect.rect.width, myRect.rect.height);
        Vector2 newSize = targetPrevSize + specing;
        if (uniformWidth) myPrevSize.x = newSize.x;
        if (uniformHeight) myPrevSize.y = newSize.y;
        myRect.sizeDelta = myPrevSize;
    }
}
