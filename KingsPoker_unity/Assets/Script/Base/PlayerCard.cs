using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class PlayerCard : MonoBehaviour
{
    public List<Card> cards = new List<Card>();

    public List<RectTransform> showPos = new List<RectTransform>();

    [SerializeField]
    private CardAnimationManager cardMoveer;

    [SerializeField]
    private float showMoveTime = 0.3f;

    // Start is called before the first frame update
    void Start() { }

    // Update is called once per frame
    void Update() { }

    public virtual void SetCard(string[] cardArr = null)
    {
        if (cardArr == null)
        {
            for (int i = 0; i < cards.Count; i++)
            {
                cards[i].gameObject.SetActive(true);
                cards[i].SetCard("**");
            }
        }
        else
        {
            for (int i = 0; i < cardArr.Length; i++)
            {
                cards[i].gameObject.SetActive(true);
                cards[i].SetCard(cardArr[i]);
            }
        }
    }

    public List<string> GetCard()
    {
        List<string> result = new List<string>();
        for (int i = 0; i < cards.Count; i++)
        {
            string data = cards[i].GetCardData();
            if (data != "**")
            {
                result.Add(data);
            }
        }
        return result;
    }

    public void CardHighlight(string[] list, int type)
    {
        for (int i = 0; i < cards.Count; i++)
        {
            //cards[i].Highlight(System.Array.IndexOf(list, cards[i].GetCardData()) != -1, type);
            cards[i].gray = System.Array.IndexOf(list, cards[i].GetCardData()) == -1;
            cards[i].Highlight(!cards[i].gray);
        }
    }

    public void CardHighlightOff(bool onlyHighlight = false, bool direct = false)
    {
        for (int i = 0; i < cards.Count; i++)
        {
            cards[i].Highlight(false, 0, direct);
            if (!onlyHighlight)
                cards[i].gray = false;
        }
    }

    public virtual void SetCard(int index, string card)
    {
        cards[index].gameObject.SetActive(true);
        cards[index].SetCard(card);
    }

    public virtual void ForceBack()
    {
        CardHighlightOff(false, true);
        for (int i = 0; i < cards.Count; i++)
        {
            cards[i].gameObject.SetActive(false);
            cards[i].transform.localPosition = Vector3.zero;
            cards[i].transform.localRotation = Quaternion.identity;
            cards[i].GetComponent<RectTransform>().sizeDelta = cards[i]
                .transform.parent.GetComponent<RectTransform>()
                .sizeDelta;
            cards[i].animator.enabled = true;
        }
    }

    public virtual void OpenCard(string[] cards, bool skipAni = false) { }

    public void ShowCard(string[] cards)
    {
        for (int i = 0; i < cards.Length; i++)
        {
            var idx = this.cards.FindIndex((obj) => obj.GetCardData() == cards[i]);
            if (idx >= 0 && idx < this.cards.Count)
            {
                cardMoveer.CardMove(
                    this.cards[idx].GetComponent<RectTransform>(),
                    this.cards[idx].GetComponent<RectTransform>(),
                    showPos[idx],
                    0,
                    showMoveTime
                );
                this.cards[idx].animator.enabled = false;
            }
        }
    }
}
