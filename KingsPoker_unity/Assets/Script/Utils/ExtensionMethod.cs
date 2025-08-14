using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.UI;

public static class ExtensionMethod
{
    private static Toggle.ToggleEvent EmptyToggleEvent = new Toggle.ToggleEvent();

    public static bool ContainsKey(this JObject obj, string key)
    {
        JProperty[] props = obj.Properties()
            .Where<JProperty>(
                (prop) =>
                {
                    return prop.Name == key;
                }
            )
            .ToArray();

        bool contains = false;
        for (int i = 0; i < props.Length; i++)
        {
            if (props[i].Value.Type != JTokenType.Null)
            {
                contains = true;
                break;
            }
        }
        return contains;
    }

    public static T CastOrEmpty<T>(this JObject obj, string key, bool nullChange = false)
        where T : JToken, new()
    {
        if (obj == null)
            return new T();
        if (obj.ContainsKey(key))
        {
            if (nullChange && obj[key].Type == JTokenType.Null)
                return new T();
            return obj[key] as T;
        }
        return new T();
    }

    public static T ValueOrDefault<T>(this JObject obj, string key, T defaultValue, bool nullChange = false)
    {
        if (obj == null)
            return defaultValue;
        if (obj.ContainsKey(key))
        {
            try
            {
                var velue = obj[key];
                if(nullChange && velue.Type == JTokenType.Null)
                {
                    return defaultValue;
                }
                return velue.ToObject<T>();
            }
            catch (Exception e)
            {
                Debug.LogWarning(e.Message);
            }
        }
        return defaultValue;
    }

    public static void DontEventCallToggleChange(this Toggle toggle, bool isOn)
    {
        var temp = toggle.onValueChanged;
        toggle.onValueChanged = EmptyToggleEvent;
        toggle.isOn = isOn;
        toggle.onValueChanged = temp;
    }

    public static string[] SplitAndTrimAll(this string str, params char[] separator)
    {
        string[] result = str.Split(separator);
        for (int i = 0; i < result.Length; i++)
        {
            result[i] = result[i].Trim();
        }
        return result;
    }

    public static T Clamp<T>(this T val, T min, T max)
        where T : IComparable<T>
    {
        if (val.CompareTo(min) < 0)
            return min;
        else if (val.CompareTo(max) > 0)
            return max;
        else
            return val;
    }

    public static string Color(this string s, string color)
    {
        return string.Format("<color={0}>{1}</color>", color, s);
    }

    public static void CleanIdentity(this Transform tr)
    {
        tr.localPosition = Vector3.zero;
        tr.localRotation = Quaternion.identity;
        tr.localScale = Vector3.one;
    }

    public static void Set<TKey, TValue>(this Dictionary<TKey, TValue> dic, TKey key, TValue value)
    {
        if (dic.ContainsKey(key))
        {
            dic[key] = value;
        }
        else
        {
            dic.Add(key, value);
        }
    }

    // 헥사값 컬러 반환( 코드 순서 : RGBA )
    public static Color HexColor(string hexCode)
    {
        Color color;
        if (ColorUtility.TryParseHtmlString(hexCode, out color))
        {
            return color;
        }

        Debug.LogError("[UnityExtension::HexColor]invalid hex code - " + hexCode);
        return UnityEngine.Color.white;
    }

    public static string GetLocalizedFormatString(this string f)
    {
        string format = f;
        string safe = "";
        bool open = false;
        string cur = "";
        for (int i = 0; i < format.Length; i++)
        {
            if (format[i] == '{')
                open = true;

            if ((open || format[i] == ' ') && cur.Length > 0)
            {
                safe += LocalizeManager.GetLocalString(cur);
                cur = "";
            }
            if (!open && format[i] != ' ')
            {
                cur += format[i];
            }
            else
            {
                safe += format[i];
            }
            if (format[i] == '}')
                open = false;
        }
        if (cur.Length > 0)
        {
            safe += LocalizeManager.GetLocalString(cur);
        }
        return safe;
    }

    public static string FormatWithLocalString(this string f, params object[] argv)
    {
        return string.Format(GetLocalizedFormatString(f), argv);
    }

    public static int ToInt32(this float f, int point = 4)
    {
        float t = 1;
        for (int i = 0; i < point; i++)
            t /= 10f;
        return (int)(f + t);
    }

    public static long ToInt64(this float f, int point = 4)
    {
        float t = 1;
        for (int i = 0; i < point; i++)
            t /= 10f;
        return (long)(f + t);
    }

    public static int ToInt32(this double f, int point = 4)
    {
        double t = 1;
        for (int i = 0; i < point; i++)
            t /= 10;
        return (int)(f + t);
    }

    public static long ToInt64(this double f, int point = 4)
    {
        double t = 1;
        for (int i = 0; i < point; i++)
            t /= 10;
        return (long)(f + t);
    }

    public static Sprite ToSprite(this Texture2D source)
    {
        if (source == null)
            return null;
        Rect rect = new Rect(0, 0, source.width, source.height);
        Sprite sprite = Sprite.Create(source, rect, new Vector2(0.5f, 0.5f));
        return sprite;
    }

    public static Texture2D DeCompress(this Texture2D source)
    {
        RenderTexture renderTex = RenderTexture.GetTemporary(
            source.width,
            source.height,
            0,
            RenderTextureFormat.Default,
            RenderTextureReadWrite.Linear
        );

        Graphics.Blit(source, renderTex);
        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = renderTex;
        Texture2D readableText = new Texture2D(source.width, source.height);
        readableText.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
        readableText.Apply();
        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(renderTex);
        return readableText;
    }
}
