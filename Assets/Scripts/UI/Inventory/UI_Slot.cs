using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using DG.Tweening;

[DefaultExecutionOrder(Order.UI)]
public class UI_Slot : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, IPointerUpHandler
{

    public event Action<UI_Slot> Selected;
    public event Action<UI_Slot> SelectedAlt;
    public event Action<UI_Slot> Hovered;

    [SerializeField] private Image _itemImage;
    [SerializeField] private TextMeshProUGUI _numberLabel;
    [SerializeField] private Image _borderImage;
    [SerializeField] private Color _borderColor;
    [SerializeField] private Color _borderColorHighlighted;

    private int _fakeSubstraction;

    public ItemSlot TargetSlot { get; private set; }

    public void SetTarget(ItemSlot slot)
    {
        TargetSlot = slot;
        TargetSlot.Changed += OnTargetSlotChanged;
        OnTargetSlotChanged(TargetSlot);
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
        bool showSlot = false; // || _isHidden > 0;

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

    public void OnPointerClick(PointerEventData eventData)
    {
        return;
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

    public void OnPointerEnter(PointerEventData eventData)
    {
        //transform.DOScale(1.2f, 0.2f).From(1f).SetLoops(2, LoopType.Yoyo).SetUpdate(true);
        //_border.rectTransform.DOScale(1.15f, 0.1f).From(1f).SetUpdate(true);
        //_borderImage.DOColor(_borderColorHighlighted, 0.1f).From(_borderColor).SetUpdate(true);
        _borderImage.color = _borderColorHighlighted;

        Hovered?.Invoke(this);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        //_border.rectTransform.DOScale(1f, 0.1f).From(1.15f).SetUpdate(true);
        //_borderImage.DOColor(_borderColor, 0.1f).From(_borderColorHighlighted).SetUpdate(true);
        _borderImage.color = _borderColor;
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

}
