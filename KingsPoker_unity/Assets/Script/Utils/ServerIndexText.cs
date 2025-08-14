using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ServerIndexText : MonoBehaviour
{
    public static Text serverIndexText;
    void Start()
    {
        serverIndexText = GetComponent<Text>();
    }

    
}
