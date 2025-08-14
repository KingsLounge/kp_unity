using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.UI;
using pingak9;
using System;

public class CustomUIDate : CustomUI
{
    [SerializeField]
    private InputField timeInfo;
    private DateTime _pickTime;
    private DateTime pickTime
    {
        get
        {
            return _pickTime;
        }
        set
        {
            Debug.Log("edited Pick Time");
            _pickTime = value;
            timeInfo.SetTextWithoutNotify(_pickTime.ToString("yyyy'/'MM'/'dd HH:mm:ss"));
        }
    }
    private DateTime day;

    private void Awake()
    {
        timeInfo.onEndEdit.AddListener(OnEndEditInputField);
    }

    public void OnEndEditInputField(string str)
    {
        DateTime pick;
        if(DateTime.TryParse(str,out pick))
        {
            pickTime = pick;
        }
        else
        {
            pickTime = pickTime;
        }
    }

    public override void SetUI(JObject data)
    {
        base.SetUI(data);

        pickTime = DateTime.Now;
    }

    public override JToken GetValue()
    {
        return pickTime;
    }

    public void OnClickSelectButton()
    {
#if UNITY_EDITOR
        timeInfo.ActivateInputField();
#else
        DateTime now = DateTime.Now;
        NativeDialog.OpenDatePicker(now.Year, now.Month, now.Day, null, OnCloseDatePicker);
#endif
    }

    private void OnCloseDatePicker(DateTime day)
    {
        this.day = day;
        NativeDialog.OpenTimePicker(null, OnCloseTimePicker);
        Debug.Log("DatePicker Closed");
    }

    private void OnCloseTimePicker(DateTime time)
    {
        Debug.Log("TimePicker Closed");
        pickTime = day + new TimeSpan(time.Hour, time.Minute, time.Second);
    }
}
