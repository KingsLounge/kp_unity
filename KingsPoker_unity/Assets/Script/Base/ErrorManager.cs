using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

[System.Serializable]
public class ErrorPanel {
    public GameObject panel;
    public Text title;
    public Text contents;
    public Button[] buttons;
}
public class ErrorManager : MonoBehaviour
{
    private static ErrorManager instance;
    public static ErrorManager Instance {
        get {return instance;}
    }
    public List<ErrorPanel> errorPanels;
    private Image image;
    public delegate void ButtonClickBehaviour(ErrorPanel errorPanel);
    // Start is called before the first frame update
    void Awake()
    {
        image = GetComponent<Image>();
        instance = this;
    }
    public void Clear() {
        image.raycastTarget = false;
        for(int i = 0; i < errorPanels.Count; i ++) {
            errorPanels[i].panel.SetActive(false);
        }
    }

    public void OnError(string title, string contents) {
        OnError(title,contents,0);
    }
    public void OnError(string title,string contents,int panelNo) {
        List<ButtonClickBehaviour> behaviours = new List<ButtonClickBehaviour>();
        behaviours.Add(delegate(ErrorPanel panel) {
            Clear();
        });
        OnError(title,contents,0,behaviours);
    }
    public void OnError(string title,string contents,int panelNo, List<ButtonClickBehaviour> behaviours) {
        image.raycastTarget = true;
        ErrorPanel panel = errorPanels[panelNo];
        panel.title.text = title;
        panel.contents.text = contents;
        panel.panel.SetActive(true);
        for(int i = 0; i < behaviours.Count;i++) {
            panel.buttons[i].onClick.RemoveAllListeners();
            int j = i;
            panel.buttons[i].onClick.AddListener(delegate() {
                behaviours[j](panel);
            });
        }
    }
}
