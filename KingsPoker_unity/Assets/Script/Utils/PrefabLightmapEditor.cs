using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class PrefabLightmapEditor : MonoBehaviour
{

#region Properties and Varaibles
    public PrefabLightmapData pParent;
    public PrefabLightmapData.RendererInfo item;
    List<PrefabLightmapData.RendererInfo> itemsList = new List<PrefabLightmapData.RendererInfo>();
    #endregion

    #region Methods
    void Start()
    {
    }

    void Update()
    {

    }
#if UNITY_EDITOR
    public void addData()
    {
        bool IsActive = false;
        for (int i = 0; i < Selection.objects.Length; ++i)
        {
            if (Selection.objects[i] == this.gameObject)
                IsActive = true;
        }
        if (!IsActive)
            return;

        itemsList.Clear();
        itemsList.AddRange(pParent.getRendererInfo());
        itemsList.Add(item);

        pParent.setRendererInfo(itemsList);
    }

    public void findData()
    {
        itemsList.Clear();
        itemsList.AddRange(pParent.getRendererInfo());

        for (int i = 0; i < itemsList.Count; ++i)
        {
            if (itemsList[i].renderer.gameObject == item.renderer.gameObject)
            {
                item.renderer = itemsList[i].renderer;
                item.lightmapIndex = itemsList[i].lightmapIndex;
                item.lightmapOffsetScale = itemsList[i].lightmapOffsetScale;
                itemsList.Add(item);
                Debug.Log("LightmapData Find is Success");
                return;
            }
        }
    }
#endif
    #endregion
}

#if UNITY_EDITOR
[CustomEditor(typeof(PrefabLightmapEditor))]
public class LightmapDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Add Lightmap Data"))
        {
            GameObject[] targets = Selection.gameObjects;
            for (int i = 0; i < targets.Length; ++i)
            {
                PrefabLightmapEditor target;
                target = targets[i].GetComponent<PrefabLightmapEditor>();
                if (target)
                    target.addData();
            }
        }

        if (GUILayout.Button("Find Lightmap Data"))
        {
            GameObject[] targets = Selection.gameObjects;
            for (int i = 0; i < targets.Length; ++i)
            {
                PrefabLightmapEditor target;
                target = targets[i].GetComponent<PrefabLightmapEditor>();
                if (target)
                    target.findData();
            }
        }
    }
}
#endif
