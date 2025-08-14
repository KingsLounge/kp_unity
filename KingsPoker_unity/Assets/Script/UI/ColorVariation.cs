using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
[System.Serializable]
public class ColorVariationInfo
{
    public string key = "";
    public Color color = Color.white;
}
public class ColorVariation : Variation
{
    public List<ColorVariationInfo> variation = new List<ColorVariationInfo>();
    public Color defaultColor = Color.white;
    private MaskableGraphic graphic;
    private bool init = false;
    protected override void Init()
    {
        init = true;
        graphic = GetComponent<MaskableGraphic>();
        if (graphic == null)
        {
            Console.Error(gameObject.name + " has not Graphic Component!");
        }
    }
    public void Awake()
    {
        if(!init)
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
        ColorVariationInfo info = variation.Find(value => value.key == key);
        if(info != null)
        {
            SetColor(info.color);
        }
        else
        {
            SetColor(defaultColor);
        }
    }

    private void SetColor(Color color)
    {
        graphic.color = color;
    }
}
