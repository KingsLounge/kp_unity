using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PokerWinRatioTester : MonoBehaviour
{
    public List<string> hands;
    public List<string> gids;
    public string commCards;
    public HoldemCalcTestPanel panel;

    private PokerRatio[] CalcPokerRatio()
    {
        HandsItem hi = new HandsItem();
        hi.gids = new string[hands.Count];
        hi.hands = new string[hands.Count];
        for (int i = 0; i < hands.Count; i++)
        {
            hi.gids[i] = gids[i];
            hi.hands[i] = hands[i];
            panel?.TestJockbo($"{commCards} {hi.hands[i]}");
        }
        hi.comm = commCards;

        return PokerOddsManager.PostHandsItem(hi);
    }


    public void ShowPokerOddsRatio()
    {
        PokerRatio[] pr = CalcPokerRatio();
        Debug.Log(pr.ToString());
        if (pr != null)
        {
            string resultString = "Win Ratio\n";
            for (int j = 0; j < pr.Length; j++)
            {
                
                resultString += string.Format("{0} / win: {1} / tie: {2}\n", pr[j].gid, pr[j].win, pr[j].tie);
            }
            Debug.Log(resultString);
        }
    }
}
