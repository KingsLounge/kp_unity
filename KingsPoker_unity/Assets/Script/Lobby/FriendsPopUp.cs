using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;
using UnityEngine.UI;
using System;

[Serializable]
public struct ToggleTab
{
    
    public Toggle toggle;
    
    public GameObject tab;
}
public class FriendsPopUp : MonoBehaviour
{
    [SerializeField]
    private GameObject friendSlotPrefab;
    [SerializeField]
    private GameObject friendRequestSlotPrefab;
    [SerializeField]
    private Transform friendsSlotContainer;
    [SerializeField]
    private Transform friendRequestSlotContainer;
    private List<FriendSlot> friendSlotList;
    private List<FriendRequestSlot> friendRequestSlotList;
    private static FriendsPopUp instance;
    public static FriendsPopUp Instance{get{return instance;}}
    [SerializeField]
    private List<ToggleTab> toggleTabList;


    private void Awake() {
         Init();
         TogleOnValueChanged();
    }
    private void OnEnable() {
        ResetList();
    }

    public void Init()
    {
        instance = this;
        friendRequestSlotList = new List<FriendRequestSlot>();
        friendSlotList = new List<FriendSlot>();
    }
    public void ResetList()
    {
        GetFriendsList();
        GetFriendsRequestList();
    }

    public void GetFriendsList()
    {
        PublisherApiManager.Instance.GetFriendListAPI(GetFriendListCallBack);
    }
    
    public void GetFriendsRequestList()
    {
        PublisherApiManager.Instance.GetFriendsRequestListAPI(GetFriendRequestListCallBack);
    }

    public void GetFriendListCallBack(long statusCode, JObject jObject)
    {
        switch(statusCode) 
        {
            case 200://success
                JArray arr = jObject.GetValue("list")as JArray;
                FriendListReset(arr);
                Console.Log("GetFriends Success");
                break;
            case 400://BadRequest
                Console.Log("BadRequest");
                break;
            case 402://BadRequest
                Console.Log("AbnormalReceipt");
                break;
        }
    }

    public void GetFriendRequestListCallBack(long statusCode, JObject jObject)
    {
        switch(statusCode) 
        {
            case 200://success
                JArray arr = jObject.GetValue("list")as JArray;
                FriendRequestListReset(arr);
                Console.Log("GetFriendsRequest Success");
                break;
            case 400://BadRequest
                Console.Log("BadRequest");
                break;
            case 402://BadRequest
                Console.Log("AbnormalReceipt");
                break;
        }
    }
    public void ChangeFriendCallback(long statusCode, JObject jObject)
    {
        switch(statusCode) 
        {
            case 200://success
                ResetList();
                Console.Log("ChangeFriends Success");
                break;
            case 400://BadRequest
                Console.Log("BadRequest");
                break;
            case 402://BadRequest
                Console.Log("AbnormalReceipt");
                break;
        }
    }

    public void FriendListReset(JArray arr)
    {
        Console.Log(arr.Count);
        for(int i = 0; i < arr.Count; i++)
        {
            Console.Log(arr[i].ToString());
            FriendSlot slot;
            
            if(friendSlotList.Count > i)
            {
                slot = friendSlotList[i];
            }
            else
            {
                slot = Instantiate(friendSlotPrefab, friendsSlotContainer).GetComponent<FriendSlot>();
                friendSlotList.Add(slot); 
            }
            var job = arr[i]as JObject;
            
            slot.idx = (int)job.GetValue("idx");
            slot.nickNameText.text = (string)job.GetValue("f_nick");
            slot.state = (int)job.GetValue("state");
            slot.stateText.text = slot.state.ToString();
            slot.SetProfileUrl((string)job.GetValue("picture"));
            Console.Log(string.Format("{0} : {1}",(string)job.GetValue("nick"),(int)job.GetValue("idx")));
        }
        while(friendSlotList.Count > arr.Count)
        {
            var go  = friendSlotList[arr.Count].gameObject;
            friendSlotList.RemoveAt(arr.Count);
            Destroy(go); 
        }
    }

    public void FriendRequestListReset(JArray arr)
    {
        Console.Log(arr.Count);
        for(int i = 0; i < arr.Count; i++)
        {
            Console.Log(arr[i].ToString());
            FriendRequestSlot slot;
            if(friendRequestSlotList.Count > i)
            {
                slot = friendRequestSlotList[i];
            }
            else
            {
                slot = Instantiate(friendRequestSlotPrefab, friendRequestSlotContainer).GetComponent<FriendRequestSlot>();
                friendRequestSlotList.Add(slot);                
            }
            var job = arr[i]as JObject;
            slot.idx = (int)job.GetValue("idx");
            slot.nickNameText.text = (string)job.GetValue("nick");
            slot.state = (int)job.GetValue("state");
        }
        while(friendRequestSlotList.Count > arr.Count)
        {
            var go  = friendRequestSlotList[arr.Count].gameObject;
            friendRequestSlotList.RemoveAt(arr.Count);
            Destroy(go); 
        }
    }

    
    

    public void TogleOnValueChanged()
    {
        for(int i = 0; i < toggleTabList.Count; i++)
        {
            toggleTabList[i].tab.SetActive(toggleTabList[i].toggle.isOn);
        }
    }
}
