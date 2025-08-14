using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//Start때 자동으로 초기세팅 하기위한 컴포넌트
public class ToggleReminderInStart : MonoBehaviour
{
    public void Start()
    {
        Toggle toggle = GetComponent<Toggle>();
        toggle.onValueChanged.Invoke(toggle.isOn);
    }
}
