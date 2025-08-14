using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

public class HandLogBalloon : MonoBehaviour
{
    [SerializeField]
    private Text nickname;

    [SerializeField]
    private Text amount;

    [SerializeField]
    private RawImage profileUrlImgae;
    public GameObject profileImageObj;
    public Image characterProfileImag;
    public SpriteAtlas characterAtlas;
    protected string profileUrl = "";
    public bool showProfileImage = true;

    public void Setting(string nick, int seat, string contents, string gid, string url, int icon_no)
    {
        if (nickname)
            nickname.text = nick;
        if (amount)
            amount.text = contents;
        if (showProfileImage)
        {
            //SetProfileUrl(url, !string.IsNullOrEmpty(url), gid, icon_no);
            SetProfileUrl(url, false, gid, icon_no);
        }
    }

    public async void SetProfileUrl(string url, bool useUrl, string gid, int icon_no)
    {
        if (useUrl)
        {
            if (url != profileUrl && !string.IsNullOrEmpty(url))
            {
                SetProfileTexture(
                    await ImageDatabase.LoadImageTexture(
                        url,
                        Application.persistentDataPath + "/profileImg",
                        gid
                    )
                );
            }
            else if (string.IsNullOrEmpty(url))
            {
                profileImageObj.SetActive(false);
            }
        }
        else
        {
            var imageStr = "";
            try
            {
                imageStr = TableDataManager.characterTable[icon_no]["2d_index"].ToObject<string>();
            }
            catch { }

            var sprite = characterAtlas.GetSprite(imageStr);
            if (sprite != null)
            {
                SetProfileTexture(sprite);
            }
            else
            {
                profileImageObj.SetActive(false);
            }
        }
    }

    private void SetProfileTexture(Texture2D texture)
    {
        profileUrlImgae.texture = texture;
        characterProfileImag.enabled = false;
        profileUrlImgae.enabled = true;
        profileImageObj.SetActive(true);
    }

    private void SetProfileTexture(Sprite sprite)
    {
        characterProfileImag.sprite = sprite;
        characterProfileImag.enabled = true;
        profileUrlImgae.enabled = false;
        profileImageObj.SetActive(true);
    }
}
