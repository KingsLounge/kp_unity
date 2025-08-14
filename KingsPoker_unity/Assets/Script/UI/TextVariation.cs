using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable()]
public class TextVariationInfo
{
    public string key = "";
    public bool useLocalization = false;
    public string value;
}
public class TextVariation : Variation
{
    public List<TextVariationInfo> variation = new List<TextVariationInfo>();
    private Text text;
    public string defaultStr = "";
    public bool useLocalization = false;
    private bool init = false;
    protected override void Init()
    {
        init = true;
        text = GetComponent<Text>();
        if (text == null)
        {
            Console.Error(gameObject.name + " has not Image Component!");
        }
    }
    public void Awake()
    {
        if (!init)
        {
            Init();
        }
    }
    public override void SetVariation(string key)
    {
        if (!init)
        {
            Init();
        }
        TextVariationInfo info = variation.Find(value => value.key == key);
        if (info != null)
        {
            text.text = info.useLocalization ? LocalizeManager.GetLocalString(info.value) : info.value;
        }
        else
        {
            text.text = useLocalization ? LocalizeManager.GetLocalString(defaultStr) : defaultStr;
        }
    }
}
