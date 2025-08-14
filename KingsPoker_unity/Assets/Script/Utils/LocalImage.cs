using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LocalImage : Image
{
    public string localKey;
    
    protected override void Awake()
    {
        base.Awake();
        SetLocalImage();
        LocalizeManager.AddEvent(SetLocalImage);
    }



    protected override void OnDestroy()
    {
        base.OnDestroy();
        LocalizeManager.RemoveEvent(SetLocalImage);
    }


    public void SetLocalImage()
    {
        Sprite sp = LocalizeManager.GetLocalSprite(localKey);
        if (sp == null)
        {
            Debug.LogError(string.Format("Not contain local sprite in SpriteAtlas.", localKey));
        }
        else
        {
            sprite = sp;
        }
        //if (!string.IsNullOrEmpty(localKey))
        //{
        //    if (TableDataManager.lanKeyList.Contains(languageKey))
        //    {
        //        try
        //        {
        //            sprite = LocalizeManager.GetLocalSprite( TableDataManager.localTable[localKey][languageKey].ToObject<string>());
        //        }
        //        catch
        //        {
        //            Debug.LogError(string.Format("Is not Contain Table <color:blue>{0}</color> in <color:blue>{1}</color>", localKey, languageKey));
        //        }
        //        if(sprite == null)
        //        {
        //            Debug.LogError(string.Format("{0} Not contain local sprite in SpriteAtlas.", LocalizeManager.GetLocalSprite(TableDataManager.localTable[localKey][languageKey].ToObject<string>())));
        //            enabled = false;
        //        }
        //    }
        //    else
        //    {
        //        Debug.LogError(string.Format("{0} : is not contains key {1}", gameObject.name, languageKey));
        //    }
        //}
        //else
        //{
        //    Debug.LogError(string.Format("{0} : is not enabled local key", gameObject.name));
        //}
    }
}
