using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class UI_ItemNotification : MonoBehaviour
{

    [SerializeField] private PlayerCharacter _character;
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private Image _itemImage;
    [SerializeField] private TextMeshProUGUI _itemLabel;

    private void OnEnable()
    {
        _character.Inventory.ItemAdded += OnItemAdded;
        _character.Inventory.ItemRemoved += OnItemRemoved;
    }
    private void OnDisable()
    {
        _character.Inventory.ItemAdded -= OnItemAdded;
        _character.Inventory.ItemRemoved -= OnItemRemoved;
    }

    private void OnItemAdded(Item item)
    {
        _itemImage.sprite = item.Sprite;
        _itemLabel.text = $"{item.DisplayName} added";
        RunShowcaseSequence();
    }

    private void OnItemRemoved(Item item)
    {
        _itemImage.sprite = item.Sprite;
        _itemLabel.text = $"{item.DisplayName} removed";
        RunShowcaseSequence();
    }

    private void RunShowcaseSequence()
    {
        DOTween.Sequence().
            Append(_canvasGroup.DOFade(1f, 0.2f).From(0f)).
            AppendInterval(1.2f).
            Append(_canvasGroup.DOFade(0f, 0.2f));
    }

}
