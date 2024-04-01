using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DefaultExecutionOrder(Order.UI)]
public class UI_Slot : MonoBehaviour
{

    [SerializeField] private Image _itemImage;
    [SerializeField] private TextMeshProUGUI _numberLabel;

    private ExpItemSlot _targetSlot;

    public void SetTarget(ExpItemSlot slot)
    {
        _targetSlot = slot;
        _targetSlot.Changed += OnTargetSlotChanged;
        OnTargetSlotChanged(_targetSlot);
    }

    private void OnEnable()
    {
        if (_targetSlot != null)
            _targetSlot.Changed += OnTargetSlotChanged;
    }

    private void OnDisable()
    {
        _targetSlot.Changed -= OnTargetSlotChanged;
    }

    private void OnTargetSlotChanged(ExpItemSlot slot)
    {
        _itemImage.gameObject.SetActive(slot.IsEmpty == false);
        _numberLabel.gameObject.SetActive(slot.IsEmpty == false);

        if (slot.IsEmpty == false)
        {
            _itemImage.sprite = slot.Stack.Definition.Sprite;
            _numberLabel.enabled = slot.Stack.Count > 1;
            _numberLabel.text = slot.Stack.Count.ToString();
        }
    }

}
