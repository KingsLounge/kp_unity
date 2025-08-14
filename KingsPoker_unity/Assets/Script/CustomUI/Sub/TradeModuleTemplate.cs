using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json.Linq;

public class TradeModuleTemplate : MonoBehaviour
{
    public Text userIdText;
    public Text requestTypeText;
    public Text requestAmountText;
    public Button acceptButton;
    public Button rejectButton;
    private CustomUITradeModule module;
    private TRANSFER_TYPE status = TRANSFER_TYPE.none;
    public GameObject cover;
    public int idx = -1;

    private void Awake()
    {
        acceptButton.onClick.AddListener(OnClickAcceptButton);
        rejectButton.onClick.AddListener(OnClickRejectButton);
    }

    public void SetData(CustomUITradeModule module, JObject d)
    {
        status = (TRANSFER_TYPE)((int)d["status"]);
        idx = (int)d["idx"];
        this.module = module;
        userIdText.text = d["nick"].ToString();
        requestTypeText.text = LocalizeManager.GetLocalString(status.ToString());
        requestAmountText.text = MoneyToString.Converting((long)d["cc"]);
        bool complete = (status != TRANSFER_TYPE.buy && status != TRANSFER_TYPE.sell && status != TRANSFER_TYPE.borrow);
        cover.SetActive(complete);
        if(complete)
        {
            bool reject = status.ToString().Contains("rejected");
            rejectButton.gameObject.SetActive(reject);
            acceptButton.gameObject.SetActive(!reject);
        }
        else
        {
            rejectButton.gameObject.SetActive(true);
            acceptButton.gameObject.SetActive(true);
        }
    }

    public void OnClickAcceptButton()
    {
        module.OnClickAccept(idx,status);
    }

    public void OnClickRejectButton()
    {
        module.OnClickReject(idx,status);
    }
}
