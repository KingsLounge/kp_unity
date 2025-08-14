using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TableModuleTemplate : MonoBehaviour
{
    [SerializeField]
    private Text roomNumber = null;

    [SerializeField]
    private Text playerCount = null;

    [SerializeField]
    private Text maxChip = null;

    [SerializeField]
    private Text minChip = null;

    [SerializeField]
    private Button button = null;

    private int gtn = 0;

    private CustomUITableModule module = null;

    public void SetData(CustomUITableModule module, JToken data)
    {
        int gtn = (int)data;

        roomNumber.text = gtn.ToString();
        this.gtn = gtn;
        this.module = module;

        var opener = module.root.GetComponentInParent<CustomUIOpenerLobby>();
        var tnmtData = MyStatus.GetTnmt(opener.selectedTnmt.tn); // 토너먼트 참여중
        var roomStatus = InfoManager.Instance.GetTourmentRoom(opener.selectedTnmt.tn);

        bool canObserve = tnmtData == null && (roomStatus?.gtn != gtn);
        if(opener.selectedTnmt.state < (int)TNMT_FLOW.start || opener.selectedTnmt.state >= (int)TNMT_FLOW.end)
        {
            canObserve = false;
        }

        button.interactable = canObserve;
    }

    public void OnCLick()
    {
        module.OnClickObserveBtn(gtn);
    }
}
