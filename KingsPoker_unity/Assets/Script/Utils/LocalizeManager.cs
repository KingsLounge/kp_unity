using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.U2D;
using System.IO;

public class LocalizeManager:MonoBehaviour
{
    private static UnityEvent changeLanguage = new UnityEvent();
    //private static List<LocalText> localTextList = new List<LocalText>();
    //private static List<LocalImage> localImageList = new List<LocalImage>();
    private static string languageKey = "";
    private static SpriteAtlas LocalAtlas;
    [SerializeField]
    private SpriteAtlas localAtlas;

    private static List<string> unknownKeys = new List<string>();
    private void Awake()
    {
        LocalAtlas = localAtlas;
        Init();
    }

    public static string LanguageKey
    {
        get { return languageKey; }
        set {
            languageKey = value; 
        }
    }

    public static void AddEvent(UnityAction action)
    {
        changeLanguage.AddListener(action);
    }

    public static void RemoveEvent(UnityAction action)
    {
        changeLanguage.RemoveListener(action);
    }

    public static void Init()
    {
        languageKey = PlayerPrefs.GetString("languageKey", SystemLanguage.Korean.ToString());
        SetLocal(LanguageKey);
        changeLanguage.Invoke();
    }

    public static Sprite GetLocalSprite(string localKey)
    {
        Sprite sprite = null;
        string key = "";
        if (!string.IsNullOrEmpty(localKey))
        {
            if (TableDataManager.lanKeyList != null && TableDataManager.lanKeyList.Contains(languageKey))
            {
                try
                {
                    key = TableDataManager.localTable[localKey][languageKey].ToObject<string>();
                }
                catch
                {
                    Debug.LogError(string.Format("Is not Contain Table <color:blue>{0}</color> in <color:blue>{1}</color>", localKey, languageKey));
                }
               
            }
            else
            {
                Debug.LogError(string.Format("is not contains key {0}",  languageKey));
            }
        }
        else
        {
            Debug.LogError(string.Format("is not enabled local key"));
        }

        if (LocalAtlas!= null)
        {
            sprite = LocalAtlas.GetSprite(key);
        }
        else
        {
            Console.Log("Local Atlas Is null");
        }
        return sprite;
    }


    public  static void RiteUnknownKeys(string key)
    {
#if UNITY_EDITOR
        string pathKey = Path.Combine(Application.persistentDataPath, "UnknownLocal.txt");
        if (!unknownKeys.Contains(key))
        {
            unknownKeys.Add(key);
            File.WriteAllLines(pathKey, unknownKeys.ToArray());
        }
#endif
    }
    public static void SetLocal(string lanKey)
    {
        languageKey = lanKey;
        PlayerPrefs.SetString("languageKey", lanKey);
        PlayerPrefs.Save();
        changeLanguage.Invoke();
    }

    public static string GetLocalString(string localKey)
    {
        string str = "";
        if (!string.IsNullOrEmpty(localKey))
        {
            if (TableDataManager.lanKeyList!= null && TableDataManager.lanKeyList.Contains(languageKey))
            {
                try
                {
                    str = TableDataManager.localTable[localKey][languageKey].ToObject<string>();
                }
                catch
                {
                    Debug.LogWarning(string.Format("Is not Contain Table <color=blue>{0}</color> in <color=blue>{1}</color>", localKey, languageKey));
                    RiteUnknownKeys(localKey);
                }
            }
            else
            {
                Debug.LogWarning(string.Format("is not contains key {0}",  languageKey));
            }
        }
        else
        {
            Debug.LogWarning(string.Format(localKey + " is not enabled local key"));
        }
        if(str!=null)
        {
            str = str.Replace("\\n", System.Environment.NewLine);
        }
        // Console.Log(string.Format("<color=green>LOCAL STRING</color> : {0} in {1} to {2}", languageKey, localKey, str));
        if(string.IsNullOrEmpty(str))
        {
            str = localKey;
        }
        return str;
    }

    
}
