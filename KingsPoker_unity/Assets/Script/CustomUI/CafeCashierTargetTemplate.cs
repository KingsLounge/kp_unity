using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json.Linq;

public class CafeCashierTargetTemplate : MonoBehaviour
{
    [SerializeField]
    private Text userIdText;
    [SerializeField]
    private Toggle checkToggle;
    private CustomUICafeCashierTargetModule module;
    public int idx {
        get; private set;
    }


    // Start is called before the first frame update

    public void SetData(CustomUICafeCashierTargetModule module, JObject d)
    {
        idx = (int)d["idx"];
        this.module = module;
        userIdText.text = d["nick"].ToString();
        checkToggle.SetIsOnWithoutNotify(module.selectedIdx == idx);
    }

    public void ToggleCheck(bool check)
    {
        if(check)
        {
            module.selectedIdx = idx;
        }
    }
}
