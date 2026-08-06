using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TournamentResultPanel : MonoBehaviour
{
    [SerializeField]
    private Image medalImage;
    [SerializeField]
    private Image resultImage;
    [SerializeField]
    private List<Sprite> medalSprites;
    [SerializeField]
    private Text rewardText;
    [SerializeField]
    private Text rankText;
    [SerializeField]
    private Sprite victorySprite;
    [SerializeField]
    private Sprite loseSprite;
    [SerializeField]
    private Outline rankOutLine;
    [SerializeField]
    private List<Color> outlineColors;
    public void SetRewardPanel(int rank, long reward, string enemyUid, long kp = 0)
    {
        if(rank < medalSprites.Count+1)
        {
            medalImage.sprite = medalSprites[rank-1];
        }
        else
        {
            medalImage.sprite = medalSprites[medalSprites.Count-1];
        }

        if(rank < outlineColors.Count+1)
        {
            rankOutLine.effectColor = outlineColors[rank-1];
        }
        else
        {
            rankOutLine.effectColor = outlineColors[outlineColors.Count-1];
        }

        if(rank == 1)
        {
            resultImage.sprite = victorySprite;
        }
        else
        {
            resultImage.sprite = loseSprite;
        }

        rankText.text = rank.ToString();
        // KP 상금은 기존 상금 텍스트에 함께 표기 (프리팹 변경 없이)
        if (kp > 0 && reward > 0)
        {
            rewardText.text = $"{MoneyToString.Converting(reward)} + {MoneyToString.Converting(kp)}KP";
        }
        else if (kp > 0)
        {
            rewardText.text = $"{MoneyToString.Converting(kp)}KP";
        }
        else
        {
            rewardText.text = MoneyToString.Converting(reward);
        }
        gameObject.SetActive(true);
    }
}
