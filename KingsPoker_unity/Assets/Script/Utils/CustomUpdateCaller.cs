using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScheduleData
{
    public delegate void ScheduleCallback();
    public System.Action callback;
    public bool loop;
    public float delay;
    public float timer;
    public int instanceID = 0;
    public string key = "";
}
public class CustomUpdateCaller : MonoBehaviour
{
    private int instanceID = 0;
    private static CustomUpdateCaller instance = null;
    public static CustomUpdateCaller Instance {
        get {
            return instance;
        }
    }
    public delegate void CustomUpdate(double dt);
    public CustomUpdate PreUpdate = delegate(double dt) {
    };

    private List<ScheduleData> scheduleList = new List<ScheduleData>();
    // Start is called before the first frame update
    void Awake() {
        instance = this;
    }

    public void UnSchedule(ScheduleData data)
    {
        if (data == null)
            return;
        int index = scheduleList.IndexOf(data);
        if(index != -1)
        {
            scheduleList.RemoveAt(index);
        }
    }
    public void UnSchedule(string key)
    {
        for(int i = scheduleList.Count - 1; i >= 0; i--)
        {
            if(scheduleList[i].key == key)
            {
                scheduleList.RemoveAt(i);
            }
        }
    }
    // Update is called once per frame
    void Update()
    {
        PreUpdate(Time.deltaTime);
        for(int i = scheduleList.Count - 1; i >= 0; i--) {
            scheduleList[i].timer += Time.deltaTime;
            if(scheduleList[i].timer >= scheduleList[i].delay) {
                scheduleList[i].timer -= scheduleList[i].delay;
                ScheduleData data = scheduleList[i];
                if (!scheduleList[i].loop)
                {
                    scheduleList.RemoveAt(i);
                }
                try
                {
                    if(data.callback != null)
                    {
                        data.callback();
                    }
                }
                catch
                {
                    Console.Error("Error Schedule ID : " + data.instanceID);
                }
            }
        }
    }

    public void PlayCoroutine(IEnumerator coroutine)
    {
        StartCoroutine(coroutine);
    }

    public ScheduleData Schedule(System.Action callback,float delay, string key = "") {
        ScheduleData data = new ScheduleData();
        data.callback = callback;
        data.delay = delay;
        data.timer = 0;
        data.loop = true;
        data.instanceID = instanceID++;
        data.key = key;
        scheduleList.Add(data);
        return data;
    }

    public ScheduleData ScheduleOnce(System.Action callback,float delay, string key = "") {
        ScheduleData data = new ScheduleData();
        data.callback = callback;
        data.delay = delay;
        data.timer = 0;
        data.loop = false;
        data.instanceID = instanceID++;
        data.key = key;
        scheduleList.Add(data);
        return data;
    }
}
