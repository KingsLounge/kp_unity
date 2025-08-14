using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DevServerList : MonoBehaviour
{
    public GameObject itemPrefab;

    private void Awake()
    {
        Debug.Log(ServerConfigManager.GetConfigSize());
        for (int i = 0; i < ServerConfigManager.GetConfigSize(); i++)
        {
            DevServerListItem item = Instantiate(itemPrefab, transform).GetComponent<DevServerListItem>();
            item.SetItem(i);
        }
    }
}
