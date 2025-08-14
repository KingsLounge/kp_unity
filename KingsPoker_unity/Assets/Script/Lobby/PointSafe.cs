using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.UI;
using Newtonsoft.Json.Linq;

public class PointSafe : MonoBehaviour
{
    [SerializeField]
    private Text pointText;
    [SerializeField]
    private InputField input;
    private long point = 0;
    private long inputPoint = 0;
    private void OnEnable()
    {
        GetPoint();
    }
    public async void GetPoint()
    {
        var p = new Packet(CPProtocol.CP_KINGSHILL_GET_USER_INFO);
        
        WebSocketManager.defaultCli.Send(p.ToJson());
        var wait = new WaitForPCProtocol(PCProtocol.PC_KINGSHILL_GET_USER_INFO);
        await wait;
        point = wait.Result.c.ValueOrDefault<long>("point", 0);
        SetPointText();
    }

    private void SetPointText()
    {
        pointText.text = MoneyToString.Converting(point);
    }

    public void PointInputValueChange(string value)
    {
        inputPoint = long.Parse(value);
    }

    public async void RequestGetPoint()
    {
        if(inputPoint > point)
        {
            NormalMessage.instance.OnOneButtonMessagePopUp("not_enugh_point");
        }
        else
        {
            var p = new Packet(CPProtocol.CP_KINGSHILL_POINT_IN);
            p.Add("cafeIdx", Cafe.instance.curEnterCafeInfo["cafe"]["idx"]);
            p.Add("point", inputPoint);
            WebSocketManager.defaultCli.Send(p);
            var wait = new WaitForPCProtocol(PCProtocol.PC_KINGSHILL_POINT_IN);
            await wait;
            var err = wait.Result.c.ValueOrDefault<ERR>("ecode", 0);
            if (err == ERR.OK)
            {
                point -= wait.Result.c.ValueOrDefault<long>("point", 0);
                SetPointText();
                Cafe.instance.curEnterCafeInfo["cafeMember"]["cc"] = wait.Result.c.ValueOrDefault<long>("cc", 0);
                Cafe.instance.CafeInfoSet();
                NormalMessage.instance.AddSimpleMessage("sucess_get_point");
                gameObject.SetActive(false);
            }
        }    
    }
    public async void RequestReturnPoint()
    {
        var cc = (Cafe.instance.curEnterCafeInfo["cafeMember"]as JObject).ValueOrDefault<long>("cc",0);
        if (inputPoint > cc)
        {
            NormalMessage.instance.OnOneButtonMessagePopUp("not_enugh_chip");
        }
        else
        {
            var p = new Packet(CPProtocol.CP_KINGSHILL_POINT_OUT);
            p.Add("cafeIdx", Cafe.instance.curEnterCafeInfo["cafe"]["idx"]);
            p.Add("point", inputPoint);
            WebSocketManager.defaultCli.Send(p);
            var wait = new WaitForPCProtocol(PCProtocol.PC_KINGSHILL_POINT_OUT);
            await wait;
            var err = wait.Result.c.ValueOrDefault<ERR>("ecode", 0);
            if (err == ERR.OK)
            {
                point += wait.Result.c.ValueOrDefault<long>("point", 0);
                SetPointText();
                Cafe.instance.curEnterCafeInfo["cafeMember"]["cc"] = wait.Result.c.ValueOrDefault<long>("cc", 0);
                Cafe.instance.CafeInfoSet();
                NormalMessage.instance.AddSimpleMessage("sucess_return_point");
                gameObject.SetActive(false);
            }
        }
    }
}
