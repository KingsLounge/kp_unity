using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardFocusing : MonoBehaviour
{
    protected Animation anim;
    protected AnimationState state;
    public AnimationClip[] clips;
    protected Vector2 downPos;
    public float secondPerY = 100f;
    protected bool isPlaying = false;
    protected System.Action<bool> callback = null;
    protected Vector2 uvSize = new Vector2(0.125f, -0.1428f);
    protected Vector2 uvGrid = new Vector2(8, 7);
    public SkinnedMeshRenderer[] cards;
    protected float completeTime = 0;
    protected bool autoSkip = false;
    protected bool forceBack = false;
    protected bool playNextFrame = false;
    protected bool lastForceBack = false;
    protected virtual void Awake()
    {
        anim = GetComponent<Animation>();
    }

    public bool IsPlaying()
    {
        return isPlaying;
    }

    protected Vector2 GetCardUV(string card)
    {
        char[] shapes = new char[4] { 's', 'd', 'h', 'c' };
        char[] numbers = new char[13] { '2', '3', '4', '5', '6', '7', '8', '9', 't', 'j', 'q', 'k', 'a' };
        char[] cardInfo = card.ToCharArray();
        int shapeIdx = System.Array.IndexOf(shapes, cardInfo[1]);
        int numberIdx = System.Array.IndexOf(numbers, cardInfo[0]);
        int idx = (shapeIdx * 13) + numberIdx;
        if (shapeIdx == -1 || numberIdx == -1)
        {
            idx = 53;
        }
        int x = idx % (int)uvGrid.x;
        int y = (idx / (int)uvGrid.x);
        Vector2 uv = new Vector2(x * uvSize.x, y * uvSize.y);
        return uv;
    }

    public void SetCard(string[] cards)
    {
        for(int i = 0; i < this.cards.Length; i++)
        {
            this.cards[i].materials[1].SetTextureOffset("_MainTex", GetCardUV(cards[i]));
        }
    }
    public void Play(int idx,System.Action<bool> complete = null)
    {
        isPlaying = true;
        forceBack = false;
        lastForceBack = false;
        state = anim[clips[idx].name];
        autoSkip = false;
        completeTime = 0;
        anim.Play(clips[idx].name);
        state.speed = 0;
        state.time = 0;
        callback?.Invoke(true);
        callback = complete;
    }

    public void Play(int idx,float completeTime, System.Action<bool> complete = null)
    {
        Play(idx,complete);
        autoSkip = true;
        this.completeTime = completeTime;
    }

    public void ForceBack()
    {
        if(isPlaying)
        {
            forceBack = true;
            state.speed = -10;
        }
    }
    protected void Complete()
    {
        isPlaying = false;
        forceBack = false;
        if(state)
        {
            state.time = 0;
            state.speed = 0;
        }
        var temp = callback;
        callback = null;
        temp?.Invoke(false);
    }
    protected virtual void Update()
    {
        if(isPlaying)
        {
            if(!anim.isPlaying)
            {
                anim.Play(state.name);
                state.speed = 0;
            }
            if(autoSkip)
            {
                completeTime -= Time.deltaTime;
                if (completeTime <= 0f)
                {
                    completeTime = 0f;
                    ForceBack();
                    autoSkip = false;
                    lastForceBack = true;
                }
            }

            if (!forceBack)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    downPos = Input.mousePosition;
                }
                if (Input.GetMouseButton(0))
                {
                    Vector2 mousePos = Input.mousePosition;
                    float duration = Mathf.Max(0, (mousePos.y - downPos.y) / secondPerY);
                    duration = Mathf.Min(duration, state.clip.length);
                    state.time = duration;
                }
                if (Input.GetMouseButtonUp(0))
                {
                    ForceBack();
                }
            }
            else
            {
                if(state.time <= 0f)
                {
                    state.time = 0;
                    forceBack = false;
                    if (lastForceBack)
                    {
                        Complete();
                    }
                }
            }
        }
    }
}
