using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed class UI_FloatParameter : MonoBehaviour
{

    [SerializeField] private Slider _slider;
    [SerializeField] private FloatParameter _parameter;

    private void OnEnable()
    {
        _slider.onValueChanged.AddListener(OnSliderValueChanged);
        _slider.value = Mathf.InverseLerp(_parameter.MinValue, _parameter.MaxValue, _parameter.Value);
    }

    private void OnDisable()
    {
        _slider.onValueChanged.RemoveListener(OnSliderValueChanged);
    }

    private void OnSliderValueChanged(float sliderValue)
    {
        _parameter.SetValue(Mathf.Lerp(_parameter.MinValue, _parameter.MaxValue, sliderValue));
    }

}
