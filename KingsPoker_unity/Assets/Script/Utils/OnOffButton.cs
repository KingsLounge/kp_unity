using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnOffButton : MonoBehaviour
{
    [SerializeField]
    GameObject targetObj;
    public void OnOff()
    {
        targetObj.SetActive(!targetObj.activeSelf);
    }
}
