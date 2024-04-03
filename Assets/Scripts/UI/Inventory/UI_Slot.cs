﻿using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using DG.Tweening;

[DefaultExecutionOrder(Order.UI)]
public class UI_Slot : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{

    public event Action<UI_Slot> Selected;

    [SerializeField] private Image _itemImage;
    [SerializeField] private TextMeshProUGUI _numberLabel;
    [SerializeField] private Image _borderImage;
    [SerializeField] private Color _borderColor;
    [SerializeField] private Color _borderColorHighlighted;

    private int _isHidden;

    public ExpItemSlot TargetSlot { get; private set; }

    public void SetTarget(ExpItemSlot slot)
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

    private void OnTargetSlotChanged(ExpItemSlot slot)
    {
        Refresh();
    }

    private void Refresh()
    {
        bool showEmptySlot = TargetSlot.IsEmpty || _isHidden > 0;

        _itemImage.gameObject.SetActive(showEmptySlot == false);
        _numberLabel.gameObject.SetActive(showEmptySlot == false);

        if (showEmptySlot == false)
        {
            _itemImage.sprite = TargetSlot.FirstItem.Sprite;
            _numberLabel.enabled = TargetSlot.ItemsCount > 1;
            _numberLabel.text = TargetSlot.ItemsCount.ToString();
        }

    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Selected?.Invoke(this);
    }

    public void Hide()
    {
        _isHidden++;
        Refresh();
    }

    public void Show()
    {
        _isHidden--;
        Refresh();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        //transform.DOScale(1.2f, 0.2f).From(1f).SetLoops(2, LoopType.Yoyo).SetUpdate(true);
        //_border.rectTransform.DOScale(1.15f, 0.1f).From(1f).SetUpdate(true);
        //_borderImage.DOColor(_borderColorHighlighted, 0.1f).From(_borderColor).SetUpdate(true);
        _borderImage.color = _borderColorHighlighted;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        //_border.rectTransform.DOScale(1f, 0.1f).From(1.15f).SetUpdate(true);
        //_borderImage.DOColor(_borderColor, 0.1f).From(_borderColorHighlighted).SetUpdate(true);
        _borderImage.color = _borderColor;
    }
}