using System.Collections;
using System.Collections.Generic;
using PokerOdds;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;

public class HoldemCalcTestPanel : MonoBehaviour
{
    [SerializeField]
    InputField input;
    private List<string> carddeck = new List<string>{};
    List<string> calcString = new List<string>();
    private void Start()
    {
        Init();
    }
    public void Init()
    {
        List<string> numList = new List<string> { "a", "2", "3", "4", "5", "6", "7", "8", "9", "t", "j", "q", "k" };
        List<string> shape = new List<string> { "c", "h", "s", "d" };
        for (int i = 0; i < numList.Count; i++)
        {
            for(int j = 0; j<shape.Count; j++)
            {
                carddeck.Add($"{numList[i]}{shape[j]}");
            }
        }
        //StartCoroutine(CalcWriteText());
    }
    public void AllCombinationCards(int cardCount)
    {
        StartCoroutine(AllCombinationCardsCo(string.Empty, cardCount, cardCount));
    }

    public IEnumerator CalcWriteText()
    {

        yield return StartCoroutine(AllCombinationCardsCo(string.Empty, 7, 7));
        
        File.WriteAllLines($"{Application.persistentDataPath}/holdemAllCalc.txt", calcString.ToArray());
#if UNITY_EDITOR
        UnityEditor.EditorUtility.ClearProgressBar();
#endif
    }

    public IEnumerator AllCombinationCardsCo(string cardString, int cardCount, int maxCardCount, int curIdx = 0)
    {

        if (cardCount < 1)
            yield break;
        for(int i = curIdx; i <= carddeck.Count - (cardCount); i++)
        {
            string curCards = string.Empty;
            if (cardCount == maxCardCount)
            {
                curCards = carddeck[i];
#if UNITY_EDITOR
                UnityEditor.EditorUtility.DisplayCancelableProgressBar("진행중", curCards, (float)i / (float)carddeck.Count);
#endif
            }
            else
            {
                curCards = $"{cardString} {carddeck[i]}";
            }
            
            if(cardCount == 1)
            {
                TestJockbo(curCards);
            }
            else
            {
                AllCombinationCards(curCards, cardCount - 1, maxCardCount, i + 1);
            }
            yield return 0;
        }

    }
    public void AllCombinationCards(string cardString, int cardCount, int maxCardCount, int curIdx = 0)
    {

        if (cardCount < 1)
            return;
        for (int i = curIdx; i <= carddeck.Count - (cardCount); i++)
        {
            string curCards = string.Empty;
            if (cardCount == maxCardCount)
            {
                curCards = carddeck[i];
#if UNITY_EDITOR
                UnityEditor.EditorUtility.DisplayCancelableProgressBar("진행중", curCards, (float)i / (float)carddeck.Count);
#endif
            }
            else
            {
                curCards = $"{cardString} {carddeck[i]}";
            }

            if (cardCount == 1)
            {
                TestJockbo(curCards);
            }
            else
            {
                AllCombinationCards(curCards, cardCount - 1, maxCardCount, i + 1);
            }

        }

    }
    public void TestJockbo(string cards)
    {
        var jokbov = CalculateCardsValue.calc(cards);
        var cardsString = CalculateCardsValue.sort(cards);
        var cardStringList = CalculateCardsValue.makeCardsString(cardsString);
        var jokboName = CalculateCardsValue.jokboName(jokbov);
        var jokboCards = CalculateCardsValue.makeMadeCardsOnly(cardStringList, jokbov);
        Debug.Log($"Cards : <color=red>{cards}</color>jockboCard : <color=yellow>{ jokboCards}</color>\njokbo Name : <color=lime>{string.Join(",",jokboName)}</color> ");
        calcString.Add( $"{cardStringList},{ jokboCards},{ jokboName[0]}");
       
    }
    public void OnClickTestButton()
    {
        var jokbov = CalculateCardsValue.calc(input.text);
        var cardsString = CalculateCardsValue.sort(input.text);
        var cardStringList = CalculateCardsValue.makeCardsString(cardsString);
        var jokboName = CalculateCardsValue.jokboName(jokbov);
        var jokboCards = CalculateCardsValue.makeMadeCardsOnly(cardStringList, jokbov);
        Debug.Log($"Calclulate Card Sort : <color=red>{cardStringList}</color>  jockboCard : <color=yellow>{ jokboCards}</color>  jokbo Name : <color=yellow>{String.Join(" ",jokboName)}</color>");
        
    }
    
}
