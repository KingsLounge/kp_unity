using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BaccaratInfoTab : InfoTab
{
    public Text winText, tieText, loseText, super6Text, winMoneyText, bankerPairText, playerPairText, maxWinMoneyText, maxWinningStreakText, playerWinText, bankerWinText;
    public override void SetInfo(JObject data)
    {
        base.SetInfo(data);
        long tie = 0;
        long win = 0;
        long lose = 0;
        
        long super6 = 0;
        long winMoney = 0;
        long bankerPair = 0;
        long playerPair = 0;
        long maxWinMoney = 0;
        long maxWinningStreak = 0;
        long playerWin = 0;
        long bankerWin = 0;
        if(data != null)
        {
            tie = data["tie"].ToObject<long>();
            win = data["win"].ToObject<long>();
            lose = data["lose"].ToObject<long>();
            super6 = data["super6"].ToObject<long>();
            winMoney = data["winMoney"].ToObject<long>();
            bankerPair = data["bankerPair"].ToObject<long>();
            playerPair = data["playerPair"].ToObject<long>();
            maxWinMoney = data["maxWinMoney"].ToObject<long>(); 
            maxWinningStreak = data["maxWinningStreak"].ToObject<long>();
            try
            {
                playerWin = data["playerWin"].ToObject<long>();
                bankerWin = data["bankerWin"].ToObject<long>();
            }
            catch{ }
        }
        winText.text = win.ToString();
        tieText.text = tie.ToString();
        loseText.text = lose.ToString();
        super6Text.text = super6.ToString();
        winMoneyText.text = winMoney.ToString();
        bankerPairText.text = bankerPair.ToString();
        playerPairText.text = playerPair.ToString();
        maxWinMoneyText.text = maxWinMoney.ToString();
        maxWinningStreakText.text = maxWinningStreak.ToString();
        bankerWinText.text = bankerWin.ToString();
        playerWinText.text = playerWin.ToString();
        for(int i = 0; i < totalGameTabText.Length; i++)
        {
            if (totalGameTabText[i])
            {
                totalGameTabText[i].text = (win + lose).ToString();
            }
        }
       
    }
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
