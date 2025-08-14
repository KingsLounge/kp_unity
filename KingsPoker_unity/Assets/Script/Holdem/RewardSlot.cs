using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RewardSlot : MonoBehaviour
{
    public Text rankText;
    public Text rewardText;
    public Image slotImage;
    public Sprite[] rankColorSprite;

    public void SetSlot(int rank, long reward)
    {
        rankText.text = string.Format("{0} 위", rank+1);
        rewardText.text = reward.ToString();
        if(rank < rankColorSprite.Length)
        {
            slotImage.sprite = rankColorSprite[rank];
        }
        else
        {
            slotImage.sprite = rankColorSprite[rankColorSprite.Length - 1];
        }
    }
}
