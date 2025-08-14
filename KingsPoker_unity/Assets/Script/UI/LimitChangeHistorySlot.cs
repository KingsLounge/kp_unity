using Newtonsoft.Json.Linq;
using System;
using UnityEngine;
using UnityEngine.UI;

public class LimitChangeHistorySlot : MonoBehaviour
{
    public Text historyTimeText;
    public Text historyChipText;
    public Button historyButton;
    public Text buttonText;
    public LimitChangeWindow limitChangeWindow;

    private long limit;
    private string time;
    private int index;

    public void Init(JObject data, int i)
    {
        limit = data.ValueOrDefault<long>("limit", 0);
        time = data.ValueOrDefault<string>("date", "");
        index = i;
        Init();
    }

    public void Init()
    {
        historyChipText.text = MoneyToString.Converting(limit);
        historyTimeText.text = DateTimeParser.Parse(time).ToLocalTime().ToString();
        historyButton.interactable = index == 0;
        buttonText.text = index == 0 ? "변경하기" : "변경완료";
    }
    public void OnClickButton()
    {
        limitChangeWindow.Init();
        limitChangeWindow.gameObject.SetActive(true);
    }
}
