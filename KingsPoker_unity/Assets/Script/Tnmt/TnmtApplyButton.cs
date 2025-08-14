using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class TnmtApplyButton : MonoBehaviour
{
    public int tn;
    public int cafeIdx;

    public Text tnText;
    public Text cafeIdxText;
    public Text buyInText;
    public Text startTimeText;
    public Text closeTimeText;

    public void Setting(int tn, int cafeIdx, long buyIn, string startTime, string closeTime)
    {
        this.tn = tn;
        tnText.text = string.Format("tn\n{0}", this.tn);

        this.cafeIdx = cafeIdx;
        cafeIdxText.text = string.Format("cafeIdx\n{0}", this.cafeIdx);

        buyInText.text = string.Format("BuyIn\n{0}", buyIn);

        startTimeText.text = string.Format("StartTime: {0}", startTime);
        closeTimeText.text = string.Format("CloseTime: {0}", closeTime);
    }

    public void OnClickTnmtApply()
    {
        LoadingCircle.Instance.StartSpin();
        StartCoroutine(TnmtApplyFlow());
    }

    private IEnumerator TnmtApplyFlow()
    {
        Packet p = new Packet(CPProtocol.CP_TNMT_APPLY);
        p.Add("tn", tn);
        p.Add("cafeIdx", cafeIdx);
        p.Add("ticket", 0);
        p.Add("rebuy", 0);
        p.Add("double", 0);
        WebSocketManager.defaultCli.Send(p);
        WaitForPCProtocol wait = new WaitForPCProtocol(PCProtocol.PC_TNMT_APPLY, PCProtocol.PC_TNMT_APPLY_FAIL);
        yield return wait;
        LoadingCircle.Instance.StopSpin();
        if (wait.Result.c.ValueOrDefault("ecode", 0) == 0)
            NormalMessage.instance.OnOneButtonMessagePopUp("confirm_success");
        else
            NormalMessage.instance.OnOneButtonMessagePopUp("table_fail");
    }

    public void OnClickTnmtUnapply()
    {
        LoadingCircle.Instance.StartSpin();
        StartCoroutine(TnmtUnapplyFlow());
    }

    private IEnumerator TnmtUnapplyFlow()
    {
        Packet p = new Packet(CPProtocol.CP_TNMT_UNAPPLY);
        p.Add("tn", tn);
        WebSocketManager.defaultCli.Send(p);
        WaitForPCProtocol wait = new WaitForPCProtocol(PCProtocol.PC_TNMT_UNAPPLY, PCProtocol.PC_TNMT_UNAPPLY_FAIL);
        yield return wait;
        LoadingCircle.Instance.StopSpin();
        if (wait.Result.c.ValueOrDefault("ecode", 0) == 0)
            NormalMessage.instance.OnOneButtonMessagePopUp("confirm_success");
        else
            NormalMessage.instance.OnOneButtonMessagePopUp("table_fail");
    }
}
