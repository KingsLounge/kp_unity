using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ToggleEventCaller : MonoBehaviour
{
    private Toggle toggle;
    public Button.ButtonClickedEvent toggleOnEvent;
    public Button.ButtonClickedEvent toggleOffEvent;

    public void Awake()
    {
        toggle = GetComponent<Toggle>();
        toggle.onValueChanged.AddListener((check) =>
        {
            if(check)
            {
                toggleOnEvent.Invoke();
            }
            else
            {
                toggleOffEvent.Invoke();
            }
        });
    }
}
