using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ChangeSizeSemaphore : MonoBehaviour
{
    private ChangeSizeSemaphore parent;
    public UnityAction changeSizeEvent;

    public void SetParentComponent(ChangeSizeSemaphore p)
    {
        parent = p;
    }

    private void Awake()
    {
        
    }

    public void ReceiveSignal()
    {

    }

    private void OnRectTransformDimensionsChange()
    {
        if(parent)
        {
            parent.ReceiveSignal();
        }
    }
}
