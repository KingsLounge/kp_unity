using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json.Linq;

public class CafeChangeOwnerTemplate : MonoBehaviour
{
    public Text userIdText;
    public Text permitText;
    private CAFE_MEMBER_PERMIT permit;
    private CustomUiCafeChangeOwner module;
    public List<Variation> myPermitVariation = new List<Variation>();
    public List<GameObject> ownerDisables = new List<GameObject>();
    public int idx = -1;

    // Start is called before the first frame update

    public void SetData(CustomUiCafeChangeOwner module, JObject d)
    {
        permit = (CAFE_MEMBER_PERMIT)((int)d["permit"]);
        ownerDisables.ForEach(g => g.SetActive(true));
        myPermitVariation.ForEach(v => v.SetVariation(Cafe.instance.permit.ToString()));
        idx = (int)d["idx"];
        if (permit == CAFE_MEMBER_PERMIT.owner)
        {
            try
            {
                JObject t = Cafe.instance.curEnterCafeInfo;
                if((int)t["cafeMember"]["idx"] == idx)
                {
                    ownerDisables.ForEach(g => g.SetActive(false));
                }
            }
            catch(System.Exception e)
            {

            }
            
        }

        this.module = module;
        userIdText.text = d["nick"].ToString();

        permitText.text = LocalizeManager.GetLocalString(permit.ToString());
        if (permit == CAFE_MEMBER_PERMIT.owner)
        {
            


            

        }
    }

    public void OnClickChangePermitButton()
    {
        // if (permit == CAFE_MEMBER_PERMIT.owner) return;

        CAFE_MEMBER_PERMIT newPermit = CAFE_MEMBER_PERMIT.none;
        if (permit == CAFE_MEMBER_PERMIT.member) newPermit = CAFE_MEMBER_PERMIT.owner;
        if (permit == CAFE_MEMBER_PERMIT.manager) newPermit = CAFE_MEMBER_PERMIT.owner;
        if (permit == CAFE_MEMBER_PERMIT.owner) newPermit = CAFE_MEMBER_PERMIT.manager;

        if (newPermit != CAFE_MEMBER_PERMIT.none)
        {
            string title = LocalizeManager.GetLocalString("really_change_permit");
            string body = LocalizeManager.GetLocalString("really_change_permit_" + permit.ToString() + "_to_" + newPermit.ToString());
            body = body.Replace("{nick}", userIdText.text);
            NormalMessage.instance.OnMessagePopup(title, body,()=> {
                Packet packet = new Packet(CPProtocol.CP_CAFE_MEMBER_UPDATE);
                packet.Add("cafeIdx", Cafe.instance.curEnterCafeInfo["cafe"]["idx"]);
                packet.Add("cafeMemberIdx", idx);
                packet.Add("status", (int)CAFE_MEMBER_STATUS.accepted);
                packet.Add("permit", (int)newPermit);
                WebSocketManager.defaultCli.Send(packet);
            });
        }
    }

    public void OnClickCafeKickButton()
    {
        if (permit == CAFE_MEMBER_PERMIT.owner) return;

        string title = LocalizeManager.GetLocalString("really_kick_cafe");
        string body = LocalizeManager.GetLocalString("really_kick_cafe_body");
        body = body.Replace("{nick}", userIdText.text);
        NormalMessage.instance.OnMessagePopup(title, body, () => {
            Packet packet = new Packet(CPProtocol.CP_CAFE_MEMBER_UPDATE);
            packet.Add("cafeIdx", Cafe.instance.curEnterCafeInfo["cafe"]["idx"]);
            packet.Add("cafeMemberIdx", idx);
            packet.Add("status", (int)CAFE_MEMBER_STATUS.suspended);
            packet.Add("permit", (int)permit);
            WebSocketManager.defaultCli.Send(packet);
        });
    }
}
