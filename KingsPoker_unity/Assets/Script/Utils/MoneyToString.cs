using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

class MoneyToString
{
    private static bool bbText = false;
    public static bool BBText
    {
        get { return bbText; }
    }
    private static Dictionary<long, string> unit;

    private static void Init()
    {
#if A9J
        unit = new Dictionary<long, string>();
        unit.Add(1000, ",");
        unit.Add(1000000, ",");
        unit.Add(1000000000, ",");
#else
        unit = new Dictionary<long, string>();
        unit.Add(10000, "만");
        unit.Add(100000000, "억");
        unit.Add(1000000000000, "조");
#endif
        bbText = PlayerPrefs.GetString("bb_text", "false") == "true";
        SetBBText(bbText);
        bbOptionChangeEvent?.Invoke();
    }

    public class ChangeEvent : UnityEvent { };

    public static ChangeEvent bbOptionChangeEvent = new ChangeEvent();
    public static void AddBBOptionChangeEvent(UnityAction action)
    {
        bbOptionChangeEvent.AddListener(action);
    }

    public static void RemoveBBOptionChangeEvent(UnityAction action)
    {
        bbOptionChangeEvent.RemoveListener(action);
    }

    public class BBChangeEvent : UnityEvent<long> { };

    public static BBChangeEvent bbChangeEvent = new BBChangeEvent();
    public static void AddBBChangeEvent(UnityAction<long> action)
    {
        bbChangeEvent.AddListener(action);
    }

    public static void RemoveBBChangeEvent(UnityAction<long> action)
    {
        bbChangeEvent.RemoveListener(action);
    }

    public static void SetBBText(bool set)
    {
        bbText = set;
        PlayerPrefs.SetString("bb_text", bbText ? "true" : "false");
        bbOptionChangeEvent?.Invoke();
    }

    public static string ConvertingBB(long money, long bb)
    {
        if (unit == null)
        {
            Init();
        }
        if (bbText && bb > 0)
        {
            var convertBB = money / (float)bb;
            return $"{convertBB:,0.##}BB";
        }
        else
        {
            return Converting(money, unit);
        }
    }

    public static void OnChangeBB(long rn)
    {
        bbChangeEvent?.Invoke(rn);
    }

    public static string Converting(long money)
    {
        if (unit == null)
        {
            Init();
        }

        return Converting(money, unit);
    }

    public static string Converting(ulong money)
    {
        if (unit == null)
        {
            Init();
        }

        return Converting((long)money, unit);
    }

    public static string Converting(long money, Dictionary<long, string> unit)
    {
        //return string.Format("{0:#,##0.##}", money);

        string result = "";
        string str = money.ToString();
        int cur = 0;
        for (int i = str.Length - 1; i >= 0; i--)
        {
            long a = 1;
            for (int j = 0; j < cur; j++)
            {
                a *= 10;
            }
            cur++;
            if (unit.ContainsKey(a))
            {
                result = unit[a] + result;
            }
            result = str[i] + result;
        }
        int startIdx = -1;
        for (int i = result.Length - 1; i >= 0; i--)
        {
            int numcheck = -1;
            if (!int.TryParse(result[i].ToString(), out numcheck))
            {
                if (startIdx != -1)
                {
                    result = result.Remove(i + 1, startIdx - i);
                }
                startIdx = -1;
            }
            if (numcheck == 0)
            {
                if (startIdx == -1)
                {
                    startIdx = i;
                }
            }
            else
                startIdx = -1;
        }
        return result;
    }

    public static long StringToMoney(string str)
    {
        if (unit == null)
        {
            Init();
        }
        return long.Parse(str.Replace(@"[^0-9]", ""));
    }

    private static readonly SortedDictionary<int, string> abbrevations = new SortedDictionary<
        int,
        string
    >
    {
        { 1000, "K" },
        { 1000000, "M" },
        { 1000000000, "B" }
    };

    public static string AbbreviateNumber(float number)
    {
        for (int i = abbrevations.Count - 1; i >= 0; i--)
        {
            KeyValuePair<int, string> pair = abbrevations.ElementAt(i);
            if (Mathf.Abs(number) >= pair.Key)
            {
                int roundedNumber = Mathf.FloorToInt(number / pair.Key);
                return roundedNumber.ToString() + pair.Value;
            }
        }
        return number.ToString();
    }

    //public static string ToKMB( decimal num)   반올림된다.
    //{
    //    if (num > 999999999 || num < -999999999)
    //    {
    //        return num.ToString("0,,,.###B", CultureInfo.InvariantCulture);
    //    }
    //    else
    //    if (num > 999999 || num < -999999)
    //    {
    //        return num.ToString("0,,.##M", CultureInfo.InvariantCulture);
    //    }
    //    else
    //    if (num > 999 || num < -999)
    //    {
    //        return num.ToString("0,.#K", CultureInfo.InvariantCulture);
    //    }
    //    else
    //    {
    //        return num.ToString(CultureInfo.InvariantCulture);
    //    }
    //}

    public static string ToKMB(double d)
    {
        double exponent = Math.Log10(Math.Abs(d));
        if (Math.Abs(d) >= 1)
        {
            switch ((int)Math.Floor(exponent))
            {
                case 0:
                case 1:
                case 2:
                    return d.ToString();
                case 3:
                case 4:
                case 5:
                    return (d / 1e3).ToString() + "K";
                case 6:
                case 7:
                case 8:
                    return (d / 1e6).ToString() + "M";
                case 9:
                case 10:
                case 11:
                    return (d / 1e9).ToString() + "B";
                default:
                    return d.ToString();
            }
        }
        else
        {
            return "0";
        }
    }
}
