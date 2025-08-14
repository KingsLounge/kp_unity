using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[System.Serializable]
public enum Button3DState
{
    None,
    Normal,
    Over,
    Pressed,
    Disable
}
[System.Serializable]
public enum Button3DTransition
{
    None,
    Animation
}
public class Button3D : MonoBehaviour
{
    private bool enter = false;
    private bool down = false;
    public bool interactable = true;
    private Button3DState prevState = Button3DState.None;
    public Button3DTransition transition = Button3DTransition.Animation;
    private Animation anim;
    public string normalAnimation = "";
    public string overAnimation = "";
    public string pressedAnimation = "";
    public string disabledAnimation = "";
    public Button.ButtonClickedEvent onClick;
    public EventSystem eventSystem;
    // Start is called before the first frame update

    private void Awake()
    {
        eventSystem = GameObject.Find("EventSystem").GetComponent<EventSystem>();
        anim = GetComponent<Animation>();
    }
    void Start()
    {
        Normal();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButtonUp(0))
        {
            down = false;
        }
        if(interactable)
        {
            if (!enter)
            {
                if (!down)
                {
                    Normal();
                }
                else
                {
                    Pressed();
                }
            }
            else
            {
                if (down)
                {
                    Pressed();
                }
                else
                {
                    Over();
                }
            }
        }
        else
        {
            Disable();
        }
    }

    private void Normal()
    {
        if (prevState == Button3DState.Normal) return;
        prevState = Button3DState.Normal;
        anim.Play(normalAnimation);
    }

    private void Pressed()
    {
        if (prevState == Button3DState.Pressed) return;
        prevState = Button3DState.Pressed;
        anim.Play(pressedAnimation);
    }

    private void Over()
    {
        if (prevState == Button3DState.Over) return;
        prevState = Button3DState.Over;
        anim.Play(overAnimation);
    }

    private void Disable()
    {
        if (prevState == Button3DState.Disable) return;
        prevState = Button3DState.Disable;
        anim.Play(disabledAnimation);
    }

    private void OnMouseDown()
    {
        if (eventSystem.IsPointerOverGameObject()) return;
        down = true;
    }

    private void OnMouseUp()
    {
        if (eventSystem.IsPointerOverGameObject()) return;
        if (interactable && enter)
        {
            onClick.Invoke();
        }
    }

    private void OnMouseEnter()
    {
        if (eventSystem.IsPointerOverGameObject()) return;
        enter = true;
    }

    private void OnMouseExit()
    {
        if (eventSystem.IsPointerOverGameObject()) return;
        enter = false;
    }
}
