using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
[ExecuteAlways]
public class ToggleHelper : MonoBehaviour
{    
    public Vector3 onAddPosition = Vector3.zero;
    private Toggle toggle;
    private bool init = false;
    private bool prevOn = false;
    public bool disableOff = true;
    void Awake()
    {
        Init();
    }

    private void Init()
    {
        if (init) return;
        init = true;
        toggle = GetComponent<Toggle>();
        prevOn = toggle.isOn;
    }

    // Update is called once per frame
    void Update()
    {
        if(prevOn != toggle.isOn) {
            prevOn = toggle.isOn;
            if(prevOn) {
                OnBehaviour();
            } else {
                OffBehaviour();
            }
        }
    }

    void OnDisable() {
        if(prevOn && disableOff) {
            prevOn = false;
            OffBehaviour();
        }
    }

    public void SetActive(bool active)
    {
        Init();
        toggle.isOn = active;
    }

    public void Switching()
    {
        Init();
        toggle.isOn = !toggle.isOn;
    }

    void OnBehaviour() {
        transform.localPosition += onAddPosition;
    }

    void OffBehaviour() {
        transform.localPosition -= onAddPosition;
    }
}
