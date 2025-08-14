using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PokerOdds;
using Newtonsoft.Json.Linq;
using System;

public class InfoTab : MonoBehaviour
{
    //[SerializeField]
    //protected Text totalGameCountText, buttonTotalgameText, maxWinChipText, oddsText, raiseText, checkText, foldText, callText, preflopText, flopText, turnText, riverText;
    //[SerializeField]
    //protected Image[] cardImages;
    //[SerializeField]
    //protected Image hands;
    //[SerializeField]
    //protected List<JockboSet> jockboSets;
    [SerializeField]
    protected Text[] totalGameTabText;
    
    public virtual void SetInfo(JObject data)
    {

    }
    public virtual void SetTotalInfo(JObject data1, JObject data2)
    {

    }

    //public void SetInfo(int win, int lose, int flop, int turn, int river, int preflop, int call, int fold, int check, int raise, string bestHand, long winMoney, long loseMoney, long bestValue)
    //{
    //    hands.enabled = true;
    //    totalGameCountText.text = string.Format("{0} 핸드", win + lose);
    //    buttonTotalgameText.text = (win + lose).ToString();
    //    maxWinChipText.text = string.Format("{0} 칩", winMoney);
    //    oddsText.text = string.Format("{0} %", win==0 ? 0:(100 * win / (win+lose)));
    //    var totalBetCount = raise + check + fold + call;
    //    raiseText.text = string.Format("{0} %",totalBetCount == 0 ? 0 : (100 * raise / totalBetCount));
    //    checkText.text = string.Format("{0} %",totalBetCount == 0 ? 0 : (100 * check / totalBetCount));
    //    callText.text = string.Format("{0} %",totalBetCount == 0 ? 0 : (100 * call / totalBetCount));
    //    foldText.text = string.Format("{0} %",totalBetCount == 0 ? 0 : (100 * fold / totalBetCount));

        
    //    preflopText.text = string.Format("{0} %",fold == 0 ? 0 : (100 * preflop / fold));
    //    flopText.text = string.Format("{0} %",fold == 0 ? 0 : (100 * flop / fold));
    //    turnText.text = string.Format("{0} %",fold == 0 ? 0 : (100 * turn / fold));
    //    riverText.text = string.Format("{0} %",fold == 0 ? 0 : (100 * river / fold));
    //    if(!string.IsNullOrEmpty(bestHand))
    //    {
    //        Console.Log(bestHand);
    //        var jokbov = CalculateCardsValue.calc(bestHand);
    //        var jokbo = CalculateCardsValue.jokboName(jokbov);
    //        var cards = bestHand.Split(',');
    //        for(int i = 0; i < cardImages.Length; i++)
    //        {
    //            if(i < cards.Length)
    //            {
    //                cardImages[cardImages.Length - i -1].sprite = CardSets.GetCard(cards[i]);
    //            }
    //            else
    //            {
    //                cardImages[cardImages.Length - i -1].sprite = CardSets.GetCard("**");
    //            }
    //        }
    //        for(int i = 0; i < jockboSets.Count; i++)
    //        {
    //            if(jockboSets[i].jockboName == jokbo[0])
    //            {
    //                hands.sprite = jockboSets[i].jockboImage;
    //            }
    //        }
    //        Console.Log(jokbo[0]);
    //    }
    //    else
    //    {
    //        hands.enabled = false;
    //    }
    //}
}

[Serializable]
public class JockboSet
{
    public string jockboName;
    public Sprite jockboImage;
}
