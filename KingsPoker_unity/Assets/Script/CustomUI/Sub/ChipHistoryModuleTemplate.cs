using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json.Linq;

public class ChipHistoryModuleTemplate : MonoBehaviour
{
    public Text text_date;
    public Text text_toWho;
    public Text text_status;
    public Text text_amount;
    
    public Image    img_chip;

    private CustomUIChipHistoryModule module;
    private TRANSFER_TYPE status = TRANSFER_TYPE.none;
    //public GameObject cover;
    public int idx = -1;

    private void Awake()
    {

    }

    public void SetData(CustomUIChipHistoryModule module, JObject d)
    {
        status = (TRANSFER_TYPE)((int)d["status"]);
        idx = d.ValueOrDefault("idx", 0);
        this.module = module;
        int t = (int)status;
        DateTime time = DateTimeParser.Parse(d["createdAt"].ToString());
        text_date.text = time.ToLocalTime().ToString("yyyy/MM/dd HH:mm");

        string temp_str = (string)d.ValueOrDefault("nick", "");
        if( temp_str == "")
        {
            temp_str = "to Cafe";
        }
        text_toWho.text = temp_str;

        text_status.text = LocalizeManager.GetLocalString(status.ToString());
        text_status.color = module.color_chips[(int)status];
        img_chip.sprite = module.img_chips[t];
        try
        {
            text_amount.text = module.prefixs[(int)status] + MoneyToString.Converting(d.ValueOrDefault("cc", 0));
        }catch(System.Exception e)
        {
            
            Debug.Log((int)status + " "  + e.ToString());
        }
        
        text_amount.color = module.color_chips[(int)status];
    }
}
