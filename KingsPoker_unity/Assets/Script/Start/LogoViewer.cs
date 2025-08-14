using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;

public class LogoViewer : MonoBehaviour
{
    public List<LogoPage> pages;
    public string nextScene;
    public UnityEvent logoEndEvent;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(ShowPages());
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public IEnumerator ShowPages()
    {
        for(int i = 0; i < pages.Count; i++)
        {
            if(!pages[i].show)
            {
                continue;
            }
            pages[i].canvas.gameObject.SetActive(true);
            float time = 0;
            while(time < pages[i].fadeInTime)
            {
                time += Time.deltaTime;
                pages[i].canvas.alpha = Mathf.Lerp(0, 1, time / pages[i].fadeInTime);
                yield return 0;
            }
            time = 0;
            while (time < pages[i].showTime)
            {
                time += Time.deltaTime;
                yield return 0;
            }
            time = 0;
            while (time < pages[i].fadeOutTime)
            {
                time += Time.deltaTime;
                pages[i].canvas.alpha = Mathf.Lerp(1, 0, time / pages[i].fadeOutTime);
                yield return 0;
            }
            yield return new WaitForSeconds(pages[i].nextDelayTime);
            pages[i].canvas.gameObject.SetActive(false);
        }
        logoEndEvent.Invoke();
    }
    public void LoadNextScene()
    {
        CustomSceneManager.LoadScene(nextScene);
    }
}



[Serializable]
public class LogoPage
{
    public bool show = true;
    public float fadeInTime;
    public float showTime;
    public float fadeOutTime;
    public float nextDelayTime;
    public CanvasGroup canvas;
}
