using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tnmt
{
    public int tn;
    public int cafeIdx;
    public long buyIn;
    public string startTime;
    public string closeTime;
}

public class TnmtUI : MonoBehaviour
{
    public GameObject tnmtListPopup;

    public static List<Tnmt> playTournamentList = new List<Tnmt>();

    public void TnmtPopupOnOffButton()
    {
        if (tnmtListPopup.activeSelf == true)
        {
            tnmtListPopup.SetActive(false);
        }
        else
        {
            tnmtListPopup.SetActive(true);
        }
    }
}
