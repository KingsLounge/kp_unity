using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HideInInspectorWithCondition : PropertyAttribute
{
    public string boolProperty
    {
        get;private set;
    }
    public bool reverse
    {
        get;private set;
    }
    public HideInInspectorWithCondition(string boolProperty,bool reverse = false)
    {
        this.boolProperty = boolProperty;
        this.reverse = reverse;
    }
}
