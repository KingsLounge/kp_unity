using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;
using UnityEngine.UI;

public class RankingWindow : MonoBehaviour
{
    [SerializeField]
    private Transform weeklycontainer;
    [SerializeField]
    private Transform dailycontainer;
    [SerializeField]
    private GameObject slotPrefab;
    [SerializeField]
    private List<RankingSlot> dailyRankList;
    [SerializeField]
    private List<RankingSlot> weeklyRankList;

    [SerializeField]
    private RawImage myProfile;
    [SerializeField]
    private Text myNickText;
    [SerializeField]
    private Text myRankText;
    [SerializeField]
    private Text myPointText;

    
    
    

    public void RequestRanking(int type)
    {
        var p = new Packet((int)CPProtocol.CP_RANKING);
        p.Add("type", type);
        WebSocketManager.defaultCli.Send(p);
        LoadingCircle.Instance.StartSpin(CPProtocol.CP_RANKING);
    }

    

    
    public async void SetRank()
    {
        LoadingCircle.Instance.StopSpin();
        var c = LobbyManager.Instance.rankData;
        var type = c["type"].ToObject<int>();
        var arr = c["list"].ToObject<JArray>();
        var list = type == 1 ? dailyRankList : weeklyRankList;
        var cont = type == 1 ? dailycontainer : weeklycontainer;
        if(list == null)
        {
            list = new List<RankingSlot>();
        }
        for(int i = 0; i < arr.Count; i++)
        {
            RankingSlot slot;
            if(i < list.Count)
            {
                slot = list[i];
            }
            else
            {
                slot = Instantiate(slotPrefab, cont).GetComponent<RankingSlot>();
                list.Add(slot);
            }
            slot.SetSlat(arr[i].ToObject<JObject>(),type);
            
        }
        for(int i = arr.Count; i < list.Count; i++)
        {
            list[i].gameObject.SetActive(false);
        }


        myProfile.enabled = false;
        if(!string.IsNullOrEmpty(MyStatus.photoURL))
        {
            myProfile.texture = await ImageDatabase.LoadImageTexture(MyStatus.photoURL, Application.persistentDataPath + "/profileImg", MyStatus.uid);
            myProfile.enabled = true;
        }
        myNickText.text = MyStatus.nick;
        
        if(type == 1)
        {
            myRankText.text = MyStatus.d_rank.ToString();
            myPointText.text = MyStatus.d_exp.ToString();
        }
        else
        {
            myRankText.text = MyStatus.w_rank.ToString();
            myPointText.text = MyStatus.w_exp.ToString();
        }
    }
    void Update()
    {
        
    }
}
