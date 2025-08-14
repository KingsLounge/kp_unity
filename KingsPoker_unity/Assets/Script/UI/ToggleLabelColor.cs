using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class ToggleLabelColor : MonoBehaviour
{
    private Toggle toggle;
    public Color onColor = new Color(1f, 1f, 1f);
    public Color offColor = new Color(1f, 1f, 1f);
    private MaskableGraphic graphic = null;
    // Start is called before the first frame update
    void Start()
    {
        toggle = GetComponentInParent<Toggle>();
        graphic = GetComponent<MaskableGraphic>();
    }

    // Update is called once per frame
    void Update()
    {
        Color color = toggle.isOn ? onColor : offColor;
        if (graphic.color != color)
        {
            graphic.color = color;
        }
    }
}
