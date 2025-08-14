using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VariableText : Text
{
    // Update is called once per frame
    void Update()
    {
        if(Application.isPlaying) {
            string curStr = GetValueString();
            if(text != curStr) {
                text = curStr;
            }
        }
    }

    protected virtual string GetValueString() {
        return "";
    }
}
