using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToggleEvent : MonoBehaviour
{
    public Toggle toggle;
    public GameObject onImage;
    public GameObject offImage;
    private void Awake() {
        toggle = GetComponent<Toggle>();
    }

    public virtual void OnToggle()
    {
        if (toggle.isOn == true)
        {
            if (offImage != null)
            {
                offImage.SetActive(false);
            }
            if (onImage != null)
            {
                onImage.SetActive(true);
            }
            Debug.Log(gameObject.name + " 켰다");
        }
        else if (toggle.isOn == false)
        {
            if (offImage != null)
            {
                offImage.SetActive(true);
            }
            if (onImage != null)
            {
                onImage.SetActive(false);
            }

            Debug.Log(gameObject.name + " 껐다");
        }
    }
}


