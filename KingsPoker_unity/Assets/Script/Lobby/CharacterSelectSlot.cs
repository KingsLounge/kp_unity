using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;
using System;
using UnityEngine.UI;
using UnityEngine.U2D;

public class CharacterSelectSlot : MonoBehaviour
{
    private int idx;
    public int Idx
    {
        get{return idx;}
    }
    private JObject charData;
    [SerializeField]
    private SpriteAtlas charImageAtals;
    [SerializeField]
    private Image charImage;
    [SerializeField]
    private GameObject maskObj;
    [SerializeField]
    private Image EquipImage;
    [SerializeField]
    private Button EquipButton;
    
    public CharacterTab characterTab; 

    [SerializeField]
    private Text expireTimeText;
    [SerializeField]
    private GameObject buyButton;
    private bool hasItem;
    private bool usedItem;
    public void SetSlot(int refidx, bool hasItem, bool equipd, bool used,string time = null)
    {   
        idx = refidx;
        this.usedItem = used;
        JObject item = InfoManager.itemData[refidx];
        string name = item["name"].ToObject<string>();
        
        foreach(var data in TableDataManager.characterTable)
        {
            string chName = data.Value["name"].ToObject<string>();
            if(name == chName)
            {
                charData = data.Value;
                break;
            }
        }
        string spriteName = charData["2d_index"].ToObject<string>();
        if(charImageAtals)
        {
            charImage.sprite = charImageAtals.GetSprite(spriteName);

        }
        EquipButton.interactable = used;
        buyButton.SetActive(!used);

        ExpireTimeTextSet(time);

        Equip(equipd);

        maskObj.SetActive(true);
    }

    public void OnClickEquipButton()
    {
        if(!EquipImage.enabled)
        {
            characterTab.Equip(idx);
        }
        
    }

    public void Equip(bool equip)
    {
        EquipImage.enabled = equip;
        if(equip && !MyStatus.usePhotoURL)
        {
            LobbyManager.Instance.SetProfile(charImage.sprite);
        }
    }

    public void ExpireTimeTextSet(string expTime)
    {
        if(usedItem)
        {
            expireTimeText.enabled = true;
            TimeSpan span = DateTimeParser.Parse(expTime) - DateTime.UtcNow;
            expireTimeText.text = TimeSpanToText(span);
        }
        else
        {
            expireTimeText.enabled = false;
        }
    }

    public string TimeSpanToText(TimeSpan span)
    {
        string timeString;
        Console.SpecialLog(span.ToString());
        if((long)span.TotalDays > 18250)
        {
            timeString = "무기한";
        }
        else if((long)span.TotalDays > 0)
        {
            timeString = string.Format("{0}일 {1}시간", span.Days, span.Hours);
        }
        else if((long)span.TotalHours > 0)
        {
            timeString = string.Format("{0}시간", (int)span.TotalHours);
        }
        else if((long)span.TotalMinutes > 0)
        {
            timeString = "만료 임박";
        }
        else
        {
            timeString = "만료";
        }

        return timeString;
    }
    public void OnClickBuyButton()
    {
        LobbyManager.Instance.VipPanel.SetActive(true);
    }
   
}
