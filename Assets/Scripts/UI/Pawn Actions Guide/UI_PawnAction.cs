using System;
using TMPro;
using UnityEngine;

[DefaultExecutionOrder(Order.UI)]
public class UI_PawnAction : MonoBehaviour
{

    [SerializeField] private TextMeshProUGUI _mainKeyLabel;
    [SerializeField] private OptionalKey[] _optionalKeys;
    [SerializeField] private TextMeshProUGUI _actionLabel;

    private PawnAction _targetAction;

    public void SetTarget(PawnAction action)
    {
        _targetAction = action;
        _targetAction.StateChanged += OnActionStateChanged;

        _mainKeyLabel.text = action.Key.ToString();
        _actionLabel.text = action.Description;

        for (int i = 0; i < _optionalKeys.Length; i++)
        {
            bool isValidKey = i < action.AdditionalKeys.Length;
            _optionalKeys[i].Button.SetActive(isValidKey);
            if (isValidKey == true)
                _optionalKeys[i].Label.text = action.AdditionalKeys[i].ToString();
        }
    }

    private void OnActionStateChanged()
    {
        gameObject.SetActive(_targetAction.IsActive);
    }

    private void OnDestroy()
    {
        _targetAction.StateChanged -= OnActionStateChanged;
    }

    [Serializable]
    private struct OptionalKey
    {
        public GameObject Button;
        public TextMeshProUGUI Label;
    }

}
