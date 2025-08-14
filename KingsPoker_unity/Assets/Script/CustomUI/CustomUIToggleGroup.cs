using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CustomUIToggleGroup : CustomUI
{
    public ToggleGroup group = null;
    private JArray data = null;

    private List<Toggle> toggles = new List<Toggle>();

    public override void SetUI(JObject data)
    {
        base.SetUI(data);
        if (data.ContainsKey("data"))
        {
            this.data = data["data"] as JArray;
        }
        else
        {
            this.data = null;
        }

        if (data.ContainsKey("default"))
        {
            if ((bool)data["default"])
            {
                for (int i = 0; i < toggles.Count; i++)
                {
                    toggles[i].isOn = i == 0;
                }
            }
        }

    }

    public override void SetValue(JToken d)
    {
        base.SetValue(d);
    }

    public override void CompleteSetting()
    {
        base.CompleteSetting();


        if (data == null) return;
        for (int i = 0; i < data.Count; i++)
        {
            List<CustomUI> temp = root.GetGroup(data[i].ToString());
            for (int j = 0; j < temp.Count; j++)
            {
                var toggle = temp[j].GetComponent<Toggle>();
                if (toggle != null)
                {
                    toggle.group = group;
                }
                temp[j].transform.SetParent(group.transform);
                temp[j].transform.localRotation = Quaternion.identity;
                temp[j].transform.localScale = Vector3.one;
                temp[j].transform.localPosition = Vector3.zero;

                toggles.Add(toggle);
            }
        }
    }
}
