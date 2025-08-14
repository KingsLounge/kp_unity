using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ReserveToggle : ToggleEvent
{
    public ReserveBettingToggles reserveBetting;
    public POKER_RESERVE_TYPE reserveType;

    private void Awake()
    {
        if (toggle == null)
        {
            toggle = transform.GetComponent<Toggle>();
        }
    }

    public override void OnToggle()
    {
        base.OnToggle();

        reserveBetting.ReserveToggleChange(reserveType, toggle.isOn);
    }
}
