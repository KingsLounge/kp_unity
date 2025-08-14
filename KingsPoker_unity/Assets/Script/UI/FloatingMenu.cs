using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FloatingMenu : MonoBehaviour
{
    [SerializeField]
    private Button button;
    [SerializeField]
    private Button closeButton;
    [SerializeField]
    private RectTransform pannel;
    [SerializeField]
    private RectTransform screenGuard;
    private RectTransform originalParent = null;
    public Vector2 closeSize = Vector2.zero;
    public Vector2 openSize = Vector2.zero;
    private float _fillAmount = 0;
    private float fillAmount
    {
        get
        {
            return _fillAmount;
        }
        set
        {
            _fillAmount = value;
            pannel.sizeDelta = (closeSize + (openSize - closeSize) * _fillAmount);
        }
    }
    public float openTime = 0.5f;
    private bool isOpen = false;
    private GraphicRaycaster gr;

    private void Awake()
    {
        gr = GetComponentInParent<GraphicRaycaster>();
    }

    protected void Start()
    {
        originalParent = transform.parent as RectTransform;
        if (button)
        {
            button.onClick.AddListener(OnClick);
        }
        if (closeButton)
        {
            closeButton.onClick.AddListener(OnClick);
        }
    }

    private void OnClick()
    {
        StopAllCoroutines();
        if (isOpen)
            Close(false);
        else
            Open(false);
    }


    public void Close(bool direct)
    {
        if (isOpen)
            StartCoroutine(CloseFloating(direct));
    }

    public void Open(bool direct)
    {
        if(!isOpen)
            StartCoroutine(OpenFloating(direct));
    }

    private void OnStartOpen()
    {
        if(closeButton)
        {
            closeButton.gameObject.SetActive(true);
            button.gameObject.SetActive(false);
        }
        if(screenGuard)
        {
            screenGuard.gameObject.SetActive(true);
            transform.SetParent(screenGuard, true);
        }
    }

    private void OnStartClose()
    {
        if (closeButton)
        {
            closeButton.gameObject.SetActive(false);
            button.gameObject.SetActive(true);
        }
    }

    private void OnCompleteOpen()
    {

    }

    private void OnCompleteClose()
    {
        if (screenGuard)
        {
            screenGuard.gameObject.SetActive(false);
            transform.SetParent(originalParent, true);
        }
    }

    private IEnumerator OpenFloating(bool direct)
    {
        OnStartOpen();
        isOpen = true;
        float duration = fillAmount;
        while(duration < 1)
        {
            if(direct)
            {
                duration = 1;
            }
            else
            {
                duration += Time.deltaTime / openTime;
                duration = duration.Clamp(0, 1);
            }
            fillAmount = duration;
            if(duration < 1)
                yield return null;
        }
        OnCompleteOpen();
    }

    private IEnumerator CloseFloating(bool direct)
    {
        OnStartClose();
        isOpen = false;
        float duration = fillAmount;
        while (duration > 0)
        {
            if (direct)
            {
                duration = 0;
            }
            else
            {
                duration -= Time.deltaTime / openTime;
                duration = duration.Clamp(0, 1);
            }
            fillAmount = duration;
            if (duration > 0)
                yield return null;
        }
        OnCompleteClose();
    }

    public void Update()
    {
        if(Input.GetMouseButtonDown(0) && isOpen)
        {
            PointerEventData ped = new PointerEventData(null);
            ped.position = Input.mousePosition;
            List<RaycastResult> results = new List<RaycastResult>();
            gr.Raycast(ped, results);
            bool clickButton = false;
            bool clickPannel = false;
            for(int i = 0; i < results.Count; i++)
            {
                if (results[i].gameObject == button.gameObject)
                    clickButton = true;
                if (results[i].gameObject == pannel.gameObject)
                    clickPannel = true;
            }
            if (!clickButton && !clickPannel)
            {
                OnClick();
            }
        }
    }
}
