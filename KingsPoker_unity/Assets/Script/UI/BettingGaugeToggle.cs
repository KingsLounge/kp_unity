using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BettingGaugeToggle : ToggleEvent
{
    public BettingGauge bettingGauge;
    public int index;
    public bool isActive = true;

    public void Awake()
    {
        toggle = GetComponent<Toggle>();
        onImage = transform.GetChild(1).gameObject;
        offImage = transform.GetChild(0).gameObject;
        bettingGauge = transform.parent.GetComponent<BettingGauge>();
    }

    public override void OnToggle()
    {
        if(index == bettingGauge.currentIndex)
        {
            base.OnToggle();
        }
        else
        {
            base.OnToggle();

            if (isActive == true)
            {
                bettingGauge.OnBettingGaugeChanged(index);
                toggle.interactable = false;
                isActive = true;
                
                if (toggle.isOn == false)
                {
                    toggle.isOn = true;
                }
            }

            if (isActive == false)
            {
                isActive = true;
            }
        }
    }

    public void OnBettingGaugeChanged(bool isActive, bool isOn)
    {
        this.isActive = isActive;

        toggle.interactable = true;

        if (toggle.isOn != isOn)
        {
            toggle.isOn = isOn;
        }
        else
        {
            if (this.isActive == false)
            {
                this.isActive = true;
            }
            
        }
    }
}
