using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoomInfoItemKH : RoomInfoItem
{
    public Material effectMaterial;
    public Image effectImage;
    public GameObject disableImage;
    public float effectBetweenOfset = 0.32f;
    
    public List<ColorSet> colorSetList;
    //public override void Setting(long bng, long emn, long sb, long bb, GAME_TYPE gameType)
    //{
       
    //    this.gameType = gameType;
    //    this.bng = bng;
    //    this.emn = emn;
    //    string spriteFontText = MoneyToString.Converting(emn);
    //    var tr = spriteFontContainer.transform;

    //    //Console.Error(string.Format("{0}   {1}   {2}", bng, emn, gameType));
    //    //spriteFontContainer.transform.DestroyChildren();
    //    UpdateCanJoin();

    //    buyinText.text = MoneyToString.Converting(sb) + '/' + MoneyToString.Converting(bb);

    //    spriteParent.SetSpriteFont(spriteFontText);
    //    SetEffectImage();
    //}

    

    public override void SetEnable(bool en)
    {
        base.SetEnable(en);
        effectImage.enabled = en;
        disableImage.SetActive(!en);
    }

    public void SetEffectImage()
    {
        var parent = transform.parent;
        for(int i = 0; i <parent.childCount; i++)
        {
            if(transform == parent.GetChild(i))
            {
                var material = new Material(effectMaterial);
                effectImage.material = material;
                material.SetFloat("_InitUOffset2", effectBetweenOfset * i);
                Color co = material.GetColor("_TintColor");
                for (int j = 0; j < colorSetList.Count; j++)
                {
                    if (colorSetList[j].emn < roomData.emn) 
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
