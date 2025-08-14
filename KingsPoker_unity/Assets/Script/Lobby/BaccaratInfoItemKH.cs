using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BaccaratInfoItemKH : BaccaratInfoItem
{
    public Material effectMaterial;
    public Image effectImage;
    public float effectBetweenOfset = 0.32f;
    public List<ColorSet> colorSetList;
    private long max;
    public override void Setting(int hall, int mn, int mx)
    {
        //base.Setting(hall, mn, mx);

        this.hall = hall;
        max = mx;
        string spriteFontText = MoneyToString.Converting(hall);
        List<Sprite> sprites = spriteFont.ConvertSpriteFont(spriteFontText);

        //spriteFontContainer.transform.DestroyChildren();

        hallText.text = MoneyToString.Converting(mn) + '/' + MoneyToString.Converting(mx);
        spriteParent.SetSpriteFont(spriteFontText);
        SetEffectImage();
    }
    public void SetEffectImage()
    {
        var parent = transform.parent;
        for (int i = 0; i < parent.childCount; i++)
        {
            if (transform == parent.GetChild(i))
            {
                var material = new Material(effectMaterial);
                effectImage.material = material;
                material.SetFloat("_InitUOffset2", effectBetweenOfset * i);
                Color co = material.GetColor("_TintColor");
                for (int j = 0; j < colorSetList.Count; j++)
                {
                    if (colorSetList[j].emn < max)
                    {
                        co = colorSetList[j].color;
                    }
                    else
                    {
                        break;
                    }
                }
                material.SetColor("_TintColor", co);
                break;
            }
        }//_TintColor

    }
}
