using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
public class NoticePanel : MonoBehaviour
{
    public Text noticeText;
    protected System.Action callBack;
    public Vector3 waitPos = Vector3.zero;
    public Vector3 noticePos = Vector3.zero;
    public void SetNotice(NoticeData n, System.Action callBack = null)
    {
        noticeText.text = LocalizeManager.GetLocalString(n.msg);
        this.callBack = callBack;
        gameObject.SetActive(true);
        StartCoroutine(Notice(n.time));
    }


    public virtual IEnumerator Notice(float time)
    {
        var recttransform = transform as RectTransform;
        recttransform.DOAnchorPos3D(noticePos, 0.3f);
        yield return new WaitForSeconds(0.3f + time);
        recttransform.DOAnchorPos3D(waitPos, 0.3f);
        yield return new WaitForSeconds(0.3f);
        callBack?.Invoke();
        gameObject.SetActive(false);
    }
}
