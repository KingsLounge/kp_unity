using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class CustomUITableModule : CustomUI
{
    public ItemControllerServerCommunication itemController;
    private List<TableModuleTemplate> templates = new List<TableModuleTemplate>();

    private void Awake()
    {
        itemController.updateItemCallback = (go, data) =>
        {
            TableModuleTemplate t = go.GetComponent<TableModuleTemplate>();
            if (!templates.Contains(t))
            {
                templates.Add(t);
            }

            t.SetData(this, data);
        };
    }

    public override void SetValue(JToken data)
    {
        SetList(data as JArray);
    }

    //public void OnClickObserveBtn(int gtn)
    //{
    //    JArray jarray = new JArray();
    //    JObject t_info = new JObject();
    //    t_info.Add("tnmt_info_rank", 0);
    //    t_info.Add("tnmt_info_name", "test");
    //    t_info.Add("tnmt_info_chip", 0);
    //    jarray.Add(t_info);

    //    var opener = root.GetComponentInParent<CustomUIOpenerLobby>();
    //    var tnmtData = MyStatus.GetTnmt(opener.selectedTnmt.tn); // 토너먼트 참여중
    //    var roomStatus = InfoManager.Instance.GetTourmentRoom(opener.selectedTnmt.tn);

    //    bool canObserve = tnmtData == null && (roomStatus?.gtn != gtn);

    //    Cafe.instance.TnmtUserInfoPopup.SetData(gtn, jarray, canObserve);
    //    Cafe.instance.TnmtUserInfoPopup.Open();
    //}

    public async void OnClickObserveBtn(long gtn)
    {
        LoadingCircle.Instance.StartSpin();
        Packet enter = new Packet(CPProtocol.CP_PLAY_GAME_ENTER_OBSERVER);
        enter.Add("gtn", gtn);
        WebSocketManager.defaultCli.Send(enter);
        WaitForPCProtocol enter_wait = new WaitForPCProtocol(
            PCProtocol.PC_PLAY_GAME_ENTER_OBSERVER
        );
        await enter_wait;
        root.GetComponentInParent<CustomUIOpenerLobby>()?.CloseUI();
        LoadingCircle.Instance.StopSpin();
    }

    private void SetList(JArray list)
    {
        itemController.dataArray = list;
        itemController.Refresh();
        Debug.Log(itemController.dataArray);
    }

    private void OnEnable()
    {
        StartCoroutine(ReSize());
    }

    private IEnumerator ReSize()
    {
        yield return new WaitForEndOfFrame();

        var rect = (transform as RectTransform);
        rect.sizeDelta = new Vector2(rect.sizeDelta.x, root.viewportSize.y);
    }
}
