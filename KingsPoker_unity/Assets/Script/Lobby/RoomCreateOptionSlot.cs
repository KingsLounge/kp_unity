using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json.Linq;
using UnityEngine.Events;


public class RoomCreateOptionSlot : MonoBehaviour
{
    [SerializeField]
    public Toggle optionToggle;
    [SerializeField]
    private Text sb;
    [SerializeField]
    private Text bb;
    [SerializeField]
    private Text buyinMin;
    

    public void SetSlot(JObject option, UnityAction<bool> toggleAction)
    {
        
        var toggle = optionToggle;
        toggle.onValueChanged.AddListener(toggleAction);
        sb.text = MoneyToString.Converting(option.ValueOrDefault<long>("small_blind", 0));
        bb.text = MoneyToString.Converting(option.ValueOrDefault<long>("blind", 0));
        buyinMin.text = MoneyToString.Converting(option.ValueOrDefault<long>("buyin_min", 0));
    }
}
