using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

class UIHealthBar : UIBehaviour
{

    public RectTransform rt;

    [HideInInspector]
    public int minValue = 0;
    public int maxValue = 15;

    [SerializeField]
    protected float _value;
    public virtual float value
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

    public float normalizedValue
    {
        get
        {
            if (Mathf.Approximately(minValue, maxValue))
                return 0;
            return Mathf.InverseLerp(minValue, maxValue, value);
        }
        set
        {
            this.value = Mathf.Lerp(minValue, maxValue, value);
        }
    }

    private void UpdateVisuals()
    {
        Vector2 anchorMax = Vector2.one;
        anchorMax[0] = normalizedValue;
        rt.anchorMax = anchorMax;
    }
}
