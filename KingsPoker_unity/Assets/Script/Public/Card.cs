using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Card : MonoBehaviour
{
    private Sprite backSprite = null;
    private Sprite frontSprite = null;
    private Sprite rabbitSprite = null;
    private Sprite currentBackSprite
    {
        get { return rabbitMode ? rabbitSprite : backSprite; }
    }

    [SerializeField]
    private Image img;
    private bool isBack = false;
    public Animator animator;
    private string curCard = "**";

    [SerializeField]
    public GameObject mark;
    private bool init = false;
    public Image highlightCover;
    public Sprite[] highlightTextures = null;
    private bool _highlighting = false;
    public bool highlighting
    {
        get { return _highlighting; }
    }
    public float highlightY = 20f;
    private int cardSetIdx = 0;
    public Color grayColor = Color.gray;
    private bool _gray = false;

    [SerializeField]
    private Transform cardTransform;
    public UnityAction onClick;
    public bool gray
    {
        get { return _gray; }
        set
        {
            if (!init)
                Init();
            if (_gray != value)
            {
                _gray = value;
                img.color = _gray ? grayColor : Color.white;
            }
        }
    }
    public bool isMiniCard = false;
    private bool rabbitMode = false;

    private void Init()
    {
        if (init)
            return;

        init = true;
        if (CardTextureSetter.Instance != null)
        {
            cardSetIdx = CardTextureSetter.Instance.cardSetIndex;
            //SetCard(curCard);
        }
        //img = GetComponent<Image>();
        animator = GetComponent<Animator>();
        backSprite = CardSets.GetCard("**", cardSetIdx, isMiniCard);
        rabbitSprite = CardSets.GetCard("rabbit", cardSetIdx, isMiniCard);
        frontSprite = backSprite;
        img.sprite =
            Mathf.Abs(transform.rotation.eulerAngles.y) <= 90 ? frontSprite : currentBackSprite;
        img.color = _gray ? grayColor : Color.white;
    }

    public void Start()
    {
        Init();
    }

    public void RabbitMode(bool active)
    {
        Init();
        rabbitMode = active;
        transform.rotation = Quaternion.Euler(0f, 180f, 0f);
        img.sprite =
            Mathf.Abs(transform.rotation.eulerAngles.y) <= 90 ? frontSprite : currentBackSprite;
    }

    IEnumerator Highlight_(bool enable, int type = 0)
    {
        //var pos = transform.localPosition;
        //pos.y += enable ? highlightY : -highlightY;
        //transform.localPosition = pos;
        var tr = cardTransform ? cardTransform : transform;
        float duration = 0;
        Vector3 startPos = enable ? Vector3.zero : Vector3.up * highlightY;
        float time = 0.2f;
        Vector3 endPos = enable ? Vector3.up * highlightY : Vector3.zero;
        while (duration < 1f)
        {
            duration = (duration + (Time.deltaTime / time)).Clamp(0f, 1f);
            tr.localPosition = Vector3.Lerp(startPos, endPos, duration);

            if (transform.gameObject.activeSelf == false)
            {
                Debug.Log("if(transform.gameObject.activeSelf === false)");
            }

            yield return null;
        }

        tr.localPosition = endPos;

        yield break;
    }

    public void Highlight(bool enable, int type = 0, bool direct = false)
    {
        if (highlightCover)
        {
            highlightCover.sprite = highlightTextures[type];
            highlightCover.gameObject.SetActive(enable);
        }

        if (_highlighting != enable)
        {
            _highlighting = enable;

            if (!direct && gameObject.activeInHierarchy)
            {
                StartCoroutine(Highlight_(enable, type));
            }
            else
            {
                var tr = cardTransform ? cardTransform : transform;
                Vector3 pos = enable ? Vector3.zero : Vector3.up * highlightY;
                tr.localPosition = pos;
            }
        }
    }

    public void PlayIdle()
    {
        animator.Play("Idle", -1, 0);
    }

    public void SetAlpha(float range)
    {
        Color newColor = img.color;
        newColor.a = range;
        img.color = newColor;
    }

    public void SetCard(string card, bool front = true)
    {
        if (!init)
        {
            Init();
        }
        ResetCard();
        frontSprite = CardSets.GetCard(card, cardSetIdx, isMiniCard);
        curCard = card;
        Vector3 eulerAngle = transform.rotation.eulerAngles;
        eulerAngle.y = front ? 0f : -180f;
        isBack = !front;
        transform.rotation = Quaternion.Euler(eulerAngle);
        img.sprite =
            curCard == "**"
                ? currentBackSprite
                : (
                    Mathf.Abs(transform.rotation.eulerAngles.y) <= 90
                        ? frontSprite
                        : currentBackSprite
                );
    }

    public void Flip(bool skipAni = false)
    {
        if (gameObject.activeSelf)
        {
            if (skipAni)
            {
                animator.Play("CardFlip", 0, 1f);
            }
            else
            {
                animator.Play("CardFlip");
            }
        }
        if (!skipAni)
        {
            SoundManager.Instance.PlayEffectSound(Sound_TableEnum.SFX_CARD_OPEN);
        }

        //Update();
    }

    public void Flip(UnityAction callback)
    {
        if (gameObject.activeSelf)
        {
            //Flip();
        }
        Flip();
        //TODO:애니메이션 끝나고 콜백 호출되도록 변경 필요
        callback();
    }

    public void Comeback(Vector3 startPos, float time = 1f)
    {
        StartCoroutine(TryComeback(startPos, transform.position, time));
    }

    private IEnumerator TryComeback(Vector3 startPos, Vector3 endPos, float time)
    {
        float duration = 0;
        while (duration < 1f)
        {
            duration = (duration + (Time.deltaTime / time)).Clamp(0f, 1f);
            transform.position = Vector3.Lerp(startPos, endPos, duration);
            yield return null;
        }
    }

    public string GetCardData()
    {
        return curCard;
    }

    private void ResetCard()
    {
        if (animator)
        {
            PlayIdle();
        }
        var tr = cardTransform ? cardTransform : transform;
        tr.localRotation = Quaternion.identity;
        tr.localScale = Vector3.one;
        tr.localPosition = Vector3.zero;
        gray = false;
    }

    // Update is called once per frame
    void Update()
    {
        bool front = Mathf.Abs(transform.rotation.eulerAngles.y) <= 90;
        if (front && isBack)
        {
            img.sprite = curCard == "**" ? currentBackSprite : frontSprite;
            isBack = false;
        }
        else if (!front && !isBack)
        {
            img.sprite = currentBackSprite;
            isBack = true;
        }
    }

    public void OnClickCard()
    {
        onClick?.Invoke();
    }

    private void OnDisable()
    {
        curCard = "**";
        ResetCard();
    }
}
