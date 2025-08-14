using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Console
{
    public static void Log(object data) {
        if(DevOptionsManager.devOptions !=null && DevOptionsManager.devOptions.mode == MODE.dev)
        {
            Debug.Log(data);
            LogTextManager.Instance.TextLog(data.ToString());
        } 
    }

    public static void SpecialLog(object data)
    {
        if (DevOptionsManager.devOptions !=null && DevOptionsManager.devOptions.mode == MODE.dev)
        {
            Debug.Log("<size=20><color=red>■</color><color=blue>■</color><color=green>■</color><color=yellow>■</color><color=magenta>■</color><color=#007ACC>■</color></size>" + data);
            LogTextManager.Instance.TextLog(data.ToString());
        }
    }

    public static void Error(object data) {
        if(DevOptionsManager.devOptions !=null && DevOptionsManager.devOptions.mode == MODE.dev)
        {
            Debug.LogError(data);
            LogTextManager.Instance.TextLog(string.Format("<color=red>{0}</color>", data.ToString()));
        }
    }
}
