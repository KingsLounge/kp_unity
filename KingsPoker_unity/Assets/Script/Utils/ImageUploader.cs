using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using ImageAndVideoPicker;

public class ImageUploader : MonoBehaviour
{
    public delegate void Complete(bool success, Texture2D texture);
    private Vector2 size = Vector2.zero;
    private Complete callback;
    private static ImageUploader instance = null;
    public static ImageUploader Instance
    {
        get {
            if (instance != null)
                return instance;
            else
            {
                GameObject go = new GameObject();
                go.name = "ImageUploader";
                DontDestroyOnLoad(go);
                instance = go.AddComponent<ImageUploader>();
                return instance;
            }    
        }
    }

    void OnEnable()
    {
        PickerEventListener.onImageSelect += OnImageSelect;
        PickerEventListener.onImageLoad += OnImageLoad;
        PickerEventListener.onVideoSelect += OnVideoSelect;
        PickerEventListener.onError += OnError;
        PickerEventListener.onCancel += OnCancel;

#if UNITY_ANDROID
        AndroidPicker.CheckPermissions();
#endif
    }

    void OnDisable()
    {
        PickerEventListener.onImageSelect -= OnImageSelect;
        PickerEventListener.onImageLoad -= OnImageLoad;
        PickerEventListener.onVideoSelect -= OnVideoSelect;
        PickerEventListener.onError -= OnError;
        PickerEventListener.onCancel -= OnCancel;
    }

    void OnImageSelect(string imgPath, ImageAndVideoPicker.ImageOrientation imgOrientation)
    {
        Debug.Log("OnImageSelect : Image Location : " + imgPath);
    }


    void OnImageLoad(string imgPath, Texture2D tex, ImageAndVideoPicker.ImageOrientation imgOrientation)
    {
        Debug.Log("OnImageLoad : Image Location : " + imgPath);

        byte[] textureBytes = null;
        Texture2D imageTexture = GetTextureCopy(tex);
        imageTexture = ResizeTexture(imageTexture, size.x.ToInt32(), size.y.ToInt32());

        textureBytes = imageTexture.EncodeToPNG();

        string base64 = Convert.ToBase64String(textureBytes);

        callback?.Invoke(true, imageTexture);

        //StartCoroutine(Upload("http://192.168.0.222:5000/api/pub/upload/image", base64));
    }

    void OnVideoSelect(string vidPath)
    {
        Debug.Log("OnVideoSelect : Video Location : " + vidPath);
#if UNITY_ANDROID || UNITY_IOS
        Handheld.PlayFullScreenMovie("file://" + vidPath, Color.blue, FullScreenMovieControlMode.Full, FullScreenMovieScalingMode.AspectFill);
#endif
    }

    void OnError(string errorMsg)
    {
        Debug.Log("Error : " + errorMsg);
        callback?.Invoke(false, null);
    }

    void OnCancel()
    {
        Debug.Log("Cancel by user");
        callback?.Invoke(false, null);
    }

    public void BrowseImage(bool cropping = false)
    {
        BrowseImage(Vector2.zero,cropping);
    }

    public void BrowseImage(Vector2 size, bool cropping = false)
    {
        BrowseImage(size, null, cropping);
    }

    public void BrowseImage(Vector2 size, Complete callback, bool cropping = false)
    {
        this.callback = callback;
        this.size = size;
#if UNITY_ANDROID
        AndroidPicker.BrowseImage(cropping);
#elif UNITY_IPHONE
        IOSPicker.BrowseImage(cropping); // true for pick and crop
#endif
    }

    Texture2D GetTextureCopy(Texture2D source)
    {
        RenderTexture rt = RenderTexture.GetTemporary(
            source.width,
            source.height,
            0,
            RenderTextureFormat.Default,
            RenderTextureReadWrite.Linear
        );

        Graphics.Blit(source, rt);

        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = rt;

        Texture2D readableTexture = new Texture2D(source.width, source.height);
        readableTexture.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        readableTexture.Apply();

        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(rt);

        return readableTexture;
    }

    public Texture2D ResizeTexture(Texture2D source, int width, int height)
    {
        Texture2D copy = new Texture2D(width, height, source.format, true);
        Color[] rpixels = copy.GetPixels(0);
        float incX = (1.0f / (float)width);
        float incY = (1.0f / (float)height);
        for (int px = 0; px < rpixels.Length; px++)
        {
            rpixels[px] = source.GetPixelBilinear(incX * ((float)px % width), incY * ((float)Mathf.Floor(px / height)));
        }
        copy.SetPixels(rpixels, 0);
        copy.Apply();

        return copy;
    }
}
