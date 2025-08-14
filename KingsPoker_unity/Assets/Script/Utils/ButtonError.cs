using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ButtonError : MonoBehaviour
{
    public string title = "";
    [TextArea]
    public string contents = "";

    public void Start()
    {
        Button[] btns = GetComponents<Button>();
        for(int i = 0; i < btns.Length; i++)
        {
            Button btn = btns[i];
            btn.onClick = new Button.ButtonClickedEvent();
            btn.onClick.AddListener(()=> {
                ErrorMessageManager.Instance.AddGameError(0,title, contents);
            });
        }

        Toggle[] toggles = GetComponents<Toggle>();
        for (int i = 0; i < toggles.Length; i++)
        {
            Toggle toggle = toggles[i];
            toggle.onValueChanged = new Toggle.ToggleEvent();
            toggle.onValueChanged.AddListener((on) => {
                if(on){ErrorMessageManager.Instance.AddGameError(0, title, contents);};
                //toggle.isOn = !on;
            });
        }
    }
}
