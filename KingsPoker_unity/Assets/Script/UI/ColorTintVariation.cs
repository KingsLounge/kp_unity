using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ColorTintVariation : Variation
{
    public List<ColorVariationInfo> variation = new List<ColorVariationInfo>();
    public Color defaultColor = Color.white;
    private Graphic[] graphics;
    private bool init = false;
    protected override void Init()
    {
        init = true;
        graphics = GetComponentsInChildren<Graphic>(true);
        if (graphics == null)
        {
            Console.Error(gameObject.name + " has not Graphic Component!");
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
        ColorVariationInfo info = variation.Find(value => value.key == key);
        if (info != null)
        {
            SetColorTint(info.color);
        }
        else
        {
            SetColorTint(defaultColor);
        }
    }

    private void SetColorTint(Color color)
    {
        for (int i = 0; i < graphics.Length; ++i) graphics[i].CrossFadeColor(color, 0f, true, true);
    }
}
