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
    //private float _speed;
    //private bool moving = false;

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
            UpdateVisuals();
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

    private void UpdateVisuals()
    {
        //TODO: This is all a bit hacky...
        
        Vector3 pos = _rt.anchoredPosition;
        pos.x = - _rt.rect.width * (1-NormalizedValue);
        _rt.anchoredPosition = pos;
        pos.x += _rt.rect.width / 2;
        _sparks.anchoredPosition = pos;

        _txt.text = string.Format("{0:N2}", _value);
    }

    void Start()
    {
        //_speed = _rt.rect.width / MaxValue;
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
    }
    /*
    void Update()
    {
        if (_moving)
        {


        }
    }
    */
}
