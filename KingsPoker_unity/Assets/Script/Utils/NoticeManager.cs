using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;
using System;
using UnityEngine.UI;


public enum NoticeTypeEnum
{
    normal = 0,
    important = 1,
    repeat = 2
}
public class NoticeData
{
    public string msg;
    public string nickname;
    public float time;
    public NoticeTypeEnum type;
    
    public NoticeData(NoticeTypeEnum t, string msg,  float time = 1f, string nickname = "")
    {
        this.type = t;
        this.msg = msg;
        this.nickname = nickname;
        this.time = time;
    }
}

public class NoticeManager : MonoBehaviour
{
    public NoticePanel noticePanel;
    public NoticePanel importantNoticePanel;
    public NoticePanel repeatNoticePanel;
    GameObject parent;

    Queue<NoticeData> normalQueue = new Queue<NoticeData>();
    Queue<NoticeData> importantQueue = new Queue<NoticeData>();
    Queue<NoticeData> repeatQueue = new Queue<NoticeData>();

    public bool normalNoticeActive = false;
    public bool importantNoticeActive = false;
    public bool repeatNoticeActive = false;

    private NoticeData lastNormalMessage = null;
    private NoticeData lastImporantMessage = null;
    private NoticeData lastRepeatMessage = null;

    private DateTime gameStartTime;
    private DateTime now;
    private double playTimeSeconds;
    private static NoticeManager instance;
    public static NoticeManager Instance
    {
        get 
        {
            if (instance == null)
                instance = new NoticeManager();
            return instance; 
        }
    }

    protected void Awake()
    {
        instance = this;
        gameStartTime = DateTime.UtcNow;
        now = gameStartTime;
        //InvokeRepeating("TimeNotice", 5f, 5f);
    }

    public void TimeNotice()
    {
        //queue.Enqueue(new NoticeData(NoticeTypeEnum.normal, "테스트테스트", "정운", 2.0f));
    }

    public void SetLoginTime()
    {
        gameStartTime = DateTime.UtcNow;
        playTimeSeconds = 0d;
    }
    public void ChekTime()
    {
        if(!string.IsNullOrEmpty(MyStatus.token))
        {
            var curTimespan = now - gameStartTime;
            now = DateTime.UtcNow;
            var newTimespan = now - gameStartTime;
            if ((int)curTimespan.TotalHours < (int)newTimespan.TotalHours)
            {
                normalQueue.Enqueue(new NoticeData(NoticeTypeEnum.normal, $"게임을 시작한지 <size=50><b><color=yellow> {(int)newTimespan.TotalHours} </color></b></size>시간 지났습니다."));
            }
        }
    }
    public void ChekTimeWidthDeltaTime()
    {
        if (!string.IsNullOrEmpty(MyStatus.token))
        {
            var bforeSpan = TimeSpan.FromSeconds(playTimeSeconds);
            playTimeSeconds += Time.deltaTime;
            var playTime = TimeSpan.FromSeconds(playTimeSeconds);
            
            if ((int)bforeSpan.TotalHours < (int)playTime.TotalHours)
            {
                normalQueue.Enqueue(new NoticeData(NoticeTypeEnum.normal, $"게임을 시작한지 <size=50><b><color=yellow> {(int)playTime.TotalHours} </color></b></size>시간 지났습니다."));
            }
        }
    }


    public void AddNotice(NoticeData n)
    {
        switch(n.type)
        {
            case NoticeTypeEnum.normal:
                
                if(normalNoticeActive == true && lastNormalMessage != null && lastNormalMessage.msg.Equals(n.msg))
                {
                    
                }
                else
                {
                    normalQueue.Enqueue(n);
                    lastNormalMessage = n;
                }
                
                
                break;
            case NoticeTypeEnum.important:
                if (importantNoticeActive == true && lastImporantMessage != null && lastImporantMessage.msg.Equals(n.msg))
                {

                }
                else
                {
                    importantQueue.Enqueue(n);
                    lastImporantMessage = n;
                }
                    
                break;
            case NoticeTypeEnum.repeat:
                if (repeatNoticeActive == true && lastRepeatMessage != null && lastRepeatMessage.msg.Equals(n.msg))
                {

                }
                else
                {
                    repeatQueue.Enqueue(n);
                    lastRepeatMessage = n;
                }
                
                break;
        }
        
    }
    public void AddNotice(string str, NoticeTypeEnum t = NoticeTypeEnum.normal)
    {
        normalQueue.Enqueue(new NoticeData(t, str));
    }

    private void Update()
    {
        CheckQueue();
        //ChekTime();
        //ChekTimeWidthDeltaTime();
#if UNITY_EDITOR
        if(Input.GetKeyDown(KeyCode.W))
        {
            AddNotice(new NoticeData(NoticeTypeEnum.important, "중요한 공지사항 메시지입니다. 작동은 제대로 하는걸까요?"));
        }
        if(Input.GetKeyDown(KeyCode.E))
        {
            normalQueue.Enqueue(new NoticeData(NoticeTypeEnum.normal, $"게임을 시작한지 <size=50><b><color=yellow> {5} </color></b></size>시간 지났습니다."));
        }
#endif
    }

    //protected override void ReceivePacket(Packet packet)
    //{
    //    base.ReceivePacket(packet);
    //    int p = packet.p;
    //    JObject c = packet.c;
    //    Notice n = null;

    //    string test = "안녕하세요\n원게임입니다\n지금부터 긴급점검이 있을 예정이니\n유저분들은 정상적으로 게임을 종료해주십시오";
    //    string test2 = "용준님께서 잭팟에 당첨되셨습니다. 축하드립니다";
    //    switch ((PCProtocol)p)
    //    {
    //        case PCProtocol.PC_USER_NOTICE:
    //            {
    //                queue.Enqueue(new Notice((PCProtocol)p, test2, "정운", 5.0f));
    //                Debug.Log($"Test log : {p} in Notice");
    //                break;
    //            }
    //        //테스트용 프로토콜 잭팟용
    //        case (PCProtocol)14:
    //            {
    //                // msg, nickname, time은 추후 조정
    //                n = new Notice((PCProtocol)p, test2, "정운", 5.0f);
    //                break;
    //            }
    //        //테스트용 프로토콜 긴급공지
    //        case (PCProtocol)127:
    //            {
    //                // msg, nickname, time은 추후 조정
    //                n = new Notice((PCProtocol)p, test, null, 5.0f);
    //                break;
    //            }
    //    }
    //    //queue.Enqueue(n);
    //}

    private void CheckQueue()
    {

        if (normalQueue.Count > 0)
        {
            if (!normalNoticeActive)
            {
                NoticeData n = normalQueue.Dequeue();
                noticePanel.SetNotice(n,()=>normalNoticeActive = false);
                normalNoticeActive = true;
            }
        }
        if (importantQueue.Count > 0)
        {
            if (!importantNoticeActive && importantNoticePanel)
            {
                NoticeData n = importantQueue.Dequeue();
                importantNoticePanel.SetNotice(n, () => importantNoticeActive = false);
                importantNoticeActive = true;
            }
        }
        if(repeatQueue.Count > 0)
        {
            if (!repeatNoticeActive && repeatNoticePanel)
            {
                NoticeData n = repeatQueue.Dequeue();
                repeatNoticePanel.SetNotice(n, () => repeatNoticeActive = false);
                repeatNoticeActive = true;
            }
        }
    }

    //private void CreateNotice(NoticeData n)
    //{
    //    if (notice != null)
    //        return;
    //    switch (n.type)
    //    {
    //        case (PCProtocol)14:
    //            prefab = Resources.Load<Text>("Prefab/OneGame/NoticeEvent");
    //            break;
    //        case (PCProtocol)127:
    //            prefab = Resources.Load<Text>("Prefab/OneGame/NoticeEmergency");
    //            break;
    //    }
      
    //    notice = Instantiate<Text>(prefab);
    //    parent = GameObject.Find("Canvas");
    //    notice.transform.SetParent(parent.transform, false);
    //    StartCoroutine(Notify(n, DestroyNotice));
    //}
    
    private IEnumerator Notify(NoticeData n, Action callback)
    {
        string[] sentences = n.msg.Split('\n');

        foreach(string sentence in sentences)
        {
            //notice.text = sentence;
            yield return new WaitForSeconds(n.time);
        }

        /*for(int i =0; i<sentences.Length; i++)
        {
            notification.text = sentences[i];
            yield return new WaitForSeconds(1.5f);
        }*/

        callback();
    }

    //private void DestroyNotice()
    //{
    //    Destroy(notice.gameObject);
    //    notice = null;
    //}




}