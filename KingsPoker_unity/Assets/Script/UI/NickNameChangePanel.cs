using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
public class NickNameChangePanel : MonoBehaviour
{
    [SerializeField]
    private Text notiText;
    [SerializeField]
    private InputField nickNameInputField;
    [SerializeField]
    private Button updateNicknameButton;
    public int minLength = 2;
    public int maxLength = 16;
    public delegate void ClosePannelDelegate(bool complateChange);
    public ClosePannelDelegate ClosedPannelOnce = null;
    public ClosePannelDelegate ClosedPannel = null;
    private bool nicknameChanged = false;

    private void Awake()
    {
        nickNameInputField.characterLimit = maxLength;
    }

    private void OnEnable() {
        nicknameChanged = false;
        notiText.text = "";
        nickNameInputField.text = "";
        updateNicknameButton.interactable = false;
    }

    private void OnDisable()
    {
        ClosedPannelOnce?.Invoke(nicknameChanged);
        ClosedPannel?.Invoke(nicknameChanged);
        ClosedPannelOnce = null;
    }

    public void ValidateNickname()
    {

        //bool validNickname = Regex.IsMatch(nickNameInputField.text, @"^[a-zA-Z가-힣][a-zA-Z가-힣0-9]{2,16}$");
        bool validNickname = Regex.IsMatch(nickNameInputField.text, $@"^[a-zA-Z가-힣][a-zA-Z가-힣0-9]{"{" + (minLength-1) + "," + (maxLength-1) + "}"}$");

        if (validNickname)
        {
            notiText.text = "";
            updateNicknameButton.interactable = true;
        }
        else
        {
            notiText.text = LocalizeManager.GetLocalString("no_nickname");
            updateNicknameButton.interactable = false;
        }
    }

    public void OnClickNicknameCheckButton()
    {
        Debug.Log("ChangeNickName");
        LoadingCircle.Instance.StartSpin();
        RequestPubNicknameCheck();
    }

    private void RequestPubNicknameCheck()
    {
        PublisherApiManager.Instance.CheckNickname(nickNameInputField.text, (statusCode) =>
        {
            if (statusCode == 200)
            {
                SetNicknameCheckNotification("", Color.red);
                RequestPubRegister(nickNameInputField.text);
            }
            else if (statusCode == 422)
            {
                LoadingCircle.Instance.StopSpin();
                SetNicknameCheckNotification("중복된 닉네임입니다.", Color.red);
            }
            else
            {
                LoadingCircle.Instance.StopSpin();
                ErrorMessageManager.Instance.AddGameError((int)statusCode, "SYS_ERR_NICK_CHECKING", "SYS_ERR_NICK_CHECKING_NOT_DEFINED");
            }
        });
    }

    public void SetNicknameCheckNotification(string text, Color color)
    {
        notiText.color = color;
        notiText.text = text;
    }

    private void RequestPubRegister(string nickName)
    {
        PublisherApiManager.Instance.UpdateNickname(nickName, (statusCode) =>
        {
            if (statusCode == 200)
            {
                MyStatus.nick = nickName;
                if(LobbyManager.Instance)
                {
                    for (int i = 0; i < LobbyManager.Instance.nicknameText.Length; i++)
                    {
                        LobbyManager.Instance.nicknameText[i].text = MyStatus.nick;
                    }
                }
                
                LoadingCircle.Instance.StopSpin();
                nicknameChanged = true;
                gameObject.SetActive(false);
                //nicknameCheckPanel.SetActive(false);
            }
            else
            {
                LoadingCircle.Instance.StopSpin();
                ErrorMessageManager.Instance.AddGameError((int)statusCode, "SYS_ERR_JOIN", "SYS_ERR_NICK_CHANGE");
            }
        });
    }
}
