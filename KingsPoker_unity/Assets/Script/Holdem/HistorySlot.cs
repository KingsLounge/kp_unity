using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json.Linq;
using PokerOdds;

public class HistorySlot : MonoBehaviour
{
    public List<Image> cardImageList;
    public RawImage profileImage;
    public Text winText;

    public Color winColor;
    public Color loseColor;
    public Color foldColor;

    public Text nickText;
    public Text handText;

    public async void SetHistory(long win, bool fold, string nick, string card, string[] cc, string uid,string url = null)
    {
        if(win > 0)
        {
            winText.text = MoneyToString.Converting(win);
            winText.color = winColor;
        }
        else if(fold)
        {
            winText.text = "폴드";
            winText.color = foldColor;
        }
        else
        {
            winText.text = "패배";
            winText.color = loseColor;
        }

        nickText.text = nick;
        var cards = card.Split(',');
        for(int i = 0; i < 2; i++)
        {
            if(i < cards.Length)
            {
                cardImageList[i].sprite = CardSets.GetCard(cards[i]);
            }
            else
            {
                cardImageList[i].sprite = CardSets.GetCard("**");
            }
        }
        for(int i = 2; i < cardImageList.Count; i++)
        {
            if(i-2 < cc.Length)
            {
                if(!string.IsNullOrEmpty(cc[i-2]))
                {
                    cardImageList[i].sprite = CardSets.GetCard(cc[i-2]);
                }
                else
                {
                    cardImageList[i].sprite = CardSets.GetCard("**");
                }
            }
        }
        if(!string.IsNullOrEmpty(url))
        {
            SetProfile(await ImageDatabase.LoadImageTexture(url, Application.persistentDataPath + "/profileImg", uid));
        }
        else
        {
           //SetProfile(ProfileImageManager.LoadTextureFromFile(MyStatus.uid));
        }
        string cardset = "";
        for(int i = 0; i < cards.Length; i++)
        {
            if(!string.IsNullOrEmpty(cards[i]) && cards[i] != "**")
            {
                cardset += cards[i];
            }
        }
        for(int i = 0; i < cc.Length; i++)
        {
            if(!string.IsNullOrEmpty(cc[i]) && cc[i] != "**")
            {
                cardset += cc[i];
            }
        }
        
        var jokbov = CalculateCardsValue.calc(cardset);
        var jokbo = CalculateCardsValue.jokboName(jokbov);
        handText.text = LocalizeManager.GetLocalString(jokbo[0]);        
    }
    private void SetProfile(Texture2D texture)
    {
        profileImage.texture = texture;
        profileImage.enabled = true;
    }
}
