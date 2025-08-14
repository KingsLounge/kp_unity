using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable()]
public class ActiveVariationInfo
{
    public string key = "";
    public bool active = true;
}
public class ActiveVariation : Variation
{
    public List<ActiveVariationInfo> variation = new List<ActiveVariationInfo>();
    public bool defaultActive = true;

    public override void SetVariation(string key)
    {
        ActiveVariationInfo info = variation.Find(value => value.key == key);
        if (info != null)
        {
            gameObject.SetActive(info.active);
        }
        else
        {
            gameObject.SetActive(defaultActive);
        }
    }
}
