using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class PlayGameItemGuage : MonoBehaviour
{
    PlayGameInfo playGameInfo;
    TournamentInfo tnmtInfo;
    
    public Sprite[] spr_guages;
    public Color[] color_guages;

    public Image img_guage;
    public Text txt_personel;

    public void Set(PlayGameInfo info)
    {
        playGameInfo = info;

        int game_type = (int)playGameInfo.info["game_type"];

        // System.Random rand = new System.Random();
        // int playCount = rand.Next(0, 7);
        int playerCount = (int)playGameInfo.info["player_count"];
        int perssonel = (int)playGameInfo.info["personnel"];
        float guage = (float)playerCount / perssonel;
        if(game_type < spr_guages.Length)
            img_guage.sprite = spr_guages[game_type];
        if (playerCount == 0)
            txt_personel.color = color_guages[0]; // grey
        else if(game_type < color_guages.Length)
            txt_personel.color = color_guages[game_type];
        
        txt_personel.text = playerCount + "/" + perssonel;
        if (game_type < color_guages.Length)
            img_guage.color = color_guages[game_type];
        img_guage.fillAmount = guage;



    }

    public void Set(TournamentInfo info)
    {
        tnmtInfo = info;

        // System.Random rand = new System.Random();
        // int playCount = rand.Next(0, 7);
        int maxPlayer = (int)tnmtInfo.info["t_max_player"];
        int minPlayer = (int)tnmtInfo.info["t_min_player"];
        int countAllUser = tnmtInfo.countAllUser;
        txt_personel.text = countAllUser + "/" + maxPlayer;
        txt_personel.color = color_guages[(int)GAME_TYPE.mtt];
        img_guage.color = color_guages[(int)GAME_TYPE.mtt];
        img_guage.fillAmount = 1.0f;



    }
}
