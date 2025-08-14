using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.UI;
using System;
public class RecomWindow : MonoBehaviour
{
    [SerializeField]
    private InputField numInput;
    [SerializeField]
    private InputField codeInput;
    [SerializeField]
    private GameObject recomCodeObj;
    [SerializeField]
    private Text timeText;

    private int state = 0;

    private void OnEnable()
    {
        init();
    }
    public void init()
    {
        numInput.text = string.Empty;
        codeInput.text = string.Empty;
        numInput.interactable = true;
        timeText.text = string.Empty;
        recomCodeObj.SetActive(false);
        state = 0;
    }

    public void OnClickButton()
    {
        switch(state)
        {
            case 0:
                SendUserLink();
                break;
            case 1:
                SendUserLinkCode();
                break;
        }
    }
    public async void SendUserLink()
    {
        LoadingCircle.Instance.StartSpin();
        var p = new Packet(CPProtocol.CP_KINGSHILL_USER_LINK);
        p.Add("phoneNum", numInput.text);
        WebSocketManager.defaultCli.Send(p);
        var wait= new WaitForPCProtocol(PCProtocol.PC_KINGSHILL_USER_LINK);
        await wait;
        var err = wait.Result.c.ValueOrDefault("ecode", ERR.OK);
        var expireTime = wait.Result.c.ValueOrDefault("expireTime", 0);
        switch (err)
        {
            case ERR.KINGSHILL_OK_USER_LINK__SEND_CODE:
                recomCodeObj.SetActive(true);
                numInput.interactable = false;
                state = 1;
                StartCountDown(expireTime);
                break;
            case ERR.KINGSHILL_ERR_ALREADY_PHONE_USED:
                NormalMessage.instance.OnOneButtonMessagePopUp("already_phone_num");
                break;
        }
       
        LoadingCircle.Instance.StopSpin();
    }
    public async void StartCountDown(float seconds)
    {
        var targetTime = DateTime.Now.AddSeconds(seconds-10);
        TimeSpan time;
        do
        {
            time = targetTime - DateTime.Now;
            if(time.TotalSeconds < 0)
            {
                time = TimeSpan.Zero;
            }
            timeText.text = $"코드입력 남은시간 {((int)time.TotalSeconds).ToString()} 초";
            await UniTask.WaitForSeconds(0.1f);
        }
        while (time.TotalSeconds > 0);
        numInput.interactable = true;
        recomCodeObj.SetActive(false);
        timeText.text = string.Empty;
        state = 0;
    }
    public async void SendUserLinkCode()
    {
        LoadingCircle.Instance.StartSpin();
        Packet p = new Packet((int)CPProtocol.CP_KINGSHILL_USER_LINK);
        p.Add("phoneNum", numInput.text);
        p.Add("code", codeInput.text);
        WebSocketManager.defaultCli.Send(p.ToJson());
        var wait = new WaitForPCProtocol(PCProtocol.PC_KINGSHILL_USER_LINK);
        await wait;
        var err = wait.Result.c.ValueOrDefault("ecode", ERR.OK);
        switch (err)
        {
            case ERR.OK:
            case ERR.KINGSHILL_OK_USER_LINK__SUCCESS:
                gameObject.SetActive(false);
                break;
            case ERR.KINGSHILL_ERR_ALREADY_PHONE_USED:
                NormalMessage.instance.OnOneButtonMessagePopUp("already_phone_num");
                break;
        }
        
        
        
        LoadingCircle.Instance.StopSpin();
    }
}
