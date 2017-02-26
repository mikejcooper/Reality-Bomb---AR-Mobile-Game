using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

class UIHealthBar : MonoBehaviour
{

    private RectTransform _rt;
    private Text _txt;

    [HideInInspector]
    public float MinValue = 0;
    [HideInInspector]
    public float MaxValue = 15;

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
        Vector2 anchorMax = Vector2.one;
        anchorMax[0] = NormalizedValue;
        _rt.anchorMax = anchorMax;

        _txt.text = string.Format("{0:N2}", _value);
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
        }
    }
}
