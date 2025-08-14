using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using PokerOdds;
using HoldemHand;

public class HandsItem
{
    public string[] gids { get; set; }
    public string[] hands { get; set; }
    public string comm { get; set; }
}

public class HandValuesItem
{
    public string tabname;
    public string[] results { get; set; }
}

public struct HandsInfo
{
    public string title;
    public string desc;
    public string[] best_cards;

    public HandsInfo(string title, string desc, string[] best_cards)
    {
        this.title = title;
        this.desc = desc;
        this.best_cards = best_cards;
    }
}

public struct PokerRatio
{
    public string gid;
    public float win;
    public float tie;
}

public class PokerOddsManager : MonoBehaviour
{
    private void Start()
    {

        //Console.SpecialLog("cards = " + String.Join(",",MyHandsInfo("7s 8h", "8s, 7h, td").best_cards)); // , "8s 8h 9s"

        //HandsItem hi = new HandsItem();
        //hi.hands = new string[3] { "8c 7h", "jh 9c", "3c kd" };
        //hi.comm = "7d 3d th";

        //PokerRatio[] pr = PostHandsItem(hi);

        //for(int i = 0; i < pr.Length; i++)
        //{
        //    Debug.Log(string.Format("Win: {0} / Tie: {1}", pr[i].win, pr[i].tie));
        //}
    }

    public static HandsInfo MyHandsInfo(string hole, string comm = null)
    {
        string cards;
        if(comm == null)
        {
            cards = hole;
        }
        else if(string.IsNullOrEmpty(hole))
        {
            cards = comm;
        }
        else
        {
            cards = string.Format("{0} {1}", hole, comm);
        }


        long jokbov = CalculateCardsValue.calc(cards);
        string[] jokbo = CalculateCardsValue.jokboName(jokbov);
        string cardsorted = CalculateCardsValue.makeCardsString(CalculateCardsValue.sort(cards));
        string cardsortedonly = CalculateCardsValue.makeMadeCardsOnly(cardsorted, jokbov);


        string desc = jokbo[1];
        for (int i = 2; i < jokbo.Length; i++)
        {
            if(jokbo[i] != "")
            {
                desc += string.Format(", {0}", jokbo[i]);
            }
        }

        Debug.Log(desc);

        return new HandsInfo(LocalizeManager.GetLocalString(jokbo[0]), desc, cardsortedonly.Split(' '));
        //return string.Format("{0} {1}", jokbo[0], jokbo[1]);
    }

    public static PokerRatio[] PostHandsItem(HandsItem item)
    {
        if (item.hands == null)
        {
            Debug.LogError("Bad Request : Hands is NULL");
            return null;
        }

        if (item.hands.Length <= 1)
        {
            Debug.LogError("Bad Request : Hands is too little");
            return null;
        }

        if (item.hands.Length > 10)
        {
            Debug.LogError("Bad Request : Hands is too much");
            return null;
        }

        if (item.comm == null)
        {
            item.comm = "";
        }

        int count = 0;
        string[] holecards;
        string commcards = "";
        string deadcards = "";

        // 중간에 빠진 slot 이 있나요 ?
        // is there an empty slot ?
        bool[] hand_exist = new bool[10];
        for (int i = 0; i < item.hands.Length; i++)
        {
            if (string.IsNullOrEmpty(item.hands[i]) == false)
            {
                count++;
            }
        }

        holecards = new string[count];
        count = 0;
        for (int i = 0; i < item.hands.Length; i++)
        {
            if (string.IsNullOrEmpty(item.hands[i]))
            {
                hand_exist[i] = false;
            }
            else
            {
                hand_exist[i] = true;
                holecards[count] = item.hands[i];
                count++;
            }
        }

        long[] wins = new long[count];
        long[] losses = new long[count];
        float[] ties = new float[count];
        long totalhands = 0;

        double total_analy = 0.0;

        try
        {
            bool ok;
            ok = string.IsNullOrEmpty(item.comm); // item.comm 이  null이거나 "" 인건 괜찮아.

            if (ok == false)
            {
                ok = Hand.ValidateHand(item.comm);
                if (ok == false)
                {
                    Debug.LogError("Bad Request : Wrong comm cards " + item.comm);
                    return null;
                }
            }

            commcards = item.comm;

            for (int i = 0; i < count; i++)
            {
                ulong t = Hand.ParseHand(holecards[i], "", ref count);
                if (count != 2)
                {
                    Debug.LogError("Bad Request : Wrong hole card. " + item.hands[i]);
                    return null;
                }
                ok = Hand.ValidateHand(holecards[i]);
                if (ok == false)
                {
                    Debug.LogError("Bad Request : Wrong hole card.. " + item.hands[i]);
                    return null;
                }
            }

        }
        catch (Exception e)
        {
            Console.SpecialLog("comm items : " + item.comm);
            Console.SpecialLog("hands items : " + string.Join(",",item.hands));
            Console.Error(e.Source);
            Debug.LogError("Bad Request : Wrong cards... " + e.Message);
            return null;
        }

        HandValuesItem v = new HandValuesItem { };
        v.tabname = "holecards, commcards, odds, tie, loss, value, handname0, handname1, sorted";
        v.results = new string[item.hands.Length];

        PokerRatio[] pokerRatios = new PokerRatio[item.hands.Length];
        try
        {

            Hand.HandOdds(holecards, commcards, deadcards, wins, ties, losses, ref totalhands);
            if (totalhands != 0)
            {
                count = 0;
                for (int i = 0; i < item.hands.Length; i++)
                {
                    if (hand_exist[i] == true)
                    {
                        float odd;//float odd;          // 우승 확률.
                        float tie;          // 타이.
                        long loss;         //  .
                        long jokbov;        // 현재까지의 카드 가치.
                        string[] jokbo;     // 족보 이름.
                        string cardsorted;  // 족보대로 카드를 정리.
                        string cardsortedonly;

                        string temp_cards = holecards[count];
                        if (commcards.Length > 0)
                        {
                            temp_cards += " " + commcards;
                        }

                        odd = (float)Math.Round(((double)wins[count] / (double)totalhands * 100), 2);//(float)((double)wins[count] / (double)totalhands * 100);
                        tie = (float)Math.Round(((double)ties[count] / (double)totalhands * 100), 2);//ties[count];
                        loss = losses[count];
                        jokbov = CalculateCardsValue.calc(temp_cards);
                        jokbo = CalculateCardsValue.jokboName(jokbov);
                        cardsorted = CalculateCardsValue.makeCardsString(CalculateCardsValue.sort(temp_cards));
                        cardsortedonly = CalculateCardsValue.makeMadeCardsOnly(cardsorted, jokbov);

                        System.Diagnostics.Debug.WriteLine(temp_cards);

                        v.results[i] = holecards[count] + "," + commcards + "," + odd + "," + tie + "," + loss + "," + jokbov + "," + LocalizeManager.GetLocalString(jokbo[0]) + "," + jokbo[1] + "," + cardsortedonly;
                        pokerRatios[i].gid = item.gids[i];
                        pokerRatios[i].win = odd;
                        pokerRatios[i].tie = tie;
                        count++;
                    }
                    else
                    {
                        v.results[i] = ",,,,,,,,";
                        pokerRatios[i].win = 0f;
                        pokerRatios[i].tie = 0f;
                    }
                }
                return pokerRatios;
            }
            else
            {
                Debug.LogError("Bad Request : Wrong cards, Can not calculate..");
                return null;
            }

        }
        catch (Exception e)
        {
            Debug.Log(e);
            Console.Error("Bad Request : " + e.Message);
            return null;
        }

        Debug.Log(v.tabname);
        for(int i = 0; i < v.results.Length; i++)
        {
            Debug.Log(v.results[i]);
        }
        
    }
}
