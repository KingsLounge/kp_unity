using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;

public class SpineCompleteAfterDisabler : MonoBehaviour
{
    public string animationName = "";


    public void Awake()
    {
        GetComponent<SkeletonGraphic>().AnimationState.Complete += (track) =>
        {
            if(track.Animation.Name == animationName)
            {
                gameObject.SetActive(false);
            }
        };
    }
}
