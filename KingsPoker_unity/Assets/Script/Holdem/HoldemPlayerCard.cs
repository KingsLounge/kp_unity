using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HoldemPlayerCard : PlayerCard
{
    public GameObject handRankImage;
    public Text handRankTitleText;
    public Text handRankDescText;
    public List<Card> resultCards = new List<Card>();

    public void SetHandRank(string hole, string comm = null)
    {
        if (handRankImage != null && handRankTitleText != null && handRankDescText != null)
        {
            //HandsInfo handsInfo = PokerOddsManager.MyHandsInfo(hole, comm);
            //handRankTitleText.text = string.Format("{0} {1}", handsInfo.title, handsInfo.desc.Split(',')[0]);
            //handRankDescText.text = handsInfo.desc;
            //handRankImage.SetActive(true);
        }
        else if (handRankTitleText)
        {
            //HandsInfo handsInfo = PokerOddsManager.MyHandsInfo(hole, comm);
            //handRankTitleText.text = string.Format("{0} {1}", handsInfo.title, handsInfo.desc.Split(',')[0]);
        }
    }

    public void OnDisable()
    {
        if (resultCards.Count > 0)
        {
            resultCards[0].transform.parent.gameObject.SetActive(false);
        }
    }

    public void OnEnable()
    {
        if (resultCards.Count > 0)
        {
            resultCards[0].transform.parent.gameObject.SetActive(true);
        }
    }

    public override void ForceBack()
    {
        base.ForceBack();

        if (handRankImage != null)
        {
            handRankImage.SetActive(false);
        }

        for (int i = 0; i < resultCards.Count; i++)
        {
            // resultCards[i].gameObject.SetActive(false);
        }
    }

    public override void OpenCard(string[] cards, bool skipAni = false)
    {
        base.OpenCard(cards);

        for (int i = 0; i < this.cards.Count; i++)
        {
            this.cards[i].gameObject.SetActive(false);
        }
        for (int i = 0; i < resultCards.Count; i++)
        {
            resultCards[i].gameObject.SetActive(false);
        }
        for (int i = 0; i < cards.Length; i++)
        {
            if (string.IsNullOrEmpty(cards[i]))
                continue;
            resultCards[i].gameObject.SetActive(true);
            resultCards[i].SetCard(cards[i], false);
            resultCards[i].Flip(skipAni);
        }
    }
}
