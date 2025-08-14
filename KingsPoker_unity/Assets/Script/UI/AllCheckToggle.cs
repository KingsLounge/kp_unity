using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class AllCheckToggle : MonoBehaviour
{
    [SerializeField]
    private List<Toggle> toggles = new List<Toggle>();
    private Toggle myToggle;
    private bool checking = false;

    private void Awake()
    {
        myToggle = GetComponentInChildren<Toggle>();
        toggles.ForEach(toggle => toggle.onValueChanged.AddListener(ContentsToggleValueChanged));
        myToggle.onValueChanged.AddListener(AllCheck);
        CheckInspection();
    }

    private void ContentsToggleValueChanged(bool check)
    {
        if (checking)
            return;
        CheckInspection();
    }

    private void AllCheck(bool check)
    {
        checking = true;
        toggles.ForEach(toggle => toggle.isOn = check);
        checking = false;
    }

    private void CheckInspection()
    {
        bool allCheck = true;
        toggles.ForEach(toggle =>
        {
            if (!toggle.isOn)
                allCheck = false;
        });
        myToggle.SetIsOnWithoutNotify(allCheck);
    }
}
