using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagneticMover : MonoBehaviour
{
    private Vector3 startPos;
    private Quaternion startAngle;
    private bool started = false;
    private float distance = 0;
    private float time = 1;
    public delegate void Callback();
    private Callback callback = null;
    public bool rotationChange = true;
    public bool positionChange = true;
    private Vector3 rotateOffset = new Vector3();

    public void MoveStart(Transform endPos, float time, Callback callback = null)
    {
        transform.SetParent(endPos);
        startPos = transform.localPosition;
        startAngle = transform.localRotation;
        started = true;
        rotateOffset = Vector3.zero;
        distance = 0;
        this.time = time;
        this.callback = callback;
    }

    public void MoveStartWithSpeed(Transform endPos, float secondPerUnit, Callback callback = null)
    {
        float distance = Vector3.Distance(transform.position, endPos.position);
        float time = distance / secondPerUnit;
        MoveStart(endPos, time, callback);
    }

    public void MoveStart(Transform endPos,Vector3 rotateOffset, float time, Callback callback = null)
    {
        transform.SetParent(endPos);
        startPos = transform.localPosition;
        startAngle = transform.localRotation;
        started = true;
        this.rotateOffset = rotateOffset;
        distance = 0;
        this.time = time;
        this.callback = callback;
    }

    public void Update()
    {
        if(started)
        {
            distance += Time.deltaTime / time;
            if(distance >= 1f)
            {
                distance = 1;
            }
            if(positionChange)
            {
                transform.localPosition = Vector3.Lerp(startPos, Vector3.zero, distance);
            }
            if (rotationChange)
            {
                Quaternion angle = Quaternion.Euler(rotateOffset);
                transform.localRotation = Quaternion.Lerp(startAngle, angle, distance);
            }
            if (distance >= 1f && callback != null)
            {
                started = false;
                callback();
                callback = null;
            }
        }
    }
}
