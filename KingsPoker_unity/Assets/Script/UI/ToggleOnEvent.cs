using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
public class ToggleOnEvent : ToggleEvent
{
    public UnityEvent Event;
    public bool autoRegister = true;
    private void Awake() {
        if(autoRegister)
        {
            toggle = GetComponent<Toggle>();
            toggle.onValueChanged.AddListener(Toggle);
        }
    }
    public void Toggle(bool isOn)
    {
        if(isOn)
        {
            Event.Invoke();
        }
        else 
        {
            
        }
    }
}
