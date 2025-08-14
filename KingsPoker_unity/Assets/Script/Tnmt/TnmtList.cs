using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TnmtList : MonoBehaviour
{
    public TnmtApplyButton original;
    public Transform content;

    private void OnEnable()
    {
        for (int i = 0; i < content.childCount; i++)
        {
            content.GetChild(i).gameObject.SetActive(false);
        }

        List<Tnmt> list = TnmtUI.playTournamentList;
        for (int i = 0; i < list.Count; i++)
        {
            TnmtApplyButton tab = Instantiate(original);
            tab.Setting(list[i].tn, list[i].cafeIdx, list[i].buyIn, list[i].startTime, list[i].closeTime);
            tab.transform.parent = content;
            tab.gameObject.SetActive(true);
        }
    }
}
