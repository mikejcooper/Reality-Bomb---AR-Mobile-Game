using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class TextureSlider : MonoBehaviour
{
    private RectTransform _rt;
    private RawImage _img;
    void Start()
    {
        _rt = GetComponent<RectTransform>();
        _img = GetComponent<RawImage>();
    }

    void Update()
    {
        var width = _rt.sizeDelta.x;
        var uvWidth = width / 60;
        var rect = _img.uvRect;
        rect.width = uvWidth;
        _img.uvRect = rect;
    }
}
