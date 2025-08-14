using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;

public class LoginCheckQuest : MonoBehaviour
{
    public GameObject loginQuestSlotPrefab;
    public List<DailyLoginSlot> questSlot;
    public Transform container;
    public int maxDay = 7;
    public bool SetLoginCheckData()
    {
        if (MyStatus.isLoginRewarded)
        {
            return false;
        }
        else
        {
            if(questSlot == null)
            {
                questSlot = new List<DailyLoginSlot>();
            }
            for(int i = 0; i < maxDay; i++)
            {
                DailyLoginSlot go;
                if(i < questSlot.Count)
                {
                    go = questSlot[i];
                }
                else
                {
                    go = Instantiate(loginQuestSlotPrefab, container).GetComponent<DailyLoginSlot>();
                    questSlot.Add(go);
                }
                go.SetDailyQuestSlotSet(i+1, MyStatus.loginContinued);
            }
            gameObject.SetActive(true);
        }
        return true;
    }
}
