using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

public class ShopManager : MonoBehaviour
{
    public ItemControllerServerCommunication shopSlotItemController;
    public ItemControllerServerCommunication vipSlotItemController;
    public SpriteAtlas shopAtlas;
    public SpriteAtlas vipAtals;
    public List<Sprite> tagImages;
    public Animator ani;
    public GameObject limitOverPanel;
    public Text limitOverText;

    // Start is called before the first frame update
    void Awake()
    {
        if (shopSlotItemController != null)
        {
            shopSlotItemController.updateItemCallback = (GameObject self, JToken data) =>
            {
                ShopItemSlot slot = self.GetComponent<ShopItemSlot>();
                if (slot != null)
                {
                    slot.SetSlot(data as JObject, shopAtlas, this);
                }
                else
                {
                    Console.Error(self.name + "에 ShopItemSlot컴포넌트가 존재하지 않습니다.");
                }
            };
        }
        if (vipSlotItemController != null)
        {
            vipSlotItemController.updateItemCallback = (GameObject self, JToken data) =>
            {
                ShopItemSlot slot = self.GetComponent<ShopItemSlot>();
                if (slot != null)
                {
                    slot.SetSlot(data as JObject, shopAtlas, this);
                }
                else
                {
                    Console.Error(self.name + "에 ShopItemSlot컴포넌트가 존재하지 않습니다.");
                }
            };
        }

        Init();
    }

    public void Init()
    {
        List<JObject> list;
#if ONE_STORE && UNITY_ANDROID
        list = OnestoreIAPManager.Instance.ShopItemList;
#else
        list = IAPManager.Instance.ShopItemList;
#endif
        JArray shopList = new JArray();
        JArray vipShopList = new JArray();
        for (int i = 0; i < list.Count; i++)
        {
            int shop = list[i]["shop"].ToObject<int>();
            if (shop == 2 || shop == 3)
            {
                shopList.Add(list[i]);
            }
            else if (shop == 5)
            {
                vipShopList.Add(list[i]);
            }
        }
        if (shopSlotItemController != null)
        {
            shopSlotItemController.dataArray = shopList;
            shopSlotItemController.Refresh();
            shopSlotItemController.SetScrollOnTop();
        }
        if (vipSlotItemController != null)
        {
            vipSlotItemController.dataArray = vipShopList;
            vipSlotItemController.Refresh();
            vipSlotItemController.SetScrollOnTop();
        }
    }

    public void OnClickPurchasingButton(string key)
    {
#if ONE_STORE && UNITY_ANDROID
        OnestoreIAPManager.Instance.BuyWithID(key);
#else
        IAPManager.Instance.BuyWithID(key);
#endif
    }

    public void LmitOverPrice(long total, long price)
    {
        ErrorMessageManager.Instance.AddGameErrorBodyNotLocal(
            500,
            "SYS_ERR_OVER_LIMIT_PURCHASE_MONTHLY",
            string.Format(
                LocalizeManager.GetLocalString("SYS_CANT_PURCHASE_COZ_LIMIT_MONTHLY"),
                total,
                price
            ),
            ErrorHandlingType.NONE
        );
        //limitOverText.text = string.Format("SYS_CANT_PURCHASE_COZ_LIMIT_MONTHLY",total, price);
        //limitOverPanel.SetActive(true);
    }

    public void OnClickClose()
    {
        CloseWindow();
        //ani.Play("ShopCloseAni");
    }

    public void CloseWindow()
    {
        gameObject.SetActive(false);
    }
}
