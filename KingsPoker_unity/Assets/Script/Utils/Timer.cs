using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{
    [System.Serializable]
    public enum GaugeType
    {
        Filled,
        Horizontal,
        Vertical
    }
    public delegate void Callback();
    private double _time = 0;
    public double time
    {
        get => _time;
    }
    private double _maxTime = 0;
    public double maxTime
    {
        get => _maxTime;
    }
    private Callback callback = null;
    private System.Action<float> progress = null;
    public bool autoActivate = true;
    public delegate string TextFormatter(double time);
    [SerializeField]
    [InspectorName("Text Format")]
    private string _textFormat = "{0:D1}";
    public string textFormat
    {
        get => textFormat;
        set {
            _textFormat = value;
            textFormatter = null;
        }
    }
    public TextFormatter textFormatter = null;
    public Text label = null;
    public Image gaugeBar;
    private bool playing = false;
    private Vector2 originalSize;
    private Rect rect;
    public GaugeType type = GaugeType.Filled;

    private float fillAmount
    {
        get
        {
            if (gaugeBar == null) return 0;

            if (type == GaugeType.Filled)
                return gaugeBar.fillAmount;
            else if (type == GaugeType.Horizontal)
                return gaugeBar.rectTransform.rect.width / originalSize.x;
            else if (type == GaugeType.Vertical)
                return gaugeBar.rectTransform.rect.height / originalSize.y;
            return 0;
        }
        set
        {
            if (gaugeBar != null)
            {
                Init();

                if (type == GaugeType.Filled)
                    gaugeBar.fillAmount = value;
                else if (type == GaugeType.Horizontal)
                    gaugeBar.rectTransform.sizeDelta = new Vector2(originalSize.x * value, originalSize.y);
                else if (type == GaugeType.Vertical)
                    gaugeBar.rectTransform.sizeDelta = new Vector2(originalSize.x, originalSize.y * value);
            }
        }
    }

    public bool isPlaying
    {
        get
        {
            return playing;
        }
    }

    private void Awake()
    {
        Init();
        // Rect rect = gaugeBar.rectTransform.rect;
        // originalSize = new Vector2(rect.width, rect.height);
    }

    private bool init = false;
    private void Init()
    {
        if (init) return;
        init = true;
        if (gaugeBar != null)
        {
            Rect rect = gaugeBar.rectTransform.rect;
            originalSize = new Vector2(rect.width, rect.height);
        }
        
    }

    void Update() {
        if(!playing)
            return;
        if(label != null) {
            label.text = textFormatter == null ? ((long)_time).ToString() : textFormatter.Invoke(this._time);
        }
        if(gaugeBar != null) {
            fillAmount = ((float)(_time / _maxTime)).Clamp(0f,1f);
        }
        if(_time > 0) {
            _time -= Time.deltaTime;
            progress?.Invoke(((float)(_time / _maxTime)).Clamp(0f, 1f));
            if (_time <= 0) {
                if(autoActivate) {
                    gameObject.SetActive(false);
                }
                if(callback != null) {
                    callback();
                }
            }            
        }
    }

    public void Clear() {
        _time = 0;
        callback = null;
        playing = false;
        if(autoActivate)
            gameObject.SetActive(false);
    }

    public void Stop() {
        playing = false;
        if(autoActivate)
        {
            gameObject.SetActive(false);
        }
    }

    public void Resume() {
        playing = true;
    }

    public void SetTimer(bool playing, double time) {
        if(autoActivate)
            gameObject.SetActive(true);
        _maxTime = time;
        _time = time;
        this.playing = playing;

        SoundManager.Instance.PlayEffectSound(Sound_TableEnum.SFX_BC_BETSTART);
        // SoundManager.Instance.PlayEffectSound(Sound_TableEnum.SFX_BC_TIMERCOUNT,true);   
    }

    public void SetTimer(bool playing, double time, Callback callback, System.Action<float> progress = null) {
        SetTimer(playing, time);
        this.callback = callback;
        this.progress = progress;
    }

    public void SetTimer(bool playing, double time, double maxTime)
    {
        SetTimer(playing, time);
        _maxTime = maxTime;

        // 최초 한번은 세팅을 해야 한다. Alberto 2021-04-18
        if (label != null)
        {
            label.text = textFormatter == null ? string.Format(textFormat,_time) : textFormatter.Invoke(_time);
        }
        if (gaugeBar != null)
        {
            fillAmount = ((float)(_time / _maxTime)).Clamp(0f, 1f);
        }

    }
}
