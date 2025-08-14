using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestPrefab : MonoBehaviour
{

    public GameObject go;

    private void Awake()
    {
        Instantiate(go);
    }


}
