using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class GameTypeToggle : ToggleEvent
{
    public LobbyManager lobbyManager;
    public GAME_TYPE gameType;

    public void Awake()
    {
        toggle = GetComponent<Toggle>();
        onImage = transform.GetChild(1).gameObject;
        offImage = transform.GetChild(0).gameObject;
    }
    public GAME_TYPE GetGameTypeString()
    {
        return gameType;
    }
    public override void OnToggle()
    {
        toggle.targetGraphic.gameObject.SetActive(!toggle.isOn);
        if(toggle.isOn)
        {
            if (lobbyManager.curGameType.Equals(gameType.ToString().ToLower()) == false)
            {
                if (gameType == GAME_TYPE.mtt)
                {
                    Debug.Log(GetGameTypeString());
                    lobbyManager.OnClickGetTournamentListButton();
                }
                else
                {
                    Debug.Log(GetGameTypeString());
                    lobbyManager.OnChangedGameTypeTab(GetGameTypeString());
                }
            }
        }
        else
        {
            return;
        }
    }
}
