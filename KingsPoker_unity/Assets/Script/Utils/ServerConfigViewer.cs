using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ServerConfigViewer : MonoBehaviour
{
    public GameObject serverListItem;
    public Transform serverListContainer;
    public InputField owner;
    public InputField apiAddress;
    public InputField gsAddress;
    public InputField gameVersion;

    public int index = 0;

    private void Awake()
    {
        for (int i = 0; i < ServerConfigManager.GetConfigSize(); i++)
        {
            GameObject go = Instantiate(serverListItem, serverListContainer);
            go.GetComponent<DevServerListItem>().SetItem(i, this);
            if (i == 0)
            {
                go.GetComponent<Button>().onClick.Invoke();
                go.GetComponent<Button>().Select();
            }
        }
    }

    public void ChangeServerConfigItem(int index)
    {
        owner.text = ServerConfigManager.GetConfig(index, "title").ToString();
        apiAddress.text = (string)ServerConfigManager.GetConfig(index, "pub_url");
        gsAddress.text = (string)ServerConfigManager.GetConfig(index, "gs_url");
        gameVersion.text = ServerConfigManager.GetConfig(index, "version").ToString();
        this.index = index;
    }

    public void OnClickConnectButton()
    {
        DevOptionsManager.ChangeServer(index);
        GamePreprocessing.instance.StartPup();
        gameObject.SetActive(false);
    }
}
