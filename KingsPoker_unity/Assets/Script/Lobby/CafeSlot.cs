using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CafeSlot : MonoBehaviour
{
    [SerializeField]private string cafeKey;
    public void OnclickCafe()
    {
        LobbyTabsManager.Instance.CafeSet(cafeKey);
    }
}
