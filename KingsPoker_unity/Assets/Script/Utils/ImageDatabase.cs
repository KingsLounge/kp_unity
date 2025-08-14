using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System;
using Cysharp.Threading.Tasks;

public class ImageDatabase
{
    private static Dictionary<string, Texture2D> imageCache = new Dictionary<string, Texture2D>();
    public static bool CheckImageFile(string path, string fileName)
    {
        return System.IO.File.Exists(string.Format("{0}/{1}", path, fileName));
    }

    private static Texture2D LoadTextureFromFile(string path, string fileName)
    {
        var full_path = string.Format("{0}/{1}", path, fileName);
        byte[] _bytes = System.IO.File.ReadAllBytes(full_path);
        var texture = new Texture2D(2, 2);
        texture.LoadImage(_bytes);
        Console.Log(string.Format("file Load Success : {0}", full_path));
        return texture;
    }

    public static async UniTask<Texture2D> LoadImageTexture(string url ,string path, string filename)
    {
        var strings = url.Split('/');
        var downloadFileName = strings[strings.Length - 1];
        if(imageCache.ContainsKey($"{filename}_{url}"))
        {
            return imageCache[$"{filename}_{url}"];
        }
        Texture2D texture;
        if (CheckImageFile(path, $"{downloadFileName}_{filename}"))
        {
            texture = LoadTextureFromFile(path, $"{downloadFileName}_{filename}");
        }
        else
        {
            texture = await CoLoadImageTexture(url, path, $"{downloadFileName}_{filename}");
        }
        imageCache.Set($"{filename}_{url}", texture);
        return texture;
    }


    private static void SaveTextureToFile(Texture2D texture,string dirPath, string fileName)
    {
        if (!System.IO.Directory.Exists(dirPath))
        {
            System.IO.Directory.CreateDirectory(dirPath);
        }
        var path = string.Format("{0}/{1}", dirPath, fileName);
        byte[] _bytes = texture.EncodeToPNG();
        System.IO.File.WriteAllBytes(path, _bytes);
        Debug.Log(_bytes.Length / 1024 + "Kb was saved as: " + path);
    }

    private static async UniTask<Texture2D> CoLoadImageTexture(string url, string path, string fileName)
    {
        if (string.IsNullOrEmpty(url)) return null;
        UnityWebRequest www = UnityWebRequestTexture.GetTexture(url);

        await www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.Log(www.error);
            return null;
        }
        else
        {
            var texture = DownloadHandlerTexture.GetContent(www);

            SaveTextureToFile(texture, path, fileName);
            return texture;
        }
    }
}
