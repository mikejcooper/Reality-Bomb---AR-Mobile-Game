using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

class UIHealthBar : MonoBehaviour
{

    private RectTransform _rt;
    private RectTransform _sparks;
    private Text _txt;

    [HideInInspector]
    public float MinValue = 0;
    [HideInInspector]
    public float MaxValue = 15;
    private Vector3 _start, _end;
    //private float _speed;
    private bool _counting = false;
    private float _time;

    private float _value;
    public float Value
    {
        get
        {
            return _value;
        }
        set
        {
            _value = value;
            //UpdateVisuals();
        }
    }

    public float NormalizedValue
    {
        get
        {
            if (Mathf.Approximately(MinValue, MaxValue))
                return 0;
            return Mathf.InverseLerp(MinValue, MaxValue, Value);
        }
        set
        {
            this.Value = Mathf.Lerp(MinValue, MaxValue, value);
        }
    }

    public void UpdateCountdown(float value, bool counting)
    {
        if (this._counting == counting)
            return;
        this._counting = counting;
        this.Value = value;
        if(counting)
        {
            _time = Time.time;
        }
    }

    void Start()
    {
        foreach (Transform child in transform)
        {
            if (child.gameObject.name == "Fill")
            {
                _rt = child.GetComponent<RectTransform>();
            }
            if (child.gameObject.name == "Text")
            {
                _txt = child.GetComponent<Text>();
            }
            if (child.gameObject.name == "Sparks")
            {
                _sparks = child.GetComponent<RectTransform>();
            }
        }
        _start = _rt.anchoredPosition;
        _end = _start;
        _end.x = _start.x - _rt.rect.width;
        _time = Time.time;
    }
    
    void Update()
    {
        if (_counting)
        {
            float t = Value - (Time.time - _time); //current time
            if (t < 0)
                t = 0;
            Vector3 pos = _rt.anchoredPosition;
            pos.x = -_rt.rect.width * (1 - t/MaxValue);
            _rt.anchoredPosition = pos;
            pos.x += _rt.rect.width / 2;
            _sparks.anchoredPosition = pos;

            _txt.text = string.Format("{0:N2}", t);

        }
    }
    
}
