using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TnmtUserInfoTemplate : MonoBehaviour
{
    [SerializeField]
    private Text rank = null;

    [SerializeField]
    private Text nickname = null;

    [SerializeField]
    private Text chip = null;

    public void SetData(JObject data)
    {
        rank.text = data.ValueOrDefault<long>("tnmt_info_rank", 0).ToString();
        nickname.text = data.ValueOrDefault("tnmt_info_name", "");
        chip.text = data.ValueOrDefault<long>("tnmt_info_chip", 0).ToString();
    }
}
