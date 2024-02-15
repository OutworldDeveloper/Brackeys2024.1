using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public sealed class UI_PauseMenu : MonoBehaviour
{

    [SerializeField] private CanvasGroup _mainPanel;
    [SerializeField] private RectTransform _buttonsPanel;
    [SerializeField] private Transform _buttonsParent;

    private Sequence _sequeence;

    private void OnEnable()
    {
        _sequeence = DOTween.Sequence().
            Append(_mainPanel.DOFade(1f, 0.75f).From(0f).SetEase(Ease.OutExpo)).
            Join(_buttonsPanel.DOScale(1f, 0.35f).From(0.9f).SetEase(Ease.OutExpo)).
            SetUpdate(true);

        int index = 0;
        foreach (Transform child in _buttonsParent)
        {
            if (child.gameObject.activeSelf == true)
                index++;
            else
                continue;

            _sequeence.Join(child.DOScale(1f, 0.14f).From(0.9f).SetDelay(index * 0.0065f));
        }
    }

    private void OnDisable()
    {
        _sequeence?.Kill();
    }

}
