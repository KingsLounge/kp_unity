using UnityEngine;
using UnityEngine.UI;

// KP -> 칩(cc) 1:1 전환 UI (기존 티켓 교환 표 자리에 배치)
public class KingshillKpChangeItem : MonoBehaviour
{
    // 전환 비율 (KP 1 -> 칩 N). 서버(fw kingshillManager.kpToPoint)와 함께 1:1 고정
    private const int KP_TO_CHIP_RATE = 1;

    [SerializeField]
    private Text kpAmountText;
    [SerializeField]
    private InputField changeInputField;

    // 환율 안내 "1KP  ->  1칩" — KP쪽/칩쪽 텍스트가 별도 오브젝트 (화살표는 정적, 미연결 시 무시)
    [SerializeField]
    private Text rateKpText;
    [SerializeField]
    private Text rateChipText;

    private void OnEnable()
    {
        Refresh();
    }

    public void Refresh()
    {
        var kp = MyStatus.loungeData != null ? MyStatus.loungeData.ValueOrDefault<long>("newKp", 0) : 0;
        if (kpAmountText != null)
        {
            kpAmountText.text = MoneyToString.Converting(kp);
        }
        if (rateKpText != null)
        {
            rateKpText.text = "1KP";
        }
        if (rateChipText != null)
        {
            rateChipText.text = $"{KP_TO_CHIP_RATE}칩";
        }
    }

    public void OnClickChangeKp()
    {
        if (!int.TryParse(changeInputField.text, out int changeKp) || changeKp <= 0)
        {
            return;
        }
        var hasKp = MyStatus.loungeData.ValueOrDefault<long>("newKp", 0);
        if (hasKp < changeKp)
        {
            ErrorMessageManager.Instance.AddGameError(0, "KP 교환 에러", "보유한 KP가 부족합니다.", ErrorHandlingType.NONE, null, null);
            return;
        }
        var p = new Packet(CPProtocol.CP_KINGSHILL_TICKET_TO_POINT);
        p.Add("changeTicket", changeKp);
        p.Add("changeType", 1); // 0: ticket, 1: KP

        WebSocketManager.defaultCli.Send(p);
    }

    public void OnChangeKpCount()
    {
        if (!int.TryParse(changeInputField.text, out int changeKp) || changeKp < 0)
        {
            changeInputField.text = "0";
            return;
        }
        var hasKp = MyStatus.loungeData.ValueOrDefault<long>("newKp", 0);
        if (hasKp < changeKp)
        {
            changeInputField.text = hasKp.ToString();
        }
        else
        {
            changeInputField.text = changeKp.ToString();
        }
    }
}
