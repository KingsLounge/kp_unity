using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class ImageWith3D : MonoBehaviour
{
    public Camera cam;
    private RawImage img;
    private RenderTexture render;
    private Texture2D tex;

    public void Start()
    {
        img = GetComponent<RawImage>();
        Debug.Log("Setting");
        render = new RenderTexture(Screen.width, Screen.height, 0);
        render.name = "StreamingRenderTexture";
        render.format = RenderTextureFormat.ARGB32;
        img.texture = tex;
    }

    public void Update()
    {
        if(cam && cam.targetTexture != render)
            cam.targetTexture = render;
    }

    public void OnPostRender()
    {
        
    }
}
