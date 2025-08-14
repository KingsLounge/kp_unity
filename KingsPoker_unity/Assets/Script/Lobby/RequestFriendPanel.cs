using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json.Linq;

public class RequestFriendPanel : MonoBehaviour
{
    public InputField nickNameInputField;
    public Text notiText;
    public void OnclickRequestFriendButton()
    {
        PublisherApiManager.Instance.FriendRequestAPI(nickNameInputField.text,FriendRequestCallBack);
    }

    private void OnEnable()
    {
        if(notiText)
        {
            notiText.enabled = false;
        }
        
    }

    private void FriendRequestCallBack(long statusCode, JObject jObject)
    {
        switch(statusCode) 
        {
            case 200://success
                gameObject.SetActive(false);
                FriendsPopUp.Instance.ResetList();
                Console.Log("Friend Request Success");
                break;
            case 400://BadRequest
                Console.Log("BadRequest");
                break;
            case 402://BadRequest
                Console.Log("AbnormalReceipt");
                break;
        }
    }
}
