using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    private Animator anim;
    private bool zoomAnimationing = false;
    public bool isPlayingZoomAnimation
    {
        get
        {
            return zoomAnimationing;
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
    }

    public void PlayZoomIn()
    {
        zoomAnimationing = true;
        anim.SetTrigger("zoom_in");
    }
    public void PlayZoomOut()
    {
        zoomAnimationing = false;
        anim.SetTrigger("zoom_out");
    }
}
