using TMPro;
using UnityEngine;

[DefaultExecutionOrder(Order.UI)]
public class UI_PawnAction : MonoBehaviour
{

    [SerializeField] private UI_KeyHint[] _keyHints;
    [SerializeField] private TextMeshProUGUI _actionLabel;

    private PawnAction _targetAction;

    public void SetTarget(PawnAction action)
    {
        _targetAction = action;
        _targetAction.StateChanged += OnActionStateChanged;

        _actionLabel.text = action.Description;

        _keyHints[0].Show(action.Key);

        for (int i = 1; i < _keyHints.Length; i++)
        {
            bool isValidKey = i - 1 < action.AdditionalKeys.Length;

            _keyHints[i].gameObject.SetActive(isValidKey);

            if (isValidKey == true)
                _keyHints[i].Show(action.AdditionalKeys[i - 1]);
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

}
