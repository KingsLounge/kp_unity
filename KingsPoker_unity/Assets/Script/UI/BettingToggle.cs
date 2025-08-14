using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BettingToggle : ToggleEvent
{
    public BettingGauge gauge;
    public BETTING_TOGGLE_TYPE type;
    public override void OnToggle()
    {
        base.OnToggle();
        if (toggle.isOn == true)
        {
            //gauge.OnToggelChange(type);
        }
        else if (toggle.isOn == false)
        {

        }
    }

    

}


