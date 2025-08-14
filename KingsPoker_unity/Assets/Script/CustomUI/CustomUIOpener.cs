using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;

public class CustomUIOpener : MonoBehaviour
{
    public TextAsset json;
    public CustomUIRoot root;
    private JObject obj;
    protected bool init = false;
    private string curScene = "";

    protected virtual void Awake()
    {
        Init();
    }

    protected virtual void SetEvent()
    {

    }

    public void Init()
    {
        if (init) return;
        
        obj = JObject.Parse(json.text);
        SetEvent();
        init = true;
    }
    public virtual void ShowUI(string scene)
    {
        Debug.Log(scene);

        if (!init) Init();
        if(!obj.ContainsKey(scene))
        {
            Debug.Log(scene + " scene is not found");
            return;
        }
        root.gameObject.SetActive(true);
        root.Setting(obj[scene] as JArray);
        curScene = scene;
    }

    public string GetCurrentScene()
    {
        return curScene;
    }
    
    public void CloseUI()
    {
        root.Clear();
        root.gameObject.SetActive(false);
    }
}
