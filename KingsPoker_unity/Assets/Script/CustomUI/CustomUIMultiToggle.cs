using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.UI;

public class CustomUIMultiToggle : CustomUI
{
    protected class CustomUIMultiToggleTemplateData
    {
        public GameObject go;
        public Text title;
        public Toggle toggle;
        public CustomUIMultiToggleTemplateData(GameObject go)
        {
            this.go = go;
            title = go.GetComponentInChildren<Text>();
            toggle = go.GetComponentInChildren<Toggle>();
        }
    }
    public Text title;
    public string defaultAlign = "center";
    public GameObject template;
    public GameObject container;
    protected List<CustomUIMultiToggleTemplateData> toggleTemplates = new List<CustomUIMultiToggleTemplateData>();
    protected int prev = 0;
    protected override string defaultStyle => string.Format("align={0}", defaultAlign);

    public override JToken GetValue()
    {
        bool[] arr = new bool[toggleTemplates.Count];
        for(int i = 0; i < toggleTemplates.Count; i++)
        {
            arr[i] = toggleTemplates[i].toggle.isOn;
        }
        return new JArray(arr);
    }

    public override void SetValue(JToken d)
    {
        base.SetValue(d);
        JArray arr = d as JArray;
        for (int i = 0; i < arr.Count; i++)
        {
            toggleTemplates[i].toggle.isOn = (bool)arr[i];
        }
    }

    protected override void SetStyle(List<KeyValuePair<string, string>> list)
    {
        base.SetStyle(list);
        for (int i = 0; i < list.Count; i++)
        {
            KeyValuePair<string, string> cur = list[i];
            string key = cur.Key;
            string value = cur.Value;
            switch (key)
            {
                case "align":
                    for (int j = 0; j < toggleTemplates.Count; j++)
                    {
                        Text title = toggleTemplates[j].title;
                        if (value == "left") title.alignment = TextAnchor.MiddleLeft;
                        else if (value == "center") title.alignment = TextAnchor.MiddleCenter;
                        else if (value == "right") title.alignment = TextAnchor.MiddleRight;
                    }
                    break;
            }
        }
    }

    public override void SetUI(JObject data)
    {
        base.SetUI(data);
        if (title != null)
        {
            if (data.ContainsKey("title"))
            {
                title.text = LocalizeManager.GetLocalString(data["title"].ToString());
            }
            else if (!string.IsNullOrEmpty(key))
            {
                title.text = key;
            }
        }
        if (data.ContainsKey("default"))
        {
            JArray arr = data["default"] as JArray;
            for (int i = toggleTemplates.Count - 1; i >= arr.Count; i--)
            {
                GameObject.Destroy(toggleTemplates[i].go);
                toggleTemplates.RemoveAt(i);
            }
            for (int i = toggleTemplates.Count; i < arr.Count; i++)
            {
                GameObject go = Instantiate(template, container.transform);
                go.SetActive(true);
                toggleTemplates.Add(new CustomUIMultiToggleTemplateData(go));
            }
            for (int i = 0; i < arr.Count; i++)
            {
                toggleTemplates[i].toggle.isOn = (bool)arr[i];
            }
        }
        if (data.ContainsKey("names"))
        {
            JArray arr = data["names"] as JArray;
            for (int i = 0; i < arr.Count; i++)
            {
                if (toggleTemplates.Count > i)
                {
                    toggleTemplates[i].title.text = LocalizeManager.GetLocalString(arr[i].ToString());
                }
            }
        }
        else
        {
            toggleTemplates.ForEach(t => t.toggle.isOn = false);
        }
    }

    public void ChangedToggle(bool changed)
    {
        OnChangeToggle();
    }

    public override void CompleteSetting()
    {
        base.CompleteSetting();
        OnChangeToggle();
    }

    protected virtual void OnChangeToggle()
    {
        EmitEvent("changed");
    }
}
