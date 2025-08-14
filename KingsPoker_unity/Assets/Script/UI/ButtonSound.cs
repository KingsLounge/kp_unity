using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class ButtonSound : MonoBehaviour
{
    public bool customSound = false;
    [Header("Button")]
    public Sound_TableEnum buttonSound;
    [Header("Toggle")]
    public Sound_TableEnum onSound;
    public Sound_TableEnum offSound;
    private void Awake() {
        Selectable component = GetComponent<Selectable>();
        try
        {
            System.Type type = component.GetType();
            bool customSound = false;
            if (type == typeof(Button) || type.IsSubclassOf(typeof(Button)))
            {
                (component as Button).onClick.AddListener(() => {
                    if (customSound)
                        SoundManager.Instance.ButtonSound(buttonSound);
                    else
                        SoundManager.Instance.ButtonSound();
                });
            }
            if (type == typeof(Toggle) || type.IsSubclassOf(typeof(Toggle)))
            {
                (component as Toggle).onValueChanged.AddListener((bool isOn) => {
                    if (isOn || !(component as Toggle).group)
                    {
                        if (customSound)
                            SoundManager.Instance.ToggleSound(isOn, onSound, offSound);
                        else
                            SoundManager.Instance.ToggleSound(isOn);
                    }
                });
            }
        }
        catch (System.Exception e)
        {

        }
    }
}
