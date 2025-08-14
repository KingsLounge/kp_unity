using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json.Linq;

public class CafeMemberJoinModuleTemplate : MonoBehaviour
{
    public Text userIdText;
    public Button acceptButton;
    public Button rejectButton;
    private CustomUICafeMemberJoinModule module;
    private CAFE_JOIN_STATUS status = CAFE_JOIN_STATUS.none;
    public int idx = -1;

    private void Awake()
    {
        acceptButton.onClick.AddListener(OnClickAcceptButton);
        rejectButton.onClick.AddListener(OnClickRejectButton);
    }

    public void SetData(CustomUICafeMemberJoinModule module, JObject d)
    {
        status = (CAFE_JOIN_STATUS)((int)d["status"]);
        idx = (int)d["idx"];
        this.module = module;
        userIdText.text = d["nick"].ToString();
    }

    public void OnClickAcceptButton()
    {
        module.OnClickAccept(idx);
    }

    public void OnClickRejectButton()
    {
        module.OnClickReject(idx);
    }
}
