using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class URLOpener : MonoBehaviour
{
    private Button button;
    public string url = "http://gtest.fromthered.com/jackpot/test.html";

    void Awake() {
        this.button = this.GetComponent<Button>();
        this.button.onClick.AddListener(delegate() {
            Application.OpenURL(this.url);
        });
    }
}
