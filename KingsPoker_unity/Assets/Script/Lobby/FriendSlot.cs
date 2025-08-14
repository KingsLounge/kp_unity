using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class FriendSlot : MonoBehaviour
{
    public int idx;
    public Text nickNameText;
    public int state;
    public RawImage profileImage;
    public string profileUrl;
    public Text stateText;
    public GameObject UrlImageObj;
    

    public void OnClickRemoveFriendButton()
    {
        PublisherApiManager.Instance.FriendDeclineAPI(idx,FriendsPopUp.Instance.ChangeFriendCallback);
    }

    public async void SetProfileUrl(string url)
    {
        if(url != profileUrl && !string.IsNullOrEmpty(url))
        {
            SetProfileTexture( await ImageDatabase.LoadImageTexture(url,Application.persistentDataPath + "/profileImg", nickNameText.text));
        }
        else
        {
            UrlImageObj.SetActive(false);
        }
    }

    private void SetProfileTexture(Texture2D texture)
    {
        profileImage.texture = texture;
        UrlImageObj.SetActive(true);
    }
}
