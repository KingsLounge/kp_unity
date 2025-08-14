using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToggleText : MonoBehaviour
{
    private Text text;
    private Toggle toggle;
    // Start is called before the first frame update
    void Awake()
    {
        toggle = gameObject.GetComponentInParent<Toggle>();
        text = GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        text.enabled = toggle.isOn;
    }
}
