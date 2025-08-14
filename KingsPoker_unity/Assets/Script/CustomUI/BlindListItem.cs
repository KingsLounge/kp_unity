using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json.Linq;
public class BlindListItem : MonoBehaviour
{
    [SerializeField]
    private Text levelText;

    [SerializeField]
    private Text blindText;

    [SerializeField]
    private Text antiText;

    [SerializeField]
    private Text timeText;

    [SerializeField]
    private Color normal = Color.white;

    [SerializeField]
    private Color highlighted = Color.white;


    public void SetBlind(JObject data, int currentLevel)
    {
        var level = data.ValueOrDefault("id", 0);
        var sb = data.ValueOrDefault("small", 0);
        var bb = data.ValueOrDefault("big", 0);
        var ante = data.ValueOrDefault("ante", 0);
        var breakTime = data.ValueOrDefault("is_breaktime", 0) != 0;
        var next_second = data.ValueOrDefault("next_second", 0);

        levelText.text = level.ToString();

        if (breakTime)
        {
            blindText.text = LocalizeManager.GetLocalString("break_time");

            antiText.text = string.Empty;
            timeText.text = next_second.ToString();
        }
        else
        {
            blindText.text = $"{MoneyToString.Converting(sb)}/{MoneyToString.Converting(bb)}";

            antiText.text = MoneyToString.Converting(ante);
            timeText.text = next_second.ToString();
        }

        levelText.color = level == currentLevel ? highlighted : normal;
        blindText.color = level == currentLevel ? highlighted : normal;
        antiText.color = level == currentLevel ? highlighted : normal;
        timeText.color = level == currentLevel ? highlighted : normal;

    }

}
