using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingRotation : MonoBehaviour
{
    // Start is called before the first frame update
    public float rotationSpeed = 100f;
    public Transform rotationTarget;

    // Update is called once per frame
    void Update()
    {
        rotationTarget.Rotate(0f,0f,- rotationSpeed*Time.deltaTime);
    }
}
