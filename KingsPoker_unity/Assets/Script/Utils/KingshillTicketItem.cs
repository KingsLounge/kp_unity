using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KingshillTicketItem : MonoBehaviour
{
    [SerializeField]
    private Text ticketText;
    [SerializeField]
    private Text amountText;

    public async void SetData(string ticket, long amount)
    {
        var ticketString = await KingshillInfo.GetTicketString(ticket);
        ticketText.text = ticketString;
        amountText.text = MoneyToString.Converting(amount);
    }
}
