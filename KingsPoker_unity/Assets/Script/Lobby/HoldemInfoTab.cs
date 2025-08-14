using Newtonsoft.Json.Linq;
using PokerOdds;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HoldemInfoTab : InfoTab
{
    [SerializeField]
    protected Text totalGameCountText, buttonTotalgameText, maxWinChipText, oddsText, raiseText, checkText, foldText, callText, preflopText, flopText, turnText, riverText;
    [SerializeField]
    protected Image[] cardImages;
    [SerializeField]
    protected Image hands;
    [SerializeField]
    protected List<JockboSet> jockboSets;
    [SerializeField]
    

    public override void SetInfo(JObject data)
    {
        base.SetInfo(data);

        int win = 0;
        int flop = 0;
        int turn = 0;
        int river = 0;
        int preflop = 0;
        int lose = 0;
        int call = 0;
        int fold = 0;
        int check = 0;
        int raise = 0;
        string bestHand = "";
        int winMoney = 0;
        int loseMoney = 0;
        int MaxWin = 0;
        long BestValue = 0;
        if (data != null)
        {
            win = data["win"].ToObject<int>();
            flop = data["fold"]["flop"].ToObject<int>();
            turn = data["fold"]["turn"].ToObject<int>();
            river = data["fold"]["river"].ToObject<int>();
            preflop = data["fold"]["preflop"].ToObject<int>();
            lose = data["lose"].ToObject<int>();
            call = data["betting"]["call"].ToObject<int>();
            fold = data["betting"]["fold"].ToObject<int>();
            check = data["betting"]["check"].ToObject<int>();
            raise = data["betting"]["raise"].ToObject<int>();
            bestHand = data["bestHand"].ToObject<string>();
            winMoney = data["winMoney"].ToObject<int>();
            loseMoney = data["loseMoney"].ToObject<int>();
            MaxWin = data["maxWinMoney"].ToObject<int>();
            BestValue = data["bestHandValue"].ToObject<long>();
        }


        hands.enabled = true;
        totalGameCountText.text = string.Format("{0} 핸드", win + lose);
        buttonTotalgameText.text = (win + lose).ToString();
        maxWinChipText.text = string.Format("{0} 칩", winMoney);
        oddsText.text = string.Format("{0} %", win == 0 ? 0 : (100 * win / (win + lose)));
        var totalBetCount = raise + check + fold + call;
        raiseText.text = string.Format("{0} %", totalBetCount == 0 ? 0 : (100 * raise / totalBetCount));
        checkText.text = string.Format("{0} %", totalBetCount == 0 ? 0 : (100 * check / totalBetCount));
        callText.text = string.Format("{0} %", totalBetCount == 0 ? 0 : (100 * call / totalBetCount));
        foldText.text = string.Format("{0} %", totalBetCount == 0 ? 0 : (100 * fold / totalBetCount));


        preflopText.text = string.Format("{0} %", fold == 0 ? 0 : (100 * preflop / fold));
        flopText.text = string.Format("{0} %", fold == 0 ? 0 : (100 * flop / fold));
        turnText.text = string.Format("{0} %", fold == 0 ? 0 : (100 * turn / fold));
        riverText.text = string.Format("{0} %", fold == 0 ? 0 : (100 * river / fold));

        if (!string.IsNullOrEmpty(bestHand))
        {
            Console.Log(bestHand);
            var jokbov = CalculateCardsValue.calc(bestHand);
            var jokbo = CalculateCardsValue.jokboName(jokbov);
            var cards = bestHand.Split(',');
            for (int i = 0; i < cardImages.Length; i++)
            {
                if (i < cards.Length)
                {
                    cardImages[cardImages.Length - i - 1].sprite = CardSets.GetCard(cards[i]);
                }
                else
                {
                    cardImages[cardImages.Length - i - 1].sprite = CardSets.GetCard("**");
                }
            }
            for (int i = 0; i < jockboSets.Count; i++)
            {
                if (jockboSets[i].jockboName == jokbo[0])
                {
                    hands.sprite = jockboSets[i].jockboImage;
                }
            }
            Console.Log(jokbo[0]);
        }
        else
        {
            hands.enabled = false;
        }
        for (int i = 0; i < totalGameTabText.Length; i++)
        {
            if (totalGameTabText[i])
            {
                totalGameTabText[i].text = (win + lose).ToString();
            }
        }
    }

    public override void SetTotalInfo(JObject data1, JObject data2)
    {
        int win = 0;
        int flop = 0;
        int turn = 0;
        int river = 0;
        int preflop = 0;
        int lose = 0;
        int call = 0;
        int fold = 0;
        int check = 0;
        int raise = 0;
        string bestHand = "";
        int winMoney = 0;
        int loseMoney = 0;
        int MaxWin = 0;
        long BestValue = 0;
        if(data1 != null)
        {
            win = data1["win"].ToObject<int>();
            flop = data1["fold"]["flop"].ToObject<int>();
            turn = data1["fold"]["turn"].ToObject<int>();
            river = data1["fold"]["river"].ToObject<int>();
            preflop = data1["fold"]["preflop"].ToObject<int>();
            lose = data1["lose"].ToObject<int>();
            call = data1["betting"]["call"].ToObject<int>();
            fold = data1["betting"]["fold"].ToObject<int>();
            check = data1["betting"]["check"].ToObject<int>();
            raise = data1["betting"]["raise"].ToObject<int>();
            
            winMoney = data1["winMoney"].ToObject<int>();
            loseMoney = data1["loseMoney"].ToObject<int>();
            MaxWin = data1["maxWinMoney"].ToObject<int>();
            bestHand = data1["bestHand"].ToObject<string>();
            BestValue = data1["bestHandValue"].ToObject<long>();
        }

        if(data2 != null)
        {
            win += data2["win"].ToObject<int>();
            flop += data2["fold"]["flop"].ToObject<int>();
            turn += data2["fold"]["turn"].ToObject<int>();
            river += data2["fold"]["river"].ToObject<int>();
            preflop += data2["fold"]["preflop"].ToObject<int>();
            lose += data2["lose"].ToObject<int>();
            call += data2["betting"]["call"].ToObject<int>();
            fold += data2["betting"]["fold"].ToObject<int>();
            check += data2["betting"]["check"].ToObject<int>();
            raise += data2["betting"]["raise"].ToObject<int>();
            
            winMoney += data2["winMoney"].ToObject<int>();
            loseMoney += data2["loseMoney"].ToObject<int>();
            MaxWin += data2["maxWinMoney"].ToObject<int>();
            if(BestValue < data2["bestHandValue"].ToObject<long>())
            {
                BestValue = data2["bestHandValue"].ToObject<long>();
                bestHand = data2["bestHand"].ToObject<string>();
            }
        }

        hands.enabled = true;
        totalGameCountText.text = string.Format("{0} 핸드", win + lose);
        buttonTotalgameText.text = (win + lose).ToString();
        maxWinChipText.text = string.Format("{0} 칩", winMoney);
        oddsText.text = string.Format("{0} %", win == 0 ? 0 : (100 * win / (win + lose)));
        var totalBetCount = raise + check + fold + call;
        raiseText.text = string.Format("{0} %", totalBetCount == 0 ? 0 : (100 * raise / totalBetCount));
        checkText.text = string.Format("{0} %", totalBetCount == 0 ? 0 : (100 * check / totalBetCount));
        callText.text = string.Format("{0} %", totalBetCount == 0 ? 0 : (100 * call / totalBetCount));
        foldText.text = string.Format("{0} %", totalBetCount == 0 ? 0 : (100 * fold / totalBetCount));


        preflopText.text = string.Format("{0} %", fold == 0 ? 0 : (100 * preflop / fold));
        flopText.text = string.Format("{0} %", fold == 0 ? 0 : (100 * flop / fold));
        turnText.text = string.Format("{0} %", fold == 0 ? 0 : (100 * turn / fold));
        riverText.text = string.Format("{0} %", fold == 0 ? 0 : (100 * river / fold));

        if (!string.IsNullOrEmpty(bestHand))
        {
            Console.Log(bestHand);
            var jokbov = CalculateCardsValue.calc(bestHand);
            var jokbo = CalculateCardsValue.jokboName(jokbov);
            var cards = bestHand.Split(',');
            for (int i = 0; i < cardImages.Length; i++)
            {
                if (i < cards.Length)
                {
                    cardImages[cardImages.Length - i - 1].sprite = CardSets.GetCard(cards[i]);
                }
                else
                {
                    cardImages[cardImages.Length - i - 1].sprite = CardSets.GetCard("**");
                }
            }
            for (int i = 0; i < jockboSets.Count; i++)
            {
                if (jockboSets[i].jockboName == jokbo[0])
                {
                    hands.sprite = jockboSets[i].jockboImage;
                }
            }
            Console.Log(jokbo[0]);
        }
        else
        {
            hands.enabled = false;
        }
        for (int i = 0; i < totalGameTabText.Length; i++)
        {
            if (totalGameTabText[i])
            {
                totalGameTabText[i].text = (win + lose).ToString();
            }
        }
    }



}
