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
    public void SetRewardPanel(int rank, long reward, string enemyUid)
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
        rewardText.text = MoneyToString.Converting(reward);
        gameObject.SetActive(true);
    }
}
