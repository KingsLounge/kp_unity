using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BreakPoint : MonoBehaviour, IMoveHandler
{
    public void OnMove(AxisEventData eventData)
    {
        if (transform.localPosition.y < -1)
            Debug.Log("Here!");
    }
}
