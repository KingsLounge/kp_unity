using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PingUI : MonoBehaviour
{
    [SerializeField]
    private Text text = null;

    [SerializeField]
    private Image img = null;

    [System.Serializable]
    public class PingColor
    {
        public int ping = 0;
        public Color color = Color.white;
    }


    [SerializeField]
    private List<PingColor> pingColors = new List<PingColor>();
    


    public void Awake()
    {
        PingCheckManager.success.AddListener((ms) =>
        {
            text.text = $"{ms} ms";

            img.color = getPingColor(ms);
        });

        PingCheckManager.failure.AddListener(() =>
        {
            text.text = "Disconnected";

            img.color = getPingColor(0);
        });
    }

    public Color getPingColor(int ms)
    {
        var sorted = pingColors.OrderByDescending((n => n.ping));

        foreach(var i  in pingColors) 
        { 
            if(ms > i.ping)
            {
                return i.color;
            }
        }

        return Color.red;
    }
}
