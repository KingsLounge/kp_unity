using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;
public class SpinePlayOnEnable : MonoBehaviour
{
    private SkeletonGraphic skeleton;
    public string animation = "";
    public bool loop = false;
    private bool init = false;

    private void Init()
    {
        if (init)
            return;
        init = true;
        skeleton = GetComponent<SkeletonGraphic>();
    }


    public void OnEnable()
    {
        Init();
        skeleton.AnimationState.SetAnimation(0, animation,loop);
    }
}
