using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class LobbyContent : MonoBehaviour
{
    [SerializeField]
    private string contentName;

    public string ContentName
    {
        get { return contentName; }
    }

    public UnityEvent contentInitEvent;
    
    public virtual void InitContent()
    {
        contentInitEvent.Invoke();
    }
}
