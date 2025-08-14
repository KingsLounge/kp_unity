using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowDownCardEffector : MonoBehaviour
{
    private Animation anim;
    public AnimationClip[] animations;
    private bool init = false;
    // Start is called before the first frame update
    void Awake()
    {
        Init();
    }

    private void Init()
    {
        if(init)
        {
            return;
        }
        init = true;
        anim = GetComponent<Animation>();
        for (int i = 0; i < animations.Length; i++)
        {
            anim.AddClip(animations[i], animations[i].name);
        }
    }

    public void SetCard(string cardStr)
    {
        string[] card = cardStr.Split(',');
        SetCard(card);
    }

    public void SetCard(string[] card)
    {
        Init();
        for (int i = 0; i < card.Length; i++)
        {
            // cards[i].SetCard(card[i]);
        }
    }

    public void Highlight(string[] list, int type)
    {
        //for (int i = 0; i < cards.Length; i++)
        //{
        //    cards[i].Highlight(System.Array.IndexOf(list, cards[i].GetCardData()) != -1, type);
        //}
    }

    public void PlayAnimation()
    {
        Init();
        anim.Play(animations[Random.Range(0, animations.Length)].name);
    }
}
