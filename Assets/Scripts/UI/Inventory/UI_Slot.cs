using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

[DefaultExecutionOrder(Order.UI)]
public class UI_Slot : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, IPointerUpHandler
{

    public event Action<UI_Slot> Selected;
    public event Action<UI_Slot> SelectedAlt;
    public event Action<UI_Slot> Hovered;
    public event Action<UI_Slot> Exited;

    [SerializeField] private Image _itemImage;
    [SerializeField] private TextMeshProUGUI _numberLabel;
    [SerializeField] private Image _borderImage;
    [SerializeField] private Color _borderColor;
    [SerializeField] private Color _borderColorHighlighted;

    [Header("Optional")]
    [SerializeField] private TextMeshProUGUI _indexLabel;

    private int _fakeSubstraction;

    public ItemSlot TargetSlot { get; private set; }

    public void SetTarget(ItemSlot slot, int index)
    {
        TargetSlot = slot;
        TargetSlot.Changed += OnTargetSlotChanged;
        OnTargetSlotChanged(TargetSlot);

        if(_indexLabel != null)
            _indexLabel.text = (index + 1).ToString();
    }

    private void OnValidate()
    {
        if (_borderImage != null)
        {
            _borderImage.color = _borderColor;
        }
    }

    private void OnEnable()
    {
        if (TargetSlot != null)
            TargetSlot.Changed += OnTargetSlotChanged;
    }

    private void OnDisable()
    {
        TargetSlot.Changed -= OnTargetSlotChanged;
    }

    private void OnTargetSlotChanged(ItemSlot slot)
    {
        Refresh();
    }

    private void Refresh()
    {
        bool showSlot = false;

        if (TargetSlot.IsEmpty == false)
        {
            int showCount = TargetSlot.Stack.Count - _fakeSubstraction;

            _itemImage.sprite = TargetSlot.Stack.Item.Sprite;

            string numberText = string.Empty;

            if (TargetSlot.Stack.Attributes.Has(WeaponItem.LOADED_AMMO) == true)
            {
                numberText = TargetSlot.Stack.Attributes.Get(WeaponItem.LOADED_AMMO).ToString();
            }
            else if (showCount > 1)
            {
                numberText = showCount.ToString();
            }

            _numberLabel.text = numberText;
            _numberLabel.enabled = numberText != string.Empty;

            showSlot = showCount > 0;
        }

        _itemImage.gameObject.SetActive(showSlot);
        _numberLabel.gameObject.SetActive(showSlot);


        if (_indexLabel != null)
            _indexLabel.gameObject.SetActive(!showSlot);
    }

    public void SetFakeSubstraction(int amount)
    {
        _fakeSubstraction = amount;
        Refresh();
    }

    public void ClearFakeSubstraction()
    {
        _fakeSubstraction = 0;
        Refresh();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _borderImage.color = _borderColorHighlighted;
        Hovered?.Invoke(this);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _borderImage.color = _borderColor;
        Exited?.Invoke(this);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        switch (eventData.button)
        {
            case PointerEventData.InputButton.Left:
                Selected?.Invoke(this);
                break;
            case PointerEventData.InputButton.Right:
                SelectedAlt?.Invoke(this);
                break;
        }
    }

    public void OnPointerClick(PointerEventData eventData) { }

}
