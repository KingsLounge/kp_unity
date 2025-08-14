using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
[RequireComponent(typeof(Graphic))]
public class InteractableColor : MonoBehaviour
{
    public Selectable selectable;
    private Graphic graphic;
    public Color onColor = Color.white;
    public Color offColor = Color.gray;

    private void Awake()
    {
        graphic = GetComponent<Graphic>();
    }
    private void Update()
    {
        if (selectable)
        {
            Color color = selectable.interactable ? onColor : offColor;
            if (!graphic.color.Equals(color))
                graphic.color = color;
        }
    }
}
