using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using UnityEngine;

public class LimitChangeHistoryWindow : MonoBehaviour
{
    private const string back_button_key = "limit_change_popup_close";
    public Transform limitHistoryContainer;
    public GameObject limitHistorySlotPrefab;
    public List<LimitChangeHistorySlot> limitChangeHistoryList = new List<LimitChangeHistorySlot>();
    private JArray history;

    //private void OnEnable()
    //{
    //    BackButtonManager.Instance.AddBehaviour(back_button_key, () =>
    //    {
    //        gameObject.SetActive(false);
    //    });
    //}

    //private void OnDisable()
    //{
    //    BackButtonManager.Instance.RemoveBehaviour(back_button_key);
    //}

    public void Init(JArray history)
    {
        this.history = history;
        Init();
    }
    public void Init()
    {
        for (int i = 0; i < history.Count; i++)
        {
            JObject d = history[i].ToObject<JObject>();
            LimitChangeHistorySlot slot;
            if (i < limitChangeHistoryList.Count)
            {
                slot = limitChangeHistoryList[i];
            }
            else
            {
                slot = Instantiate(limitHistorySlotPrefab, limitHistoryContainer).GetComponent<LimitChangeHistorySlot>();
                limitChangeHistoryList.Add(slot);
            }
            slot.Init(d, i);
        }
    }
}
