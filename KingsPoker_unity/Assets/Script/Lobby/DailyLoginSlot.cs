using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json.Linq;
using UnityEngine.Networking;
using Cysharp.Threading.Tasks;

public class DailyLoginSlot : MonoBehaviour
{
    public Text titleText;
    public Text rewardText;
    public Button rewardButton;
    public GameObject completeImageObj;
    public Image slotImage;
    public List<Sprite> slotImageSprite;
    private int day;
    private int id;
    [SerializeField]
    private float stampDelay = 0.5f;
    private string nameKey;
    [SerializeField]
    private Animator effectAnimator;
    [SerializeField]
    private GameObject effects;
    public bool emailReward = true;
    public void SetDailyQuestSlotSet(int day, int continued)
    {
        var data = InfoManager.Instance.GetRefQuestWithDay(day);
        // 임시코드 Table 변경 후 return 만 남기고 삭제할 것!

        if (data == null)
            return;

        id = data.id;
        this.day = day;
        nameKey = data.name;
        titleText.text = LocalizeManager.GetLocalString(nameKey);

        if (data.rewardChip > 0)
        {
            rewardText.text = MoneyToString.Converting(data.rewardChip) + "<size=32><color=#ffff99>코인</color></size>";
        }
        //if (data.rewardSilver > 0)
        //{
        //    rewardText.text = MoneyToString.Converting(data.rewardSilver) + "<size=32><color=#a0b0ff>칩</color></size>";
        //}


        if (day < continued)
        {
            completeImageObj.SetActive(true);
            rewardButton.interactable = false;
        }
        else if (day == continued)
        {
            completeImageObj.SetActive(false);
            rewardButton.interactable = !MyStatus.isLoginRewarded;
            if (rewardButton.interactable)
            {
                Invoke("OnclickButton", stampDelay);
            }
        }
        else
        {
            completeImageObj.SetActive(false);
            rewardButton.interactable = false;
        }
    }


    public void OnclickButton()
    {
        LoadingCircle.Instance.StartSpin();
        RequestDailyClear(id);
        /*
        if (!(await RequestDailyClear(id)))
            return;

        completeImageObj.SetActive(true);
        SoundManager.Instance.PlayEffectSound(Sound_TableEnum.SFX_ATTENDANCE_SUUCESS);
        rewardButton.interactable = false;
        RefQuest data = InfoManager.Instance.GetRefQuestWithDay(day);

        LoadingCircle.Instance.StartSpin();
        UnityWebRequest www = await PublisherApiManager.Instance.PointInAPIAsync("silver", data.rewardChip);
        LoadingCircle.Instance.StopSpin();
        */
    }

    public async void RequestDailyClear(int id)
    {
        LoadingCircle.Instance.StartSpin();
        UnityWebRequest www = await PublisherApiManager.Instance.RequestDailyClear(id);
        LoadingCircle.Instance.StopSpin();
        if (www == null || www.responseCode != 200)
        {
            Debug.Log("RequestDailyClear Fail");
            
        }
        else
        {
            DailyCallback();
        }
       
    }

    public void DailyCallback()
    {
        LoadingCircle.Instance.StopSpin();
        completeImageObj.SetActive(true);
        if (effectAnimator)
        {
            effectAnimator.SetTrigger("Play");
        }
        if (effects)
        {
            effects.SetActive(true);
        }
        SoundManager.Instance.PlayEffectSound(Sound_TableEnum.SFX_ATTENDANCE_SUUCESS);
        rewardButton.interactable = false;
        var data = InfoManager.Instance.GetRefQuestWithDay(day);
        LoadingCircle.Instance.StartSpin();
        if (!emailReward)
        {
            //if (data.rewardGold > 0)
            //{
            //    PublisherApiManager.Instance.PointInAPI("gold", data.rewardGold, PointInCallBack);
            //}
            //if (data.rewardSilver > 0)
            //{
            //    PublisherApiManager.Instance.PointInAPI("silver", data.rewardSilver, PointInCallBack);
            //}
        }
        else
        {
            NextAction();
        }
    }

    public void PointInCallBack(long statusCode, JObject json)
    {
        //LoadingCircle.Instance.StopSpin();
        //LobbyManager.Instance.TryRequestChip();//돈없으면 무료칩 지급 시도
        //var p = new Packet((int)CPProtocol.CP_CONSOLE_USERINFO);
        //WebSocketManager.defaultCli.Send(p.ToJson());
        NextAction();
    }

    public void NextAction()
    {
        LoadingCircle.Instance.StopSpin();
        LobbyManager.Instance.GetUserInfo();
    }
}
