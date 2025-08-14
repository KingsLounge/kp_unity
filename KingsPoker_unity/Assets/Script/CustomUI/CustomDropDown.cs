using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class CustomDropDown : Dropdown
{
    protected override GameObject CreateDropdownList(GameObject template)
    {
        template.GetComponent<Canvas>().sortingLayerName = "UI";
        return base.CreateDropdownList(template);
        
    }
}
