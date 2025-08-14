using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerRate : MonoBehaviour
{
    public GameObject balloon;
    public Image onImage;
    public Image offImage;
    public Text winRateText;
    public Text tieRateText;

    public virtual void ForceBack()
    {
        balloon.SetActive(false);
    }

    public virtual void ShowRate(float winRate, float tieRate)
    {
        if (!offImage)
            return;
        if (winRate + tieRate > 0f)
        {
            offImage.gameObject.SetActive(false);
            onImage.gameObject.SetActive(true);
            winRateText.text = string.Format("{0:0.##}%", winRate + tieRate);
        }
        else
        {
            onImage.gameObject.SetActive(false);
            offImage.gameObject.SetActive(true);
            winRateText.text = string.Format("{0:0.##}%", winRate + tieRate); ;
        }

        if (tieRate > 0f)
        {
            tieRateText.text = string.Format("동률 : {0}%", tieRate);
            tieRateText.gameObject.SetActive(true);
        }
        else
        {
            tieRateText.gameObject.SetActive(false);
        }

        balloon.SetActive(true);
    }
}
