using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
[CustomEditor (typeof (CustomUIRoot))]
public class CustomUIRootEditor : Editor
{
    public override void OnInspectorGUI()   //OnInspectorGUI 에 오버라이드 해 줍니다.
    {
        base.OnInspectorGUI();
        CustomUIRoot customUIRoot = target as CustomUIRoot;
        if (GUILayout.Button("Log JSON"))
        {
            if (customUIRoot)
            {
                Debug.Log(customUIRoot.GetJSON());
            }
        }
    }
}
