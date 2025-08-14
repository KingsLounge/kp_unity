using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CircleRate : MonoBehaviour
{
    [SerializeField]
    private Text ratename = null;
    [SerializeField]
    private string format = "";

    [SerializeField]
    private Image rate = null;

    [SerializeField]
    private Text rateText = null;


    public void SetValue(float val, float max, string name = "")
    {
        
        rate.fillAmount = max > 0 ? val / max :  0;
        rateText.text = string.Format(format, Mathf.RoundToInt(rate.fillAmount * 100));
        if(ratename)
        {
            ratename.text = name;
        }
       
    }

}
