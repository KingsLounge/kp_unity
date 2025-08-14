using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class CustomUILink : CustomUI
{
    public GameObject template = null;
    public CustomUILinkContainer container = null;
    private JArray data = null;
    private JArray ratio = null;

    public override void SetUI(JObject data)
    {
        base.SetUI(data);
        if(data.ContainsKey("data"))
        {
            this.data = data["data"] as JArray;
        }
        else
        {
            this.data = null;
        }
        if(data.ContainsKey("default"))
        {
            ratio = (data["default"] as JArray);
        }
        else
        {
            ratio = null;
        }
    }

    public override void CompleteSetting()
    {
        base.CompleteSetting();
        if (data == null) return;
        for (int i = container.transform.childCount; i < data.Count; i++)
        {
            RectTransform tr = Instantiate(template).transform as RectTransform;

            tr.SetParent(container.transform);
            tr.anchorMax = new Vector2(0, 1);
            tr.anchorMin = new Vector2(0, 1);

            tr.anchoredPosition = Vector3.zero;
            tr.localScale = Vector3.one;
        }
        for (int i = 0; i < container.transform.childCount; i++)
        {
            Transform curContainer = container.transform.GetChild(i);
            curContainer.gameObject.SetActive(i < data.Count);
            if(i < data.Count)
            {
                List<CustomUI> temp = root.GetGroup(data[i].ToString());
                for (int j = 0; j < temp.Count; j++)
                {
                    temp[j].transform.SetParent(curContainer.transform);
                    temp[j].transform.localRotation = Quaternion.identity;
                    temp[j].transform.localScale = Vector3.one;
                    temp[j].transform.localPosition = Vector3.zero;
                }
            }
        }
        container.SetRatio(ratio);
    }
}
