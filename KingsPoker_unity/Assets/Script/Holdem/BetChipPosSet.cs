using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BetChipPosSet : MonoBehaviour
{
    private void OnEnable() {
        var child = transform.GetChild(0);
        child.localPosition = new Vector3(Random.Range(-4f,4f), 0f, 0f);
    }
}
