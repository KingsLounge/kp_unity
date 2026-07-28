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

    [SerializeField]
    private KingshillKpChangeItem kpChangeItem; // KP -> 칩 전환 UI (티켓 교환 표 대체)

    private List<KingshillTicketItem> ticketItems = new List<KingshillTicketItem>();
    private List<KingshillTicketAmountItem> ticketAmountItems = new List<KingshillTicketAmountItem>();
    public void OnEnable()
    {
        SetTicketList();
        // 티켓 -> 칩 교환은 KP -> 칩 교환으로 대체됨 (SetTicketAmountList 미호출)
        if (ticketAmountParent != null)
        {
            ticketAmountParent.gameObject.SetActive(false);
        }
        if (kpChangeItem != null)
        {
            kpChangeItem.gameObject.SetActive(true);
            kpChangeItem.Refresh();
        }
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
