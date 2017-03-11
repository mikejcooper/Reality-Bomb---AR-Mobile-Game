using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

class UIHealthBar : MonoBehaviour
{

    private RectTransform _rt;
    private GameObject _sparks;
    private Text _txt;
    private ParticleSystem[] _psystems;

    private float _maxWidth;
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

    //Should only be called for local player
    public void UpdateCountdown(float value, bool counting)
    {
        //Debug.Log("UpdateCountdown: " + value + " isBomb: " + counting);
        if (_counting == counting)
            return;
        _counting = counting;
        _value = value;
        if(counting)
        {
            Debug.Log("Playing");
            //_sparks.GetComponent<ParticleSystem>().Play();
            for (int i = 0; i < _psystems.Length; i++)
            {
                var em = _psystems[i].emission;
                em.enabled = true;
            }
            _time = Time.time;
        }
        else
        {
            Debug.Log("Pausing");
            //_sparks.GetComponent<ParticleSystem>().Pause();
            for (int i = 0; i < _psystems.Length; i++)
            {
                var em = _psystems[i].emission;
                em.enabled = false;
            }
        }
    }

    void Start()
    {
        foreach (Transform child in transform)
        {
            if (child.gameObject.name == "Fill")
            {
                _rt = child.GetComponent<RectTransform>();
                _sparks = child.GetChild(0).gameObject;
                Debug.Log("Pausing");
                _psystems = child.GetComponentsInChildren<ParticleSystem>();
                for (int i=0; i<_psystems.Length; i++)
                {
                    var em = _psystems[i].emission;
                    em.enabled = false;
                }
            }
            if (child.gameObject.name == "Text")
            {
                _txt = child.GetComponent<Text>();
            }
        }
        _time = Time.time;
        _maxWidth = _rt.rect.width;
    }
    
    void Update()
    {
        if (_counting)
        {
            float t = _value - (Time.time - _time); //current time
            if (t < 0)
                t = 0;

            //Slide fuse to the left
            _rt.sizeDelta = new Vector2(_maxWidth * t / MaxValue, _rt.rect.height);

            _txt.text = string.Format("{0:N2}", t);
        }
    }
    
}
