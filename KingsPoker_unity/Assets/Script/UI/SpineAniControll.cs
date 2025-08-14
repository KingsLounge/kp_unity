using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;
using Spine;
using Event = Spine.Event;
public class SpineAniControll : MonoBehaviour
{
    [SerializeField]
    private SkeletonGraphic skeleton;
    public string nextScene = "Login_Astar";
    public float nextTime = 4f;
    private void Start() {
        //skeleton = GetComponent<SkeletonGraphic>();
        //skeleton.AnimationState.Complete += AniEvent;
        Invoke("NextScene", nextTime);
        skeleton.AnimationState.SetAnimation(0, "01", false);
        skeleton.AnimationState.AddAnimation(0, "02", false, 0);
        skeleton.AnimationState.AddAnimation(0, "03", false, 0);
    }

    public void NextScene()
    {
        CustomSceneManager.LoadScene(nextScene);
    }

    public void AniEvent(TrackEntry trackEntry)
    {
        //if(trackEntry.OnEnd)
        Debug.Log(trackEntry.ToString());
        switch(trackEntry.ToString())
        {
            case "01":
                skeleton.AnimationState.SetAnimation(0, "02", false);
            break;
            case "02":
                skeleton.AnimationState.SetAnimation(0, "03", false);
            break;
            case "03":
                CustomSceneManager.LoadScene(nextScene);
            break;

        }
       
    }
}
