using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SimpleMessageItem : MonoBehaviour
{
    public float fadeInTime = 0.2f;
    public float stayTime = 1f;
    public float fadeOutTime = 0.2f;

    private float duration = 0;
    private UnityAction callback;
    public void Setting(string message,UnityAction callback = null)
    {
        GetComponentInChildren<Text>().text = LocalizeManager.GetLocalString(message);
        duration = 0;
        this.callback = callback;
        StartCoroutine(PlayAnimation());
    }

    private IEnumerator PlayAnimation()
    {
        Vector3 scale = new Vector3(0f,1f,1f);

        while(scale.x < 1)
        {
            scale.x += Time.deltaTime / fadeInTime;
            if (scale.x >= 1) scale.x = 1;
            transform.localScale = scale;
            yield return 0;
        }

        yield return new WaitForSeconds(stayTime);

        while (scale.x > 0)
        {
            scale.x -= Time.deltaTime / fadeOutTime;
            if (scale.x <= 0) scale.x = 0;
            transform.localScale = scale;
            yield return 0;
        }
        callback?.Invoke();
    }
}
