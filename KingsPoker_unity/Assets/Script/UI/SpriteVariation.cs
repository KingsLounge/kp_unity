using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable()]
public class SpriteVariationInfo
{
    public string key = "";
    public Sprite sprite = null;
    public Color color = Color.white;
}
public class SpriteVariation : Variation
{
    public List<SpriteVariationInfo> variation = new List<SpriteVariationInfo>();
    private Sprite defaultSprite = null;
    private Color defaultColor = Color.white;
    private Image image;
    private bool init = false;
    protected override void Init()
    {
        init = true;
        image = GetComponent<Image>();
        defaultSprite = image.sprite;
        defaultColor = image.color;
        if (image == null)
        {
            Console.Error(gameObject.name + " has not Image Component!");
        }
    }
    public void Awake()
    {
        if (!init)
        {
            Init();
        }
    }
    public override void SetVariation(string key)
    {
        if (!init)
        {
            Init();
        }
        SpriteVariationInfo info = variation.Find(value => value.key == key);
        if (info != null)
        {
            SetSprite(info.sprite,info.color);
        }
        else
        {
            SetSprite(defaultSprite,defaultColor);
        }
    }

    private void SetSprite(Sprite sprite, Color color)
    {
        image.sprite = sprite;
        image.color = color;
    }
}
