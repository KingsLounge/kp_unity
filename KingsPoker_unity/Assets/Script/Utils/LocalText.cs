using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;


public class LocalText : MonoBehaviour
{
    [SerializeField]
    private string localKey = "";
    public string LocalKey
    {
        get
        {
            return localKey;
        }
        set
        {
            localKey = value;
            SetLocalText();
        }
    }
    private object[] parameters = new object[5];
    private bool isInit = false;
    
    private Text curText;
    // Start is called before the first frame update
    

    private void Start()
    {
        SetLocalText();
    }

    private void OnDestroy()
    {
        LocalizeManager.RemoveEvent(SetLocalText);
    }


    public void SetLocalText(string key, params object[] par)
    {
        localKey = key;
        parameters = par;
        SetLocalText();
    }
    public void Init()
    {
        if(!isInit)
        {
            curText = GetComponent<Text>();
            LocalizeManager.AddEvent(SetLocalText);
            isInit = true;
        }
    }

    [ContextMenu("로컬라이징")]
    public void SetLocalText()
    {
        Init();

        if (string.IsNullOrEmpty(localKey))
        {
            curText.text = "";
            return;
        }



        string str = LocalizeManager.GetLocalString(localKey);
        try
        {
            if (!string.IsNullOrEmpty(str) && curText)
            {
                curText.text = string.Format(str, parameters);
            }
        }
        catch (Exception e)
        {
            Console.Error($"{localKey} foromating error :\n{e}");
        }
        
    }
    
}
