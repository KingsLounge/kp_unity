using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;

public class MailManager : MonoBehaviour
{
    public GameObject mailSlotPrefab;
    private List<MailSlot> mails;
    public Transform contentContainer;
    public Animator ani;
    

    
    public void OpenMailBox()
    {
        PublisherApiManager.Instance.GetMailListPubApi(MailCallback);
    }

    public void MailCallback(long statusCode, JObject data)
    {
        switch(statusCode)
        {
            case 200:
                Console.Log("success Get Mails");
                //gameObject.SetActive(true);
                SetMailBox(data);
            break;
            default:
                Console.Error("Faild Get Mails");
            break;
        }
    }

    public void SetMailBox(JObject data)
    {
        if(mails == null)
        {
            mails = new List<MailSlot>();
        }

        var list = (JArray)data["list"];
        Console.Log(list.Count.ToString());
        for(int i = 0; i < list.Count; i++)
        {
            MailSlot slot;
            if(i < mails.Count)
            {
                slot = mails[i];
            }
            else
            {
                slot = Instantiate(mailSlotPrefab, contentContainer).GetComponent<MailSlot>();
                mails.Add(slot);
            }
            slot.manager = this;
            slot.SetSlot((JObject)list[i]);
        }

        while(mails.Count > list.Count)
        {
            var m = mails[list.Count];
            mails.RemoveAt(list.Count);
            Destroy(m.gameObject);
        }
    }

    public void RemoveSlot(int idx)
    {
        var slot = mails.Find((MailSlot sl)=>{return sl.idx == idx;});
        mails.Remove(slot);
        Destroy(slot.gameObject);
    }

    public void OnClickCloseButton()
    {
        ani.Play("ShopCloseAni");
    }
    public void CloseWindow()
    {
        gameObject.SetActive(false);
    }
}
