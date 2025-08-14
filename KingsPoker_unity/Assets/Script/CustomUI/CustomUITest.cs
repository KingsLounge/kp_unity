using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;

public class CustomUITest : MonoBehaviour
{
    public string scene;
    public CustomUIRoot root;
    public CustomUIOpener opener;

    public void Start()
    {
        opener.ShowUI(scene);
    }

    public void Update()
    {
        if(Input.GetKeyDown(KeyCode.F1))
        {
            Debug.Log(root.GetJSON());
        }
        if (Input.GetKeyDown(KeyCode.F2))
        {
            opener.ShowUI(scene);
        }
        if(Input.GetKeyDown(KeyCode.F3))
        {
            root.Clear();
        }
    }
}
