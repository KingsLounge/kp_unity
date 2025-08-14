using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FriendRequestSlot : MonoBehaviour
{
    public int idx;
    public Text nickNameText;
    public int state;
    public RawImage profileImage;
    public string profileImageUrl;

    public void OnClickRejectButton()
    {
        PublisherApiManager.Instance.FriendDeclineAPI(idx,FriendsPopUp.Instance.ChangeFriendCallback);
    }

    public void OnClickAcceptButton()
    {
        PublisherApiManager.Instance.FriendAcceptAPI(idx,FriendsPopUp.Instance.ChangeFriendCallback);
    }
    
}
