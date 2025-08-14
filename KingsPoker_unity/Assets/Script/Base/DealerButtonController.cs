using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DealerButtonController : MonoBehaviour
{
    public List<GameObject> containers = new List<GameObject>();
    public MagneticMover dealerButton = null;

    public void Move(int idx,bool direct = false)
    {
        if(direct)
        {
            Transform parent = dealerButton.transform.parent;
            dealerButton.transform.SetParent(containers[idx].transform);
            dealerButton.transform.localPosition = Vector3.zero;
            dealerButton.transform.SetParent(parent);
        }
        else
        {
            dealerButton.MoveStart(containers[idx].transform, 0.2f);
        }
    }
}
