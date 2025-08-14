using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BadugiInfoTab : InfoTab
{
    [SerializeField]
    protected Text dieText, winText, loseText, golfText, secondText, thirdText, madeText, baseText, base2Text, topText;
    [SerializeField]
    protected Image[] cardImages;
    [SerializeField]
    protected Image hands;
    public override void SetInfo(JObject data)
    {
        base.SetInfo(data);
        long die = 0;
        long win = 0;
        long lose = 0;
        string bestHand = "";

        long wtop = 0;
        long wbase = 0;
        long wbase2 = 0;
        long wgolf = 0;
        long wsecond = 0;
        long wthrid = 0;
        long wmade5 = 0;
        long wmade6 = 0;
        long wmade7 = 0;
        long wmade8 = 0;
        long wmade9 = 0;
        long wmade10 = 0;
        long wmadeJ = 0;
        long wmadeQ = 0;
        long wmadeK = 0;

        long ltop = 0;
        long lbase = 0;
        long lbase2 = 0;
        long lgolf = 0;
        long lsecond = 0;
        long lthrid = 0;
        long lmade5 = 0;
        long lmade6 = 0;
        long lmade7 = 0;
        long lmade8 = 0;
        long lmade9 = 0;
        long lmade10 = 0;
        long lmadeJ = 0;
        long lmadeQ = 0;
        long lmadeK = 0;

        if (data != null)
        {
            die = data["die"].ToObject<long>();
            win = data["win"].ToObject<long>();
            lose = data["lose"].ToObject<long>();
            bestHand = data["bestHand"].ToObject<string>();

            JToken winData = data["winHand"];
            wtop = winData["top"].ToObject<long>();
            wbase = winData["base"].ToObject<long>();
            wbase2 = winData["base2"].ToObject<long>();
            wgolf = winData["golf"].ToObject<long>();
            wsecond = winData["second"].ToObject<long>();
            wthrid = winData["thrid"].ToObject<long>();
            wmade5 = winData["made5"].ToObject<long>();
            wmade6 = winData["made6"].ToObject<long>();
            wmade7 = winData["made7"].ToObject<long>();
            wmade8 = winData["made8"].ToObject<long>();
            wmade9 = winData["made9"].ToObject<long>();
            wmade10 = winData["made10"].ToObject<long>();
            wmadeJ = winData["madeJ"].ToObject<long>();
            wmadeQ = winData["madeQ"].ToObject<long>();
            wmadeK = winData["madeK"].ToObject<long>();
            JToken loseData = data["loseHand"];
            ltop = loseData["top"].ToObject<long>();
            lbase = loseData["base"].ToObject<long>();
            lbase2 = loseData["base2"].ToObject<long>();
            lgolf = loseData["golf"].ToObject<long>();
            lsecond = loseData["second"].ToObject<long>();
            lthrid = loseData["thrid"].ToObject<long>();
            lmade5 = loseData["made5"].ToObject<long>();
            lmade6 = loseData["made6"].ToObject<long>();
            lmade7 = loseData["made7"].ToObject<long>();
            lmade8 = loseData["made8"].ToObject<long>();
            lmade9 = loseData["made9"].ToObject<long>();
            lmade10 = loseData["made10"].ToObject<long>();
            lmadeJ = loseData["madeJ"].ToObject<long>();
            lmadeQ = loseData["madeQ"].ToObject<long>();
            lmadeK = loseData["madeK"].ToObject<long>();
        }
        long wmade = wmade5 + wmade6 + wmade7 + wmade8 + wmade9 + wmade10 + wmadeJ + wmadeQ + wmadeK;
        long lmade = lmade5 + lmade6 + lmade7 + lmade8 + lmade9 + lmade10 + lmadeJ + lmadeQ + lmadeK;
        dieText.text = die.ToString();
        winText.text = win.ToString();
        loseText.text = lose.ToString();
        golfText.text = string.Format("{0}승 {1}패 ({2:0}%)", wgolf, lgolf, (wgolf + lgolf) > 0 ? (wgolf / (wgolf + lgolf)) : 0);
        secondText.text = string.Format("{0}승 {1}패 ({2:0}%)", wsecond, lsecond, (wsecond + lsecond) > 0 ? (wsecond / (wsecond + lsecond)) : 0);
        thirdText.text = string.Format("{0}승 {1}패 ({2:0}%)", wthrid, lthrid, (wthrid + lthrid) > 0 ? (wthrid / (wthrid + lthrid)) : 0);
        madeText.text = string.Format("{0}승 {1}패 ({2:0}%)", wmade, lmade, (wmade + lmade) > 0 ? (wmade / (wmade + lmade)) : 0);
        baseText.text = string.Format("{0}승 {1}패 ({2:0}%)", wbase, lbase, (wbase + lbase) > 0 ? (wbase / (wbase + lbase)) : 0);
        base2Text.text = string.Format("{0}승 {1}패 ({2:0}%)", wbase2, lbase2, (wbase2 + lbase2) > 0 ? (wbase2 / (wbase2 + lbase2)) : 0);
        topText.text = string.Format("{0}승 {1}패 ({2:0}%)", wtop, ltop, (wtop + ltop) > 0 ? (wtop / (wtop + ltop)) : 0);

        if (!string.IsNullOrEmpty(bestHand))
        {
            Console.Log(bestHand);
            
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
        }
        else
        {
            hands.enabled = false;
        }
        for (int i = 0; i < totalGameTabText.Length; i++)
        {
            if (totalGameTabText[i])
            {
                totalGameTabText[i].text = (win + lose + die).ToString();
            }
        }
        
    }

}
