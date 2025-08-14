using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;

public class SpineHelper : MonoBehaviour
{
    private SkeletonGraphic spine;
    // Start is called before the first frame update
    void Awake()
    {
        spine = GetComponent<SkeletonGraphic>();
        StartCoroutine(LoadAndDisable());
    }

    private IEnumerator LoadAndDisable()
    {
        spine.enabled = true;
        while(spine.AnimationState == null)
        {
            yield return 0;
        }
        gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
    }
}
