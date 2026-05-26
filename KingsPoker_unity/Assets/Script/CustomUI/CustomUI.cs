using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;

public class CustomUI : MonoBehaviour
{
    protected string _key = "";
    public string key
    {
        get
        {
            return _key;
        }
    }
    protected List<string> my_groups = new List<string>();
    public string type = "";
    protected JObject eventData = null;
    protected JObject settingData = null;
    private string style = "";
    private bool firstEnable = true;
    protected virtual string defaultStyle => "";

    private CustomUIRoot _root;
    public CustomUIRoot root
    {
        get
        {
            return _root;
        }
        set
        {
            _root = value;
        }
    }

    public virtual void SetValue(JToken d)
    {

    }

    public virtual void SetMin(JToken d)
    {

    }

    public virtual void SetMax(JToken d)
    {

    }

    protected void EmitEvent(string evt)
    {
        if (eventData == null) return;
        if (!eventData.ContainsKey(evt)) return;
        JArray evtList = eventData[evt] as JArray;
        for(int i = 0; i < evtList.Count; i++)
        {
            string eventKey = evtList[i]["event"].ToString();
            if(root.events.ContainsKey(eventKey))
            {
                CustomUIRoot.CustomUIEvent func = root.events[eventKey];
                func(this, settingData, evtList[i] as JObject);
            }
        }
    }

    public virtual void ChangeActiveWithOtherCustomUI(bool active)
    {

    }

    public virtual void Clear()
    {
        EmitEvent("clear");
    }

    public virtual void SetUI(JObject data)
    {
        settingData = data;
        if (data != null)
        {
            if(data.ContainsKey("event"))
            {
                if(data["event"].Type == JTokenType.Object)
                    eventData = data["event"] as JObject;
            }
            else
            {
                eventData = null;
            }
            EmitEvent("load");
            my_groups.Clear();
            for (int i = 0; true; i++)
            {
                string column = "group";
                if(i > 0)
                {
                    column += "_" + i.ToString();
                }
                if(data.ContainsKey(column))
                {
                    my_groups.Add(data[column].ToString());
                }
                else
                {
                    break;
                }
            }
            if (data.ContainsKey("key"))
            {
                _key = data["key"].ToString();
            }
            else
            {
                _key = "";
            }
            if(data.ContainsKey("style"))
            {
                style = data["style"].ToString();
            }
            else
            {
                style = "";
            }

            // JSON 에 "default_active": false 가 있으면 생성 시점부터 GameObject 를 비활성화.
            // Directive: fail-closed — 권한/상태 평가 코드가 도달하지 못해도 절대 노출되지 않음.
            if (data.ContainsKey("default_active"))
            {
                gameObject.SetActive((bool)data["default_active"]);
            }
        }
    }

    protected virtual void SetStyle(List<KeyValuePair<string,string>> data)
    {

    }

    public bool IsBelongToAGroup(string group)
    {
        return my_groups.Contains(group);
    }

    public virtual JToken GetValue()
    {
        return null;
    }

    public virtual JProperty GetProperty()
    {
        if (string.IsNullOrEmpty(key))
            return null;
        return new JProperty(key, GetValue());
    }

    public virtual void CompleteSetting() {
        string[] split = style.Split(';');
        List<KeyValuePair<string, string>> list = new List<KeyValuePair<string, string>>();
        for (int i = 0; i < split.Length; i++)
        {
            if (string.IsNullOrEmpty(split[i])) continue;
            string[] style_arr = split[i].Split('=');
            string style_key = style_arr[0].Replace(" ", "");
            string style_value = style_arr[1];
            list.Add(new KeyValuePair<string, string>(style_key, style_value));
        }


        string[] defaultSplit = defaultStyle.Split(';');
        for (int i = 0; i < defaultSplit.Length; i++)
        {
            if (string.IsNullOrEmpty(defaultSplit[i])) continue;
            string[] style_arr = defaultSplit[i].Split('=');
            string style_key = style_arr[0].Replace(" ","");
            string style_value = style_arr[1];
            if (list.FindIndex(data => data.Key == style_key) == -1)
                list.Add(new KeyValuePair<string, string>(style_key, style_value));
        }
        SetStyle(list);
        EmitEvent("load_end");
    }
}
