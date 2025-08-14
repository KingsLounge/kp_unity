using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class OptionToggle : MonoBehaviour
{
    public GameObject onImage;
    public GameObject offImage;
    public Toggle toggle;
    public delegate void onToggle(bool isOn);
    public onToggle t;
    public void Toggle()
    {
        onImage.SetActive(toggle.isOn);
        offImage.SetActive(!toggle.isOn);
        t(toggle.isOn);
    }
}
