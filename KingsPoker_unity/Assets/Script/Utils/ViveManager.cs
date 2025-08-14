

using distriqt.plugins.vibration;
using UnityEngine;

public static class ViveManager
{
    private static bool isInit = false;
    private static bool isVive = false;
    private static bool support = false;
    private static void Init()
    {
        isInit = true;
        isVive = bool.Parse(PlayerPrefs.GetString("vive", true.ToString()));
        support = Vibration.isSupported;
    }

    public static void SetVive(bool value)
    {
        isVive = value;
        PlayerPrefs.SetString("vive", value.ToString());
    }

    public static void Vibrate(int duration)
    {
        if (!isInit) Init();
        if (isVive && support) Vibration.Instance.Vibrate(duration);
    }
}
