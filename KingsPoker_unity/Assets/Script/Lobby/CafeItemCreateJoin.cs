using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class CafeItemCreateJoin : MonoBehaviour
{
    public Cafe manager;

    public CustomUIOpener popupOpener;

    public Button btnCreateCafe;
    public Button btnJoinCafe;

    private void Awake()
    {
        btnCreateCafe.onClick.RemoveAllListeners();
        btnCreateCafe.onClick.AddListener(delegate { OnClickCreateCafe(); });
        btnJoinCafe.onClick.RemoveAllListeners();
        btnJoinCafe.onClick.AddListener(delegate { OnClickJoinCafe(); });
    }
    private void OnDestroy()
    {
    }

    public void OnClickCreateCafe()
    {
        popupOpener.ShowUI("cafe_create");
    }
    public void OnClickJoinCafe()
    {
        popupOpener.ShowUI("cafe_join_1");
        // LobbyTabsManager.Instance.CafeSet("defaltCafe");
    }
}
