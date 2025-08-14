using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

public enum MODE
{
    prod,
    dev
}

[Serializable]
public class DevOptions
{
    public int serverIndex = 0; // 0: AWS, 1: JH
    public int bettingMode = 1; // 0은 kr 1은 int
    public MODE mode = MODE.prod;
    public bool autologin = false;

    public DevOptions()
    {
        SetInit();
    }

    public DevOptions(JObject data)
    {
        SetInit();
        if (data["mode"] != null)
        {
            mode = (MODE)data["mode"].Value<int>();
        }
        if (data["serverIndex"] != null)
        {
            serverIndex = data["serverIndex"].Value<int>();
        }
        if (data["autoLogin"] != null)
        {
            autologin = data["autoLogin"].Value<bool>();
        }
    }

    private void SetInit()
    {
        serverIndex = 0;
        bettingMode = 1;

#if UNITY_EDITOR
        mode = MODE.dev;
#else
        mode = MODE.prod;
#endif
        autologin = false;
    }
}

public class DevOptionsManager : MonoBehaviour
{
    public static DevOptions devOptions;

    public GameObject logbutton;

    private void Awake()
    {
        Init();
        if (logbutton)
        {
            logbutton.SetActive(devOptions.mode != 0);
        }
    }

    public static void Init()
    {
        string path = Path.Combine(Application.persistentDataPath, "devConfig.json");
        Debug.Log(Path.Combine(Application.persistentDataPath, "devConfig.json"));

        FileInfo fileInfo = new FileInfo(path);

        if (fileInfo.Exists == false)
        {
            devOptions = new DevOptions();
        }
        else
        {
            try
            {
                FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
                StreamReader sr = new StreamReader(fs);
                JObject json = JObject.Parse(sr.ReadToEnd());
                devOptions = new DevOptions(json);
                sr.Close();
                fs.Close();
                Console.Log("파일 읽기 완료");
            }
            catch (Exception e)
            {
                devOptions = new DevOptions();
            }
        }

#if A9J
        // 2020-12-17 프로토타이핑 제작
        //devOptions.mode = MODE.dev;
#endif
        Console.Log(
            string.Format(
                "serverIndex: {0}    bettingMode:{1}",
                devOptions.serverIndex,
                devOptions.bettingMode
            )
        );
    }

    public static void SaveDevConfig(string s)
    {
        try
        {
            string path = Path.Combine(Application.persistentDataPath, "devConfig.json");
            FileStream fs = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            StreamWriter sw = new StreamWriter(fs);

            sw.WriteLine(s);
            sw.Close();
            fs.Close();
            Console.Log("파일 쓰기 완료");
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }

    public static void ChangeServer(int index)
    {
        devOptions.serverIndex = index;
    }
}
