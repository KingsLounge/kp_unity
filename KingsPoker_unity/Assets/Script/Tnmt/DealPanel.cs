using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DealPanel : MonoBehaviour
{
    public GameObject dealButton;
    public GameObject cancelDealButton;

    public Text dealText;

    public string dealString;
    public string cancelString;
    

    public void OnDeal(bool imDeal)
    {
        dealText.text = imDeal ? cancelString : dealString;
        dealButton.SetActive(!imDeal);
        cancelDealButton.SetActive(imDeal);
    }
    public void OnClickDealButton()
    {
        dealText.text = dealString;
    }

    public void OnClickCancelDealButton()
    {
        dealText.text = cancelString;
    }
}
