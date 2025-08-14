using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

public class KingshillTicketAmountItem : MonoBehaviour
{
    [SerializeField]
    private Text ticketText;
    [SerializeField]
    private Text amountText;

    [SerializeField]
    private InputField changeInputField;
    private string ticket;
    private long amount;

    public async void SetData(string ticket, long amount)
    {
        this.ticket = ticket;
        this.amount = amount;
        var ticketString = await KingshillInfo.GetTicketString(ticket);
        ticketText.text = ticketString;
        amountText.text = MoneyToString.Converting(amount);
    }

    public void OnCklickChangeTicekt()
    {
        var ticketGbn = KingshillInfo.GetTicketGbn(ticket);
        if(ticketGbn == 0)
        {
            return;
        }
        var ticketCount = int.Parse(changeInputField.text);
        var hasTicketCount = MyStatus.loungeData.ValueOrDefault<long>(ticket, 0);
        if(hasTicketCount < ticketCount)
        {
            ErrorMessageManager.Instance.AddGameError(0, "소유한 티켓 교환 에러", "소유한 티켓 수가 부족합니다.", ErrorHandlingType.NONE, null, null);
            return;
        }
        var p = new Packet(CPProtocol.CP_KINGSHILL_TICKET_TO_POINT);
        p.Add("ticketGbn", ticketGbn);
        p.Add("changeTicket", ticketCount);
        
        WebSocketManager.defaultCli.Send(p);
    }

    public void OnChangeTicketCount()
    {
        var ticketCount = int.Parse(changeInputField.text);
        var hasTicketCount = MyStatus.loungeData.ValueOrDefault<long>(ticket, 0);
        if(hasTicketCount < ticketCount)
        {
            changeInputField.text = hasTicketCount.ToString();
        }
        else
        {
            changeInputField.text = ticketCount.ToString();
        }
    }
}
