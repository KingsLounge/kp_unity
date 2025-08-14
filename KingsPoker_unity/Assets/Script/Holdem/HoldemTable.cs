using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HoldemTable : MonoBehaviour, IBeginDragHandler, IDragHandler
{
    private Vector2 dragStartPos = Vector2.zero;

    [SerializeField]
    private Vector2 dragDist = Vector2.zero;

    private bool isDragging = false;

    public void Awake()
    {
        dragDist *= (float)Screen.width / 1080;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log(eventData.position);
        dragStartPos = eventData.position;

        isDragging = true;
    }

    public void OnDrag(PointerEventData eventData)
    {
        Debug.Log(eventData.position);

        var distance = eventData.position.x - dragStartPos.x;

        if (Mathf.Abs(eventData.position.x - dragStartPos.x) > dragDist.x && isDragging)
        {
            isDragging = false;
            var gm = GamesManager.instance;
            if (gm != null)
            {
                int targetIdx = distance < 0 ? gm.currentGame + 1 : gm.currentGame - 1;
                Debug.Log(targetIdx);
                if (gm.CheckGameIndex(targetIdx))
                {
                    gm.tabsList[targetIdx].GetComponent<Toggle>().isOn = true;
                }
            }
        }
    }

}
