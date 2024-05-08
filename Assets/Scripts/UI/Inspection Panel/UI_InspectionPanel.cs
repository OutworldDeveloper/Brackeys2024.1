using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public sealed class UI_InspectionPanel : MonoBehaviour
{

    [SerializeField] private RectTransform _actionLabel;
    [SerializeField] private TextMeshProUGUI _buttonLabel;
    [SerializeField] private TextMeshProUGUI _interactionLabel;
    [SerializeField] private Player _player;

    private InspectionPawn _inspector;

    private void OnEnable()
    {
        _player.PawnStack.ActivePawnChanged += OnPawnChanged;
        _actionLabel.gameObject.SetActive(false);
        _buttonLabel.text = "[F]";
    }

    private void OnDisable()
    {
        _player.PawnStack.ActivePawnChanged -= OnPawnChanged;

        if (_inspector != null)
            _inspector.ActionSelected -= OnActionSelected;

        _actionLabel.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (_inspector == null || _inspector.SelectedAction == null)
            return;

        UpdatePosition(_inspector.SelectedAction);
    }

    private void OnPawnChanged(Pawn pawn)
    {
        if (pawn is InspectionPawn inspector)
        {
            _inspector = inspector;
            _inspector.ActionSelected += OnActionSelected;
        }
        else
        {
            if (_inspector != null)
                _inspector.ActionSelected -= OnActionSelected;
            _inspector = null;
            _actionLabel.gameObject.SetActive(false);
        }
    }

    private void OnActionSelected(InspectAction action)
    {
        _actionLabel.gameObject.SetActive(action != null);
        if (action != null)
            _interactionLabel.text = action.GetText(null);

        UpdatePosition(action);
    }

    private void UpdatePosition(InspectAction action)
    {
        if (action == null)
            return;

        var screenPos = Camera.main.WorldToScreenPoint(action.transform.position);
        _actionLabel.position = screenPos;
    }

}
