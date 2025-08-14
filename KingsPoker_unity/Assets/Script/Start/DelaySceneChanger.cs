using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DelaySceneChanger : MonoBehaviour
{
    public float delayTime = 1f;
    public string nextScene = "";
    // Start is called before the first frame update
    void Start()
    {
        Invoke("NextScene", delayTime);
    }
    public void NextScene()
    {

        CustomSceneManager.LoadScene(nextScene);
    }
}
