using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardEffector : MonoBehaviour
{
    private Animator anim;
    public delegate void AnimationEndCallback();
    private AnimationEndCallback callback;
    // Start is called before the first frame update
    void Awake()
    {
        anim = GetComponent<Animator>();
    }
    
    public void PlayAnim(string key,AnimationEndCallback callback) {
        anim.Play(key);
        this.callback = callback;
    }

    public void PlayAnim(string key) {
        anim.Play(key);
    }

    public void EndAnim() {
        if(callback != null)
            callback();
        Destroy(gameObject);
    }
}
