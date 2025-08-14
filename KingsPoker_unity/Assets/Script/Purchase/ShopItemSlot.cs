using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

public class ShopItemSlot : MonoBehaviour
{
    public Image itemImage;
    public Image tagImage;
    public Text itemNameText;
    public Text priceText;
    public ShopManager shop;

    [HideInInspector]
    public string buyCode;

    [HideInInspector]
    public string allowDate;

    [HideInInspector]
    public string denyDate;

    [HideInInspector]
    public long price;

    public void OnClickBuyButton()
    {
        PublisherApiManager.Instance.GetPayMent(PaymentCallBack);
    }

    private void Awake()
    {
        IAPManager.Instance.initSuccessEvent.AddListener(GetPrice);
    }

    private void OnDestroy()
    {
        IAPManager.Instance.initSuccessEvent.RemoveListener(GetPrice);
    }

    public virtual void SetSlot(JObject data, SpriteAtlas atals, ShopManager shop)
    {
        this.shop = shop;
        var refidx = data["ref_idx"].ToObject<int>();
        var itemData = InfoManager.itemData[refidx];
        //var sp = atals.GetSprite(itemData["img"].ToString()); //이미지는 일단 나중에
        //if (sp == null)
        //{
        //    Console.Error(string.Format("다음 이미지를 찾을 수 없습니다.: {0}", data["img"].ToString()));
        //}
        //else
        //{
        //    itemImage.sprite = sp;
        //}
        var imgString = data.ValueOrDefault("img", "");
        itemNameText.text = LocalizeManager.GetLocalString(data["name"].ToString());
        buyCode = data["store_code"].ToString();

        var sp = atals.GetSprite(imgString);
        itemImage.sprite = sp;
        //itemImage.SetNativeSize();

        GetPrice();

        price = data["price"].ToObject<long>();

        if (tagImage != null)
        {
            var tag = (int)data["tag"];

            if (tag >= shop.tagImages.Count || shop.tagImages[tag] == null || tag < 1)
            {
                tagImage.enabled = false;
            }
            else
            {
                tagImage.enabled = true;
                tagImage.sprite = shop.tagImages[tag];
            }
        }
    }

    public async void GetPrice()
    {
#if ONE_STORE
        priceText.text = OnestoreIAPManager.Instance.GetPrice(buyCode);
#else
        priceText.text = await IAPManager.Instance.GetPrice(buyCode);
#endif
    }

    public void PaymentCallBack(long statusCode, JObject json)
    {
        if (statusCode == 200)
        {
            long total = json["total_price"].ToObject<long>();
            Console.SpecialLog(string.Format("Total Price  : {0}", total));
            if (total + price > Constant.GameConfig.BUY_LIMIT_PRICE)
            {
                shop.LmitOverPrice(total, Constant.GameConfig.BUY_LIMIT_PRICE - total);
            }
            else
            {
#if ONE_STORE
                OnestoreIAPManager.Instance.BuyWithID(buyCode);
#else
                IAPManager.Instance.BuyWithID(buyCode);
#endif
            }
        }
    }
}
