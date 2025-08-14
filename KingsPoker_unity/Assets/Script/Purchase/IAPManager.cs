using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;
using UnityEngine.Purchasing.Security;
using Newtonsoft.Json.Linq;
using Cysharp.Threading.Tasks;

public class IAPManager : IStoreListener
{
    private IStoreController controller;
    private IExtensionProvider provider;
    private bool isInitialized = false;
    private bool isInitializing = false;
    private List<JObject> shopItemList;
    public List<JObject> ShopItemList{get{return shopItemList;}}
    private Dictionary<int, JObject> shopItemDic;
    
    private static IAPManager instance = null;
    public static IAPManager Instance {
        get {
            if(instance == null) {
                instance = new IAPManager();
            }
            return instance;
        }
    }
    
    public async UniTask<string> GetPrice(string productID)
    {
        if(controller!=null)
        {
            while (string.IsNullOrEmpty(controller.products.WithID(productID).metadata.localizedPriceString))
            {
                await UniTask.WaitForSeconds(0.1f);
            }
            return controller.products.WithID(productID).metadata.localizedPriceString;
        }
        else
        {
            Initialize();
        }
        return null;
    }

    public void Initialize() {
        
        //builder.AddProduct("test_purchasing",ProductType.Consumable,new IDs(){{"test_purchasing",GooglePlay.Name},{"test_purchasing2", AppleAppStore.Name}});
        Console.Log("IAP INITialize start!!");
        if(!isInitialized && !isInitializing)
        {
            isInitializing = true;
            GetShopItemList();
        }
        
        // ConfigurationBuilder builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
        // builder.AddProduct("item_01",ProductType.Consumable,new IDs(){{ "item_01", GooglePlay.Name},{ "item_01", AppleAppStore.Name}});
        // UnityPurchasing.Initialize(this,builder);
    }

    

    public void GetShopItemList()
    {
        shopItemList = new List<JObject>();
        PublisherApiManager.Instance.GetShopListPubAPI(GetShopListPubAPICallBack);
    }

    public void AddProduct()
    {
        ConfigurationBuilder builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
        for(int i = 0; i < shopItemList.Count; i++)
        {
            var item = shopItemList[i];
            
            builder.AddProduct(item["store_code"].ToString(), ProductType.Consumable);
            
        }
        UnityPurchasing.Initialize(this,builder);
        
    }

    public void GetShopListPubAPICallBack(long statusCode, JObject res)
    {
        Console.Log("GetShopListPubApiCallBack!!");
        switch(statusCode)
        {
            case 200:
                shopItemList.Clear();
                var list = res["shopItems"] as JArray;
                for(int i = 0; i < list.Count; i++)
                {
                    var ob = list[i] as JObject;
                    //                    Debug.Log(ob.ToString());
                    //Console.SpecialLog(string.Format("product Id : {0}", ob["store_code"].ToObject<string>()));
                    if(ob["store_code"].ToString()=="0")
                    {
                        Console.Log(string.Format("{0}는 스토어에 등록되지 않은 상품입니다.",ob["name"]));
                    }
                    else
                    {
#if UNITY_ANDROID
                        if((int)ob["store_type"] == 1)
#elif UNITY_IOS
                        if((int)ob["store_type"] == 2)
#endif
                        shopItemList.Add(ob);
                    }

                }
                SetDic();
                AddProduct();
                break;
            case 400:
                break;
            case 402:
                break;
            case 409:
                break;
        }
    }
    public void SetDic()
    {
        shopItemDic = new Dictionary<int, JObject>();
        for(int i = 0; i < shopItemList.Count; i++)
        {
            var go = shopItemList[i];
            if(!shopItemDic.ContainsKey((int)go["ref_idx"]))
            {
                shopItemDic.Add((int)go["ref_idx"],go);
                //Console.Log(go);
            }
            else
            {
                Console.Error(string.Format("{0}는 이미 사용중인 아이템 키 입니다.",(int)go["idx"]));
            }
        }
    }
    public JObject GetDicData(int key)
    {
        if(shopItemDic.ContainsKey(key))
        {
            return shopItemDic[key];
        }
        else
        {
            return null;
        }
        
    }
    public void OnInitializeFailed(InitializationFailureReason reson, string message) {
        Console.Log(String.Format("initialize Failed, reson : {0}",reson));
        isInitializing = false;
    }
    public void OnInitializeFailed(InitializationFailureReason reson)
    {
        Console.Log(String.Format("initialize Failed, reson : {0}", reson));
        isInitializing = false;
    }

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args) {
        Console.Log(String.Format("[{0}] Purchasing Successed, receipt : {1}",args.purchasedProduct.definition.id,args.purchasedProduct.receipt));
        PurchaseWithServer(args);
        // PublisherApiManager.Instance.PurchasingVerificationAPI(args.purchasedProduct,(long statusCode,JObject res)=>{    
        //     PurchaseWithServer(args);
        //     switch(statusCode) {
        //         case 200://success
        //             Console.Log("Purchasing Success");
        //             break;
        //         case 201://Success
        //             controller.ConfirmPendingPurchase(args.purchasedProduct);
                    
        //             break;
        //         case 400://BadRequest
        //             Console.Log("BadRequest");
        //             break;
        //         case 402://BadRequest
        //             Console.Log("AbnormalReceipt");
        //             break;
        //         case 409://BadRequest
        //             Console.Log("UsedReceipt");
        //             break;
        //     }
            
        // });
        return PurchaseProcessingResult.Pending;
    }

    public void PurchaseWithServer(PurchaseEventArgs args)
    {
        PublisherApiManager.Instance.PurchasingVerificationAPI(args.purchasedProduct,(Action<long, JObject>)(async (long statusCode, JObject res)=>{    
            
            switch(statusCode) {
                case 200://success
                    Console.Log("Purchasing Success : "+ res.ToString());
#if UNITY_IOS
                    controller.ConfirmPendingPurchase(args.purchasedProduct);
#endif
                    var update = (JObject)res["update"];
                    if(update["silver"] != null)
                    {
                        var chip = (long)update["silver"];
                        await PublisherApiManager.Instance.PointInAPIAsync("silver", chip);
                    } 
                    if(update["gold"] != null)
                    {
                        var chip = (long)update["gold"];
                        await PublisherApiManager.Instance.PointInAPIAsync("gold", chip);
                    }
                    NormalMessage.instance.OnOneButtonMessagePopUp("purchase_complete");
                    SendUserInfoCheck();
                    LoadingCircle.Instance.LoadingComplete("BuyWidthID");
                    InfoManager.Instance.GetMyItems();
                    break;
                case 201://Success
                    controller.ConfirmPendingPurchase(args.purchasedProduct);
                    PurchaseWithServer(args);
                    break;
                case 400://BadRequest
                    Console.Log("BadRequest");
                    LoadingCircle.Instance.LoadingComplete("BuyWidthID");
                    break;
                case 402://BadRequest
                    Console.Log("AbnormalReceipt");
                    LoadingCircle.Instance.LoadingComplete("BuyWidthID");
                    break;
                case 409://BadRequest
                    controller.ConfirmPendingPurchase(args.purchasedProduct);
                    Console.Log("UsedReceipt");
                    LoadingCircle.Instance.LoadingComplete("BuyWidthID");
                    break;
            }
            
        }));
    }
    
    private void SendUserInfoCheck()
    {
        var packet = new Packet((int)CPProtocol.CP_CONSOLE_USERINFO);
        WebSocketManager.defaultCli.Send(packet.ToJson());
    }


   

    public void OnPurchaseFailed(Product product, PurchaseFailureReason reson) {
        LoadingCircle.Instance.LoadingComplete("BuyWidthID");
        Console.Log(String.Format("[{0}] Purchasing Failed, reson : {1}",product.definition.id,reson));
    }

    public void BuyWithID(string productID) {
        try
        {
            if (isInitialized)
            {
                Product product = controller.products.WithID(productID);
                if (product != null && product.availableToPurchase)
                {
                    LoadingCircle.Instance.LoadingStart("BuyWidthID");
                    
                    Debug.Log (string.Format("Purchasing product asychronously: '{0}'", product.definition.id));// ... buy the product. Expect a response either through ProcessPurchase or OnPurchaseFailed asynchronously.
                    controller.InitiatePurchase(product);
                }
                else
                {
                    Debug.Log(string.Format("BuyProductID ({0}): FAIL. Not purchasing product, either is not found or is not available for purchase", productID));
                }
            }
            else
            {
                Debug.Log("BuyProductID FAIL. Not initialized.");
            }
        }
        catch (Exception e)
        {
            Console.Error ("BuyProductID: FAIL. Exception during purchase. " + e);
        }
    }

    public Event initSuccessEvent = new Event();

    public void OnInitialized(IStoreController controller, IExtensionProvider provider) {
        Console.Log("Initialize Complete");
        this.controller = controller;
        this.provider = provider;
        isInitialized = true;
        isInitializing = false;
        initSuccessEvent?.Invoke();
    }

    
}
