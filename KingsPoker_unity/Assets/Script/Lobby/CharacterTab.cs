using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;

public class CharacterTab : MonoBehaviour
{
    [SerializeField]
    private List<CharacterSelectSlot> characterSelectSlots;
    [SerializeField]
    private GameObject characterSelectSlotPrefab;
    [SerializeField]
    private Transform characterSelectSlotContainer;
    private int equipIdx;

    private void Awake()
    {
        InfoManager.AddValueChangeLisener(OnValueChange);
    }
    public void OnValueChange(string dataName)
    {
        switch (dataName)
        {
            case "myItems":
                SetCharacterSelectTab();
                break;
        }

    }
    private void OnEnable() {
        SetCharacterSelectTab();
    }

    public void SetCharacterSelectTab()
    {
        Dictionary<int,JObject> itemData = InfoManager.itemData;
        var invenList = InfoManager.MyItems;

        List<JObject> characterData = new List<JObject>();

        foreach(var charItem in itemData)
        {
            if(charItem.Value["type"].ToObject<int>() == 3)
            {
                characterData.Add(charItem.Value);
            }
        }
        characterData.Sort((data1,data2)=>{return data1["idx"].ToObject<int>() < data2["idx"].ToObject<int>()? 1:0;});
        for(int i = 0; i < characterData.Count; i++)
        {
            bool hasItem = false;
            int refIdx = characterData[i]["idx"].ToObject<int>();
            bool equipd = false;
            bool used = false;
            string expiryTime = null;

            try
            {
                for (int j = 0; j < invenList.Count; j++)
                {
                    if (refIdx == invenList[j]["ref_idx"].ToObject<int>())
                    {
                        hasItem = true;
                        equipd = invenList[j]["equipped"].ToObject<int>() != 0;
                        Console.SpecialLog(string.Format("idx {0} : {1}", refIdx, invenList[j]["equipped"].ToObject<int>()));
                        used = invenList[j]["used"].ToObject<int>() == 2;
                        if (used)
                        {
                            expiryTime = invenList[j]["expiry_date"].ToObject<string>();
                        }

                        break;
                    }
                }
            }
            catch { }
            

            

            if(i < characterSelectSlots.Count)
            {
                characterSelectSlots[i].SetSlot(refIdx, hasItem, equipd, used, expiryTime);
                characterSelectSlots[i].characterTab = this;
            }
        }
    }

    public void Equip(int idx)
    {
        equipIdx = idx;
        PublisherApiManager.Instance.EquipItemAPI(idx,EquipCallback);
    }

    public void EquipCallback(long statusCode)
    {
        if(statusCode == 200)
        {
            for(int i = 0; i < characterSelectSlots.Count; i++)
            {
                characterSelectSlots[i].Equip(equipIdx == characterSelectSlots[i].Idx);
            }
            LobbyManager.Instance.GetUserInfo();
            Console.Log(string.Format("장착 성공 : {0}", equipIdx));
        }
        else
        {
            Console.Log(string.Format("장착 실패 : {0}", equipIdx));
        }
        
        
    }
}
