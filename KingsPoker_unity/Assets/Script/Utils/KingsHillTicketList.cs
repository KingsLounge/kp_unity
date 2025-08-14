using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;

public class KingsHillTicketList : MonoBehaviour
{
    [SerializeField]
    private Transform content;
    [SerializeField]
    private GameObject ticketItemPrefab;

    [SerializeField]
    private Transform ticketAmountParent;
    [SerializeField]
    private GameObject ticketAmountItemPrefab;
    
    private List<KingshillTicketItem> ticketItems = new List<KingshillTicketItem>();
    private List<KingshillTicketAmountItem> ticketAmountItems = new List<KingshillTicketAmountItem>();
    public void OnEnable()
    {
        SetTicketList();
        SetTicketAmountList();
    }

    public async void SetTicketList()
    {
        var loungeData = MyStatus.loungeData;
        if(loungeData != null)
        {
           
            var keys = await KingshillInfo.GetTicketList();
           
            for(int i = 0; i < keys.Count; i++)
            {
                KingshillTicketItem ticketitem;
                if(i < ticketItems.Count)
                {
                    ticketitem = ticketItems[i];
                }
                else
                {
                    ticketitem = Instantiate(ticketItemPrefab, content).GetComponent<KingshillTicketItem>();
                    ticketItems.Add(ticketitem);
                }
                ticketitem.SetData(keys[i], loungeData.ValueOrDefault<long>(keys[i], 0));
                ticketitem.gameObject.SetActive(true);
            }
            for(int i = keys.Count; i < ticketItems.Count; i++)
            {
                ticketItems[i].gameObject.SetActive(false);
            }
        }
    }

    public async void SetTicketAmountList()
    {
        var ticketPriceData = await KingshillInfo.GetTicketPriceData();
        if(ticketPriceData != null)
        {
           
            for(int i = 0; i < ticketPriceData.Count; i++)
            {
                var ticketPrice = ticketPriceData[i] as JObject;
                var ticketGbn = ticketPrice.ValueOrDefault<string>("ticketGbn", "");
                var point = ticketPrice.ValueOrDefault<long>("point", 0);
                KingshillTicketAmountItem ticketAmountItem;
                if(i < ticketAmountItems.Count)
                {
                    ticketAmountItem = ticketAmountItems[i];
                }
                else
                {
                    ticketAmountItem = Instantiate(ticketAmountItemPrefab, ticketAmountParent).GetComponent<KingshillTicketAmountItem>();
                    ticketAmountItems.Add(ticketAmountItem);
                }
                ticketAmountItem.SetData(ticketGbn, point);
                ticketAmountItem.gameObject.SetActive(true);
            }
            for(int i = ticketAmountItems.Count; i < ticketPriceData.Count; i++)
            {
                ticketAmountItems[i].gameObject.SetActive(false);
            }
        }
    }
}
