using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Constant;

public class ActiveSetOnOptions : MonoBehaviour
{
    [SerializeField]
    private string optionName;

    private void Awake()
    {
        gameObject.SetActive((bool)typeof(GameConfig).GetField(optionName).GetValue(null));
    }
}
