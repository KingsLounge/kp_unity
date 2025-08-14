using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenGuard : MonoBehaviour
{
    public static ScreenGuard instance
    {
        get;private set;
    }

    ScreenGuard()
    {
        instance = this;
    }

    public void Activate()
    {
        gameObject.SetActive(true);
    }
    public void Inactivate()
    {
        gameObject.SetActive(false);
    }
}
