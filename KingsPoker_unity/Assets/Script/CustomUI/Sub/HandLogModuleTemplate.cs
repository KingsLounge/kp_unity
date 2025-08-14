using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json.Linq;
using System;

public class HandLogModuleTemplate : MonoBehaviour
{
    public Text dateText;
    public List<Image> holeCards = new List<Image>();
    public Text winnerText;
    public Text potText;
    public Text winLossText;
    public int gn = -1;
    private CustomUIHandLogModule module;

    private void Awake()
    {

    }

    public void SetData(CustomUIHandLogModule module, JObject d)
    {
        this.module = module;
        gn = (int)d["gn"];
        DateTime time = DateTimeParser.Parse(d["createdAt"].ToString());

        



        // string dateString = string.Format("{0}/{1:d2}/{2:d2} {3:d2}:{4:d2}", time.Year % 100, time.Month, time.Day, time.Hour, time.Minute);
        dateText.text = time.ToLocalTime().ToString("y/MM/dd HH:mm");
        string[] cardsArr = d["hands"].ToString().SplitAndTrimAll(',');
        int cardSetNumber = CardTextureSetter.Instance != null ? CardTextureSetter.Instance.cardSetIndex : 0;
        for(int i = 0; i < holeCards.Count;i++)
        {
            bool active = i < cardsArr.Length && !string.IsNullOrEmpty(cardsArr[i]);
            holeCards[i].gameObject.SetActive(active);
            if(active)
            {
                holeCards[i].sprite = CardSets.GetCard(cardsArr[i], cardSetNumber, true);
            }
        }

        winnerText.text = d["winner"] != null ? d["winner"].ToString() : "";
        potText.text = MoneyToString.Converting((long)d["pot"]);
        try
        {
            
            winLossText.text = MoneyToString.Converting((long)d["winloss"]);
        }
        catch(Exception e)
        {
            Debug.LogError(e);
            winLossText.text = "-";
        }
    }

    public void OnClickThis()
    {
        CustomUIHandLogRoot root = (module.root as CustomUIHandLogRoot);
        HandLogDetailViewer.Instance.SetGame(root.isCafeHandLog,gn);
    }
}
