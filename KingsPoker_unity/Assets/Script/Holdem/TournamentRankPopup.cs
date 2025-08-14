using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TournamentRankPopup : MonoBehaviour
{
    public Transform rankParent;
    public Transform rewardParent;
    public GameObject rankPrefab;
    public GameObject rewardPrefab;
    public void Popup()
    {
        gameObject.SetActive(true);
    }
    public void Close()
    {
        gameObject.SetActive(false);
    }
}
