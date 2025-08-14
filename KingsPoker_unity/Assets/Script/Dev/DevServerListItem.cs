using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DevServerListItem : MonoBehaviour
{
    public Text descText;
    public int index = -1;
    public ServerConfigViewer viewer;

    public void SetItem (int index)
    {
        this.index = index;
        string desc = string.Format("{0} / {1}", index, ServerConfigManager.GetConfig(index, "title").ToString());

        //PublisherApiManager.Instance.SetURL(ServerConfigManager.GetConfig(index, "publisher_url").ToString(), (int)ServerConfigManager.GetConfig(index, "publisher_port"));

        descText.text = desc;
    }

    public void SetItem(int index, ServerConfigViewer viewer)
    {
        this.index = index;
        string desc = string.Format("{0} / {1}", index, ServerConfigManager.GetConfig(index, "title").ToString());

        //PublisherApiManager.Instance.SetURL(ServerConfigManager.GetConfig(index, "publisher_url").ToString(), (int)ServerConfigManager.GetConfig(index, "publisher_port"));

        descText.text = desc;
        this.viewer = viewer;
    }

    public void OnClickConnectButton()
    {
        //string url;
        //int port;
        //JsonDataParser.Parse(ServerConfigManager.GetConfig(index, "gameServer_url"), out url);
        //JsonDataParser.Parse(ServerConfigManager.GetConfig(index, "gameServer_port"), out port);
        //WebSocketManager.Init(string.Format("http://{0}:{1}/", url, port));
        DevOptionsManager.ChangeServer(index);
        CustomSceneManager.LoadLoginScene();
    }
    
    public void OnClickStartServerSelectButton()
    { 
        DevOptionsManager.ChangeServer(index);
        GamePreprocessing.instance.StartPup();
        transform.parent.parent.gameObject.SetActive(false);
    }

    public void OnClickServerConfigItem()
    {
        if (viewer != null)
        {
            viewer.ChangeServerConfigItem(index);
        }
    }
}
