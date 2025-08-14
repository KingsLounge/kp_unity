using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardRotationSaver : MonoBehaviour
{
    public Vector3 rotation;


#if UNITY_EDITOR
    public void GetRotation()
    {
        rotation = UnityEditor.TransformUtils.GetInspectorRotation(transform);
    }
#endif
}
