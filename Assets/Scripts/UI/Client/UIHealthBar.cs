using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

class UIHealthBar : MonoBehaviour
{

    private RectTransform _rt;
    private RectTransform _sparks;
    private Text _txt;
    

    private float _maxValue = 15;
    public float MaxValue
    {
        get
        {
            return _maxValue;
        }
        set
        {
            _txt.text = string.Format("{0:N2}", value);
            _maxValue = value;
        }
    }
    
    private bool _counting = false;
    private float _time;
    private float _value;

    public void UpdateCountdown(float value, bool counting)
    {
        Debug.Log("UpdateCountdown: " + value + " isBomb: " + counting);
        if (_counting == counting)
            return;
        _counting = counting;
        _sparks.gameObject.SetActive(counting);
        _value = value;
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
                _sparks.gameObject.SetActive(false);
            }
        }
        _time = Time.time;
    }
    
    void Update()
    {
        if (_counting)
        {
            float t = _value - (Time.time - _time); //current time
            if (t < 0)
                t = 0;

            //Slide fuse to the left
            Vector3 pos = _rt.anchoredPosition;
            pos.x = -_rt.rect.width * (1 - t/MaxValue);
            _rt.anchoredPosition = pos;
            pos.x += _rt.rect.width / 2;
            _sparks.anchoredPosition = pos;

            _txt.text = string.Format("{0:N2}", t);

        }
    }
    
}
