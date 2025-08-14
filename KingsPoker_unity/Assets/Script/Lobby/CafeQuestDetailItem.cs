using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.UI;

public class CafeQuestDetailItem : MonoBehaviour
{
    public LocalText questText;
    public Text countText;

    public void SetQuestData(JObject data)
    {
        var key = data.ValueOrDefault("key", "notKey");
        var cur = data.ValueOrDefault("cur", 0);
        var max = data.ValueOrDefault("max", 1);
        questText.SetLocalText(data.ValueOrDefault("key", "notKey"));
        countText.text = $"({cur}/{max})";
    }
}
