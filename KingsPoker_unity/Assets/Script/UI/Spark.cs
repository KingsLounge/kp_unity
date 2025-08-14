using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Spine.Unity;

public class Spark : MonoBehaviour
{
    private SkeletonGraphic spine;
    private Image parentImg;
    public float range = 49;
    public float startAngle = -90.0f;
    
    // Start is called before the first frame update
    void Awake()
    {
        //spine = GetComponent<SkeletonGraphic>();
        //parentImg = transform.parent.GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        //float angle = (startAngle + 360.0f * parentImg.fillAmount) * Mathf.Deg2Rad;
        //float x = Mathf.Cos(angle);
        //float y = Mathf.Sin(angle);
        //transform.localPosition = new Vector3(x * range,y * range,0);
    }
}
