using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CardFocusingWithAngle : CardFocusing
{
    public int cardWidth = 63;
    public int cardHeight = 88;
    public int columnAnimationIndex = 0;
    public int rowAnimationIndex = 1;
    public UISensor uiSensor;
    private bool rotationing = false;
    private bool fliping = false;
    public Vector3 dragPerRotate = new Vector3(0, 0, 0);
    private Vector3 beforeRot;
    private Vector3 originRot;
    public Transform rotateObject;
    private float smallAngle = 70;
    private float largeAngle;
    public enum Direction
    {
        None,
        Row,
        Column
    }
    public float calculateDirectionRange = 30f;
    private Direction dir = Direction.None;
    protected override void Awake()
    {
        base.Awake();
        originRot = rotateObject.eulerAngles;
        largeAngle = 180f - smallAngle;
        Play(Mathf.Infinity,null);
    }

    private AnimationState GetAnimationState(float y)
    {
        if(y <= (smallAngle / 2) && y >= (-smallAngle / 2))
        {
            return anim[clips[columnAnimationIndex].name];
        }
        else
        {
            return anim[clips[rowAnimationIndex].name];
        }
    }

    public void Play(float completeTime,System.Action<bool> complete)
    {
        isPlaying = true;
        forceBack = false;
        lastForceBack = false;
        rotateObject.eulerAngles = originRot;
        state = GetAnimationState(0);
        autoSkip = true;
        this.completeTime = completeTime;
        anim.Play(state.name);
        state.speed = 0;
        state.time = 0;
        callback?.Invoke(true);
        callback = complete;
    }

    public void NoAutoStopPlay()
    {
        isPlaying = true;
        forceBack = false;
        lastForceBack = false;
        rotateObject.eulerAngles = originRot;
        state = GetAnimationState(0);
        anim.Play(state.name);
        state.speed = 0;
        state.time = 0;
        callback = null;
    }

    public void Stop(System.Action<bool> callback = null)
    {
        if(callback != null)
        {
            this.callback = callback;
        }
        if (fliping)
        {
            ForceBack();
            lastForceBack = true;
        }
        else
        {
            Complete();
            return;
        }
    }

    // Update is called once per frame
    protected override void Update()
    {
        if (isPlaying)
        {
            if (autoSkip)
            {
                completeTime -= Time.deltaTime;
                if (completeTime <= 0f)
                {
                    completeTime = 0f;
                    autoSkip = false;
                    if(fliping)
                    {
                        ForceBack();
                        lastForceBack = true;
                    }
                    else
                    {
                        Complete();
                        return;
                    }
                }
            }
            if (!anim.isPlaying)
            {
                anim.Play(state.name);
                state.speed = 0;
            }
            if (dir == Direction.Row)
            {
                if (rotationing && Input.GetMouseButton(0))
                {
                    Vector2 mousePos = Input.mousePosition;
                    float distance = Vector2.Distance(downPos, mousePos) * (downPos.x < mousePos.x ? 1 : -1);
                    Vector3 angle = beforeRot + (dragPerRotate * distance);
                    while(angle.y <= -((smallAngle / 2) + largeAngle))
                    {
                        angle.y += 180f;
                    }
                    while(angle.y >= smallAngle / 2)
                    {
                        angle.y -= 180f;
                    }
                    var newState = GetAnimationState(angle.y);
                    if(state != newState)
                    {
                        state = newState;
                        anim.Play(state.name);
                        state.speed = 0;
                        state.time = 0;
                    }
                    rotateObject.eulerAngles = angle;
                }
                if (rotationing && Input.GetMouseButtonUp(0))
                {
                    dir = Direction.None;
                    rotationing = false;
                }
            }
            else if(dir == Direction.Column)
            {
                if (!forceBack)
                {
                    if (fliping && Input.GetMouseButton(0))
                    {
                        Vector2 mousePos = Input.mousePosition;
                        float duration = Mathf.Max(0, (mousePos.y - downPos.y) / secondPerY);
                        duration = Mathf.Min(duration, state.clip.length);
                        state.time = duration;
                    }
                    if (fliping && Input.GetMouseButtonUp(0))
                    {
                        ForceBack();
                    }
                }
                else
                {
                    if (state.time <= 0f)
                    {
                        state.time = 0;
                        forceBack = false;
                        fliping = false;
                        dir = Direction.None;
                        if (lastForceBack)
                        {
                            Complete();
                        }
                    }
                }
            }
            else
            {
                if (Input.GetMouseButtonDown(0))
                {
                    downPos = Input.mousePosition;
                }
                if(Input.GetMouseButton(0))
                {
                    Vector2 mousePosition = Input.mousePosition;
                    if (Vector2.Distance(downPos, mousePosition) >= calculateDirectionRange)
                    {
                        float xDis = Mathf.Abs(downPos.x - mousePosition.x);
                        float yDis = Mathf.Abs(downPos.y - mousePosition.y);
                        dir = xDis > yDis ? Direction.Row : Direction.Column;
                        if(dir == Direction.Column)
                        {
                            fliping = true;
                            downPos = mousePosition;
                        }
                        else if(dir == Direction.Row)
                        {
                            downPos = Input.mousePosition;
                            rotationing = true;
                            beforeRot = rotateObject.eulerAngles;
                        }
                    }
                }
            }
        }
        
    }
}
