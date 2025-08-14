using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class NumberPad : MonoBehaviour
{
    private string inputStr = "0";
    public Slider slider = null;
    public List<Text> valueTexts = new List<Text>();
    public delegate double ClampNumber(double value);
    public ClampNumber clamper = null;
    public delegate string ValueToString(double value);
    public ValueToString valueToString = null;
    public delegate void OnValueChanged(double value);
    public OnValueChanged onValueChanged = null;
    public string defaultFormat = "#,##0.#";
    public int demicalMaxCount = 2;
    private double value = 0;

    private void OnEnable()
    {
        UpdateText();
    }

    public void OnClickNumber(string n)
    {
        int temp;
        string before = inputStr;
        if(int.TryParse(n,out temp))
        {

            inputStr += n;
        }
        else if(n == ".")
        {
            int dotCount = 0;
            for(int i = 0; i < inputStr.Length;i++)
            {
                if (inputStr[i] == '.')
                    dotCount++;
            }
            if (dotCount == 0)
                inputStr += n;
        }
        for (int i = 0; i < inputStr.Length; i++)
        {
            if (inputStr[i] == '.' && inputStr.Length > i + demicalMaxCount)
            {
                inputStr = inputStr.Remove(i + demicalMaxCount);
            }
        }
        if (before != inputStr)
            UpdateText();
    }

    public void OnClickBackspaceButton()
    {
        if(inputStr.Length > 0)
        {
            inputStr = inputStr.Remove(inputStr.Length - 1);
            if (inputStr.Length == 0) inputStr = "0";
            UpdateText();
        }
    }

    public void Clear()
    {
        SetValue(0);
    }

    public void UpdateText()
    {
        string calculatingNumber = inputStr;
        bool lastIsDot = calculatingNumber[calculatingNumber.Length - 1] == '.';
        if (lastIsDot)
        {
            calculatingNumber.Remove(calculatingNumber.Length - 1);
        }
        value = double.Parse(calculatingNumber);
        if(clamper != null)
        {
            value = clamper(value);
        }
        inputStr = value.ToString() + (lastIsDot ? "." : "");
        string resultStr = valueToString == null ? value.ToString(defaultFormat) : valueToString(value);
        if(lastIsDot)
        {
            resultStr += ".";
        }
        for(int i = 0; i < valueTexts.Count; i++)
        {
            valueTexts[i].text = resultStr;
        }
        onValueChanged?.Invoke(value);
    }

    public void Update()
    {
        if(Input.GetKeyDown(KeyCode.Alpha0) || Input.GetKeyDown(KeyCode.Keypad0))
        {
            OnClickNumber("0");
        }
        else if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
        {
            OnClickNumber("1");
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
        {
            OnClickNumber("2");
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))
        {
            OnClickNumber("3");
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4) || Input.GetKeyDown(KeyCode.Keypad4))
        {
            OnClickNumber("4");
        }
        else if (Input.GetKeyDown(KeyCode.Alpha5) || Input.GetKeyDown(KeyCode.Keypad5))
        {
            OnClickNumber("5");
        }
        else if (Input.GetKeyDown(KeyCode.Alpha6) || Input.GetKeyDown(KeyCode.Keypad6))
        {
            OnClickNumber("6");
        }
        else if (Input.GetKeyDown(KeyCode.Alpha7) || Input.GetKeyDown(KeyCode.Keypad7))
        {
            OnClickNumber("7");
        }
        else if (Input.GetKeyDown(KeyCode.Alpha8) || Input.GetKeyDown(KeyCode.Keypad8))
        {
            OnClickNumber("8");
        }
        else if (Input.GetKeyDown(KeyCode.Alpha9) || Input.GetKeyDown(KeyCode.Keypad9))
        {
            OnClickNumber("9");
        }
        else if (Input.GetKeyDown(KeyCode.KeypadPeriod) || Input.GetKeyDown(KeyCode.Period))
        {
            OnClickNumber(".");
        }
        else if (Input.GetKeyDown(KeyCode.Backspace))
        {
            OnClickBackspaceButton();
        }
    }

    public double GetValue()
    {
        return value;
    }

    public void SetValue(double value)
    {
        inputStr = value.ToString();
        UpdateText();
    }
}
