using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OtherTouchClose : MonoBehaviour 
{
    // Start is called before the first frame update

    public bool onMouse = false;

    public void OnMouseEnter()
    {
        onMouse = true;
    }

    public void OnMouseExit()
    {
        onMouse = false;
    }
    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButtonDown(0) && !onMouse)
        {
            this.gameObject.SetActive(false);
        }
    }
}
