using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImportantNoticePanel : NoticePanel
{
    [SerializeField] RectTransform canvasRect;
    [SerializeField] RectTransform thisRectTr;
    
    public float speed = 50f;
    // Start is called before the first frame update
    
    public override IEnumerator Notice(float time)
    {
        thisRectTr.anchoredPosition = waitPos;
        float endPosX = - (canvasRect.rect.width + thisRectTr.rect.width);
        var wait = new WaitForEndOfFrame();
        do
        {
            endPosX = - (canvasRect.rect.width + thisRectTr.rect.width + 100);
            thisRectTr.Translate(Vector3.left * speed * Time.deltaTime, Space.Self);
            yield return wait;

        }
        while (thisRectTr.anchoredPosition.x > endPosX);
        
        callBack?.Invoke();
        gameObject.SetActive(false);
    }
    // Update is called once per frame
    
}
