using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Globalization;


public class TournamentInfoItem : MonoBehaviour
{
    public GameObject spriteFontContainer = null;
    public SpriteFontData spriteFont = null;

    public Text titleText;
    public Text buyinText = null;

    [Space]

    public GameObject detailPanel;
    public Text startTimeText;
    public Text closeTimeText;
    public Text countAllUserText;
    public Text startChipText;
    public Text rebuyText;
    public Text addOnText;
    public Text countDownText;

    public Button moreInfoButton;
    public Button applyButton;
    public Button unapplyButton;
    public Button enterRoomButton;

    [Space]

    int tn;
    TNMT_FLOW state;
    long totalPrize;
    string title;
    long buyIn, startingChip;

    string startTime;
    string closeTime;

    long rebuyCount, rebuyCost, rebuyChip;
    long addOnCount, addOnCost, addOnChip;
    public List<SpriteFontPrefab> spriteFontScripts = new List<SpriteFontPrefab>();
    int applyCount;
    
    //private List<long> rewardList;
    public Coroutine countCo;
    private bool canEnterRoom;

    private void OnEnable() {
       
            if(countCo != null)
            {
                StopCoroutine(countCo);
            }
            countCo = StartCoroutine(CountDownCo());
        
    }
    public void Setting(int tn, TNMT_FLOW state, long totalPrize, string title, long buyIn, long startingChip, string startTime, string closeTime, long rebuyCount, long rebuyCost, long rebuyChip, long addOnCount, long addOnCost, long addOnChip, int countAllUser)
    {
        if (state == TNMT_FLOW.cancel_abnormal)
        {
            gameObject.SetActive(false);
            return;
        }

        this.tn = tn;
        this.state = state;
        this.totalPrize = totalPrize;
        this.title = title;

        this.buyIn = buyIn;
        this.startingChip = startingChip;

        this.startTime = startTime;
        this.closeTime = closeTime;
        
        this.rebuyCount = rebuyCount;
        this.rebuyCost = rebuyCost;
        this.rebuyChip = rebuyChip;
        
        this.addOnCount = addOnCount;
        this.addOnCost = addOnCost;
        this.addOnChip = addOnChip;
        this.applyCount = countAllUser;

        
        

        //string spriteFontText = MoneyToString.Converting(sb, unit) + '/' + MoneyToString.Converting(bb, unit);
        string spriteFontText = MoneyToString.Converting(totalPrize);
        List<Sprite> sprites = spriteFont.ConvertSpriteFont(spriteFontText);
        var tr = spriteFontContainer.transform;
        
        //spriteFontContainer.transform.DestroyChildren();
        countAllUserText.text = countAllUser.ToString();
        buyinText.text = MoneyToString.Converting(buyIn);
        titleText.text = title;
        DateTime startDate = DateTimeParser.Parse(startTime);
        var ci =  new CultureInfo("en-US");
        startTimeText.text = string.Format(startDate.ToString("MM/dd(ddd) HH:mm",ci));

        for(int i = 0; i < sprites.Count; i++) {
            SpriteFontPrefab go = null;
            if(spriteFontScripts.Count > i) {
                go = spriteFontScripts[i];
            }
            else {
                go = ObjectPoolManager.Instance.spritfontPool.Pop();
                spriteFontScripts.Add(go);
                go.rect.SetParent(spriteFontContainer.transform);
            }
            
            //rect.sizeDelta = new Vector2(sprites[i].texture.width, sprites[i].texture.height);
            
            go.image.sprite = sprites[i];
            go.image.SetNativeSize();
            
            
            go.rect.localScale = new Vector3(1,1,1);
        }
        while(spriteFontScripts.Count > sprites.Count)
        {
            var child = spriteFontScripts[0];
            spriteFontScripts.RemoveAt(0);
            ObjectPoolManager.Instance.spritfontPool.Push(child);
        }
        
        
        UpdateApplyOrUnapply();
    }

    public void UpdateApplyOrUnapply()
    {
        gameObject.SetActive(true);

//        Debug.LogError(state.ToString());
        var el = MyStatus.eliminates.Find((Eliminate)=>{return Eliminate.tn == tn;});
        if(el == null)
        {
            if (state == TNMT_FLOW.open)
            {
                
                if (MyStatus.TnmtApplyInspection(tn))
                {
                    if(canEnterRoom)
                    {
                        unapplyButton.gameObject.SetActive(false);
                        enterRoomButton.gameObject.SetActive(true);
                        applyButton.gameObject.SetActive(false);
                    }
                    else
                    {
                        unapplyButton.gameObject.SetActive(true);
                        enterRoomButton.gameObject.SetActive(false);
                        applyButton.gameObject.SetActive(false);
                    }
                
                }
                else 
                {
                    applyButton.gameObject.SetActive(true);
                    enterRoomButton.gameObject.SetActive(false);
                    unapplyButton.gameObject.SetActive(false);
                }
            }
            else if (state == TNMT_FLOW.start)
            {
                if (MyStatus.TnmtApplyInspection(tn))
                {
                    unapplyButton.gameObject.SetActive(false);
                    enterRoomButton.gameObject.SetActive(true);
                    applyButton.gameObject.SetActive(false);
                }
                else
                {
                    applyButton.gameObject.SetActive(true);
                    enterRoomButton.gameObject.SetActive(false);
                    unapplyButton.gameObject.SetActive(false);
                }
            }
            else if(state == TNMT_FLOW.close)
            {
                if (MyStatus.TnmtApplyInspection(tn))
                {
                    unapplyButton.gameObject.SetActive(false);
                    enterRoomButton.gameObject.SetActive(true);
                    applyButton.gameObject.SetActive(false);
                }
                else
                {
                    gameObject.SetActive(false);
                }
            }

        }
        else
        {
            gameObject.SetActive(false);
        }
        
        
        
    }

    public void OnClickEnterButton()
    {
        //WebSocketManager.defaultCli.Send("{\"p\":103,\"c\":{\"gtn\":0,\"game_type\":\"" + gameType + "\",\"blind\":" + bng + "}}");
        DateTime startDateTime = DateTimeParser.Parse(startTime);
        DateTime closeDateTime = DateTimeParser.Parse(closeTime);
        string startTimeStr = string.Format("{0} {1}", startDateTime.ToShortDateString(), startDateTime.ToShortTimeString());
        string closeTimeStr = string.Format("{0} {1}", closeDateTime.ToShortDateString(), closeDateTime.ToShortTimeString());
        //countAllUserText = string.Format("");
        string totalPlayerStr = string.Format("{0} 명", applyCount);
        string buyInStr = MoneyToString.Converting(buyIn);
        string startChipStr = string.Format("{0} 칩", startingChip);
        string rebuy = string.Format("{0}번, {1} ({2})", rebuyCount, rebuyCost, rebuyChip);
        string addOn = string.Format("{0}번, {1} ({2})", addOnCount, addOnCost, addOnChip);
        //detailPanel.SetActive(true);
        var rewardList = HoldemRewardTable.GetRewardTable(applyCount, totalPrize);
        LobbyManager.Instance.OnTournamentDetailPopup(tn, title, buyInStr, startTimeStr, closeTimeStr, totalPlayerStr, startChipStr, rebuy, addOn, rewardList );
    }

    public void OnClickApply()
    {
        Packet p = new Packet((int)CPProtocol.CP_TNMT_APPLY);
        p.Add("tn", tn);
        p.Add("ticket", 0);
        p.Add("rebuy", 0);
        p.Add("double", 0);
        WebSocketManager.defaultCli.Send(p.ToJson());
        //WebSocketManager.defaultCli.Send("{\"p\":31,\"c\":{\"gtn\":0,\"game_type\":\"" + curGameType + "\",\"blind\":" + cb.bng + "}}");
    }

    public void OnClickUnapply()
    {
        Packet p = new Packet((int)CPProtocol.CP_TNMT_UNAPPLY);
        p.Add("tn", tn);
        WebSocketManager.defaultCli.Send(p.ToJson());
        //WebSocketManager.defaultCli.Send("{\"p\":31,\"c\":{\"gtn\":0,\"game_type\":\"" + curGameType + "\",\"blind\":" + cb.bng + "}}");
    }

    public void OnClickEnterRoom()
    {
        Packet p = new Packet((int)CPProtocol.CP_ROOM_ENTER);
        //p.Add("gtn", MyStatus.tnrn);
        p.Add("game_type", (int)GAME_TYPE.nlh);
        p.Add("blind", 0);
        WebSocketManager.defaultCli.Send(p.ToJson());
    }
    private IEnumerator CountDownCo()
    {
        var stTime = DateTimeParser.Parse(startTime);
        var wait = new WaitForSeconds(1f);
        var korSpan = new TimeSpan(9,0,0);
        
        TimeSpan count;
        do
        {
            var now = DateTime.UtcNow; //+ korSpan;
            count = stTime - now;
            countDownText.text = TimeToString.SpanToString(count);
           
            
            if(canEnterRoom != count.TotalSeconds <= 60)
            {
                canEnterRoom = count.TotalSeconds <= 60;
                UpdateApplyOrUnapply();
            }
            
            countDownText.enabled = count.TotalSeconds > 0;
            yield return wait;
        }
        while(count.TotalSeconds > 0);
    }

}
