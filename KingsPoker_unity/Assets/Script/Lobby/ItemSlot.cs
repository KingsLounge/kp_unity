using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;
using System;
using System.Globalization;

public class ItemSlot : MonoBehaviour
{

    [Header("UI")]
    [SerializeField]
    private Image usingEffect;
    [SerializeField]
    private Image typeIconImage;
    [SerializeField]
    private Text itemNameText;
    [SerializeField]
    private Text itemCountText;
    [SerializeField]
    private Text timeText;
    [SerializeField]
    private Button useItemButton;
    [SerializeField]
    private Text useItemButtonText;


    [Header("Data")]
    [SerializeField]
    private SpriteAtlas atlas;
    private StorageItemManager itemManager;
    int idx;

    string user_idx;
    int ref_idx;
    string giver_idx;
    string receipt_idx;
    string shop_idx;
    string mail_idx;
    string used;
    bool equipped;
    string count;
    string expiry_date;
    string payload;
    string created_at;
    string updated_at;

    JObject refItem;
    Sprite sp;
    string des_key;
    bool isExist;
    itemType type;

    const int CARD = 300;


    public int Refidx
    {
        get { return ref_idx; }
    }

    public bool Equipped
    {
        get { return equipped; }
        set
        {
            equipped = value;
            SetText();
            SetImage();
        }
    }

    public void Init(JObject data, StorageItemManager itemManager)
    {
        this.itemManager = itemManager;
        SetData(data);
        GetRefItem();
        SetImage();
        SetButton();
        SetText();
        CheckValid();
        // TEMP
    }

    private void SetData(JObject data)
    {
        //  idx, , ref_idx, giver_idx, receipt_idx, shop_idx, mail_idx, used, equipped, count, expiry_date, payload, created_at, updated_at       <-- 이전 
        //  idx, ,          giver_idx,                                  used, equipped, count, expiry_date                                        <-- 2022-03-12
        
        idx = data.ValueOrDefault<int>("idx", 0);
        // user_idx = data.ValueOrDefault("user_idx", "");
        ref_idx = data.ValueOrDefault<int>("ref_idx", 0);
        giver_idx = data.ValueOrDefault("giver_idx", "");
        // receipt_idx = data.ValueOrDefault("receipt_idx", "");
        // shop_idx = data.ValueOrDefault("shop_idx", "");
        // mail_idx = data.ValueOrDefault("mail_idx", "");
        used = data.ValueOrDefault("used", "");
        equipped = data.ValueOrDefault<int>("equipped", 0) != 0;
        count = data.ValueOrDefault("count", "");
        expiry_date = data.ValueOrDefault("expiry_date", "");
        payload = data.ValueOrDefault("payload", "");
        //created_at = data.ValueOrDefault("created_at", "");
        //updated_at = data.ValueOrDefault("updated_at", "");
    }

    private void GetRefItem()
    {
        isExist = InfoManager.itemData.TryGetValue(ref_idx, out refItem);
        if (isExist)
        {
            Debug.Log("ItemData : " + refItem);
            sp = atlas.GetSprite(refItem.ValueOrDefault("img", ""));
            des_key = refItem.ValueOrDefault("description", "");
            type = refItem.ValueOrDefault<itemType>("type", itemType.none);
            //if (type >= itemType.package && type <= itemType.membership)
            //    isExist = false;
        }
    }
    private void SetImage()
    {
        if (isExist)
        {
            if (sp == null)
            {
                Console.Log(string.Format("다음 이미지를 찾을 수 없습니다.: {0}", refItem["img"].ToString()));
            }
            else
            {
                if (typeIconImage != null)
                    typeIconImage.sprite = sp;
            }
            if(usingEffect)
                usingEffect.gameObject.SetActive(Equipped);
        }
    }

    private void SetButton()
    {
        if (isExist)
        {

        }
    }

    private void SetText()
    {
        if (isExist)
        {
            string text = "사용하기";

            if (itemNameText)
                itemNameText.GetComponent<LocalText>().LocalKey = des_key;
            if (timeText)
                timeText.text = CalLastTime();
            if (useItemButtonText)
            {
                
                if (equipped)
                    text = "해제하기";
                useItemButtonText.text = text;
            }


            switch( type)
            {
                case itemType.ticket_tnmt: // 티켓
                    useItemButton.enabled = false;
                    useItemButtonText.text = "-";
                    itemCountText.text = count + " 장";
                    break;

                case itemType.ticket_kickout: // 강퇴권 
                    useItemButton.enabled = false;
                    useItemButtonText.text = "-";
                    itemCountText.text = $"<b><color=yellow>{count}</color></b> 장 ( 게임 중에 사용 가능 )";
                    break;

                case itemType.ticket_nick:  // 닉네임 변경권 
                    useItemButton.enabled = true;
                    useItemButtonText.text = text;
                    itemCountText.text = $"<b><color=yellow>{count}</color></b> 장"; 
                    break;

                default: 
                    useItemButton.enabled = true;
                    itemCountText.text = "";

                    break;
            }

        }
    }

    private void CheckValid()
    {
        bool ok = isExist && !(DateTimeParser.Parse(expiry_date) < DateTime.UtcNow);
        gameObject.SetActive(ok);
    }

    private string CalLastTime()
    {
        //string format = "yyyy-MM-ddTHH:mm:ss.fffZ";
        //TimeSpan span = DateTime.ParseExact(expiry_date, format, CultureInfo.CurrentCulture) - DateTime.UtcNow;
        TimeSpan span = DateTimeParser.Parse(expiry_date) - DateTime.UtcNow;
        return TimeSpanToText(span);
    }

    public string TimeSpanToText(TimeSpan span)
    {
        string timeString;
        // Console.SpecialLog(span.ToString());
        if ((long)span.TotalDays > 18250)
        {
            timeString = "무기한";
        }
        else if ((long)span.TotalDays > 0)
        {
            timeString = string.Format("{0}일 {1}시간", span.Days, span.Hours);
        }
        else if ((long)span.TotalHours > 0)
        {
            timeString = string.Format("{0}시간", (int)span.TotalHours);
        }
        else if ((long)span.TotalMinutes > 0)
        {
            timeString = "만료 임박";
        }
        else
        {
            timeString = "만료";
        }

        return timeString;
    }

    public void Equip(bool equip)
    {
        //EquipImage.enabled = equip;
        Equipped = equip;
        if (equip)
        {
            //CardSets.BackIdx = ref_idx - CARD;
        }
    }

    public void OnClickUseItemButton()
    {
        if (isExist)
        {
            switch (type)
            {
                case itemType.package:
                    itemManager.UseItem(idx);
                    break;


                case itemType.ticket_kickout: // 강퇴권 
                    break;

                case itemType.ticket_nick:  // 닉네임 변경권 
                    if(InfoManager.canChangeNick)
                    {
                        LobbyManager lobbyManager = this.GetComponentInParent<LobbyManager>();
                        if (lobbyManager)
                        {
                            lobbyManager.NickNamePopupButton();
                        }
                    }
                    else
                    {
                        itemManager.UseItem(idx);
                    }
                    break;

                default:
                    itemManager.EquipItem(ref_idx);
                    break;
            }
        }
    }

}
