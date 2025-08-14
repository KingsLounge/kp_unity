using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MinigameSlot : MonoBehaviour
{
    public Transform slotTransform;
    
    public Transform spriteFontContainer = null;
    public SpriteFontData spriteFont = null;
    public Text chipText = null;
    public List<SpriteFontPrefab> spriteFontScripts = new List<SpriteFontPrefab>();
    public GameObject winSlot;
    public SpriteFontParent spriteParent;
    
    public void SetSlot(long chip)
    {
        string spriteFontText = string.Format("{0:0,000}", chip);
        chipText.text = spriteFontText;
        List<Sprite> sprites = spriteFont.ConvertSpriteFont(spriteFontText);
        spriteParent.SetSpriteFont(spriteFontText);
        // for(int i = 0; i < sprites.Count; i++) {
        //    SpriteFontPrefab go = null;
        //    if(spriteFontScripts.Count > i) {
        //        go = spriteFontScripts[i];
        //    }
        //    else { 
        //        go = ObjectPoolManager.Instance.spritfontPool.Pop();
        //        spriteFontScripts.Add(go);
        //        go.rect.SetParent(spriteFontContainer.transform);
                
        //    }
            
        //    go.image.sprite = sprites[i];
        //    go.image.SetNativeSize();
        //    go.rect.localScale = new Vector3(1,1,1);
        //}

        //while(spriteFontScripts.Count > sprites.Count)
        //{
        //    var child = spriteFontScripts[spriteFontScripts.Count-1];
        //    spriteFontScripts.RemoveAt(spriteFontScripts.Count-1);
        //    ObjectPoolManager.Instance.spritfontPool.Push(child);
        //}
    }

}
