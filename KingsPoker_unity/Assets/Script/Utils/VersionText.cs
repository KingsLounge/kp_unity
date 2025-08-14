using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VersionText : MonoBehaviour
{
    private void Awake() {
        var text = GetComponent<Text>();
        text.text = string.Format("Ver_{0}", Application.version);
    }
}
