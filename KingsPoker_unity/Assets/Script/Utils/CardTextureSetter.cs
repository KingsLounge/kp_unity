using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardTextureSetter : MonoBehaviour
{
    public int cardSetIndex = 0;
    public Material front;
    public Material back;
    private static CardTextureSetter instance;
    public static CardTextureSetter Instance
    {
        get
        {
            return instance;
        }
    }

    public void Awake()
    {
        CardTextureSetter.instance = this;
    }

    public void OnDestroy()
    {
        CardTextureSetter.instance = null;
    }
}
