using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StorageItemManager : MonoBehaviour
{
    [SerializeField]
    private ScrollRect scrollView;
    [SerializeField]
    private Transform itemSlotContainer;
    [SerializeField]
    private ItemSlot itemSlot;

    private List<ItemSlot> itemSlotList = new List<ItemSlot>();
    private int tempEquipIdx;
    [HideInInspector]public int equipIdx;

    private int tempUseIdx;
    private int useIdx;


    //public void OnEnable()
    //{
    //    Init();
    //}
    public virtual void Init()
    {

        InfoManager.AddValueChangeLisener(OnValueChange);
        RequestItems();
    }
    private void OnDestroy()
    {
        InfoManager.RemoveValueChangeLisener(OnValueChange);
    }

    public void OnValueChange(string dataName)
    {
        switch(dataName)
        {
            case "myItems":
                SetInven(InfoManager.MyItems);
                break;
        }
           
    }

    public void RequestItems()
    {
        LoadingCircle.Instance.StartSpin();
        InfoManager.Instance.GetMyItems();
        //PublisherApiManager.Instance.GetInvenListPubAPI(ItemCallback);
    }

    //public void ItemCallback(long statusCode, JObject data)
    //{
    //    LoadingCircle.Instance.StopSpin();
    //    ClearList();
    //    switch (statusCode)
    //    {
    //        case 200:
    //            Console.Log("success Get Mails");
    //            //gameObject.SetActive(true);
    //            SetInven(data);
    //            break;
    //        default:
    //            Console.Error("Faild Get Items");
    //            break;
    //    }
    //}
    private void ClearList()
    {
        foreach (ItemSlot item in itemSlotList)
        {
            Destroy(item.gameObject);
        }
        itemSlotList.Clear();
    }

    public void SetInven(JArray list)
    {
        if (itemSlotList == null)
        {
            itemSlotList = new List<ItemSlot>();
        }

        
        if (list != null)
        {
            var count = 0;
            foreach (JToken token in list)
            {
                JObject itemData = (JObject)token;

                int ref_idx= itemData.ValueOrDefault<int>("ref_idx", 0);
                JObject refItem;
                bool isExist = InfoManager.itemData.TryGetValue(ref_idx, out refItem);
                if (isExist == false) continue;

                itemType type = refItem.ValueOrDefault<itemType>("type", 0);

                //switch( type )
                //{
                //    case itemType.character:
                //    case itemType.membership:
                //    case itemType.ticket_tnmt:
                //        continue;
                //}

                ItemSlot i = null;
                if(count < itemSlotList.Count)
                {
                    i = itemSlotList[count];
                }else
                {
                    i = Instantiate(itemSlot, itemSlotContainer);
                    itemSlotList.Add(i);
                }

                i.Init(token as JObject, this);
                count++;
            }
        }
        for(int i = list.Count; i < itemSlotList.Count; i++)
        {
            itemSlotList[i].gameObject.SetActive(false);
        }
        Debug.Log("성공");
        LoadingCircle.Instance.StopSpin();
    }

    public void EquipItem(int ref_idx)
    {
        tempEquipIdx = ref_idx;
        Debug.Log("장착시도 : " + ref_idx);
        PublisherApiManager.Instance.EquipItemAPI(ref_idx, EquipCallback);
    }
    public void EquipCallback(long statusCode)
    {
        if (statusCode == 200)
        {
            equipIdx = tempEquipIdx;
            for (int i = 0; i < itemSlotList.Count; i++)
            {
                itemSlotList[i].Equip(equipIdx == itemSlotList[i].Refidx);
            }
            InfoManager.Instance.GetMyItems();
            Console.Log(string.Format("장착 성공 : {0}", equipIdx));
            LobbyManager.Instance.GetUserInfo();
        }
        else
        {
            Console.Log(string.Format("장착 실패 : {0}", equipIdx));
        }
    }

    public void UseItem(int idx)
    {
        tempUseIdx= idx;
        Debug.Log("사용시도 : " + idx);
        PublisherApiManager.Instance.UseItemAPI(idx, UseCallback);
    }

    public void UseCallback(long statusCode, string text)
    {
        if (statusCode == 200)
        {
            useIdx = tempUseIdx;

            try
            {
                JObject json = JObject.Parse(text);

                /* {
                       "ecode":0,
                       "update":{
                           "ticket_nick":{
                               "refItemIdx":700,"quantity":0,"used":1
                           }
                       },
                       "check":{
                           "can_chgnick":true,"can_addRefUser":true,"need_adultcert":false,"need_termsofuse":false
                       },
                   } */

                int refItemIdx = -1;
                int quantity = 0;
                usedType used = usedType.notUsed;
                JObject update = json.ValueOrDefault<JObject>("update", new JObject());

                
                //int new_ticket_nick_count = update.ValueOrDefault<int>("ticket_nick", 0);
                //if (new_ticket_nick_count > 0)
                //{
                //    // 닉네임 변경권 new_ticket_nick_count 장이 추가 되었다. 
                //}

                // ticket_nick 닉네임 변경권을 사용하였다. 
                JObject ticket_nick = update.ValueOrDefault<JObject>("ticket_nick", new JObject());
                refItemIdx = ticket_nick.ValueOrDefault<int>("refItemIdx", -1);
                quantity = ticket_nick.ValueOrDefault<int>("quantity", -1);
                used = ticket_nick.ValueOrDefault<usedType>("used", usedType.notUsed);
                if( used == usedType.used ) // 사용하였다면, 아이템을 지운다. 
                {
                    // 모든 닉네임 변경권을 사용하였다. 
                }

                if( quantity >= 0)
                {
                    InfoManager.canChangeNick = true;

                    // TODO:  닉네임 변경 팝업을 띄운다. 
                    LobbyManager lobbyManager = this.GetComponentInParent<LobbyManager>();
                    if( lobbyManager)
                    {
                        lobbyManager.NickNamePopupButton();
                    }
                }

                RequestItems();
            }
            catch (System.Exception e)
            {
                Debug.LogError(e);
            }

            // InfoManager.Instance.GetMyInvenToryList();
            Console.Log(string.Format("사용 성공 : {0}", useIdx));
        }
        else
        {
            Console.Log(string.Format("사용 실패 : {0}", useIdx));
        }
    }
}
