using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

public class GenerateImageGameObject : Editor
{
    [MenuItem("Assets/Create/Image")]
    public static void Create()
    {
        Debug.Log(Selection.activeObject.GetType());
        GameObject go = new GameObject(Selection.activeObject.name);
        Image img = go.AddComponent<Image>();
        Texture2D tex = Selection.activeObject as Texture2D;
        img.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        img.sprite.name = Selection.activeObject.name;
        img.rectTransform.sizeDelta = new Vector2(tex.width, tex.height);
        Selection.SetActiveObjectWithContext(go,null);
    }
}
