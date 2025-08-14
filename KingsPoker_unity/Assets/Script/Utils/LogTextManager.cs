using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;

public class LogTextManager : MonoBehaviour
{
    private static LogTextManager instance;
    public static LogTextManager Instance
    {
        get{return instance;}
    }
    private ScrollRect scroll;
    [SerializeField]
    private GameObject textPrefab;
    public List<GameObject> textList;
    public GameObject logBack;
    public int maxLogCount = 100;
    private bool autoDown = true;
    private RectTransform tr = null;
    private int scrollSizeCount = 0;
    private Queue<string> queue = new Queue<string>();
    private void Awake() {
        instance = this;
        scroll = GetComponent<ScrollRect>();
        tr = GetComponent<RectTransform>();
        // var args = Environment.GetCommandLineArgs();
        // foreach(var arg in args)
        // {
        //     Console.Error(arg);
        // }
    }

    private void Update() {
        if(Input.GetMouseButton(0)) {
            if (scroll.verticalNormalizedPosition > 0) {
                autoDown = false;
            } else {
                autoDown = true;
            }
        } else {
            if(autoDown && scroll.verticalNormalizedPosition > 0) {
                scroll.StopMovement();
                scroll.verticalNormalizedPosition = 0f;
            }
        }
        GenText();
    }

    public void GenText()
    {
        while(queue.Count>0)
        {
            var log = queue.Dequeue();
            var go = Instantiate(textPrefab, scroll.content);
            var text = go.GetComponent<Text>();
            text.text = log;
            textList.Add(go);
        }
        TextDelete();
    }
    public void TextLog(string log)
    {
        queue.Enqueue(log);  
    }

    public void TextDelete()
    {
        while(textList.Count > maxLogCount)
        {
            var go = textList[0];
            textList.RemoveAt(0);
            Destroy(go);
        }
    }

    public void OnClickLogButton()
    {
        scrollSizeCount ++;
        scrollSizeCount %= 4;
        var rect = tr.sizeDelta;
        rect.y = scrollSizeCount * 200f;
        tr.sizeDelta = rect;
        logBack.SetActive(scrollSizeCount != 0);
    }


}
