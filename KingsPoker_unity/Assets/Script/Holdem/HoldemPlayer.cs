using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum HOLDEM_POSITION
{
    NONE, SB, BB
}

public class HoldemPlayer : Player
{
    //public PlayerRate rate;
    public HOLDEM_POSITION position = HOLDEM_POSITION.NONE;
    public bool isUTG = false;

    public override void Awake()
    {
        base.Awake();
        HoldemPlayerBetting holdemBetting = (betting as HoldemPlayerBetting);
        if(holdemBetting.chipObj != null)
        {
            holdemBetting.chipObj.player = this;
        }
    }

    public override void ForceBack()
    {
        base.ForceBack();
        rate.ForceBack();
    }

    public override void PlayResult(string hands, long a, long w, long gc, bool win, bool split = false)
    {
        base.PlayResult(hands, a, w, gc, win, split);
        rate.ForceBack();
    }

    public override void ShowRate(float winRate, float tieRate)
    {
        rate.ShowRate(winRate, tieRate);
    }

    public void SetHandRank(string hole, string comm = null)
    {
        (playerCard as HoldemPlayerCard).SetHandRank(hole, comm);
    }
    public void MoveChip(Vector3 pos, float time)
    {
        var hb = betting as HoldemPlayerBetting;
        hb.MoveChip(pos, time);
    }
    public void ClearCard()
    {
        playerCard.ForceBack();
    }
}
