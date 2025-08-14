using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json.Linq;

public class MyCashierOrderTemplate : MonoBehaviour
{
    public Text dateText;
    public Text requestTypeText;
    public Text requestAmountText;
    public Image img_chip;
    public Button cancelButton;
    private CustomUIMyCahsierOrderModule module;
    private TRANSFER_TYPE status = TRANSFER_TYPE.none;
    public int idx = -1;

    private void Awake()
    {
        cancelButton.onClick.AddListener(OnClickCancelButton);
    }

    public void SetData(CustomUIMyCahsierOrderModule module, JObject d)
    {
        DateTime time = DateTimeParser.Parse(d["createdAt"].ToString());
        dateText.text = time.ToLocalTime().ToString("yyyy/MM/dd HH:mm");

        status = (TRANSFER_TYPE)(d.ValueOrDefault("status",0));
        idx = (int)d["idx"];
        this.module = module;
        requestTypeText.text = LocalizeManager.GetLocalString(status.ToString());
        requestTypeText.color = module.color_chips[(int)status];
        requestAmountText.text = MoneyToString.Converting((long)d["cc"]);
        requestAmountText.color = module.color_chips[(int)status];

        img_chip.sprite = module.img_chips[(int)status];
    }

    public void OnClickCancelButton()
    {
        module.OnClickCancel(idx,status);
    }
}
