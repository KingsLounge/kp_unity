using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HoldemPlayerResult : PlayerResult
{
    public Sprite winPanelTexture;
    public Sprite splitWinPanelTexture;
    public Image winPanelImage;

    public override void PlayResult(string hands, long a, long w, long gc, bool win, bool split = false) {
        base.PlayResult(hands,a,w,gc,win,split);
        if(win) {
            if(winPanelImage)
            {
                if (split)
                {
                    winPanelImage.sprite = splitWinPanelTexture;
                }
                else
                {
                    winPanelImage.sprite = winPanelTexture;
                }
            }
        }
    }
}
