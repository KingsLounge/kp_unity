using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.UI;

public class CustomUIRadio : CustomUI
{
    protected class CustomUIRadioTemplateData
    {
        public GameObject go;
        public Text title;
        public Toggle toggle;
        public JToken value;
        public CustomUIRadioTemplateData(GameObject go)
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
    protected List<CustomUIRadioTemplateData> activeTemplates = new List<CustomUIRadioTemplateData>();
    protected int prev = 0;
    protected override string defaultStyle => string.Format("align={0}",defaultAlign);

    public override JToken GetValue()
    {
        return activeTemplates[prev].value;
    }

    public JToken GetOnlyValue()
    {
        return activeTemplates[prev].value;
    }

    public override void SetValue(JToken d)
    {
        base.SetValue(d);
        int idx = -1;
        for (int i = 0; i < activeTemplates.Count; i++)
        {
            if(activeTemplates[i].value.ToString() == d.ToString())
            {
                idx = i;
            }
        }
        if (idx == -1) return;
        prev = idx;
        activeTemplates[idx].toggle.isOn = true;
    }
    public void SetIndex(int idx)
    {
        if (idx < 0 || activeTemplates.Count <= idx) return;
        prev = idx;
        activeTemplates[idx].toggle.isOn = true;
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
                    for(int j = 0; j < activeTemplates.Count; j++)
                    {
                        Text title = activeTemplates[j].title;
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
        if(title != null)
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
        if (data.ContainsKey("data"))
        {
            JArray arr = data["data"] as JArray;
            for (int i = activeTemplates.Count - 1; i >= arr.Count ; i--)
            {
                GameObject.Destroy(activeTemplates[i].go);
                activeTemplates.RemoveAt(i);
            }
            for (int i = activeTemplates.Count; i < arr.Count; i++)
            {
                GameObject go = Instantiate(template,container.transform);
                go.SetActive(true);
                activeTemplates.Add(new CustomUIRadioTemplateData(go));
            }
            for (int i = 0; i < arr.Count; i++)
            {
                activeTemplates[i].title.text = arr[i].ToString();
                activeTemplates[i].value = arr[i];
            }
        }
        if (data.ContainsKey("names"))
        {
            JArray arr = data["names"] as JArray;
            for (int i = 0; i < arr.Count; i++)
            {
                if (activeTemplates.Count > i)
                {
                    activeTemplates[i].title.text = LocalizeManager.GetLocalString(arr[i].ToString());
                }
            }
        }
        prev = data.ValueOrDefault<int>("default", 0);
        activeTemplates[prev].toggle.isOn = true;
        //if (data.ContainsKey("default"))
        //{
        //    prev = (int)data["default"];
        //    activeTemplates[(int)data["default"]].toggle.isOn = true;
        //}
        //else
        //{
        //    prev = 0;
        //    activeTemplates[0].toggle.isOn = true;
        //}
    }

    public override void CompleteSetting()
    {
        base.CompleteSetting();
        OnChangeToggle();
    }

    protected virtual void OnChangeToggle()
    {
        for (int i = 0; i < activeTemplates.Count; i++)
        {
            if (activeTemplates[i].toggle.isOn)
            {
                prev = i;
            }
        }
        EmitEvent("changed");
    }

    protected virtual void Update()
    {
        if (!activeTemplates[prev].toggle.isOn)
        {
            OnChangeToggle();
        }
    }
}
