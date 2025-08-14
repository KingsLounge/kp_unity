using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class ConditionalInteractable : MonoBehaviour
{
    private Selectable selectable;

    public List<Toggle> conditionalToggle = new List<Toggle>();

    public bool toggleCondition
    {
        get
        {
            bool result = true;
            for (int i = 0; i < conditionalToggle.Count; i++)
                if (!conditionalToggle[i].isOn)
                    result = false;
            return result;
        }
    }

    private void Awake()
    {
        selectable = GetComponent<Selectable>();
        conditionalToggle.ForEach(toggle => toggle.onValueChanged.AddListener(OnToggleValueChanged));
        InteractableUpdate();
    }

    private void OnToggleValueChanged(bool active)
    {
        InteractableUpdate();
    }

    private void InteractableUpdate()
    {
        selectable.interactable = toggleCondition;
    }
}
