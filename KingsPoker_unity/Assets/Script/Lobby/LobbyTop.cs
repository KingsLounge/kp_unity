using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyTop : MonoBehaviour
{
    [SerializeField]
    private LocalText contentNameLocal;

    public void SetTopName(string str)
    {
        contentNameLocal.LocalKey = str;
    }
}
