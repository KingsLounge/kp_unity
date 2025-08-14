using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CustomUIModuleList", menuName = "Scriptable Object/CustomUIModuleList", order = int.MaxValue)]
public class CustomUIModuleList : ScriptableObject
{
    [SerializeField]
    private List<GameObject> prefabs = new List<GameObject>();

    public GameObject Find(System.Predicate<GameObject> match)
    {
        return prefabs.Find(match);
    }
}