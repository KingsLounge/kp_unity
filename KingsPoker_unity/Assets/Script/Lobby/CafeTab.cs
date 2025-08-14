using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CafeTab : MonoBehaviour
{
    [SerializeField]private List<Transform> tabs;
    public List<Transform>Tabs
    {
        get { return new List<Transform>(tabs); }
    }
}
