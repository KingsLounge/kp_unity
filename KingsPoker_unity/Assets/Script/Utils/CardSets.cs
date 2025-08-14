using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class CardSets : MonoBehaviour
{
    private struct CardData
    {
        public Sprite img;
        public string cards;

        public CardData(Sprite img, string cards)
        {
            this.img = img;
            this.cards = cards;
        }
    }

    private static List<List<CardData>> staticCardSets = new List<List<CardData>>();
    private static List<List<CardData>> staticMiniCardSets = new List<List<CardData>>();
    public Sprite[] cardSets;
    public Sprite[] miniCardSets;
    public Sprite backCard;

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < cardSets.Length; i++)
        {
            if (cardSets[i] == null)
                continue;
            List<CardData> datas = new List<CardData>();
            string[] temp = cardSets[i].name.Split('_');
            string[] temp2 = new string[temp.Length - 1];
            for (int j = 0; j < temp2.Length; j++)
            {
                temp2[j] = temp[j];
            }
            string itemName = string.Join("_", temp2);
            Sprite[] arr = Resources.LoadAll<Sprite>("Cards/" + itemName);
            for (int j = 0; j < arr.Length; j++)
            {
                string cards = IndexToCard(j);
                CardData item;
                if (cards == "**")
                {
                    item = new CardData(backCard, cards);
                }
                else
                {
                    item = new CardData(arr[j], cards);
                }

                datas.Add(item);
            }
            staticCardSets.Add(datas);
        }

        for (int i = 0; i < miniCardSets.Length; i++)
        {
            if (miniCardSets[i] == null)
            {
                staticMiniCardSets.Add(null);
                continue;
            }
            List<CardData> datas = new List<CardData>();
            string[] temp = miniCardSets[i].name.Split('_');
            string[] temp2 = new string[temp.Length - 1];
            for (int j = 0; j < temp2.Length; j++)
            {
                temp2[j] = temp[j];
            }
            string itemName = string.Join("_", temp2);
            Sprite[] arr = Resources.LoadAll<Sprite>("Cards/" + itemName);
            for (int j = 0; j < arr.Length; j++)
            {
                string cards = IndexToCard(j);
                CardData item;
                if (cards == "**")
                {
                    item = new CardData(backCard, cards);
                }
                else
                {
                    item = new CardData(arr[j], cards);
                }
                datas.Add(item);
            }
            staticMiniCardSets.Add(datas);
        }
    }

    private string IndexToCard(int index)
    {
        char[] shapes = new char[4] { 's', 'd', 'h', 'c' };
        char[] numbers = new char[13]
        {
            '2',
            '3',
            '4',
            '5',
            '6',
            '7',
            '8',
            '9',
            't',
            'j',
            'q',
            'k',
            'a'
        };
        int t = 0;
        for (int i = 0; i < shapes.Length; i++)
        {
            for (int j = 0; j < numbers.Length; j++)
            {
                if (t == index)
                {
                    return string.Concat(numbers[j], shapes[i]);
                }
                t++;
            }
        }
        index -= t;
        if (index == 0)
        {
            return "joker";
        }
        else if (index == 1)
        {
            return "**";
        }
        else if (index == 2)
        {
            return "rabbit";
        }
        else
        {
            return "";
        }
    }

    public static Sprite GetCard(string card, int cardSetNum = 0, bool isMiniCard = false)
    {
        List<List<CardData>> sets = !isMiniCard ? staticCardSets : staticMiniCardSets;
        if (cardSetNum < 0 || sets.Count <= cardSetNum)
        {
            Debug.Log("No Card Set This Index");
            return null;
        }
        else
        {
            return sets[cardSetNum].Find(item => item.cards == card).img;
        }
    }
}
