using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UISensor : MonoBehaviour,IPointerEnterHandler,IPointerExitHandler,IPointerDownHandler,IPointerUpHandler
{
    public System.Action<PointerEventData> PointerDown = null;
    public System.Action<PointerEventData> PointerUp = null;
    public System.Action<PointerEventData> PointerExit = null;
    public System.Action<PointerEventData> PointerEnter = null;

    public void OnPointerEnter(PointerEventData eventData)
    {
        PointerEnter?.Invoke(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        PointerUp?.Invoke(eventData);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        PointerExit?.Invoke(eventData);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        PointerDown?.Invoke(eventData);
    }
}
