using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[ExecuteAlways]
public class DualSlider : MonoBehaviour, IPointerDownHandler, IDragHandler
{
    [SerializeField]
    private Text minText;
    [SerializeField]
    private Text maxText;
    [SerializeField]
    [InspectorName("Text Format")]
    private string _textFormat = "{0}";
    public delegate string TextFormatter(double value);
    public TextFormatter textFormatter = null;
    [SerializeField]
    private RectTransform handle1 = null;
    [SerializeField]
    private RectTransform handle2 = null;
    [SerializeField]
    private RectTransform fillRect = null;
    [SerializeField]
    [InspectorName("Min Limit")]
    private double _minLimit = 0;
    [SerializeField]
    [InspectorName("Max Limit")]
    private double _maxLimit = 0;
    
    private double p_value1 = 0;
    private double p_value2 = 0;
    private double p_minLimit = 0;
    private double p_maxLimit = 0;
    [SerializeField]
    private double value1 = 0;
    [SerializeField]
    private double value2 = 0;
    public double minValue
    {
        get
        {
            return value1 < value2 ? value1 : value2;
        }
    }
    public double maxValue
    {
        get
        {
            return value1 < value2 ? value2 : value1;
        }
    }

    public double minLimit
    {
        get
        {
            return _minLimit;
        }
    }

    public double maxLimit
    {
        get
        {
            return _maxLimit;
        }
    }

    public string textFormat
    {
        get
        {
            return _textFormat;
        }

        set
        {
            _textFormat = value;
            UpdateText();
        }
    }

    private bool handling1 = false;
    private bool handling2 = false;
    public bool handling
    {
        get
        {
            return handling1 || handling2;
        }
    }


    public Slider.Direction direction = Slider.Direction.LeftToRight;

    private RectTransform myRect = null;

    private void Awake()
    {
        SetValues(value1, value2);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        GameObject hitObject = eventData.pointerPressRaycast.gameObject;
        handling1 = hitObject == handle1.gameObject || hitObject == fillRect.gameObject;
        handling2 = hitObject == handle2.gameObject || hitObject == fillRect.gameObject;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if(handling1)
        {
            Rect parentRect = (handle1.parent as RectTransform).rect;
            Vector3 pos = handle1.anchoredPosition;
            if (direction == Slider.Direction.LeftToRight || direction == Slider.Direction.RightToLeft)
            {
                pos.x = (pos.x + eventData.delta.x).Clamp(-parentRect.width / 2, parentRect.width / 2);
            }
            else
            {
                pos.y = (pos.y + eventData.delta.y).Clamp(-parentRect.height / 2, parentRect.height / 2); 
            }
            handle1.anchoredPosition = pos;

            Rect rect = (handle1.parent as RectTransform).rect;
            if (direction == Slider.Direction.LeftToRight)
            {
                double fillAmount = (handle1.anchoredPosition.x + (rect.width / 2)) / rect.width;
                p_value1 = value1 = ((_maxLimit - _minLimit) * fillAmount) + _minLimit;
            }
            else if (direction == Slider.Direction.RightToLeft)
            {
                double fillAmount = (rect.width - (handle1.anchoredPosition.x + (rect.width / 2))) / rect.width;
                p_value1 = value1 = ((_maxLimit - _minLimit) * fillAmount) + _minLimit;
            }
            else if (direction == Slider.Direction.BottomToTop)
            {
                double fillAmount = (handle1.anchoredPosition.y + (rect.height / 2)) / rect.height;
                p_value1 = value1 = ((_maxLimit - _minLimit) * fillAmount) + _minLimit;
            }
            else if (direction == Slider.Direction.TopToBottom)
            {
                double fillAmount = (rect.height - (handle1.anchoredPosition.y + (rect.height / 2))) / rect.height;
                p_value1 = value1 = ((_maxLimit - _minLimit) * fillAmount) + _minLimit;
            }
        }
        if(handling2)
        {
            Rect parentRect = (handle2.parent as RectTransform).rect;
            Vector3 pos = handle2.anchoredPosition;
            if (direction == Slider.Direction.LeftToRight || direction == Slider.Direction.RightToLeft)
            {
                pos.x = (pos.x + eventData.delta.x).Clamp(-parentRect.width / 2, parentRect.width / 2);
            }
            else
            {
                pos.y = (pos.y + eventData.delta.y).Clamp(-parentRect.height / 2, parentRect.height / 2);
            }
            handle2.anchoredPosition = pos;

            Rect rect = (handle2.parent as RectTransform).rect;
            if (direction == Slider.Direction.LeftToRight)
            {
                double fillAmount2 = (handle2.anchoredPosition.x + (rect.width / 2)) / rect.width;
                p_value2 = value2 = ((_maxLimit - _minLimit) * fillAmount2) + _minLimit;
            }
            else if (direction == Slider.Direction.RightToLeft)
            {
                double fillAmount2 = (rect.width - (handle2.anchoredPosition.x + (rect.width / 2))) / rect.width;
                p_value2 = value2 = ((_maxLimit - _minLimit) * fillAmount2) + _minLimit;
            }
            else if (direction == Slider.Direction.BottomToTop)
            {
                double fillAmount2 = (handle2.anchoredPosition.y + (rect.height / 2)) / rect.height;
                p_value2 = value2 = ((_maxLimit - _minLimit) * fillAmount2) + _minLimit;
            }
            else if (direction == Slider.Direction.TopToBottom)
            {
                double fillAmount2 = (rect.height - (handle2.anchoredPosition.y + (rect.height / 2))) / rect.height;
                p_value2 = value2 = ((_maxLimit - _minLimit) * fillAmount2) + _minLimit;
            }
        }
        if (handling2 || handling1)
        {
            UpdateText();
            FillRectUpdate();
        }
    }

    public void SetValues(double min, double max)
    {
        min = min.Clamp(_minLimit, _maxLimit);
        max = max.Clamp(_minLimit, _maxLimit);
        p_value1 = value1 = min;
        p_value2 = value2 = max;
        double minFillAmount = (min - _minLimit) / (_maxLimit - _minLimit);
        double maxFillAmount = (max - _minLimit) / (_maxLimit - _minLimit);
        RectTransform minHandle = handle1;//GetMinHandle();
        RectTransform maxHandle = handle2;//GetMaxHandle();
        Vector3 minHandlePos = minHandle.anchoredPosition;
        Vector3 maxHandlePos = maxHandle.anchoredPosition;
        Rect minParentRect = (minHandle.parent as RectTransform).rect;
        Rect maxParentRect = (maxHandle.parent as RectTransform).rect;
        if (direction == Slider.Direction.LeftToRight)
        {
            minHandlePos.x = (float)((minFillAmount * minParentRect.width) - (minParentRect.width / 2));
            maxHandlePos.x = (float)((maxFillAmount * maxParentRect.width) - (maxParentRect.width / 2));
        }
        else if (direction == Slider.Direction.RightToLeft)
        {
            minHandlePos.x = (float)(((1f - minFillAmount) * minParentRect.width) - (minParentRect.width / 2));
            maxHandlePos.x = (float)(((1f - maxFillAmount) * maxParentRect.width) - (maxParentRect.width / 2));
        }
        else if (direction == Slider.Direction.BottomToTop)
        {
            minHandlePos.y = (float)((minFillAmount * minParentRect.height) - (minParentRect.height / 2));
            maxHandlePos.y = (float)((maxFillAmount * maxParentRect.height) - (maxParentRect.height / 2));
        }
        else if (direction == Slider.Direction.TopToBottom)
        {
            minHandlePos.y = (float)(((1f - minFillAmount) * minParentRect.height) - (minParentRect.height / 2));
            maxHandlePos.y = (float)(((1f - maxFillAmount) * maxParentRect.height) - (maxParentRect.height / 2));
        }
        minHandle.anchoredPosition = minHandlePos;
        maxHandle.anchoredPosition = maxHandlePos;
        UpdateText();
        FillRectUpdate();
    }

    public void SetLimits(double min, double max)
    {
        p_minLimit = _minLimit = min;
        p_maxLimit = _maxLimit = max;
        SetValues(value1, value2);
    }

    private void OnRectTransformDimensionsChange()
    {
        SetValues(value1, value2);
    }

    private void Update()
    {
        if(Input.GetMouseButtonUp(0))
        {
            handling1 = false;
            handling2 = false;
        }
        if (p_minLimit != _minLimit || p_maxLimit != _maxLimit)
            SetLimits(_minLimit, _maxLimit);
        else if (p_value1 != value1 || p_value2 != value2)
            SetValues(value1, value2);

        if (fillRect && fillRect.hasChanged)
            FillRectUpdate();
    }


    private RectTransform GetMinHandle()
    {
        try
        {
            if (direction == Slider.Direction.LeftToRight)
                return handle1.anchoredPosition.x <= handle2.anchoredPosition.x ? handle1 : handle2;
            else if (direction == Slider.Direction.RightToLeft)
                return handle1.anchoredPosition.x <= handle2.anchoredPosition.x ? handle2 : handle1;
            else if (direction == Slider.Direction.BottomToTop)
                return handle1.anchoredPosition.y <= handle2.anchoredPosition.y ? handle1 : handle2;
            else if (direction == Slider.Direction.BottomToTop)
                return handle1.anchoredPosition.y <= handle2.anchoredPosition.y ? handle2 : handle1;
        }
        catch
        {
            return null;
        }
        return null;
    }

    private RectTransform GetMaxHandle()
    {
        try
        {
            if (direction == Slider.Direction.LeftToRight)
                return handle1.anchoredPosition.x <= handle2.anchoredPosition.x ? handle2 : handle1;
            else if (direction == Slider.Direction.RightToLeft)
                return handle1.anchoredPosition.x <= handle2.anchoredPosition.x ? handle1 : handle2;
            else if (direction == Slider.Direction.BottomToTop)
                return handle1.anchoredPosition.y <= handle2.anchoredPosition.y ? handle2 : handle1;
            else if (direction == Slider.Direction.BottomToTop)
                return handle1.anchoredPosition.y <= handle2.anchoredPosition.y ? handle1 : handle2;
        }
        catch
        {
            return null;
        }
        return null;
    }

    public void FillRectUpdate()
    {
        if (fillRect == null) return;

        fillRect.pivot = new Vector2(0, 0.5f);
        if(direction == Slider.Direction.LeftToRight)
        {
            RectTransform left = GetMinHandle();
            RectTransform right = GetMaxHandle();
            fillRect.anchoredPosition = left.anchoredPosition;
            Vector2 size = fillRect.sizeDelta;
            size.x = right.anchoredPosition.x - left.anchoredPosition.x;
            fillRect.sizeDelta = size;
        }
        else if(direction == Slider.Direction.RightToLeft)
        {
            RectTransform left = GetMaxHandle();
            RectTransform right = GetMinHandle();
            fillRect.anchoredPosition = left.anchoredPosition;
            Vector2 size = fillRect.sizeDelta;
            size.x = right.anchoredPosition.x - left.anchoredPosition.x;
            fillRect.sizeDelta = size;
        }
        else if (direction == Slider.Direction.BottomToTop)
        {
            RectTransform bottom = GetMinHandle();
            RectTransform top = GetMaxHandle();
            fillRect.anchoredPosition = bottom.anchoredPosition;
            Vector2 size = fillRect.sizeDelta;
            size.y = top.anchoredPosition.y - bottom.anchoredPosition.y;
            fillRect.sizeDelta = size;
        }
        else if (direction == Slider.Direction.TopToBottom)
        {
            RectTransform bottom = GetMaxHandle();
            RectTransform top = GetMinHandle();
            fillRect.anchoredPosition = bottom.anchoredPosition;
            Vector2 size = fillRect.sizeDelta;
            size.y = top.anchoredPosition.y - bottom.anchoredPosition.y;
            fillRect.sizeDelta = size;
        }
        fillRect.hasChanged = false;
    }


    private void UpdateText()
    {
        if (minText)
            minText.text = textFormatter == null ? string.Format(textFormat, minValue) : textFormatter.Invoke(minValue);
        if (maxText)
            maxText.text = textFormatter == null ? string.Format(textFormat, maxValue) : textFormatter.Invoke(maxValue);
    }
}
