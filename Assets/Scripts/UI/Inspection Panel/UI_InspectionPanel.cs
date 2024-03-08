using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public sealed class UI_InspectionPanel : MonoBehaviour
{

    [SerializeField] private RectTransform _actionLabel;
    [SerializeField] private TextMeshProUGUI _label;
    [SerializeField] private Player _player;

    private InspectionPawn _inspector;

    private void OnEnable()
    {
        _player.PawnChanged += OnPawnChanged;
        _actionLabel.gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        _player.PawnChanged -= OnPawnChanged;

        if (_inspector != null)
            _inspector.ActionSelected -= OnActionSelected;
    }

    private void Update()
    {
        if (_inspector == null || _inspector.SelectedAction == null)
            return;

        var screenPos = Camera.main.WorldToScreenPoint(_inspector.SelectedAction.transform.position);
       // Vector3 uiPos = new Vector3(screenPos.x, Screen.height - screenPos.y, screenPos.z);
        _actionLabel.position = screenPos;
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
        }
    }

    private void OnActionSelected(InspectAction action)
    {
        _actionLabel.gameObject.SetActive(action != null);
        if (action != null)
            _label.text = $"[F] to {action.GetText(null)}";
    }

}
