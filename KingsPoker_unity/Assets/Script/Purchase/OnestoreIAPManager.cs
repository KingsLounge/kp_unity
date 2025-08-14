#if UNITY_ANDROID

using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OneStore.Auth;
using OneStore.Purchasing;

public class OnestoreIAPManager : IPurchaseCallback
{
    private bool isInitialized = false;
    private bool isInitializing = false;
    private List<JObject> shopItemList;
    public List<JObject> ShopItemList
    {
        get { return shopItemList; }
    }
    private Dictionary<int, JObject> shopItemDic;
    private static OnestoreIAPManager instance = null;
    public static OnestoreIAPManager Instance
    {
        get
        {
            Init();
            return instance;
        }
    }
    private static PurchaseClientImpl purchaseClient;

    public void GetShopItemList()
    {
        shopItemList = new List<JObject>();
        PublisherApiManager.Instance.GetShopListPubAPI(GetShopListPubAPICallBack);
    }

    public static void Init()
    {
        if (instance == null)
        {
            instance = new OnestoreIAPManager();
#if !UNITY_EDITOR
            purchaseClient = new PurchaseClientImpl(Constant.GameConfig.onestoreAppKey);
            purchaseClient.Initialize(instance);
#endif
            instance.GetShopItemList();
        }
    }

    public void GetShopListPubAPICallBack(long statusCode, JObject res)
    {
        Console.Log("GetShopListPubApiCallBack!!");
        switch (statusCode)
        {
            case 200:
                shopItemList.Clear();
                var list = res["shopItems"] as JArray;
                for (int i = 0; i < list.Count; i++)
                {
                    var ob = list[i] as JObject;
                    //                    Debug.Log(ob.ToString());
                    //Console.SpecialLog(string.Format("product Id : {0}", ob["store_code"].ToObject<string>()));
                    if (ob["store_code"].ToString() == "0")
                    {
                        Console.Log(string.Format("{0}는 스토어에 등록되지 않은 상품입니다.", ob["name"]));
                    }
                    else
                    {
                        if ((int)ob["store_type"] == 4)
                            shopItemList.Add(ob);
                    }
                }
                SetDic();
                AddProduct();
                QueryPurchases();
                break;
            case 400:
                break;
            case 402:
                break;
            case 409:
                break;
        }
    }

    private void QueryPurchases()
    {
        if (purchaseClient != null)
            purchaseClient.QueryPurchases(ProductType.INAPP);
    }

    public string GetPrice(string productID)
    {
        if (productDetails != null)
        {
            var detail = productDetails.Find(
                (item) =>
                {
                    return item.productId == productID;
                }
            );
            if (detail != null)
            {
                return detail.price;
            }
        }
        return null;
    }

    public void SetDic()
    {
        shopItemDic = new Dictionary<int, JObject>();
        for (int i = 0; i < shopItemList.Count; i++)
        {
            var go = shopItemList[i];
            if (!shopItemDic.ContainsKey((int)go["ref_idx"]))
            {
                shopItemDic.Add((int)go["ref_idx"], go);
                //Console.Log(go);
            }
            else
            {
                Console.Error(string.Format("{0}는 이미 사용중인 아이템 키 입니다.", (int)go["idx"]));
            }
        }
    }

    public void BuyWithID(string productID)
    {
        try
        {
            var purchaseFlowParams = new PurchaseFlowParams.Builder()
                .SetProductId(productID) // mandatory
                .SetProductType(ProductType.INAPP) // mandatory
                .Build();
            if (purchaseClient != null)
                purchaseClient.Purchase(purchaseFlowParams);
        }
        catch (Exception e)
        {
            Console.Error("BuyProductID: FAIL. Exception during purchase. " + e);
        }
    }

    public void AddProduct()
    {
        var items = shopItemList.Select(store => store["store_code"].ToString()).ToList();
        if (purchaseClient != null)
            purchaseClient.QueryProductDetails(items.AsReadOnly(), ProductType.INAPP);
    }

    public void OnAcknowledgeFailed(IapResult iapResult)
    {
        throw new System.NotImplementedException();
    }

    public void OnAcknowledgeSucceeded(PurchaseData purchase, ProductType type)
    {
        throw new System.NotImplementedException();
    }

    public void OnConsumeFailed(IapResult iapResult)
    {
        Console.Error($"On Consume Failed : {iapResult}");
    }

    public void OnConsumeSucceeded(PurchaseData purchase)
    {
        Console.Log($"On Consume Successed : {purchase.JsonReceipt}");
    }

    public void OnManageRecurringProduct(
        IapResult iapResult,
        PurchaseData purchase,
        RecurringAction action
    )
    {
        throw new System.NotImplementedException();
    }

    public void OnNeedLogin()
    {
        new OneStoreAuthClientImpl().LaunchSignInFlow(
            (signInResult) =>
            {
                if (signInResult.IsSuccessful())
                {
                    Init();
                }
            }
        );
    }

    public void OnNeedUpdate()
    {
        if (purchaseClient != null)
            purchaseClient.LaunchUpdateOrInstallFlow(
                (IapResult result) =>
                {
                    if (result.Code == (int)OneStore.Auth.ResponseCode.RESULT_OK)
                    {
                        Init();
                    }
                }
            );
    }

    public void PurchaseWithServer(PurchaseData args)
    {
        PublisherApiManager.Instance.PurchasingVerificationAPI(
            args,
            (Action<long, JObject>)(
                async (long statusCode, JObject res) =>
                {
                    switch (statusCode)
                    {
                        case 200: //success
                            Console.Log("Purchasing Success : " + res.ToString());
                            if (purchaseClient != null)
                                purchaseClient.ConsumePurchase(args);
                            var update = (JObject)res["update"];
                            if (update["silver"] != null)
                            {
                                var chip = (long)update["silver"];
                                await PublisherApiManager.Instance.PointInAPIAsync("silver", chip);
                            }
                            if (update["gold"] != null)
                            {
                                var chip = (long)update["gold"];
                                await PublisherApiManager.Instance.PointInAPIAsync("gold", chip);
                            }
                            NormalMessage.instance.OnOneButtonMessagePopUp("purchase_complete");
                            SendUserInfoCheck();
                            LoadingCircle.Instance.LoadingComplete("BuyWidthID");
                            InfoManager.Instance.GetMyItems();
                            break;
                        case 201: //Success
                            if (purchaseClient != null)
                                purchaseClient.ConsumePurchase(args);
                            PurchaseWithServer(args);
                            break;
                        case 400: //BadRequest
                            Console.Log("BadRequest");
                            LoadingCircle.Instance.LoadingComplete("BuyWidthID");
                            break;
                        case 402: //BadRequest
                            Console.Log("AbnormalReceipt");
                            LoadingCircle.Instance.LoadingComplete("BuyWidthID");
                            break;
                        case 409: //BadRequest
                            if (purchaseClient != null)
                                purchaseClient.ConsumePurchase(args);
                            Console.Log("UsedReceipt");
                            LoadingCircle.Instance.LoadingComplete("BuyWidthID");
                            break;
                    }
                }
            )
        );
    }

    private void SendUserInfoCheck()
    {
        var packet = new Packet((int)CPProtocol.CP_CONSOLE_USERINFO);
        WebSocketManager.defaultCli.Send(packet.ToJson());
    }

    public void OnProductDetailsFailed(IapResult iapResult)
    {
        Console.Log(
            String.Format(
                "[{0}] OnProductDetails failed, \nMessage : {1}",
                iapResult.Code,
                iapResult.Message
            )
        );
        throw new System.NotImplementedException();
    }

    private List<ProductDetail> productDetails = null;

    public void OnProductDetailsSucceeded(List<ProductDetail> productDetails)
    {
        this.productDetails = productDetails;
    }

    public void OnPurchaseFailed(IapResult iapResult)
    {
        Console.Log(
            String.Format(
                "[{0}] Purchasing failed, \nMessage : {1}",
                iapResult.Code,
                iapResult.Message
            )
        );
        throw new System.NotImplementedException();
    }

    public void OnPurchaseSucceeded(List<PurchaseData> purchases)
    {
        foreach (var data in purchases)
        {
            Console.Log(
                String.Format(
                    "[{0}] Purchasing Successed, receipt : {1}",
                    data.ProductId,
                    data.JsonReceipt
                )
            );
            PurchaseWithServer(data);
        }
    }

    public void OnSetupFailed(IapResult iapResult)
    {
        throw new System.NotImplementedException();
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        QueryPurchases();
    }
}
#endif
