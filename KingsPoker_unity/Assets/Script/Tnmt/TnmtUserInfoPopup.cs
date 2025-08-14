using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.UI;

public class TnmtUserInfoPopup : MonoBehaviour
{
    [SerializeField]
    private CustomUIOpener uiOpener = null;

    [SerializeField]
    private Button observeButton = null;

    public ItemControllerServerCommunication itemController;
    private List<TnmtUserInfoTemplate> templates = new List<TnmtUserInfoTemplate>();

    public long gtn { get; private set; } = 0;

    private void Awake()
    {
        itemController.updateItemCallback = (go, data) =>
        {
            TnmtUserInfoTemplate t = go.GetComponent<TnmtUserInfoTemplate>();
            if (!templates.Contains(t))
            {
                templates.Add(t);
            }

            t.SetData(data as JObject);
        };
    }
    
    public void Open()
    {
        gameObject.SetActive(true);
    }

    public void SetData(int gtn, JArray list, bool canObserve)
    {
        this.gtn = gtn;
        itemController.dataArray = list;
        itemController.Refresh();

        observeButton.interactable = canObserve;
    }

    public async void OnClickObserverBtn(TnmtUserInfoTemplate target)
    {
        LoadingCircle.Instance.StartSpin();
        Packet enter = new Packet(CPProtocol.CP_PLAY_GAME_ENTER_OBSERVER);
        enter.Add("gtn", gtn);
        WebSocketManager.defaultCli.Send(enter);
        WaitForPCProtocol enter_wait = new WaitForPCProtocol(PCProtocol.PC_PLAY_GAME_ENTER_OBSERVER);
        await enter_wait;
        uiOpener.CloseUI();
        gameObject.SetActive(false);
        LoadingCircle.Instance.StopSpin();
    }
}
