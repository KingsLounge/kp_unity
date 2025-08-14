using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.UI;

public class CustomUIInput : CustomUI
{
    public Text title;
    public InputField inputField;
    public Text subText;
    private JTokenType dataType = JTokenType.None;
    private bool hasMin = false;
    private bool hasMax = false;
    private double min = 0;
    private double max = 0;
    private int decimal_point = 1;
    protected override string defaultStyle => "sub-text=;placeholder=;decimal-point=1;color=#000000";

    public override void SetUI(JObject data)
    {
        base.SetUI(data);
        if(title)
        {
            if (data.ContainsKey("title"))
            {
                title.text = LocalizeManager.GetLocalString(data["title"].ToString());
            }
            else
            {
                title.text = key;
            }
        }
        hasMax = data.ContainsKey("max");
        if (data.ContainsKey("max"))
        {
            var m = data["max"];
            max = (double)data["max"];
        }
        hasMin = data.ContainsKey("min");
        if (hasMin)
        {
            min = (double)data["min"];
        }
        dataType = JTokenType.Integer;
        bool hasDefault = false;
        if (data.ContainsKey("default"))
        {
            hasDefault = true;
            inputField.text = data["default"].ToString();
            dataType = data["default"].Type;
        }
        if (data.ContainsKey("data_type"))
        {
            string t = data["data_type"].ToString();
            if (t == "int")
                dataType = JTokenType.Integer;
            if (t == "float")
                dataType = JTokenType.Float;
            if (t == "string")
                dataType = JTokenType.String;
        }
        if(!hasDefault)
        {
            switch (dataType)
            {
                case JTokenType.Float:
                case JTokenType.Integer:
                    inputField.text = "0";
                    break;
                case JTokenType.String:
                    inputField.text = "";
                    break;
            }
        }
        bool isPassword = false;
        if (data.ContainsKey("data"))
        {
            if (data["data"].ToString() == "password")
            {
                isPassword = true;
            }
        }
        switch (dataType)
        {
            case JTokenType.String:
                if(isPassword)
                {
                    inputField.contentType = InputField.ContentType.Password;
                }
                else
                {
                    inputField.contentType = InputField.ContentType.Standard;
                    inputField.keyboardType = TouchScreenKeyboardType.Default;
                }
                if (hasMax)
                {
                    inputField.characterLimit = (int)max;
                }
                break;
            case JTokenType.Integer:
                if(isPassword)
                {
                    inputField.contentType = InputField.ContentType.Custom;
                    inputField.inputType = InputField.InputType.Password;
                    inputField.characterValidation = InputField.CharacterValidation.Integer;
                    if (hasMax)
                        inputField.characterLimit = (int)max;
                }
                else
                {
                    inputField.inputType = InputField.InputType.Standard;
                    inputField.contentType = InputField.ContentType.IntegerNumber;
                }
                inputField.keyboardType = TouchScreenKeyboardType.NumberPad;
                break;
            case JTokenType.Float:
                if (isPassword)
                {
                    inputField.inputType = InputField.InputType.Password;
                    if (hasMax)
                        inputField.characterLimit = (int)max;
                }
                else
                {
                    inputField.inputType = InputField.InputType.Standard;
                }
                inputField.characterValidation = InputField.CharacterValidation.Decimal;
                inputField.contentType = InputField.ContentType.DecimalNumber;
                inputField.keyboardType = TouchScreenKeyboardType.DecimalPad;
                break;
            case JTokenType.None:
                if (isPassword)
                {
                    inputField.inputType = InputField.InputType.Password;
                    if (hasMax)
                        inputField.characterLimit = (int)max;
                }
                else
                {
                    inputField.inputType = InputField.InputType.Password;
                }
                inputField.contentType = InputField.ContentType.Standard;
                inputField.keyboardType = TouchScreenKeyboardType.Default;
                inputField.characterValidation = InputField.CharacterValidation.None;
                break;
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
                    if (value == "left") title.alignment = TextAnchor.MiddleLeft;
                    else if (value == "center") title.alignment = TextAnchor.MiddleCenter;
                    else if (value == "right") title.alignment = TextAnchor.MiddleRight;
                    break;
                case "placeholder":
                    (inputField.placeholder as Text).text = LocalizeManager.GetLocalString(value);
                    break;

                case "font-size":
                    // title.fontSize = int.Parse(value);
                    // subText.fontSize = int.Parse(value);
                    break;

                case "sub-text":
                    if(subText != null)
                    {
                        subText.text = LocalizeManager.GetLocalString(value);
                    }
                    break;
                case "decimal-point":
                    decimal_point = int.Parse(value);
                    break;
                case "color":
                    Color color;
                    if (ColorUtility.TryParseHtmlString(value, out color))
                        (inputField.targetGraphic as Image).color = color;
                    break;
            }
        }
    }

    public override void SetMax(JToken d)
    {
        base.SetMax(d);
        hasMax = true;
        max = (double)d;
        RefreshText(inputField.text);
    }

    public void Awake()
    {
        inputField.onEndEdit.AddListener(OnEndEdit);
    }

    private void OnEndEdit(string text)
    {
        RefreshText(text);
        EmitEvent("changed");
    }

    private void RefreshText(string text)
    {

        try
        {
            if (dataType == JTokenType.Float)
            {
                if (string.IsNullOrEmpty(text))
                {
                    text = "0";
                }
                string[] arr = text.Split('.');
                string temp = arr.Length > 1 ? arr[1] : "";
                text = arr[0];
                for (int i = 0; i < temp.Length && i < decimal_point; i++)
                {
                    if (i == 0) text += ".";
                    text += temp[i];
                }
                double val = double.Parse(text);
                if (hasMin && val < min) val = min;
                if (hasMax && val > max) val = max;
                inputField.text = val.ToString();
            }
            else if (dataType == JTokenType.Integer)
            {
                if (string.IsNullOrEmpty(text))
                {
                    text = "0";
                }
                long val = long.Parse(text);
                if (hasMin && val < min) val = (long)min;
                if (hasMax && val > max) val = (long)max;
                inputField.text = val.ToString();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError(e.Message);
        }
    }

    public override void SetValue(JToken d)
    {
        base.SetValue(d);
        JToken value = GetValue();
        inputField.text = d.ToString();
        if (!JToken.DeepEquals(value, d))
        {
            OnEndEdit(inputField.text);
        }
    }

    public override JToken GetValue()
    {
        if (dataType == JTokenType.Integer)
        {
            if (string.IsNullOrEmpty(inputField.text)) OnEndEdit(inputField.text);
            return long.Parse(inputField.text);
        }
        else if (dataType == JTokenType.Float)
        {
            if (string.IsNullOrEmpty(inputField.text)) OnEndEdit(inputField.text);
            return float.Parse(inputField.text);
        }
        return inputField.text;
    }

    public override void CompleteSetting()
    {
        base.CompleteSetting();
        OnEndEdit(inputField.text);
    }
}
