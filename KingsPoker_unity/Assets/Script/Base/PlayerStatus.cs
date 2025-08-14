using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStatus : MonoBehaviour
{
    public Text levelText;
    public Text nicknameText;
    public Text chipText;

    public void PlayResult(bool flag)
    {
        //levelText.gameObject.SetActive(flag);
        nicknameText.gameObject.SetActive(flag);
    }
}
