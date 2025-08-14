using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class BaccaratInfoItem : MonoBehaviour
{
    public GameObject spriteFontContainer = null;
    public SpriteFontData spriteFont = null;
    public Text hallText = null;
    public int hall = 0;
    public SpriteFontParent spriteParent;

    public virtual void Setting(int hall, int mn, int mx)
    {
        this.hall = hall;
        string spriteFontText = MoneyToString.Converting(mn) + '/' + MoneyToString.Converting(mx);
        List<Sprite> sprites = spriteFont.ConvertSpriteFont(spriteFontText);

        
        //spriteFontContainer.transform.DestroyChildren();

        hallText.text = MoneyToString.Converting(hall);
        spriteParent.SetSpriteFont(spriteFontText);
        
        
    }

    public void OnClickEnterButton()
    {
    }
   
}
